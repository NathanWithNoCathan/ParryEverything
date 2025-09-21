using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace ParryEverything.Content.Items.Accessories
{
    public class ParryCharm : ModItem, IParryAccessory
    {
        public override void SetDefaults()
        {
            Item.width = 20;
            Item.height = 20;
            Item.accessory = true;
            Item.value = Item.buyPrice(gold: 1);
            Item.rare = ItemRarityID.Blue;
        }

        static int ParticleCount = 20;
        List<Dust> Dusts = new();

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            player.GetModPlayer<ParryPlayer>().RegisterAccessory(this);
            
            if (Dusts.Count != ParticleCount) return;

            ulong LastParryTick = player.GetModPlayer<ParryPlayer>().LastParryTick;
            int TimeSinceLastParry = (int)(Main.GameUpdateCount - LastParryTick);
            if (TimeSinceLastParry < ParryWindowOnStart)
            {
                Main.NewText($"Time since parry {TimeSinceLastParry}");

                for (int i = 0; i < ParticleCount; i++)
                {
                    double x = player.Center.X + 10 * (ParryWindowOnStart - TimeSinceLastParry) * Math.Cos(i * 2 * Math.PI / ParticleCount);
                    double y = player.Center.Y + 10 * (ParryWindowOnStart - TimeSinceLastParry) * Math.Sin(i * 2 * Math.PI / ParticleCount);

                    Dusts[i].position = new Vector2((float)x, (float)y);
                }
            }
        }
        public void ModifyParryConfig(Player player, ref ParryConfig context)
        {
            context.ParryEnabled = true;
        }

        private int ParryWindowOnStart = 3;
        public void OnParryStart(Player player, ref ParryConfig context) 
        {
            ParryWindowOnStart = context.ParryWindowTicks;
            Dusts.Clear();

            for (int i = 0; i < ParticleCount; i++)
            {
                double x = player.Center.X + 10 * ParryWindowOnStart * Math.Cos(i * 2 * Math.PI / ParticleCount);
                double y = player.Center.Y + 10 * ParryWindowOnStart * Math.Sin(i * 2 * Math.PI / ParticleCount);

                //Create particles around player
                Dusts.Add(Dust.NewDustPerfect(new Vector2((float)x, (float)y), DustID.GoldFlame, new Vector2(0, 0), 150, Color.Yellow, 1.5f));
            }
        }
        public void OnParry(Player player, ref ParryConfig context)
        {
            SoundEngine.PlaySound(SoundID.Item4, player.position);
        }
        public void OnNoAttackParry(Player player, ref ParryConfig context) { }
        public void OnFailedParry(Player player, ref ParryConfig context) 
        {
            Main.NewText("Parry Failed!", Color.Red);
        }
    }
}
