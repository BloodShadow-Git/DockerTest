using System.IO;
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
                bool result = ClearLogin(hp, file);
                if (!result) { return; }
            }
            else
            {
                string refreshToken = FileSystem.Load<string>(file) ?? "";
                if (string.IsNullOrEmpty(refreshToken))
                {
                    ClearLogin(hp, file);
                    return;
                }
                HttpResponseMessage hrm = hp.PostAsync("/auth/login", new ByteArrayContent(SerializeModule.Serialize(new LoginRTRequest(refreshToken, "Client RT)")))).Result;
                if (hrm.StatusCode != HttpStatusCode.OK)
                {
                    ClearLogin(hp, file);
                    return;
                }
                string test = new StreamReader(hrm.Content.ReadAsStream()).ReadToEnd();
                LoginResponce lr = SerializeModule.Deserialize<LoginResponce>(hrm.Content.ReadAsByteArrayAsync().Result);
                if (lr == null) { return; }
                FileSystem.Save(file, lr.RefreshToken);
            }
        }

        private static bool ClearLogin(HttpClient hp, string file)
        {
            HttpResponseMessage hrm = hp.PostAsync("/auth/login", new ByteArrayContent(SerializeModule.Serialize(new LoginRequest("bloodshadow", "password", false, "Client")))).Result;
            if (hrm.StatusCode != HttpStatusCode.OK) { return false; }
            LoginResponce lr = SerializeModule.Deserialize<LoginResponce>(hrm.Content.ReadAsByteArrayAsync().Result);
            if (lr == null) { return false; }
            FileSystem.Save(file, lr.RefreshToken);
            return true;
        }
    }
}