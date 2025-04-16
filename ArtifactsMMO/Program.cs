using System.Diagnostics.Eventing.Reader;
using Microsoft.VisualBasic;

namespace ArtifactsMMO_Controller
{
    public class Program
    {

        public static async Task Main(string[] args)
        {
            try
            {
                const string _accountName = "IamCloud";

                GameClient client = new GameClient();
                List<OriginalCharacter>? origChars = await client.GetCharacters(_accountName);

                var builder = WebApplication.CreateBuilder(args);
                var app = builder.Build();
                app.UseDefaultFiles();
                app.UseStaticFiles();

                app.MapGet("/characters", async () =>
                {
                    List<OriginalCharacter>? allChars = await client.GetCharacters(_accountName);
                    var sb = new System.Text.StringBuilder();
                    foreach (var c in allChars)
                    {
                        bool isLooping = client.isCharInLoop(origChars, c);
                        sb.AppendLine("<div class='character'>");
                        sb.AppendLine($@"
                                        <h2>{c.Name}</h2>
                                        <progress max='{c.MaxHp}' value='{c.Hp}'></progress>
                                        <p><strong>Looping?</strong> {isLooping}</p>
                                        <p><strong>Combat lvl:</strong> {c.Level}</p>
                                        <p><strong>XP:</strong> {c.Xp} / {c.MaxXp}</p>
                                        <p><strong>Gold:</strong> {c.Gold}</p>                                        
                                        <p><strong>Coords:</strong> ({c.X}, {c.Y})</p>
                                        <p><strong>Woodcutting:</strong> Lv. {c.WoodcuttingLevel}</p>
                                        <p><strong>Alchemy:</strong> Lv. {c.AlchemyLevel}</p>
                                        <p><strong>Fishing:</strong> Lv. {c.FishingLevel}</p>
                                        <p><strong>Mining:</strong> Lv. {c.MiningLevel}</p>
                                    ");
                        if (isLooping)
                        {
                            sb.AppendLine($@"<div>
                                            <button hx-post='/perform-action' hx-swap='none' hx-vals='{{ ""character-select"": ""{c.Name}"", ""action-type"": ""{ActionType.Cancel}"" }}'>Cancel loop</button>
                                        </div>");
                        }

                        sb.AppendLine("</div>");
                    }
                    var html = sb.ToString();


                    return Results.Text(html, "text/html");
                });

                app.MapGet("/character-select", async () =>
                {
                    string html = string.Join("\n", origChars.Select(c => $@"
                    <option value='{c.Name}'>{c.Name}</option>
                    "));
                    html += $"\n<option value='All'>All</option>";
                    return Results.Text(html, "text/html");
                });

                app.MapPost("/perform-action", async (HttpRequest req) =>
                {
                    var form = await req.ReadFormAsync();
                    string characterName = form["character-select"];
                    string actionType = form["action-type"];
                    string gatheringType = form["gathering-type"];
                    bool isLoop = form["loop"] == "on";

                    if (characterName == "All")
                    {
                        foreach (OriginalCharacter character in origChars)
                        {
                            if (actionType == ActionType.Cancel)
                            {
                                character.LoopCancelTokenSource.Cancel();
                                Csl.WriteLine($"Cancelling loop for {character.Name}");
                            }
                            else
                            {
                                if (isLoop)
                                {
                                    if (!client.isCharInLoop(origChars, character))
                                    {
                                        Loop(client, actionType, character, gatheringType);
                                    }
                                    else
                                    {
                                        Csl.WriteError($"Loop already running for {character.Name}");
                                    }
                                }
                                else
                                {
                                    ActionResponse? response = await client.PerformAction(character.Name, actionType, null);
                                    if (response != null)
                                    {
                                        client.LogConsoleActionResponse(response);
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        if (actionType == ActionType.Cancel)
                        {
                            OriginalCharacter character = origChars.First(c => c.Name == characterName);
                            character.LoopCancelTokenSource.Cancel();
                            Csl.WriteLine($"Cancelling loop for {character.Name}");
                        }
                        else
                        {
                            if (isLoop)
                            {

                                OriginalCharacter character = origChars.First(c => c.Name == characterName);
                                if (!client.isCharInLoop(origChars, character))
                                {
                                    Loop(client, actionType, character, gatheringType);
                                }
                                else
                                {
                                    Csl.WriteError($"Loop already running for {character.Name}");
                                }
                            }
                            else
                            {
                                ActionResponse? response = await client.PerformAction(characterName, actionType, null);
                                if (response != null)
                                {
                                    client.LogConsoleActionResponse(response);
                                    return Results.Ok();
                                }
                            }

                        }
                    }

                    return Results.Ok();
                });

                var serverTask = app.RunAsync();


                Console.ForegroundColor = ConsoleColor.Gray;
                Console.BackgroundColor = ConsoleColor.Black;
                if (!client.IsNetworkAvailable())
                {
                    Csl.WriteError("Error: No network connection available.  Please check your internet connection.");
                    Console.WriteLine("Press any key to exit.");
                    Console.ReadKey();
                    return;
                }





                Csl.WriteLine($"Welcome {_accountName}! You have {origChars.Count} characters.");
                Csl.WriteLine($"-------------------------------------------");

                //StartInitialLoop(client, origChars);


                while (true)
                {
                    string? command = Console.ReadLine();
                    string[] commandSplit = command.Split(" ");

                    if (commandSplit[0] == "exit")
                    {
                        Csl.WriteLine("Cancelling loops...");
                        origChars.ForEach(c => c.LoopCancelTokenSource.Cancel());
                        Csl.WriteLine("Loops canceled.");
                        Csl.WriteLine("Goodbye!");
                        await app.StopAsync();
                        return;
                    }
                    else if (command == "BankAll")
                    {
                        foreach (Character character in origChars)
                        {
                            await client.BankAllItems(character.Name);
                        }
                    }
                    else if (commandSplit[0] == "act")
                    {
                        string action = commandSplit[1];
                        string charName = commandSplit[2];
                        string? data = String.Empty;
                        if (commandSplit.Length > 3)
                        {
                            data = commandSplit[3];
                        }
                        if (charName == "all")
                        {
                            foreach (Character character in origChars)
                            {
                                ActionResponse? response = await client.PerformAction(character.Name, action, data);
                                if (response != null)
                                {
                                    client.LogConsoleActionResponse(response);
                                }
                            }
                        }
                        else
                        {
                            ActionResponse response = await client.PerformAction(charName, action, data);
                            if (response != null)
                            {
                                client.LogConsoleActionResponse(response);
                            }
                        }

                    }
                    else if (commandSplit[0] == "loop")
                    {
                        string action = commandSplit[1];
                        string charName = commandSplit[2];

                        OriginalCharacter origChar = origChars.First(c => c.Name == charName);
                        Loop(client, action, origChar, null);

                    }
                }
            }
            catch (HttpRequestException ex)
            {
                Csl.WriteError($"Error: {ex.Message}");
            }
            catch (System.Text.Json.JsonException ex)
            {
                Csl.WriteError($"JSON Error: {ex.Message}");
            }
            catch (Exception ex)
            {
                Csl.WriteError($"An error occurred: {ex.Message}");
            }

        }

        private static async void StartInitialLoop(GameClient client, List<OriginalCharacter> origChars)
        {
            Csl.WriteLine("Starting loops...");


            foreach (OriginalCharacter origChar in origChars)
            {
                if (origChar.Name == "Chopper")
                {

                    Loop(client, "woodcut", origChar, GatheringType.Woodcutting);
                }
                else if (origChar.Name == "JayRemy")
                {
                    Loop(client, "alch", origChar, GatheringType.Alchemy);
                }
                else if (origChar.Name == "JulieNgov")
                {
                    Loop(client, "fish", origChar, GatheringType.Fishing);
                    //Loop(client, "alch", origChar, null);
                }
                else if (origChar.Name == "Vicent")
                {
                    Loop(client, "fight", origChar);
                }
                else if (origChar.Name == "Marquise")
                {
                    Loop(client, "mine", origChar, GatheringType.Mining);
                    //Loop(client, "mine", origChar, null);
                }
            }
        }

        private static async void Loop(GameClient client, string actionType, OriginalCharacter origChar, string gatheringType = null)
        {
            switch (actionType)
            {
                case ActionType.Gathering:
                    origChar.LoopCancelTokenSource = new CancellationTokenSource();

                    List<Resource> resources = await client.GetResources(gatheringType, client.GetCharGatherLvl(origChar, gatheringType));
                    resources.Sort((r1, r2) => r2.Level.CompareTo(r1.Level));
                    Resource highestLvlResource = resources[0];

                    List<Map> maps = await client.GetMaps(MapContentType.Resource, highestLvlResource.Code);
                    Dictionary<string, string> pos = new Dictionary<string, string>();
                    pos.Add("x", maps[0].X.ToString());
                    pos.Add("y", maps[0].Y.ToString());
                    LoopAction(client, origChar, ActionType.Gathering, $"x:{pos["x"]},y:{pos["y"]}");
                    break;
                case ActionType.Fight:
                    origChar.LoopCancelTokenSource = new CancellationTokenSource();
                    LoopAction(client, origChar, ActionType.Fight, "x:1,y:-2"); //Temporary coordinates
                    break;
                case ActionType.Cancel:
                    origChar.LoopCancelTokenSource.Cancel();
                    break;
                default:
                    break;
            }
        }

        private static void LoopAction(GameClient client, OriginalCharacter origCharacter, string actionType, string actionPosition)
        {
            _ = Task.Run(async () =>
            {
                Character startChar = await client.GetCharacter(origCharacter.Name);

                TimeSpan remainingCooldown = startChar.CooldownExpiration - DateTime.UtcNow;
                remainingCooldown = remainingCooldown.Add(new TimeSpan(0, 0, 0, 1));

                if (remainingCooldown.TotalMilliseconds > 0)
                {
                    Csl.WriteInfo($"Waiting {remainingCooldown.TotalSeconds} seconds for {origCharacter.Name} cooldown.");
                    await Task.Delay((int)remainingCooldown.TotalMilliseconds);
                }

                ActionResponse rest = await RestIfNeeded(client, origCharacter, startChar);
                if (rest != null)
                {
                    client.LogConsoleActionResponse(rest);
                    await Task.Delay(rest.Cooldown.RemainingSeconds * 1000);
                }
                ActionResponse bank = await BackAllItemsIfNeeded(client, origCharacter, actionPosition, startChar);
                if (bank != null)
                {
                    client.LogConsoleActionResponse(bank);
                    await Task.Delay(bank.Cooldown.RemainingSeconds * 1000);
                }
                // Do the move action once
                ActionResponse move = await MoveToActionPositionIfNeeded(client, origCharacter, actionPosition, startChar);
                if (move != null)
                {
                    client.LogConsoleActionResponse(move);
                    await Task.Delay(move.Cooldown.RemainingSeconds * 1000);
                }

                // Start infinite gathering
                while (true)
                {
                    try
                    {
                        if (origCharacter.LoopCancelTokenSource.IsCancellationRequested)
                        {
                            origCharacter.LoopCancelTokenSource = null;
                            Csl.WriteLine($"[{origCharacter.Name}] Loop cancelled.");
                            break;
                        }
                        ActionResponse actionResponse = await client.PerformAction(origCharacter.Name, actionType, "");
                        if (actionResponse != null)
                        {

                            client.LogConsoleActionResponse(actionResponse);
                            await Task.Delay(actionResponse.Cooldown.RemainingSeconds * 1000);

                            rest = await RestIfNeeded(client, origCharacter, actionResponse.Character);
                            if (rest != null)
                            {
                                client.LogConsoleActionResponse(rest);
                                await Task.Delay(rest.Cooldown.RemainingSeconds * 1000);
                            }

                            bank = await BackAllItemsIfNeeded(client, origCharacter, actionPosition, actionResponse.Character);
                            if (bank != null)
                            {
                                client.LogConsoleActionResponse(bank);
                                await Task.Delay(bank.Cooldown.RemainingSeconds * 1000);
                            }

                            move = await MoveToActionPositionIfNeeded(client, origCharacter, actionPosition, actionResponse.Character);
                            if (move != null)
                            {
                                client.LogConsoleActionResponse(move);
                                await Task.Delay(move.Cooldown.RemainingSeconds * 1000);
                            }
                        }
                        else
                        {
                            Csl.WriteError($"[{origCharacter.Name}] Main loop action failed. Waiting 5 seconds and trying again.");
                            await Task.Delay(5000);
                        }
                    }
                    catch (Exception ex)
                    {
                        Csl.WriteError($"[{origCharacter.Name}] Error: {ex.Message}");
                        await Task.Delay(5000);
                    }
                }
            });
        }

        private static async Task<ActionResponse>? RestIfNeeded(GameClient client, OriginalCharacter origCharacter, Character character)
        {
            if (character.Hp == character.MaxHp) return null;

            return await client.PerformAction(origCharacter.Name, "rest", "");
        }

        private static async Task<ActionResponse>? MoveToActionPositionIfNeeded(GameClient client, OriginalCharacter origCharacter, string actionPosition, Character character)
        {
            if (actionPosition == $"x:{character.X},y:{character.Y}") return null;

            Csl.WriteWarning($"[{origCharacter.Name}] at wrong position to do action, moving to correct position.");
            return await client.PerformAction(origCharacter.Name, ActionType.Move, actionPosition);
        }

        private static async Task<ActionResponse>? BackAllItemsIfNeeded(GameClient client, OriginalCharacter origCharacter, string actionPosition, Character character)
        {
            int inventoryCount = 0;
            character.Inventory.ForEach((InventoryItem item) => inventoryCount += item.Quantity);
            if (inventoryCount < character.InventoryMaxItems) return null;

            await client.BankAllItems(origCharacter.Name);
            return await client.PerformAction(origCharacter.Name, "move", actionPosition);

        }
    }
}
