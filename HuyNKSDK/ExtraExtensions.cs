using LeagueSharp;
using LeagueSharp.SDK.Core.Extensions;
using LeagueSharp.SDK.Core.Wrappers;
using LeagueSharp.SDK.Core.Wrappers.Spells;
using System;
using System.Linq;

namespace HuyNK_Series_SDK
{
    static class ExtraExtensions
    {
        public static bool isKillableAndValidTarget(this Obj_AI_Hero Target, double CalculatedDamage, float distance = float.MaxValue)
        {
            if (Target == null || !Target.IsValidTarget(distance) || Target.Health <= 0)
                return false;

            if (ObjectManager.Player.HasBuff("summonerexhaust"))
                CalculatedDamage *= 0.6;

            if (Target.HasBuff("FerociousHowl"))
                CalculatedDamage *= 0.3;

            return Target.Health + Target.HPRegenRate + Target.PhysicalShield <= CalculatedDamage;
        }

        public static bool isKillableAndValidTarget(this Obj_AI_Minion Target, double CalculatedDamage, float distance = float.MaxValue)
        {
            if (Target == null || !Target.IsValidTarget(distance) || Target.Health <= 0)
                return false;

            if (ObjectManager.Player.HasBuff("summonerexhaust"))
                CalculatedDamage *= 0.6;

            BuffInstance dragonSlayerBuff = ObjectManager.Player.GetBuff("s5test_dragonslayerbuff");
            if (Target.Name.ToLowerInvariant().Contains("dragon") && dragonSlayerBuff != null)
                CalculatedDamage -= CalculatedDamage * (0.07 * dragonSlayerBuff.Count);

            if (Target.Name.ToLowerInvariant().Contains("baron") && ObjectManager.Player.HasBuff("barontarget"))
                CalculatedDamage *= 0.5;

            return Target.Health + Target.HPRegenRate + Target.PhysicalShield <= CalculatedDamage;
        }

        public static bool isReadyPerfectly(this Spell spell)
        {
            return spell != null && spell.Slot != SpellSlot.Unknown && spell.Instance.State != SpellState.Cooldown && spell.Instance.State != SpellState.Disabled && spell.Instance.State != SpellState.NoMana && spell.Instance.State != SpellState.NotLearned && spell.Instance.State != SpellState.Surpressed && spell.Instance.State != SpellState.Unknown && spell.Instance.State == SpellState.Ready;
        }

        public static double isImmobileUntil(this Obj_AI_Hero unit)
        {
            var result =
                unit.Buffs.Where(
                    buff =>
                        buff.IsActive && Game.Time <= buff.EndTime &&
                        (buff.Type == BuffType.Charm || buff.Type == BuffType.Knockup || buff.Type == BuffType.Stun ||
                         buff.Type == BuffType.Suppression || buff.Type == BuffType.Snare))
                    .Aggregate(0d, (current, buff) => Math.Max(current, buff.EndTime));
            return (result - Game.Time);
        }
    }
}
