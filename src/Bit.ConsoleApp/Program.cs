using Bit.ConsoleApp.Commands;
using Bit.ConsoleApp.Commands.Handler;
using Bit.ConsoleApp.Configuration;
using Bit.ConsoleApp.IO;
using Bit.ConsoleApp.Logger;
using Bit.ConsoleApp.Repository;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.IO;

namespace Bit.ConsoleApp
{
    class Program
    {
        static void ConfigureServices(IServiceCollection services)
        {
            services.AddTransient<ILogger, LoggerImpl>();
            services.AddTransient<IStorageDevice, StorageDevice>();
            services.AddTransient<IBitRepository, BitRepository>();
            CommandHandler.Configure(services);
        }

        static int Main(string[] args_)
        {
            var args = new ReadOnlySpan<string>(args_);
            var config = new AppConfigurationImpl();

#if DEBUG
            config.EnableDebugLog = true;
#endif

            var serviceCollection = new ServiceCollection();
            serviceCollection.AddSingleton<IAppConfiguration>(config);
            ConfigureServices(serviceCollection);

            var serviceProvider = serviceCollection.BuildServiceProvider();

            string command = null;
            var command_args_index = 0;

            for (int i = 0; i < args.Length; i++)
            {
                if (args[i] == "--version")
                {
                    serviceProvider.GetService<ILogger>()
                        .Error("To be implemented");

                    return 0;
                }

                if (args[i] == "--trace")
                {
                    config.EnableTracing = true;
                    continue;
                }

                command = args[i];
                command_args_index = i + 1;
                break;
            }

            if (string.IsNullOrWhiteSpace(command))
            {
                ShowUsage(serviceProvider);
                return 1;
            }

            var remaining_args = args.Slice(command_args_index, args.Length - command_args_index);

            return CommandHandler.Invoke(serviceProvider, command, remaining_args);
        }

        static void ShowUsage(IServiceProvider serviceProvider)
        {
        }
    }
}
