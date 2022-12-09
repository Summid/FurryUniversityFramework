using SFramework.Threading.Tasks.Internal;
using System;
using System.Runtime.CompilerServices;
using System.Threading;
using UnityEngine;

namespace SFramework.Threading.Tasks
{
    public static partial class UnityAsyncExtensions
    {
        #region AsyncOperation
#if !UNITY_2023_1_OR_NEWER
        // unity2023.1 之后的版本 Unity 自带了 AsyncOperationAwaitableExtensions.GetAwaiter
        public static AsyncOperationAwaiter GetAwaiter(this AsyncOperation asyncOperation)
        {
            Error.ThrowArgumentNullException(asyncOperation, nameof(asyncOperation));
            return new AsyncOperationAwaiter(asyncOperation);
        }
#endif

        public static STask WithCancellation(this AsyncOperation asyncOperation, CancellationToken cancellationToken)
        {
            return ToUniTask(asyncOperation, cancellationToken: cancellationToken);
        }

        public static STask ToUniTask(this AsyncOperation asyncOperation, IProgress<float> progress = null, PlayerLoopTiming timing = PlayerLoopTiming.Update, CancellationToken cancellationToken = default(CancellationToken))
        {
            Error.ThrowArgumentNullException(asyncOperation, nameof(asyncOperation));
            if (cancellationToken.IsCancellationRequested)
                return STask.FromCanceled(cancellationToken);
            if (asyncOperation.isDone)
                return STask.CompletedTask;
            return new STask(AsyncOperationConfiguredSource.Create(asyncOperation, timing, progress, cancellationToken, out short token), token);
        }

        public struct AsyncOperationAwaiter : ICriticalNotifyCompletion
        {
            private AsyncOperation asyncOperation;
            private Action<AsyncOperation> continuationAction;

            public AsyncOperationAwaiter(AsyncOperation asyncOperation)
            {
                this.asyncOperation = asyncOperation;
                this.continuationAction = null;
            }

            public bool IsCompleted => this.asyncOperation.isDone;

            public void GetResult()
            {
                if (this.continuationAction != null)
                {
                    this.asyncOperation.completed -= this.continuationAction;
                    this.continuationAction = null;
                    this.asyncOperation = null;
                }
                else
                {
                    this.asyncOperation = null;
                }
            }

            public void OnCompleted(Action continuation)
            {
                this.UnsafeOnCompleted(continuation);
            }

            public void UnsafeOnCompleted(Action continuation)
            {
                Error.ThrowWhenContinuationIsAlreadyRegistered(this.continuationAction);
                this.continuationAction = PooledDelegate<AsyncOperation>.Create(continuation);
                this.asyncOperation.completed += this.continuationAction;
            }
        }

        private sealed class AsyncOperationConfiguredSource : ISTaskSource, IPlayerLoopItem, ITaskPoolNode<AsyncOperationConfiguredSource>
        {
            private static TaskPool<AsyncOperationConfiguredSource> pool;
            private AsyncOperationConfiguredSource nodeNode;
            public ref AsyncOperationConfiguredSource NextNode => ref this.nodeNode;

            static AsyncOperationConfiguredSource()
            {
                TaskPool.RegisterSizeGetter(typeof(AsyncOperationConfiguredSource), () => pool.Size);
            }

            private AsyncOperation asyncOperation;
            private IProgress<float> progress;
            private CancellationToken cancellationToken;

            STaskCompletionSourceCore<AsyncUnit> core;

            private AsyncOperationConfiguredSource() { }

            public static ISTaskSource Create(AsyncOperation asyncOperation, PlayerLoopTiming timing, IProgress<float> progress, CancellationToken cancellationToken, out short token)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    return AutoResetSTaskCompletionSource.CreateFromCanceled(cancellationToken, out token);
                }

                if (!pool.TryPop(out AsyncOperationConfiguredSource result))
                {
                    result = new AsyncOperationConfiguredSource();
                }

                result.asyncOperation = asyncOperation;
                result.progress = progress;
                result.cancellationToken = cancellationToken;

                PlayerLoopHelper.AddAction(timing, result);

                token = result.core.Version;
                return result;
            }

            public void GetResult(short token)
            {
                try
                {
                    this.core.GetResult(token);
                }
                finally
                {
                    this.TryReturn();
                }
            }

            public STaskStatus GetStatus(short token)
            {
                return this.core.GetStatus(token);
            }

            public STaskStatus UnsafeGetStatus()
            {
                return this.core.UnsafeGetStatus();
            }

            public void OnCompleted(Action<object> continuation, object state, short token)
            {
                this.core.OnCompleted(continuation, state, token);
            }

            public bool MoveNext()
            {
                if (this.cancellationToken.IsCancellationRequested)
                {
                    this.core.TrySetCanceled(this.cancellationToken);
                    return false;
                }

                if (this.progress != null)
                {
                    this.progress.Report(this.asyncOperation.progress);
                }

                if (this.asyncOperation.isDone)
                {
                    this.core.TrySetResult(AsyncUnit.Default);
                    return false;
                }

                return true;
            }

            private bool TryReturn()
            {
                this.core.Reset();
                this.asyncOperation = default;
                this.progress = default;
                cancellationToken = default;
                return pool.TryPush(this);
            }
        }
        #endregion
    }
}