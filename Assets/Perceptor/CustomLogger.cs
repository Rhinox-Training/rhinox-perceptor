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
        }

        public void ApplySettings(LoggerSettings settings)
        {
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