using Newtonsoft.Json;

public class PublicKey
{
    [JsonProperty("key_identifier")]
    public string KeyIdentifier { get; set; } = string.Empty;

    [JsonProperty("key")]
    public string Key { get; set; } = string.Empty;

    [JsonProperty("is_current")]
    public bool IsCurrent { get; set; }
}

public class PublicKeysResponse
{
    [JsonProperty("public_keys")]
    public List<PublicKey>? PublicKeys { get; set; }
}