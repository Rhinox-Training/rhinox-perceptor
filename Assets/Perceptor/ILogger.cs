namespace Rhinox.Perceptor
{
    public interface ILogger
    {
        void Log(LogLevels level, string message, UnityEngine.Object associatedObject = null);
        void Update();
    }
}