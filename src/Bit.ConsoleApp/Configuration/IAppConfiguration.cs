using System;
using System.Collections.Generic;
using System.Text;

namespace Bit.ConsoleApp.Configuration
{
    public interface IAppConfiguration
    {
        bool EnableTracing { get; }

        bool EnableDebugLog { get; }
    }
}
