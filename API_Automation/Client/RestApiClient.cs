using Newtonsoft.Json;
using RestSharp;
using System;
using System.Threading.Tasks;

namespace API_Automation.Client
{
    public class RestApiClient : IApiClient
    {
        private readonly RestClient _client;
        private readonly TimeSpan _timeout = TimeSpan.FromSeconds(100);

        public RestApiClient(string baseUrl)
        {
            var options = new RestClientOptions(baseUrl)
            {
                ThrowOnAnyError = false,
                MaxTimeout = (int)_timeout.TotalMilliseconds
            };
            _client = new RestClient(options);
        }

        // GET
        public async Task<RestResponse> GetAsync(string endpoint, string token = null)
        {
            var request = new RestRequest(endpoint, Method.Get);

            if (!string.IsNullOrEmpty(token))
                request.AddHeader("Authorization", $"Bearer {token}");

            var response = await _client.ExecuteAsync(request);
            return response;
        }

        // POST
        public async Task<RestResponse> PostAsync<T>(string endpoint, T body, string token = null)
        {
            var request = new RestRequest(endpoint, Method.Post);

            if (!string.IsNullOrEmpty(token))
                request.AddHeader("Authorization", $"Bearer {token}");

            request.AddHeader("Content-Type", "application/json");
            request.AddBody(body, ContentType.Json);

            var response = await _client.ExecuteAsync(request);
            return response;
        }

        // PUT
        public async Task<RestResponse> PutAsync<T>(string endpoint, T body, string token = null)
        {
            var request = new RestRequest(endpoint, Method.Put);

            if (!string.IsNullOrEmpty(token))
                request.AddHeader("Authorization", $"Bearer {token}");

            request.AddHeader("Content-Type", "application/json");
            request.AddBody(body, ContentType.Json);

            var response = await _client.ExecuteAsync(request);
            return response;
        }

        // DELETE user or resource by endpoint
        public async Task<RestResponse> DeleteAsync(string endpoint, string token = null)
        {
            var request = new RestRequest(endpoint, Method.Delete);

            if (!string.IsNullOrEmpty(token))
                request.AddHeader("Authorization", $"Bearer {token}");

            var response = await _client.ExecuteAsync(request);

            Console.WriteLine($"DELETE {endpoint} → {response.StatusCode}, Body: {response.Content}");

            return response;
        }


        // DELETE book for a specific user
        public async Task<RestResponse> DeleteBookAsync(string userId, string isbn, string token)
        {
            var request = new RestRequest("/BookStore/v1/Book", Method.Delete);

            if (!string.IsNullOrEmpty(token))
                request.AddHeader("Authorization", $"Bearer {token}");

            request.AddHeader("Content-Type", "application/json");

            var body = new
            {
                userId = userId,
                isbn = isbn
            };

            request.AddBody(body, ContentType.Json);

            var response = await _client.ExecuteAsync(request);
            return response;
        }

        // Create user
        public async Task<RestResponse> CreateUser(string username, string password)
        {
            var body = new
            {
                userName = username,
                password = password
            };

            var request = new RestRequest("/Account/v1/User", Method.Post);
            request.AddHeader("Content-Type", "application/json");
            request.AddBody(body, ContentType.Json);

            var response = await _client.ExecuteAsync(request);
            return response;
        }

        // Generate token
        public async Task<string> GenerateToken(string username, string password)
        {
            var body = new
            {
                userName = username,
                password = password
            };

            var request = new RestRequest("/Account/v1/GenerateToken", Method.Post);
            request.AddHeader("Content-Type", "application/json");
            request.AddBody(body, ContentType.Json);

            var response = await _client.ExecuteAsync(request);

            if (response.IsSuccessful)
            {
                dynamic json = JsonConvert.DeserializeObject(response.Content);
                return json.token;
            }

            return null;
        }
    }
}
