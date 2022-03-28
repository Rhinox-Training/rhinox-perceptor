using System;
using System.Collections.Generic;
using Object = UnityEngine.Object;

namespace Rhinox.Perceptor
{
    public abstract class CustomLogger : ILogger
    {
        public LogLevels LogLevel = LogLevels.Debug;
        public bool ShouldThrowErrors = false;
        private readonly List<ILogTarget> _logTargets;

        public delegate void LogHandler(LogLevels level, string message, UnityEngine.Object associatedObject = null);
        public event LogHandler Logged; 
        public static event LogHandler GlobalLogged; 

        protected CustomLogger(LogLevels levels = LogLevels.Debug, bool shouldThrowErrors = false)
        {
            LogLevel = levels;
            ShouldThrowErrors = shouldThrowErrors;
            _logTargets = new List<ILogTarget>();
        }

        internal void SetupTargets()
        {
            foreach (var target in GetTargets())
            {
                if (target == null)
                    continue;
                
                _logTargets.Add(target);
            }
        }

        protected virtual ILogTarget[] GetTargets()
        {
            return new[]
            {
                new UnityLogTarget()
            };
        }

        internal void AppendTarget(ILogTarget target)
        {
            if (target != null && !_logTargets.Contains(target))
                _logTargets.Add(target);
        }

        public void Log(LogLevels level, string message, UnityEngine.Object associatedObject = null)
        {
            if (_logTargets == null)
                return;

            foreach (var target in _logTargets)
            {
                if (target == null)
                    continue;
                
                target.Log(level, message, associatedObject);
            }
            
            TriggerLogged(level, message, associatedObject);
        }

        protected virtual void TriggerLogged(LogLevels level, string message, UnityEngine.Object associatedObject = null)
        {
            Logged?.Invoke(level, message, associatedObject);
            GlobalLogged?.Invoke(level, message, associatedObject);
        }

        public void ApplySettings(LoggerSettings settings)
        {
            if (settings == null || !settings.CanApplyTo(this))
                return;
            
            if (_logTargets == null)
                return;

            foreach (var target in _logTargets)
            {
                if (target == null)
                    continue;
                target.ApplySettings(settings);
            }
        }

        public virtual void Update()
        {
            if (_logTargets == null)
                return;

            foreach (var target in _logTargets)
            {
                if (target == null)
                    continue;
                target.Update();
            }
        }
    }
}