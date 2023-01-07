using SFramework.Threading.Tasks;
using SFramework.Utilities;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using System;
using System.Linq;
using System.IO;
using UnityEditor;

namespace SFramework.Core.GameManagers
{
    public interface IAssetBundleLoader
    {
        AssetBundle Load(string bundleName);
        STask<AssetBundle> LoadAsync(string bundleName);
    }

    public class DefaultAssetBundleLoader : IAssetBundleLoader
    {
        public AssetBundle Load(string bundleName)
        {
            string path = this.GetLoadingPath(bundleName);
            this.Log(bundleName, path, false);
            AssetBundle ab = AssetBundle.LoadFromFile(path);
            return ab;
        }

        public async STask<AssetBundle> LoadAsync(string bundleName)
        {
            string path = this.GetLoadingPath(bundleName);
            this.Log(bundleName, path, true);
            AssetBundle ab = await AssetBundle.LoadFromFileAsync(path);
            return ab;
        }

        public virtual string GetLoadingPath(string bundleName)
        {
            return $"{StaticVariables.StreamingAssetsPath}/{StaticVariables.AssetBundlesFolderName}/{bundleName}{StaticVariables.AssetBundlesFileExtension}";
        }

        [Conditional("LOAD_BUNDLE_LOG")]
        private void Log(string bundleName, string path, bool isAsync)
        {
            string async = isAsync ? "async" : "";
            UnityEngine.Debug.Log($"AssetBundle load {async} {bundleName}\nfrom:{path}");
        }
    }

    public class AssetBundleVO
    {
        public enum State { Unloaded, Loading, Loaded }

        public string BundleName { get; private set; }
        public AssetBundle Content { get; private set; }

        /// <summary>依赖的资源</summary>
        public HashSet<AssetBundleVO> DependentVO { get; }
        /// <summary>依赖自己的资源</summary>
        public HashSet<AssetBundleVO> BeDependedVO { get; }

        private uint refCount;
        private bool isLoading;

        public State BundleState
        {
            get
            {
                if(this.Content != null)
                    return State.Loaded;

                if(this.isLoading)
                    return State.Loading;

                return State.Unloaded;
            }
        }

        public AssetBundleVO(string bundleName)
        {
            this.BundleName = bundleName;
            this.DependentVO = new HashSet<AssetBundleVO>();
            this.BeDependedVO = new HashSet<AssetBundleVO>();
            this.isLoading = false;
            this.refCount = 0;
        }

        public void InitDependencies(AssetBundleManifest manifest)
        {
            var dependent = manifest.GetAllDependencies(this.BundleName)//获取所有的依赖资源，包括递归依赖
                .Select(AssetBundleManager.GetAssetBundleVO);//转为VO集合，并且加载依赖资源的依赖资源

            foreach (var vo in dependent)
            {
                this.DependentVO.Add(vo);
                vo.BeDependedVO.Add(this);
            }
        }

        public async STask UnLoadAsync(bool unloadAllLoadedObjects = true, bool handleDependentBundles = true)
        {
            if (this.BundleState == State.Loaded)
            {
                this.refCount--;
            }
            else if(this.BundleState == State.Loading)
            {
                //等待AB加载完毕
                await STask.WaitUntil(() => this.BundleState != State.Loading);
                this.refCount--;
            }
            else
            {
                throw new Exception($"AssetBundleVO::Unload failed, {this.BundleName}, {this.BundleState}");
            }

            //处理依赖资源
            if (handleDependentBundles)
            {
                foreach (var depVO in this.DependentVO)
                {
                    await this.UnLoadAsync(true, false);
                }
            }

            if (this.refCount <= 0)
            {
                this.Content.Unload(unloadAllLoadedObjects);
                this.Content = null;//State == Unloaded
                this.isLoading = false;
            }
        }

        public void Load()
        {
            this.refCount++;
            if (this.BundleState == State.Loaded)
            {
                return;
            }
            else if(this.BundleState == State.Unloaded)
            {
                this.Content = AssetBundleManager.AssetBundleLoader.Load(this.BundleName);
                this.HandleDependentBundlesLoad();
            }
            else
            {
                throw new Exception($"AssetBundleVO::Load failed, {this.BundleName}, {this.BundleState}");
            }
        }

        public async STask LoadAsync()
        {
            while (this.BundleState == State.Loading)
            {
                await STask.NextFrame();
            }

            this.refCount++;

            if (this.BundleState == State.Loaded)
                return;

            if (this.BundleState == State.Unloaded)
            {
                this.Content = await this.LoadAsync(this.BundleName);
                await this.HandleDependentBundlesLoadAsync();
            }
        }

        private async STask<AssetBundle> LoadAsync(string bundleName)
        {
            this.isLoading = true;
            AssetBundle bundle = await AssetBundleManager.AssetBundleLoader.LoadAsync(bundleName);
            this.isLoading = false;
            return bundle;
        }

        private void HandleDependentBundlesLoad()
        {
            foreach (var vo in this.DependentVO)
            {
                if (vo.BundleState == State.Unloaded)
                {
                    vo.Load();
                }
            }
        }

        private async STask HandleDependentBundlesLoadAsync()
        {
            foreach (var vo in this.DependentVO)
            {
                if (vo.BundleState == State.Unloaded)
                {
                    await vo.LoadAsync();
                }
            }
        }

        public AssetBundleManifest LoadManifest()
        {
            if (this.BundleState == State.Loaded)
            {
                throw new InvalidOperationException("Manifest is not null, but still trying to load.");
            }
            else if (this.BundleState == State.Unloaded)
            {
                this.Content = AssetBundle.LoadFromFile($"{StaticVariables.StreamingAssetsPath}/{StaticVariables.AssetBundlesFolderName}/{this.BundleName}");
                return this.Content.LoadAsset<AssetBundleManifest>("AssetBundleManifest");
            }
            else
            {
                throw new Exception($"AssetBundleVO::Load failed, {this.BundleName}, {this.BundleName}");
            }
        }

        public override string ToString()
        {
            return this.BundleName;
        }
    }

    public static class AssetBundleManager
    {
        internal static IAssetBundleLoader AssetBundleLoader = new DefaultAssetBundleLoader();

        private static readonly Dictionary<string,AssetBundleVO> bundleVOMap = new Dictionary<string,AssetBundleVO>();

        private static AssetBundleManifest manifest;

#if LOAD_ASSET_IN_EDITOR
        private static string[] allAssetPaths;
#endif

        public static void SetupLoader(IAssetBundleLoader assetBundleLoader)
        {
            AssetBundleLoader = assetBundleLoader;
        }

        public static void LoadManifest()
        {
            if (manifest != null)
            {
                throw new InvalidOperationException("Manifest is not null, but still trying to load.");
            }
            AssetBundleVO vo = GetAssetBundleVO(StaticVariables.AssetBundlesFolderName);
            if (vo.BundleState == AssetBundleVO.State.Unloaded)
            {
                manifest = vo.LoadManifest();
            }
        }

        /// <summary>
        /// 只加载AB
        /// </summary>
        /// <param name="bundleName"></param>
        /// <returns></returns>
        public static async STask<AssetBundleVO> LoadAssetBundleAsync(string bundleName)
        {
            AssetBundleVO vo = GetAssetBundleVO(bundleName);
            if (vo.BundleState == AssetBundleVO.State.Unloaded)
            {
                await vo.LoadAsync();
            }

            return vo;
        }

        /// <summary>
        /// 从AB中加载资源，若AB未被加载，则先加载AB
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="assetPath"></param>
        /// <param name="bundleName"></param>
        /// <returns></returns>
        public static async STask<T> LoadAssetInAssetBundleAsync<T>(string assetPath, string bundleName) where T : UnityEngine.Object
        {
#if LOAD_ASSET_IN_EDITOR
            //TODO 按照类型T给assetPath加文件扩展名，用于匹配path；用editor加载时跳过卸载AB逻辑
            if(allAssetPaths == null)
                allAssetPaths = AssetDatabase.GetAllAssetPaths();
            foreach (string path in allAssetPaths)
            {
                if (path.Contains(assetPath))
                    UnityEngine.Debug.Log($"{path} contains {assetPath}");
            }
            foreach (string path in allAssetPaths)
            {
                if (Path.GetFileName(path) == assetPath)
                {
                    Log(false, path);
                    return AssetDatabase.LoadAssetAtPath<T>(path);
                }
            }
#endif

            AssetBundleVO vo = GetAssetBundleVO(bundleName);
            if (vo.BundleState == AssetBundleVO.State.Unloaded)
            {
                await vo.LoadAsync();
            }

            T asset = await vo.Content.LoadAssetAsync(assetPath) as T;
            return asset;
        }

        /// <summary>
        /// 卸载AB，从中实例化的GameObject也会被清理
        /// </summary>
        /// <param name="bundleName"></param>
        /// <returns></returns>
        public static async STask UnloadAssetBundleAsync(string bundleName)
        {
            AssetBundleVO vo = GetAssetBundleVO(bundleName);
            await vo.UnLoadAsync();
        }

        /// <summary>
        /// 获取AB的包装VO，当新建VO时会加载其依赖资源
        /// </summary>
        /// <param name="bundleName"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="Exception"></exception>
        public static AssetBundleVO GetAssetBundleVO(string bundleName)
        {
            if (string.IsNullOrEmpty(bundleName))
                throw new ArgumentNullException("GetAssetBundleVO:: bundlName is null");

            if (!bundleVOMap.ContainsKey(bundleName))
            {
                AssetBundleVO vo = new AssetBundleVO(bundleName);
                bundleVOMap.Add(bundleName, vo);

                if (bundleName != StaticVariables.AssetBundlesFolderName)
                {
                    if (manifest != null)
                    {
                        vo.InitDependencies(manifest);
                    }
                    else
                    {
                        throw new Exception("Call LoadManifest before use");
                    }
                }
            }

            return bundleVOMap[bundleName];
        }

        [Conditional("LOAD_BUNDLE_LOG")]
        private static void Log(bool fromAB,string assetPath)
        {
            UnityEngine.Debug.Log($"load from {(fromAB ? "AssetBundle" : "AssetDataBase")}, assetPath is {assetPath}");
        }
    }
}