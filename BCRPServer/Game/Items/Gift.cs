﻿using GTANetworkAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace BCRPServer.Game.Items
{
    public class Gift
    {
        private static Queue<uint> FreeIDs { get; set; } = new Queue<uint>();

        private static uint LastAddedMaxId { get; set; }

        public static uint MoveNextId()
        {
            uint id;

            if (!FreeIDs.TryDequeue(out id))
            {
                id = ++LastAddedMaxId;
            }

            return id;
        }

        public static void AddFreeId(uint id) => FreeIDs.Enqueue(id);

        public static void AddOnLoad(Gift gift)
        {
            if (gift == null)
                return;

            if (gift.ID > LastAddedMaxId)
                LastAddedMaxId = gift.ID;
        }

        public static void Add(Gift gift, uint cid)
        {
            if (gift == null)
                return;

            MySQL.GiftAdd(gift, cid);
        }

        public static void Remove(Gift gift)
        {
            if (gift == null)
                return;

            var id = gift.ID;

            AddFreeId(id);

            MySQL.GiftDelete(gift);
        }

        public class Prototype
        {
            public Types Type { get; set; }

            public SourceTypes SourceType { get; set; }

            public string GID { get; set; }

            public int Variation { get; set; }

            public int Amount { get; set; }

            public Prototype(Types Type, SourceTypes SourceType, string GID, int Vatiation, int Amount)
            {
                this.Type = Type;
                this.SourceType = SourceType;
                this.GID = GID;
                this.Variation = Variation;
                this.Amount = Amount;
            }

            public static Prototype CreateAchievement(Types Type, string GID, int Variation, int Amount) => new Prototype(Type, SourceTypes.Achievement, GID, Variation, Amount);
        }

        public enum Types
        {
            /// <summary>Предмет</summary>
            Item = 0,
            /// <summary>Транспорт</summary>
            Vehicle,
            /// <summary>Деньги</summary>
            Money
        }

        public enum SourceTypes
        {
            /// <summary>Сервер</summary>
            Server = 0,
            /// <summary>Магазин</summary>
            Shop,
            /// <summary>Выполненное достижение</summary>
            Achievement,
        }

        /// <summary>ID подарка</summary>
        public uint ID { get; set; }

        /// <summary>Тип подарка</summary>
        public Types Type { get; set; }

        /// <summary>Good ID - ID предмета/транспорта</summary>
        /// <remarks>Если тип предмета не подразумевает таковой - null</remarks>
        public string GID { get; set; }

        /// <summary>Номер вариации (в основном, для одежды)</summary>
        /// <remarks>Если тип предмета не подразумевает таковой - 0</remarks>
        public int Variation { get; set; }

        /// <summary>Кол-во подарка</summary>
        public int Amount { get; set; }

        /// <summary>Источник выдачи подарка</summary>
        public SourceTypes SourceType { get; set; }

        /// <summary>Конструктор для MySQL</summary>
        public Gift(uint ID, SourceTypes SourceType, Types Type, string GID = null, int Variation = 0, int Amount = 1) : this(SourceType, Type, GID, Variation, Amount)
        {
            this.ID = ID;

            this.Amount = Amount;
            this.Type = Type;
            this.GID = GID;
            this.Variation = Variation;
            this.Amount = Amount;

            this.SourceType = SourceType;
        }

        public Gift(SourceTypes SourceType, Types Type, string GID = null, int Variation = 0, int Amount = 1)
        {
            this.ID = MoveNextId();

            this.Amount = Amount;
            this.Type = Type;
            this.GID = GID;
            this.Variation = Variation;
            this.Amount = Amount;

            this.SourceType = SourceType;
        }

        /// <summary>Метод для удаления подарка</summary>
        public void Delete() => Remove(this);

        /// <summary>Метод для выдачи подарка игроку</summary>
        /// <param name="pData">PlayerData игрока</param>
        /// <param name="type">Тип подарка</param>
        /// <param name="GID">ID предмета/транспорта</param>
        /// <param name="variation">Вариация предмета</param>
        /// <param name="amount">Кол-во</param>
        /// <param name="sType">Источник выдачи</param>
        /// <param name="notify">Уведомить ли игрока?</param>
        /// <param name="spaceHint">Уведомить, но с подсказкой об освобождении места в инвентаре?</param>
        public static Gift Give(PlayerData.PlayerInfo pInfo, Types type, string GID = null, int variation = 0, int amount = 1, SourceTypes sType = SourceTypes.Server, bool notify = false)
        {
            var gift = new Gift(sType, type, GID, variation, amount);

            Add(gift, pInfo.CID);

            pInfo.Gifts.Add(gift);

            if (pInfo.PlayerData != null)
                pInfo.PlayerData.Player.TriggerEvent("Menu::Gifts::Update", true, gift.ID, notify, (int)type, GID, amount, (int)sType);

            return gift;
        }

        public static Gift Give(PlayerData.PlayerInfo pInfo, Prototype prototype, bool notify = false) => Give(pInfo, prototype.Type, prototype.GID, prototype.Variation, prototype.Amount, prototype.SourceType, notify);

        /// <summary>Метод для распаковки подарка</summary>
        /// <param name="pData">PlayerData игрока</param>
        /// <returns>true - если подарок распакован успешно, false - в противном случае</returns>
        public bool Collect(PlayerData pData)
        {
            if (Type == Types.Item)
            {
                return pData.GiveItem(GID, Variation, Amount);
            }
            else if (Type == Types.Vehicle)
            {

            }
            else if (Type == Types.Money)
            {
                return pData.AddCash(Amount);
            }

            return false;
        }
    }
}
