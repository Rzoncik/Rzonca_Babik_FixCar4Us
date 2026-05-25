using Microsoft.EntityFrameworkCore;
using Rzonca_Babik_FixCar4Us.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();

// Rejestracja połączenia z bazą SQLite
builder.Services.AddDbContext<Rzonca_Babik_FixCar4Us.Data.AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

// Rejestracja Mediatora (Orkiestratora Warsztatu)
builder.Services.AddScoped<IWorkshopMediator, WorkshopMediator>();

// Rejestracja Silnika Wyceny (Pricing Engine)
builder.Services.AddScoped<RepairPricingEngine>();

// Rejestracja Systemu Zarządzania Etapami i Cofania (Rollback Engine)
builder.Services.AddScoped<RepairRollbackEngine>();

// Rejestracja Wzorca Builder do budowy zleceń
builder.Services.AddTransient<IRepairOrderBuilder, RepairOrderBuilder>();
builder.Services.AddTransient<RepairOrderDirector>();

// Rejestracja Wzorca Facade dla Panelu Mechanika
builder.Services.AddScoped<IMechanicPanelFacade, MechanicPanelFacade>();

// Rejestracja Wzorca Observer (Powiadomienia)
builder.Services.AddScoped<IRepairOrderNotifier, RepairOrderNotifier>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();
app.MapRazorPages()
    .WithStaticAssets();

app.Run();