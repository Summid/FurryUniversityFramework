
using SFramework.Threading.Tasks.Internal;
using System;
using System.Linq;
using System.Threading;
using UnityEditor;
using UnityEngine;
using UnityEngine.LowLevel;

namespace SFramework.Threading.Tasks
{
    public static class STaskLoopRunners
    {
        public struct STaskLoopRunnerInitialization { };
        public struct STaskLoopRunnerEarlyUpdate { };
        public struct STaskLoopRunnerFixedUpdate { };
        public struct STaskLoopRunnerPreUpdate { };
        public struct STaskLoopRunnerUpdate { };
        public struct STaskLoopRunnerPreLateUpdate { };
        public struct STaskLoopRunnerPostLateUpdate { };

        // Last

        public struct STaskLoopRunnerLastInitialization { };
        public struct STaskLoopRunnerLastEarlyUpdate { };
        public struct STaskLoopRunnerLastFixedUpdate { };
        public struct STaskLoopRunnerLastPreUpdate { };
        public struct STaskLoopRunnerLastUpdate { };
        public struct STaskLoopRunnerLastPreLateUpdate { };
        public struct STaskLoopRunnerLastPostLateUpdate { };

        // Yield

        public struct STaskLoopRunnerYieldInitialization { };
        public struct STaskLoopRunnerYieldEarlyUpdate { };
        public struct STaskLoopRunnerYieldFixedUpdate { };
        public struct STaskLoopRunnerYieldPreUpdate { };
        public struct STaskLoopRunnerYieldUpdate { };
        public struct STaskLoopRunnerYieldPreLateUpdate { };
        public struct STaskLoopRunnerYieldPostLateUpdate { };

        // Yield Last

        public struct STaskLoopRunnerLastYieldInitialization { };
        public struct STaskLoopRunnerLastYieldEarlyUpdate { };
        public struct STaskLoopRunnerLastYieldFixedUpdate { };
        public struct STaskLoopRunnerLastYieldPreUpdate { };
        public struct STaskLoopRunnerLastYieldUpdate { };
        public struct STaskLoopRunnerLastYieldPreLateUpdate { };
        public struct STaskLoopRunnerLastYieldPostLateUpdate { };

        //需要 Unity 2020.2 以及更新版本支持
        public struct STaskLoopRunnerTimeUpdate { };
        public struct STaskLoopRunnerLastTimeUpdate { };
        public struct STaskLoopRunnerYieldTimeUpdate { };
        public struct STaskLoopRunnerLastYieldTimeUpdate { };
    }

    [Flags]
    public enum InjectPlayerLoopTimings
    {
        /// <summary>
        /// 预设: All loops(default).
        /// </summary>
        All =
            Initialization | LastInitialization |
            EarlyUpdate | LastEarlyUpdate |
            FixedUpdate | LastFixedUpdate |
            PreUpdate | LastPreUpdate |
            Update | LastUpdate |
            PreLateUpdate | LastPreLateUpdate |
            PostLateUpdate | LastPostLateUpdate
            //需要 Unity 2020.2 以及更新版本支持
            | TimeUpdate | LastTimeUpdate,


        /// <summary>
        /// 预设: 排除 LastPostLateUpdate
        /// </summary>
        Standard =
            Initialization |
            EarlyUpdate |
            FixedUpdate |
            PreUpdate |
            Update |
            PreLateUpdate |
            PostLateUpdate | LastPostLateUpdate
        //需要 Unity 2020.2 以及更新版本支持
            | TimeUpdate,

        /// <summary>
        /// 预设: 最小配置, Update | FixedUpdate | LastPostLateUpdate
        /// </summary>
        Minimum =
            Update | FixedUpdate | LastPostLateUpdate,


        // PlayerLoopTiming

        Initialization = 1,
        LastInitialization = 2,

        EarlyUpdate = 4,
        LastEarlyUpdate = 8,

        FixedUpdate = 16,
        LastFixedUpdate = 32,

        PreUpdate = 64,
        LastPreUpdate = 128,

        Update = 256,
        LastUpdate = 512,

        PreLateUpdate = 1024,
        LastPreLateUpdate = 2048,

        PostLateUpdate = 4096,
        LastPostLateUpdate = 8192,
        //需要 Unity 2020.2 以及更新版本支持
        // Unity 2020.2 added TimeUpdate https://docs.unity3d.com/2020.2/Documentation/ScriptReference/PlayerLoop.TimeUpdate.html
        TimeUpdate = 16384,
        LastTimeUpdate = 32768
    }

    public static class PlayerLoopHelper
    {
        private static int mainThreadId;
        private static SynchronizationContext unitySynchronizationContext;
        private static ContinuationQueue[] yielders;
        private static PlayerLoopRunner[] runners;

        public static int MainThreadId => mainThreadId;
        public static bool IsMainThread => Thread.CurrentThread.ManagedThreadId == mainThreadId;
        public static SynchronizationContext UnitySynchronizationContext => unitySynchronizationContext;

        private static PlayerLoopSystem[] InsertUnner(PlayerLoopSystem loopSystem,bool injectOnFirst,
            Type loopRunnerYieldType,ContinuationQueue cq,
            Type loopRunnerType,PlayerLoopRunner runner)
        {
#if UNITY_EDITOR
            //Play前后清除迭代对象，清除之前再跑一次
            EditorApplication.playModeStateChanged += state =>
            {
                if (state == PlayModeStateChange.EnteredEditMode || state == PlayModeStateChange.ExitingEditMode)
                {
                    if (runner != null)
                    {
                        runner.Run();
                        runner.Clear();
                    }
                    if (cq != null)
                    {
                        cq.Run();
                        cq.Clear();
                    }
                }
            };
#endif

            PlayerLoopSystem yieldLoop = new PlayerLoopSystem
            {
                type = loopRunnerYieldType,
                updateDelegate = cq.Run
            };

            PlayerLoopSystem runnerLoop = new PlayerLoopSystem
            {
                type = loopRunnerType,
                updateDelegate = runner.Run
            };

            //若重复添加subLoopSystem，则移除之前的
            PlayerLoopSystem[] source = RemoveRunner(loopSystem, loopRunnerYieldType, loopRunnerType);
            PlayerLoopSystem[] dest = new PlayerLoopSystem[source.Length + 2];

            Array.Copy(source, 0, dest, injectOnFirst ? 2 : 0, source.Length);
            if (injectOnFirst)
            {
                dest[0] = yieldLoop;
                dest[1] = runnerLoop;
            }
            else
            {
                dest[dest.Length - 2] = yieldLoop;
                dest[dest.Length - 1] = runnerLoop;
            }

            return dest;
        }

        /// <summary>
        /// 删除PlayerLoopSystem中指定的 yieldType 和 runnerType 并返回该PlayerLoopSystem新的 subSystemList
        /// </summary>
        /// <param name="loopSystem"></param>
        /// <param name="loopRunnerYieldType"></param>
        /// <param name="loopRunnerType"></param>
        /// <returns></returns>
        private static PlayerLoopSystem[] RemoveRunner(PlayerLoopSystem loopSystem,Type loopRunnerYieldType,Type loopRunnerType)
        {
            return loopSystem.subSystemList
                .Where(ls => ls.type != loopRunnerYieldType && ls.type != loopRunnerType)
                .ToArray();
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Init()
        {
            //捕获 unity 的同步上下文
            unitySynchronizationContext = SynchronizationContext.Current;
            mainThreadId = Thread.CurrentThread.ManagedThreadId;

#if UNITY_EDITOR
            //解决编辑器关闭域重载的问题，需要 2019.3 以上的版本
            //当域重载关闭后，进入 play 前需要重新重新初始化，否则之前的 tasks 会停留在内存中导致内存泄漏
            bool domainReloadDisabled = UnityEditor.EditorSettings.enterPlayModeOptionsEnabled &&
                UnityEditor.EditorSettings.enterPlayModeOptions.HasFlag(UnityEditor.EnterPlayModeOptions.DisableDomainReload);
            if (!domainReloadDisabled && runners != null)//没有关闭域重载，就当无事发生
                return;
#endif

            PlayerLoopSystem playerLoop = PlayerLoop.GetCurrentPlayerLoop();
            //to do Initialze(ref playerLoop);
        }

#if UNITY_EDITOR
        [InitializeOnLoadMethod]
        private static void InitOnEditor()
        {
            //play 后，执行初始方法
            Init();

            //注册 Editor 的 update 生命周期方法，用于迭代 playerLoop
            EditorApplication.update += ForceEditorPlayerLoopUpdate;
        }

        private static void ForceEditorPlayerLoopUpdate()
        {

        }
#endif
    }
}