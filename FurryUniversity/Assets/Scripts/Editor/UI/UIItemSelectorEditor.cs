using SFramework.EditorExtensions;
using SFramework.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEditor.SceneManagement;
#if !UNITY_2021_3_OR_NEWER
using UnityEditor.Experimental.SceneManagement;
#endif

namespace SFramework.Core.UI.Editor
{
    [CustomEditor(typeof(UIItemSelector))]
    public class UIItemSelectorEditor : UnityEditor.Editor
    {
        private List<string> canSelectClassList;
        private List<Type> types;

        private void OnEnable()
        {
            this.canSelectClassList = new List<string> { string.Empty };
            this.types = typeof(UIItemBase).GetSubTypesInAssemblies().ToList();

            this.canSelectClassList.AddRange(this.types.Select(t => t.FullName));
        }

        public override void OnInspectorGUI()
        {
            var target = this.target as UIItemSelector;

            if (this.canSelectClassList.Count > 0)
            {
                var options = this.canSelectClassList.Select(classStr => string.IsNullOrEmpty(classStr) ? "<NULL>" : classStr).ToArray();

                int selectIndex = this.canSelectClassList.IndexOf(target.SelectClass);

                var rect = EditorGUILayout.BeginVertical();
                {
                    if (GUILayout.Button(selectIndex == -1 ? $"{target.SelectClass}(missing)" : options[selectIndex]))
                    {
                        SearchablePopup.Show(rect, options, selectIndex, (select, selectName) =>
                        {
                            target.SelectClass = this.canSelectClassList[select];
                            Undo.RegisterCompleteObjectUndo(target, nameof(UIItemSelector));
                            EditorUtility.SetDirty(target);
                        });
                    }
                }
                EditorGUILayout.EndVertical();
            }

            this.DrawSerializeField();

            if (GUI.changed)
            {
                Undo.RegisterCompleteObjectUndo(target, nameof(UIItemSelector));

                var prefabStage = PrefabStageUtility.GetPrefabStage(target.gameObject);
                if(prefabStage != null)
                {
                    EditorSceneManager.MarkSceneDirty(prefabStage.scene);
                }
                else
                {
                    EditorUtility.SetDirty(target);
                }
            }
        }

        private void DrawSerializeField()
        {
            var target = this.target as UIItemSelector;
            int index = this.canSelectClassList.IndexOf(target.SelectClass);
            if (index > 0)
            {
                Type uiObjectType = this.types[index - 1];//canSelectClassList默认有一条空字符串占位，对应到types中索引 - 1
                FieldInfo[] fields = uiObjectType.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                fields = fields.Where(f => f.GetCustomAttribute<UISerializableAttribute>() != null).ToArray();

                foreach(var field in fields)
                {
                    this.DrawFieldUI(field, target);
                }
            }
        }

        private void DrawFieldUI(FieldInfo fieldInfo, UIItemSelector selector)
        {
            //去重
            var repeat = selector.UIConfigParam.Where(s => s.Name == fieldInfo.Name).ToList();
            if (repeat.Count > 1)
            {
                for (int i = 1; i < repeat.Count; ++i)
                {
                    selector.UIConfigParam.Remove(repeat[i]);
                }
            }

            bool newParam = false;
            UIConfigParameter param = selector.UIConfigParam.FirstOrDefault(s => s.Name == fieldInfo.Name);
            if (param == null)
            {
                newParam = true;
                var tempObj = Activator.CreateInstance(fieldInfo.DeclaringType);
                var defaultValue = fieldInfo.GetValue(tempObj);
                if (defaultValue == null)//引用类型？不伺候了
                {
                    return;
                }
                param = new UIConfigParameter { Name = fieldInfo.Name, Value = defaultValue.ToString() };
            }

            if (fieldInfo.FieldType == UIItemSelector.INI_TYPE)
            {
                int.TryParse(param.Value, out int Value);
                Value = EditorGUILayout.IntField(param.Name, Value);
                param.Value = Value.ToString();
            }
            else if (fieldInfo.FieldType == UIItemSelector.LONG_TYPE)
            {
                long.TryParse(param.Value, out long Value);
                Value = EditorGUILayout.LongField(param.Name, Value);
                param.Value = Value.ToString();
            }
            else if (fieldInfo.FieldType == UIItemSelector.BOOL_TYPE)
            {
                bool.TryParse(param.Value, out bool Value);
                Value = EditorGUILayout.Toggle(param.Name, Value);
                param.Value = Value.ToString();
            }
            else if (fieldInfo.FieldType == UIItemSelector.FLOAT_TYPE)
            {
                float.TryParse(param.Value, out float Value);
                Value = EditorGUILayout.FloatField(param.Name, Value);
                param.Value = Value.ToString();
            }
            else if (fieldInfo.FieldType == UIItemSelector.DOUBLE_TYPE)
            {
                double.TryParse(param.Value, out double Value);
                Value = EditorGUILayout.DoubleField(param.Name, Value);
                param.Value = Value.ToString();
            }
            else if (fieldInfo.FieldType == UIItemSelector.STR_TYPE)
            {
                param.Value = EditorGUILayout.TextField(param.Name, param.Value);
            }
            else if (fieldInfo.FieldType.IsSubclassOf(UIItemSelector.ENUM_TYPE))
            {
                var enumNames = Enum.GetNames(fieldInfo.FieldType);
                var enumValues = Enum.GetValues(fieldInfo.FieldType).Cast<int>().ToArray();
                int.TryParse(param.Value, out int Value);

                Value = EditorGUILayout.IntPopup(fieldInfo.Name, Value, enumNames, enumValues);
                param.Value = Value.ToString();
            }

            if (GUI.changed && newParam)
            {
                selector.UIConfigParam.Add(param);
            }
        }
    }    
}