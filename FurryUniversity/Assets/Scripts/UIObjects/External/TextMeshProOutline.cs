using TMPro;
using UnityEngine;

namespace SFramework.Core.UI.External
{
    public class TextMeshProOutline : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI tmp;
        [SerializeField] private float outlineWidth;
        [SerializeField] private Color32 outlineColor;

        private void Awake()
        {
            if(this.tmp == null)
                this.tmp = this.GetComponent<TextMeshProUGUI>();
            
            this.tmp.outlineWidth = this.outlineWidth;
            this.tmp.outlineColor = this.outlineColor;
        }
    }
}