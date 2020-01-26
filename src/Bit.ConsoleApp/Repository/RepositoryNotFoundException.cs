using System;
using System.Collections.Generic;
using System.Text;

namespace Bit.ConsoleApp.Repository
{
    public class RepositoryNotFoundException : Exception
    {
        public RepositoryNotFoundException() :
            base("The repository was not found.")
        {
        }
    }
}
