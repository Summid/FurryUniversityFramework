using System;
using System.Collections.Generic;
using UnityEngine;

namespace SFramework.Core.UI
{
    [RequireComponent(typeof(ReferenceCollector))]
    public class UIItemSelector : MonoBehaviour
    {
        public static readonly Type INI_TYPE = typeof(int);
        public static readonly Type LONG_TYPE = typeof(long);
        public static readonly Type BOOL_TYPE = typeof(bool);
        public static readonly Type FLOAT_TYPE = typeof(float);
        public static readonly Type DOUBLE_TYPE = typeof(double);
        public static readonly Type STR_TYPE = typeof(string);
        public static readonly Type ENUM_TYPE = typeof(Enum);

        public string SelectClass = string.Empty;

        public Action OnGameObjectEnable { get; set; }
        public Action OnGameObjectDisable { get; set; }

        public object UIObject { get; set; }

        [SerializeField]
        public List<UIConfigParameter> UIConfigParam = new List<UIConfigParameter>();

        private void OnEnable()
        {
            this.OnGameObjectEnable?.Invoke();
        }

        private void OnDisable()
        {
            this.OnGameObjectDisable?.Invoke();
        }
    }

    [Serializable]
    public class UIConfigParameter
    {
        [SerializeField]
        public string Name;
        [SerializeField]
        public string Value;

        public int IntValue
        {
            get
            {
                int res = default;
                int.TryParse(this.Value, out res);
                return res;
            }
        }

        public long LongValue
        {
            get
            {
                long res = default;
                long.TryParse(this.Value, out res);
                return res;
            }
        }

        public float FloatValue
        {
            get
            {
                float res = default;
                float.TryParse(this.Value, out res);
                return res;
            }
        }

        public double DoubleValue
        {
            get
            {
                double res = default;
                double.TryParse(this.Value, out res);
                return res;
            }
        }

        public bool BoolValue
        {
            get
            {
                bool res = default;
                bool.TryParse(this.Value, out res);
                return res;
            }
        }

        public string StrValue
        {
            get
            {
                return this.Value;
            }
        }
    }
}