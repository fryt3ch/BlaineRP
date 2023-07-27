using GTANetworkAPI;
using System.Collections.Generic;
using System.Linq;
using BlaineRP.Server.Additional;
using BlaineRP.Server.EntitiesData.Players;
using BlaineRP.Server.EntitiesData.Vehicles;
using BlaineRP.Server.Extensions.System;
using BlaineRP.Server.Game.Management.AntiCheat;

namespace BlaineRP.Server.Game.Estates
{
    public partial class Garage
    {
        public static Dictionary<uint, Garage> All { get; set; } = new Dictionary<uint, Garage>();

        public PlayerInfo Owner { get; set; }

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

        public void UpdateOwner(PlayerInfo pInfo)
        {
            Owner = pInfo;

            World.Service.SetSharedData($"Garages::{Id}::OName", pInfo == null ? null : $"{pInfo.Name} {pInfo.Surname} [#{pInfo.CID}]");
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

        public void ChangeOwner(PlayerInfo pInfo, bool buyGov = false)
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

            vData.Vehicle.Teleport(vPos.Position, Dimension, vPos.RotationZ, true, VehicleTeleportType.All);

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

        public void SetVehicleToGarageOnlyData(VehicleInfo vInfo, int slot)
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

        public List<VehicleInfo> GetVehiclesInGarage()
        {
            if (Owner == null)
                return new List<VehicleInfo>();

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
