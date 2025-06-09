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

        // POST: Appointment/Create (Para realizar una reserva de un slot existente)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(int serviceScheduleId)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
            {
                TempData["ErrorMessage"] = "Debes iniciar sesión para reservar un turno.";
                return Unauthorized("User not logged in or ID not found.");
            }
            var userId = int.Parse(userIdClaim.Value);

            var serviceSchedule = await _context.ServiceSchedules
                .Include(ss => ss.Appointment)
                .FirstOrDefaultAsync(ss => ss.Id == serviceScheduleId);

            if (serviceSchedule == null)
            {
                TempData["ErrorMessage"] = "Turno no encontrado.";
                return NotFound("Service Schedule not found.");
            }

            if (serviceSchedule.Appointment == null ||
                (serviceSchedule.Appointment.Status != AppointmentStatus.Pending && serviceSchedule.Appointment.Status != AppointmentStatus.Canceled) ||
                (serviceSchedule.Appointment.Status == AppointmentStatus.Pending && serviceSchedule.Appointment.UserId != null))
            {
                TempData["ErrorMessage"] = "Este turno no está disponible para reserva.";
                return BadRequest("This slot is not available for booking.");
            }

            serviceSchedule.Appointment.UserId = userId;
            serviceSchedule.Appointment.Status = AppointmentStatus.Confirmed;

            // Eliminado: Referencia a AppointmentHistory
            // _context.AppointmentHistories.Add(new AppointmentHistory { /* ... */ });

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Turno reservado exitosamente.";
            return RedirectToAction("BookingsCustomer");
        }

        // GET: Appointment/Edit/5 (Para cargar la vista de edición de una cita)
        public async Task<IActionResult> Edit(int id)
        {
            var appointment = await _context.Appointments
                .Include(a => a.User)
                .Include(a => a.ServiceSchedule)
                    .ThenInclude(ss => ss.Service)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (appointment == null)
            {
                return NotFound();
            }

            return View(appointment);
        }

        // POST: Appointment/Edit (Para cambiar el estado de una cita existente)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, AppointmentStatus status)
        {
            var appointment = await _context.Appointments.FindAsync(id);
            if (appointment == null)
            {
                return NotFound();
            }

            // Eliminado: Referencia a AppointmentHistory
            // _context.AppointmentHistories.Add(new AppointmentHistory { /* ... */ });

            appointment.Status = status;
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Estado de la cita actualizado exitosamente.";
            return RedirectToAction("BusinessAppointments");
        }

        // POST: Appointment/Delete/5 (Para cancelar una cita)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var appointment = await _context.Appointments.FindAsync(id);
            if (appointment == null)
            {
                return NotFound();
            }

            // Eliminado: Referencia a AppointmentHistory
            // _context.AppointmentHistories.Add(new AppointmentHistory { /* ... */ });

            appointment.Status = AppointmentStatus.Canceled;
            appointment.UserId = null;
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Cita cancelada exitosamente.";
            return RedirectToAction("BookingsCustomer");
        }

        // GET: Appointment/BookingsCustomer
        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> BookingsCustomer()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
                return RedirectToAction("Login", "User");

            int userId = int.Parse(userIdClaim.Value);

            var appointments = await _context.Appointments
                .Include(a => a.ServiceSchedule)
                    .ThenInclude(ss => ss.Service)
                .Where(a => a.UserId == userId)
                .OrderByDescending(a => a.ServiceSchedule.StartDateTime)
                .ToListAsync();

            return View("~/Views/CustomerFuncts/BookingsCustomer.cshtml", appointments);
        }

        // GET: Appointment/BusinessAppointments (Para la vista de citas del negocio)
        [Authorize(Roles = "BusinessOwner")]
        public async Task<IActionResult> BusinessAppointments()
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var business = await _context.Businesses.FirstOrDefaultAsync(b => b.OwnerId == userId);

            if (business == null)
            {
                TempData["ErrorMessage"] = "No se encontró un negocio asociado a tu cuenta.";
                return Unauthorized("No business found for this user.");
            }

            var appointments = await _context.Appointments
                .Include(a => a.User)
                .Include(a => a.ServiceSchedule)
                    .ThenInclude(ss => ss.Service)
                .Where(a => a.ServiceSchedule.Service.BusinessId == business.Id)
                .OrderByDescending(a => a.ServiceSchedule.StartDateTime)
                .ToListAsync();

            return View("~/Views/BusinessFuncts/BusinessAppointments.cshtml", appointments);
        }
    }
}