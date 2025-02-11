using Newtonsoft.Json;
public record TodoItem
{
    [JsonProperty("id")]
    public string Id { get; set; } = default!;
    
    [JsonProperty("userId")]
    public string UserId { get; set; } = default!;
    
    [JsonProperty("title")]
    public string Title { get; set; } = default!;
    
    [JsonProperty("description")]
    public string Description { get; set; } = default!;
    
    [JsonProperty("dueDate")]
    public DateTime? DueDate { get; set; }
    
    [JsonProperty("completed")]
    public bool? Completed { get; set; }
}
