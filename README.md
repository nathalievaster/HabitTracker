# HabitTracker

HabitTracker är en enkel, terminalbaserad applikation skriven i **C# / .NET** för att logga studietid enligt **Pomodoro-principen**.  
Användaren kan skapa vanor (Habits), logga studietillfällen (Sessions) och se summerad studietid per vecka.

Applikationen är byggd för att vara lätt att förstå, köra och bygga vidare på.  
Kodbasen följer grundläggande OOP-principer och delar av **SOLID**, bl.a. genom abstraktion av datalagring via `IDataStore`.

---

## Funktioner

- Skapa vana  
- Lista vanor  
- Byt namn på vana  
- Ändra mål/vecka  
- Ta bort vana (inkl. sessioner)  
- Logga pomodoro (25 min standard)  
- Se sessioner för en vana  
- Visa antal loggade minuter innevarande vecka  
- Validering av GUID  
- Validering av heltal  

---

## Tekniker

- C# / .NET
- Konsolapplikation
- OOP
- SOLID (ex. DIP, SRP, OCP)
- Datamodeller: `Habit`, `Session`
- Lagring:
  - JSON (`JsonStore`)
  - SQLite (`SqliteStore` via ADO.NET)

`IDataStore` gör att lagringsmetoden kan bytas utan att övrig kod ändras

## Länk till github repo:
https://github.com/nathalievaster/HabitTracker.git

## För att starta projektet: 
dotnet build
dotnet run --project HabitTracker.Console