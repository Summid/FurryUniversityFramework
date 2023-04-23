using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SFramework.Utilities
{
    public static class UIUtility
    {
        public static Vector2 AnchorToPivot(this TextAnchor anchor)
        {
            switch (anchor)
            {
                case TextAnchor.UpperLeft:
                    return new Vector2(0, 1);
                case TextAnchor.UpperCenter:
                    return new Vector2(0.5f, 1);
                case TextAnchor.UpperRight:
                    return new Vector2(1, 1);
                case TextAnchor.MiddleLeft:
                    return new Vector2(0, 0.5f);
                case TextAnchor.MiddleCenter:
                    return new Vector2(0.5f, 0.5f);
                case TextAnchor.MiddleRight:
                    return new Vector2(1, 0.5f);
                case TextAnchor.LowerLeft:
                    return new Vector2(0, 0);
                case TextAnchor.LowerCenter:
                    return new Vector2(0.5f, 0);
                case TextAnchor.LowerRight:
                    return new Vector2(1, 0);
                default:
                    return new Vector2(0.5f, 0.5f);
            }
        }

        public static Vector3 ScreenToWorldPosition(this Vector3 screenPos, Canvas canvas)
        {
            if (canvas == null)
                return Vector3.zero;
            Camera uiCamera = canvas.worldCamera;

            screenPos.x = Mathf.Clamp(screenPos.x, 0, uiCamera.pixelWidth);
            screenPos.y = Mathf.Clamp(screenPos.y, 0, uiCamera.pixelHeight);

            RectTransformUtility.ScreenPointToWorldPointInRectangle(canvas.transform as RectTransform, screenPos,
                uiCamera, out Vector3 worldPoint);
            return worldPoint;
        }

        public static void ClampToScreenRect(RectTransform viewTransform, RectTransform rectTransform)
        {
            Canvas.ForceUpdateCanvases();
            FixPositionInView(viewTransform, rectTransform);
        }

        /// <summary>
        /// 将 <paramref name="rectTransform"/> 保持在 <paramref name="viewTransform"/> 里
        /// </summary>
        /// <param name="viewTransform"></param>
        /// <param name="rectTransform"></param>
        /// <remarks>
        /// 建议把 <paramref name="viewTransform"/> 和 <paramref name="rectTransform"/> 的 anchor 坐标设置为 (0.5, 0.5)
        /// </remarks>
        public static void FixPositionInView(RectTransform viewTransform, RectTransform rectTransform)
        {
            Rect viewRect = viewTransform.rect;
            Vector2 minRect = viewRect.min;
            Vector2 maxRect = viewRect.max;

            Rect rectRect = rectTransform.rect;
            Vector2 rectPivot = rectTransform.pivot;

            //minPos 增大，maxPos 减小到 rectRect.pivot 到其 rect 左边界和右边界的大小
            //得到的 [minPos, maxPos] 就是 rectTransform.anchoredPosition 的范围
            minRect.x += rectRect.width * rectPivot.x;
            minRect.y += rectRect.height * rectPivot.y;
            maxRect.x += rectRect.width * (rectPivot.x - 1);
            maxRect.y += rectRect.height * (rectPivot.y - 1);

            Vector2 rectAnchoredPos = rectTransform.anchoredPosition;
            rectAnchoredPos.x = Mathf.Clamp(rectAnchoredPos.x, minRect.x, maxRect.x);
            rectAnchoredPos.y = Mathf.Clamp(rectAnchoredPos.y, minRect.y, maxRect.y);
            rectTransform.anchoredPosition = rectAnchoredPos;
        }
    }
}