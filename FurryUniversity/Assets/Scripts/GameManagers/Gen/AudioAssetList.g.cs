using System.Collections.Generic;
using UnityEngine;

namespace SFramework.Core.Audio
{
    public static class AudioAssetList
    {
        public static Dictionary<string, AudioLoader.AudioAssetType> audioAssetInfo = new Dictionary<string, AudioLoader.AudioAssetType>();
        public static Dictionary<string, string> sfxGroupAssetInfo = new Dictionary<string, string>();
        public static void Init()
        {
            audioAssetInfo.Add("TheLastCity",AudioLoader.AudioAssetType.BGM);
            audioAssetInfo.Add("ui_effect_14",AudioLoader.AudioAssetType.CommonSFX);
            audioAssetInfo.Add("ui_effect_5",AudioLoader.AudioAssetType.CommonSFX);
            audioAssetInfo.Add("ui_effect_6",AudioLoader.AudioAssetType.CommonSFX);
            sfxGroupAssetInfo.Add("ui_effect_48","mainview_sfxgroup");
            sfxGroupAssetInfo.Add("ui_effect_94","mainview_sfxgroup");
            sfxGroupAssetInfo.Add("ui_effect_95","mainview_sfxgroup");
            sfxGroupAssetInfo.Add("ui_effect_45","testview_sfxgroup");
            sfxGroupAssetInfo.Add("ui_effect_46","testview_sfxgroup");
            sfxGroupAssetInfo.Add("ui_effect_47","testview_sfxgroup");

        }
    }
}