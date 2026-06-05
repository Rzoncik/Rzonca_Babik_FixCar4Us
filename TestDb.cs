using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Rzonca_Babik_FixCar4Us.Data;
using Rzonca_Babik_FixCar4Us.Models;

public class TestDb
{
    public static void Main()
    {
        var services = new ServiceCollection();
        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlite("Data Source=database.db"));
        
        var provider = services.BuildServiceProvider();
        using var scope = provider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var part = context.Parts.FirstOrDefault();
        if (part != null)
        {
            Console.WriteLine($"Przed: Part {part.Id} - {part.Name}, StockQuantity: {part.StockQuantity}");
            part.StockQuantity -= 1;
            context.Parts.Update(part);
            context.SaveChanges();
            Console.WriteLine($"Zapisano. StockQuantity powinno byc mniejsze o 1.");
            
            var partAfter = context.Parts.AsNoTracking().FirstOrDefault(p => p.Id == part.Id);
            Console.WriteLine($"Po: Part {partAfter.Id} - {partAfter.Name}, StockQuantity: {partAfter.StockQuantity}");
        }
        else
        {
            Console.WriteLine("Brak czesci w bazie.");
        }
    }
}
