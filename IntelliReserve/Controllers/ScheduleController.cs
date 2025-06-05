using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using IntelliReserve.Data;
using IntelliReserve.Models;

namespace IntelliReserve.Controllers
{
    public class ScheduleController : Controller
    {
        private readonly AppDbContext _context;

        public ScheduleController(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var schedules = _context.Schedules.Include(s => s.Business);
            return View(await schedules.ToListAsync());
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var schedule = await _context.Schedules
                .Include(s => s.Business)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (schedule == null) return NotFound();

            return View(schedule);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("BusinessId,DayOfWeek,OpenTime,CloseTime")] Schedule schedule)
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
            var schedule = await _context.Schedules.FindAsync(id);
            if (schedule == null) return NotFound();
            return View(schedule);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,BusinessId,DayOfWeek,OpenTime,CloseTime")] Schedule schedule)
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
            var schedule = await _context.Schedules
                .Include(s => s.Business)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (schedule == null) return NotFound();
            return View(schedule);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var schedule = await _context.Schedules.FindAsync(id);
            _context.Schedules.Remove(schedule);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        public IActionResult List(int serviceId)
        {
            var schedules = _context.ServiceSchedules
                .Where(s => s.ServiceId == serviceId && s.StartDateTime > DateTime.Now)
                .OrderBy(s => s.StartDateTime)
                .ToList();

            var service = _context.Services.FirstOrDefault(s => s.Id == serviceId);
            ViewBag.ServiceName = service?.Name;

            return View("SchedulesCustomer", schedules);
        }
    }
}
