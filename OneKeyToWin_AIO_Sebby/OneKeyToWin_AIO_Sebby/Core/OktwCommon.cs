﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
namespace OneKeyToWin_AIO_Sebby
{
    class YasuoWall
    {
        public Vector3 YasuoPosition { get; set; }
        public float CastTime { get; set; }
        public Vector3 CastPosition { get; set; }
        public float WallLvl { get; set; }

        public YasuoWall()
        {
            CastTime = 0;
        }
    }

    class OktwCommon
    {
        private static int LastAATick = Utils.GameTimeTickCount;
        public static bool 
            blockMove = false,
            blockAttack = false,
            blockSpells = false;

        public static Obj_AI_Hero Player { get { return ObjectManager.Player; } }
        public static Orbwalking.Orbwalker Orbwalker = Program.Orbwalker;
        private static List<Obj_AI_Base> minions;
        private static YasuoWall yasuoWall = new YasuoWall();
        public void LoadOKTW()
        {
            Obj_AI_Base.OnIssueOrder += Obj_AI_Base_OnIssueOrder;
            Spellbook.OnCastSpell +=Spellbook_OnCastSpell;
            Obj_AI_Base.OnProcessSpellCast += Obj_AI_Base_OnProcessSpellCast;
            Obj_AI_Base.OnDoCast += Obj_AI_Base_OnDoCast;
            Spellbook.OnStopCast += SpellbookOnStopCast;
        }

        private void SpellbookOnStopCast(Spellbook sender, SpellbookStopCastEventArgs args)
        {
            if (sender.Owner.IsMe)
            {
                if(args.StopAnimation && args.DestroyMissile)
                    LastAATick = 0;
            }
        }

        private void Obj_AI_Base_OnDoCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            if (sender.IsMe)
            {
                if (!args.SData.IsAutoAttack())
                {
                    if (Utils.GameTimeTickCount + Game.Ping / 2 + 25 >= LastAATick + Player.AttackDelay * 1000)
                    {
                        //Program.debug("Can attack: " + Orbwalking.CanAttack());
                        Orbwalking.ResetAutoAttackTimer();
                    }
                }
            }
        }

        private void Obj_AI_Base_OnProcessSpellCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            if (sender.IsMe)
            {
                if (args.SData.IsAutoAttack())
                {
                    //Program.debug(Utils.GameTimeTickCount + " AA detection " + args.SData.Name);
                    LastAATick = Utils.GameTimeTickCount - Game.Ping / 2;
                }
            }

            if (!sender.IsEnemy || sender.IsMinion || args.SData.IsAutoAttack() || !sender.IsValid<Obj_AI_Hero>() || Player.Distance(sender.ServerPosition) > 2000)
                return;

            if (args.SData.Name == "YasuoWMovingWall")
            {
                yasuoWall.CastTime = Game.Time;
                yasuoWall.CastPosition = sender.Position.Extend(args.End, 400);
                yasuoWall.YasuoPosition = sender.Position;
                yasuoWall.WallLvl = sender.Spellbook.Spells[1].Level;
            }
        }

        public static bool CanHarras()
        {            if ( Player.IsWindingUp)
                return false;
            //if (!Program.Farm)
              //  return true;
            minions = MinionManager.GetMinions(Player.Position, Player.AttackRange+300, MinionTypes.All);
            //public static List<Obj_AI_Base> GetMinions(Vector3 from, float range, MinionTypes type = MinionTypes.All, MinionTeam team = MinionTeam.Enemy, MinionOrderTypes order = MinionOrderTypes.Health);
            var minionsAlly = MinionManager.GetMinions(Player.Position, Player.AttackRange + 200, MinionTypes.All, MinionTeam.Ally);

            if (minions == null || minions.Count == 0)
                return true;

            var minion = minions.First(minion2 => minion2.IsValidTarget());

            if (minion.Health < Player.GetAutoAttackDamage(minion) + minionsAlly.Count * minion.GetAutoAttackDamage(minion))
                return false;
            else
                return true;
            
        }

        public static float GetEchoLudenDamage(Obj_AI_Hero target)
        {
            float totalDamage = 0;

            if (Player.HasBuff("itemmagicshankcharge"))
            {
                if (Player.GetBuff("itemmagicshankcharge").Count == 100)
                {
                    totalDamage += (float)Player.CalcDamage(target, Damage.DamageType.Magical, 100 + 0.1 * Player.FlatMagicDamageMod);
                }
            }
            return totalDamage; 
        }

        private void Spellbook_OnCastSpell(Spellbook sender, SpellbookCastSpellEventArgs args)
        {
            if (blockSpells)
            {
                args.Process = false;
            }
        }

        private void Obj_AI_Base_OnIssueOrder(Obj_AI_Base sender, GameObjectIssueOrderEventArgs args)
        {
            if (!sender.IsMe)
                return;
            
            if (blockMove  && !args.IsAttackMove)
            {
                args.Process = false;
            } 
            if (blockAttack && args.IsAttackMove)
            {
                args.Process = false;
            }
        }

        public static bool CollisionYasuo(Vector3 from, Vector3 to)
        {
            if ( Game.Time - yasuoWall.CastTime > 4)
                return false;

            var level = yasuoWall.WallLvl;
            var wallWidth = (350 + 50 * level);
            var wallDirection = (yasuoWall.CastPosition.To2D() - yasuoWall.YasuoPosition.To2D()).Normalized().Perpendicular();
            var wallStart = yasuoWall.CastPosition.To2D() + wallWidth / 2f * wallDirection;
            var wallEnd = wallStart - wallWidth * wallDirection;

            if (wallStart.Intersection(wallEnd, to.To2D(), from.To2D()).Intersects)
            {
                return true;
            }
            return false;
        }

        public static bool IsFaced(Obj_AI_Hero target)
        {
            Vector2 LastWaypoint = target.GetWaypoints().Last();
            if (LastWaypoint.Distance(Player.Position) < target.Distance(Player.Position))
                return true;

            return false;
        }

        public static float GetKsDamage(Obj_AI_Base t, Spell QWER)
        {
            var totalDmg = QWER.GetDamage(t);

            if (Player.HasBuff("itemmagicshankcharge"))
            {
                if (Player.GetBuff("itemmagicshankcharge").Count == 100)
                {
                    totalDmg += (float)Player.CalcDamage(t, Damage.DamageType.Magical, 100 + 0.1 * Player.FlatMagicDamageMod);
                }
            }

            if (Player.HasBuff("summonerexhaust"))
                totalDmg = totalDmg * 0.6f;

            if (t.HasBuff("ferocioushowl"))
                totalDmg = totalDmg * 0.7f;

            if (t is Obj_AI_Hero)
            {
                var champion = (Obj_AI_Hero)t;
                if (champion.ChampionName == "Blitzcrank" && !champion.HasBuff("BlitzcrankManaBarrierCD") && !champion.HasBuff("ManaBarrier"))
                {
                    totalDmg -= champion.Mana / 2f;
                }
            }

            var extraHP = t.Health - HealthPrediction.GetHealthPrediction(t, 500);

            totalDmg += extraHP;
            totalDmg -= t.HPRegenRate;
            totalDmg -= t.PercentLifeStealMod * 0.005f * t.FlatPhysicalDamageMod;

            return totalDmg;
        }

        public static bool ValidUlt(Obj_AI_Base target)
        {
            if (target.HasBuffOfType(BuffType.PhysicalImmunity)
                || target.HasBuffOfType(BuffType.SpellImmunity)
                || target.IsZombie
                || target.IsInvulnerable
                || target.HasBuffOfType(BuffType.Invulnerability)
                || target.HasBuffOfType(BuffType.SpellShield)
                || target.HasBuff("deathdefiedbuff")
                || target.HasBuff("Undying Rage")
                || target.HasBuff("Chrono Shift")
                || target.HasBuff("kindrednodeathbuff")
                )
                return false;
            else
                return true;
        }

        public static bool CanMove(Obj_AI_Hero target)
        {
            if (target.HasBuffOfType(BuffType.Stun) || target.HasBuffOfType(BuffType.Snare) || target.HasBuffOfType(BuffType.Knockup) ||
                target.HasBuffOfType(BuffType.Charm) || target.HasBuffOfType(BuffType.Fear) || target.HasBuffOfType(BuffType.Knockback) ||
                target.HasBuffOfType(BuffType.Taunt) || target.HasBuffOfType(BuffType.Suppression) ||
                target.IsStunned || target.IsChannelingImportantSpell() || target.MoveSpeed < 50f)
            {
                return false;
            }
            else
                return true;
        }

        public static int CountEnemiesInRangeDeley(Vector3 position, float range, float delay)
        {
            int count = 0;
            foreach (var t in Program.Enemies.Where(t => t.IsValidTarget()))
            {
                Vector3 prepos = Prediction.GetPrediction(t, delay).CastPosition;
                if (position.Distance(prepos) < range)
                    count++;
            }
            return count;
        }

        public static void DrawLineRectangle(Vector3 start2, Vector3 end2, int radius, float width, System.Drawing.Color color)
        {
            Vector2 start = start2.To2D();
            Vector2 end = end2.To2D();
            var dir = (end - start).Normalized();
            var pDir = dir.Perpendicular();

            var rightStartPos = start + pDir * radius;
            var leftStartPos = start - pDir * radius;
            var rightEndPos = end + pDir * radius;
            var leftEndPos = end - pDir * radius;

            var rStartPos = Drawing.WorldToScreen(new Vector3(rightStartPos.X, rightStartPos.Y, ObjectManager.Player.Position.Z));
            var lStartPos = Drawing.WorldToScreen(new Vector3(leftStartPos.X, leftStartPos.Y, ObjectManager.Player.Position.Z));
            var rEndPos = Drawing.WorldToScreen(new Vector3(rightEndPos.X, rightEndPos.Y, ObjectManager.Player.Position.Z));
            var lEndPos = Drawing.WorldToScreen(new Vector3(leftEndPos.X, leftEndPos.Y, ObjectManager.Player.Position.Z));

            Drawing.DrawLine(rStartPos, rEndPos, width, color);
            Drawing.DrawLine(lStartPos, lEndPos, width, color);
            Drawing.DrawLine(rStartPos, lStartPos, width, color);
            Drawing.DrawLine(lEndPos, rEndPos, width, color);
        }

        public static List<Vector3> CirclePoints(float CircleLineSegmentN, float radius, Vector3 position)
        {
            List<Vector3> points = new List<Vector3>();
            for (var i = 1; i <= CircleLineSegmentN; i++)
            {
                var angle = i * 2 * Math.PI / CircleLineSegmentN;
                var point = new Vector3(position.X + radius * (float)Math.Cos(angle), position.Y + radius * (float)Math.Sin(angle), position.Z);
                points.Add(point);
            }
            return points;
        }

        public static bool GetCollision(Obj_AI_Base target, Spell QWER, bool champion, bool minion)
        {
            var rDmg = QWER.GetDamage(target);
            int collision = 0;
            PredictionOutput output = QWER.GetPrediction(target);
            Vector2 direction = output.CastPosition.To2D() - ObjectManager.Player.Position.To2D();
            direction.Normalize();
            if (champion)
            {
                foreach (var enemy in Program.Enemies.Where(x => x.IsEnemy && x.IsValidTarget()))
                {
                    PredictionOutput prediction = QWER.GetPrediction(enemy);
                    Vector3 predictedPosition = prediction.CastPosition;
                    Vector3 v = output.CastPosition - ObjectManager.Player.ServerPosition;
                    Vector3 w = predictedPosition - ObjectManager.Player.ServerPosition;
                    double c1 = Vector3.Dot(w, v);
                    double c2 = Vector3.Dot(v, v);
                    double b = c1 / c2;
                    Vector3 pb = ObjectManager.Player.ServerPosition + ((float)b * v);
                    float length = Vector3.Distance(predictedPosition, pb);
                    if (length < QWER.Width )
                        return true;
                }
            }
            if (minion)
            {
                var allMinions = MinionManager.GetMinions(ObjectManager.Player.ServerPosition, QWER.Range, MinionTypes.All);
                foreach (var enemy in allMinions.Where(x => x.IsEnemy && x.IsValidTarget()))
                {
                    PredictionOutput prediction = QWER.GetPrediction(enemy);
                    Vector3 predictedPosition = prediction.CastPosition;
                    Vector3 v = output.CastPosition - ObjectManager.Player.ServerPosition;
                    Vector3 w = predictedPosition - ObjectManager.Player.ServerPosition;
                    double c1 = Vector3.Dot(w, v);
                    double c2 = Vector3.Dot(v, v);
                    double b = c1 / c2;
                    Vector3 pb = ObjectManager.Player.ServerPosition + ((float)b * v);
                    float length = Vector3.Distance(predictedPosition, pb);
                    if (length < QWER.Width)
                        return true;
                }
            }
            return false;
        }

        public static int GetBuffCount(Obj_AI_Base target, String buffName)
        {
            foreach (var buff in target.Buffs.Where(buff => buff.Name == buffName))
            {
                if (buff.Count == 0)
                    return 1;
                else
                    return buff.Count;
            }
            return 0;
        }

        public static int CountEnemyMinions(Obj_AI_Base target, float range)
        {
            var allMinions = MinionManager.GetMinions(target.Position, range);
            if (allMinions != null)
                return allMinions.Count;
            else
                return 0;
        }


        public static float GetPassiveTime(Obj_AI_Base target, String buffName)
        {
            return
                target.Buffs.OrderByDescending(buff => buff.EndTime - Game.Time)
                    .Where(buff => buff.Name == buffName)
                    .Select(buff => buff.EndTime)
                    .FirstOrDefault() - Game.Time;
        }

        public static int WayPointAnalysis(Obj_AI_Base unit , Spell QWER)
        {
            int HC = 0;

            if (QWER.Delay < 0.25f)
                HC = 2;
            else
                HC = 1;

            if (unit.Path.Count() == 1)
                HC = 2;

            return HC;

        }
    }
}
