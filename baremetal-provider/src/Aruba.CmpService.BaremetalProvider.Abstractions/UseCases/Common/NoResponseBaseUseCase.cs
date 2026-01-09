using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using Aruba.CmpService.BaremetalProvider.Abstractions.Extensions;
using Aruba.CmpService.BaremetalProvider.Abstractions.Interfaces.Services;
using Aruba.CmpService.BaremetalProvider.Abstractions.MessageBus;
using Aruba.CmpService.BaremetalProvider.Abstractions.Models;
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
    public abstract class NoResponseBaseUseCase<TRequest, TResponse> : UseCase<TRequest, TResponse>
    where TRequest : BaseUseCaseRequest
    where TResponse : EmptyMessageBusResponse
    {
        protected override async Task<HandlerResult<TResponse>> HandleAsync([NotNull] TRequest request, CancellationToken cancellationToken)
        {
            request.ThrowIfNull();
            request.ResourceId.ThrowIfNull();

            await AdditionalNullChecks(request).ConfigureAwait(false);

            var ret = await ExecuteService(request, cancellationToken).ConfigureAwait(false);

            return this.ExecutedResult(ret);
        }

        protected abstract Task<ServiceResult> ExecuteService(TRequest request, CancellationToken cancellationToken);

        protected async virtual Task AdditionalNullChecks(TRequest request)
        {
            await Task.CompletedTask.ConfigureAwait(false);
        }
    }
}
