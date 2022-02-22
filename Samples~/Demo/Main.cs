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
			var sharedFileTarget = FileLogTarget.CreateByName("game");
			Rhinox.Perceptor.PLog.AppendLogTargetToDefault(sharedFileTarget);
			Rhinox.Perceptor.PLog.AppendLogTarget<MyCustomLogger>(sharedFileTarget);
        }

        [MenuItem("Test/TestLogin")]
        public static void LogTest()
        {
            PLog.Info<MyCustomLogger>($"Test log from MenuItem");
        }
    }
}