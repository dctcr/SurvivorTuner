using System;
using System.Collections;
using System.Linq;
using BepInEx;
using BepInEx.Logging;
using RoR2;
using SurvivorTuner.Configuration;
using SurvivorTuner.Tuning;
using UnityEngine;

namespace SurvivorTuner
{
    [BepInPlugin(PluginGuid, PluginName, PluginVersion)]
    public sealed class Plugin : BaseUnityPlugin
    {
        public const string PluginGuid = "angel.survivortuner";
        public const string PluginName = "SurvivorTuner";
        public const string PluginVersion = "0.1.0";

        internal static ManualLogSource Log { get; } = BepInEx.Logging.Logger.CreateLogSource("SurvivorTuner");

        private void Awake()
        {
            Log.LogInfo("Awake() starting.");

            SkillStateHooks.Hook();

            StartCoroutine(ApplyTunersWhenReady());

            Log.LogInfo($"{PluginName} loaded.");
        }

        private IEnumerator ApplyTunersWhenReady()
        {
            Log.LogInfo("Waiting for BodyCatalog...");

            yield return new WaitUntil(() =>
                BodyCatalog.allBodyPrefabBodyBodyComponents != null &&
                BodyCatalog.allBodyPrefabBodyBodyComponents.Any()
            );

            int bodyCount = BodyCatalog.allBodyPrefabBodyBodyComponents.Count();
            Log.LogInfo($"BodyCatalog ready. Body count = {bodyCount}");

            try
            {
                CommandoConfig.Init();
                Log.LogInfo("CommandoConfig.Init() complete.");

                BodyTuner.ApplyCommando();
                SkillDefTuner.ApplyCommando();

                Log.LogInfo("Applied Commando body + skill tuners.");
            }
            catch (Exception ex)
            {
                Log.LogError($"Failed to apply Commando tuners: {ex}");
            }
        }

        private void OnDestroy()
        {
            SkillStateHooks.Unhook();
        }
    }
}