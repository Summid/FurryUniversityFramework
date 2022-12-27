using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor.SceneManagement;
using UnityEditor;
using System.Linq;
using SFramework.Utilities;
using System.Reflection;
using System;
using System.Text;
#if !UNITY_2021_3_OR_NEWER
using UnityEditor.Experimental.SceneManagement;
#endif

namespace SFramework.Core.UI.Editor
{
    [CustomEditor(typeof(ReferenceCollector))]
    [CanEditMultipleObjects]
    public class ReferenceCollectorEditor : UnityEditor.Editor
    {
        private ReferenceCollector referenceCollector;
        private UnityEngine.Object heroPrefab;
        private string searchKey = "";

        private string SearchKey
        {
            get
            {
                return this.searchKey;
            }
            set
            {
                if(this.searchKey != value)
                {
                    this.searchKey = value;
                    this.heroPrefab = this.referenceCollector.Get<UnityEngine.Object>(this.searchKey);
                }
            }
        }

        private void OnEnable()
        {
            if (this.targets.Count() > 1)//只允许一个target
            {
                return;
            }
            this.referenceCollector = this.target as ReferenceCollector;
        }

        public override void OnInspectorGUI()
        {
            if (this.targets.Count() > 1)
            {
                return;
            }
            Undo.RecordObject(this.referenceCollector, "Changed Settings");
            var dataProperty = this.serializedObject.FindProperty("data");

            EditorGUILayout.BeginHorizontal();
            {
                if (GUILayout.Button("全部删除"))
                {
                    dataProperty.ClearArray();
                    dataProperty.serializedObject.ApplyModifiedProperties();
                }
                if (GUILayout.Button("删除空引用"))
                {
                    this.DeleteNullReference();
                }
                if (GUILayout.Button("排序"))
                {
                    this.referenceCollector.Sort();
                }
                if (this.IsViewPrefab())
                {
                    if (GUILayout.Button("生成.g文件"))
                    {
                        var currentPrefabStage = PrefabStageUtility.GetCurrentPrefabStage();
                        if (currentPrefabStage != null)
                        {
                            if (currentPrefabStage.scene.isDirty)
                            {
                                if (EditorUtility.DisplayDialog("ん?", "是否先保存预制件", "确定", "取消"))
                                {
                                    PrefabUtility.SaveAsPrefabAsset(currentPrefabStage.prefabContentsRoot, currentPrefabStage.assetPath);
                                    KeyBdEvent.SaveKey();//模拟CTRL+S保存
                                    this.ReplaceViewCode(this.referenceCollector.name);
                                    return;
                                }
                            }
                        }
                        else
                        {
                            var propertyModifications = PrefabUtility.GetObjectOverrides(this.referenceCollector.gameObject);
                            if (propertyModifications.Count > 0)
                            {
                                if (EditorUtility.DisplayDialog("ん?", "是否先保存预制件", "确定", "取消"))
                                {
                                    PrefabUtility.ApplyPrefabInstance(this.referenceCollector.gameObject, InteractionMode.AutomatedAction);
                                    this.ReplaceViewCode(this.referenceCollector.name);
                                    return;
                                }
                            }
                        }
                        this.ReplaceViewCode(this.referenceCollector.name);
                    }
                }
                else
                {
                    if (GUILayout.Button("生成代码到剪切板"))
                    {
                        this.CopyCode();
                    }
                }
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            {
                this.SearchKey = EditorGUILayout.TextField(this.SearchKey);
                var obj = EditorGUILayout.ObjectField(this.heroPrefab, typeof(UnityEngine.Object), true);
                if (obj != null)
                {
                    var temp = DragAndDrop.objectReferences.FirstOrDefault(i => i == obj);
                    if (temp != null)
                    {
                        this.AddReference(dataProperty, temp.name, DragAndDrop.objectReferences);
                    }
                }
                if (GUILayout.Button("删除"))
                {
                    this.referenceCollector.Remove(this.SearchKey);
                    this.heroPrefab = null;
                }
            }
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space();

            List<int> delList = new List<int>();
            SerializedProperty property;
            for (int i = this.referenceCollector.data.Count - 1; i >= 0; i--)
            {
                EditorGUILayout.BeginHorizontal();
                property = dataProperty.GetArrayElementAtIndex(i).FindPropertyRelative("key");
                property.stringValue = EditorGUILayout.TextField(property.stringValue, GUILayout.Width(150));
                bool isList = dataProperty.GetArrayElementAtIndex(i).FindPropertyRelative("IsList").boolValue;
                if (!isList)
                {
                    property = dataProperty.GetArrayElementAtIndex(i).FindPropertyRelative("gameObject");
                    property.objectReferenceValue = EditorGUILayout.ObjectField(property.objectReferenceValue, typeof(UnityEngine.Object), true);
                }
                if (GUILayout.Button("X"))
                {
                    delList.Add(i);
                }
                EditorGUILayout.EndHorizontal();

                //显示数组元素
                if (isList)
                {
                    property = dataProperty.GetArrayElementAtIndex(i).FindPropertyRelative("gameObjectList");
                    for (int index = 0; index < property.arraySize; index++)
                    {
                        var go = property.GetArrayElementAtIndex(index);
                        go.objectReferenceValue = EditorGUILayout.ObjectField(go.objectReferenceValue, typeof(UnityEngine.Object), true);
                    }
                }
            }

            foreach (var i in delList)
            {
                dataProperty.DeleteArrayElementAtIndex(i);
            }
            this.serializedObject.ApplyModifiedProperties();
            this.serializedObject.UpdateIfRequiredOrScript();


        }

        /// <summary>
        /// 删除空引用的对象
        /// </summary>
        private void DeleteNullReference()
        {
            var dataProperty = this.serializedObject.FindProperty("data");
            for (int i = dataProperty.arraySize - 1; i >= 0; --i)
            {
                var gameObjectProperty = dataProperty.GetArrayElementAtIndex(i).FindPropertyRelative("gameObject");
                if (gameObjectProperty.objectReferenceValue == null)
                {
                    dataProperty.DeleteArrayElementAtIndex(i);
                }
            }
            dataProperty.serializedObject.ApplyModifiedProperties();
        }

        /// <summary>
        /// 是否是UIView
        /// </summary>
        /// <returns></returns>
        public bool IsViewPrefab()
        {
            if (this.referenceCollector.gameObject.GetComponent<UIItemSelector>() == null)
            {
                //UIView没有selector组件
                var stage = PrefabStageUtility.GetCurrentPrefabStage();//处于预制件模式下
                if (stage != null)
                {
                    if (stage.prefabContentsRoot == this.referenceCollector.gameObject)
                        return true;
                }
                else
                {
                    if (PrefabUtility.IsAnyPrefabInstanceRoot(this.referenceCollector.gameObject))
                        return true;
                }
            }

            return false;
        }

        private void AddReference(SerializedProperty dataProperty, string key, params UnityEngine.Object[] objects)
        {
            int index = dataProperty.arraySize;
            dataProperty.InsertArrayElementAtIndex(index);
            var element = dataProperty.GetArrayElementAtIndex(index);
            element.FindPropertyRelative("key").stringValue = key;
            if (objects.Length == 1)
            {
                element.FindPropertyRelative("gameObject").objectReferenceValue = objects[0];
                element.FindPropertyRelative("IsList").boolValue = false;
            }
            else if (objects.Length > 1)
            {
                var array = element.FindPropertyRelative("gameObjectList");
                element.FindPropertyRelative("IsList").boolValue = true;
                array.ClearArray();
                array.arraySize = objects.Length;
                for (int i = objects.Length - 1; i >= 0; --i)
                {
                    array.GetArrayElementAtIndex(i).objectReferenceValue = objects[i];
                }
            }
        }

        private void ReplaceViewCode(string prefabName)
        {
            string uiListInfoPath = StaticVariables.UIViewPrefabsPath + "/" + StaticVariables.UIListName;

            UIInfoList uiInfoList = AssetDatabase.LoadAssetAtPath<UIInfoList>(uiListInfoPath);
            UIViewInfo viewInfo = uiInfoList.UIList.FirstOrDefault(s => s.ViewAssetName.ToLower() == prefabName.ToLower());

            if(viewInfo != null)
            {
                UIManagerEditor.GenerateCode(new List<UIViewInfo>() { viewInfo });
            }
            else
            {
                Type[] types = Assembly.GetAssembly(typeof(UIViewBase)).GetTypes();
                var it = UIManagerEditor.GetViewInfoByPrefabName(prefabName);
                var uiViewInfo = it.Item1;
                int index = it.Item2;
                if (uiViewInfo != null)
                {
                    UIManagerEditor.GenerateCode(new List<UIViewInfo>() { uiViewInfo });
                    if (uiInfoList.UIList.Count > index)
                    {
                        uiInfoList.UIList.Insert(index, uiViewInfo);
                    }
                    else
                    {
                        uiInfoList.UIList.Add(uiViewInfo);
                    }
                    EditorUtility.SetDirty(uiInfoList);
                    AssetDatabase.SaveAssets();
                    var uiListImporter = AssetImporter.GetAtPath(uiListInfoPath);
                    uiListImporter.assetBundleName = StaticVariables.UIListBundleName;
                    uiListImporter.SaveAndReimport();
                }
                else
                {
                    Debug.LogError($"未找到{prefabName}的预制或绑定{prefabName}的脚本");
                }
            }
        }

        private void CopyCode()
        {
            var rc = this.referenceCollector;
            var selfSelector = this.referenceCollector.GetComponent<UIItemSelector>();
            if (rc != null)
            {
                StringBuilder uiFieldBuilder = new StringBuilder();
                foreach (var rcData in rc.data)
                {
                    if ((rcData.IsList && rcData.gameObjectList.Count == 0) && rcData.gameObject == null)
                    {
                        Debug.LogError($"{rcData.key}所指向的GameObject为空，跳过");
                        continue;
                    }
                    string name = rcData.key;
                    GameObject go = (rcData.IsList ? rcData.gameObjectList[0] : rcData.gameObject) as GameObject;
                    if (go == null)
                    {
                        Debug.LogError($"{rcData.key}指定的gameObject为空");
                        continue;
                    }

                    var scripts = go.GetComponents<Behaviour>().Where(s => (!(s is ReferenceCollector)) && (!(s is IIgnoreUIGenCode))).ToList();
                    if (scripts.Count == 0)
                    {
                        uiFieldBuilder.AppendLine(CreateUIField(typeof(GameObject), name, name, rcData.IsList));
                    }
                    else if (scripts.Count == 1)
                    {
                        if (selfSelector != null && scripts[0] == selfSelector)
                            continue;//不要误伤了自己
                        Type fieldType = null;
                        if (scripts[0] is UIItemSelector selector)
                        {
                            if (string.IsNullOrEmpty(selector.SelectClass))
                            {
                                continue;
                            }
                            var assembly = Assembly.GetAssembly(typeof(UIItemBase));
                            fieldType = assembly.GetType(selector.SelectClass);
                        }
                        else
                        {
                            fieldType = scripts[0].GetType();
                        }
                        Debug.Log(name);
                        if (fieldType != null)
                        {
                            uiFieldBuilder.AppendLine(CreateUIField(fieldType, name, name, rcData.IsList));
                        }
                        else
                        {
                            Debug.LogError($"[{rcData.key}]有错误");
                        }
                    }
                    else
                    {
                        foreach (var script in scripts)
                        {
                            Type fieldType = null;
                            if(script is UIItemSelector selector)
                            {
                                if (selfSelector != null && script == selfSelector)
                                    continue;//排除自己
                                if (string.IsNullOrEmpty(selector.SelectClass))
                                {
                                    continue;
                                }
                                var assembly = Assembly.GetAssembly(typeof(UIItemBase));
                                fieldType = assembly.GetType(selector.SelectClass);
                            }
                            else
                            {
                                fieldType = script.GetType();
                            }
                            string typeSuffix = fieldType.Name;
                            uiFieldBuilder.AppendLine(CreateUIField(fieldType, $"{name}_{typeSuffix}", name, rcData.IsList));
                        }
                    }
                }
                GUIUtility.systemCopyBuffer = uiFieldBuilder.ToString();//拷贝到剪切板

            }
        }

        private static string CreateUIField(Type type, string fieldName, string keyName, bool isList)
        {
            string result = $"        [UIFieldInit(\"{keyName}\")]\r\n";
            if (isList)
                return result + $"        public List<{type.FullName}> {fieldName};";
            else
                return result + $"        public {type.FullName} {fieldName};";
        }
    }
}