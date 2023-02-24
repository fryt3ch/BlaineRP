using GTANetworkAPI;
using System.Collections.Generic;
using System.Linq;

namespace BCRPServer.Game.Estates
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
            private static Dictionary<Types, GarageRoot> All { get; set; } = new Dictionary<Types, GarageRoot>();

            public enum Types
            {
                Complex1 = 0,
            }

            public Types Type { get; set; }

            public Utils.Vector4 EnterPosition { get; set; }

            public List<Utils.Vector4> VehicleExitPositions { get; set; }

            public Vector3 EnterPositionVehicle { get; set; }

            private int LastExitUsed { get; set; }

            public GarageRoot(Types Type, Utils.Vector4 EnterPosition, Vector3 EnterPositionVehicle, List<Utils.Vector4> VehicleExitPositions)
            {
                this.Type = Type;

                this.EnterPosition = EnterPosition;
                this.EnterPositionVehicle = EnterPositionVehicle;
                this.VehicleExitPositions = VehicleExitPositions;

                All.Add(Type, this);
            }

            public static void LoadAll()
            {
                new GarageRoot(Types.Complex1, new Utils.Vector4(-1167.71f, -700.1437f, 21.89413f, 295.6788f), new Vector3(-1204.965f, -715.033f, 21.62106f), new List<Utils.Vector4>()
                {
                    new Utils.Vector4(-1191.297f, -735.8434f, 20.17742f, 307.4758f),
                    new Utils.Vector4(-1189.372f, -738.7911f, 19.98976f, 307.4758f),
                    new Utils.Vector4(-1186.283f, -742.4839f, 19.73591f, 307.4758f),
                    new Utils.Vector4(-1184.012f, -745.5002f, 19.54056f, 307.4758f),
                });

                var lines = new List<string>();

                foreach (var x in All.Values)
                {
                    lines.Add($"new GarageRoot(GarageRoot.Types.{x.Type.ToString()}, {x.EnterPosition.Position.ToCSharpStr()}, {x.EnterPositionVehicle.ToCSharpStr()});");
                }

                Utils.FillFileToReplaceRegion(Settings.DIR_CLIENT_LOCATIONS_DATA_PATH, "GROOTS_TO_REPLACE", lines);
            }

            public Utils.Vector4 GetNextVehicleExit()
            {
                var nextId = LastExitUsed + 1;

                if (nextId >= VehicleExitPositions.Count)
                    nextId = 0;

                LastExitUsed = nextId;

                return VehicleExitPositions[nextId];
            }

            public bool IsEntityNearEnter(Entity entity) => entity.Dimension == Utils.Dimensions.Main && entity.Position.DistanceIgnoreZ(EnterPosition.Position) <= Settings.ENTITY_INTERACTION_MAX_DISTANCE;

            public bool IsEntityNearVehicleEnter(Entity entity) => entity.Dimension == Utils.Dimensions.Main && entity.Position.DistanceIgnoreZ(EnterPositionVehicle) <= Settings.ENTITY_INTERACTION_MAX_DISTANCE;

            public static GarageRoot Get(Types type) => All.GetValueOrDefault(type);
        }

        public PlayerData.PlayerInfo Owner { get; set; }

        public Style StyleData { get; set; }

        public uint Id { get; set; }

        public uint Dimension { get; set; }

        public GarageRoot Root { get; set; }

        public int Price { get; set; }

        public ClassTypes ClassType { get; set; }

        public int Tax => GetTax(ClassType);

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

        private static Dictionary<ClassTypes, int> Taxes = new Dictionary<ClassTypes, int>()
        {
            { ClassTypes.GA, 50 },
            { ClassTypes.GB, 75 },
            { ClassTypes.GC, 90 },
            { ClassTypes.GD, 100 },
        };

        public static int GetTax(ClassTypes cType) => Taxes[cType];

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

        public Garage(uint Id, GarageRoot.Types RootType, Types Type, byte Variation, int Price)
        {
            this.Id = Id;

            this.Root = GarageRoot.Get(RootType);

            this.StyleData = Style.Get(Type, Variation);

            this.Price = Price;

            this.ClassType = GetClass(this);

            this.Dimension = (uint)(Id + Utils.GarageDimBase);

            All.Add(Id, this);
        }

        public static void LoadAll()
        {
            GarageRoot.LoadAll();

            new Garage(1, GarageRoot.Types.Complex1, Types.Two, 0, 25_000);

            var lines = new List<string>();

            foreach (var x in All.Values)
            {
                MySQL.LoadGarage(x);

                lines.Add($"new Garage({x.Id}, GarageRoot.Types.{x.Root.Type.ToString()}, Garage.Types.{x.StyleData.Type.ToString()}, {x.Variation}, Garage.ClassTypes.{x.ClassType.ToString()}, {x.Tax}, {x.Price});");
            }

            Utils.FillFileToReplaceRegion(Settings.DIR_CLIENT_LOCATIONS_DATA_PATH, "GARAGES_TO_REPLACE", lines);
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
                    /*                    if (PlayerInfo.PlayerData != null)
                                        {
                                            PlayerInfo.PlayerData.Player.Notify("Bank::NotEnough", Balance);
                                        }*/
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

            if (!pData.TryRemoveCash((uint)Price, out newCash, true))
                return false;

            if (pData.GaragesSlots <= 0)
            {
                pData.Player.Notify("Trade::MGOW", pData.OwnedGarages.Count);

                return false;
            }

            pData.SetCash(newCash);

            ChangeOwner(pData.Info);

            return true;
        }

        public void ChangeOwner(PlayerData.PlayerInfo pInfo)
        {
            if (Owner != null)
            {
                Owner.PlayerData?.RemoveGarageProperty(this);
            }

            if (pInfo != null)
            {
                pInfo.PlayerData?.AddGarageProperty(this);
            }

            var vehicles = GetVehiclesInGarage()?.ToList();

            if (vehicles != null)
            {
                foreach (var x in vehicles)
                {
                    x.SetToVehiclePound();
                }
            }

            UpdateOwner(pInfo);

            MySQL.GarageUpdateOwner(this);
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

        public IEnumerable<VehicleData.VehicleInfo> GetVehiclesInGarage() => Owner?.OwnedVehicles.Where(x => x.LastData.GarageSlot >= 0 && (x.VehicleData?.Vehicle.Dimension ?? x.LastData.Dimension) == Dimension) ?? new List<VehicleData.VehicleInfo>();

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

                Utils.TeleportPlayers(pos.Position, false, Utils.Dimensions.Main, pos.RotationZ, true, players);
            }
            else
            {
                NAPI.ClientEvent.TriggerClientEventToPlayers(players, "Garage::Exit");
            }
        }
    }
}
