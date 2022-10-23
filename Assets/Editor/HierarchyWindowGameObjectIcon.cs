/*
using UnityEditor;
using UnityEngine;

/// <summary>
/// Hierarchy window game object icon.
/// http://diegogiacomelli.com.br/unitytips-hierarchy-window-gameobject-icon/
/// </summary>
[InitializeOnLoad]
public static class HierarchyWindowGameObjectIcon
{
    const string IgnoreIcons = "d_GameObject Icon, d_Prefab Icon";

    static HierarchyWindowGameObjectIcon()
    {
        EditorApplication.hierarchyWindowItemOnGUI += HandleHierarchyWindowItemOnGUI;
    }

    static void HandleHierarchyWindowItemOnGUI(int instanceID, Rect selectionRect)
    {
        var content = EditorGUIUtility.ObjectContent(EditorUtility.InstanceIDToObject(instanceID), null);
        if (content.image != null && !IgnoreIcons.Contains(content.image.name))
            GUI.DrawTexture(new Rect(selectionRect.xMax - 16, selectionRect.yMin, 16, 16), content.image);
    }
}
*/

using UnityEditor;
using UnityEngine;

/// <summary>
/// Hierarchy window game object icon.
/// http://diegogiacomelli.com.br/unitytips-hierarchy-window-gameobject-icon/
/// </summary>
[InitializeOnLoad]
public static class HierarchyWindowGameObjectIcon
{
    static string[] IgnoreIcons = new string[]{"d_GameObject Icon"};

    static HierarchyWindowGameObjectIcon()
    {
        EditorApplication.hierarchyWindowItemOnGUI += HandleHierarchyWindowItemOnGUI;
    }

    static void HandleHierarchyWindowItemOnGUI(int instanceID, Rect selectionRect)
    {
        var content = EditorGUIUtility.ObjectContent(EditorUtility.InstanceIDToObject(instanceID), null);
        if ((content.image != null) && (!Contains(content.image.name)))
        {
            //Debug.Log(content.image.name);
            GUI.DrawTexture(new Rect(selectionRect.xMax - 16, selectionRect.yMin, 16, 16), content.image);
        }
    }

    static bool Contains(string name)
    {
        foreach (string iconName in IgnoreIcons)
        {
            if (name == iconName)
                return true;
        }
        return false;
    }
}