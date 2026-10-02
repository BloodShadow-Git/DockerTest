using System.Net;
using System.Net.Http;
using NetNotepad.Base;
using NetNotepad.Client.Source;
using NetNotepad.Contracts;

namespace NetNotepad.Client.ViewModels
{
    public partial class MainViewModel : ViewModelBase
    {
        public MainViewModel()
        {
            HttpClient hp = new() { BaseAddress = new("http://localhost:8080") };
            string file = "test.txt";
            if (!FileSystem.Exists(file))
            {
                bool result = ClearLogin(hp, file).Item1;
                if (!result) { return; }
            }
            else
            {
                string refreshToken = FileSystem.Load<string>(file) ?? "";
                LoginResponce? lr;
                if (string.IsNullOrEmpty(refreshToken))
                {
                    (bool result, lr) = ClearLogin(hp, file);
                    if (result) { return; }
                }
                HttpResponseMessage hrm = hp.PostAsync("/auth/login", new ByteArrayContent(SerializeModule.Serialize(new LoginRTRequest(refreshToken, "Client RT)")))).Result;
                if (hrm.StatusCode != HttpStatusCode.OK) { return; }
                lr = SerializeModule.Deserialize<LoginResponce>(hrm.Content.ReadAsByteArrayAsync().Result);
                if (lr == null) { return; }
                FileSystem.Save(file, lr.RefreshToken);
            }
        }

        private static (bool, LoginResponce?) ClearLogin(HttpClient hp, string file)
        {
            // HttpResponseMessage hrm = hp.PostAsync("/auth/login", new ByteArrayContent(SerializeModule.Serialize(new LoginRequest("bloodshadow", "password", false, "Client")))).Result;
            HttpResponseMessage hrm = hp.PostAsync("/auth/register", new ByteArrayContent(SerializeModule.Serialize(new RegisterRequest("bloodshadow", "password", false, "Client")))).Result;
            if (hrm.StatusCode != HttpStatusCode.OK) { return (false, null); }
            LoginResponce lr = SerializeModule.Deserialize<LoginResponce>(hrm.Content.ReadAsByteArrayAsync().Result);
            if (lr == null) { return (false, null); }
            FileSystem.Save(file, lr.RefreshToken);
            return (true, lr);
        }
    }
}