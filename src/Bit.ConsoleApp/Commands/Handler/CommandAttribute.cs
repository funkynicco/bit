using System;
using System.Collections.Generic;
using System.Text;

namespace Bit.ConsoleApp.Commands.Handler
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class CommandAttribute : Attribute
    {
        public string Command { get; }

        public CommandAttribute(string command)
        {
            Command = command;
        }
    }
}
