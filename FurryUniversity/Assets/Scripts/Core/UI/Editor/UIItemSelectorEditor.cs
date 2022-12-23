using SFramework.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;

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

        private void DrawSerializeField()
        {
            var target = this.target as UIItemSelector;
            int index = this.canSelectClassList.IndexOf(target.SelectClass);
            if (index > 0)
            {
                Type uiObjectType = this.types[index - 1];//canSelectClassList默认有一条空字符串占位
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

        }
    }    
}