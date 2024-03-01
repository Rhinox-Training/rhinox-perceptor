using System.Collections;
using System.Collections.Generic;
using Rhinox.Perceptor;
using UnityEngine;

public class LogTests : MonoBehaviour
{
    void Start()
    {
        PLog.TraceDetailed<PerceptorLogger>("TestTrace");
        PLog.Trace<PerceptorLogger>("TestTrace");
        PLog.Debug<PerceptorLogger>("TestDebug");
        PLog.Info<PerceptorLogger>("TestInfo");
        PLog.Warn<PerceptorLogger>("TestWarn");
        PLog.Error<PerceptorLogger>("TestError");
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
