

#nullable disable warnings


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

                Console.ForegroundColor = ConsoleColor.Gray;
                Console.BackgroundColor = ConsoleColor.Black;
                if (!client.IsNetworkAvailable())
                {
                    Csl.WriteError("Error: No network connection available.  Please check your internet connection.");
                    Console.WriteLine("Press any key to exit.");
                    Console.ReadKey();
                    return;
                }

                List<OriginalCharacter> origChars = await client.GetCharacters(_accountName);

                Csl.WriteLine($"Welcome {_accountName}! You have {origChars.Count} characters.");
                Csl.WriteLine($"-------------------------------------------");
                List<Map> maps = await client.GetMaps(MapContentType.Resource);
                Csl.WriteJson(maps.FindAll((m) => m.Content != null));
                List<Resource> resources = await client.GetResources();
                List<Resource> targets = resources.FindAll((r) => r.Skill == "mining" && r.Level < 20);
                targets.Sort((r1, r2) => r2.Level.CompareTo(r1.Level));
                Resource target = targets[0];

                //Utils.WriteJson(target);
                // Map targetMap = maps.First((map) => map.Content.Code == target.Code);

                //Utils.WriteJson(targetMap);

                StartInitialLoop(client, origChars);


                while (true)
                {
                    string command = Console.ReadLine();
                    string[] commandSplit = command.Split(" ");

                    if (commandSplit[0] == "exit")
                    {
                        Csl.WriteLine("Cancelling loops...");
                        origChars.ForEach(c => c.LoopCancelTokenSource.Cancel());
                        Csl.WriteLine("Loops canceled.");
                        Csl.WriteLine("Goodbye!");
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
                                ActionResponse response = await client.CallActionAsync(character.Name, action, data);
                                if (response != null)
                                {
                                    client.DisplayActionResponse(response);
                                }
                            }
                        }
                        else
                        {
                            ActionResponse response = await client.CallActionAsync(charName, action, data);
                            if (response != null)
                            {
                                client.DisplayActionResponse(response);
                            }
                        }

                    }
                    else if (commandSplit[0] == "loop")
                    {
                        string action = commandSplit[1];
                        string charName = commandSplit[2];
                        if (charName == "all")
                        {
                            foreach (OriginalCharacter origChar in origChars)
                            {
                                Loop(client, action, origChar);
                            }
                        }
                        else
                        {
                            OriginalCharacter origChar = origChars.First(c => c.Name == charName);
                            Loop(client, action, origChar);
                        }

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

        private static void StartInitialLoop(GameClient client, List<OriginalCharacter> origChars)
        {
            Csl.WriteLine("Starting loops...");
            foreach (OriginalCharacter origChar in origChars)
            {
                if (origChar.Name == "Chopper")
                {
                    Loop(client, "woodcut", origChar);
                }
                else if (origChar.Name == "JayRemy")
                {
                    Loop(client, "alch", origChar);
                }
                else if (origChar.Name == "JulieNgov")
                {
                    Loop(client, "alch", origChar);
                }
                else if (origChar.Name == "Vicent")
                {
                    Loop(client, "fight", origChar);
                }
                else if (origChar.Name == "Marquise")
                {
                    Loop(client, "mine", origChar);
                }
            }
        }

        private static void Loop(GameClient client, string action, OriginalCharacter origChar)
        {
            if (action == "cancel")
            {
                origChar.LoopCancelTokenSource.Cancel();
            }
            else if (action == "woodcut")
            {
                origChar.LoopCancelTokenSource = new CancellationTokenSource();
                LoopAction(client, origChar, ActionType.Gathering, "x:2,y:6");
            }
            else if (action == "mine")
            {
                origChar.LoopCancelTokenSource = new CancellationTokenSource();
                LoopAction(client, origChar, ActionType.Gathering, "x:1,y:7");
            }
            else if (action == "fish")
            {
                origChar.LoopCancelTokenSource = new CancellationTokenSource();
                LoopAction(client, origChar, ActionType.Gathering, "x:5,y:2");
            }
            else if (action == "alch")
            {
                origChar.LoopCancelTokenSource = new CancellationTokenSource();
                LoopAction(client, origChar, ActionType.Gathering, "x:2,y:2");
            }
            else if (action == "fight")
            {
                origChar.LoopCancelTokenSource = new CancellationTokenSource();
                LoopAction(client, origChar, ActionType.Fight, "x:1,y:-2");
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


                // Do the move action once
                string actionX = actionPosition.Split(",")[0].Split(":")[1];
                string actionY = actionPosition.Split(",")[1].Split(":")[1];
                if (startChar.X != int.Parse(actionX) || startChar.Y != int.Parse(actionY))
                {
                    ActionResponse moveResponse = await client.CallActionAsync(origCharacter.Name, ActionType.Move, actionPosition);
                    if (moveResponse != null)
                    {
                        client.DisplayActionResponse(moveResponse);
                        await Task.Delay(moveResponse.Cooldown.RemainingSeconds * 1000);
                    }
                }


                // Start infinite gathering
                while (true)
                {
                    try
                    {
                        if (origCharacter.LoopCancelTokenSource.IsCancellationRequested)
                        {
                            Csl.WriteLine($"[{origCharacter.Name}] Loop cancelled.");
                            break;
                        }
                        ActionResponse actionResponse = await client.CallActionAsync(origCharacter.Name, actionType, "");
                        if (actionResponse != null)
                        {
                            int inventoryCount = 0;
                            actionResponse.Character.Inventory.ForEach((InventoryItem item) => inventoryCount += item.Quantity);
                            client.DisplayActionResponse(actionResponse);
                            await Task.Delay(actionResponse.Cooldown.RemainingSeconds * 1000);

                            if (actionResponse.Character.Hp < actionResponse.Character.MaxHp)
                            {
                                ActionResponse restResponse = await client.CallActionAsync(origCharacter.Name, "rest", "");
                                if (restResponse != null)
                                {
                                    client.DisplayActionResponse(restResponse);
                                    await Task.Delay(restResponse.Cooldown.RemainingSeconds * 1000);
                                }
                            }
                            if (inventoryCount == actionResponse.Character.InventoryMaxItems)
                            {
                                await client.BankAllItems(origCharacter.Name);
                                ActionResponse moveResponse = await client.CallActionAsync(origCharacter.Name, "move", actionPosition);
                                if (moveResponse != null)
                                {
                                    client.DisplayActionResponse(moveResponse);
                                    if (moveResponse.Cooldown.RemainingSeconds > 0)
                                    {
                                        await Task.Delay(moveResponse.Cooldown.RemainingSeconds * 1000);
                                    }
                                }
                            }
                            if (actionPosition != $"x:{actionResponse.Character.X},y:{actionResponse.Character.Y}")
                            {
                                Csl.WriteWarning($"[{origCharacter.Name}] at wrong position to do action, moving to correct position.");
                                ActionResponse reMoveResponse = await client.CallActionAsync(origCharacter.Name, ActionType.Move, actionPosition);
                                if (reMoveResponse != null)
                                {
                                    client.DisplayActionResponse(reMoveResponse);
                                    await Task.Delay(reMoveResponse.Cooldown.RemainingSeconds * 1000);
                                }
                            }
                        }
                        else
                        {
                            Csl.WriteError($"[{origCharacter.Name}] No loop action response.");
                            /*await client.BankAllItems(origCharacter.Name);

                            moveResponse = await client.CallActionAsync(origCharacter.Name, "move", actionPosition);
                            if (moveResponse != null)
                            {
                                client.DisplayActionResponse(moveResponse);
                                if (moveResponse.Cooldown.RemainingSeconds > 0)
                                {
                                    await Task.Delay(moveResponse.Cooldown.RemainingSeconds * 1000);
                                }
                            }*/
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
    }
}
