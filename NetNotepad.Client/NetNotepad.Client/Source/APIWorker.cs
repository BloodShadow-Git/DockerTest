using System;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Avalonia;
using NetNotepad.Base;
using NetNotepad.Client.ViewModels;
using NetNotepad.Contracts;

namespace NetNotepad.Client.Source
{
    public class APIWorker
    {
        public static Uri? BaseUrl
        {
            get => _client?.BaseAddress;
            set
            {
                if (value != null)
                {
                    _client?.Dispose();
                    _client = new() { BaseAddress = value };
                }
            }
        }
        private static HttpClient? _client;
        public static readonly string CLIENT = $"{Application.Current!.Name} ({RuntimeInformation.OSDescription})";

        public static async Task<bool> ServerAvailable(string? baseUri)
        {
            if (Uri.TryCreate(baseUri, UriKind.RelativeOrAbsolute, out Uri? result)) { return await ServerAvailable(result); }
            else { return false; }
        }
        public static async Task<bool> ServerAvailable(Uri? baseUri)
        {
            if (baseUri == null) { throw new Exception("Base URL is not set"); }
            HttpClient client = new() { BaseAddress = baseUri };
            HttpRequestMessage requestMessage = new(HttpMethod.Get, "http/health");
            HttpResponseMessage responseMessage = await client.SendAsync(requestMessage);
            return responseMessage.StatusCode == System.Net.HttpStatusCode.OK;
        }
        public static Task<LoginResponse> Login(LoginRequest request) => Request<LoginResponse>(request, EndpointsContract.Login, HttpMethod.Post);
        public static Task<LoginResponse> Login(LoginRTRequest request) => Request<LoginResponse>(request, EndpointsContract.Login, HttpMethod.Post);
        public static Task<RegisterResponse> Register(RegisterRequest request) => Request<RegisterResponse>(request, EndpointsContract.Register, HttpMethod.Post);
        public static Task<Response> Remove(RemoveRequest request) => Request(request, EndpointsContract.Remove, HttpMethod.Post);
        public static Task<Response> Logout(LogoutRequest request) => Request(request, EndpointsContract.Logout, HttpMethod.Post);
        public static Task<Response> Logout(LogoutRTRequest request) => Request(request, EndpointsContract.Logout, HttpMethod.Post);
        public static Task<Response> UpdateUser(UpdatePassRequest request) => Request(request, EndpointsContract.User, HttpMethod.Post);
        public static Task<Response> UpdateUser(UpdateRTRequest request) => Request(request, EndpointsContract.User, HttpMethod.Post);
        public static Task<UserGetResponse> GetUser(UserGetRequest request) => Request<UserGetResponse>(request, EndpointsContract.User, HttpMethod.Get);
        public static Task<Response> UserExists(UserExistsRequest request) => Request(request, EndpointsContract.User, HttpMethod.Get);

        private static Task<Response> Request(object request, string path, HttpMethod method) => Request<Response>(request, path, method);
        private static async Task<T> Request<T>(object request, string path, HttpMethod method) where T : Response
        {
            if (_client == null) { throw new Exception("Base URL is not set"); }
            if (string.IsNullOrEmpty(path) || request == null || method == null) { throw new Exception("Invalid request"); }
            HttpRequestMessage requestMessage = new(method, path) { Content = new ByteArrayContent(await SerializeModule.SerializeAsync(request)) };
            HttpResponseMessage responseMessage = await _client.SendAsync(requestMessage);
            byte[] data = await responseMessage.Content.ReadAsByteArrayAsync();
            if (data.Length <= 0) { throw new Exception("Invalid response"); }
            T result = SerializeModule.Deserialize<T>(data) ?? throw new Exception("Invalid response");
            return result;
        }
    }
}