namespace Aruba.CmpService.BaremetalProvider.Api.Model;
public class ProblemDetailError
{
    [JsonPropertyName("field")]
    public string? PropertyName { get; set; }
    [JsonPropertyName("message")]
    public string? ErrorMessage { get; set; }
}


