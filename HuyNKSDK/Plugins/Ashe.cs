using System;
using System.Linq;
using System.Windows.Forms;
using LeagueSharp;
using LeagueSharp.Common;
using LeagueSharp.SDK.Core;
using LeagueSharp.SDK.Core.UI.IMenu.Values;
using LeagueSharp.SDK.Core.Enumerations;
using LeagueSharp.SDK.Core.Wrappers;
using LeagueSharp.SDK.Core.Extensions;
using LeagueSharp.SDK.Core.Utils;

using Color = System.Drawing.Color;
using HitChance = LeagueSharp.SDK.Core.Enumerations.HitChance;
using Menu = LeagueSharp.SDK.Core.UI.IMenu.Menu;
using SkillshotType = LeagueSharp.SDK.Core.Enumerations.SkillshotType;
using Spell = LeagueSharp.SDK.Core.Wrappers.Spells.Spell;

namespace HuyNK_Series_SDK.Plugins
{
    public class Ashe
    {
        private Spell Q, W, E, R;

        public Ashe()
        {
            //Spell
            Q = new Spell(SpellSlot.Q);
            W = new Spell(SpellSlot.W, 1200f) { DamageType = DamageType.Physical };
            E = new Spell(SpellSlot.E);
            R = new Spell(SpellSlot.R, 2500f) { DamageType = DamageType.Physical, MinHitChance = HitChance.High };

            W.SetSkillshot(0.25f, 40f, 902f, true, SkillshotType.SkillshotLine);
            R.SetSkillshot(0.25f, 130f, 1600f, false, SkillshotType.SkillshotLine);

            //Menu
            Menu ComboMenu = new Menu("Combo", "Combo");
            Menu HarassMenu = new Menu("Harass", "Harass");
            Menu LaneClearMenu = new Menu("LaneClear", "LaneClear");
            Menu JungleClearMenu = new Menu("JungleClear", "JungleClear");
            Menu MiscMenu = new Menu("Misc", "Misc");
            Menu DrawingsMenu = new Menu("Drawings", "Drawings");

            ComboMenu.Add(new MenuBool("UseQ", "Use Q", true));
            ComboMenu.Add(new MenuBool("UseW", "Use W", true));
            ComboMenu.Add(new MenuBool("UseE", "Use E", false));
            ComboMenu.Add(new MenuBool("UseR", "Use R", true));

            HarassMenu.Add(new MenuBool("UseW", "Use W", true));

            LaneClearMenu.Add(new MenuBool("UseW", "Use W", false));

            JungleClearMenu.Add(new MenuBool("UseQ", "Use Q", true));
            JungleClearMenu.Add(new MenuBool("UseW", "Use W", true));

            MiscMenu.Add(new MenuBool("UseKillsteal", "Use Killsteal", true));
            MiscMenu.Add(new MenuBool("AutoRimmobile", "Auto R against immobile target", true));

            DrawingsMenu.Add(new MenuSeparator("Wseparator", "W"));
            DrawingsMenu.Add(new MenuBool("DrawW", "Draw W Range", true));
            DrawingsMenu.Add(new MenuColor("WColor", "Color", SharpDX.Color.Blue));

            DrawingsMenu.Add(new MenuSeparator("Rseparator", "R"));
            DrawingsMenu.Add(new MenuBool("DrawR", "Draw R Range", true));
            DrawingsMenu.Add(new MenuColor("RColor", "Color", SharpDX.Color.Blue));

            DrawingsMenu.Add(new MenuSeparator("Dseparator", "DamageIndicator"));

            var UseDamageIndicator = new MenuBool("UseDamageIndicator", "Use DamageIndicator", true);
            var DamageIndicatorFillColor = new MenuColor("DamageIndicatorFillColor", "Color", SharpDX.Color.Goldenrod);

            UseDamageIndicator.ValueChanged += (object sender, EventArgs e) => { DamageIndicator.Enabled = UseDamageIndicator.Value; };
            DamageIndicatorFillColor.ValueChanged += (object sender, EventArgs e) => { DamageIndicator.FillColor = Color.FromArgb(DamageIndicatorFillColor.Color.ToBgra()); };

            DrawingsMenu.Add(UseDamageIndicator);
            DrawingsMenu.Add(DamageIndicatorFillColor);

            MenuProvider.MainMenu.Add(ComboMenu);
            MenuProvider.MainMenu.Add(HarassMenu);
            MenuProvider.MainMenu.Add(LaneClearMenu);
            MenuProvider.MainMenu.Add(JungleClearMenu);
            MenuProvider.MainMenu.Add(MiscMenu);
            MenuProvider.MainMenu.Add(DrawingsMenu);

            //Event
            Game.OnUpdate += Game_OnGameUpdate;
            Drawing.OnDraw += Game_OnDraw;
            DamageIndicator.DamageToUnit = GetComboDamage;

            //Loaded
            Logging.Write()(LogLevel.Info, "HuyNK_Series_SDK: Ashe Loaded!");
        }

        private void Game_OnGameUpdate(EventArgs args)
        {
           
            if (!ObjectManager.Player.IsDead)
            {

                Killsteal();
                AutoRimmobile();
                switch (Variables.Orbwalker.GetActiveMode())
                {
                    case OrbwalkingMode.Combo:
                        Combo();
                        break;
                    case OrbwalkingMode.Hybrid:
                        Harass();
                        break;
                    case OrbwalkingMode.LaneClear:
                        LaneClear();
                        break;
                    case OrbwalkingMode.LastHit:
                       LaneClear();
                        break;
                    
                }
            }
        }

        private void Game_OnDraw(EventArgs args)
        {
            if (!ObjectManager.Player.IsDead)
            {
                if (MenuProvider.MainMenu["Drawings"]["DrawW"].GetValue<MenuBool>().Value && W.IsReady())
                    Drawing.DrawCircle(GameObjects.Player.Position, W.Range, Color.FromArgb(MenuProvider.MainMenu["Drawings"]["WColor"].GetValue<MenuColor>().Color.ToBgra()));

                if (MenuProvider.MainMenu["Drawings"]["DrawR"].GetValue<MenuBool>().Value && R.IsReady())
                    Drawing.DrawCircle(GameObjects.Player.Position, R.Range, Color.FromArgb(MenuProvider.MainMenu["Drawings"]["RColor"].GetValue<MenuColor>().Color.ToBgra()));
            }
        }

        private float GetComboDamage(Obj_AI_Hero Enemy)
        {
            return
                (W.IsReady() ? (float)LeagueSharp.Common.Damage.GetSpellDamage(ObjectManager.Player, Enemy, SpellSlot.W) : 0) +
                (R.IsReady() ? (float)LeagueSharp.Common.Damage.GetSpellDamage(ObjectManager.Player, Enemy, SpellSlot.R) : 0);
        }

        private void Combo()
        {
            if (MenuProvider.MainMenu["Combo"]["UseQ"].GetValue<MenuBool>().Value && Q.IsReady())
                if (ObjectManager.Player.HasBuff("asheqcastready"))
                    if (GameObjects.EnemyHeroes.Any(x => x.InAutoAttackRange()))
                        Q.Cast();

            if (MenuProvider.MainMenu["Combo"]["UseW"].GetValue<MenuBool>().Value && W.IsReady())
                W.CastOnBestTarget();

            if (MenuProvider.MainMenu["Combo"]["UseR"].GetValue<MenuBool>().Value && R.IsReady())
            {
                foreach (var Target in GameObjects.EnemyHeroes.Where(x => Unit.IsValidTarget(x, R.Range) && R.GetPrediction(x).Hitchance >= HitChance.High))
                {
                    //R Logics

                    if (Target.isKillableAndValidTarget(LeagueSharp.Common.Damage.GetSpellDamage(ObjectManager.Player, Target, SpellSlot.R), R.Range) && !Target.InAutoAttackRange())
                        R.Cast(Target);//killable

                    if (Unit.IsValidTarget(Target, 300f))
                        R.Cast(Target);//too close

                    if (Target.isImmobileUntil() > Target.Distance(ObjectManager.Player.ServerPosition) / R.Speed)
                        R.Cast(Target);//immobile
                }
            }
        }

        private void Harass()
        {
            if (MenuProvider.MainMenu["Harass"]["UseW"].GetValue<MenuBool>().Value && W.IsReady())
                W.CastOnBestTarget();
        }

        private void LaneClear()
        {
            if (_Getmenu.get_bool("LaneClear","UseW") && W.IsReady())
            {
                //good than shit
                var FarmLocation = W.GetLineFarmLocation(GameObjects.EnemyMinions.ToList<Obj_AI_Base>());

                if (FarmLocation.MinionsHit >= 2)
                    W.Cast(FarmLocation.Position);
            }
        }

        private void JungleClear()
        {
            if (GameObjects.Jungle.Any(x => x.InAutoAttackRange()))
            {
                if (MenuProvider.MainMenu["JungleClear"]["UseQ"].GetValue<MenuBool>().Value && Q.IsReady())
                    if (ObjectManager.Player.HasBuff("asheqcastready"))
                        Q.Cast();

                if (MenuProvider.MainMenu["JungleClear"]["UseW"].GetValue<MenuBool>().Value && W.IsReady())
                {
                    var WTarget = GameObjects.Jungle.FirstOrDefault(x => W.GetPrediction(x).Hitchance >= HitChance.High && Unit.IsValidTarget(x, W.Range));

                    if (WTarget != null)
                        W.Cast(WTarget);
                }
            }
        }

        private void Killsteal()
        {
            if (MenuProvider.MainMenu["Misc"]["UseKillsteal"].GetValue<MenuBool>().Value && R.IsReady())
            {
                var RTarget = GameObjects.EnemyHeroes.FirstOrDefault(x => R.GetPrediction(x).Hitchance >= HitChance.High && x.isKillableAndValidTarget(LeagueSharp.Common.Damage.GetSpellDamage(ObjectManager.Player, x, SpellSlot.R), R.Range));

                if (RTarget != null && !RTarget.InAutoAttackRange())
                    R.Cast(RTarget);
            }
        }

        private void AutoRimmobile()
        {
            if (MenuProvider.MainMenu["Misc"]["AutoRimmobile"].GetValue<MenuBool>().Value && R.IsReady())
            {
                var RTarget = GameObjects.EnemyHeroes.FirstOrDefault(x => R.GetPrediction(x).Hitchance >= HitChance.High && Unit.IsValidTarget(x, R.Range) && x.isImmobileUntil() > x.Distance(ObjectManager.Player.ServerPosition) / R.Speed);

                if (RTarget != null)
                    R.Cast(RTarget);
            }
        }
    }
}
