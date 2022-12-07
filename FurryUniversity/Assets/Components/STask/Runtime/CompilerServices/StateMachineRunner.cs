using System;
using System.Runtime.CompilerServices;

namespace SFramework.Threading.Tasks.CompilerServices
{

    /// <summary>
    /// （自定义）状态机执行逻辑的对象，实现 task-like object （STask） 的最基本功能，因此都是 Set 方法
    /// </summary>
    internal interface IStateMachineRunnerPromise : ISTaskSource
    {
        Action MoveNext { get; }
        STask Task { get; }
        void SetResult();
        void SetException(Exception exception);
    }

    internal sealed class AsyncSTask<TStateMachine> : IStateMachineRunnerPromise, ISTaskSource, ITaskPoolNode<AsyncSTask<TStateMachine>>
        where TStateMachine : IAsyncStateMachine
    {
        private TStateMachine stateMachine;
        private STaskCompletionSourceCore<AsyncUnit> core;

        private AsyncSTask()
        {
            this.MoveNext = this.Run;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void Run()
        {
            this.stateMachine.MoveNext();
        }

        #region Pool
        private static TaskPool<AsyncSTask<TStateMachine>> pool;

        public static void SetStateMachine(ref TStateMachine stateMachine, ref IStateMachineRunnerPromise runnerPromiseFieldRef)
        {
            if (!pool.TryPop(out AsyncSTask<TStateMachine> result))
            {
                result = new AsyncSTask<TStateMachine>();
            }
            runnerPromiseFieldRef = result;// set runner before copied
            result.stateMachine = stateMachine;
        }

        private AsyncSTask<TStateMachine> nextNode;
        public ref AsyncSTask<TStateMachine> NextNode => ref this.nextNode;

        static AsyncSTask()//静态构造函数，只执行一次（实例化前或引用其他静态成员前调用一次）
        {
            TaskPool.RegisterSizeGetter(typeof(AsyncSTask<TStateMachine>), () => pool.Size);
        }
        #endregion

        private void Return()
        {
            this.core.Reset();
            this.stateMachine = default;
            pool.TryPush(this);
        }

        private bool TryReturn()
        {
            this.core.Reset();
            this.stateMachine = default;
            return pool.TryPush(this);
        }

        #region IStateMahineRunnerPromise
        public Action MoveNext { get; }

        public STask Task
        {
            get
            {
                return new STask(this, core.Version);
            }
        }

        public void SetException(Exception exception)
        {
            this.core.TrySetException(exception);
        }

        public void SetResult()
        {
            this.core.TrySetResult(AsyncUnit.Default);
        }
        #endregion

        #region ISTaskSource
        public STaskStatus GetStatus(short token)
        {
            return this.core.GetStatus(token);
        }

        public void OnCompleted(Action<object> continuation, object state, short token)
        {
            this.core.OnCompleted(continuation, state, token);
        }

        public void GetResult(short token)
        {
            try
            {
                _ = this.core.GetResult(token);
            }
            finally
            {
                this.TryReturn();//reset
            }
        }

        public STaskStatus UnsafeGetStatus()
        {
            return this.core.UnsafeGetStatus();
        }
        #endregion




    }
}
