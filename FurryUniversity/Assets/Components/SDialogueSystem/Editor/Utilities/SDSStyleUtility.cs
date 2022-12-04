using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace SDS.Utilities
{
    public static class SDSStyleUtility
    {
        public static VisualElement AddStyleSheets(this VisualElement element, params string[] styleSheetNames)
        {
            foreach (string styleSheetName in styleSheetNames)
            {
                StyleSheet styleSheet = EditorGUIUtility.Load(styleSheetName) as StyleSheet;//从Editor Default Resources目录加载

                element.styleSheets.Add(styleSheet);
            }

            return element;
        }

        public static VisualElement AddClasses(this VisualElement element, params string[] classNames)
        {
            foreach (string className in classNames)
            {
                element.AddToClassList(className);
            }

            return element;
        }
    }
}