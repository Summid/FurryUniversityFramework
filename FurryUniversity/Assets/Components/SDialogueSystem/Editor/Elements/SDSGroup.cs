using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace SDS.Elements
{
    public class SDSGroup : Group
    {
        public string ID { get; set; }
        public string OldTitle { get; set; }

        private Color defaultBorderColor;
        private float defaultBorderWidth;

        public SDSGroup(string groupTitle, Vector2 position)
        {
            this.ID = Guid.NewGuid().ToString();
            this.title = groupTitle;
            this.OldTitle = groupTitle;

            this.SetPosition(new Rect(position, Vector2.zero));

            this.defaultBorderColor = this.contentContainer.style.borderBottomColor.value;
            this.defaultBorderWidth = this.contentContainer.style.borderBottomWidth.value;
        }

        public void SetErrorStyle(Color color)
        {
            this.contentContainer.style.borderBottomColor = color;
            this.contentContainer.style.borderBottomWidth = 2f;
        }

        public void ResetStyle()
        {
            this.contentContainer.style.borderBottomColor = this.defaultBorderColor;
            this.contentContainer.style.borderBottomWidth = this.defaultBorderWidth;
        }
    }
}