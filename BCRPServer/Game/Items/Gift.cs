using GTANetworkAPI;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using static BCRPServer.Locale.Chat;

namespace BCRPServer.Game.Items
{
    public class Gift
    {
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
        public int ID { get; set; }

        /// <summary>CID владельца</summary>
        public int CID { get; set; }

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
        public Gift(int CID, int ID, SourceTypes SourceType, Types Type, string GID = null, int Variation = 0, int Amount = 1) : this(CID, SourceType, Type, GID, Variation, Amount)
        {
            this.ID = ID;
        }

        public Gift(int CID, SourceTypes SourceType, Types Type, string GID = null, int Variation = 0, int Amount = 1)
        {
            this.ID = -1;

            this.CID = CID;
            this.Amount = Amount;
            this.Type = Type;
            this.GID = GID;
            this.Variation = Variation;
            this.Amount = Amount;

            this.SourceType = SourceType;
        }

        /// <summary>Метод для удаления подарка</summary>
        public async Task Delete() => await MySQL.DeleteGift(this);

        /// <summary>Метод для выдачи подарка игроку</summary>
        /// <param name="pData">PlayerData игрока</param>
        /// <param name="type">Тип подарка</param>
        /// <param name="GID">ID предмета/транспорта</param>
        /// <param name="variation">Вариация предмета</param>
        /// <param name="amount">Кол-во</param>
        /// <param name="sType">Источник выдачи</param>
        /// <param name="notify">Уведомить ли игрока?</param>
        /// <param name="spaceHint">Уведомить, но с подсказкой об освобождении места в инвентаре?</param>
        public static async Task<Gift> Give(PlayerData pData, Types type, string GID = null, int variation = 0, int amount = 1, SourceTypes sType = SourceTypes.Server, bool notify = false, bool spaceHint = false)
        {
            var player = pData.Player;

            var gift = MySQL.AddGift(new Gift(pData.CID, sType, type, GID, variation, amount));

            if (gift == null)
                return null;

            await NAPI.Task.RunAsync(() =>
            {
                if (player?.Exists != true)
                    return;

                player.TriggerEvent("Menu::Gifts::Update", true, gift.ID, notify, spaceHint, (int)type, GID, amount, (int)sType);
            });

            return gift;
        }

        /// <summary>Метод для распаковки подарка</summary>
        /// <param name="pData">PlayerData игрока</param>
        /// <returns>true - если подарок распакован успешно, false - в противном случае</returns>
        public async Task<bool> Collect(PlayerData pData)
        {
            if (Type == Types.Item)
            {
                return await Game.Items.Items.GiveItem(pData, GID, Variation, Amount, false) != null;
            }
            else if (Type == Types.Vehicle)
            {

            }
            else if (Type == Types.Money)
            {
                return await pData.AddCash(Amount);
            }

            return false;
        }
    }
}
