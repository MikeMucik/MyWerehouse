# Seedery danych testowych

Seedery są składane przez wywołania metod, bez dziedziczenia.
Zachowują dane i identyfikatory dotychczasowego `TestDataSeeder`.

## Użycie

```csharp
using TestSupport.Seeders.Scenarios;

SeederForCategory.SeedDatabase(DbContext); // Categories only
SeederForIssue.SeedDatabase(DbContext);    // Issue, items, and related data
```

`TestDataSeeder.SeedDatabase(DbContext)` nadal przygotowuje pełny zestaw.
Nie trzeba zmieniać istniejących testów od razu.

## Organizacja

- `Tables` — seeder pojedynczej tabeli lub grupy powiązanej relacją 1:1.
- `Scenarios` — wybór tabel i kolejność ich przygotowania.
- `SeedIds` — dotychczasowe stałe identyfikatory GUID używane w relacjach.

Grupy 1:1:

- `ProductsSeeder`: Products, ProductDetails i Inventories.
- `PalletsSeeder`: Pallets i VirtualPallets.

Pozostałe seedery: Clients, Users, Categories, Locations, NumberCounters,
Addresses, Receipts, Issues, IssueItems, ProductsOnPallet, PickingTasks,
HistoryPallets, HistoryPalletDetails i ReversePickings.

`SeederForIssue` przygotowuje klientów, adresy, kategorie, produkty z danymi
1:1, lokalizacje, przyjęcia, wydanie, pozycje wydania i palety z zawartością.
Nie dodaje zadań kompletacji, zwrotów ani historii. To zestaw do odczytów
wydania; testy innych operacji mogą potrzebować dodatkowych tabel.

Seeder tabeli zapisuje zmiany, ale nie wywołuje automatycznie swoich zależności.
Nowy seeder scenariusza wywołuje potrzebne seedery w kolejności kluczy obcych.
Przykładowo `CategoriesSeeder` musi poprzedzać `ProductsSeeder`.
Kontekst powinien służyć wyłącznie do przygotowania danych, bez innych
oczekujących zmian.

Tak jak wcześniej, istniejące dane w danej tabeli powodują pominięcie jej
zasiania (`Any`). Ponowne uruchomienie kompletnego scenariusza nie dubluje
rekordów, ale seedery nie uzupełniają dowolnych częściowo wypełnionych tabel.
Można najpierw uruchomić scenariusz kategorii, a potem pełny zestaw.

`EnsureCreated` może również wstawić dane z konfiguracji modelu (np. licznik
palet). Seeder zachowuje istniejący licznik, zgodnie z poprzednim zachowaniem.

Testy SQLite sprawdzające składanie zestawów znajdują się w
`MyWerehouse.Infrastructure.Tests/Seeding/SeedersTests.cs`.
