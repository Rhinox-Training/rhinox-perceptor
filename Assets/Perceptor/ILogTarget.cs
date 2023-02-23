namespace Rhinox.Perceptor
{
    public interface ILogTarget
    {
        void Log(LogLevels level, string message, UnityEngine.Object associatedObject = null, ILogger sender = null);
        void ApplySettings(LoggerSettings settings);
        void Update();
    }
}