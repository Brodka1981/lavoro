using System.Diagnostics.CodeAnalysis;

namespace Aruba.CmpService.BaremetalProvider.Abstractions.Providers.Models.Profile;
[ExcludeFromCodeCoverage(Justification = "It's a dto from providers")]
public class UserProfile
{
    /// <summary>
    /// ID
    /// </summary>
    public string? Id { get; set; }

    /// <summary>
    /// Username
    /// </summary>
    public string? Username { get; set; }

    /// <summary>
    /// Account
    /// </summary>
    public string? Account { get; set; }

    /// <summary>
    /// Tenant
    /// </summary>
    public string? Tenant { get; set; }

    /// <summary>
    /// Company
    /// </summary>
    public string? Company { get; set; }

    /// <summary>
    /// Profile
    /// </summary>
    public string? Profile { get; set; }

    /// <summary>
    /// Role
    /// </summary>
    public string? Role { get; set; }

    /// <summary>
    /// PriceList
    /// </summary>
    public string? PriceList { get; set; }

    /// <summary>
    /// IsResellerCustomer
    /// </summary>
    public bool? IsResellerCustomer { get; set; }
}
