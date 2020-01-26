using System;
using System.Collections.Generic;
using System.Text;

namespace Bit.ConsoleApp.Configuration
{
    public class AppConfigurationImpl : IAppConfiguration
    {
        public bool EnableTracing { get; set; }

        public bool EnableDebugLog { get; set; }
    }
}
