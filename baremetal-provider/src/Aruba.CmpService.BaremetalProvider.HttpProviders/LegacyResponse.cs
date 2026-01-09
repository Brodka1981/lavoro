namespace Aruba.CmpService.BaremetalProvider.HttpProviders;
internal class LegacyResponse
{
    public object? Body { get; set; }
    public bool IsAsync { get; set; }
    public object? MessageId { get; set; }
    public bool Success { get; set; }
    public string? ResultCode { get; set; }
    public string? Version { get; set; }
}
