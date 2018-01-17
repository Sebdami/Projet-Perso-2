﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
namespace QuickUtility
{
    public class QuickUtilityTools : EditorWindow
    {

        /*
            Planned Tools:

            - Mirror movements between 2 objects (if you move the right side object to the right, move the left side object to the left...) on an axis, or on a plane

            - Hide some objects in editor view (Special button for canvases)

            v- Create a new shared parent for the selected objects

            o- Recenter parent from children positions

            v- Select all children

            - Rename children

            v- Select parents

            - Fix children movement (Allows to move parent without moving children when enabled)

            - Move objects to the floor level

            v- Deselect all

            - Multiple Tags System ?

            - Clear parent

            - Apply changes to prefab

            - Break prefab instance?

            - Set as first / last sibling
        */

        GameObject selectedObject = null;
        GameObject[] selectedObjects = null;

        string parentName = "NewParent";
        bool showParentTools = false;


        bool errorNoSelection = false;
        bool centerPivotPoint = true;

        [MenuItem("Tools/Quick Utility Tools/Open Window", priority = 1)]
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

            if (errorNoSelection)
                GUI.enabled = false;

            /*if(GUILayout.Button("Select only TopLevel"))
            {
                SelectOnlyTopLevelItem();
            }

            if (GUILayout.Button("Select root objects"))
            {
                SelectRootsItem();
            }

            if (GUILayout.Button("Select all children"))
            {
                SelectChildrenOnlyItem();
            }

            if (GUILayout.Button("Select object with children"))
            {
                SelectChildrenItem();
            }

            if (GUILayout.Button("Select parents"))
            {
                SelectParentsItem();
            }*/
            //showParentTools = GUI.Toggle(new Rect(10, 50, 100, 50), showParentTools, "Parent tools");
            //GUI.BeginGroup(new Rect(10, 50, 500, 250));
            //GUI.Box(new Rect(0, 0, 500, 250), "ParentTools");
            //GUILayout.BeginVertical();
            string currentWindow = focusedWindow.ToString();
            GUILayout.Label(currentWindow);
            parentName = EditorGUILayout.TextField("New parent name", parentName);

            centerPivotPoint = EditorGUILayout.Toggle("Center parent pivot point", centerPivotPoint);

            if (GUILayout.Button("Make parent"))
            {
                CreateParent();
            }

            //GUILayout.EndVertical();
            //GUI.EndGroup();
        }

        [MenuItem("Tools/Quick Utility Tools/Selection/Select only TopLevel %t")]
        static void SelectOnlyTopLevelItem()
        {
            if (EditorWindow.focusedWindow.ToString() != " (UnityEditor.SceneHierarchyWindow)" && EditorWindow.focusedWindow.ToString() != " (UnityEditor.SceneView)" && EditorWindow.focusedWindow.ToString() != " (QuickUtility.QuickUtilityTools)") //The space before the name is needed
                return;
            EditorApplication.ExecuteMenuItem("Window/Hierarchy");
            Object[] selection = Selection.GetFiltered(typeof(GameObject), SelectionMode.TopLevel | SelectionMode.ExcludePrefab);
            GameObject[] goSelect = new GameObject[selection.Length];
            for (int i = 0; i < selection.Length; i++)
            {
                goSelect[i] = selection[i] as GameObject;
            }

            Selection.objects = goSelect;
        }

        [MenuItem("Tools/Quick Utility Tools/Selection/Select Roots %u")]
        static void SelectRootsItem()
        {
            if (EditorWindow.focusedWindow.ToString() != " (UnityEditor.SceneHierarchyWindow)" && EditorWindow.focusedWindow.ToString() != " (UnityEditor.SceneView)") //The space before the name is needed
                return;
            EditorApplication.ExecuteMenuItem("Window/Hierarchy");
            Object[] selection = Selection.GetFiltered(typeof(GameObject), SelectionMode.TopLevel | SelectionMode.ExcludePrefab);
            GameObject[] goSelect = new GameObject[selection.Length];
            for (int i = 0; i < selection.Length; i++)
            {
                goSelect[i] = ((GameObject)selection[i]).transform.root.gameObject;
            }

            Selection.objects = goSelect;
        }

        [MenuItem("Tools/Quick Utility Tools/Selection/Select All Children Only %g")]
        static void SelectChildrenOnlyItem()
        {
            if (EditorWindow.focusedWindow.ToString() != " (UnityEditor.SceneHierarchyWindow)" && EditorWindow.focusedWindow.ToString() != " (UnityEditor.SceneView)") //The space before the name is needed
                return;
            EditorApplication.ExecuteMenuItem("Window/Hierarchy");
            Object[] selection = Selection.GetFiltered(typeof(GameObject), SelectionMode.ExcludePrefab);
            List<GameObject> goList = new List<GameObject>();
            for (int i = 0; i < selection.Length; i++)
            {
                Transform childTransform = ((GameObject)selection[i]).transform;
                for (int j = 0; j < childTransform.transform.childCount; j++)
                {
                    goList.Add(childTransform.GetChild(j).gameObject);
                }
            }
            Selection.objects = goList.ToArray();
        }
        [MenuItem("Tools/Quick Utility Tools/Selection/Select Object and Children %l")]
        static void SelectChildrenItem()
        {
            if (EditorWindow.focusedWindow.ToString() != " (UnityEditor.SceneHierarchyWindow)" && EditorWindow.focusedWindow.ToString() != " (UnityEditor.SceneView)") //The space before the name is needed
                return;
            EditorApplication.ExecuteMenuItem("Window/Hierarchy");
            Object[] selection = Selection.GetFiltered(typeof(GameObject), SelectionMode.ExcludePrefab);
            List<GameObject> goList = new List<GameObject>();
            for (int i = 0; i < selection.Length; i++)
            {
                Transform childTransform = ((GameObject)selection[i]).transform;
                goList.Add(childTransform.gameObject);
                for (int j = 0; j < childTransform.transform.childCount; j++)
                {
                    goList.Add(childTransform.GetChild(j).gameObject);
                }
            }
            Selection.objects = goList.ToArray();
        }
        [MenuItem("Tools/Quick Utility Tools/Selection/Select Parents %h")]
        static void SelectParentsItem()
        {
            if (EditorWindow.focusedWindow.ToString() != " (UnityEditor.SceneHierarchyWindow)" && EditorWindow.focusedWindow.ToString() != " (UnityEditor.SceneView)") //The space before the name is needed
                return;
            EditorApplication.ExecuteMenuItem("Window/Hierarchy");
            Object[] selection = Selection.GetFiltered(typeof(GameObject), SelectionMode.ExcludePrefab);
            List<GameObject> goList = new List<GameObject>();
            for (int i = 0; i < selection.Length; i++)
            {
                if (((GameObject)selection[i]).transform.parent == null)
                    continue;
                GameObject toAdd = ((GameObject)selection[i]).transform.parent.gameObject;
                if (!goList.Contains(toAdd))
                    goList.Add(toAdd);
            }
            Selection.objects = goList.ToArray();
        }

        [MenuItem("Tools/Quick Utility Tools/Selection/Deselect All %i")]
        static void DeselectAll()
        {
            Selection.objects = new Object[0];
        }

        //void SelectOnlyTopLevel()
        //{
        //    Object[] selection = Selection.GetFiltered(typeof(GameObject), SelectionMode.TopLevel | SelectionMode.ExcludePrefab);
        //    selectedObjects = new GameObject[selection.Length];
        //    for (int i = 0; i < selection.Length; i++)
        //    {
        //        selectedObjects[i] = selection[i] as GameObject;
        //    }

        //    Selection.objects = selectedObjects;
        //}

        //void SelectRoots()
        //{
        //    Object[] selection = Selection.GetFiltered(typeof(GameObject), SelectionMode.TopLevel | SelectionMode.ExcludePrefab);
        //    selectedObjects = new GameObject[selection.Length];
        //    for (int i = 0; i < selection.Length; i++)
        //    {
        //        selectedObjects[i] = ((GameObject)selection[i]).transform.root.gameObject;
        //    }

        //    Selection.objects = selectedObjects;
        //}

        //void SelectChildren()
        //{
        //    Object[] selection = Selection.GetFiltered(typeof(GameObject), SelectionMode.ExcludePrefab);
        //    List<GameObject> goList = new List<GameObject>();
        //    for (int i = 0; i < selection.Length; i++)
        //    {
        //        Transform childTransform = ((GameObject)selection[i]).transform;
        //        for (int j = 0; j < childTransform.transform.childCount; j++)
        //        {
        //            goList.Add(childTransform.GetChild(j).gameObject);
        //        }
        //    }
        //    selectedObjects = goList.ToArray();
        //    Selection.objects = selectedObjects;
        //}

        static GameObject FindClosestToRootSelectedGameObjectStatic(GameObject[] selectedObjects)
        {
            Object[] selection = Selection.GetFiltered(typeof(GameObject), SelectionMode.ExcludePrefab);
            selectedObjects = new GameObject[selection.Length];
            for (int i = 0; i < selection.Length; i++)
            {
                selectedObjects[i] = selection[i] as GameObject;
            }

            int smallestDistanceFromRoot = 10000;
            GameObject closestObjectFromRoot = null;
            GameObject currentObject = null;
            int currentDistance = 0;
            for (int i = 0; i < selectedObjects.Length; i++)
            {
                currentObject = selectedObjects[i];
                currentDistance = 0;
                while (currentObject.transform.parent != null)
                {
                    currentDistance++;
                    currentObject = currentObject.transform.parent.gameObject;
                }
                if (currentDistance < smallestDistanceFromRoot)
                {
                    smallestDistanceFromRoot = currentDistance;
                    closestObjectFromRoot = selectedObjects[i];
                }
            }

            return closestObjectFromRoot;
        }

        GameObject FindClosestToRootSelectedGameObject()
        {
            if (selectedObjects == null || selectedObjects.Length == 0)
                return null;
            int smallestDistanceFromRoot = 10000;
            GameObject closestObjectFromRoot = null;
            GameObject currentObject = null;
            int currentDistance = 0;
            for (int i = 0; i < selectedObjects.Length; i++)
            {
                currentObject = selectedObjects[i];
                currentDistance = 0;
                while (currentObject.transform.parent != null)
                {
                    currentDistance++;
                    currentObject = currentObject.transform.parent.gameObject;
                }
                if (currentDistance < smallestDistanceFromRoot)
                {
                    smallestDistanceFromRoot = currentDistance;
                    closestObjectFromRoot = selectedObjects[i];
                }
            }

            return closestObjectFromRoot;
        }
        [MenuItem("GameObject/Create Parent", true)]
        static bool ValidateCreateParentItem()
        {
            Object[] selection = Selection.GetFiltered(typeof(GameObject), SelectionMode.ExcludePrefab);
            if (selection == null || selection.Length == 0)
                return false;
            return true;
        }

        [MenuItem("GameObject/Create Parent", false, 0)]
        static void CreateParentItem()
        {

            Object[] selection = Selection.GetFiltered(typeof(GameObject), SelectionMode.ExcludePrefab);
            if (selection == null || selection.Length == 0)
                return;
            GameObject[] selectedObjects = new GameObject[selection.Length];
            for (int i = 0; i < selection.Length; i++)
            {
                selectedObjects[i] = selection[i] as GameObject;
            }

            GameObject parent = new GameObject("Parent");

            Undo.RegisterCreatedObjectUndo(parent, "CreatedParent");
            GameObject closestToRoot = FindClosestToRootSelectedGameObjectStatic(selectedObjects);
            parent.transform.SetParent(closestToRoot.transform.parent);
            parent.transform.SetSiblingIndex(closestToRoot.transform.GetSiblingIndex());
            for (int i = 0; i < selectedObjects.Length; i++)
            {
                Undo.SetTransformParent(selectedObjects[i].transform, parent.transform, "SetParent" + i);
            }
            Selection.activeObject = parent;
        }

        void CreateParent()
        {
            GameObject parent = new GameObject(parentName);
            if (centerPivotPoint)
                parent.transform.position = FindSelectionCenter();

            Undo.RegisterCreatedObjectUndo(parent, "CreatedParent");
            GameObject closestToRoot = FindClosestToRootSelectedGameObject();
            parent.transform.SetParent(closestToRoot.transform.parent);
            parent.transform.SetSiblingIndex(closestToRoot.transform.GetSiblingIndex());
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
}
