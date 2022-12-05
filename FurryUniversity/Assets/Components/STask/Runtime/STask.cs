using SFramework.Threading.Tasks.CompilerServices;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace SFramework.Threading.Tasks
{
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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public STask(ISTaskSource source, short token)
        {
            this.source = source;
            this.token = token;
        }
    }
}