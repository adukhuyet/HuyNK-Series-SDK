using System;
using System.Linq;

using SharpDX;
using MasterSharp.Evade;


using System.Collections.Generic;

using LeagueSharp;
using LeagueSharp.Common;

using Color = System.Drawing.Color;
namespace MasterSharp
{
    internal class MasterYi
    {
        public static Spell Q = new Spell(SpellSlot.Q, 600);
        public static Spell W = new Spell(SpellSlot.W, 0);
        public static Spell E = new Spell(SpellSlot.E, 0);
        public static Spell R = new Spell(SpellSlot.R, 0);
        public static SpellSlot Smite = SpellSlot.Unknown;
        public static Obj_AI_Base SelectedTarget = null;

        public static void SetSkillShots()
        {
            SetupSmite();
        }

        public static void SetupSmite()
        {
            if (Player.Spellbook.GetSpell(SpellSlot.Summoner1).SData.Name.ToLower().Contains("smite"))
            {
                Smite = SpellSlot.Summoner1;
            }
            else if (Player.Spellbook.GetSpell(SpellSlot.Summoner2).SData.Name.ToLower().Contains("smite"))
            {
                Smite = SpellSlot.Summoner2;
            }
        }
        public static float Get_R_Dmg(Obj_AI_Hero target)
        {
            double dmg = 0;

            dmg += ObjectManager.Player.GetSpellDamage(target, SpellSlot.Q);

            var rPred = Q.GetPrediction(target);
            var collisionCount = rPred.CollisionObjects.Count;

            if (collisionCount >= 7)
                dmg = dmg * .3;
            else if (collisionCount != 0)
                dmg = dmg * ((10 - collisionCount) / 10);

          
            return (float)dmg;
        }


            	
                   

                
        
        public static void SlayMaderDuker(Obj_AI_Base target)
        {
            try
            {
                if (target == null)
                {
                    return;
                }
                UseQtoKill(target);
                if (MasterSharp.Config.Item("useSmite").GetValue<bool>())
                {
                    UseSmiteOnTarget(target);
                }

                if (target.Distance(Player) < 500)
                {
                    SumItems.cast(SummonerItems.ItemIds.Ghostblade);
                }

                if (target.Distance(Player) < 300)
                {
                    SumItems.cast(SummonerItems.ItemIds.Hydra);
                }

                if (target.Distance(Player) < 300)
                {
                    SumItems.cast(SummonerItems.ItemIds.Tiamat);
                }

                if (target.Distance(Player) < 300)
                {
                    SumItems.cast(SummonerItems.ItemIds.Cutlass, target);
                }

                if (target.Distance(Player) < 500 && (Player.Health/Player.MaxHealth)*100 < 85)
                {
                    SumItems.cast(SummonerItems.ItemIds.BotRk, target);
                }

                if (MasterSharp.Config.Item("useQ").GetValue<bool>())
                {
                    UseQSmart(target);
                }

                if (MasterSharp.Config.Item("useE").GetValue<bool>())
                {
                    UseESmart(target);
                }

                if (MasterSharp.Config.Item("useR").GetValue<bool>())
                {
                    UseRSmart(target);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        public static void UseQtoKill(Obj_AI_Base target)
        {
            if (Q.IsReady() && (target.Health <= Q.GetDamage(target) || AmLow(0.20f)))
            {
                Q.Cast(target, MasterSharp.Config.Item("packets").GetValue<bool>());
            }
        }

        public static void UseESmart(Obj_AI_Base target)
        {
            if (LxOrbwalker.InAutoAttackRange(target) && E.IsReady() && (AaToKill(target) > 2 || AmLow()))
            {
                E.Cast(MasterSharp.Config.Item("packets").GetValue<bool>());
            }
        }

        public static void UseRSmart(Obj_AI_Base target)
        {
            if (LxOrbwalker.InAutoAttackRange(target) && R.IsReady() && AaToKill(target) > 5)
            {
                R.Cast(MasterSharp.Config.Item("packets").GetValue<bool>());
            }
        }

        public static void UseQSmart(Obj_AI_Base target)
        {
            if (!Q.IsReady() || target.Path.Count() == 0 || !target.IsMoving)
            {
                return;
            }

            var nextEnemPath = target.Path[0].To2D();
            var dist = Player.Position.To2D().Distance(target.Position.To2D());
            var distToNext = nextEnemPath.Distance(Player.Position.To2D());
            if (distToNext <= dist)
            {
                return;
            }

            var msDif = Player.MoveSpeed - target.MoveSpeed;
            if (msDif <= 0 && !LxOrbwalker.InAutoAttackRange(target) && LxOrbwalker.CanAttack())
            {
                Q.Cast(target);
            }

            var reachIn = dist/msDif;
            if (reachIn > 4)
            {
                Q.Cast(target);
            }
        }

        public static void UseSmiteOnTarget(Obj_AI_Base target)
        {
            if (Smite == SpellSlot.Unknown || Player.Spellbook.CanUseSpell(Smite) != SpellState.Ready)
            {
                return;
            }

            if (target.Distance(Player, true) <= 700*700 &&
                (YiGotItemRange(3714, 3718) || YiGotItemRange(3706, 3710)))
            {
                Player.Spellbook.CastSpell(Smite, target);
            }
        }

        public static bool AmLow(float lownes = .25f)
        {
            return Player.Health/Player.MaxHealth < lownes;
        }

        public static int AaToKill(Obj_AI_Base target)
        {
            return 1 + (int) (target.Health/Player.GetAutoAttackDamage(target));
        }

        public static void EvadeBuff(BuffInstance buf, TargetedSkills.TargSkill skill)
        {
            if (Q.IsReady() && JumpEnesAround() != 0 && buf.EndTime - Game.Time < skill.Delay/1000)
            {
                // Console.WriteLine("evade buuf");
                UseQonBest();
            }
            else if (W.IsReady() && (!Q.IsReady() || JumpEnesAround() != 0) && buf.EndTime - Game.Time < 0.4f)
            {
                const int dontMove = 400;
                LxOrbwalker.CantMoveTill = Environment.TickCount + dontMove;
                W.Cast();
            }
        }

        public static void EvadeDamage(int useQ, int useW, GameObjectProcessSpellCastEventArgs psCast, int delay = 250)
        {
            if (useQ != 0 && Q.IsReady() && JumpEnesAround() != 0)
            {
                if (delay != 0)
                {
                    Utility.DelayAction.Add(delay, UseQonBest);
                }
                else
                {
                    UseQonBest();
                }
            }
            else if (useW != 0 && W.IsReady())
            {
                var dontMove = (psCast.CastedSpellCount > 2) ? 2000 : psCast.CastedSpellCount*1000;
                LxOrbwalker.CantMoveTill = Environment.TickCount + (int) dontMove;
                W.Cast();
            }
        }

        public static int JumpEnesAround()
        {
            return
                ObjectManager.Get<Obj_AI_Base>()
                    .Count(ob => ob.IsEnemy && (ob is Obj_AI_Minion || ob is Obj_AI_Hero) &&
                                 ob.Distance(Player) < 600 && !ob.IsDead);
        }

        public static void EvadeSkillShot(Skillshot sShot)
        {
            var sd = SpellDatabase.GetByMissileName(sShot.SpellData.MissileSpellName);
            if (LxOrbwalker.CurrentMode == LxOrbwalker.Mode.Combo &&
                (MasterSharp.SkillShotMustBeEvaded(sd.MenuItemName) ||
                 MasterSharp.SkillShotMustBeEvadedW(sd.MenuItemName)))
            {
                var spellDamage = (float) sShot.Unit.GetSpellDamage(Player, sd.SpellName);
                var willKill = Player.Health <= spellDamage;
                if (Q.IsReady() && JumpEnesAround() != 0 && (MasterSharp.SkillShotMustBeEvaded(sd.MenuItemName)) ||
                    willKill)
                {
                    UseQonBest();
                }
                else if ((!Q.IsReady(150) || !MasterSharp.SkillShotMustBeEvaded(sd.MenuItemName)) && W.IsReady() &&
                         (MasterSharp.SkillShotMustBeEvadedW(sd.MenuItemName)))
                {
                    LxOrbwalker.CantMoveTill = Environment.TickCount + 500;
                    W.Cast();
                }
            }

            if (LxOrbwalker.CurrentMode != LxOrbwalker.Mode.None &&
                (MasterSharp.SkillShotMustBeEvadedAllways(sd.MenuItemName) ||
                 MasterSharp.SkillShotMustBeEvadedWAllways(sd.MenuItemName)))
            {
                var spellDamage = (float) sShot.Unit.GetSpellDamage(Player, sd.SpellName);
                var willKill = Player.Health <= spellDamage;
                if (Q.IsReady() && JumpEnesAround() != 0 &&
                    (MasterSharp.SkillShotMustBeEvadedAllways(sd.MenuItemName) || willKill))
                {
                    UseQonBest();
                }
                else if ((!Q.IsReady() || !MasterSharp.SkillShotMustBeEvadedAllways(sd.MenuItemName)) && W.IsReady() &&
                         (MasterSharp.SkillShotMustBeEvadedWAllways(sd.MenuItemName) || willKill))
                {
                    LxOrbwalker.CantMoveTill = Environment.TickCount + 500;
                    W.Cast();
                }
            }
        }

        public static void UseQonBest()
        {
            try
            {
                if (!Q.IsReady())
                {
                    // Console.WriteLine("Fuk uo here ");
                    return;
                }

                if (SelectedTarget != null)
                {
                    if (SelectedTarget.Distance(Player) < 600)
                    {
                        // Console.WriteLine("Q on targ ");
                        Q.Cast(SelectedTarget, MasterSharp.Config.Item("packets").GetValue<bool>());
                        return;
                    }

                    var bestOther =
                        ObjectManager.Get<Obj_AI_Base>()
                            .Where(
                                ob =>
                                    ob.IsEnemy && (ob is Obj_AI_Minion || ob is Obj_AI_Hero) &&
                                    ob.Distance(Player) < 600 && !ob.IsDead)
                            .OrderBy(ob => ob.Distance(SelectedTarget, true)).FirstOrDefault();
                    // Console.WriteLine("do shit? " + bestOther.Name);

                    if (bestOther != null)
                    {
                        Q.Cast(bestOther, MasterSharp.Config.Item("packets").GetValue<bool>());
                    }
                }
                else
                {
                    var bestOther =
                        ObjectManager.Get<Obj_AI_Base>()
                            .Where(
                                ob =>
                                    ob.IsEnemy && (ob is Obj_AI_Minion || ob is Obj_AI_Hero) &&
                                    ob.Distance(Player) < 600 && !ob.IsDead)
                            .OrderBy(ob => ob.Distance(Game.CursorPos, true)).FirstOrDefault();
                    // Console.WriteLine("do shit? " + bestOther.Name);

                    if (bestOther != null)
                    {
                        Q.Cast(bestOther, MasterSharp.Config.Item("packets").GetValue<bool>());
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        public static bool YiGotItemRange(int from, int to)
        {
            return Player.InventoryItems.Any(item => (int) item.Id >= @from && (int) item.Id <= to);
        }

        public static Obj_AI_Hero Player = ObjectManager.Player;
        public static SummonerItems SumItems = new SummonerItems(Player);
        public static Spellbook SBook = Player.Spellbook;
        public static SpellDataInst Qdata = SBook.GetSpell(SpellSlot.Q);
        public static SpellDataInst Wdata = SBook.GetSpell(SpellSlot.W);
        public static SpellDataInst Edata = SBook.GetSpell(SpellSlot.E);
        public static SpellDataInst Rdata = SBook.GetSpell(SpellSlot.R);
    }
}
