using Terraria;
using Terraria.Audio;
using Terraria.ModLoader;
using Terraria.ID;
using Terraria.GameInput;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;
using ParryEverything.Content.Items.Accessories;
using Microsoft.Xna.Framework;

namespace ParryEverything.Content
{
    /// <summary>
    /// Context information about parrying stats; includes things like parry timing windows, i-frame durations, etc.
    /// </summary>
    public class ParryConfig
    {
        /// <summary>
        /// The amount of time (in ticks) after initiating a parry during which an incoming attack can be parried.
        /// </summary>
        public int ParryWindowTicks;
        /// <summary>
        /// The time period after the parry window finishes during which the player cannot parry again. If an attack is incurred within this window, it will be considered a failed parry. If no attack is incurred within this window, it will be considered a "no attack" failed parry. If a parry was successful, this cooldown does not apply.
        /// </summary>
        public int ParryCooldownTicks;
        /// <summary>
        /// The number of ticks of invincibility granted upon a successful parry.
        /// </summary>
        public int ParryImmunityTicks;
        /// <summary>
        /// Whether the player is currently able to parry, based on accessory toggles or other conditions.
        /// </summary>
        public bool ParryEnabled;

        public static ParryConfig Default => new ParryConfig
        {
            ParryWindowTicks = 3, // ~50 ms
            ParryCooldownTicks = 20, // ~333 ms
            ParryImmunityTicks = 30, // ~500 ms
            ParryEnabled = false
        };
    }

    public class ParryKeybind : ModSystem
    {
        public static ModKeybind Keybind;

        public override void Load()
        {
            Keybind = KeybindLoader.RegisterKeybind(Mod, "Parry", Keys.F);
        }

        public override void Unload()
        {
            Keybind = null;
        }
    }

    public class ParryPlayer : ModPlayer
    {
        /// <summary>
        /// Ticks since world start; Main.GameUpdateCount is a ulong
        /// </summary>
        public ulong LastParryTick = 0;

        /// <summary>
        /// The list of accessories that modify the parry config
        /// </summary>
        private List<IParryAccessory> Accessories = new();

        public void RegisterAccessory(IParryAccessory acc)
        {
            if (!Accessories.Contains(acc))
                Accessories.Add(acc);
        }

        private ParryConfig CurrentConfig = ParryConfig.Default;

        public override void PreUpdate()
        {
            Accessories.Clear();
        }

        private void UpdateParryConfig()
        {
            // Reset to default
            CurrentConfig = ParryConfig.Default;

            // Let accessories modify the config
            foreach (var acc in Accessories)
                acc.ModifyParryConfig(Player, ref CurrentConfig);
        }

        bool Parried = false;
        public override void PostUpdateEquips()
        {
            if (Parried)
            {
                LastParryTick = Main.GameUpdateCount;

                foreach (var acc in Accessories)
                    acc.OnParryStart(Player, ref CurrentConfig);
                Parried = false;
            }
        }

        public override void ProcessTriggers(TriggersSet triggersSet)
        {
            UpdateParryConfig();

            //Ignore if parry is disabled
            if (!CurrentConfig.ParryEnabled) 
                return;

            //Shouldn't be null here, but just in case
            if (ParryKeybind.Keybind != null && ParryKeybind.Keybind.JustPressed)
                Parried = true;
        }

        // Called before damage is applied; return true to completely dodge the hit (like Master Ninja Gear)
        public override bool FreeDodge(Player.HurtInfo info)
        {
            UpdateParryConfig();

            // Ignore if parry is disabled
            if (!CurrentConfig.ParryEnabled) 
                return false;
            
            ulong now = Main.GameUpdateCount;
            int ticksSinceParry = (int)(now - LastParryTick);
            Main.NewText($"Ticks since parry {ticksSinceParry}\n" +
                $"Parry Window + Parry Cooldown {CurrentConfig.ParryWindowTicks + CurrentConfig.ParryCooldownTicks}\n" +
                $"Parry Window {CurrentConfig.ParryWindowTicks}", Color.Yellow);

            if (ticksSinceParry > CurrentConfig.ParryWindowTicks + CurrentConfig.ParryCooldownTicks)
            {
                // Parry window and cooldown are over; this is a "no attack" failed parry
                foreach (var acc in Accessories)
                    acc.OnNoAttackParry(Player, ref CurrentConfig);
                return false;
            }
            else if (ticksSinceParry > CurrentConfig.ParryWindowTicks)
            {
                // Parry window is over, but still in cooldown; this is a failed parry
                foreach (var acc in Accessories)
                    acc.OnFailedParry(Player, ref CurrentConfig);
                return false;
            }
            else
            {
                TryGiveIFrames(CurrentConfig.ParryImmunityTicks);

                foreach (var acc in Accessories)
                    acc.OnParry(Player, ref CurrentConfig);

                return true; // Dodge the hit
            }
        }

        private void TryGiveIFrames(int ticks)
        {
            Player.SetImmuneTimeForAllTypes(ticks);
        }
    }
}
