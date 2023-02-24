using RAGE;
using RAGE.Elements;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BCRPClient.CEF
{
    class Numberplates
    {
        public static bool IsActive { get => CEF.Browser.IsActive(Browser.IntTypes.VehicleMisc); }

        private static DateTime LastSent;

        private static int StoreID { get => Player.LocalPlayer.HasData("CurrentNumberplatesStore") ? Player.LocalPlayer.GetData<int>("CurrentNumberplatesStore") : -1; }

        private static Dictionary<string, int[]> Prices { get; set; } = new Dictionary<string, int[]>()
        {
            {
                "np_0",

                new int[]
                {
                    500_000, 450_000, 400_000, 150_000, 50_000, 15_000, 5_500, 1_500,
                }
            },

            {
                "np_1",

                new int[]
                {
                    500_000, 450_000, 400_000, 150_000, 50_000, 15_000, 5_500, 1_500,
                }
            },

            {
                "np_2",

                new int[]
                {
                    500_000, 450_000, 400_000, 150_000, 50_000, 15_000, 5_500, 1_500,
                }
            },

            {
                "np_3",

                new int[]
                {
                    500_000, 450_000, 400_000, 150_000, 50_000, 15_000, 5_500, 1_500,
                }
            },

            {
                "np_4",

                new int[]
                {
                    500_000, 450_000, 400_000, 150_000, 50_000, 15_000, 5_500, 1_500,
                }
            },
        };

        public Numberplates()
        {
            Events.Add("Numberplates::Buy", (object[] args) =>
            {
                bool byCash = (bool)args[0];
                int num = (int)args[1];
                int signsAmount = (int)args[2];

                if (!LastSent.IsSpam(500, false, false))
                {
                    Events.CallRemote("", $"np_{num}", signsAmount);

                    LastSent = Sync.World.ServerTime;
                }
            });


            Events.Add("Numberplates::Show", async (object[] args) =>
            {
                float margin = (float)args[0];

                await Show(margin);
            });
        }

        public static void RequestShow()
        {
            if (IsActive)
                return;

            if (!Player.LocalPlayer.HasData("CurrentGasStation"))
                return;

            var storeId = StoreID;

            if (storeId < 0)
                return;

            var pData = Sync.Players.GetData(Player.LocalPlayer);

            if (pData == null)
                return;
        }

        public static async System.Threading.Tasks.Task Show(float margin)
        {
            await CEF.Browser.Render(Browser.IntTypes.VehicleMisc, true, true);

            CEF.Browser.Window.ExecuteJs("CarMaint.drawPlates", new object[] { Prices.Select(x => x.Value.Select(y => y * margin)) });
        }

        public static void SetText(string text)
        {
            if (!IsActive)
                return;

            CEF.Browser.Window.ExecuteJs("CarMain.setPlate", text);
        }

        public static void Close()
        {
            if (!IsActive)
                return;

            CEF.Browser.Render(Browser.IntTypes.VehicleMisc, false);
        }
    }
}
