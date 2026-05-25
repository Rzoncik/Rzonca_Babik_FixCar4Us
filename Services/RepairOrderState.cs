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
            // Po Przyjęciu, auto zawsze idzie do diagnostyki
            context.SetState(new InDiagnosticsState());
        }

        public void SkipToRepair(RepairOrderContext context)
        {
            // Jeśli klient wie, co jest zepsute, pomijamy diagnostykę
            context.SetState(new InRepairState());
        }

        public string GetStatusName() => "Przyjęte";
    }

    public class InDiagnosticsState : IRepairState
    {
        public void NextState(RepairOrderContext context)
        {
            // Po diagnostyce zazwyczaj musimy zamówić części
            context.SetState(new WaitingForPartsState());
        }

        public void SkipToRepair(RepairOrderContext context)
        {
            // Jeśli mamy części na stanie, omijamy oczekiwanie
            context.SetState(new InRepairState());
        }

        public string GetStatusName() => "W diagnostyce";
    }

    public class WaitingForPartsState : IRepairState
    {
        public void NextState(RepairOrderContext context)
        {
            // Gdy części przyjdą, ruszamy z naprawą
            context.SetState(new InRepairState());
        }

        public void SkipToRepair(RepairOrderContext context)
        {
            // Jesteśmy już w trakcie procesu, ta metoda tu nie ma sensu - ignorujemy
        }

        public string GetStatusName() => "Oczekiwanie na części";
    }

    public class InRepairState : IRepairState
    {
        public void NextState(RepairOrderContext context)
        {
            // Po naprawie auto jest gotowe
            context.SetState(new ReadyForPickupState());
            
            // Logika specyficzna dla przejścia: ustawiamy datę zakończenia
            context.Order.CompletedAt = DateTime.Now.ToString("yyyy-MM-dd HH:mm");
        }

        public void SkipToRepair(RepairOrderContext context) { } // ignoruj

        public string GetStatusName() => "W naprawie";
    }

    public class ReadyForPickupState : IRepairState
    {
        public void NextState(RepairOrderContext context)
        {
            // Jesteśmy w stanie końcowym, dalej się nie da
            // Moglibyśmy rzucić wyjątkiem lub zignorować
        }

        public void SkipToRepair(RepairOrderContext context) { }

        public string GetStatusName() => "Gotowe do odbioru";
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
                "W diagnostyce" => new InDiagnosticsState(),
                "Oczekiwanie na części" => new WaitingForPartsState(),
                "W naprawie" => new InRepairState(),
                "Gotowe do odbioru" => new ReadyForPickupState(),
                _ => new AcceptedState() // Domyślny startowy
            };
        }
    }
}
