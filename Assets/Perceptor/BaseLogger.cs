using System;
using System.IO;
using Perceptor.Util;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Rhinox.Perceptor
{
    public abstract class BaseLogger : ILogger
    {
        public LogLevels LogLevel = LogLevels.Debug;
        public bool ShouldThrowErrors = false;

        private const string LOGS_FOLDER = "Logs";
        private RotatingFileLogger _rotatingFileLogger;

        protected BaseLogger(LogLevels levels = LogLevels.Debug, bool shouldThrowErrors = false, bool logToFile = false)
        {
            LogLevel = levels;
            ShouldThrowErrors = shouldThrowErrors;
            if (logToFile)
                SetupFile();
        }

        public void Log(LogLevels level, string message, UnityEngine.Object associatedObject = null)
        {
#if NO_LOGGING
            return;
#endif
            if (LogLevel > level || LogLevel == LogLevels.None)
                return;

            OnLog(level, message, associatedObject);
        }

        protected virtual void OnLog(LogLevels level, string message, UnityEngine.Object associatedObject = null)
        {
            // Log to file if enabled
            LogToFile(level, message, associatedObject);
            
            // Also log to Unity
            switch (level)
            {
                case LogLevels.Trace:
                case LogLevels.Debug:
                case LogLevels.Info:
                    UnityEngine.Debug.Log(message, associatedObject);
                    break;
                case LogLevels.Warn:
                    UnityEngine.Debug.LogWarning(message, associatedObject);
                    break;
                case LogLevels.Error:
                case LogLevels.Fatal:
                    if (ShouldThrowErrors)
                        throw new Exception(message);
                    else
                        UnityEngine.Debug.LogError(message, associatedObject);
                    break;
            }
        }

        public virtual void Update()
        {
            
        }
        
        private void LogToFile(LogLevels level, string message, UnityEngine.Object associatedObject = null)
        {
            if (_rotatingFileLogger == null)
                return;

            string logLine = "";
            
            string shortLevel = ToShortString(level);
            if (shortLevel != null)
                logLine += $"{shortLevel}> ";
            logLine += message;
            
            _rotatingFileLogger.WriteLine(logLine);
        }

        private string ToShortString(LogLevels level)
        {
            switch (level)
            {
                case LogLevels.Trace:
                    return "TRAC";
                case LogLevels.Debug:
                    return "DEBG";
                case LogLevels.Info:
                    return "INFO";
                case LogLevels.Warn:
                    return "WARN";
                case LogLevels.Error:
                    return "ERRO";
                case LogLevels.Fatal:
                    return "FATL";
                case LogLevels.None:
                    return null;
                default:
                    throw new ArgumentOutOfRangeException(nameof(level), level, null);
            }
        }

        private void SetupFile()
        {
            if (_rotatingFileLogger != null)
                return;

            string sanitizedTypeName = GetType().Name;
            string fileName = $"{sanitizedTypeName.ToLowerInvariant()}.log";

#if UNITY_EDITOR
            string filePath = Path.GetFullPath(Path.Combine(Application.dataPath, "../Logs/", fileName));
#else
            string filePath = Path.GetFullPath(Path.Combine(Application.streamingAssetsPath, "Logs/", fileName));
#endif
            
            _rotatingFileLogger = new RotatingFileLogger(filePath);
            
            _rotatingFileLogger.WriteLine("====================================================================");
            _rotatingFileLogger.WriteLine($"Log opened at {DateTime.Now.ToLocalTime()}");
        }

        private string SanitizeTypeName(string typeName)
        {
            typeName = typeName.Trim();
            return typeName.Replace(" ", "-");
        }
    }
}