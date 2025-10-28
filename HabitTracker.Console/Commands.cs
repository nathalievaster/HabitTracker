using HabitTracker.Domain.Models;

static class Commands
{
    // 1) Skapa vana
    public static void CreateHabit(IDataStore store)
    {
        var name   = ConsoleIO.ReadNonEmpty("Namn: ");
        var target = ConsoleIO.ReadPositiveInt("Mål/vecka (antal pomodoros): ");

        try
        {
            store.CreateHabit(name, target);
            ConsoleIO.WriteOk("Vana skapad!");
        }
        catch (Exception ex)
        {
            ConsoleIO.WriteError($"Fel: {ex.Message}");
        }
    }

    // 2) Lista vanor
    public static void ListHabits(IDataStore store)
    {
        var habits = store.GetHabits();
        if (habits.Count == 0)
        {
            ConsoleIO.WriteWarn("Inga vanor än.");
            return;
        }

        ConsoleIO.WriteInfo("ID  —  Namn (mål/vecka)");
        foreach (var h in habits)
            Console.WriteLine($"{h.Id}  —  {h.Name} (mål {h.TargetPerWeek}/v)");
    }

    // 3) Logga pomodoro
    public static void LogPomodoro(IDataStore store)
    {
        var habitId = ConsoleIO.ReadGuid("Ange HabitId: ");
        var minutes = ConsoleIO.ReadIntOrDefault("Minuter: ", @default: 25, min: 1);

        Console.Write("Anteckning (valfritt): ");
        var notes = Console.ReadLine();
        if (string.IsNullOrWhiteSpace(notes)) notes = null;

        store.LogPomodoro(habitId, minutes, notes);
        ConsoleIO.WriteOk($"Pomodoro loggad ({minutes} min).");
    }

    // 4) Visa veckans minuter
    public static void ShowMinutesThisWeek(IDataStore store)
    {
        var minutes = store.MinutesThisWeek();
        ConsoleIO.WriteInfo($"Denna vecka: {minutes} minuter.");
    }

    // 5) Visa sessions för en vana
    public static void ShowSessionsForHabit(IDataStore store)
    {
        var habitId = ConsoleIO.ReadGuid("Ange HabitId: ");
        var habit = store.GetHabit(habitId);
        if (habit is null)
        {
            ConsoleIO.WriteError("Hittade inte vanan.");
            return;
        }

        var sessions = store.GetSessionsForHabit(habitId);
        if (sessions.Count == 0)
        {
            ConsoleIO.WriteWarn("Inga sessioner för denna vana.");
            return;
        }

        ConsoleIO.WriteInfo($"\nSessioner för: {habit.Name}");
        foreach (var s in sessions)
            Console.WriteLine($"{s.StartTime:g} -> {s.EndTime:t}  ({s.DurationMinutes} min)  {s.Notes}");
    }

    // 6) Byt namn på vana
    public static void RenameHabit(IDataStore store)
    {
        var habitId = ConsoleIO.ReadGuid("Ange HabitId: ");
        var newName = ConsoleIO.ReadNonEmpty("Nytt namn: ");

        if (store.UpdateHabitName(habitId, newName))
            ConsoleIO.WriteOk("Namn uppdaterat.");
        else
            ConsoleIO.WriteError("Hittade inte vanan.");
    }

    // 7) Ändra mål/vecka
    public static void UpdateHabitTarget(IDataStore store)
    {
        var habitId   = ConsoleIO.ReadGuid("Ange HabitId: ");
        var newTarget = ConsoleIO.ReadPositiveInt("Nytt mål/vecka: ");

        if (store.UpdateHabitTarget(habitId, newTarget))
            ConsoleIO.WriteOk("Mål uppdaterat.");
        else
            ConsoleIO.WriteError("Hittade inte vanan.");
    }

    // 8) Ta bort vana
    public static void DeleteHabit(IDataStore store)
    {
        var id = ConsoleIO.ReadGuid("HabitId att ta bort: ");
        if (store.DeleteHabit(id))
            ConsoleIO.WriteOk("Vana (och dess sessioner) borttagen.");
        else
            ConsoleIO.WriteError("Hittade inte vanan.");
    }
}
