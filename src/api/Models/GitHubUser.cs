using System;
using Newtonsoft.Json;

public class GitHubUser
{
    [JsonProperty("login")]
    public string? Login { get; set; }

    [JsonProperty("id")]
    public int Id { get; set; }

    [JsonProperty("avatar_url")]
    public string? AvatarUrl { get; set; }

    [JsonProperty("gravatar_id")]
    public string? GravatarId { get; set; }

    [JsonProperty("url")]
    public string? Url { get; set; }

    [JsonProperty("html_url")]
    public string? HtmlUrl { get; set; }


    [JsonProperty("name")]
    public string? Name { get; set; }

    [JsonProperty("company")]
    public string? Company { get; set; }

    [JsonProperty("email")]
    public string? Email { get; set; }

    [JsonProperty("bio")]
    public string? Bio { get; set; }


}
