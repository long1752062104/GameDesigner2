using System;

namespace Net.Share
{
    public class RPCModelTask
    {
        public bool IsCompleted { get; internal set; }
        public RPCModel model;
        internal bool intercept;
        internal Delegate callback;
        internal uint tick;//会有一个定时事件检查是否内存遗留
    }
}
