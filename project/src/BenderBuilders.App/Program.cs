using BenderBuilders.Services;
using ElectronNET.API;
using ElectronNET.API.Entities;
using SharpDataAccess.Migrations;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddBenderBuildersServices();

// Add this line to enable Electron.NET:
builder.UseElectron(args, () => ElectronAppReady(builder.Environment));

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
    name: "proposalsPaged",
    pattern: "Proposals/Index/{page:int}",
    defaults: new { controller = "Proposals", action = "Index" });

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();

static async Task ElectronAppReady(IWebHostEnvironment environment)
{
    var browserWindow = await Electron.WindowManager.CreateWindowAsync(new BrowserWindowOptions
    {
        Show = false,
        Title = "Bender Builders",
        // Enable Node integration so the renderer can use Electron's native
        // APIs (e.g., webContents.printToPDF for saving PDFs). The ElectronNET
        // C#<->JS bridge runs over socket.io in the main process and does not
        // depend on renderer Node integration.
        WebPreferences = new WebPreferences
        {
            NodeIntegration = false
        }
    });
    browserWindow.SetTitle("Bender Builders");
    browserWindow.OnReadyToShow += () =>
    {
        if (environment.IsDevelopment())
        {
            browserWindow.WebContents.OpenDevTools();
        }
        browserWindow.Show();
    };
}
