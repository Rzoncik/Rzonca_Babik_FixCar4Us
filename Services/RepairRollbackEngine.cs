using System;
using System.Collections.Generic;
using System.Linq;
using Rzonca_Babik_FixCar4Us.Data;
using Rzonca_Babik_FixCar4Us.Models;

namespace Rzonca_Babik_FixCar4Us.Services
{
    //Wzorzec memento
    public class RepairOrderMemento
    {
        public string Status { get; private set; }

        public Dictionary<int, int> PartsStockSnapshot { get; private set; }

        public RepairOrderMemento(string status, Dictionary<int, int> partsSnapshot)
        {
            Status = status;
            PartsStockSnapshot = new Dictionary<int, int>(partsSnapshot);
        }
    }

    // Wzorzec command
    public interface IRepairCommand
    {
        void Execute();
        void Undo();
        string GetCommandName();
    }

    // Zmiana etapu naprawy i pobranie części z magazynu
    public class ChangeRepairStageCommand : IRepairCommand
    {
        private readonly AppDbContext _context;
        private readonly RepairOrder _order;
        private readonly string _newStatus;
        private readonly List<OrderPart> _partsUsedInStage;

        // Obiekt wzorca memento przechowujacy stan zlecenia przed wykonaniem polecenia
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
            // Zapisanie obecnego stanu
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
            // Zapisanie memento z tym co było przed zmiana
            _memento = new RepairOrderMemento(_order.Status ?? "Przyjęto na serwis", currentPartsSnapshot);

            _order.Status = _newStatus;

            // Dodanie wpisu do logów naprawy
            var log = new RepairHistoryLog
            {
                RepairOrderId = _order.Id,
                StageAction = $"ZMIANA ETAPU na: {_newStatus}",
                Timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                SnapshotData = "Wykonano pomyślnie"
            };
            _context.RepairHistoryLogs.Add(log);

            // Zapis w bazie
            _context.SaveChanges();
        }

        public void Undo()
        {
            if (_memento == null) return;

            // Przywracanie dawnego statusu naprawy z memento
            _order.Status = _memento.Status;

            // Przywracanie wcześniejszego stanu magazynowego z memento
            foreach (var kvp in _memento.PartsStockSnapshot)
            {
                int partId = kvp.Key;
                int oldQuantity = kvp.Value;

                var part = _context.Parts.Find(partId);
                if (part != null)
                {
                    // Wycofanie pobrania części
                    part.StockQuantity = oldQuantity;
                }
            }

            // Dodanie wpisu o wycofaniu etapu
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

    public class RepairRollbackEngine
    {
        private readonly Stack<IRepairCommand> _commandHistory = new Stack<IRepairCommand>();

        public void ExecuteCommand(IRepairCommand command)
        {
            command.Execute();
            _commandHistory.Push(command);
        }

        public bool UndoLastCommand()
        {
            if (_commandHistory.Count > 0)
            {
                var lastCommand = _commandHistory.Pop();
                lastCommand.Undo();
                return true;
            }
            return false;
        }

        public int GetHistoryCount()
        {
            return _commandHistory.Count;
        }
    }
}
