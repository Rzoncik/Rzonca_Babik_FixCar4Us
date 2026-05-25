using System;
using System.Collections.Generic;
using System.Linq;
using Rzonca_Babik_FixCar4Us.Data;
using Rzonca_Babik_FixCar4Us.Models;

namespace Rzonca_Babik_FixCar4Us.Services
{
    // =========================================================================
    // WZORZEC MEMENTO (Pamiątka)
    // Przechowuje "migawkę" stanu zamówienia (status, stan magazynowy)
    // =========================================================================
    public class RepairOrderMemento
    {
        public string Status { get; private set; }

        // Słownik przechowujący IdCzęści -> ZapisanyStanMagazynowy
        public Dictionary<int, int> PartsStockSnapshot { get; private set; }

        public RepairOrderMemento(string status, Dictionary<int, int> partsSnapshot)
        {
            Status = status;
            // Tworzymy nową kopię słownika, aby nikt z zewnątrz nie mógł go zmienić
            PartsStockSnapshot = new Dictionary<int, int>(partsSnapshot);
        }
    }

    // =========================================================================
    // WZORZEC COMMAND (Polecenie)
    // Definiuje abstrakcję dla operacji na zleceniu (np. przejście etapu)
    // =========================================================================
    public interface IRepairCommand
    {
        void Execute();
        void Undo();
        string GetCommandName();
    }

    // Konkretne Polecenie - zmiana etapu naprawy (i pobranie części z magazynu)
    public class ChangeRepairStageCommand : IRepairCommand
    {
        private readonly AppDbContext _context;
        private readonly RepairOrder _order;
        private readonly string _newStatus;
        private readonly List<OrderPart> _partsUsedInStage;

        // Obiekt wzorca Memento przechowujący stan zlecenia przed wykonaniem polecenia
        private RepairOrderMemento? _memento;

        public ChangeRepairStageCommand(AppDbContext context, RepairOrder order, string newStatus, List<OrderPart> partsUsedInStage)
        {
            _context = context;
            _order = order;
            _newStatus = newStatus;
            _partsUsedInStage = partsUsedInStage ?? new List<OrderPart>();
        }

        public void Execute()
        {
            // 1. Zapisanie obecnego stanu (Tworzenie Pamiątki / Memento) przed zmianami
            var currentPartsSnapshot = new Dictionary<int, int>();
            foreach (var partUse in _partsUsedInStage)
            {
                if (partUse.PartId.HasValue)
                {
                    var part = _context.Parts.Find(partUse.PartId.Value);
                    if (part != null)
                    {
                        currentPartsSnapshot[part.Id] = part.StockQuantity ?? 0;

                        // Zmniejszenie ilości sztuk w magazynie o 1
                        part.StockQuantity -= 1;
                    }
                }
            }
            // Zapisujemy Memento z tym, co było PRZED zmianą
            _memento = new RepairOrderMemento(_order.Status ?? "Przyjęto na serwis", currentPartsSnapshot);

            // 2. Właściwa akcja Polecenia
            _order.Status = _newStatus;

            // 3. Dodanie wpisu do logów naprawy (ślad rewizyjny)
            var log = new RepairHistoryLog
            {
                RepairOrderId = _order.Id,
                StageAction = $"ZMIANA ETAPU na: {_newStatus}",
                Timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                SnapshotData = "Wykonano pomyślnie"
            };
            _context.RepairHistoryLogs.Add(log);

            // 4. Zapis w bazie (w prawdziwej aplikacji użylibyśmy transakcji)
            _context.SaveChanges();
        }

        public void Undo()
        {
            if (_memento == null) return; // brak stanu do przywrócenia

            // 1. Przywracanie dawnego statusu naprawy (Restore from Memento)
            _order.Status = _memento.Status;

            // 2. Przywracanie wcześniejszego stanu magazynowego z Pamiątki
            foreach (var kvp in _memento.PartsStockSnapshot)
            {
                int partId = kvp.Key;
                int oldQuantity = kvp.Value;

                var part = _context.Parts.Find(partId);
                if (part != null)
                {
                    // Wycofujemy pobranie części (przywracamy poprzednią liczbę sztuk na magazyn)
                    part.StockQuantity = oldQuantity;
                }
            }

            // 3. Dodanie wpisu informującego o WYCOFANIU etapu
            var log = new RepairHistoryLog
            {
                RepairOrderId = _order.Id,
                StageAction = $"WYCOFANIE ETAPU (Rollback). Powrót do: {_memento.Status}",
                Timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                SnapshotData = "Wycofano akcję z użyciem Memento"
            };
            _context.RepairHistoryLogs.Add(log);

            _context.SaveChanges();
        }

        public string GetCommandName()
        {
            return $"ChangeStage to: {_newStatus}";
        }
    }

    // =========================================================================
    // Zarządca (Invoker) - obsługuje uruchamianie i cofanie poleceń
    // =========================================================================
    public class RepairRollbackEngine
    {
        // Stos poleceń, by móc je cofać w odwrotnej kolejności
        private readonly Stack<IRepairCommand> _commandHistory = new Stack<IRepairCommand>();

        public void ExecuteCommand(IRepairCommand command)
        {
            command.Execute();
            _commandHistory.Push(command); // dodajemy do historii
        }

        public bool UndoLastCommand()
        {
            if (_commandHistory.Count > 0)
            {
                var lastCommand = _commandHistory.Pop();
                lastCommand.Undo();
                return true;
            }
            return false; // brak historii do wycofania
        }

        public int GetHistoryCount()
        {
            return _commandHistory.Count;
        }
    }
}
