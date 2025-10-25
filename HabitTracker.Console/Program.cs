using HabitTracker.Infrastructure;

class Program
{
    static void Main()
    {
        var store = new JsonStore(); // Skapar en instans av JsonStore för att hantera data

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
    static void CreateHabit(JsonStore store)
    {
        Console.Write("Namn: ");
        var name = Console.ReadLine() ?? "";
        // Kontrollera att namnet inte är tomt
        if (string.IsNullOrWhiteSpace(name))
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine("Namnet får inte vara tomt. Försök igen.");
        Console.ResetColor();
        return;
    }
        Console.Write("Mål/vecka (antal pomodoros): ");
        if (!int.TryParse(Console.ReadLine(), out var target))
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("Felaktigt tal.");
            Console.ResetColor();
            return;
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
    static void ListHabits(JsonStore store)
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
    static void LogPomodoro(JsonStore store)
    {
        Console.Write("Ange HabitId: ");
        if (!Guid.TryParse(Console.ReadLine(), out var habitId))
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("Felaktigt Id.");
            Console.ResetColor();
            return;
        }

        Console.Write("Minuter: ");
        var minutesText = Console.ReadLine();

        int minutes;

        // Om användaren inte skrev något alls, använd 25 och skriv ut info
        if (string.IsNullOrWhiteSpace(minutesText))
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("Ingen tid angiven. Använder 25 minuter som standard.");
            minutes = 25;
            Console.ResetColor();
        }
        // Om användaren skrev något som inte är ett giltigt tal
        else if (!int.TryParse(minutesText, out minutes))
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
    static void ShowMinutesThisWeek(JsonStore store)
    {
        var minutes = store.MinutesThisWeek();
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine($"Denna vecka: {minutes} minuter.");
        Console.ResetColor();
    }
    // --------Visa sessions för en vana--------
    static void ShowSessionsForHabit(JsonStore store)
    {
        Console.Write("Ange HabitId: ");
        if (!Guid.TryParse(Console.ReadLine(), out var habitId))
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("Felaktigt Id.");
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

        foreach (var s in sessions)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"{s.StartTime:g} -> {s.EndTime:t}  ({s.DurationMinutes} min)  {s.Notes}");
            Console.ResetColor();
        }
    }
    // --------Byt namn på vana--------
    static void RenameHabit(JsonStore store)
    {
        Console.Write("HabitId: ");
        if (!Guid.TryParse(Console.ReadLine(), out var id))
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("Felaktigt Id.");
            Console.ResetColor();
            return;
        }

        Console.Write("Nytt namn: ");
        var newName = Console.ReadLine() ?? "";

        // Kontrollera att namnet inte är tomt
        if (string.IsNullOrWhiteSpace(newName))
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("Namnet får inte vara tomt. Försök igen.");
            Console.ResetColor();
            return;
        }


        bool success = store.UpdateHabitName(id, newName);

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
    static void UpdateHabitTarget(JsonStore store)
    {
        Console.Write("HabitId: ");
        if (!Guid.TryParse(Console.ReadLine(), out var id))
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("Felaktigt Id.");
            Console.ResetColor();
            return;
        }

        Console.Write("Nytt mål/vecka: ");
        if (!int.TryParse(Console.ReadLine(), out var newTarget))
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("Felaktigt tal.");
            Console.ResetColor();
            return;
        }

        bool success = store.UpdateHabitTarget(id, newTarget);

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
    static void DeleteHabit(JsonStore store)
    {
        Console.Write("HabitId att ta bort: ");
        if (!Guid.TryParse(Console.ReadLine(), out var id))
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("Felaktigt Id.");
            Console.ResetColor();
            return;
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
