using System;
using System.Diagnostics;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using IdentityModel.Client;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using Simple.Serilog.Attributes;
using Simple.Serilog.Middleware;
using SimpleUI.Models;

namespace SimpleUI.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly SimpleApiClient _apiClient;

        public HomeController(SimpleApiClient apiClient)
        {
            _apiClient = apiClient;
        }

        [AllowAnonymous]
        [LogUsage("View Home Page")]
        public IActionResult Index()
        {
            return View();
        }

        [LogUsage("View About Page")]
        public IActionResult About()
        {
            Log.Debug("We got here....");
            ViewData["Message"] = "Your application description page.";

            return View();
        }

        [LogUsage("View Bad Page")]
        public IActionResult BadPage(int id)
        {            
            ViewData["Message"] = "Your exception page.";
            throw new Exception("Craziness!!!");
            //return View();
        }

        public IActionResult BadPageWithQuery(int id, string code)
        {
            throw new Exception("Something bad happened.");
        }

        [LogUsage("View Good API Page")]
        public async Task<IActionResult> GoodApi()
        {
            var results = await _apiClient.GetAllValuesAsync();
            ViewBag.Json = string.Join(", ", results); //JsonSerializer.Serialize(results);

            return View();
        }

        public async Task<IActionResult> UnauthApi()
        {
            // wrong way to make API call -- wrong token and not using httpclientfactory
            var client = new HttpClient();
            var token = await HttpContext.GetTokenAsync("id_token");  // consciously getting wrong token here
            client.SetBearerToken(token);

            var response = await GetWithHandlingAsync(client, "https://localhost:44389/api/Values");
            
            ViewBag.Json = await response.Content.ReadAsStringAsync();
            return View("BadApi");  // should never really get here....            
        }

        public async Task<IActionResult> GoodParamApi()
        {
            var results = await _apiClient.GetSingleValueAsync(456);
            ViewBag.Json = results;
            return View(); 
        }

        public async Task<IActionResult> BadApi()
        {
            var results = await _apiClient.GetSingleValueAsync(123);
            ViewBag.Json = results;
            return View();
        }        

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        private static async Task<HttpResponseMessage> GetWithHandlingAsync(HttpClient client, string apiRoute)
        {
            var response = await client.GetAsync(apiRoute);
            if (!response.IsSuccessStatusCode)
            {
                var ex = new Exception("API Failure");
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
            return response;
        }
    }
}
