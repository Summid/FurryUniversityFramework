using SFramework.Utilities;
using TMPro;
using UnityEditor;
using UnityEngine;

namespace SFramework.Core.UI.External
{
    public class TextMeshProUGUIEx : TextMeshProUGUI
    {
        protected override void Awake()
        {
            base.Awake();
            
            //在编辑器加载安卓AB，TMP的shader不会正确加载，需要重新加载一次(ˉ▽ˉ；)...
            #if UNITY_EDITOR && UNITY_ANDROID
            this.fontMaterial.shader = Shader.Find(this.fontMaterial.shader.name);
            #endif
        }
    }
}