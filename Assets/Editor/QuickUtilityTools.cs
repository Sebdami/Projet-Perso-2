﻿using System.Collections;
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

        - Deselect all

        - Multiple Tags System ?
    */

    GameObject selectedObject = null;
    GameObject[] selectedObjects = null;

    string parentName = "NewParent";
    bool showParentTools = false;


    bool errorNoSelection = false;
    bool centerPivotPoint = true;

    [MenuItem("Tools/Quick Utility Tools/Open Window")]
    static void Init()
    {
        // Get existing open window or if none, make a new one:
        QuickUtilityTools window = (QuickUtilityTools)EditorWindow.GetWindow(typeof(QuickUtilityTools), false, "Quick Utility");
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
        if (selectedObjects == null)
            OnSelectionChange();
        errorNoSelection = false;
        if (selectedObjects == null || selectedObjects.Length < 1)
            errorNoSelection = true;

        if(errorNoSelection)
            GUI.enabled = false;

        if(GUILayout.Button("Select only TopLevel"))
        {
            SelectOnlyTopLevel();
        }

        if (GUILayout.Button("Select root objects"))
        {
            SelectRoots();
        }

        //showParentTools = GUI.Toggle(new Rect(10, 50, 100, 50), showParentTools, "Parent tools");
        //GUI.BeginGroup(new Rect(10, 50, 500, 250));
        //GUI.Box(new Rect(0, 0, 500, 250), "ParentTools");
        //GUILayout.BeginVertical();
        parentName = EditorGUILayout.TextField("New parent name", parentName);
           
        centerPivotPoint = EditorGUILayout.Toggle("Center parent pivot point", centerPivotPoint);
        
        if(GUILayout.Button("Make parent"))
        {
            CreateParent();
        }
        //GUILayout.EndVertical();
        //GUI.EndGroup();
    }

    [MenuItem("Tools/Quick Utility Tools/Select only TopLevel %&t")]
    static void SelectOnlyTopLevelItem()
    {
        Object[] selection = Selection.GetFiltered(typeof(GameObject), SelectionMode.TopLevel | SelectionMode.ExcludePrefab);
        GameObject[] goSelect = new GameObject[selection.Length];
        for (int i = 0; i < selection.Length; i++)
        {
            goSelect[i] = selection[i] as GameObject;
        }

        Selection.objects = goSelect;
    }

    [MenuItem("Tools/Quick Utility Tools/Select Roots %&r")]
    static void SelectRootsItem()
    {
        Object[] selection = Selection.GetFiltered(typeof(GameObject), SelectionMode.TopLevel | SelectionMode.ExcludePrefab);
        GameObject[] goSelect = new GameObject[selection.Length];
        for (int i = 0; i < selection.Length; i++)
        {
            goSelect[i] = ((GameObject)selection[i]).transform.root.gameObject;
        }

        Selection.objects = goSelect;
    }

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

    void SelectRoots()
    {
        Object[] selection = Selection.GetFiltered(typeof(GameObject), SelectionMode.TopLevel | SelectionMode.ExcludePrefab);
        selectedObjects = new GameObject[selection.Length];
        for (int i = 0; i < selection.Length; i++)
        {
            selectedObjects[i] = ((GameObject)selection[i]).transform.root.gameObject;
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
        GameObject parent = new GameObject(parentName);
        if(centerPivotPoint)
            parent.transform.position = FindSelectionCenter();

        Undo.RegisterCreatedObjectUndo(parent, "CreatedParent");
        parent.transform.SetParent(FindClosestToRootSelectedGameObject().transform.parent);
        for (int i = 0; i < selectedObjects.Length; i++)
        {
            Undo.SetTransformParent(selectedObjects[i].transform, parent.transform, "SetParent" + i);
        }
        Selection.activeObject = parent;
    }

    Vector3 FindSelectionCenter()
    {
        Vector3 center = Vector3.zero;
        Object[] selection = Selection.GetFiltered(typeof(GameObject), SelectionMode.Deep | SelectionMode.ExcludePrefab);
        GameObject[] goSelection = new GameObject[selection.Length];
        for (int i = 0; i < selection.Length; i++)
        {
            goSelection[i] = selection[i] as GameObject;
        }


        for (int i = 0; i < goSelection.Length; i++)
        {
            center += goSelection[i].transform.position;
        }
        center /= goSelection.Length;

        return center;
    }

    void MoveToFloor()
    {

    }

}