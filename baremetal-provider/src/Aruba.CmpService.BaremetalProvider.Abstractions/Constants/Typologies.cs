using System.Diagnostics.CodeAnalysis;
using Aruba.CmpService.BaremetalProvider.Abstractions.Models.Enums;

namespace Aruba.CmpService.BaremetalProvider.Abstractions.Constants;

[ExcludeFromCodeCoverage(Justification = "It's a constants class without logic")]
public class Typologies : StringEnumeration
{
    private Typologies(string value) : base(value)
    { }
    public static Typologies Server => new Typologies("server");
    public static Typologies Switch => new Typologies("switch");
    public static Typologies Firewall => new Typologies("firewall");
    public static Typologies Swaas => new Typologies("swaas");
    public static Typologies SmartStorage => new Typologies("smartStorage");
    public static Typologies MCI => new Typologies("mci");
    public static Typologies LegacyBackupM365 => new Typologies("backup365");
    public static Typologies LegacyBackup => new Typologies("xxxxxxxxx");
    public static Typologies LegacyCloudStorage => new Typologies("xxxxxxxxx");
    public static Typologies LegacyCustomService => new Typologies("xxxxxxxxx");
    public static Typologies LegacyDatabase => new Typologies("xxxxxxxxx");
    public static Typologies Dns => new Typologies("dns");
    public static Typologies Domain => new Typologies("domain");
    public static Typologies LegacyFTP => new Typologies("xxxxxxxxx");
    public static Typologies HybridLink => new Typologies("hybridLink");
    public static Typologies ApplicationPlatform => new Typologies("applicationPlatform");
    public static Typologies LegacyPleskLicense => new Typologies("xxxxxxxxx");
    public static Typologies LegacyLoadBalancer => new Typologies("xxxxxxxxx");
    public static Typologies LegacySLAMonitoring => new Typologies("xxxxxxxxx");
    public static Typologies LegacyPrivateCloud => new Typologies("xxxxxxxxx");
    public static Typologies LegacySharedStorage => new Typologies("xxxxxxxxx");
    public static Typologies LegacyVirtualSwitch => new Typologies("xxxxxxxxx");
    public static Typologies LegacyPublicIP => new Typologies("xxxxxxxxx");
    public static Typologies LegacyCloudServer => new Typologies("xxxxxxxxx");
    public static Typologies VPS => new Typologies("vps");

    // FIXME: @matteo.filippa check if ok
    public static Typologies HPC => new Typologies("hpc");
}
