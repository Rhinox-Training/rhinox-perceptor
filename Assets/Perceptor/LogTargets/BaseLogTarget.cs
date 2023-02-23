﻿using UnityEngine;

namespace Rhinox.Perceptor
{
    public abstract class BaseLogTarget : ILogTarget
    {
        public bool Muted { get; protected set; }
        public LogLevels LogLevel { get; protected set; } = LogLevels.Debug;
        public bool ShouldThrowErrors { get; protected set; } = false;

        protected ILogger ActiveLogger { get; private set; }
        
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

            OnLog(level, message, associatedObject);

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