using System;
using System.IO;

namespace Rhinox.Perceptor
{
    [Serializable]
    public class LoggerSettings
    {
        public string TypeName;
        public string FullTypeName;
        public bool Muted = false;
        public LogLevels Level;
        public bool ThrowExceptionOnFatal = false;
        
        
        internal const LogLevels DEFAULT_LOG_LEVEL = LogLevels.Info;

        public static LoggerSettings CreateDefault<T>() where T : ILogger
        {
            return CreateDefault(typeof(T));
        }
        
        public static LoggerSettings CreateDefault(Type t)
        {
            if (t == null)
                return null;

            return new LoggerSettings()
            {
                FullTypeName = t.FullName,
                TypeName = t.Name,
                Muted = false,
                Level = DEFAULT_LOG_LEVEL,
                ThrowExceptionOnFatal = false
            };
        }

        public bool CanApplyTo<T>() where T : ILogger
        {
            return CanApplyTo(typeof(T));
        }

        public bool CanApplyTo(Type t)
        {
            if (t == null || FullTypeName == null)
                return false;

            return FullTypeName.Equals(t.FullName);
        }

        public bool CanApplyTo(ILogger logger)
        {
            return CanApplyTo(logger.GetType());
        }
        
        public LoggerSettings Clone()
        {
            return new LoggerSettings()
            {
                TypeName = this.TypeName,
                FullTypeName = this.FullTypeName,
                Muted = this.Muted,
                Level = this.Level,
                ThrowExceptionOnFatal = this.ThrowExceptionOnFatal
            };
        }
    }
}