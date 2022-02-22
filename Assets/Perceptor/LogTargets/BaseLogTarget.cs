using UnityEngine;

namespace Rhinox.Perceptor
{
    public abstract class BaseLogTarget : ILogTarget
    {
        public bool Muted { get; private set; }
        public LogLevels LogLevel { get; private set; } = LogLevels.Debug;
        public bool ShouldThrowErrors { get; private set; } = false;

        public void Log(LogLevels level, string message, Object associatedObject = null)
        {
#if NO_LOGGING
            return;
#endif
            if (Muted)
                return;
            
            if (LogLevel > level || LogLevel == LogLevels.None)
                return;

            OnLog(level, message, associatedObject);
        }

        protected abstract void OnLog(LogLevels level, string message, Object associatedObject = null);

        public virtual void ApplySettings(LoggerSettings settings)
        {
            Muted = settings.Muted;
            LogLevel = settings.Level;
            ShouldThrowErrors = settings.ThrowExceptionOnFatal;
        }

        public virtual void Update()
        {
            
        }
    }
}