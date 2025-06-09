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
                return View("~/Views/BusinessFuncts/CreateService.cshtml", model);

            }

            // Obtener el ID del negocio del usuario logueado (asumiendo que se guarda en la BD)
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var business = _context.Businesses.FirstOrDefault(b => b.OwnerId == userId);

            if (business == null)
            {
                // Mostrar error si el usuario no tiene negocio asociado
                ModelState.AddModelError("", "No associated business found for the logged-in user.");
                return View("~/Views/BusinessFuncts/CreateService.cshtml", model);

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

            foreach (var scheduleVM in model.Schedules)
            {
                var newSchedule = new ServiceSchedule
                {
                    StartDateTime = DateTime.SpecifyKind(scheduleVM.StartDateTime, DateTimeKind.Local).ToUniversalTime(),
                    EndDateTime = DateTime.SpecifyKind(scheduleVM.EndDateTime, DateTimeKind.Local).ToUniversalTime()
                };

                // 1. Añadimos el ServiceSchedule al servicio (para que se guarde con el servicio)
                service.Schedules.Add(newSchedule);

                // 2. Creamos el Appointment y lo asociamos directamente al ServiceSchedule
                //    usando la propiedad de navegación. EF Core resolverá el ServiceScheduleId.
                var newAppointment = new Appointment
                {
                    UserId = null,
                    Status = AppointmentStatus.Pending,
                    ServiceSchedule = newSchedule // ¡Esto es clave para la relación!
                };

                // 3. Añadimos explícitamente el Appointment al DbContext.
                //    Aunque ServiceSchedule es parte del grafo de Service, Appointment no lo es.
                _context.Appointments.Add(newAppointment);
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
            _context.SaveChanges();

            return RedirectToAction("HomeBusiness", "Home");
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
        [Route("services-customer/{businessId}")]
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
    
        [HttpGet]
        [Route("edit-service-business/{id}")]
        public IActionResult EditService(int id)
        {
            var service = _context.Services
                .Include(s => s.Schedules)
                .Include(s => s.AvailableDays)
                .FirstOrDefault(s => s.Id == id);

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
                StartDate = service.Schedules.Min(s => s.StartDateTime).ToLocalTime().Date,
                EndDate = service.Schedules.Max(s => s.EndDateTime).ToLocalTime().Date,
                AvailableDays = service.AvailableDays.Select(d => d.DayOfWeek).ToList(),
                Schedules = service.Schedules.Select(sc => new ServiceScheduleViewModel
                {
                    StartDateTime = sc.StartDateTime.ToLocalTime(),
                    EndDateTime = sc.EndDateTime.ToLocalTime()
                }).ToList()
            };

            return View("~/Views/BusinessFuncts/EditService.cshtml", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult EditService(EditServiceViewModel model)
        {
            if (!ModelState.IsValid)
            {
               
                return View("~/Views/BusinessFuncts/EditService.cshtml", model);
            }

            // Carga el servicio existente, incluyendo sus horarios y los appointments asociados a esos horarios.
            // Es crucial usar .Include().ThenInclude() para cargar los Appointments.
            var service = _context.Services
                .Include(s => s.Schedules)
                    .ThenInclude(ss => ss.Appointment)
                .Include(s => s.AvailableDays)
                .FirstOrDefault(s => s.Id == model.Id);

            if (service == null)
            {
                return NotFound();
            }

            // 1. Actualiza las propiedades directas del servicio con los nuevos valores del modelo.
            service.Name = model.Name;
            service.Duration = model.Duration;
            service.Price = model.Price;
            // Asegura que las fechas y horas se guarden en UTC.
            service.AvailableFrom = DateTime.SpecifyKind(DateTime.Today.Add(model.AvailableFrom), DateTimeKind.Local).ToUniversalTime();
            service.AvailableTo = DateTime.SpecifyKind(DateTime.Today.Add(model.AvailableTo), DateTimeKind.Local).ToUniversalTime();

            // 2. Elimina los ServiceSchedules existentes y sus Appointments asociados.
            // Es importante eliminar primero los Appointments, ya que tienen una clave foránea a ServiceSchedule.
            foreach (var schedule in service.Schedules)
            {
                if (schedule.Appointment != null)
                {
                    _context.Appointments.Remove(schedule.Appointment);
                }
            }
            _context.ServiceSchedules.RemoveRange(service.Schedules); // Elimina todos los ServiceSchedules antiguos del servicio.
            _context.ServiceAvailabilities.RemoveRange(service.AvailableDays); // Elimina todos los AvailableDays antiguos del servicio.

            // 3. Vuelve a crear los AvailableDays del servicio basándose en los datos del modelo.
            service.AvailableDays = model.AvailableDays.Select(day => new ServiceAvailability
            {
                DayOfWeek = day
            }).ToList();

            // 4. Vuelve a crear los ServiceSchedules y sus Appointments asociados para el servicio.
            // Esta lógica es idéntica a la que ya tienes en el método Create POST del ServiceController.
            foreach (var scheduleVM in model.Schedules)
            {
                var newSchedule = new ServiceSchedule
                {
                    StartDateTime = DateTime.SpecifyKind(scheduleVM.StartDateTime, DateTimeKind.Local).ToUniversalTime(),
                    EndDateTime = DateTime.SpecifyKind(scheduleVM.EndDateTime, DateTimeKind.Local).ToUniversalTime()
                };

                // Añade el nuevo ServiceSchedule a la colección del servicio.
                service.Schedules.Add(newSchedule);

                // Crea un nuevo Appointment asociado al nuevo ServiceSchedule.
                var newAppointment = new Appointment
                {
                    UserId = null, // Inicialmente sin usuario asignado.
                    Status = AppointmentStatus.Pending, // Estado por defecto para un slot nuevo.
                    ServiceSchedule = newSchedule // Vincula el Appointment al ServiceSchedule.
                };
                _context.Appointments.Add(newAppointment); // Añade el nuevo Appointment al contexto.
            }

            // Guarda todos los cambios en la base de datos (actualizaciones, eliminaciones y nuevas inserciones).
            _context.SaveChanges();

            TempData["SuccessMessage"] = "Servicio actualizado correctamente.";
            // Redirige a la página principal del negocio.
            return RedirectToAction("HomeBusiness", "Home");
        }





        [HttpGet]
        public IActionResult List(int businessId)
        {
            var business = _context.Businesses
                .Include(b => b.Services)
                    .ThenInclude(s => s.Schedules)
                .Include(b => b.Services)
                    .ThenInclude(s => s.AvailableDays)
                .FirstOrDefault(b => b.Id == businessId);

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