namespace Aruba.CmpService.BaremetalProvider.MongoDb.Configuration;

public class DbSettings
{
    public DbSettings()
    {

    }

    public string? ConnectionString { get; set; }
    public string? NameDb { get; set; }
    public long HealthCheckTimeoutInMs { get; set; } = 2000;
}
