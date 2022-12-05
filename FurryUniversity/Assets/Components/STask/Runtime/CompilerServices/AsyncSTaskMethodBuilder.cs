using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace SFramework.Threading.Tasks.CompilerServices
{
    [StructLayout(LayoutKind.Auto)]
    public struct AsyncSTaskMethodBuilder
    {
        IStateMachineRunnerPromise runnerPromise;
        Exception ex;

        /// <summary>
        /// Static Create Method
        /// </summary>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static AsyncSTaskMethodBuilder Create()
        {
            return default;
        }

        /// <summary>
        /// task-like 对象属性
        /// </summary>
        //public STask Task
        //{
        //    get
        //    {
        //        if (runnerPromise != null)
        //        {
        //            return runnerPromise.Task;
        //        }
        //        else if (ex != null)
        //        {
        //            return 
        //        }
        //    }
        //}
    }
}