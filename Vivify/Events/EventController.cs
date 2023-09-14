﻿using System;
using System.Collections.Generic;
using CustomJSONData.CustomBeatmap;
using HarmonyLib;
using Heck;
using Heck.Animation.Transform;
using Heck.ReLoad;
using JetBrains.Annotations;
using UnityEngine;
using Vivify.Controllers;
using Vivify.Managers;
using Vivify.PostProcessing;
using Zenject;
using static Vivify.VivifyController;

namespace Vivify.Events
{
    internal partial class EventController : IDisposable
    {
        private readonly IInstantiator _instantiator;
        private readonly BeatmapCallbacksController _callbacksController;
        private readonly AssetBundleManager _assetBundleManager;
        private readonly PrefabManager _prefabManager;
        private readonly DeserializedData _deserializedData;
        private readonly IAudioTimeSource _audioTimeSource;
        private readonly IBpmController _bpmController;
        private readonly CoroutineDummy _coroutineDummy;
        private readonly TransformControllerFactory _transformControllerFactory;
        private readonly BeatmapObjectPrefabManager _beatmapObjectPrefabManager;
        private readonly bool _leftHanded;
        private readonly ReLoader? _reLoader;
        private readonly BeatmapDataCallbackWrapper _callbackWrapper;

        private readonly HashSet<IDisposable> _disposables = new();

        [UsedImplicitly]
        private EventController(
            IInstantiator instantiator,
            BeatmapCallbacksController callbacksController,
            AssetBundleManager assetBundleManager,
            PrefabManager prefabManager,
            [Inject(Id = ID)] DeserializedData deserializedData,
            IAudioTimeSource audioTimeSource,
            IBpmController bpmController,
            CoroutineDummy coroutineDummy,
            TransformControllerFactory transformControllerFactory,
            BeatmapObjectPrefabManager beatmapObjectPrefabManager,
            [Inject(Id = HeckController.LEFT_HANDED_ID)] bool leftHanded,
            [InjectOptional] ReLoader? reLoader)
        {
            _instantiator = instantiator;
            _callbacksController = callbacksController;
            _assetBundleManager = assetBundleManager;
            _prefabManager = prefabManager;
            _deserializedData = deserializedData;
            _audioTimeSource = audioTimeSource;
            _bpmController = bpmController;
            _coroutineDummy = coroutineDummy;
            _transformControllerFactory = transformControllerFactory;
            _beatmapObjectPrefabManager = beatmapObjectPrefabManager;
            _leftHanded = leftHanded;
            _reLoader = reLoader;
            if (reLoader != null)
            {
                reLoader.Rewinded += PostProcessingController.ResetMaterial;
            }

            _callbackWrapper = callbacksController.AddBeatmapCallback<CustomEventData>(HandleCallback);
        }

        public void Dispose()
        {
            _callbacksController.RemoveBeatmapCallback(_callbackWrapper);
            if (_reLoader != null)
            {
                _reLoader.Rewinded -= PostProcessingController.ResetMaterial;
            }

            _disposables.Do(n => n.Dispose());

            CameraPropertyController.ResetProperties();
        }

        private void HandleCallback(CustomEventData customEventData)
        {
            switch (customEventData.eventType)
            {
                case APPLY_POST_PROCESSING:
                    ApplyPostProcessing(customEventData);
                    break;

                case ASSIGN_TRACK_PREFAB:
                    AssignTrackPrefab(customEventData);
                    break;

                case DECLARE_CULLING_TEXTURE:
                    DeclareCullingMask(customEventData);
                    break;

                case DECLARE_TEXTURE:
                    DeclareRenderTexture(customEventData);
                    break;

                case DESTROY_TEXTURE:
                    DestroyTexture(customEventData);
                    break;

                case DESTROY_PREFAB:
                    DestroyPrefab(customEventData);
                    break;

                case INSTANTIATE_PREFAB:
                    InstantiatePrefab(customEventData);
                    break;

                case SET_MATERIAL_PROPERTY:
                    SetMaterialProperty(customEventData);
                    break;

                case SET_ANIMATOR_PROPERTY:
                    SetAnimatorProperty(customEventData);
                    break;

                case SET_GLOBAL_PROPERTY:
                    SetGlobalProperty(customEventData);
                    break;

                case SET_CAMERA_PROPERTY:
                    SetCameraProperty(customEventData);
                    break;

                case SET_RENDER_SETTING:
                    SetRenderSetting(customEventData);
                    break;
            }
        }
    }
}
