using SandBox;
using System;
using System.Collections.Generic;
using System.Linq;

using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.ComponentInterfaces;

namespace VillageBoyGoesBad
{
    public class VBGBIssueMissionSonFight : AgentApplyDamageModel
    {
        public override int CalculateDamage(ref AttackInformation attackInformation, ref AttackCollisionData collisionData, in MissionWeapon weapon)
        {
            return 1;
            //base.CalculateDamage(ref attackInformation, ref collisionData, weapon);
        }

        public override float CalculateDefenderShieldStunMultiplier(Agent attackerAgent)
        {
            throw new NotImplementedException();
        }

        public override float CalculateEffectiveMissileSpeed(Agent attackerAgent, ref Vec3 missileStartDirection, float missileStartSpeed)
        {
            throw new NotImplementedException();
        }

        public override float CalculateMoraleEffects(Agent attackerAgent, Agent defenderAgent, in MissionWeapon attackerWeapon)
        {
            throw new NotImplementedException();
        }

        public override float CalculatePassiveAttackDamage(BasicCharacterObject attackerCharacter, ref AttackCollisionData collisionData, float baseDamage)
        {
            throw new NotImplementedException();
        }

        public override float CalculateShieldDamage(float baseDamage)
        {
            throw new NotImplementedException();
        }

        public override float CalculateStaggerThresholdMultiplier(Agent defenderAgent)
        {
            throw new NotImplementedException();
        }

        public override bool DecideCrushedThrough(Agent attackerAgent, Agent defenderAgent, float totalAttackEnergy, Agent.UsageDirection attackDirection, StrikeType strikeType, WeaponComponentData defendItem, bool isPassiveUsageHit)
        {
            throw new NotImplementedException();
        }

        public override void DecideMeleeBlowFlags(Agent attackerAgent, Agent defenderAgent, WeaponComponentData weapon, in AttackCollisionData collisionData, ref BlowFlags blowFlags)
        {
            throw new NotImplementedException();
        }

        public override void DecideMissileBlowFlags(Agent attackerAgent, Agent defenderAgent, WeaponComponentData weapon, in AttackCollisionData collisionData, ref BlowFlags blowFlags)
        {
            throw new NotImplementedException();
        }

        public override void DecideMissileWeaponFlags(Agent attackerAgent, MissionWeapon missileWeapon, ref WeaponFlags missileWeaponFlags)
        {
            throw new NotImplementedException();
        }

        public override MeleeCollisionReaction DecidePassiveAttackCollisionReaction(Agent attacker, Agent defender, bool isFatalHit)
        {
            throw new NotImplementedException();
        }

        public override float GetDamageMultiplierForBodyPart(BoneBodyPartType bodyPart, DamageTypes type)
        {
            throw new NotImplementedException();
        }
    }
}
