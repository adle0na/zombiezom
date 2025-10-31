using UnityEngine;
#if UNITY_EDITOR
using System;
using System.Reflection;
using UnityEditor;
#endif

public static class ResolutionScaler
{
    public const int TargetWidth = 1920;
    public const int TargetHeight = 1080;

    public static void ApplyTargetResolution()
    {
#if UNITY_EDITOR
        EnsureEditorGameViewResolution(TargetWidth, TargetHeight);
#endif
        if (Screen.width != TargetWidth || Screen.height != TargetHeight)
        {
            Screen.SetResolution(TargetWidth, TargetHeight, FullScreenMode.FullScreenWindow);
        }
    }

#if UNITY_EDITOR
    private static void EnsureEditorGameViewResolution(int width, int height)
    {
        var groupTypeValue = GetGameViewSizeGroupTypeValue("Standalone");
        if (groupTypeValue == null)
        {
            return;
        }

        int sizeIndex = FindGameViewSizeIndex(groupTypeValue, width, height);

        if (sizeIndex == -1)
        {
            AddCustomGameViewSize(groupTypeValue, width, height);
            sizeIndex = FindGameViewSizeIndex(groupTypeValue, width, height);
        }

        if (sizeIndex == -1)
        {
            return;
        }

        var gameViewType = typeof(Editor).Assembly.GetType("UnityEditor.GameView");
        if (gameViewType == null)
        {
            return;
        }

        var gameViewWindow = EditorWindow.GetWindow(gameViewType);
        var sizeSelectionCallback = gameViewType.GetMethod("SizeSelectionCallback", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        sizeSelectionCallback?.Invoke(gameViewWindow, new object[] { sizeIndex, null });
    }

    private static object GetGameViewSizesInstance()
    {
        var gameViewSizesType = typeof(Editor).Assembly.GetType("UnityEditor.GameViewSizes");
        if (gameViewSizesType == null)
        {
            return null;
        }

        var scriptableSingletonGeneric = typeof(Editor).Assembly.GetType("UnityEditor.ScriptableSingleton`1");
        if (scriptableSingletonGeneric == null)
        {
            return null;
        }

        var scriptableSingletonType = scriptableSingletonGeneric.MakeGenericType(gameViewSizesType);
        var instanceProperty = scriptableSingletonType.GetProperty("instance", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
        return instanceProperty?.GetValue(null, null);
    }

    private static object GetGroup(object groupType)
    {
        var instance = GetGameViewSizesInstance();
        if (instance == null)
        {
            return null;
        }

        var getGroupMethod = instance.GetType().GetMethod("GetGroup", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        return getGroupMethod?.Invoke(instance, new object[] { groupType });
    }

    private static int FindGameViewSizeIndex(object groupType, int width, int height)
    {
        var group = GetGroup(groupType);
        if (group == null)
        {
            return -1;
        }

        var groupTypeInstance = group.GetType();
        var getTotalCountMethod = groupTypeInstance.GetMethod("GetTotalCount", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        var getGameViewSizeMethod = groupTypeInstance.GetMethod("GetGameViewSize", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        if (getTotalCountMethod == null || getGameViewSizeMethod == null)
        {
            return -1;
        }

        int totalCount = (int)getTotalCountMethod.Invoke(group, null);
        var gameViewSizeType = typeof(Editor).Assembly.GetType("UnityEditor.GameViewSize");
        var widthProperty = gameViewSizeType.GetProperty("width");
        var heightProperty = gameViewSizeType.GetProperty("height");

        for (int i = 0; i < totalCount; i++)
        {
            var size = getGameViewSizeMethod.Invoke(group, new object[] { i });
            if (size == null)
            {
                continue;
            }

            int existingWidth = (int)widthProperty.GetValue(size, null);
            int existingHeight = (int)heightProperty.GetValue(size, null);

            if (existingWidth == width && existingHeight == height)
            {
                return i;
            }
        }

        return -1;
    }

    private static void AddCustomGameViewSize(object groupType, int width, int height)
    {
        var group = GetGroup(groupType);
        if (group == null)
        {
            return;
        }

        var gameViewSizeType = typeof(Editor).Assembly.GetType("UnityEditor.GameViewSize");
        if (gameViewSizeType == null)
        {
            return;
        }

        var gameViewSizeTypeEnum = typeof(Editor).Assembly.GetType("UnityEditor.GameViewSizeType");
        if (gameViewSizeTypeEnum == null)
        {
            return;
        }

        var gameViewSizeCtor = gameViewSizeType.GetConstructor(new[] { gameViewSizeTypeEnum, typeof(int), typeof(int), typeof(string) });
        if (gameViewSizeCtor == null)
        {
            return;
        }

        var fixedResolutionValue = GetEnumValueOrDefault(gameViewSizeTypeEnum, "FixedResolution");
        if (fixedResolutionValue == null)
        {
            return;
        }
        var newSize = gameViewSizeCtor.Invoke(new object[] { fixedResolutionValue, width, height, $"{width}x{height}" });

        var addCustomSizeMethod = group.GetType().GetMethod("AddCustomSize", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        addCustomSizeMethod?.Invoke(group, new object[] { newSize });
    }

    private static object GetGameViewSizeGroupTypeValue(string enumName)
    {
        var enumType = typeof(Editor).Assembly.GetType("UnityEditor.GameViewSizeGroupType");
        return GetEnumValueOrDefault(enumType, enumName);
    }

    private static object GetEnumValueOrDefault(Type enumType, string enumName)
    {
        if (enumType == null)
        {
            return null;
        }

        try
        {
            return Enum.Parse(enumType, enumName);
        }
        catch (ArgumentException)
        {
            return null;
        }
    }
#endif
}