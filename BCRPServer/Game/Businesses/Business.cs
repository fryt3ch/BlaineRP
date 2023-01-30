using BCRPServer.Game.Items;
using GTANetworkAPI;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace BCRPServer.Game.Businesses
{
    public class MaterialsData
    {
        public int BuyPrice { get; set; }

        public int SellPrice { get; set; }

        public int RealPrice { get; set; }

        public Dictionary<string, int> Prices { get; set; }

        public MaterialsData(int BuyPrice, int SellPrice, int RealPrice)
        {
            this.BuyPrice = BuyPrice;
            this.SellPrice = SellPrice;
            this.RealPrice = RealPrice;

            this.Prices = Prices;
        }
    }

    public abstract partial class Business
    {
        public static int CurrentStatisticsDayIdx { get; set; }

        public static int PreviousStatisticsDayIdx { get; set; }

        public const float INCASSATION_TAX = 0.05f;

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
        public int Cash { get; set; }

        /// <summary>Денег в банке</summary>
        public int Bank { get; set; }

        /// <summary>Кол-во материалов</summary>
        public int Materials { get; set; }

        /// <summary>Кол-во заказанных материалов</summary>
        public int OrderedMaterials { get; set; }

        public bool IncassationState { get; set; }

        /// <summary>Гос. цена</summary>
        public int GovPrice { get; set; }

        /// <summary>Наценка на товары</summary>
        public float Margin { get; set; }

        public float Tax { get; set; }

        public int Rent { get; set; }

        /// <summary>Позиция бизнеса</summary>
        public Vector3 PositionInfo { get; set; }

        public Utils.Vector4 PositionInteract { get; set; }

        /// <summary>Статистика прибыли</summary>
        public int[] Statistics { get; set; }

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

        public bool HasEnoughMaterials(int value, PlayerData pData = null)
        {
            if (Materials >= value)
                return true;

            if (pData != null)
                pData.Player.Notify("Business:NoMats");

            return false;
        }

        public void UpdateStatistics(int value) => Statistics[CurrentStatisticsDayIdx] += value;

        public void PaymentProceed(PlayerData pData, bool cash, int mats, int realPrice)
        {
            if (cash)
            {
                if (Owner != null)
                {
                    Materials -= mats;

                    var moneyGet = (int)Math.Floor(mats * MaterialsData.SellPrice * Margin * (1f - Tax));

                    Cash += moneyGet;

                    UpdateStatistics(moneyGet);

                    MySQL.BusinessUpdateBalances(this);
                }

                pData.Cash -= realPrice;

                MySQL.CharacterCashUpdate(pData.Info);
            }
            else
            {
                if (Owner != null)
                {
                    Materials -= mats;

                    var moneyGet = (int)Math.Floor(mats * MaterialsData.SellPrice * Margin * (1f - Tax - INCASSATION_TAX));

                    Bank += moneyGet;

                    UpdateStatistics(moneyGet);

                    MySQL.BusinessUpdateBalances(this);
                }

                pData.BankBalance -= realPrice;

                MySQL.BankAccountUpdate(pData.BankAccount);
            }
        }

        public void PaymentProceed(PlayerData pData, bool cash, int fixedPrice)
        {
            if (cash)
            {
                if (Owner != null)
                {
                    var moneyGet = (int)Math.Floor(fixedPrice * (1f - Tax));

                    Cash += moneyGet;

                    UpdateStatistics(moneyGet);

                    MySQL.BusinessUpdateBalances(this);
                }

                pData.Cash -= fixedPrice;

                MySQL.CharacterCashUpdate(pData.Info);
            }
            else
            {
                if (Owner != null)
                {
                    var moneyGet = (int)Math.Floor(fixedPrice * (1f - Tax - INCASSATION_TAX));

                    Bank += moneyGet;

                    UpdateStatistics(moneyGet);

                    MySQL.BusinessUpdateBalances(this);
                }

                pData.BankBalance -= fixedPrice;

                MySQL.BankAccountUpdate(pData.BankAccount);
            }
        }

        public void SellToGov(bool moneyBack = true)
        {
            if (Owner == null)
                return;

            if (Owner.PlayerData == null)
            {
                if (moneyBack)
                {
                    if (Owner.BankAccount != null)
                    {
                        Owner.Cash += Cash;

                        Owner.BankAccount.Balance += Bank + GovPrice / 2;

                        MySQL.BankAccountUpdate(Owner.PlayerData.BankAccount);
                    }
                    else
                    {
                        Owner.Cash += Cash + Bank + GovPrice / 2;
                    }

                    MySQL.CharacterCashUpdate(Owner);
                }
            }
            else
            {
                if (moneyBack)
                {
                    if (Owner.PlayerData.BankAccount != null)
                    {
                        Owner.PlayerData.Cash += Cash;

                        Owner.PlayerData.BankBalance += Bank + GovPrice / 2;

                        MySQL.BankAccountUpdate(Owner.PlayerData.BankAccount);
                    }
                    else
                    {
                        Owner.PlayerData.Cash += Cash + Bank + GovPrice / 2;
                    }

                    MySQL.CharacterCashUpdate(Owner);
                }

                Owner.PlayerData.RemoveBusinessProperty(this);
            }

            Cash = 0;
            Bank = 0;
            Materials = 0;
            Margin = 1f;

            UpdateOwner(null);

            MySQL.BusinessUpdateComplete(this);
        }

        public bool BuyFromGov(PlayerData pData)
        {
            if (!pData.HasEnoughCash(GovPrice, true))
                return false;

            if (pData.OwnedBusinesses.Count >= pData.BusinessesSlots)
            {
                pData.Player.Notify("Business::HMA");

                return false;
            }

            if (Settings.NEED_BUSINESS_LICENSE && !pData.Licenses.Contains(PlayerData.LicenseTypes.Business))
            {
                pData.Player.Notify("License::NTB");

                return false;
            }

            pData.Cash -= GovPrice;

            ChangeOwner(pData.Info);

            return true;
        }

        public void ChangeOwner(PlayerData.PlayerInfo pInfo)
        {
            if (Owner != null)
            {
                Owner.PlayerData?.RemoveBusinessProperty(this);
            }

            if (pInfo != null)
            {
                pInfo.PlayerData?.AddBusinessProperty(this);
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
                { "DS", 0 },
                { "MB", MaterialsData.BuyPrice },
                { "MS", MaterialsData.SellPrice },
                { "DP", 2000 },
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
