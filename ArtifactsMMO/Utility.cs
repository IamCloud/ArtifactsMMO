
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

public static class Csl
{
    private static void WriteDateTime()
    {
        Console.Write(DateTime.Now.ToString("HH:mm:ss.ffff") + "| ");
    }
    internal static void Write(string v, ConsoleColor color = ConsoleColor.Gray, bool writeDateTime = false)
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
        Csl.WriteLine(v, ConsoleColor.Red);
    }

    internal static void WriteSuccess(string v)
    {
        Csl.WriteLine(v, ConsoleColor.Green);
    }

    internal static void WriteWarning(string v)
    {
        Csl.WriteLine(v, ConsoleColor.Yellow);
    }

    internal static void WriteInfo(string v)
    {
        Csl.WriteLine(v, ConsoleColor.Blue);
    }

    internal static void WriteJson(Object v)
    {
        Csl.WriteLine(JsonConvert.SerializeObject(
       v, Formatting.Indented,
       new Newtonsoft.Json.JsonConverter[] { new StringEnumConverter() }), ConsoleColor.Cyan);
    }
}