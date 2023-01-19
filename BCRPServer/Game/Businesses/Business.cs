using BCRPServer.Game.Items;
using GTANetworkAPI;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;
using static BCRPServer.Game.Bank;

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

    public abstract class Business
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

        public MaterialsData MaterialsData { get; set; }

        public IEnumerable<PlayerData> PlayersInteracting => PlayerData.All.Values.Where(x => x.CurrentBusiness == this);

        public bool IsBuyable => PositionInfo != null;

        public Business(int ID, Vector3 PositionInfo, Utils.Vector4 PositionInteract, Types Type)
        {
            this.ID = ID;

            this.Type = Type;

            this.PositionInfo = PositionInfo;
            this.PositionInteract = PositionInteract;

            this.MaterialsData = Shop.AllPrices.GetValueOrDefault(Type);

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

        public static int LoadAll()
        {
            #region Clothes (Cheap)
            new ClothesShop1(1, new Vector3(1201.013f, 2701.155f, 37.15381f), new Utils.Vector4(1201.885f, 2710.143f, 38.2226f, 105f));
            new ClothesShop1(3, new Vector3(-1096.97f, 2701.689f, 17.93724f), new Utils.Vector4(-1097.523f, 2714.026f, 19.108f, 150f));
            new ClothesShop1(4, new Vector3(1683.12f, 4822.236f, 41.03899f), new Utils.Vector4(1694.727f, 4817.582f, 42.06f, 20f));
            new ClothesShop1(5, new Vector3(-4.590637f, 6516.01f, 30.48112f), new Utils.Vector4(1f, 6508.753f, 31.87f, 325f));

            new ClothesShop1(11, new Vector3(-814.2472f, -1078.953f, 10.13255f), new Utils.Vector4(-817.8808f, -1070.944f, 11.32811f, 135f));
            new ClothesShop1(12, new Vector3(84.3979f, -1388.727f, 28.41426f), new Utils.Vector4(75.42346f, -1387.689f, 29.37614f, 195.8f));
            new ClothesShop1(13, new Vector3(416.1883f, -810.5462f, 28.33503f), new Utils.Vector4(425.6321f, -811.4822f, 29.49114f, 11f));
            #endregion

            #region Clothes (Expensive)
            new ClothesShop2(2, new Vector3(623.3489f, 2744.308f, 41.01938f), new Utils.Vector4(613.035f, 2761.843f, 42.088f, 265f));
            new ClothesShop2(14, new Vector3(-3168.294f, 1062.636f, 19.84479f), new Utils.Vector4(-3169.008f, 1044.211f, 20.86322f, 48.6f));

            new ClothesShop2(9, new Vector3(126.1404f, -203.9787f, 53.53281f), new Utils.Vector4(127.3073f, -223.18f, 54.55783f, 66f));
            new ClothesShop2(10, new Vector3(-1203.813f, -784.1447f, 16.05554f), new Utils.Vector4(-1194.725f, -767.6141f, 17.31629f, 208f));

            #endregion

            #region Clothes (Brand)
            new ClothesShop3(6, new Vector3(-1459.713f, -233.0381f, 48.45752f), new Utils.Vector4(-1448.824f, -237.893f, 49.81332f, 45f));
            new ClothesShop3(7, new Vector3(-717.7559f, -159.8642f, 35.99546f), new Utils.Vector4(-708.95f, -151.6612f, 37.415f, 114f));
            new ClothesShop3(8, new Vector3(-152.0491f, -303.7605f, 37.95464f), new Utils.Vector4(-165f, -303.2f, 39.73328f, 251f));
            #endregion

            new Market(15, new Vector3(546.4489f, 2675.079f, 41.16602f), new Utils.Vector4(549.1185f, 2671.407f, 42.1565f, 91.55f));

            new GasStation(16, new Vector3(270.1317f, 2601.239f, 43.64737f), null, new Vector3(263.9698f, 2607.402f, 44.98298f));

            new CarShop1(17, new Vector3(-62.48621f, -1089.3f, 25.69341f), new Utils.Vector4(-55.08611f, -1111.217f, 26.05543f, 36.2f), new Utils.Vector4[] { new Utils.Vector4(-41.65706f, -1116.344f, 26.05584f, 3f), new Utils.Vector4(-45.15728f, -1116.411f, 26.05584f, 3f), new Utils.Vector4(-47.71569f, -1116.379f, 26.05584f, 3f), new Utils.Vector4(-50.56787f, -1116.191f, 26.05584f, 3f), new Utils.Vector4(-53.62245f, -1116.565f, 26.05584f, 3f), new Utils.Vector4(-56.34209f, -1116.566f, 26.05584f, 3f), new Utils.Vector4(-59.11841f, -1116.814f, 26.05584f, 3f), new Utils.Vector4(-62.03639f, -1117.178f, 26.05584f, 3f) }, new Utils.Vector4(-54.68786f, -1088.418f, 26.42234f, 155.8f));

            new BoatShop(18, new Vector3(-813.3688f, -1336.428f, 4.150263f), new Utils.Vector4(-852.8972f, -1335.998f, 0.1195435f, 108.4271f), new Utils.Vector4[] { }, new Utils.Vector4(-813.8713f, -1343.797f, 5.150264f, 49.62344f));

            new AeroShop(19, new Vector3(1757.495f, 3239.969f, 40.94524f), new Utils.Vector4(1770.4f, 3239.908f, 42.02776f, 352.3067f), new Utils.Vector4[] { }, new Utils.Vector4(1760.724f, 3234.819f, 42.13989f, 314.5554f));

            new TuningShop(20, new Vector3(1178.526f, 2647.779f, 36.79328f), new Utils.Vector4(1175.327f, 2639.85f, 37.3765f, 325f), new Utils.Vector4[] { new Utils.Vector4(1188.023f, 2650.051f, 37.46183f, 50f), new Utils.Vector4(1188.778f, 2658.188f, 37.44228f, 315f) }, new Utils.Vector4(1175.327f, 2639.85f, 37.3765f));
            new TuningShop(21, new Vector3(-356.59f, -129.5743f, 38.43067f), new Utils.Vector4(-338.4644f, -136.1622f, 38.62881f, 115f), new Utils.Vector4[] { new Utils.Vector4(-356.0863f, -115.5013f, 38.31553f, 72f), new Utils.Vector4(-377.5053f, -146.7845f, 38.30297f, 300f), new Utils.Vector4(-378.845f, -143.6312f, 38.30399f, 300f), new Utils.Vector4(-380.2281f, -140.2031f, 38.30404f, 300f), new Utils.Vector4(-382.0588f, -137.2818f, 38.30477f, 300f), new Utils.Vector4(-384.2779f, -134.4641f, 38.30402f, 300f), new Utils.Vector4(-385.8221f, -131.2085f, 38.30363f, 300f), new Utils.Vector4(-387.4566f, -128.2165f, 38.30125f, 300f), new Utils.Vector4(-388.955f, -124.9111f, 38.30383f, 300f), new Utils.Vector4(-390.6784f, -121.6463f, 38.29424f, 300f), new Utils.Vector4(-392.3392f, -118.4143f, 38.21078f, 300f) }, new Utils.Vector4(-338.4644f, -136.1622f, 38.62881f));
            new TuningShop(22, new Vector3(720.2494f, -1082.472f, 21.25672f), new Utils.Vector4(732.6448f, -1088.88f, 21.78887f, 140f), new Utils.Vector4[] { new Utils.Vector4(719.222f, -1079.078f, 21.85986f, 95f), new Utils.Vector4(705.7102f, -1071.86f, 22.03793f, 270f), new Utils.Vector4(705.0659f, -1061.33f, 22.0562f, 270f), new Utils.Vector4(706.761f, -1053.564f, 21.96827f, 250f) }, new Utils.Vector4(732.6448f, -1088.88f, 21.78887f));
            new TuningShop(23, new Vector3(-1139.816f, -1992.655f, 12.16545f), new Utils.Vector4(-1154.301f, -2006.171f, 12.79945f, 0f), new Utils.Vector4[] { new Utils.Vector4(-1151.391f, -1982.481f, 12.77934f, 280f), new Utils.Vector4(-1136.405f, -1975.773f, 12.78101f, 180f), new Utils.Vector4(-1132.026f, -1975.115f, 12.78097f, 180f), new Utils.Vector4(-1127.017f, -1976.082f, 12.79133f, 180f) }, new Utils.Vector4(-1154.301f, -2006.171f, 12.79945f));

            new WeaponShop(24, new Vector3(-1109.188f, 2690.507f, 17.6103f), new Utils.Vector4(-1118.7f, 2699.981f, 18.55415f, 218.2682f), new Utils.Vector4(-1122.019f, 2696.923f, 18.55415f, 305.8211f));

            new BarberShop(25, new Vector3(-285.7959f, 6232.987f, 30.49523f), new Utils.Vector4(-277.8783f, 6230.489f, 31.69553f, 42.97519f), new Utils.Vector4(-278.4798f, 6227.705f, 31.69553f, 354.1428f));
            new BarberShop(26, new Vector3(1933.23f, 3721.24f, 31.86353f), new Utils.Vector4(1930.866f, 3728.215f, 32.84443f, 212.5517f), new Utils.Vector4(-278.4798f, 6227.705f, 31.69553f, 354.1428f));
            new BarberShop(27, new Vector3(-827.2922f, -186.6121f, 36.63704f), new Utils.Vector4(-822.0836f, -183.4575f, 37.56893f, 202.6368f), new Utils.Vector4(-815.0012f, -183.6297f, 37.56892f, 166.9111f));
            new BarberShop(28, new Vector3(-31.65811f, -144.1567f, 56.07059f), new Utils.Vector4(-30.80062f, -151.6473f, 57.0765f, 337.2878f), new Utils.Vector4(-278.4798f, 6227.705f, 31.69553f, 354.1428f));
            new BarberShop(29, new Vector3(1203.78f, -472.2391f, 65.20185f), new Utils.Vector4(1211.432f, -470.6421f, 66.20801f, 71.84773f), new Utils.Vector4(-278.4798f, 6227.705f, 31.69553f, 354.1428f));
            new BarberShop(30, new Vector3(-1291.942f, -1119.263f, 5.557382f), new Utils.Vector4(-1284.172f, -1115.377f, 6.990112f, 85.004921f), new Utils.Vector4(-278.4798f, 6227.705f, 31.69553f, 354.1428f));
            new BarberShop(31, new Vector3(129.2226f, -1712.456f, 28.25804f), new Utils.Vector4(134.6306f, -1707.976f, 29.29162f, 139.164f), new Utils.Vector4(-278.4798f, 6227.705f, 31.69553f, 354.1428f));

            new BagShop(32, null, new Utils.Vector4(1929.689f, 4635.728f, 40.44981f, 344.2735f), new Utils.Vector4(1928.622f, 4642.992f, 40.41078f, 197.1759f)); // new Vector3(1925.026f, 4637.98f, 39.16909f)

            new MaskShop(33, null, new Utils.Vector4(-1334.991f, -1277.063f, 4.96355f, 114.1392f), new Utils.Vector4(-1342.1f, -1277.109f, 4.89769f, 245.6555f)); // new Vector3(-1334.951f, -1283.553f, 3.835981f)

            new JewelleryShop(34, new Vector3(-630.8311f, -241.3816f, 37.16668f), new Utils.Vector4(-623.4676f, -230.2429f, 38.05705f, 123.827f), new Utils.Vector4(-619.3085f, -228.7722f, 38.05704f, 127.965f));

            new TattooShop(35, new Vector3(-286.4024f, 6199.159f, 30.45244f), new Utils.Vector4(-292.0363f, 6199.72f, 31.48712f, 222.5584f), new Utils.Vector4(-293.9031f, 6200.241f, 31.48711f, 186.6973f));
            new TattooShop(36, new Vector3(-3165.543f, 1075.181f, 20.84793f), new Utils.Vector4(-3170.584f, 1073.112f, 20.82918f, 335.5721f), new Utils.Vector4(-3169.231f, 1077.034f, 20.82918f, 194.2503f));

            var lines = new List<string>();

            foreach (var x in All.Values)
            {
                MySQL.LoadBusiness(x);

                var daysDiff = CurrentStatisticsDayIdx - PreviousStatisticsDayIdx;

                for (int j = 0; j < daysDiff; j++)
                {
                    for (int i = 0; i < x.Statistics.Length; i++)
                    {
                        if (i == x.Statistics.Length - 1)
                        {
                            x.Statistics[i] = 0;
                        }
                        else
                        {
                            x.Statistics[i] = x.Statistics[i + 1];
                        }
                    }
                }

                lines.Add($"new {x.GetType().Name}({x.ClientData});");
            }

            Utils.FillFileToReplaceRegion(Settings.DIR_CLIENT_LOCATIONS_DATA_PATH, "BIZS_TO_REPLACE", lines);

            lines = new List<string>();

            foreach (var x in Shop.AllPrices)
            {
                lines.Add($"Prices.Add(Types.{x.Key}, new Dictionary<string, int>() {{{string.Join(", ", x.Value.Prices.Select(y => $"{{\"{y.Key}\", {y.Value * x.Value.RealPrice}}}"))}}});");
            }

            Utils.FillFileToReplaceRegion(Settings.DIR_CLIENT_SHOP_DATA_PATH, "TO_REPLACE", lines);

            WeaponShop.ShootingRangePrice = 150;

            return All.Count;
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
            var obj = new JObject();

            obj.Add("C", Cash);
            obj.Add("B", Bank);
            obj.Add("M", Materials);
            obj.Add("MA", Margin);
            obj.Add("IS", false);
            obj.Add("IT", INCASSATION_TAX);
            obj.Add("DS", 0);
            obj.Add("MB", MaterialsData.BuyPrice);
            obj.Add("MS", MaterialsData.SellPrice);
            obj.Add("DP", 2000);
            obj.Add("S", Statistics.SerializeToJson());

            return obj;
        }
    }

    public interface IEnterable
    {
        public Utils.Vector4 EnterProperties { get; set; }

        public Utils.Vector4[] ExitProperties { get; set; }

        public int LastExitUsed { get; set; }
    }

    public abstract class Shop : Business
    {
        /// <summary>Все цены в магазинах (бизнесах)</summary>
        /// <remarks>Цены - в материалах, не в долларах</remarks>
        public static Dictionary<Types, MaterialsData> AllPrices { get; private set; } = new Dictionary<Types, MaterialsData>()
        {
            #region ClothesShop1
            {
                Types.ClothesShop1,

                new MaterialsData(5, 7, 50) { Prices = new Dictionary<string, int>()
                {
                    { "top_m_0", 100 },
                    { "top_m_1", 100 },
                    { "top_m_2", 100 },
                    { "top_m_3", 100 },
                    { "top_m_4", 100 },
                    { "top_m_5", 100 },
                    { "top_m_6", 100 },
                    { "top_m_7", 100 },
                    { "top_m_8", 100 },
                    { "top_m_9", 100 },
                    { "top_m_10", 100 },
                    { "top_m_11", 100 },
                    { "top_m_12", 100 },
                    { "top_m_13", 100 },
                    { "top_m_14", 100 },
                    { "top_m_15", 100 },
                    { "top_m_16", 100 },
                    { "top_m_17", 100 },
                    { "top_m_18", 100 },
                    { "top_m_19", 100 },
                    { "top_m_20", 100 },
                    { "top_m_21", 100 },
                    { "top_m_22", 100 },
                    { "top_m_23", 100 },
                    { "top_m_24", 100 },
                    { "top_m_25", 100 },
                    { "top_m_26", 100 },
                    { "top_m_27", 100 },
                    { "top_m_28", 100 },
                    { "top_m_29", 100 },
                    { "top_m_30", 100 },
                    { "top_m_31", 100 },
                    { "top_m_32", 100 },
                    { "top_m_33", 100 },
                    { "top_m_34", 100 },
                    { "top_m_35", 100 },
                    { "top_m_36", 100 },
                    { "top_m_37", 100 },
                    { "top_m_38", 100 },
                    { "top_m_39", 100 },
                    { "top_m_40", 100 },
                    { "top_m_41", 100 },
                    { "top_m_42", 100 },
                    { "top_m_43", 100 },
                    { "top_m_44", 100 },
                    { "top_m_45", 100 },
                    { "top_m_46", 100 },
                    { "top_m_47", 100 },
                    { "top_m_48", 100 },
                    { "top_m_49", 100 },
                    { "top_m_50", 100 },
                    { "top_m_51", 100 },
                    { "top_m_52", 100 },
                    { "top_m_53", 100 },
                    { "top_m_54", 100 },
                    { "top_m_55", 100 },
                    { "top_m_56", 100 },
                    { "top_m_57", 100 },
                    { "top_m_58", 100 },
                    { "top_m_59", 100 },
                    { "top_m_60", 100 },
                    { "top_m_61", 100 },
                    { "top_m_62", 100 },
                    { "top_m_63", 100 },
                    { "top_m_64", 100 },
                    { "top_m_65", 100 },
                    { "top_m_66", 100 },
                    { "top_m_67", 100 },
                    { "top_m_68", 100 },
                    { "top_m_69", 100 },
                    { "top_m_70", 100 },
                    { "top_m_71", 100 },
                    { "top_m_72", 100 },
                    { "top_m_73", 100 },
                    { "top_m_74", 100 },
                    { "top_m_75", 100 },
                    { "top_m_76", 100 },
                    { "top_m_77", 100 },
                    { "top_m_78", 100 },
                    { "top_m_79", 100 },
                    { "top_m_80", 100 },
                    { "top_m_81", 100 },
                    { "top_m_82", 100 },
                    { "top_m_83", 100 },
                    { "top_m_84", 100 },
                    { "top_m_85", 100 },
                    { "top_m_86", 100 },
                    { "top_m_87", 100 },
                    { "top_m_88", 100 },
                    { "top_m_89", 100 },
                    { "top_m_90", 100 },
                    { "top_m_91", 100 },
                    { "top_m_92", 100 },
                    { "top_m_93", 100 },
                    { "top_m_94", 100 },
                    { "top_m_97", 100 },
                    { "top_m_98", 100 },
                    { "top_m_99", 100 },
                    { "top_m_100", 100 },
                    { "top_m_101", 100 },
                    { "top_m_102", 100 },
                    { "top_m_103", 100 },
                    { "top_m_104", 100 },
                    { "top_m_105", 100 },
                    { "top_m_106", 100 },
                    { "top_m_107", 100 },
                    { "top_m_108", 100 },
                    { "top_m_109", 100 },
                    { "top_m_110", 100 },
                    { "top_m_112", 100 },
                    { "top_m_113", 100 },
                    { "top_m_114", 100 },
                    { "top_m_115", 100 },
                    { "top_m_116", 100 },
                    { "top_m_117", 100 },
                    { "top_m_118", 100 },
                    { "top_m_119", 100 },
                    { "top_m_120", 100 },
                    { "top_m_121", 100 },
                    { "top_m_122", 100 },
                    { "top_m_123", 100 },
                    { "top_m_124", 100 },
                    { "top_m_126", 100 },
                    { "top_m_127", 100 },
                    { "top_m_128", 100 },
                    { "top_m_129", 100 },
                    { "top_m_130", 100 },
                    { "top_m_132", 100 },
                    { "top_m_133", 100 },

                    { "top_f_1", 100 },
                    { "top_f_2", 100 },
                    { "top_f_3", 100 },
                    { "top_f_4", 100 },
                    { "top_f_5", 100 },
                    { "top_f_6", 100 },
                    { "top_f_8", 100 },
                    { "top_f_10", 100 },
                    { "top_f_11", 100 },
                    { "top_f_12", 100 },
                    { "top_f_13", 100 },
                    { "top_f_14", 100 },
                    { "top_f_15", 100 },
                    { "top_f_16", 100 },
                    { "top_f_17", 100 },
                    { "top_f_18", 100 },
                    { "top_f_19", 100 },
                    { "top_f_20", 100 },
                    { "top_f_21", 100 },
                    { "top_f_22", 100 },
                    { "top_f_23", 100 },
                    { "top_f_24", 100 },
                    { "top_f_25", 100 },
                    { "top_f_26", 100 },
                    { "top_f_27", 100 },
                    { "top_f_28", 100 },
                    { "top_f_29", 100 },
                    { "top_f_30", 100 },
                    { "top_f_31", 100 },
                    { "top_f_32", 100 },
                    { "top_f_33", 100 },
                    { "top_f_34", 100 },
                    { "top_f_35", 100 },
                    { "top_f_36", 100 },
                    { "top_f_37", 100 },
                    { "top_f_38", 100 },
                    { "top_f_39", 100 },
                    { "top_f_40", 100 },
                    { "top_f_41", 100 },
                    { "top_f_42", 100 },
                    { "top_f_43", 100 },
                    { "top_f_44", 100 },
                    { "top_f_45", 100 },
                    { "top_f_46", 100 },
                    { "top_f_47", 100 },
                    { "top_f_48", 100 },
                    { "top_f_49", 100 },
                    { "top_f_50", 100 },
                    { "top_f_51", 100 },
                    { "top_f_52", 100 },
                    { "top_f_53", 100 },
                    { "top_f_54", 100 },
                    { "top_f_55", 100 },
                    { "top_f_56", 100 },
                    { "top_f_57", 100 },
                    { "top_f_58", 100 },
                    { "top_f_59", 100 },
                    { "top_f_60", 100 },
                    { "top_f_61", 100 },
                    { "top_f_62", 100 },
                    { "top_f_63", 100 },
                    { "top_f_64", 100 },
                    { "top_f_65", 100 },
                    { "top_f_66", 100 },
                    { "top_f_68", 100 },
                    { "top_f_69", 100 },
                    { "top_f_70", 100 },
                    { "top_f_71", 100 },
                    { "top_f_72", 100 },
                    { "top_f_73", 100 },
                    { "top_f_74", 100 },
                    { "top_f_75", 100 },
                    { "top_f_76", 100 },
                    { "top_f_77", 100 },
                    { "top_f_78", 100 },
                    { "top_f_79", 100 },
                    { "top_f_80", 100 },
                    { "top_f_81", 100 },
                    { "top_f_82", 100 },
                    { "top_f_83", 100 },
                    { "top_f_84", 100 },
                    { "top_f_85", 100 },
                    { "top_f_86", 100 },
                    { "top_f_87", 100 },
                    { "top_f_88", 100 },
                    { "top_f_89", 100 },
                    { "top_f_90", 100 },
                    { "top_f_91", 100 },
                    { "top_f_92", 100 },
                    { "top_f_93", 100 },
                    { "top_f_94", 100 },
                    { "top_f_95", 100 },
                    { "top_f_97", 100 },
                    { "top_f_98", 100 },
                    { "top_f_99", 100 },
                    { "top_f_100", 100 },
                    { "top_f_101", 100 },
                    { "top_f_102", 100 },
                    { "top_f_103", 100 },
                    { "top_f_104", 100 },
                    { "top_f_105", 100 },
                    { "top_f_106", 100 },
                    { "top_f_107", 100 },
                    { "top_f_108", 100 },
                    { "top_f_109", 100 },
                    { "top_f_110", 100 },
                    { "top_f_111", 100 },
                    { "top_f_112", 100 },
                    { "top_f_113", 100 },
                    { "top_f_114", 100 },
                    { "top_f_115", 100 },
                    { "top_f_117", 100 },
                    { "top_f_118", 100 },
                    { "top_f_119", 100 },
                    { "top_f_120", 100 },
                    { "top_f_121", 100 },
                    { "top_f_122", 100 },
                    { "top_f_123", 100 },
                    { "top_f_124", 100 },
                    { "top_f_125", 100 },
                    { "top_f_126", 100 },
                    { "top_f_127", 100 },
                    { "top_f_128", 100 },
                    { "top_f_129", 100 },
                    { "top_f_130", 100 },
                    { "top_f_131", 100 },
                    { "top_f_132", 100 },
                    { "top_f_133", 100 },
                    { "top_f_134", 100 },
                    { "top_f_135", 100 },
                    { "top_f_137", 100 },
                    { "top_f_138", 100 },
                    { "top_f_139", 100 },

                    { "under_m_0", 100 },
                    { "under_m_1", 100 },
                    { "under_m_2", 100 },
                    { "under_m_3", 100 },
                    { "under_m_4", 100 },
                    { "under_m_5", 100 },
                    { "under_m_6", 100 },
                    { "under_m_7", 100 },
                    { "under_m_8", 100 },
                    { "under_m_9", 100 },
                    { "under_m_10", 100 },
                    { "under_m_11", 100 },
                    { "under_m_12", 100 },
                    { "under_m_13", 100 },
                    { "under_m_14", 100 },
                    { "under_m_15", 100 },
                    { "under_m_16", 100 },
                    { "under_m_17", 100 },
                    { "under_m_18", 100 },
                    { "under_m_19", 100 },
                    { "under_m_20", 100 },
                    { "under_m_21", 100 },
                    { "under_m_22", 100 },
                    { "under_m_23", 100 },
                    { "under_m_24", 100 },
                    { "under_m_25", 100 },
                    { "under_m_26", 100 },

                    { "under_f_0", 100 },
                    { "under_f_1", 100 },
                    { "under_f_2", 100 },
                    { "under_f_3", 100 },
                    { "under_f_4", 100 },
                    { "under_f_5", 100 },
                    { "under_f_6", 100 },
                    { "under_f_7", 100 },
                    { "under_f_8", 100 },
                    { "under_f_9", 100 },
                    { "under_f_10", 100 },
                    { "under_f_11", 100 },
                    { "under_f_12", 100 },
                    { "under_f_13", 100 },
                    { "under_f_14", 100 },
                    { "under_f_15", 100 },
                    { "under_f_16", 100 },
                    { "under_f_17", 100 },
                    { "under_f_18", 100 },
                    { "under_f_19", 100 },

                    { "pants_m_0", 100 },
                    { "pants_m_1", 100 },
                    { "pants_m_2", 100 },
                    { "pants_m_3", 100 },
                    { "pants_m_4", 100 },
                    { "pants_m_5", 100 },
                    { "pants_m_6", 100 },
                    { "pants_m_7", 100 },
                    { "pants_m_8", 100 },
                    { "pants_m_9", 100 },
                    { "pants_m_10", 100 },
                    { "pants_m_11", 100 },
                    { "pants_m_12", 100 },
                    { "pants_m_13", 100 },
                    { "pants_m_14", 100 },
                    { "pants_m_15", 100 },
                    { "pants_m_16", 100 },
                    { "pants_m_17", 100 },
                    { "pants_m_18", 100 },
                    { "pants_m_19", 100 },
                    { "pants_m_20", 100 },
                    { "pants_m_21", 100 },
                    { "pants_m_22", 100 },
                    { "pants_m_23", 100 },
                    { "pants_m_24", 100 },
                    { "pants_m_25", 100 },
                    { "pants_m_26", 100 },
                    { "pants_m_27", 100 },
                    { "pants_m_28", 100 },
                    { "pants_m_29", 100 },
                    { "pants_m_30", 100 },
                    { "pants_m_31", 100 },
                    { "pants_m_32", 100 },
                    { "pants_m_33", 100 },
                    { "pants_m_34", 100 },
                    { "pants_m_35", 100 },
                    { "pants_m_36", 100 },
                    { "pants_m_37", 100 },
                    { "pants_m_38", 100 },
                    { "pants_m_39", 100 },
                    { "pants_m_40", 100 },
                    { "pants_m_41", 100 },
                    { "pants_m_42", 100 },
                    { "pants_m_43", 100 },
                    { "pants_m_44", 100 },

                    { "pants_f_0", 100 },
                    { "pants_f_1", 100 },
                    { "pants_f_2", 100 },
                    { "pants_f_3", 100 },
                    { "pants_f_4", 100 },
                    { "pants_f_5", 100 },
                    { "pants_f_6", 100 },
                    { "pants_f_7", 100 },
                    { "pants_f_8", 100 },
                    { "pants_f_9", 100 },
                    { "pants_f_10", 100 },
                    { "pants_f_11", 100 },
                    { "pants_f_12", 100 },
                    { "pants_f_13", 100 },
                    { "pants_f_14", 100 },
                    { "pants_f_15", 100 },
                    { "pants_f_16", 100 },
                    { "pants_f_17", 100 },
                    { "pants_f_18", 100 },
                    { "pants_f_19", 100 },
                    { "pants_f_20", 100 },
                    { "pants_f_21", 100 },
                    { "pants_f_22", 100 },
                    { "pants_f_23", 100 },
                    { "pants_f_24", 100 },
                    { "pants_f_25", 100 },
                    { "pants_f_26", 100 },
                    { "pants_f_27", 100 },
                    { "pants_f_28", 100 },
                    { "pants_f_29", 100 },
                    { "pants_f_30", 100 },
                    { "pants_f_31", 100 },
                    { "pants_f_32", 100 },
                    { "pants_f_33", 100 },
                    { "pants_f_34", 100 },
                    { "pants_f_35", 100 },
                    { "pants_f_36", 100 },
                    { "pants_f_37", 100 },
                    { "pants_f_38", 100 },
                    { "pants_f_39", 100 },
                    { "pants_f_40", 100 },
                    { "pants_f_41", 100 },
                    { "pants_f_42", 100 },
                    { "pants_f_43", 100 },
                    { "pants_f_44", 100 },
                    { "pants_f_45", 100 },
                    { "pants_f_46", 100 },
                    { "pants_f_47", 100 },
                    { "pants_f_48", 100 },

                    { "shoes_m_0", 100 },
                    { "shoes_m_1", 100 },
                    { "shoes_m_2", 100 },
                    { "shoes_m_3", 100 },
                    { "shoes_m_4", 100 },
                    { "shoes_m_5", 100 },
                    { "shoes_m_6", 100 },
                    { "shoes_m_7", 100 },
                    { "shoes_m_8", 100 },
                    { "shoes_m_9", 100 },
                    { "shoes_m_10", 100 },
                    { "shoes_m_11", 100 },
                    { "shoes_m_12", 100 },
                    { "shoes_m_13", 100 },
                    { "shoes_m_14", 100 },
                    { "shoes_m_15", 100 },
                    { "shoes_m_16", 100 },
                    { "shoes_m_17", 100 },
                    { "shoes_m_18", 100 },
                    { "shoes_m_19", 100 },
                    { "shoes_m_20", 100 },
                    { "shoes_m_21", 100 },
                    { "shoes_m_22", 100 },
                    { "shoes_m_23", 100 },
                    { "shoes_m_24", 100 },
                    { "shoes_m_25", 100 },
                    { "shoes_m_26", 100 },
                    { "shoes_m_27", 100 },
                    { "shoes_m_28", 100 },
                    { "shoes_m_29", 100 },
                    { "shoes_m_30", 100 },
                    { "shoes_m_31", 100 },
                    { "shoes_m_32", 100 },
                    { "shoes_m_33", 100 },
                    { "shoes_m_34", 100 },
                    { "shoes_m_35", 100 },
                    { "shoes_m_36", 100 },
                    { "shoes_m_37", 100 },
                    { "shoes_m_38", 100 },
                    { "shoes_m_39", 100 },
                    { "shoes_m_40", 100 },
                    { "shoes_m_41", 100 },
                    { "shoes_m_42", 100 },
                    { "shoes_m_43", 100 },
                    { "shoes_m_44", 100 },
                    { "shoes_m_45", 100 },
                    { "shoes_m_46", 100 },
                    { "shoes_m_47", 100 },

                    { "shoes_f_0", 100 },
                    { "shoes_f_1", 100 },
                    { "shoes_f_2", 100 },
                    { "shoes_f_3", 100 },
                    { "shoes_f_4", 100 },
                    { "shoes_f_5", 100 },
                    { "shoes_f_6", 100 },
                    { "shoes_f_7", 100 },
                    { "shoes_f_8", 100 },
                    { "shoes_f_9", 100 },
                    { "shoes_f_10", 100 },
                    { "shoes_f_11", 100 },
                    { "shoes_f_12", 100 },
                    { "shoes_f_13", 100 },
                    { "shoes_f_14", 100 },
                    { "shoes_f_15", 100 },
                    { "shoes_f_16", 100 },
                    { "shoes_f_17", 100 },
                    { "shoes_f_18", 100 },
                    { "shoes_f_19", 100 },
                    { "shoes_f_20", 100 },
                    { "shoes_f_21", 100 },
                    { "shoes_f_22", 100 },
                    { "shoes_f_23", 100 },
                    { "shoes_f_24", 100 },
                    { "shoes_f_25", 100 },
                    { "shoes_f_26", 100 },
                    { "shoes_f_27", 100 },
                    { "shoes_f_28", 100 },
                    { "shoes_f_29", 100 },
                    { "shoes_f_30", 100 },
                    { "shoes_f_31", 100 },
                    { "shoes_f_32", 100 },
                    { "shoes_f_33", 100 },
                    { "shoes_f_34", 100 },
                    { "shoes_f_35", 100 },
                    { "shoes_f_36", 100 },
                    { "shoes_f_37", 100 },
                    { "shoes_f_38", 100 },
                    { "shoes_f_39", 100 },
                    { "shoes_f_40", 100 },
                    { "shoes_f_41", 100 },
                    { "shoes_f_42", 100 },
                    { "shoes_f_43", 100 },
                    { "shoes_f_44", 100 },
                    { "shoes_f_45", 100 },
                    { "shoes_f_46", 100 },

                    { "hat_m_0", 100 },
                    { "hat_m_1", 100 },
                    { "hat_m_2", 100 },
                    { "hat_m_3", 100 },
                    { "hat_m_4", 100 },
                    { "hat_m_5", 100 },
                    { "hat_m_6", 100 },
                    { "hat_m_7", 100 },
                    { "hat_m_8", 100 },
                    { "hat_m_9", 100 },
                    { "hat_m_10", 100 },
                    { "hat_m_11", 100 },
                    { "hat_m_12", 100 },
                    { "hat_m_13", 100 },
                    { "hat_m_14", 100 },
                    { "hat_m_15", 100 },
                    { "hat_m_16", 100 },
                    { "hat_m_17", 100 },
                    { "hat_m_18", 100 },
                    { "hat_m_19", 100 },
                    { "hat_m_20", 100 },
                    { "hat_m_21", 100 },
                    { "hat_m_22", 100 },
                    { "hat_m_23", 100 },
                    { "hat_m_24", 100 },
                    { "hat_m_25", 100 },
                    { "hat_m_26", 100 },
                    { "hat_m_27", 100 },
                    { "hat_m_28", 100 },
                    { "hat_m_29", 100 },
                    { "hat_m_30", 100 },
                    { "hat_m_31", 100 },
                    { "hat_m_32", 100 },
                    { "hat_m_33", 100 },
                    { "hat_m_34", 100 },
                    { "hat_m_35", 100 },
                    { "hat_m_36", 100 },
                    { "hat_m_37", 100 },

                    { "hat_f_0", 100 },
                    { "hat_f_1", 100 },
                    { "hat_f_2", 100 },
                    { "hat_f_3", 100 },
                    { "hat_f_4", 100 },
                    { "hat_f_5", 100 },
                    { "hat_f_6", 100 },
                    { "hat_f_7", 100 },
                    { "hat_f_8", 100 },
                    { "hat_f_9", 100 },
                    { "hat_f_10", 100 },
                    { "hat_f_11", 100 },
                    { "hat_f_12", 100 },
                    { "hat_f_13", 100 },
                    { "hat_f_14", 100 },
                    { "hat_f_15", 100 },
                    { "hat_f_16", 100 },
                    { "hat_f_17", 100 },
                    { "hat_f_18", 100 },
                    { "hat_f_19", 100 },
                    { "hat_f_20", 100 },
                    { "hat_f_21", 100 },
                    { "hat_f_22", 100 },
                    { "hat_f_23", 100 },
                    { "hat_f_24", 100 },
                    { "hat_f_25", 100 },
                    { "hat_f_26", 100 },
                    { "hat_f_27", 100 },
                    { "hat_f_28", 100 },
                    { "hat_f_29", 100 },
                    { "hat_f_30", 100 },
                    { "hat_f_31", 100 },
                    { "hat_f_32", 100 },
                    { "hat_f_33", 100 },
                    { "hat_f_34", 100 },
                    { "hat_f_35", 100 },
                    { "hat_f_36", 100 },

                    { "accs_m_2", 100 },
                    { "accs_m_8", 100 },
                    { "accs_m_9", 100 },
                    { "accs_m_11", 100 },
                    { "accs_m_12", 100 },

                    { "accs_f_0", 100 },
                    { "accs_f_1", 100 },
                    { "accs_f_2", 100 },
                    { "accs_f_3", 100 },
                    { "accs_f_4", 100 },
                    { "accs_f_5", 100 },

                    { "watches_m_0", 100 },
                    { "watches_m_1", 100 },
                    { "watches_m_2", 100 },
                    { "watches_m_3", 100 },
                    { "watches_m_4", 100 },
                    { "watches_m_5", 100 },
                    { "watches_m_6", 100 },
                    { "watches_m_7", 100 },
                    { "watches_m_8", 100 },
                    { "watches_m_9", 100 },
                    { "watches_m_10", 100 },
                    { "watches_m_11", 100 },

                    { "watches_f_0", 100 },
                    { "watches_f_1", 100 },
                    { "watches_f_2", 100 },
                    { "watches_f_3", 100 },
                    { "watches_f_4", 100 },
                    { "watches_f_5", 100 },
                    { "watches_f_6", 100 },
                    { "watches_f_7", 100 },

                    { "glasses_m_0", 100 },
                    { "glasses_m_1", 100 },
                    { "glasses_m_2", 100 },
                    { "glasses_m_3", 100 },
                    { "glasses_m_4", 100 },
                    { "glasses_m_5", 100 },
                    { "glasses_m_6", 100 },
                    { "glasses_m_7", 100 },
                    { "glasses_m_8", 100 },
                    { "glasses_m_9", 100 },

                    { "glasses_f_0", 100 },
                    { "glasses_f_1", 100 },
                    { "glasses_f_2", 100 },
                    { "glasses_f_3", 100 },
                    { "glasses_f_4", 100 },
                    { "glasses_f_5", 100 },
                    { "glasses_f_6", 100 },
                    { "glasses_f_7", 100 },

                    { "gloves_m_0", 100 },
                    { "gloves_m_1", 100 },
                    { "gloves_m_2", 100 },
                    { "gloves_m_3", 100 },
                    { "gloves_m_4", 100 },

                    { "gloves_f_0", 100 },
                    { "gloves_f_1", 100 },
                    { "gloves_f_2", 100 },
                    { "gloves_f_3", 100 },
                    { "gloves_f_4", 100 },

                    { "bracelet_m_6", 100 },
                    { "bracelet_m_7", 100 },
                    { "bracelet_m_8", 100 },

                    { "bracelet_f_13", 100 },
                    { "bracelet_f_14", 100 },
                    { "bracelet_f_15", 100 },
                } }
            },
            #endregion

            #region ClothesShop2
            {
                Types.ClothesShop2,

                new MaterialsData(5, 7, 50) { Prices = new Dictionary<string, int>()
                {
                    { "top_m_134", 100},
                    { "top_m_135", 100},
                    { "top_m_136", 100},
                    { "top_m_137", 100},
                    { "top_m_138", 100},
                    { "top_m_139", 100},
                    { "top_m_140", 100},
                    { "top_m_141", 100},
                    { "top_m_142", 100},
                    { "top_m_143", 100},
                    { "top_m_144", 100},
                    { "top_m_145", 100},
                    { "top_m_146", 100},
                    { "top_m_147", 100},
                    { "top_m_148", 100},
                    { "top_m_149", 100},
                    { "top_m_150", 100},
                    { "top_m_151", 100},
                    { "top_m_152", 100},
                    { "top_m_153", 100},
                    { "top_m_154", 100},
                    { "top_m_155", 100},
                    { "top_m_156", 100},
                    { "top_m_157", 100},
                    { "top_m_158", 100},
                    { "top_m_159", 100},
                    { "top_m_160", 100},
                    { "top_m_161", 100},
                    { "top_m_162", 100},
                    { "top_m_163", 100},
                    { "top_m_164", 100},
                    { "top_m_165", 100},
                    { "top_m_166", 100},
                    { "top_m_167", 100},
                    { "top_m_168", 100},
                    { "top_m_170", 100},
                    { "top_m_171", 100},
                    { "top_m_172", 100},
                    { "top_m_173", 100},
                    { "top_m_174", 100},
                    { "top_m_175", 100},
                    { "top_m_176", 100},
                    { "top_m_177", 100},
                    { "top_m_178", 100},
                    { "top_m_179", 100},
                    { "top_m_180", 100},
                    { "top_m_181", 100},
                    { "top_m_182", 100},
                    { "top_m_183", 100},
                    { "top_m_184", 100},
                    { "top_m_185", 100},
                    { "top_m_186", 100},
                    { "top_m_187", 100},
                    { "top_m_188", 100},
                    { "top_m_189", 100},
                    { "top_m_190", 100},
                    { "top_m_191", 100},
                    { "top_m_192", 100},
                    { "top_m_193", 100},
                    { "top_m_194", 100},
                    { "top_m_195", 100},
                    { "top_m_196", 100},
                    { "top_m_197", 100},
                    { "top_m_198", 100},

                    { "top_f_140", 100 },
                    { "top_f_141", 100 },
                    { "top_f_142", 100 },
                    { "top_f_143", 100 },
                    { "top_f_144", 100 },
                    { "top_f_145", 100 },
                    { "top_f_146", 100 },
                    { "top_f_147", 100 },
                    { "top_f_148", 100 },
                    { "top_f_149", 100 },
                    { "top_f_150", 100 },
                    { "top_f_151", 100 },
                    { "top_f_152", 100 },
                    { "top_f_153", 100 },
                    { "top_f_154", 100 },
                    { "top_f_155", 100 },
                    { "top_f_156", 100 },
                    { "top_f_157", 100 },
                    { "top_f_158", 100 },
                    { "top_f_159", 100 },
                    { "top_f_160", 100 },
                    { "top_f_161", 100 },
                    { "top_f_162", 100 },
                    { "top_f_163", 100 },
                    { "top_f_164", 100 },
                    { "top_f_165", 100 },
                    { "top_f_166", 100 },
                    { "top_f_167", 100 },
                    { "top_f_168", 100 },
                    { "top_f_169", 100 },
                    { "top_f_170", 100 },
                    { "top_f_171", 100 },
                    { "top_f_172", 100 },
                    { "top_f_173", 100 },
                    { "top_f_174", 100 },
                    { "top_f_175", 100 },
                    { "top_f_176", 100 },
                    { "top_f_177", 100 },
                    { "top_f_178", 100 },
                    { "top_f_179", 100 },
                    { "top_f_180", 100 },
                    { "top_f_181", 100 },
                    { "top_f_182", 100 },
                    { "top_f_183", 100 },
                    { "top_f_184", 100 },
                    { "top_f_185", 100 },
                    { "top_f_186", 100 },
                    { "top_f_187", 100 },
                    { "top_f_188", 100 },
                    { "top_f_189", 100 },
                    { "top_f_191", 100 },
                    { "top_f_192", 100 },
                    { "top_f_193", 100 },
                    { "top_f_194", 100 },
                    { "top_f_195", 100 },
                    { "top_f_196", 100 },
                    { "top_f_197", 100 },
                    { "top_f_198", 100 },
                    { "top_f_199", 100 },
                    { "top_f_200", 100 },
                    { "top_f_201", 100 },
                    { "top_f_202", 100 },
                    { "top_f_203", 100 },
                    { "top_f_204", 100 },
                    { "top_f_205", 100 },
                    { "top_f_206", 100 },
                    { "top_f_207", 100 },
                    { "top_f_208", 100 },
                    { "top_f_209", 100 },
                    { "top_f_210", 100 },

                    { "under_m_27", 100 },
                    { "under_m_28", 100 },
                    { "under_m_29", 100 },
                    { "under_m_30", 100 },
                    { "under_m_31", 100 },
                    { "under_m_32", 100 },
                    { "under_m_33", 100 },
                    { "under_m_34", 100 },
                    { "under_m_35", 100 },

                    { "under_f_20", 100 },
                    { "under_f_21", 100 },
                    { "under_f_22", 100 },
                    { "under_f_23", 100 },
                    { "under_f_24", 100 },

                    { "pants_m_45", 100 },
                    { "pants_m_46", 100 },
                    { "pants_m_47", 100 },
                    { "pants_m_48", 100 },
                    { "pants_m_49", 100 },
                    { "pants_m_50", 100 },
                    { "pants_m_51", 100 },
                    { "pants_m_52", 100 },
                    { "pants_m_53", 100 },
                    { "pants_m_54", 100 },
                    { "pants_m_55", 100 },
                    { "pants_m_56", 100 },
                    { "pants_m_57", 100 },
                    { "pants_m_58", 100 },
                    { "pants_m_59", 100 },
                    { "pants_m_60", 100 },
                    { "pants_m_61", 100 },
                    { "pants_m_62", 100 },
                    { "pants_m_63", 100 },
                    { "pants_m_64", 100 },
                    { "pants_m_65", 100 },
                    { "pants_m_66", 100 },
                    { "pants_m_67", 100 },
                    { "pants_m_68", 100 },
                    { "pants_m_69", 100 },
                    { "pants_m_70", 100 },
                    { "pants_m_71", 100 },
                    { "pants_m_72", 100 },
                    { "pants_m_73", 100 },
                    { "pants_m_74", 100 },
                    { "pants_m_75", 100 },

                    { "pants_f_49", 100 },
                    { "pants_f_50", 100 },
                    { "pants_f_51", 100 },
                    { "pants_f_52", 100 },
                    { "pants_f_53", 100 },
                    { "pants_f_54", 100 },
                    { "pants_f_55", 100 },
                    { "pants_f_56", 100 },
                    { "pants_f_57", 100 },
                    { "pants_f_58", 100 },
                    { "pants_f_59", 100 },
                    { "pants_f_60", 100 },
                    { "pants_f_61", 100 },
                    { "pants_f_62", 100 },
                    { "pants_f_63", 100 },
                    { "pants_f_64", 100 },
                    { "pants_f_65", 100 },
                    { "pants_f_66", 100 },
                    { "pants_f_67", 100 },
                    { "pants_f_68", 100 },
                    { "pants_f_69", 100 },
                    { "pants_f_70", 100 },
                    { "pants_f_71", 100 },
                    { "pants_f_72", 100 },
                    { "pants_f_73", 100 },
                    { "pants_f_74", 100 },
                    { "pants_f_75", 100 },
                    { "pants_f_76", 100 },
                    { "pants_f_77", 100 },
                    { "pants_f_78", 100 },
                    { "pants_f_79", 100 },
                    { "pants_f_80", 100 },
                    { "pants_f_81", 100 },
                    { "pants_f_82", 100 },
                    { "pants_f_83", 100 },
                    { "pants_f_84", 100 },

                    { "shoes_m_48", 100 },
                    { "shoes_m_49", 100 },
                    { "shoes_m_50", 100 },
                    { "shoes_m_51", 100 },
                    { "shoes_m_52", 100 },
                    { "shoes_m_53", 100 },
                    { "shoes_m_54", 100 },
                    { "shoes_m_55", 100 },
                    { "shoes_m_56", 100 },
                    { "shoes_m_57", 100 },
                    { "shoes_m_58", 100 },
                    { "shoes_m_59", 100 },
                    { "shoes_m_60", 100 },
                    { "shoes_m_61", 100 },
                    { "shoes_m_62", 100 },
                    { "shoes_m_63", 100 },
                    { "shoes_m_64", 100 },
                    { "shoes_m_65", 100 },
                    { "shoes_m_66", 100 },
                    { "shoes_m_67", 100 },
                    { "shoes_m_68", 100 },
                    { "shoes_m_69", 100 },
                    { "shoes_m_70", 100 },

                    { "shoes_f_47", 100 },
                    { "shoes_f_48", 100 },
                    { "shoes_f_49", 100 },
                    { "shoes_f_50", 100 },
                    { "shoes_f_51", 100 },
                    { "shoes_f_52", 100 },
                    { "shoes_f_53", 100 },
                    { "shoes_f_54", 100 },
                    { "shoes_f_55", 100 },
                    { "shoes_f_56", 100 },
                    { "shoes_f_57", 100 },
                    { "shoes_f_58", 100 },
                    { "shoes_f_59", 100 },
                    { "shoes_f_60", 100 },
                    { "shoes_f_61", 100 },
                    { "shoes_f_62", 100 },
                    { "shoes_f_63", 100 },
                    { "shoes_f_64", 100 },
                    { "shoes_f_65", 100 },
                    { "shoes_f_66", 100 },
                    { "shoes_f_67", 100 },
                    { "shoes_f_68", 100 },
                    { "shoes_f_69", 100 },
                    { "shoes_f_70", 100 },
                    { "shoes_f_71", 100 },
                    { "shoes_f_72", 100 },
                    { "shoes_f_73", 100 },
                    { "shoes_f_74", 100 },
                    { "shoes_f_75", 100 },
                    { "shoes_f_76", 100 },
                    { "shoes_f_77", 100 },
                    { "shoes_f_78", 100 },
                    { "shoes_f_79", 100 },

                    { "hat_m_38", 100 },
                    { "hat_m_39", 100 },
                    { "hat_m_40", 100 },
                    { "hat_m_41", 100 },
                    { "hat_m_42", 100 },
                    { "hat_m_43", 100 },
                    { "hat_m_44", 100 },
                    { "hat_m_45", 100 },
                    { "hat_m_46", 100 },
                    { "hat_m_47", 100 },
                    { "hat_m_48", 100 },
                    { "hat_m_49", 100 },
                    { "hat_m_50", 100 },
                    { "hat_m_51", 100 },
                    { "hat_m_52", 100 },
                    { "hat_m_53", 100 },
                    { "hat_m_54", 100 },

                    { "hat_f_37", 100 },
                    { "hat_f_38", 100 },
                    { "hat_f_39", 100 },
                    { "hat_f_40", 100 },
                    { "hat_f_41", 100 },
                    { "hat_f_42", 100 },
                    { "hat_f_43", 100 },
                    { "hat_f_44", 100 },
                    { "hat_f_45", 100 },
                    { "hat_f_46", 100 },
                    { "hat_f_47", 100 },
                    { "hat_f_48", 100 },
                    { "hat_f_49", 100 },

                    { "accs_m_13", 100 },
                    { "accs_m_14", 100 },
                    { "accs_m_15", 100 },
                    { "accs_m_16", 100 },
                    { "accs_m_17", 100 },
                    { "accs_m_18", 100 },
                    { "accs_m_19", 100 },
                    { "accs_m_20", 100 },
                    { "accs_m_21", 100 },
                    { "accs_m_22", 100 },

                    { "accs_f_7", 100 },
                    { "accs_f_8", 100 },
                    { "accs_f_9", 100 },
                    { "accs_f_10", 100 },
                    { "accs_f_11", 100 },

                    { "watches_m_12", 100 },
                    { "watches_m_13", 100 },
                    { "watches_m_14", 100 },
                    { "watches_m_15", 100 },
                    { "watches_m_16", 100 },
                    { "watches_m_17", 100 },
                    { "watches_m_18", 100 },
                    { "watches_m_19", 100 },
                    { "watches_m_20", 100 },
                    { "watches_m_21", 100 },
                    { "watches_m_22", 100 },
                    { "watches_m_23", 100 },
                    { "watches_m_24", 100 },
                    { "watches_m_25", 100 },

                    { "watches_f_8", 100 },
                    { "watches_f_9", 100 },
                    { "watches_f_10", 100 },
                    { "watches_f_11", 100 },
                    { "watches_f_12", 100 },
                    { "watches_f_13", 100 },
                    { "watches_f_14", 100 },
                    { "watches_f_15", 100 },

                    { "glasses_m_10", 100 },
                    { "glasses_m_11", 100 },
                    { "glasses_m_12", 100 },
                    { "glasses_m_13", 100 },
                    { "glasses_m_14", 100 },
                    { "glasses_m_15", 100 },
                    { "glasses_m_16", 100 },
                    { "glasses_m_17", 100 },
                    { "glasses_m_18", 100 },
                    { "glasses_m_19", 100 },
                    { "glasses_m_20", 100 },
                    { "glasses_m_21", 100 },
                    { "glasses_m_22", 100 },
                    { "glasses_m_23", 100 },

                    { "glasses_f_8", 100 },
                    { "glasses_f_9", 100 },
                    { "glasses_f_10", 100 },
                    { "glasses_f_11", 100 },
                    { "glasses_f_12", 100 },
                    { "glasses_f_13", 100 },
                    { "glasses_f_14", 100 },
                    { "glasses_f_15", 100 },
                    { "glasses_f_16", 100 },
                    { "glasses_f_17", 100 },
                    { "glasses_f_18", 100 },
                    { "glasses_f_19", 100 },
                    { "glasses_f_20", 100 },
                    { "glasses_f_21", 100 },
                    { "glasses_f_22", 100 },
                    { "glasses_f_23", 100 },
                    { "glasses_f_24", 100 },
                    { "glasses_f_25", 100 },

                    { "gloves_m_5", 100 },
                    { "gloves_m_6", 100 },
                    { "gloves_m_7", 100 },
                    { "gloves_m_8", 100 },

                    { "gloves_f_5", 100 },
                    { "gloves_f_6", 100 },
                    { "gloves_f_7", 100 },
                    { "gloves_f_8", 100 },

                    { "bracelet_m_0", 100 },
                    { "bracelet_m_1", 100 },
                    { "bracelet_m_2", 100 },
                    { "bracelet_m_3", 100 },
                    { "bracelet_m_4", 100 },
                    { "bracelet_m_5", 100 },

                    { "bracelet_f_0", 100 },
                    { "bracelet_f_1", 100 },
                    { "bracelet_f_2", 100 },
                    { "bracelet_f_3", 100 },
                    { "bracelet_f_4", 100 },
                    { "bracelet_f_5", 100 },
                    { "bracelet_f_6", 100 },
                    { "bracelet_f_7", 100 },
                    { "bracelet_f_8", 100 },
                    { "bracelet_f_9", 100 },
                    { "bracelet_f_10", 100 },
                    { "bracelet_f_11", 100 },
                    { "bracelet_f_12", 100 },
                } }
            },
            #endregion

            {
                Types.JewelleryShop,

                new MaterialsData(5, 7, 50) { Prices = new Dictionary<string, int>()
                {
                    { "ring_m_0", 10 },
                    { "ring_m_1", 10 },

                    { "ears_m_0", 10 },
                    { "ears_m_1", 10 },
                    { "ears_m_2", 10 },
                    { "ears_m_3", 10 },
                    { "ears_m_4", 10 },
                    { "ears_m_5", 10 },
                    { "ears_m_6", 10 },
                    { "ears_m_7", 10 },
                    { "ears_m_8", 10 },
                    { "ears_m_9", 10 },
                    { "ears_m_10", 10 },
                    { "ears_m_11", 10 },
                    { "ears_m_12", 10 },
                    { "ears_m_13", 10 },
                    { "ears_m_14", 10 },
                    { "ears_m_15", 10 },
                    { "ears_m_16", 10 },
                    { "ears_m_17", 10 },
                    { "ears_m_18", 10 },
                    { "ears_m_19", 10 },
                    { "ears_m_20", 10 },
                    { "ears_m_21", 10 },
                    { "ears_m_22", 10 },
                    { "ears_m_23", 10 },
                    { "ears_m_24", 10 },
                    { "ears_m_25", 10 },
                    { "ears_m_26", 10 },
                    { "ears_m_27", 10 },
                    { "ears_m_28", 10 },
                    { "ears_m_29", 10 },
                    { "ears_m_30", 10 },
                    { "ears_m_31", 10 },
                    { "ears_m_32", 10 },
                    { "ears_m_33", 10 },
                    { "ears_m_34", 10 },
                    { "ears_m_35", 10 },
                    { "ears_m_36", 10 },

                    { "accs_m_0", 10 },
                    { "accs_m_1", 10 },
                    { "accs_m_3", 10 },
                    { "accs_m_4", 10 },
                    { "accs_m_5", 10 },
                    { "accs_m_6", 10 },
                    { "accs_m_7", 00 },
                    { "accs_m_10", 10 },

                    { "ring_f_0", 10 },
                    { "ring_f_1", 10 },

                    { "ears_f_0", 10 },
                    { "ears_f_1", 10 },
                    { "ears_f_2", 10 },
                    { "ears_f_3", 10 },
                    { "ears_f_4", 10 },
                    { "ears_f_5", 10 },
                    { "ears_f_6", 10 },
                    { "ears_f_7", 10 },
                    { "ears_f_8", 10 },
                    { "ears_f_9", 10 },
                    { "ears_f_10", 10 },
                    { "ears_f_11", 10 },
                    { "ears_f_12", 10 },
                    { "ears_f_13", 10 },
                    { "ears_f_14", 10 },
                    { "ears_f_15", 10 },
                    { "ears_f_16", 10 },
                    { "ears_f_17", 10 },

                    { "accs_f_6", 10 },
                } }
            },

            {
                Types.MaskShop,

                new MaterialsData(5, 7, 50) { Prices = new Dictionary<string, int>()
                {
                    { "mask_m_0", 10 },
                    { "mask_m_1", 10 },

                    { "mask_f_0", 10 },
                    { "mask_f_1", 10 },
                } }
            },

            {
                Types.BagShop,

                new MaterialsData(5, 7, 50) { Prices = new Dictionary<string, int>()
                {
                    { "bag_m_0", 10 },

                    { "bag_f_0", 10 },
                } }
            },

            {
                Types.BarberShop,

                new MaterialsData(5, 7, 50) { Prices = new Dictionary<string, int>()
                {
                    { "hair_m_0", 10 },
                    { "hair_m_1", 10 },
                    { "hair_m_2", 10 },
                    { "hair_m_3", 10 },
                    { "hair_m_4", 10 },
                    { "hair_m_5", 10 },
                    { "hair_m_6", 10 },
                    { "hair_m_7", 10 },
                    { "hair_m_8", 10 },
                    { "hair_m_9", 10 },
                    { "hair_m_10", 10 },
                    { "hair_m_11", 10 },
                    { "hair_m_12", 10 },
                    { "hair_m_13", 10 },
                    { "hair_m_14", 10 },
                    { "hair_m_15", 10 },
                    { "hair_m_16", 10 },
                    { "hair_m_17", 10 },
                    { "hair_m_18", 10 },
                    { "hair_m_19", 10 },
                    { "hair_m_20", 10 },
                    { "hair_m_21", 10 },
                    { "hair_m_22", 10 },
                    { "hair_m_23", 10 },
                    { "hair_m_24", 10 },
                    { "hair_m_25", 10 },
                    { "hair_m_26", 10 },
                    { "hair_m_27", 10 },
                    { "hair_m_28", 10 },
                    { "hair_m_29", 10 },
                    { "hair_m_30", 10 },
                    { "hair_m_31", 10 },
                    { "hair_m_32", 10 },
                    { "hair_m_33", 10 },
                    { "hair_m_34", 10 },
                    { "hair_m_35", 10 },
                    { "hair_m_36", 10 },
                    { "hair_m_37", 10 },
                    { "hair_m_38", 10 },
                    { "hair_m_39", 10 },
                    { "hair_m_40", 10 },

                    { "hair_f_0", 10 },
                    { "hair_f_1", 10 },
                    { "hair_f_2", 10 },
                    { "hair_f_3", 10 },
                    { "hair_f_4", 10 },
                    { "hair_f_5", 10 },
                    { "hair_f_6", 10 },
                    { "hair_f_7", 10 },
                    { "hair_f_8", 10 },
                    { "hair_f_9", 10 },
                    { "hair_f_10", 10 },
                    { "hair_f_11", 10 },
                    { "hair_f_12", 10 },
                    { "hair_f_13", 10 },
                    { "hair_f_14", 10 },
                    { "hair_f_15", 10 },
                    { "hair_f_16", 10 },
                    { "hair_f_17", 10 },
                    { "hair_f_18", 10 },
                    { "hair_f_19", 10 },
                    { "hair_f_20", 10 },
                    { "hair_f_21", 10 },
                    { "hair_f_22", 10 },
                    { "hair_f_23", 10 },
                    { "hair_f_24", 10 },
                    { "hair_f_25", 10 },
                    { "hair_f_26", 10 },
                    { "hair_f_27", 10 },
                    { "hair_f_28", 10 },
                    { "hair_f_29", 10 },
                    { "hair_f_30", 10 },
                    { "hair_f_31", 10 },
                    { "hair_f_32", 10 },
                    { "hair_f_33", 10 },
                    { "hair_f_34", 10 },
                    { "hair_f_35", 10 },
                    { "hair_f_36", 10 },
                    { "hair_f_37", 10 },
                    { "hair_f_38", 10 },
                    { "hair_f_39", 10 },
                    { "hair_f_40", 10 },
                    { "hair_f_41", 10 },
                    { "hair_f_42", 10 },

                    { "beard_255", 10 },
                    { "beard_0", 10 },
                    { "beard_1", 10 },
                    { "beard_2", 10 },
                    { "beard_3", 10 },
                    { "beard_4", 10 },
                    { "beard_5", 10 },
                    { "beard_6", 10 },
                    { "beard_7", 10 },
                    { "beard_8", 10 },
                    { "beard_9", 10 },
                    { "beard_10", 10 },
                    { "beard_11", 10 },
                    { "beard_12", 10 },
                    { "beard_13", 10 },
                    { "beard_14", 10 },
                    { "beard_15", 10 },
                    { "beard_16", 10 },
                    { "beard_17", 10 },
                    { "beard_18", 10 },
                    { "beard_19", 10 },
                    { "beard_20", 10 },
                    { "beard_21", 10 },
                    { "beard_22", 10 },
                    { "beard_23", 10 },
                    { "beard_24", 10 },
                    { "beard_25", 10 },
                    { "beard_26", 10 },
                    { "beard_27", 10 },

                    { "chest_255", 10 },
                    { "chest", 10 },

                    { "eyebrows_255", 10 },
                    { "eyebrows", 10 },

                    { "makeup_255", 10 },
                    { "makeup", 10 },

                    { "blush_255", 10 },
                    { "blush", 10 },

                    { "lipstick_255", 10 },
                    { "lipstick", 10 },
                } }
            },

            {
                Types.TattooShop,

                new MaterialsData(5, 7, 50) { Prices = new Dictionary<string, int>()
                {
                    { "tat_-1_0", 100 },
                    { "tat_-1_1", 100 },
                    { "tat_-1_2", 100 },
                    { "tat_-1_3", 100 },
                    { "tat_-1_4", 100 },
                    { "tat_-1_5", 100 },
                    { "tat_-1_6", 100 },
                    { "tat_-1_7", 100 },
                    { "tat_-1_8", 100 },
                    { "tat_-1_9", 100 },
                    { "tat_-1_10", 100 },

                    { "tat_0", 10 },
                    { "tat_1", 10 },
                    { "tat_2", 10 },
                    { "tat_3", 10 },
                    { "tat_4", 10 },
                    { "tat_5", 10 },
                    { "tat_6", 10 },
                    { "tat_7", 10 },
                    { "tat_8", 10 },
                    { "tat_9", 10 },
                    { "tat_10", 10 },
                    { "tat_11", 10 },
                    { "tat_12", 10 },
                    { "tat_13", 10 },
                    { "tat_14", 10 },
                    { "tat_15", 10 },
                    { "tat_16", 10 },
                    { "tat_17", 10 },
                    { "tat_18", 10 },
                    { "tat_19", 10 },
                    { "tat_20", 10 },
                    { "tat_21", 10 },
                    { "tat_22", 10 },
                    { "tat_23", 10 },
                    { "tat_24", 10 },
                    { "tat_25", 10 },
                    { "tat_26", 10 },
                    { "tat_27", 10 },
                    { "tat_28", 10 },
                    { "tat_29", 10 },
                    { "tat_30", 10 },
                    { "tat_31", 10 },
                    { "tat_32", 10 },
                    { "tat_33", 10 },
                    { "tat_34", 10 },
                    { "tat_35", 10 },
                    { "tat_36", 10 },
                    { "tat_37", 10 },
                    { "tat_38", 10 },
                    { "tat_39", 10 },
                    { "tat_40", 10 },
                    { "tat_41", 10 },
                    { "tat_42", 10 },
                    { "tat_43", 10 },
                    { "tat_44", 10 },
                    { "tat_45", 10 },
                    { "tat_46", 10 },
                    { "tat_47", 10 },
                    { "tat_48", 10 },
                    { "tat_49", 10 },
                    { "tat_50", 10 },
                    { "tat_51", 10 },
                    { "tat_52", 10 },
                    { "tat_53", 10 },
                    { "tat_54", 10 },
                    { "tat_55", 10 },
                    { "tat_56", 10 },
                    { "tat_57", 10 },
                    { "tat_58", 10 },
                    { "tat_59", 10 },
                    { "tat_60", 10 },
                    { "tat_61", 10 },
                    { "tat_62", 10 },
                    { "tat_63", 10 },
                    { "tat_64", 10 },
                    { "tat_65", 10 },
                    { "tat_66", 10 },
                    { "tat_67", 10 },
                    { "tat_68", 10 },
                    { "tat_69", 10 },
                    { "tat_70", 10 },
                    { "tat_71", 10 },
                    { "tat_72", 10 },
                    { "tat_73", 10 },
                    { "tat_74", 10 },
                    { "tat_75", 10 },
                    { "tat_76", 10 },
                    { "tat_77", 10 },
                    { "tat_78", 10 },
                    { "tat_79", 10 },
                    { "tat_80", 10 },
                    { "tat_81", 10 },
                    { "tat_82", 10 },
                    { "tat_83", 10 },
                    { "tat_84", 10 },
                    { "tat_85", 10 },
                    { "tat_86", 10 },
                    { "tat_87", 10 },
                    { "tat_88", 10 },
                    { "tat_89", 10 },
                    { "tat_90", 10 },
                    { "tat_91", 10 },
                    { "tat_92", 10 },
                    { "tat_93", 10 },
                    { "tat_94", 10 },
                    { "tat_95", 10 },
                    { "tat_96", 10 },
                    { "tat_97", 10 },
                    { "tat_98", 10 },
                    { "tat_99", 10 },
                    { "tat_100", 10 },
                    { "tat_101", 10 },
                    { "tat_102", 10 },
                    { "tat_103", 10 },
                    { "tat_104", 10 },
                    { "tat_105", 10 },
                    { "tat_106", 10 },
                    { "tat_107", 10 },
                    { "tat_108", 10 },
                    { "tat_109", 10 },
                    { "tat_110", 10 },
                    { "tat_111", 10 },
                    { "tat_112", 10 },
                    { "tat_113", 10 },
                    { "tat_114", 10 },
                    { "tat_115", 10 },
                    { "tat_116", 10 },
                    { "tat_117", 10 },
                    { "tat_118", 10 },
                    { "tat_119", 10 },
                    { "tat_120", 10 },
                    { "tat_121", 10 },
                    { "tat_122", 10 },
                    { "tat_123", 10 },
                    { "tat_124", 10 },
                    { "tat_125", 10 },
                    { "tat_126", 10 },
                    { "tat_127", 10 },
                    { "tat_128", 10 },
                    { "tat_129", 10 },
                    { "tat_130", 10 },
                    { "tat_131", 10 },
                    { "tat_132", 10 },
                    { "tat_133", 10 },
                    { "tat_134", 10 },
                    { "tat_135", 10 },
                    { "tat_136", 10 },
                    { "tat_137", 10 },
                    { "tat_138", 10 },
                    { "tat_139", 10 },
                    { "tat_140", 10 },
                    { "tat_141", 10 },
                    { "tat_142", 10 },
                    { "tat_143", 10 },
                    { "tat_144", 10 },
                    { "tat_145", 10 },
                    { "tat_146", 10 },
                    { "tat_147", 10 },
                    { "tat_148", 10 },
                    { "tat_149", 10 },
                    { "tat_150", 10 },
                    { "tat_151", 10 },
                    { "tat_152", 10 },
                    { "tat_153", 10 },
                    { "tat_154", 10 },
                    { "tat_155", 10 },
                    { "tat_156", 10 },
                    { "tat_157", 10 },
                    { "tat_158", 10 },
                    { "tat_159", 10 },
                    { "tat_160", 10 },
                    { "tat_161", 10 },
                    { "tat_162", 10 },
                    { "tat_163", 10 },
                    { "tat_164", 10 },
                    { "tat_165", 10 },
                    { "tat_166", 10 },
                    { "tat_167", 10 },
                    { "tat_168", 10 },
                    { "tat_169", 10 },
                    { "tat_170", 10 },
                    { "tat_171", 10 },
                    { "tat_172", 10 },
                    { "tat_173", 10 },
                    { "tat_174", 10 },
                    { "tat_175", 10 },
                    { "tat_176", 10 },
                    { "tat_177", 10 },
                    { "tat_178", 10 },
                    { "tat_179", 10 },
                    { "tat_180", 10 },
                    { "tat_181", 10 },
                    { "tat_182", 10 },
                    { "tat_183", 10 },
                    { "tat_184", 10 },
                    { "tat_185", 10 },
                    { "tat_186", 10 },
                    { "tat_187", 10 },
                    { "tat_188", 10 },
                    { "tat_189", 10 },
                    { "tat_190", 10 },
                    { "tat_191", 10 },
                    { "tat_192", 10 },
                    { "tat_193", 10 },
                    { "tat_194", 10 },
                    { "tat_195", 10 },
                    { "tat_196", 10 },
                    { "tat_197", 10 },
                    { "tat_198", 10 },
                    { "tat_199", 10 },
                    { "tat_200", 10 },
                    { "tat_201", 10 },
                    { "tat_202", 10 },
                    { "tat_203", 10 },
                    { "tat_204", 10 },
                    { "tat_205", 10 },
                    { "tat_206", 10 },
                    { "tat_207", 10 },
                    { "tat_208", 10 },
                    { "tat_209", 10 },
                    { "tat_210", 10 },
                    { "tat_211", 10 },
                    { "tat_212", 10 },
                    { "tat_213", 10 },
                    { "tat_214", 10 },
                    { "tat_215", 10 },
                    { "tat_216", 10 },
                    { "tat_217", 10 },
                    { "tat_218", 10 },
                    { "tat_219", 10 },
                    { "tat_220", 10 },
                    { "tat_221", 10 },
                    { "tat_222", 10 },
                    { "tat_223", 10 },
                    { "tat_224", 10 },
                    { "tat_225", 10 },
                    { "tat_226", 10 },
                    { "tat_227", 10 },
                    { "tat_228", 10 },
                    { "tat_229", 10 },
                    { "tat_230", 10 },
                    { "tat_231", 10 },
                    { "tat_232", 10 },
                    { "tat_233", 10 },
                    { "tat_234", 10 },
                    { "tat_235", 10 },
                    { "tat_236", 10 },
                    { "tat_237", 10 },
                    { "tat_238", 10 },
                    { "tat_239", 10 },
                    { "tat_240", 10 },
                    { "tat_241", 10 },
                    { "tat_242", 10 },
                    { "tat_243", 10 },
                    { "tat_244", 10 },
                    { "tat_245", 10 },
                    { "tat_246", 10 },
                    { "tat_247", 10 },
                    { "tat_248", 10 },
                    { "tat_249", 10 },
                    { "tat_250", 10 },
                    { "tat_251", 10 },
                    { "tat_252", 10 },
                    { "tat_253", 10 },
                    { "tat_254", 10 },
                    { "tat_255", 10 },
                    { "tat_256", 10 },
                    { "tat_257", 10 },
                    { "tat_258", 10 },
                    { "tat_259", 10 },
                    { "tat_260", 10 },
                    { "tat_261", 10 },
                    { "tat_262", 10 },
                    { "tat_263", 10 },
                    { "tat_264", 10 },
                    { "tat_265", 10 },
                    { "tat_266", 10 },
                    { "tat_267", 10 },
                    { "tat_268", 10 },
                    { "tat_269", 10 },
                    { "tat_270", 10 },
                    { "tat_271", 10 },
                    { "tat_272", 10 },
                    { "tat_273", 10 },
                    { "tat_274", 10 },
                    { "tat_275", 10 },
                    { "tat_276", 10 },
                    { "tat_277", 10 },
                    { "tat_278", 10 },
                    { "tat_279", 10 },
                    { "tat_280", 10 },
                    { "tat_281", 10 },
                    { "tat_282", 10 },
                    { "tat_283", 10 },
                    { "tat_284", 10 },
                    { "tat_285", 10 },
                    { "tat_286", 10 },
                    { "tat_287", 10 },
                    { "tat_288", 10 },
                    { "tat_289", 10 },
                    { "tat_290", 10 },
                    { "tat_291", 10 },
                    { "tat_292", 10 },
                    { "tat_293", 10 },
                    { "tat_294", 10 },
                    { "tat_295", 10 },
                    { "tat_296", 10 },
                    { "tat_297", 10 },
                    { "tat_298", 10 },
                    { "tat_299", 10 },
                    { "tat_300", 10 },
                    { "tat_301", 10 },
                    { "tat_302", 10 },
                    { "tat_303", 10 },
                    { "tat_304", 10 },
                    { "tat_305", 10 },
                    { "tat_306", 10 },
                    { "tat_307", 10 },
                    { "tat_308", 10 },
                    { "tat_309", 10 },
                    { "tat_310", 10 },
                    { "tat_311", 10 },
                    { "tat_312", 10 },
                    { "tat_313", 10 },
                    { "tat_314", 10 },
                    { "tat_315", 10 },
                    { "tat_316", 10 },
                    { "tat_317", 10 },
                    { "tat_318", 10 },
                    { "tat_319", 10 },
                    { "tat_320", 10 },
                    { "tat_321", 10 },
                    { "tat_322", 10 },
                    { "tat_323", 10 },
                    { "tat_324", 10 },
                    { "tat_325", 10 },
                    { "tat_326", 10 },
                    { "tat_327", 10 },
                    { "tat_328", 10 },
                    { "tat_329", 10 },
                    { "tat_330", 10 },
                    { "tat_331", 10 },
                    { "tat_332", 10 },
                    { "tat_333", 10 },
                    { "tat_334", 10 },
                    { "tat_335", 10 },
                    { "tat_336", 10 },
                    { "tat_337", 10 },
                    { "tat_338", 10 },
                    { "tat_339", 10 },
                    { "tat_340", 10 },
                    { "tat_341", 10 },
                    { "tat_342", 10 },
                    { "tat_343", 10 },
                    { "tat_344", 10 },
                    { "tat_345", 10 },
                    { "tat_346", 10 },
                    { "tat_347", 10 },
                    { "tat_348", 10 },
                    { "tat_349", 10 },
                    { "tat_350", 10 },
                    { "tat_351", 10 },
                    { "tat_352", 10 },
                    { "tat_353", 10 },
                    { "tat_354", 10 },
                    { "tat_355", 10 },
                    { "tat_356", 10 },
                    { "tat_357", 10 },
                    { "tat_358", 10 },
                    { "tat_359", 10 },
                    { "tat_360", 10 },
                    { "tat_361", 10 },
                    { "tat_362", 10 },
                    { "tat_363", 10 },
                    { "tat_364", 10 },
                    { "tat_365", 10 },
                    { "tat_366", 10 },
                    { "tat_367", 10 },
                    { "tat_368", 10 },
                    { "tat_369", 10 },
                    { "tat_370", 10 },
                    { "tat_371", 10 },
                    { "tat_372", 10 },
                    { "tat_373", 10 },
                    { "tat_374", 10 },
                    { "tat_375", 10 },
                    { "tat_376", 10 },
                    { "tat_377", 10 },
                    { "tat_378", 10 },
                    { "tat_379", 10 },
                    { "tat_380", 10 },
                    { "tat_381", 10 },
                    { "tat_382", 10 },
                    { "tat_383", 10 },
                    { "tat_384", 10 },
                    { "tat_385", 10 },
                    { "tat_386", 10 },
                    { "tat_387", 10 },
                    { "tat_388", 10 },
                    { "tat_389", 10 },
                    { "tat_390", 10 },
                    { "tat_391", 10 },
                    { "tat_392", 10 },
                    { "tat_393", 10 },
                    { "tat_394", 10 },
                    { "tat_395", 10 },
                    { "tat_396", 10 },
                    { "tat_397", 10 },
                    { "tat_398", 10 },
                    { "tat_399", 10 },
                    { "tat_400", 10 },
                    { "tat_401", 10 },
                    { "tat_402", 10 },
                    { "tat_403", 10 },
                    { "tat_404", 10 },
                    { "tat_405", 10 },
                    { "tat_406", 10 },
                    { "tat_407", 10 },
                    { "tat_408", 10 },
                    { "tat_409", 10 },
                    { "tat_410", 10 },
                    { "tat_411", 10 },
                    { "tat_412", 10 },
                    { "tat_413", 10 },
                    { "tat_414", 10 },
                    { "tat_415", 10 },
                    { "tat_416", 10 },
                    { "tat_417", 10 },
                    { "tat_418", 10 },
                    { "tat_419", 10 },
                    { "tat_420", 10 },
                    { "tat_421", 10 },
                    { "tat_422", 10 },
                    { "tat_423", 10 },
                    { "tat_424", 10 },
                    { "tat_425", 10 },
                    { "tat_426", 10 },
                    { "tat_427", 10 },
                    { "tat_428", 10 },
                    { "tat_429", 10 },
                    { "tat_430", 10 },
                    { "tat_431", 10 },
                    { "tat_432", 10 },
                    { "tat_433", 10 },
                    { "tat_434", 10 },
                    { "tat_435", 10 },
                    { "tat_436", 10 },
                    { "tat_437", 10 },
                    { "tat_438", 10 },
                    { "tat_439", 10 },
                    { "tat_440", 10 },
                    { "tat_441", 10 },
                    { "tat_442", 10 },
                    { "tat_443", 10 },
                    { "tat_444", 10 },
                    { "tat_445", 10 },
                    { "tat_446", 10 },
                    { "tat_447", 10 },
                    { "tat_448", 10 },
                    { "tat_449", 10 },
                    { "tat_450", 10 },
                    { "tat_451", 10 },
                    { "tat_452", 10 },
                    { "tat_453", 10 },
                    { "tat_454", 10 },
                    { "tat_455", 10 },
                    { "tat_456", 10 },
                    { "tat_457", 10 },
                    { "tat_458", 10 },
                    { "tat_459", 10 },
                    { "tat_460", 10 },
                    { "tat_461", 10 },
                    { "tat_462", 10 },
                    { "tat_463", 10 },
                    { "tat_464", 10 },
                    { "tat_465", 10 },
                    { "tat_466", 10 },
                    { "tat_467", 10 },
                    { "tat_468", 10 },
                    { "tat_469", 10 },
                    { "tat_470", 10 },
                    { "tat_471", 10 },
                    { "tat_472", 10 },
                    { "tat_473", 10 },
                    { "tat_474", 10 },
                    { "tat_475", 10 },
                    { "tat_476", 10 },
                    { "tat_477", 10 },
                    { "tat_478", 10 },
                    { "tat_479", 10 },
                    { "tat_480", 10 },
                    { "tat_481", 10 },
                    { "tat_482", 10 },
                    { "tat_483", 10 },
                    { "tat_484", 10 },
                    { "tat_485", 10 },
                    { "tat_486", 10 },
                    { "tat_487", 10 },
                    { "tat_488", 10 },
                    { "tat_489", 10 },
                    { "tat_490", 10 },
                    { "tat_491", 10 },
                    { "tat_492", 10 },
                    { "tat_493", 10 },
                    { "tat_494", 10 },
                    { "tat_495", 10 },
                    { "tat_496", 10 },
                    { "tat_497", 10 },
                    { "tat_498", 10 },
                    { "tat_499", 10 },
                    { "tat_500", 10 },
                    { "tat_501", 10 },
                    { "tat_502", 10 },
                    { "tat_503", 10 },
                    { "tat_504", 10 },
                    { "tat_505", 10 },
                    { "tat_506", 10 },
                    { "tat_507", 10 },
                    { "tat_508", 10 },
                    { "tat_509", 10 },
                    { "tat_510", 10 },
                    { "tat_511", 10 },
                    { "tat_512", 10 },
                    { "tat_513", 10 },
                    { "tat_514", 10 },
                    { "tat_515", 10 },
                    { "tat_516", 10 },
                    { "tat_517", 10 },
                    { "tat_518", 10 },
                    { "tat_519", 10 },
                    { "tat_520", 10 },
                    { "tat_521", 10 },
                    { "tat_522", 10 },
                    { "tat_523", 10 },
                    { "tat_524", 10 },
                    { "tat_525", 10 },
                    { "tat_526", 10 },
                    { "tat_527", 10 },
                    { "tat_528", 10 },
                    { "tat_529", 10 },
                    { "tat_530", 10 },
                    { "tat_531", 10 },
                    { "tat_532", 10 },
                    { "tat_533", 10 },
                    { "tat_534", 10 },
                    { "tat_535", 10 },
                    { "tat_536", 10 },
                    { "tat_537", 10 },
                    { "tat_538", 10 },
                    { "tat_539", 10 },
                    { "tat_540", 10 },
                    { "tat_541", 10 },
                    { "tat_542", 10 },
                    { "tat_543", 10 },
                    { "tat_544", 10 },
                    { "tat_545", 10 },
                    { "tat_546", 10 },
                    { "tat_547", 10 },
                    { "tat_548", 10 },
                    { "tat_549", 10 },
                    { "tat_550", 10 },
                    { "tat_551", 10 },
                    { "tat_552", 10 },
                    { "tat_553", 10 },
                    { "tat_554", 10 },
                    { "tat_555", 10 },
                    { "tat_556", 10 },
                    { "tat_557", 10 },
                    { "tat_558", 10 },
                    { "tat_559", 10 },
                    { "tat_560", 10 },
                    { "tat_561", 10 },
                    { "tat_562", 10 },
                    { "tat_563", 10 },
                    { "tat_564", 10 },
                    { "tat_565", 10 },
                    { "tat_566", 10 },
                    { "tat_567", 10 },
                    { "tat_568", 10 },
                    { "tat_569", 10 },
                    { "tat_570", 10 },
                    { "tat_571", 10 },
                    { "tat_572", 10 },
                    { "tat_573", 10 },
                    { "tat_574", 10 },
                    { "tat_575", 10 },
                    { "tat_576", 10 },
                    { "tat_577", 10 },
                    { "tat_578", 10 },
                    { "tat_579", 10 },
                    { "tat_580", 10 },
                    { "tat_581", 10 },
                    { "tat_582", 10 },
                    { "tat_583", 10 },
                    { "tat_584", 10 },
                    { "tat_585", 10 },
                    { "tat_586", 10 },
                    { "tat_587", 10 },
                    { "tat_588", 10 },
                    { "tat_589", 10 },
                    { "tat_590", 10 },
                    { "tat_591", 10 },
                    { "tat_592", 10 },
                    { "tat_593", 10 },
                    { "tat_594", 10 },
                    { "tat_595", 10 },
                    { "tat_596", 10 },
                    { "tat_597", 10 },
                    { "tat_598", 10 },
                    { "tat_599", 10 },
                    { "tat_600", 10 },
                    { "tat_601", 10 },
                    { "tat_602", 10 },
                    { "tat_603", 10 },
                    { "tat_604", 10 },
                    { "tat_605", 10 },
                    { "tat_606", 10 },
                    { "tat_607", 10 },
                    { "tat_608", 10 },
                    { "tat_609", 10 },
                    { "tat_610", 10 },
                    { "tat_611", 10 },
                    { "tat_612", 10 },
                    { "tat_613", 10 },
                    { "tat_614", 10 },
                    { "tat_615", 10 },
                    { "tat_616", 10 },
                    { "tat_617", 10 },
                    { "tat_618", 10 },
                    { "tat_619", 10 },
                    { "tat_620", 10 },
                    { "tat_621", 10 },
                    { "tat_622", 10 },
                    { "tat_623", 10 },
                    { "tat_624", 10 },
                    { "tat_625", 10 },
                    { "tat_626", 10 },
                    { "tat_627", 10 },
                    { "tat_628", 10 },
                    { "tat_629", 10 },
                    { "tat_630", 10 },
                    { "tat_631", 10 },
                    { "tat_632", 10 },
                    { "tat_633", 10 },
                    { "tat_634", 10 },
                    { "tat_635", 10 },
                    { "tat_636", 10 },
                    { "tat_637", 10 },
                    { "tat_638", 10 },
                    { "tat_639", 10 },
                    { "tat_640", 10 },
                    { "tat_641", 10 },
                    { "tat_642", 10 },
                    { "tat_643", 10 },
                    { "tat_644", 10 },
                    { "tat_645", 10 },
                    { "tat_646", 10 },
                    { "tat_647", 10 },
                    { "tat_648", 10 },
                    { "tat_649", 10 },
                    { "tat_650", 10 },
                    { "tat_651", 10 },
                    { "tat_652", 10 },
                    { "tat_653", 10 },
                    { "tat_654", 10 },
                    { "tat_655", 10 },
                    { "tat_656", 10 },
                    { "tat_657", 10 },
                    { "tat_658", 10 },
                    { "tat_659", 10 },
                    { "tat_660", 10 },
                    { "tat_661", 10 },
                    { "tat_662", 10 },
                    { "tat_663", 10 },
                    { "tat_664", 10 },
                    { "tat_665", 10 },
                    { "tat_666", 10 },
                    { "tat_667", 10 },
                    { "tat_668", 10 },
                    { "tat_669", 10 },
                    { "tat_670", 10 },
                    { "tat_671", 10 },
                    { "tat_672", 10 },
                    { "tat_673", 10 },
                    { "tat_674", 10 },
                    { "tat_675", 10 },
                    { "tat_676", 10 },
                    { "tat_677", 10 },
                    { "tat_678", 10 },
                    { "tat_679", 10 },
                    { "tat_680", 10 },
                    { "tat_681", 10 },
                    { "tat_682", 10 },
                    { "tat_683", 10 },
                    { "tat_684", 10 },
                    { "tat_685", 10 },
                    { "tat_686", 10 },
                    { "tat_687", 10 },
                    { "tat_688", 10 },
                    { "tat_689", 10 },
                    { "tat_690", 10 },
                    { "tat_691", 10 },
                    { "tat_692", 10 },
                    { "tat_693", 10 },
                    { "tat_694", 10 },
                    { "tat_695", 10 },
                    { "tat_696", 10 },
                    { "tat_697", 10 },
                    { "tat_698", 10 },
                    { "tat_699", 10 },
                    { "tat_700", 10 },
                    { "tat_701", 10 },
                    { "tat_702", 10 },
                    { "tat_703", 10 },
                    { "tat_704", 10 },
                    { "tat_705", 10 },
                    { "tat_706", 10 },
                    { "tat_707", 10 },
                    { "tat_708", 10 },
                    { "tat_709", 10 },
                    { "tat_710", 10 },
                    { "tat_711", 10 },
                    { "tat_712", 10 },
                    { "tat_713", 10 },
                    { "tat_714", 10 },
                    { "tat_715", 10 },
                    { "tat_716", 10 },
                    { "tat_717", 10 },
                    { "tat_718", 10 },
                    { "tat_719", 10 },
                    { "tat_720", 10 },
                    { "tat_721", 10 },
                    { "tat_722", 10 },
                    { "tat_723", 10 },
                    { "tat_724", 10 },
                    { "tat_725", 10 },
                    { "tat_726", 10 },
                    { "tat_727", 10 },
                    { "tat_728", 10 },
                    { "tat_729", 10 },
                    { "tat_730", 10 },
                    { "tat_731", 10 },
                    { "tat_732", 10 },
                    { "tat_733", 10 },
                    { "tat_734", 10 },
                    { "tat_735", 10 },
                    { "tat_736", 10 },
                    { "tat_737", 10 },
                    { "tat_738", 10 },
                    { "tat_739", 10 },
                    { "tat_740", 10 },
                    { "tat_741", 10 },
                } }
            },

            {
                Types.Market,

                new MaterialsData(5, 7, 50) { Prices = new Dictionary<string, int>()
                {
                    { "f_burger", 100 },

                    { "med_b_0", 100 },
                    { "med_kit_0", 100 },
                    { "med_kit_1", 100 },

                    { "cigs_0", 100 },
                    { "cigs_1", 100 },
                    { "cig_0", 100 },
                    { "cig_1", 100 },
                } }
            },

            {
                Types.GasStation,

                new MaterialsData(5, 7, 50) { Prices = new Dictionary<string, int>()
                {

                } }
            },

            {
                Types.CarShop1,

                new MaterialsData(5, 7, 50) { Prices = new Dictionary<string, int>()
                {
                    { "adder", 100 },
                    { "airbus", 100 },
                    { "airtug", 100 },
                    { "alpha", 100 },
                    { "ambulance", 100 },
                    { "apc", 100 },
                    { "ardent", 100 },
                    { "asbo", 100 },
                    { "asea", 100 },
                    { "asea2", 100 },
                    { "asterope", 100 },
                    { "autarch", 100 },
                    { "baller", 100 },
                    { "baller2", 100 },
                    { "baller3", 100 },
                    { "baller4", 100 },
                    { "baller5", 100 },
                    { "baller6", 100 },
                    { "banshee", 100 },
                    { "banshee2", 100 },
                    { "barracks", 100 },
                    { "barracks2", 100 },
                    { "barracks3", 100 },
                    { "barrage", 100 },
                    { "benson", 100 },
                    { "bestiagts", 100 },
                    { "bfinjection", 100 },
                    { "biff", 100 },
                    { "bifta", 100 },
                    { "bison", 100 },
                    { "bison2", 100 },
                    { "bison3", 100 },
                    { "bjxl", 100 },
                    { "blade", 100 },
                    { "blazer", 100 },
                    { "blazer2", 100 },
                    { "blazer3", 100 },
                    { "blazer4", 100 },
                    { "blazer5", 100 },
                    { "blista", 100 },
                    { "blista2", 100 },
                    { "blista3", 100 },
                    { "bobcatxl", 100 },
                    { "bodhi2", 100 },
                    { "boxville", 100 },
                    { "boxville2", 100 },
                    { "boxville3", 100 },
                    { "boxville4", 100 },
                    { "boxville5", 100 },
                    { "brawler", 100 },
                    { "brickade", 100 },
                    { "brioso", 100 },
                    { "brioso2", 100 },
                    { "brioso3", 100 },
                    { "bruiser", 100 },
                    { "bruiser2", 100 },
                    { "bruiser3", 100 },
                    { "brutus", 100 },
                    { "brutus2", 100 },
                    { "brutus3", 100 },
                    { "btype", 100 },
                    { "btype2", 100 },
                    { "btype3", 100 },
                    { "buccaneer", 100 },
                    { "buccaneer2", 100 },
                    { "buffalo", 100 },
                    { "buffalo2", 100 },
                    { "buffalo3", 100 },
                    { "bulldozer", 100 },
                    { "bullet", 100 },
                    { "burrito", 100 },
                    { "burrito2", 100 },
                    { "burrito3", 100 },
                    { "burrito4", 100 },
                    { "burrito5", 100 },
                    { "bus", 100 },
                    { "caddy", 100 },
                    { "caddy2", 100 },
                    { "caddy3", 100 },
                    { "calico", 100 },
                    { "camper", 100 },
                    { "caracara", 100 },
                    { "caracara2", 100 },
                    { "carbonizzare", 100 },
                    { "casco", 100 },
                    { "cavalcade", 100 },
                    { "cavalcade2", 100 },
                    { "cerberus", 100 },
                    { "cerberus2", 100 },
                    { "cerberus3", 100 },
                    { "cheburek", 100 },
                    { "cheetah", 100 },
                    { "cheetah2", 100 },
                    { "chernobog", 100 },
                    { "chino", 100 },
                    { "chino2", 100 },
                    { "clique", 100 },
                    { "club", 100 },
                    { "coach", 100 },
                    { "cog55", 100 },
                    { "cog552", 100 },
                    { "cogcabrio", 100 },
                    { "cognoscenti", 100 },
                    { "cognoscenti2", 100 },
                    { "comet2", 100 },
                    { "comet3", 100 },
                    { "comet4", 100 },
                    { "comet5", 100 },
                    { "comet6", 100 },
                    { "contender", 100 },
                    { "coquette", 100 },
                    { "coquette2", 100 },
                    { "coquette3", 100 },
                    { "coquette4", 100 },
                    { "corsita", 100 },
                    { "crusader", 100 },
                    { "cutter", 100 },
                    { "cyclone", 100 },
                    { "cypher", 100 },
                    { "deluxo", 100 },
                    { "deveste", 100 },
                    { "deviant", 100 },
                    { "dloader", 100 },
                    { "docktug", 100 },
                    { "dominator", 100 },
                    { "dominator2", 100 },
                    { "dominator3", 100 },
                    { "dominator4", 100 },
                    { "dominator5", 100 },
                    { "dominator6", 100 },
                    { "dominator7", 100 },
                    { "dominator8", 100 },
                    { "drafter", 100 },
                    { "draugur", 100 },
                    { "dubsta", 100 },
                    { "dubsta2", 100 },
                    { "dubsta3", 100 },
                    { "dukes", 100 },
                    { "dukes2", 100 },
                    { "dukes3", 100 },
                    { "dump", 100 },
                    { "dune", 100 },
                    { "dune2", 100 },
                    { "dune3", 100 },
                    { "dune4", 100 },
                    { "dune5", 100 },
                    { "dynasty", 100 },
                    { "elegy", 100 },
                    { "elegy2", 100 },
                    { "ellie", 100 },
                    { "emerus", 100 },
                    { "emperor", 100 },
                    { "emperor2", 100 },
                    { "emperor3", 100 },
                    { "entity2", 100 },
                    { "entityxf", 100 },
                    { "euros", 100 },
                    { "everon", 100 },
                    { "exemplar", 100 },
                    { "f620", 100 },
                    { "faction", 100 },
                    { "faction2", 100 },
                    { "faction3", 100 },
                    { "fagaloa", 100 },
                    { "fbi", 100 },
                    { "fbi2", 100 },
                    { "felon", 100 },
                    { "felon2", 100 },
                    { "feltzer2", 100 },
                    { "feltzer3", 100 },
                    { "firetruk", 100 },
                    { "flashgt", 100 },
                    { "flatbed", 100 },
                    { "fmj", 100 },
                    { "forklift", 100 },
                    { "formula", 100 },
                    { "formula2", 100 },
                    { "fq2", 100 },
                    { "freecrawler", 100 },
                    { "fugitive", 100 },
                    { "furia", 100 },
                    { "furoregt", 100 },
                    { "fusilade", 100 },
                    { "futo", 100 },
                    { "futo2", 100 },
                    { "gauntlet", 100 },
                    { "gauntlet2", 100 },
                    { "gauntlet3", 100 },
                    { "gauntlet4", 100 },
                    { "gauntlet5", 100 },
                    { "gb200", 100 },
                    { "gburrito", 100 },
                    { "gburrito2", 100 },
                    { "glendale", 100 },
                    { "glendale2", 100 },
                    { "gp1", 100 },
                    { "granger", 100 },
                    { "greenwood", 100 },
                    { "gresley", 100 },
                    { "growler", 100 },
                    { "gt500", 100 },
                    { "guardian", 100 },
                    { "habanero", 100 },
                    { "halftrack", 100 },
                    { "handler", 100 },
                    { "hauler", 100 },
                    { "hauler2", 100 },
                    { "hellion", 100 },
                    { "hermes", 100 },
                    { "hotknife", 100 },
                    { "hotring", 100 },
                    { "huntley", 100 },
                    { "hustler", 100 },
                    { "imorgon", 100 },
                    { "impaler", 100 },
                    { "impaler2", 100 },
                    { "impaler3", 100 },
                    { "impaler4", 100 },
                    { "imperator", 100 },
                    { "imperator2", 100 },
                    { "imperator3", 100 },
                    { "infernus", 100 },
                    { "infernus2", 100 },
                    { "ingot", 100 },
                    { "insurgent", 100 },
                    { "insurgent2", 100 },
                    { "insurgent3", 100 },
                    { "intruder", 100 },
                    { "issi2", 100 },
                    { "issi3", 100 },
                    { "issi4", 100 },
                    { "issi5", 100 },
                    { "issi6", 100 },
                    { "issi7", 100 },
                    { "italigtb", 100 },
                    { "italigtb2", 100 },
                    { "italigto", 100 },
                    { "italirsx", 100 },
                    { "jackal", 100 },
                    { "jb700", 100 },
                    { "jb7002", 100 },
                    { "jester", 100 },
                    { "jester2", 100 },
                    { "jester3", 100 },
                    { "jester4", 100 },
                    { "journey", 100 },
                    { "jugular", 100 },
                    { "kalahari", 100 },
                    { "kamacho", 100 },
                    { "kanjo", 100 },
                    { "kanjosj", 100 },
                    { "khamelion", 100 },
                    { "khanjali", 100 },
                    { "komoda", 100 },
                    { "krieger", 100 },
                    { "kuruma", 100 },
                    { "kuruma2", 100 },
                    { "landstalker", 100 },
                    { "landstalker2", 100 },
                    { "le7b", 100 },
                    { "lguard", 100 },
                    { "limo2", 100 },
                    { "lm87", 100 },
                    { "locust", 100 },
                    { "lurcher", 100 },
                    { "lynx", 100 },
                    { "mamba", 100 },
                    { "manana", 100 },
                    { "manana2", 100 },
                    { "marshall", 100 },
                    { "massacro", 100 },
                    { "massacro2", 100 },
                    { "menacer", 100 },
                    { "mesa", 100 },
                    { "mesa2", 100 },
                    { "mesa3", 100 },
                    { "michelli", 100 },
                    { "minitank", 100 },
                    { "minivan", 100 },
                    { "minivan2", 100 },
                    { "mixer", 100 },
                    { "mixer2", 100 },
                    { "monroe", 100 },
                    { "monster", 100 },
                    { "monster3", 100 },
                    { "monster4", 100 },
                    { "monster5", 100 },
                    { "moonbeam", 100 },
                    { "moonbeam2", 100 },
                    { "mower", 100 },
                    { "mule", 100 },
                    { "mule2", 100 },
                    { "mule3", 100 },
                    { "mule4", 100 },
                    { "nebula", 100 },
                    { "neo", 100 },
                    { "nero", 100 },
                    { "nero2", 100 },
                    { "nightshade", 100 },
                    { "nightshark", 100 },
                    { "ninef", 100 },
                    { "ninef2", 100 },
                    { "novak", 100 },
                    { "omnis", 100 },
                    { "openwheel1", 100 },
                    { "openwheel2", 100 },
                    { "oracle", 100 },
                    { "oracle2", 100 },
                    { "osiris", 100 },
                    { "outlaw", 100 },
                    { "packer", 100 },
                    { "panto", 100 },
                    { "paradise", 100 },
                    { "paragon", 100 },
                    { "paragon2", 100 },
                    { "pariah", 100 },
                    { "patriot", 100 },
                    { "patriot2", 100 },
                    { "pbus", 100 },
                    { "pbus2", 100 },
                    { "penetrator", 100 },
                    { "penumbra", 100 },
                    { "penumbra2", 100 },
                    { "peyote", 100 },
                    { "peyote2", 100 },
                    { "peyote3", 100 },
                    { "pfister811", 100 },
                    { "phantom", 100 },
                    { "phantom2", 100 },
                    { "phantom3", 100 },
                    { "phoenix", 100 },
                    { "picador", 100 },
                    { "pigalle", 100 },
                    { "police", 100 },
                    { "police2", 100 },
                    { "police3", 100 },
                    { "police4", 100 },
                    { "policeold1", 100 },
                    { "policeold2", 100 },
                    { "policet", 100 },
                    { "pony", 100 },
                    { "pony2", 100 },
                    { "postlude", 100 },
                    { "pounder", 100 },
                    { "pounder2", 100 },
                    { "prairie", 100 },
                    { "pranger", 100 },
                    { "premier", 100 },
                    { "previon", 100 },
                    { "primo", 100 },
                    { "primo2", 100 },
                    { "prototipo", 100 },
                    { "radi", 100 },
                    { "rallytruck", 100 },
                    { "rancherxl", 100 },
                    { "rancherxl2", 100 },
                    { "rapidgt", 100 },
                    { "rapidgt2", 100 },
                    { "rapidgt3", 100 },
                    { "raptor", 100 },
                    { "ratloader", 100 },
                    { "ratloader2", 100 },
                    { "rcbandito", 100 },
                    { "reaper", 100 },
                    { "rebel", 100 },
                    { "rebel2", 100 },
                    { "rebla", 100 },
                    { "regina", 100 },
                    { "remus", 100 },
                    { "rentalbus", 100 },
                    { "retinue", 100 },
                    { "retinue2", 100 },
                    { "revolter", 100 },
                    { "rhapsody", 100 },
                    { "rhinehart", 100 },
                    { "rhino", 100 },
                    { "riata", 100 },
                    { "riot", 100 },
                    { "riot2", 100 },
                    { "ripley", 100 },
                    { "rocoto", 100 },
                    { "romero", 100 },
                    { "rt3000", 100 },
                    { "rubble", 100 },
                    { "ruiner", 100 },
                    { "ruiner2", 100 },
                    { "ruiner3", 100 },
                    { "ruiner4", 100 },
                    { "rumpo", 100 },
                    { "rumpo2", 100 },
                    { "rumpo3", 100 },
                    { "ruston", 100 },
                    { "s80", 100 },
                    { "sabregt", 100 },
                    { "sabregt2", 100 },
                    { "sadler", 100 },
                    { "sadler2", 100 },
                    { "sandking", 100 },
                    { "sandking2", 100 },
                    { "savestra", 100 },
                    { "sc1", 100 },
                    { "scarab", 100 },
                    { "scarab2", 100 },
                    { "scarab3", 100 },
                    { "schafter2", 100 },
                    { "schafter3", 100 },
                    { "schafter4", 100 },
                    { "schafter5", 100 },
                    { "schafter6", 100 },
                    { "schlagen", 100 },
                    { "schwarzer", 100 },
                    { "scramjet", 100 },
                    { "scrap", 100 },
                    { "seminole", 100 },
                    { "seminole2", 100 },
                    { "sentinel", 100 },
                    { "sentinel2", 100 },
                    { "sentinel3", 100 },
                    { "sentinel4", 100 },
                    { "serrano", 100 },
                    { "seven70", 100 },
                    { "sheava", 100 },
                    { "sheriff", 100 },
                    { "sheriff2", 100 },
                    { "slamtruck", 100 },
                    { "slamvan", 100 },
                    { "slamvan2", 100 },
                    { "slamvan3", 100 },
                    { "slamvan4", 100 },
                    { "slamvan5", 100 },
                    { "slamvan6", 100 },
                    { "sm722", 100 },
                    { "specter", 100 },
                    { "specter2", 100 },
                    { "speedo", 100 },
                    { "speedo2", 100 },
                    { "speedo4", 100 },
                    { "squaddie", 100 },
                    { "stafford", 100 },
                    { "stalion", 100 },
                    { "stalion2", 100 },
                    { "stanier", 100 },
                    { "stinger", 100 },
                    { "stingergt", 100 },
                    { "stockade", 100 },
                    { "stockade3", 100 },
                    { "stratum", 100 },
                    { "streiter", 100 },
                    { "stretch", 100 },
                    { "stromberg", 100 },
                    { "sugoi", 100 },
                    { "sultan", 100 },
                    { "sultan2", 100 },
                    { "sultan3", 100 },
                    { "sultanrs", 100 },
                    { "superd", 100 },
                    { "surano", 100 },
                    { "surfer", 100 },
                    { "surfer2", 100 },
                    { "swinger", 100 },
                    { "t20", 100 },
                    { "taco", 100 },
                    { "tailgater", 100 },
                    { "tailgater2", 100 },
                    { "taipan", 100 },
                    { "tampa", 100 },
                    { "tampa2", 100 },
                    { "tampa3", 100 },
                    { "taxi", 100 },
                    { "technical", 100 },
                    { "technical2", 100 },
                    { "technical3", 100 },
                    { "tempesta", 100 },
                    { "tenf", 100 },
                    { "tenf2", 100 },
                    { "terbyte", 100 },
                    { "tezeract", 100 },
                    { "thrax", 100 },
                    { "tigon", 100 },
                    { "tiptruck", 100 },
                    { "tiptruck2", 100 },
                    { "toreador", 100 },
                    { "torero", 100 },
                    { "torero2", 100 },
                    { "tornado", 100 },
                    { "tornado2", 100 },
                    { "tornado3", 100 },
                    { "tornado4", 100 },
                    { "tornado5", 100 },
                    { "tornado6", 100 },
                    { "toros", 100 },
                    { "tourbus", 100 },
                    { "towtruck", 100 },
                    { "towtruck2", 100 },
                    { "tractor", 100 },
                    { "tractor2", 100 },
                    { "tractor3", 100 },
                    { "trash", 100 },
                    { "trash2", 100 },
                    { "trophytruck", 100 },
                    { "trophytruck2", 100 },
                    { "tropos", 100 },
                    { "tulip", 100 },
                    { "turismo2", 100 },
                    { "turismor", 100 },
                    { "tyrant", 100 },
                    { "tyrus", 100 },
                    { "utillitruck", 100 },
                    { "utillitruck2", 100 },
                    { "utillitruck3", 100 },
                    { "vacca", 100 },
                    { "vagner", 100 },
                    { "vagrant", 100 },
                    { "vamos", 100 },
                    { "vectre", 100 },
                    { "verlierer2", 100 },
                    { "verus", 100 },
                    { "vetir", 100 },
                    { "veto", 100 },
                    { "veto2", 100 },
                    { "vigero", 100 },
                    { "vigero2", 100 },
                    { "vigilante", 100 },
                    { "virgo", 100 },
                    { "virgo2", 100 },
                    { "virgo3", 100 },
                    { "viseris", 100 },
                    { "visione", 100 },
                    { "voodoo", 100 },
                    { "voodoo2", 100 },
                    { "vstr", 100 },
                    { "warrener", 100 },
                    { "washington", 100 },
                    { "wastelander", 100 },
                    { "weevil", 100 },
                    { "weevil2", 100 },
                    { "windsor", 100 },
                    { "windsor2", 100 },
                    { "winky", 100 },
                    { "xa21", 100 },
                    { "xls", 100 },
                    { "xls2", 100 },
                    { "yosemite", 100 },
                    { "yosemite2", 100 },
                    { "yosemite3", 100 },
                    { "youga", 100 },
                    { "youga2", 100 },
                    { "youga3", 100 },
                    { "z190", 100 },
                    { "zentorno", 100 },
                    { "zhaba", 100 },
                    { "zion", 100 },
                    { "zion2", 100 },
                    { "zion3", 100 },
                    { "zorrusso", 100 },
                    { "zr350", 100 },
                    { "zr380", 100 },
                    { "zr3802", 100 },
                    { "zr3803", 100 },
                    { "ztype", 100 },
                    { "dilettante", 100 },
                    { "dilettante2", 100 },
                    { "neon", 100 },
                    { "omnisegt", 100 },
                    { "raiden", 100 },
                    { "surge", 100 },
                    { "voltic", 100 },
                    { "voltic2", 100 },
                } }
            },

            {
                Types.CarShop2,

                new MaterialsData(5, 7, 50) { Prices = new Dictionary<string, int>()
                {

                } }
            },

            {
                Types.CarShop3,

                new MaterialsData(5, 7, 50) { Prices = new Dictionary<string, int>()
                {

                } }
            },

            {
                Types.MotoShop,

                new MaterialsData(5, 7, 50) { Prices = new Dictionary<string, int>()
                {
                    { "akuma", 100 },
                    { "avarus", 100 },
                    { "bagger", 100 },
                    { "bati", 100 },
                    { "bati2", 100 },
                    { "bf400", 100 },
                    { "carbonrs", 100 },
                    { "chimera", 100 },
                    { "cliffhanger", 100 },
                    { "daemon", 100 },
                    { "daemon2", 100 },
                    { "deathbike", 100 },
                    { "deathbike2", 100 },
                    { "deathbike3", 100 },
                    { "defiler", 100 },
                    { "diablous", 100 },
                    { "diablous2", 100 },
                    { "double", 100 },
                    { "enduro", 100 },
                    { "esskey", 100 },
                    { "faggio", 100 },
                    { "faggio2", 100 },
                    { "faggio3", 100 },
                    { "fcr", 100 },
                    { "fcr2", 100 },
                    { "gargoyle", 100 },
                    { "hakuchou", 100 },
                    { "hakuchou2", 100 },
                    { "hexer", 100 },
                    { "innovation", 100 },
                    { "lectro", 100 },
                    { "manchez", 100 },
                    { "manchez2", 100 },
                    { "nemesis", 100 },
                    { "nightblade", 100 },
                    { "oppressor", 100 },
                    { "oppressor2", 100 },
                    { "pcj", 100 },
                    { "ratbike", 100 },
                    { "ruffian", 100 },
                    { "rrocket", 100 },
                    { "sanchez", 100 },
                    { "sanchez2", 100 },
                    { "sanctus", 100 },
                    { "shotaro", 100 },
                    { "sovereign", 100 },
                    { "stryder", 100 },
                    { "thrust", 100 },
                    { "vader", 100 },
                    { "vindicator", 100 },
                    { "vortex", 100 },
                    { "wolfsbane", 100 },
                    { "zombiea", 100 },
                    { "zombieb", 100 },
                    { "policeb", 100 },
                    { "bmx", 100 },
                    { "cruiser", 100 },
                    { "fixter", 100 },
                    { "scorcher", 100 },
                    { "tribike", 100 },
                    { "tribike2", 100 },
                    { "tribike3", 100 },
                } }
            },

            {
                Types.BoatShop,

                new MaterialsData(5, 7, 50) { Prices = new Dictionary<string, int>()
                {
                    { "avisa", 100 },
                    { "dinghy", 100 },
                    { "dinghy2", 100 },
                    { "dinghy3", 100 },
                    { "dinghy4", 100 },
                    { "dinghy5", 100 },
                    { "jetmax", 100 },
                    { "kosatka", 100 },
                    { "longfin", 100 },
                    { "marquis", 100 },
                    { "patrolboat", 100 },
                    { "predator", 100 },
                    { "seashark", 100 },
                    { "seashark2", 100 },
                    { "seashark3", 100 },
                    { "speeder", 100 },
                    { "speeder2", 100 },
                    { "squalo", 100 },
                    { "submersible", 100 },
                    { "submersible2", 100 },
                    { "suntrap", 100 },
                    { "toro", 100 },
                    { "toro2", 100 },
                    { "tropic", 100 },
                    { "tropic2", 100 },
                    { "tug", 100 },
                } }
            },

            {
                Types.AeroShop,

                new MaterialsData(5, 7, 50) { Prices = new Dictionary<string, int>()
                {
                    { "akula", 100 },
                    { "annihilator", 100 },
                    { "annihilator2", 100 },
                    { "buzzard", 100 },
                    { "buzzard2", 100 },
                    { "cargobob", 100 },
                    { "cargobob2", 100 },
                    { "cargobob3", 100 },
                    { "cargobob4", 100 },
                    { "conada", 100 },
                    { "frogger", 100 },
                    { "frogger2", 100 },
                    { "havok", 100 },
                    { "hunter", 100 },
                    { "maverick", 100 },
                    { "savage", 100 },
                    { "seasparrow", 100 },
                    { "seasparrow2", 100 },
                    { "seasparrow3", 100 },
                    { "skylift", 100 },
                    { "supervolito", 100 },
                    { "supervolito2", 100 },
                    { "swift", 100 },
                    { "swift2", 100 },
                    { "valkyrie", 100 },
                    { "valkyrie2", 100 },
                    { "volatus", 100 },
                    { "polmav", 100 },
                    { "alphaz1", 100 },
                    { "avenger", 100 },
                    { "avenger2", 100 },
                    { "besra", 100 },
                    { "bombushka", 100 },
                    { "cargoplane", 100 },
                    { "cuban800", 100 },
                    { "dodo", 100 },
                    { "duster", 100 },
                    { "howard", 100 },
                    { "hydra", 100 },
                    { "jet", 100 },
                    { "lazer", 100 },
                    { "luxor", 100 },
                    { "luxor2", 100 },
                    { "mammatus", 100 },
                    { "microlight", 100 },
                    { "miljet", 100 },
                    { "mogul", 100 },
                    { "molotok", 100 },
                    { "nimbus", 100 },
                    { "nokota", 100 },
                    { "pyro", 100 },
                    { "rogue", 100 },
                    { "seabreeze", 100 },
                    { "shamal", 100 },
                    { "starling", 100 },
                    { "strikeforce", 100 },
                    { "stunt", 100 },
                    { "titan", 100 },
                    { "tula", 100 },
                    { "velum", 100 },
                    { "velum2", 100 },
                    { "vestra", 100 },
                    { "volatol", 100 },
                    { "alkonost", 100 },
                } }
            },

            {
                Types.TuningShop,

                new MaterialsData(5, 7, 50) { Prices = new Dictionary<string, int>()
                {
                    { "fix_0", 100 },
                    { "fix_1", 100 },

                    { "keys_0", 100 },
                    { "keys_1", 100 },

                    { "engine_0", 100 },
                    { "engine_1", 100 },
                    { "engine_2", 100 },
                    { "engine_3", 100 },
                    { "engine_4", 100 },

                    { "brakes_0", 100 },
                    { "brakes_1", 100 },
                    { "brakes_2", 100 },
                    { "brakes_3", 100 },

                    { "trm_0", 100 },
                    { "trm_1", 100 },
                    { "trm_2", 100 },
                    { "trm_3", 100 },

                    { "susp_0", 100 },
                    { "susp_1", 100 },
                    { "susp_2", 100 },
                    { "susp_3", 100 },
                    { "susp_4", 100 },

                    { "xenon_0", 100 },
                    { "xenon_1", 100 },
                    { "xenon_2", 100 },
                    { "xenon_3", 100 },
                    { "xenon_4", 100 },
                    { "xenon_5", 100 },
                    { "xenon_6", 100 },
                    { "xenon_7", 100 },
                    { "xenon_8", 100 },
                    { "xenon_9", 100 },
                    { "xenon_10", 100 },
                    { "xenon_11", 100 },
                    { "xenon_12", 100 },
                    { "xenon_13", 100 },
                    { "xenon_14", 100 },

                    { "wtint_0", 100 },
                    { "wtint_1", 100 },
                    { "wtint_2", 100 },
                    { "wtint_3", 100 },

                    { "tt_0", 100 },
                    { "tt_1", 100 },

                    { "neon_0", 100 },
                    { "neon", 100 },

                    { "tsmoke_0", 100 },
                    { "tsmoke", 100 },

                    { "horn", 100 },

                    { "spoiler", 100 },

                    { "fbump", 100 },

                    { "rbump", 100 },

                    { "skirt", 100 },

                    { "exh", 100 },

                    { "frame", 100 },

                    { "grill", 100 },

                    { "hood", 100 },

                    { "roof", 100 },

                    { "livery", 100 },

                    { "swheel", 100 },

                    { "seats", 100 },

                    { "colourt_0", 100 },
                    { "colourt_1", 100 },
                    { "colourt_2", 100 },
                    { "colourt_3", 100 },
                    { "colourt_4", 100 },
                    { "colourt_5", 100 },

                    { "colour", 100 },

                    { "pearl_0", 100 },
                    { "pearl", 100 },

                    { "wcolour_0", 100 },
                    { "wcolour", 100 },

                    { "wheel_0", 100 },
                    { "wheel_1", 100 },
                    { "wheel_2", 100 },
                    { "wheel_3", 100 },
                    { "wheel_4", 100 },
                    { "wheel_5", 100 },
                    { "wheel_6", 100 },
                    { "wheel_7", 100 },
                    { "wheel_8", 100 },
                    { "wheel_9", 100 },
                    { "wheel_10", 100 },
                    { "wheel_11", 100 },
                    { "wheel_12", 100 },
                    { "wheel_13", 100 },

                    // bike rear wheel
                    { "rwheel_0", 100 },
                    { "rwheel_7", 100 },
                } }
            },

            {
                Types.WeaponShop,

                new MaterialsData(5, 7, 50) { Prices = new Dictionary<string, int>()
                {
                    { "w_asrifle", 100 },
                    { "w_asrifle_mk2", 100 },
                    { "w_advrifle", 100 },
                    { "w_carbrifle", 100 },
                    { "w_comprifle", 100 },
                    { "w_heavyrifle", 100 },

                    { "w_microsmg", 100 },
                    { "w_minismg", 100 },
                    { "w_smg", 100 },
                    { "w_smg_mk2", 100 },
                    { "w_asmsg", 100 },
                    { "w_combpdw", 100 },

                    { "w_combmg", 100 },
                    { "w_gusenberg", 100 },

                    { "w_heavysnp", 100 },
                    { "w_markrifle", 100 },
                    { "w_musket", 100 },

                    { "w_assgun", 100 },
                    { "w_heavysgun", 100 },
                    { "w_pumpsgun", 100 },
                    { "w_pumpsgun_mk2", 100 },
                    { "w_sawnsgun", 100 },

                    { "w_pistol", 100 },
                    { "w_pistol_mk2", 100 },
                    { "w_appistol", 100 },
                    { "w_combpistol", 100 },
                    { "w_heavypistol", 100 },
                    { "w_machpistol", 100 },
                    { "w_markpistol", 100 },
                    { "w_vintpistol", 100 },

                    { "w_revolver", 100 },
                    { "w_revolver_mk2", 100 },

                    { "w_bat", 100 },
                    { "w_bottle", 100 },
                    { "w_crowbar", 100 },
                    { "w_dagger", 100 },
                    { "w_flashlight", 100 },
                    { "w_golfclub", 100 },
                    { "w_hammer", 100 },
                    { "w_hatchet", 100 },
                    { "w_knuckles", 100 },
                    { "w_machete", 100 },
                    { "w_nightstick", 100 },
                    { "w_poolcue", 100 },
                    { "w_switchblade", 100 },
                    { "w_wrench", 100 },

                    { "am_5.56", 1 },
                    { "am_7.62", 1 },
                    { "am_9", 1 },
                    { "am_11.43", 1 },
                    { "am_12", 1 },
                    { "am_12.7", 1 },

                    { "wc_s", 10 },
                    { "wc_f", 10 },
                    { "wc_g", 10 },
                    { "wc_sc", 10 },
                } }
            },
        };

        public Dictionary<string, int> Prices => AllPrices[Type].Prices;

        public (int MatPrice, int RealPrice)? CanBuy(PlayerData pData, bool useCash, string itemId, int amount)
        {
            var priceData = MaterialsData;

            if (priceData == null)
                return null;

            int matPrice;

            if (!priceData.Prices.TryGetValue(itemId, out matPrice))
                return null;

            matPrice *= amount;

            if (Owner != null)
            {
                if (!HasEnoughMaterials(matPrice, pData))
                    return null;
            }

            var realPrice = (int)Math.Floor(matPrice * priceData.RealPrice * Margin);

            if (useCash)
            {
                if (!pData.HasEnoughCash(realPrice, true))
                    return null;
            }
            else
            {
                if (!pData.HasBankAccount(true))
                    return null;

                var cb = pData.BankAccount.HasEnoughMoneyDebit(realPrice, true, true);

                if (cb < 0)
                    return null;

                realPrice -= cb;
            }

            return (matPrice, realPrice);
        }

        public virtual bool BuyItem(PlayerData pData, bool useCash, string itemId)
        {
            var iData = itemId.Split('&');

            if (iData.Length != 3)
                return false;

            int variation, amount;

            if (!int.TryParse(iData[1], out variation) || !int.TryParse(iData[2], out amount))
                return false;

            if (variation < 0 || amount <= 0)
                return false;

            var res = CanBuy(pData, useCash, iData[0], amount);

            if (res == null)
                return false;

            if (!pData.GiveItem(iData[0], variation, amount, true, true))
                return false;

            PaymentProceed(pData, useCash, res.Value.MatPrice, res.Value.RealPrice);

            return true;
        }

        public Shop(int ID, Vector3 PositionInfo, Utils.Vector4 PositionInteract, Types Type) : base(ID, PositionInfo, PositionInteract, Type)
        {

        }
    }

    public abstract class ClothesShop : Shop, IEnterable
    {
        public override string ClientData => $"{ID}, {PositionInfo.ToCSharpStr()}, {GovPrice}, {Rent}, {Tax}f, {PositionInteract.ToCSharpStr()}";

        public Utils.Vector4 EnterProperties { get; set; }

        public Utils.Vector4[] ExitProperties { get; set; }

        public int LastExitUsed { get; set; }

        public ClothesShop(int ID, Vector3 Position, Utils.Vector4 EnterProperies, Types Type, Utils.Vector4 PositionInteract) : base(ID, Position, PositionInteract, Type)
        {
            this.EnterProperties = EnterProperies;

            this.ExitProperties = new Utils.Vector4[] { new Utils.Vector4(PositionInteract.Position.GetFrontOf(PositionInteract.RotationZ, 1.5f), Utils.GetOppositeAngle(PositionInteract.RotationZ)) };
        }
    }

    public class ClothesShop1 : ClothesShop
    {
        public ClothesShop1(int ID, Vector3 Position, Utils.Vector4 PositionInteract) : base(ID, Position, new Utils.Vector4(1190.617f, 2714.198f, 38.22264f, 217f), Types.ClothesShop1, PositionInteract)
        {

        }
    }

    public class ClothesShop2 : ClothesShop
    {
        public ClothesShop2(int ID, Vector3 Position, Utils.Vector4 PositionInteract) : base(ID, Position, new Utils.Vector4(617.65f, 2766.828f, 42.0881f, 176f), Types.ClothesShop2, PositionInteract)
        {

        }
    }

    public class ClothesShop3 : ClothesShop
    {
        public ClothesShop3(int ID, Vector3 Position, Utils.Vector4 PositionInteract) : base(ID, Position, new Utils.Vector4(-1447.433f, -243.1756f, 49.82227f, 70f), Types.ClothesShop3, PositionInteract)
        {

        }
    }

    public class BagShop : ClothesShop
    {
        public BagShop(int ID, Vector3 Position, Utils.Vector4 PositionInteract, Utils.Vector4 ViewPosition) : base(ID, Position, ViewPosition, Types.BagShop, PositionInteract)
        {

        }
    }

    public class MaskShop : ClothesShop
    {
        public MaskShop(int ID, Vector3 Position, Utils.Vector4 PositionInteract, Utils.Vector4 ViewPosition) : base(ID, Position, ViewPosition, Types.MaskShop, PositionInteract)
        {

        }
    }

    public class JewelleryShop : ClothesShop
    {
        public JewelleryShop(int ID, Vector3 Position, Utils.Vector4 PositionInteract, Utils.Vector4 ViewPosition) : base(ID, Position, ViewPosition, Types.JewelleryShop, PositionInteract)
        {

        }
    }

    public class TattooShop : ClothesShop
    {
        public TattooShop(int ID, Vector3 Position, Utils.Vector4 PositionInteract, Utils.Vector4 ViewPosition) : base(ID, Position, ViewPosition, Types.TattooShop, PositionInteract)
        {

        }

        public override bool BuyItem(PlayerData pData, bool useCash, string itemId)
        {
            Console.WriteLine(itemId);

            var res = CanBuy(pData, useCash, itemId, 1);

            if (res == null)
                return false;

            var iData = itemId.Split('_');

            if (iData.Length == 3)
            {
                int zTypeNum;

                if (!int.TryParse(iData[2], out zTypeNum) || !Enum.IsDefined(typeof(Data.Customization.TattooData.ZoneTypes), zTypeNum))
                    return false;

                var zType = (Data.Customization.TattooData.ZoneTypes)zTypeNum;

                var tattooIdx = pData.Decorations.ToDictionary(x => x, x => Data.Customization.GetTattooData(x)).Where(x => x.Value?.ZoneType == zType).Select(x => x.Key).FirstOrDefault();

                if (!pData.Decorations.Remove(tattooIdx))
                    return false;
            }
            else if (iData.Length == 2)
            {
                int tattooIdx;

                if (!int.TryParse(iData[1], out tattooIdx))
                    return false;

                var tattooData = Data.Customization.GetTattooData(tattooIdx);

                if (tattooData == null)
                    return false;

                var tattooToDelete = pData.Decorations.ToDictionary(x => x, x => Data.Customization.GetTattooData(x)).Where(x => x.Value?.ZoneType == tattooData.ZoneType).Select(x => x.Key).FirstOrDefault();

                pData.Decorations.Remove(tattooToDelete);

                pData.Decorations.Add(tattooIdx);
            }
            else
            {
                return false;
            }

            PaymentProceed(pData, useCash, res.Value.MatPrice, res.Value.RealPrice);

            return true;
        }
    }

    public class BarberShop : ClothesShop
    {
        public BarberShop(int ID, Vector3 Position, Utils.Vector4 PositionInteract, Utils.Vector4 ViewPosition) : base(ID, Position, ViewPosition, Types.BarberShop, PositionInteract)
        {

        }

        private static Dictionary<string, int> HeadOverlayNums = new Dictionary<string, int>()
        {
            { "beard", 1 },
            { "eyebrows", 2 },
            { "makeup", 4 },
            { "blush", 5 },
            { "lipstick", 8 },
            { "chest", 10 },
        };

        public override bool BuyItem(PlayerData pData, bool useCash, string itemId)
        {
            Console.WriteLine(itemId);

            var iData = itemId.Split('&');

            if (iData.Length < 2)
                return false;

            var itemIdData = iData[0].Split('_');

            if (itemIdData.Length < 2)
                return false;

            var realItemId = itemIdData[0];

            byte variation;

            if (realItemId == "hair")
            {
                if (itemIdData.Length != 3 || iData.Length != 4)
                    return false;

                if ((itemIdData[1] != "m" && itemIdData[1] != "f") || (itemIdData[1] == "m" && !pData.Sex))
                    return false;

                if (!byte.TryParse(itemIdData[2], out variation))
                    return false;

                realItemId = iData[0];
            }
            else
            {
                if (itemIdData.Length != 2)
                    return false;

                if (realItemId == "beard" || realItemId == "chest")
                {
                    if (!pData.Sex)
                        return false;
                }

                if (!byte.TryParse(itemIdData[1], out variation))
                    return false;

                if (variation == 255 || realItemId == "beard")
                    realItemId = iData[0];
            }

            var res = CanBuy(pData, useCash, realItemId, 1);

            if (res == null)
                return false;

            if (itemIdData[0] == "hair")
            {
                byte hairOverlayIdx, colour1, colour2;

                if (!byte.TryParse(iData[1], out hairOverlayIdx) || !byte.TryParse(iData[2], out colour1) || !byte.TryParse(iData[3], out colour2))
                    return false;

                // hair overlay & colours validation

                pData.HairStyle.Id = variation;
                pData.HairStyle.Overlay = hairOverlayIdx;
                pData.HairStyle.Color = colour1;
                pData.HairStyle.Color2 = colour2;

                //MySQL.CharacterHairStyleUpdate(pData.Info);
            }
            else
            {
                float opacity = 1f;
                byte colour;

                if (!byte.TryParse(iData[1], out colour))
                    return false;

                if (itemIdData[0] != "beard" && itemIdData[0] != "chest" && itemIdData[0] != "eyebrows")
                {
                    if (iData.Length != 3)
                        return false;

                    if (!float.TryParse(iData[2], out opacity))
                        return false;
                }

                int headOverlayIdx;

                if (!HeadOverlayNums.TryGetValue(itemIdData[0], out headOverlayIdx))
                    return false;

                var headOverlay = pData.HeadOverlays[headOverlayIdx];

                headOverlay.Color = colour;
                headOverlay.SecondaryColor = colour;
                headOverlay.Index = variation;
                headOverlay.Opacity = opacity;

                //MySQL.CharacterHeadOverlaysUpdate(pData.Info);
            }

            PaymentProceed(pData, useCash, res.Value.MatPrice, res.Value.RealPrice);

            return true;
        }
    }

    public class Market : Shop
    {
        public override string ClientData => $"{ID}, {PositionInfo.ToCSharpStr()}, {GovPrice}, {Rent}, {Tax}f, {PositionInteract.ToCSharpStr()}";

        public Market(int ID, Vector3 PositionInfo, Utils.Vector4 PositionInteract) : base(ID, PositionInfo, PositionInteract, Types.Market)
        {

        }
    }

    public class GasStation : Shop
    {
        public override string ClientData => $"{ID}, {PositionInfo.ToCSharpStr()}, {GovPrice}, {Rent}, {Tax}f, {GasolinesPosition.ToCSharpStr()}, {PositionInteract.ToCSharpStr()}";

        private static Dictionary<Game.Data.Vehicles.Vehicle.FuelTypes, int> GasPrices = new Dictionary<Game.Data.Vehicles.Vehicle.FuelTypes, int>()
        {
            { Game.Data.Vehicles.Vehicle.FuelTypes.Petrol, 10 },

            { Game.Data.Vehicles.Vehicle.FuelTypes.Electricity, 5 },
        };

        public Vector3 GasolinesPosition { get; set; }

        public int GetGasPrice(Game.Data.Vehicles.Vehicle.FuelTypes fType, bool addMargin)
        {
            int price;

            if (!GasPrices.TryGetValue(fType, out price))
                return -1;

            if (addMargin)
                return (int)Math.Floor(price * Margin);
            else
                return price;
        }

        public GasStation(int ID, Vector3 PositionInfo,  Utils.Vector4 PositionInteract, Vector3 GasolinesPosition) : base(ID, PositionInfo, PositionInteract, Types.GasStation)
        {
            this.GasolinesPosition = GasolinesPosition;
        }
    }

    public abstract class VehicleShop : Shop, IEnterable
    {
        public override string ClientData => $"{ID}, {PositionInfo.ToCSharpStr()}, {GovPrice}, {Rent}, {Tax}f, {PositionInteract.ToCSharpStr()}";

        public Utils.Vector4 EnterProperties { get; set; }

        public Utils.Vector4[] AfterBuyPositions { get; set; }

        public Utils.Vector4[] ExitProperties { get; set; }

        public int LastExitUsed { get; set; }

        public int LastAfterBuyExitUsed { get; set; }

        public VehicleShop(int ID, Vector3 PositionInfo, Utils.Vector4 EnterProperties, Types Type, Utils.Vector4[] AfterBuyPositions, Utils.Vector4 PositionInteract) : base(ID, PositionInfo, PositionInteract, Type)
        {
            this.EnterProperties = EnterProperties;

            this.AfterBuyPositions = AfterBuyPositions;

            this.ExitProperties = new Utils.Vector4[] { new Utils.Vector4(PositionInteract.Position.GetFrontOf(PositionInteract.RotationZ, 1.5f), Utils.GetOppositeAngle(PositionInteract.RotationZ)) };
        }

        public override bool BuyItem(PlayerData pData, bool useCash, string itemId)
        {
            var iData = itemId.Split('_');

            if (iData.Length != 7)
                return false;

            byte r1, g1, b1, r2, g2, b2;

            if (!byte.TryParse(iData[1], out r1) || !byte.TryParse(iData[2], out g1) || !byte.TryParse(iData[3], out b1) || !byte.TryParse(iData[4], out r2) || !byte.TryParse(iData[5], out g2) || !byte.TryParse(iData[6], out b2))
                return false;

            var res = CanBuy(pData, useCash, iData[0], 1);

            if (res == null)
                return false;

            var vType = Data.Vehicles.GetData(iData[0]);

            if (vType == null)
                return false;

            var vPos = AfterBuyPositions[AfterBuyPositions.Length == 1 ? 0 : AfterBuyPositions.Length < LastExitUsed + 1 ? ++LastExitUsed : LastExitUsed = 0];

            var vData = VehicleData.New(pData, vType, new Utils.Colour(r1, g1, b1), new Utils.Colour(r2, g2, b2), vPos.Position, vPos.RotationZ, Utils.Dimensions.Main, true);

            PaymentProceed(pData, useCash, res.Value.MatPrice, res.Value.RealPrice);

            Sync.Players.ExitFromBuiness(pData, false);

            pData.Player.Teleport(vPos.Position, false, Utils.Dimensions.Main, vPos.RotationZ, true);

            return true;
        }
    }

    public class CarShop1 : VehicleShop
    {
        public CarShop1(int ID, Vector3 Position, Utils.Vector4 EnterProperties, Utils.Vector4[] AfterBuyPositions, Utils.Vector4 PositionInteract) : base(ID, Position, EnterProperties, Types.CarShop1, AfterBuyPositions, PositionInteract)
        {

        }
    }

    public class CarShop2 : VehicleShop
    {
        public CarShop2(int ID, Vector3 Position, Utils.Vector4 EnterProperties, Utils.Vector4[] AfterBuyPositions, Utils.Vector4 PositionInteract) : base(ID, Position, EnterProperties, Types.CarShop2, AfterBuyPositions, PositionInteract)
        {

        }
    }

    public class CarShop3 : VehicleShop
    {
        public CarShop3(int ID, Vector3 Position, Utils.Vector4 EnterProperties, Utils.Vector4[] AfterBuyPositions, Utils.Vector4 PositionInteract) : base(ID, Position, EnterProperties, Types.CarShop3, AfterBuyPositions, PositionInteract)
        {

        }
    }

    public class MotoShop : VehicleShop
    {
        public MotoShop(int ID, Vector3 Position, Utils.Vector4 EnterProperties, Utils.Vector4[] AfterBuyPositions, Utils.Vector4 PositionInteract) : base(ID, Position, EnterProperties, Types.MotoShop, AfterBuyPositions, PositionInteract)
        {

        }
    }

    public class BoatShop : VehicleShop
    {
        public BoatShop(int ID, Vector3 Position, Utils.Vector4 EnterProperties, Utils.Vector4[] AfterBuyPositions, Utils.Vector4 PositionInteract) : base(ID, Position, EnterProperties, Types.BoatShop, AfterBuyPositions, PositionInteract)
        {

        }
    }

    public class AeroShop : VehicleShop
    {
        public AeroShop(int ID, Vector3 Position, Utils.Vector4 EnterProperties, Utils.Vector4[] AfterBuyPositions, Utils.Vector4 PositionInteract) : base(ID, Position, EnterProperties, Types.AeroShop, AfterBuyPositions, PositionInteract)
        {

        }
    }

    public class TuningShop : Shop, IEnterable
    {
        public override string ClientData => $"{ID}, {PositionInfo.ToCSharpStr()}, {GovPrice}, {Rent}, {Tax}f, {PositionInteract.ToCSharpStr()}";

        public Utils.Vector4 EnterProperties { get; set; }

        public Utils.Vector4[] ExitProperties { get; set; }

        public int LastExitUsed { get; set; }

        private static Dictionary<Game.Data.Vehicles.Vehicle.ClassTypes, float> VehicleClassMargins = new Dictionary<Data.Vehicles.Vehicle.ClassTypes, float>()
        {
            { Data.Vehicles.Vehicle.ClassTypes.Classic, 1f },
            { Data.Vehicles.Vehicle.ClassTypes.Premium, 1.5f },
            { Data.Vehicles.Vehicle.ClassTypes.Luxe, 2f },
            { Data.Vehicles.Vehicle.ClassTypes.Elite, 2.5f },
        };

        private static Dictionary<string, byte> ModSlots = new Dictionary<string, byte>()
        {
            { "engine", 11 },
            { "brakes", 12 },
            { "trm", 13 },
            { "susp", 15 },
            { "horn", 14 },
            { "spoiler", 0 },
            { "fbump", 1 },
            { "rbump", 2 },
            { "skirt", 3 },
            { "exh", 4 },
            { "frame", 5 },
            { "grill", 6 },
            { "hood", 7 },
            { "roof", 10 },
            { "livery", 48 },
            { "swheel", 33 },
            { "seats", 32 },
        };

        public TuningShop(int ID, Vector3 PositionInfo, Utils.Vector4 EnterProperties, Utils.Vector4[] ExitProperties, Utils.Vector4 PositionInteract) : base(ID, PositionInfo, PositionInteract, Types.TuningShop)
        {
            this.EnterProperties = EnterProperties;

            this.ExitProperties = ExitProperties;
        }

        public float GetVehicleClassMargin(Data.Vehicles.Vehicle.ClassTypes cType) => VehicleClassMargins[cType];

        public byte? GetModSlot(string id)
        {
            byte mod;

            if (!ModSlots.TryGetValue(id, out mod))
                return null;

            return mod;
        }

        public override bool BuyItem(PlayerData pData, bool useCash, string item)
        {
            var vData = pData.CurrentTuningVehicle;

            if (vData?.Vehicle?.Exists != true)
                return false;

            var iData = item.Split('_');

            if (iData.Length <= 1)
                return false;

            byte p;

            if (!byte.TryParse(iData[1], out p))
                return false;

            var slot = GetModSlot(iData[0]);

            (int MatPrice, int RealPrice)? res = null;

            if (slot is byte bSlot)
            {
                if (iData[0] == "engine" || iData[0] == "brakes" || iData[0] == "trm" || iData[0] == "susp")
                {
                    res = CanBuy(pData, useCash, item, 1);
                }
                else
                {
                    res = CanBuy(pData, useCash, iData[0], 1);
                }

                if (res == null)
                    return false;

                if (p == 0)
                    p = 255;
                else
                    p--;

                vData.Tuning.Mods[bSlot] = p;

                MySQL.VehicleTuningUpdate(vData.Info);
            }
            else
            {
                if (iData[0] == "fix")
                {
                    res = CanBuy(pData, useCash, item, 1);

                    if (res == null)
                        return false;

                    if (p == 0)
                    {
                        vData.Vehicle.SetFixed();
                    }
                    else
                    {
                        vData.Vehicle.SetVisualFixed();
                    }
                }
                else if (iData[0] == "keys")
                {

                }
                else if (iData[0] == "wheel" || iData[0] == "rwheel")
                {
                    if (iData.Length != 3)
                        return false;

                    byte n;

                    if (!byte.TryParse(iData[2], out n))
                        return false;

                    res = CanBuy(pData, useCash, $"{iData[0]}_{iData[1]}", 1);

                    if (res == null)
                        return false;

                    if (vData.Data.Type == Game.Data.Vehicles.Vehicle.Types.Motorcycle)
                    {
                        vData.Tuning.WheelsType = 6;
                    }
                    else
                    {
                        if (p > 0)
                            p--;

                        vData.Tuning.WheelsType = p;
                    }

                    if (n == 0)
                        n = 255;

                    vData.Tuning.Mods[(byte)(iData[0] == "wheel" ? 23 : 24)] = n;

                    MySQL.VehicleTuningUpdate(vData.Info);
                }
                else if (iData[0] == "neon")
                {
                    if (iData.Length == 2)
                    {
                        res = CanBuy(pData, useCash, item, 1);

                        if (res == null)
                            return false;

                        vData.Tuning.NeonColour = null;
                    }
                    else if (iData.Length == 4)
                    {
                        byte g, b;

                        if (!byte.TryParse(iData[2], out g) || !byte.TryParse(iData[3], out b))
                            return false;

                        res = CanBuy(pData, useCash, iData[0], 1);

                        if (res == null)
                            return false;

                        if (vData.Tuning.NeonColour == null)
                        {
                            vData.Tuning.NeonColour = new Utils.Colour(p, g, b, 255);
                        }
                        else
                        {
                            vData.Tuning.NeonColour.Red = p;
                            vData.Tuning.NeonColour.Green = g;
                            vData.Tuning.NeonColour.Blue = b;
                        }
                    }
                    else
                        return false;

                    MySQL.VehicleTuningUpdate(vData.Info);
                }
                else if (iData[0] == "tsmoke")
                {
                    if (iData.Length == 2)
                    {
                        res = CanBuy(pData, useCash, item, 1);

                        if (res == null)
                            return false;

                        vData.Tuning.TyresSmokeColour = null;
                    }
                    else if (iData.Length == 4)
                    {
                        byte g, b;

                        if (!byte.TryParse(iData[2], out g) || !byte.TryParse(iData[3], out b))
                            return false;

                        res = CanBuy(pData, useCash, iData[0], 1);

                        if (res == null)
                            return false;

                        if (vData.Tuning.TyresSmokeColour == null)
                        {
                            vData.Tuning.TyresSmokeColour = new Utils.Colour(p, g, b, 255);
                        }
                        else
                        {
                            vData.Tuning.TyresSmokeColour.Red = p;
                            vData.Tuning.TyresSmokeColour.Green = g;
                            vData.Tuning.TyresSmokeColour.Blue = b;
                        }
                    }
                    else
                        return false;

                    MySQL.VehicleTuningUpdate(vData.Info);
                }
                else if (iData[0] == "colour")
                {
                    if (iData.Length != 7)
                        return false;

                    byte g1, b1, r2, g2, b2;

                    if (!byte.TryParse(iData[2], out g1) || !byte.TryParse(iData[3], out b1) || !byte.TryParse(iData[4], out r2) || !byte.TryParse(iData[5], out g2) || !byte.TryParse(iData[6], out b2))
                        return false;

                    res = CanBuy(pData, useCash, iData[0], 1);

                    if (res == null)
                        return false;

                    vData.Tuning.Colour1.Red = p;
                    vData.Tuning.Colour1.Green = g1;
                    vData.Tuning.Colour1.Blue = b1;

                    vData.Tuning.Colour2.Red = r2;
                    vData.Tuning.Colour2.Green = g2;
                    vData.Tuning.Colour2.Blue = b2;

                    MySQL.VehicleTuningUpdate(vData.Info);
                }
                else
                {
                    if (iData[0] == "pearl")
                    {
                        res = CanBuy(pData, useCash, p == 0 ? item : iData[0], 1);

                        if (res == null)
                            return false;

                        vData.Tuning.PearlescentColour = p;

                        MySQL.VehicleTuningUpdate(vData.Info);
                    }
                    else if (iData[0] == "wcolour")
                    {
                        res = CanBuy(pData, useCash, p == 0 ? item : iData[0], 1);

                        if (res == null)
                            return false;

                        vData.Tuning.WheelsColour = p;

                        MySQL.VehicleTuningUpdate(vData.Info);
                    }
                    else
                    {
                        res = CanBuy(pData, useCash, item, 1);

                        if (res == null)
                            return false;

                        if (iData[0] == "colourt")
                        {
                            vData.Tuning.ColourType = p;
                        }
                        else if (iData[0] == "tt")
                        {
                            vData.Tuning.Turbo = p == 1;
                        }
                        else if (iData[0] == "wtint")
                        {
                            vData.Tuning.WindowTint = p;
                        }
                        else if (iData[0] == "xenon")
                        {
                            vData.Tuning.Xenon = (sbyte)(p - 2);
                        }
                        else
                            return false;

                        MySQL.VehicleTuningUpdate(vData.Info);
                    }
                }
            }

            if (res == null)
                return false;

            PaymentProceed(pData, useCash, res.Value.MatPrice, res.Value.RealPrice);

            return true;
        }
    }

    public class WeaponShop : Shop
    {
        public override string ClientData => $"{ID}, {PositionInfo.ToCSharpStr()}, {GovPrice}, {Rent}, {Tax}f, {PositionInteract.ToCSharpStr()}, {PositionShootingRangeEnter.Position.ToCSharpStr()}";

        public Utils.Vector4 PositionShootingRangeEnter { get; set; }

        public static int ShootingRangePrice { get => Sync.World.GetSharedData<int>("SRange::Price"); set => Sync.World.SetSharedData("SRange::Price", value); }

        public static Utils.Vector4 ShootingRangePosition { get; private set; } = new Utils.Vector4(13.00517f, -1098.977f, 29.79701f, 337.5131f);

        public WeaponShop(int ID, Vector3 PositionInfo, Utils.Vector4 PositionInteract, Utils.Vector4 PositionShootingRangeEnter) : base(ID, PositionInfo, PositionInteract, Types.WeaponShop)
        {
            PositionShootingRangeEnter.Position.Z -= 0.5f;

            this.PositionShootingRangeEnter = PositionShootingRangeEnter;
        }

        public bool IsPlayerNearShootingRangeEnterPosition(PlayerData pData)
        {
            return Vector3.Distance(pData.Player.Position, PositionShootingRangeEnter.Position) <= 10f;
        }

        public bool BuyShootingRange(PlayerData pData)
        {
            var price = ShootingRangePrice;

            if (!pData.HasEnoughCash(price))
                return false;

            PaymentProceed(pData, true, price);

            return true;
        }
    }

    /*    public class Masks : Business
        {
            public static Dictionary<string, int> Prices = new Dictionary<string, int>()
            {

            };

            private static int Counter = 1;

            public Masks(int ID, Vector3 Position, Vector3 PositionInfo, Vector3 PositionEnter) : base(ID, PositionInfo)
            {
                Blip = NAPI.Blip.CreateBlip(362, Position, 1f, 5, "", 255, 0, true);

                SubID = Counter++;
            }
        }

        public class Bags : Business
        {
            public static Dictionary<string, int> Prices = new Dictionary<string, int>()
            {

            };

            private static int Counter = 1;

            public Bags(int ID, Vector3 Position, Vector3 PositionInfo, Vector3 PositionEnter) : base(ID, PositionInfo, PositionEnter)
            {
                Blip = NAPI.Blip.CreateBlip(676, Position, 1f, 5, "", 255, 0, true);

                SubID = Counter++;
            }
        }

        public class Jewellery : Business
        {
            public static Dictionary<string, int> Prices = new Dictionary<string, int>()
            {

            };

            private static int Counter = 1;

            public Jewellery(int ID, Vector3 Position, Vector3 PositionInfo, Vector3 PositionEnter) : base(ID, PositionInfo, PositionEnter)
            {
                Blip = NAPI.Blip.CreateBlip(617, Position, 1f, 5, "", 255, 0, true);

                SubID = Counter++;
            }
        }

        public class Shop : Business
        {
            public static Dictionary<string, int> Prices = new Dictionary<string, int>()
            {

            };

            private static int Counter = 1;

            public Shop(int ID, Vector3 Position, Vector3 PositionInfo, Vector3 PositionEnter) : base(ID, PositionInfo, PositionEnter)
            {
                Blip = NAPI.Blip.CreateBlip(11, Position, 1f, 5, "", 255, 0, true);

                SubID = Counter++;
            }
        }

        public class WeaponShop : Business
        {
            public static Dictionary<string, int> Prices = new Dictionary<string, int>()
            {

            };

            private static int Counter = 1;

            public WeaponShop(int ID, Vector3 Position, Vector3 PositionInfo, Vector3 PositionEnter) : base(ID, PositionInfo, PositionEnter)
            {
                Blip = NAPI.Blip.CreateBlip(110, Position, 1f, 5, "", 255, 0, true);

                SubID = Counter++;
            }
        }

        public class Petrol : Business
        {
            public static Dictionary<string, int> Prices = new Dictionary<string, int>()
            {

            };

            private static int Counter = 1;

            public Petrol(int ID, Vector3 Position, Vector3 PositionInfo, Vector3 PositionEnter) : base(ID, PositionInfo, PositionEnter)
            {
                Blip = NAPI.Blip.CreateBlip(361, Position, 1f, 5, "", 255, 0, true);

                SubID = Counter++;
            }
        }

        public class Farm : Business
        {
            public Farm(int ID, Vector3 PositionInfo, Vector3 Position) : base(ID, PositionInfo, Position)
            {
                Blip = NAPI.Blip.CreateBlip(73, PositionInfo, 1f, 0, "");

            }
        }*/
}
