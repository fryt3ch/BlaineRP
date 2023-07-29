using System.Collections.Generic;
using BlaineRP.Server.Game.Attachments;

namespace BlaineRP.Server.Game.Items
{
    public partial class Cigarette
    {
        public static readonly Dictionary<string, Item.ItemData> IdList = new Dictionary<string, Item.ItemData>()
        {
            {
                "cig_0", new ItemData("Сигарета Redwood",
                    new string[]
                    {
                        "prop_cs_ciggy_01",
                        "ng_proc_cigarette01a"
                    },
                    25,
                    15,
                    300000,
                    AttachmentType.ItemCigMouth,
                    20
                )
            },
            {
                "cig_1", new ItemData("Сигарета Chartman",
                    new string[]
                    {
                        "prop_sh_cigar_01",
                        "prop_sh_cigar_01"
                    },
                    25,
                    15,
                    300000,
                    AttachmentType.ItemCig1Mouth,
                    20
                )
            },
            {
                "cig_c_0", new ItemData("Сигара",
                    new string[]
                    {
                        "prop_cigar_02",
                        "prop_cigar_01"
                    },
                    25,
                    15,
                    300000,
                    AttachmentType.ItemCig2Mouth,
                    20
                )
            },
            {
                "cig_j_0", new ItemData("Косяк",
                    new string[]
                    {
                        "p_cs_joint_02",
                        "prop_sh_joint_01"
                    },
                    25,
                    15,
                    300000,
                    AttachmentType.ItemCig3Mouth,
                    20
                )
            },
        };
    }
}