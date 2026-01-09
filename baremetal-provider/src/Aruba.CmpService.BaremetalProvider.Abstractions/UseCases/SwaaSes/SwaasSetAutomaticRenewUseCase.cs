using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Aruba.CmpService.BaremetalProvider.Abstractions.UseCases.Servers.Requests;
using Aruba.CmpService.BaremetalProvider.Abstractions.UseCases.Servers.Responses;
using Aruba.CmpService.BaremetalProvider.Abstractions.UseCases.Servers;
using Aruba.CmpService.BaremetalProvider.Abstractions.UseCases.Swaases;
using Aruba.CmpService.BaremetalProvider.Abstractions.UseCases.SwaaSes.Requests;
using Aruba.CmpService.BaremetalProvider.Abstractions.UseCases.SwaaSes.Responses;
using Aruba.CmpService.BaremetalProvider.Abstractions.Interfaces.Services;
using Aruba.CmpService.BaremetalProvider.Abstractions.Models;

namespace Aruba.CmpService.BaremetalProvider.Abstractions.UseCases.SwaaSes;
public class SwaasSetAutomaticRenewUseCase : SwaasNoResponseBaseUseCase<SwaasSetAutomaticRenewUseCaseRequest, SwaasSetAutomaticRenewUseCaseResponse>
{
  public SwaasSetAutomaticRenewUseCase(ISwaasesService SwaasService)
: base(SwaasService)
    {
    }

    protected override async Task<ServiceResult> ExecuteService(SwaasSetAutomaticRenewUseCaseRequest request, CancellationToken cancellationToken)
    {
        var ret = await SwaasesService.SetAutomaticRenew(request, cancellationToken).ConfigureAwait(false);

        return ret;
    }
}
