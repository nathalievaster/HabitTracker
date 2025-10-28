using System;

// Klass som hanterar input och output i konsolen
static class ConsoleIO
{
    public static string ReadNonEmpty(string prompt)
    {
        while (true)
        {
            Console.Write(prompt);
            var s = Console.ReadLine() ?? "";
            if (!string.IsNullOrWhiteSpace(s)) return s;
            WriteError("Fältet får inte vara tomt. Försök igen.");
        }
    }

    public static int ReadPositiveInt(string prompt)
    {
        while (true)
        {
            Console.Write(prompt);
            var s = Console.ReadLine();
            if (int.TryParse(s, out var n) && n > 0) return n;
            WriteError("Felaktigt tal. Ange ett positivt heltal.");
        }
    }

    public static int ReadIntOrDefault(string prompt, int @default, int min = 1)
    {
        Console.Write(prompt);
        var s = Console.ReadLine();
        if (string.IsNullOrWhiteSpace(s))
        {
            WriteWarn($"Ingen tid angiven. Använder {@default} som standard.");
            return @default;
        }
        if (int.TryParse(s, out var n) && n >= min) return n;
        WriteWarn($"Felaktigt tal. Använder {@default} som standard.");
        return @default;
    }

    public static Guid ReadGuid(string prompt)
    {
        while (true)
        {
            Console.Write(prompt);
            var s = Console.ReadLine();
            if (Guid.TryParse(s, out var id)) return id;
            WriteError("Felaktigt Id. Försök igen.");
        }
    }

    public static void WriteColor(ConsoleColor color, string text)
    {
        var old = Console.ForegroundColor;
        Console.ForegroundColor = color;
        Console.WriteLine(text);
        Console.ForegroundColor = old;
    }
    public static void WriteError(string text) => WriteColor(ConsoleColor.Red, text);
    public static void WriteWarn(string text) => WriteColor(ConsoleColor.Yellow, text);
    public static void WriteOk(string text) => WriteColor(ConsoleColor.Green, text);
    public static void WriteInfo(string text) => WriteColor(ConsoleColor.Cyan, text);
}
