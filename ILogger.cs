namespace Rhinox.Perceptor
{
    public interface ILogger
    {
        void Log(LogLevels level, string message, UnityEngine.Object associatedObject = null);
        void ApplySettings(LoggerSettings settings);
        void Update();
    }

    public static class ILoggerExtensions
    {
        public static void Log(this ILogger logger, string message, LogLevels level = LogLevels.Info)
        {
            if (logger == null)
                return;
            logger.Log(level, message);
        }
    }
}