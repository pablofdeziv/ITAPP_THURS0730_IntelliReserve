using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using IntelliReserve.Data;
using IntelliReserve.Models;

namespace IntelliReserve.Controllers
{
    public class AppointmentHistoryController : Controller
    {
        private readonly AppDbContext _context;

        public AppointmentHistoryController(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var history = _context.AppointmentHistories
                .Include(h => h.Appointment);
            return View(await history.ToListAsync());
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var item = await _context.AppointmentHistories
                .Include(h => h.Appointment)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (item == null) return NotFound();

            return View(item);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("AppointmentId,Timestamp,StatusChange,Comment")] AppointmentHistory history)
        {
            if (ModelState.IsValid)
            {
                _context.Add(history);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(history);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var item = await _context.AppointmentHistories.FindAsync(id);
            if (item == null) return NotFound();
            return View(item);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,AppointmentId,Timestamp,StatusChange,Comment")] AppointmentHistory history)
        {
            if (id != history.Id) return NotFound();

            if (ModelState.IsValid)
            {
                _context.Update(history);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(history);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var item = await _context.AppointmentHistories
                .Include(h => h.Appointment)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (item == null) return NotFound();
            return View(item);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var item = await _context.AppointmentHistories.FindAsync(id);
            _context.AppointmentHistories.Remove(item);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}
