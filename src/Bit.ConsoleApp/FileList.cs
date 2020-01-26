using Bit.ConsoleApp.Repository;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Bit.ConsoleApp
{
    public struct FileInfo
    {
        public string Name { get; }

        public bool IsDirectory { get; }

        public long Length { get; }

        public DateTime CreationTimeUtc { get; }

        public DateTime LastWriteTimeUtc { get; }

        public FileInfo(
            string name,
            bool isDirectory,
            long length,
            DateTime creationTimeUtc,
            DateTime lastWriteTimeUtc)
        {
            Name = name;
            IsDirectory = isDirectory;
            Length = length;
            CreationTimeUtc = creationTimeUtc;
            LastWriteTimeUtc = lastWriteTimeUtc;
        }

        public override string ToString()
            => Name;
    }

    public class FileList
    {
        private class FileListNode
        {
            public FileListNode Parent { get; }

            public FileInfo FileInfo { get; }

            public FileListNode Previous { get; private set; }

            public FileListNode Next { get; private set; }

            public FileListNode FirstChild { get; private set; }

            public FileListNode LastChild { get; private set; }

            public int SubfolderCount { get; private set; }

            public int FileCount { get; private set; }

            public FileListNode(FileListNode parent, FileInfo fileInfo)
            {
                Parent = parent;
                FileInfo = fileInfo;
            }

            public override string ToString()
                => FileInfo.ToString();

            public void AddChild(FileListNode node)
            {
                if (node.FileInfo.IsDirectory)
                    ++SubfolderCount;
                else
                    ++FileCount;

                if (FirstChild == null)
                {
                    FirstChild = node;
                    LastChild = node;
                    return;
                }

                LastChild.Next = node;
                node.Previous = LastChild;
                LastChild = node;
            }
        }

        private readonly FileListNode _root = new FileListNode(null, new FileInfo("root", true, 0, new DateTime(0), new DateTime(0)));

        private FileList()
        {
        }

        public bool TryGetFileInfo(string path, out FileInfo fileInfo)
        {
            var parts = Utilities.SplitPathIntoParts(path.AsMemory());
            var root = _root;

            foreach (var part in parts)
            {
                var child = root.FirstChild;
                for (; child != null; child = child.Next)
                {
                    if (part.Span.CompareTo(child.FileInfo.Name.AsSpan(), StringComparison.InvariantCultureIgnoreCase) != 0)
                        continue;

                    root = child;
                    break;
                }

                if (child == null)
                    break;
            }

            fileInfo = root?.FileInfo ?? new FileInfo();
            return root != null;
        }

        // static

        private static void RecursiveReadFolder(FileListNode parent, string path)
        {
            foreach (var folder in Directory.GetDirectories(path))
            {
                var node = new FileListNode(parent, new FileInfo(Path.GetFileName(folder), true, 0, new DateTime(0), new DateTime(0)));
                parent.AddChild(node);
                RecursiveReadFolder(node, folder);
            }

            foreach (var filename in Directory.GetFiles(path))
            {
                var fi = new System.IO.FileInfo(filename);

                parent.AddChild(new FileListNode(parent,
                    new FileInfo(
                        Path.GetFileName(filename),
                        false,
                        fi.Length,
                        fi.CreationTimeUtc,
                        fi.LastWriteTimeUtc)));
            }
        }

        public static FileList FromFolder(string path)
        {
            path = Path.GetFullPath(path);

            var fileList = new FileList();
            RecursiveReadFolder(fileList._root, path);
            return fileList;
        }

        public static FileList FromRepository(IBitRepository repository)
        {
            throw new NotImplementedException();
        }
    }
}
