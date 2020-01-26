using Bit.ConsoleApp.Logger;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Bit.ConsoleApp.Commands.Handler
{
    public static class CommandHandler
    {
        private static readonly Dictionary<string, Type> _handlers = new Dictionary<string, Type>();

        public static void Configure(IServiceCollection services)
        {
            foreach (var type in Assembly.GetExecutingAssembly().GetTypes())
            {
                if (type.IsClass &&
                    type.IsSubclassOf(typeof(Command)))
                {
                    foreach (var attribute in type.GetCustomAttributes<CommandAttribute>())
                    {
                        services.AddTransient(type);
                        _handlers.Add(attribute.Command.ToLowerInvariant(), type);
                    }
                }
            }
        }

        public static int Invoke(IServiceProvider serviceProvider, string command, ReadOnlySpan<string> args)
        {
            if (!_handlers.TryGetValue(command.ToLowerInvariant(), out Type type))
            {
                serviceProvider.GetService<ILogger>()
                    .Error("Invalid command");

                return 1;
            }

            return (serviceProvider.GetService(type) as Command).Execute(args);
        }
    }
}
