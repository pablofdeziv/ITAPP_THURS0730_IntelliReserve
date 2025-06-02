using IntelliReserve.Data;
using IntelliReserve.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using System.Security.Claims;
using System.Security.Principal;

namespace IntelliReserve.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly AppDbContext _context;

        public HomeController(ILogger<HomeController> logger, AppDbContext context)
        {
            _logger = logger;
            _context = context;
        }


        public IActionResult Index()
        {
            return View();
        }
       
        
        public IActionResult AdminHome()
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
            // Obtener el ID del usuario logueado
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            // Buscar el negocio del usuario
            var business = _context.Businesses
                .Include(b => b.Services)
                .FirstOrDefault(b => b.OwnerId == userId);

            if (business == null)
            {
                return RedirectToAction("RegisterBusiness");
            }

            // Obtener solo los servicios de esta empresa
            var services = business.Services.ToList();

            // Pasar los servicios como modelo a la vista
            return View("~/Views/Home/AdminHome.cshtml", services);
        }


        [Route("profile-customer")]
        [HttpGet]
        public IActionResult ProfileCustomer()
        {
            return View("~/Views/Profile/ProfileCustomer.cshtml"); // Redirige a la vista de registro

        }

        [Route("home-customer")]
        [HttpGet]
        public IActionResult CustomerHome()
        {

            return View("~/Views/Home/CustomerHome.cshtml"); 

        }

        [Route("profile-business")]
        [HttpGet]
        public IActionResult ProfileBusiness()
        {
            return View("~/Views/Profile/ProfileBusiness.cshtml"); // Redirige a la vista de registro

        }

    }


}
