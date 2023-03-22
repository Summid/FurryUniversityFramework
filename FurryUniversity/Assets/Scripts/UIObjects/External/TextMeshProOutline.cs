using TMPro;
using UnityEngine;

namespace SFramework.Core.UI.External
{
    [DisallowMultipleComponent]
    public class TextMeshProOutline : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI tmp;
        [SerializeField] private float outlineWidth;
        [SerializeField] private Color32 outlineColor;

        private void Awake()
        {
            if (this.tmp == null)
            {
                if (!this.TryGetComponent(out this.tmp))
                {
                    Debug.LogError($"GameObject {this.gameObject.name} doesn't find TMP script. (TextMeshProOutline)");
                    return;
                }
            }
            
            this.tmp.outlineWidth = this.outlineWidth;
            this.tmp.outlineColor = this.outlineColor;
        }
    }
}