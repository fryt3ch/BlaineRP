using GTANetworkAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BCRPServer.Game.Items
{
    public class Gift
    {
        private static Queue<uint> FreeIDs { get; set; } = new Queue<uint>();

        public static Dictionary<uint, Gift> All { get; private set; } = new Dictionary<uint, Gift>();

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

            All.Add(gift.ID, gift);

            if (gift.ID > LastAddedMaxId)
                LastAddedMaxId = gift.ID;
        }

        public static void Add(Gift gift)
        {
            if (gift == null)
                return;

            All.Add(gift.ID, gift);

            MySQL.GiftAdd(gift);
        }

        public static void Remove(Gift gift)
        {
            if (gift == null)
                return;

            var id = gift.ID;

            AddFreeId(id);

            All.Remove(id);

            MySQL.GiftDelete(gift);
        }

        public static List<Gift> GetAllByCID(uint cid) => All.Values.Where(x => x?.CID == cid).ToList();

        public static Gift Get(uint id) => All.GetValueOrDefault(id);

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
        }

        /// <summary>ID подарка</summary>
        public uint ID { get; set; }

        /// <summary>CID владельца</summary>
        public uint CID { get; set; }

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
        public Gift(uint CID, uint ID, SourceTypes SourceType, Types Type, string GID = null, int Variation = 0, int Amount = 1) : this(CID, SourceType, Type, GID, Variation, Amount)
        {
            this.ID = ID;

            this.Amount = Amount;
            this.Type = Type;
            this.GID = GID;
            this.Variation = Variation;
            this.Amount = Amount;

            this.SourceType = SourceType;
        }

        public Gift(uint CID, SourceTypes SourceType, Types Type, string GID = null, int Variation = 0, int Amount = 1)
        {
            this.ID = MoveNextId();

            this.CID = CID;
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
        public static Gift Give(PlayerData pData, Types type, string GID = null, int variation = 0, int amount = 1, SourceTypes sType = SourceTypes.Server, bool notify = false, bool spaceHint = false)
        {
            var player = pData.Player;

            var gift = new Gift(pData.CID, sType, type, GID, variation, amount);

            Add(gift);

            player.TriggerEvent("Menu::Gifts::Update", true, gift.ID, notify, spaceHint, (int)type, GID, amount, (int)sType);

            return gift;
        }

        /// <summary>Метод для распаковки подарка</summary>
        /// <param name="pData">PlayerData игрока</param>
        /// <returns>true - если подарок распакован успешно, false - в противном случае</returns>
        public bool Collect(PlayerData pData)
        {
            if (Type == Types.Item)
            {
                return Game.Items.Items.GiveItem(pData, GID, Variation, Amount, false) != null;
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
