using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using IntelliReserve.Data;
using IntelliReserve.Models;
using System.Security.Claims;
using IntelliReserve.Helpers;

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
               // .Include(b => b.Employees)
                .Include(b => b.Services)
               // .Include(b => b.Schedules)
               // .Include(b => b.Reviews)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (business == null) return NotFound();

            return View(business);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> GetAllBusinesses()
        {
            var businesses = await _context.Businesses
                .Select(b => new
                {
                    id = b.Id,
                    name = b.Name,
                    address = b.Address,
                    phone = b.Phone,
                    servicesLink = Url.Action("List", "Service", new { businessId = b.Id }) // o "#"
                })
                .ToListAsync();

            return Json(businesses);
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

        [HttpPost]
        [ValidateAntiForgeryToken]

        public async Task<IActionResult> RegisterWithUser(RegisterBusinessViewModel model)
        {
            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = "El modelo no es válido. Revisa los campos del formulario.";
                return View("~/Views/Account/RegisterBusiness.cshtml", model);
            }

            try
            {
                // Verificamos que no exista ya el email
                var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == model.Email);
                if (existingUser != null)
                {
                    ModelState.AddModelError("Email", "Este email ya está en uso.");
                    TempData["ErrorMessage"] = "Este email ya está registrado.";
                    return View("~/Views/Account/RegisterBusiness.cshtml", model);
                }

                // Creamos el usuario
                var user = new User
                {
                    Name = model.Name,
                    Email = model.Email,
                    Password = PasswordUtils.HashPassword(model.Password), // ⚠️ Considera hashear esta contraseña
                    Role = UserRole.BusinessAdmin
                };

                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                // Creamos el negocio
                var business = new Business
                {
                    Name = model.OrganizationName,
                    OwnerId = user.Id,
                    Address = model.Address,
                    Phone = model.Phone,
                    Description = model.Description
                };

                _context.Businesses.Add(business);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Negocio y usuario creados correctamente.";
                return RedirectToAction("Login", "User");
            }
            catch (Exception ex)
            {
                // Captura el error y lo muestra
                TempData["ErrorMessage"] = "Ocurrió un error al registrar el negocio: " + ex.Message;
                return View("~/Views/Account/RegisterBusiness.cshtml", model);
            }
        }

        [HttpGet]
        public async Task<IActionResult> ProfileBusiness()
        {
            int userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);

            var user = await _context.Users.FindAsync(userId);
            if (user == null) return NotFound();

            var business = await _context.Businesses.FirstOrDefaultAsync(b => b.OwnerId == userId);
            if (business == null) return NotFound();

            var model = new EditBusinessProfileViewModel
            {
                OwnerId = user.Id,  
                Name = user.Name,
                Email = user.Email,
                Password = "", // ⚠️ Solo para testing. En producción, no muestres contraseñas.

                OrganizationName = business.Name,
                Address = business.Address,
                Phone = business.Phone,
                Description = business.Description
            };

            return View("~/Views/Profile/ProfileBusiness.cshtml", model);
        }



        [HttpPost]
        public async Task<IActionResult> UpdateBusinessProfile(EditBusinessProfileViewModel model)
        {
            if (!ModelState.IsValid)
                return View("ProfileBusiness", model);

            // Actualizar usuario
            var user = await _context.Users.FindAsync(model.OwnerId);
            if (user == null) return NotFound();

            user.Name = model.Name;
            user.Email = model.Email;
            if (!string.IsNullOrWhiteSpace(model.Password))
            {
                user.Password = PasswordUtils.HashPassword(model.Password);
            }


            // Actualizar negocio
            var business = await _context.Businesses.FirstOrDefaultAsync(b => b.OwnerId == model.OwnerId);
            if (business == null) return NotFound();

            business.Name = model.OrganizationName;
            business.Address = model.Address;
            business.Phone = model.Phone;
            business.Description = model.Description;

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Business profile updated properly.";
            return RedirectToAction("ProfileBusiness", "Business");
        }


    }
}
