using Microsoft.AspNetCore.Http.Features;

public class ApiWrapper<T>
{
    public T? Data { get; set; }
}

public class Items
{
    public List<ItemData> Data { get; set; } = new();
    public int Total { get; set; }
    public int Page { get; set; }
    public int Size { get; set; }
    public int Pages { get; set; }
}

public class ItemData
{
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public int Level { get; set; }
    public string Type { get; set; } = string.Empty;
    public string Subtype { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public List<Effect> Effects { get; set; } = new();
    public Craft Craft { get; set; } = new();
    public bool Tradeable { get; set; }
}

public class Effect
{
    public string Code { get; set; } = string.Empty;
    public int Value { get; set; }
}

public class Craft
{
    public string Skill { get; set; } = string.Empty;
    public int Level { get; set; }
    public List<CraftItem> Items { get; set; } = new();
    public int Quantity { get; set; }
}

public class CraftItem
{
    public string Code { get; set; } = string.Empty;
    public int Quantity { get; set; }
}

public class ActionResponse
{
    public Cooldown Cooldown { get; set; } = new();
    public Character Character { get; set; } = new();

    public Fight? Fight { get; set; }

    public Destination? Destination { get; set; }

    public Details? Details { get; set; }

    public int? HpRestored { get; set; }
}

public class Details
{
    public int Xp { get; set; }
    public List<DetailsItems> Items { get; set; } = new();
}

public class DetailsItems 
{
    public string Code { get; set; } = string.Empty;
    public int Quantity { get; set; }
}

public class Fight
{
    public int Xp { get; set; }
    public int Gold { get; set; }
    public List<Drop> Drops { get; set; } = new();
    public int Turns { get; set; }
    public BlockedHits MonsterBlockedHits { get; set; } = new();
    public BlockedHits PlayerBlockedHits { get; set; } = new();
    public List<string> Logs { get; set; } = new();
    public string Result { get; set; } = "win";
}

public class Drop
{
    public string Code { get; set; } = string.Empty;
    public int Quantity { get; set; }
}

public class BlockedHits
{
    public int Fire { get; set; }
    public int Earth { get; set; }
    public int Water { get; set; }
    public int Air { get; set; }
    public int Total { get; set; }
}


public class Cooldown
{
    public int TotalSeconds { get; set; }
    public int RemainingSeconds { get; set; }
    public DateTime StartedAt { get; set; }
    public DateTime Expiration { get; set; }
    public string Reason { get; set; } = string.Empty;
}

public class Destination
{
    public string Name { get; set; } = string.Empty;
    public string Skin { get; set; } = string.Empty;
    public int X { get; set; }
    public int Y { get; set; }
    public DestinationContent Content { get; set; } = new();
}

public class DestinationContent
{
    public string Type { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
}

public class OriginalCharacters
{
    public List<OriginalCharacter> Data { get; set; } = new();
}
public class Character
{
    public string Name { get; set; } = string.Empty;
    public string Account { get; set; } = string.Empty;
    public string Skin { get; set; } = string.Empty;
    public int Level { get; set; }
    public int Xp { get; set; }
    public int MaxXp { get; set; }
    public int Gold { get; set; }
    public int Speed { get; set; }
    public int MiningLevel { get; set; }
    public int MiningXp { get; set; }
    public int MiningMaxXp { get; set; }
    public int WoodcuttingLevel { get; set; }
    public int WoodcuttingXp { get; set; }
    public int WoodcuttingMaxXp { get; set; }
    public int FishingLevel { get; set; }
    public int FishingXp { get; set; }
    public int FishingMaxXp { get; set; }
    public int WeaponcraftingLevel { get; set; }
    public int WeaponcraftingXp { get; set; }
    public int WeaponcraftingMaxXp { get; set; }
    public int GearcraftingLevel { get; set; }
    public int GearcraftingXp { get; set; }
    public int GearcraftingMaxXp { get; set; }
    public int JewelrycraftingLevel { get; set; }
    public int JewelrycraftingXp { get; set; }
    public int JewelrycraftingMaxXp { get; set; }
    public int CookingLevel { get; set; }
    public int CookingXp { get; set; }
    public int CookingMaxXp { get; set; }
    public int AlchemyLevel { get; set; }
    public int AlchemyXp { get; set; }
    public int AlchemyMaxXp { get; set; }
    public int Hp { get; set; }
    public int MaxHp { get; set; }
    public int Haste { get; set; }
    public int CriticalStrike { get; set; }
    public int Wisdom { get; set; }
    public int Prospecting { get; set; }
    public int AttackFire { get; set; }
    public int AttackEarth { get; set; }
    public int AttackWater { get; set; }
    public int AttackAir { get; set; }
    public int Dmg { get; set; }
    public int DmgFire { get; set; }
    public int DmgEarth { get; set; }
    public int DmgWater { get; set; }
    public int DmgAir { get; set; }
    public int ResFire { get; set; }
    public int ResEarth { get; set; }
    public int ResWater { get; set; }
    public int ResAir { get; set; }
    public int X { get; set; }
    public int Y { get; set; }
    public int Cooldown { get; set; }
    public DateTime CooldownExpiration { get; set; }
    public string WeaponSlot { get; set; } = string.Empty;
    public string RuneSlot { get; set; } = string.Empty;
    public string ShieldSlot { get; set; } = string.Empty;
    public string HelmetSlot { get; set; } = string.Empty;
    public string BodyArmorSlot { get; set; } = string.Empty;
    public string LegArmorSlot { get; set; } = string.Empty;
    public string BootsSlot { get; set; } = string.Empty;
    public string Ring1Slot { get; set; } = string.Empty;
    public string Ring2Slot { get; set; } = string.Empty;
    public string AmuletSlot { get; set; } = string.Empty;
    public string Artifact1Slot { get; set; } = string.Empty;
    public string Artifact2Slot { get; set; } = string.Empty;
    public string Artifact3Slot { get; set; } = string.Empty;
    public string Utility1Slot { get; set; } = string.Empty;
    public int Utility1SlotQuantity { get; set; }
    public string Utility2Slot { get; set; } = string.Empty;
    public int Utility2SlotQuantity { get; set; }
    public string BagSlot { get; set; } = string.Empty;
    public string Task { get; set; } = string.Empty;
    public string TaskType { get; set; } = string.Empty;
    public int TaskProgress { get; set; }
    public int TaskTotal { get; set; }
    public int InventoryMaxItems { get; set; }
    public List<InventoryItem> Inventory { get; set; } = new();
}

public class InventoryItem
{
    public int Slot { get; set; }
    public string Code { get; set; } = string.Empty;
    public int Quantity { get; set; }
}
