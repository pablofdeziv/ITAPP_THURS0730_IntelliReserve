# IntelliReserve🧠

> University project for the course *IT Applications – Electronic Media in Business & Commerce*

**IntelliReserve** is a web-based booking and scheduling platform tailored for small businesses. It supports user and employee management, service configuration, appointment scheduling, payments, and customer feedback.

---

## ⚙️ Technologies Used

- ✅ ASP.NET Core MVC (.NET 6)
- ✅ Entity Framework Core 6
- ✅ PostgreSQL
- 🌐 Razor Views
- 🚧 Tailwind CSS *(planned)*
- 🤖 ChatGPT API *(planned AI assistant)*

---

## 🗃️ Database Structure (UML-based)

- `User` – login and roles
- `Business` – company profiles
- `Employee` – staff members
- `Service` – services offered
- `ServiceSchedule` – time slots
- `Appointment` – bookings
- `Payment` – transaction records
- `Review` – user feedback
- `Notification`, `Schedule`, `AppointmentHistory`, etc.

---

## ✅ Completed So Far

- Domain model (based on UML)
- PostgreSQL database integration
- Migrations with clean schema setup
- Replaced `Guid` with `int` IDs for clarity
- Foreign key relationships fixed and optimized

---

## 🚀 Next Steps

- [ ] CRUD operations for key entities
- [ ] Authentication & role-based access
- [ ] Frontend with Tailwind CSS
- [ ] ChatGPT assistant integration

---

## 📂 Project Structure

```
IntelliReserve/
├── Controllers/
├── Models/
├── Views/
├── Migrations/
├── wwwroot/
├── appsettings.json
├── Program.cs
└── IntelliReserve.csproj
```

---

## ▶️ How to Run

1. Clone the repository:
   ```bash
   git clone https://github.com/yourusername/IntelliReserve.git
   cd IntelliReserve
   ```

2. Set up your PostgreSQL connection string in `appsettings.json`.

3. Apply database migrations:
   ```bash
   dotnet ef database update
   ```

4. Run the project:
   ```bash
   dotnet run
   ```

---

## 📝 License

This project is open-source under the **MIT License**.
