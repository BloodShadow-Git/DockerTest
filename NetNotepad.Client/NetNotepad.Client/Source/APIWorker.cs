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

        public static Task<LoginResponse> Login(LoginRequest request) => Request<LoginResponse>(request, EndpointsContract.Login, HttpMethod.Post);
        public static Task<LoginResponse> Login(LoginRTRequest request) => Request<LoginResponse>(request, EndpointsContract.Login, HttpMethod.Post);
        public static Task<RegisterResponse> Register(RegisterRequest request) => Request<RegisterResponse>(request, EndpointsContract.Register, HttpMethod.Post);
        public static Task<Response> Remove(RemoveRequest request) => Request(request, EndpointsContract.Remove, HttpMethod.Post);
        public static Task<Response> Logout(LogoutRequest request) => Request(request, EndpointsContract.Logout, HttpMethod.Post);
        public static Task<Response> Logout(LogoutRTRequest request) => Request(request, EndpointsContract.Logout, HttpMethod.Post);
        public static Task<Response> UpdateUser(UpdatePassRequest request) => Request(request, EndpointsContract.User, HttpMethod.Post);
        public static Task<Response> UpdateUser(UpdateRTRequest request) => Request(request, EndpointsContract.User, HttpMethod.Post);
        public static Task<UserGetResponse> GetUser(UserGetRequest request) => Request<UserGetResponse>(request, EndpointsContract.User, HttpMethod.Get);

        private static Task<Response> Request(object request, string path, HttpMethod method) => Request<Response>(request, path, method);
        private static async Task<T> Request<T>(object request, string path, HttpMethod method) where T : Response
        {
            if (string.IsNullOrEmpty(path) || request == null || method == null) { throw new Exception("Invalid request"); }
            HttpRequestMessage requestMessage = new(method, path) { Content = new ByteArrayContent(await SerializeModule.SerializeAsync(request)) };
            using HttpResponseMessage responseMessage = _client.Send(requestMessage);
            byte[] data = await responseMessage.Content.ReadAsByteArrayAsync();
            if (data.Length <= 0) { throw new Exception("Invalid response"); }
            T? result = SerializeModule.Deserialize<T>(data);
            if (result == null) { throw new Exception("Invalid response"); }
            return result;
        }
    }

    public class HttpException(string message) : Exception(message) { }
}