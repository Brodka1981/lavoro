//using Aruba.CmpService.BaremetalProvider.Abstractions.Constants;
//using Aruba.CmpService.BaremetalProvider.Abstractions.Extensions;
//using Aruba.CmpService.BaremetalProvider.Abstractions.Models;
//using Aruba.CmpService.BaremetalProvider.Abstractions.UseCases.Common.Requests;
//using Aruba.CmpService.BaremetalProvider.Abstractions.UseCases.Common.Responses;
//using Aruba.CmpService.BaremetalProvider.Api.Code.Controllers;
//using Aruba.CmpService.BaremetalProvider.Api.Code.Swagger;
//using Aruba.CmpService.ResourceProvider.Common.Dtos.Shared;
//using Aruba.CmpService.ResourceProvider.Common.Messages.v1;
//using Aruba.MessageBus;
//using Aruba.MessageBus.Models;
//using Microsoft.AspNetCore.Mvc;

//namespace Aruba.CmpService.BaremetalProvider.Api.v1.Controllers;

//[Route("test/")]
//public partial class TestController : BaseController
//{
//    private readonly ILogger<TestController> logger;
//    private readonly IMessageBus messageBus;

//    public TestController(ILogger<TestController> logger, IMessageBus messageBus)
//    {
//        this.logger = logger;
//        this.messageBus = messageBus;
//    }

//    [HttpPost("serverdeploymentstatuschanged")]
//    [Produce200Family(StatusCodes.Status200OK)]
//    [AllowAnonymous]
//    public async Task ServerDeploymentStatusChanged(SendDeploymentStatusChanged model)
//    {
//        await this.DeploymentStatusChanged(model, Typologies.Server).ConfigureAwait(false);
//    }

//    [HttpPost("switchdeploymentstatuschanged")]
//    [Produce200Family(StatusCodes.Status200OK)]
//    [AllowAnonymous]
//    public async Task SwitchDeploymentStatusChanged(SendDeploymentStatusChanged model)
//    {
//        await this.DeploymentStatusChanged(model, Typologies.Swaas).ConfigureAwait(false);
//    }

//    [HttpPost("firewalldeploymentstatuschanged")]
//    [Produce200Family(StatusCodes.Status200OK)]
//    [AllowAnonymous]
//    public async Task FirewallDeploymentStatusChanged(SendDeploymentStatusChanged model)
//    {
//        await this.DeploymentStatusChanged(model, Typologies.Firewall).ConfigureAwait(false);
//    }

//    private async Task DeploymentStatusChanged(SendDeploymentStatusChanged model, Typologies typology)
//    {

//        var deploymentStatusChanged = new DeploymentStatusChanged();
//        deploymentStatusChanged.Status = new StatusData()
//        {
//            CreationDate = DateTimeOffset.UtcNow,
//            State = StatusValues.Active.Value
//        };
//        deploymentStatusChanged.CreatedBy = User.GetUserId();
//        deploymentStatusChanged.DeploymentId = model?.DeploymentId;
//        deploymentStatusChanged.Typology = new TypologyData()
//        {
//            Id = typology.Value,
//            Name = typology.Value.ToUpperInvariant(),
//        };

//        var envelope = new EnvelopeBuilder().WithSubject(deploymentStatusChanged.CreatedBy ?? deploymentStatusChanged.DeploymentId)
//            .Build(deploymentStatusChanged);


//        var request = new SendMessageUseCaseRequest()
//        {
//            MessageBusRequestId = Guid.NewGuid().ToString(),
//            EnvelopeToSend = envelope
//        };
//        var result = await messageBus.ExecuteAsync<SendMessageUseCaseRequest, SendMessageUseCaseResponse>(request, CancellationToken.None)
//            .ConfigureAwait(false);
//    }
//}

//public class SendDeploymentStatusChanged
//{
//    public string? DeploymentId { get; set; }
//}
