using IntelliReserve.Data;
using IntelliReserve.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace IntelliReserve.Controllers
{
    [Authorize]
    public class AppointmentController : Controller
    {
        private readonly AppDbContext _context;

        public AppointmentController(AppDbContext context)
        {
            _context = context;
        }

        // POST: Appointment/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(int serviceScheduleId)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            var appointment = new Appointment
            {
                UserId = userId,
                ServiceScheduleId = serviceScheduleId,
                Status = AppointmentStatus.Pending
            };

            _context.Appointments.Add(appointment);
            _context.SaveChanges();

            return RedirectToAction("CalendarView"); // Cambia esto al destino deseado
        }

        // GET: Appointment/Edit/5
        public IActionResult Edit(int id)
        {
            var appointment = _context.Appointments
                .Include(a => a.User)
                .Include(a => a.ServiceSchedule)
                .FirstOrDefault(a => a.Id == id);

            if (appointment == null)
            {
                return NotFound();
            }

            return View(appointment);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, AppointmentStatus status)
        {
            var appointment = _context.Appointments.Find(id);
            if (appointment == null)
            {
                return NotFound();
            }

            appointment.Status = status;
            _context.SaveChanges();

            return RedirectToAction("CalendarView"); // Cambia esto al destino deseado
        }

        // POST: Appointment/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(int id)
        {
            var appointment = _context.Appointments.Find(id);
            if (appointment == null)
            {
                return NotFound();
            }

            _context.Appointments.Remove(appointment);
            _context.SaveChanges();

            return RedirectToAction("CalendarView"); // Cambia esto al destino deseado
        }
    }
}