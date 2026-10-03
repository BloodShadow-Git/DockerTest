using System;
using System.IO;
using NetNotepad.Base;

namespace NetNotepad.Client.Source
{
    public static class FileSystem
    {
        private static string DirectoryPath =>
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData, Environment.SpecialFolderOption.Create), "NetNotepad");

        public static void Save(string path, object obj)
        {
            path = Path.Combine(DirectoryPath, path);
            FileInfo fi = new(path);
            if (!fi.Exists)
            {
                if (fi.Directory == null) { return; }
                if (!fi.Directory.Exists) { fi.Directory.Create(); }
                fi.Create().Close();
            }
            File.WriteAllBytes(path, SerializeModule.Serialize(obj));
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

        public static bool Exists(string path) => new FileInfo(Path.Combine(DirectoryPath, path)).Exists;
    }
}