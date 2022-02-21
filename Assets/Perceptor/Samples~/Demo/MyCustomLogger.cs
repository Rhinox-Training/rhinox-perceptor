using Rhinox.Perceptor;

namespace Samples
{
    public class MyCustomLogger : CustomLogger
    {
        public override void SetupTargets()
        {
            _logTargets.Add(FileLogTarget.CreateByName("custom"));
            _logTargets.Add(FileLogTarget.CreateByPath("../Logs/custom.log"));
            _logTargets.Add(new UnityLogTarget());
        }
    }
}