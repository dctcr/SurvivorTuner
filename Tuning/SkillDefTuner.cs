using System;
using RoR2;
using RoR2.Skills;
using SurvivorTuner.Configuration;

namespace SurvivorTuner.Tuning
{
    internal static class SkillDefTuner
    {
        public static void ApplyCommando()
        {
            SkillLocator skillLocator = GetCommandoSkillLocator();

            ApplyVariant(skillLocator.primary.skillFamily, 0, CommandoConfig.PrimarySkill, "Primary/DoubleTap");
            
            ApplyVariant(skillLocator.secondary.skillFamily, 0, CommandoConfig.PhaseRoundSkill, "Secondary/PhaseRound");
            ApplyVariant(skillLocator.secondary.skillFamily, 1, CommandoConfig.PhaseBlastSkill, "Secondary/PhaseBlast");

            ApplyVariant(skillLocator.utility.skillFamily, 0, CommandoConfig.TacticalDiveSkill, "Utility/TacticalDive");
            ApplyVariant(skillLocator.utility.skillFamily, 1, CommandoConfig.TacticalSlideSkill, "Utility/TacticalSlide");

            ApplyVariant(skillLocator.special.skillFamily, 0, CommandoConfig.SuppressiveFireSkill, "Special/SuppressiveFire");
            ApplyVariant(skillLocator.special.skillFamily, 1, CommandoConfig.FragGrenadeSkill, "Special/FragGrenade");

            LogVariantState(skillLocator.primary.skillFamily, 0, "Primary/DoubleTap");

            LogVariantState(skillLocator.secondary.skillFamily, 0, "Secondary/PhaseRound");
            LogVariantState(skillLocator.secondary.skillFamily, 1, "Secondary/PhaseBlast");

            LogVariantState(skillLocator.utility.skillFamily, 0, "Utility/TacticalDive");
            LogVariantState(skillLocator.utility.skillFamily, 1, "Utility/TacticalSlide");

            LogVariantState(skillLocator.special.skillFamily, 0, "Special/SuppressiveFire");
            LogVariantState(skillLocator.special.skillFamily, 1, "Special/FragGrenade");

            Plugin.Log.LogInfo("SkillDef tuning applied to Commando.");
        }

        private static void ApplyVariant(SkillFamily family, int index, SkillDefTuning tuning, string label)
        {
            if (family == null || family.variants == null || family.variants.Length <= index)
            {
                Plugin.Log.LogWarning($"Skill family variant missing for {label}.");
                return;
            }

            SkillDef def = family.variants[index].skillDef;
            if (def == null)
            {
                Plugin.Log.LogWarning($"SkillDef missing for {label}.");
                return;
            }

            def.baseRechargeInterval = tuning.BaseRechargeInterval.Value;
            def.baseMaxStock = tuning.BaseMaxStock.Value;
        }

        private static void LogVariantState(SkillFamily family, int index, string label)
        {
            if (family == null || family.variants == null || family.variants.Length <= index)
            {
                Plugin.Log.LogInfo($"Could not log state {label}: missing family/variant");
                return;
            }

            SkillDef def = family.variants[index].skillDef;
            if (def == null)
            {
                Plugin.Log.LogInfo($"Could not log state for {label}: SkillDef missing");
                return;
            }

            string activationStateName = "(null)";
            Type? activationType = def.activationState.stateType;
            if (activationType != null)
            {
                activationStateName = activationType.FullName ?? activationType.Name;
            }

            Plugin.Log.LogInfo(
                $"Commando skill state | {label} | skillDef={def.skillNameToken} | activationState={activationStateName} | stateMachine={def.activationStateMachineName}"
            );
        }

        private static SkillLocator GetCommandoSkillLocator()
        {
            if (BodyCatalog.allBodyPrefabBodyBodyComponents == null)
            {
                throw new Exception("BodyCatalog body array is null.");
            }

            foreach (CharacterBody body in BodyCatalog.allBodyPrefabBodyBodyComponents)
            {
                if (body != null && body.name == "CommandoBody")
                {
                    SkillLocator skillLocator = body.GetComponent<SkillLocator>();
                    if (skillLocator == null)
                    {
                        throw new Exception("CommandoBody exists but has no SkillLocator");
                    }

                    return skillLocator;
                }
            }

            throw new Exception("Could not find CommandoBody in BodyCatalog");
        }
    }
}