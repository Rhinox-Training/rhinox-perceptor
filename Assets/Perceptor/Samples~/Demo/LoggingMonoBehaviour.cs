using Rhinox.Perceptor;
using UnityEngine;

namespace Samples
{
    public class LoggingMonoBehaviour : MonoBehaviour
    {
        void Awake()
        {
            PLog.Debug<MyCustomLogger>($"Log from awake", this);
        }
    }
}