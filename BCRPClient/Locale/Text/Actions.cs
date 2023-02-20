using System;
using System.Collections.Generic;
using System.Text;

namespace BCRPClient
{
    public static partial class Locale
    {
        public static class Actions
        {
            public static string Drop = "Выбросить {0}?";
            public static string Split = "Разделить {0}?";
            public static string GetAmmo = "Достать патроны из {0}?";
            public static string LoadAmmo = "Зарядить {0}?";
            public static string Take = "Подобрать {0}?";

            public static string GiveCash = "Передать деньги {0}?";

            public static string HouseExitActionBoxHeader = "Выход";

            public static string HouseExitActionBoxOutside = "На улицу";
            public static string HouseExitActionBoxToGarage = "В гараж";
            public static string HouseExitActionBoxToHouse = "В дом";

            public static string GarageVehicleActionBoxHeader = "Загнать Т/С в гараж";

            public static string NumberplateSelectHeader = "Выбор номерного знака";

            public static string GarageVehicleSlotSelectHeader = "Выбор места в гараже";

            public static string VehiclePassportSelectHeader = "Выбор тех. паспорта";

            public static string VehiclePoundSelectHeader = "Выбор транспорта";

            public static string WeaponSkinsMenuSelectHeader = "Текущие раскраски оружия";

            public static string VehicleTuningVehicleSelect = "Выбор транспорта для тюнинга";

            public static string HouseBalanceDeposit = "Пополнение счета дома";
            public static string ApartmentsBalanceDeposit = "Пополнение счета квартиры";
            public static string GarageBalanceDeposit = "Пополнение счета гаража";
            public static string BusinessBalanceDeposit = "Пополнение счета бизнеса";

            public static string HouseBalanceWithdraw = "Снятие со счета дома";
            public static string ApartmentsBalanceWithdraw = "Снятие со счета квартиры";
            public static string GarageBalanceWithdraw = "Снятие со счета гаража";
            public static string BusinessBalanceWithdraw = "Снятие со счета бизнеса";

            public static string PlacedItemOnGroundSelectHeader = "Выберите действие с предметом";

            public static string PlacedItemOnGroundSelectTake = "Забрать";
            public static string PlacedItemOnGroundSelectLock = "Закрыть доступ к взаимодействию для всех";
            public static string PlacedItemOnGroundSelectUnlock = "Открыть доступ к взаимодействию для всех";
            public static string PlacedItemOnGroundSelectInteract = "Взаимодействовать";

            public static string JobVehicleRentTitle = "Аренда рабочего транспорта";
            public static string JobVehicleRentText = "Вы хотите взять в аренду этот рабочий транспорт?\n\n{0}\n\nСтоимость аренды составит {1}";

            public static string JobVehicleOrderSelectTitle = "Список доступных заказов";

            public static string JobTruckerOrderText = "#{0} | -> {1} км. -> {2} км. | {3}";
            public static string JobCabbieOrderText = "#{0} | {1} | {2}";

            public static string SelectOkBtn0 = "Принять";
            public static string SelectCancelBtn0 = "Отменить";

            public static string SelectOkBtn1 = "Снять";
            public static string SelectOkBtn2 = "Взять заказ";
            public static string SelectCancelBtn1 = "Закрыть";

            public static Dictionary<Data.Items.WeaponSkin.ItemData.Types, string> WeaponSkinTypeNames = new Dictionary<Data.Items.WeaponSkin.ItemData.Types, string>()
            {
                { Data.Items.WeaponSkin.ItemData.Types.UniDef, "Универсальная (все)" },
                { Data.Items.WeaponSkin.ItemData.Types.UniMk2, "Универсальная (Mk2)" },
            };
        }
    }
}
