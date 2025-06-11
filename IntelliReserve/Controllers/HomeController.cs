using IntelliReserve.Data;
using IntelliReserve.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using System.Security.Claims;
using System.Security.Principal;
using System.Linq; // ¡Asegúrate de que esta línea esté presente!
using IntelliReserve.Models.ViewModels;


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
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var business = _context.Businesses
                .Include(b => b.Services)
                .FirstOrDefault(b => b.OwnerId == userId);

            if (business == null)
                return RedirectToAction("RegisterBusiness");

            var serviceIds = business.Services.Select(s => s.Id).ToList();

            var appointments = _context.Appointments
                .Include(a => a.ServiceSchedule)
                .Where(a => serviceIds.Contains(a.ServiceSchedule.ServiceId))
                .ToList();

            /*foreach (var a in appointments)
            {
                Console.WriteLine($"ID: {a.Id} | Fecha: {a.ServiceSchedule.StartDateTime.ToShortDateString()} | Estado: {a.Status}");
            }
            Console.WriteLine($"TOTAL RESERVAS: {appointments.Count}");*/


            // Ventana dinámica: 3 meses antes, actual y 2 después
            var currentMonth = DateTime.Now.Month;
            var currentYear = DateTime.Now.Year;
            var months = Enumerable.Range(-3, 6)
                .Select(offset => new DateTime(currentYear, currentMonth, 1).AddMonths(offset))
                .ToList();

            var counts = months.Select(m =>
                appointments.Count(a =>
                    (a.Status == AppointmentStatus.Confirmed ||
                    a.Status == AppointmentStatus.Completed ||
                    a.Status == AppointmentStatus.Canceled) &&
                    a.ServiceSchedule.StartDateTime.Year == m.Year &&
                    a.ServiceSchedule.StartDateTime.Month == m.Month
                )
            ).ToList();


            var labels = months.Select(m => m.ToString("MMM")).ToList();

            var stats = new BookingStatisticsViewModel
            {
                Labels = labels,
                MonthlyCounts = counts,
                TotalBookings = appointments.Count(a =>
                    a.Status == AppointmentStatus.Confirmed ||
                    a.Status == AppointmentStatus.Completed ||
                    a.Status == AppointmentStatus.Canceled
                ),
                ActiveBookings = appointments.Count(a => a.Status == AppointmentStatus.Confirmed),
                CancelledBookings = appointments.Count(a => a.Status == AppointmentStatus.Canceled)
            };

            ViewBag.BookingStats = stats;

            
            var services = business.Services.ToList();

            // Obtener las últimas 5 reservas de sus servicios
            var overview = _context.Appointments
                .Include(a => a.ServiceSchedule)
                    .ThenInclude(ss => ss.Service)
                .Include(a => a.User)
                .Where(a => a.ServiceSchedule.Service.BusinessId == business.Id && a.UserId != null)
                .OrderBy(a => a.ServiceSchedule.StartDateTime)
                .Take(5)
                .Select(a => new
                {
                    ServiceName = a.ServiceSchedule.Service.Name,
                    CustomerName = a.User.Name,
                    Date = a.ServiceSchedule.StartDateTime
                })
                .ToList();

            ViewBag.ScheduleOverview = overview;

            return View("~/Views/Home/AdminHome.cshtml", business.Services.ToList());
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

            // 1. IDs de servicios ya reservados
            var previousServiceIds = await _context.Appointments
                .Where(a => a.User.Id == parsedUserId)
                .Select(a => a.ServiceSchedule.ServiceId)
                .Distinct()
                .ToListAsync();

            // 2. Servicios previos (para nombre y negocio)
            var previousServices = await _context.Services
                .Where(s => previousServiceIds.Contains(s.Id))
                .ToListAsync();

            var previousBusinessIds = previousServices
                .Select(s => s.BusinessId)
                .Distinct()
                .ToList();

            var processedFirstWords = previousServices
                .Select(s => s.Name.Split(' ').FirstOrDefault())
                .Where(word => !string.IsNullOrEmpty(word))
                .Distinct()
                .ToList();

            // 3. Recomendaciones del mismo negocio
            var sameBusinessServices = await _context.Services
                .Where(s =>
                    !previousServiceIds.Contains(s.Id) &&
                    previousBusinessIds.Contains(s.BusinessId)
                )
                .ToListAsync();

            // 4. Recomendaciones por nombre similar (sin importar el negocio)
            var similarNameServices = await _context.Services
                .Where(s =>
                    !previousServiceIds.Contains(s.Id)
                )
                .ToListAsync();

            similarNameServices = similarNameServices
                .Where(s =>
                    processedFirstWords.Any(word => s.Name.Contains(word))
                )
                .ToList();

            // 5. Unir y quitar duplicados
            var allRecommended = sameBusinessServices
                .Concat(similarNameServices)
                .GroupBy(s => s.Id)
                .Select(g => g.First())
                .Take(3)
                .ToList();

            return View("CustomerHome", allRecommended);
        }



        [Route("profile-business")]
        [HttpGet]
        public IActionResult ProfileBusiness()
        {
            return View("~/Views/Profile/ProfileBusiness.cshtml"); // Redirige a la vista de registro

        }

        

    }


}
