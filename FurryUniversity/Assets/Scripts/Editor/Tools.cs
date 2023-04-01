using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Windows;

namespace SFramework.Utilities.Editor
{
    public class Tools : EditorWindow
    {
        [MenuItem("Tools/打开存档保存目录")]
        public static void OpenArchivePath()
        {
            if (Directory.Exists(StaticVariables.ArchivePath))
            {
                EditorUtility.RevealInFinder(StaticVariables.ArchivePath);            
            }
            EditorUtility.RevealInFinder(StaticVariables.PersistentAssetsPath);
        }
    }
}