﻿using System;
using System.Collections.Generic;
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

        private static Dictionary<string, FileLogTarget> _fileLogTargets;

        private FileLogTarget(string rootedFilePath)
        {
            FilePath = rootedFilePath;
            SetupFile();
        }

        public static FileLogTarget CreateByPath(string customFilePath)
        {
            if (string.IsNullOrWhiteSpace(customFilePath))
                throw new ArgumentNullException(nameof(customFilePath));
            return FindOrCreateTarget(customFilePath);
        }

        public static FileLogTarget CreateByName(string name)
        {
            string sanitizedTypeName = SanitizeTypeName(name);
            string fileName = $"{sanitizedTypeName.ToLowerInvariant()}.log";

            string filePath = Path.Combine($"{DEFAULT_LOGS_FOLDER}/", fileName);
            return FindOrCreateTarget(filePath);
        }

        private static FileLogTarget FindOrCreateTarget(string filePath)
        {
            if (_fileLogTargets == null)
                _fileLogTargets = new Dictionary<string, FileLogTarget>();

            filePath = GetAbsolutePath(filePath);

            if (_fileLogTargets.ContainsKey(filePath))
                return _fileLogTargets[filePath];

            var target = new FileLogTarget(filePath);
            _fileLogTargets.Add(filePath, target);
            return target;
        }

        private static string GetAbsolutePath(string filePath)
        {
            if (Path.IsPathRooted(filePath))
            {
                return filePath;
            }
            else
            {
#if UNITY_EDITOR
                return Path.GetFullPath(Path.Combine(Application.dataPath, "../", filePath));
#else
                return Path.GetFullPath(Path.Combine(Application.streamingAssetsPath, filePath));
#endif
            }
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