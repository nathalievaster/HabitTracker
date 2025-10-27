using System.Data;
using HabitTracker.Domain.Models;
using HabitTracker.Infrastructure;

class Program
{
    static void Main()
    {
        IDataStore store = new JsonStore(); // Skapar en instans av JsonStore för att hantera data

        while (true)
        {
            Console.WriteLine("\n=== HabitTracker ===");
            Console.WriteLine("1) Skapa vana");
            Console.WriteLine("2) Lista vanor");
            Console.WriteLine("3) Logga pomodoro (standard 25 min)");
            Console.WriteLine("4) Visa veckans minuter");
            Console.WriteLine("5) Visa sessions för en vana");
            Console.WriteLine("6) Byt namn på vana");
            Console.WriteLine("7) Ändra mål/vecka");
            Console.WriteLine("8) Ta bort vana");
            Console.WriteLine("0) Avsluta");
            Console.Write("> ");

            var choice = Console.ReadLine();

            switch (choice)
            {
                case "1":
                    CreateHabit(store);
                    break;

                case "2":
                    ListHabits(store);
                    break;

                case "3":
                    LogPomodoro(store);
                    break;

                case "4":
                    ShowMinutesThisWeek(store);
                    break;

                case "5":
                    ShowSessionsForHabit(store);
                    break;

                case "6":
                    RenameHabit(store);
                    break;

                case "7":
                    UpdateHabitTarget(store);
                    break;

                case "8":
                    DeleteHabit(store);
                    break;

                case "0":
                    return;

                default:
                    Console.WriteLine("Okänt val.");
                    break;
            }
        }
    }

    //--------Skapa vanor--------
    static void CreateHabit(IDataStore store)
    {
        string name = "";

        // Loop tills användaren skriver ett giltigt namn
        while (true)
        {
            Console.Write("Namn: ");
            name = Console.ReadLine() ?? "";

            if (!string.IsNullOrWhiteSpace(name))
                break; // namnet är giltigt, gå vidare

            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("Namnet får inte vara tomt. Försök igen.");
            Console.ResetColor();
        }

        int target;
        while (true)
        {
            Console.Write("Mål/vecka (antal pomodoros): ");
            var input = Console.ReadLine();
            if (int.TryParse(input, out target) && target > 0)
                break; // målet är giltigt, gå vidare
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("Felaktigt tal.");
            Console.ResetColor();
        }

        try
        {
            var h = store.CreateHabit(name, target);
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Vana skapad!");
            Console.ResetColor();
        }
        catch (Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"Fel: {ex.Message}");
            Console.ResetColor();
        }
    }
    // --------Lista vanor--------
    static void ListHabits(IDataStore store)
    {
        var habits = store.GetHabits();
        if (habits.Count == 0)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("Inga vanor än.");
            Console.ResetColor();
            return;
        }
        foreach (var h in habits)
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine($"{h.Id} - {h.Name} (mål {h.TargetPerWeek}/v)");
            Console.ResetColor();
        }
    }

    // --------Logga pomodoro--------
    static void LogPomodoro(IDataStore store)
    {
        Guid habitId;

        while (true)
        {
            Console.Write("Ange HabitId: ");
            var input = Console.ReadLine();

            if (Guid.TryParse(input, out habitId))
                break; // giltigt, gå vidare

            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("Felaktigt Id. Försök igen.");
            Console.ResetColor();
        }

        Console.Write("Minuter: ");
        var minutesText = Console.ReadLine();

        int minutes;

        if (string.IsNullOrWhiteSpace(minutesText))
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("Ingen tid angiven. Använder 25 minuter som standard.");
            minutes = 25;
            Console.ResetColor();
        }
        else if (int.TryParse(minutesText, out var parsed) && parsed > 0)
        {
            minutes = parsed; //giltigt tal
        }
        else
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("Felaktigt tal. Använder 25 minuter som standard.");
            minutes = 25;
            Console.ResetColor();
        }

        Console.Write("Anteckning (valfritt): ");
        var notes = Console.ReadLine();
        if (string.IsNullOrWhiteSpace(notes)) notes = null;

        store.LogPomodoro(habitId, minutes, notes);
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"Pomodoro loggad ({minutes} min).");
        Console.ResetColor();
    }
    // --------Visa veckans minuter--------
    static void ShowMinutesThisWeek(IDataStore store)
    {
        var minutes = store.MinutesThisWeek();
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine($"Denna vecka: {minutes} minuter.");
        Console.ResetColor();
    }
    // --------Visa sessions för en vana--------
    static void ShowSessionsForHabit(IDataStore store)
    {
        Guid habitId;

        while (true)
        {
            Console.Write("Ange HabitId: ");
            var input = Console.ReadLine();

            if (Guid.TryParse(input, out habitId))
                break; // giltigt ID, gå vidare

            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("Felaktigt Id. Försök igen.");
            Console.ResetColor();
        }

        // Hämtar vanan
        var habit = store.GetHabit(habitId);
        if (habit is null)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("Hittade inte vanan.");
            Console.ResetColor();
            return;
        }

        var sessions = store.GetSessionsForHabit(habitId);
        if (sessions.Count == 0)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("Inga sessioner för denna vana.");
            Console.ResetColor();
            return;
        }

        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine($"\nSessioner för: {habit.Name}");
        Console.ResetColor();

        foreach (var s in sessions)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"{s.StartTime:g} -> {s.EndTime:t}  ({s.DurationMinutes} min)  {s.Notes}");
            Console.ResetColor();
        }
    }
    // --------Byt namn på vana--------
    static void RenameHabit(IDataStore store)
    {
        Guid habitId;

        while (true)
        {
            Console.Write("Ange HabitId: ");
            var input = Console.ReadLine();

            if (Guid.TryParse(input, out habitId))
                break; // giltigt ID, gå vidare

            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("Felaktigt Id. Försök igen.");
            Console.ResetColor();
        }
        string newName = "";

        while (true)
        {
            Console.Write("Nytt namn: ");
            newName = Console.ReadLine() ?? "";

            if (!string.IsNullOrWhiteSpace(newName))
                break; // namnet är giltigt, gå vidare

            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("Namnet får inte vara tomt. Försök igen.");
            Console.ResetColor();
        }

        bool success = store.UpdateHabitName(habitId, newName);

        if (success)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Namn uppdaterat.");
            Console.ResetColor();
        }
        else
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("Hittade inte vanan.");
            Console.ResetColor();
        }
    }
    // --------Ändra mål/vecka--------
    static void UpdateHabitTarget(IDataStore store)
    {
        Guid habitId;

        while (true)
        {
            Console.Write("Ange HabitId: ");
            var input = Console.ReadLine();

            if (Guid.TryParse(input, out habitId))
                break; // giltigt ID, gå vidare

            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("Felaktigt Id. Försök igen.");
            Console.ResetColor();
        }

        int newTarget;

        while (true)
        {
            Console.Write("Nytt mål/vecka: ");
            var input = Console.ReadLine();

            // giltigt om det går att parsa och är större än 0
            if (int.TryParse(input, out newTarget) && newTarget > 0)
                break;

            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("Felaktigt tal. Ange ett positivt heltal.");
            Console.ResetColor();
        }

        bool success = store.UpdateHabitTarget(habitId, newTarget);

        if (success)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Mål uppdaterat.");
            Console.ResetColor();
        }
        else
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("Hittade inte vanan.");
            Console.ResetColor();
        }
    }
    // --------Ta bort vana--------
    static void DeleteHabit(IDataStore store)
    {
        Guid id;

        // Loop tills användaren skriver ett giltigt HabitId
        while (true)
        {
            Console.Write("HabitId att ta bort: ");
            var input = Console.ReadLine();

            if (Guid.TryParse(input, out id))
                break; // giltigt, fortsätt

            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("Felaktigt Id. Försök igen.");
            Console.ResetColor();
        }

        bool success = store.DeleteHabit(id);

        if (success)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Vana (och dess sessioner) borttagen.");
            Console.ResetColor();
        }
        else
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("Hittade inte vanan.");
            Console.ResetColor();
        }
    }

}
