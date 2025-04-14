using System.Text.Json;
using System.Net.NetworkInformation;
using System.Net.Http.Headers;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Microsoft.VisualBasic;
using System.Net;
using System.Security.Cryptography.X509Certificates;
public class GameClient
{
    private readonly HttpClient _httpClient;
    private readonly string _baseUrl = "https://api.artifactsmmo.com/"; // Correct base URL
    private readonly string? _apiKey = Environment.GetEnvironmentVariable("ARTIFACTSMMO_API_KEY"); // MAke sure

    public GameClient()
    {
        _httpClient = new HttpClient();
        _httpClient.Timeout = TimeSpan.FromSeconds(10);
        if (_apiKey == null) throw new Exception("Missing API key in environment variable.");
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey); // Set the Authorization header
    }

    public bool IsNetworkAvailable()
    {
        return NetworkInterface.GetIsNetworkAvailable();
    }

    public async Task<ActionResponse?> CallActionAsync(string charName, string action, string data)
    {
        if (!IsNetworkAvailable())
        {
            throw new Exception("No network connection available.");
        }

        var jsonData = String.Empty;
        if (data != null && data != "")
        {
            var dict = data.Split(",").Select(part => part.Split(":")).ToDictionary(part => part[0], part => part[1]);
            jsonData = System.Text.Json.JsonSerializer.Serialize(dict);
        }


        var request = new HttpRequestMessage
        {
            Method = HttpMethod.Post,
            RequestUri = new Uri($"{_baseUrl}my/{charName}/action/{action}"),
            Headers =
                        {
                            { "Accept", "application/json" },
                            { "Authorization", $"Bearer {_apiKey}" },
                        },
            Content = new StringContent(jsonData)
            {
                Headers =
                            {
                                ContentType = new MediaTypeHeaderValue("application/json")
                            }
            }
        };
        try
        {
            HttpResponseMessage response = await _httpClient.SendAsync(request);

            if (response.StatusCode == (HttpStatusCode)490)
            {
                Utils.WriteWarning($"{charName} move failed, already on map during [{action}].");
                return null;
            }
            if (response.StatusCode == (HttpStatusCode)499)
            {
                Utils.WriteLine($"{charName} on cooldown [{action}] cancelled.");
                return null;
            }

            response.EnsureSuccessStatusCode();
            string responseBody = await response.Content.ReadAsStringAsync();

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
            };

            var wrapper = System.Text.Json.JsonSerializer.Deserialize<ApiWrapper<ActionResponse>>(responseBody, options);
            return wrapper?.Data;
        }
        catch (HttpRequestException ex)
        {
            Utils.WriteError($"{charName} unhandlded exception doing action: {action} with Data [{data}]. Message: {ex.Message}");

            return null;
        }
    }

    public async Task<Items?> GetAllItems(string action, string data)
    {
        if (!IsNetworkAvailable())
        {
            throw new Exception("No network connection available.");
        }

        var jsonData = String.Empty;
        if (data != null && data != "")
        {
            var dict = data.Split(",").Select(part => part.Split(":")).ToDictionary(part => part[0], part => part[1]);
            jsonData = System.Text.Json.JsonSerializer.Serialize(dict);
        }


        var request = new HttpRequestMessage
        {
            Method = HttpMethod.Get,
            RequestUri = new Uri($"{_baseUrl}{action}"),
            Headers =
                        {
                            { "Accept", "application/json" },
                            { "Authorization", $"Bearer {_apiKey}" },
                        },
            Content = new StringContent(jsonData)
            {
                Headers =
                            {
                                ContentType = new MediaTypeHeaderValue("application/json")
                            }
            }
        };
        try
        {
            HttpResponseMessage response = await _httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();
            string responseBody = await response.Content.ReadAsStringAsync();

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
            };

            var items = System.Text.Json.JsonSerializer.Deserialize<Items>(responseBody, options);
            return items;
        }
        catch (HttpRequestException ex)
        {
            Utils.WriteError($"Unhandlded exception doing action: {action} with Data [{data}]. Message: {ex.Message}");

            return null;
        }
    }

    public async Task<Character?> GetCharacter(string charName)
    {
        if (!IsNetworkAvailable())
        {
            throw new Exception("No network connection available.");
        }

        var request = new HttpRequestMessage
        {
            Method = HttpMethod.Get,
            RequestUri = new Uri($"{_baseUrl}characters/{charName}"),
            Headers =
                        {
                            { "Accept", "application/json" },
                            { "Authorization", $"Bearer {_apiKey}" },
                        },
            Content = new StringContent("")
            {
                Headers =
                            {
                                ContentType = new MediaTypeHeaderValue("application/json")
                            }
            }
        };
        try
        {
            HttpResponseMessage response = await _httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();
            string responseBody = await response.Content.ReadAsStringAsync();

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
            };

            var wrapper = System.Text.Json.JsonSerializer.Deserialize<ApiWrapper<Character>>(responseBody, options);
            return wrapper?.Data;
        }
        catch (HttpRequestException ex)
        {
            Utils.WriteError($"Error retreiving character[{charName}]: {ex.Message} {ex}");

            return null;
        }
    }

    public async Task<List<OriginalCharacter>?> GetCharacters(string accountName)
    {
        if (!IsNetworkAvailable())
        {
            throw new Exception("No network connection available.");
        }

        var request = new HttpRequestMessage
        {
            Method = HttpMethod.Get,
            RequestUri = new Uri($"{_baseUrl}accounts/{accountName}/characters"),
            Headers =
                        {
                            { "Accept", "application/json" },
                            { "Authorization", $"Bearer {_apiKey}" },
                        },
            Content = new StringContent("")
            {
                Headers =
                            {
                                ContentType = new MediaTypeHeaderValue("application/json")
                            }
            }
        };
        try
        {
            HttpResponseMessage response = await _httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();
            string responseBody = await response.Content.ReadAsStringAsync();

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
            };

            var wrapper = System.Text.Json.JsonSerializer.Deserialize<OriginalCharacters>(responseBody, options);
            return wrapper?.Data;
        }
        catch (HttpRequestException ex)
        {
            Utils.WriteError($"Error retreiving characters from account[{accountName}]: {ex.Message} {ex}");

            return null;
        }
    }

    public void DisplayActionResponse(ActionResponse response)
    {
        if (response == null) return;

        if (response.Fight != null)
        {
            string resultText = response.Fight.Result == "win" ? "won" : "lost";
            Utils.WriteSuccess($"{response.Character.Name} fought and {resultText}.");
            if (response.Fight.Drops.Count > 0)
            {
                Utils.Write($"{response.Character.Name} looted ");
                foreach (Drop drop in response.Fight.Drops)
                {
                    Utils.Write($"{drop.Quantity}x {drop.Code} ", ConsoleColor.Green, false);
                }
                Utils.WriteLine("", ConsoleColor.Gray, false);
            } else {
                Utils.WriteLine($"{response.Character.Name} looted nothing.");
            }

        }
        if (response.Destination != null)
        {
            Utils.WriteLine($"{response.Character.Name} moved to {response.Destination.X}, {response.Destination.Y}");
        }
        if (response.HpRestored != null)
        {
            Utils.WriteSuccess($"{response.Character.Name} healed for {response.HpRestored} health.");
        }
        if (response.Details != null)
        {
            foreach (DetailsItems item in response.Details.Items)
            {
                Utils.WriteSuccess($"{response.Character.Name} found {item.Quantity}x {item.Code}.");
            }

            Utils.WriteSuccess($"{response.Character.Name} gained {response.Details.Xp} exp.");
        }

        Utils.WriteLine($"{response.Character.Name}: {response.Cooldown.RemainingSeconds} seconds cooldown.");

        if (response.Cooldown.RemainingSeconds > 0)
        {
            // Call function async after remaining seconds
            /*Task.Delay(response.Cooldown.RemainingSeconds * 1000).ContinueWith((task) =>
            {
                Utils.Write($"{response.Character.Name}'s cooldown finished.");
            });*/

        }
    }


    public async Task<Boolean> BankAll(string characterName)
    {
        Character? character = await GetCharacter(characterName);
        if (character == null)
        {
            Utils.WriteError($"Error retreiving character[{characterName}]");
            return false;
        }

        ActionResponse? moveBankResponse = await CallActionAsync(character.Name, "move", "x:4,y:1");
        if (moveBankResponse != null)
        {
            await Task.Delay(moveBankResponse.Cooldown.RemainingSeconds * 1000);
        }

        foreach (InventoryItem item in character.Inventory)
        {
            if (item.Code != "")
            {
                ActionResponse? bankResponse = await CallActionAsync(character.Name, "bank/deposit", $"code:{item.Code},quantity:{item.Quantity}");
                if (bankResponse != null)
                {
                    DisplayActionResponse(bankResponse);
                    await Task.Delay(bankResponse.Cooldown.RemainingSeconds * 1000);
                }

            }
        }


        return true;
    }

    public async Task<Boolean> BankAllItems(string characterName)
    {

        bool response = await BankAll(characterName);
        if (response == true)
        {
            Utils.WriteSuccess($"Banked all items for {characterName}");
        }
        else
        {
            Utils.WriteError($"Failed to bank all items for {characterName}");
        }

        return true;
    }
    public void HandleGetResponse(Items response)
    {
        if (response == null) return;

        Utils.WriteLine("Items:" + response.ToString());

        Utils.WriteJson(response);
    }
}