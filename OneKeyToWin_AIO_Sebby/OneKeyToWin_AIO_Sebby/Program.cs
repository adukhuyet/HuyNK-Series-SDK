﻿
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using System.Drawing;
using SPrediction;
namespace OneKeyToWin_AIO_Sebby
{
    internal class Program
    {
        
        public static Menu Config;
        public static Orbwalking.Orbwalker Orbwalker;
        public static Spell Q, W, E, R, DrawSpell;
        public static float JungleTime, DrawSpellTime=0;
        public static Obj_AI_Hero jungler = ObjectManager.Player;
        public static int timer, HitChanceNum = 4, tickNum = 4, tickIndex = 0;
        public static Obj_SpawnPoint enemySpawn;
        public static Core.PredictionOutput DrawSpellPos;
        public static List<Obj_AI_Hero> Enemies = new List<Obj_AI_Hero>() , Allies = new List<Obj_AI_Hero>();
        private static Obj_AI_Hero Player { get { return ObjectManager.Player; } }
        public static bool SPredictionLoad = false;
        public static int AIOmode = 0;
        private static float dodgeRange = 420;
        private static float dodgeTime = Game.Time;

        static void Main(string[] args) { CustomEvents.Game.OnGameLoad += GameOnOnGameLoad;}

        private static void GameOnOnGameLoad(EventArgs args)
        {

            Q = new Spell(SpellSlot.Q);
            E = new Spell(SpellSlot.E);
            W = new Spell(SpellSlot.W);
            R = new Spell(SpellSlot.R);

            Config = new Menu("OneKeyToWin AIO", "OneKeyToWin_AIO" + ObjectManager.Player.ChampionName, true);

            #region MENU ABOUT OKTW
            Config.SubMenu("About OKTW©").AddItem(new MenuItem("debug", "Debug").SetValue(false));
            Config.SubMenu("About OKTW©").AddItem(new MenuItem("0", "OneKeyToWin© by Sebby"));
            Config.SubMenu("About OKTW©").AddItem(new MenuItem("1", "visit joduska.me"));
            Config.SubMenu("About OKTW©").AddItem(new MenuItem("2", "DONATE: kaczor.sebastian@gmail.com"));
            #endregion

            Config.AddItem(new MenuItem("AIOmode", "(Press F5 after the change) AIO mode:", true).SetValue(new StringList(new[] { "Utility and champion", "Only Champion", "Only Utility" }, 0)));

            AIOmode = Config.Item("AIOmode", true).GetValue<StringList>().SelectedIndex;

            var targetSelectorMenu = new Menu("Target Selector", "Target Selector");
            TargetSelector.AddToMenu(targetSelectorMenu);
            Config.AddSubMenu(targetSelectorMenu);

            if (AIOmode != 2)
            {
                Config.AddSubMenu(new Menu("Orbwalking", "Orbwalking"));
                Orbwalker = new Orbwalking.Orbwalker(Config.SubMenu("Orbwalking"));
            }

            if (AIOmode != 1)
            {
                foreach (var enemy in ObjectManager.Get<Obj_AI_Hero>().Where(enemy => enemy.IsEnemy))
                    Config.SubMenu("Utility, Draws OKTW©").SubMenu("GankTimer").SubMenu("Custome jungler (select one)").AddItem(new MenuItem("ro" + enemy.ChampionName, enemy.ChampionName).SetValue(false));

                Config.SubMenu("Utility, Draws OKTW©").SubMenu("GankTimer").AddItem(new MenuItem("timer", "GankTimer").SetValue(true));
                Config.SubMenu("Utility, Draws OKTW©").SubMenu("GankTimer").AddItem(new MenuItem("1", "RED - be careful"));
                Config.SubMenu("Utility, Draws OKTW©").SubMenu("GankTimer").AddItem(new MenuItem("2", "ORANGE - you have time"));
                Config.SubMenu("Utility, Draws OKTW©").SubMenu("GankTimer").AddItem(new MenuItem("3", "GREEN - jungler visable"));
                Config.SubMenu("Utility, Draws OKTW©").SubMenu("GankTimer").AddItem(new MenuItem("4", "CYAN jungler dead - take objectives"));
            }

            Config.SubMenu("Prediction MODE").AddItem(new MenuItem("PredictionMODE", "Prediction MODE", true).SetValue(new StringList(new[] { "Common prediction", "OKTW© PREDICTION", "SPediction press F5 if not loaded"}, 1)));
            Config.SubMenu("Prediction MODE").AddItem(new MenuItem("HitChance", "Hit Chance", true).SetValue(new StringList(new[] { "Very High", "High", "Medium" }, 0)));
            Config.SubMenu("Prediction MODE").AddItem(new MenuItem("debugPred", "Draw Aiming OKTW© PREDICTION").SetValue(false));

            if (Config.Item("PredictionMODE", true).GetValue<StringList>().SelectedIndex == 2)
            {
                SPrediction.Prediction.Initialize(Config.SubMenu("Prediction MODE"));
                SPredictionLoad = true;
            }
            else
                Config.SubMenu("Prediction MODE").AddItem(new MenuItem("322", "SPREDICTION NOT LOADED"));

            Config.SubMenu("Extra settings OKTW©").AddItem(new MenuItem("supportMode", "Support Mode", true).SetValue(false));
            Config.SubMenu("Extra settings OKTW©").AddItem(new MenuItem("comboDisableMode", "Disable auto-attack in combo mode", true).SetValue(false));
            Config.SubMenu("Extra settings OKTW©").AddItem(new MenuItem("manaDisable", "Disable mana manager in combo", true).SetValue(false));
            Config.SubMenu("Extra settings OKTW©").AddItem(new MenuItem("collAA", "Disable auto-attack if Yasuo wall collision", true).SetValue(true));
            Config.SubMenu("Extra settings OKTW©").SubMenu("Anti-Melee Positioning Assistant OKTW©").AddItem(new MenuItem("positioningAssistant", "Anti-Melee Positioning Assistant OKTW©").SetValue(true));

            foreach (var enemy in ObjectManager.Get<Obj_AI_Hero>().Where(enemy => enemy.IsEnemy ))
                Config.SubMenu("Extra settings OKTW©").SubMenu("Anti-Melee Positioning Assistant OKTW©").SubMenu("Positioning Assistant:").AddItem(new MenuItem("posAssistant" + enemy.ChampionName, enemy.ChampionName).SetValue(true));
            Config.SubMenu("Extra settings OKTW©").SubMenu("Anti-Melee Positioning Assistant OKTW©").AddItem(new MenuItem("positioningAssistantDraw", "Show notification").SetValue(true));


            Config.Item("manaDisable", true).SetValue(false);
            Config.Item("supportMode", true).SetValue(false);

            #region LOAD CHAMPIONS

            if (AIOmode != 2)
            {
                switch (Player.ChampionName)
                {
                    case "Jinx":
                        new Jinx().LoadOKTW();
                        break;
                    case "Sivir":
                        new Sivir().LoadOKTW();
                        break;
                    case "Ezreal":
                        new Ezreal().LoadOKTW();
                        break;
                    case "KogMaw":
                        new KogMaw().LoadOKTW();
                        break;
                    case "Annie":
                        new Annie().LoadOKTW();
                        break;
                    case "Ashe":
                        new Ashe().LoadOKTW();
                        break;
                    case "MissFortune":
                        new MissFortune().LoadOKTW();
                        break;
                    case "Quinn":
                        new Quinn().LoadOKTW();
                        break;
                    case "Kalista":
                        new Kalista().LoadOKTW();
                        break;
                    case "Caitlyn":
                        new Caitlyn().LoadOKTW();
                        break;
                    case "Graves":
                        new Graves().LoadOKTW();
                        break;
                    case "Urgot":
                        new Urgot().LoadOKTW();
                        break;
                    case "Anivia":
                        new Anivia().LoadOKTW();
                        break;
                    case "Orianna":
                        new Orianna().LoadOKTW();
                        break;
                    case "Ekko":
                        new Ekko().LoadOKTW();
                        break;
                    case "Vayne":
                        new Vayne().LoadOKTW();
                        break;
                    case "Lucian":
                        new Lucian().LoadOKTW();
                        break;
                    case "Darius":
                        new Champions.Darius().LoadOKTW();
                        break;
                    case "Blitzcrank":
                        new Champions.Blitzcrank().LoadOKTW();
                        break;
                    case "Corki":
                        new Champions.Corki().LoadOKTW();
                        break;
                    case "Varus":
                        new Champions.Varus().LoadOKTW();
                        break;
                    case "Twitch":
                        new Champions.Twitch().LoadOKTW();
                        break;
                    case "Tristana":
                        new Champions.Tristana().LoadMenuOKTW();
                        break;
                    case "Xerath":
                        new Champions.Xerath().LoadOKTW();
                        break;
                    case "Jayce":
                        new Champions.Jayce().LoadOKTW();
                        break;
                    case "Kayle":
                        new Champions.Kayle().LoadOKTW();
                        break;
                    case "Thresh":
                        new Champions.Thresh().LoadOKTW();
                        break;
                    case "Draven":
                        new Champions.Draven().LoadOKTW();
                        break;
                    case "Evelynn":
                        new Champions.Evelynn().LoadOKTW();
                        break;
                    case "Ahri":
                        new Champions.Ahri().LoadOKTW();
                        break;
                    case "Brand":
                        new Champions.Brand().LoadOKTW();
                        break;
                    case "Morgana":
                        new Champions.Morgana().LoadOKTW();
                        break;
                    case "Lux":
                        new Champions.Lux().LoadOKTW();
                        break;
                    case "Malzahar":
                        new Champions.Malzahar().LoadOKTW();
                        break;
                    case "Karthus":
                        new Champions.Karthus().LoadOKTW();
                        break;
                }
            }

            #endregion

            foreach (var hero in ObjectManager.Get<Obj_AI_Hero>())
            {
                if (hero.IsEnemy)
                {
                    Enemies.Add(hero);
                    if (IsJungler(hero))
                        jungler = hero;
                }
                if (hero.IsAlly)
                    Allies.Add(hero);
            }

            if (Config.Item("debug").GetValue<bool>())
            {
                new Core.OKTWlab().LoadOKTW();
            }

            if (AIOmode != 1)
            {
                new Activator().LoadOKTW();
                new Core.OKTWward().LoadOKTW();
                new Core.AutoLvlUp().LoadOKTW();
                new Core.OKTWtracker().LoadOKTW();
                new Core.OKTWdraws().LoadOKTW();
            }

            new OktwCommon().LoadOKTW();
            new Core.OKTWtracker().LoadOKTW();
           

            Config.AddItem(new MenuItem("aiomodes", "!!! PRESS F5 TO RELOAD MODE !!!" ));
            //new Core.OKTWtargetSelector().LoadOKTW();

            //new AfkMode().LoadOKTW();
            Config.AddToMainMenu();
            Game.OnUpdate += OnUpdate;
            Orbwalking.BeforeAttack += Orbwalking_BeforeAttack;
            Drawing.OnDraw += OnDraw;
        }

        private static void PositionHelper()
        {
            if (Player.ChampionName == "Draven" || !Config.Item("positioningAssistant").GetValue<bool>() || AIOmode == 2)
                return;

            if (Player.IsMelee)
            {
                Orbwalker.SetOrbwalkingPoint(new Vector3());
                return;
            }

            foreach (var enemy in Enemies.Where(enemy => enemy.IsMelee && enemy.IsValidTarget(dodgeRange) && Config.Item("posAssistant" + enemy.ChampionName).GetValue<bool>() && enemy.IsFacing(Player)))
            {
                if (Player.FlatMagicDamageMod > Player.FlatPhysicalDamageMod)
                    OktwCommon.blockAttack = true;

                var points = OktwCommon.CirclePoints(20, 200, Player.Position);
                Vector3 bestPoint = Vector3.Zero;

                foreach (var point in points)
                {
                    if (enemy.Distance(point) > dodgeRange && (bestPoint == Vector3.Zero || Game.CursorPos.Distance(point) < Game.CursorPos.Distance(bestPoint)))
                    {
                        bestPoint = point;
                    }
                }

                if (enemy.Distance(bestPoint) > dodgeRange)
                {
                    Orbwalker.SetOrbwalkingPoint(bestPoint);
                    
                }
                else
                {
                    var fastPoint = enemy.ServerPosition.Extend(Player.ServerPosition, dodgeRange);
                    if(fastPoint.CountEnemiesInRange(dodgeRange) <= Player.CountEnemiesInRange(dodgeRange))
                        Orbwalker.SetOrbwalkingPoint(fastPoint);
                }
                dodgeTime = Game.Time;
                return;
            }   

            Orbwalker.SetOrbwalkingPoint(new Vector3());
            if (OktwCommon.blockAttack == true)
                OktwCommon.blockAttack = false;
        }

        private static void Orbwalking_BeforeAttack(Orbwalking.BeforeAttackEventArgs args)
        {

            if (Config.Item("comboDisableMode", true).GetValue<bool>() && Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Combo && 2 * Player.GetAutoAttackDamage((Obj_AI_Base)args.Target) < args.Target.Health)
            {
                args.Process = false;
            }

            if (OktwCommon.CollisionYasuo(Player.ServerPosition, args.Target.Position) && !Player.IsMelee && Config.Item("collAA", true).GetValue<bool>())
            {
                args.Process = false;
            }

            if (Config.Item("supportMode",true).GetValue<bool>() && (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Mixed))
            {
                if (((Obj_AI_Base)Orbwalker.GetTarget()).IsMinion) args.Process = false;
            }
        }

        private static void OnUpdate(EventArgs args)
        {
            PositionHelper();
            tickIndex++;
             
            if (tickIndex > 4)
                tickIndex = 0;

            if (!LagFree(0))
                return;
            
            if(AIOmode != Config.Item("AIOmode", true).GetValue<StringList>().SelectedIndex)
                Config.Item("aiomodes").Show(true);
            else
                Config.Item("aiomodes").Show(false);

            JunglerTimer();
        }

        public static void JunglerTimer()
        {
            if (Config.Item("timer").GetValue<bool>() && jungler != null && jungler.IsValid)
            {
                foreach (var enemy in Enemies.Where(enemy => enemy.IsValid))
                {
                    if (Config.Item("ro" + enemy.ChampionName) != null && Config.Item("ro" + enemy.ChampionName).GetValue<bool>())
                        jungler = enemy;
                }

                if (jungler.IsDead)
                {
                    enemySpawn = ObjectManager.Get<Obj_SpawnPoint>().FirstOrDefault(x => x.IsEnemy);
                    timer = (int)(enemySpawn.Position.Distance(ObjectManager.Player.Position) / 370);
                }
                else if (jungler.IsVisible && jungler.IsValid)
                {
                    float Way = 0;
                    var JunglerPath = Player.GetPath(Player.Position, jungler.Position);
                    var PointStart = Player.Position;
                    if (JunglerPath == null)
                        return;
                    foreach (var point in JunglerPath)
                    {
                        if (PointStart.Distance(point) > 0)
                        {
                            Way += PointStart.Distance(point);
                            PointStart = point;
                        }
                    }
                    timer = (int)(Way / jungler.MoveSpeed);
                }
            }
        }

        public static bool LagFree(int offset)
        {
            if (tickIndex == offset)
                return true;
            else
                return false;
        }

        public static bool Farm { get { return Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.LaneClear || Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Mixed; } }

        public static bool None { get { return (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.None); } }

        public static bool Combo { get {
                if (AIOmode == 2)
                {
                    if (Player.IsMoving)
                        return true;
                    else
                        return false;
                }
                else
                    return (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Combo);

            } }

        public static bool LaneClear { get { return (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.LaneClear); } }

        private static bool IsJungler(Obj_AI_Hero hero) { return hero.Spellbook.Spells.Any(spell => spell.Name.ToLower().Contains("smite")); }

        public static float GetRealDmg(Spell QWER, Obj_AI_Hero target)
        {
            if (Orbwalking.InAutoAttackRange(target) || target.CountAlliesInRange(300) > 0)
                return QWER.GetDamage(target) + (float)ObjectManager.Player.GetAutoAttackDamage(target) * 2;
            else
                return QWER.GetDamage(target);
        }

        public static void CastSpell(Spell QWER, Obj_AI_Base target)
        {
            if (Config.Item("PredictionMODE", true).GetValue<StringList>().SelectedIndex == 1)
            {
                Core.SkillshotType CoreType2 = Core.SkillshotType.SkillshotLine;
                bool aoe2 = false;
                if (QWER.Type == SkillshotType.SkillshotCircle)
                {
                    CoreType2 = Core.SkillshotType.SkillshotCircle;
                    aoe2 = true;
                }
                if (QWER.Width > 80 && !QWER.Collision)
                    aoe2 = true;
                var predInput2 = new Core.PredictionInput
                {
                    Aoe = aoe2,
                    Collision = QWER.Collision,
                    Speed = QWER.Speed,
                    Delay = QWER.Delay,
                    Range = QWER.Range,
                    From = Player.ServerPosition,
                    Radius = QWER.Width,
                    Unit = target,
                    Type = CoreType2
                };
                var poutput2 = Core.Prediction.GetPrediction(predInput2);

                //var poutput2 = QWER.GetPrediction(target);

                if (QWER.Speed != float.MaxValue && OktwCommon.CollisionYasuo(Player.ServerPosition, poutput2.CastPosition))
                    return;

                if (Config.Item("HitChance", true).GetValue<StringList>().SelectedIndex == 0)
                {
                    if (poutput2.Hitchance >= Core.HitChance.VeryHigh)
                        QWER.Cast(poutput2.CastPosition);
                    else if (predInput2.Aoe && poutput2.AoeTargetsHitCount > 1 && poutput2.Hitchance >= Core.HitChance.High)
                    {
                        QWER.Cast(poutput2.CastPosition);
                    }

                }
                else if (Config.Item("HitChance", true).GetValue<StringList>().SelectedIndex == 1)
                {
                    if (poutput2.Hitchance >= Core.HitChance.High)
                        QWER.Cast(poutput2.CastPosition);

                }
                else if (Config.Item("HitChance", true).GetValue<StringList>().SelectedIndex == 2)
                {
                    if (poutput2.Hitchance >= Core.HitChance.Medium)
                        QWER.Cast(poutput2.CastPosition);
                }
                if (Game.Time - DrawSpellTime > 0.5)
                {
                    DrawSpell = QWER;
                    DrawSpellTime = Game.Time;

                }
                DrawSpellPos = poutput2;
            }
            else if (Config.Item("PredictionMODE", true).GetValue<StringList>().SelectedIndex == 0)
            {
                if (Config.Item("HitChance", true).GetValue<StringList>().SelectedIndex == 0)
                {
                    QWER.CastIfHitchanceEquals(target, HitChance.VeryHigh);
                    return;
                }
                else if (Config.Item("HitChance", true).GetValue<StringList>().SelectedIndex == 1)
                {
                    QWER.CastIfHitchanceEquals(target, HitChance.High);
                    return;
                }
                else if (Config.Item("HitChance ", true).GetValue<StringList>().SelectedIndex == 2)
                {
                    QWER.CastIfHitchanceEquals(target, HitChance.Medium);
                    return;
                }
            }
            else if (Config.Item("PredictionMODE", true).GetValue<StringList>().SelectedIndex == 2 )
            {
                
                if (target is Obj_AI_Hero && target.IsValid)
                {
                    var t = target as Obj_AI_Hero;
                    if (Config.Item("HitChance", true).GetValue<StringList>().SelectedIndex == 0)
                    {
                        QWER.SPredictionCast(t, HitChance.VeryHigh);
                        return;
                    }
                    else if (Config.Item("HitChance", true).GetValue<StringList>().SelectedIndex == 1)
                    {
                        QWER.SPredictionCast(t, HitChance.High);
                        return;
                    }
                    else if (Config.Item("HitChance ", true).GetValue<StringList>().SelectedIndex == 2)
                    {
                        QWER.SPredictionCast(t, HitChance.Medium);
                        return;
                    }
                }
                else
                {
                    QWER.CastIfHitchanceEquals(target, HitChance.High);
                }
            }
        }

        public static void drawText(string msg, Vector3 Hero, System.Drawing.Color color, int weight = 0)
        {
            var wts = Drawing.WorldToScreen(Hero);
            Drawing.DrawText(wts[0] - (msg.Length) * 5, wts[1] + weight, color, msg);
        }

        public static void debug(string msg)
        {
            if (Config.Item("debug").GetValue<bool>())
            {
                Console.WriteLine(msg);
            }
        }

        private static void OnDraw(EventArgs args)
        {
            if (!SPredictionLoad && (int)Game.Time % 2 == 0 && Config.Item("PredictionMODE", true).GetValue<StringList>().SelectedIndex == 2)
                drawText("PRESS F5 TO LOAD SPREDICTION", Player.Position, System.Drawing.Color.Yellow, -300);

            if (Config.Item("disableDraws").GetValue<bool>())
                return;

            if (Game.Time - dodgeTime < 0.01 && !Player.IsMelee && Config.Item("positioningAssistant").GetValue<bool>() && Config.Item("positioningAssistantDraw").GetValue<bool>())
            {
                Render.Circle.DrawCircle(Player.Position, dodgeRange, System.Drawing.Color.DimGray, 1);
                if((int)(Game.Time * 10) % 2 == 0)
                    drawText("Anti-Melle Positioning Assistant" , Player.Position, System.Drawing.Color.Gray);
            }

            if (Game.Time - DrawSpellTime < 0.5 && Config.Item("debugPred").GetValue<bool>() && Config.Item("PredictionMODE", true).GetValue<StringList>().SelectedIndex == 1  )
            {
                if (DrawSpell.Type == SkillshotType.SkillshotLine)
                    OktwCommon.DrawLineRectangle(DrawSpellPos.CastPosition, Player.Position, (int)DrawSpell.Width, 1, System.Drawing.Color.DimGray);
                if (DrawSpell.Type == SkillshotType.SkillshotCircle)
                    Render.Circle.DrawCircle(DrawSpellPos.CastPosition, DrawSpell.Width, System.Drawing.Color.DimGray, 1);

                drawText("Aiming " + DrawSpellPos.Hitchance, Player.Position.Extend(DrawSpellPos.CastPosition, 400), System.Drawing.Color.Gray);
            }
            
            if (Config.Item("timer").GetValue<bool>() && jungler != null)
            {
                if (jungler == Player)
                    drawText("Jungler not detected", Player.Position, System.Drawing.Color.Yellow, 100);
                else if (jungler.IsDead)
                    drawText("Jungler dead " + timer, Player.Position, System.Drawing.Color.Cyan, 100);
                else if (jungler.IsVisible)
                    drawText("Jungler visable " + timer, Player.Position, System.Drawing.Color.GreenYellow, 100);
                else
                {
                    if (timer > 0)
                        drawText("Jungler in jungle " + timer, Player.Position, System.Drawing.Color.Orange, 100);
                    else if ((int)(Game.Time * 10) % 2 == 0)
                        drawText("BE CAREFUL " + timer, Player.Position, System.Drawing.Color.OrangeRed, 100);
                    if (Game.Time - JungleTime >= 1)
                    {
                        timer = timer - 1;
                        JungleTime = Game.Time;
                    }
                }
            }
        }
    }
}