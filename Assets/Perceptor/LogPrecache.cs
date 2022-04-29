using System;
using System.Collections.Generic;
using UnityEngine;

namespace Rhinox.Perceptor
{
    public class LogPrecache
    {
        private class LogEntryCache
        {
            public LogLevels Levels;
            public string Message;
            public UnityEngine.Object AssociatedObject;

            public LogEntryCache(LogLevels levels, string message, UnityEngine.Object associatedObject)
            {
                Levels = levels;
                Message = message;
                AssociatedObject = associatedObject;
            }
        }

        private Dictionary<Type, List<LogEntryCache>> _cache;
        private readonly int _entryLimitPerType;

        public const int ENTRY_LIMIT = 1000;

        public LogPrecache(int entryLimitPerType = ENTRY_LIMIT)
        {
            _entryLimitPerType = Math.Max(0, entryLimitPerType);
            _cache = new Dictionary<Type, List<LogEntryCache>>();
        }

        public bool CacheEntry<T>(LogLevels levels, string message, GameObject associatedObject = null)
            where T : ILogger
        {
            return CacheEntry(typeof(T), levels, message, associatedObject);
        }

        public bool CacheEntry(Type loggerType, LogLevels levels, string message, UnityEngine.Object associatedObject = null)
        {
            if (!_cache.ContainsKey(loggerType))
                _cache.Add(loggerType, new List<LogEntryCache>());
            var list = _cache[loggerType];
            if (list.Count >= _entryLimitPerType)
            {
                if (list.Count == _entryLimitPerType)
                {
                    list.Add(new LogEntryCache(LogLevels.Warn,
                        $"The cache for logger of type '{loggerType?.Name}' has overflowed.", null));
                    
                    _cache[loggerType] = list;
                }
                return false;
            }

            list.Add(new LogEntryCache(levels, message, associatedObject));
            _cache[loggerType] = list;
            return true;
        }


        public void FlushCache(IReadOnlyCollection<ILogger> loggers)
        {
            if (_cache == null || _cache.Count == 0)
                return;

            UnityLogTarget.Silence = true; // NOTE: UnityLogging is already handled while caching, this prevents logging to Unity twice
            try
            {
                foreach (var logger in loggers)
                {
                    if (logger == null)
                        continue;
                    var loggerType = logger.GetType();
                    if (_cache.ContainsKey(loggerType))
                    {
                        foreach (var entry in _cache[loggerType])
                            logger.Log(entry.Levels, entry.Message, entry.AssociatedObject);
                        _cache[loggerType].Clear();
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"Something went wrong during FlushCache: " + e.ToString());
            }

            UnityLogTarget.Silence = false;

            _cache.Clear();
        }

        public void Clear()
        {
            _cache.Clear();
        }
    }
}