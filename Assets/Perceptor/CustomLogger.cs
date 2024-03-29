using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Rhinox.Perceptor
{
    public abstract class CustomLogger : ILogger
    {
        public LogLevels LogLevel = LogLevels.Debug;
        public bool ShouldThrowErrors = false;
        private readonly List<ILogTarget> _logTargets;
        private LoggerSettings _currentSettings;

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
            {
                if (_currentSettings != null)
                    target.ApplySettings(_currentSettings);
                _logTargets.Add(target);
            }
        }

#if UNITY_2021_3_OR_NEWER
        [HideInCallstack]
#endif
        public void Log(LogLevels level, string message, UnityEngine.Object associatedObject = null)
        {
            if (_logTargets == null)
                return;

            foreach (var target in _logTargets)
            {
                if (target == null)
                    continue;
                
                target.Log(level, message, associatedObject, this);
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
            
            _currentSettings = settings;
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