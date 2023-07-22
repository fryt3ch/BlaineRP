using GTANetworkAPI;
using System.Collections.Generic;

namespace BlaineRP.Server.Sync.Phone
{
    public class SMS
    {
        public enum DefaultTypes
        {
            DeliveryBusinessNewOrder = 0,
            DeliveryBusinessCancelOrder,
            DeliveryBusinessTakenOrder,
            DeliveryBusinessTakenCOrder,
            DeliveryBusinessFinishOrder,
        }

        public enum DefaultNumbers : uint
        {
            Bank = 900,

            Delivery = 873,
        }

        private static Dictionary<DefaultTypes, string> DefaultSmsMessages = new Dictionary<DefaultTypes, string>()
        {
            { DefaultTypes.DeliveryBusinessNewOrder, "Создан заказ №{0} для Вашего бизнеса, идет поиск курьера.\nКол-во материалов: {1}\nСумма оплаты: ${2}" },
            { DefaultTypes.DeliveryBusinessCancelOrder, "Заказ №{0} для Вашего бизнеса был отменен.\nВозвращено средств: ${1}" },
            { DefaultTypes.DeliveryBusinessTakenOrder, "Заказ №{0} для Вашего бизнеса в пути, Вы получите SMS по факту доставки или при возникновении проблем с ней" },
            { DefaultTypes.DeliveryBusinessTakenCOrder, "Заказ №{0} для Вашего бизнеса задерживается, идет поиск нового курьера" },
            { DefaultTypes.DeliveryBusinessFinishOrder, "Заказ №{0} для Вашего бизнеса был доставлен!" },
        };

        public static string GetDefaultSmsMessage(DefaultTypes type) => DefaultSmsMessages.GetValueOrDefault(type);

        public string Data { get; set; }

        public SMS(PlayerData.PlayerInfo SenderInfo, PlayerData.PlayerInfo ReceiverInfo, string Text) : this(SenderInfo.PhoneNumber, ReceiverInfo.PhoneNumber, Text)
        {

        }

        public SMS(uint SenderPhone, PlayerData.PlayerInfo ReceiverInfo, string Text) : this(SenderPhone, ReceiverInfo.PhoneNumber, Text)
        {

        }

        public SMS(PlayerData.PlayerInfo SenderInfo, uint ReceiverPhone, string Text) : this(SenderInfo.PhoneNumber, ReceiverPhone, Text)
        {

        }

        public SMS(uint SenderPhone, uint ReceiverPhone, string Text)
        {
            Data = $"{SenderPhone}_{ReceiverPhone}_{Utils.GetCurrentTime().GetUnixTimestamp()}_{Text}";
        }

        public static void TriggerRemove(Player player, int idx) => player.TriggerEvent("Phone::CSMS", idx);

        public void TriggerAdd(Player player) => player.TriggerEvent("Phone::CSMS", Data);

        public static void Add(PlayerData.PlayerInfo pInfo, SMS sms, bool triggerAdd)
        {
            if (pInfo.AllSMS.Count >= Properties.Settings.Static.PHONE_SMS_MAX_COUNT)
            {
                pInfo.AllSMS.RemoveAt(0);

                if (pInfo.PlayerData != null)
                    TriggerRemove(pInfo.PlayerData.Player, 0);
            }

            pInfo.AllSMS.Add(sms);

            if (triggerAdd && pInfo.PlayerData != null)
            {
                sms.TriggerAdd(pInfo.PlayerData.Player);
            }
        }
    }
}

