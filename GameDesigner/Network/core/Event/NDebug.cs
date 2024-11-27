namespace Net.Event
{
    using global::System;
    using global::System.IO;
    using global::System.Text;
#if SERVICE && WINDOWS
    using global::System.Drawing;
    using global::System.Windows.Forms;
#endif
    using Net.System;
    using Net.Helper;
    using Net.Share;

    public enum LogType
    {
        Log,
        Warning,
        Error,
    }

    public class LogEntity
    {
        public int count;
        public int row = -1;
        public DateTime time;
        public LogType log;
        public string msg;

        public override string ToString()
        {
            return $"[{time.ToString("yyyy-MM-dd HH:mm:ss")}][{log}] {msg}";
        }
    }

    /// <summary>
    /// 控制台输出帮助类
    /// </summary>
    public class ConsoleDebug : IDebug
    {
        private readonly MyDictionary<string, LogEntity> dic = new();
        public int count = 1000;
        private int cursorTop;
        private readonly bool collapse;

        public ConsoleDebug(bool collapse)
        {
            this.collapse = collapse;
        }

        public void Output(DateTime time, LogType log, string msg)
        {
            if (collapse)
            {
                if (dic.Count > count)
                {
                    dic.Clear();
                    Console.Clear();
                    cursorTop = 0;
                }
                ref var entity = ref dic.GetValueRefOrAddDefault(log + msg, out var exists);
                if (!exists)
                {
                    entity = new LogEntity
                    {
                        time = time,
                        log = log,
                        msg = msg
                    };
                }
                entity.count++;
                if (entity.row == -1)
                {
                    entity.row = cursorTop;
                    Console.SetCursorPosition(0, cursorTop);
                }
                else
                {
                    Console.SetCursorPosition(0, entity.row);
                }
                var info = $"[{time.ToString("yyyy-MM-dd HH:mm:ss")}][";
                Console.Write(info);
                Console.ForegroundColor = log == LogType.Log ? ConsoleColor.Green : log == LogType.Warning ? ConsoleColor.Yellow : ConsoleColor.Red;
                info = $"{log}";
                Console.Write(info);
                Console.ResetColor();
                if (entity.count > 1)
                    Console.Write($"] ({entity.count}) {msg}\r\n");
                else
                    Console.Write($"] {msg}\r\n");
                if (Console.CursorTop > cursorTop)
                    cursorTop = Console.CursorTop;
            }
            else
            {
                var info = $"[{time.ToString("yyyy-MM-dd HH:mm:ss")}][";
                Console.Write(info);
                Console.ForegroundColor = log == LogType.Log ? ConsoleColor.Green : log == LogType.Warning ? ConsoleColor.Yellow : ConsoleColor.Red;
                info = $"{log}";
                Console.Write(info);
                Console.ResetColor();
                Console.Write($"] {msg}\r\n");
            }
        }
    }

#if SERVICE && WINDOWS
    /// <summary>
    /// Form窗口程序输出帮助类
    /// </summary>
    public class FormDebug : IDebug
    {
        private MyDictionary<string, LogEntity> dic = new MyDictionary<string, LogEntity>();
        public int count = 1000;
        public ListBox listBox;
        /// <summary>
        /// 字体颜色
        /// </summary>
        public Brush BackgroundColor;
        /// <summary>
        /// 日志颜色
        /// </summary>
        public Brush LogColor = Brushes.Blue;
        /// <summary>
        /// 警告颜色
        /// </summary>
        public Brush WarningColor = Brushes.Yellow;
        /// <summary>
        /// 错误颜色
        /// </summary>
        public Brush ErrorColor = Brushes.Red;

        public FormDebug(ListBox listBox, Brush backgroundColor = null)
        {
            if (backgroundColor == null)
                backgroundColor = Brushes.Black;
            this.listBox = listBox;
            this.BackgroundColor = backgroundColor;
            listBox.DrawMode = DrawMode.OwnerDrawFixed;
            listBox.DrawItem += DrawItem;
        }

        public void Output(DateTime time, LogType log, string msg)
        {
            if (dic.Count > count)
            {
                dic.Clear();
                listBox.Items.Clear();
            }
            if (!dic.TryGetValue(log + msg, out var entity))
                dic.TryAdd(log + msg, entity = new LogEntity() { time = time, log = log, msg = msg });
            entity.count++;
            if (entity.row == -1)
            {
                entity.row = listBox.Items.Count;
                listBox.Items.Add(entity);
            }
        }

        public void DrawItem(object sender, DrawItemEventArgs e)
        {
            if (e.Index == -1)
                return;
            var entity = listBox.Items[e.Index] as LogEntity;
            e.DrawBackground();
            e.DrawFocusRectangle();
            var y = e.Bounds.Y;
            var msg = $"[{entity.time.ToString("yyyy-MM-dd HH:mm:ss")}][";
            e.Graphics.DrawString(msg, e.Font, BackgroundColor, 0, y);
            var x = msg.Length * 6;
            msg = $"{entity.log}";
            var color = entity.log == LogType.Log ? LogColor : entity.log == LogType.Warning ? WarningColor : ErrorColor;
            e.Graphics.DrawString(msg, e.Font, color, x, y);
            x += msg.Length * 6;
            msg = entity.msg.Split('\r', '\n')[0];
            if (msg.Length >= byte.MaxValue) //文字过多会报异常
                msg = msg.Substring(0, byte.MaxValue);
            if (entity.count > 1)
                e.Graphics.DrawString($"] ({entity.count}) {msg}", e.Font, BackgroundColor, x, y);
            else
                e.Graphics.DrawString($"] {msg}", e.Font, BackgroundColor, x, y);
        }
    }
#endif

    public interface IDebug
    {
        void Output(DateTime time, LogType log, string msg);
    }

    /// <summary>
    /// 写入日志模式
    /// </summary>
    public enum WriteLogMode
    {
        /// <summary>
        /// 啥都不干
        /// </summary>
        None = 0,
        /// <summary>
        /// 写入普通日志
        /// </summary>
        Log,
        /// <summary>
        /// 写入警告日志
        /// </summary>
        Warn,
        /// <summary>
        /// 写入错误日志
        /// </summary>
        Error,
        /// <summary>
        /// 三种日志全部写入
        /// </summary>
        All,
        /// <summary>
        /// 只写入警告日志和错误日志
        /// </summary>
        WarnAndError,
    }

    /// <summary>
    /// 消息输入输出处理类
    /// </summary>
    public static class NDebug
    {
        /// <summary>
        ///  输出日志名称--如果使用
        /// </summary>
        public static string Name { get; set; }
        /// <summary>
        /// 输出调式消息
        /// </summary>
        public static event Action<string> LogHandle;
        /// <summary>
        /// 输出调式错误消息
        /// </summary>
        public static event Action<string> LogErrorHandle;
        /// <summary>
        /// 输出调式警告消息
        /// </summary>
        public static event Action<string> LogWarningHandle;
        /// <summary>
        /// 输出信息处理事件
        /// </summary>
        public static event Action<DateTime, LogType, string> Output;
        /// <summary>
        /// 输出日志最多容纳条数
        /// </summary>
        public static int LogMax { get; set; } = 10000;
        /// <summary>
        /// 输出错误日志最多容纳条数
        /// </summary>
        public static int LogErrorMax { get; set; } = 10000;
        /// <summary>
        /// 输出警告日志最多容纳条数
        /// </summary>
        public static int LogWarningMax { get; set; } = 10000;
        /// <summary>
        /// 每次执行可连续输出多少条日志, 默认输出300 * 3条
        /// </summary>
        public static int LogOutputMax { get; set; } = 300;
        private static readonly QueueSafe<object> logQueue = new();
        private static readonly QueueSafe<object> errorQueue = new();
        private static readonly QueueSafe<object> warningQueue = new();
        private static readonly MyDictionary<uint, ValueTuple<int, int, int>> collapseDict = new();
        /// <summary>
        /// 绑定的输入输出对象
        /// </summary>
        public static IDebug Debug { get; set; }
#if !UNITY_WEBGL
        private static FileStream fileStream;
        private static int writeFileModeID;
#endif
        private static WriteLogMode writeFileMode;
        /// <summary>
        /// 写入日志到文件模式
        /// </summary>
        public static WriteLogMode WriteFileMode
        {
            get { return writeFileMode; }
            set
            {
                writeFileMode = value;
#if !UNITY_WEBGL
                if (value != WriteLogMode.None & fileStream == null)
                {
                    CreateLogFile();
                    writeFileModeID = ThreadManager.Invoke("CreateLogFile", GetResetTime(), CreateLogFile);//每0点会创建新的日志文件
                }
                else if (value == WriteLogMode.None & fileStream != null)
                {
                    fileStream.Close();
                    fileStream = null;
                    ThreadManager.Event.RemoveEvent(writeFileModeID);
                }
#endif
            }
        }
        /// <summary>
        /// 相同的日志是否重叠？
        /// </summary>
        public static bool IsCollapse { get; set; } = true;

#if !UNITY_WEBGL
        private static int GetResetTime() //获取毫秒数
        {
            var now = DateTime.Now;
            var day = now.AddDays(1);
            day = new DateTime(day.Year, day.Month, day.Day, 0, 0, 0);//明天0点
            var time = (day - now).TotalMilliseconds;
            var seconds = (int)Math.Ceiling(time);
            return seconds;
        }

        public static bool CreateLogFile()
        {
            try
            {
                var now = DateTime.Now;
                var path = PathHelper.Combine(Config.Config.BasePath, $"/Log/{now.Year}/{now.Month.ToString("00")}/");
                if (!Directory.Exists(path))
                    Directory.CreateDirectory(path);
                path += $"{Name}{now.Year}{now.Month.ToString("00")}{now.Day.ToString("00")}{now.Hour.ToString("00")}{now.Minute.ToString("00")}{now.Second.ToString("00")}.txt";
                if (fileStream != null)
                    fileStream.Close();
                fileStream = new FileStream(path, FileMode.OpenOrCreate); //不加try会导致服务器崩溃闪退问题
                var position = fileStream.Length;
                fileStream.Seek(position, SeekOrigin.Begin);
            }
            catch (Exception ex)
            {
                LogError(ex);
            }
            finally
            {
                ThreadManager.Event.ResetTimeInterval(writeFileModeID, GetResetTime());
            }
            return true;
        }
#endif

#if SERVICE || (UNITY_SERVER && !UNITY_EDITOR)
        static NDebug()
        {
            ThreadManager.Invoke("OutputLog", OutputLog, true);
            AppDomain.CurrentDomain.UnhandledException += UnhandledExceptionHandler;
        }

        private static void UnhandledExceptionHandler(object sender, UnhandledExceptionEventArgs e)
        {
            if (e.ExceptionObject is Exception exception)
            {
                var message = new StringBuilder();
                message.AppendLine("程序崩溃信息:");
                if (Environment.OSVersion.Platform == PlatformID.Win32NT)
                {
                    var memoryStatus = new MEMORYSTATUSEX();
                    memoryStatus.Initialize();
                    Win32KernelAPI.GlobalMemoryStatusEx(ref memoryStatus);
                    message.AppendLine($"内存使用率: {ByteHelper.ToString(memoryStatus.dwMemoryLoad)}%");
                    message.AppendLine($"总物理内存: {ByteHelper.ToString(memoryStatus.ullTotalPhys)}");
                    message.AppendLine($"可用物理内存: {ByteHelper.ToString(memoryStatus.ullAvailPhys)}");
                }
                var currentProcess = global::System.Diagnostics.Process.GetCurrentProcess();
                var memoryUsed = currentProcess.WorkingSet64;
                message.AppendLine($"当前进程使用的内存: {ByteHelper.ToString(memoryUsed)}");
                message.AppendLine($"异常信息:{exception.Message}");
                message.AppendLine($"错误名称:{exception.Source}");
                message.AppendLine("调用堆栈:");
                message.AppendLine(exception.StackTrace);
                message.AppendLine("如果希望崩溃重启, 可在Main主函数调用Net.Helper.ApplicationHelper.CrashRecovery()注册异常崩溃重启事件!");
                LogError(message.ToString());
                OutputLog();
            }
        }

        private static bool OutputLog()
        {
            try
            {
                var currTime = DateTime.Now;
                var logTime = currTime.ToString("yyyy-MM-dd HH:mm:ss");
                var isWrite = writeFileMode == WriteLogMode.All | writeFileMode == WriteLogMode.Log;
                var output = LogOutputMax;
                Log(logQueue, currTime, logTime, output, LogType.Log, LogHandle, isWrite);
                isWrite = writeFileMode == WriteLogMode.All | writeFileMode == WriteLogMode.Warn | writeFileMode == WriteLogMode.WarnAndError;
                output = LogOutputMax;
                Log(warningQueue, currTime, logTime, output, LogType.Warning, LogWarningHandle, isWrite);
                isWrite = writeFileMode == WriteLogMode.All | writeFileMode == WriteLogMode.Error | writeFileMode == WriteLogMode.WarnAndError;
                output = LogOutputMax;
                Log(errorQueue, currTime, logTime, output, LogType.Error, LogErrorHandle, isWrite);
                fileStream?.Flush();
            }
            catch (Exception ex)
            {
                errorQueue.Enqueue(ex.Message);
            }
            return true;
        }

        private static void Log(QueueSafe<object> logQueue, in DateTime currTime, in string logTime, int output, LogType logType, Action<string> logAction, bool isWrite)
        {
            string msg;
            string log;
            while (logQueue.TryDequeue(out object message))
            {
                if (message == null)
                    continue;
                msg = message.ToString();
                if (logAction != null)
                {
                    log = $"[{logTime}][{logType}] {msg}";
                    logAction(log);
                }
                Output?.Invoke(currTime, logType, msg);
                if (isWrite)
                {
                    byte[] bytes;
                    if (IsCollapse)
                    {
                        var hash = (logType.ToString() + msg).CRCU32();
                        ref var item = ref collapseDict.GetValueRefOrAddDefault(hash, out var exists);
                        item.Item2++;
                        if (exists)
                        {
                            var position = fileStream.Position;
                            fileStream.Seek(item.Item1, SeekOrigin.Begin);
                            log = $"[{logTime}][Log] ({item.Item2}) {msg}";
                            bytes = new byte[item.Item3];
                            bytes[item.Item3 - 2] = 13;
                            bytes[item.Item3 - 1] = 10;
                            Encoding.UTF8.GetBytes(log, 0, log.Length, bytes, 0);
                            fileStream.Write(bytes, 0, bytes.Length);
                            fileStream.Position = position;
                        }
                        else
                        {
                            log = $"[{logTime}][{logType}] {msg}";
                            bytes = Encoding.UTF8.GetBytes(log + "            " + Environment.NewLine); //留12个空位放重叠的数字
                            item.Item1 = (int)fileStream.Position;
                            item.Item3 = bytes.Length;
                            fileStream.Write(bytes, 0, bytes.Length);
                        }
                    }
                    else
                    {
                        log = $"[{logTime}][{logType}] {msg}";
                        bytes = Encoding.UTF8.GetBytes(log + Environment.NewLine);
                        fileStream.Write(bytes, 0, bytes.Length);
                    }
                }
                if (--output <= 0)
                    break;
            }
        }
#endif

        /// <summary>
        /// 输出调式消息
        /// </summary>
        /// <param name="message"></param>
        public static void Log(object message)
        {
            if (logQueue.Count >= LogMax)
                return;
#if SERVICE || (UNITY_SERVER && !UNITY_EDITOR)
            logQueue.Enqueue(message);
#else
            LogHandle?.Invoke($"[{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}][Log] {message}");
            Output?.Invoke(DateTime.Now, LogType.Log, message.ToString());
            if (!ThreadManager.IsRuning)
                UnityEngine.Debug.Log(message);
#endif
        }

        /// <summary>
        /// 输出错误消息
        /// </summary>
        /// <param name="message"></param>
        public static void LogError(object message)
        {
            if (errorQueue.Count >= LogErrorMax)
                return;
#if SERVICE || (UNITY_SERVER && !UNITY_EDITOR)
            errorQueue.Enqueue(message);
#else
            LogErrorHandle?.Invoke($"[{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}][Error] {message}");
            Output?.Invoke(DateTime.Now, LogType.Error, message.ToString());
            if (!ThreadManager.IsRuning)
                UnityEngine.Debug.LogError(message);
#endif
        }

        /// <summary>
        /// 输出警告消息
        /// </summary>
        /// <param name="message"></param>
        public static void LogWarning(object message)
        {
            if (warningQueue.Count >= LogWarningMax)
                return;
#if SERVICE || (UNITY_SERVER && !UNITY_EDITOR)
            warningQueue.Enqueue(message);
#else
            LogWarningHandle?.Invoke($"[{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}][Warning] {message}");
            Output?.Invoke(DateTime.Now, LogType.Warning, message.ToString());
            if (!ThreadManager.IsRuning)
                UnityEngine.Debug.LogWarning(message);
#endif
        }

        /// <summary>
        /// 写入日志文件
        /// </summary>
        /// <param name="message"></param>
        public static void WriteLog(object message)
        {
#if !UNITY_WEBGL
            if (fileStream == null)
                return;
            var bytes = Encoding.UTF8.GetBytes(message.ToString());
            fileStream.Write(bytes, 0, bytes.Length);
            fileStream.Flush();
#endif
        }

        public static void BindLogAll(Action<string> log)
        {
            BindLogAll(log, log, log);
        }

        public static void BindLogAll(Action<string> log, Action<string> warning, Action<string> error)
        {
            if (log != null) LogHandle += log;
            if (warning != null) LogWarningHandle += warning;
            if (error != null) LogErrorHandle += error;
        }

        public static void RemoveLogAll(Action<string> log)
        {
            RemoveLogAll(log, log, log);
        }

        public static void RemoveLogAll(Action<string> log, Action<string> warning, Action<string> error)
        {
            if (log != null) LogHandle -= log;
            if (warning != null) LogWarningHandle -= warning;
            if (error != null) LogErrorHandle -= error;
        }

        /// <summary>
        /// 绑定控制台输出
        /// </summary>
        public static void BindConsoleLog(bool collapse = true, int logOutputMax = 1000)
        {
            BindDebug(new ConsoleDebug(collapse) { count = logOutputMax });
        }

        /// <summary>
        /// 移除控制台输出
        /// </summary>
        public static void RemoveConsoleLog()
        {
            RemoveDebug();
        }

#if SERVICE && WINDOWS
        /// <summary>
        /// 绑定窗体程序输出
        /// </summary>
        public static void BindFormLog(ListBox listBox, Brush backgroundColor = null)
        {
            BindDebug(new FormDebug(listBox, backgroundColor));
        }

        /// <summary>
        /// 移除窗体程序输出
        /// </summary>
        public static void RemoveFormLog()
        {
            RemoveDebug();
        }
#endif

        /// <summary>
        /// 绑定输出接口
        /// </summary>
        /// <param name="debug"></param>
        public static void BindDebug(IDebug debug)
        {
            if (Debug != null)
                RemoveDebug();
            Debug = debug;
            Output += debug.Output;
        }

        /// <summary>
        /// 移除输出接口
        /// </summary>
        public static void RemoveDebug()
        {
            if (Debug == null)
                return;
            Output -= Debug.Output;
            Debug = null;
        }
    }
}