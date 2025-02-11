using Microsoft.Identity.Client;
using Microsoft.Net.Http.Headers;
using Newtonsoft.Json;
using System.Net.Http.Headers;
using System.Threading.Tasks;

public class GitHubService
{
    private readonly HttpClient _httpClient;
    public GitHubService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }
    //public string Token { get; set; } = string.Empty;

    
    public async Task<List<PublicKey>> GetPublicKeysAsync()
    {
        string json = await _httpClient.GetStringAsync("meta/public_keys/copilot_api");
        var response = JsonConvert.DeserializeObject<PublicKeysResponse>(json) ?? new PublicKeysResponse();
        return response.PublicKeys ?? new List<PublicKey>();
    }
    public async Task<GitHubUser> GetCurrentUserAsync(string token)
    {
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        string json = await _httpClient.GetStringAsync("user");
        return JsonConvert.DeserializeObject<GitHubUser>(json) ?? new GitHubUser();
    }


}
