using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using System.Reflection;

using Object = UnityEngine.Object;

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

            v- Fix children movement (Allows to move parent without moving children when enabled)

            - Move objects to the floor level

            v- Deselect all

            v- Clear parent

            - Apply changes to prefab

            - Break prefab instance?

            - Set as first / last sibling (May be useless just use drag and drop)
        */

        Object obj;
        GameObject[] selectedObjects = null;

        string parentName = "NewParent";

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

            string currentWindow = focusedWindow.ToString();
            GUILayout.Label(currentWindow);
            parentName = EditorGUILayout.TextField("New parent name", parentName);

            centerPivotPoint = EditorGUILayout.Toggle("Center parent pivot point", centerPivotPoint);

            if (GUILayout.Button("Create parent"))
            {
                CreateParent();
            }
            obj = EditorGUILayout.ObjectField("Object to extract methods from", obj, typeof(Transform), true);
            // obj = null;
            if (GUILayout.Button("Log methods"))
            {
                Debug.Log(getMethodsOn(obj));
            }
            if (GUILayout.Button("Log members"))
            {
                Debug.Log(getMembersOf(obj));
            }
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

        // ---------- Create parent ----------
        [MenuItem("GameObject/ Create Parent", true)]
        static bool ValidateCreateParentItem()
        {
            Object[] selection = Selection.GetFiltered(typeof(GameObject), SelectionMode.ExcludePrefab);
            if (selection == null || selection.Length == 0)
                return false;
            return true;
        }
        [MenuItem("GameObject/ Create Parent", false, 0)]
        static void CreateParentItem(MenuCommand menuCommand)
        {
            Object[] selection = Selection.GetFiltered(typeof(GameObject), SelectionMode.ExcludePrefab);
            if (selection == null || selection.Length == 0)
                return;
            GameObject[] selectedObjects = new GameObject[selection.Length];
            for (int i = 0; i < selection.Length; i++)
            {
                selectedObjects[i] = selection[i] as GameObject;
            }

            // Only execute once, not for each object
            if (selectedObjects.Length > 1)
            {
                if (((GameObject)menuCommand.context) != selectedObjects[0])
                {
                    return;
                }
            }

            GameObject parent = new GameObject("Parent");

            Undo.RegisterCreatedObjectUndo(parent, "CreatedParent");
            GameObject closestToRoot = FindClosestToRootSelectedGameObjectStatic(selectedObjects);
            parent.transform.SetParent(closestToRoot.transform.parent);
            parent.transform.position = selectedObjects[0].transform.position;
            parent.transform.SetSiblingIndex(closestToRoot.transform.GetSiblingIndex());
            for (int i = 0; i < selectedObjects.Length; i++)
            {
                Undo.SetTransformParent(selectedObjects[i].transform, parent.transform, "SetParent" + i);
            }
            Selection.activeObject = parent;

        }
        // -----------------------------------

        // ---------- Clear parent ----------
        [MenuItem("GameObject/ Clear Parent", true)]
        static bool ValidateClearParentItem()
        {
            Object[] selection = Selection.GetFiltered(typeof(GameObject), SelectionMode.ExcludePrefab);
            if (selection == null || selection.Length == 0)
                return false;
            return true;
        }

        [MenuItem("GameObject/ Clear Parent", false, 0)]
        static void ClearParentItem()
        {
            EditorApplication.ExecuteMenuItem("GameObject/Clear Parent");
        }
        // ----------------------------------

        [MenuItem("GameObject/ Center On.../Immediate Children", true)]
        static bool ValidateCenterOnChildrenItem()
        {
            Object[] selection = Selection.GetFiltered(typeof(GameObject), SelectionMode.ExcludePrefab);
            if (selection == null || selection.Length == 0)
                return false;
            for(int i = 0; i < selection.Length; i++)
            {
                if (((GameObject)selection[i]).transform.childCount > 0)
                    return true;
            }
            return false;
        }
        [MenuItem("GameObject/ Center On.../Immediate Children", false, 0)]
        static void CenterOnChildrenItem()
        {
            EditorApplication.ExecuteMenuItem("GameObject/Center On Children");
        }

        [MenuItem("GameObject/ Center On.../All Children", true)]
        static bool ValidateCenterOnAllChildrenItem()
        {
            Object[] selection = Selection.GetFiltered(typeof(GameObject), SelectionMode.ExcludePrefab);
            if (selection == null || selection.Length == 0)
                return false;
            for (int i = 0; i < selection.Length; i++)
            {
                if (((GameObject)selection[i]).transform.childCount > 0)
                    return true;
            }
            return false;
        }
        [MenuItem("GameObject/ Center On.../All Children", false, 0)]
        static void CenterOnAllChildrenItem()
        {
            
        }

        [MenuItem("GameObject/ Move To Floor Level", true)]
        static bool ValidateMoveToFloorItem()
        {
            Object[] selection = Selection.GetFiltered(typeof(GameObject), SelectionMode.ExcludePrefab);
            if (selection == null || selection.Length == 0)
                return false;
            //for(int i = 0; i < selection.Length; i++)
            //{
            //    if(!((GameObject)selection[i]).GetComponent<Collider>())
            //        return false;
            //}
            return true;
        }
        [MenuItem("GameObject/ Move To Floor Level", false, 0)]
        static void MoveToFloorItem()
        {
            Object[] selection = Selection.GetFiltered(typeof(GameObject), SelectionMode.ExcludePrefab);
            if (selection == null || selection.Length == 0)
                return;
            GameObject[] selectedObjects = new GameObject[selection.Length];
            for (int i = 0; i < selection.Length; i++)
            {
                selectedObjects[i] = selection[i] as GameObject;
            }

            Array.Sort(selectedObjects, new SortFromHierarchyDepthComparer());

            GameObject firstToMove = null;
            for(int i = 0; i < selectedObjects.Length; i++)
            {
                if (!selectedObjects[i].GetComponent<Collider>())
                    continue;
                if (!firstToMove)
                    firstToMove = selectedObjects[i];
                RaycastHit hit;
                float heightFactor = 0.0f; // if we're below the floor level, this factor will be higher to compensate the difference
                Vector3 position = selectedObjects[i].transform.position;
                if (Physics.Raycast(position, Vector3.down, out hit))
                    heightFactor = 1.0f;
                else if (Physics.Raycast(position, Vector3.up, out hit))
                    heightFactor = 2.0f;
                else if (Physics.Raycast(new Vector3(position.x, firstToMove.transform.position.y, position.z), Vector3.down, out hit)) // Check from the first object that has moved height
                    heightFactor = 2.0f;

                if(hit.collider != null)
                {
                    Undo.RecordObject(selectedObjects[i].transform, "MoveToFloor" + i);
                    Debug.Log(hit.collider.name);
                    selectedObjects[i].transform.position = new Vector3(selectedObjects[i].transform.position.x, (hit.point.y + selectedObjects[i].GetComponent<Collider>().bounds.extents.y * heightFactor), selectedObjects[i].transform.position.z);
                }
            }
        }

        class SortFromHierarchyDepthComparer : IComparer<GameObject>
        {
            public int Compare(GameObject x, GameObject y)
            {
                int xDepth = GetDepthInHierarchy(x);
                int yDepth = GetDepthInHierarchy(y);
                if (xDepth > yDepth)
                    return 1;
                else if (yDepth > xDepth)
                    return -1;
                else return 0;
            }
        }

        public static int GetDepthInHierarchy(GameObject go)
        {
            int i = 0;
            GameObject cursor = go; 
            while(cursor.transform.parent != null)
            {
                cursor = cursor.transform.parent.gameObject;
                i++;
            }
            return i;
        }

        void CreateParent()
        {
            GameObject parent = new GameObject(parentName);
           

            Undo.RegisterCreatedObjectUndo(parent, "CreatedParent");
            GameObject closestToRoot = FindClosestToRootSelectedGameObject();
            parent.transform.SetParent(closestToRoot.transform.parent);
            parent.transform.SetSiblingIndex(closestToRoot.transform.GetSiblingIndex());
            for (int i = 0; i < selectedObjects.Length; i++)
            {
                Undo.SetTransformParent(selectedObjects[i].transform, parent.transform, "SetParent" + i);
            }

            Selection.activeObject = parent;

            if (centerPivotPoint)
            {
                Vector3 pos = FindCenterOfChildren(parent);
                GameObject temp = new GameObject("temp");
                foreach(Transform child in parent.transform)
                {
                    child.SetParent(temp.transform);
                }
                parent.transform.position = pos;
                foreach (Transform child in temp.transform)
                {
                    child.SetParent(parent.transform);
                }
                DestroyImmediate(temp);
            }
                
            //if (centerPivotPoint)
            //    CenterOnChildrenItem();
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

        public static Vector3 FindCenterOfChildren(GameObject go)
        {
            Vector4 toReturn = Vector4.zero;
            for (int i = 0; i < go.transform.childCount; i++)
            {
                Transform child = go.transform.GetChild(i);
                toReturn += new Vector4(child.position.x, child.position.y, child.position.z);
                toReturn += FindCenterOfChildren4(child.gameObject);
            }

            return toReturn /= toReturn.w;
        }

        static Vector4 FindCenterOfChildren4(GameObject go)
        {
            Vector4 toReturn = Vector4.zero;
            for(int i = 0; i < go.transform.childCount; i++)
            {
                Transform child = go.transform.GetChild(i);
                toReturn += new Vector4(child.position.x, child.position.y, child.position.z, 1);
                toReturn += FindCenterOfChildren4(child.gameObject);
            }

            return toReturn;
        }

        void MoveToFloor()
        {

        }

        public static string getMethodsOn(System.Object t)
        {
            Type obj = t.GetType();
            string log = "METHODS FOR : " + obj.Name;
            MethodInfo[] method_info = obj.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            foreach (MethodInfo method in method_info)
            {
                string parameters = "";

                ParameterInfo[] param_info = method.GetParameters();
                if (0 < param_info.Length)
                {
                    for (int i = 0; i < param_info.Length; i++)
                    {
                        parameters += param_info[i].ParameterType.Name;
                        parameters += (i < (param_info.Length - 1)) ? ", " : "";
                    }
                }
                log += "\nFunction :" + method.Name + "(" + parameters + ")";
            }
            return log;
        }

        public static string getMembersOf(System.Object t)
        {
            Type obj = t.GetType();
            string log = "MEMBERS OF : " + obj.Name;
            MemberInfo[] member_info = obj.GetMembers(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            foreach (MemberInfo member in member_info)
            {
                log += "\nMember :" + member.Name;
            }
            return log;
        }

    }
}
