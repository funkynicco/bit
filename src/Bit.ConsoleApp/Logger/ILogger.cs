using System;
using System.Collections.Generic;
using System.Text;

namespace Bit.ConsoleApp.Logger
{
    public interface ILogger
    {
        void Critical(string message);
        
        void Error(string message);
        
        void Debug(string message);
        
        void Success(string message);
        
        void Trace(string message);
        
        void Normal(string message);
    }
}
