using Aruba.CmpService.BaremetalProvider.Abstractions.Dtos.Internal;
using Aruba.CmpService.BaremetalProvider.Abstractions.Extensions;
using Aruba.CmpService.BaremetalProvider.Abstractions.Interfaces.Providers;
using Aruba.CmpService.BaremetalProvider.Abstractions.Interfaces.Services;
using Aruba.CmpService.BaremetalProvider.Abstractions.Mapping;
using Aruba.CmpService.BaremetalProvider.Abstractions.Models;
using Aruba.CmpService.BaremetalProvider.Abstractions.Models.Internal;
using Aruba.CmpService.BaremetalProvider.Abstractions.Providers.Models.Legacy.Internal;
using Aruba.CmpService.BaremetalProvider.Abstractions.Providers.Models.Profile;
using Aruba.CmpService.BaremetalProvider.Abstractions.UseCases.Internal.Requests;
using Aruba.CmpService.BaremetalProvider.Abstractions.Utils;
using Aruba.Extensions.Linq;
using Microsoft.Extensions.Logging;

namespace Aruba.CmpService.BaremetalProvider.Infrastructure.Services;
public class InternalService :
    IInternalService
{
    private readonly ILogger<InternalService> logger;
    private readonly IProjectProvider projectProvider;
    private readonly IProfileProvider profileProvider;
    private readonly IInternalLegacyProvider internalLegacyProvider;
    private readonly IAdminLegacyProvider adminLegacyProvider;

    public InternalService(
        ILogger<InternalService> logger,
        IProjectProvider projectProvider,
        IProfileProvider profileProvider,
        IInternalLegacyProvider internalLegacyProvider,
        IAdminLegacyProvider adminLegacyProvider)
    {
        this.logger = logger;
        this.projectProvider = projectProvider;
        this.profileProvider = profileProvider;
        this.internalLegacyProvider = internalLegacyProvider;
        this.adminLegacyProvider = adminLegacyProvider;
    }

    public async Task<ServiceResult<IEnumerable<Abstractions.Models.Internal.LegacyResource>>> GetAllResources(InternalGetResourcesUseCaseRequest request, CancellationToken cancellationToken)
    {
        //Prendo il progetto di default
        var projectResponse = await this.projectProvider.GetDefaultProjectAsync(request.UserId).ConfigureAwait(false);
        if (!projectResponse.Success || projectResponse.Result == null || !(projectResponse.Result.Properties?.Default ?? false))
        {
            Log.LogWarning(this.logger, "{MethodName} > no default prject found", nameof(GetAllResources));
            return ServiceResult<IEnumerable<Abstractions.Models.Internal.LegacyResource>>.CreateInternalServerError();
        }
        var project = projectResponse.Result;
        var ret = new List<Abstractions.Models.Internal.LegacyResource>();
        var profile = await this.GetProfile(request.UserId!).ConfigureAwait(false);
        if (profile == null)
        {
            Log.LogWarning(this.logger, "{MethodName} > user not found found", nameof(GetAllResources));
            return ServiceResult<IEnumerable<Abstractions.Models.Internal.LegacyResource>>.CreateInternalServerError();
        }


        var legacyFolderBody = new List<LegacyResourceFilter>();
        var res = new Abstractions.Providers.Models.ApiCallOutput<IEnumerable<Abstractions.Providers.Models.Legacy.Internal.LegacyResource>>();
        if (!(request.Ids ?? new List<LegacyResourceIdDto>()).Any()) //Recupero tutte le risorse dell'utente
        {
            res = await internalLegacyProvider.GetLegacyResources().ConfigureAwait(false);
            if (res.Success)
            {
                return new ServiceResult<IEnumerable<Abstractions.Models.Internal.LegacyResource>>()
                {
                    Value = res.Result.MapToResource(request.UserId, profile.IsResellerCustomer ?? false, project.Metadata.Id!)
                };
            }
            else
            {
                return ServiceResult<IEnumerable<Abstractions.Models.Internal.LegacyResource>>.CreateInternalServerError();
            }
        }
        else //recupero solo le risorse della folder
        {
            request.Ids!.ForEach(id =>
            {
                legacyFolderBody.Add(new LegacyResourceFilter
                {
                    Id = id.Id,
                    ServiceType = id.TypologyId!.MapToLegacyTypology()
                });
            });
            res = await internalLegacyProvider.GetFolderServices(legacyFolderBody, request.GetPrices).ConfigureAwait(false);
            if (res.Success)
            {
                return new ServiceResult<IEnumerable<Abstractions.Models.Internal.LegacyResource>>()
                {
                    Value = res.Result.MapToResource(request.UserId, profile.IsResellerCustomer ?? false, project.Metadata.Id!)
                };
            }
            else
            {
                return ServiceResult<IEnumerable<Abstractions.Models.Internal.LegacyResource>>.CreateInternalServerError();
            }
        }
    }

    public async Task<ServiceResult> UpsertAutomaticRenew(InternalAutomaticRenewUseCaseRequest request, CancellationToken cancellationToken)
    {
        var paymentData = request.RenewData!.PaymentMethodId.CreateInfo();

        var autorenewDto = new LegacyAutoRenew()
        {
            DeviceId = paymentData.DeviceId,
            DeviceType = (long)paymentData.DeviceType,
            FrequencyDuration = request.RenewData.Months,
            Services = request.Resources.Select(r => new LegacyResourceRenew()
            {
                Id = r.Id,
                ServiceType = r.TypologyId.MapToLegacyTypology()
            })
        };

        var result = await this.internalLegacyProvider.UpsertAutomaticRenewAsync(autorenewDto).ConfigureAwait(false);
        if (!result.Success || !result.Result)
        {
            Log.LogInfo(logger, "InternalUpsertAutomaticRenew - renew result {0} bool value {1} http status code {2}", result.Success, result.Result, result.StatusCode);
            return ServiceResult.CreateInternalServerError();
        }
        var ret = new ServiceResult();
        var envelopes = request.Resources.Select(s => request.CreateEnvelopeForDeploymentStatusChanged(s.Id.ToString(), s.TypologyId)).ToArray();
        ret.AddEnvelopes(envelopes);
        return ret;
    }

    public async Task<ServiceResult<AutorechargeResponse>> GetAutorecharge(InternalAutorechargeUseCaseRequest request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        var user = await this.GetProfile(request.UserId!).ConfigureAwait(false);
        if (user is null)
        {
            Log.LogInfo(logger, "Get user  user not found");
            return ServiceResult<AutorechargeResponse>.CreateInternalServerError();
        }
        if (user.IsResellerCustomer ?? false)
        {
            return new ServiceResult<AutorechargeResponse>()
            {
                Value = new AutorechargeResponse()
                {
                    Enabled = false
                }
            };
        }

        var result = request.UseAdminProvider
            ? await this.adminLegacyProvider.GetAutorechargeAsync(request.UserId!).ConfigureAwait(false)
            : await this.internalLegacyProvider.GetAutorechargeAsync().ConfigureAwait(false);

        if (!result.Success || result.Result is null)
        {
            Log.LogInfo(logger, "Legacy GetAutorecharge api call error http status code {1}", result.StatusCode);
            return ServiceResult<AutorechargeResponse>.CreateInternalServerError();
        }

        return new ServiceResult<AutorechargeResponse>()
        {
            Value = result.Result.MapToResponse(),
        };
    }

    public async Task<ServiceResult<IEnumerable<BaseLegacyResource>>> AdminGetAllResources(InternalAdminGetResourcesUseCaseRequest request, CancellationToken cancellationToken)
    {
        // All resources
        if (!(request.Ids ?? new List<LegacyResourceIdDto>()).Any())
        {
            var res = await adminLegacyProvider.GetFolderLinkableServices(request.UserId).ConfigureAwait(false);

            if (!res.Success || res.Result is null)
            {
                return ServiceResult<IEnumerable<BaseLegacyResource>>.CreateInternalServerError();
            }

            return new ServiceResult<IEnumerable<BaseLegacyResource>>()
            {
                Value = res.Result.MapToBaseLegacyResource(request.UserId)
            };
        }
        else // Resources by ids
        {
            var legacyFolderBody = new List<LegacyResourceFilter>();

            request.Ids!.ForEach(id =>
            {
                legacyFolderBody.Add(new LegacyResourceFilter
                {
                    Id = id.Id,
                    ServiceType = id.TypologyId!.MapToLegacyTypology()
                });

            });
            var res = await adminLegacyProvider.GetFolderServices(legacyFolderBody, request.UserId).ConfigureAwait(false);

            if (!res.Success || res.Result is null)
            {
                return ServiceResult<IEnumerable<BaseLegacyResource>>.CreateInternalServerError();
            }

            return new ServiceResult<IEnumerable<BaseLegacyResource>>()
            {
                Value = res.Result.MapToBaseLegacyResource(request.UserId)
            };
        }
    }

    public async Task<ServiceResult<IEnumerable<Region>>> GetRegions(InternalGetRegionsUseCaseRequest request, CancellationToken cancellationToken)
    {
        var result = await this.internalLegacyProvider.GetRegions().ConfigureAwait(false);

        if (!result.Success || result.Result is null)
        {
            Log.LogWarning(logger, "Legacy GetRegions api call error http status code {1}", result.StatusCode);
            return ServiceResult<IEnumerable<Region>>.CreateInternalServerError();
        }

        return new ServiceResult<IEnumerable<Region>>()
        {
            Value = result.Result.Map()
        };
    }

    protected async Task<UserProfile?> GetProfile(string userId)
    {
        var profileResponse = await this.profileProvider.GetUser(userId).ConfigureAwait(false);
        if (profileResponse.Success && profileResponse.Result != null)
        {
            return new UserProfile()
            {
                IsResellerCustomer = profileResponse.Result.IsResellerCustomer
            };
        }
        return null;
    }
}
