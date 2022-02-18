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
        private static PLog _instance = null;

        private Dictionary<Type, ILogger> _loggerCache;
        private ILogger _defaultLogger;
        private static bool _loggedWithoutInitialization;

        private class DefaultLogger : CustomLogger { }

#if RHINOX_MAGNUS
        [BetterRuntimeInitialize(-20000)]
#endif
        public static void CreateIfNotExists()
        {
            if (_instance != null)
                return;
            
            GameObject loggerObject = new GameObject("[AUTO-GENERATED] Logger")
            {
                hideFlags = HideFlags.DontSaveInEditor | HideFlags.HideInHierarchy
            };
            _instance = loggerObject.AddComponent<PLog>();
            
            // Load loggers
            _instance.LoadLoggerInstances();
            
            // Load logger settings
            LoggerDefaults ld = LoggerDefaults.Instance;
            foreach (var key in _instance._loggerCache.Keys)
            {
                var logger = _instance._loggerCache[key];
                if (ld.HasSetting(key))
                    ld.ApplySettings(logger);
            }
        }

        public static ICollection<Type> FindLoggerTypes()
        {
            var result = new List<Type>();
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                foreach (var type in assembly.GetTypes())
                {
                    if (!type.IsDefinedTypeOf<ILogger>())
                        continue;
                    
                    result.Add(type);
                }
            }
            return result;
        }

        private void LoadLoggerInstances()
        {
            _loggerCache = new Dictionary<Type, ILogger>();
            foreach (var type in FindLoggerTypes())
            {
                var logger = Activator.CreateInstance(type) as ILogger;

                if (logger is DefaultLogger)
                    _defaultLogger = logger;
                _loggerCache.Add(type, logger);
            }
        }
        
        protected virtual void Awake()
        {
            DontDestroyOnLoad(gameObject);
        }
        
        private ILogger GetLogger<T>() where T : ILogger
        {
            return GetLogger(typeof(T));
        }

        private ILogger GetLogger(Type t)
        {
            if (t.IsDefinedTypeOf<ILogger>())
                return _loggerCache.ContainsKey(t) ? _loggerCache[t] : _defaultLogger;
            return _defaultLogger;
        }
        
        // =============================================================================================================
        // API

        public static void Trace(string message, GameObject associatedObject = null)
        {
            Log(LogLevels.Trace, message, associatedObject: associatedObject);
        }
        
        public static void Trace<T>(string message, UnityEngine.Object associatedObject = null) where T : CustomLogger
        {
            if (_instance == null)
            {
                BackupLog(LogLevels.Trace, message);
                return;
            }
            _instance.GetLogger<T>().Log(LogLevels.Trace, message, associatedObject);
        }

        public static void TraceDetailed(string message, [CallerMemberName] string caller = "",
            [CallerFilePath] string callerPath = "", UnityEngine.Object associatedObject = null)
        {
            message = GetCallerMessage(message, caller, callerPath);
            if (_instance == null)
            {
                BackupLog(LogLevels.Trace, message);
                return;
            }
            Log(LogLevels.Trace, message, associatedObject: associatedObject);
        }
        
        public static void TraceDetailed<T>(string message, UnityEngine.Object associatedObject = null, [CallerMemberName] string caller = "",
            [CallerFilePath] string callerPath = "") where T : CustomLogger
        {
            message = GetCallerMessage(message, caller, callerPath);
            if (_instance == null)
            {
                BackupLog(LogLevels.Trace, message);
                return;
            }
            _instance.GetLogger<T>().Log(LogLevels.Trace, message, associatedObject: associatedObject);
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
        
        public static void Debug<T>(string message, UnityEngine.Object associatedObject = null) where T : CustomLogger
        {
            if (_instance == null)
            {
                BackupLog(LogLevels.Debug, message);
                return;
            }
            _instance.GetLogger<T>().Log(LogLevels.Debug, message, associatedObject);
        }

        public static void Info(string message, UnityEngine.Object associatedObject = null)
        {
            Log(LogLevels.Info, message, associatedObject: associatedObject);
        }
        
        public static void Info<T>(string message, UnityEngine.Object associatedObject = null) where T : CustomLogger
        {
            if (_instance == null)
            {
                BackupLog(LogLevels.Info, message);
                return;
            }
            _instance.GetLogger<T>().Log(LogLevels.Info, message, associatedObject);
        }

        public static void Warn(string message, UnityEngine.Object associatedObject = null)
        {
            Log(LogLevels.Warn, message, associatedObject: associatedObject);
        }
        
        public static void Warn<T>(string message, UnityEngine.Object associatedObject = null) where T : CustomLogger
        {
            
            if (_instance == null)
            {
                BackupLog(LogLevels.Warn, message);
                return;
            }
            _instance.GetLogger<T>().Log(LogLevels.Warn, message, associatedObject);
        }

        public static void Error(string message, UnityEngine.Object associatedObject = null)
        {
            Log(LogLevels.Error, message, associatedObject: associatedObject);
        }
        
        public static void Error<T>(string message, UnityEngine.Object associatedObject = null) where T : CustomLogger
        {
            if (_instance == null)
            {
                BackupLog(LogLevels.Error, message);
                return;
            }
            _instance.GetLogger<T>().Log(LogLevels.Error, message, associatedObject);
        }

        public static void Fatal(string message, UnityEngine.Object associatedObject = null)
        {
            Log(LogLevels.Fatal, message, associatedObject: associatedObject);
        }
        
        public static void Fatal<T>(string message, UnityEngine.Object associatedObject = null) where T : CustomLogger
        {
            if (_instance == null)
            {
                BackupLog(LogLevels.Fatal, message);
                return;
            }
            _instance.GetLogger<T>().Log(LogLevels.Fatal, message, associatedObject);
        }

        public static void Fatal(Exception exception, UnityEngine.Object associatedObject = null)
        {
            Log(LogLevels.Fatal, exception.Message, associatedObject);
        }
        
        public static void Fatal<T>(Exception exception, UnityEngine.Object associatedObject = null) where T : CustomLogger
        {
            if (_instance == null)
            {
                BackupLog(LogLevels.Fatal, exception.Message);
                return;
            }
            _instance.GetLogger<T>().Log(LogLevels.Fatal, exception.Message, associatedObject);
        }

        public static void Log(LogLevels level, string message, UnityEngine.Object associatedObject = null)
        {
            if (_instance == null)
            {
                BackupLog(level, message);
                return;
            }
            _instance._defaultLogger.Log(level, message, associatedObject);
        }
        
        public static void Log<T>(LogLevels level, string message, UnityEngine.Object associatedObject = null) where T : CustomLogger
        {
            if (_instance == null)
            {
                BackupLog(level, message);
                return;
            }
            _instance.GetLogger<T>().Log(level, message, associatedObject);
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

        private void Update()
        {
            if (_loggerCache == null)
                return;
            
            foreach (var logger in _loggerCache.Values)
            {
                if (logger == null)
                    continue;
                logger.Update();
            }
        }
    }
}