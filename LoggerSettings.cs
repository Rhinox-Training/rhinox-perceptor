using System;

namespace Rhinox.Perceptor
{
    [Serializable]
    public class LoggerSettings
    {
        public string TypeName;
        public string FullTypeName;
        public bool Muted = false;
        public LogLevels Level;
        public bool ThrowExceptionOnFatal = false;
    }
}