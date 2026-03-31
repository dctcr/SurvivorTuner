using System;
using System.Collections.Generic;
using EntityStates.Commando.CommandoWeapon;
using RoR2;
using SurvivorTuner.Configuration;
using UnityEngine;

namespace SurvivorTuner.Tuning
{
    internal static class SkillStateHooks
    {
        private const string FirePistol2StateName = "EntityStates.Commando.CommandoWeapon.FirePistol2";
        private const string FireFMJStateName = "EntityStates.Commando.CommandoWeapon.FireFMJ";
        private const string FireShotgunBlastStateName = "EntityStates.Commando.CommandoWeapon.FireShotgunBlast";
        private const string FireBarrageStateName = "EntityStates.Commando.CommandoWeapon.FireBarrage";

        private static readonly HashSet<string> LoggedUnknownCommandoStates = new();

        public static void Hook()
        {
            On.RoR2.BulletAttack.Fire += BulletAttack_Fire;
            Plugin.Log.LogInfo("SkillStateHooks: hooked BulletAttack.Fire");
        }

        public static void Unhook()
        {
            On.RoR2.BulletAttack.Fire -= BulletAttack_Fire;
        }

        private static void BulletAttack_Fire(On.RoR2.BulletAttack.orig_Fire orig, BulletAttack self)
        {
            try
            {
                if (self == null || self.owner == null)
                {
                    orig(self);
                    return;
                }

                CharacterBody body = self.owner.GetComponent<CharacterBody>();
                if (body == null || body.bodyIndex != BodyCatalog.FindBodyIndex("CommandoBody"))
                {
                    orig(self);
                    return;
                }

                string? stateName = GetWeaponStateName(self.owner);
                if (string.IsNullOrEmpty(stateName))
                {
                    orig(self);
                    return;
                }

                switch (stateName)
                {
                    case FirePistol2StateName:
                        ApplyPrimaryTuning(self, body);
                        break;

                    case FireFMJStateName:
                        ApplyPhaseRoundTuning(self, body);
                        break;
                    
                    case FireShotgunBlastStateName:
                        ApplyPhaseBlastTuning(self, body);
                        break;

                    case FireBarrageStateName:
                        ApplySuppressiveFireTuning(self, body);
                        break;
                }
            }
            catch (Exception ex)
            {
                Plugin.Log.LogError($"SkillStateHooks BulletAttack_Fire failed: {ex}");
            }

            orig(self);
        }
        
        private static void ApplyPrimaryTuning(BulletAttack self, CharacterBody body)
        {
            self.damage = body.damage * CommandoConfig.PrimaryDamageCoefficient.Value;
            self.procCoefficient = CommandoConfig.PrimaryProcCoefficient.Value;
            self.maxDistance = CommandoConfig.PrimaryMaxDistance.Value;

            if (CommandoConfig.PrimaryIgniteOnHit.Value)
            {
                self.damageType |= DamageType.IgniteOnHit;
            }

            Plugin.Log.LogInfo(
                $"Commando primary bullet tuned | damage={self.damage}, proc={self.procCoefficient}, maxDistance={self.maxDistance}, ignite={CommandoConfig.PrimaryIgniteOnHit.Value}"
            );
        }

        private static void ApplyPhaseRoundTuning(BulletAttack self, CharacterBody body)
        {
            self.damage = body.damage * CommandoConfig.PhaseRoundAttack.DamageCoefficient.Value;

            if (CommandoConfig.PhaseRoundAttack.IgniteOnHit.Value)
            {
                self.damageType |= DamageType.IgniteOnHit;
            }

            Plugin.Log.LogInfo(
                $"Commando phase blast tuned | damage={self.damage}, proc={self.procCoefficient}, maxDistance={self.maxDistance}, ignite={CommandoConfig.PhaseRoundAttack.IgniteOnHit.Value}"
            );
        }

        private static void ApplyPhaseBlastTuning(BulletAttack self, CharacterBody body)
        {
            self.damage = body.damage * CommandoConfig.PhaseBlastAttack.DamageCoefficient.Value;

            if (CommandoConfig.PhaseBlastAttack.IgniteOnHit.Value)
            {
                self.damageType |= DamageType.IgniteOnHit;
            }

            Plugin.Log.LogInfo(
                $"Commando phase blast tuned | damage={self.damage}, proc={self.procCoefficient}, maxDistance={self.maxDistance}, ignite={CommandoConfig.PhaseBlastAttack.IgniteOnHit.Value}"
            );
        }

        private static void ApplySuppressiveFireTuning(BulletAttack self, CharacterBody body)
        {
            self.damage = body.damage * CommandoConfig.SuppressiveFireAttack.DamageCoefficient.Value;

            if (CommandoConfig.SuppressiveFireAttack.IgniteOnHit.Value)
            {
                self.damageType |= DamageType.IgniteOnHit;
            }

            Plugin.Log.LogInfo(
                $"Commando suppressive fire tuned | damage={self.damage}, ignite={CommandoConfig.SuppressiveFireAttack.IgniteOnHit.Value}"
            );
        }

        private static string? GetWeaponStateName(GameObject owner)
        {
            if (owner == null)
            {
                return null;
            }

            EntityStateMachine weaponStateMachine = EntityStateMachine.FindByCustomName(owner, "Weapon");
            if (weaponStateMachine == null || weaponStateMachine.state == null)
            {
                return null;
            }

            return weaponStateMachine.state.GetType().FullName;
        }
    }
}