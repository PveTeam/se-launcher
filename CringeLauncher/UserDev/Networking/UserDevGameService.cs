using VRage.GameServices;

namespace CringeLauncher.UserDev.Networking;

public class UserDevGameService(uint appId) : IMyGameService
{
    private readonly List<MyPackage> m_packageIds = [];

    public uint AppId { get; } = appId;

    public bool IsActive => true;

    public bool IsOnline { get; }

    public bool IsOverlayEnabled { get; }

    public bool IsOverlayBrowserAvailable { get; }

    public bool OwnsGame { get; }

    public ulong UserId
    {
        get => 1234567891011;
        set
        {
        }
    }

    public ulong OnlineUserId => UserId;

    public char PlatformIcon => VRage.GameServices.PlatformIcon.PC;

    public string UserName => "Dev";

    public string OnlineName => UserName;

    public bool CanQuickLaunch { get; }

    public void QuickLaunch()
    {
    }

    public MyGameServiceUniverse UserUniverse => MyGameServiceUniverse.Dev;

    public string BranchName => "userdev";

    public string BranchNameFriendly => "UserDev";

    public bool IsSteamDeck => false;

    public uint AvailableFlexMemory { get; }
    public event Action<bool>? OnOverlayActivated;

    public event Action<uint>? OnDLCInstalled;

    public event Action<bool>? OnUserChanged;

    public event Action<bool>? OnSignInStateChanged;

    public event Action<string>? OnActivityLaunch;

    public event Action? OnUpdate;

    public event Action<bool>? OnUpdateNetworkThread;

    public event Action? OnLobbyStart;

    public event Action? OnSessionUnload;

    public void OpenOverlayUrl(string url, bool predetermined = true)
    {
    }

    public void SetNotificationPosition(NotificationPosition notificationPosition)
    {
    }

    public void ShutDown()
    {
    }

    public bool IsAppInstalled(uint appId) => true;

    public void OpenDlcInShop(uint dlcId)
    {
    }

    public void OpenInventoryItemInShop(int itemId)
    {
    }

    public bool IsDlcSupported(uint dlcId) => true;

    public bool IsDlcInstalled(uint dlcId) => true;

    public void AddDlcPackages(List<MyDlcPackage> packages)
    {
        m_packageIds.Clear();
        foreach (var package in packages)
        {
            if (!string.IsNullOrEmpty(package.XboxPackageId) && !string.IsNullOrEmpty(package.XboxStoreId))
                m_packageIds.Add(new MyPackage
                {
                    DlcId = package.AppId,
                    PackageId = package.XboxPackageId,
                    StoreId = package.XboxStoreId
                });
        }
    }

    public int GetDLCCount() => m_packageIds.Count;

    public bool GetDLCDataByIndex(
        int index,
        out uint dlcId,
        out bool available,
        out string name,
        int nameBufferSize)
    {
        if (index >= m_packageIds.Count)
        {
            dlcId = 0U;
            available = false;
            name = string.Empty;
            return false;
        }
        available = true;
        dlcId = m_packageIds[index].DlcId;
        name = "Dev" + index;
        return true;
    }

    public void OpenOverlayUser(ulong id)
    {
    }

    public bool GetAuthSessionTicket(out uint ticketHandle, byte[] buffer, out uint length)
    {
        ticketHandle = 0U;
        length = 0U;
        return false;
    }

    public void LoadStats()
    {
    }

    public IMyAchievement? GetAchievement(string achievementName, string statName, float maxValue)
    {
        return null;
    }

    public IMyAchievement? GetAchievement(string achievementName) => null;

    public void RegisterAchievement(string achievementName, string xblId)
    {
    }

    public void ResetAllStats(bool achievementsToo)
    {
    }

    public void StoreStats()
    {
    }

    public string GetPersonaName(ulong userId) => string.Empty;

    public bool HasFriend(ulong userId) => false;

    public string GetClanName(ulong groupId) => string.Empty;

    public void Update()
    {
        OnUpdate?.Invoke();
    }

    public void UpdateNetworkThread(bool sessionEnabled)
    {
        OnUpdateNetworkThread?.Invoke(sessionEnabled);
    }

    public bool IsUserInGroup(ulong groupId) => false;

    public bool GetRemoteStorageQuota(out ulong totalBytes, out ulong availableBytes)
    {
        totalBytes = 0UL;
        availableBytes = 0UL;
        return false;
    }

    public int GetRemoteStorageFileCount() => 0;

    public string GetRemoteStorageFileNameAndSize(int fileIndex, out int fileSizeInBytes)
    {
        fileSizeInBytes = 0;
        return string.Empty;
    }

    public bool IsRemoteStorageFilePersisted(string file) => false;

    public bool RemoteStorageFileForget(string file) => false;

    public ulong CreatePublishedFileUpdateRequest(ulong publishedFileId) => 0;

    public void UpdatePublishedFileTags(ulong updateHandle, string[] tags)
    {
    }

    public void UpdatePublishedFileFile(ulong updateHandle, string steamItemFileName)
    {
    }

    public void UpdatePublishedFilePreviewFile(ulong updateHandle, string steamPreviewFileName)
    {
    }

    public void FileDelete(string steamItemFileName)
    {
    }

    public bool FileExists(string fileName) => false;

    public int GetFileSize(string fileName) => 0;

    public ulong FileWriteStreamOpen(string fileName) => 0;

    public void FileWriteStreamWriteChunk(ulong handle, byte[] buffer, int size)
    {
    }

    public void FileWriteStreamClose(ulong handle)
    {
    }

    public void CommitPublishedFileUpdate(
        ulong updateHandle,
        Action<bool, MyRemoteStorageUpdatePublishedFileResult> onCallResult)
    {
    }

    public void PublishWorkshopFile(
        string file,
        string previewFile,
        string title,
        string description,
        string longDescription,
        MyPublishedFileVisibility visibility,
        string[] tags,
        Action<bool, MyRemoteStoragePublishFileResult> onCallResult)
    {
    }

    public void SubscribePublishedFile(
        ulong publishedFileId,
        Action<bool, MyRemoteStorageSubscribePublishedFileResult> onCallResult)
    {
    }

    public void FileShare(
        string file,
        Action<bool, MyRemoteStorageFileShareResult> onCallResult)
    {
    }

    public string ServiceName => nameof (UserDevGameService);

    public string ServiceDisplayName => "UserDev Game Service";

    public bool OpenProfileForMute { get; }

    public int GetFriendsCount() => 0;

    public ulong GetFriendIdByIndex(int index) => 0;

    public string GetFriendNameByIndex(int index) => string.Empty;

    public void SaveToCloudAsync(
        string storageName,
        byte[] buffer,
        Action<CloudResult>? completedAction)
    {
        completedAction?.Invoke(CloudResult.Failed);
    }

    public CloudResult SaveToCloud(string fileName, byte[] buffer) => CloudResult.Failed;

    public CloudResult SaveToCloud(string containerName, List<MyCloudFile> fileNames)
    {
        return CloudResult.Failed;
    }

    public bool LoadFromCloudAsync(string fileName, Action<byte[]> completedAction) => false;

    public List<MyCloudFileInfo>? GetCloudFiles(string directoryFilter)
    {
        return null;
    }

    public byte[]? LoadFromCloud(string fileName) => null;

    public bool DeleteFromCloud(string fileName) => false;

    public bool IsProductOwned(uint productId, out DateTime? purchaseTime)
    {
        purchaseTime = null;
        return false;
    }

    public void RequestEncryptedAppTicket(string url, Action<bool, string> onDone)
    {
    }

    public void RequestAuthToken(string clientId, Action<bool, string, int> onDone)
    {
    }

    public void RequestPermissions(
        Permissions permission,
        bool attemptResolution,
        Action<PermissionResult>? onDone)
    {
        onDone?.Invoke(PermissionResult.Granted);
    }

    public void RequestPermissionsWithTargetUser(
        Permissions permission,
        ulong userId,
        Action<PermissionResult>? onDone)
    {
        onDone?.Invoke(PermissionResult.Granted);
    }

    public void OnThreadpoolInitialized()
    {
    }

    public bool GetInstallStatus(out int percentage)
    {
        percentage = 100;
        return true;
    }

    public void Trace(bool enable)
    {
    }

    public void SetPlayerMuted(ulong playerId, bool muted)
    {
    }

    public ulong[]? GetBlockListRaw() => null;

    HashSet<ulong>? IMyGameService.GetBlockList() => null;

    public ulong[]? GetBlockList() => null;

    public bool IsPlayerMuted(ulong playerId) => false;

    public void UpdateMutedPlayers(Action onDone) => onDone.InvokeIfNotNull();

    public MyGameServiceAccountType GetServerAccountType(ulong steamId)
    {
        return MyGameServiceAccountType.Invalid;
    }

    public void DeleteUnnecessaryFilesFromTempFolder()
    {
    }

    public void OnSessionLoaded(string campaignName, string currentMissionName)
    {
    }

    public void OnSessionReady(bool multiplayer, bool dedicated)
    {
    }

    public void OnLoadingScreenCompleted()
    {
    }

    public void OnGameSaved(bool success, string savePath)
    {
    }

    public void OnCampaignFinishing()
    {
    }

    public void LobbyStarts() => OnLobbyStart.InvokeIfNotNull();

    public void OnMissionFinished(string missionName)
    {
    }

    public void OnSessionUnloaded() => OnSessionUnload.InvokeIfNotNull();

    public void OnSessionUnloading()
    {
    }

    public bool ActivityInProgress { get; }

    public LoadActivityResult GetActivityLoadInformation(string activityId)
    {
        return new LoadActivityResult();
    }

    public void OnPlayersChanged(int playersCount)
    {
    }

    public ulong GetModsCacheFreeSpace() => ulong.MaxValue;

    public void FormatModsCache()
    {
    }

    public void PrintStats()
    {
    }

    public bool IsPlayerBlocked(ulong playerId) => false;

    private struct MyPackage
    {
        public uint DlcId;
        public string PackageId;
        public string StoreId;
    }
}