using System;
using Object = UnityEngine.Object;


namespace Rhinox.Perceptor
{
    public class LogLevelBuilder : ILogMessageBuilder
    {
        private bool _useLongForm;
        
        public LogLevelBuilder(bool useLongForm=false)
        {
            _useLongForm = useLongForm;
        }
        
        public string BuildMessage(LogLevels level, string message, Object associatedObject = null)
        {
            if (_useLongForm)
                return level.ToString();

            return ToShortString(level);
        }
        
        private string ToShortString(LogLevels level)
        {
            switch (level)
            {
                case LogLevels.Trace:
                    return "TRAC";
                case LogLevels.Debug:
                    return "DEBG";
                case LogLevels.Info:
                    return "INFO";
                case LogLevels.Warn:
                    return "WARN";
                case LogLevels.Error:
                    return "ERRO";
                case LogLevels.Fatal:
                    return "FATL";
                case LogLevels.None:
                    return null;
                default:
                    throw new ArgumentOutOfRangeException(nameof(level), level, null);
            }
        }
    }
}