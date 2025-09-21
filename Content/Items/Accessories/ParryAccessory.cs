using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;

namespace ParryEverything.Content.Items.Accessories
{
    public interface IParryAccessory
    {
        /// <summary>
        /// Modifies the parrying config when this accessory is equipped.
        /// </summary>
        void ModifyParryConfig(Player player, ref ParryConfig context);

        /// <summary>
        /// Called when the parry action is initiated (key pressed).
        /// </summary>
        void OnParryStart(Player player, ref ParryConfig context);

        /// <summary>
        /// Called when a successful parry is executed.
        /// </summary>
        void OnParry(Player player, ref ParryConfig context);

        /// <summary>
        /// Called when a parry is initiated, but no attack was present to parry.
        /// </summary>
        void OnNoAttackParry(Player player, ref ParryConfig context);

        /// <summary>
        /// Called when a parry attempt fails (an attack was present, but the timing window was missed).
        /// </summary>
        void OnFailedParry(Player player, ref ParryConfig context);
    }
}
