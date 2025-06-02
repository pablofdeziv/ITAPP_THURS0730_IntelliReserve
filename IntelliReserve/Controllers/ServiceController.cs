using IntelliReserve.Data;
using IntelliReserve.Models;
using IntelliReserve.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace IntelliReserve.Controllers
{
    [Authorize]
    public class ServiceController : Controller
    {
        private readonly AppDbContext _context;

        public ServiceController(AppDbContext context)
        {
            _context = context;
        }
        // GET: Crear nuevo servicio
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(CreateServiceViewModel model)
        {
            if (!ModelState.IsValid)
            {
                // Devuelve la vista con los errores de validación
                return View(model);
            }

            // Obtener el ID del negocio del usuario logueado (asumiendo que se guarda en la BD)
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var business = _context.Businesses.FirstOrDefault(b => b.OwnerId == userId);

            if (business == null)
            {
                // Mostrar error si el usuario no tiene negocio asociado
                ModelState.AddModelError("", "No associated business found for the logged-in user.");
                return View(model);
            }

            // Crear el servicio
            var service = new Service
            {
                Name = model.Name,
                Duration = model.Duration,
                Price = model.Price,
                AvailableFrom = DateTime.SpecifyKind(DateTime.Today.Add(model.AvailableFrom), DateTimeKind.Local).ToUniversalTime(),
                AvailableTo = DateTime.SpecifyKind(DateTime.Today.Add(model.AvailableTo), DateTimeKind.Local).ToUniversalTime(),
                BusinessId = business.Id,
                AvailableDays = new List<ServiceAvailability>(),
                Schedules = new List<ServiceSchedule>()
            };

            // Agregar los días de disponibilidad
            foreach (var day in model.AvailableDays)
            {
                service.AvailableDays.Add(new ServiceAvailability
                {
                    DayOfWeek = day
                });
            }

            // Agregar los horarios generados (ServiceSchedules)
            foreach (var scheduleVM in model.Schedules)
            {
                service.Schedules.Add(new ServiceSchedule
                {
                    StartDateTime = DateTime.SpecifyKind(scheduleVM.StartDateTime, DateTimeKind.Local).ToUniversalTime(),
                    EndDateTime = DateTime.SpecifyKind(scheduleVM.EndDateTime, DateTimeKind.Local).ToUniversalTime()

                });
            }

            // Guardar en la base de datos
            _context.Services.Add(service);
            _context.SaveChanges();

            // Redirigir a la lista de servicios
            return RedirectToAction("HomeBusiness", "Home");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(int id)
        {
            var service = _context.Services
                .Include(s => s.Schedules)
                .Include(s => s.AvailableDays)
                .FirstOrDefault(s => s.Id == id);

            if (service == null)
            {
                return NotFound();
            }

            _context.ServiceSchedules.RemoveRange(service.Schedules);
            _context.ServiceAvailabilities.RemoveRange(service.AvailableDays);
            _context.Services.Remove(service);
            _context.SaveChanges();

            return RedirectToAction("Index", "Business");
        }
    
       

        [Route("service-business")]
        [HttpGet]
        public IActionResult ProfileBusiness()
        {
            return View("~/Views/BusinessFuncts/CreateService.cshtml"); // Redirige a la vista de registro

        }
        [Route("edit-service-business")]
        [HttpGet]
        public IActionResult EditService()
        {
            return View("~/Views/BusinessFuncts/EditService.cshtml"); // Redirige a la vista de editar servicio }
        }

        [HttpGet]
        [Route("my-services")]
        public IActionResult GetUserServices()
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var business = _context.Businesses
                .Include(b => b.Services)
                .ThenInclude(s => s.Schedules)
                .Include(b => b.Services)
                .ThenInclude(s => s.AvailableDays)
                .FirstOrDefault(b => b.OwnerId == userId);

            if (business == null)
            {
                return NotFound("No business associated with the current user.");
            }

            var services = business.Services.Select(s => new
            {
                s.Id,
                s.Name,
                s.Duration,
                s.Price,
                s.AvailableFrom,
                s.AvailableTo,
                AvailableDays = s.AvailableDays.Select(d => d.DayOfWeek.ToString()),
                Schedules = s.Schedules.Select(sc => new
                {
                    sc.StartDateTime,
                    sc.EndDateTime
                })
            }).ToList();

            return Ok(services);
        }
        [HttpGet]
        [Route("services-by-business/{businessId}")]
        public IActionResult GetServicesByBusiness(int businessId)
        {
            var business = _context.Businesses
                .Include(b => b.Services)
                .ThenInclude(s => s.Schedules)
                .Include(b => b.Services)
                .ThenInclude(s => s.AvailableDays)
                .FirstOrDefault(b => b.Id == businessId);

            if (business == null)
            {
                return NotFound($"No business found with ID {businessId}.");
            }

            var services = business.Services.Select(s => new
            {
                s.Id,
                s.Name,
                s.Duration,
                s.Price,
                s.AvailableFrom,
                s.AvailableTo,
                AvailableDays = s.AvailableDays.Select(d => d.DayOfWeek.ToString()),
                Schedules = s.Schedules.Select(sc => new
                {
                    sc.StartDateTime,
                    sc.EndDateTime
                })
            }).ToList();

            return Ok(services);
        }

    }
}
