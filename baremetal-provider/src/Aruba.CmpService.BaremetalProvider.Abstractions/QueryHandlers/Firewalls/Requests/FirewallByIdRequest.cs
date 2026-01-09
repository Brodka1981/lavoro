using Aruba.CmpService.BaremetalProvider.Abstractions.Models.Firewalls;
using Aruba.CmpService.BaremetalProvider.Abstractions.QueryHandlers.Common.Requests;

namespace Aruba.CmpService.BaremetalProvider.Abstractions.QueryHandlers.Firewalls.Requests;
public class FirewallByIdRequest :
    BaseGetByIdRequest<Firewall>
{
}
