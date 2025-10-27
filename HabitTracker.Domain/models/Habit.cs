namespace HabitTracker.Domain.Models;

public class Habit
{
    // Skapar ett unikt ID för varje vana
    public Guid Id { get; init; } = Guid.NewGuid();
    public string Name { get; set; } = "";
    public int TargetPerWeek { get; set; } // t.ex. 5 pomodoros
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;
    // Om vanan är arkiverad eller inte
    public bool IsArchived { get; set; }
}
