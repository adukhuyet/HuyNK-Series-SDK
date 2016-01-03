using System;

using LeagueSharp;
using LeagueSharp.SDK.Core;
using LeagueSharp.SDK.Core.Utils;
using LeagueSharp.SDK.Core.Enumerations;
using LeagueSharp.SDK.Core.Wrappers.Orbwalking;
using LeagueSharp.SDK.Core.Wrappers.TargetSelector;

namespace HuyNK_Series_SDK
{
    class OrbwalkerTargetIndicator
    {
        public static void initialize()
        {
            Drawing.OnDraw += Drawing_OnDraw;

            Logging.Write()(LogLevel.Info, "HuyNK Series SDK: OrbwalkerTargetIndicator initialized.");
        }

        private static void Drawing_OnDraw(EventArgs args)
        {
            
        }
    }
}