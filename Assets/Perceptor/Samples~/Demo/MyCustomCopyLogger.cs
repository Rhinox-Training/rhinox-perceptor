using Rhinox.Perceptor;

namespace Samples
{
    public class MyCustomLogger : CustomCopyLogger<DefaultLogger>
    {
        public override void SetupTargets()
        {
            _logTargets.Add(FileLogTarget.CreateByPath("../Logs/default.log"));
        }
    }
}