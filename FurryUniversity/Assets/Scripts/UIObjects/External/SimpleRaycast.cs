using UnityEngine;
using UnityEngine.UI;

namespace SFramework.Core.UI.External
{
    public class SimpleRaycast : Graphic
    {
        public bool IsRaycastLocationValid(Vector2 sp, Camera eventCamera)
        {
            return true;
        }

        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();
        }
    }
}