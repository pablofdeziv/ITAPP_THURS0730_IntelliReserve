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

        // POST: Appointment/Cancel (Método para cancelar una cita específica)
        // Reutilizamos el método Delete pero lo renombramos y ajustamos para ser más semántico con "Cancelar"
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancel(int id) // Recibe el ID de la cita a cancelar
        {
            var appointment = await _context.Appointments
                                            .Include(a => a.ServiceSchedule) // Incluimos ServiceSchedule para redirigir
                                            .FirstOrDefaultAsync(a => a.Id == id);

            if (appointment == null)
            {
                TempData["ErrorMessage"] = "No se encontró la cita a cancelar.";
                return NotFound();
            }

            // Opcional: Verificar si el usuario que intenta cancelar es el dueño de la cita
            var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdString) || !int.TryParse(userIdString, out int userId) || appointment.UserId != userId)
            {
                TempData["ErrorMessage"] = "No tienes permiso para cancelar esta cita.";
                return Forbid(); // 403 Forbidden
            }

            // Solo permitir cancelar citas futuras y confirmadas
            if (appointment.Status != AppointmentStatus.Confirmed || appointment.ServiceSchedule.StartDateTime <= DateTime.UtcNow)
            {
                TempData["ErrorMessage"] = "Esta cita no puede ser cancelada (ya pasó o no está confirmada).";
                return BadRequest();
            }

            appointment.Status = AppointmentStatus.Canceled;
            appointment.UserId = null; // Liberamos el slot
            // Opcional: _context.AppointmentHistories.Add(new AppointmentHistory { /* ... */ }); si tienes un historial

            try
            {
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Cita cancelada con éxito.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Ocurrió un error al cancelar la cita: {ex.Message}";
            }

            // Redirigir a la página de reservas del cliente
            return RedirectToAction("BookingsCustomer");
        }

        // GET: Appointment/BookingsCustomer
        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> BookingsCustomer(int page = 1, int pageSize = 10) // Añadimos paginación
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
            {
                // Si el usuario no está logueado, redirigir al login
                TempData["ErrorMessage"] = "Debes iniciar sesión para ver tus reservas.";
                return RedirectToAction("Login", "User");
            }

            int userId = int.Parse(userIdClaim.Value);

            // Consulta para obtener las citas del usuario, incluyendo todas las relaciones necesarias
            // Y filtrando para solo mostrar citas Futuras y Confirmadas.
            var query = _context.Appointments
                .Include(a => a.ServiceSchedule)
                    .ThenInclude(ss => ss.Service)
                        .ThenInclude(s => s.Business) // <--- ¡Importante! Incluimos la información del negocio
                .Where(a => a.UserId == userId &&
                            a.Status == AppointmentStatus.Confirmed // Solo citas confirmadas
                             && a.ServiceSchedule.StartDateTime > DateTime.UtcNow) // Solo citas futuras
                .OrderBy(a => a.ServiceSchedule.StartDateTime); // Ordenamos por fecha ascendente para ver las próximas primero

            // Aplicar paginación
            var totalItems = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);
            var appointments = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.PageSize = pageSize; // Pasamos pageSize a la vista por si acaso

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



        //metodo para confirmar la reserva de un Usuario
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> BookAppointment(int serviceScheduleId) // Nuevo nombre del método
        {
            var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdString) || !int.TryParse(userIdString, out int userId))
            {
                TempData["ErrorMessage"] = "No se pudo identificar al usuario para realizar la reserva. Por favor, inicie sesión de nuevo.";
                return RedirectToAction("Login", "User");
            }

            var serviceSchedule = await _context.ServiceSchedules
                                                .Include(ss => ss.Appointment)
                                                .FirstOrDefaultAsync(ss => ss.Id == serviceScheduleId);

            if (serviceSchedule == null)
            {
                TempData["ErrorMessage"] = "El turno seleccionado no existe.";
                return RedirectToAction("Index", "Home");
            }

            if (serviceSchedule.Appointment == null || (serviceSchedule.Appointment.UserId != null && serviceSchedule.Appointment.Status != AppointmentStatus.Canceled))
            {
                TempData["ErrorMessage"] = "This shift has already been booked or is unavailable..";
                return RedirectToAction("ServiceCalendar", "Service", new { serviceId = serviceSchedule.ServiceId });
            }

            serviceSchedule.Appointment.UserId = userId;
            serviceSchedule.Appointment.Status = AppointmentStatus.Confirmed;

            try
            {
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Your reservation has been successfully confirmed.";
            }
            catch (DbUpdateConcurrencyException)
            {
                TempData["ErrorMessage"] = "Sorry, your appointment has just been booked. Please choose another one.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"An error occurred while processing your reservation: {ex.Message}";
            }

            return RedirectToAction("ServiceCalendar", "ServiceSchedule", new { serviceId = serviceSchedule.ServiceId });

        }

        [HttpPost]
        [Authorize]
        public IActionResult CancelByBusiness(int appointmentId)
        {
            var appointment = _context.Appointments.Find(appointmentId);
            if (appointment == null)
                return NotFound();

            _context.Appointments.Remove(appointment);
            _context.SaveChanges();

            return RedirectToAction("MySchedule", "Business");
        }

    }
}