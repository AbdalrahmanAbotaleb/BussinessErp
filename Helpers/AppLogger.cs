using System;
using System.IO;

namespace BussinessErp.Helpers
{
    /// <summary>
    /// Centralized file-based logging system.
    /// </summary>
    public static class AppLogger
    {
        private static readonly object _lock = new object();
        private static readonly string _logDir;
        private static readonly string _logFile;

        static AppLogger()
        {
            _logDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logs");
            if (!Directory.Exists(_logDir))
                Directory.CreateDirectory(_logDir);

            _logFile = Path.Combine(_logDir, $"app_{DateTime.Now:yyyy-MM-dd}.log");
        }

        public static void Info(string message) => WriteLog("INFO", message);
        public static void Warn(string message) => WriteLog("WARN", message);
        public static void Error(string message, Exception ex = null)
        {
            string full = ex != null ? $"{message} | {ex.GetType().Name}: {ex.Message}\n{ex.StackTrace}" : message;
            WriteLog("ERROR", full);
        }

        private static void WriteLog(string level, string message)
        {
            string entry = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] [{level}] {message}";
            lock (_lock)
            {
                try
                {
                    File.AppendAllText(_logFile, entry + Environment.NewLine);
                }
                catch
                {
                    // Fail silently — logging should never crash the app
                }
            }
        }
    }
}
