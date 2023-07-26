using System;
using System.Linq;
using BlaineRP.Client.Extensions.RAGE.Ui;
using BlaineRP.Client.Extensions.System;
using BlaineRP.Client.Game.Fractions;
using BlaineRP.Client.Game.Helpers.Colshapes;
using BlaineRP.Client.Game.Helpers.Colshapes.Types;
using RAGE;
using RAGE.Elements;

namespace BlaineRP.Client.Game.UI.CEF
{
    [Script(int.MaxValue)]
    public class Numberplates
    {
        private static DateTime LastSent;

        public Numberplates()
        {
            Events.Add("Numberplates::Buy",
                async (args) =>
                {
                    var byCash = (bool)args[0];
                    var num = (int)args[1];
                    var signsAmount = (int)args[2];

                    var npcData = NPCs.NPC.GetData(Player.LocalPlayer.GetData<string>("NumberplatesBuy::NpcId"));

                    if (npcData == null)
                        return;

                    if (!LastSent.IsSpam(1000, false, true))
                    {
                        LastSent = World.Core.ServerTime;

                        var res = (string)await npcData.CallRemoteProc("cop_np_buy", $"np_{num}", signsAmount, byCash ? 1 : 0);

                        if (res != null)
                            SetText(res);
                    }
                }
            );
        }

        public static bool IsActive => Browser.IsActive(Browser.IntTypes.VehicleMisc);

        private static int EscBindIdx { get; set; } = -1;

        private static ExtraColshape CloseColshape { get; set; }

        public static async System.Threading.Tasks.Task Show(decimal margin, string npcId)
        {
            await Browser.Render(Browser.IntTypes.VehicleMisc, true, true);

            Browser.Window.ExecuteJs("CarMaint.drawPlates",
                new object[]
                {
                    Police.NumberplatePrices.Select(x => x.Value.Select(y => Math.Floor(y * margin))),
                }
            );

            Cursor.Show(true, true);

            EscBindIdx = Input.Core.Bind(RAGE.Ui.VirtualKeys.Escape, true, () => Close());

            Player.LocalPlayer.SetData("NumberplatesBuy::NpcId", npcId);

            CloseColshape = new Sphere(Player.LocalPlayer.Position, 2.5f, false, Utils.Misc.RedColor, uint.MaxValue, null)
            {
                OnExit = (cancel) =>
                {
                    if (CloseColshape?.Exists == true)
                        Close();
                },
            };
        }

        public static void SetText(string text)
        {
            if (!IsActive)
                return;

            Browser.Window.ExecuteJs("CarMaint.setPlate", text);
        }

        public static void Close()
        {
            if (!IsActive)
                return;

            Input.Core.Unbind(EscBindIdx);

            EscBindIdx = -1;

            Player.LocalPlayer.ResetData("NumberplatesBuy::NpcId");

            CloseColshape?.Destroy();

            CloseColshape = null;

            Browser.Render(Browser.IntTypes.VehicleMisc, false);

            Cursor.Show(false, false);
        }
    }
}