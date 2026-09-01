using System.Threading.Tasks;
using Demo.Client.Model;
using Demo.Client.System.UI;
using LYFramework;
using LYFramework.Log;
using LYFramework.TaskEx;
using LYUnity.Resource;
using LYUnity.UI;
using UnityEngine;
using YooAsset;

namespace QFramework.Demo
{
    public class GameManager : GameManagerBase<GameManager>
    {
        GameObject m_Root;
        
        public void Init(GameObject root)
        {
            m_Root = root;
            
            Init();
        }
        
        public override void Init()
        {
            LYLogger.SetLogHelper(new DefaultLogHelper());

            RegisterSystems();
            
            InitAsync().Forget();
        }

        public void RegisterSystems()
        {
            RegisterSystem<ResourceLoaderSystem>(); // 资源加载器

            RegisterSystem<UIManagerSystem>();  // UI管理器
            RegisterSystem<UISystem>();

        }
        
        async ValueTask InitAsync()
        {
            YooAssets.Initialize();
            YooAssets.CreatePackage("DefaultPackage");

            await InitPackage();
            var version = await RequestPackageVersion();
            await UpdatePackageManifest(version);
            
            var domain = Game.World.AddDomain();
            
            // 初始化UI组件
            var uiManagerEntity = domain.AddChild();
            var uiManager = uiManagerEntity.AddComponent<UIManagerComponent, GameObject>(m_Root);
            uiManager.RegisterLayerGroup(new LayerGroup());

            await uiManager.OpenUI<UILoginComponent>("Assets/Bundles/Prefabs/UI/UILogin.prefab", 1);
        }
        
        private async ValueTask InitPackage()
        {  
            var package = YooAssets.GetPackage("DefaultPackage");
            var buildResult = EditorSimulateBuildInvoker.Build("DefaultPackage", (int)EBundleType.VirtualAssetBundle);
            var packageRoot = buildResult.PackageRootDirectory;
            var fileSystemParams = FileSystemParameters.CreateDefaultEditorFileSystemParameters(packageRoot);
    
            var createParameters = new EditorSimulateModeOptions();
            createParameters.EditorFileSystemParameters = fileSystemParams;
    
            var initOperation = package.InitializePackageAsync(createParameters);
            await initOperation;
    
            if(initOperation.Status == EOperationStatus.Succeeded)
                Debug.Log("资源包初始化成功！");
            else 
                Debug.LogError($"资源包初始化失败：{initOperation.Error}");
        }
        
        private async ValueTask<string> RequestPackageVersion()
        {
            var package = YooAssets.GetPackage("DefaultPackage");
            var operation = package.RequestPackageVersionAsync();
            await operation;

            if (operation.Status == EOperationStatus.Succeeded)
            {
                //请求成功
                string packageVersion = operation.PackageVersion;
                Debug.Log($"Request package Version : {packageVersion}");
                return packageVersion;
            }
            else
            {
                //请求失败
                Debug.LogError(operation.Error);
            }

            return "";
        }
        
        private async ValueTask UpdatePackageManifest(string packageVersion)
        {
            var package = YooAssets.GetPackage("DefaultPackage");
            var operation = package.LoadPackageManifestAsync(new LoadPackageManifestOptions(packageVersion, 60));
            await operation;

            if (operation.Status == EOperationStatus.Succeeded)
            {
                //更新成功
            }
            else
            {
                //更新失败
                Debug.LogError(operation.Error);
            }
        }
    }
}