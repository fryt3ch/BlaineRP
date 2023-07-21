using GTANetworkAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace BCRPServer.Game.Casino
{
    public class LuckyWheel
    {
        public static TimeSpan SpinWheelAnimationTime { get; } = TimeSpan.FromMilliseconds(4_500);
        public static TimeSpan SpinWheelTime { get; } = TimeSpan.FromMilliseconds(22_000);

        public enum ZoneTypes : byte
        {
            Cash_0 = 0,
            Vehicle_0 = 1,
            Mystery_0 = 2,
            Clothes_0 = 3,
            Chips_0 = 4,
            Cash_1 = 5,
            Mystery_1 = 6,
            Clothes_1 = 7,
            Mystery_2 = 8,
            Chips_1 = 9,
            Mystery_3 = 10,
            Clothes_2 = 11,
            Chips_3 = 12,
            Cash_2 = 13,
            Mystery_4 = 14,
            Donate_0 = 15,
            Chips_4 = 16,
            Cash_3 = 17,
            Mystery_5 = 18,
            Clothes_3 = 19,
        }

        private static ChancePicker<Items.Gift.Prototype> ChancePicker { get; set; } = new ChancePicker<Items.Gift.Prototype>
        (
            new ChancePicker<Items.Gift.Prototype>.Item<Items.Gift.Prototype>(0.90d, Items.Gift.Prototype.CreateCasino(Items.Gift.Types.CasinoChips, null, 0, 100)),
            new ChancePicker<Items.Gift.Prototype>.Item<Items.Gift.Prototype>(0.10d, Items.Gift.Prototype.CreateCasino(Items.Gift.Types.Money, null, 0, 50_000))
        );

        public static TimeSpan SpinDefaultCooldown { get; } = TimeSpan.FromHours(24);

        public Vector3 Position { get; set; }

        public uint CurrentCID { get; private set; }

        private Timer Timer { get; set; }

        public LuckyWheel(int CasinoId, int Id, float PosX, float PosY, float PosZ)
        {
            this.Position = new Vector3(PosX, PosY, PosZ);
        }

        public bool IsAvailableNow() => Timer == null;

        public void Spin(int casinoId, int wheelId, PlayerData pData)
        {
            if (Timer != null)
                Timer.Dispose();

            var zoneTypesList = Enum.GetValues(typeof(ZoneTypes)).Cast<ZoneTypes>().ToList();

            double chance;

            var itemPrototype = ChancePicker.GetNextItem(out chance);

            List<ZoneTypes> zones = null;

            if (itemPrototype.Type == Items.Gift.Types.Money)
            {
                zones = zoneTypesList.Where(x => x.ToString().StartsWith("Cash")).ToList();
            }
            else if (itemPrototype.Type == Items.Gift.Types.CasinoChips)
            {
                zones = zoneTypesList.Where(x => x.ToString().StartsWith("Chips")).ToList();
            }
            else if (itemPrototype.Type == Items.Gift.Types.Vehicle)
            {
                zones = zoneTypesList.Where(x => x.ToString().StartsWith("Vehicle")).ToList();
            }

            var zoneType = zones[SRandom.NextInt32(0, zones.Count)];

            var spinParam = ((byte)zoneType - 1) * 18;

            var spinOffset = (float)SRandom.NextInt32(spinParam - 4, spinParam + 10);

            CurrentCID = pData.CID;

            pData.PlayAnim(Sync.Animations.FastTypes.FakeAnim, SpinWheelAnimationTime);

            Utils.TriggerEventInDistance(Position, Properties.Settings.Static.MainDimension, 50f, "Casino::LCWS", casinoId, wheelId, pData.Player.Id, (byte)zoneType, spinOffset);

            Timer = new Timer((obj) =>
            {
                NAPI.Task.Run(() =>
                {
                    Timer.Dispose();

                    Timer = null;

                    var pInfo = PlayerData.PlayerInfo.Get(CurrentCID);

                    if (pInfo == null)
                    {
                        return;
                    }

                    Items.Gift.Give(pInfo, itemPrototype, true);

                    CurrentCID = 0;
                });
            }, null, SpinWheelTime, Timeout.InfiniteTimeSpan);
        }
    }
}
