using System;
using System.Collections.Generic;
using System.Text;

namespace Bit.ConsoleApp.Commands.Handler
{
    public abstract class Command
    {
        public abstract int Execute(ReadOnlySpan<string> args);
    }
}
