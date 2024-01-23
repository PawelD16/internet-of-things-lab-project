using System.Net;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MQTTnet;
using MQTTnet.Client;
using RemoteLight.Data;
using RemoteLight.MQTTServices;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

var connectionString = builder.Configuration.GetConnectionString("MyDb");

//builder.Services.AddHostedService<MqttBackgroundService>();
builder.Services.AddSingleton<MqttBackgroundService>();
builder.Services.AddHostedService(provider => provider.GetService<MqttBackgroundService>());

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

//MQTTInitializer mqttInit = new(connectionString);

builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = false)
    .AddEntityFrameworkStores<ApplicationDbContext>();
builder.Services.AddControllersWithViews();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
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

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapRazorPages();

var scopeFactory = app.Services.GetRequiredService<IServiceScopeFactory>();

using (var scope = scopeFactory.CreateScope())
{
    // var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();
    MyIdentityDataInitializer.SeedData(userManager);
}

ApplyMigrations(app);
app.Run();

ServicePointManager
	.ServerCertificateValidationCallback +=
	(sender, cert, chain, sslPolicyErrors) => true;

static void ApplyMigrations(IApplicationBuilder app)
{
	using var scope = app.ApplicationServices.GetRequiredService<IServiceScopeFactory>().CreateScope();
	using var ctx = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

	ctx.Database.Migrate();
}