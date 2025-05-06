using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using IntelliReserve.Data;
using IntelliReserve.Models;

namespace IntelliReserve.Controllers
{
    public class BusinessController : Controller
    {
        private readonly AppDbContext _context;

        public BusinessController(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            return View(await _context.Businesses.Include(b => b.Owner).ToListAsync());
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var business = await _context.Businesses
                .Include(b => b.Owner)
                .Include(b => b.Employees)
                .Include(b => b.Services)
                .Include(b => b.Schedules)
                .Include(b => b.Reviews)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (business == null) return NotFound();

            return View(business);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Name,OwnerId,Address,Phone,Description")] Business business)
        {
            if (ModelState.IsValid)
            {
                _context.Add(business);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(business);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var business = await _context.Businesses.FindAsync(id);
            if (business == null) return NotFound();
            return View(business);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,OwnerId,Address,Phone,Description")] Business business)
        {
            if (id != business.Id) return NotFound();
            if (ModelState.IsValid)
            {
                _context.Update(business);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(business);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var business = await _context.Businesses.FirstOrDefaultAsync(m => m.Id == id);
            if (business == null) return NotFound();
            return View(business);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var business = await _context.Businesses.FindAsync(id);
            _context.Businesses.Remove(business);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public IActionResult RegisterBusiness()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RegisterBusiness(RegisterBusinessViewModel model)
        {
            if (!ModelState.IsValid)
            {
                // Muestra errores si el modelo no es válido
                foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
                {
                    Console.WriteLine("Model error: " + error.ErrorMessage);
                }
                return View(model);
            }

            try
            {
                // Verifica si ya existe un usuario con ese correo
                if (await _context.Users.AnyAsync(u => u.Email == model.Email))
                {
                    ModelState.AddModelError("Email", "El correo ya está registrado.");
                    return View(model);
                }

                // Crear el usuario propietario
                var user = new User
                {
                    Name = model.Name,
                    Email = model.Email,
                    Password = model.Password, // ⚠️ Hashear en producción
                    Role = UserRole.BusinessAdmin
                };

                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                // Confirmar que se generó el ID
                if (user.Id == 0)
                {
                    ModelState.AddModelError("", "No se pudo generar el ID del usuario.");
                    return View(model);
                }

                // Crear el negocio
                var business = new Business
                {
                    Name = model.OrganizationName,
                    Address = model.Address,
                    Phone = model.Phone,
                    Description = model.Description,
                    OwnerId = user.Id
                };

                _context.Businesses.Add(business);
                await _context.SaveChangesAsync();

                return RedirectToAction("Login", "User");
            }
            catch (Exception ex)
            {
                Console.WriteLine("ERROR AL REGISTRAR: " + ex.Message);
                ModelState.AddModelError("", "Ocurrió un error al registrar el negocio. Intenta nuevamente.");
                return View(model);
            }
        }

    }
}