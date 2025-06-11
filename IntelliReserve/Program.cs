using IntelliReserve.Data;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using DotNetEnv; // Asegúrate de que tienes este namespace
using Microsoft.AspNetCore.Localization;
using System.Globalization;

namespace IntelliReserve
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Cargar las variables de entorno desde el archivo .env
            var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

            // Usar la variable de entorno CONNECTION_STRING para la conexión a PostgreSQL
            builder.Services.AddDbContext<AppDbContext>(options =>
                options.UseNpgsql(connectionString));


            //PRECIO DECIMAL CON ,
            builder.Services.Configure<RequestLocalizationOptions>(options =>
            {
                var supportedCultures = new[] { new CultureInfo("en-US") }; // Forzar el uso del punto como separador decimal
                options.DefaultRequestCulture = new RequestCulture("en-US"); // Establecer 'en-US' como cultura por defecto
                options.SupportedCultures = supportedCultures;
                options.SupportedUICultures = supportedCultures;
                options.ApplyCurrentCultureToResponseHeaders = true; // Aplicar esta cultura a las respuestas
            });
            // Opcional: Personalizar el mensaje de error de validación para números
            builder.Services.Configure<Microsoft.AspNetCore.Mvc.MvcOptions>(options =>
            {
                options.ModelBindingMessageProvider.SetValueMustBeANumberAccessor(
                    (name) => $"El campo '{name}' debe ser un número válido (usa punto como separador decimal).");
            });


            Console.WriteLine(Env.GetString("CONNECTION_STRING"));
            // Add services to the container.
            builder.Services.AddControllersWithViews();

            builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie(options =>
                {
                    options.LoginPath = "/User/Login";
                    options.LogoutPath = "/User/Logout";
                    options.AccessDeniedPath = "/User/AccessDenied";
                });

            var app = builder.Build();


            app.UseRequestLocalization();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllerRoute(
                name: "login",
                pattern: "login",
                defaults: new { controller = "User", action = "Login" });

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            app.Run();
        }
    }
}
