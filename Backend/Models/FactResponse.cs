using System.Text.Json.Serialization;

namespace Backend.Models;

public class FactResponse
{
    [JsonPropertyName("fact")]
    public string Fact { get; set; }
    
    [JsonPropertyName("length")]
    public int Length { get; set; }
}