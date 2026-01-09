using Aruba.CmpService.BaremetalProvider.Abstractions.Interfaces.Services;
using Aruba.CmpService.BaremetalProvider.Abstractions.Models;
using Aruba.CmpService.BaremetalProvider.Abstractions.UseCases.Internal;
using Aruba.CmpService.BaremetalProvider.Abstractions.UseCases.Internal.Requests;
using Aruba.CmpService.BaremetalProvider.Abstractions.UseCases.Internal.Responses;
using Aruba.MessageBus.UseCases;

namespace Aruba.CmpService.BaremetalProvider.Tests.UseCases.Internal;
public class InternalUseCaseWrappers
{
    public class InternalAdminGetResourcesUseCaseWrapper :
        InternalAdminGetResourcesUseCase
    {
        public InternalAdminGetResourcesUseCaseWrapper(IInternalService internalService) :
            base(internalService)
        {

        }
        public async Task<HandlerResult<InternalAdminGetResourcesUseCaseResponse>> Execute(InternalAdminGetResourcesUseCaseRequest request)
        {
            return await base.HandleAsync(request, CancellationToken.None).ConfigureAwait(false);
        }
    }

    public class InternalAutorechargeUseCaseWrapper :
        InternalAutorechargeUseCase
    {
        public InternalAutorechargeUseCaseWrapper(IInternalService internalService) :
            base(internalService)
        {

        }
        public async Task<HandlerResult<InternalAutorechargeUseCaseResponse>> Execute(InternalAutorechargeUseCaseRequest request)
        {
            return await base.HandleAsync(request, CancellationToken.None).ConfigureAwait(false);
        }
    }
    
    public class InternalGetRegionsUseCaseWrapper :
        InternalGetRegionsUseCase
    {
        public InternalGetRegionsUseCaseWrapper(IInternalService internalService) :
            base(internalService)
        {

        }
        public async Task<HandlerResult<InternalGetRegionsUseCaseResponse>> Execute(InternalGetRegionsUseCaseRequest request)
        {
            return await base.HandleAsync(request, CancellationToken.None).ConfigureAwait(false);
        }
    }
    
    public class InternalGetResourcesUseCaseWrapper :
        InternalGetResourcesUseCase
    {
        public InternalGetResourcesUseCaseWrapper(IInternalService internalService) :
            base(internalService)
        {

        }
        public async Task<HandlerResult<InternalGetResourcesUseCaseResponse>> Execute(InternalGetResourcesUseCaseRequest request)
        {
            return await base.HandleAsync(request, CancellationToken.None).ConfigureAwait(false);
        }
    }

    public class InternalAutomaticRenewUseCaseWrapper :
        InternalAutomaticRenewUseCase
    {
        public InternalAutomaticRenewUseCaseWrapper(IInternalService internalService) :
            base(internalService)
        {

        }
        public async Task<ServiceResult> Execute(InternalAutomaticRenewUseCaseRequest request)
        {
            return await base.ExecuteService(request, CancellationToken.None).ConfigureAwait(false);
        }
    }
}
