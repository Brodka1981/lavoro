namespace Aruba.CmpService.BaremetalProvider.Abstractions.QueryHandlers.SmartStorages.Requests;

public class SmartStorageStatisticsRequest
{
    public string? ResourceId { get; set; }
    public string? UserId { get; set; }
    public string? ProjectId { get; set; }
}
