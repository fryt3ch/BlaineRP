using System;
using System.Collections.Generic;
using BlaineRP.Server.Game.Animations;
using BlaineRP.Server.Game.Attachments;

namespace BlaineRP.Server.Game.Items
{
    public partial class Healing
    {
        public static readonly Dictionary<string, Item.ItemData> IdList = new Dictionary<string, Item.ItemData>()
        {
            {
                "med_b_0",
                new ItemData("Бинт", 0.1f, "prop_gaffer_arm_bind", 10, true, -1.0d, 64, TimeSpan.FromMilliseconds(4_000), FastType.ItemBandage, AttachmentType.ItemBandage)
            },

            {
                "med_kit_0", new ItemData("Аптечка",
                    0.25f,
                    "prop_ld_health_pack",
                    50,
                    false,
                    0.50d,
                    3,
                    TimeSpan.FromMilliseconds(7_000),
                    FastType.ItemMedKit,
                    AttachmentType.ItemMedKit
                )
                {
                    ResurrectionTime = TimeSpan.FromSeconds(10),
                }
            },
            {
                "med_kit_ems_0", new ItemData("Аптечка EMS",
                    0.25f,
                    "prop_ld_health_pack",
                    Properties.Settings.Static.PlayerMaxHealth,
                    true,
                    0.75d,
                    64,
                    TimeSpan.FromMilliseconds(7_000),
                    FastType.ItemMedKit,
                    AttachmentType.ItemMedKit
                )
                {
                    ResurrectionTime = TimeSpan.FromSeconds(8),
                }
            },
        };
    }
}