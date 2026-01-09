using Aruba.CmpService.BaremetalProvider.Abstractions.Interfaces.Jobs;
using Aruba.CmpService.BaremetalProvider.Abstractions.Interfaces.Services;
using Aruba.CmpService.BaremetalProvider.Abstractions.Utils;
using Hangfire;
using Microsoft.Extensions.Logging;

namespace Aruba.CmpService.BaremetalProvider.Infrastructure.Jobs;
public class DeleteTokensJob:IDeleteTokensJob
{
    private readonly ITokenProvider tokenProvider;
    private readonly ILogger<DeleteTokensJob> logger;
    private readonly IRecurringJobManager recurringJobManager;

    public DeleteTokensJob(ITokenProvider tokenProvider,ILogger<DeleteTokensJob> logger, IRecurringJobManager  recurringJobManager)
    {
        this.tokenProvider = tokenProvider;
        this.logger = logger;
        this.recurringJobManager = recurringJobManager;
    }

    public void Execute()
    {
        Log.LogWarning(logger, "DeleteTokensJob > execute {time}", DateTime.UtcNow);
        recurringJobManager.AddOrUpdate(
            "delete-tokens-job", // Identificativo univoco del job
            () => tokenProvider.DeleteExpiredTokens(),
            "00 03 * * *" // Cron expression per le 03:00 ogni giorno
        );
    }
}
