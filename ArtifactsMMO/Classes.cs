public class OriginalCharacter : Character
{
    public CancellationTokenSource? LoopCancelTokenSource { get; set; }
}

public static class ActionType
{
    public const string Gathering = "gathering";
    public const string Fight = "fight";
    public const string Move = "move";
}