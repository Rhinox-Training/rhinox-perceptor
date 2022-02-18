using System;
using System.Collections.Generic;
using System.IO;
using Perceptor.Util;
using UnityEngine;
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

        public virtual void SetupTargets()
        {
            _logTargets.Add(new UnityLogTarget());
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