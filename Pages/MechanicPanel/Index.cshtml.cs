using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Rzonca_Babik_FixCar4Us.Data;
using Rzonca_Babik_FixCar4Us.Models;
using Rzonca_Babik_FixCar4Us.Services;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;

namespace Rzonca_Babik_FixCar4Us.Pages.MechanicPanel
{
    public class IndexModel : PageModel
    {
        private readonly AppDbContext _context;
        private readonly IMechanicPanelFacade _facade;

        public IndexModel(AppDbContext context, IMechanicPanelFacade facade)
        {
            _context = context;
            _facade = facade;
        }

        public IList<RepairOrder> ActiveOrders { get; set; } = default!;

        [BindProperty]
        public int SelectedOrderId { get; set; }
        
        [BindProperty]
        public int ServiceId { get; set; } // Zalogowana praca (np id dla "Wymiana oleju")
        
        [BindProperty]
        public double LoggedHours { get; set; }
        
        [BindProperty]
        public int PartIdToConsume { get; set; }
        
        [BindProperty]
        public int QuantityToConsume { get; set; }

        public async Task OnGetAsync()
        {
            await LoadActiveOrders();
        }

        public async Task<IActionResult> OnPostAdvanceStageAsync()
        {
            var order = await _context.RepairOrders.FindAsync(SelectedOrderId);
            if (order == null) return RedirectToPage();

            // Używamy wzorca State, by ustalić prawidłowy następny status
            var stateContext = new RepairOrderContext(order);
            stateContext.NextState();
            string newValidStatus = stateContext.GetStatusName();

            // Używamy Fasady, która za jednym zamachem: loguje godziny, zużywa części i zmienia status (z powiadomieniem Observerem!)
            var parts = new List<PartUsageDto>();
            if (PartIdToConsume > 0 && QuantityToConsume > 0)
            {
                var part = await _context.Parts.FindAsync(PartIdToConsume);
                if (part != null)
                {
                    parts.Add(new PartUsageDto 
                    { 
                        PartId = part.Id, 
                        Quantity = QuantityToConsume, 
                        CurrentPrice = part.SalePrice ?? 0 
                    });
                }
            }

            // Jeden strzał z Fasady zamiast wielkiego spaghetti
            _facade.LogWorkAndCompleteStage(order.Id, ServiceId, LoggedHours, parts, newValidStatus);

            return RedirectToPage();
        }

        private async Task LoadActiveOrders()
        {
            ActiveOrders = await _context.RepairOrders
                .Include(r => r.Vehicle)
                .Where(r => r.Status != "Gotowe do odbioru") // Pokazuj tylko niezakończone
                .ToListAsync();
        }
    }
}
