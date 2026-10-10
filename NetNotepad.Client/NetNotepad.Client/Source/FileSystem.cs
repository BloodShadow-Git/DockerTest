using System;
using System.Collections.Generic;
using System.IO;
using NetNotepad.Base;

namespace NetNotepad.Client.Source
{
    public static class FileSystem
    {
        private static string DirectoryPath =>
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData, Environment.SpecialFolderOption.Create), "NetNotepad");

        public static void Save(string path, object obj) => Save(path, SerializeModule.Serialize(obj));
        public static void Save(string path, byte[] data)
        {
            path = Path.Combine(DirectoryPath, path);
            FileInfo fi = new(path);
            if (!fi.Exists)
            {
                if (fi.Directory == null) { return; }
                if (!fi.Directory.Exists) { fi.Directory.Create(); }
                fi.Create().Close();
            }
            File.WriteAllBytes(path, data);
        }

        public static T? Load<T>(string path)
        {
            path = Path.Combine(DirectoryPath, path);
            FileInfo fi = new(path);
            if (!fi.Exists)
            {
                if (fi.Directory == null) { return default; }
                if (!fi.Directory.Exists) { fi.Directory.Create(); }
                fi.Create().Close();
                return default;
            }
            return SerializeModule.Deserialize<T>(File.ReadAllBytes(path));
        }

        public static byte[] Load(string path)
        {
            path = Path.Combine(DirectoryPath, path);
            FileInfo fi = new(path);
            if (!fi.Exists)
            {
                if (fi.Directory == null) { return []; }
                if (!fi.Directory.Exists) { fi.Directory.Create(); }
                fi.Create().Close();
                return [];
            }
            return File.ReadAllBytes(path);
        }

        public static T[] LoadAll<T>(string path, string pattern = null!)
        {
            path = Path.Combine(DirectoryPath, path);
            DirectoryInfo di = new(path);
            if (!di.Exists)
            {
                di.Create();
                return [];
            }
            List<T> result = [];
            foreach (FileInfo fi in pattern == null ? di.EnumerateFiles() : di.EnumerateFiles(pattern, SearchOption.TopDirectoryOnly))
            {
                try
                {
                    T? data = SerializeModule.Deserialize<T>(File.ReadAllBytes(fi.FullName));
                    if (data != null) { result.Add(data); }
                }
                catch { }
            }
            return [.. result];
        }

        public static byte[][] LoadAll(string path, string pattern = null!)
        {
            path = Path.Combine(DirectoryPath, path);
            DirectoryInfo di = new(path);
            if (!di.Exists)
            {
                di.Create();
                return [];
            }
            List<byte[]> result = [];
            foreach (FileInfo fi in pattern == null ? di.EnumerateFiles() : di.EnumerateFiles(pattern, SearchOption.TopDirectoryOnly))
            {
                try
                {
                    byte[]? data = File.ReadAllBytes(fi.FullName);
                    if (data != null) { result.Add(data); }
                }
                catch { }
            }
            return [.. result];
        }

        public static bool Exists(string path) => new FileInfo(Path.Combine(DirectoryPath, path)).Exists;
    }
}