using IntelliReserve.Data;
using IntelliReserve.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using System.Security.Claims;
using System.Security.Principal;
using System.Linq; // ¡Asegúrate de que esta línea esté presente!


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

        /*[Route("home-customer")]
        [HttpGet]
        public IActionResult CustomerHome()
        {

            return View("~/Views/Home/CustomerHome.cshtml"); 
        
        }*/

        [Route("home-customer")]
        [HttpGet]
        public async Task<IActionResult> CustomerHome()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
            {
                return RedirectToAction("Login", "Home");
            }

            int parsedUserId = int.Parse(userId);

            // 1. Obtener IDs de servicios ya reservados por este usuario
            var previousServiceIds = await _context.Appointments
                .Where(a => a.User.Id == parsedUserId)
                .Select(a => a.ServiceSchedule.ServiceId)
                .Distinct()
                .ToListAsync();

            // 2. Obtener los IDs de negocios de los servicios previos
            //    Esto se hace en la base de datos
            var previousBusinessIds = await _context.Services
                .Where(s => previousServiceIds.Contains(s.Id))
                .Select(s => s.BusinessId)
                .Distinct()
                .ToListAsync();

            // 3. Obtener las primeras palabras de los nombres de los servicios previos
            //    Esto se hace en la base de datos y luego se procesa en memoria
            var firstWordsOfPreviousServiceNames = await _context.Services
                .Where(s => previousServiceIds.Contains(s.Id))
                .Select(s => s.Name)
                .ToListAsync(); // <-- Traemos los nombres a memoria
                
            var processedFirstWords = firstWordsOfPreviousServiceNames
                .Select(name => name.Split(' ').FirstOrDefault())
                .Where(word => !string.IsNullOrEmpty(word))
                .ToList();


            // 4. Obtener una lista inicial de servicios candidatos de la base de datos.
            //    Filtramos por los que NO han sido reservados y que pertenecen a los mismos negocios.
            var candidateServicesQuery = _context.Services
                .Where(s =>
                    !previousServiceIds.Contains(s.Id) && // Que no sean servicios que ya haya reservado
                    (previousBusinessIds.Any() && previousBusinessIds.Contains(s.BusinessId)) // Del mismo negocio
                );

            // 5. Traer los servicios candidatos a memoria para aplicar el filtro de nombres.
            var candidateServices = await candidateServicesQuery.ToListAsync(); // <-- ¡Aquí se ejecuta la consulta a DB!


            // 6. Ahora, aplicar el filtro de nombres en memoria.
            //    Filtramos entre los servicios candidatos aquellos cuyos nombres contengan alguna de las primeras palabras.
            var recommendedServices = candidateServices
                .Where(s =>
                    processedFirstWords.Any() && // Solo si hay palabras para buscar
                    processedFirstWords.Any(firstWord => s.Name.Contains(firstWord))
                )
                .Take(3) // Tomar hasta 3 servicios recomendados
                .ToList(); // Convertir a lista final

            return View("CustomerHome", recommendedServices);
        }



        [Route("profile-business")]
        [HttpGet]
        public IActionResult ProfileBusiness()
        {
            return View("~/Views/Profile/ProfileBusiness.cshtml"); // Redirige a la vista de registro

        }

        

    }


}
