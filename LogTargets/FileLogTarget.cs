using System;
using System.IO;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Rhinox.Perceptor
{
    public class FileLogTarget : BaseLogTarget
    {
        private RotatingFileLogger _rotatingFileLogger;
        private string FilePath { get; }
        
        private const string DEFAULT_LOGS_FOLDER = "Logs";

        private FileLogTarget(string filePath)
        {
            if (Path.IsPathRooted(filePath))
            {
                FilePath = filePath;
            }
            else
            {
#if UNITY_EDITOR
                FilePath = Path.GetFullPath(Path.Combine(Application.dataPath, "../", filePath));
#else
            FilePath = Path.GetFullPath(Path.Combine(Application.streamingAssetsPath, filePath));
#endif
            }

            SetupFile();
        }

        public static FileLogTarget Create<T>(string customFilePath = null) where T : ILogger
        {
            if (string.IsNullOrWhiteSpace(customFilePath))
            {
                string sanitizedTypeName = SanitizeTypeName(typeof(T).Name);
                string fileName = $"{sanitizedTypeName.ToLowerInvariant()}.log";

                string filePath = Path.Combine($"{DEFAULT_LOGS_FOLDER}/", fileName);
                return new FileLogTarget(filePath);
            }

            return new FileLogTarget(customFilePath);
        }
        
        protected override void OnLog(LogLevels level, string message, Object associatedObject = null)
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
            
            _rotatingFileLogger = new RotatingFileLogger(FilePath);
            
            _rotatingFileLogger.WriteLine("====================================================================");
            _rotatingFileLogger.WriteLine($"Log opened at {DateTime.Now.ToLocalTime()}");
        }

        private static string SanitizeTypeName(string typeName)
        {
            typeName = typeName.Trim();
            return typeName.Replace(" ", "-");
        }
    }
}