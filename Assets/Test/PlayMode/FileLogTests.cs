using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using Rhinox.Perceptor;
using Unity.EditorCoroutines.Editor;
using UnityEngine;
using UnityEngine.TestTools;


public class FileLogTests
{
    private Dictionary<Task, CancellationTokenSource> _taskRegistry;

    private class LogCache
    {
        private int _registeredCount;
        private int _success;
        private int _failure;
        private static readonly object _lock = new object();
        private Exception _exception;
        public string Exception => _exception?.ToString() ?? "null";

        public LogCache(int regCount)
        {
            _registeredCount = regCount;
        }

        public bool IsDone => _success + _failure >= _registeredCount;

        public bool HasFailed => IsDone && _failure > 0;

        public void Success()
        {
            lock (_lock)
            {
                ++_success;
            }
        }

        public void Failure(Exception exception)
        {
            lock (_lock)
            {
                ++_failure;
                Debug.LogError(exception.ToString());
                _exception = exception;
            }
        }
    }
    
    [UnityTest]
    public IEnumerator LogTargetTest()
    {
        var path = Path.Combine(Application.dataPath, "..", "Library/unittest-run.log");

        var v = FileLogTarget.CreateByPath(path);

        const int count = 200;

        var cache = new LogCache(count);
        
        for (int i = 0; i < count; ++i)
        {
            EditorCoroutineUtility.StartCoroutineOwnerless(RunLogs(v, cache));
        }

        int iteration = 0;
        while (!cache.IsDone)
        {
            if (iteration++ % 10 == 0)
                Debug.Log($"Waiting {iteration}");
            yield return new WaitForEndOfFrame();
        }

        if (_taskRegistry != null)
        {
            foreach (var task in _taskRegistry.Keys)
            {
                var cancellationToken = _taskRegistry[task];
                if (!task.IsCompleted)
                    cancellationToken.Cancel();
            }
        }
        _taskRegistry.Clear();

        File.Delete(path);

        if (cache.HasFailed)
            Assert.Fail(cache.Exception);
        else
            Assert.Pass();

    }

    private IEnumerator RunLogs(ILogTarget target, LogCache cache)
    {
        if (_taskRegistry == null)
            _taskRegistry = new Dictionary<Task, CancellationTokenSource>();
        
        for (int i = 0; i < 200; ++i)
        {
            
            var tokenSource2 = new CancellationTokenSource();
            Action task = () =>
            {
                try
                {
                    target.Log(LogLevels.Info, "Test " + i);
                    cache.Success();
                }
                catch (ThreadAbortException) // Cancelled
                {
                }
                catch(Exception e)
                {
                    cache.Failure(e);
                }
            };
            
            
            _taskRegistry.Add(Task.Run(task, tokenSource2.Token), tokenSource2);
            yield return null;
        }

        yield return null;
    }
}
