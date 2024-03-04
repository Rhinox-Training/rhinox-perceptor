using System;
using UnityEngine;

namespace Rhinox.Perceptor
{
    public class UnityLogTarget : BaseLogTarget
    {
        internal static bool Silence = false;
        
#if UNITY_2021_3_OR_NEWER
        [HideInCallstack]
#endif
        protected override void OnLog(LogLevels level, string message, UnityEngine.Object associatedObject = null)
        {
            if (Silence)
                return;
            
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