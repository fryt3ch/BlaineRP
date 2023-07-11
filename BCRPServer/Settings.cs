using System.Collections.Generic;
using System.Globalization;

namespace BCRPServer
{
    public static class Settings
    {
        public static CultureInfo CultureInfo { get; private set; } = new CultureInfo("en-US", false)
        {
            NumberFormat = new NumberFormatInfo()
            {
                CurrencyDecimalSeparator = ".",
                NumberDecimalSeparator = ".",
            }
        };

        public const string VERSION = "Beta 1.0";

        public const string DIR_BASE_PATH = @"C:\Users\fryte\OneDrive\Documents\My Projects\BlaineRP";

        public const string DIR_RESOURCES_PATH = DIR_BASE_PATH + @"\dotnet\resources\BCRPMode";

        public const string DIR_SOURCES_PATH = DIR_BASE_PATH + @"\backend\BCRPMode";

        public const string DIR_CLIENT_PACKAGES_PATH = DIR_BASE_PATH + @"\client_packages";
        public const string DIR_CLIENT_PACKAGES_CS_PATH = DIR_CLIENT_PACKAGES_PATH + @"\cs_packages";
        public const string DIR_CLIENT_SOURCES_PATH = DIR_SOURCES_PATH + @"\BCRPClient";

        public const string DIR_CLIENT_ITEMS_DATA_PATH = DIR_CLIENT_PACKAGES_CS_PATH + @"\Data\Items\Items.cs";
        public const string DIR_CLIENT_VEHICLES_DATA_PATH = DIR_CLIENT_PACKAGES_CS_PATH + @"\Data\Vehicles.cs";
        public const string DIR_CLIENT_FURNITURE_DATA_PATH = DIR_CLIENT_PACKAGES_CS_PATH + @"\Data\Furniture.cs";
        public const string DIR_CLIENT_SHOP_DATA_PATH = DIR_CLIENT_PACKAGES_CS_PATH + @"\CEF\Shop.cs";
        public const string DIR_CLIENT_LOCATIONS_DATA_PATH = DIR_CLIENT_PACKAGES_CS_PATH + @"\Data\Locations\Locations.cs";
        public const string DIR_CLIENT_DOOR_SYSTEM_DATA_PATH = DIR_CLIENT_PACKAGES_CS_PATH + @"\Sync\DoorSystem.cs";
        public const string DIR_CLIENT_SYNC_HOUSE_DATA_PATH = DIR_CLIENT_PACKAGES_CS_PATH + @"\Sync\House.cs";
        public const string DIR_CLIENT_LANGUAGE_STRINGS_DATA_PATH = DIR_CLIENT_PACKAGES_CS_PATH + @"\Language\Strings.cs";

        /// <summary>Основное игровое измерение</summary>
        public const uint MAIN_DIMENSION = 7;
        /// <summary>Техническое измерение (в основном, пре-спавнинг сущностей)</summary>
        public const uint STUFF_DIMENSION = 1;
        /// <summary>Деморган</summary>
        public const uint DEMORGAN_DIMENSION = 2;

        public const uint PLAYER_PRIVATE_DIMENSION_BASE = 1_000;
        public const uint HOUSE_DIMENSION_BASE = 10_000;
        public const uint APARTMENTS_DIMENSION_BASE = 20_000;
        public const uint APARTMENTS_ROOT_DIMENSION_BASE = 30_000;
        public const uint GARAGE_DIMENSION_BASE = 50_000;

        /// <summary>Номер первого CID</summary>
        /// <remarks>Используется, чтобы отличать CID от Remote ID<br/>Пусть 3000 - макс. кол-во игроков на сервере, тогда 2999 - последний Remote ID</remarks>
        public const uint META_UID_FIRST_CID = 3_000;
        /// <summary>Номер первого VID</summary>
        /// <remarks>Используется, чтобы отличать CID от Remote ID<br/>Пусть 3000 - макс. кол-во игроков на сервере, а машин у каждого - 100, тогда 299999 - посдений RemoteID</remarks>
        public const uint META_UID_FIRST_VID = 100_000;

        /// <summary>Задержка до выхода из программы, когда сервер остановлен</summary>
        public const int SERVER_STOP_DELAY = 5000;

        /// <summary>Дальность стрима</summary>
        /// <remarks>Сюда устанавливать значение строго такое же, как и в config.xml сервера!</remarks>
        public const float STREAM_DISTANCE = 300f;

        /// <summary>Дистанция апдейта для частовызываемых ивентов (например, обновление перемещения указания пальцем для игрока)</summary>
        public const float FREQ_UPDATE_DISTANCE = 25f;

        /// <summary>Время, в течение которого игрок должен зайти на сервер/зарегистрироваться, в противном случае будет кикнут</summary>
        public const int AUTH_TIMEOUT_TIME = 300000;

        /// <summary>Максимальное кол-во попыток входа на сервер</summary>
        public const int AUTH_ATTEMPTS = 3 + 1;

        /// <summary>Максимальное кол-во спама одновременно</summary>
        /// <remarks>После превышения заданного кол-ва последует кик игрока с сервера</remarks>
        public const int ANTISPAM_MAX_COUNT = 50;

        public const int ANTISPAM_WARNING_COUNT = 25;

        /// <summary>Максимальный радиус для обычных сообщений</summary>
        public const float CHAT_MAX_RANGE_DEFAULT = 20f;

        /// <summary>Максимальный радиус для крика</summary>
        public const float CHAT_MAX_RANGE_LOUD = 35f;

        /// <summary>Максимальный радиус для шёпота</summary>
        public const float CHAT_MAX_RANGE_WHISPER = 5f;

        /// <summary>Максимальная дистанция прослушивания микрофона</summary>
        public const float MICROPHONE_MAX_RANGE_DEFAULT = 20f;

        /// <summary>Максимально возможная дистанция для взаимодействия игрока с сущностью</summary>
        /// <remarks>Ставить строго больше, чем для рендера, т.к. расстояние от игрока до сущности может быть больше, чем расстояние от головы игрока до одной из визуальной части сущности</remarks>
        public const float ENTITY_INTERACTION_MAX_DISTANCE = 10f;

        /// <summary>Максимально возможная дистанция для взаимодействия игрока с сущностью</summary>
        /// <remarks>Для клиента, рендер</remarks>
        public const float ENTITY_INTERACTION_MAX_DISTANCE_RENDER = 2.5f;

        /// <summary>Минимальная скорость для активации круиз-контроля</summary>
        /// <remarks>НЕ КМ/Ч! (умножить на 3.6 чтобы получить в км/ч)</remarks>
        public const float MIN_CRUISE_CONTROL_SPEED = 8.3f; // 30 km.h

        /// <summary>Максимальная скорость для активации круиз-контроля</summary>
        /// <remarks>НЕ КМ/Ч! (умножить на 3.6 чтобы получить в км/ч)</remarks>
        public const float MAX_CRUISE_CONTROL_SPEED = 33.3f; // 120 km.h

        /// <summary>Максимальная скорость толкания транспорта, итоговая зависит от навыка силы персонажа</summary>
        public const float PUSHING_VEHICLE_STRENGTH_MAX = 1f;

        /// <summary>Минимальная скорость толкания транспорта, итоговая зависит от навыка силы персонажа</summary>
        public const float PUSHING_VEHICLE_STRENGTH_MIN = 0.25f;

        /// <summary>Максимальный полный вес инвентаря</summary>
        public const float MAX_INVENTORY_WEIGHT = 15f;

        public const int MAX_VEHICLES_IN_TRADE = 2;
        public const int MAX_HOUSEBASES_IN_TRADE = 1;
        public const int MAX_GARAGES_IN_TRADE = 1;
        public const int MAX_BUSINESS_IN_TRADE = 1;

        /// <summary>Время в мс., в течение которого предмет нельзя использовать после ранения</summary>
        public const int WOUNDED_USE_TIMEOUT = 2500;

        /// <summary>Шанс получения тяжелого ранения (от 0 до 1)</summary>
        public const float WOUND_CHANCE = 0.25f;

        /// <summary>Время действия предложения между игроками</summary>
        /// <remarks>Если предложение не было принято/отклонено в течение данного времени, то оно будет автоматечески отменено сервером</remarks>
        public const int OFFER_DEFAULT_DURATION = 30_000;

        /// <summary>Максимальное кол-во игроков, которые могут одновременно просматривать контейнер</summary>
        public const byte CONTAINER_MAX_PLAYERS = 5;

        /// <summary>Максимальное кол-во игроков, которые могут одновременно просматривать верстак</summary>
        public const byte WORKBENCH_MAX_PLAYERS = 1;

        /// <summary>Максимально возможное кол-во мебели, которой игрок может владеть</summary>
        /// <remarks>Считается только та мебель, что не установлена, а находится у игрока</remarks>
        public const int FURNITURE_MAX_OWNED = 50;

        /// <summary>Максимально возможное кол-во мебели, которую игрок может разместить в доме/квартире</summary>
        public const int HOUSE_MAX_FURNITURE = 50;

        /// <summary>Время для удаления выброшенных предметов в мс.</summary>
        public const int IOG_TIME_TO_AUTODELETE = 300_000;

        /// <summary>Дистанция поиска аналогичных предметов для стака</summary>
        public const float IOG_MAX_DISTANCE_TO_STACK = 5f;

        /// <summary>Выбрасывать ли оружие после смерти?</summary>
        /// <remarks>Выбрасывается то оружие, которое находится в слотах для оружия и кобуре</remarks>
        public const bool DROP_WEAPONS_AFTER_DEATH = true;

        /// <summary>Максимальное кол-во патронов, которое выпадет после смерти</summary>
        public const int MAX_AMMO_TO_DROP_AFTER_DEATH = 250;

        /// <summary>Процент патронов, который выпадет после смерти (для каждого слота!)</summary>
        public const float PERCENT_OF_AMMO_TO_DROP_AFTER_DEATH = 0.5f;

        /// <summary>Время в секундах, необходимое для получения игроком зарплаты</summary>
        public const int MIN_SESSION_TIME_FOR_PAYDAY = 600;

        /// <summary>Время, после которого транспорт удалится с сервера (если до этого не зайдет владелец/владелец ключа)</summary>
        public const int OWNED_VEHICLE_TIME_TO_AUTODELETE = 300_000;

        public const int RENTED_VEHICLE_TIME_TO_AUTODELETE = 300_000;

        /// <summary>Стандартное кол-во наличных у нового игрока</summary>
        public const int CHARACTER_DEFAULT_MONEY_CASH = 500;

        /// <summary>Стандартная сытость игрока (от 0 до 100)</summary>
        public const int CHARACTER_DEFAULT_SATIETY = 100;

        /// <summary>Стандартное настроение игрока (от 0 до 100)</summary>
        public const int CHARACTER_DEFAULT_MOOD = 100;

        /// <summary>Изначальное кол-во доступного для владения игроком транспорта</summary>
        public const int MIN_VEHICLE_SLOTS = 1;

        /// <summary>Максимальное кол-во домов, которыми может владеть игрок</summary>
        /// <remarks>Сервер в целом допускает большее значение, чем 1, но клиент к такому не готов!</remarks>
        public const int MAX_HOUSES = 1;

        /// <summary>Максимальное кол-во квартир, которыми может владеть игрок</summary>
        /// <remarks>Сервер в целом допускает большее значение, чем 1, но клиент к такому не готов!</remarks>
        public const int MAX_APARTMENTS = 1;

        /// <summary>Максимальное кол-во гаражей, которыми может владеть игрок</summary>
        /// <remarks>Сервер в целом допускает большее значение, чем 1, но клиент к такому не готов!</remarks>
        public const int MAX_GARAGES = 1;

        /// <summary>Максимальное кол-во бизнесов, которыми может владеть игрок</summary>
        /// <remarks>Сервер в целом допускает большее значение, чем 1, но клиент к такому не готов!</remarks>
        public const int MAX_BUSINESSES = 1;

        /// <summary>Нужна ли лицензия игрокам для приобретения бизнеса?</summary>
        public const bool NEED_BUSINESS_LICENSE = false;

        /// <summary>Стоимость выдачи транспорта на штрафстоянке (в долларах)</summary>
        public const uint VEHICLEPOUND_PAY_PRICE = 525;

        /// <summary>Стоимость аренды мопеда (в долларах)</summary>
        public const uint VEHICLERENT_S_PAY_PRICE = 100;

        /// <summary>Минимальное кол-во часов, сумма оплаты за которые должна быть на счету дома/квартиры</summary>
        /// <remarks>Игрок не сможет продать/обменять дом/квартиру, если баланс его счета меньше либо равен необходимому. Также, игрок не сможет снять деньги со счета, если итоговый баланс будет меньше либо равен необходимому</remarks>
        public const uint MIN_PAID_HOURS_HOUSE_APS = 3;

        /// <summary>Максимальное кол-во часов, сумму оплаты за которые игрок может держать на счете дома/квартиры</summary>
        /// <remarks>Игрок не сможет пополнить баланс счета дома/квартиры, если итоговый баланс будет больше, чем максимально возможный</remarks>
        public const uint MAX_PAID_HOURS_HOUSE_APS = 7 * 24;

        /// <summary>Минимальное кол-во часов, сумма оплаты за которые должна быть на счету гаража</summary>
        /// <remarks>Игрок не сможет продать/обменять гараж, если баланс его счета меньше либо равен необходимому. Также, игрок не сможет снять деньги со счета, если итоговый баланс будет меньше либо равен необходимому</remarks>
        public const uint MIN_PAID_HOURS_GARAGE = 3;

        /// <summary>Максимальное кол-во часов, сумму оплаты за которые игрок может держать на счете гаража</summary>
        /// <remarks>Игрок не сможет пополнить баланс счета гаража, если итоговый баланс будет больше, чем максимально возможный</remarks>
        public const uint MAX_PAID_HOURS_GARAGE = 7 * 24;

        /// <summary>Минимальное кол-во часов, сумма оплаты за которые должна быть на счету бизнеса</summary>
        /// <remarks>Игрок не сможет продать/обменять бизнес, если баланс его счета меньше либо равен необходимому. Также, игрок не сможет снять деньги со счета (или использовать на оплату материалов), если итоговый баланс будет меньше либо равен необходимому</remarks>
        public const uint MIN_PAID_HOURS_BUSINESS = 3;

        /// <summary>Максимальное кол-во часов, сумму оплаты за которые игрок может держать на счете бизнеса</summary>
        /// <remarks>Игрок не сможет пополнить баланс счета бизнеса, если итоговый баланс будет больше, чем максимально возможный. Так как это бизнес, ограничений на пополнение быть не должно (ведь нужно покупать материалы)</remarks>
        public const uint MAX_PAID_HOURS_BUSINESS = 0;

        /// <summary>Стоимость за X мсек. исходящего вызова (в долларах)</summary>
        /// <remarks>Параметр X - ниже, например, если X = 4000, а COST_X = 1, то стоимость за 1 мин. исходящего вызова = 60000/4000 * 1 = $15</remarks>
        public const uint PHONE_CALL_COST_X = 1;

        /// <summary>Параметр X для рассчета стоимости исходящего вызова (мсек.)</summary>
        /// <remarks>Результат деления 60000 на это число должен быть целым, иначе деньги за исходящий звонок спишутся некорректно</remarks>
        public const uint PHONE_CALL_X = 4_000;

        /// <summary>Стоимость 1 символа сообщения (телефон)</summary>
        public const uint PHONE_SMS_COST_PER_CHAR = 1;

        /// <summary>Максимальное кол-во SMS, которое может быть у игрока</summary>
        /// <remarks>При превышении данного значения, самое первое сообщение в списке сообщений игрока будет удалено</remarks>
        public const byte PHONE_SMS_MAX_COUNT = 50;

        public const int PHONE_SMS_MAX_LENGTH = 120;
        public const int PHONE_SMS_MIN_LENGTH = 5;

        /// <summary>Максимальный баланс телефона</summary>
        /// <remarks>Игрок не сможет пополнить баланс телефона, если итоговый баланс будет больше, чем это значение. (MAX_BALANCE / COST_X) * CALL_X меньше либо равно int.MaxValue, ОБЯЗАТЕЛЬНО!</remarks>
        public const uint PHONE_MAX_BALANCE = 20_000;

        /// <summary>Максимальное время исходящего вызова (до момента принятия/отклонения такового любым из собеседников)</summary>
        /// <remarks>Если вызов не был принят/отклонен в течение этого кол-ва времени, то последует автоматическое отклонение вызова со стороны сервера</remarks>
        public const int PHONE_MAX_CALL_OUT_TIME = 25_000;

        public const int PHONE_BLACKLIST_MAX_AMOUNT = 50;

        public const int PHONE_CONTACT_NAME_MAX_LENGTH = 24;

        public const int PHONE_CONTACTS_MAX_AMOUNT = 50;

        public const uint VEHICLE_EVACUATION_COST = 1_000;

        public const int FRACTION_NEWS_MAX_COUNT = 25;

        public const int FRACTION_NEWS_MIN_CHAR = 10;
        public const int FRACTION_NEWS_MAX_CHAR = 250;
        public const int FRACTION_NEWS_MAX_NL = 10;

        public const int FRACTION_VEHICLE_RESPAWN_CD = 120;

        public const byte MAX_PUNISHMENTS_PER_TYPE_HISTORY = 5;

        public const byte MAX_WARNS_BEFORE_BAN = 3;

        public const ushort WARN_DAYS_TO_UNWARN = 30;

        public const ushort FRACTION_MUTE_MAX_MINUTES = 10080; // 7 * 24 * 60

        public const ushort MUTE_MAX_MINUTES = 10080; // 7 * 24 * 60

        public const int COOLDOWN_SHOOTINGRANGE_SHOP = 3 * 60 * 60;
        public const int COOLDOWN_CASINO_ROULETTE_FREE_0 = 24 * 60 * 60;
        public const int COOLDOWN_POLICE_CODE = 5 * 60;

        /// <summary>Стандартные показатели навыков у созданных персонажей</summary>
        public static Dictionary<PlayerData.SkillTypes, int> CHARACTER_DEFAULT_SKILLS => new Dictionary<PlayerData.SkillTypes, int>() { { PlayerData.SkillTypes.Strength, 0 }, { PlayerData.SkillTypes.Cooking, 0 }, { PlayerData.SkillTypes.Shooting, 0 }, { PlayerData.SkillTypes.Fishing, 0 } };

        /// <summary>Стандартный набор лицензий у созданных персонажей</summary>
        public static List<PlayerData.LicenseTypes> CHARACTER_DEFAULT_LICENSES => new List<PlayerData.LicenseTypes> { PlayerData.LicenseTypes.M };

        public static string SettingsToClientStr = (STREAM_DISTANCE, ENTITY_INTERACTION_MAX_DISTANCE, ENTITY_INTERACTION_MAX_DISTANCE_RENDER, MIN_CRUISE_CONTROL_SPEED, MAX_CRUISE_CONTROL_SPEED, MAX_INVENTORY_WEIGHT).SerializeToJson();
    }
}
