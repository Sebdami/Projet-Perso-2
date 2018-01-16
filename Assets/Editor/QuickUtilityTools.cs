using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class QuickUtilityTools : EditorWindow
{
    /*
        Planned Tools:

        - Mirror movements between 2 objects (if you move the right side object to the right, move the left side object to the left...) on an axis, or on a plane

        - Hide some objects in editor view (Special button for canvases)

        - Create a new shared parent for the selected objects

        - Recenter parent from children positions

        - Select all children

        - Rename children

        - Select parents

        - Fix children movement (Allows to move parent without moving children when enabled)

        - Move objects to the floor level



        - Multiple Tags System ?
    */

    GameObject selectedObject = null;
    GameObject[] selectedObjects = null;

    bool errorNoSelection = false;

    [MenuItem("Window/Quick Utility Tools")]
    static void Init()
    {
        // Get existing open window or if none, make a new one:
        QuickUtilityTools window = (QuickUtilityTools)EditorWindow.GetWindow(typeof(QuickUtilityTools));
        window.Show();
    }

    void OnSelectionChange()
    {
        Object[] selection = Selection.GetFiltered(typeof(GameObject), SelectionMode.ExcludePrefab);
        selectedObjects = new GameObject[selection.Length];
        for (int i = 0; i < selection.Length; i++)
        {
            selectedObjects[i] = selection[i] as GameObject;
        }
        selectedObject = Selection.activeObject as GameObject;
        Repaint();
    }

    void OnGUI()
    {
        errorNoSelection = false;
        if (selectedObjects == null || selectedObjects.Length < 1)
            errorNoSelection = true;

        if(errorNoSelection)
            GUI.enabled = false;

        if(GUILayout.Button("Select only TopLevel"))
        {
            SelectOnlyTopLevel();
        }

        if(GUILayout.Button("Make parent"))
        {
            CreateParent();
        }
    }
    [ContextMenu("Select only Topmost", false)]
    void SelectOnlyTopLevel()
    {
        Object[] selection = Selection.GetFiltered(typeof(GameObject), SelectionMode.TopLevel | SelectionMode.ExcludePrefab);
        selectedObjects = new GameObject[selection.Length];
        for (int i = 0; i < selection.Length; i++)
        {
            selectedObjects[i] = selection[i] as GameObject;
        }

        Selection.objects = selectedObjects;
    }

    GameObject FindClosestToRootSelectedGameObject()
    {
        if (selectedObjects == null || selectedObjects.Length == 0)
            return null;
        int smallestDistanceFromRoot = 10000;
        GameObject closestObjectFromRoot = null;
        GameObject currentObject = null;
        int currentDistance = 0;
        for(int i = 0; i < selectedObjects.Length; i++)
        {
            currentObject = selectedObjects[i];
            currentDistance = 0;
            while (currentObject.transform.parent != null)
            {
                currentDistance++;
                currentObject = currentObject.transform.parent.gameObject;
            }
            if(currentDistance < smallestDistanceFromRoot)
            {
                smallestDistanceFromRoot = currentDistance;
                closestObjectFromRoot = selectedObjects[i];
            }
        }

        return closestObjectFromRoot;
    }

    void CreateParent()
    {
        
        GameObject parent = new GameObject("NewParent");
        Undo.RegisterCreatedObjectUndo(parent, "CreatedParent");
        parent.transform.SetParent(FindClosestToRootSelectedGameObject().transform.parent);
        for (int i = 0; i < selectedObjects.Length; i++)
        {
            Undo.SetTransformParent(selectedObjects[i].transform, parent.transform, "SetParent" + i);
        }
    }

}
