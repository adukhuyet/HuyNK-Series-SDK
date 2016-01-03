using System;
using System.Linq;
using System.Collections.Generic;
using System.Windows.Forms;
using LeagueSharp;
using LeagueSharp.Common;

using LeagueSharp.SDK.Core;
using LeagueSharp.SDK.Core.UI.IMenu.Values;
using LeagueSharp.SDK.Core.Enumerations;
using LeagueSharp.SDK.Core.Extensions;
using LeagueSharp.SDK.Core.Utils;
using LeagueSharp.SDK.Core.Extensions.SharpDX;

using Collision = LeagueSharp.SDK.Core.Math.Collision;
using Color = System.Drawing.Color;

using SharpDX;
using HitChance = LeagueSharp.SDK.Core.Enumerations.HitChance;
using KeyBindType = LeagueSharp.SDK.Core.Enumerations.KeyBindType;
using Menu = LeagueSharp.SDK.Core.UI.IMenu.Menu;
using LeagueSharp.SDK.Core.Wrappers.Orbwalking;


namespace HuyNK_Series_SDK.Plugins
{
    class Jinx
    {
       
        private Spell Q, W, E, R,_r2;
        private int LastECastTime;
        public Jinx()
        {

            Q = new Spell(SpellSlot.Q, 725);
         

            W = new Spell(SpellSlot.W, 1500f);
            W.SetSkillshot(0.6f, 60f, 3300f, true, LeagueSharp.Common.SkillshotType.SkillshotLine);

            E = new Spell(SpellSlot.E, 900f);
            E.SetSkillshot(0.7f, 120f, 1750f, false, LeagueSharp.Common.SkillshotType.SkillshotCircle);

            R = new Spell(SpellSlot.R, 20000);
            R.SetSkillshot(0.6f, 140f, 1700f, false, LeagueSharp.Common.SkillshotType.SkillshotLine);

            _r2 = new Spell(SpellSlot.R, 20000);
            _r2.SetSkillshot(0.6f, 140f, 1700f, false, LeagueSharp.Common.SkillshotType.SkillshotLine);
            //Menu
       
            
           
           
           //Set key
           
            Menu Skill = new Menu("Keys", "Cài đặt kill");
            Skill.Add(new MenuSlider("Q_Max_Range", "Q khi đủ tầm", 650, 500, 1200));
            Skill.Add(new MenuSlider("W_Max_Range", "W khi đủ tầm", 900, 500, 1500));
            Skill.Add(new MenuSlider("R_Min_Range", "Tầm đánh R nhỏ nhât", 300, 300, 1200));
            Skill.Add(new MenuSlider("R_Max_Range", "Tầm đánh R lớn nhất", 20000, 500, 20000));
            Skill.Add(new MenuKeyBind("Combo_WE", "Combo WE", System.Windows.Forms.Keys.E, KeyBindType.Press, "E"));
            
            // Combo
            Menu ComboMenu = new Menu("Combo", "Combo");
            ComboMenu.Add(new MenuBool("UseQ","Dùng Q",true));
            ComboMenu.Add(new MenuBool("UseW", "Dùng W", true));
            ComboMenu.Add(new MenuBool("UseE", "Dùng E", true));
            ComboMenu.Add(new MenuBool("UseR", "Dùng R", true));
            ComboMenu.Add(new MenuBool("R_Nearest_Killable", "Giết khi gần Chết"));
            
            //Cấu rỉa
            Menu HarassMenu = new Menu("Harass", "Cấu Rỉa");
            HarassMenu.Add(new MenuBool("UseQ", "Dùng Q", true));
            HarassMenu.Add(new MenuBool("UseW", "Dùng W", true));
            //Đẩy đường
            Menu LaneClearMenu = new Menu("LaneClear", "Đẩy Đường");
           
            LaneClearMenu.Add(new MenuBool("UseQ", "Use Q", true));
            


            //Dọn rừng
            Menu JungleClearMenu = new Menu("JungleClear", "Dọn Rừng");
            JungleClearMenu.Add(new MenuBool("UseQ", "Use Q", true));
           
            //Tiện ích
            Menu MiscMenu = new Menu("Misc", "Tiện ích");
            MiscMenu.Add(new MenuBool("UseKillsteal", "Dùng R KS", true));
            MiscMenu.Add(new MenuBool("Misc_Use_WE", "Combo E + W", true));
            //Hiển thị
            Menu DrawingsMenu = new Menu("Drawings", "Hiển Thị");
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
            DrawingsMenu.Add(new MenuBool("Draw_R_Killable", "Tìm Muc tiêu ULti"));
            
         
            DrawingsMenu.Add(new MenuSeparator("Dseparator", "DamageIndicator"));

            var UseDamageIndicator = new MenuBool("UseDamageIndicator", "Use DamageIndicator", true);
            var DamageIndicatorFillColor = new MenuColor("DamageIndicatorFillColor", "Color", SharpDX.Color.Goldenrod);

            UseDamageIndicator.ValueChanged += (object sender, EventArgs e) => { DamageIndicator.Enabled = UseDamageIndicator.Value; };
            DamageIndicatorFillColor.ValueChanged += (object sender, EventArgs e) => { DamageIndicator.FillColor = Color.FromArgb(DamageIndicatorFillColor.Color.ToBgra()); };

            DrawingsMenu.Add(UseDamageIndicator);
            DrawingsMenu.Add(DamageIndicatorFillColor);

            MenuProvider.MainMenu.Add(ComboMenu);
            MenuProvider.MainMenu.Add(Skill);
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
            Logging.Write()(LogLevel.Info, "HuyNK Series SDK: Jinx Loaded!");

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
                //check if player is dead
                if (ObjectManager.Player.IsDead) return;


              
            
            //adjust range
                if (Q.IsReady())
                    Q.Range = _Getmenu.get_slider("Keys", "Q_Max_Range");
                if (W.IsReady())
                    W.Range = _Getmenu.get_slider("Keys", "W_Max_Range");
                if (R.IsReady())
                    R.Range = _Getmenu.get_slider("Keys", "R_Max_Range");
           
             
                Killsteal();
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
                     // JungleClear();
                        break;
                  

                       
                }

                if (_Getmenu.get_bool("Keys", "Combo_WE"))
                {
                    CastWE();
                }
            }
          
        }

        private void Game_OnDraw(EventArgs args)
        {
            if (!ObjectManager.Player.IsDead)
            {
            	if (_Getmenu.get_bool("Drawings","DrawQ") && Q.IsReady())
                    Drawing.DrawCircle(GameObjects.Player.Position, Q.Range, Color.FromArgb(MenuProvider.MainMenu["Drawings"]["QColor"].GetValue<MenuColor>().Color.ToBgra()));

            	if (_Getmenu.get_bool("Drawings","DrawW") && W.IsReady())
                    Drawing.DrawCircle(GameObjects.Player.Position, W.Range, Color.FromArgb(MenuProvider.MainMenu["Drawings"]["WColor"].GetValue<MenuColor>().Color.ToBgra()));

            	if (_Getmenu.get_bool("Drawings","DrawE") && E.IsReady())
                    Drawing.DrawCircle(GameObjects.Player.Position, E.Range, Color.FromArgb(MenuProvider.MainMenu["Drawings"]["EColor"].GetValue<MenuColor>().Color.ToBgra()));

            	if (_Getmenu.get_bool("Drawings","DrawR") && R.IsReady())
                    Drawing.DrawCircle(GameObjects.Player.Position, R.Range, Color.FromArgb(MenuProvider.MainMenu["Drawings"]["RColor"].GetValue<MenuColor>().Color.ToBgra()));
            }
        }

        private float GetComboDamage(Obj_AI_Hero Enemy)
        {
            return
               (Q.IsReady() ? (float)LeagueSharp.Common.Damage.GetSpellDamage(ObjectManager.Player, Enemy, SpellSlot.Q) : 0)+
         (W.IsReady() ? (float)LeagueSharp.Common.Damage.GetSpellDamage(ObjectManager.Player, Enemy, SpellSlot.W) : 0)+
             (R.IsReady() ? (float)LeagueSharp.Common.Damage.GetSpellDamage(ObjectManager.Player, Enemy, SpellSlot.R) : 0); ;
        }

        private void Combo()
        {
            
             Cast_Q();
            if (_Getmenu.get_bool("Combo", "UseW") && W.IsReady())

                W.CastOnBestTarget();

            if (_Getmenu.get_bool("Combo", "UseE") && E.IsReady())
                E.CastOnBestTarget();


        }

        private void Harass()
        {
        
                Cast_Q();

            if (_Getmenu.get_bool("Harass", "UseW") && W.IsReady())

                W.CastOnBestTarget();
            
            if (_Getmenu.get_bool("Harass", "UseE") && E.IsReady())
            
                E.CastOnBestTarget(E.Width, true);
            
        }

        public  void LaneClear()
        {
            var useQ = _Getmenu.get_bool("LaneClear","UseQ");
            
            if (useQ )
            {
                var orbT = Variables.Orbwalker.GetTarget();
                if (orbT.IsValid<Obj_AI_Minion>() && CountMinionsInRange(250, orbT.Position) >= 5)
                {
                    Q.Cast();
                }
            }
        }

        private static int CountMinionsInRange(float range, Vector3 pos)
        {
            var minions = MinionManager.GetMinions(pos, range);
            int count = 0;
            foreach (var minion in minions)
            {
                count++;
            }
            return count;
        }
        private void JungleClear()
        {
            var useQ = _Getmenu.get_bool("LaneClear", "UseQ");

            if (useQ)
            {
                Cast_Q();
            }
        }

        private void Killsteal()
        {
            foreach (
              var unit in
                  ObjectManager.Get<Obj_AI_Hero>()
                      .Where(x => x.IsValid && !x.IsDead && x.IsEnemy)
                      .OrderBy(x => x.Health))
            {
                var health = unit.Health + unit.HPRegenRate * 3 + 25;
              
                if (Get_R_Dmg(unit) > health)
                {
                    Drawing.DrawText(Drawing.Width * 0.39f, Drawing.Height * 0.80f, Color.DeepPink,
                    "[[TUONG DICH DANG YEU ULTI DE GIET NGAY ]] ");
                    Vector2 wts = Drawing.WorldToScreen(unit.Position);

                    Drawing.DrawText(wts[0] - 20, wts[1], Color.Yellow, "  [[[(+KILL+)]]]");

                    R.Cast(unit);
                }
            }
         
            	if (_Getmenu.get_bool("Misc","UseKillsteal") && R.IsReady())
                {
                    
                   

                    foreach (
                var unit in
                    ObjectManager.Get<Obj_AI_Hero>()
                        .Where(x => x.IsTargetable && !x.IsDead && x.IsEnemy)
                        .OrderBy(x => x.Health))
                    {
                        
                                var health = unit.Health + unit.HPRegenRate * 3 + 25;
                                if (Get_R_Dmg(unit) + 100 > health )
                                {
                                    R.Cast(unit);
                                    return;
                                }
                         
                    }
                }
        }

        private float Get_R_Dmg(Obj_AI_Hero target)
        {
            double dmg = 0;

            dmg += ObjectManager.Player.GetSpellDamage(target, SpellSlot.R);

            var rPred = _r2.GetPrediction(target);
            var collisionCount = rPred.CollisionObjects.Count;

            if (collisionCount >= 7)
                dmg = dmg * .3;
            else if (collisionCount != 0)
                dmg = dmg * ((10 - collisionCount) / 10);

          
            return (float)dmg;
        }
        private void CastWE()
        {
            
            if (W.IsReady() && E.IsReady())
            {
                var vec = ObjectManager.Player.ServerPosition + Vector3.Normalize(Game.CursorPos - ObjectManager.Player.ServerPosition) * E.Range;

                W.Cast(vec);
                E.Cast(vec);
            }
        }
        private bool IsCannon()
        {

            return ObjectManager.Player.AttackRange > 525;
        }
        private void Cast_Q()
        {
        	var dungQ = _Getmenu.get_bool("Combo","UseQ");
            if (!Q.IsReady()) return;
            if (dungQ)
            {
                Obj_AI_Hero target = LeagueSharp.Common.TargetSelector.GetTarget(Q.Range, LeagueSharp.Common.TargetSelector.DamageType.Physical);

                if (target == null)
                    return;

                float distance = ObjectManager.Player.Position.Distance(target.Position);

                if (IsCannon())
                {
                    if (distance <= 600 || distance >1500)
                        Q.Cast();
                }
                else
                {
                    if (distance > 600 || distance < 1500)
                        Q.Cast();
                }
            }
        }
        private void Cast_E()
        {
            var target = LeagueSharp.Common.TargetSelector.GetTarget(E.Range, LeagueSharp.Common.TargetSelector.DamageType.Magical);

            if (E.IsReady() && target != null)
            {
                 
                       
                       
                            E.CastOnBestTarget(W.Width, true);
                  
                
            }
        }
      


    }
}
