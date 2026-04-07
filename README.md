Readme
MyWarehouse
Overview

Short:

Architectur:
Domain Drive Design
Clean Architecture - Onion
CQRS - Command Query Responsibility Segregation

Język:
C#
platformna:
.NET
ORM:
Entity Framework Core
API:
ASP.NET Core
baza danych:
Microsoft SQL Server
GIT - GITHUB
Testy:
DateBaseInMemory( Crud )
SQLiteInMemory( Handlers)


Założenia projektu:
Magazyn producenta lub działający w imieniu producenta, powinien przychodzić asortyment jedna partia, data - jedna paleta

Full;

MyWarehouse to backendowa aplikacja do zarządzania procesami magazynowymi

System WMS ma za zadanie organizować proces przepływu produktu przez magazyn, odpowiada za przyjecia asortymentu, przygotowaniu 
i wydaniu towaru, w tym zawiera się kompletacja(wybór policy jak dobierane są palety) jak i anulowanie zmówienia, jak i dekompletacja,
z naciskiem na wydanie towaru (Issue), picking oraz zarządzanie paletami. 

System wspiera pracę magazynu, w którym towary są składowane na paletach,
 wydawane w kartonach oraz kontrolowane pod kątem daty przydatności (BestBefore). 

Aplikacja została zaprojektowana w sposób umożliwiający precyzyjne planowanie pracy pickerów oraz bezpieczną alokację zasobów magazynowych.

Model uproszczony – emergency flow powoduje rozdzielenie logiki między VirtualPallet i PickingTask - dwa agregaty zmiast jednego, w innym wypadku
całkiem nowy flow dla emergency picking.

Key Business Concepts:

Issue – zlecenie wydania towaru (np. tworzone na podstawie e-maila od klienta)

Receipt - przyjęcie towaru, zestaw palet przyjętych w jednej dostawie

Pallet – fizyczna paleta magazynowa z określoną ilością kartonów

Picking – proces kompletacji towaru pod konkretne Issue

ReversePicking - proces dekompletacji, gdy kompletacja ukończona ale zlecenie Issue wycofane

BestBefore – data przydatności, brana pod uwagę przy alokacji palet

Pallet Movements – rejestr zmian lokalizacji palet oraz jej historia

HistoryReceipt, HistoryIssue, HistoryPicking, HistoryReversePicking - zapis historii operacji

Architecture"

Projekt oparty jest o Clean Architecture oraz CQRS:

rozdzielenie logiki domenowej od infrastruktury - w toku

jawne use-case’y (Command / Query)

brak logiki biznesowej w kontrolerach

Technologie

.NET 9

ASP.NET Core Web API

MediatR (CQRS)

EF Core

SQLite (In-Memory) – testy integracyjne
InMemoryDataBase - testy dla CRUD
Swagger / OpenAPI

Solution Structure(w toku)
src/
├─ MyWarehouse.Api
│  └─ Controllers, Swagger, Middleware
├─ MyWarehouse.Application
│  └─ Commands, Queries, Handlers, Results
├─ MyWarehouse.Domain
│  └─ Entities, ValueObjects, Domain Logic
├─ MyWarehouse.Infrastructure
│  └─ DbContext, Repositories, EF Core Config
└─ tests/
   └─ MyWarehouse.IntegrationTests

Project Responsibilities

Api – wystawia REST API, obsługuje HTTP, mapuje wyniki na odpowiedzi

Application – implementuje przypadki użycia (Issue, Picking, Allocation, ...)

Domain – model domenowy magazynu i reguły biznesowe

Infrastructure – dostęp do bazy danych i integracje techniczne

Tests – testy integracyjne bez mocków

Testy - jednostkowe

CQRS & MediatR
Commands

Służą do zmiany stanu systemu:

CreateIssueCommand

AddPalletsToIssueByProductCommand

ChangeLocationPalletCommand
...

Wywoływane przez:

await _mediator.Send(command);
Queries

Służą do odczytu danych:

GetListToPickingQuery

Notifications (Events) - na razie tylko historia i stock

Reprezentują zdarzenia domenowe:

np. ChangeStock

Wywoływane przez:

await _mediator.Publish(notification);
Picking & Planning

System umożliwia:

generowanie listy produktów do pickingu na dany dzień / zakres dat

agregację zapotrzebowania na produkty

//estymację obciążenia pracy (roboczogodziny, liczba pickerów) - plan

Logika ta realizowana jest w warstwie Application i opiera się na przypisanych(zadaniach PickingTask) paletach do Issue.

Database & Transactions

EF Core jako ORM

jawne transakcje (BeginTransactionAsync)

kontrola współbieżności przy alokacji palet

Testy integracyjne wykorzystują:

SQLite In-Memory

realne repozytoria (bez mocków)

Testy jednostkowe do rozbudowy

Testing

Projekt zawiera testy integracyjne dla kluczowych scenariuszy:

alokacja pełnych palet i reszty

brak wystarczającej ilości kartonów

filtrowanie po BestBefore

Uruchomienie testów:

dotnet test
Running Locally
Requirements

.NET 9 SDK

Start API
dotnet restore
dotnet run --project src/MyWarehouse.Api

Swagger dostępny pod:

/ swagger
Error Handling

wyjątki domenowe (np. brak zasobów) są mapowane na wyniki biznesowe

globalny middleware obsługuje wyjątki techniczne

kontrolery nie zawierają logiki domenowej

Status

Projekt w aktywnym rozwoju. Fokus:

stabilizacja modelu domenowego

rozbudowa planowania pickingu

inne polityki dobierania palet

dalsze testy integracyjne