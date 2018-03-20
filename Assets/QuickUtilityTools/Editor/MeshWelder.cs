using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
namespace QuickUtility
{
    class MeshWelder : EditorWindow
    {
        GameObject[] selectedObjects = null;
        Transform LocalSpacePoint = null;
        bool errorLoadNoComponent = false;
        bool errorLoadNoSelection = true;
        bool mergeSubmeshes = false;
        bool intelligentMergeSubmeshes = false;
        bool spawnInstance = false;
        bool optimizeMesh = true;
        bool meshCombined = false;
        Mesh combined;
        void OnSelectionChange()
        {
            errorLoadNoComponent = false;
            Object[] selection = Selection.GetFiltered(typeof(GameObject), SelectionMode.TopLevel | SelectionMode.ExcludePrefab);
            errorLoadNoSelection = selection.Length < 2;
            selectedObjects = new GameObject[selection.Length];
            for (int i = 0; i < selection.Length; i++)
            {
                selectedObjects[i] = selection[i] as GameObject;
                if (!selectedObjects[i].GetComponent<MeshFilter>() || !selectedObjects[i].GetComponent<MeshRenderer>())
                {
                    errorLoadNoComponent = true;
                }
            }
            Repaint();
        }

        [MenuItem("Tools/Quick Utility Tools/Mesh Welder")]
        public static void ShowWindow()
        {
            EditorWindow.GetWindow(typeof(MeshWelder));
        }

        void OnGUI()
        {
            LocalSpacePoint = EditorGUILayout.ObjectField("Local space ref point", LocalSpacePoint, typeof(Transform), true) as Transform;
            if (errorLoadNoSelection)
            {
                EditorGUILayout.HelpBox("Select at least 2 objects in the Hierarchy", MessageType.Info);
                GUI.enabled = false;
            }
            if (errorLoadNoComponent)
            {
                EditorGUILayout.HelpBox("The selected objects must have a \"Mesh Filter\" and a \"Mesh Renderer\" component.", MessageType.Warning);
                GUI.enabled = false;
            }
            EditorGUILayout.HelpBox("If all your objects use the same material, check Merge Submeshes", MessageType.Info);
            if (intelligentMergeSubmeshes)
            {
                GUI.enabled = false;
            }
            mergeSubmeshes = GUILayout.Toggle(mergeSubmeshes, "Merge Submeshes");
            if (intelligentMergeSubmeshes)
            {
                GUI.enabled = true;
            }

            EditorGUILayout.HelpBox("If some of your objects use the same material, check Intelligent Merge Submeshes", MessageType.Info);

            if (mergeSubmeshes)
            {
                GUI.enabled = false;
            }
            intelligentMergeSubmeshes = GUILayout.Toggle(intelligentMergeSubmeshes, "Intelligent Merge Submeshes");
            if (mergeSubmeshes)
            {
                GUI.enabled = true;
            }

            spawnInstance = GUILayout.Toggle(spawnInstance, "Spawn a new instance with the new mesh");
            //optimizeMesh = GUILayout.Toggle(optimizeMesh, "Optimize Mesh");

            if (!LocalSpacePoint)
            {
                GUI.enabled = false;
                EditorGUILayout.HelpBox("Pivot Point needs to be set", MessageType.Warning);
            }
            if (GUILayout.Button("Weld Meshes", GUILayout.Height(30), GUILayout.Width(200)))
            {
                WeldMeshes();
            }
            GUI.enabled = true;
            if (meshCombined && combined != null)
            {
                EditorGUILayout.HelpBox("Combined mesh has been created. It contains " + combined.vertexCount + " vertices and " + combined.triangles.Length + " tris.", MessageType.Info);
                if (!mergeSubmeshes)
                {
                    EditorGUILayout.HelpBox("The new mesh contains " + combined.subMeshCount.ToString() + " submeshes. As many materials will be needed to be set in the mesh renderer component when you use the mesh.", MessageType.Info);
                }
                meshCombined = false;
            }
        }

        void WeldMeshes()
        {
            Dictionary<Material, List<MeshFilter>> usedMaterials = new Dictionary<Material, List<MeshFilter>>();
            combined = new Mesh();
            CombineInstance[] combine = new CombineInstance[selectedObjects.Length];
            if (intelligentMergeSubmeshes)
            {
                List<Mesh> CombinedMeshes = new List<Mesh>();
                for (int i = 0; i < selectedObjects.Length; i++)
                {
                    Material mat = selectedObjects[i].GetComponent<MeshRenderer>().sharedMaterial;
                    if (!usedMaterials.ContainsKey(mat))
                    {
                        usedMaterials.Add(mat, new List<MeshFilter>());
                    }
                    usedMaterials[mat].Add(selectedObjects[i].GetComponent<MeshFilter>());
                }
                foreach (Material mat in usedMaterials.Keys)
                {
                    MeshFilter[] meshes = usedMaterials[mat].ToArray();
                    CombineInstance[] comb = new CombineInstance[meshes.Length];
                    for (int i = 0; i < comb.Length; i++)
                    {
                        comb[i].mesh = meshes[i].sharedMesh;
                        comb[i].transform = meshes[i].transform.localToWorldMatrix;
                    }
                    Mesh curmesh = new Mesh();
                    curmesh.CombineMeshes(comb);
                    CombinedMeshes.Add(curmesh);
                }
                Matrix4x4 refPoint = LocalSpacePoint.worldToLocalMatrix;
                refPoint.SetTRS(new Vector3(refPoint.m03, refPoint.m13, refPoint.m23), Quaternion.identity, Vector3.one);
                combine = new CombineInstance[CombinedMeshes.Count];
                for (int i = 0; i < CombinedMeshes.Count; i++)
                {
                    combine[i].mesh = CombinedMeshes[i];
                    combine[i].transform = refPoint;
                }
                combined.CombineMeshes(combine, false);

                if (spawnInstance)
                {
                    GameObject go = new GameObject("CombinedMesh", typeof(MeshFilter), typeof(MeshRenderer));
                    go.GetComponent<MeshFilter>().sharedMesh = combined;
                    MeshRenderer mr = go.GetComponent<MeshRenderer>();
                    Material[] mats = new Material[usedMaterials.Keys.Count];
                    int i = 0;
                    foreach (Material mat in usedMaterials.Keys)
                    {
                        mats[i] = mat;
                        i++;
                    }
                    mr.sharedMaterials = mats;
                }
            }
            else
            {
                for (int i = 0; i < selectedObjects.Length; i++)
                {
                    combine[i].mesh = selectedObjects[i].GetComponent<MeshFilter>().sharedMesh;
                    combine[i].transform = selectedObjects[i].GetComponent<MeshFilter>().transform.localToWorldMatrix;
                }
                combined.CombineMeshes(combine, mergeSubmeshes);

                if (spawnInstance)
                {
                    GameObject go = new GameObject("CombinedMesh", typeof(MeshFilter), typeof(MeshRenderer));
                    go.GetComponent<MeshFilter>().sharedMesh = combined;
                    MeshRenderer mr = go.GetComponent<MeshRenderer>();
                    if (mergeSubmeshes)
                    {
                        mr.sharedMaterial = selectedObjects[0].GetComponent<MeshRenderer>().sharedMaterial;
                    }
                    else
                    {
                        Material[] mats = new Material[selectedObjects.Length];

                        for (int i = 0; i < selectedObjects.Length; i++)
                        {
                            if (selectedObjects[i].GetComponent<MeshRenderer>())
                                mats[i] = selectedObjects[i].GetComponent<MeshRenderer>().sharedMaterial;
                        }
                        mr.sharedMaterials = mats;
                    }

                }
            }



            string path = EditorUtility.SaveFilePanel("Save Welded Mesh Asset", "Assets/", name, "asset");
            if (string.IsNullOrEmpty(path))
            {
                return;
            }
            path = FileUtil.GetProjectRelativePath(path);

            Mesh meshToSave = combined;
            if (optimizeMesh)
                MeshUtility.Optimize(meshToSave);

            AssetDatabase.CreateAsset(meshToSave, path);
            AssetDatabase.SaveAssets();
            meshCombined = true;
        }
    }
}
