using Bit.ConsoleApp.Commands.Handler;
using Bit.ConsoleApp.IO;
using Bit.ConsoleApp.Logger;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;

namespace Bit.ConsoleApp.Commands
{
    [Command("init")]
    public class Init : Command
    {
        private readonly ILogger _logger;
        private readonly IStorageDevice _storageDevice;

        public Init(
            ILogger logger,
            IStorageDevice storageDevice)
        {
            _logger = logger;
            _storageDevice = storageDevice;
        }

        public override int Execute(ReadOnlySpan<string> args)
        {
            var force = false;
            for (int i = 0; i < args.Length; i++)
            {
                if (args[i] == "--force")
                {
                    force = true;
                }
            }

            if (Utilities.TryFindFolder(Constants.BitFolderName, out string path))
            {
                if (!force)
                {
                    _logger.Error("Cannot initialize inside of an existing repository.");
                    _logger.Normal($"Previous bit folder: {path}");
                    return 1;
                }

                var expected_path = Path.GetFullPath(Constants.BitFolderName);
                if (string.Compare(path, expected_path, true, CultureInfo.InvariantCulture) != 0)
                {
                    _logger.Error("Cannot forcefully overwrite a repository that is higher up in the directory hierarchy.");
                    _logger.Normal($"Previous bit folder: {path}");
                    return 1;
                }

                _logger.Trace("Previous .bit repository found, overwriting due to --force flag");

                _storageDevice.DeleteFolder(path);
            }

            path = Path.GetFullPath(Constants.BitFolderName);
            _logger.Trace($"Initializing {path}");

            _storageDevice.CreateFolder(path, hidden: true);

            CreateBitStructure(path);

            _logger.Success($"Initialized {path}");
            return 0;
        }

        private void CreateBitStructure(string root_path)
        {
            _storageDevice.WriteAllBytes(Path.Combine(root_path, "config.xml"), Media.default_config);
            _storageDevice.CreateFolder(Path.Combine(root_path, "data"));
        }
    }
}
