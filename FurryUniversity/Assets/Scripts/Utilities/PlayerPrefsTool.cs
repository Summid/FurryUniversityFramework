using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SFramework.Utilities
{
    public static class PlayerPrefsTool
    {
        #region Pref
        /// <summary> 背景音乐开关 </summary>
        public static readonly Pref<int> Music_On = new Pref<int>("musicon", 1);
        /// <summary> 音效开关 </summary>
        public static readonly Pref<int> SFX_On = new Pref<int>("sfxon", 1);
        #endregion

        public abstract class PrefBase<T>
        {
            protected readonly string KeyName;
            protected readonly T DefaultValue;

            public PrefBase(string keyName, T defaultValue)
            {
                this.KeyName = keyName;
                this.DefaultValue = defaultValue;
            }

            protected virtual string GetKey(string keyPost)
            {
                return this.KeyName + keyPost;
            }

            public void SetValue(T newValue, string keyPost = "")
            {
                this.OnSetValue(this.GetKey(keyPost), newValue);
            }

            public T GetValue(string keyPost = "")
            {
                return this.OnGetValue(this.GetKey(keyPost));
            }

            public bool HasKey(string keyPost = "")
            {
                return PlayerPrefs.HasKey(this.GetKey(keyPost));
            }

            public void Clear(string keyPost = "")
            {
                PlayerPrefs.DeleteKey(this.GetKey(keyPost));
            }

            protected void OnSetValue(string key, T newValue)
            {
                if (newValue is string)
                {
                    PlayerPrefs.SetString(key, newValue as string);
                }
                else if (newValue is int)
                {
                    PlayerPrefs.SetInt(key, (int)(object)newValue);
                }
                else if (newValue is float)
                {
                    PlayerPrefs.SetFloat(key, (float)(object)newValue);
                }
                else if (newValue is bool)
                {
                    PlayerPrefs.SetInt(key, (bool)(object)newValue ? 1 : 0);
                }
                else
                {
                    Debug.LogError("PlayerPrefsTool" + typeof(T).Name + " is not support");
                }
            }

            protected T OnGetValue(string key)
            {
                if (typeof(T) == typeof(string))
                {
                    return (T)(object)(PlayerPrefs.GetString(key, (string)(object)this.DefaultValue));
                }
                else if (typeof(T) == typeof(int))
                {
                    return (T)(object)(PlayerPrefs.GetInt(key, (int)(object)this.DefaultValue));
                }
                else if (typeof(T) == typeof(float))
                {
                    return (T)(object)(PlayerPrefs.GetFloat(key, (float)(object)this.DefaultValue));
                }
                else if (typeof(T) == typeof(bool))
                {
                    return (T)(object)(PlayerPrefs.GetInt(key, (bool)(object)this.DefaultValue ? 1 : 0) == 1);
                }
                else
                {
                    Debug.LogError("PlayerPrefsTool" + typeof(T).Name + " is not support");
                    return default;
                }
            }
        }

        public class Pref<T> : PrefBase<T>
        {
            public Pref(string keyName, T defaultValue) : base(keyName, defaultValue)
            {
            }
        }
    }
}