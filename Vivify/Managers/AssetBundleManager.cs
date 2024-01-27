﻿using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using CustomJSONData.CustomBeatmap;
using JetBrains.Annotations;
using UnityEngine;
using static Vivify.VivifyController;
using Logger = IPA.Logging.Logger;
using Object = UnityEngine.Object;

namespace Vivify.Managers
{
    internal class AssetBundleManager : IDisposable
    {
        private readonly Dictionary<string, Object> _assets = new();
        private readonly AssetBundle _mainBundle;

        [UsedImplicitly]
        private AssetBundleManager(IDifficultyBeatmap difficultyBeatmap, Config config)
        {
            if (difficultyBeatmap is not CustomDifficultyBeatmap customDifficultyBeatmap)
            {
                throw new ArgumentException(
                    $"Was not correct type. Expected: {nameof(CustomDifficultyBeatmap)}, was: {difficultyBeatmap.GetType().Name}.",
                    nameof(difficultyBeatmap));
            }

            string path = Path.Combine(((CustomBeatmapLevel)customDifficultyBeatmap.level).customLevelPath, BUNDLE);
            if (!File.Exists(path))
            {
                throw new InvalidOperationException($"[{BUNDLE}] not found!"); // TODO: Figure out a way to not just obliterate everything
            }

            if (Heck.HeckController.DebugMode)
            {
                _mainBundle = AssetBundle.LoadFromFile(path);
            }
            else
            {
                CustomData levelCustomData = ((CustomBeatmapSaveData)customDifficultyBeatmap.beatmapSaveData).levelCustomData;
                uint assetBundleChecksum = levelCustomData.GetRequired<uint>(ASSET_BUNDLE);
                _mainBundle = AssetBundle.LoadFromFile(path, assetBundleChecksum);
            }

            if (_mainBundle == null)
            {
                throw new InvalidOperationException($"Failed to load [{path}]");
            }

            string[] assetnames = _mainBundle.GetAllAssetNames();
            foreach (string name in assetnames)
            {
                Object asset = _mainBundle.LoadAsset(name);
                _assets.Add(name, asset);
            }

            /*
             In Version 1.31.0,
             If you have any of the DLC, AssetBundle.UnloadAllAssetBundles(true) is called in
             BeatmapLevelDataLoader.Dispose when switching scenes.
             AssetBundle used by the mod will be unloaded. */
            ////mainBundle.Unload(false);
        }

        public void Dispose()
        {
            Log.Logger.Log("disposed");
            if (_mainBundle != null)
            {
                _mainBundle.Unload(true);
            }
        }

        internal bool TryGetAsset<T>(string assetName, [NotNullWhen(true)] out T? asset)
        {
            if (_assets.TryGetValue(assetName, out Object gameObject))
            {
                if (gameObject is T t)
                {
                    asset = t;
                    return true;
                }

                Log.Logger.Log($"Found {assetName}, but was null or not [{typeof(T).FullName}]!", Logger.Level.Error);
            }
            else
            {
                Log.Logger.Log($"Could not find {typeof(T).FullName} [{assetName}].", Logger.Level.Error);
            }

            asset = default;
            return false;
        }
    }
}
