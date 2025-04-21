# IntelliReserve

> University project for the course *IT Applications – Electronic Media in Business & Commerce*

## 🧠 Description
**IntelliReserve** is a web-based booking platform designed for small businesses. It enables managing users, services, employees, appointments, payments, and reviews.  
The solution is built using **ASP.NET Core MVC**, **PostgreSQL**, and will incorporate **Tailwind CSS** and **ChatGPT** for future enhancements.

---

## ⚙️ Technologies used
- ✅ ASP.NET Core MVC (.NET 6)
- ✅ Entity Framework Core 6
- ✅ PostgreSQL
- 🚧 Tailwind CSS (coming soon)
- 🚧 ChatGPT API (AI assistant)

---

## 🗃️ Database structure (UML-based)
- `User`: platform login and roles
- `Business`: registered companies
- `Employee`: workers of each business
- `Service`: offerings from each business
- `ServiceSchedule`: available time slots
- `Appointment`: booked services
- `Payment`: linked to appointments
- `Review`: user feedback
- `Notification`, `Schedule`, `AppointmentHistory`, etc.

---

## ✅ Completed so far
- All domain models implemented (based on UML)
- Replaced all `Guid` IDs with `int` for compatibility
- Fixed foreign key relationships and circular dependencies
- PostgreSQL database connection established
- Initial and final clean migration applied successfully 💾

---

## 🚀 Next steps
- [ ] Implement CRUD for key entities (Users, Appointments, etc.)
- [ ] Develop UI with Tailwind CSS
- [ ] Add user authentication and role-based access
- [ ] Integrate ChatGPT assistant for smart bookings

---

📌 *Project maintained by PF with weekly commits & version control via GitHub*
