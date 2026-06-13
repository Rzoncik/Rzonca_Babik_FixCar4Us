using Microsoft.EntityFrameworkCore;
using Rzonca_Babik_FixCar4Us.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();

// Rejestracja połączenia z bazą SQLite
builder.Services.AddDbContext<Rzonca_Babik_FixCar4Us.Data.AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

// Rejestracja mediatora
builder.Services.AddScoped<IWorkshopMediator, WorkshopMediator>();

// Rejestracja pricing engine
builder.Services.AddScoped<RepairPricingEngine>();

// Rejestracja rollback engine
builder.Services.AddScoped<RepairRollbackEngine>();

// Rejestracja wzorca builder
builder.Services.AddTransient<IRepairOrderBuilder, RepairOrderBuilder>();
builder.Services.AddTransient<RepairOrderDirector>();

// Rejestracja wzorca facade
builder.Services.AddScoped<IMechanicPanelFacade, MechanicPanelFacade>();

// Rejestracja wzorca observer
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

app.Use(async (context, next) =>
{
    var path = context.Request.Path.Value ?? "";
    bool isAdminRoute = (path.StartsWith("/Admin", StringComparison.OrdinalIgnoreCase) && !path.StartsWith("/AdminLogin", StringComparison.OrdinalIgnoreCase)) ||
                        path.StartsWith("/Calendar", StringComparison.OrdinalIgnoreCase) ||
                        path.StartsWith("/Catalog", StringComparison.OrdinalIgnoreCase) ||
                        path.StartsWith("/Customers", StringComparison.OrdinalIgnoreCase) ||
                        path.StartsWith("/Inventory", StringComparison.OrdinalIgnoreCase) ||
                        path.StartsWith("/MechanicPanel", StringComparison.OrdinalIgnoreCase) ||
                        path.StartsWith("/RepairHistory", StringComparison.OrdinalIgnoreCase) ||
                        path.StartsWith("/Vehicles", StringComparison.OrdinalIgnoreCase);

    if (isAdminRoute)
    {
        if (!context.Request.Cookies.ContainsKey("LoggedEmployeeId"))
        {
            context.Response.Redirect("/AdminLogin");
            return;
        }
    }

    await next();
});

app.UseAuthorization();

app.MapStaticAssets();
app.MapRazorPages()
    .WithStaticAssets();

app.Run();