using System.Globalization;
using HabitTracker.Domain.Models;
using Microsoft.Data.Sqlite;

namespace HabitTracker.Infrastructure;

public class SqliteStore : IDataStore
{
    // Använder readonly så att connection string är satt vid konstruktion och inte ändras
    private readonly string _connectionString;

    public SqliteStore(string databasePath = "habits.db")
    {
        _connectionString = $"Data Source={databasePath};Cache=Shared";
        using var con = OpenConnection();
        using var cmd = con.CreateCommand();
        // Skapa tabeller om de inte finns
        cmd.CommandText = @"
        PRAGMA foreign_keys=ON;
        
        CREATE TABLE IF NOT EXISTS Habits (
        Id TEXT PRIMARY KEY,
        Name TEXT NOT NULL,
        TargetPerWeek INTEGER NOT NULL,
        CreatedAt TEXT NOT NULL,
        IsArchived INTEGER NOT NULL
        );
        
        CREATE TABLE IF NOT EXISTS Sessions (
        Id TEXT PRIMARY KEY,
        HabitId TEXT NOT NULL REFERENCES Habits(Id) ON DELETE CASCADE,
        StartTime TEXT NOT NULL,
        EndTime TEXT NOT NULL,
        Notes TEXT NULL
        );
        
        CREATE INDEX IF NOT EXISTS IX_Sessions_HabitId_Start
        ON Sessions(HabitId, StartTime DESC);
        ";
        cmd.ExecuteNonQuery();
    }

    // Metoder för Habits

    public List<Habit> GetHabits()
    {
        var list = new List<Habit>();
        using var con = OpenConnection();
        using var cmd = con.CreateCommand();
        cmd.CommandText = "SELECT Id, Name, TargetPerWeek, CreatedAt, IsArchived FROM Habits ORDER BY CreatedAt DESC;";
        // ExecuteReader returnerar en DataReader som vi kan iterera över
        using var row = cmd.ExecuteReader();
        // Läser varje rad och lägger till i listan
        while (row.Read())
        {
            // Lägger till vana i listan
            list.Add(MapHabit(row));
        }
        return list;
    }

    public Habit? GetHabit(Guid id)
    {
        using var con = OpenConnection();
        using var cmd = con.CreateCommand();
        cmd.CommandText = "SELECT Id, Name, TargetPerWeek, CreatedAt, IsArchived FROM Habits WHERE Id = $id;";
        cmd.Parameters.AddWithValue("$id", id.ToString());
        using var row = cmd.ExecuteReader();
        // Om vi hittar en rad, lägg till den som en Habit och returnera
        if (row.Read())
        {
            return MapHabit(row);
        }
        else
        {
            return null;
        }

    }

    public Habit CreateHabit(string name, int targetPerWeek)
    {
        var h = new Habit
        {
            Name = name,
            TargetPerWeek = targetPerWeek,
            // Habit constructor sätter Id och CreatedAt
        };

        using var con = OpenConnection();
        using var cmd = con.CreateCommand();
        cmd.CommandText = @"
        INSERT INTO Habits (Id, Name, TargetPerWeek, CreatedAt, IsArchived)
        VALUES ($id, $name, $tpw, $created, $arch);";
        cmd.Parameters.AddWithValue("$id", h.Id.ToString());
        cmd.Parameters.AddWithValue("$name", h.Name);
        cmd.Parameters.AddWithValue("$tpw", h.TargetPerWeek);
        cmd.Parameters.AddWithValue("$created", h.CreatedAt.ToUniversalTime().ToString("o"));
        cmd.Parameters.AddWithValue("$arch", h.IsArchived ? 1 : 0);
        cmd.ExecuteNonQuery();

        return h;
    }

    public bool DeleteHabit(Guid id)
    {
        using var con = OpenConnection();
        using var transaction = con.BeginTransaction();
        using var cmd = con.CreateCommand();
        cmd.Transaction = transaction;
        // Tack vare FK ON DELETE CASCADE behöver vi bara ta bort raden i Habits.
        cmd.CommandText = "DELETE FROM Habits WHERE Id = $id;";
        cmd.Parameters.AddWithValue("$id", id.ToString());
        var rows = cmd.ExecuteNonQuery();
        transaction.Commit();
        return rows > 0;
    }

    public bool UpdateHabitName(Guid id, string newName)
    {
        using var con = OpenConnection();
        using var cmd = con.CreateCommand();
        cmd.CommandText = "UPDATE Habits SET Name = $name WHERE Id = $id;";
        cmd.Parameters.AddWithValue("$id", id.ToString());
        cmd.Parameters.AddWithValue("$name", newName);
        // Kollar hur om någon rad påverkades
        return cmd.ExecuteNonQuery() > 0;
    }

    public bool UpdateHabitTarget(Guid id, int newTarget)
    {
        using var con = OpenConnection();
        using var cmd = con.CreateCommand();
        cmd.CommandText = "UPDATE Habits SET TargetPerWeek = $tpw WHERE Id = $id;";
        cmd.Parameters.AddWithValue("$id", id.ToString());
        cmd.Parameters.AddWithValue("$tpw", newTarget);
        // Kollar hur om någon rad påverkades
        return cmd.ExecuteNonQuery() > 0;
    }

    //

    public void LogPomodoro(Guid habitId, int minutes = 25, string? notes = null)
    {
        var nowLocal = DateTime.Now;
        var start = nowLocal.AddMinutes(-minutes);
        var end = nowLocal;

        var s = new Session
        {
            HabitId = habitId,
            // Session construktor sätter Id
            // Sparar tider som UTC ISO-8601 i DB
        };

        using var con = OpenConnection();
        using var cmd = con.CreateCommand();
        cmd.CommandText = @"
        INSERT INTO Sessions (Id, HabitId, StartTime, EndTime, Notes)
        VALUES ($id, $habitId, $start, $end, $notes);";
        cmd.Parameters.AddWithValue("$id", s.Id.ToString());
        cmd.Parameters.AddWithValue("$habitId", habitId.ToString());
        cmd.Parameters.AddWithValue("$start", start.ToUniversalTime().ToString("o"));
        cmd.Parameters.AddWithValue("$end", end.ToUniversalTime().ToString("o"));
        cmd.Parameters.AddWithValue("$notes", (object?)notes ?? DBNull.Value);
        cmd.ExecuteNonQuery();
    }

    public List<Session> GetSessionsForHabit(Guid habitId)
    {
        var list = new List<Session>();
        using var con = OpenConnection();
        using var cmd = con.CreateCommand();
        cmd.CommandText = @"
        SELECT Id, HabitId, StartTime, EndTime, Notes
        FROM Sessions
        WHERE HabitId = $habitId
        ORDER BY StartTime DESC;";
        cmd.Parameters.AddWithValue("$habitId", habitId.ToString());
        using var row = cmd.ExecuteReader();
        // Läser varje rad och lägger till i listan
        while (row.Read())
        {
            list.Add(MapSession(row));
        }
        return list;
    }

    public int MinutesThisWeek()
    {
        var (start, end) = WeekBounds(DateTime.Today);
        // Sparar tider som UTC i databasen
        var startUtc = start.ToUniversalTime().ToString("o");
        var endUtc = end.ToUniversalTime().ToString("o");

        using var con = OpenConnection();
        using var cmd = con.CreateCommand();
        cmd.CommandText = @"
        SELECT StartTime, EndTime
        FROM Sessions
        WHERE StartTime >= $start AND EndTime < $end;";
        cmd.Parameters.AddWithValue("$start", startUtc);
        cmd.Parameters.AddWithValue("$end", endUtc);

        var total = 0;
        using var row = cmd.ExecuteReader();
        while (row.Read())
        {
            // Beräkna minuter för varje session
            var startTime = DateTime.Parse(row.GetString(0), null, DateTimeStyles.AdjustToUniversal | DateTimeStyles.AssumeUniversal);
            var endTime = DateTime.Parse(row.GetString(1), null, DateTimeStyles.AdjustToUniversal | DateTimeStyles.AssumeUniversal);
            total += (int)(endTime - startTime).TotalMinutes;
        }
        return total;
    }

    // Hjälpmetod för att öppna en SQLite-anslutning med foreign keys påslaget för DRY-principen
    private SqliteConnection OpenConnection()
    {
        var con = new SqliteConnection(_connectionString);
        con.Open();
        using var pragma = con.CreateCommand();
        pragma.CommandText = "PRAGMA foreign_keys=ON;";
        pragma.ExecuteNonQuery();
        return con;
    }

    // Hjälpmetod för att lägga till en Habit från en databasrad. Följer DRY-principen
    private static Habit MapHabit(SqliteDataReader r)
    {
        return new Habit
        {
            Id = Guid.Parse(r.GetString(0)),
            Name = r.GetString(1),
            TargetPerWeek = r.GetInt32(2),
            CreatedAt = DateTime.Parse(r.GetString(3), null, DateTimeStyles.AdjustToUniversal | DateTimeStyles.AssumeUniversal),
            IsArchived = r.GetInt32(4) != 0
        };
    }
    // Hjälpmetod för att lägga till en Session från en databasrad. Följer DRY-principen
    private static Session MapSession(SqliteDataReader r)
    {
        return new Session
        {
            Id = Guid.Parse(r.GetString(0)),
            HabitId = Guid.Parse(r.GetString(1)),
            StartTime = DateTime.Parse(r.GetString(2), null, DateTimeStyles.AdjustToUniversal | DateTimeStyles.AssumeUniversal),
            EndTime = DateTime.Parse(r.GetString(3), null, DateTimeStyles.AdjustToUniversal | DateTimeStyles.AssumeUniversal),
            Notes = r.IsDBNull(4) ? null : r.GetString(4)
        };
    }

    // Samma veckologik som i JsonStore (måndag–söndag)
    private static (DateTime start, DateTime end) WeekBounds(DateTime day)
    {
        var start = day.Date;
        while (start.DayOfWeek != DayOfWeek.Monday)
            start = start.AddDays(-1);
        var end = start.AddDays(7);
        return (start, end);
    }
}
