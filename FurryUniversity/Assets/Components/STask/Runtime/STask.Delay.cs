using System;
using System.Threading;
using UnityEngine;

namespace SFramework.Threading.Tasks
{
    public enum DelayType
    {
        /// <summary>use Time.DeltaTime</summary>
        DeltaTime,
        /// <summary>Ignore timescale, use Time.unscaledDeltaTime</summary>
        UnsacledDeltaTime,
        /// <summary>use Stopwatch.GetTimestamp()</summary>
        RealTime
    }

    public partial struct STask
    {
        private sealed class DelayPromise : ISTaskSource, IPlayerLoopItem, ITaskPoolNode<DelayPromise>
        {
            private static TaskPool<DelayPromise> pool;
            private DelayPromise nextNode;
            public ref DelayPromise NextNode => ref this.nextNode;

            static DelayPromise()
            {
                TaskPool.RegisterSizeGetter(typeof(DelayPromise), () => pool.Size);
            }

            private int initialFrame;
            private float delayTimeSpan;
            private float elapsed;
            private CancellationToken cancellationToken;

            STaskCompletionSourceCore<object> core;

            private DelayPromise() { }

            public static ISTaskSource Create(TimeSpan delayTimeSpan, PlayerLoopTiming timing, CancellationToken cancellationToken, out short token)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    return AutoResetSTaskCompletionSource.CreateFromCanceled(cancellationToken, out token);
                }
                
                if(!pool.TryPop(out DelayPromise result))
                {
                    result = new DelayPromise();
                }

                result.elapsed = 0.0f;
                result.delayTimeSpan = (float)delayTimeSpan.TotalSeconds;
                result.cancellationToken = cancellationToken;
                result.initialFrame = PlayerLoopHelper.IsMainThread ? Time.frameCount : -1;

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
                if(this.cancellationToken.IsCancellationRequested)
                {
                    this.core.TrySetCanceled(cancellationToken);
                    return false;
                }

                if (this.elapsed == 0.0f)//刚开始
                {
                    if (this.initialFrame == Time.frameCount)
                    {
                        return true;
                    }
                }

                this.elapsed += Time.deltaTime;
                if (this.elapsed >= this.delayTimeSpan)
                {
                    this.core.TrySetResult(null);
                    return false;
                }

                return true;
            }

            private bool TryReturn()
            {
                this.core.Reset();
                this.delayTimeSpan = default;
                this.elapsed = default;
                this.cancellationToken = default;
                return pool.TryPush(this);
            }
        }

        private sealed class DelayIgnoreTimeScalePromise : ISTaskSource, IPlayerLoopItem, ITaskPoolNode<DelayIgnoreTimeScalePromise>
        {
            private static TaskPool<DelayIgnoreTimeScalePromise> pool;
            private DelayIgnoreTimeScalePromise nextNode;
            public ref DelayIgnoreTimeScalePromise NextNode => ref this.nextNode;

            static DelayIgnoreTimeScalePromise()
            {
                TaskPool.RegisterSizeGetter(typeof(DelayIgnoreTimeScalePromise), () => pool.Size);
            }

            float delayFrameTimeSpan;
            float elapsed;
            int initialFrame;
            CancellationToken cancellationToken;

            STaskCompletionSourceCore<object> core;

            DelayIgnoreTimeScalePromise() { }

            public static ISTaskSource Create(TimeSpan delayFrameTimeSpan, PlayerLoopTiming timing, CancellationToken cancellationToken, out short token)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    return AutoResetSTaskCompletionSource.CreateFromCanceled(cancellationToken, out token);
                }

                if (!pool.TryPop(out var result))
                {
                    result = new DelayIgnoreTimeScalePromise();
                }

                result.elapsed = 0.0f;
                result.delayFrameTimeSpan = (float)delayFrameTimeSpan.TotalSeconds;
                result.initialFrame = PlayerLoopHelper.IsMainThread ? Time.frameCount : -1;
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

                if (this.elapsed == 0.0f)
                {
                    if (this.initialFrame == Time.frameCount)
                    {
                        return true;
                    }
                }

                this.elapsed += Time.unscaledDeltaTime;
                if (this.elapsed >= this.delayFrameTimeSpan)
                {
                    this.core.TrySetResult(null);
                    return false;
                }

                return true;
            }

            private bool TryReturn()
            {
                this.core.Reset();
                this.delayFrameTimeSpan = default;
                this.elapsed = default;
                this.cancellationToken = default;
                return pool.TryPush(this);
            }
        }


    }
}