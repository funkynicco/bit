using System;
using System.Collections.Generic;
using System.Text;

namespace Bit.ConsoleApp.Repository
{
    public interface IBitRepository
    {
        string GetCurrentCommitHash();

        IEnumerable<string> GetPendingChanges();
    }
}
