using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace QuickUtility
{
    public class MovePivotPointTool : EditorWindow
    {
        private static GUIStyle ToggleButtonStyleNormal = null;
        private static GUIStyle ToggleButtonStyleToggled = null;

        GameObject selectedObject = null;

        Vector3 newPivotPoint;

        bool errorNoSelection = false;
        bool errorMultiSelection;
        bool errorNoMeshFilter = false;

        bool saveNewMesh = false;

        bool isToolSelected = false;

        bool moveCollider = false;

        [MenuItem("Tools/Quick Utility Tools/Move Pivot Point", priority = 0)]
        static void Init()
        {
            // Get existing open window or if none, make a new one:
            MovePivotPointTool window = (MovePivotPointTool)EditorWindow.GetWindow(typeof(MovePivotPointTool), false, "Move Pivot");
            if (ToggleButtonStyleNormal == null)
            {
                ToggleButtonStyleNormal = "Button";
                ToggleButtonStyleToggled = new GUIStyle(ToggleButtonStyleNormal);
                ToggleButtonStyleToggled.normal.background = ToggleButtonStyleToggled.active.background;
            }
            window.Show();
        }

        void OnSelectionChange()
        {
            errorNoMeshFilter = false;
            errorNoSelection = false;
            errorMultiSelection = false;

            Object[] selection = Selection.GetFiltered(typeof(GameObject), SelectionMode.ExcludePrefab);
            if (selection.Length == 0)
            {
                errorNoSelection = true;
                selectedObject = null;
                Repaint();
                return;
            }
            if (selection.Length > 1)
            {
                errorMultiSelection = true;
                selectedObject = null;
                Repaint();
                return;
            }
            if(!((GameObject)selection[0]).GetComponent<MeshFilter>())
            {
                errorNoMeshFilter = true;
                selectedObject = null;
                Repaint();
                return;
            }
            if(selectedObject != null && selectedObject != selection[0] as GameObject)
            {
                newPivotPoint = (selection[0] as GameObject).transform.position;
            }
            selectedObject = selection[0] as GameObject;

            moveCollider = HasMovableCollider(selectedObject);

            Repaint();
        }

        void OnFocus()
        {
            OnSelectionChange();
            // Remove delegate listener if it has previously
            // been assigned.
            SceneView.onSceneGUIDelegate -= this.OnSceneGUI;
            // Add (or re-add) the delegate.
            SceneView.onSceneGUIDelegate += this.OnSceneGUI;
        }

        void OnDestroy()
        {
            // When the window is destroyed, remove the delegate
            // so that it will no longer do any drawing.
            SceneView.onSceneGUIDelegate -= this.OnSceneGUI;
            if (isToolSelected)
                Tools.current = Tool.Move;
        }

        void OnSceneGUI(SceneView sceneView)
        {
            if (errorNoSelection || errorNoMeshFilter || errorMultiSelection)
                return;

            if (!isToolSelected && selectedObject)
            {
                newPivotPoint = selectedObject.transform.position;
                return;
            }
            Vector3 oldpos = newPivotPoint;
            newPivotPoint = Handles.PositionHandle(newPivotPoint, Quaternion.identity);

            if (newPivotPoint != oldpos)
                Repaint();

            Handles.DrawWireCube(newPivotPoint, new Vector3(.125f, .125f, .125f));
        }

        void OnGUI()
        {
            if (errorNoSelection)
            {
                EditorGUILayout.HelpBox("Select an object with a Mesh Filter to edit its pivot point", MessageType.Info);
                GUI.enabled = false;
            }
            if(errorMultiSelection)
            {
                EditorGUILayout.HelpBox("Please select only one object with a Mesh Filter to edit its pivot point", MessageType.Warning);
                GUI.enabled = false;
            }
            if(errorNoMeshFilter)
            {
                EditorGUILayout.HelpBox("The selected object has no Mesh Filter", MessageType.Error);
                GUI.enabled = false;
            }

            if (!selectedObject)
                return;
            if (Tools.current != Tool.None)
                isToolSelected = false;

            if (GUILayout.Button("Click to Move Pivot Point", isToolSelected ? ToggleButtonStyleToggled : ToggleButtonStyleNormal, GUILayout.Height(50)))
            {
                if (!isToolSelected)
                    Tools.current = Tool.None;
                else
                    Tools.current = Tool.Move;
                isToolSelected = !isToolSelected;
            }
            //Not good, newPivotPoint should be local coordinates
            if (!isToolSelected)
            {
                GUI.enabled = false;
                EditorGUILayout.Vector3Field("Pivot Point", Vector3.zero);
                GUI.enabled = true;
            }
            else
            {
                Vector3 oldPp = newPivotPoint;
                newPivotPoint = selectedObject.transform.localToWorldMatrix.MultiplyPoint3x4(EditorGUILayout.Vector3Field("Pivot Point", selectedObject.transform.worldToLocalMatrix.MultiplyPoint3x4(newPivotPoint)));
                if(newPivotPoint != oldPp)
                    SceneView.RepaintAll();
            }

            if (errorNoSelection || errorNoMeshFilter || errorMultiSelection)
                GUI.enabled = false;
            if (selectedObject && HasMovableCollider(selectedObject))
            {
                moveCollider = EditorGUILayout.Toggle("Move Collider", moveCollider);
            }

            saveNewMesh = EditorGUILayout.Toggle("Save Modified Mesh", saveNewMesh);
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Apply new pivot point", GUILayout.Width(200), GUILayout.Height(25)))
            {
                ApplyPivotPoint();
            }
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
        }

        void ApplyPivotPoint()
        {
            Undo.RecordObject(selectedObject.GetComponent<MeshFilter>(), "Change pivot point");
            MeshFilter mf = selectedObject.GetComponent<MeshFilter>();
            Mesh oldMesh = mf.sharedMesh;
            Mesh newMesh = (Mesh)Instantiate(mf.sharedMesh);

            List<Vector3> vertices = new List<Vector3>();
            newMesh.GetVertices(vertices);
            Vector3 translationVector = -selectedObject.transform.worldToLocalMatrix.MultiplyPoint3x4(newPivotPoint);
            for(int i = 0; i < vertices.Count; i++)
            {
                vertices[i] += translationVector;
            }
            newMesh.SetVertices(vertices);

            if (saveNewMesh)
            {
                string filePath = EditorUtility.SaveFilePanelInProject("Save modified mesh as asset", oldMesh.name + "_new", "asset", "");
                if (filePath != string.Empty)
                {
                    filePath = filePath.Remove(filePath.LastIndexOf('.'));
                    filePath += ".asset";
                    AssetDatabase.CreateAsset(newMesh, filePath);
                    AssetDatabase.SaveAssets();
                }
            }

            mf.sharedMesh = newMesh;
            MoveOnlyParentWithUndo(selectedObject, newPivotPoint);
            if(moveCollider)
            {
                Undo.RecordObject(selectedObject.GetComponent<Collider>(), "Change pivot point move collider");
                if (selectedObject.GetComponent<BoxCollider>())
                {
                    selectedObject.GetComponent<BoxCollider>().center += translationVector;
                }
                if (selectedObject.GetComponent<SphereCollider>())
                {
                    selectedObject.GetComponent<SphereCollider>().center += translationVector;
                }
                if (selectedObject.GetComponent<CapsuleCollider>())
                {
                    selectedObject.GetComponent<CapsuleCollider>().center += translationVector;
                }
            }
            
            isToolSelected = false;
            Tools.current = Tool.Move;
        }

        void MoveOnlyParentWithUndo(GameObject go, Vector3 newPosition)
        {
            GameObject temp = new GameObject("temp");
            while (go.transform.childCount > 0)
            {
                Undo.RecordObject(selectedObject.transform.GetChild(0), "Change pivot point move object child");
                go.transform.GetChild(0).SetParent(temp.transform);
            }
            Undo.RecordObject(selectedObject.transform, "Change pivot point move object");
            go.transform.position = newPosition;

            while (temp.transform.childCount > 0)
                temp.transform.GetChild(0).SetParent(go.transform);
            DestroyImmediate(temp);
        }

        bool HasMovableCollider(GameObject obj)
        {
            return obj.GetComponent<BoxCollider>() || obj.GetComponent<SphereCollider>() || obj.GetComponent<CapsuleCollider>();
        }
    }
}
