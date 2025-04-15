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

public class MapContentType
{
    public const string Monster = "monster";
    public const string Resource = "resource";
    public const string Workshop = "workshop";
    public const string Bank = "bank";
    public const string GrandExchange = "grand_exchange";
    public const string TaskMaster = "tasks_master";
    public const string Npc = "npc";
}

public class Skill {
    public const string Woodcutting = "woodcutting";
    public const string Mining = "mining";
    public const string Fishing = "fishing";
    public const string Alchemy = "alchemy";
}