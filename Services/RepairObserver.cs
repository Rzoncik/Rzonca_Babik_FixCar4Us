using System;
using System.Collections.Generic;
using Rzonca_Babik_FixCar4Us.Models;

namespace Rzonca_Babik_FixCar4Us.Services
{
    // Interfejs observer
    public interface IRepairStatusObserver
    {
        void Update(Customer customer, RepairOrder order, string message);
    }

    // Powiadomienie email
    public class EmailNotificationObserver : IRepairStatusObserver
    {
        public void Update(Customer customer, RepairOrder order, string message)
        {
            if (string.IsNullOrEmpty(customer.Email)) return;

            Console.WriteLine($"\n[SYSTEM EMAIL] Wysłano e-mail do {customer.Email}");
            Console.WriteLine($"Temat: Twój pojazd (Zlecenie #{order.Id})");
            Console.WriteLine($"Treść: {message}\n");
        }
    }

    // Powiadomienie sms
    public class SmsNotificationObserver : IRepairStatusObserver
    {
        public void Update(Customer customer, RepairOrder order, string message)
        {
            if (customer.PhoneNumber == null || customer.PhoneNumber == 0) return;
            Console.WriteLine($"\n[SYSTEM SMS] Wysłano SMS na numer {customer.PhoneNumber}");
            Console.WriteLine($"Treść: FixCar4Us: {message}\n");
        }
    }

    public interface IRepairOrderNotifier
    {
        void Attach(IRepairStatusObserver observer);
        void Detach(IRepairStatusObserver observer);
        void NotifyAll(Customer customer, RepairOrder order, string message);
    }

    public class RepairOrderNotifier : IRepairOrderNotifier
    {
        private readonly List<IRepairStatusObserver> _observers = new List<IRepairStatusObserver>();

        public void Attach(IRepairStatusObserver observer)
        {
            if (!_observers.Contains(observer))
            {
                _observers.Add(observer);
            }
        }

        public void Detach(IRepairStatusObserver observer)
        {
            _observers.Remove(observer);
        }

        // Powiadamia wszystkich przypisanych obserwatorów o zmianie
        public void NotifyAll(Customer customer, RepairOrder order, string message)
        {
            foreach (var observer in _observers)
            {
                observer.Update(customer, order, message);
            }
        }
    }
}
