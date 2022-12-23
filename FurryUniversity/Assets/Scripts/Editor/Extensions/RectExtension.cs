using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SFramework.EditorExtensions
{
    public enum AnchorType
    {
        UpperLeft = 0,
        UpperCenter = 1,
        UpperRight = 2,
        MiddleLeft = 3,
        MiddleCenter = 4,
        MiddleRight = 5,
        LowerLeft = 6,
        LowerCenter = 7,
        LowerRight = 8,
    }

    public enum SplitType
    {
        Vertical,
        Horizontal,
    }

    public static class RectExtension
    {
        //缩放，pixel可为负
        public static Rect Zoom(this Rect rect, float pixel, AnchorType anchorType)
        {
            return Zoom(rect, new Vector2(pixel, pixel), anchorType);
        }

        public static Rect Zoom(this Rect rect, Vector2 pixelOffset, AnchorType anchorType)
        {
            float tempW = rect.width + pixelOffset.x;
            float tempH = rect.height + pixelOffset.y;
            switch (anchorType)
            {
                case AnchorType.UpperLeft:
                    break;
                case AnchorType.UpperCenter:
                    rect.x -= (tempW - rect.width) * 0.5f;
                    break;
                case AnchorType.UpperRight:
                    rect.x -= tempW - rect.width;
                    break;
                case AnchorType.MiddleLeft:
                    rect.y -= (tempH - rect.height) * 0.5f;
                    break;
                case AnchorType.MiddleCenter:
                    rect.x -= (tempW - rect.width) * 0.5f;
                    rect.y -= (tempH - rect.height) * 0.5f;
                    break;
                case AnchorType.MiddleRight:
                    rect.x -= tempW - rect.width;
                    rect.y -= (tempH - rect.height) * 0.5f;
                    break;
                case AnchorType.LowerLeft:
                    rect.y -= tempH - rect.height;
                    break;
                case AnchorType.LowerCenter:
                    rect.x -= (tempW - rect.width) * 0.5f;
                    rect.y -= tempH - rect.height;
                    break;
                case AnchorType.LowerRight:
                    rect.x -= tempW - rect.width;
                    rect.y -= tempH - rect.height;
                    break;
            }
            rect.width = tempW;
            rect.height = tempH;
            return rect;
        }

        #region 切割Rect

        /// <summary>
        /// 切割Rect
        /// </summary>
        /// <param name="self"></param>
        /// <param name="splitType"></param>
        /// <param name="size"></param>
        /// <param name="padding"></param>
        /// <param name="justMid"></param>
        /// <returns></returns>
        public static Rect[] Split(this Rect self, SplitType splitType, float size, float padding, bool justMid = true)
        {
            if (splitType == SplitType.Vertical)
            {
                return VerticalSplit(self, size, padding, justMid);
            }
            else if (splitType == SplitType.Horizontal)
            {
                return HorizontalSplit(self, size, padding, justMid);
            }
            return null;
        }

        public static Rect[] VerticalSplit(this Rect self, float width, float padding = 0, bool justMid = true)
        {
            if (justMid)
            {
                //两个Rect之间留有padding的距离
                return new Rect[2]
                {
                    self.CutRight(self.width - width).CutRight(padding).CutRight(-Mathf.CeilToInt(padding / 2f)),
                    self.CutLeft(width).CutLeft(padding).CutLeft(-Mathf.CeilToInt(padding / 2f)),
                };
            }

            return new Rect[2]
            {
                new Rect(0,0,0,0),
                new Rect(0,0,0,0)
            };
        }

        public static Rect[] HorizontalSplit(this Rect self, float height, float padding = 0, bool justMid = true)
        {
            if (justMid)
            {
                //两个Rect之间留有padding的距离
                return new Rect[2]
                {
                    self.CutButtom(self.height - height).CutButtom(padding).CutButtom(-Mathf.CeilToInt(padding / 2f)),
                    self.CutTop(height).CutTop(padding).CutTop(-Mathf.CeilToInt(padding / 2f)),
                };
            }

            return new Rect[2]
            {
                new Rect(0,0,0,0),
                new Rect(0,0,0,0)
            };
        }
        #endregion

        #region 获取中间padding的rect
        /// <summary>
        /// 获取中间 padding 的 rect
        /// </summary>
        /// <param name="self"></param>
        /// <param name="splitType"></param>
        /// <param name="size"></param>
        /// <param name="padding"></param>
        /// <returns></returns>
        public static Rect SplitRect(this Rect self, SplitType splitType, float size, float padding = 0)
        {
            if (splitType == SplitType.Vertical)
            {
                return VerticalSplitRect(self, size, padding);
            }
            else if (splitType == SplitType.Horizontal)
            {
                return HorizontalSplitRect(self, size, padding);
            }
            return new Rect(0, 0, 0, 0);
        }

        //获取中间 padding 的 rect
        public static Rect VerticalSplitRect(this Rect self, float width, float padding = 0)
        {
            Rect rect = self.CutRight(self.width - width).CutRight(padding).CutRight(-Mathf.CeilToInt(padding / 2));
            rect.x += rect.width;
            rect.width = padding;
            return rect;
        }

        //获取中间 padding 的 rect
        public static Rect HorizontalSplitRect(this Rect self, float height, float padding = 0)
        {
            Rect rect = self.CutButtom(self.height - height).CutButtom(padding).CutButtom(-Mathf.CeilToInt(padding / 2));
            rect.y += rect.height;
            rect.height = padding;
            return rect;
        }
        #endregion

        #region 切割方法
        public static Rect CutRight(this Rect self, float pixels)
        {
            self.xMax -= pixels;
            return self;
        }

        public static Rect CutLeft(this Rect self, float pixels)
        {
            self.xMin += pixels;
            return self;
        }

        public static Rect CutTop(this Rect self, float pixels)
        {
            self.yMin += pixels;
            return self;
        }

        public static Rect CutButtom(this Rect self, float pixels)
        {
            self.yMax -= pixels;
            return self;
        }
        #endregion
    }
}