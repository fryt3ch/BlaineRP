using GTANetworkAPI;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;
using BlaineRP.Server.EntitiesData.Players;
using BlaineRP.Server.Extensions.System;
using BlaineRP.Server.UtilsT;

namespace BlaineRP.Server.Game.Estates
{
    public abstract partial class HouseBase
    {
        private static Dictionary<ClassTypes, uint> Taxes = new Dictionary<ClassTypes, uint>()
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

        public static uint GetTax(ClassTypes cType) => Taxes.GetValueOrDefault(cType, uint.MinValue);

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

        public ushort StyleType { get; set; }

        public Style StyleData => Style.Get(StyleType);

        /// <summary>Владелец</summary>
        public PlayerInfo Owner { get; set; }

        /// <summary>Список сожителей</summary>
        /// <remarks>0 - свет, 1 - двери, 2 - шкаф, 3 - гардероб, 4 - холодильник</remarks>
        public Dictionary<PlayerInfo, bool[]> Settlers { get; set; }

        /// <summary>Баланс дома</summary>
        public ulong Balance { get; set; }

        /// <summary>Заблокированы ли двери?</summary>
        public bool IsLocked { get; set; }

        public bool ContainersLocked { get; set; }

        /// <summary>Налог</summary>
        public uint Tax => GetTax(Class);

        public abstract Vector4 PositionParams { get; }

        public uint Locker { get; set; }

        public uint Wardrobe { get; set; }

        public uint Fridge { get; set; }

        /// <summary>Список FID мебели в доме</summary>
        public List<Furniture> Furniture { get; set; }

        public Light[] LightsStates { get; set; }

        public bool[] DoorsStates { get; set; }

        /// <summary>Стандартная цена дома</summary>
        public uint Price { get; set; }

        public uint Dimension { get; set; }

        public ClassTypes Class { get; set; }

        public HouseBase(uint ID, Types Type, Style.RoomTypes RoomType)
        {
            this.Type = Type;

            this.RoomType = RoomType;

            this.Id = ID;

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

        public virtual void UpdateOwner(PlayerInfo pInfo)
        {
            Owner = pInfo;
        }

        public string ToClientJson()
        {
            var data = new JObject();

            data.Add("I", Id);
            data.Add("T", (int)Type);
            data.Add("S", StyleType);

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

        public abstract void ChangeOwner(PlayerInfo pInfo, bool buyGov = false);

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

            ContainersLocked = true;
            IsLocked = false;

            for (int i = 0; i < DoorsStates.Length; i++)
                DoorsStates[i] = false;

            for (int i = 0; i < LightsStates.Length; i++)
            {
                LightsStates[i].Colour = DefaultLightColour;
                LightsStates[i].State = true;
            }

            MySQL.HouseUpdateLockState(this);
            MySQL.HouseUpdateContainersLockState(this);

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

        public void SettlePlayer(PlayerInfo pInfo, bool state, PlayerData pDataInit = null)
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

            if (!pData.TryRemoveCash(Price, out newCash, true))
                return false;

            if (pData.SettledHouseBase?.Type == Type)
            {
                pData.Player.Notify(Type == Types.House ? "Trade::ASH" : "Trade::ASA");

                return false;
            }

            if (Type == Types.House)
            {
                if (pData.FreeHouseSlots <= 0)
                {
                    pData.Player.Notify("Trade::MHOW", pData.OwnedHouses.Count);

                    return false;
                }
            }
            else
            {
                if (pData.FreeApartmentsSlots <= 0)
                {
                    pData.Player.Notify("Trade::MAOW", pData.OwnedApartments.Count);

                    return false;
                }
            }

            pData.SetCash(newCash);

            ChangeOwner(pData.Info, true);

            return true;
        }

        public void SetStyle(ushort styleId, Style style, bool furnitureRefund)
        {
            var ownerInfo = Owner;

            var oldStyleId = StyleType;
            var oldStyle = StyleData;

            StyleType = styleId;

            if (style.IsTypeFamiliar(oldStyleId))
            {
                var parentStyle = Style.Get(style.ParentType);

                var offsetOld = oldStyle.InteriorPosition.Position - parentStyle.InteriorPosition.Position;
                var offsetNew = style.InteriorPosition.Position - parentStyle.InteriorPosition.Position;

                foreach (var x in Furniture)
                {
                    x.Data.Position = x.Data.Position - offsetOld + offsetNew;

                    MySQL.FurnitureUpdate(x);
                }
            }
            else
            {
                if (furnitureRefund && ownerInfo != null)
                {
                    foreach (var x in Furniture)
                    {
                        x.Delete(this);
                    }

                    ownerInfo.AddFurniture(Furniture.ToArray());

                    Furniture.Clear();
                }
                else
                {
                    foreach (var x in Furniture)
                    {
                        x.Delete(this);

                        Game.Estates.Furniture.Remove(x);
                    }

                    Furniture.Clear();
                }

                LightsStates = new Light[style.LightsAmount];

                for (int i = 0; i < LightsStates.Length; i++)
                {
                    LightsStates[i] = new Light(true, DefaultLightColour);
                }

                DoorsStates = new bool[style.DoorsAmount];

                MySQL.HouseFurnitureUpdate(this);
                MySQL.HouseUpdateDoorsStates(this);
                MySQL.HouseUpdateLightsStates(this);
            }

            MySQL.HouseUpdateStyleType(this);

            var hDim = Dimension;

            var playersInside = PlayerData.All.Keys.Where(x => x.Dimension == hDim).ToArray();

            SetPlayersInside(true, playersInside);
            SetPlayersInside(false, playersInside);
        }
    }
}
