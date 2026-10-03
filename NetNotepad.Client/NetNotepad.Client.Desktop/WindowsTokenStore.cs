using System;
using System.IO;
using System.Runtime.Versioning;
using System.Security.Cryptography;
using System.Text;
using NetNotepad.Base;
using NetNotepad.Client.Source;

namespace NetNotepad.Client.Desktop
{
    [SupportedOSPlatform("Windows")]
    public class WindowsTokenStore : TokenStore
    {
        private static readonly byte[] Entropy = SHA256.HashData(Encoding.UTF8.GetBytes("efefb0dc971e1e5ee9724820e4b8c400"));
        private static string DirPath => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "NetNotepad");

        protected override void SaveInternal(string path, object obj)
        {
            path = Path.Combine(DirPath, path);
            FileInfo fi = new(path);
            if (!fi.Exists)
            {
                if (fi.Directory == null) { return; }
                if (!fi.Directory.Exists) { fi.Directory.Create(); }
                fi.Create().Close();
            }
            byte[] data = ProtectedData.Protect(SerializeModule.Serialize(obj), Entropy, DataProtectionScope.CurrentUser);
            File.WriteAllBytes(path, data);
        }
        protected override T LoadInternal<T>(string path)
        {
            path = Path.Combine(DirPath, path);
            FileInfo fi = new(path);
            if (!fi.Exists)
            {
                if (fi.Directory == null) { return default!; }
                if (!fi.Directory.Exists) { fi.Directory.Create(); }
                fi.Create().Close();
            }
            return SerializeModule.Deserialize<T>(ProtectedData.Unprotect(File.ReadAllBytes(path), Entropy, DataProtectionScope.CurrentUser));
        }
        protected override void DeleteInternal(string path) => new FileInfo(Path.Combine(DirPath, path)).Delete();
        protected override bool ExistsInternal(string path) => new FileInfo(Path.Combine(DirPath, path)).Exists;
    }
}