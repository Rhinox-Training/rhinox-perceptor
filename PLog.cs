using System.IO;
using System.Runtime.CompilerServices;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
using System;
using System.Collections.Generic;

namespace Rhinox.Perceptor
{
#if RHINOX_MAGNUS
    [ExecutionOrder(-20000), InitializationHandler]
#endif
    public class PLog : MonoBehaviour
    {
        private static PLog instance = null;

        private Dictionary<Type, BaseLogger> _loggerCache;
        private BaseLogger _defaultLogger;
        private static bool _loggedWithoutInitialization;

        private class DefaultLogger : BaseLogger { }

#if RHINOX_MAGNUS
        [BetterRuntimeInitialize(-20000)]
#endif
        public static void CreateIfNotExists()
        {
            if (instance != null)
                return;
            
            GameObject loggerObject = new GameObject("[AUTO-GENERATED] Logger")
            {
                hideFlags = HideFlags.DontSaveInEditor | HideFlags.HideInHierarchy
            };
            instance = loggerObject.AddComponent<PLog>();
            
            // Load loggers
            instance.LoadLoggerCache();
            
            // Load logger settings
            LoggerDefaults ld = LoggerDefaults.Instance;
            foreach (var key in instance._loggerCache.Keys)
            {
                var logger = instance._loggerCache[key];
                if (ld.HasSetting(key))
                    ld.ApplySettings(logger);
                else
                    ld.Add(key, logger.LogLevel, logger.ShouldThrowErrors);
            }
        }

        private void LoadLoggerCache()
        {
            _loggerCache = new Dictionary<Type, BaseLogger>();
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                foreach (var type in assembly.GetTypes())
                {
                    if (!type.IsDefinedTypeOf<BaseLogger>())
                        continue;

                    var logger = Activator.CreateInstance(type) as BaseLogger;

                    if (logger is DefaultLogger)
                        _defaultLogger = logger;
                    _loggerCache.Add(type, logger);
                }
            }
        }
        
        protected virtual void Awake()
        {
            DontDestroyOnLoad(gameObject);
        }
        
        private BaseLogger GetLogger<T>() where T : BaseLogger
        {
            return GetLogger(typeof(T));
        }

        private BaseLogger GetLogger(Type t)
        {
            if (t.IsDefinedTypeOf<BaseLogger>())
                return _loggerCache.ContainsKey(t) ? _loggerCache[t] : _defaultLogger;
            return _defaultLogger;
        }
        
        // =============================================================================================================
        // API

        public static void Trace(string message, GameObject associatedObject = null)
        {
            Log(LogLevels.Trace, message, associatedObject: associatedObject);
        }
        
        public static void Trace<T>(string message, UnityEngine.Object associatedObject = null) where T : BaseLogger
        {
            if (instance == null)
            {
                BackupLog(LogLevels.Trace, message);
                return;
            }
            instance.GetLogger<T>().Log(LogLevels.Trace, message, associatedObject);
        }

        public static void TraceDetailed(string message, [CallerMemberName] string caller = "",
            [CallerFilePath] string callerPath = "", UnityEngine.Object associatedObject = null)
        {
            message = GetCallerMessage(message, caller, callerPath);
            if (instance == null)
            {
                BackupLog(LogLevels.Trace, message);
                return;
            }
            Log(LogLevels.Trace, message, associatedObject: associatedObject);
        }
        
        public static void TraceDetailed<T>(string message, UnityEngine.Object associatedObject = null, [CallerMemberName] string caller = "",
            [CallerFilePath] string callerPath = "") where T : BaseLogger
        {
            message = GetCallerMessage(message, caller, callerPath);
            if (instance == null)
            {
                BackupLog(LogLevels.Trace, message);
                return;
            }
            instance.GetLogger<T>().Log(LogLevels.Trace, message, associatedObject: associatedObject);
        }

        private static string GetCallerMessage(string message, string caller, string callerPath)
        {
            var file = Path.GetFileNameWithoutExtension(callerPath);
            if (string.IsNullOrWhiteSpace(message))
                message = "NULL";
            return $"[{file}::{caller}] {message}";
        }

        public static void Debug(string message, UnityEngine.Object associatedObject = null)
        {
            Log(LogLevels.Debug, message, associatedObject: associatedObject);
        }
        
        public static void Debug<T>(string message, UnityEngine.Object associatedObject = null) where T : BaseLogger
        {
            if (instance == null)
            {
                BackupLog(LogLevels.Debug, message);
                return;
            }
            instance.GetLogger<T>().Log(LogLevels.Debug, message, associatedObject);
        }

        public static void Info(string message, UnityEngine.Object associatedObject = null)
        {
            Log(LogLevels.Info, message, associatedObject: associatedObject);
        }
        
        public static void Info<T>(string message, UnityEngine.Object associatedObject = null) where T : BaseLogger
        {
            if (instance == null)
            {
                BackupLog(LogLevels.Info, message);
                return;
            }
            instance.GetLogger<T>().Log(LogLevels.Info, message, associatedObject);
        }

        public static void Warn(string message, UnityEngine.Object associatedObject = null)
        {
            Log(LogLevels.Warn, message, associatedObject: associatedObject);
        }
        
        public static void Warn<T>(string message, UnityEngine.Object associatedObject = null) where T : BaseLogger
        {
            
            if (instance == null)
            {
                BackupLog(LogLevels.Warn, message);
                return;
            }
            instance.GetLogger<T>().Log(LogLevels.Warn, message, associatedObject);
        }

        public static void Error(string message, UnityEngine.Object associatedObject = null)
        {
            Log(LogLevels.Error, message, associatedObject: associatedObject);
        }
        
        public static void Error<T>(string message, UnityEngine.Object associatedObject = null) where T : BaseLogger
        {
            if (instance == null)
            {
                BackupLog(LogLevels.Error, message);
                return;
            }
            instance.GetLogger<T>().Log(LogLevels.Error, message, associatedObject);
        }

        public static void Fatal(string message, UnityEngine.Object associatedObject = null)
        {
            Log(LogLevels.Fatal, message, associatedObject: associatedObject);
        }
        
        public static void Fatal<T>(string message, UnityEngine.Object associatedObject = null) where T : BaseLogger
        {
            if (instance == null)
            {
                BackupLog(LogLevels.Fatal, message);
                return;
            }
            instance.GetLogger<T>().Log(LogLevels.Fatal, message, associatedObject);
        }

        public static void Fatal(Exception exception, UnityEngine.Object associatedObject = null)
        {
            Log(LogLevels.Fatal, exception.Message, associatedObject);
        }
        
        public static void Fatal<T>(Exception exception, UnityEngine.Object associatedObject = null) where T : BaseLogger
        {
            if (instance == null)
            {
                BackupLog(LogLevels.Fatal, exception.Message);
                return;
            }
            instance.GetLogger<T>().Log(LogLevels.Fatal, exception.Message, associatedObject);
        }

        public static void Log(LogLevels level, string message, UnityEngine.Object associatedObject = null)
        {
            if (instance == null)
            {
                BackupLog(level, message);
                return;
            }
            instance._defaultLogger.Log(level, message, associatedObject);
        }
        
        public static void Log<T>(LogLevels level, string message, UnityEngine.Object associatedObject = null) where T : BaseLogger
        {
            if (instance == null)
            {
                BackupLog(level, message);
                return;
            }
            instance.GetLogger<T>().Log(level, message, associatedObject);
        }
        
        private static void BackupLog(LogLevels level, string message)
        {
            if (!_loggedWithoutInitialization)
            {
                UnityEngine.Debug.LogWarning(
                    $"[WARNING] {nameof(PLog)} hasn't been initialized yet, some of the following lines might not be included in the appropriate logs.");
                _loggedWithoutInitialization = true;
            }
            
            switch (level)
            {
                case LogLevels.Trace:
                case LogLevels.Debug:
                case LogLevels.Info:
                case LogLevels.None:
                    UnityEngine.Debug.Log(message);
                    break;
                case LogLevels.Warn:
                    UnityEngine.Debug.LogWarning(message);
                    break;
                case LogLevels.Error:
                case LogLevels.Fatal:
                    UnityEngine.Debug.LogError(message);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(level), level, null);
            }
        }
    }
}