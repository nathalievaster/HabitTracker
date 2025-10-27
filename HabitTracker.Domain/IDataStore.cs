namespace HabitTracker.Domain;

// Interface för hur datalagringen ska se ut
public interface IDataStore
{
    List<Habit> GetHabits();
    Habit? GetHabit(Guid id);
    Habit CreateHabit(string name, int targetPerWeek);
    bool DeleteHabit(Guid id);
    bool UpdateHabitName(Guid id, string newName);
    bool UpdateHabitTarget(Guid id, int newTarget);
    List<Session> GetSessionsForHabit(Guid habitId);
    int MinutesThisWeek();
    void LogPomodoro(Guid habitId, int minutes = 25, string? notes = null);
}