using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using BlaineRP.Server.EntityData.Players;
using GTANetworkAPI;

namespace BlaineRP.Server.Game.CasinoSystem.Games
{
    public partial class LuckyWheel
    {
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

                    var pInfo = PlayerInfo.Get(CurrentCID);

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
