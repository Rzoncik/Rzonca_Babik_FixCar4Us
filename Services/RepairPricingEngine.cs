using System;
using System.Collections.Generic;

namespace Rzonca_Babik_FixCar4Us.Services
{
    // =========================================================================
    // CZĘŚĆ 1: Wzorzec STRATEGIA (Strategy)
    // Różne algorytmy naliczania kosztów pracy mechanika
    // =========================================================================

    public interface ILaborPricingStrategy
    {
        double CalculateLaborCost(double baseHourlyRate, double loggedHours, double standardNormHours, double flatRate);
    }

    // Strategia 1: Rozliczanie według czasu rzeczywistego (LoggedHours * Rate)
    public class RealTimePricingStrategy : ILaborPricingStrategy
    {
        public double CalculateLaborCost(double baseHourlyRate, double loggedHours, double standardNormHours, double flatRate)
        {
            return baseHourlyRate * loggedHours;
        }
    }

    // Strategia 2: Rozliczanie według norm producenta (NormHours * Rate)
    public class StandardNormsPricingStrategy : ILaborPricingStrategy
    {
        public double CalculateLaborCost(double baseHourlyRate, double loggedHours, double standardNormHours, double flatRate)
        {
            return baseHourlyRate * standardNormHours;
        }
    }

    // Strategia 3: Ryczałt za konkretną usługę (stała kwota)
    public class FlatRatePricingStrategy : ILaborPricingStrategy
    {
        public double CalculateLaborCost(double baseHourlyRate, double loggedHours, double standardNormHours, double flatRate)
        {
            return flatRate;
        }
    }


    // =========================================================================
    // CZĘŚĆ 2: Wzorzec DEKORATOR (Decorator)
    // Dynamiczne doliczanie opłat dodatkowych i rabatów bez modyfikacji bazy
    // =========================================================================

    // Wspólny interfejs dla kosztorysu
    public interface IRepairCost
    {
        double GetTotalCost();
        string GetDescription();
    }

    // Klasa bazowa reprezentująca podstawowy kosztorys (Części + Robocizna wyliczona Strategią)
    public class BaseRepairCost : IRepairCost
    {
        private readonly double _partsCost;
        private readonly double _laborCost;

        public BaseRepairCost(double partsCost, double laborCost)
        {
            _partsCost = partsCost;
            _laborCost = laborCost;
        }

        public double GetTotalCost()
        {
            return _partsCost + _laborCost;
        }

        public string GetDescription()
        {
            return $"Koszt bazowy (części: {_partsCost:C}, robocizna: {_laborCost:C})";
        }
    }

    // Klasa bazowa Dekoratora
    public abstract class RepairCostDecorator : IRepairCost
    {
        protected readonly IRepairCost _repairCost;

        public RepairCostDecorator(IRepairCost repairCost)
        {
            _repairCost = repairCost;
        }

        public virtual double GetTotalCost()
        {
            return _repairCost.GetTotalCost();
        }

        public virtual string GetDescription()
        {
            return _repairCost.GetDescription();
        }
    }

    // Konkretny Dekorator 1: Trudny dostęp do śrub (doliczenie stałej opłaty)
    public class DifficultAccessDecorator : RepairCostDecorator
    {
        private readonly double _extraFee;

        public DifficultAccessDecorator(IRepairCost repairCost, double extraFee = 150.0) 
            : base(repairCost)
        {
            _extraFee = extraFee;
        }

        public override double GetTotalCost()
        {
            return base.GetTotalCost() + _extraFee;
        }

        public override string GetDescription()
        {
            return base.GetDescription() + $"\n + Trudny dostęp do śrub (+{_extraFee:C})";
        }
    }

    // Konkretny Dekorator 2: Utylizacja płynów (np. stary olej)
    public class FluidDisposalDecorator : RepairCostDecorator
    {
        private readonly double _disposalFee;

        public FluidDisposalDecorator(IRepairCost repairCost, double disposalFee = 50.0) 
            : base(repairCost)
        {
            _disposalFee = disposalFee;
        }

        public override double GetTotalCost()
        {
            return base.GetTotalCost() + _disposalFee;
        }

        public override string GetDescription()
        {
            return base.GetDescription() + $"\n + Ekologiczna utylizacja płynów (+{_disposalFee:C})";
        }
    }

    // Konkretny Dekorator 3: Szybki termin ("Express") - doliczenie 20% do całości
    public class ExpressServiceDecorator : RepairCostDecorator
    {
        public ExpressServiceDecorator(IRepairCost repairCost) : base(repairCost)
        {
        }

        public override double GetTotalCost()
        {
            return base.GetTotalCost() * 1.20; // 20% drożej
        }

        public override string GetDescription()
        {
            return base.GetDescription() + "\n + Usługa ekspresowa (+20%)";
        }
    }

    // Konkretny Dekorator 4: Rabat dla klientów flotowych (np. minus 10% od całości)
    public class FleetDiscountDecorator : RepairCostDecorator
    {
        private readonly double _discountPercentage;

        public FleetDiscountDecorator(IRepairCost repairCost, double discountPercentage = 0.10) 
            : base(repairCost)
        {
            _discountPercentage = discountPercentage;
        }

        public override double GetTotalCost()
        {
            double total = base.GetTotalCost();
            return total - (total * _discountPercentage);
        }

        public override string GetDescription()
        {
            return base.GetDescription() + $"\n - Rabat flotowy (-{_discountPercentage * 100}%)";
        }
    }

    // =========================================================================
    // Fasada / Fabryka pomocnicza do obsługi Systemu Wyceny
    // =========================================================================
    public class RepairPricingEngine
    {
        // Ta metoda ułatwia studentowi demonstrację działania wzorców
        public IRepairCost CreatePricing(
            double partsTotal, 
            double baseHourlyRate, 
            double loggedHours, 
            double standardNorm, 
            double flatRate, 
            ILaborPricingStrategy pricingStrategy,
            List<string> activeDecorators)
        {
            // 1. Obliczenie kosztów robocizny z użyciem Strategii
            double laborTotal = pricingStrategy.CalculateLaborCost(baseHourlyRate, loggedHours, standardNorm, flatRate);

            // 2. Utworzenie obiektu bazowego
            IRepairCost finalCost = new BaseRepairCost(partsTotal, laborTotal);

            // 3. Dynamiczne dekorowanie wyceny (Dekorator)
            if (activeDecorators.Contains("DifficultAccess"))
            {
                finalCost = new DifficultAccessDecorator(finalCost, 100.0);
            }
            if (activeDecorators.Contains("FluidDisposal"))
            {
                finalCost = new FluidDisposalDecorator(finalCost);
            }
            if (activeDecorators.Contains("Express"))
            {
                finalCost = new ExpressServiceDecorator(finalCost);
            }
            if (activeDecorators.Contains("FleetDiscount"))
            {
                finalCost = new FleetDiscountDecorator(finalCost, 0.15); // 15% zniżki
            }
            
            var customFleetDiscount = activeDecorators.FirstOrDefault(d => d.StartsWith("FleetDiscount:"));
            if (customFleetDiscount != null)
            {
                if (double.TryParse(customFleetDiscount.Split(':')[1], System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out double parsedDiscount))
                {
                    finalCost = new FleetDiscountDecorator(finalCost, parsedDiscount);
                }
            }

            return finalCost;
        }
    }
}
