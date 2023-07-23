using System.Collections.Generic;

namespace BlaineRP.Client.Data
{
    public partial class Locations
    {
        public class FishBuyer
        {
            public static List<FishBuyer> All { get; set; } = new List<FishBuyer>();

            public static Dictionary<string, uint> BasePrices { get; set; }

            public int Id => All.IndexOf(this);

            public decimal CurrentPriceCoef => decimal.Parse(Sync.World.GetSharedData<string>($"FishBuyer::{Id}::C"));

            public uint GetPrice(string fishId) => (uint)System.Math.Floor(BasePrices.GetValueOrDefault(fishId) * CurrentPriceCoef);

            public FishBuyer(Utils.Vector4 Position)
            {
                All.Add(this);

                var id = Id;

                var blip = new Additional.ExtraBlip(762, Position.Position, "Скупщик рыбы", 1f, 3, 255, 0f, true, 0, 0f, Settings.App.Static.MainDimension);

                if (id == 0)
                {
                    var npc = new Data.NPC("fishbuyer_0", "Остин", NPC.Types.Talkable, "a_m_o_genstreet_01", Position.Position, Position.RotationZ, Settings.App.Static.MainDimension)
                    {
                        SubName = "NPC_SUBNAME_FISH_BUYER",

                        Data = this,

                        DefaultDialogueId = "fishbuyer_0_g",
                    };
                }

                Sync.World.AddDataHandler($"FishBuyer::{id}::C", OnCurrentPriceCoefChange);
            }

            private static void OnCurrentPriceCoefChange(string key, object value, object oldValue)
            {
                var fbId = int.Parse(key.Split("::")[1]);

                var fb = All[fbId];

                var npc = NPC.CurrentNPC;

                if (npc?.Data == fb)
                {
                    if (CEF.ActionBox.CurrentContextStr == "FishBuyerRange")
                    {
                        CEF.ActionBox.Close(true);
                    }

                    npc.SwitchDialogue(false);

                    CEF.Notification.Show(CEF.Notification.Types.Information, Locale.Get("NOTIFICATION_HEADER_DEF"), "Расценки у этого скупщика рыбы изменились, перейдите в диалог с ним еще раз!");
                }
            }
        }
    }
}