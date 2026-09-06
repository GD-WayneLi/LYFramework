using System.Collections.Generic;
using System.Threading.Tasks;
using LYFramework.Log;
using LYUnity.Utility.Unity;
using YooAsset;

namespace Demo
{
    public class YooAssetUtility
    {
        public static void Init()
        {
            YooAssets.Initialize();
        }

        public static async ValueTask LoadBootPage(EPlayMode playMode, string packageName, string defaultHostServer = "",
            string fallbackHostServer = "")
        {
            // 初始化package
            var ret = await InitPackage(playMode, packageName, defaultHostServer, fallbackHostServer);
            if (!ret)
            {
                // 失败
                return;
            }

            // 取本地版本号，如果没有取远程版本号
            var version = UnityLocalStorage.GetString("packageVersion");
            if (string.IsNullOrEmpty(version))
            {
                // 请求版本号
                version = await RequestPackageVersion(packageName);
                if (string.IsNullOrEmpty(version))
                {
                    // 没取到
                    return;
                }
            }

            // 更新包资源清单
            await UpdatePackageManifest(packageName, version);

            var downloader = CreateDownloader(packageName);
            if (downloader.TotalDownloadCount != 0)
            {
                if (!await StartDownload(downloader))
                {
                    // 失败
                    return;
                }
            }

            UnityLocalStorage.SetString("packageVersion", version);
            UnityLocalStorage.Save();
        }

        public static async ValueTask Update(EPlayMode playMode, string packageName, string defaultHostServer = "",
            string fallbackHostServer = "")
        {
            // 初始化package
            var ret = await InitPackage(playMode, packageName, defaultHostServer, fallbackHostServer);
            if (!ret)
            {
                // 失败
                return;
            }

            // 请求版本号
            var version = await RequestPackageVersion(packageName);
            if (string.IsNullOrEmpty(version))
            {
                // 没取到
                return;
            }

            // 更新包资源清单
            await UpdatePackageManifest(packageName, version);

            var downloader = CreateDownloader(packageName);
            if (downloader.TotalDownloadCount != 0)
            {
                if (!await StartDownload(downloader))
                {
                    // 失败
                    return;
                }
            }

            await ClearCache(packageName);
        }

        private static async ValueTask<bool> InitPackage(EPlayMode playMode, string packageName,
            string defaultHostServer, string fallbackHostServer)
        {
            // Create package.
            if (!YooAssets.TryGetPackage(packageName, out var package))
                package = YooAssets.CreatePackage(packageName);

            // Editor simulation mode.
            InitializePackageOperation initializationOperation = null;
            if (playMode == EPlayMode.EditorSimulateMode)
            {
                var buildResult = EditorSimulateBuildInvoker.Build(packageName, (int)EBundleType.VirtualAssetBundle);
                var packageRoot = buildResult.PackageRootDirectory;
                var createParameters = new EditorSimulateModeOptions();
                createParameters.EditorFileSystemParameters =
                    FileSystemParameters.CreateDefaultEditorFileSystemParameters(packageRoot);
                createParameters.EditorFileSystemParameters.AddParameter(EFileSystemParameter.VirtualWebglMode, true);
                createParameters.EditorFileSystemParameters.AddParameter(EFileSystemParameter.VirtualDownloadMode,
                    true);
                createParameters.EditorFileSystemParameters.AddParameter(EFileSystemParameter.VirtualDownloadSpeed,
                    1024 * 1000);
                createParameters.EditorFileSystemParameters.AddParameter(EFileSystemParameter.AsyncSimulateMinFrame, 5);
                createParameters.EditorFileSystemParameters.AddParameter(EFileSystemParameter.AsyncSimulateMaxFrame,
                    10);
                initializationOperation = package.InitializePackageAsync(createParameters);
            }

            // Offline play mode.
            if (playMode == EPlayMode.OfflinePlayMode)
            {
                var createParameters = new OfflinePlayModeOptions();
                createParameters.BuiltinFileSystemParameters =
                    FileSystemParameters.CreateDefaultBuiltinFileSystemParameters();
                initializationOperation = package.InitializePackageAsync(createParameters);
            }

            // Host play mode.
            if (playMode == EPlayMode.HostPlayMode)
            {
                IRemoteService remoteService = new RemoteService(defaultHostServer, fallbackHostServer);
                var createParameters = new HostPlayModeOptions();
                createParameters.BuiltinFileSystemParameters =
                    FileSystemParameters.CreateDefaultBuiltinFileSystemParameters();
                createParameters.BuiltinFileSystemParameters.AddParameter(
                    EFileSystemParameter.CopyBuiltinPackageManifest, true);
                createParameters.CacheFileSystemParameters =
                    FileSystemParameters.CreateDefaultSandboxFileSystemParameters(remoteService);
                createParameters.CacheFileSystemParameters.AddParameter(EFileSystemParameter.DownloadMaxConcurrency, 5);
                createParameters.CacheFileSystemParameters.AddParameter(EFileSystemParameter.DownloadMaxRequestPerFrame,
                    1);
                createParameters.CacheFileSystemParameters.AddParameter(EFileSystemParameter.DownloadWatchdogTimeout,
                    10);
                initializationOperation = package.InitializePackageAsync(createParameters);
            }

            // Web play mode.
            if (playMode == EPlayMode.WebPlayMode)
            {
#if UNITY_WEBGL && (WEIXINMINIGAME || UNITY_WECHATMINIGAME) && !UNITY_EDITOR
            var createParameters = new WebPlayModeOptions();
            string defaultHostServer = GetHostServerURL();
            string fallbackHostServer = GetHostServerURL();
            string packageRoot =
 $"{WeChatWASM.WX.env.USER_DATA_PATH}/__GAME_FILE_CACHE"; // Change this path if subdirectories are required.
            IRemoteService remoteService = new RemoteService(defaultHostServer, fallbackHostServer);
            createParameters.WebNetworkFileSystemParameters =
 WechatFileSystemCreater.CreateFileSystemParameters(packageRoot, remoteService);
            initializationOperation = package.InitializePackageAsync(createParameters);
#else
                var createParameters = new WebPlayModeOptions();
                createParameters.WebServerFileSystemParameters =
                    FileSystemParameters.CreateDefaultWebServerFileSystemParameters();
                initializationOperation = package.InitializePackageAsync(createParameters);
#endif
            }

            await initializationOperation;

            if (initializationOperation.Status != EOperationStatus.Succeeded)
            {
                LYLogger.Error($"{initializationOperation.Error}");
            }

            return initializationOperation.Status == EOperationStatus.Succeeded;
        }

        private static async ValueTask<string> RequestPackageVersion(string packageName)
        {
            var package = YooAssets.GetPackage(packageName);
            var operation = package.RequestPackageVersionAsync();
            await operation;

            if (operation.Status == EOperationStatus.Succeeded)
            {
                //请求成功
                string packageVersion = operation.PackageVersion;
                LYLogger.Log($"Request package Version : {packageVersion}");
                return packageVersion;
            }
            else
            {
                //请求失败
                LYLogger.Error(operation.Error);
            }

            return "";
        }

        private static async ValueTask<bool> UpdatePackageManifest(string packageName, string packageVersion)
        {
            var package = YooAssets.GetPackage(packageName);
            var operation = package.LoadPackageManifestAsync(new LoadPackageManifestOptions(packageVersion, 60));
            await operation;

            if (operation.Status != EOperationStatus.Succeeded)
            {
                //更新失败
                LYLogger.Error(operation.Error);
            }

            return operation.Status == EOperationStatus.Succeeded;
        }

        private static ResourceDownloaderOperation CreateDownloader(string packageName)
        {
            var package = YooAssets.GetPackage(packageName);
            int downloadingMaxNum = 10;
            int failedTryAgain = 3;
            var options = new ResourceDownloaderOptions(downloadingMaxNum, failedTryAgain);
            var downloader = package.CreateResourceDownloader(options);


            return downloader;
        }

        private static async ValueTask<bool> StartDownload(ResourceDownloaderOperation downloader)
        {
            downloader.DownloadError += OnDownloadError;
            downloader.DownloadProgressChanged += OnDownloadProgressChanged;
            downloader.StartDownload();

            await downloader;

            downloader.DownloadError -= OnDownloadError;
            downloader.DownloadProgressChanged -= OnDownloadProgressChanged;

            return downloader.Status == EOperationStatus.Succeeded;
        }

        private static async ValueTask ClearCache(string packageName)
        {
            var package = YooAssets.GetPackage(packageName);
            var options = new ClearCacheOptions(ClearCacheMethods.ClearUnusedBundleFiles);
            var operation = package.ClearCacheAsync(options);
            await operation;
        }

        private static void OnDownloadError(DownloadErrorEventArgs args)
        {
            LYLogger.Error(args.ErrorInfo);
        }

        private static void OnDownloadProgressChanged(DownloadProgressChangedEventArgs args)
        {
            // todo:发送进度事件
            LYLogger.Info($"downloadProgress: {args.Progress}");
        }

        /// <summary>
        /// Remote resource URL query service.
        /// </summary>
        private class RemoteService : IRemoteService
        {
            private readonly string _defaultHostServer;
            private readonly string _fallbackHostServer;

            public RemoteService(string defaultHostServer, string fallbackHostServer)
            {
                _defaultHostServer = defaultHostServer;
                _fallbackHostServer = fallbackHostServer;
            }

            public IReadOnlyList<string> GetRemoteUrls(string fileName)
            {
                List<string> result = new List<string>();
                result.Add($"{_defaultHostServer}/{fileName}");
                result.Add($"{_fallbackHostServer}/{fileName}");
                return result;
            }
        }
    }
}