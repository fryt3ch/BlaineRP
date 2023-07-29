using System.Collections.Generic;
using BlaineRP.Server.Game.Attachments;

namespace BlaineRP.Server.Game.Items
{
    public partial class CigarettesPack
    {
        public static readonly Dictionary<string, Item.ItemData> IdList = new Dictionary<string, Item.ItemData>()
        {
            {
                "cigs_0", new ItemData("Сигареты Redwood",
                    new string[]
                    {
                        "v_ret_ml_cigs",
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
                "cigs_1", new ItemData("Сигареты Chartman",
                    new string[]
                    {
                        "prop_cigar_pack_01",
                        "prop_sh_cigar_01"
                    },
                    25,
                    15,
                    300000,
                    AttachmentType.ItemCig1Mouth,
                    20
                )
            },
        };
    }
}