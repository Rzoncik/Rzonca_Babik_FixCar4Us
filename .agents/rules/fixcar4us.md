---
trigger: always_on
---

Program w C# z wykorzystaniem bazy SQLite oraz frontendem Razor. Pisz jak student na drugim roku informatyki dlatego wykorzystuj proste formuły. Nie pisz jak senior developer z 10 letnim stażem. Dostosuj się do poniższych wymagań.

Baza danych database.db znajduje sie w /home/rzoncik/Dokumenty/Projekty/Rzonca_Babik_FixCar4Us/Rzonca_Babik_FixCar4Us

-----------------------------------------------------------------------

- [System Zarządzania Warsztatem Samochodowym (FixCar4Us)]()
  
   Należy napisać w dowolnej technologii GUI system do obsługi warsztatu samochodowego (naprawy bieżące, blacharskie, diagnostyka) z warstwą zapisu danych. 
  
  ### [Funkcjonalność podstawowa:]()
  
- Baza pojazdów i historia serwisowa: Rejestracja samochodów (numer rejestracyjny, VIN, przebieg) wraz z pełną historią napraw, wymienionych części i wykonanych badań technicznych.
- Kartoteka klientów: Zarządzanie danymi właścicieli pojazdów (osoby prywatne, floty) oraz historia ich zgłoszeń.
- Katalog części i usług: Baza części zamiennych z cenami zakupu/sprzedaży oraz cennik roboczogodzin dla różnych typów prac (mechanika, elektryka, lakiernictwo).
- Zarządzanie magazynem: Rejestrowanie przychodów i rozchodów części, automatyczna aktualizacja stanów przy dodawaniu części do zlecenia naprawy.
- Kalendarz przyjęć: Planowanie terminów wizyt z przypisaniem auta do konkretnego stanowiska przyjęć. 
  
  ### [Funkcjonalność dodatkowa:]()
  
- Inteligentne Przypisywanie Zasobów Warsztatowych (Workshop Orchestrator). Naprawa wymaga jednoczesnej dostępności trzech elementów: mechanika o odpowiedniej specjalizacji, wolnego podnośnika oraz zestawu narzędzi specjalistycznych (np. komputera diagnostycznego). System musi uniemożliwić zaplanowanie naprawy silnika, jeśli wszystkie podnośniki o odpowiednim udźwigu są zajęte, nawet jeśli mechanik jest wolny. Należy użyć wzorca Mediator, który koordynuje dostępność pracowników, stanowisk i narzędzi, zapobiegając konfliktom w grafiku warsztatu.
- Dynamiczny System Wyceny Naprawy (Repair Pricing Engine). Ostateczny kosztorys jest generowany na podstawie: bazowej ceny roboczogodziny (zależnej od trudności prac), cen użytych części (oryginały vs zamienniki), marży zależnej od klienta (np. rabat dla stałych klientów flotowych) oraz nieprzewidzianych trudności napotkanych podczas prac. System musi pozwalać na elastyczne dodawanie kolejnych pozycji do kosztorysu w trakcie trwania naprawy bez psucia struktury danych. Implementacja wzorca Dekorator lub Strategia, gdzie każda dodatkowa usługa lub modyfikator ceny (np. "trudny dostęp do śrub") jest osobnym obiektem modyfikującym cenę końcową.
- Zarządzanie Etapami Naprawy z Funkcją Cofania (Repair Rollback & History). Proces naprawy składa się z etapów: Diagnostyka -> Zamawianie części -> Prace właściwe -> Kontrola jakości. System musi pozwalać na wycofanie etapu (np. gdy zamówiona część okazała się niepasująca) i automatyczne przywrócenie stanu magazynowego oraz korektę czasu pracy mechanika, zachowując ślad rewizyjny. Zastosowanie wzorca Polecenie (Command) w połączeniu z Memento, co pozwoli na bezpieczne cofanie zmian w zleceniu naprawy i pełną kontrolę nad historią zdarzeń przy aucie. 
  
  ### [Oczekiwane wzorce projektowe:]()
  
- Builder – tworzenie (składanie) skomplikowanego zlecenia naprawy, które zawiera listę usterek, wymagane części, przydzielonych mechaników i szacowany czas.
- Decorator – dynamiczne doliczanie opłat dodatkowych do zlecenia (np. utylizacja płynów, opłata za szybki termin, wypożyczenie auta zastępczego).
- Facade – ukrycie złożoności systemu pod prostym interfejsem "Panelu Mechanika", który za jednym kliknięciem pobiera części z magazynu, aktualizuje status zlecenia i loguje czas pracy.
- State – zarządzanie statusem zlecenia i pojazdu: Przyjęte -> W diagnostyce -> Oczekiwanie na części -> W naprawie -> Gotowe do odbioru.
- Observer – automatyczne powiadamianie klienta (np. e-mail) o zmianie statusu naprawy lub o konieczności akceptacji dodatkowych kosztów wykrytych podczas prac.
- Strategy – różne algorytmy naliczania kosztów pracy: rozliczanie według czasu rzeczywistego, według norm producenta lub ryczałt za konkretną usługę.