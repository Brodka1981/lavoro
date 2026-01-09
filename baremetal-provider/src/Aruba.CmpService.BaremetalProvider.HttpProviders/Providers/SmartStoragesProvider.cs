using System.Globalization;
using System.Security.Cryptography;
using Aruba.CmpService.BaremetalProvider.Abstractions.Dtos.SmartStorages;
using Aruba.CmpService.BaremetalProvider.Abstractions.Extensions;
using Aruba.CmpService.BaremetalProvider.Abstractions.Interfaces.Providers;
using Aruba.CmpService.BaremetalProvider.Abstractions.Models;
using Aruba.CmpService.BaremetalProvider.Abstractions.Models.SmartStorages;
using Aruba.CmpService.BaremetalProvider.Abstractions.Providers.Models;
using Aruba.CmpService.BaremetalProvider.Abstractions.Providers.Models.Legacy;
using Aruba.CmpService.BaremetalProvider.Abstractions.Providers.Models.Legacy.SmartStorages;
using Aruba.CmpService.BaremetalProvider.Abstractions.Utils;
using Aruba.CmpService.BaremetalProvider.HttpProviders.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Throw;

namespace Aruba.CmpService.BaremetalProvider.HttpProviders.Providers;

public class SmartStoragesProvider :
    LegacyProvider<LegacySmartStorageListItem, LegacySmartStorageDetail>,
    ISmartStoragesProvider
{
    private static readonly byte[] SALT = new byte[] { 0x26, 0xdc, 0xff, 0x00, 0xad, 0xed, 0x7a, 0xee, 0xc5, 0xfe, 0x07, 0xaf, 0x4d, 0x08, 0x22, 0x3c };

    private readonly IOptions<BaremetalOptions> options;

    public SmartStoragesProvider(
        IHttpClientFactory httpClientFactory,
        ILogger<SmartStoragesProvider> logger,
        IOptions<BaremetalOptions> options) : base(httpClientFactory, logger)
    {
        this.options = options;
    }

    public async override Task<ApiCallOutput<bool>> DeleteAutomaticRenew(ResourceDeleteAutomaticRenew deleteAutomaticRenew)
    {
        return await base.DeleteAutomaticRenewInternal($"/dc-services/api/clouddcssmartstorages/postsetautorenew", deleteAutomaticRenew).ConfigureAwait(false);
    }

    public override async Task<ApiCallOutput<LegacySmartStorageDetail>> GetById(long id)
    {
        var result = await this.GetById($"/dc-services/api/clouddcssmartstorages/getDetail?id={id}").ConfigureAwait(false);

        return result;
    }

    public override async Task<ApiCallOutput<IEnumerable<LegacyCatalog>>> GetCatalog()
    {
        return await base.GetCatalog($"/dc-services/api/clouddcssmartstorages/getavailablemodels").ConfigureAwait(false);
    }

    public override async Task<ApiCallOutput<bool>> Rename(ResourceRename resourceRename)
    {
        using var httpClient = this.CreateHttpClient();
        return await httpClient.CallPostAsync<bool>($"/dc-services/api/CloudDcsSmartStorages/PostSetCustomName", resourceRename).ConfigureAwait(false);
    }

    public override async Task<ApiCallOutput<LegacyListResponse<LegacySmartStorageListItem>>> Search(LegacySearchFilters filterRequest)
    {
        filterRequest.ThrowIfNull();
        var fieldMapping = GetDefaultSortFieldMapping();

        var url = filterRequest.SetSort(fieldMapping).ToQueryString("/dc-services/api/clouddcssmartstorages/getlist");
        return await base.Search(url).ConfigureAwait(false);
    }

    public override async Task<ApiCallOutput<bool>> UpsertAutomaticRenew(ResourceUpsertAutomaticRenew upsertAutomaticRenew)
    {
        return await base.UpsertAutomaticRenewInternal($"/dc-services/api/clouddcssmartstorages/postsetautorenew", upsertAutomaticRenew).ConfigureAwait(false);
    }
    public async Task<ApiCallOutput<LegacySmartFolders>> GetSmartFolders(long id)
    {
        return await this.GetFolders($"/dc-services/api/CloudDcsSmartStorages/GetSmartFolders?id={id}").ConfigureAwait(false);
    }

    public async Task<ApiCallOutput<List<LegacyProtocol>>> GetProtolList(string id)
    {
        using var httpClient = this.CreateHttpClient();
        return await httpClient.CallGetAsync<List<LegacyProtocol>>($"/dc-services/api/clouddcssmartstorages/getservicesstatus?id={id}").ConfigureAwait(false);
    }

    public async Task<ApiCallOutput<bool>> ToggleProtocol(string id, ServiceType serviceType, bool enable)
    {
        using var httpClient = this.CreateHttpClient();
        var body = new
        {
            Id = long.Parse(id, CultureInfo.InvariantCulture),
            Enable = enable,
            ServiceType = (int)serviceType
        };

        Log.LogDebug(Logger, "ToggleProtocol: api call postenableservice body {body}", body.Serialize());

        return await httpClient.CallPostAsync<bool>($"/dc-services/api/clouddcssmartstorages/postenableservice", body).ConfigureAwait(false);
    }

    public async Task<ApiCallOutput<bool>> ActivateSmartStorage(string id, string password)
    {
        using var httpClient = this.CreateHttpClient();
        var body = new
        {
            Id = long.Parse(id, CultureInfo.InvariantCulture),
            Password = Encrypt(password, options.Value.CryptoKey!)
        };

        Log.LogInfo(Logger, "ActivateSmartStorage: api call PostSetPassword body {body}", body.Serialize());

        return await httpClient.CallPostAsync<bool>($"/dc-services/api/CloudDcsSmartStorages/PostSetPassword", body).ConfigureAwait(false);
    }

    public async Task<ApiCallOutput<LegacyStatistics>> GetStatistics(string id)
    {
        using var httpClient = this.CreateHttpClient();
        return await httpClient.CallGetAsync<LegacyStatistics>($"/dc-services/api/clouddcssmartstorages/getstatistics?id={id}").ConfigureAwait(false);
    }

    public async Task<ApiCallOutput<LegacySnapshots>> GetSnapshots(string id)
    {
        using var httpClient = this.CreateHttpClient();
        return await httpClient.CallGetAsync<LegacySnapshots>($"/dc-services/api/clouddcssmartstorages/getsnapshots?id={id}").ConfigureAwait(false);
    }

    public async Task<ApiCallOutput<bool>> DeleteSnapshot(string id, string snapshotId)
    {
        using var httpClient = this.CreateHttpClient();
        var body = new
        {
            Id = long.Parse(id, CultureInfo.InvariantCulture),
            SnapshotId = snapshotId
        };

        Log.LogDebug(Logger, "DeleteSnapshot: api call PostDeleteSnapshot body {body}", body.Serialize());

        return await httpClient.CallPostAsync<bool>($"/dc-services/api/clouddcssmartstorages/postdeletesnapshot", body).ConfigureAwait(false);
    }

    public async Task<ApiCallOutput<bool>> DeleteSnapshotTask(string id, Int32 snapshotId)
    {
        using var httpClient = this.CreateHttpClient();
        var body = new
        {
            Id = long.Parse(id, CultureInfo.InvariantCulture),
            SnapshotTaskId = snapshotId
        };

        Log.LogDebug(Logger, "DeleteSnapshot: api call PostDeleteSnapshotTask body {body}", body.Serialize());

        return await httpClient.CallPostAsync<bool>($"/dc-services/api/clouddcssmartstorages/postdeletesnapshottask", body).ConfigureAwait(false);
    }

    public async Task<ApiCallOutput<bool>> CreateSmartFolder(string id, string name)
    {
        using var httpClient = this.CreateHttpClient();
        var body = new
        {
            Id = long.Parse(id, CultureInfo.InvariantCulture),
            SmartFolderName = name
        };

        Log.LogDebug(Logger, "CreateSmartFolder: api call PostCreateSmartFolder body {body}", body.Serialize());

        return await httpClient.CallPostAsync<bool>($"/dc-services/api/clouddcssmartstorages/postcreatesmartfolder", body).ConfigureAwait(false);
    }

    public async Task<ApiCallOutput<bool>> DeleteSmartFolder(string id, string smartFolderId)
    {
        using var httpClient = this.CreateHttpClient();
        var body = new
        {
            Id = long.Parse(id, CultureInfo.InvariantCulture),
            SmartFolderId = smartFolderId
        };

        Log.LogInfo(Logger, "DeleteSmartFolder: api call PostDeleteSmartFolder body {body}", body.Serialize());

        return await httpClient.CallPostAsync<bool>($"/dc-services/api/clouddcssmartstorages/postdeletesmartfolder", body).ConfigureAwait(false);
    }

    public async Task<ApiCallOutput<bool>> CreateSnapshotTask(string id, SmartStorageCreateSnapshotTaskDto snapshot, bool isRootFolder = false)
    {
        snapshot.ThrowIfNull();
        using var httpClient = this.CreateHttpClient();
        // Aggiungo sempre i minuti
        var snapshotLegacy = new LegacyCreateSnapshotTask
        {
            id = int.Parse(id, CultureInfo.InvariantCulture),
            SmartFolderName = isRootFolder ? String.Empty : snapshot.FolderName,
            LifeTimeUnit = (int)snapshot.LifeTimeUnitType!,
            LifeTimeValue = snapshot.Quantity!.Value,
            Enabled = snapshot.Enabled,
            SnapshotSchedule = new LegacyCreateSnapshotTaskSchedule
            {
                Minutes = snapshot.Minute!.Value
            }
        };
        switch (snapshot.LifeTimeUnitType)
        {
            case SnapshotLifeTimeUnitTypes.Daily:
                snapshotLegacy.SnapshotSchedule.Hours = snapshot.Hour;
                break;

            case SnapshotLifeTimeUnitTypes.Weekly:
                snapshotLegacy.SnapshotSchedule.Hours = snapshot.Hour;
                snapshotLegacy.SnapshotSchedule.DayOfWeek = (byte)snapshot.DayOfWeek!;
                break;

            case SnapshotLifeTimeUnitTypes.Monthly:
                snapshotLegacy.SnapshotSchedule.Hours = snapshot.Hour;
                snapshotLegacy.SnapshotSchedule.DayOfMonth = (byte)snapshot.DaysOfMonth!;
                break;
        }
        Log.LogInfo(Logger, "CreateSnapshotTask: api call CloudDcsSmartStorages/PostCreateSnapshotTask body {body}", snapshotLegacy.Serialize());
        return await httpClient.CallPostAsync<bool>($"/dc-services/api/clouddcssmartstorages/postcreatesnapshottask", snapshotLegacy).ConfigureAwait(false);
    }

    public async Task<ApiCallOutput<bool>> CreateSnapshot(string id, string smartFolderName, bool isRootFolder = false)
    {
        using var httpClient = this.CreateHttpClient();
        var body = new
        {
            Id = long.Parse(id, CultureInfo.InvariantCulture),
            SmartFolderName = isRootFolder ? String.Empty : smartFolderName
        };

        Log.LogInfo(Logger, "CreateSnapshot: api call CreateSnapshot body {body}", body.Serialize());

        return await httpClient.CallPostAsync<bool>($"/dc-services/api/clouddcssmartstorages/postcreatesnapshot", body).ConfigureAwait(false);
    }

    public async Task<ApiCallOutput<bool>> UpdateSnapshotTask(string id, int snapshotTaskId, bool enable)
    {
        using var httpClient = this.CreateHttpClient();
        var body = new
        {
            Id = long.Parse(id, CultureInfo.InvariantCulture),
            SnapshotTaskID = snapshotTaskId,
            Enable = enable
        };

        Log.LogInfo(Logger, "UpdateSnapshotTask: api call PostEnableSnapshotTask body {body}", body.Serialize());

        return await httpClient.CallPostAsync<bool>($"/dc-services/api/clouddcssmartstorages/postenablesnapshottask", body).ConfigureAwait(false);

    }

    public async Task<ApiCallOutput<bool>> RestoreSnapshot(string id, string snapshotId)
    {
        using var httpClient = this.CreateHttpClient();
        var body = new
        {
            Id = long.Parse(id, CultureInfo.InvariantCulture),
            SnapshotID = snapshotId
        };

        Log.LogInfo(Logger, "ApplySnapshot: api call RestoreSnapshot body {body}", body.Serialize());

        return await httpClient.CallPostAsync<bool>($"/dc-services/api/clouddcssmartstorages/postrestoresnapshot", body).ConfigureAwait(false);
    }

    private static string Encrypt(string originalString, string cryptoKey)
    {
        if (string.IsNullOrWhiteSpace(originalString))
        {
            return string.Empty;
        }

        // Encrypt the string to an array of bytes. 
        return Convert.ToBase64String(EncryptStringToBytes(originalString, cryptoKey));
    }

    static byte[] EncryptStringToBytes(string plainText, string key)
    {
        // Check arguments. 
        if (plainText == null || plainText.Length <= 0)
            throw new ArgumentNullException("plainText");
        if (key == null || key.Length <= 0)
            throw new ArgumentNullException("key");
        byte[] encrypted;
        // Create an Rijndael object 
        // with the specified key and IV. 
        using (Rijndael rijAlg = Rijndael.Create())
        {
            Rfc2898DeriveBytes pdb = new Rfc2898DeriveBytes(key, SALT);
            rijAlg.Key = pdb.GetBytes(32);
            rijAlg.IV = pdb.GetBytes(16);

            // Create a decrytor to perform the stream transform.
            ICryptoTransform encryptor = rijAlg.CreateEncryptor(rijAlg.Key, rijAlg.IV);

            // Create the streams used for encryption. 
            using (MemoryStream msEncrypt = new MemoryStream())
            {
                using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                {
                    using (StreamWriter swEncrypt = new StreamWriter(csEncrypt))
                    {
                        //Write all data to the stream.
                        swEncrypt.Write(plainText);
                    }
                    encrypted = msEncrypt.ToArray();
                }
            }
        }

        // Return the encrypted bytes from the memory stream. 
        return encrypted;
    }
}
