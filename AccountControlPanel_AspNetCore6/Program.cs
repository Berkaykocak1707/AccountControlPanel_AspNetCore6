using AccountControlPanel_AspNetCore6.Infrastructure.Extensions;
using Business;
using Business.Contracts;

namespace AccountControlPanel_AspNetCore6
{
	public class Program
	{
		public static void Main(string[] args)
		{
			var builder = WebApplication.CreateBuilder(args);

			// Add services to the container.
			builder.Services.AddControllersWithViews();
            builder.Services.ConfigureServiceRegistration();
            builder.Services.ConfigureDbContext(builder.Configuration);
            builder.Services.ConfigureIdentity();
            builder.Services.ConfigureSession();
            builder.Services.ConfigureRouting();
            builder.Services.AddHttpClient();
            builder.Services.ConfigureServiceRegistration();
            builder.Services.ConfigureApplicationCookie();
            builder.Services.ConfigureMailSenderServices(builder.Configuration);
            builder.Services.ConfigureAuthentication(builder.Configuration);
            builder.Services.AddAutoMapper(typeof(Program));
            if (builder.Environment.IsDevelopment())
            {
                builder.Configuration.AddUserSecrets<Program>();
            }
            // Add services to the container.
            builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));

            var app = builder.Build();

			// Configure the HTTP request pipeline.
			if (!app.Environment.IsDevelopment())
			{
				app.UseExceptionHandler("/Home/Error");
				// The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
				app.UseHsts();
			}
            app.ConfigureAndCheckMigration();
            app.ConfigureDefaultAdminUser();
            app.UseHttpsRedirection();

			app.UseStaticFiles();
            app.UseRouting();

            app.UseSession();
            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapAreaControllerRoute(
                    name: "Admin",
                    areaName: "Admin",
                    pattern: "Admin/{controller=Dashboard}/{action=Index}/{id?}"
                );

                endpoints.MapControllerRoute("default", "{controller=Home}/{action=Index}/{id?}");
                
            });

            app.Run();
		}
	}
}