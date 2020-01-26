using Bit.ConsoleApp.Commands.Handler;
using Bit.ConsoleApp.Repository;
using System;
using System.Collections.Generic;
using System.Text;

namespace Bit.ConsoleApp.Commands
{
    [Command("status")]
    public class Status : Command
    {
        private readonly IBitRepository _repository;

        public Status(IBitRepository repository)
        {
            _repository = repository;
        }

        public override int Execute(ReadOnlySpan<string> args)
        {
            foreach (var path in _repository.GetPendingChanges())
            {
                Console.WriteLine(path);
            }

            return 0;
        }
    }
}
