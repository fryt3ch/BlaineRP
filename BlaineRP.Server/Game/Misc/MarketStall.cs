using GTANetworkAPI;
using System.Collections.Generic;
using BlaineRP.Server.EntitiesData.Players;
using BlaineRP.Server.UtilsT;

namespace BlaineRP.Server.Game.Misc
{
    public partial class MarketStall
    {
        private static MarketStall[] All;

        public Vector4 Position { get; set; }

        public bool IsLocked { get; set; }

        public List<Item> Items { get; private set; }

        public static uint RentPrice { get => World.Service.GetRetrievableData<uint>("MARKETSTALL_RP", 0); set => World.Service.SetRetrievableData("MARKETSTALL_RP", value); }

        public static ushort GetCurrentRenterRID(int stallIdx)
        {
            return Utils.ToUInt16(World.Service.GetSharedData<object>($"MARKETSTALL_{stallIdx}_R") ?? ushort.MaxValue);
        }

        private static void SetCurrentRenterRID(int stallIdx, ushort rid)
        {
            if (rid == ushort.MaxValue)
                World.Service.ResetSharedData($"MARKETSTALL_{stallIdx}_R");
            else
                World.Service.SetSharedData($"MARKETSTALL_{stallIdx}_R", rid);
        }

        public bool IsPlayerNear(Player player, float maxDistance = 7.5f) => player.Dimension == Properties.Settings.Static.MainDimension && player.Position.DistanceTo(Position.Position) <= maxDistance;

        public static MarketStall GetByIdx(int stallIdx) => stallIdx < 0 || stallIdx >= All.Length ? null : All[stallIdx];

        public static MarketStall GetByRenter(ushort rid, out int stallIdx)
        {
            for (int i = 0; i < All.Length; i++)
            {
                if (GetCurrentRenterRID(i) == rid)
                {
                    stallIdx = i;

                    return All[i];
                }
            }

            stallIdx = -1;

            return null;
        }

        public MarketStall(float PosX, float PosY, float PosZ, float RotZ)
        {
            this.Position = new Vector4(PosX, PosY, PosZ, RotZ);
        }

        public bool IsPlayerRenter(int stallIdx, Player player, bool notify, out ushort renterRid)
        {
            renterRid = GetCurrentRenterRID(stallIdx);

            if (renterRid == ushort.MaxValue)
                return false;

            if (renterRid != player.Id)
            {
                if (notify)
                {
                    player.Notify("MarketStall::NO");
                }

                return false;
            }

            return true;
        }

        public bool IsLockedNow(int stallIdx, Player player, bool notify)
        {
            if (IsLocked)
            {
                if (notify)
                {
                    player.Notify("MarketStall::LN");
                }

                return true;
            }

            return false;
        }

        public void SetCurrentRenter(int stallIdx, PlayerData pData)
        {
            if (pData == null)
            {
                IsLocked = false;
                SetItems(null);

                SetCurrentRenterRID(stallIdx, ushort.MaxValue);
            }
            else
            {
                SetCurrentRenterRID(stallIdx, pData.Player.Id);
            }
        }

        public void SetItems(List<Item> items)
        {
            Items = items;
        }
    }
}
