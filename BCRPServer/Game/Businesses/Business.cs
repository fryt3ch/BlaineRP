using Google.Protobuf.WellKnownTypes;
using GTANetworkAPI;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BCRPServer.Game.Businesses
{
    public class MaterialsData
    {
        public uint BuyPrice { get; set; }

        public uint SellPrice { get; set; }

        public uint RealPrice { get; set; }

        public uint MaxMaterialsBalance { get; set; }

        public uint MaxMaterialsPerOrder { get; set; }

        public Dictionary<string, uint> Prices { get; set; }

        public MaterialsData(uint BuyPrice, uint SellPrice, uint RealPrice, uint MaxMaterialsBalance = 500_000, uint maxMaterialsPerOrder = 100_000) // todo maxMatBalance&maxMatPerOrder - individual
        {
            this.BuyPrice = BuyPrice;
            this.SellPrice = SellPrice;
            this.RealPrice = RealPrice;

            this.Prices = Prices;
            this.MaxMaterialsBalance = MaxMaterialsBalance;
            this.MaxMaterialsPerOrder = maxMaterialsPerOrder;
        }
    }

    public abstract partial class Business
    {
        public static int CurrentStatisticsDayIdx { get; set; }

        public static int PreviousStatisticsDayIdx { get; set; }

        public const decimal INCASSATION_TAX = 0.05m;

        public const uint MATS_DELIVERY_PRICE = 2000;

        public const decimal MAX_MARGIN = 2.5m;
        public const ushort MAX_MARGIN_CLIENT = 150;

        public enum Types
        {
            ClothesShop1 = 0,
            ClothesShop2,
            ClothesShop3,

            JewelleryShop,

            MaskShop,

            BagShop,

            BarberShop,

            TattooShop,

            CarShop1,
            CarShop2,
            CarShop3,
            MotoShop,
            BoatShop,
            AeroShop,

            FurnitureShop,

            Market,

            GasStation,

            TuningShop,

            WeaponShop,

            Farm,
        }

        public Game.Jobs.Trucker ClosestTruckerJob
        {
            get
            {
                var truckerJobs = Jobs.Trucker.AllTruckerJobs;

                var minDist = PositionInfo.DistanceTo(truckerJobs[0].Position.Position);

                var minJob = truckerJobs[0];

                for (int i = 1; i < truckerJobs.Count; i++)
                {
                    var dist = PositionInfo.DistanceTo(truckerJobs[i].Position.Position);

                    if (dist < minDist)
                    {
                        minDist = dist;

                        minJob = truckerJobs[i];
                    }
                }

                return minJob;
            }
        }

        public Game.Jobs.Collector ClosestCollectorJob
        {
            get
            {
                var collectorJobs = Jobs.Collector.AllCollectorJobs;

                var minDist = PositionInfo.DistanceTo(collectorJobs[0].Position.Position);

                var minJob = collectorJobs[0];

                for (int i = 1; i < collectorJobs.Count; i++)
                {
                    var dist = PositionInfo.DistanceTo(collectorJobs[i].Position.Position);

                    if (dist < minDist)
                    {
                        minDist = dist;

                        minJob = collectorJobs[i];
                    }
                }

                return minJob;
            }
        }

        public abstract string ClientData { get; }

        public static Dictionary<int, Business> All { get; private set; } = new Dictionary<int, Business>();

        /// <summary>Тип бизнеса</summary>
        public Types Type { get; set; }

        /// <summary>ID бизнеса (уникальный)</summary>
        public int ID { get; set; }

        /// <summary>Владельца</summary>
        public PlayerData.PlayerInfo Owner { get; set; }

        /// <summary>Наличных в кассе</summary>
        public ulong Cash { get; set; }

        /// <summary>Денег в банке</summary>
        public ulong Bank { get; set; }

        /// <summary>Кол-во материалов</summary>
        public uint Materials { get; set; }

        /// <summary>Кол-во заказанных материалов</summary>
        public uint OrderedMaterials { get; set; }

        public bool IncassationState { get; set; }

        /// <summary>Гос. цена</summary>
        public uint GovPrice { get; set; }

        /// <summary>Наценка на товары</summary>
        public decimal Margin { get; set; }

        public decimal Tax { get; set; }

        public uint Rent { get; set; }

        /// <summary>Позиция бизнеса</summary>
        public Vector3 PositionInfo { get; set; }

        public Utils.Vector4 PositionInteract { get; set; }

        /// <summary>Статистика прибыли</summary>
        public ulong[] Statistics { get; set; }

        public MaterialsData MaterialsData => Shop.AllPrices.GetValueOrDefault(Type);

        public IEnumerable<PlayerData> PlayersInteracting => PlayerData.All.Values.Where(x => x.CurrentBusiness == this);

        public bool IsBuyable => PositionInfo != null;

        public Business(int ID, Vector3 PositionInfo, Utils.Vector4 PositionInteract, Types Type)
        {
            this.ID = ID;

            this.Type = Type;

            this.PositionInfo = PositionInfo;
            this.PositionInteract = PositionInteract;

            All.Add(ID, this);
        }

        public bool IsPlayerNearInfoPosition(PlayerData pData)
        {
            if (PositionInfo == null)
                return false;

            return Vector3.Distance(pData.Player.Position, PositionInfo) <= 10f;
        }

        public bool IsPlayerNearInteractPosition(PlayerData pData)
        {
            return Vector3.Distance(pData.Player.Position, PositionInteract.Position) <= 10f;
        }

        public static Utils.Vector4 GetNextExitProperty(IEnterable enterable)
        {
            if (enterable.ExitProperties.Length == 1)
                return enterable.ExitProperties[0];

            if (enterable.LastExitUsed >= enterable.ExitProperties.Length)
                enterable.LastExitUsed = 0;

            return enterable.ExitProperties[enterable.LastExitUsed++];
        }

        public static Business Get(int id) => All.GetValueOrDefault(id);

        public void UpdateOwner(PlayerData.PlayerInfo pInfo)
        {
            Owner = pInfo;

            Sync.World.SetSharedData($"Business::{ID}::OName", pInfo == null ? null : $"{pInfo.Name} {pInfo.Surname} [#{pInfo.CID}]");
        }

        public bool TryAddMoneyCash(ulong amount, out ulong newBalance, bool notifyOnFault = true, PlayerData tData = null)
        {
            if (!Cash.TryAdd(amount, out newBalance))
            {
                if (notifyOnFault)
                {

                }

                return false;
            }

            return true;
        }

        public bool TryRemoveMoneyCash(ulong amount, out ulong newBalance, bool notifyOnFault = true, PlayerData tData = null)
        {
            if (!Cash.TrySubtract(amount, out newBalance))
            {
                if (notifyOnFault)
                {
                    if (tData != null)
                    {
                        tData.Player.Notify("Business::NEMC", Bank);
                    }
                }

                return false;
            }

            return true;
        }

        public void SetCash(ulong amount)
        {
            Cash = amount;
        }

        public bool TryAddMoneyBank(ulong amount, out ulong newBalance, bool notifyOnFault = true, PlayerData tData = null)
        {
            if (!Bank.TryAdd(amount, out newBalance))
            {
                if (notifyOnFault)
                {

                }

                return false;
            }

            return true;
        }

        public bool TryRemoveMoneyBank(ulong amount, out ulong newBalance, bool notifyOnFault = true, PlayerData tData = null)
        {
            if (!Bank.TrySubtract(amount, out newBalance))
            {
                if (notifyOnFault)
                {
                    if (tData != null)
                    {
                        tData.Player.Notify("Business::NEMB", Bank);
                    }
                }

                return false;
            }

            return true;
        }

        public void SetBank(ulong amount)
        {
            Bank = amount;
        }

        public bool TryAddMaterials(uint amount, out uint newBalance, bool notifyOnFault = true, PlayerData tData = null)
        {
            if (!Materials.TryAdd(amount, out newBalance))
            {
                if (notifyOnFault)
                {

                }

                return false;
            }

            return true;
        }

        public bool TryRemoveMaterials(uint amount, out uint newBalance, bool notifyOnFault = true, PlayerData tData = null)
        {
            if (!Materials.TrySubtract(amount, out newBalance))
            {
                if (notifyOnFault)
                {
                    if (tData != null)
                    {
                        tData.Player.Notify("Business:NoMats");
                    }
                }

                return false;
            }

            return true;
        }

        public void SetMaterials(uint amount)
        {
            Materials = amount;
        }

        public void SetMargin(decimal value)
        {
            Margin = value;

            MySQL.BusinessUpdateMargin(this);

            var players = PlayersInteracting.Select(x => x.Player).ToArray();

            if (players.Length == 0)
                return;

            NAPI.ClientEvent.TriggerClientEventToPlayers(players, "Shop::UM", (float)value);
        }

        public void UpdateStatistics(ulong value)
        {
            ulong newStatDaySum;

            if (!Statistics[CurrentStatisticsDayIdx].TryAdd(value, out newStatDaySum))
            {
                if (Statistics[CurrentStatisticsDayIdx] == ulong.MaxValue)
                    return;

                newStatDaySum = ulong.MaxValue;
            }

            Statistics[CurrentStatisticsDayIdx] = newStatDaySum;
        }

        public ulong GetBusinessPrice(uint mats, bool incassation) => incassation ? (ulong)Math.Floor((decimal)mats * MaterialsData.SellPrice * Margin * (1m - Tax - INCASSATION_TAX)) : (ulong)Math.Floor((decimal)mats * MaterialsData.SellPrice * Margin * (1m - Tax));

        public ulong GetBusinessPriceFixed(ulong fixedPrice, bool incassation) => incassation ? (ulong)Math.Floor(fixedPrice * (1m - Tax - INCASSATION_TAX)) : (ulong)Math.Floor(fixedPrice * (1m - Tax));

        public void AddOrder(bool isCustom, Game.Jobs.Trucker truckerJob)
        {
            if (truckerJob == null)
                truckerJob = ClosestTruckerJob;

            if (isCustom)
            {
                truckerJob.AddCustomOrder(this);
            }
            else
            {
                truckerJob.AddDefaultOrder(this);
            }
        }

        public virtual void ProceedPayment(PlayerData pData, bool useCash, uint newMats, ulong newBalance, ulong newPlayerBalance)
        {
            if (useCash && !IncassationState)
            {
                if (Owner != null)
                {
                    if (newMats != Materials)
                        SetMaterials(newMats);

                    if (newBalance > Cash)
                        UpdateStatistics(newBalance - Cash);

                    SetCash(newBalance);

                    MySQL.BusinessUpdateBalances(this, false);
                }

                pData.SetCash(newPlayerBalance);
            }
            else
            {
                if (Owner != null)
                {
                    if (newMats != Materials)
                        SetMaterials(newMats);

                    if (newBalance > Bank)
                        UpdateStatistics(newBalance - Bank);

                    SetBank(newBalance);

                    MySQL.BusinessUpdateBalances(this, false);
                }

                if (useCash)
                    pData.SetCash(newPlayerBalance);
                else
                    pData.BankAccount.SetDebitBalance(newPlayerBalance, null);
            }
        }

        public void SellToGov(bool balancesBack = true, bool govHalfPriceBack = true)
        {
            if (Owner == null)
                return;

            ulong newBalance;

            if (balancesBack)
            {
                var totalMoney = Cash + Bank;

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
                var totalMoney = GovPrice / 2;

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

            if (Owner.PlayerData != null)
            {
                Owner.PlayerData.RemoveBusinessProperty(this);
            }

            Cash = 0;
            Bank = 0;
            Materials = 0;
            Margin = 1m;

            var players = PlayersInteracting.Select(x => x.Player).ToArray();

            if (players.Length == 0)
                return;

            NAPI.ClientEvent.TriggerClientEventToPlayers(players, "Shop::UM", (float)Margin);

            UpdateOwner(null);

            MySQL.BusinessUpdateComplete(this);
        }

        public bool BuyFromGov(PlayerData pData)
        {
            if (Settings.NEED_BUSINESS_LICENSE && !pData.HasLicense(PlayerData.LicenseTypes.Business))
                return false;

            ulong newCash;

            if (!pData.TryRemoveCash(GovPrice, out newCash, true))
                return false;

            if (pData.BusinessesSlots <= 0)
            {
                pData.Player.Notify("Trade::MBOW", pData.OwnedBusinesses.Count);

                return false;
            }

            pData.SetCash(newCash);

            ChangeOwner(pData.Info);

            return true;
        }

        public void ChangeOwner(PlayerData.PlayerInfo pInfo, bool buyGov = false)
        {
            if (Owner != null)
            {
                Owner.PlayerData?.RemoveBusinessProperty(this);
            }

            if (pInfo != null)
            {
                pInfo.PlayerData?.AddBusinessProperty(this);

                var minBalance = Settings.MIN_PAID_HOURS_BUSINESS * Rent;

                if (buyGov && Bank < minBalance)
                    SetBank(minBalance);
            }

            UpdateOwner(pInfo);

            MySQL.BusinessUpdateOwner(this);
        }

        public JObject ToClientMenuObject()
        {
            var obj = new JObject
            {
                { "C", Cash },
                { "B", Bank },
                { "M", Materials },
                { "MA", Margin },
                { "IS", false },
                { "IT", INCASSATION_TAX },
                { "DS", (OrderedMaterials > 0 && ClosestTruckerJob.ActiveOrders.Values.Where(x => x.TargetBusiness == this && x.IsCustom && x.CurrentWorker != null).FirstOrDefault() != null) ? $"{OrderedMaterials}_0" : $"{OrderedMaterials}" },
                { "MB", MaterialsData.BuyPrice },
                { "MS", MaterialsData.SellPrice },
                { "DP", MATS_DELIVERY_PRICE },
                { "S", Statistics.SerializeToJson() }
            };

            return obj;
        }
    }

    public interface IEnterable
    {
        public Utils.Vector4 EnterProperties { get; set; }

        public Utils.Vector4[] ExitProperties { get; set; }

        public int LastExitUsed { get; set; }
    }
}
