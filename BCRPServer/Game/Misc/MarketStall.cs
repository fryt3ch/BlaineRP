using GTANetworkAPI;
using System;
using System.Collections.Generic;

namespace BCRPServer.Game.Misc
{
    public class MarketStall
    {
        private static MarketStall[] All;

        public class Item
        {
            public Game.Items.Item ItemRoot { get; set; }
            public int Amount { get; set; }
            public uint Price { get; set; }

            public Item(Game.Items.Item ItemRoot, int Amount, uint Price)
            {
                this.ItemRoot = ItemRoot;
                this.Amount = Amount;
                this.Price = Price;
            }
        }

        public Utils.Vector4 Position { get; set; }

        public bool IsLocked { get; set; }

        public List<Item> Items { get; private set; }

        public static uint RentPrice { get => Utils.ToUInt32(Sync.World.GetSharedData<object>("MARKETSTALL_RP")); set => Sync.World.SetSharedData("MARKETSTALL_RP", value); }

        public static ushort GetCurrentRenterRID(int stallIdx)
        {
            return Utils.ToUInt16(Sync.World.GetSharedData<object>($"MARKETSTALL_{stallIdx}_R") ?? ushort.MaxValue);
        }

        private static void SetCurrentRenterRID(int stallIdx, ushort rid)
        {
            if (rid == ushort.MaxValue)
                Sync.World.ResetSharedData($"MARKETSTALL_{stallIdx}_R");
            else
                Sync.World.SetSharedData($"MARKETSTALL_{stallIdx}_R", rid);
        }

        public bool IsPlayerNear(Player player, float maxDistance = 7.5f) => player.Dimension == Settings.MAIN_DIMENSION && player.Position.DistanceTo(Position.Position) <= maxDistance;

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
            this.Position = new Utils.Vector4(PosX, PosY, PosZ, RotZ);
        }

        public static void Initialize()
        {
            if (All != null)
                return;

            All = new MarketStall[]
            {
                new MarketStall(-1162.669f, -1715.937f, 3.603473f, -125f + 90f),
                new MarketStall(-1167.805f, -1708.603f, 3.667917f, -125f + 90f),
                new MarketStall(-1172.509f, -1698.171f, 3.7616f, -90f + 90f),
                new MarketStall(-1164.441f, -1692.301f, 3.767002f, -35f + 90f),
                new MarketStall(-1151.815f, -1683.46f, 3.767002f, -35f + 90f),
                new MarketStall(-1140.888f, -1682.408f, 3.767002f, 20f + 90f),
                new MarketStall(-1133.602f, -1689.125f, 3.767002f, 55f + 90f),
                new MarketStall(-1128.458f, -1696.47f, 3.767002f, 55f + 90f),
                new MarketStall(-1138.354f, -1704.733f, 3.767002f, 145f + 90f),
                new MarketStall(-1150.549f, -1713.272f, 3.611554f, 145f + 90f),
                new MarketStall(-1161.594f, -1706.65f, 3.702462f, 55f + 90f),
                new MarketStall(-1155.441f, -1694.278f, 3.767002f, 155f + 90f),
                new MarketStall(-1149.936f, -1699.669f, 3.723997f, -35f + 90f),
                new MarketStall(-1138.576f, -1692.609f, 3.786295f, -125f + 90f),
            };

            var lines = new List<string>();

            for (int i =  0; i < All.Length; i++)
            {
                var x = All[i];

                lines.Add($"new MarketStall({i}, {x.Position.ToCSharpStr()});");
            }

            Utils.FillFileToReplaceRegion(Settings.DIR_CLIENT_LOCATIONS_DATA_PATH, "MARKETSTALLS_TO_REPLACE", lines);
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
