using UnityEngine;


namespace Rhinox.Perceptor
{
    public class RawMessageBuilder : ILogMessageBuilder
    {
        public string BuildMessage(LogLevels level, string message, Object associatedObject = null)
        {
            return message;
        }
    }
}