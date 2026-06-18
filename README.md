# FixCar4Us - System Zarządzania Warsztatem Samochodowym

FixCar4Us to aplikacja internetowa stworzona w technologii C# (ASP.NET Core Razor Pages) z wykorzystaniem bazy danych SQLite. System ma na celu kompleksową obsługę warsztatu samochodowego, automatyzację procesów zarządzania naprawami, magazynem oraz komunikację z klientem.



## Funkcjonalności podstawowe

1. Baza pojazdów i historia serwisowa
   Rejestracja i zarządzanie danymi samochodów (numer rejestracyjny, VIN, przebieg). Moduł przechowuje pełną historię przeprowadzonych napraw, wymienionych części oraz zrealizowanych usług dla każdego pojazdu.

2. Kartoteka klientów
   Zarządzanie informacjami o właścicielach pojazdów (zarówno osobach prywatnych, jak i klientach flotowych). System umożliwia wgląd w historię zgłoszeń przypisanych do konkretnego klienta.

3. Katalog części i usług
   Zintegrowana baza części zamiennych zawierająca ceny zakupu oraz sprzedaży. Obejmuje również cennik roboczogodzin zróżnicowany dla różnych typów prac warsztatowych.

4. Zarządzanie magazynem
   Moduł odpowiedzialny za ewidencję przychodów i rozchodów części. Zapewnia automatyczną aktualizację stanów magazynowych w momencie przypisania części do zlecenia naprawy oraz blokuje operacje w przypadku braków w asortymencie.

5. Kalendarz przyjęć
   System umożliwiający planowanie terminów wizyt, przyjmowanie rezerwacji od klientów oraz przypisywanie konkretnych pojazdów do dedykowanych stanowisk roboczych.



## Funkcjonalności dodatkowe

1. Inteligentne Przypisywanie Zasobów Warsztatowych (Workshop Orchestrator)
   Zaawansowany system koordynacji, który weryfikuje dostępność wszystkich wymaganych zasobów przed zaplanowaniem naprawy. System dba o to, aby naprawa została zainicjowana tylko wtedy, gdy jednocześnie dostępny jest mechanik o odpowiednich kwalifikacjach, wolne stanowisko (np. podnośnik o odpowiednim udźwigu) oraz specjalistyczny zestaw narzędzi. Zapobiega to konfliktom w harmonogramie.

2. Dynamiczny System Wyceny Naprawy (Repair Pricing Engine)
   Moduł generujący ostateczny kosztorys na podstawie wielu zmiennych. Koszt finalny uwzględnia bazową cenę roboczogodziny, cenę i rodzaj użytych części (oryginały względem zamienników), specyficzne zniżki klienta (np. rabat dla stałych klientów flotowych) oraz nieprzewidziane opłaty wynikające z trudności napotkanych podczas naprawy.

3. Zarządzanie Etapami Naprawy z Funkcją Cofania (Repair Rollback & History)
   Proces naprawy podzielony został na odrębne etapy: diagnostyka, zamawianie części, prace właściwe oraz kontrola jakości. Aplikacja pozwala na cofnięcie określonego etapu (np. w sytuacji zamówienia niewłaściwej części) z jednoczesnym automatycznym przywróceniem stanu magazynowego oraz korektą ewidencji czasu pracy mechanika. Zmiany te zachowują pełny ślad rewizyjny.



## Zastosowane wzorce projektowe

Architektura aplikacji opiera się na uznanych wzorcach projektowych, które zapewniają skalowalność, separację obowiązków oraz łatwość w utrzymaniu i rozwoju kodu.

- Builder
  Wykorzystany do tworzenia skomplikowanych obiektów zleceń naprawy. Pozwala na sekwencyjne i czytelne budowanie zlecenia, łącząc w jedną całość zgłoszone usterki, wymóg konkretnych części, przydział pracowników oraz estymację czasu trwania.

- Decorator
  Zastosowany w module wyceny do dynamicznego doliczania opłat. Umożliwia nakładanie wielu modyfikatorów na cenę bazową (np. opłata za utylizację płynów, tryb ekspresowy naprawy, trudny dostęp do śrub) w postaci niezależnych obiektów, bez ingerencji w strukturę wyceny.

- Facade
  Zastosowany w panelu pracownika. Interfejs ten ukrywa skomplikowaną logikę systemową (m.in. transakcje magazynowe, zmianę statusu, obliczanie marż), udostępniając mechanikowi zestaw prostych metod wywoływanych jednym kliknięciem interfejsu graficznego.

- State
  Zarządza płynnym cyklem życia zlecenia w warsztacie. Zastępuje rozbudowane instrukcje warunkowe, definiując ścisłe reguły przejść pomiędzy stanami takimi jak: "Przyjęte", "W diagnostyce", "Oczekiwanie na części", "W naprawie" i "Gotowe do odbioru". 

- Observer
  Realizuje mechanizm zdarzeniowy dla powiadomień. Przy zmianie statusu zlecenia, zarejestrowani obserwatorzy (EmailNotificationObserver) są wywoływani automatycznie, aby dostarczyć klientowi wiadomość e-mail dotyczącą postępów w naprawie jego pojazdu.

- Strategy
  Wykorzystany w komponencie naliczania kosztów pracy. Oddziela algorytmy od logiki biznesowej, co pozwala na proste przełączanie pomiędzy rozliczaniem naprawy na podstawie czasu rzeczywistego, a stałą kwotą za wykonaną usługę.

- Mediator
  Stanowi fundament systemu "Workshop Orchestrator". Centralizuje i obudowuje logikę współdzielenia zasobów w obrębie warsztatu. Koordynuje dostępność pracowników, stanowisk i narzędzi, rozwiązując problem rezerwacji i uniemożliwiając nadpisania w kalendarzu.

- Command oraz Memento
  Zestaw wzorców odpowiedzialnych za funkcję cofania etapów naprawy (Rollback). Każda akcja mechanika jest kapsułkowana w obiekcie Command, podczas gdy Memento wykonuje zapis migawki systemu (np. stanów magazynowych). Gwarantuje to bezpieczeństwo operacji wycofania i poprawność danych inwentaryzacyjnych.



## Środowisko i technologia

- Backend: C#, ASP.NET Core, Entity Framework Core
- Frontend: HTML5, CSS (z wykorzystaniem biblioteki Bootstrap), Razor Pages
- Baza danych: SQLite
- Komunikacja: System powiadomień e-mail oparty na mailtrap.io.



## Architektura systemu

Projekt został zaprojektowany jako aplikacja monolityczna oparta na wzorcu warstwowym.

Frontend został stworzony z wykorzystaniem ASP.NET Core Razor Pages i jest renderowany po stronie serwera, co zapewnia szybkość działania i proste zarządzanie stanem (pod warunkiem że serwer jest szybki).

Backend jest przetwarzany przez PageModels, Services oraz wzorce projektowe. Dzięki temu kod programu jest przejrzysty oraz skalowalny.

Na komunikacje z bazą danych pozwala Entity Framework Core. Aplikacja korzysta z lekkiej i prostej bazy danych SQLite. Program nie jest na tyle skomplikowany, więc wykorzystanie takiej bazy może nawet uprościć jej zarządzaniem.





<mark>**Hasła dla wszystkich pracowników: admin123**</mark>