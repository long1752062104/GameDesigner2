using System;
using System.Diagnostics;

namespace Net.Helper
{
    public class ApplicationHelper
    {
        /// <summary>
        /// 崩溃重启处理
        /// </summary>
        public static void CrashRecovery()
        {
            AppDomain.CurrentDomain.UnhandledException += UnhandledExceptionHandler;
        }

        private static void UnhandledExceptionHandler(object sender, UnhandledExceptionEventArgs e)
        {
            RestartApplication();
        }

        private static void RestartApplication()
        {
            var process = Process.GetCurrentProcess();
            var exePath = process.MainModule.FileName;
            Process.Start(new ProcessStartInfo(exePath));
            Environment.Exit(0);
        }
    }
}