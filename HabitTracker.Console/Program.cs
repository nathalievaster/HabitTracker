using HabitTracker.Domain.Models;
using HabitTracker.Infrastructure;

class Program
{
    static void Main()
    {
        IDataStore store = new SqliteStore("habits.db");

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

            switch (Console.ReadLine())
            {
                case "1": Commands.CreateHabit(store); break;
                case "2": Commands.ListHabits(store); break;
                case "3": Commands.LogPomodoro(store); break;
                case "4": Commands.ShowMinutesThisWeek(store); break;
                case "5": Commands.ShowSessionsForHabit(store); break;
                case "6": Commands.RenameHabit(store); break;
                case "7": Commands.UpdateHabitTarget(store); break;
                case "8": Commands.DeleteHabit(store); break;
                case "0": return;
                default:  ConsoleIO.WriteError("Okänt val."); break;
            }
        }
    }
}
