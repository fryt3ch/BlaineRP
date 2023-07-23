using BlaineRP.Client.Extensions.RAGE.Ui;
using BlaineRP.Client.Extensions.System;
using BlaineRP.Client.Utils;
using RAGE;
using RAGE.Elements;
using System;
using System.Linq;

namespace BlaineRP.Client.CEF
{
    [Script(int.MaxValue)]
    public class Numberplates
    {
        public static bool IsActive { get => CEF.Browser.IsActive(Browser.IntTypes.VehicleMisc); }

        private static DateTime LastSent;

        private static int EscBindIdx { get; set; } = -1;

        private static Additional.ExtraColshape CloseColshape { get; set; }

        public Numberplates()
        {
            Events.Add("Numberplates::Buy", async (args) =>
            {
                var byCash = (bool)args[0];
                var num = (int)args[1];
                var signsAmount = (int)args[2];

                var npcData = Data.NPC.GetData(Player.LocalPlayer.GetData<string>("NumberplatesBuy::NpcId"));

                if (npcData == null)
                    return;

                if (!LastSent.IsSpam(1000, false, true))
                {
                    LastSent = Sync.World.ServerTime;

                    var res = (string)await npcData.CallRemoteProc("cop_np_buy", $"np_{num}", signsAmount, byCash ? 1 : 0);

                    if (res != null)
                    {
                        SetText(res);
                    }
                }
            });
        }

        public static async System.Threading.Tasks.Task Show(decimal margin, string npcId)
        {
            await CEF.Browser.Render(Browser.IntTypes.VehicleMisc, true, true);

            CEF.Browser.Window.ExecuteJs("CarMaint.drawPlates", new object[] { Data.Fractions.Police.NumberplatePrices.Select(x => x.Value.Select(y => System.Math.Floor(y * margin))) });

            CEF.Cursor.Show(true, true);

            EscBindIdx = KeyBinds.Bind(RAGE.Ui.VirtualKeys.Escape, true, () => Close());

            Player.LocalPlayer.SetData("NumberplatesBuy::NpcId", npcId);

            CloseColshape = new Additional.Sphere(Player.LocalPlayer.Position, 2.5f, false, Misc.RedColor, uint.MaxValue, null)
            {
                OnExit = (cancel) =>
                {
                    if (CloseColshape?.Exists == true)
                        Close();
                }
            };
        }

        public static void SetText(string text)
        {
            if (!IsActive)
                return;

            CEF.Browser.Window.ExecuteJs("CarMaint.setPlate", text);
        }

        public static void Close()
        {
            if (!IsActive)
                return;

            KeyBinds.Unbind(EscBindIdx);

            EscBindIdx = -1;

            Player.LocalPlayer.ResetData("NumberplatesBuy::NpcId");

            CloseColshape?.Destroy();

            CloseColshape = null;

            CEF.Browser.Render(Browser.IntTypes.VehicleMisc, false);

            CEF.Cursor.Show(false, false);
        }
    }
}
