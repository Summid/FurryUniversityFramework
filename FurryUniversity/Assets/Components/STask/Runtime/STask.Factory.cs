using System;
using System.Runtime.ExceptionServices;
using System.Threading;

namespace SFramework.Threading.Tasks
{
    public partial struct STask
    {
        public static readonly STask CompletedTask = new STask();

        public static STask FromException(Exception exception)
        {
            if (exception is OperationCanceledException oce)
            {
                return FromCanceled(oce.CancellationToken);
            }
            return new STask(new ExceptionResultSource(exception), 0);
        }

        /// <summary> 默认的 Canceled STask 对象 （CancellationToken 为 None）</summary>
        private static readonly STask CanceledSTask = new Func<STask>(() =>//用委托包装下
        {
            return new STask(new CanceledResultSource(CancellationToken.None), 0);
        })();

        public static STask FromCanceled(CancellationToken cancellationToken = default)
        {
            if (cancellationToken == CancellationToken.None)//默认token
            {
                return CanceledSTask;
            }
            else
            {
                return new STask(new CanceledResultSource(cancellationToken), 0);
            }
        }

        #region ISTaskSource
        private sealed class ExceptionResultSource : ISTaskSource
        {
            private readonly ExceptionDispatchInfo exception;
            private bool calledGet;

            public ExceptionResultSource(Exception exception)
            {
                this.exception = ExceptionDispatchInfo.Capture(exception);
            }

            public void GetResult(short token)
            {
                if (!calledGet)
                {
                    calledGet = true;
                    GC.SuppressFinalize(this);//不要调用析构函数，防止在Finalize线程调用析构函数之后，GC再重复调用
                }
                exception.Throw();
            }

            public STaskStatus GetStatus(short token)
            {
                return STaskStatus.Faulted;
            }

            public STaskStatus UnsafeGetStatus()
            {
                return STaskStatus.Faulted;
            }

            public void OnCompleted(Action<object> continuation, object state, short token)
            {
                continuation(state);
            }

            ~ExceptionResultSource()
            {
                if (!calledGet)
                {
                    STaskScheduler.PublishUnobservedTaskException(exception.SourceException);
                }
            }
        }

        /// <summary>
        /// 用于创建被取消的 STask
        /// </summary>
        private sealed class CanceledResultSource : ISTaskSource
        {
            private readonly CancellationToken cancellationToken;

            public CanceledResultSource(CancellationToken cancellationToken)
            {
                this.cancellationToken = cancellationToken;
            }

            public void GetResult(short token)
            {
                throw new OperationCanceledException(cancellationToken);
            }

            public STaskStatus GetStatus(short token)
            {
                return STaskStatus.Canceled;
            }

            public STaskStatus UnsafeGetStatus()
            {
                return STaskStatus.Canceled;
            }

            public void OnCompleted(Action<object> continuation, object state, short token)
            {
                continuation(state);
            }
        }
        #endregion
    }
}