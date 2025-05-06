using IntelliReserve.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Security.Principal;

namespace IntelliReserve.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }


        [Route("login")]
        [HttpGet]
        public IActionResult Login()
        {
            return View("~/Views/Account/login.cshtml");

        }


        [Route("register-customer")]
        [HttpGet]
        public IActionResult RegisterCustomer()
        {
            return View("~/Views/Account/RegisterCustomer.cshtml"); // Redirige a la vista de registro

        }

        [Route("register-business")]
        [HttpGet]
        public IActionResult RegisterBusiness()
        {
            return View("~/Views/Account/RegisterBusiness.cshtml"); // Redirige a la vista de registro
       
        }

        [Route("home-business")]
        [HttpGet]
        public IActionResult HomeBusiness()
        {
            return View("~/Views/Home/AdminHome.cshtml"); // Redirige a la vista de registro

        }

        [Route("profile-customer")]
        [HttpGet]
        public IActionResult ProfileCustomer()
        {
            return View("~/Views/Profile/ProfileCustomer.cshtml"); // Redirige a la vista de registro

        }

        [Route("home-customer")]
        [HttpGet]
        public IActionResult HomeCustomer()
        {
            return View("~/Views/Home/CustomerHome.cshtml"); // Redirige a la vista de registro

        }

        [Route("profile-business")]
        [HttpGet]
        public IActionResult ProfileBusiness()
        {
            return View("~/Views/Profile/ProfileBusiness.cshtml"); // Redirige a la vista de registro

        }

    }


}
