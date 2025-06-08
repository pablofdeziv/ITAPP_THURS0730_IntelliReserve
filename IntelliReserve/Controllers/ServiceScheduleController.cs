using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using IntelliReserve.Data;
using IntelliReserve.Models;
using IntelliReserve.Models.ViewModels;
using System.Diagnostics;
using System.Globalization;

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



        public async Task<IActionResult> ServiceCalendar(int serviceId)
        {
            var service = await _context.Services.FirstOrDefaultAsync(s => s.Id == serviceId);
            if (service == null) return NotFound();

            var viewModel = new ServiceCalendarViewModel
            {
                ServiceId = service.Id,
                ServiceName = service.Name,
                ServiceDurationMinutes = service.Duration
            };

            return View("~/Views/Calendar/ServiceCalendar.cshtml", viewModel);
        }




        [HttpGet("/api/services/availability/{serviceId}")]
        public async Task<IActionResult> GetAvailability(int serviceId, [FromQuery] string start, [FromQuery] string end)
        {
            // Parsear las fechas 'start' y 'end' de la URL (rango de vista del calendario)
            if (!DateTime.TryParse(start, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out DateTime calendarStartDateUtc) ||
                !DateTime.TryParse(end, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out DateTime calendarEndDateUtc))
            {
                return BadRequest("Invalid start or end date format. ExpectedYYYY-MM-ddTHH:mm:ssZ");
            }

            var service = await _context.Services
                .Include(s => s.Schedules) // Incluye los schedules existentes
                .Include(s => s.AvailableDays) // Incluye los días disponibles
                .FirstOrDefaultAsync(s => s.Id == serviceId);

            if (service == null)
                return NotFound();

            var result = new List<object>();

            var duration = TimeSpan.FromMinutes(service.Duration);

            // Extraer las horas diarias disponibles de las propiedades del servicio
            var dailyAvailableStartTime = service.AvailableFrom.ToLocalTime().TimeOfDay;
            var dailyAvailableEndTime = service.AvailableTo.ToLocalTime().TimeOfDay;

            // Extraer las fechas de inicio y fin globales del servicio
            var serviceOverallStartDate = service.AvailableFrom.ToLocalTime().Date;
            var serviceOverallEndDate = service.AvailableTo.ToLocalTime().Date;

            // 1. Determinar el rango de fechas para el bucle: la intersección
            var loopStartDate = Max(calendarStartDateUtc.ToLocalTime().Date, serviceOverallStartDate);
            var loopEndDate = Min(calendarEndDateUtc.ToLocalTime().Date, serviceOverallEndDate);

            if (loopStartDate > loopEndDate)
            {
                return Json(result);
            }

            // 2. Iterar a través de cada día en el rango `loopStartDate` a `loopEndDate`.
            for (var day = loopStartDate; day <= loopEndDate; day = day.AddDays(1))
            {
                // 3. Filtrar por AvailableDays (si están definidos para servicios recurrentes)
                if (service.AvailableDays != null && service.AvailableDays.Any())
                {
                    if (!service.AvailableDays.Any(ad => ad.DayOfWeek == day.DayOfWeek))
                    {
                        continue; // Saltar si este día de la semana no está disponible para el servicio
                    }
                }

                // 4. Generar slots para las horas disponibles del día
                for (var time = dailyAvailableStartTime; time + duration <= dailyAvailableEndTime; time += duration)
                {
                    var slotStartCombinedLocal = day + time;
                    var slotEndCombinedLocal = slotStartCombinedLocal + duration;

                    // Esta es la compensación de +2 horas que aplicaste en tu código
                    // Asegúrate de que esta compensación sea intencional y consistente con tu configuración de zona horaria.
                    var compensatedSlotStart = slotStartCombinedLocal.AddHours(2);
                    var compensatedSlotEnd = compensatedSlotStart + duration; // Corregido: duration, no slotEndCombinedLocal + duration

                    // Convertir slots compensados a UTC para FullCalendar
                    var slotStartUtc = compensatedSlotStart.ToUniversalTime();
                    var slotEndUtc = compensatedSlotEnd.ToUniversalTime();

                    // Aquí, el código original solo verificaba la existencia de un schedule,
                    // pero no tenía una lógica de "reservado" basada en propiedades de los modelos.
                    var existingSchedule = service.Schedules.FirstOrDefault(s =>
                        s.StartDateTime == slotStartUtc && s.EndDateTime == slotEndUtc);

                    int? scheduleId = existingSchedule?.Id; // Captura el ID del schedule si existe

                    // Por defecto, se muestran como "Disponible" y en verde, sin lógica de reserva.
                    result.Add(new
                    {
                        title = "Disponible",
                        start = slotStartUtc.ToString("yyyy-MM-ddTHH:mm:ssZ"),
                        end = slotEndUtc.ToString("yyyy-MM-ddTHH:mm:ssZ"),
                        color = "#28a745", // Siempre verde
                        id = scheduleId,
                        isReserved = false // Se asume falso si no hay lógica de reserva
                    });
                }
            }

            return Json(result);
        }


        // Helper functions
        private DateTime Max(DateTime d1, DateTime d2) => d1 > d2 ? d1 : d2;
        private DateTime Min(DateTime d1, DateTime d2) => d1 < d2 ? d1 : d2;

    }
}
