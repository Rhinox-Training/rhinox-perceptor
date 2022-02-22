using Rhinox.Perceptor;

namespace Samples
{
    public class MyCustomLogger : CustomLogger
    {
        protected override ILogTarget[] GetTargets()
        {
			return new ILogTarget[] 
			{
				FileLogTarget.CreateByName("custom"),
				FileLogTarget.CreateByPath("../Logs/custom.log"),
				new UnityLogTarget()
			};
        }
    }
}