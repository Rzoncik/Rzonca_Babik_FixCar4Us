using System;
using Rzonca_Babik_FixCar4Us.Models;

namespace Rzonca_Babik_FixCar4Us.Services
{
    // =========================================================================
    // INTERFEJS STANU
    // =========================================================================
    public interface IRepairState
    {
        // Metoda przechodząca do kolejnego, zdefiniowanego etapu
        void NextState(RepairOrderContext context);

        // Metoda powrotu do poprzedniego statusu (Rollback)
        void PreviousState(RepairOrderContext context);

        // Metoda ułatwiająca ewentualne ominięcie czekania na części
        void SkipToRepair(RepairOrderContext context);

        // Zwraca nazwę stanu czytelną dla bazy danych i człowieka
        string GetStatusName();
    }

    // =========================================================================
    // KONTEKST
    // Klasa "opakowująca" nasze Zlecenie Naprawy. Przechowuje aktualny stan 
    // i deleguje do niego zachowania.
    // =========================================================================
    public class RepairOrderContext
    {
        private IRepairState _currentState;
        public RepairOrder Order { get; private set; }

        public RepairOrderContext(RepairOrder order)
        {
            Order = order;
            // Inicjalizujemy odpowiedni obiekt stanu na podstawie tego, co jest w bazie
            _currentState = StateFactory.GetStateFromString(order.Status);
        }

        // Zmiana stanu zaktualizuje też obiekt biznesowy w bazie
        public void SetState(IRepairState state)
        {
            _currentState = state;
            Order.Status = _currentState.GetStatusName();
        }

        // Zlecenie "samo" decyduje wewnątrz stanu, do jakiego etapu ma przejść
        public void NextState()
        {
            _currentState.NextState(this);
        }

        public void PreviousState()
        {
            _currentState.PreviousState(this);
        }

        public void SkipToRepair()
        {
            _currentState.SkipToRepair(this);
        }

        public string GetStatusName()
        {
            return _currentState.GetStatusName();
        }
    }

    // =========================================================================
    // KONKRETNE STANY
    // =========================================================================

    public class AcceptedState : IRepairState
    {
        public void NextState(RepairOrderContext context)
        {
            context.SetState(new DiagnosticsState());
        }

        public void PreviousState(RepairOrderContext context) { } // Startowy, nie ma powrotu

        public void SkipToRepair(RepairOrderContext context) { }

        public string GetStatusName() => "Przyjęte";
    }

    public class DiagnosticsState : IRepairState
    {
        public void NextState(RepairOrderContext context)
        {
            context.SetState(new OrderingPartsState());
        }

        public void PreviousState(RepairOrderContext context)
        {
            context.SetState(new AcceptedState());
        }

        public void SkipToRepair(RepairOrderContext context) { }

        public string GetStatusName() => "Diagnostyka";
    }

    public class OrderingPartsState : IRepairState
    {
        public void NextState(RepairOrderContext context)
        {
            context.SetState(new InRepairState());
        }

        public void PreviousState(RepairOrderContext context)
        {
            context.SetState(new DiagnosticsState());
        }

        public void SkipToRepair(RepairOrderContext context)
        {
            context.SetState(new InRepairState());
        }

        public string GetStatusName() => "Zamawianie części";
    }

    public class InRepairState : IRepairState
    {
        public void NextState(RepairOrderContext context)
        {
            context.SetState(new DifficultyPricingState());
        }

        public void PreviousState(RepairOrderContext context)
        {
            context.SetState(new OrderingPartsState());
        }

        public void SkipToRepair(RepairOrderContext context) { } // ignoruj

        public string GetStatusName() => "W naprawie";
    }

    public class DifficultyPricingState : IRepairState
    {
        public void NextState(RepairOrderContext context)
        {
            context.SetState(new CompletedState());
            context.Order.CompletedAt = DateTime.Now.ToString("yyyy-MM-dd HH:mm");
        }

        public void PreviousState(RepairOrderContext context)
        {
            context.SetState(new InRepairState());
        }

        public void SkipToRepair(RepairOrderContext context) { }

        public string GetStatusName() => "Wycena dodatkowa";
    }

    public class CompletedState : IRepairState
    {
        public void NextState(RepairOrderContext context) { }

        public void PreviousState(RepairOrderContext context) { } // Zakończone, blokada powrotu

        public void SkipToRepair(RepairOrderContext context) { }

        public string GetStatusName() => "Zakończone";
    }

    public class PaidState : IRepairState
    {
        public void NextState(RepairOrderContext context) { }
        public void PreviousState(RepairOrderContext context) { }
        public void SkipToRepair(RepairOrderContext context) { }
        public string GetStatusName() => "Opłacone";
    }

    // =========================================================================
    // PROSTA FABRYKA STANÓW (Ułatwienie konwersji ze stringa z DB)
    // =========================================================================
    public static class StateFactory
    {
        public static IRepairState GetStateFromString(string? status)
        {
            return status switch
            {
                "Przyjęte" => new AcceptedState(),
                "Diagnostyka" => new DiagnosticsState(),
                "Zamawianie części" => new OrderingPartsState(),
                "W naprawie" => new InRepairState(),
                "Wycena dodatkowa" => new DifficultyPricingState(),
                "Zakończone" => new CompletedState(),
                "Opłacone" => new PaidState(),
                _ => new AcceptedState() // Domyślny startowy
            };
        }
    }
}
