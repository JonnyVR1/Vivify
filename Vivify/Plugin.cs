﻿using Heck;
using IPA;
using IPA.Config.Stores;
using IPA.Logging;
using JetBrains.Annotations;
using SiraUtil.Zenject;
using UnityEngine.SceneManagement;
using Vivify.Installers;
using Vivify.Managers;
using static Vivify.VivifyController;

namespace Vivify
{
    [Plugin(RuntimeOptions.DynamicInit)]
    internal class Plugin
    {
        [UsedImplicitly]
        [Init]
        public Plugin(Logger pluginLogger, IPA.Config.Config conf, Zenjector zenjector)
        {
            Log = pluginLogger;

            DepthShaderManager.LoadFromMemory();
            zenjector.Install<VivifyAppInstaller>(Location.App, conf.Generated<Config>());
            zenjector.Install<VivifyPlayerInstaller>(Location.Player);
            zenjector.Install<VivifyMenuInstaller>(Location.Menu);
            zenjector.UseLogger(pluginLogger);

            HeckPatchManager.Register(HARMONY_ID);
        }

        internal static Logger Log { get; private set; } = null!;

#pragma warning disable CA1822
        [UsedImplicitly]
        [OnEnable]
        public void OnEnable()
        {
            VivifyController.Capability.Register();
            SceneManager.activeSceneChanged += OnActiveSceneChanged;
        }

        [UsedImplicitly]
        [OnDisable]
        public void OnDisable()
        {
            VivifyController.Capability.Deregister();
            SceneManager.activeSceneChanged -= OnActiveSceneChanged;
        }
#pragma warning restore CA1822
    }
}
