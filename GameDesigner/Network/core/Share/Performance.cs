namespace Net.Share
{
    /// <summary>
    /// 网络代码性能相关模式
    /// </summary>
    public enum Performance
    {
        /// <summary>
        /// 实时性模式，可以做实时性游戏，如帧同步。但是这个模式会多占用CPU
        /// </summary>
        Realtime = 0,
        /// <summary>
        /// 优化模式，这个模式降低CPU，会损失实时性
        /// </summary>
        Optimization = 1,
    }
}