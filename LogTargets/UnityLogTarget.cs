using System;

namespace Rhinox.Perceptor
{
    public class UnityLogTarget : BaseLogTarget
    {
        protected override void OnLog(LogLevels level, string message, UnityEngine.Object associatedObject = null)
        {
            // Also log to Unity
            switch (level)
            {
                case LogLevels.Trace:
                case LogLevels.Debug:
                case LogLevels.Info:
                    UnityEngine.Debug.Log(message, associatedObject);
                    break;
                case LogLevels.Warn:
                    UnityEngine.Debug.LogWarning(message, associatedObject);
                    break;
                case LogLevels.Error:
                case LogLevels.Fatal:
                    if (ShouldThrowErrors)
                        throw new Exception(message);
                    else
                        UnityEngine.Debug.LogError(message, associatedObject);
                    break;
            }
        }
    }
}