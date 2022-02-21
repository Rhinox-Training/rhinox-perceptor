namespace Rhinox.Perceptor
{
    public abstract class CustomCopyLogger<TSelf, TSource> : CustomLogger 
        where TSelf : CustomCopyLogger<TSelf, TSource> 
        where TSource : CustomLogger
    {
        public override void SetupTargets()
        {
            base.SetupTargets();

            var customLogger = PLog.GetLogger<TSource>() as CustomLogger;
            if (customLogger == null)
                return;
            customLogger.AppendTarget(PipeTarget<TSelf>.Create());

        }
    }
}