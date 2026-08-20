using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using Student_Management_System.Models;
using Student_Management_System.Security;
using Student_Management_System.Services;
using static System.Collections.Specialized.BitVector32;

namespace Student_Management_System
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            //Add DbContext with  connection string
            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("dbConn")).EnableSensitiveDataLogging());

            // Register DataSecurity provider
            builder.Services.AddSingleton<DataSecurityProvider>();

            // add authentication and session services
            builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie(o => o.LoginPath = "/Login/Login");

            builder.Services.AddSession(o =>
            {
                o.IdleTimeout = TimeSpan.FromMinutes(1);
                o.Cookie.HttpOnly = true;
            });

            // Add services to the container.
            builder.Services.AddControllersWithViews();

            // Register ApplicationDbContext with SQL Server
            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("dbConn")));

            var app = builder.Build();

            using (var scope = app.Services.CreateScope())
            {
                try
                {
                    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                    db.Database.ExecuteSqlRaw(@"
                        IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Users' AND COLUMN_NAME = 'Phone')
                        BEGIN
                            ALTER TABLE Users ADD Phone NVARCHAR(50) NULL;
                        END;
                        IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Users' AND COLUMN_NAME = 'Address')
                        BEGIN
                            ALTER TABLE Users ADD Address NVARCHAR(255) NULL;
                        END;
                        IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Users' AND COLUMN_NAME = 'ProfilePicture')
                        BEGIN
                            ALTER TABLE Users ADD ProfilePicture NVARCHAR(255) NULL;
                        END;
                    ");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"DB Migration Note: {ex.Message}");
                }
            }

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }


            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();

            app.UseAuthorization();

            app.UseSession();

            app.MapStaticAssets();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Login}/{action=Login}/{id?}")
                .WithStaticAssets();

            app.Run();
        }
    }
}
