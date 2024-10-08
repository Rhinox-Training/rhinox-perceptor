using System.IO;
using System.Runtime.CompilerServices;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
using System;
using System.Collections.Generic;
using System.Diagnostics;
using Object = UnityEngine.Object;

namespace Rhinox.Perceptor
{
    public class PLog : MonoBehaviour
    {
        private static PLog _instance = null;

        private Dictionary<Type, ILogger> _loggerCache;
        private ILogger _defaultLogger;
        public static readonly ILogMessageBuilder DefaultBuilder = new RawMessageBuilder();
        
        public bool Loaded { get; private set; }
        
        private static bool _loggedWithoutInitialization;


        private class DefaultLogger : CustomLogger { }

        public static void CreateIfNotExists(bool flushPrecacheOnInit = true)
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
            
            // Load LogTargetCache
            _instance.TryLoadLogTargetCache();
            
            // Load logger settings
            LoggerDefaults ld = LoggerDefaults.Instance;
            foreach (var loggerType in _instance._loggerCache.Keys)
            {
                var logger = _instance._loggerCache[loggerType];
                if (ld.HasSetting(loggerType))
                {
                    var setting = ld.FindSetting(loggerType);
                    logger.ApplySettings(setting);
                }
            }

            _instance.Loaded = true;

            if (_cacheLogger != null)
            {
                if (flushPrecacheOnInit)
                    _cacheLogger.FlushCache(_instance._loggerCache.Values);
                else
                    _cacheLogger.Clear();
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

                if (logger is CustomLogger customLogger)
                    customLogger.SetupTargets();
                
                _loggerCache.Add(type, logger);
            }
        }
        
        protected virtual void Awake()
        {
            DontDestroyOnLoad(gameObject);
        }

        public static ILogger GetLogger<T>() where T : ILogger
        {
            if (_instance == null)
                return null;
            return _instance.GetLoggerInternal<T>();
        }

        public static ILogger GetLogger(Type loggerType)
        {
            if (_instance == null)
                return null;
            return _instance.GetLoggerInternal(loggerType);
        }
        
        public static ILogger GetDefaultLogger()
        {
            if (_instance == null)
                return null;
            return _instance._defaultLogger;
        }
        
        public static bool SetForDefault(LogLevels level, bool throwOnException = false)
        {
            return Set<DefaultLogger>(level, throwOnException);
        }

        public static bool Set<T>(LogLevels level, bool throwOnException = false) where T : ILogger
        {
            var logger = GetLogger<T>();
            if (logger == null)
                return false;

            var settings = LoggerDefaults.Instance.FindSetting(typeof(T));
            if (settings == null)
                settings = LoggerSettings.CreateDefault<T>();
            
            settings.Level = level;
            settings.ThrowExceptionOnFatal = throwOnException;
            
            logger.ApplySettings(settings);
            return true;
        }
        
        public static bool Set(Type loggerType, LogLevels level, bool throwOnException = false)
        {
            var logger = GetLogger(loggerType);
            if (logger == null)
                return false;

            var settings = LoggerDefaults.Instance.FindSetting(loggerType);
            if (settings == null)
                settings = LoggerSettings.CreateDefault(loggerType);

            settings.Level = level;
            settings.ThrowExceptionOnFatal = throwOnException;
            
            logger.ApplySettings(settings);
            return true;
        }
        
        private ILogger GetLoggerInternal<T>() where T : ILogger
        {
            return GetLoggerInternal(typeof(T));
        }

        private ILogger GetLoggerInternal(Type t)
        {
            if (t != null && t.IsDefinedTypeOf<ILogger>())
                return _loggerCache.ContainsKey(t) ? _loggerCache[t] : _defaultLogger;
            return _defaultLogger;
        }

        private static Dictionary<Type, List<ILogTarget>> _targetInitializationCache;
        public static Dictionary<Type, List<ILogTarget>> LoggerTargetDict => _targetInitializationCache;
        private static LogPrecache _cacheLogger;

        public static void AppendLogTarget<T>(ILogTarget target) where T : CustomLogger
        {
            if (target == null)
                return;
            
            if (_targetInitializationCache == null)
                _targetInitializationCache = new Dictionary<Type, List<ILogTarget>>();

            Type t = typeof(T);

            List<ILogTarget> list = null;
            if (!_targetInitializationCache.ContainsKey(t))
            {
                list = new List<ILogTarget>();
                _targetInitializationCache.Add(t, list);
            }
            else
            {
                list = _targetInitializationCache[t];
            }

            if (!list.Contains(target))
                list.Add(target);

            _targetInitializationCache[t] = list;

            if (_instance != null && _instance.Loaded)
                _instance.TryLoadLogTargetCache();
        }

        public static void AppendLogTargetToDefault(ILogTarget target) => AppendLogTarget<DefaultLogger>(target);

        private void TryLoadLogTargetCache()
        {
            if (_targetInitializationCache == null)
                return;

            foreach (var type in _targetInitializationCache.Keys)
            {
                if (type == null)
                    continue;

                var logger = GetLoggerInternal(type);
                if (!(logger is CustomLogger customLogger))
                    continue;
                
                var cacheList = _targetInitializationCache[type];
                foreach (var entry in cacheList)
                {
                    if (entry == null)
                        continue;
                    
                    customLogger.AppendTarget(entry);
                }
            }
        }

        // =============================================================================================================
        // API

        [Conditional(LoggerDefaults.DefineSymbolPrefix + "TRACE")]
        public static void Trace(string message, GameObject associatedObject = null)
        {
            Log(LogLevels.Trace, message, associatedObject: associatedObject);
        }
        
#if UNITY_2021_3_OR_NEWER
        [HideInCallstack]
#endif
        [Conditional(LoggerDefaults.DefineSymbolPrefix + "TRACE")]
        public static void Trace<T>(string message, UnityEngine.Object associatedObject = null) where T : ILogger
        {
            if (_instance == null)
            {
                BackupLog(typeof(T), LogLevels.Trace, message, associatedObject);
                return;
            }
            _instance.GetLoggerInternal<T>().Log(LogLevels.Trace, message, associatedObject);
        }

#if UNITY_2021_3_OR_NEWER
        [HideInCallstack]
#endif
        [Conditional(LoggerDefaults.DefineSymbolPrefix + "TRACE")]
        public static void TraceDetailed(string message, [CallerMemberName] string caller = "",
            [CallerFilePath] string callerPath = "", UnityEngine.Object associatedObject = null)
        {
            message = GetCallerMessage(message, caller, callerPath);
            if (_instance == null)
            {
                BackupLog(typeof(DefaultLogger), LogLevels.Trace, message, associatedObject);
                return;
            }
            Log(LogLevels.Trace, message, associatedObject: associatedObject);
        }
        
#if UNITY_2021_3_OR_NEWER
        [HideInCallstack]
#endif
        [Conditional(LoggerDefaults.DefineSymbolPrefix + "TRACE")]
        public static void TraceDetailed<T>(string message, UnityEngine.Object associatedObject = null, [CallerMemberName] string caller = "",
            [CallerFilePath] string callerPath = "") where T : ILogger
        {
            message = GetCallerMessage(message, caller, callerPath);
            if (_instance == null)
            {
                BackupLog(typeof(T), LogLevels.Trace, message, associatedObject);
                return;
            }
            _instance.GetLoggerInternal<T>().Log(LogLevels.Trace, message, associatedObject: associatedObject);
        }

        private static Dictionary<string, string> _fileNameByPath = new Dictionary<string, string>();
        private static string GetCallerMessage(string message, string caller, string callerPath)
        {
            if (!_fileNameByPath.TryGetValue(callerPath, out var file))
                _fileNameByPath[callerPath] = Path.GetFileNameWithoutExtension(callerPath);
            
            if (string.IsNullOrWhiteSpace(message))
                message = "NULL";
            return $"[{file}::{caller}] {message}";
        }

#if UNITY_2021_3_OR_NEWER
        [HideInCallstack]
#endif
        [Conditional(LoggerDefaults.DefineSymbolPrefix + "TRACE")]
        [Conditional(LoggerDefaults.DefineSymbolPrefix + "DEBUG")]
        public static void Debug(string message, UnityEngine.Object associatedObject = null)
        {
            Log(LogLevels.Debug, message, associatedObject: associatedObject);
        }
        
#if UNITY_2021_3_OR_NEWER
        [HideInCallstack]
#endif
        [Conditional(LoggerDefaults.DefineSymbolPrefix + "TRACE")]
        [Conditional(LoggerDefaults.DefineSymbolPrefix + "DEBUG")]
        public static void Debug<T>(string message, UnityEngine.Object associatedObject = null) where T : ILogger
        {
            if (_instance == null)
            {
                BackupLog(typeof(T), LogLevels.Debug, message, associatedObject);
                return;
            }
            _instance.GetLoggerInternal<T>().Log(LogLevels.Debug, message, associatedObject);
        }

#if UNITY_2021_3_OR_NEWER
        [HideInCallstack]
#endif
        [Conditional(LoggerDefaults.DefineSymbolPrefix + "TRACE")]
        [Conditional(LoggerDefaults.DefineSymbolPrefix + "DEBUG")]
        [Conditional(LoggerDefaults.DefineSymbolPrefix + "INFO")]
        public static void Info(string message, UnityEngine.Object associatedObject = null)
        {
            Log(LogLevels.Info, message, associatedObject: associatedObject);
        }
        
#if UNITY_2021_3_OR_NEWER
        [HideInCallstack]
#endif
        [Conditional(LoggerDefaults.DefineSymbolPrefix + "TRACE")]
        [Conditional(LoggerDefaults.DefineSymbolPrefix + "DEBUG")]
        [Conditional(LoggerDefaults.DefineSymbolPrefix + "INFO")]
        public static void Info<T>(string message, UnityEngine.Object associatedObject = null) where T : ILogger
        {
            if (_instance == null)
            {
                BackupLog(typeof(T), LogLevels.Info, message, associatedObject);
                return;
            }
            _instance.GetLoggerInternal<T>().Log(LogLevels.Info, message, associatedObject);
        }

#if UNITY_2021_3_OR_NEWER
        [HideInCallstack]
#endif
        [Conditional(LoggerDefaults.DefineSymbolPrefix + "TRACE")]
        [Conditional(LoggerDefaults.DefineSymbolPrefix + "DEBUG")]
        [Conditional(LoggerDefaults.DefineSymbolPrefix + "INFO")]
        [Conditional(LoggerDefaults.DefineSymbolPrefix + "WARN")]
        public static void Warn(string message, UnityEngine.Object associatedObject = null)
        {
            Log(LogLevels.Warn, message, associatedObject: associatedObject);
        }
        
#if UNITY_2021_3_OR_NEWER
        [HideInCallstack]
#endif
        [Conditional(LoggerDefaults.DefineSymbolPrefix + "TRACE")]
        [Conditional(LoggerDefaults.DefineSymbolPrefix + "DEBUG")]
        [Conditional(LoggerDefaults.DefineSymbolPrefix + "INFO")]
        [Conditional(LoggerDefaults.DefineSymbolPrefix + "WARN")]
        public static void Warn<T>(string message, UnityEngine.Object associatedObject = null) where T : ILogger
        {
            
            if (_instance == null)
            {
                BackupLog(typeof(T), LogLevels.Warn, message, associatedObject);
                return;
            }
            _instance.GetLoggerInternal<T>().Log(LogLevels.Warn, message, associatedObject);
        }

#if UNITY_2021_3_OR_NEWER
        [HideInCallstack]
#endif
        [Conditional(LoggerDefaults.DefineSymbolPrefix + "TRACE")]
        [Conditional(LoggerDefaults.DefineSymbolPrefix + "DEBUG")]
        [Conditional(LoggerDefaults.DefineSymbolPrefix + "INFO")]
        [Conditional(LoggerDefaults.DefineSymbolPrefix + "WARN")]
        [Conditional(LoggerDefaults.DefineSymbolPrefix + "ERROR")]
        public static void Error(string message, UnityEngine.Object associatedObject = null)
        {
            Log(LogLevels.Error, message, associatedObject: associatedObject);
        }
        
#if UNITY_2021_3_OR_NEWER
        [HideInCallstack]
#endif
        [Conditional(LoggerDefaults.DefineSymbolPrefix + "TRACE")]
        [Conditional(LoggerDefaults.DefineSymbolPrefix + "DEBUG")]
        [Conditional(LoggerDefaults.DefineSymbolPrefix + "INFO")]
        [Conditional(LoggerDefaults.DefineSymbolPrefix + "WARN")]
        [Conditional(LoggerDefaults.DefineSymbolPrefix + "ERROR")]
        public static void Error<T>(string message, UnityEngine.Object associatedObject = null) where T : ILogger
        {
            if (_instance == null)
            {
                BackupLog(typeof(T), LogLevels.Error, message, associatedObject);
                return;
            }
            _instance.GetLoggerInternal<T>().Log(LogLevels.Error, message, associatedObject);
        }

#if UNITY_2021_3_OR_NEWER
        [HideInCallstack]
#endif
        public static void Fatal(string message, UnityEngine.Object associatedObject = null)
        {
            Log(LogLevels.Fatal, message, associatedObject: associatedObject);
        }
        
#if UNITY_2021_3_OR_NEWER
        [HideInCallstack]
#endif
        public static void Fatal<T>(string message, UnityEngine.Object associatedObject = null) where T : ILogger
        {
            if (_instance == null)
            {
                BackupLog(typeof(T), LogLevels.Fatal, message, associatedObject);
                return;
            }
            _instance.GetLoggerInternal<T>().Log(LogLevels.Fatal, message, associatedObject);
        }

#if UNITY_2021_3_OR_NEWER
        [HideInCallstack]
#endif
        public static void Fatal(Exception exception, UnityEngine.Object associatedObject = null)
        {
            Log(LogLevels.Fatal, exception.Message, associatedObject);
        }
        
#if UNITY_2021_3_OR_NEWER
        [HideInCallstack]
#endif
        public static void Fatal<T>(Exception exception, UnityEngine.Object associatedObject = null) where T : ILogger
        {
            if (_instance == null)
            {
                BackupLog(typeof(T), LogLevels.Fatal, exception.Message, associatedObject);
                return;
            }
            _instance.GetLoggerInternal<T>().Log(LogLevels.Fatal, exception.Message, associatedObject);
        }

#if UNITY_2021_3_OR_NEWER
        [HideInCallstack]
#endif
        public static void Log(LogLevels level, string message, UnityEngine.Object associatedObject = null)
        {
            if (_instance == null)
            {
                BackupLog(typeof(DefaultLogger), level, message, associatedObject);
                return;
            }
            _instance._defaultLogger.Log(level, message, associatedObject);
        }
        
#if UNITY_2021_3_OR_NEWER
        [HideInCallstack]
#endif
        public static void Log<T>(LogLevels level, string message, UnityEngine.Object associatedObject = null) where T : ILogger
        {
            if (_instance == null)
            {
                BackupLog(typeof(T), level, message, associatedObject);
                return;
            }
            _instance.GetLoggerInternal<T>().Log(level, message, associatedObject);
        }
        
#if UNITY_2021_3_OR_NEWER
        [HideInCallstack]
#endif
        private static void BackupLog(Type loggerType, LogLevels level, string message, UnityEngine.Object associatedObject = null)
        {
            if (!_loggedWithoutInitialization)
            {
                UnityEngine.Debug.LogWarning(
                    $"[WARNING] {nameof(PLog)} hasn't been initialized yet, entries will be cached and can be flushed during initialization.");
                _loggedWithoutInitialization = true;
            }
            
            if (_cacheLogger == null)
                _cacheLogger = new LogPrecache();

            _cacheLogger.CacheEntry(loggerType, level, message, associatedObject);
            
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