using System;
using System.Collections.Generic;

namespace Rzonca_Babik_FixCar4Us.Services
{
    // Interfejs strategy
    public interface ILaborPricingStrategy
    {
        double CalculateLaborCost(double baseHourlyRate, double loggedHours, double standardNormHours, double flatRate);
    }

    // Rozliczanie według czasu rzeczywistego
    public class RealTimePricingStrategy : ILaborPricingStrategy
    {
        public double CalculateLaborCost(double baseHourlyRate, double loggedHours, double standardNormHours, double flatRate)
        {
            return baseHourlyRate * loggedHours;
        }
    }

    // Rozliczanie według norm producenta
    public class StandardNormsPricingStrategy : ILaborPricingStrategy
    {
        public double CalculateLaborCost(double baseHourlyRate, double loggedHours, double standardNormHours, double flatRate)
        {
            return baseHourlyRate * standardNormHours;
        }
    }

    // Kwota za konkretną usługę
    public class FlatRatePricingStrategy : ILaborPricingStrategy
    {
        public double CalculateLaborCost(double baseHourlyRate, double loggedHours, double standardNormHours, double flatRate)
        {
            return flatRate;
        }
    }


    // Wzorzec decorator
    public interface IRepairCost
    {
        double GetTotalCost();
        string GetDescription();
    }

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

    // Kwota dodatkowa za utrudnienia
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

    public class CustomFeeDecorator : RepairCostDecorator
    {
        private readonly double _extraFee;
        private readonly string _description;

        public CustomFeeDecorator(IRepairCost repairCost, double extraFee, string description)
            : base(repairCost)
        {
            _extraFee = extraFee;
            _description = description;
        }

        public override double GetTotalCost()
        {
            return base.GetTotalCost() + _extraFee;
        }

        public override string GetDescription()
        {
            return base.GetDescription() + $"\n + {_description} (+{_extraFee:C})";
        }
    }

    public class ExpressServiceDecorator : RepairCostDecorator
    {
        public ExpressServiceDecorator(IRepairCost repairCost) : base(repairCost)
        {
        }

        public override double GetTotalCost()
        {
            return base.GetTotalCost() * 1.20;
        }

        public override string GetDescription()
        {
            return base.GetDescription() + "\n + Usługa ekspresowa (+20%)";
        }
    }

    // Rabat dla klientów flotowych
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

    // Facade
    public class RepairPricingEngine
    {
        public IRepairCost CreatePricing(
            double partsTotal,
            double baseHourlyRate,
            double loggedHours,
            double standardNorm,
            double flatRate,
            ILaborPricingStrategy pricingStrategy,
            List<string> activeDecorators)
        {
            // Obliczenie kosztów robocizny
            double laborTotal = pricingStrategy.CalculateLaborCost(baseHourlyRate, loggedHours, standardNorm, flatRate);

            // Utworzenie obiektu bazowego
            IRepairCost finalCost = new BaseRepairCost(partsTotal, laborTotal);

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
                finalCost = new FleetDiscountDecorator(finalCost, 0.15);
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
