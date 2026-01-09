using System.Globalization;
using Aruba.CmpService.BaremetalProvider.Abstractions.Constants;
using Aruba.CmpService.BaremetalProvider.Abstractions.Extensions;
using Aruba.CmpService.BaremetalProvider.Abstractions.Interfaces.Messages;
using Aruba.CmpService.BaremetalProvider.Abstractions.Interfaces.Providers;
using Aruba.CmpService.BaremetalProvider.Abstractions.Interfaces.Repositories;
using Aruba.CmpService.BaremetalProvider.Abstractions.Interfaces.Services;
using Aruba.CmpService.BaremetalProvider.Abstractions.Mapping;
using Aruba.CmpService.BaremetalProvider.Abstractions.Messages.Deployments;
using Aruba.CmpService.BaremetalProvider.Abstractions.Models;
using Aruba.CmpService.BaremetalProvider.Abstractions.Models.Enums;
using Aruba.CmpService.BaremetalProvider.Abstractions.Models.SwaaSes;
using Aruba.CmpService.BaremetalProvider.Abstractions.Models.Validation;
using Aruba.CmpService.BaremetalProvider.Abstractions.Providers.Models.Legacy;
using Aruba.CmpService.BaremetalProvider.Abstractions.Providers.Models.Legacy.Swaases;
using Aruba.CmpService.BaremetalProvider.Abstractions.QueryHandlers.Common.Requests;
using Aruba.CmpService.BaremetalProvider.Abstractions.QueryHandlers.Swaases.Requests;
using Aruba.CmpService.BaremetalProvider.Abstractions.UseCases.Common.Requests;
using Aruba.CmpService.BaremetalProvider.Abstractions.UseCases.SwaaSes.Requests;
using Aruba.CmpService.BaremetalProvider.Abstractions.Utils;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Throw;

namespace Aruba.CmpService.BaremetalProvider.Infrastructure.Services;
public class SwaasesService :
    BaseResourceService<Swaas, SwaasProperties, LegacySwaasListItem, LegacySwaasDetail, ISwaasesProvider>,
    ISwaasesService
{
    protected override Typologies Typology => Typologies.Swaas;

    private readonly ISwaasCatalogRepository swaasCatalogRepository;
    private readonly IInternalLegacyProvider internalLegacyProvider;

    public SwaasesService(
        ILogger<SwaasesService> logger,
        ISwaasesProvider swaasesProvider,
        IProjectProvider projectProvider,
        ILocationMapRepository locationMapRepository,
        IOptions<RenewFrequencyOptions> renewFrequencyOptions,
        IOptions<BaremetalOptions> baremetalOptions,
        IPaymentsProvider paymentsProvider,
        ICatalogueProvider catalogueProvider,
        IProfileProvider profileProvider,
        IOptions<EnableUpdatedEventOptions> enableUpdatedEventOptions,
        ISwaasCatalogRepository swaasCatalogRepository,
        IInternalLegacyProvider internalLegacyProvider) : base(logger, swaasesProvider, projectProvider, locationMapRepository, renewFrequencyOptions, baremetalOptions, catalogueProvider, paymentsProvider, profileProvider, enableUpdatedEventOptions)
    {
        this.swaasCatalogRepository = swaasCatalogRepository;
        this.internalLegacyProvider = internalLegacyProvider;
    }

    #region Swaas   
    public async Task<ServiceResult<SwaasList>> Search(SwaasSearchFilterRequest request, CancellationToken cancellationToken)
    {
        return await base.SearchInternal<SwaasList>(request, cancellationToken).ConfigureAwait(false);
    }

    public async Task<ServiceResult<Swaas>> GetById(BaseGetByIdRequest<Swaas> request, CancellationToken cancellationToken)
    {
        return await this.GetByIdInternal(request.ResourceId, request.UserId, request.ProjectId, cancellationToken).ConfigureAwait(false);
    }

    public async Task<ServiceResult> Rename(RenameUseCaseRequest request, CancellationToken cancellationToken)
    {
        return await base.RenameInternal(request, cancellationToken).ConfigureAwait(false);
    }

    public async Task<ServiceResult> SetAutomaticRenew(SetAutomaticRenewUseCaseRequest request, CancellationToken cancellationToken)
    {
        return await this.SetAutomaticRenewInternal(request, cancellationToken).ConfigureAwait(false);
    }

    public async Task<ServiceResult<SwaasCatalog>> SearchCatalog(CatalogFilterRequest request, CancellationToken cancellationToken)
    {
        request.ThrowIfNull();

        return await this.SearchCatalogInternal<SwaasCatalog>(request, cancellationToken).ConfigureAwait(false);
    }
    #endregion

    #region virtualSwitches
    public async Task<ServiceResult<VirtualSwitchList>> GetVirtualSwitches(VirtualSwitchSearchFilterRequest request)
    {
        ArgumentNullException.ThrowIfNull(request.SwaasId);

        var legacyResponse = await this.LegacyProvider.GetVirtualSwitches(request.SwaasId).ConfigureAwait(false);
        if (!legacyResponse.Success || legacyResponse.Result == null)
        {
            Log.LogWarning(Logger, "GetVirtualSwitches > virtual switches not found for swaas {0}", request.SwaasId);
            return ServiceResult<VirtualSwitchList>.CreateLegacyError(legacyResponse, this.Typology, FieldNames.VirtualSwitch, request.SwaasId);
        }

        var legacyRegions = await this.internalLegacyProvider.GetRegions().ConfigureAwait(false);
        if (!legacyRegions.Success || legacyRegions.Result is null)
        {
            Log.LogWarning(Logger, "GetVirtualSwitches > get regions error", request.SwaasId);
            return ServiceResult<VirtualSwitchList>.CreateInternalServerError();
        }

        return new ServiceResult<VirtualSwitchList>()
        {
            Value = SwaasMapping.MapToVirtualSwitchList(legacyResponse.Result, legacyRegions.Result),
        };
    }

    public async Task<ServiceResult<VirtualSwitch>> GetVirtualSwitch(VirtualSwitchGetByIdRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(request.VirtualSwitchId);

        var swaasValidation = await this.ValidateUserAndProject(nameof(GetVirtualSwitch), request.UserId, request.ProjectId).ConfigureAwait(false);
        if (swaasValidation.Errors.Any())
        {
            return ServiceResult<VirtualSwitch>.FromBase(swaasValidation);
        }

        if (string.IsNullOrWhiteSpace(request.SwaasId) || string.IsNullOrWhiteSpace(request.VirtualSwitchId))
        {
            return ServiceResult<VirtualSwitch>.CreateNotFound(request.VirtualSwitchId);
        }

        var legacyResponse = await this.LegacyProvider.GetVirtualSwitches(request.SwaasId).ConfigureAwait(false);
        if (!legacyResponse.Success || legacyResponse.Result == null)
        {
            Log.LogWarning(Logger, "GetVirtualSwitch > virtual switches not found for swaas {0}", request.VirtualSwitchId);
            return ServiceResult<VirtualSwitch>.CreateLegacyError(legacyResponse, this.Typology, FieldNames.VirtualSwitch, request.SwaasId);
        }

        var virtualSwitch = legacyResponse.Result.FirstOrDefault(f => f.VirtualNetworkId?.Equals(request.VirtualSwitchId, StringComparison.OrdinalIgnoreCase) ?? false);
        if (virtualSwitch == null)
        {
            Log.LogWarning(Logger, "GetVirtualSwitch > virtual switch not found {0}", request.VirtualSwitchId);
            return ServiceResult<VirtualSwitch>.CreateNotFound(request.VirtualSwitchId);
        }
        var legacyRegions = await this.internalLegacyProvider.GetRegions().ConfigureAwait(false);
        if (!legacyRegions.Success || legacyRegions.Result is null)
        {
            Log.LogWarning(Logger, "GetVirtualSwitch > get regions error", request.VirtualSwitchId);
            return ServiceResult<VirtualSwitch>.CreateInternalServerError();
        }

        return new ServiceResult<VirtualSwitch>()
        {
            Value = virtualSwitch.MapToVirtualSwitch(legacyRegions.Result)
        };
    }

    public async Task<ServiceResult<VirtualSwitch>> AddVirtualSwitch(VirtualSwitchAddUseCaseRequest request, CancellationToken cancellationToken)
    {
        //request.ThrowIfNull();
        var swaasValidationResult = await this.ValidateExistence(nameof(AddVirtualSwitch), request.UserId, request.ResourceId, request.ProjectId, false).ConfigureAwait(false);
        if (!swaasValidationResult.ContinueCheckErrors)
        {
            return ServiceResult<VirtualSwitch>.Clone(swaasValidationResult);
        }

        // Check Name
        var validation = this.ValidateVirtualSwitchName(nameof(AddVirtualSwitch), request.VirtualSwitch.Name, request.ResourceId);
        if (validation.Errors.Any())
        {
            return ServiceResult<VirtualSwitch>.FromBase(validation);
        }

        // region validation
        if (string.IsNullOrWhiteSpace(request.VirtualSwitch.Location))
        {
            Log.LogWarning(Logger, "AddVirtualSwitch > Location is required");
            validation.AddError(BadRequestError.Create(FieldNames.Location, "Virtual switch location is required.").AddLabel(BaremetalBaseLabelErrors.VirtualSwitchLocationRequired()));
            return validation;
        }

        var legacyRegionsResponse = await this.internalLegacyProvider.GetRegions().ConfigureAwait(false);
        if (!legacyRegionsResponse.Success || legacyRegionsResponse.Result is null)
        {
            Log.LogWarning(Logger, "AddVirtualSwitch > get regions error", request.ResourceId);
            return ServiceResult<VirtualSwitch>.CreateInternalServerError();
        }
        var legacyRegions = legacyRegionsResponse.Result;
        if (!legacyRegions.Select(s => s.RegionId).Contains(request.VirtualSwitch.Location, StringComparer.OrdinalIgnoreCase))
        {
            Log.LogWarning(Logger, "AddVirtualSwitch > Invalid location");
            validation.AddError(BadRequestError.Create(FieldNames.Location, "Virtual switch location is invalid.").AddLabel(BaremetalBaseLabelErrors.VirtualSwitchLocationNotFound()));
            return validation;
        }

        var legacyVirtualSwitch = new AddLegacyVirtualSwitch()
        {
            FriendlyName = request.VirtualSwitch.Name,
            Id = long.Parse(request.ResourceId, CultureInfo.InvariantCulture),
            Region = request.VirtualSwitch.Location
        };
        var legacyResponse = await this.LegacyProvider.AddVirtualSwitch(legacyVirtualSwitch).ConfigureAwait(false);
        if (!legacyResponse.Success)
        {
            Log.LogWarning(Logger, "Create Virtual Switch > legacy api call PostCreateVirtualNetwork error for swaas {Id}", request.ResourceId);
            return ServiceResult<VirtualSwitch>.CreateLegacyError(legacyResponse, this.Typology, FieldNames.VirtualSwitch, request.ResourceId);
        }
        else if (legacyResponse.Result == null)
        {
            Log.LogWarning(Logger, "Create Virtual Switch > legacy api call PostCreateVirtualNetwork returned false for swaas {Id}", request.ResourceId);
            return ServiceResult<VirtualSwitch>.CreateInternalServerError();
        }

        //Leggo le region e le mappo sul virtual switch
        return new ServiceResult<VirtualSwitch>()
        {
            Value = legacyResponse.Result.Map(legacyRegions)
        };
    }

    public async Task<ServiceResult<VirtualSwitch>> EditVirtualSwitch(VirtualSwitchEditUseCaseRequest request, CancellationToken cancellationToken)
    {
        request.ThrowIfNull();
        request.Id.ThrowIfNull();

        var swaasValidation = await this.ValidateExistence(nameof(EditVirtualSwitch), request.UserId, request.ResourceId, request.ProjectId, false).ConfigureAwait(false);

        if (!swaasValidation.ContinueCheckErrors)
        {
            return ServiceResult<VirtualSwitch>.CreateNotFound(request.ResourceId);
        }

        // get all virtualswitches for the given swaas
        var virtualSwitchesResponse = await this.LegacyProvider.GetVirtualSwitches(request.ResourceId).ConfigureAwait(false);
        if (!virtualSwitchesResponse.Success || virtualSwitchesResponse.Result == null)
        {
            Log.LogWarning(Logger, "EditVirtualSwitch > virtual switches not found for swaas {0}", request.ResourceId);
            return ServiceResult<VirtualSwitch>.CreateLegacyError(virtualSwitchesResponse, this.Typology, FieldNames.VirtualSwitch, request.ResourceId);
        }
        var virtualSwitches = virtualSwitchesResponse.Result;

        // get virtual switch
        var virtualSwitch = virtualSwitches.FirstOrDefault(f => f.VirtualNetworkId!.Equals(request.Id, StringComparison.OrdinalIgnoreCase));
        if (virtualSwitch == null)
        {
            Log.LogWarning(Logger, "EditVirtualSwitch > virtual switch not found {0}", request.Id);
            return ServiceResult<VirtualSwitch>.CreateNotFound(request.Id);
        }

        // name validation
        var validation = ValidateVirtualSwitchName(nameof(EditVirtualSwitch), request.Name, request.ResourceId);
        if (validation.Errors.Any())
        {
            Log.LogInfo(Logger, "EditVirtualSwitch > invalid virtual switch name");
            return validation;
        }

        var legacyVirtualSwitch = new EditLegacyVirtualSwitch()
        {
            FriendlyName = request.Name,
            Id = long.Parse(request.ResourceId, CultureInfo.InvariantCulture),
            VirtualNetworkId = request.Id
        };

        var legacyResponse = await this.LegacyProvider.EditVirtualSwitch(legacyVirtualSwitch).ConfigureAwait(false);
        if (!legacyResponse.Success)
        {
            Log.LogWarning(Logger, "Edit Virtual Switch > legacy api call PostSetVirtualNetworkFriendlyName error for swaas {Id}", request.ResourceId);
            return ServiceResult<VirtualSwitch>.CreateLegacyError(legacyResponse, this.Typology, FieldNames.VirtualSwitch, request.ResourceId);
        }
        else if (legacyResponse.Result == null)
        {
            Log.LogWarning(Logger, "Edit Virtual Switch > legacy api call PostSetVirtualNetworkFriendlyName returned false for swaas {Id}", request.ResourceId);
            return ServiceResult<VirtualSwitch>.CreateInternalServerError();
        }

        var legacyRegions = await this.internalLegacyProvider.GetRegions().ConfigureAwait(false);
        if (!legacyRegions.Success || legacyRegions.Result is null)
        {
            Log.LogWarning(Logger, "Edit Virtual Switch > get regions error", request.ResourceId);
            return ServiceResult<VirtualSwitch>.CreateInternalServerError();
        }

        virtualSwitch.FriendlyName = request.Name;
        return new ServiceResult<VirtualSwitch>()
        {
            Value = virtualSwitch.MapToVirtualSwitch(legacyRegions.Result),
        };
    }

    public async Task<ServiceResult> DeleteVirtualSwitch(VirtualSwitchDeleteUseCaseRequest request, CancellationToken cancellationToken)
    {
        request.ThrowIfNull();
        request.Id.ThrowIfNull();

        var swaasValidation = await this.ValidateExistence(nameof(EditVirtualSwitch), request.UserId, request.ResourceId, request.ProjectId, true).ConfigureAwait(false);

        if (!swaasValidation.ContinueCheckErrors)
        {
            return ServiceResult.CreateNotFound(request.ResourceId);
        }

        // get all virtualswitches for the given swaas
        var virtualSwitchesResponse = await this.LegacyProvider.GetVirtualSwitches(request.ResourceId).ConfigureAwait(false);
        if (!virtualSwitchesResponse.Success || virtualSwitchesResponse.Result == null)
        {
            Log.LogWarning(Logger, "DeleteVirtualSwitch > virtual switches not found for swaas {0}", request.ResourceId);
            return ServiceResult.CreateLegacyError(virtualSwitchesResponse, this.Typology, FieldNames.VirtualSwitch, request.ResourceId);
        }
        var virtualSwitches = virtualSwitchesResponse.Result;

        // get virtual switch
        var virtualSwitch = virtualSwitches.FirstOrDefault(f => f.VirtualNetworkId!.Equals(request.Id, StringComparison.OrdinalIgnoreCase));
        if (virtualSwitch == null)
        {
            Log.LogWarning(Logger, "DeleteVirtualSwitch > virtual switch not found {0}", request.Id);
            return ServiceResult.CreateNotFound(request.Id);
        }

        var legacyVirtualSwitch = new DeleteLegacyVirtualSwitch()
        {
            Id = long.Parse(request.ResourceId, CultureInfo.InvariantCulture),
            VirtualNetworkId = request.Id
        };

        var legacyResponse = await this.LegacyProvider.DeleteVirtualSwitch(legacyVirtualSwitch).ConfigureAwait(false);
        if (!legacyResponse.Success)
        {
            Log.LogWarning(Logger, "Delete virtual Switch > legacy api call PostDeleteVirtualNetwork error for swaas {Id}", request.ResourceId);
            return ServiceResult<VirtualSwitch>.CreateLegacyError(legacyResponse, this.Typology, FieldNames.VirtualSwitch, request.ResourceId);
        }

        return new ServiceResult();
    }

    public async Task<ServiceResult<List<LinkableService>>> GetLinkableServices(VirtualSwitchGetLinkableServicesRequest request, CancellationToken cancellationToken)
    {
        request.ThrowIfNull();
        request.SwaasId.ThrowIfNull();

        var virtualSwitchesResponse = await this.LegacyProvider.GetVirtualSwitches(request.SwaasId).ConfigureAwait(false);
        if (!virtualSwitchesResponse.Success || virtualSwitchesResponse.Result == null)
        {
            Log.LogWarning(Logger, "GetLinkableServices > virtual switches not found for swaas {0}", request.SwaasId);
            return ServiceResult<List<LinkableService>>.CreateLegacyError(virtualSwitchesResponse, this.Typology, FieldNames.VirtualSwitch, request.SwaasId);
        }

        var selectedVirtualSwitch = virtualSwitchesResponse.Result.FirstOrDefault(vs => (vs.VirtualNetworkId?.Equals(request.VirtualSwitchId, StringComparison.OrdinalIgnoreCase) ?? false));

        if (selectedVirtualSwitch is null)
        {
            Log.LogWarning(base.Logger, "GetLinkableServices > Selected virtual switch not found {0}", request.VirtualSwitchId);
            return ServiceResult<List<LinkableService>>.CreateNotFound(request.VirtualSwitchId);
        }

        var servicesWithRegion = await internalLegacyProvider.GetServicesWithRegions().ConfigureAwait(false);
        if (!servicesWithRegion.Success || servicesWithRegion.Result is null)
        {
            Log.LogWarning(base.Logger, "GetLinkableServices > legacy api call GetServicesWithRegions error: ", servicesWithRegion.Serialize());
            return ServiceResult<List<LinkableService>>.CreateLegacyError(servicesWithRegion, this.Typology, FieldNames.VirtualSwitch, request.SwaasId);
        }

        // Filter already linked services
        var swaasVirtualSwitchLinks = await this.LegacyProvider.GetVirtualSwitchLinks(request.SwaasId).ConfigureAwait(false);
        if (!swaasVirtualSwitchLinks.Success || swaasVirtualSwitchLinks.Result == null)
        {
            Log.LogWarning(Logger, "GetLinkableServices > virtual switch links not found for swaas {0}", request.SwaasId);
            return ServiceResult<List<LinkableService>>.CreateLegacyError(swaasVirtualSwitchLinks, this.Typology, FieldNames.VirtualSwitchLink, request.SwaasId);
        }
        var alreadyLinkedServices = swaasVirtualSwitchLinks.Result
            .Where(l => (l.VirtualNetwork?.VirtualNetworkId ?? string.Empty).Equals(selectedVirtualSwitch.VirtualNetworkId, StringComparison.OrdinalIgnoreCase))
            .Select(l => l.Resource.ConnectedService)
            .ToList();

        var linkableServices = servicesWithRegion.Result
            .Where(s => selectedVirtualSwitch.Region!.Contains(s.Region!, StringComparison.OrdinalIgnoreCase)
                     && !alreadyLinkedServices.Any(l => l.Id == s.Id))
            .ToList();

        return new ServiceResult<List<LinkableService>>()
        {
            Value = linkableServices.Select(s => new LinkableService()
            {
                Id = s.Id.ToString(CultureInfo.InvariantCulture),
                Name = s.Name,
                Typology = CommonMapping.MapToCmpTypology(s.ServiceType!.Value).Value.ToString()
            }).ToList()
        };
    }

    /// <summary>
    /// Validate name
    /// </summary>
    private ServiceResult<VirtualSwitch> ValidateVirtualSwitchName(string methodName, string? name, string id)
    {
        var ret = new ServiceResult<VirtualSwitch>();
        if (string.IsNullOrWhiteSpace(name))
        {
            Log.LogWarning(Logger, "{MethodName} > Virtual switch name is required");
            ret.AddError(BadRequestError.Create(FieldNames.Name, "virtual switch name is required.").AddLabel(BaremetalBaseLabelErrors.NameRequired(this.Typology)));
            return ret;
        }

        if (name.Length > 40)
        {
            Log.LogWarning(Logger, "{MethodName} > The virtual switch name must have a maximum of 40 alphanumeric characters");
            ret.AddError(BadRequestError.Create(FieldNames.Name, "The virtual switch name must have a maximum of 40 alphanumeric characters.").AddLabel(BaremetalBaseLabelErrors.NameMaximumLength(this.Typology)));
            return ret;
        }

        var specialChars = ".,;:?!@#$%^&*\\_~'()-/+".ToCharArray();

        var invalidChar = name.Where(f => !char.IsNumber(f) && !char.IsLetter(f) && !specialChars.Contains(f) && !char.IsWhiteSpace(f)).ToList();

        if (invalidChar.Count > 0)
        {
            Log.LogWarning(Logger, "{MethodName} > The virtual switch name contains illegal characters");
            ret.AddError(BadRequestError.Create(FieldNames.Name, $"The virtual switch name has invalid chars:{invalidChar}").AddLabel(BaremetalBaseLabelErrors.NameInvalidChars(this.Typology)).AddParam("char", invalidChar));
            return ret;
        }
        return ret;
    }
    #endregion

    #region Virtual Switch Links

    public async Task<ServiceResult<VirtualSwitchLinkList>> GetVirtualSwitchLinks(VirtualSwitchLinkSearchFilterRequest request)
    {
        ArgumentNullException.ThrowIfNull(request.SwaasId);

        var legacyResponse = await this.LegacyProvider.GetVirtualSwitchLinks(request.SwaasId).ConfigureAwait(false);
        if (!legacyResponse.Success || legacyResponse.Result == null)
        {
            Log.LogWarning(Logger, "GetVirtualSwitchLinks > virtual switch links not found for swaas {0}", request.SwaasId);
            return ServiceResult<VirtualSwitchLinkList>.CreateLegacyError(legacyResponse, this.Typology, FieldNames.VirtualSwitchLink, request.SwaasId);
        }

        return new ServiceResult<VirtualSwitchLinkList>()
        {
            Value = legacyResponse.Result.MapToVirtualSwitchLinkList(),
        };
    }

    public async Task<ServiceResult<VirtualSwitchLink>> GetVirtualSwitchLink(VirtualSwitchLinkGetByIdRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(request.VirtualSwitchLinkId);

        var swaasValidation = await this.ValidateUserAndProject(nameof(GetVirtualSwitchLink), request.UserId, request.ProjectId).ConfigureAwait(false);
        if (swaasValidation.Errors.Any())
        {
            return ServiceResult<VirtualSwitchLink>.FromBase(swaasValidation);
        }

        if (string.IsNullOrWhiteSpace(request.SwaasId) || string.IsNullOrWhiteSpace(request.VirtualSwitchLinkId))
        {
            return ServiceResult<VirtualSwitchLink>.CreateNotFound(request.VirtualSwitchLinkId);
        }

        var legacyResponse = await this.LegacyProvider.GetVirtualSwitchLinks(request.SwaasId).ConfigureAwait(false);
        if (!legacyResponse.Success || legacyResponse.Result == null)
        {
            Log.LogWarning(Logger, "GetVirtualSwitchLink > virtual switch links not found for swaas {0}", request.SwaasId);
            return ServiceResult<VirtualSwitchLink>.CreateLegacyError(legacyResponse, this.Typology, FieldNames.VirtualSwitchLink, request.SwaasId);
        }

        var virtualSwitchLink = legacyResponse.Result.FirstOrDefault(f => f.Resource?.ResourceId?.Equals(request.VirtualSwitchLinkId, StringComparison.OrdinalIgnoreCase) ?? false);
        if (virtualSwitchLink == null)
        {
            Log.LogWarning(Logger, "GetVirtualSwitchLink > virtual switch link not found for swaas {0}", request.SwaasId);
            return ServiceResult<VirtualSwitchLink>.CreateNotFound(request.VirtualSwitchLinkId);
        }

        return new ServiceResult<VirtualSwitchLink>()
        {
            Value = virtualSwitchLink.MapToVirtualSwitchLink()
        };
    }

    public async Task<ServiceResult<VirtualSwitchLink>> AddVirtualSwitchLink(VirtualSwitchLinkAddUseCaseRequest request, CancellationToken cancellationToken)
    {
        request.ThrowIfNull();
        request.Link.ThrowIfNull();

        var swaasValidation = await this.ValidateUserAndProject(nameof(AddVirtualSwitchLink), request.UserId, request.ProjectId).ConfigureAwait(false);
        if (swaasValidation.Errors.Any())
        {
            return ServiceResult<VirtualSwitchLink>.CreateNotFound(request.ResourceId);
        }

        var ret = new ServiceResult<VirtualSwitchLink>();
        if (!request.Link.LinkedServiceId.HasValue)
        {
            ret.AddError(BadRequestError.Create(FieldNames.ServiceId, "Service id to connect is required.").AddLabel(BaremetalBaseLabelErrors.ServiceToConnectIdRequired()));
        }

        if (string.IsNullOrWhiteSpace(request.Link.LinkedServiceTypology))
        {
            ret.AddError(BadRequestError.Create(FieldNames.ServiceType, "Service typology to connect is required.").AddLabel(BaremetalBaseLabelErrors.ServiceToConnectTypologyRequired()));
        }

        if (ret.Errors.Any())
        {
            return ret;
        }

        var virtualSwitchesResponse = await this.LegacyProvider.GetVirtualSwitches(request.ResourceId).ConfigureAwait(false);

        if (!virtualSwitchesResponse.Success || virtualSwitchesResponse.Result == null)
        {
            Log.LogWarning(Logger, "AddVirtualSwitchLink > virtual switches not found for swaas {0}", request.ResourceId);
            return ServiceResult<VirtualSwitchLink>.CreateLegacyError(virtualSwitchesResponse, this.Typology, FieldNames.VirtualSwitch, request.ResourceId);
        }

        var virtualSwitch = virtualSwitchesResponse.Result?.FirstOrDefault(f => f.VirtualNetworkId?.Equals(request.Link.VirtualSwitchId, StringComparison.OrdinalIgnoreCase) ?? false);
        if (virtualSwitch == null)
        {
            ret.AddError(BadRequestError.Create(FieldNames.Name, "Virtual switch not found.").AddLabel(BaremetalBaseLabelErrors.NameRequired(this.Typology)));
        }

        //Aggiungere validazione sulla region
        var servicesWithRegionResponse = await internalLegacyProvider.GetServicesWithRegions().ConfigureAwait(false);
        if (!servicesWithRegionResponse.Success || servicesWithRegionResponse.Result is null)
        {
            Log.LogWarning(base.Logger, "AddVirtualSwitchLink > legacy api call GetServicesWithRegions error: ", servicesWithRegionResponse.Serialize());
            return ServiceResult<VirtualSwitchLink>.CreateLegacyError(servicesWithRegionResponse, this.Typology, FieldNames.VirtualSwitch, request.ResourceId);
        }

        var servicesWithRegion = servicesWithRegionResponse.Result;
        var serviceWithRegion = servicesWithRegion.FirstOrDefault(f => f.Id == request.Link.LinkedServiceId && f.ServiceType == request.Link.LinkedServiceTypology?.MapToLegacyTypology());

        if (serviceWithRegion == null)
        {
            ret.AddError(BadRequestError.Create(FieldNames.Name, "Service to link not found.").AddLabel(BaremetalBaseLabelErrors.ServiceToConnectNotFound()));
        }
        else if (!virtualSwitch?.Region?.Split('-').Contains(serviceWithRegion.Region, StringComparer.OrdinalIgnoreCase) ?? false)
        {
            ret.AddError(BadRequestError.Create(FieldNames.Name, "Service to link is an invalid location.").AddLabel(BaremetalBaseLabelErrors.ServiceToConnectInvalidRegion()));
        }
        if (ret.Errors.Any())
        {
            return ret;
        }

        //Preparo la create
        var addResponse = await this.LegacyProvider.AddVirtualSwitchLink(request.ResourceId, request.Link.VirtualSwitchId!, request.Link.LinkedServiceId!.Value, request.Link.LinkedServiceTypology!.MapToLegacyTypology()).ConfigureAwait(false);
        if (!addResponse.Success)
        {
            Log.LogWarning(Logger, "AddVirtualSwitchLink > add virtual switch link error for swaas {0}", request.ResourceId);
            return ServiceResult<VirtualSwitchLink>.CreateLegacyError(addResponse, this.Typology, FieldNames.VirtualSwitchLink, request.ResourceId);

        }
        ret.Value = new VirtualSwitchLink()
        {
            VirtualSwitchId = request.Link.VirtualSwitchId,
            VirtualSwitchName = virtualSwitch.FriendlyName,
            LinkedServiceId = request.Link.LinkedServiceId.Value,
            LinkedServiceName = serviceWithRegion.CustomName ?? serviceWithRegion.Name,
            LinkedServiceTypology = request.Link.LinkedServiceTypology,
            Status = VirtualSwitchLinkStatuses.Activating,
        };
        return ret;
    }

    public async Task<ServiceResult> DeleteVirtualSwitchLink(VirtualSwitchLinkDeleteUseCaseRequest request, CancellationToken cancellationToken)
    {
        var validation = await this.ValidateUserAndProject(nameof(DeleteVirtualSwitchLink), request.UserId, request.ProjectId).ConfigureAwait(false);
        if (validation.Errors.Any())
        {
            return ServiceResult<VirtualSwitchLink>.CreateNotFound(request.Id);
        }

        var virtualSwitchLinksResponse = await this.LegacyProvider.GetVirtualSwitchLinks(request.ResourceId).ConfigureAwait(false);

        if (!virtualSwitchLinksResponse.Success || virtualSwitchLinksResponse.Result == null)
        {
            Log.LogWarning(Logger, "DeleteVirtualSwitchLink > virtual switch links not found for swaas {0}", request.ResourceId);
            return ServiceResult<VirtualSwitchLink>.CreateLegacyError(virtualSwitchLinksResponse, this.Typology, FieldNames.VirtualSwitchLink, request.ResourceId);
        }

        var virtualSwitchLink = virtualSwitchLinksResponse.Result?.FirstOrDefault(f => f.Resource?.ResourceId?.Equals(request.Id, StringComparison.OrdinalIgnoreCase) ?? false);
        if (virtualSwitchLink == null)
        {
            Log.LogWarning(Logger, "DeleteVirtualSwitchLink > virtual switch link not found {0}", request.Id);
            return ServiceResult<VirtualSwitchLink>.CreateNotFound(request.Id);
        }

        //Preparo la delete
        var ret = await this.LegacyProvider.DeleteVirtualSwitchLink(request.ResourceId, virtualSwitchLink.VirtualNetwork.VirtualNetworkId!, request.Id!).ConfigureAwait(false);
        if (!ret.Success)
        {
            Log.LogWarning(Logger, "DeleteVirtualSwitchLink > delete virtual switch links error {0}", virtualSwitchLink.VirtualNetwork.VirtualNetworkId!);
            return ServiceResult<VirtualSwitchLink>.CreateLegacyError(virtualSwitchLinksResponse, this.Typology, FieldNames.VirtualSwitchLink, request.ResourceId);
        }
        return new ServiceResult();
    }
    #endregion

    protected override async Task<Swaas> MapToListItem(LegacySwaasListItem resource, string userId, Abstractions.Providers.Models.Projects.Project project)
    {
        var location = await this.GetLocation(resource.ServerFarmCode).ConfigureAwait(false);
        return resource.MapToListitem(userId, project, location);
    }

    protected override async Task<Swaas> MapToDetail(LegacySwaasDetail resource, string userId, Abstractions.Providers.Models.Projects.Project project)
    {
        var location = await this.GetLocation(resource.ServerFarmCode).ConfigureAwait(false);
        var profile = await this.GetProfile(userId).ConfigureAwait(false);
        return resource.MapToDetail(userId, project, location, profile!.IsResellerCustomer!.Value);
    }

    protected override async Task<ICatalog> MapCatalog(IEnumerable<LegacyCatalogItem> items, long totalCount, string? language = null)
    {
        var swaasCatalog = await this.swaasCatalogRepository.GetAllAsync().ConfigureAwait(false);

        var ret = new SwaasCatalog()
        {
            TotalCount = totalCount,
            Values = items.MapToSwaasCatalog(swaasCatalog, language).ToList()
        };
        return await Task.FromResult(ret).ConfigureAwait(false);
    }

    protected override DateTimeOffset? GetDueDate(Swaas resource)
    {
        return resource?.Properties?.DueDate;
    }

    protected override IResourceMessage GetUpdatedEvent(Swaas swaas, AutorenewFolderAction? action)
    {
        return new SwaasUpdatedDeployment()
        {
            Resource = swaas.Map(action)
        };
    }
}
