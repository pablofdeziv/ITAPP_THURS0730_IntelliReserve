using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using IntelliReserve.Data;
using IntelliReserve.Models;

namespace IntelliReserve.Controllers
{
    public class ServiceScheduleController : Controller
    {
        private readonly AppDbContext _context;

        public ServiceScheduleController(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var schedules = _context.ServiceSchedules.Include(s => s.Service);
            return View(await schedules.ToListAsync());
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();
            var schedule = await _context.ServiceSchedules.Include(s => s.Service).FirstOrDefaultAsync(s => s.Id == id);
            if (schedule == null) return NotFound();
            return View(schedule);
        }

        public IActionResult Create() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ServiceId,StartDateTime,EndDateTime")] ServiceSchedule schedule)
        {
            if (ModelState.IsValid)
            {
                _context.Add(schedule);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(schedule);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var schedule = await _context.ServiceSchedules.FindAsync(id);
            if (schedule == null) return NotFound();
            return View(schedule);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,ServiceId,StartDateTime,EndDateTime")] ServiceSchedule schedule)
        {
            if (id != schedule.Id) return NotFound();
            if (ModelState.IsValid)
            {
                _context.Update(schedule);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(schedule);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var schedule = await _context.ServiceSchedules.Include(s => s.Service).FirstOrDefaultAsync(s => s.Id == id);
            if (schedule == null) return NotFound();
            return View(schedule);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var schedule = await _context.ServiceSchedules.FindAsync(id);
            _context.ServiceSchedules.Remove(schedule);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        

    }
}
