using Rhinox.Perceptor;
using UnityEditor;
using UnityEngine;

namespace Perceptor.Editor
{
    [CustomEditor(typeof(LoggerDefaults))]
    public class LoggerDefaultsEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            
            if (GUILayout.Button("Populate LoggerTypes"))
            {
                LoggerDefaults.TryPopulate();
            }
        }
    }
}