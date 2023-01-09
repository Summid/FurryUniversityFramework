#if UNITY_EDITOR
using System;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;

/// <summary>
/// 修复Unity在IL2CPP下打包报错bug，该bug在2020.3.42f1, 2021.3.14f1, 2022.1.23f1, 2022.2.0b16 and 2023.1.0a19 后已被修复
/// <see href="https://forum.unity.com/threads/workaround-for-building-with-il2cpp-with-visual-studio-2022-17-4.1355570/"/>
/// </summary>
public class MsvcStdextWorkaround : IPreprocessBuildWithReport
{
    const string kWorkaroundFlag = "/D_SILENCE_STDEXT_HASH_DEPRECATION_WARNINGS";

    public int callbackOrder => 0;

    public void OnPreprocessBuild(BuildReport report)
    {
        var clEnv = Environment.GetEnvironmentVariable("_CL_");

        if (string.IsNullOrEmpty(clEnv))
        {
            Environment.SetEnvironmentVariable("_CL_", kWorkaroundFlag);
        }
        else if (!clEnv.Contains(kWorkaroundFlag))
        {
            clEnv += " " + kWorkaroundFlag;
            Environment.SetEnvironmentVariable("_CL_", clEnv);
        }
    }
}

#endif // UNITY_EDITOR