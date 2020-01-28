using Bit.ConsoleApp.IO;
using Bit.ConsoleApp.Logger;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

/*
 * First commit:  Store all committed files in data files and added to an index
 * Second commit: (future) Store changes only in data files and index it // For now, generate a full copy if the file on disk into data files
 */

namespace Bit.ConsoleApp.Repository
{
    public class BitRepository : IBitRepository
    {
        private readonly ILogger _logger;
        private readonly IStorageDevice _storageDevice;

        private readonly string _repositoryPath = null;

        public BitRepository(
            ILogger logger,
            IStorageDevice storageDevice)
        {
            _logger = logger;
            _storageDevice = storageDevice;

            if (Utilities.TryFindFolder(Constants.BitFolderName, out string bit_path)) // the full path to .bit folder
                _repositoryPath = Path.GetDirectoryName(bit_path);
        }

        public string GetCurrentCommitHash()
            => _storageDevice.ReadAllText(Path.Combine(_repositoryPath, "current_commit_hash"));

        public IEnumerable<string> GetPendingChanges()
        {
            var physical_list = FileList.FromFolder(_repositoryPath);
            var index_list = FileList.FromRepositoryIndex(_storageDevice, Path.Combine(_repositoryPath, "index"));

            var differences = physical_list.CompareTo(index_list);
            


            throw new NotImplementedException();
        }

        // stage

        // unstage/reset

        // commit

        // revert - uncommitted changes

        // push

        // pull
    }
}
