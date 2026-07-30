using BenderBuilders.Services;
using ElectronNET.API;
using ElectronNET.API.Entities;
using SharpDataAccess.Migrations;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddBenderBuildersServices();

// Add this line to enable Electron.NET:
builder.UseElectron(args, ElectronAppReady);

var app = builder.Build();

// Ensure the SQLite database schema is up to date on startup.
app.Services.GetRequiredService<IMigrator>().Migrate();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();

static async Task ElectronAppReady()
{
    var browserWindow = await Electron.WindowManager.CreateWindowAsync(new BrowserWindowOptions
    {
        Show = false,
        Title = "Bender Builders"
    });
    browserWindow.SetTitle("Bender Builders");
    browserWindow.OnReadyToShow += () =>
    {
        browserWindow.WebContents.OpenDevTools();
        browserWindow.Show();
    };
}