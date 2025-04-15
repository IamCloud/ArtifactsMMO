using System.Text.Json;
using System.Net.NetworkInformation;
using System.Net.Http.Headers;
using System.Net;

public class GameClient
{
    private readonly HttpClient _httpClient;
    private readonly string _baseUrl = "https://api.artifactsmmo.com/"; // Correct base URL
    private readonly string? _apiKey = Environment.GetEnvironmentVariable("ARTIFACTSMMO_API_KEY"); // MAke sure

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
                Csl.WriteWarning($"{charName} move failed, already on map during [{action}].");
                return null;
            }
            if (response.StatusCode == (HttpStatusCode)499)
            {
                Csl.WriteWarning($"{charName} on cooldown [{action}] cancelled.");
                return null;
            }
            if (response.StatusCode == (HttpStatusCode)598)
            {
                Csl.WriteWarning($"{charName} could not {action} because of wrong location on map.");
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
            Csl.WriteError($"{charName} unhandlded exception doing action: {action} with Data [{data}]. Message: {ex.Message}");

            return null;
        }
    }

    public async Task<Items?> GetAllItems(string action, string data)
    {
        string? result = await GetAsync($"items", null);

        if (result == null)
        {
            Csl.WriteError($"Error retreiving items.");
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
            Csl.WriteError($"Error retreiving {charName}.");
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
            Csl.WriteError("Error retreiving characters.");
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
            Csl.WriteError($"Error retrieving data: {ex.Message} {ex}");
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
            Csl.WriteError("Error retreiving maps.");
            return null;
        }

        return System.Text.Json.JsonSerializer.Deserialize<Maps>(result, _jsonSerializerOptions)?.Data;

    }

    public async Task<List<Resource>?> GetResources(string skill, int level)
    {
        Dictionary<string, string> pams = new Dictionary<string, string>();
        pams.Add("skill", skill);
        pams.Add("max_level", level.ToString());
        string? result = await GetAsync("resources", pams);

        if (result == null)
        {
            Csl.WriteError("Error retreiving resources.");
            return null;
        }

        return System.Text.Json.JsonSerializer.Deserialize<Resources>(result, _jsonSerializerOptions)?.Data;
    }


    public void DisplayActionResponse(ActionResponse response)
    {
        if (response == null) return;

        Csl.Write($"{response.Character.Name} ", ConsoleColor.Gray, true);
        if (response.Fight != null)
        {
            string resultText = response.Fight.Result == "win" ? "won" : "lost";
            Csl.Write($"fought and {resultText}. Loot: ");
            foreach (Drop drop in response.Fight.Drops)
            {
                Csl.Write($"{drop.Quantity} {drop.Code}, ", ConsoleColor.Green, false);
            }
            Csl.Write($"{response.Fight.Gold} gold. ", ConsoleColor.Gray, false);
        }
        if (response.Destination != null)
        {
            Csl.Write($"moved to {response.Destination.X}, {response.Destination.Y} ");
        }
        if (response.HpRestored != null)
        {
            Csl.Write($"healed for {response.HpRestored} health. ", ConsoleColor.Green, false);
        }
        if (response.Details != null)
        {
            Csl.Write($"collected ", ConsoleColor.Gray, false);
            foreach (DetailsItems item in response.Details.Items)
            {
                Csl.Write($"{item.Quantity} {item.Code}, ", ConsoleColor.Green, false); //Utils.Write($"{item.Quantity} {drop.Code}, ", ConsoleColor.Green, false);
            }
            Csl.Write($"and gained {response.Details.Xp} exp. ", ConsoleColor.Gray, false);
        }

        Csl.WriteLine($"Cooldown: {response.Cooldown.RemainingSeconds} secs.", ConsoleColor.Blue, false);

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
            Csl.WriteError($"Error retreiving character[{characterName}]");
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
            Csl.WriteSuccess($"Banked all items for {characterName}");
        }
        else
        {
            Csl.WriteError($"Failed to bank all items for {characterName}");
        }

        return true;
    }
}