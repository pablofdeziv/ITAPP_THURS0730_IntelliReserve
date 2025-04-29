using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using IntelliReserve.Data;
using IntelliReserve.Models;

namespace IntelliReserve.Controllers
{
    public class NotificationController : Controller
    {
        private readonly AppDbContext _context;

        public NotificationController(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var notifications = _context.Notifications.Include(n => n.User);
            return View(await notifications.ToListAsync());
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();
            var notification = await _context.Notifications.Include(n => n.User).FirstOrDefaultAsync(n => n.Id == id);
            if (notification == null) return NotFound();
            return View(notification);
        }

        public IActionResult Create() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("UserId,Message,SentAt,IsRead")] Notification notification)
        {
            if (ModelState.IsValid)
            {
                _context.Add(notification);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(notification);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var notification = await _context.Notifications.FindAsync(id);
            if (notification == null) return NotFound();
            return View(notification);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,UserId,Message,SentAt,IsRead")] Notification notification)
        {
            if (id != notification.Id) return NotFound();
            if (ModelState.IsValid)
            {
                _context.Update(notification);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(notification);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var notification = await _context.Notifications.Include(n => n.User).FirstOrDefaultAsync(n => n.Id == id);
            if (notification == null) return NotFound();
            return View(notification);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var notification = await _context.Notifications.FindAsync(id);
            _context.Notifications.Remove(notification);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}
