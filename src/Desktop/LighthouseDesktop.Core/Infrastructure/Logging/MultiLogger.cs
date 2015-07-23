using System;
using System.Collections.Generic;

namespace LighthouseDesktop.Core.Infrastructure.Logging
{
    public interface IMultiLogger : ILogger
    {
        void AddLogger(ILogger logger);
        void RemoveLogger(ILogger logger);
    }

    public class MultiLogger : IMultiLogger
    {
        private readonly IList<ILogger> _loggers = new List<ILogger>();

        public MultiLogger() : this(new ConsoleLogger())
        {
        }

        public MultiLogger(IConsoleLogger consoleLogger)
        {
            AddLogger(consoleLogger);
        }

        public void AddLogger(ILogger logger)
        {
            if (!_loggers.Contains(logger))
            {
                _loggers.Add(logger);
            }
        }

        public void RemoveLogger(ILogger logger)
        {
            _loggers.Remove(logger);
        }

        public void Log(string message)
        {
            foreach (var logger in _loggers)
            {
                if (logger != null)
                {
                    logger.Log(message);
                }
            }
        }
    }
}