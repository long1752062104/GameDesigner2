namespace Net.Share
{
    public enum NetworkUpdateMode
    {
        /// <summary>
        /// 独立线程执行
        /// </summary>
        Thread,
        /// <summary>
        /// 统一单线程执行 -非unity主线程
        /// </summary>
        SingleThread,
        /// <summary>
        /// 自定义执行方式
        /// </summary>
        CustomExecution
    }
}
