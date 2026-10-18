using System;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Avalonia;
using NetNotepad.Base;
using NetNotepad.Contracts;

namespace NetNotepad.Client.Source
{
    public class APIWorker
    {
        public static Uri? BaseUrl
        {
            get => _client.BaseAddress;
            set { if (value != null) { _client.BaseAddress = value; } }
        }
        private static HttpClient _client = new();
        public static readonly string CLIENT = $"{Application.Current!.Name} ({RuntimeInformation.OSDescription})";

        public static Task<LoginResponce> Login(LoginRequest request) => Request<LoginResponce>(request, EndpointsContract.Login, HttpMethod.Post);
        public static Task<LoginResponce> Login(LoginRTRequest request) => Request<LoginResponce>(request, EndpointsContract.Login, HttpMethod.Post);
        public static Task<RegisterResponce> Register(RegisterRequest request) => Request<RegisterResponce>(request, EndpointsContract.Register, HttpMethod.Post);
        public static Task<Responce> Remove(RemoveRequest request) => Request(request, EndpointsContract.Remove, HttpMethod.Post);
        public static Task<Responce> Logout(LogoutRequest request) => Request(request, EndpointsContract.Logout, HttpMethod.Post);
        public static Task<Responce> Logout(LogoutRTRequest request) => Request(request, EndpointsContract.Logout, HttpMethod.Post);
        public static Task<Responce> UpdateUser(UpdatePassRequest request) => Request(request, EndpointsContract.User, HttpMethod.Post);
        public static Task<Responce> UpdateUser(UpdateRTRequest request) => Request(request, EndpointsContract.User, HttpMethod.Post);
        public static Task<UserGetResponce> GetUser(UserGetRequest request) => Request<UserGetResponce>(request, EndpointsContract.User, HttpMethod.Get);

        private static Task<Responce> Request(object request, string path, HttpMethod method) => Request<Responce>(request, path, method);
        private static async Task<T> Request<T>(object request, string path, HttpMethod method) where T : Responce
        {
            if (string.IsNullOrEmpty(path) || request == null || method == null) { throw new Exception("Invalid request"); }
            HttpRequestMessage requestMessage = new(method, path) { Content = new ByteArrayContent(await SerializeModule.SerializeAsync(request)) };
            using HttpResponseMessage responseMessage = _client.Send(requestMessage);
            byte[] data = await responseMessage.Content.ReadAsByteArrayAsync();
            if (data.Length <= 0) { throw new Exception("Invalid response"); }
            T? result = SerializeModule.Deserialize<T>(data);
            if (result is Responce sr)
            { throw new HttpException($"{sr.HttpCode}: {sr.Message}"); }
            if (result == null) { throw new Exception("Invalid response"); }
            return result;
        }
    }

    public class HttpException(string message) : Exception(message) { }
}