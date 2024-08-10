#if UNITY_EDITOR 
using System.Collections.Generic; 
using UnityEditor;
using UnityEngine;
 
[InitializeOnLoad]
public static class HierarchyIcons
{
    const string MENU_ENABLE_ICONS = "Tools/HierarchyIcons/Enable Icons";
    const string MENU_GROUP_ICONS = "Tools/HierarchyIcons/Group Icons";
    const string MENU_VIEW_TRANSFORMS = "Tools/HierarchyIcons/View Transforms"; 
    static bool EnableIcons = true;
    static bool GroupIcons = true;
    static bool ViewTransforms = true;
    const float Spacing = 5;
    const float GoLabelMargin = 15;
    const float GroupCountLabelWidth = 10;
    const float GroupCountLabelMargin = 3;

    static HierarchyIcons() 
    {
        EditorApplication.hierarchyWindowItemOnGUI += OnHierGUI;

        EditorApplication.delayCall += LoadDefaults;
    }
    static void LoadDefaults()
    {
        EnableIcons = EditorPrefs.GetBool(MENU_ENABLE_ICONS, EnableIcons);
        Menu.SetChecked(MENU_ENABLE_ICONS, EnableIcons);        
        
        GroupIcons = EditorPrefs.GetBool(MENU_GROUP_ICONS, GroupIcons);
        Menu.SetChecked(MENU_GROUP_ICONS, GroupIcons);

        ViewTransforms = EditorPrefs.GetBool(MENU_VIEW_TRANSFORMS, ViewTransforms);
        Menu.SetChecked(MENU_VIEW_TRANSFORMS, ViewTransforms);
    }

    [MenuItem(MENU_ENABLE_ICONS)]
    static void ToggleIcons()
    {
        EnableIcons = !EnableIcons;
        SetEditorPref(MENU_ENABLE_ICONS, EnableIcons);
    }

    [MenuItem(MENU_GROUP_ICONS)]
    static void ToggleGroupIcons()
    {
        GroupIcons = !GroupIcons;
        SetEditorPref(MENU_GROUP_ICONS, GroupIcons);
    }

    [MenuItem(MENU_VIEW_TRANSFORMS)]
    static void ToggleViewTransforms()
    {
        ViewTransforms = !ViewTransforms;
        SetEditorPref(MENU_VIEW_TRANSFORMS, ViewTransforms);
    } 

    static void SetEditorPref(string menuPref, bool value)
    {
        EditorPrefs.SetBool(menuPref, value);
        Menu.SetChecked(menuPref, value);
        EditorApplication.RepaintHierarchyWindow();
    }

    static void OnHierGUI(int instanceID, Rect drawRect)
    {
        if (!EnableIcons)
            return;

        var obj = EditorUtility.InstanceIDToObject(instanceID);
        if (obj is not GameObject go)
            return;
        var goLabelWidth = GUI.skin.label.CalcSize(new GUIContent(go.name)).x;
        var textMaxX = drawRect.min.x + goLabelWidth + GoLabelMargin;
         
        drawRect.x = drawRect.width;
        drawRect.width = drawRect.height;
        var margins = drawRect.width + Spacing;

        var iconsGroup = new Dictionary<Texture, (Rect rect, int count) >();

        var compCount = go.GetComponentCount();
        for (int i = 0; i < compCount && drawRect.min.x > textMaxX; i++)
        {
            var comp = go.GetComponentAtIndex(i);
            if (!comp || comp is CanvasRenderer)
                continue;

            if (!ViewTransforms && comp is Transform)
                continue;

            Texture icon = EditorGUIUtility.GetIconForObject(comp); 
            if (!icon)
                icon = EditorGUIUtility.ObjectContent(comp, comp.GetType())?.image;
            if (!icon)
                continue;

            if(GroupIcons)
            {
                if (iconsGroup.TryGetValue(icon, out (Rect, int) rectAndCount))
                {
                    rectAndCount.Item2++;
                    iconsGroup[icon] = rectAndCount;
                }
                else
                {
                    iconsGroup.Add(icon, (drawRect, 1));
                    drawRect.x -= margins;
                }
            }
            else
            {
                GUI.DrawTexture(drawRect, icon, ScaleMode.ScaleAndCrop);
                drawRect.x -= margins;
            }
        }

        if (!GroupIcons)
            return;

        var widthLabelOffset = 0f;
        foreach (var iconGroupData in iconsGroup)
        {
            var icon = iconGroupData.Key;
            var rect = iconGroupData.Value.rect;
            var count = iconGroupData.Value.count;
            rect.x -= widthLabelOffset;
            if (rect.min.x < textMaxX)
                return;

            GUI.DrawTexture(rect, icon);
            if (count > 1)
            { 
                rect.width = GroupCountLabelWidth;
                rect.xMin -= GroupCountLabelWidth;
                if (rect.min.x < textMaxX)
                    return; 
                widthLabelOffset += rect.width / 2;
                GUI.Label(rect, count.ToString());
            }
        }
    }



}
#endif