
using System;

namespace SFramework.Threading.Tasks
{
    public enum STaskStatus
    {
        /// <summary>任务执行中</summary>
        Pending = 0,
        /// <summary>任务执行成功</summary>
        Succeeded = 1,
        /// <summary>任务执行失败</summary>
        Faulted = 2,
        /// <summary>任务被取消</summary>
        Canceled = 3,
    }

    /// <summary>
    /// 在STask中（配合 Awaiter）干活的对象，类似 IValueTaskSource
    /// 实现该接口可修改STask的行为（任务何时结束，任务结果是多少），以此来扩展STask
    /// </summary>
    public interface ISTaskSource
    {
        STaskStatus GetStatus(short token);
        void OnCompleted(Action<object> continuation, object state, short token);
        void GetResult(short token);

        STaskStatus UnsafeGetStatus();//仅供 debug 使用
    }
}