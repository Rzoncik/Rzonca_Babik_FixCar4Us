using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Rzonca_Babik_FixCar4Us.Data;
using Rzonca_Babik_FixCar4Us.Models;

public class CheckDb
{
    public static void Main()
    {
        var services = new ServiceCollection();
        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlite("Data Source=database.db"));
        
        var provider = services.BuildServiceProvider();
        using var scope = provider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var parts = context.Parts.ToList();
        foreach(var p in parts) Console.WriteLine($"Part {p.Id}: {p.StockQuantity}");

        var orderParts = context.OrderParts.ToList();
        Console.WriteLine($"Total OrderParts: {orderParts.Count}");
        foreach(var op in orderParts) Console.WriteLine($"OrderPart {op.Id}: RepairOrderId={op.RepairOrderId}, PartId={op.PartId}, Qty={op.Quantity}");
    }
}
