
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

public static class Utils
{
    private static void WriteDateTime()
    {
        Console.Write(DateTime.Now.ToString("HH:mm:ss.ffff") + "| ");
    }
    internal static void Write(string v, ConsoleColor color = ConsoleColor.Gray, bool writeDateTime = true)
    {
        var original = Console.ForegroundColor;
        if (writeDateTime)
        {
            WriteDateTime();
        }
        Console.ForegroundColor = color;
        Console.Write(v);
        Console.ForegroundColor = original;
    }
    internal static void WriteLine(string v, ConsoleColor color = ConsoleColor.Gray, bool writeDateTime = true)
    {
        var original = Console.ForegroundColor;
                if (writeDateTime)
        {
            WriteDateTime();
        }
        Console.ForegroundColor = color;
        Console.WriteLine(v);
        Console.ForegroundColor = original;
    }
    internal static void WriteError(string v)
    {
        Utils.WriteLine(v, ConsoleColor.Red);
    }

    internal static void WriteSuccess(string v)
    {
        Utils.WriteLine(v, ConsoleColor.Green);
    }

    internal static void WriteWarning(string v)
    {
        Utils.WriteLine(v, ConsoleColor.Yellow);
    }

    internal static void WriteJson(Object v)
    {
        Utils.WriteLine(JsonConvert.SerializeObject(
       v, Formatting.Indented,
       new Newtonsoft.Json.JsonConverter[] { new StringEnumConverter() }), ConsoleColor.Cyan);
    }
}