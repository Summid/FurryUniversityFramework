using System;

namespace SFramework.Threading.Tasks.CompilerServices
{

    /// <summary>
    /// （自定义）状态机执行逻辑的对象，实现 task-like object 的最基本功能
    /// </summary>
    internal interface IStateMachineRunnerPromise : ISTaskSource
    {
        Action MoveNext { get; }
        STask Task { get; }
        void SetResult();
        void SetException(Exception exception);
    }
}
