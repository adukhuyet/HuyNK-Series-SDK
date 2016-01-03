using System;
using System.Linq;
using System.Collections.Generic;

using LeagueSharp;
using LeagueSharp.SDK.Core;
using LeagueSharp.SDK.Core.UI.IMenu;
using LeagueSharp.SDK.Core.UI.IMenu.Values;
using LeagueSharp.SDK.Core.Enumerations;
using LeagueSharp.SDK.Core.Wrappers;
using LeagueSharp.SDK.Core.Extensions;
using LeagueSharp.SDK.Core.Utils;
using LeagueSharp.SDK.Core.Extensions.SharpDX;

using Collision = LeagueSharp.SDK.Core.Math.Collision;
using Color = System.Drawing.Color;

using SharpDX;
using LeagueSharp.SDK.Core.Wrappers.Spells;

namespace HuyNK_Series_SDK.Plugins
{
    public class Kalista
    {
        private Spell Q, W, E, R;
        private int LastECastTime;

        public Kalista()
        {
            //Spell
            Q = new Spell(SpellSlot.Q, 1150f) { DamageType = DamageType.Physical };
            W = new Spell(SpellSlot.W, 5200f);
            E = new Spell(SpellSlot.E, 950f);
            R = new Spell(SpellSlot.R, 1500f);

            Q.SetSkillshot(0.25f, 40f, 1200f, true, SkillshotType.SkillshotLine);

            //Menu
            Menu ComboMenu = new Menu("Combo", "Combo");
            Menu HarassMenu = new Menu("Harass", "Harass");
            Menu LaneClearMenu = new Menu("LaneClear", "LaneClear");
            Menu JungleClearMenu = new Menu("JungleClear", "JungleClear");
            Menu MiscMenu = new Menu("Misc", "Misc");
            Menu DrawingsMenu = new Menu("Drawings", "Drawings");

            ComboMenu.Add(new MenuBool("UseQ", "Use Q", true));
            ComboMenu.Add(new MenuBool("UseE", "Use E", true));

            HarassMenu.Add(new MenuBool("UseQ", "Use Q", true));

            LaneClearMenu.Add(new MenuSeparator("Qseparator", "Q"));
            LaneClearMenu.Add(new MenuBool("UseQ", "Use Q", true));
            LaneClearMenu.Add(new MenuSlider("UseQNumber", "Cast Q if killable minion number >=", 4, 1, 7));

            LaneClearMenu.Add(new MenuSeparator("Eseparator", "E"));
            LaneClearMenu.Add(new MenuBool("UseE", "Use E", true));
            LaneClearMenu.Add(new MenuSlider("UseENumber", "Cast E if killable minion number >=", 3, 1, 7));

            JungleClearMenu.Add(new MenuBool("UseQ", "Use Q", true));
            JungleClearMenu.Add(new MenuBool("UseE", "Use E", true));

            MiscMenu.Add(new MenuBool("UseKillsteal", "Use Killsteal", true));
            MiscMenu.Add(new MenuBool("UseMobsteal", "Use Mobsteal", true));

            DrawingsMenu.Add(new MenuSeparator("Qseparator", "Q"));
            DrawingsMenu.Add(new MenuBool("DrawQ", "Draw Q Range", true));
            DrawingsMenu.Add(new MenuColor("QColor", "Color", SharpDX.Color.Blue));

            DrawingsMenu.Add(new MenuSeparator("Wseparator", "W"));
            DrawingsMenu.Add(new MenuBool("DrawW", "Draw W Range"));
            DrawingsMenu.Add(new MenuColor("WColor", "Color", SharpDX.Color.Blue));

            DrawingsMenu.Add(new MenuSeparator("Eseparator", "E"));
            DrawingsMenu.Add(new MenuBool("DrawE", "Draw E Range", true));
            DrawingsMenu.Add(new MenuColor("EColor", "Color", SharpDX.Color.Blue));

            DrawingsMenu.Add(new MenuSeparator("Rseparator", "R"));
            DrawingsMenu.Add(new MenuBool("DrawR", "Draw R Range"));
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
            Spellbook.OnCastSpell += Spellbook_OnCastSpell;
            DamageIndicator.DamageToUnit = GetComboDamage;

            //Loaded
            Logging.Write()(LogLevel.Info, "HuyNK Series SDK: Kalista Loaded!");
        }

        private void Spellbook_OnCastSpell(Spellbook sender, SpellbookCastSpellEventArgs args)
        {
            if (!ObjectManager.Player.IsDead && sender.Owner.IsMe)
                if (args.Slot == SpellSlot.E)
                    if (LastECastTime > Environment.TickCount - 500)
                        args.Process = false;
                    else
                        LastECastTime = Environment.TickCount;
        }

        private void Game_OnGameUpdate(EventArgs args)
        {
            if (!ObjectManager.Player.IsDead)
            {
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
                        JungleClear();
                        break;
                }

                Killsteal();
                Mobsteal();
            }
        }

        private void Game_OnDraw(EventArgs args)
        {
            if (!ObjectManager.Player.IsDead)
            {
                if (MenuProvider.MainMenu["Drawings"]["DrawQ"].GetValue<MenuBool>().Value && Q.isReadyPerfectly())
                    Drawing.DrawCircle(GameObjects.Player.Position, Q.Range, Color.FromArgb(MenuProvider.MainMenu["Drawings"]["QColor"].GetValue<MenuColor>().Color.ToBgra()));

                if (MenuProvider.MainMenu["Drawings"]["DrawW"].GetValue<MenuBool>().Value && W.isReadyPerfectly())
                    Drawing.DrawCircle(GameObjects.Player.Position, W.Range, Color.FromArgb(MenuProvider.MainMenu["Drawings"]["WColor"].GetValue<MenuColor>().Color.ToBgra()));

                if (MenuProvider.MainMenu["Drawings"]["DrawE"].GetValue<MenuBool>().Value && E.isReadyPerfectly())
                    Drawing.DrawCircle(GameObjects.Player.Position, E.Range, Color.FromArgb(MenuProvider.MainMenu["Drawings"]["EColor"].GetValue<MenuColor>().Color.ToBgra()));

                if (MenuProvider.MainMenu["Drawings"]["DrawR"].GetValue<MenuBool>().Value && R.isReadyPerfectly())
                    Drawing.DrawCircle(GameObjects.Player.Position, R.Range, Color.FromArgb(MenuProvider.MainMenu["Drawings"]["RColor"].GetValue<MenuColor>().Color.ToBgra()));
            }
        }

        private float GetComboDamage(Obj_AI_Hero Enemy)
        {
            return 
                (E.isReadyPerfectly() ? (float)LeagueSharp.Common.Damage.GetSpellDamage(ObjectManager.Player, Enemy, SpellSlot.E) : 0);
        }

        private void Combo()
        {
            if (MenuProvider.MainMenu["Combo"]["UseQ"].GetValue<MenuBool>().Value && Q.isReadyPerfectly())
            {
                if (!ObjectManager.Player.IsWindingUp)
                    Q.CastOnBestTarget();
            }

            if (MenuProvider.MainMenu["Combo"]["UseE"].GetValue<MenuBool>().Value && E.isReadyPerfectly())
                if (GameObjects.EnemyHeroes.Any(x => x.isKillableAndValidTarget(LeagueSharp.Common.Damage.GetSpellDamage(ObjectManager.Player, x, SpellSlot.E))))
                    E.Cast();
        }

        private void Harass()
        {
            if (MenuProvider.MainMenu["Harass"]["UseQ"].GetValue<MenuBool>().Value && Q.isReadyPerfectly())
                if (!ObjectManager.Player.IsWindingUp)
                    Q.CastOnBestTarget();
        }

        private void LaneClear()
        {
            if (MenuProvider.MainMenu["LaneClear"]["UseQ"].GetValue<MenuBool>().Value && Q.isReadyPerfectly())
            {
                foreach (var KillableMinion in GameObjects.EnemyMinions.Where(x => x.isKillableAndValidTarget(LeagueSharp.Common.Damage.GetSpellDamage(ObjectManager.Player, x, SpellSlot.Q), Q.Range)))
                {
                    int killableNumber = 0;

                    var CollisionMinions =
                    Collision.GetCollision(new List<Vector3> { ObjectManager.Player.ServerPosition.Extend(KillableMinion.ServerPosition, Q.Range) },
                        new LeagueSharp.SDK.Core.Math.Prediction.PredictionInput
                        {
                            Unit = ObjectManager.Player,
                            Delay = Q.Delay,
                            Speed = Q.Speed,
                            Radius = Q.Width,
                            CollisionObjects = CollisionableObjects.Minions,
                            UseBoundingRadius = false
                        }
                    ).OrderBy(x => x.Distance(ObjectManager.Player));

                    foreach (Obj_AI_Minion CollisionMinion in CollisionMinions)
                    {
                        if (CollisionMinion.isKillableAndValidTarget(LeagueSharp.Common.Damage.GetSpellDamage(ObjectManager.Player, CollisionMinion, SpellSlot.Q), Q.Range))
                            killableNumber++;
                        else
                            break;
                    }

                    if (killableNumber >= MenuProvider.MainMenu["LaneClear"]["UseQNumber"].GetValue<MenuSlider>().Value)
                    {
                        if (!ObjectManager.Player.IsWindingUp)
                        {
                            Q.Cast(KillableMinion.ServerPosition);
                            break;
                        }
                    }
                }
            }

            if (MenuProvider.MainMenu["LaneClear"]["UseE"].GetValue<MenuBool>().Value && E.isReadyPerfectly())
            {
                if (GameObjects.EnemyMinions.Count(x => x.isKillableAndValidTarget(LeagueSharp.Common.Damage.GetSpellDamage(ObjectManager.Player, x, SpellSlot.E))) >= MenuProvider.MainMenu["LaneClear"]["UseENumber"].GetValue<MenuSlider>().Value)
                    E.Cast();
            }
        }

        private void JungleClear()
        {
            if (MenuProvider.MainMenu["JungleClear"]["UseQ"].GetValue<MenuBool>().Value && Q.isReadyPerfectly())
                if (!ObjectManager.Player.IsWindingUp)
                {
                    var QTarget = GameObjects.Jungle.Where(x => x.IsValidTarget(Q.Range)).OrderByDescending(x => x.Health).FirstOrDefault();

                    if (QTarget != null)
                        Q.Cast(QTarget);
                }

            if (MenuProvider.MainMenu["JungleClear"]["UseE"].GetValue<MenuBool>().Value && E.isReadyPerfectly())
                if (GameObjects.Jungle.Any(x => x.isKillableAndValidTarget(LeagueSharp.Common.Damage.GetSpellDamage(ObjectManager.Player, x, SpellSlot.E))))
                    E.Cast();
        }

        private void Killsteal()
        {
            if (MenuProvider.MainMenu["Misc"]["UseKillsteal"].GetValue<MenuBool>().Value && E.isReadyPerfectly())
                if (GameObjects.EnemyHeroes.Any(x => x.isKillableAndValidTarget(LeagueSharp.Common.Damage.GetSpellDamage(ObjectManager.Player, x, SpellSlot.E))))
                    E.Cast();
        }

        private void Mobsteal()
        {
            if (MenuProvider.MainMenu["Misc"]["UseMobsteal"].GetValue<MenuBool>().Value && E.isReadyPerfectly())
                if (GameObjects.Jungle.Any(x => x.isKillableAndValidTarget(LeagueSharp.Common.Damage.GetSpellDamage(ObjectManager.Player, x, SpellSlot.E))) ||
                    GameObjects.EnemyMinions.Any(x => x.isKillableAndValidTarget(LeagueSharp.Common.Damage.GetSpellDamage(ObjectManager.Player, x, SpellSlot.E)) && (x.CharData.BaseSkinName.ToLowerInvariant().Contains("siege") || x.CharData.BaseSkinName.ToLowerInvariant().Contains("super"))))
                    E.Cast();
        }
    }
}