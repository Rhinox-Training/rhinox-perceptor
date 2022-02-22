using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Rhinox.Perceptor
{
    public class LoggerDefaults : ScriptableObject
    {
        public List<LoggerSettings> Settings = new List<LoggerSettings>();

        private const LogLevels DEFAULT_LOG_LEVEL = LogLevels.Info;
        
        private static LoggerDefaults _instance;
        public static LoggerDefaults Instance
        {
            get
            {
                if (_instance == null)
                    _instance = CreateIfNotExists();
                     
                return _instance;
            }
        }

        private static LoggerDefaults CreateIfNotExists()
        {
            LoggerDefaults instance = Resources.Load<LoggerDefaults>(nameof(LoggerDefaults));
            string configFullPath = $"Assets/Resources/{nameof(LoggerDefaults)}.asset";

            if (instance == null)
            {
                PLog.Warn($"Resource '{nameof(LoggerDefaults)}' not found at {configFullPath}, creating new ScriptableObject...");
                LoggerDefaults so = ScriptableObject.CreateInstance<LoggerDefaults>();
#if UNITY_EDITOR
                EnsureResourcesFolder();
                AssetDatabase.CreateAsset(so, configFullPath);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
#endif
                instance = so;
            }

            return instance;
        }
        
#if UNITY_EDITOR
        private static void EnsureResourcesFolder()
        {
            string dir = Path.Combine(Application.dataPath, "Resources");
            DirectoryInfo di = new DirectoryInfo(dir);
            if (!di.Exists)
            {
                AssetDatabase.CreateFolder("Assets", "Resources");
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }
        }
        
        public static void TryPopulate()
        {
            var instance = Instance;
            foreach (var type in PLog.FindLoggerTypes())
            {
                bool typeAlreadyConfigured = false;
                foreach (var setting in instance.Settings)
                {
                    if (setting == null || setting.FullTypeName == null)
                        continue;

                    if (setting.FullTypeName.Equals(type.FullName))
                    {
                        typeAlreadyConfigured = true;
                        break;
                    }
                }
                
                if (!typeAlreadyConfigured)
                    instance.Add(type);
            }
        }
#endif

        public void Add<T>(LogLevels levels = DEFAULT_LOG_LEVEL, bool throwExceptionOnFatal = false) where T : ILogger
        {
            Add(typeof(T), levels, throwExceptionOnFatal);
        }

        public void Add(Type loggerType, LogLevels levels = DEFAULT_LOG_LEVEL, bool throwExceptionOnFatal = false)
        {
            if (loggerType == null || !loggerType.IsDefinedTypeOf<ILogger>())
                return;

            string fullTypeName = loggerType.FullName;
            
            if (Settings == null)
                Settings = new List<LoggerSettings>();

            if (Settings.Any(x => x.FullTypeName.Equals(fullTypeName)))
                return;

            LoggerSettings settings = LoggerSettings.CreateDefault(loggerType);
            settings.Level = levels;
            settings.ThrowExceptionOnFatal = throwExceptionOnFatal;
            Settings.Add(settings);
        }
        
        public bool HasSetting(Type t)
        {
            if (Settings == null)
                return false;
            
            return Settings.Any(x => x.FullTypeName.Equals(t.FullName));
        }

        public LoggerSettings FindSetting(Type t)
        {
            if (Settings == null)
                return null;
            
            var setting = Settings.FirstOrDefault(x => x.FullTypeName.Equals(t.FullName));
            return setting?.Clone();
        }

    }
}