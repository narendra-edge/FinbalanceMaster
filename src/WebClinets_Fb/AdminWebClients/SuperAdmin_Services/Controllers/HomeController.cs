using FbSuperadmin.Models;
using FbSuperadmin.Services;
using IdentityModel.Client;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Diagnostics;


namespace FbSuperadmin.Controllers
{
    public class HomeController : Controller
    {
        private readonly ITokenService _tokenService;
        private readonly ILogger<HomeController> _logger;

        public HomeController(ITokenService tokenService,ILogger<HomeController> logger)
        {
            _tokenService = tokenService;
            _logger = logger;
        }

        public IActionResult Index()
        {
            
            return View();
        }
        [HttpGet]
        [Authorize]
        public async Task <IActionResult> Login()
        {
            var accessToken = await HttpContext.GetTokenAsync("access_token");

            return RedirectToAction(nameof(Index), "Home");
        }

       // [Authorize]
        public async Task<IActionResult> Scheme()
        {
            using var client = new HttpClient();

            var token = await HttpContext.GetTokenAsync("access_token");

            client.SetBearerToken(token);

            var result = await client.GetAsync("https://localhost:44320/api/SchemeAPI");
            
                if (result.IsSuccessStatusCode) 
                {
                var model = await result.Content.ReadAsStringAsync();

                var data = JsonConvert.DeserializeObject<List<Schemedata>>(model);

                   return View(data);
                }

            throw new Exception("Unable to Get Content");

        }
        public IActionResult Privacy()
        {
            return View();
        }

        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync();
            SignOut("Cookies", "oidc");
            return RedirectToAction("Index", "Home");
        }


        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}