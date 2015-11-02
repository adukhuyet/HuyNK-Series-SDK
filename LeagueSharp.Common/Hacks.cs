﻿using System.Drawing;

namespace LeagueSharp.Common
{
    /// <summary>
    /// Adds hacks to the menu.
    /// </summary>
    internal class Hacks
    {
        /// <summary>
        /// Initializes this instance.
        /// </summary>
        internal static void Initialize()
        {
            CustomEvents.Game.OnGameLoad += eventArgs =>
            {
                var menu = new Menu("Hacks", "Hacks");

                var draw = menu.AddItem(new MenuItem("DrawingHack", "Tắt tât cả các hiển thị").SetValue(false));
                draw.SetValue(LeagueSharp.Hacks.DisableDrawings);
                draw.ValueChanged +=
                    delegate(object sender, OnValueChangeEventArgs args)
                    {
                        LeagueSharp.Hacks.DisableDrawings = args.GetNewValue<bool>();
                    };

                var say = menu.AddItem(new MenuItem("SayHack", "Tắt tất cả L# Chat").SetValue(false)
                    .SetTooltip("Khóa game chat từ các Assemblies"));
                say.SetValue(LeagueSharp.Hacks.DisableSay);
                say.ValueChanged +=
                    delegate(object sender, OnValueChangeEventArgs args)
                    {
                        LeagueSharp.Hacks.DisableSay = args.GetNewValue<bool>();
                    };

                 var zoom = menu.AddItem(new MenuItem("ZoomHack", "Mở rộng zoom").SetValue(false));
                zoom.SetValue(LeagueSharp.Hacks.ZoomHack);
                zoom.ValueChanged +=
                    delegate (object sender, OnValueChangeEventArgs args)
                    {
                        LeagueSharp.Hacks.ZoomHack = args.GetNewValue<bool>();
                    };

               

                var tower = menu.AddItem(new MenuItem("TowerHack", "Hiện tầm trụ ").SetValue(false));
                tower.SetValue(LeagueSharp.Hacks.TowerRanges);
                tower.ValueChanged +=
                    delegate(object sender, OnValueChangeEventArgs args)
                    {
                        LeagueSharp.Hacks.TowerRanges = args.GetNewValue<bool>();
                    };

                CommonMenu.Config.AddSubMenu(menu);
            };
        }
    }
}