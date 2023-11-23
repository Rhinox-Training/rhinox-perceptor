using System;
using Object = UnityEngine.Object;


namespace Rhinox.Perceptor
{
    public class LogTimeBuilder : ILogMessageBuilder
    {
        private string _timeFormat;
        private bool _useToString;

        public LogTimeBuilder(string timeFormat = "HH:mm:ss")
        {
            _timeFormat = timeFormat;
            _useToString = !_timeFormat.Contains("{0}") && !_timeFormat.Contains("{0:");
        }

        public string BuildMessage(LogLevels level, string message, Object associatedObject = null)
        {
            if (_useToString)
                return DateTime.Now.ToString(_timeFormat);

            return string.Format(_timeFormat, DateTime.Now);
        }
    }
}