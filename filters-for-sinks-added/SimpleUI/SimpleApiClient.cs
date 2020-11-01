using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Simple.Serilog.Middleware;

namespace SimpleUI
{
    public class SimpleApiClient
    {
        private readonly HttpClient _client;

        public SimpleApiClient(HttpClient client)
        {
            client.BaseAddress = new Uri("https://localhost:44389/api/");
            _client = client;
        }

        public async Task<List<string>> GetAllValuesAsync()
        {
            var response = await _client.GetAsync("Values");
            await EnsureSuccessfulResponse(response);
            
            return await response.Content.ReadFromJsonAsync<List<string>>();
        }

        public async Task<string> GetSingleValueAsync(int id)
        {
            var response = await _client.GetAsync($"Values/{id}");
            await EnsureSuccessfulResponse(response);

            return await response.Content.ReadAsStringAsync();
        }

        private async Task EnsureSuccessfulResponse(HttpResponseMessage response)
        {
            if (!response.IsSuccessStatusCode)
            {
                var ex = new Exception("Error occurred in API call.");
                ex.Data.Add("ResponseCode", Convert.ToInt16(response.StatusCode));
                ex.Data.Add("RequestUri", response.RequestMessage?.RequestUri);
                ex.Data.Add("RequestMethod", response.RequestMessage?.Method);
                if (response.Content.Headers.ContentLength > 0)
                {
                    var error = await response.Content.ReadFromJsonAsync<ApiError>();
                    if (error != null)
                    {
                        ex.Data.Add("ErrorId", error.Id);
                        ex.Data.Add("ErrorMessage", error.Title);
                    }
                }
                throw ex;
            }
        }
    }
}
