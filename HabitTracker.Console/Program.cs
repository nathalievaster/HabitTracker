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

    static void CreateHabit(JsonStore store)
    {
        Console.Write("Namn: ");
        var name = Console.ReadLine() ?? "";
        Console.Write("Mål/vecka (antal pomodoros): ");
        if (!int.TryParse(Console.ReadLine(), out var target))
        {
            Console.WriteLine("Felaktigt tal.");
            return;
        }

        try
        {
            var h = store.CreateHabit(name, target);
            Console.WriteLine($"Vana skapad: {h.Name} (Id={h.Id})");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Fel: {ex.Message}");
        }
    }

    static void ListHabits(JsonStore store)
    {
        var habits = store.GetHabits();
        if (habits.Count == 0)
        {
            Console.WriteLine("Inga vanor än.");
            return;
        }
        foreach (var h in habits)
            Console.WriteLine($"{h.Id} - {h.Name} (mål {h.TargetPerWeek}/v)");
    }

    static void LogPomodoro(JsonStore store)
    {
        Console.Write("Ange HabitId: ");
        if (!Guid.TryParse(Console.ReadLine(), out var habitId))
        {
            Console.WriteLine("Felaktigt Id.");
            return;
        }

        Console.Write("Minuter: ");
        var minutesText = Console.ReadLine();

        int minutes;

        // Om användaren inte skrev något alls, använd 25 och skriv ut info
        if (string.IsNullOrWhiteSpace(minutesText))
        {
            Console.WriteLine("Ingen tid angiven. Använder 25 minuter som standard.");
            minutes = 25;
        }
        // Om användaren skrev något som inte är ett giltigt tal
        else if (!int.TryParse(minutesText, out minutes))
        {
            Console.WriteLine("Felaktigt tal. Använder 25 minuter som standard.");
            minutes = 25;
        }

        Console.Write("Anteckning (valfritt): ");
        var notes = Console.ReadLine();
        if (string.IsNullOrWhiteSpace(notes)) notes = null;

        store.LogPomodoro(habitId, minutes, notes);
        Console.WriteLine($"Pomodoro loggad ({minutes} min).");
    }

    static void ShowMinutesThisWeek(JsonStore store)
    {
        var minutes = store.MinutesThisWeek();
        Console.WriteLine($"Denna vecka: {minutes} minuter.");
    }

    static void ShowSessionsForHabit(JsonStore store)
    {
        Console.Write("Ange HabitId: ");
        if (!Guid.TryParse(Console.ReadLine(), out var habitId))
        {
            Console.WriteLine("Felaktigt Id.");
            return;
        }

        var sessions = store.GetSessionsForHabit(habitId);
        if (sessions.Count == 0)
        {
            Console.WriteLine("Inga sessioner för denna vana.");
            return;
        }

        foreach (var s in sessions)
            Console.WriteLine($"{s.StartTime:g} -> {s.EndTime:t}  ({s.DurationMinutes} min)  {s.Notes}");
    }

    static void RenameHabit(JsonStore store)
    {
        Console.Write("HabitId: ");
        if (!Guid.TryParse(Console.ReadLine(), out var id))
        {
            Console.WriteLine("Felaktigt Id.");
            return;
        }

        Console.Write("Nytt namn: ");
        var newName = Console.ReadLine() ?? "";

        bool success = store.UpdateHabitName(id, newName);

        if (success)
        {
            Console.WriteLine("Namn uppdaterat.");
        }
        else
        {
            Console.WriteLine("Hittade inte vanan.");
        }
    }

    static void UpdateHabitTarget(JsonStore store)
    {
        Console.Write("HabitId: ");
        if (!Guid.TryParse(Console.ReadLine(), out var id))
        {
            Console.WriteLine("Felaktigt Id.");
            return;
        }

        Console.Write("Nytt mål/vecka: ");
        if (!int.TryParse(Console.ReadLine(), out var newTarget))
        {
            Console.WriteLine("Felaktigt tal.");
            return;
        }

        bool success = store.UpdateHabitTarget(id, newTarget);

        if (success)
        {
            Console.WriteLine("Mål uppdaterat.");
        }
        else
        {
            Console.WriteLine("Hittade inte vanan.");
        }
    }

    static void DeleteHabit(JsonStore store)
    {
        Console.Write("HabitId att ta bort: ");
        if (!Guid.TryParse(Console.ReadLine(), out var id))
        {
            Console.WriteLine("Felaktigt Id.");
            return;
        }

        bool success = store.DeleteHabit(id);

        if (success)
        {
            Console.WriteLine("Vana (och dess sessioner) borttagen.");
        }
        else
        {
            Console.WriteLine("Hittade inte vanan.");
        }
    }
}
