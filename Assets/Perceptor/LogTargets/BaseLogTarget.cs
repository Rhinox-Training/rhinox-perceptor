using UnityEngine;


namespace Rhinox.Perceptor
{
    public abstract class BaseLogTarget : ILogTarget
    {
        public bool Muted { get; protected set; }
        public LogLevels LogLevel { get; protected set; } = LogLevels.Debug;
        public bool ShouldThrowErrors { get; protected set; } = false;
        
        protected ILogMessageBuilder _customBuilder;

        protected ILogger ActiveLogger { get; private set; }

        public void SetCustomMessageBuilder(ILogMessageBuilder customBuilder)
        {
            if (customBuilder == null)
                return;

            _customBuilder = customBuilder;
        }

#if UNITY_2021_3_OR_NEWER
        [HideInCallstack]
#endif
        public void Log(LogLevels level, string message, Object associatedObject = null, ILogger sender = null)
        {
#if NO_LOGGING
            return;
#endif
            if (Muted)
                return;

            if (LogLevel > level || LogLevel == LogLevels.None)
                return;

            ActiveLogger = sender;

            var messageBuilder = _customBuilder ?? PLog.DefaultBuilder;
            string builtMessage = messageBuilder.BuildMessage(level, message, associatedObject);

            OnLog(level, builtMessage, associatedObject);

            ActiveLogger = null;
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