using System.Collections;
using System.Collections.Generic;
using Rhinox.Perceptor;
using UnityEngine;

public class PerceptorLogger : CustomLogger
{
    public const string FILE_NAME = "perceptor.log";

    protected override ILogTarget[] GetTargets()
    {
        var list = new List<ILogTarget>();
        list.Add(new UnityLogTarget());
        list.Add(CreateFileTarget());
        return list.ToArray();
    }

    public static ILogTarget CreateFileTarget()
    {
        var logTarget = FileLogTarget.CreateByPath(FILE_NAME);
        var logBuilder = new LogTimeBuilder("[{0:HH:mm:ss}]")
            .Append(new LogLevelBuilder(), " ")
            .Append(PLog.DefaultBuilder, "> ");

        logTarget.SetCustomMessageBuilder(logBuilder);
        return logTarget;
    }
}
