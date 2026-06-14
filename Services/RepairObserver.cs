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

            Console.WriteLine($"\n[SYSTEM EMAIL] Przygotowuję wysyłkę e-mail do {customer.Email}...");

            try
            {
                // Konfiguracja Mailtrap.io
                using (System.Net.Mail.SmtpClient client = new System.Net.Mail.SmtpClient("sandbox.smtp.mailtrap.io", 587))
                {
                    client.EnableSsl = true;
                    client.UseDefaultCredentials = false;
                    client.Credentials = new System.Net.NetworkCredential("25c01632302180", "dd1f3c0b681f6c");

                    System.Net.Mail.MailMessage mailMessage = new System.Net.Mail.MailMessage();
                    mailMessage.From = new System.Net.Mail.MailAddress("warsztat@fixcar4us.pl", "Warsztat FixCar4Us");
                    mailMessage.To.Add(customer.Email);
                    mailMessage.Subject = $"FixCar4Us - Zmiana statusu zlecenia #{order.Id}";

                    mailMessage.Body = $"Witaj {customer.FirstName},\n\n" +
                                       $"Twój pojazd ({order.Vehicle?.LicensePlate ?? "nieznany"}) " +
                                       $"zmienił status na: {order.Status}.\n\n" +
                                       $"Wiadomość z systemu:\n{message}\n\n" +
                                       $"Pozdrawiamy,\nZespół FixCar4Us";

                    client.Send(mailMessage);
                    Console.WriteLine($"[SYSTEM EMAIL] Pomyślnie wysłano e-mail przez Mailtrap do {customer.Email}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[BŁĄD EMAIL] Nie udało się wysłać wiadomości: {ex.Message}");
            }
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
