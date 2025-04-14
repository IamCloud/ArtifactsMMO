

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
                    Utils.WriteError("Error: No network connection available.  Please check your internet connection.");
                    Console.WriteLine("Press any key to exit.");
                    Console.ReadKey();
                    return;
                }

                List<OriginalCharacter> origChars = await client.GetCharacters(_accountName);

                Utils.WriteLine($"Welcome {_accountName}! You have {origChars.Count} characters.");
                Utils.WriteLine($"-------------------------------------------");
                StartInitialLoop(client, origChars);

                while (true)
                {
                    string command = Console.ReadLine();
                    string[] commandSplit = command.Split(" ");

                    if (commandSplit[0] == "exit")
                    {
                        Utils.WriteLine("Cancelling loops...");
                        origChars.ForEach(c => c.LoopCancelTokenSource.Cancel());
                        Utils.WriteLine("Loops canceled.");
                        Utils.WriteLine("Goodbye!");
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
                        string charName = commandSplit[1];
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
                    else if (command.StartsWith("get"))
                    {
                        string action = commandSplit[1];
                        string? data = String.Empty;
                        if (commandSplit.Length > 2)
                        {
                            data = commandSplit[2];
                        }

                        Items response = await client.GetAllItems(action, null);

                        client.HandleGetResponse(response);
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
                Utils.WriteError($"Error: {ex.Message}");
            }
            catch (System.Text.Json.JsonException ex)
            {
                Utils.WriteError($"JSON Error: {ex.Message}");
            }
            catch (Exception ex)
            {
                Utils.WriteError($"An error occurred: {ex.Message}");
            }
        }

        private static void StartInitialLoop(GameClient client, List<OriginalCharacter> origChars)
        {
            Utils.WriteLine("Starting loops...");
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
                    Loop(client, "fish", origChar);
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

                if (remainingCooldown.TotalMilliseconds > 0)
                {
                    Utils.WriteLine($"Waiting {remainingCooldown.TotalSeconds} seconds for {origCharacter.Name} cooldown.");
                    await Task.Delay((int)remainingCooldown.TotalMilliseconds);
                }


                // Do the move action once
                ActionResponse moveResponse = await client.CallActionAsync(origCharacter.Name, ActionType.Move, actionPosition);
                if (moveResponse != null)
                {
                    client.DisplayActionResponse(moveResponse);
                    await Task.Delay(moveResponse.Cooldown.RemainingSeconds * 1000);
                }

                // Start infinite gathering
                while (true)
                {
                    try
                    {
                        if (origCharacter.LoopCancelTokenSource.IsCancellationRequested)
                        {
                            Utils.WriteLine($"[{origCharacter.Name}] Loop cancelled.");
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
                                moveResponse = await client.CallActionAsync(origCharacter.Name, "move", actionPosition);
                                if (moveResponse != null)
                                {
                                    client.DisplayActionResponse(moveResponse);
                                    if (moveResponse.Cooldown.RemainingSeconds > 0)
                                    {
                                        await Task.Delay(moveResponse.Cooldown.RemainingSeconds * 1000);
                                    }
                                }
                            }
                        }
                        else
                        {
                            Utils.WriteError($"[{origCharacter.Name}] No loop action response.");
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
                        Utils.WriteError($"[{origCharacter.Name}] Error: {ex.Message}");
                        await Task.Delay(5000);
                    }
                }
            });
        }
    }
}
