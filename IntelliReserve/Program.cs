<<<<<<< HEAD
=======
using IntelliReserve.Data;
using Microsoft.EntityFrameworkCore;

>>>>>>> 4370e17386555479c0ddd199dab0d915d825e24d
namespace IntelliReserve
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

<<<<<<< HEAD
=======
            // 💡 Conexión a PostgreSQL con EF Core
            builder.Services.AddDbContext<AppDbContext>(options =>
                options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

>>>>>>> 4370e17386555479c0ddd199dab0d915d825e24d
            // Add services to the container.
            builder.Services.AddControllersWithViews();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
<<<<<<< HEAD
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
=======
>>>>>>> 4370e17386555479c0ddd199dab0d915d825e24d
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthorization();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            app.Run();
        }
    }
}
