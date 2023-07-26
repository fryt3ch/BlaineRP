using System.Collections.Generic;
using BlaineRP.Client.Game.Helpers.Blips;
using BlaineRP.Client.Game.UI.CEF;

namespace BlaineRP.Client.Game.Misc
{
    public partial class FishBuyer
    {
        public FishBuyer(Utils.Vector4 Position)
        {
            All.Add(this);

            int id = Id;

            var blip = new ExtraBlip(762, Position.Position, "Скупщик рыбы", 1f, 3, 255, 0f, true, 0, 0f, Settings.App.Static.MainDimension);

            if (id == 0)
            {
                var npc = new NPCs.NPC("fishbuyer_0",
                    "Остин",
                    NPCs.NPC.Types.Talkable,
                    "a_m_o_genstreet_01",
                    Position.Position,
                    Position.RotationZ,
                    Settings.App.Static.MainDimension
                )
                {
                    SubName = "NPC_SUBNAME_FISH_BUYER",
                    Data = this,
                    DefaultDialogueId = "fishbuyer_0_g",
                };
            }

            World.Core.AddDataHandler($"FishBuyer::{id}::C", OnCurrentPriceCoefChange);
        }

        public static List<FishBuyer> All { get; set; } = new List<FishBuyer>();

        public static Dictionary<string, uint> BasePrices => Settings.App.Static.GetOther<Dictionary<string, uint>>("fishBuyersBasePrices");

        public int Id => All.IndexOf(this);

        public decimal CurrentPriceCoef => decimal.Parse(World.Core.GetSharedData<string>($"FishBuyer::{Id}::C"));

        public uint GetPrice(string fishId)
        {
            return (uint)System.Math.Floor(BasePrices.GetValueOrDefault(fishId) * CurrentPriceCoef);
        }

        private static void OnCurrentPriceCoefChange(string key, object value, object oldValue)
        {
            var fbId = int.Parse(key.Split("::")[1]);

            FishBuyer fb = All[fbId];

            NPCs.NPC npc = NPCs.NPC.CurrentNPC;

            if (npc?.Data == fb)
            {
                if (ActionBox.CurrentContextStr == "FishBuyerRange")
                    ActionBox.Close(true);

                npc.SwitchDialogue(false);

                Notification.Show(Notification.Types.Information,
                    Locale.Get("NOTIFICATION_HEADER_DEF"),
                    "Расценки у этого скупщика рыбы изменились, перейдите в диалог с ним еще раз!"
                );
            }
        }
    }
}