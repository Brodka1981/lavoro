using System.Collections.ObjectModel;
using System.Globalization;
using Aruba.CmpService.BaremetalProvider.Abstractions.Constants;
using Aruba.CmpService.BaremetalProvider.Abstractions.Dtos;
using Aruba.CmpService.BaremetalProvider.Abstractions.Mapping;
using Aruba.CmpService.BaremetalProvider.Abstractions.Models;
using Aruba.CmpService.BaremetalProvider.Abstractions.Models.Servers;
using Aruba.CmpService.BaremetalProvider.Abstractions.Providers.Models.Legacy;
using Aruba.CmpService.BaremetalProvider.Abstractions.Providers.Models.Legacy.Internal;
using Aruba.CmpService.BaremetalProvider.Abstractions.QueryHandlers.Common.Requests;
using Aruba.CmpService.BaremetalProvider.Abstractions.QueryHandlers.Servers.Requests;
using Aruba.CmpService.BaremetalProvider.Abstractions.TechnoRail.Models;
using Aruba.CmpService.BaremetalProvider.Abstractions.UseCases.Common.Requests;
using Aruba.CmpService.BaremetalProvider.Abstractions.UseCases.Internal.Requests;
using Aruba.CmpService.BaremetalProvider.Abstractions.UseCases.Servers.Requests;
using Aruba.CmpService.ResourceProvider.Common.Dtos.Response;
using Aruba.CmpService.ResourceProvider.Common.Dtos.Shared;
using Aruba.CmpService.ResourceProvider.Common.Messages.v1;
using Aruba.CmpService.ResourceProvider.Common.ResourceQuery;
using FluentAssertions;

namespace Aruba.CmpService.BaremetalProvider.Tests.Mapping;
public class CommonMappingTests : TestBase
{
    public CommonMappingTests(ITestOutputHelper output)
        : base(output)
    {
    }

    #region Response
    [Fact]
    [Unit]
    public void UpdateIpAddress_Success_NotNull()
    {
        var request = new ServerUpdateIpUseCaseRequest()
        {
            Id = 1,
            IpAddress = new IpAddressDto()
            {
                Description = "description",
                HostNames = new Collection<string>() { "HostName1", "HostName2" }
            },
            ProjectId = "1",
            ResourceId = "1",
            UserId = "1",
        };

        var mapped = request.Map();
        mapped.Should().NotBeNull();
        mapped.Hosts.Count().Should().Be(2);
        mapped.CustomName.Should().Be(request.IpAddress.Description);
    }

    [Fact]
    [Unit]
    public void UpdateIpAddress_Success_DtoNull()
    {
        var request = new ServerUpdateIpUseCaseRequest()
        {
            Id = 1,
            ProjectId = "1",
            ResourceId = "1",
            UserId = "1",
        };

        var mapped = request.Map();
        mapped.Should().NotBeNull();
        mapped.Hosts.Count().Should().Be(0);
        mapped.CustomName.Should().BeNullOrWhiteSpace();
    }

    [Fact]
    [Unit]
    public void UpdateIpAddress_Success_Null()
    {
        ServerUpdateIpUseCaseRequest request = null;

        var mapped = request?.Map();
        mapped.Should().BeNull();
    }


    [Fact]
    [Unit]
    public void Status_Success_NotNull()
    {
        var status = new Status()
        {
            CreationDate = DateTimeOffset.Now,
            State = "State"
        };

        var mapped = status.MapToResponse();
        mapped.Should().NotBeNull();
        mapped.State.Should().Be(status.State);
        mapped.CreationDate.Should().Be(status.CreationDate);
    }

    [Fact]
    [Unit]
    public void Status_Success_Null1()
    {
        Status request = null;

        var mapped = request.MapToResponse();
        mapped.Should().NotBeNull();
        mapped.State.Should().BeNull();
        mapped.CreationDate.Should().BeNull();
    }

    [Fact]
    [Unit]
    public void Status_Success_Null2()
    {
        Status request = null;

        var mapped = request?.MapToResponse();
        mapped.Should().BeNull();
    }

    [Fact]
    [Unit]
    public void ServerIpAddressesFilterRequest_Success_NotNull()
    {
        var request = new ServerIpAddressesFilterRequest()
        {
            ProjectId = "1",
            UserId = "1",
            Query = new ResourceQueryDefinition(new ResourceQueryDefinitionOptions()
            {

            }),
            External = true
        };

        var mapped = request.Map();
        mapped.Should().NotBeNull();
        mapped.External.Should().BeTrue();
        mapped.Query.Should().NotBeNull();
    }

    [Fact]
    [Unit]
    public void Server_IpAddressesFilterRequest_Success_Null()
    {
        ServerIpAddressesFilterRequest request = null;

        var mapped = request.Map();
        mapped.External.Should().BeFalse();
        mapped.Query.Should().NotBeNull();
    }

    [Fact]
    [Unit]
    public void Server_IpAddressesFilterRequest_Success_NullValues()
    {
        var request = new ServerIpAddressesFilterRequest();

        var mapped = request.Map();
        mapped.External.Should().BeFalse();
        mapped.Query.Should().NotBeNull();
    }

    [Fact]
    [Unit]
    public void Server_IpAddressesFilterRequest_Success_NullValue()
    {
        var request = new ServerIpAddressesFilterRequest();

        var mapped = request.Map();
        mapped.External.Should().BeFalse();
        mapped.Query.Should().NotBeNull();
    }



    [Fact]
    [Unit]
    public void Firewall_IpAddressesFilterRequest_Success_NotNull()
    {
        var request = new ServerIpAddressesFilterRequest()
        {
            ProjectId = "1",
            UserId = "1",
            Query = new ResourceQueryDefinition(new ResourceQueryDefinitionOptions()
            {

            }),
            External = true
        };

        var mapped = request.Map();
        mapped.Should().NotBeNull();
        mapped.External.Should().BeTrue();
        mapped.Query.Should().NotBeNull();
    }

    [Fact]
    [Unit]
    public void Firewall_IpAddressesFilterRequest_Success_Null()
    {
        ServerIpAddressesFilterRequest request = null;

        var mapped = request.Map();
        mapped.External.Should().BeFalse();
        mapped.Query.Should().NotBeNull();
    }

    [Fact]
    [Unit]
    public void Category_Success_NotNul2()
    {
        var request = new Category()
        {
            Name = "Aruba",
            Provider = "Provider",
            Typology = new Typology()
            {
                Id = "1",
                Name = Typologies.Server.Value
            }
        };

        var mapped = request.MapToResponse();
        mapped.Should().NotBeNull();
        mapped.Name.Should().Be("Aruba");
        mapped.Provider.Should().Be("Provider");
        mapped.Typology.Should().NotBeNull();
        mapped.Typology?.Id.Should().Be("1");
        mapped.Typology?.Name.Should().Be(Typologies.Server.Value);
    }

    [Fact]
    [Unit]
    public void Category_Success_NotNull2()
    {
        var request = new Category()
        {
            Name = "Aruba",
            Provider = "Provider"
        };

        var mapped = request.MapToResponse();
        mapped.Should().NotBeNull();
        mapped.Name.Should().Be("Aruba");
        mapped.Provider.Should().Be("Provider");
        mapped.Typology.Should().BeNull();
    }

    [Fact]
    [Unit]
    public void Category_Success_Null1()
    {
        Category request = null;

        var mapped = request.MapToResponse();
        mapped.Should().NotBeNull();
        mapped.Provider.Should().BeNull();
        mapped.Name.Should().BeNull();
        mapped.Typology.Should().BeNull();
    }

    [Fact]
    [Unit]
    public void Category_Success_Null2()
    {
        Category request = null;

        var mapped = request?.MapToResponse();
        mapped.Should().BeNull();
    }

    [Fact]
    [Unit]
    public void Typology_Success_NotNull()
    {
        var request = new Typology()
        {
            Id = "1",
            Name = Typologies.Server.Value
        };

        var mapped = request.MapToResponse();
        mapped.Should().NotBeNull();
        mapped.Id.Should().Be("1");
        mapped.Name.Should().Be(Typologies.Server.Value);
    }

    [Fact]
    [Unit]
    public void Typology_Success_Null1()
    {
        Typology request = null;

        var mapped = request.MapToResponse();
        mapped.Should().NotBeNull();
        mapped.Id.Should().BeNull();
        mapped.Name.Should().BeNull();
    }

    [Fact]
    [Unit]
    public void Typology_Success_Null2()
    {
        Typology request = null;

        var mapped = request?.MapToResponse();
        mapped.Should().BeNull();
    }

    [Fact]
    [Unit]
    public void Location_Success_NotNull()
    {
        var request = new Location()
        {
            Name = "Roma",
            City = "Rome",
            Code = "RM",
            Country = "Italy",
            Value = "AsRoma",
        };

        var mapped = request.MapToResponse();
        mapped.Should().NotBeNull();
        mapped?.Name.Should().Be(request.Name);
        mapped?.City.Should().Be(request.City);
        mapped?.Code.Should().Be(request.Code);
        mapped?.Country.Should().Be(request.Country);
        mapped?.Value.Should().Be(request.Value);
    }

    [Fact]
    [Unit]
    public void Location_Success_Null()
    {
        Location request = null;

        var mapped = request.MapToResponse();
        mapped.Should().BeNull();
    }

    [Fact]
    [Unit]
    public void Project_Success_NotNull()
    {
        var request = new Project()
        {
            Name = "Roma",
            Id = "1",
        };

        var mapped = request.MapToResponse();
        mapped.Should().NotBeNull();
        mapped?.Id.Should().Be(request.Id);
    }

    [Fact]
    [Unit]
    public void Project_Success_Null()
    {
        Project request = null;

        var mapped = request.MapToResponse();
        mapped.Should().BeNull();
    }

    [Fact]
    [Unit]
    public void Metadata_Success_Null()
    {
        Server request = null;

        var mapped = request.MapMetadataToResponse();
        mapped.Should().BeNull();
    }
    #endregion

    #region Data
    [Fact]
    [Unit]
    public void Data_Success()
    {
        var resource = CreateBaseResource<Server>();

        var mapped = resource.MapToDeploymentStatusChanged();
        mapped.Should().NotBeNull();
        mapped.Status.Should().NotBeNull();
        mapped.Status?.CreationDate.Should().Be(resource.Status?.CreationDate);
        mapped.Status?.State.Should().Be(resource.Status?.State);
        mapped.CreatedBy.Should().Be(resource.CreatedBy);
        mapped.DeploymentId.Should().Be(resource.Id);
        mapped.Typology.Should().NotBeNull();
        mapped.Typology?.Id.Should().Be(resource.Category?.Typology?.Id);
        mapped.Typology?.Name.Should().Be(resource.Category?.Typology?.Name);
    }
    [Fact]
    [Unit]
    public void Data_Status_Success()
    {
        var resource = CreateBaseResource<Server>();
        resource.Status = new Status()
        {
            State = "Disabled",
            CreationDate = DateTimeOffset.Now,
            DisableStatusInfo = new DisableStatusInfo()
            {
                IsDisabled = true,
                PreviousStatus = new PreviousStatus()
                {
                    CreationDate = DateTimeOffset.Now,
                    State = StatusValues.Active.Value
                }
            }
        };
        var mapped = resource.MapToDeploymentStatusChanged();
        mapped.Should().NotBeNull();
        mapped.Status.Should().NotBeNull();
        mapped.Status?.CreationDate.Should().Be(resource.Status?.CreationDate);
        mapped.Status?.State.Should().Be(resource.Status?.State);
        mapped.Status?.DisableStatusInfo.Should().NotBeNull();
        mapped.Status?.DisableStatusInfo?.IsDisabled.Should().Be(resource.Status!.DisableStatusInfo.IsDisabled);
        mapped.Status?.DisableStatusInfo?.PreviousStatus.Should().NotBeNull();
        mapped.Status?.DisableStatusInfo?.PreviousStatus?.State.Should().Be(resource.Status!.DisableStatusInfo.PreviousStatus.State);
        mapped.Status?.DisableStatusInfo?.PreviousStatus.CreationDate.Should().Be(resource.Status!.DisableStatusInfo.PreviousStatus.CreationDate);
        mapped.CreatedBy.Should().Be(resource.CreatedBy);
        mapped.DeploymentId.Should().Be(resource.Id);
        mapped.Typology.Should().NotBeNull();
        mapped.Typology?.Id.Should().Be(resource.Category?.Typology?.Id);
        mapped.Typology?.Name.Should().Be(resource.Category?.Typology?.Name);
    }
    #endregion

    #region various
    [Fact]
    [Unit]
    public void IpAddressStatus_Active()
    {
        var mapped = LegacyIpAddressStatuses.Active.Map();
        mapped.Should().Be(IpAddressStatuses.Active);
    }
    [Fact]
    [Unit]
    public void IpAddressStatus_Inactive()
    {
        var mapped = LegacyIpAddressStatuses.InActive.Map();
        mapped.Should().Be(IpAddressStatuses.Inactive);
    }
    [Fact]
    [Unit]
    public void IpAddressStatus_ToBeActivated()
    {
        var mapped = LegacyIpAddressStatuses.ToBeActivated.Map();
        mapped.Should().Be(IpAddressStatuses.Updating);
    }
    [Fact]
    [Unit]
    public void IpAddressStatus_Invalid()
    {
        try
        {
            var mapped = ((LegacyIpAddressStatuses)5).Map();
        }
        catch (Exception ex)
        {
            ex.Should().BeOfType<ArgumentOutOfRangeException>();
        }
    }

    [Fact]
    [Unit]
    public void CatalogFilter_ValidValues()
    {
        var request = new CatalogFilterRequest()
        {
            External = true,
            Query = new ResourceQueryDefinition(new ResourceQueryDefinitionOptions())
        };
        var mapped = request.Map();
        mapped.Should().NotBeNull();
        mapped.External.Should().BeTrue();
        mapped.Query.Should().NotBeNull();
    }

    [Fact]
    [Unit]
    public void CatalogFilter_Null_Values()
    {
        var request = new CatalogFilterRequest();
        var mapped = request.Map();
        mapped.Should().NotBeNull();
        mapped.External.Should().BeFalse();
        mapped.Query.Should().NotBeNull();
    }

    [Fact]
    [Unit]
    public void CatalogFilter_Null()
    {
        var mapped = ((CatalogFilterRequest)null).Map();
        mapped.Should().NotBeNull();
        mapped.External.Should().BeFalse();
        mapped.Query.Should().NotBeNull();
    }

    [Fact]
    [Unit]
    public void CreateEnvelopeForDeploymentStatusChanged_Success()
    {
        var request = new InternalAdminGetResourcesUseCaseRequest()
        {
            UserId = "aru-25198",
            ResourceId = "res-001"
        };

        var deploymentId = "1234a";
        var typologyId = "server";

        var envelope = request.CreateEnvelopeForDeploymentStatusChanged(deploymentId, typologyId);

        envelope.Should().NotBeNull();
        
        var payload = envelope.Body as DeploymentStatusChanged;
        payload.Should().NotBeNull();
        payload!.CreatedBy.Should().Be("aru-25198");
        payload.DeploymentId.Should().Be("1234a");
        payload.Typology.Should().NotBeNull();
        payload.Typology.Id.Should().Be("server");
        payload.Typology.Name.Should().Be("SERVER");

        payload.Status.Should().NotBeNull();
        payload.Status.State.Should().Be(StatusValues.Active.Value);
        payload.Status.CreationDate.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Theory]
    [Unit]
    [InlineData("server", LegacyServiceType.Server)]
    [InlineData("switch", LegacyServiceType.Switch)]
    [InlineData("firewall", LegacyServiceType.Firewall)]
    [InlineData("smartStorage", LegacyServiceType.SmartStorage)]
    [InlineData("swaas", LegacyServiceType.SwitchSWAAS)]
    [InlineData("unknown", LegacyServiceType.None)]
    public void MapToLegacyTypology_ShouldReturnExpectedEnum(string inputTypology, LegacyServiceType expected)
    {
        var result = inputTypology.MapToLegacyTypology();
        result.Should().Be(expected);
    }

    [Theory]
    [Unit]
    [InlineData(LegacyServiceType.Server, "server")]
    [InlineData(LegacyServiceType.Switch, "switch")]
    [InlineData(LegacyServiceType.Firewall, "firewall")]
    [InlineData(LegacyServiceType.SmartStorage, "smartStorage")]
    [InlineData(LegacyServiceType.SwitchSWAAS, "swaas")]
    public void MapToCmpTypology_ShouldReturnExpectedTypology(LegacyServiceType input, string expectedTypologyValue)
    {
        var result = input.MapToCmpTypology();
        result.Should().NotBeNull();
        result.Value.Should().Be(expectedTypologyValue);
    }

    [Fact]
    [Unit]
    public void MapToCmpTypology_WithInvalidValue_ShouldThrow()
    {
        var invalidValue = (LegacyServiceType)999;

        Action act = () => invalidValue.MapToCmpTypology();

        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    #endregion

    #region Utils
    internal static T CreateBaseResource<T>() where T : class, IResourceBase
    {
        var ret = Activator.CreateInstance<T>();
        ret.Name = "Server1";
        ret.Id = "1";
        ret.CreationDate = DateTimeOffset.Now;
        ret.CreatedBy = "AM";
        ret.Category = new Category()
        {
            Name = "Category",
            Provider = "Aruba",
            Typology = new Typology()
            {
                Id = "1",
                Name = "Typology"
            }
        };
        ret.Ecommerce = new Ecommerce();
        ret.Location = new Location()
        {
            Name = "Roma",
            City = "Rome",
            Code = "RM",
            Country = "Italy",
            Value = "AsRoma",
        };
        ret.Project = new Project()
        {
            Name = "Roma",
            Id = "1",
        };
        ret.Status = new Status()
        {
            CreationDate = DateTimeOffset.Now,
            State = StatusValues.Active.Value
        };
        ret.Tags.Add("1");
        ret.UpdateDate = DateTimeOffset.Now;
        ret.Uri = "http://uri";
        ret.Version = new Abstractions.Models.Version()
        {
            Data = new DataVersion()
            {
                Current = 2,
                Previous = 1,
            }
        };
        return ret;
    }

    internal static void ValidateBaseResource<TModel, TResponse, TPropertiesResponse>(TModel model, TResponse resource, string updatedBy)
        where TModel : IResourceBase
        where TResponse : ResponseDto<TPropertiesResponse>
        where TPropertiesResponse : PropertiesBaseResponseDto
    {
        resource?.Metadata.Should().NotBeNull();
        resource?.Metadata?.Id.Should().Be(model.Id);
        resource?.Metadata?.Uri.Should().Be(model.Uri);
        resource?.Metadata?.Category?.Should().NotBeNull();
        resource?.Metadata?.Category?.Name.Should().Be(model.Category.Name);
        resource?.Metadata?.Category?.Provider.Should().Be(model.Category.Provider);
        resource?.Metadata?.Category?.Typology.Should().NotBeNull();
        resource?.Metadata?.Category?.Typology?.Id.Should().Be(model.Category.Typology.Id);
        resource?.Metadata?.Category?.Typology?.Name.Should().Be(model.Category.Typology.Name);
        resource?.Metadata?.Location.Should().NotBeNull();
        resource?.Metadata?.Location?.Name.Should().Be(model.Location.Name);
        resource?.Metadata?.Location?.City.Should().Be(model.Location.City);
        resource?.Metadata?.Location?.Code.Should().Be(model.Location.Code);
        resource?.Metadata?.Location?.Country.Should().Be(model.Location.Country);
        resource?.Metadata?.Location?.Value.Should().Be(model.Location.Value);
        resource?.Metadata?.CreatedBy.Should().Be(model.CreatedBy);
        resource?.Metadata?.CreationDate.Should().Be(model.CreationDate);
        resource?.Metadata?.Ecommerce.Should().BeNull();
        resource?.Metadata?.Name.Should().Be(model.Name);
        resource?.Metadata?.Project.Should().NotBeNull();
        resource?.Metadata?.Project?.Id.Should().Be(model.Project.Id);
        resource?.Metadata?.Tags.Should().BeNull();
        resource?.Metadata?.UpdateDate.Should().Be(model.UpdateDate);
        resource?.Metadata?.UpdatedBy.Should().Be(updatedBy);
        resource?.Metadata?.Uri.Should().Be(model.Uri);
        resource?.Metadata?.Version.Should().Be(model.Version?.Data?.Current.ToString(CultureInfo.InvariantCulture));
        resource?.Status.Should().NotBeNull();
        resource?.Status?.State.Should().Be(model.Status.State);
        resource?.Status?.CreationDate.Should().Be(model.Status.CreationDate);
    }

    internal static T CreateBaseLegacyResourceListitem<T>(params LegacyComponent[] components) where T : LegacyResourceListItem
    {
        var ret = Activator.CreateInstance<T>();
        ret.Id = 1;
        ret.Name = "Resource1";
        ret.IncludedInFolders = new List<string>() { "Folder 1", "Folder 2" };
        ret.ExpirationDate = DateTime.UtcNow;
        return ret;
    }
    internal static void ValidateBaseLegacyResourceListitem<TLegacyResourceListitem, TResponse>(TLegacyResourceListitem model, TResponse resource, string updatedBy)
        where TLegacyResourceListitem : LegacyResourceListItem
        where TResponse : IResourceBase
    {
        resource?.Id.Should().Be(model.Id.ToString(CultureInfo.InvariantCulture));

        resource?.CreatedBy.Should().Be(updatedBy);
        resource?.Ecommerce.Should().BeNull();
        resource?.Name.Should().Be(model.Name);
        resource?.Project.Should().NotBeNull();
        resource?.Tags.Should().HaveCount(0);
        resource?.Version.Should().NotBeNull();
        resource?.Version.Data.Should().NotBeNull();
        resource?.Version.Data.Current.Should().Be(1);
        resource?.Status.Should().NotBeNull();
        resource?.Status?.State.Should().Be(StatusValues.Active.Value);
    }

    internal static T CreateBaseLegacyResource<T>(params LegacyComponent[] components) where T : LegacyResourceDetail
    {
        var ret = Activator.CreateInstance<T>();
        ret.Id = 1;
        ret.Name = "Resource1";
        ret.IncludedInFolders = new List<string>() { "Folder 1", "Folder 2" };
        ret.ExpirationDate = DateTime.UtcNow;
        ret.MonthlyUnitPrice = 1;
        ret.ActivationDate = DateTime.UtcNow;
        ret.AutoRenewEnabled = true;
        ret.RenewAllowed = true;
        ret.Components = components;
        return ret;
    }
    internal static void ValidateBaseLegacyResource<TLegacyResourceDetail, TResponse>(TLegacyResourceDetail model, TResponse resource, string updatedBy)
        where TLegacyResourceDetail : LegacyResourceDetail
        where TResponse : IResourceBase
    {
        resource?.Id.Should().Be(model.Id.ToString(CultureInfo.InvariantCulture));

        resource?.CreatedBy.Should().Be(updatedBy);
        resource?.CreationDate.Should().Be(model.ActivationDate);
        resource?.Ecommerce.Should().BeNull();
        resource?.Name.Should().Be(model.Name);
        resource?.Project.Should().NotBeNull();
        resource?.Tags.Should().HaveCount(0);
        resource?.UpdateDate.Should().Be(model.ActivationDate);
        resource?.Version.Should().NotBeNull();
        resource?.Version.Data.Should().NotBeNull();
        resource?.Version.Data.Current.Should().Be(1);
        resource?.Status.Should().NotBeNull();
        resource?.Status?.State.Should().Be(StatusValues.Active.Value);
        resource?.Status?.CreationDate.Should().Be(model.ActivationDate);
    }



    internal static void ValidateBaseBaremetalResource<TModel>(TModel model, ResourceBaseData resource, string updatedBy)
        where TModel : IResourceBase
    {
        resource?.Should().NotBeNull();
        resource?.Id.Should().Be(model.Id);
        resource?.Uri.Should().Be(model.Uri);
        resource?.Category?.Should().NotBeNull();
        resource?.Category?.Name.Should().Be(model.Category.Name);
        resource?.Category?.Typology.Should().NotBeNull();
        resource?.Category?.Typology?.Id.Should().Be(model.Category.Typology.Id);
        resource?.Category?.Typology?.Name.Should().Be(model.Category.Typology.Name);
        resource?.Location.Should().NotBeNull();
        resource?.Location?.Name.Should().Be(model.Location.Name);
        resource?.Location?.City.Should().Be(model.Location.City);
        resource?.Location?.Code.Should().Be(model.Location.Code);
        resource?.Location?.Country.Should().Be(model.Location.Country);
        resource?.Location?.Value.Should().Be(model.Location.Value);
        resource?.CreatedBy.Should().Be(model.CreatedBy);
        resource?.CreationDate.Should().Be(model.CreationDate);
        resource?.Name.Should().Be(model.Name);
        resource?.Project.Should().NotBeNull();
        resource?.Project?.Id.Should().Be(model.Project.Id);
        if (model.Tags == null)
        {
            resource?.Tags?.Should().BeNull();
        }
        else
        {
            resource?.Tags?.Should().HaveCount(model.Tags.Count);
        }
        resource?.UpdateDate.Should().Be(model.UpdateDate);
        resource?.UpdatedBy.Should().Be(updatedBy);
        resource?.Uri.Should().Be(model.Uri);
        resource?.Version?.Data?.Current.Should().Be(model.Version?.Data?.Current);
        resource?.Version?.Data?.Previous.Should().Be(model.Version?.Data?.Previous);
        resource?.Status.Should().NotBeNull();
        resource?.Status?.State.Should().Be(model.Status.State);
        resource?.Status?.CreationDate.Should().Be(model.Status.CreationDate);
    }

    #endregion
}
