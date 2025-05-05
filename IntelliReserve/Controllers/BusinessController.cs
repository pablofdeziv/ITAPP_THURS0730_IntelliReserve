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
        public async Task<IActionResult> RegisterWithUser(RegisterBusinessViewModel model)
        {
            if (!ModelState.IsValid)
                return View("Register", model);

            // Verificamos que no exista ya el email
            var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == model.Email);
            if (existingUser != null)
            {
                ModelState.AddModelError("Email", "This email is already in use.");
                return View("Register", model);
            }

            // Creamos el usuario
            var user = new User
            {
                Name = model.Name,
                Email = model.Email,
                Password = model.Password // ⚠️ Considera hashear esta contraseña
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

            return RedirectToAction("Login", "User");
        }

    }
}
