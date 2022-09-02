﻿using CustomJSONData.CustomBeatmap;
using HarmonyLib;
using UnityEngine;
using Vivify.Controllers;
using Object = UnityEngine.Object;

namespace Vivify.Events
{
    internal partial class EventController
    {
        internal void InstantiatePrefab(CustomEventData customEventData)
        {
            if (!_deserializedData.Resolve(customEventData, out InstantiatePrefabData? heckData))
            {
                return;
            }

            string assetName = heckData.Asset;
            if (!_assetBundleManager.TryGetAsset(assetName, out GameObject? prefab))
            {
                return;
            }

            GameObject gameObject = Object.Instantiate(prefab);

            Transform transform = gameObject.transform;
            heckData.TransformData.Apply(transform, false);

            if (heckData.Track != null)
            {
                _transformControllerFactory.Create(gameObject, heckData.Track);
            }

            gameObject.GetComponentsInChildren<Animator>().Do(n => _instantiator.InstantiateComponent<AnimatorSyncController>(n.gameObject, new object[] { customEventData.time }));

            string? id = heckData.Id;
            if (id != null)
            {
                Log.Logger.Log($"Created [{assetName}] with id [{id}].");
                _prefabManager.Add(id, gameObject);
            }
            else
            {
                string genericId = gameObject.GetHashCode().ToString();
                Log.Logger.Log($"Created [{assetName}] without id.");
                _prefabManager.Add(genericId, gameObject);
            }
        }
    }
}
