using Rhinox.Perceptor;
using UnityEditor;
using UnityEngine;

namespace Samples
{
    
    public static class Main
    {
        [RuntimeInitializeOnLoadMethod]
        public static void Start()
        {
            Rhinox.Perceptor.PLog.CreateIfNotExists();
        }

        [MenuItem("Test/TestLogin")]
        public static void LogTest()
        {
            PLog.Info<MyCustomLogger>($"Test log from MenuItem");
        }
    }
}