using System;
using RoR2;
using SurvivorTuner.Configuration;
using UnityEngine;

namespace SurvivorTuner.Tuning
{
    internal static class BodyTuner
    {
        public static void ApplyCommando()
        {
            CharacterBody body = GetCommandoBody();

            Plugin.Log.LogInfo($"Found Commando body: {body.name}");

            body.baseMaxHealth = CommandoConfig.BaseMaxHealth.Value;
            body.baseRegen = CommandoConfig.BaseRegen.Value;
            body.baseMoveSpeed = CommandoConfig.BaseMoveSpeed.Value;
            body.baseDamage = CommandoConfig.BaseDamage.Value;
            body.baseAttackSpeed = CommandoConfig.BaseAttackSpeed.Value;
            body.baseArmor = CommandoConfig.BaseArmor.Value;
        }

        private static CharacterBody GetCommandoBody()
        {
            if (BodyCatalog.allBodyPrefabBodyBodyComponents == null)
            {
                throw new Exception("BodyCatalog body array is null.");
            }

            foreach (CharacterBody body in BodyCatalog.allBodyPrefabBodyBodyComponents)
            {
                if (body != null && body.name == "CommandoBody")
                {
                    return body;
                }
            }

            throw new Exception("Could not find CommandoBody in BodyCatalog.allBodyPrefabBodyBodyComponents");
        }
    }
}