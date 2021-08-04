﻿namespace Vivify
{
    using System;
    using System.Reflection;
    using HarmonyLib;
    using Heck;
    using IPA;
    using UnityEngine;
    using UnityEngine.SceneManagement;
    using IPALogger = IPA.Logging.Logger;

    [Plugin(RuntimeOptions.DynamicInit)]
    internal class Plugin
    {
        internal const string CAPABILITY = "Vivify";
        internal const string HARMONYIDCORE = "com.aeroluna.BeatSaber.VivifyCore";
        internal const string HARMONYID = "com.aeroluna.BeatSaber.Vivify";

        internal static readonly Harmony _harmonyInstanceCore = new Harmony(HARMONYIDCORE);
        internal static readonly Harmony _harmonyInstance = new Harmony(HARMONYID);

        internal static bool CumDump { get; private set; }

#pragma warning disable CS8618
        internal static HeckLogger Logger { get; private set; }
#pragma warning restore CS8618

        [Init]
        public void Init(IPALogger pluginLogger)
        {
            Logger = new HeckLogger(pluginLogger);
            HeckData.InitPatches(_harmonyInstance, Assembly.GetExecutingAssembly());

            string[] arguments = Environment.GetCommandLineArgs();
            foreach (string arg in arguments)
            {
                if (arg.ToLower() == "-cumdump")
                {
                    CumDump = true;
                    Logger.Log("[-cumdump] launch argument detected, running in CumDump mode.");
                }
            }
        }

        [OnEnable]
        public void OnEnable()
        {
            SongCore.Collections.RegisterCapability(CAPABILITY);
            _harmonyInstanceCore.PatchAll(Assembly.GetExecutingAssembly());

            SceneManager.activeSceneChanged += VivifyController.OnActiveSceneChanged;
        }

        [OnDisable]
        public void OnDisable()
        {
            SongCore.Collections.DeregisterizeCapability(CAPABILITY);
            _harmonyInstanceCore.UnpatchAll(HARMONYIDCORE);
            _harmonyInstanceCore.UnpatchAll(HARMONYID);

            SceneManager.activeSceneChanged -= VivifyController.OnActiveSceneChanged;
        }
    }
}
