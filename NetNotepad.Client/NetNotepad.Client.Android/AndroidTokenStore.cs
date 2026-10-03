using System.Runtime.Versioning;
using System.Text;
using System.Text.Json;
using Android.Content;
using Xamarin.Google.Crypto.Tink;
using Xamarin.Google.Crypto.Tink.Aead;
using Xamarin.Google.Crypto.Tink.Integration.Android;
using NetNotepad.Client.Source;
using Android.Runtime;
using System;

namespace NetNotepad.Client.Android
{
    [SupportedOSPlatform("Android")]
    public class AndroidTokenStore : TokenStore
    {
        private const string KeysetName = "token_keyset";
        private const string KeysetPrefsFile = "token_keyset_prefs";
        private const string DataPrefsFile = "token_store";
        private const string MasterKeyUri = "android-keystore://token_master_key";

        private readonly ISharedPreferences _prefs;
        private readonly IAead _aead;

        public AndroidTokenStore()
        {
            var context = global::Android.App.Application.Context;

            AeadConfig.Register();

            var handle = new AndroidKeysetManager.Builder()
                .WithSharedPref(context, KeysetName, KeysetPrefsFile)!
                .WithKeyTemplate(AeadKeyTemplates.Aes256Gcm)!
                .WithMasterKeyUri(MasterKeyUri)!
                .Build()!
                .KeysetHandle!;

            _aead = handle.GetPrimitive(RegistryConfiguration.Get(), Java.Lang.Class.FromType(typeof(IAead)))!.JavaCast<IAead>()!;
            _prefs = context.GetSharedPreferences(DataPrefsFile, FileCreationMode.Private)!;
        }

        protected override void SaveInternal(string path, object data)
        {
            var plain = JsonSerializer.SerializeToUtf8Bytes(data);
            var cipher = _aead.Encrypt(plain, Encoding.UTF8.GetBytes(path))!;

            _prefs.Edit()!
                .PutString(path, Convert.ToBase64String(cipher))!
                .Apply();
        }

        protected override T LoadInternal<T>(string path)
        {
            string? b64 = _prefs.GetString(path, null);
            if (b64 == null) { return default!; }

            byte[] plain = _aead.Decrypt(Convert.FromBase64String(b64), Encoding.UTF8.GetBytes(path))!;
            return JsonSerializer.Deserialize<T>(plain)!;
        }

        protected override void DeleteInternal(string path) => _prefs.Edit()!.Remove(path)!.Apply();

        protected override bool ExistsInternal(string path) => _prefs.Contains(path);
    }
}