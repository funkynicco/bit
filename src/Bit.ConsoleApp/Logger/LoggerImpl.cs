using Bit.ConsoleApp.Configuration;
using System;
using System.Collections.Generic;
using System.Text;

namespace Bit.ConsoleApp.Logger
{
    public class LoggerImpl : ILogger
    {
        private readonly IAppConfiguration _configuration;

        public LoggerImpl(IAppConfiguration configuration)
        {
            _configuration = configuration;
        }

        private void Log(ConsoleColor color, string severity, string message)
        {
            Console.ForegroundColor = color;
            Console.Write($"[{severity}]".PadRight(11, ' '));
            Console.WriteLine(message);
            Console.ForegroundColor = ConsoleColor.Gray;
        }

        public void Critical(string message)
            => Log(ConsoleColor.Red, nameof(Critical), message);

        public void Error(string message)
            => Log(ConsoleColor.Red, nameof(Error), message);

        public void Debug(string message)
        {
            if (_configuration.EnableDebugLog)
                Log(ConsoleColor.Blue, nameof(Debug), message);
        }

        public void Success(string message)
            => Log(ConsoleColor.Green, nameof(Success), message);

        public void Trace(string message)
        {
            if (_configuration.EnableTracing)
                Log(ConsoleColor.Cyan, nameof(Trace), message);
        }

        public void Normal(string message)
            => Log(ConsoleColor.Gray, nameof(Normal), message);
    }
}
