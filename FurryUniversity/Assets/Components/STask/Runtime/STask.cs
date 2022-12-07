using SFramework.Threading.Tasks.CompilerServices;
using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace SFramework.Threading.Tasks
{
    internal static class AwaiterActions
    {
        internal static readonly Action<object> InvokeContinuationDelegate = Continuation;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void Continuation(object state)//state is continuation
        {
            ((Action)state).Invoke();
        }
    }

    /// <summary>
    /// 轻量级 task-like 对象
    /// <see cref="https://github.com/dotnet/roslyn/blob/main/docs/features/task-types.md"/>
    /// </summary>
    [AsyncMethodBuilder(typeof(AsyncSTaskMethodBuilder))]
    [StructLayout(LayoutKind.Auto)]
    public readonly partial struct STask
    {
        private readonly ISTaskSource source;
        private readonly short token;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="source">一般情况下是 <see cref="AsyncSTask<TStateMachine>"/></param>
        /// <param name="token"></param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public STask(ISTaskSource source, short token)
        {
            this.source = source;
            this.token = token;
        }

        public STaskStatus Status
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                if (this.source == null)
                    return STaskStatus.Succeeded;
                return this.source.GetStatus(token);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Awaiter GetAwaiter()
        {
            return new Awaiter(this);
        }

        public override string ToString()
        {
            if (this.source == null)
                return "()";
            return "(" + this.source.UnsafeGetStatus() + ")";
        }

        public readonly struct Awaiter : ICriticalNotifyCompletion
        {
            private readonly STask task;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public Awaiter(in STask task)
            {
                this.task = task;
            }

            public bool IsCompleted
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get
                {
                    return this.task.Status.IsCompleted();
                }
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void GetResult()
            {
                if (this.task.source == null)
                    return;
                this.task.source.GetResult(this.task.token);
            }


            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void OnCompleted(Action continuation)
            {
                if (this.task.source == null)
                {
                    continuation();
                }
                else
                {
                    this.task.source.OnCompleted(AwaiterActions.InvokeContinuationDelegate, continuation, this.task.token);
                }
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void UnsafeOnCompleted(Action continuation)
            {
                if (this.task.source == null)
                {
                    continuation();
                }
                else
                {
                    this.task.source.OnCompleted(AwaiterActions.InvokeContinuationDelegate, continuation, this.task.token);
                }
            }


        }
    }
}