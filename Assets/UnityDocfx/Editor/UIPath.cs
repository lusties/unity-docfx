using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Lustie.UnityDocfx
{
    public static class UIPath
    {
        public static string UxmlAssetPath(string fileName)
        {
            return AssetDatabase.GetAssetPath(LoadUxml(fileName));
        }

        public static VisualTreeAsset LoadUxml(string fileName)
        {
            return Resources.Load<VisualTreeAsset>(UxmlResourcesPath(fileName));
        }

        public static StyleSheet LoadUss(string fileName)
        {
            return Resources.Load<StyleSheet>(UssResourcesPath(fileName));
        }

        private static string UxmlResourcesPath(string fileName)
        {
            return $"{fileName.Replace(".uxml", "")}";
        }

        private static string UssResourcesPath(string fileName)
        {
            return $"{fileName.Replace(".uss", "")}";
        }
    }
}