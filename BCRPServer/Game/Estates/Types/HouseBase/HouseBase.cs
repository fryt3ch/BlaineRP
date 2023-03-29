using GTANetworkAPI;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;

namespace BCRPServer.Game.Estates
{
    public abstract class HouseBase
    {
        public static Utils.Colour DefaultLightColour => new Utils.Colour(255, 187, 96, 255);

        public class Style
        {
            /// <summary>Типы планировки</summary>
            public enum Types
            {
                First = 0,
                Second,
                Third,
                Fourth,
                Fifth,
            }

            /// <summary>Типы комнат</summary>
            public enum RoomTypes
            {
                One = 1,
                Two = 2,
                Three = 3,
                Four = 4,
                Five = 5,
            }

            public Types Type { get; private set; }

            public RoomTypes RoomType { get; private set; }

            public HouseBase.Types HouseType { get; private set; }

            public Vector3 Position { get; private set; }

            public float Heading { get; private set; }

            public int LightsCount { get; private set; }

            public int DoorsCount { get; private set; }

            /// <summary>Словарь планировок</summary>
            private static Dictionary<HouseBase.Types, Dictionary<RoomTypes, Dictionary<Types, Style>>> All { get; set; } = new Dictionary<HouseBase.Types, Dictionary<RoomTypes, Dictionary<Types, Style>>>();

            public static Style Get(HouseBase.Types hType, RoomTypes rType, Types sType) => All.GetValueOrDefault(hType)?.GetValueOrDefault(rType)?.GetValueOrDefault(sType);

            public Style(HouseBase.Types HouseType, RoomTypes RoomType, Types Type, Vector3 Position, float Heading, int LightsCount = 0, int DoorsCount = 0)
            {
                this.HouseType = HouseType;
                this.RoomType = RoomType;

                this.Type = Type;

                this.Position = Position;
                this.Heading = Heading;

                this.LightsCount = LightsCount;
                this.DoorsCount = DoorsCount;

                if (!All.ContainsKey(HouseType))
                    All.Add(HouseType, new Dictionary<RoomTypes, Dictionary<Types, Style>>());

                if (!All[HouseType].ContainsKey(RoomType))
                    All[HouseType].Add(RoomType, new Dictionary<Types, Style>());

                All[HouseType][RoomType].Add(Type, this);
            }

            public static void LoadAll()
            {
                new Style(HouseBase.Types.House, RoomTypes.Two, Types.First, new Vector3(67.955511f, 70.03592f, -9f), 272f, 6, 5);
                new Style(HouseBase.Types.House, RoomTypes.Two, Types.Second, new Vector3(67.955511f, 70.03592f, -19f), 272f, 6, 5);

                new Style(HouseBase.Types.Apartments, RoomTypes.Two, Types.First, new Vector3(67.955511f, 70.03592f, -9f), 272f, 6, 5);
                new Style(HouseBase.Types.Apartments, RoomTypes.Two, Types.Second, new Vector3(67.955511f, 70.03592f, -19f), 272f, 6, 5);

                Game.Items.Container.AllSIDs.Add("h_locker", new Items.Container.Data(50, 150f, Items.Container.AllowedItemTypes.All, Items.Container.ContainerTypes.Locker));
                Game.Items.Container.AllSIDs.Add("h_wardrobe", new Items.Container.Data(50, 80f, Items.Container.AllowedItemTypes.Wardrobe, Items.Container.ContainerTypes.Wardrobe));
                Game.Items.Container.AllSIDs.Add("h_fridge", new Items.Container.Data(50, 100f, Items.Container.AllowedItemTypes.Fridge, Items.Container.ContainerTypes.Fridge));

                Game.Items.Container.AllSIDs.Add("a_locker", new Items.Container.Data(50, 150f, Items.Container.AllowedItemTypes.All, Items.Container.ContainerTypes.Locker));
                Game.Items.Container.AllSIDs.Add("a_wardrobe", new Items.Container.Data(50, 80f, Items.Container.AllowedItemTypes.Wardrobe, Items.Container.ContainerTypes.Wardrobe));
                Game.Items.Container.AllSIDs.Add("a_fridge", new Items.Container.Data(50, 100f, Items.Container.AllowedItemTypes.Fridge, Items.Container.ContainerTypes.Fridge));
            }
        }

        public class Light
        {
            [JsonProperty(PropertyName = "S")]
            public bool State { get; set; }

            [JsonProperty(PropertyName = "C")]
            public Utils.Colour Colour { get; set; }

            public Light(bool State, Utils.Colour Colour)
            {
                this.State = State;
                this.Colour = Colour;
            }

            public Light() { }
        }

        /// <summary>Типы домов</summary>
        public enum Types
        {
            /// <summary>Дом</summary>
            House = 0,
            /// <summary>Квартира</summary>
            Apartments,
        }

        public enum ClassTypes
        {
            A = 0,
            B,
            C,
            D,

            FA,
            FB,
            FC,
            FD,
        }

        private static Dictionary<ClassTypes, int> Taxes = new Dictionary<ClassTypes, int>()
        {
            { ClassTypes.A, 50 },
            { ClassTypes.B, 75 },
            { ClassTypes.C, 90 },
            { ClassTypes.D, 100 },

            { ClassTypes.FA, 50 },
            { ClassTypes.FB, 75 },
            { ClassTypes.FC, 90 },
            { ClassTypes.FD, 100 },
        };

        public static int GetTax(ClassTypes cType) => Taxes[cType];

        public static ClassTypes GetClass(HouseBase house)
        {
            if (house.Type == Types.House)
            {
                if (house.Price <= 100_000)
                    return ClassTypes.A;

                if (house.Price <= 500_000)
                    return ClassTypes.B;

                if (house.Price <= 1_000_000)
                    return ClassTypes.C;

                return ClassTypes.D;
            }
            else
            {
                if (house.Price <= 100_000)
                    return ClassTypes.FA;

                if (house.Price <= 500_000)
                    return ClassTypes.FB;

                if (house.Price <= 1_000_000)
                    return ClassTypes.FC;

                return ClassTypes.FD;
            }
        }

        /// <summary>ID дома</summary>
        public uint Id { get; set; }

        /// <summary>Тип дома</summary>
        public Types Type { get; set; }

        public Style.RoomTypes RoomType { get; set; }

        /// <summary>Тип планировки</summary>
        public Style StyleData { get; set; }

        /// <summary>Владелец</summary>
        public PlayerData.PlayerInfo Owner { get; set; }

        /// <summary>Список сожителей</summary>
        /// <remarks>0 - свет, 1 - двери, 2 - шкаф, 3 - гардероб, 4 - холодильник</remarks>
        public Dictionary<PlayerData.PlayerInfo, bool[]> Settlers { get; set; }

        /// <summary>Баланс дома</summary>
        public ulong Balance { get; set; }

        /// <summary>Заблокированы ли двери?</summary>
        public bool IsLocked { get; set; }

        public bool ContainersLocked { get; set; }

        /// <summary>Налог</summary>
        public int Tax => GetTax(Class);

        public Utils.Vector4 PositionParams { get; set; }

        public uint Locker { get; set; }

        public uint Wardrobe { get; set; }

        public uint Fridge { get; set; }

        /// <summary>Список FID мебели в доме</summary>
        public List<Furniture> Furniture { get; set; }

        public Light[] LightsStates { get; set; }

        public bool[] DoorsStates { get; set; }

        /// <summary>Стандартная цена дома</summary>
        public int Price { get; set; }

        public uint Dimension { get; set; }

        public ClassTypes Class { get; set; }

        public HouseBase(uint ID, Utils.Vector4 PositionParams, Types Type, Style.RoomTypes RoomType)
        {
            this.Type = Type;

            this.RoomType = RoomType;

            this.Id = ID;

            this.PositionParams = PositionParams;

            this.Class = GetClass(this);
        }

        public void TriggerEventForHouseOwners(string eventName, params object[] args)
        {
            var players = Settlers.Keys.Where(x => x?.PlayerData?.Player.Dimension == Dimension).Select(x => x.PlayerData.Player).ToList();

            if (players.Count > 0)
            {
                if (Owner?.PlayerData?.Player.Dimension == Dimension)
                    players.Add(Owner.PlayerData.Player);

                NAPI.ClientEvent.TriggerClientEventToPlayers(players.ToArray(), eventName, args);
            }
            else
            {
                if (Owner?.PlayerData?.Player.Dimension == Dimension)
                    Owner.PlayerData.Player.TriggerEvent(eventName, args);
            }
        }

        public virtual void UpdateOwner(PlayerData.PlayerInfo pInfo)
        {
            Owner = pInfo;
        }

        public string ToClientJson()
        {
            var data = new JObject();

            data.Add("I", Id);
            data.Add("T", (int)Type);
            data.Add("S", (int)StyleData.Type);
            data.Add("Dim", Dimension);

            data.Add("LI", Locker);
            data.Add("WI", Wardrobe);
            data.Add("FI", Fridge);

            data.Add("DS", DoorsStates.SerializeToJson());
            data.Add("LS", LightsStates.SerializeToJson());
            data.Add("F", Furniture.SerializeToJson());

            return data.SerializeToJson();
        }

        public abstract void SetPlayersInside(bool teleport, params Player[] players);

        public abstract void SetPlayersOutside(bool teleport, params Player[] players);

        public abstract bool IsEntityNearEnter(Entity entity);

        public abstract void ChangeOwner(PlayerData.PlayerInfo pInfo, bool buyGov = false);

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
                var totalMoney = (uint)Price / 2;

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

            ContainersLocked = true;
            IsLocked = false;

            SetBalance(0, null);

            ChangeOwner(null);
        }

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

            MySQL.HouseUpdateBalance(this);
        }

        public void SettlePlayer(PlayerData.PlayerInfo pInfo, bool state, PlayerData pDataInit = null)
        {
            if (state)
            {
                if (Settlers.ContainsKey(pInfo))
                    return;

                TriggerEventForHouseOwners("HouseMenu::SettlerUpd", $"{pInfo.CID}_{pInfo.Name}_{pInfo.Surname}");

                Settlers.Add(pInfo, new bool[5]);

                if (pInfo.PlayerData != null)
                {
                    pInfo.PlayerData.SettledHouseBase = this;

                    if (pDataInit != null)
                        pInfo.PlayerData.Player.TriggerEvent("Player::SettledHB", (int)Type, Id, true, pDataInit.Player.Id);
                    else
                        pInfo.PlayerData.Player.TriggerEvent("Player::SettledHB", (int)Type, Id, true);
                }
            }
            else
            {
                Settlers.Remove(pInfo);

                if (pDataInit?.Info == pInfo)
                {
                    pDataInit.Player.CloseAll(true);
                }

                TriggerEventForHouseOwners("HouseMenu::SettlerUpd", pInfo.CID);

                if (pInfo.PlayerData != null)
                {
                    pInfo.PlayerData.SettledHouseBase = null;

                    if (pDataInit != null)
                        pInfo.PlayerData.Player.TriggerEvent("Player::SettledHB", (int)Type, Id, false, pDataInit.Player.Id);
                    else
                        pInfo.PlayerData.Player.TriggerEvent("Player::SettledHB", (int)Type, Id, false);
                }
            }

            MySQL.HouseUpdateSettlers(this);
        }

        public bool BuyFromGov(PlayerData pData)
        {
            ulong newCash;

            if (!pData.TryRemoveCash((uint)Price, out newCash, true))
                return false;

            if (pData.SettledHouseBase?.Type == Type)
            {
                pData.Player.Notify(Type == Types.House ? "Trade::ASH" : "Trade::ASA");

                return false;
            }

            if (Type == Types.House)
            {
                if (pData.HouseSlots <= 0)
                {
                    pData.Player.Notify("Trade::MHOW", pData.OwnedHouses.Count);

                    return false;
                }
            }
            else
            {
                if (pData.ApartmentsSlots <= 0)
                {
                    pData.Player.Notify("Trade::MAOW", pData.OwnedApartments.Count);

                    return false;
                }
            }

            pData.SetCash(newCash);

            ChangeOwner(pData.Info, true);

            return true;
        }
    }
}
