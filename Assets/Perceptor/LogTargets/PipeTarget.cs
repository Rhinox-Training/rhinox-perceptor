using UnityEngine;

namespace Rhinox.Perceptor
{
    internal class PipeTarget<T> : BaseLogTarget where T : ILogger
    {
        private ILogger _logger;

        private PipeTarget()
        {
            
        }

        public static PipeTarget<T> Create()
        {
            var target = new PipeTarget<T>()
            {
                _logger = PLog.GetLogger<T>()
            };
            return target;
        }

        protected override void OnLog(LogLevels level, string message, Object associatedObject = null)
        {
            if (_logger == null)
                return;
            
            _logger.Log(level, message, associatedObject);
        }
    }
}