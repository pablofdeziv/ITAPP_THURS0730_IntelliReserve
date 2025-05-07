using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using IntelliReserve.Data;
using IntelliReserve.Models;
using IntelliReserve.ViewModels;
using System.Security.Claims;

namespace IntelliReserve.Controllers
{
    public class ServiceController : Controller
    {
        private readonly AppDbContext _context;

        public ServiceController(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var services = _context.Services.Include(s => s.Business);
            return View(await services.ToListAsync());
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var service = await _context.Services
                .Include(s => s.Business)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (service == null) return NotFound();

            return View(service);
        }

        public IActionResult Create(int businessId)
        {
            var viewModel = new ServiceWithSchedulesViewModel
            {
                Service = new Service
                {
                    BusinessId = businessId
                }
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ServiceWithSchedulesViewModel viewModel)
        {
            var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userIdString) || !int.TryParse(userIdString, out int userId))
            {
                Console.WriteLine("[ERROR] No se pudo obtener el ID del usuario.");
                return RedirectToAction("Login", "Account");
            }

            // Obtener el negocio del usuario logueado
            var business = await _context.Businesses.FirstOrDefaultAsync(b => b.OwnerId == userId);

            if (business == null)
            {
                Console.WriteLine("[ERROR] No se encontró un negocio para el usuario logueado.");
                ModelState.AddModelError("", "No tienes un negocio registrado.");
                return View(viewModel); // Mostrar errores
            }

            // Asignar el BusinessId al servicio
            viewModel.Service.BusinessId = business.Id;

            Console.WriteLine($"[DEBUG] POST Create llamado con servicio: {viewModel.Service.Name}, businessId: {viewModel.Service.BusinessId}, schedules: {viewModel.ServiceSchedules.Count}");

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Services.Add(viewModel.Service);
                    await _context.SaveChangesAsync();

                    Console.WriteLine($"[DEBUG] Servicio creado con ID: {viewModel.Service.Id}");

                    foreach (var schedule in viewModel.ServiceSchedules)
                    {
                        schedule.ServiceId = viewModel.Service.Id;
                        Console.WriteLine($"[DEBUG] Añadiendo horario: {schedule.StartDateTime} - {schedule.EndDateTime}");
                        _context.ServiceSchedules.Add(schedule);
                    }

                    await _context.SaveChangesAsync();

                    Console.WriteLine($"[DEBUG] Todos los horarios guardados para el servicio ID: {viewModel.Service.Id}");

                    return RedirectToAction("AdminHome", "Home");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[ERROR] Error al crear el servicio: {ex.Message}");
                    if (ex.InnerException != null)
                        Console.WriteLine($"[ERROR] Inner exception: {ex.InnerException.Message}");

                    ModelState.AddModelError("", "Error al guardar el servicio. Revisa los datos.");
                }
            }
            else
            {
                Console.WriteLine("[WARN] ModelState no es válido. Errores:");
                foreach (var modelState in ModelState.Values)
                {
                    foreach (var error in modelState.Errors)
                    {
                        Console.WriteLine($"[ERROR] {error.ErrorMessage}");
                    }
                }
            }

            // Si algo falla, se devuelve la vista con los errores
            return RedirectToAction("AdminHome", "Home");
        }



        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var service = await _context.Services.FindAsync(id);
            if (service == null) return NotFound();
            return View(service);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,BusinessId,Name,Duration,Price")] Service service)
        {
            if (id != service.Id) return NotFound();

            if (ModelState.IsValid)
            {
                _context.Update(service);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(service);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var service = await _context.Services
                .Include(s => s.Business)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (service == null) return NotFound();
            return View(service);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var service = await _context.Services.FindAsync(id);
            _context.Services.Remove(service);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
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
            return View("~/Views/BusinessFuncts/EditService.cshtml"); // Redirige a la vista de editar servicio

        }
    }
}