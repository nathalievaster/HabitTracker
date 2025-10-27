namespace HabitTracker.Domain.Models;

public class Session
{
    // Skapar ett unikt ID för varje session
    public Guid Id { get; init; } = Guid.NewGuid();
    // ID för vanan som denna session tillhör
    public Guid HabitId { get; init; }
    public DateTime StartTime { get; init; }
    public DateTime EndTime   { get; init; }

    public int DurationMinutes => (int)(EndTime - StartTime).TotalMinutes;
    // Om man vill anteckna något om sessionen, såsom "Kunde inte fokusera". Värdet kan vara null
    public string? Notes { get; set; }
}
