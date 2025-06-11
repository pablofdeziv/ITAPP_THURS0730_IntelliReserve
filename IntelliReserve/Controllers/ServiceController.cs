using IntelliReserve.Data;
using IntelliReserve.Models;
using IntelliReserve.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore; // Añadir para usar async/await con EF Core
using System.Security.Claims;
using System.Linq; // Añadir si se usa Min/Max o otras operaciones LINQ

namespace IntelliReserve.Controllers
{
    [Authorize] // Asegura que solo usuarios autenticados pueden acceder a este controlador
    public class ServiceController : Controller
    {
        private readonly AppDbContext _context;

        public ServiceController(AppDbContext context)
        {
            _context = context;
        }

        // --- ACCIÓN PARA CREAR UN NUEVO SERVICIO (GET) ---
        // Simplemente muestra el formulario de creación.
        [HttpGet] // Atributo explícito para claridad
        public IActionResult Create()
        {
            return View(); // Asume que la vista se llama Create.cshtml en la carpeta Service
        }

        // --- ACCIÓN PARA PROCESAR LA CREACIÓN DEL SERVICIO (POST) ---
        [HttpPost]
        [ValidateAntiForgeryToken] // Protección contra ataques CSRF
        public async Task<IActionResult> Create(CreateServiceViewModel model) // Usar async Task<IActionResult>
        {
            if (!ModelState.IsValid)
            {
                return View("~/Views/BusinessFuncts/CreateService.cshtml", model);
            }

            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var business = await _context.Businesses.FirstOrDefaultAsync(b => b.OwnerId == userId); // Usar await

            if (business == null)
            {
                ModelState.AddModelError("", "No associated business found for the logged-in user.");
                return View("~/Views/BusinessFuncts/CreateService.cshtml", model);
            }

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

            foreach (var day in model.AvailableDays)
            {
                service.AvailableDays.Add(new ServiceAvailability
                {
                    DayOfWeek = day // Tu lógica original, asumiendo que 'day' ya es DayOfWeek o puede ser implícitamente convertido
                });
            }

            foreach (var scheduleVM in model.Schedules)
            {
                var newSchedule = new ServiceSchedule
                {
                    StartDateTime = DateTime.SpecifyKind(scheduleVM.StartDateTime, DateTimeKind.Local).ToUniversalTime(),
                    EndDateTime = DateTime.SpecifyKind(scheduleVM.EndDateTime, DateTimeKind.Local).ToUniversalTime()
                };

                service.Schedules.Add(newSchedule);

                var newAppointment = new Appointment
                {
                    UserId = null,
                    Status = AppointmentStatus.Pending,
                    ServiceSchedule = newSchedule
                };
                _context.Appointments.Add(newAppointment);
            }

            _context.Services.Add(service);
            await _context.SaveChangesAsync(); // Usar await

            return RedirectToAction("HomeBusiness", "Home");
        }

        // --- ACCIÓN PARA ELIMINAR UN SERVICIO (POST) ---
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id) // Usar async Task<IActionResult>
        {
            var service = await _context.Services // Usar await
                .Include(s => s.Schedules)
                    .ThenInclude(ss => ss.Appointment) // Asegurarse de incluir Appointments
                .Include(s => s.AvailableDays)
                .FirstOrDefaultAsync(s => s.Id == id); // Usar await

            if (service == null)
            {
                return NotFound();
            }

            foreach (var schedule in service.Schedules)
            {
                if (schedule.Appointment != null)
                {
                    _context.Appointments.Remove(schedule.Appointment);
                }
            }

            _context.ServiceSchedules.RemoveRange(service.Schedules);
            _context.ServiceAvailabilities.RemoveRange(service.AvailableDays);
            _context.Services.Remove(service);
            await _context.SaveChangesAsync(); // Usar await

            return RedirectToAction("HomeBusiness", "Home");
        }

        // --- ACCIÓN PARA PERFIL DE NEGOCIO (GET) ---
        // Si esta ruta redirige a CreateService.cshtml, considera si el nombre y la vista coinciden con su propósito.
        [Route("service-business")]
        [HttpGet]
        public IActionResult ProfileBusiness()
        {
            return View("~/Views/BusinessFuncts/CreateService.cshtml");
        }

        // --- ACCIÓN PARA OBTENER SERVICIOS DEL USUARIO LOGUEADO (API o JSON) ---
        [HttpGet]
        [Route("my-services")]
        public async Task<IActionResult> GetUserServices() // Usar async Task<IActionResult>
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var business = await _context.Businesses // Usar await
                .Include(b => b.Services)
                .ThenInclude(s => s.Schedules)
                .Include(b => b.Services)
                .ThenInclude(s => s.AvailableDays)
                .FirstOrDefaultAsync(b => b.OwnerId == userId); // Usar await

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

        // --- ACCIÓN PARA OBTENER SERVICIOS POR ID DE NEGOCIO (API o JSON) ---
        [HttpGet]
        [Route("services-customer/{businessId}")]
        public async Task<IActionResult> GetServicesByBusiness(int businessId) // Usar async Task<IActionResult>
        {
            var business = await _context.Businesses // Usar await
                .Include(b => b.Services)
                .ThenInclude(s => s.Schedules)
                .Include(b => b.Services)
                .ThenInclude(s => s.AvailableDays)
                .FirstOrDefaultAsync(b => b.Id == businessId); // Usar await

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

        // --- ACCIÓN GET PARA MOSTRAR EL FORMULARIO DE EDICIÓN DE SERVICIO ---
        // ESTA ES LA ACCIÓN QUE CARGARÁ LA VISTA DE EDICIÓN CON LOS DATOS.
        // Siempre debe recibir un 'id'.
        [HttpGet]
        [Route("edit-service-business/{id}")]
        public async Task<IActionResult> EditService(int id) // Usar async Task<IActionResult>
        {
            var service = await _context.Services // Usar await
                .Include(s => s.Schedules)
                .Include(s => s.AvailableDays)
                .FirstOrDefaultAsync(s => s.Id == id); // Usar await

            if (service == null)
            {
                return NotFound();
            }

            var model = new EditServiceViewModel
            {
                Id = service.Id,
                Name = service.Name,
                Duration = service.Duration,
                Price = service.Price,
                AvailableFrom = service.AvailableFrom.ToLocalTime().TimeOfDay,
                AvailableTo = service.AvailableTo.ToLocalTime().TimeOfDay,
                StartDate = service.Schedules.Any() ? service.Schedules.Min(s => s.StartDateTime).ToLocalTime().Date : DateTime.Today.Date,
                EndDate = service.Schedules.Any() ? service.Schedules.Max(s => s.EndDateTime).ToLocalTime().Date : DateTime.Today.Date,
                AvailableDays = service.AvailableDays.Select(d => d.DayOfWeek).ToList(),
                Schedules = service.Schedules.Select(sc => new ServiceScheduleViewModel
                {
                    StartDateTime = sc.StartDateTime.ToLocalTime(),
                    EndDateTime = sc.EndDateTime.ToLocalTime()
                }).ToList()
            };

            return View("~/Views/BusinessFuncts/EditService.cshtml", model);
        }

        // --- ACCIÓN POST PARA PROCESAR EL ENVÍO DEL FORMULARIO DE EDICIÓN ---
        [HttpPost]
        [Route("edit-service-business/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditService(EditServiceViewModel model) // Usar async Task<IActionResult>
        {
            if (!ModelState.IsValid)
            {
                return View("~/Views/BusinessFuncts/EditService.cshtml", model);
            }

            var service = await _context.Services // Usar await
                .Include(s => s.Schedules)
                    .ThenInclude(ss => ss.Appointment)
                .Include(s => s.AvailableDays)
                .FirstOrDefaultAsync(s => s.Id == model.Id); // Usar await

            if (service == null)
            {
                return NotFound();
            }

            // 1. Actualiza las propiedades directas del servicio.
            service.Name = model.Name;
            service.Duration = model.Duration;
            service.Price = model.Price;
            service.AvailableFrom = DateTime.SpecifyKind(DateTime.Today.Add(model.AvailableFrom), DateTimeKind.Local).ToUniversalTime();
            service.AvailableTo = DateTime.SpecifyKind(DateTime.Today.Add(model.AvailableTo), DateTimeKind.Local).ToUniversalTime();

            // 2. Elimina los ServiceSchedules existentes y sus Appointments asociados.
            foreach (var schedule in service.Schedules)
            {
                if (schedule.Appointment != null)
                {
                    _context.Appointments.Remove(schedule.Appointment);
                }
            }
            _context.ServiceSchedules.RemoveRange(service.Schedules);

            // 3. Elimina los AvailableDays existentes.
            _context.ServiceAvailabilities.RemoveRange(service.AvailableDays);

            // 4. Vuelve a crear los AvailableDays del servicio basándose en los datos del modelo.
            // Regresamos a tu lógica original, asumiendo que model.AvailableDays ya contiene DayOfWeek o string convertible.
            service.AvailableDays = model.AvailableDays.Select(day => new ServiceAvailability
            {
                DayOfWeek = day // Tu lógica original, asumiendo que 'day' ya es DayOfWeek o puede ser implícitamente convertido
            }).ToList();


            // 5. Vuelve a crear los ServiceSchedules y sus Appointments asociados para el servicio.
            foreach (var scheduleVM in model.Schedules)
            {
                var newSchedule = new ServiceSchedule
                {
                    StartDateTime = DateTime.SpecifyKind(scheduleVM.StartDateTime, DateTimeKind.Local).ToUniversalTime(),
                    EndDateTime = DateTime.SpecifyKind(scheduleVM.EndDateTime, DateTimeKind.Local).ToUniversalTime()
                };

                service.Schedules.Add(newSchedule);

                var newAppointment = new Appointment
                {
                    UserId = null,
                    Status = AppointmentStatus.Pending,
                    ServiceSchedule = newSchedule
                };
                _context.Appointments.Add(newAppointment);
            }

            await _context.SaveChangesAsync(); // Usar await

            TempData["SuccessMessage"] = "Servicio actualizado correctamente.";
            return RedirectToAction("HomeBusiness", "Home");
        }

        // --- ACCIÓN PARA MOSTRAR LISTA DE SERVICIOS PARA UN NEGOCIO (GET) ---
        [HttpGet]
        public async Task<IActionResult> List(int businessId) // Usar async Task<IActionResult>
        {
            var business = await _context.Businesses // Usar await
                .Include(b => b.Services)
                    .ThenInclude(s => s.Schedules)
                .Include(b => b.Services)
                    .ThenInclude(s => s.AvailableDays)
                .FirstOrDefaultAsync(b => b.Id == businessId); // Usar await

            if (business == null)
                return NotFound("Business not found.");

            var viewModel = new BusinessWithServicesViewModel
            {
                Business = business,
                Services = business.Services.ToList()
            };

            return View("~/Views/CustomerFuncts/ServicesCustomer.cshtml", viewModel);
        }
    }
}