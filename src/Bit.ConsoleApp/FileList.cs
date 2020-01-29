using Bit.ConsoleApp.IO;
using Bit.ConsoleApp.Repository;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace Bit.ConsoleApp
{
    [Flags]
    public enum FileListDifferences
    {
        None,
        LeftMissing = 1,
        RightMissing = 2,
        IsDirectoryMismatch = 4,
        LengthMismatch = 8,
        CreationTimeMismatch = 16,
        LastWriteTimeMismatch = 32
    }

    public struct FileListDifference
    {
        public IFileInfo Left { get; set; }

        public IFileInfo Right { get; set; }

        public FileListDifferences Difference { get; set; }

        public FileListDifference(IFileInfo left, IFileInfo right, FileListDifferences difference)
        {
            Left = left;
            Right = right;
            Difference = difference;
        }
    }

    public interface IFileInfo
    {
        string Name { get; }

        bool IsDirectory { get; }

        long Length { get; }

        DateTime CreationTimeUtc { get; }

        DateTime LastWriteTimeUtc { get; }

        string FullPath { get; }
    }

    public class FileList
    {
        private class FileListNode : IFileInfo
        {
            private string _fullPath;

            public Guid Id { get; }

            public FileListNode Parent { get; }

            public FileListNode Previous { get; private set; }

            public FileListNode Next { get; private set; }

            public FileListNode FirstChild { get; private set; }

            public FileListNode LastChild { get; private set; }

            public int SubfolderCount { get; private set; }

            public int FileCount { get; private set; }

            public string Name { get; }

            public bool IsDirectory { get; }

            public long Length { get; }

            public DateTime CreationTimeUtc { get; }

            public DateTime LastWriteTimeUtc { get; }

            public string FullPath
            {
                get
                {
                    if (_fullPath == null)
                        _fullPath = CalculateFullPath();

                    return _fullPath;
                }
            }

            public FileListNode(
                FileListNode parent,
                string name,
                bool isDirectory,
                long length,
                DateTime creationTimeUtc,
                DateTime lastWriteTimeUtc)
            {
                Id = Guid.NewGuid();
                Parent = parent;
                Name = name;
                IsDirectory = isDirectory;
                Length = length;
                CreationTimeUtc = creationTimeUtc;
                LastWriteTimeUtc = lastWriteTimeUtc;
            }

            public override string ToString()
                => Name;

            public void AddChild(FileListNode node)
            {
                if (node.IsDirectory)
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

            public FileListNode FindChild(string name)
            {
                for (var node = FirstChild; node != null; node = node.Next)
                {
                    if (string.Compare(node.Name, name, false, CultureInfo.InvariantCulture) == 0)
                        return node;
                }

                return null;
            }

            private string CalculateFullPath()
            {
                var reversed_parts = new List<ReadOnlyMemory<char>>();
                for (var node = this; node != null; node = node.Parent)
                {
                    if (node.Parent == null) // dont include the root node
                        break;

                    reversed_parts.Add(node.Name.AsMemory());
                }

                var sb = new StringBuilder(256);
                for (int i = reversed_parts.Count - 1; i >= 0; i--)
                {
                    if (sb.Length != 0)
                        sb.Append('/');

                    sb.Append(reversed_parts[i]);
                }

                return sb.ToString();
            }
        }

        private readonly FileListNode _root = new FileListNode(null, "root", true, 0, new DateTime(0), new DateTime(0));

        private FileList()
        {
        }

        public bool TryGetFileInfo(string path, out IFileInfo fileInfo)
        {
            var parts = Utilities.SplitPathIntoParts(path.AsMemory());
            var root = _root;

            foreach (var part in parts)
            {
                var child = root.FirstChild;
                for (; child != null; child = child.Next)
                {
                    if (part.Span.CompareTo(child.Name.AsSpan(), StringComparison.InvariantCultureIgnoreCase) != 0)
                        continue;

                    root = child;
                    break;
                }

                if (child == null)
                    break;
            }

            fileInfo = root;
            return root != null;
        }

        private void RecursiveWriteStream(System.IO.BinaryWriter bw, FileListNode node)
        {
            var offset = bw.BaseStream.Position;
            bw.Write(0); // pad count to write to later
            var count = 0;
            for (var child = node.FirstChild; child != null; child = child.Next)
            {
                bw.Write(child.Name);
                bw.Write(child.IsDirectory);
                bw.Write(child.Length);
                bw.Write(child.CreationTimeUtc.Ticks);
                bw.Write(child.LastWriteTimeUtc.Ticks);
                if (child.IsDirectory)
                    RecursiveWriteStream(bw, child);

                ++count;
            }

            bw.Seek((int)offset, System.IO.SeekOrigin.Begin);
            bw.Write(count);
            bw.Seek(0, System.IO.SeekOrigin.End);
        }

        public void Save(IStorageDevice storageDevice, string path)
        {
            using (var stream = storageDevice.Open(path, System.IO.FileMode.Create))
            using (var bw = new System.IO.BinaryWriter(stream, Encoding.UTF8, true))
            {
                RecursiveWriteStream(bw, _root);
            }
        }

        private bool CompareDifferences(IFileInfo left, IFileInfo right, out FileListDifference difference)
        {
            var differences = FileListDifferences.None;

            if (left.IsDirectory != right.IsDirectory)
                differences |= FileListDifferences.IsDirectoryMismatch;

            if (left.Length != right.Length)
                differences |= FileListDifferences.LengthMismatch;

            if (left.CreationTimeUtc != right.CreationTimeUtc)
                differences |= FileListDifferences.CreationTimeMismatch;

            if (left.LastWriteTimeUtc != right.LastWriteTimeUtc)
                differences |= FileListDifferences.LastWriteTimeMismatch;

            difference = new FileListDifference(left, right, differences);
            return difference.Difference == FileListDifferences.None;
        }

        private void RecursiveCompareTo(List<FileListDifference> differences, FileListNode left, FileListNode right)
        {
            // check to make sure we're not comparing two objects twice
            var check = new HashSet<Guid>();

            for (var node = left.FirstChild; node != null; node = node.Next)
            {
                check.Clear();

                var find = right.FindChild(node.Name);
                if (find != null)
                {
                    check.Add(find.Id);
                    if (node.IsDirectory)
                        RecursiveCompareTo(differences, node, find);
                    else if (!CompareDifferences(node, find, out FileListDifference diff))
                        differences.Add(diff);
                }
                else
                {
                    differences.Add(new FileListDifference()
                    {
                        Difference = FileListDifferences.RightMissing,
                        Left = node
                    });
                    continue;
                }
            }

            for (var node = right.FirstChild; node != null; node = node.Next)
            {
                if (check.Contains(node.Id))
                    continue;

                var find = left.FindChild(node.Name);
                if (find != null)
                {
                    if (node.IsDirectory)
                        RecursiveCompareTo(differences, find, node);
                    else if (!CompareDifferences(find, node, out FileListDifference diff))
                        differences.Add(diff);
                }
                else
                {
                    differences.Add(new FileListDifference()
                    {
                        Difference = FileListDifferences.LeftMissing,
                        Right = node
                    });
                    continue;
                }
            }
        }

        public IEnumerable<FileListDifference> CompareTo(FileList other)
        {
            var differences = new List<FileListDifference>();
            RecursiveCompareTo(differences, _root, other._root);
            return differences;
        }

        // static

        private static void RecursiveReadFolder(FileListNode parent, string path)
        {
            foreach (var folder in System.IO.Directory.GetDirectories(path))
            {
                var node = new FileListNode(parent, System.IO.Path.GetFileName(folder), true, 0, new DateTime(0), new DateTime(0));
                parent.AddChild(node);
                RecursiveReadFolder(node, folder);
            }

            foreach (var filename in System.IO.Directory.GetFiles(path))
            {
                var fi = new System.IO.FileInfo(filename);

                parent.AddChild(new FileListNode(
                    parent,
                    System.IO.Path.GetFileName(filename),
                    false,
                    fi.Length,
                    fi.CreationTimeUtc,
                    fi.LastWriteTimeUtc));
            }
        }

        private static void RecursiveReadStream(System.IO.BinaryReader br, FileListNode parent)
        {
            var count = br.ReadInt32();
            for (int i = 0; i < count; i++)
            {
                var name = br.ReadString();
                var isDirectory = br.ReadBoolean();
                var length = br.ReadInt64();
                var creationTimeUtc = DateTime.SpecifyKind(new DateTime(br.ReadInt64()), DateTimeKind.Utc);
                var lastWriteTimeUtc = DateTime.SpecifyKind(new DateTime(br.ReadInt64()), DateTimeKind.Utc);

                var node = new FileListNode(parent, name, isDirectory, length, creationTimeUtc, lastWriteTimeUtc);
                parent.AddChild(node);

                if (node.IsDirectory)
                    RecursiveReadStream(br, node);
            }
        }

        public static FileList FromFolder(string path)
        {
            path = System.IO.Path.GetFullPath(path);

            var fileList = new FileList();
            RecursiveReadFolder(fileList._root, path);
            return fileList;
        }

        public static FileList FromRepositoryIndex(IStorageDevice storageDevice, string path)
        {
            path = System.IO.Path.GetFullPath(path);

            var fileList = new FileList();
            if (System.IO.File.Exists(path))
            {
                using (var stream = storageDevice.Open(path, System.IO.FileMode.Open))
                using (var br = new System.IO.BinaryReader(stream, Encoding.UTF8, true))
                {
                    RecursiveReadStream(br, fileList._root);
                }
            }

            return fileList;
        }
    }
}
