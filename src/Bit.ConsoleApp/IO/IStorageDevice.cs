using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Bit.ConsoleApp.IO
{
    public interface IStorageDevice
    {
        void CreateFolder(string path, bool hidden = false);

        void DeleteFolder(string path);

        byte[] ReadAllBytes(string path);

        void WriteAllBytes(string path, byte[] bytes);

        string ReadAllText(string path);

        void WriteAllText(string path, string text);

        Stream Open(string path, FileMode fileMode);
    }
}
