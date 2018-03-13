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

        bool isToolSelected = false;

        enum Tabs
        {
            Prefab,
            Other,
            Help
        }

        string[] tabs = new string[] { "Prefab", "Other", "Help" };
        int selectedTab = 0;

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
        }

        void OnSceneGUI(SceneView sceneView)
        {
            if (errorNoSelection || errorNoMeshFilter || errorMultiSelection || !isToolSelected)
                return;

            newPivotPoint = Handles.PositionHandle(newPivotPoint, Quaternion.identity);
            // Do your drawing here using Handles.
            Handles.BeginGUI();
            // Do your drawing here using GUI.
            Handles.EndGUI();
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

            if (Tools.current != Tool.None)
                isToolSelected = false;

            if (GUILayout.Button("Move Pivot Point", isToolSelected ? ToggleButtonStyleToggled : ToggleButtonStyleNormal))
            {
                if (!isToolSelected)
                    Tools.current = Tool.None;
                else
                    Tools.current = Tool.Move;
                isToolSelected = !isToolSelected;
            }

            if (GUILayout.Button("Apply new pivot point"))
            {
                ApplyPivotPoint();
            }
        }

        void ApplyPivotPoint()
        {
            Undo.RecordObject(selectedObject.GetComponent<MeshFilter>(), "Change pivot Point");
            MeshFilter mf = selectedObject.GetComponent<MeshFilter>();
            Mesh newMesh = (Mesh)Instantiate(mf.sharedMesh);

            List<Vector3> vertices = new List<Vector3>();
            newMesh.GetVertices(vertices);
            Vector3 translationVector = -selectedObject.transform.worldToLocalMatrix.MultiplyPoint3x4(newPivotPoint);
            Debug.Log(newPivotPoint);
            for(int i = 0; i < vertices.Count; i++)
            {
                vertices[i] += translationVector;
            }
            newMesh.SetVertices(vertices);
            mf.sharedMesh = newMesh;
            selectedObject.transform.position = newPivotPoint;
        }
    }
}
