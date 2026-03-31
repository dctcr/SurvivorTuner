using System;
using System.IO;
using BepInEx;
using BepInEx.Configuration;
using RoR2;
using RoR2.Skills;
using UnityEngine;
using UnityEngine.AddressableAssets;
using IOPath = System.IO.Path;

namespace SurvivorTuner.Configuration
{
    internal sealed class SkillDefTuning
    {
        public ConfigEntry<float> BaseRechargeInterval { get; set; } = null!;
        public ConfigEntry<int> BaseMaxStock { get; set; } = null!;
    }

    internal sealed class AttackTuning
    {
        public ConfigEntry<float> DamageCoefficient { get; set; } = null!;
        public ConfigEntry<bool> IgniteOnHit { get; set; } = null!;
    }

    internal static class CommandoConfig
    {
        private const string CommandoBodyPath = "RoR2/Base/Commando/CommandoBody.prefab";

        public static ConfigFile File { get; private set; } = null!;

        // Body
        public static ConfigEntry<float> BaseMaxHealth { get; private set; } = null!;
        public static ConfigEntry<float> BaseRegen { get; private set; } = null!;
        public static ConfigEntry<float> BaseMoveSpeed { get; private set; } = null!;
        public static ConfigEntry<float> BaseDamage { get; private set; } = null!;
        public static ConfigEntry<float> BaseAttackSpeed { get; private set; } = null!;
        public static ConfigEntry<float> BaseArmor { get; private set; } = null!;

        // Primary
        public static SkillDefTuning PrimarySkill { get; private set; } = null!;
        public static AttackTuning PrimaryAttack { get; private set; } = null!;

        // Backward-compatible aliases for current primary hook
        public static ConfigEntry<float> PrimaryDamageCoefficient => PrimaryAttack.DamageCoefficient;
        public static ConfigEntry<bool> PrimaryIgniteOnHit => PrimaryAttack.IgniteOnHit;
        public static ConfigEntry<float> PrimaryProcCoefficient { get; private set; } = null!;
        public static ConfigEntry<float> PrimaryMaxDistance { get; private set; } = null!;

        // Secondaries
        public static SkillDefTuning PhaseRoundSkill { get; private set; } = null!;
        public static AttackTuning PhaseRoundAttack { get; private set; } = null!;

        public static SkillDefTuning PhaseBlastSkill { get; private set; } = null!;
        public static AttackTuning PhaseBlastAttack { get; private set; } = null!;

        // Utilities
        public static SkillDefTuning TacticalDiveSkill { get; private set; } = null!;
        public static SkillDefTuning TacticalSlideSkill { get; private set; } = null!;

        // Specials
        public static SkillDefTuning SuppressiveFireSkill { get; private set; } = null!;
        public static AttackTuning SuppressiveFireAttack { get; private set; } = null!;

        public static SkillDefTuning FragGrenadeSkill { get; private set; } = null!;
        public static AttackTuning FragGrenadeAttack { get; private set; } = null!;

        private static bool _initialized;

        public static void Init()
        {
            if (_initialized)
            {
                return;
            }

            string configDir = IOPath.Combine(Paths.ConfigPath, "SurvivorTuner");
            Directory.CreateDirectory(configDir);

            File = new ConfigFile(IOPath.Combine(configDir, "Commando.cfg"), true);

            GameObject bodyPrefab = LoadCommandoBodyPrefab();
            CharacterBody body = bodyPrefab.GetComponent<CharacterBody>();
            if (body == null)
            {
                throw new Exception("Commando prefab is missing CharacterBody.");
            }

            SkillLocator skillLocator = bodyPrefab.GetComponent<SkillLocator>();
            if (skillLocator == null)
            {
                throw new Exception("Commando prefab is missing SkillLocator.");
            }

            SkillDef? primary = GetVariantSkillDef(skillLocator.primary.skillFamily, 0, "Primary/DoubleTap");

            SkillDef? phaseRound = GetVariantSkillDef(skillLocator.secondary.skillFamily, 0, "Secondary/PhaseRound");
            SkillDef? phaseBlast = GetVariantSkillDef(skillLocator.secondary.skillFamily, 1, "Secondary/PhaseBlast");

            SkillDef? tacticalDive = GetVariantSkillDef(skillLocator.utility.skillFamily, 0, "Utility/TacticalDive");
            SkillDef? tacticalSlide = GetVariantSkillDef(skillLocator.utility.skillFamily, 1, "Utility/TacticalSlide");

            SkillDef? suppressiveFire = GetVariantSkillDef(skillLocator.special.skillFamily, 0, "Special/SuppressiveFire");
            SkillDef? fragGrenade = GetVariantSkillDef(skillLocator.special.skillFamily, 1, "Special/FragGrenade");

            // Body
            BaseMaxHealth = File.Bind(
                "Body",
                "BaseMaxHealth",
                body.baseMaxHealth,
                "Commando base max health."
            );

            BaseRegen = File.Bind(
                "Body",
                "BaseRegen",
                body.baseRegen,
                "Commando base regen."
            );

            BaseMoveSpeed = File.Bind(
                "Body",
                "BaseMoveSpeed",
                body.baseMoveSpeed,
                "Commando base move speed."
            );

            BaseDamage = File.Bind(
                "Body",
                "BaseDamage",
                body.baseDamage,
                "Commando base damage."
            );

            BaseAttackSpeed = File.Bind(
                "Body",
                "BaseAttackSpeed",
                body.baseAttackSpeed,
                "Commando base attack speed."
            );

            BaseArmor = File.Bind(
                "Body",
                "BaseArmor",
                body.baseArmor,
                "Commando base armor."
            );

            // Primary
            PrimarySkill = BindSkillDefTuning("Primary/DoubleTap", primary);
            PrimaryAttack = BindAttackTuning("Primary/DoubleTap", 1.0f);

            PrimaryProcCoefficient = File.Bind(
                "Primary/DoubleTap.Attack",
                "ProcCoefficient",
                1.0f,
                "Double Tap proc coefficient."
            );

            PrimaryMaxDistance = File.Bind(
                "Primary/DoubleTap.Attack",
                "MaxDistance",
                300f,
                "DoubleTap max distance."
            );

            // Secondary
            PhaseRoundSkill = BindSkillDefTuning("Secondary/PhaseRound", phaseRound);
            PhaseRoundAttack = BindAttackTuning("Secondary/PhaseRound", 3.0f);

            PhaseBlastSkill = BindSkillDefTuning("Secondary/PhaseBlast", phaseBlast);
            PhaseBlastAttack = BindAttackTuning("Secondary/PhaseBlast", 2.0f);

            // Utility
            TacticalDiveSkill = BindSkillDefTuning("Utility/TacticalDive", tacticalDive);
            TacticalSlideSkill = BindSkillDefTuning("Utility/TacticalSlide", tacticalSlide);

            // Special
            SuppressiveFireSkill = BindSkillDefTuning("Special/SuppressiveFire", suppressiveFire);
            SuppressiveFireAttack = BindAttackTuning("Special/SuppressiveFire", 1.0f);

            FragGrenadeSkill = BindSkillDefTuning("Special/FragGrenade", fragGrenade);
            FragGrenadeAttack = BindAttackTuning("Special/FragGrenade", 7.0f);

            _initialized = true;
        }

        private static SkillDefTuning BindSkillDefTuning(string section, SkillDef def)
        {
            return new SkillDefTuning
            {
                BaseRechargeInterval = File.Bind(
                    $"{section}.SkillDef",
                    "BaseRechargeInterval",
                    def.baseRechargeInterval,
                    $"Cooldown for {section}."
                ),
                BaseMaxStock = File.Bind(
                    $"{section}.SkillDef",
                    "BaseMaxStock",
                    def.baseMaxStock,
                    $"Stock count for {section}."
                )
            };
        }

        private static AttackTuning BindAttackTuning(string section, float defaultDamageCoefficient)
        {
            return new AttackTuning
            {
                DamageCoefficient = File.Bind(
                    $"{section}.Attack",
                    "DamageCoefficient",
                    defaultDamageCoefficient,
                    $"Damage coefficient for {section}. 1.0 = 100% base damage."
                ),
                IgniteOnHit = File.Bind(
                    $"{section}.Attack",
                    "IgniteOnHit",
                    false,
                    $"If true, {section} will ignite enemies on hit."
                )
            };
        }

        private static SkillDef GetVariantSkillDef(SkillFamily family, int index, string label)
        {
            if (family == null || family.variants == null || family.variants.Length <= index)
            {
                throw new Exception($"Could not find skill variant index {index} for {label}.");
            }

            SkillDef def = family.variants[index].skillDef;
            if (def == null)
            {
                throw new Exception($"SkillDef was null for {label}.");
            }

            return def;
        }

        private static GameObject LoadCommandoBodyPrefab()
        {
            GameObject prefab = Addressables.LoadAssetAsync<GameObject>(CommandoBodyPath).WaitForCompletion();

            if (prefab == null)
            {
                throw new IOException($"Could not load prefab at path: {CommandoBodyPath}");
            }

            return prefab;
        }
    }
}