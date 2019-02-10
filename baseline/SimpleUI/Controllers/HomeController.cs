using System;
using System.Diagnostics;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using Serilog;
using Simple.Serilog.Attributes;
using SimpleUI.Models;

namespace SimpleUI.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        [AllowAnonymous]
        [LogUsage("View Home Page")]
        public IActionResult Index()
        {
            return View();
        }

        [LogUsage("View About Page")]
        public IActionResult About()
        {
            Log.Information("We got here....");
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
            var client = new HttpClient();
            var token = await HttpContext.GetTokenAsync("access_token");
            client.SetBearerToken(token);

            var response = await GetWithHandlingAsync(client, "https://localhost:44389/api/Values");
            
            ViewBag.Json = JArray.Parse(await response.Content.ReadAsStringAsync()).ToString();

            return View();
        }

        public async Task<IActionResult> UnauthApi()
        {
            var client = new HttpClient();
            var token = await HttpContext.GetTokenAsync("id_token");  // consciously getting wrong token here
            client.SetBearerToken(token);

            var response = await GetWithHandlingAsync(client, "https://localhost:44389/api/Values");
            
            ViewBag.Json = JArray.Parse(await response.Content.ReadAsStringAsync()).ToString();
            return View("BadApi");  // should never really get here....            
        }

        public async Task<IActionResult> GoodParamApi()
        {
            var client = new HttpClient();
            var token = await HttpContext.GetTokenAsync("access_token");
            client.SetBearerToken(token);

            var response = await GetWithHandlingAsync(client, "https://localhost:44389/api/Values/456");

            ViewBag.Json = await response.Content.ReadAsStringAsync();
            return View(); 
        }

        public async Task<IActionResult> BadApi()
        {
            var client = new HttpClient();
            var token = await HttpContext.GetTokenAsync("access_token");
            client.SetBearerToken(token);

            var response = await GetWithHandlingAsync(client, "https://localhost:44389/api/Values/123");
            
            ViewBag.Json = JArray.Parse(await response.Content.ReadAsStringAsync()).ToString();
            return View(); // should never really get here....
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
                string error = "";
                string id = "";

                if (response.Content.Headers.ContentLength > 0)
                {
                    var j = JObject.Parse(await response.Content.ReadAsStringAsync());
                    error = (string) j["Title"];
                    id = (string) j["Id"];
                }
                
                var ex = new Exception("API Failure");

                ex.Data.Add("API Route", $"GET {apiRoute}");
                ex.Data.Add("API Status", (int) response.StatusCode);
                if (!string.IsNullOrEmpty(error))
                {
                    ex.Data.Add("API Error", error);
                    ex.Data.Add("API ErrorId", id);
                }                
                throw ex;
            }            

            return response;
        }
    }
}
