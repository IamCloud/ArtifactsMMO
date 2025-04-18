using System.Text.Json;
using System.Net.NetworkInformation;
using System.Net.Http.Headers;
using System.Net;
using System.Text;
using Microsoft.AspNetCore.Mvc.ModelBinding;

public class GameClient
{
    private readonly HttpClient _httpClient;
    private readonly string _baseUrl = "https://api.artifactsmmo.com/"; // Correct base URL
    private readonly string? _apiKey = Environment.GetEnvironmentVariable("ARTIFACTSMMO_API_KEY"); // MAke sure

    public HtmlLogs logs = new HtmlLogs();
    private readonly JsonSerializerOptions _jsonSerializerOptions = new JsonSerializerOptions
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
    };
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

    public async Task<ActionResponse?> PerformAction(string charName, string action, string data)
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

            if (response.StatusCode == (HttpStatusCode)497)
            {
                logs.WriteLine($"{charName} inventory full doing [{action}].", LogColors.Warning);
                return null;
            }
            if (response.StatusCode == (HttpStatusCode)490)
            {
                logs.WriteLine($"{charName} move failed, already on map during [{action}].", LogColors.Warning);
                return null;
            }
            if (response.StatusCode == (HttpStatusCode)499)
            {
                logs.WriteLine($"{charName} on cooldown [{action}] cancelled.", LogColors.Warning);
                return null;
            }
            if (response.StatusCode == (HttpStatusCode)598)
            {
                logs.WriteLine($"{charName} could not {action} because of wrong location on map.", LogColors.Warning);
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
            Console.WriteLine($"{charName} unhandlded exception doing action: {action} with Data [{data}]. Message: {ex.Message}");

            return null;
        }
    }

    public int GetCharGatherLvl(Character character, string gatheringType)
    {
        switch (gatheringType)
        {
            case GatheringType.Woodcutting:
                return character.WoodcuttingLevel;
            case GatheringType.Alchemy:
                return character.AlchemyLevel;
            case GatheringType.Fishing:
                return character.FishingLevel;
            case GatheringType.Mining:
                return character.MiningLevel;
            case GatheringType.Cooking:
                return character.CookingLevel;
            default:
                return 0;
        }
    }


    public bool isCharInLoop(List<OriginalCharacter> origChars, Character character)
    {
        OriginalCharacter? origChar = origChars.Find(o => o.Name == character.Name);
        return origChar.LoopCancelTokenSource != null;
    }

    public async Task<Items?> GetAllItems(string action, string data)
    {
        string? result = await GetAsync($"items", null);

        if (result == null)
        {
            Console.WriteLine($"Error retreiving items.");
            return null;
        }

        var items = System.Text.Json.JsonSerializer.Deserialize<Items>(result, _jsonSerializerOptions);
        return items;

    }

    public async Task<Character?> GetCharacter(string charName)
    {
        string? result = await GetAsync($"characters/{charName}", null);

        if (result == null)
        {
            Console.WriteLine($"Error retreiving {charName}.");
            return null;
        }

        var wrapper = System.Text.Json.JsonSerializer.Deserialize<ApiWrapper<Character>>(result, _jsonSerializerOptions);
        return wrapper?.Data;

    }

    public async Task<List<OriginalCharacter>?> GetCharacters(string accountName)
    {
        //Dictionary<string, string> dict = new Dictionary<string, string>();
        //dict.Add("content_type", contentType);
        string? result = await GetAsync($"accounts/{accountName}/characters", null);

        if (result == null)
        {
            Console.WriteLine("Error retreiving characters.");
            return null;
        }

        return JsonSerializer.Deserialize<OriginalCharacters>(result, _jsonSerializerOptions)?.Data;
    }

    private async Task<string?> GetAsync(string url, Dictionary<string, string>? parameters = null)
    {
        if (!IsNetworkAvailable())
        {
            throw new Exception("No network connection available.");
        }

        var queryString = string.Empty;
        if (parameters != null)
        {
            var queryParts = parameters.Select(p => $"{p.Key}={p.Value}");
            queryString = $"?{string.Join("&", queryParts)}";
        }

        var requestUri = new Uri($"{_baseUrl}{url}{queryString}");

        var request = new HttpRequestMessage
        {
            Method = HttpMethod.Get,
            RequestUri = requestUri,
            Headers =
        {
            { "Accept", "application/json" },
            { "Authorization", $"Bearer {_apiKey}" },
        }
        };

        try
        {
            HttpResponseMessage response = await _httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsStringAsync();
        }
        catch (HttpRequestException ex)
        {
            Console.WriteLine($"Error retrieving data: {ex.Message} {ex}");
            return null;
        }
    }
    public async Task<List<Map>?> GetMaps(string contentType, string contentCode)
    {
        Dictionary<string, string> pams = new Dictionary<string, string>();
        pams.Add("content_type", contentType);
        pams.Add("content_code", contentCode);
        string? result = await GetAsync("maps", pams);

        if (result == null)
        {
            Console.WriteLine("Error retreiving maps.");
            return null;
        }

        return System.Text.Json.JsonSerializer.Deserialize<Maps>(result, _jsonSerializerOptions)?.Data;

    }

    public async Task<List<Resource>?> GetResources(string gahteringType, int level)
    {
        Dictionary<string, string> pams = new Dictionary<string, string>();
        pams.Add("skill", gahteringType);
        pams.Add("max_level", level.ToString());
        string? result = await GetAsync("resources", pams);

        if (result == null)
        {
            Console.WriteLine("Error retreiving resources.");
            return null;
        }

        return System.Text.Json.JsonSerializer.Deserialize<Resources>(result, _jsonSerializerOptions)?.Data;
    }


    public void LogConsoleActionResponse(ActionResponse response)
    {
        if (response == null) return;

        logs.Open();
        logs.Write($"{response.Character.Name}: ");

        if (response.Fight != null)
        {
            string resultText = response.Fight.Result == "win" ? "won" : "lost";
            logs.Write($"fought and {resultText}. Loot: ");
            foreach (Drop drop in response.Fight.Drops)
            {
                logs.Write($"{drop.Quantity} {drop.Code}, ", LogColors.Success);
            }
            logs.Write($"{response.Fight.Xp} exp, ");
            logs.Write($"{response.Fight.Gold} gold. ");
        }

        if (response.Destination != null)
        {
            logs.Write($"moved to {response.Destination.X}, {response.Destination.Y} ");
        }

        if (response.HpRestored != null)
        {
            logs.Write($"healed for {response.HpRestored} health.", LogColors.Success);
        }

        if (response.Details != null)
        {
            logs.Write($"gathered ");
            foreach (DetailsItems item in response.Details.Items)
            {
                logs.Write($"{item.Quantity} {item.Code}, ", LogColors.Success);
            }
            logs.Write($"and gained {response.Details.Xp} exp. ");
        }

        logs.Write($"Cooldown: {response.Cooldown.RemainingSeconds} secs.", LogColors.Info);

        logs.Close();
    }


    public async Task<Boolean> BankAll(string characterName)
    {
        Character? character = await GetCharacter(characterName);
        if (character == null)
        {
            Console.WriteLine($"Error retreiving character[{characterName}]");
            return false;
        }

        ActionResponse? moveBankResponse = await PerformAction(character.Name, "move", "x:4,y:1");
        if (moveBankResponse != null)
        {
            await Task.Delay(moveBankResponse.Cooldown.RemainingSeconds * 1000);
        }

        foreach (InventoryItem item in character.Inventory)
        {
            if (item.Code != "")
            {
                ActionResponse? bankResponse = await PerformAction(character.Name, "bank/deposit", $"code:{item.Code},quantity:{item.Quantity}");
                if (bankResponse != null)
                {
                    LogConsoleActionResponse(bankResponse);
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
            logs.WriteLine($"Banked all items for {characterName}", LogColors.Info);
        }
        else
        {
            Console.WriteLine($"Failed to bank all items for {characterName}");
        }

        return true;
    }
}