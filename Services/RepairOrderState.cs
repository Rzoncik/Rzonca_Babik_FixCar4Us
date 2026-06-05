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
            // Po Przyjęciu auto idzie prosto do naprawy
            context.SetState(new InRepairState());
        }

        public void SkipToRepair(RepairOrderContext context) { }

        public string GetStatusName() => "Przyjęte";
    }

    public class InRepairState : IRepairState
    {
        public void NextState(RepairOrderContext context)
        {
            // Po naprawie auto jest zakończone
            context.SetState(new CompletedState());
            
            // Logika specyficzna dla przejścia: ustawiamy datę zakończenia
            context.Order.CompletedAt = DateTime.Now.ToString("yyyy-MM-dd HH:mm");
        }

        public void SkipToRepair(RepairOrderContext context) { } // ignoruj

        public string GetStatusName() => "W naprawie";
    }

    public class CompletedState : IRepairState
    {
        public void NextState(RepairOrderContext context) { }

        public void SkipToRepair(RepairOrderContext context) { }

        public string GetStatusName() => "Zakończone";
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
                "W naprawie" => new InRepairState(),
                "Zakończone" => new CompletedState(),
                _ => new AcceptedState() // Domyślny startowy
            };
        }
    }
}
