using System;

namespace LighthouseDesktop.Core.Infrastructure.Logging
{
    public interface IConsoleLogger : ILogger
    {
    }

    public class ConsoleLogger : ILogger, IConsoleLogger
    {
        public void Log(string message)
        {
            Console.WriteLine(message);
        }
    }
}