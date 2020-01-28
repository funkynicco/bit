using Bit.ConsoleApp.Logger;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Bit.ConsoleApp.IO
{
    public class StorageDevice : IStorageDevice
    {
        private readonly ILogger _logger;
        private readonly Encoding _encoding = Encoding.UTF8;

        public StorageDevice(ILogger logger)
        {
            _logger = logger;
        }

        private string GetPathRooted(string path)
            => Path.IsPathRooted(path) ? path : Path.GetFullPath(path);

        public void CreateFolder(string path, bool hidden = false)
        {
            path = GetPathRooted(path);

            var directoryInfo = Directory.CreateDirectory(path);
            _logger.Trace($"Created directory: {path}{(hidden ? " (hidden)" : "")}");

            if (hidden)
                directoryInfo.Attributes |= FileAttributes.Hidden;
        }

        public void DeleteFolder(string path)
        {
            path = GetPathRooted(path);

            Directory.Delete(path, true);
            _logger.Trace($"Deleted folder: {path}");
        }

        public byte[] ReadAllBytes(string path)
            => File.ReadAllBytes(path);

        public void WriteAllBytes(string path, byte[] bytes)
        {
            path = GetPathRooted(path);

            var existed = File.Exists(path);

            File.WriteAllBytes(path, bytes);

            if (!existed)
                _logger.Trace($"Created file: {path}");
        }

        public string ReadAllText(string path)
            => _encoding.GetString(ReadAllBytes(path));

        public void WriteAllText(string path, string text)
            => WriteAllBytes(path, _encoding.GetBytes(text));

        public Stream Open(string path, FileMode fileMode)
        {
            var stream = File.Open(path, fileMode, FileAccess.ReadWrite, FileShare.Read);
            switch (fileMode)
            {
                case FileMode.Create:
                case FileMode.CreateNew:
                    _logger.Trace($"Created file: {path}");
                    break;
            }

            return stream;
        }
    }
}
