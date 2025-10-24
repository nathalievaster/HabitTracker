using System.Text.Json;
using HabitTracker.Domain;

namespace HabitTracker.Infrastructure;

public class JsonStore
{
    // Skapar variabler för filvägar till JSON-filerna (Readonly låser dem efter initiering)
    private readonly string _habitsPath;
    private readonly string _sessionsPath;
    // Inställningar för JSON-serialisering så den blir lättläst
    private readonly JsonSerializerOptions _opts = new() { WriteIndented = true };

    // Listor som håller vanor och sessioner i minnet
    private List<Habit> _habits = new();
    private List<Session> _sessions = new();

    // Konstruktor
    public JsonStore(string habitsPath = "habits.json", string sessionsPath = "sessions.json")
    {
        _habitsPath = habitsPath;
        _sessionsPath = sessionsPath;

        // Skapa filerna om de inte finns
        if (!File.Exists(_habitsPath)) File.WriteAllText(_habitsPath, "[]");
        if (!File.Exists(_sessionsPath)) File.WriteAllText(_sessionsPath, "[]");

        Load(); // Läs in data från filerna
    }

    //Ladda data från JSON-filerna
    private void Load()
    {
        _habits = JsonSerializer.Deserialize<List<Habit>>(File.ReadAllText(_habitsPath)) ?? new(); // Om null, skapa en ny lista
        _sessions = JsonSerializer.Deserialize<List<Session>>(File.ReadAllText(_sessionsPath)) ?? new(); // -||-
    }
    // Spara data till JSON-filerna
    private void Save()
    {
        File.WriteAllText(_habitsPath, JsonSerializer.Serialize(_habits, _opts));
        File.WriteAllText(_sessionsPath, JsonSerializer.Serialize(_sessions, _opts));
    }

    // Habits

    // Hämtar hela listan med vanor
    public List<Habit> GetHabits() => _habits;
    // Hämtar en specifik vana baserat på dess ID
    public Habit? GetHabit(Guid id)
    {
        foreach (var habit in _habits)
        {
            if (habit.Id == id)
                return habit;
        }
        // Om ingen vana hittas med det ID:t, returnera null
        Console.WriteLine("Ingen vana med det ID:t hittades.");
        return null;
    }

    // Skapar en ny vana och lägger till den i listan
    public Habit CreateHabit(string name, int targetPerWeek)
    {
        var habit = new Habit { Name = name, TargetPerWeek = targetPerWeek };
        _habits.Add(habit);
        Save(); // Anropar Save för att spara ändringen till fil
        return habit; // Returnerar den skapade vanan
    }

    // Ta bort en vana baserat på dess ID
    public bool DeleteHabit(Guid id)
    {
        var removed = _habits.RemoveAll(h => h.Id == id) > 0;
        if (removed)
        {
            _sessions.RemoveAll(s => s.HabitId == id);
            Save();
        }
        return removed;
    }

    // Uppdatera målet för en vana
    public bool UpdateHabitTarget(Guid id, int newTarget)
    {
        var h = GetHabit(id);
        if (h == null) return false;
        h.TargetPerWeek = newTarget;
        Save();
        return true;
    }
}
