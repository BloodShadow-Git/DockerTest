using System;
using System.Collections.Generic;
using System.Runtime.Versioning;
using System.Threading.Tasks;
using NetNotepad.Base;
using NetNotepad.Client.Source;
using Tmds.DBus;

namespace NetNotepad.Client.Desktop
{
    [SupportedOSPlatform("Linux")]
    public class LinuxTokenStore : TokenStore
    {
        private const string Bus = "org.freedesktop.secrets";
        private const string AppId = "NetNotepad";
        private static readonly ObjectPath ServicePath = new("/org/freedesktop/secrets");
        private static readonly ObjectPath DefaultCollection =
            new("/org/freedesktop/secrets/aliases/default");

        private static Dictionary<string, string> Attrs(string key) => new() { ["app"] = AppId, ["key"] = key };

        private static (ObjectPath session, ISecretCollection col) Open()
        {
            Connection conn = Connection.Session;
            ISecretService svc = conn.CreateProxy<ISecretService>(Bus, ServicePath);

            (_, ObjectPath session) = svc.OpenSessionAsync("plain", "").Result;
            (_, ObjectPath prompt) = svc.UnlockAsync([DefaultCollection]).Result;
            RunPrompt(prompt);

            return (session, conn.CreateProxy<ISecretCollection>(Bus, DefaultCollection));
        }

        private static async void RunPrompt(ObjectPath prompt)
        {
            if (prompt.ToString() == "/") return;
            ISecretPrompt p = Connection.Session.CreateProxy<ISecretPrompt>(Bus, prompt);
            TaskCompletionSource<bool> tcs = new();
            using IDisposable? _ = await p.WatchCompletedAsync(r => tcs.TrySetResult(r.dismissed));
            await p.PromptAsync("");
            if (await tcs.Task) { throw new OperationCanceledException("Пользователь отклонил запрос к хранилищу секретов"); }
        }

        protected override async void SaveInternal(string path, object data)
        {
            (ObjectPath session, ISecretCollection col) = Open();

            Dictionary<string, object> props = new()
            {
                ["org.freedesktop.Secret.Item.Label"] = $"{AppId}: {path}",
                ["org.freedesktop.Secret.Item.Attributes"] = Attrs(path)
            };
            DBusSecret secret = new()
            {
                Session = session,
                Parameters = [],
                Value = SerializeModule.Serialize(data),
                ContentType = "text/plain"
            };
            (_, ObjectPath prompt) = await col.CreateItemAsync(props, secret, replace: true);
            RunPrompt(prompt);
        }
        protected override T LoadInternal<T>(string path)
        {
            (ObjectPath session, ISecretCollection col) = Open();
            ObjectPath[] found = col.SearchItemsAsync(Attrs(path)).Result;
            if (found.Length == 0) return default!;

            ISecretItem item = Connection.Session.CreateProxy<ISecretItem>(Bus, found[0]);
            DBusSecret secret = item.GetSecretAsync(session).Result;
            return SerializeModule.Deserialize<T>(secret.Value);
        }
        protected override async void DeleteInternal(string path)
        {
            (_, ISecretCollection col) = Open();
            foreach (var searchPath in await col.SearchItemsAsync(Attrs(path)))
            {
                var item = Connection.Session.CreateProxy<ISecretItem>(Bus, searchPath);
                RunPrompt(await item.DeleteAsync());
            }
        }
        protected override bool ExistsInternal(string path)
        {
            (_, ISecretCollection col) = Open();
            return col.SearchItemsAsync(Attrs(path)).Result.Length != 0;
        }
    }

    [DBusInterface("org.freedesktop.Secret.Service")]
    public interface ISecretService : IDBusObject
    {
        Task<(object output, ObjectPath session)> OpenSessionAsync(string algorithm, object input);
        Task<(ObjectPath[] unlocked, ObjectPath prompt)> UnlockAsync(ObjectPath[] objects);
    }

    [DBusInterface("org.freedesktop.Secret.Collection")]
    public interface ISecretCollection : IDBusObject
    {
        Task<ObjectPath[]> SearchItemsAsync(IDictionary<string, string> attributes);
        Task<(ObjectPath item, ObjectPath prompt)> CreateItemAsync(IDictionary<string, object> properties, DBusSecret secret, bool replace);
    }

    [DBusInterface("org.freedesktop.Secret.Item")]
    public interface ISecretItem : IDBusObject
    {
        Task<ObjectPath> DeleteAsync();
        Task<DBusSecret> GetSecretAsync(ObjectPath session);
    }

    [DBusInterface("org.freedesktop.Secret.Prompt")]
    public interface ISecretPrompt : IDBusObject
    {
        Task PromptAsync(string windowId);
        Task<IDisposable> WatchCompletedAsync(Action<(bool dismissed, object result)> handler);
    }

    public struct DBusSecret
    {
        public ObjectPath Session;
        public byte[] Parameters;
        public byte[] Value;
        public string ContentType;
    }
}