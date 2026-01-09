using System.Text.Json;
using Aruba.CmpService.BaremetalProvider.Abstractions.Extensions;
using Aruba.CmpService.BaremetalProvider.Abstractions.UseCases.Common.Requests;
using Aruba.CmpService.BaremetalProvider.Abstractions.UseCases.Common.Responses;
using Aruba.CmpService.BaremetalProvider.Abstractions.Utils;
using Aruba.MessageBus.Transactions;
using Aruba.MessageBus.UseCases;
using Microsoft.Extensions.Logging;
using Throw;

namespace Aruba.CmpService.BaremetalProvider.Abstractions.UseCases.Common
{

    [NonTransactional]
    public class SendMessageUseCase : UseCase<SendMessageUseCaseRequest, SendMessageUseCaseResponse>
    {
        private readonly ILogger<SendMessageUseCase> logger;

        public SendMessageUseCase(ILogger<SendMessageUseCase> logger)
        {
            this.logger = logger;
        }

        protected override Task<HandlerResult<SendMessageUseCaseResponse>> HandleAsync(SendMessageUseCaseRequest request, CancellationToken cancellationToken)
        {
            request.ThrowIfNull();
            request.EnvelopeToSend.ThrowIfNull();

            var outbox = new[] { request.EnvelopeToSend };
            Log.LogDebug(logger, "Send message {Message}", JsonSerializer.Serialize(request.EnvelopeToSend.Body.Serialize()));

            return Task.FromResult(HandlerResult.Executed<SendMessageUseCaseResponse>(new SendMessageUseCaseResponse.Success(true), outbox.ToArray()));
        }
    }
}
