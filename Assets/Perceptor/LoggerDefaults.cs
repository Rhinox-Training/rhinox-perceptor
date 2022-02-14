using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Rhinox.Perceptor
{
    [Serializable]
    public class LoggerSetting
    {
        public string TypeName;
        public string FullTypeName;
        public LogLevels Level;
        public bool ThrowExceptionOnFatal = false;
    }
    
    public class LoggerDefaults : ScriptableObject
    {
        public List<LoggerSetting> Settings = new List<LoggerSetting>();
        
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
#endif

        public void Add<T>(LogLevels levels, bool throwExceptionOnFatal = false) where T : BaseLogger
        {
            Add(typeof(T), levels, throwExceptionOnFatal);
        }

        public void Add(Type t, LogLevels levels, bool throwExceptionOnFatal = false)
        {
            if (!t.IsDefinedTypeOf<BaseLogger>())
                return;
            
            string fullTypeName = GetFullTypeName(t);
            if (fullTypeName == null)
                return;
            
            if (Settings == null)
                Settings = new List<LoggerSetting>();

            if (Settings.Any(x => x.FullTypeName.Equals(fullTypeName)))
                return;

            LoggerSetting setting = new LoggerSetting()
            {
                FullTypeName = fullTypeName,
                TypeName = t.Name,
                Level = levels,
                ThrowExceptionOnFatal = throwExceptionOnFatal
            };
            Settings.Add(setting);
        }

        private string GetFullTypeName<T>() where T : BaseLogger
        {
            return GetFullTypeName(typeof(T));
        }

        private string GetFullTypeName(Type t)
        {
            if (!t.IsDefinedTypeOf<BaseLogger>())
                return null;
            return t.FullName;
        }

        public void ApplySettings(BaseLogger logger)
        {
            Type t = logger.GetType();
            string fullTypeName = GetFullTypeName(t);
            if (fullTypeName == null)
                return;
            
            if (Settings == null)
                Settings = new List<LoggerSetting>();

            var setting = Settings.FirstOrDefault(x => x.FullTypeName.Equals(fullTypeName));
            if (setting == null)
                return;

            logger.LogLevel = setting.Level;
            logger.ShouldThrowErrors = setting.ThrowExceptionOnFatal;
        }

        public bool HasSetting(Type t)
        {
            if (Settings == null)
                return false;
            
            string fullTypeName = GetFullTypeName(t);
            return Settings.Any(x => x.FullTypeName.Equals(fullTypeName));
        }
    }
}