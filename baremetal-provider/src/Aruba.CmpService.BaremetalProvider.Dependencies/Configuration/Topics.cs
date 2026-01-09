namespace Aruba.CmpService.BaremetalProvider.Dependencies.Configuration;

[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage(Justification = "Integration Test: Necessità del message bus")]

public class Topics
{
    public static string? NotificationEvents { get; set; }
    public static string? DeploymentEvents { get; set; }
    public static string? ResourcesEvents { get; set; }
}
