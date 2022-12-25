using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SFramework.Core.UI
{
    public abstract class UIObject
    {

    }

    [AttributeUsage(AttributeTargets.Field)]
    public class UIFieldInitAttribute : Attribute
    {
        public string RCKey { get; private set; }

        public UIFieldInitAttribute(string rcKey)
        {
            this.RCKey = rcKey;
        }
    }

    [AttributeUsage(AttributeTargets.Field)]
    public class UISerializableAttribute : Attribute
    {

    }
}