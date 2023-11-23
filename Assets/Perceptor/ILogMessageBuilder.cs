using Object = UnityEngine.Object;


namespace Rhinox.Perceptor
{
    public interface ILogMessageBuilder
    {
        string BuildMessage(LogLevels level, string message, Object associatedObject = null);
    }
}