using GTANetworkAPI;
using System.Collections.Generic;
using System.Linq;

namespace BlaineRP.Server.Game.Estates
{
    public class Garage
    {
        public static Dictionary<uint, Garage> All { get; set; } = new Dictionary<uint, Garage>();

        public enum Types
        {
            Two = 2,
            Six = 6,
            Ten = 10,
        }

        public class Style
        {
            public static Dictionary<Types, Dictionary<byte, Style>> All { get; set; } = new Dictionary<Types, Dictionary<byte, Style>>();

            public Types Type { get; set; }

            public byte Variation { get; set; }

            public List<Utils.Vector4> VehiclePositions { get; set; }

            public Utils.Vector4 EnterPosition { get; set; }

            public int MaxVehicles => VehiclePositions.Count;

            public Style(Types Type, byte Variation, Utils.Vector4 EnterPosition, List<Utils.Vector4> VehiclePositions)
            {
                this.Type = Type;

                this.Variation = Variation;

                this.EnterPosition = EnterPosition;

                this.VehiclePositions = VehiclePositions;

                if (!All.ContainsKey(Type))
                    All.Add(Type, new Dictionary<byte, Style>() { { Variation, this } });
                else
                    All[Type].Add(Variation, this);
            }

            public static void LoadAll()
            {
                new Style(Types.Two, 0, new Utils.Vector4(179.0708f, -1005.729f, -98.99996f, 80.5f), new List<Utils.Vector4>()
                {
                    new Utils.Vector4(171.2562f, -1004.826f, -99.38025f, 182f),
                    new Utils.Vector4(174.7562f, -1004.826f, -99.38025f, 182f),
                });

                new Style(Types.Six, 0, new Utils.Vector4(207.0894f, -998.9854f, -98.99996f, 90f), new List<Utils.Vector4>()
                {
                    new Utils.Vector4(192.987f, -1004.135f, -99.38025f, 182f),
                    new Utils.Vector4(196.487f, -1004.135f, -99.38025f, 182f),

                    new Utils.Vector4(199.987f, -1004.135f, -99.38025f, 182f),
                    new Utils.Vector4(203.487f, -1004.135f, -99.38025f, 182f),

                    new Utils.Vector4(192.987f, -997.135f, -99.38025f, 182f),
                    new Utils.Vector4(196.487f, -997.135f, -99.38025f, 182f),
                });

                new Style(Types.Ten, 0, new Utils.Vector4(238.0103f, -1004.861f, -98.99996f, 78f), new List<Utils.Vector4>()
                {
                    new Utils.Vector4(233.536f, -1001.264f, -99.38025f, 125f),
                    new Utils.Vector4(233.536f, -996.764f, -99.38025f, 125f),
                    new Utils.Vector4(233.536f, -992.264f, -99.38025f, 125f),
                    new Utils.Vector4(233.536f, -987.764f, -99.38025f, 125f),
                    new Utils.Vector4(233.536f, -983.264f, -99.38025f, 125f),

                    new Utils.Vector4(223.536f, -1001.264f, -99.38025f, 250f),
                    new Utils.Vector4(223.536f, -996.764f, -99.38025f, 250f),
                    new Utils.Vector4(223.536f, -992.264f, -99.38025f, 250f),
                    new Utils.Vector4(223.536f, -987.764f, -99.38025f, 250f),
                    new Utils.Vector4(223.536f, -983.264f, -99.38025f, 250f),
                });
            }

            public static Style Get(Types type, byte variation) => All.GetValueOrDefault(type).GetValueOrDefault(variation);
        }

        public class GarageRoot
        {
            private static Dictionary<uint, GarageRoot> All { get; set; } = new Dictionary<uint, GarageRoot>();

            public uint Id { get; }

            public Utils.Vector4 EnterPosition { get; set; }

            public List<Utils.Vector4> VehicleExitPositions { get; set; }

            public Utils.Vector4 EnterPositionVehicle { get; set; }

            private int LastExitUsed { get; set; }

            public GarageRoot(uint Id, Utils.Vector4 EnterPosition, Utils.Vector4 EnterPositionVehicle, List<Utils.Vector4> VehicleExitPositions)
            {
                this.Id = Id;

                this.EnterPosition = EnterPosition;
                this.EnterPositionVehicle = EnterPositionVehicle;
                this.VehicleExitPositions = VehicleExitPositions;

                All.Add(Id, this);
            }

            public static void LoadAll()
            {
                new GarageRoot(1, new Utils.Vector4(1709.854f, 4728.354f, 42.15108f, 101.0983f), new Utils.Vector4(1721.625f, 4711.867f, 42.18731f, 5f), new List<Utils.Vector4>()
                {
                    new Utils.Vector4(1721.733f, 4748.438f, 41.53787f, 92.14223f),
                    new Utils.Vector4(1705.411f, 4747.812f, 41.60215f, 92.28343f),
                    new Utils.Vector4(1697.927f, 4733.728f, 41.69814f, 196.8979f),
                    new Utils.Vector4(1740.28f, 4700.589f, 42.26362f, 89.37487f),
                });

                new GarageRoot(2, new Utils.Vector4(-341.5808f, 6066.215f, 31.48684f, 318.1166f), new Utils.Vector4(-355.4108f, 6067.475f, 31.49911f, 5f), new List<Utils.Vector4>()
                {
                    new Utils.Vector4(-355.8688f, 6085.566f, 31.04557f, 224.6337f),
                    new Utils.Vector4(-358.3138f, 6083.337f, 31.08944f, 226.2532f),
                    new Utils.Vector4(-361.7703f, 6080.033f, 31.10662f, 224.8967f),
                    new Utils.Vector4(-365.6643f, 6076.228f, 31.10655f, 224.6852f),
                    new Utils.Vector4(-369.9357f, 6072.557f, 31.10487f, 224.3345f),
                    new Utils.Vector4(-368.7952f, 6060.91f, 31.12005f, 314.1411f),
                    new Utils.Vector4(-372.1787f, 6064.764f, 31.12002f, 310.8802f),
                });

                new GarageRoot(3, new Utils.Vector4(-1167.71f, -700.1437f, 21.89413f, 295.6788f), new Utils.Vector4(-1204.965f, -715.033f, 21.62106f, 5f), new List<Utils.Vector4>()
                {
                    new Utils.Vector4(-1191.297f, -735.8434f, 20.17742f, 307.4758f),
                    new Utils.Vector4(-1189.372f, -738.7911f, 19.98976f, 307.4758f),
                    new Utils.Vector4(-1186.283f, -742.4839f, 19.73591f, 307.4758f),
                    new Utils.Vector4(-1184.012f, -745.5002f, 19.54056f, 307.4758f),
                });

                new GarageRoot(4, new Utils.Vector4(316.4698f, -685.1124f, 29.48024f, 246.4526f), new Utils.Vector4(322.5291f, -680.5625f, 29.3077f, 5f), new List<Utils.Vector4>()
                {
                    new Utils.Vector4(290.2844f, -695.6075f, 28.91822f, 248.2921f),
                    new Utils.Vector4(292.0151f, -690.6246f, 28.91821f, 248.6051f),
                    new Utils.Vector4(293.4517f, -686.2487f, 28.91976f, 251.8784f),
                    new Utils.Vector4(306.4645f, -701.316f, 28.92953f, 249.9491f),
                    new Utils.Vector4(308.2393f, -696.9249f, 28.94152f, 248.7504f),
                    new Utils.Vector4(310.027f, -692.1644f, 28.96142f, 250.5269f),
                });

                new GarageRoot(5, new Utils.Vector4(926.3544f, -1560.203f, 30.74199f, 91.18516f), new Utils.Vector4(947.8146f, -1570.903f, 30.51659f, 5f), new List<Utils.Vector4>()
                {
                    new Utils.Vector4(922.8512f, -1563.964f, 30.34886f, 89.51781f),
                    new Utils.Vector4(922.3788f, -1556.564f, 30.39615f, 89.63982f),
                    new Utils.Vector4(921.799f, -1548.615f, 30.41576f, 89.55083f),
                    new Utils.Vector4(913.3278f, -1578.475f, 30.29019f, 0.1281081f),
                    new Utils.Vector4(920.2516f, -1577.328f, 30.21041f, 3.114594f),
                    new Utils.Vector4(927.0467f, -1578.27f, 30.06562f, 1.26491f),
                });

                var lines = new List<string>();

                foreach (var x in All.Values)
                {
                    lines.Add($"new GarageRoot({x.Id}, {x.EnterPosition.Position.ToCSharpStr()}, {x.EnterPositionVehicle.ToCSharpStr()});");
                }

                Utils.FillFileToReplaceRegion(System.IO.Directory.GetCurrentDirectory() + Properties.Settings.Static.ClientScriptsTargetPath + @"\Game\Estates\Initialization.cs", "GROOTS_TO_REPLACE", lines);
            }

            public Utils.Vector4 GetNextVehicleExit()
            {
                var nextId = LastExitUsed + 1;

                if (nextId >= VehicleExitPositions.Count)
                    nextId = 0;

                LastExitUsed = nextId;

                return VehicleExitPositions[nextId];
            }

            public bool IsEntityNearEnter(Entity entity) => entity.Dimension == Properties.Settings.Static.MainDimension && entity.Position.DistanceIgnoreZ(EnterPosition.Position) <= Properties.Settings.Static.ENTITY_INTERACTION_MAX_DISTANCE;

            public bool IsEntityNearVehicleEnter(Entity entity) => entity.Dimension == Properties.Settings.Static.MainDimension && entity.Position.DistanceTo(EnterPositionVehicle.Position) <= EnterPositionVehicle.RotationZ + 2.5f;

            public static GarageRoot Get(uint id) => All.GetValueOrDefault(id);
        }

        public PlayerData.PlayerInfo Owner { get; set; }

        public Style StyleData { get; set; }

        public uint Id { get; set; }

        public uint Dimension { get; set; }

        public uint RootId { get; }

        public GarageRoot Root => GarageRoot.Get(RootId);

        public uint Price { get; set; }

        public ClassTypes ClassType { get; set; }

        public uint Tax => GetTax(ClassType);

        public ulong Balance { get; set; }

        public bool IsLocked { get; set; }

        public byte Variation { get; set; }

        public enum ClassTypes
        {
            GA = 0,
            GB,
            GC,
            GD,
        }

        private static Dictionary<ClassTypes, uint> Taxes = new Dictionary<ClassTypes, uint>()
        {
            { ClassTypes.GA, 50 },
            { ClassTypes.GB, 75 },
            { ClassTypes.GC, 90 },
            { ClassTypes.GD, 100 },
        };

        public static uint GetTax(ClassTypes cType) => Taxes.GetValueOrDefault(cType, uint.MinValue);

        public static ClassTypes GetClass(Garage garage)
        {
            if (garage.Price <= 50_000)
                return ClassTypes.GA;

            if (garage.Price <= 150_000)
                return ClassTypes.GB;

            if (garage.Price <= 500_000)
                return ClassTypes.GC;

            return ClassTypes.GD;
        }

        public Garage(uint Id, uint RootId, Types Type, byte Variation, uint Price)
        {
            this.Id = Id;

            this.RootId = RootId;

            this.StyleData = Style.Get(Type, Variation);

            this.Price = Price;

            this.ClassType = GetClass(this);

            this.Dimension = (uint)(Id + Properties.Settings.Profile.Current.Game.GarageDimensionBaseOffset);

            All.Add(Id, this);
        }

        public static void LoadAll()
        {
            GarageRoot.LoadAll();

            new Garage(1, 3, Types.Two, 0, 25_000);

            var lines = new List<string>();

            foreach (var x in All.Values)
            {
                MySQL.LoadGarage(x);

                lines.Add($"new Garage({x.Id}, {x.Root.Id}, {(int)x.StyleData.Type}, {x.Variation}, {(int)x.ClassType}, {x.Tax}, {x.Price});");
            }

            Utils.FillFileToReplaceRegion(System.IO.Directory.GetCurrentDirectory() + Properties.Settings.Static.ClientScriptsTargetPath + @"\Game\Estates\Initialization.cs", "GARAGES_TO_REPLACE", lines);
        }

        public static Garage Get(uint id) => All.GetValueOrDefault(id);

        public bool TryAddMoneyBalance(ulong amount, out ulong newBalance, bool notifyOnFault = true, PlayerData tData = null)
        {
            if (!Balance.TryAdd(amount, out newBalance))
            {
                if (notifyOnFault)
                {

                }

                return false;
            }

            return true;
        }

        public bool TryRemoveMoneyBalance(ulong amount, out ulong newBalance, bool notifyOnFault = true, PlayerData tData = null)
        {
            if (!Balance.TrySubtract(amount, out newBalance))
            {
                if (notifyOnFault)
                {
                    if (tData != null)
                    {
                        tData.Player.Notify("Estate::NEMB", Balance);
                    }
                }

                return false;
            }

            return true;
        }

        public void SetBalance(ulong value, string reason)
        {
            Balance = value;

            MySQL.GarageUpdateBalance(this);
        }

        public void UpdateOwner(PlayerData.PlayerInfo pInfo)
        {
            Owner = pInfo;

            Sync.World.SetSharedData($"Garages::{Id}::OName", pInfo == null ? null : $"{pInfo.Name} {pInfo.Surname} [#{pInfo.CID}]");
        }

        public bool BuyFromGov(PlayerData pData)
        {
            ulong newCash;

            if (!pData.TryRemoveCash(Price, out newCash, true))
                return false;

            if (pData.FreeGaragesSlots <= 0)
            {
                pData.Player.Notify("Trade::MGOW", pData.OwnedGarages.Count);

                return false;
            }

            pData.SetCash(newCash);

            ChangeOwner(pData.Info, true);

            return true;
        }

        public void ChangeOwner(PlayerData.PlayerInfo pInfo, bool buyGov = false)
        {
            if (Owner != null)
            {
                Owner.PlayerData?.RemoveGarageProperty(this);
            }

            if (pInfo != null)
            {
                pInfo.PlayerData?.AddGarageProperty(this);

                var minBalance = Properties.Settings.Static.MIN_PAID_HOURS_HOUSE_APS * (uint)Tax;

                if (buyGov && Balance < minBalance)
                    SetBalance(minBalance, null);
            }

            var vehicles = GetVehiclesInGarage();

            foreach (var x in vehicles)
            {
                x.SetToVehiclePound();
            }

            UpdateOwner(pInfo);

            MySQL.GarageUpdateOwner(this);
        }

        public void SellToGov(bool balancesBack = true, bool govHalfPriceBack = true)
        {
            if (Owner == null)
                return;

            ulong newBalance;

            if (balancesBack)
            {
                var totalMoney = Balance;

                if (totalMoney > 0)
                {
                    if (Owner.BankAccount != null)
                    {
                        if (Owner.BankAccount.TryAddMoneyDebit(totalMoney, out newBalance, true))
                        {
                            Owner.BankAccount.SetDebitBalance(newBalance, null);
                        }
                    }
                    else
                    {
                        if (Owner.TryAddCash(totalMoney, out newBalance, true))
                        {
                            Owner.SetCash(newBalance);
                        }
                    }
                }
            }

            if (govHalfPriceBack)
            {
                var totalMoney = Price / 2;

                if (Owner.BankAccount != null)
                {
                    if (Owner.BankAccount.TryAddMoneyDebit(totalMoney, out newBalance, true))
                    {
                        Owner.BankAccount.SetDebitBalance(newBalance, null);
                    }
                }
                else
                {
                    if (Owner.TryAddCash(totalMoney, out newBalance, true))
                    {
                        Owner.SetCash(newBalance);
                    }
                }
            }

            IsLocked = false;

            SetBalance(0, null);

            ChangeOwner(null, true);
        }

        public void SetVehicleToGarage(VehicleData vData, int slot)
        {
            vData.EngineOn = false;

            var vPos = StyleData.VehiclePositions[slot];

            vData.AttachBoatToTrailer();

            vData.Vehicle.Teleport(vPos.Position, Dimension, vPos.RotationZ, true, Additional.AntiCheat.VehicleTeleportTypes.All);

            vData.SetFreezePosition(vPos.Position, vPos.RotationZ);
            vData.IsInvincible = true;

            SetVehicleToGarageOnlyData(vData.Info, slot);
        }

        public void SetVehicleToGarageOnSpawn(VehicleData vData)
        {
            var vPos = StyleData.VehiclePositions[vData.LastData.GarageSlot];

            vData.Vehicle.Position = vPos.Position;
            vData.Vehicle.SetHeading(vPos.RotationZ);

            vData.IsFrozen = true;
            vData.IsInvincible = true;

            vData.AttachBoatToTrailer();
        }

        public void SetVehicleToGarageOnlyData(VehicleData.VehicleInfo vInfo, int slot)
        {
            var vPos = StyleData.VehiclePositions[slot];

            vInfo.LastData.Position.X = vPos.X;
            vInfo.LastData.Position.Y = vPos.Y;
            vInfo.LastData.Position.Z = vPos.Z;

            vInfo.LastData.Heading = vPos.RotationZ;

            vInfo.LastData.Dimension = Dimension;

            vInfo.LastData.GarageSlot = slot;

            MySQL.VehicleDeletionUpdate(vInfo);
        }

        public List<VehicleData.VehicleInfo> GetVehiclesInGarage()
        {
            if (Owner == null)
                return new List<VehicleData.VehicleInfo>();

            return Owner.OwnedVehicles.Where(x => x.LastData.GarageSlot >= 0 && (x.VehicleData?.Vehicle.Dimension ?? x.LastData.Dimension) == Dimension).ToList();
        }

        public void SetPlayersInside(bool teleport, params Player[] players)
        {
            /*            var vehsInGarage = GetVehiclesInGarage();

                        foreach (var x in vehsInGarage)
                        {
                            if (x.VehicleData == null)
                                continue;

                            var curPos = x.VehicleData.Vehicle.Position;
                            var actualPos = x.VehicleData.FrozenPosition;

                            if (actualPos == null)
                                continue;

                            if (curPos.DistanceTo(actualPos.Position) >= 200f)
                            {
                                x.VehicleData.Vehicle.Teleport(actualPos.Position, null, null, false, Additional.AntiCheat.VehicleTeleportTypes.Default);
                            }
                        }*/

            if (teleport)
            {
                var sData = StyleData;

                Utils.TeleportPlayers(sData.EnterPosition.Position, false, Dimension, sData.EnterPosition.RotationZ, true, players);
            }
            else
            {
                NAPI.ClientEvent.TriggerClientEventToPlayers(players, "Garage::Enter", Id);
            }
        }

        public void SetPlayersOutside(bool teleport, params Player[] players)
        {
            if (teleport)
            {
                var pos = Root.EnterPosition;

                Utils.TeleportPlayers(pos.Position, false, Properties.Settings.Static.MainDimension, pos.RotationZ, true, players);
            }
            else
            {
                NAPI.ClientEvent.TriggerClientEventToPlayers(players, "Garage::Exit");
            }
        }
    }
}
