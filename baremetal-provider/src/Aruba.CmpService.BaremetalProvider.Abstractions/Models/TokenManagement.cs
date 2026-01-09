namespace Aruba.CmpService.BaremetalProvider.Abstractions.Models;
public class TokenManagement
{
    public string? TokenEndpoint { get; set; }
    public string? ClientId { get; set; }
    public string? ClientSecret { get; set; }
    public string? Scope { get; set; }
    public int CacheLifeTimeBuffer { get; set; } = 60;
}
