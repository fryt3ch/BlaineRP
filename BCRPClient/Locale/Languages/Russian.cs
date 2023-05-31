using System;
using System.Collections.Generic;
using System.Text;

namespace BCRPClient
{
    public static partial class Locale
    {
        public static Language RussianLanguage => new Language
        (
            Texts: new (string Key, string Value)[]
            {
                ("HOUSE_STYLE_0@Name", "Вар. 1, Стиль: 1"),

                ("HOUSEMENU_LAMPS_SET", "Набор ламп #{0}"),
                ("HOUSEMENU_LAMPS_SINGLE", "Лампа #{0}"),

                ("HOUSE_STYLE_OVERVIEW_T1", "{0} - завершить просмотр"),
                ("HOUSE_STYLE_OVERVIEW_T2", "{0} - смотреть другие"),

                ("SETTING_USESERVERTIME", "Время сервера"),
                ("SETTING_HIDEHINTS", "Скрыть подсказки"),
                ("SETTING_HIDENAMES", "Скрыть имена игроков"),
                ("SETTING_HIDECIDS", "Скрыть CID игроков"),
                ("SETTING_HIDEHUD", "Скрыть HUD"),
                ("SETTING_HIDEQUEST", "Скрыть задание"),
                ("SETTING_HIDEINTERACT", "Скрыть кнопку взаимодействия"),
                ("SETTING_HIDENAMES_ITEMS", "Скрыть названия\nпредметов на земле"),
                ("SETTING_AUTORELOAD", "Автоматическая перезарядка"),
                ("SETTING_FINGERPOINT", "Включить указание\nпальцем на объекты"),

                ("MAPEDITOR_ROTATION_ANGLE", "Угол поворота: {0}"),

                ("AUTH_STARTPLACE_CANNOT", "Вы не можете выбрать это место для появления!"),

                ("POLICETABLET_CALL_NOTAKEN", "Вы не принимались ни за один вызов!"),
                ("POLICETABLET_CALL_FINISH_0", "Для того, чтобы завершить текущий вызов, вы должны находиться не более, чем в {0} метрах от него!"),
                ("POLICETABLET_APB_FINISH", "Ориентировка №{0} исполнена (удалена из базы)!"),
                ("POLICETABLET_APB_ADD", "Ориентировка успешно добавлена в базу!\nОна будет активна {0} часа, после чего удалится"),
                ("POLICETABLET_CALL_NOTEXISTS", "Такой вызов не существует!"),
                ("POLICETABLET_GPSTR_NOTEXISTS", "Такой GPS-трекер не существует!"),
                ("POLICETABLET_RFIND_INHOUSENOW", "Вы уже находитесь в этом доме!"),
                ("POLICETABLET_RFIND_INFLATNOW", "Вы уже находитесь в этой квартире!"),

                ("SHOP_TUNING_NEONCOL_0", "Цвет неона"),
                ("SHOP_TUNING_FIX_L", "Ремонт"),
                ("SHOP_TUNING_FIX_0", "Полноценный"),
                ("SHOP_TUNING_FIX_1", "Косметический (визуальный)"),
                ("SHOP_TUNING_KEYS_L", "Ключи"),
                ("SHOP_TUNING_KEYS_0", "Смена замков"),
                ("SHOP_TUNING_KEYS_1", "Дубликат ключей"),

                ("SHOP_RET_PREVIEW_HELP_0", "{0} - вернуться к ассортименту"),
                ("SHOP_RET_PREVIEW_HELP_1", "V - смена вида, Ctrl + колесико - зум"),
                ("SHOP_RET_PREVIEW_HELP_2", "ПКМ по предмету - предпросмотр"),

                ("MARKETSTALL_R_HEADER", "Аренда торговой лавки"),
                ("MARKETSTALL_R_CONTENT", "Вы действительно хотите арендовать данную торговую лавку?\nИспользуя её, вы сможете продавать свои предметы другим игрокам без риска быть обманутым.\n\nЕсли вы отойдёте от лавки слишком далеко, то лишитесь аренды!\n\n\nСтоимость: {0}"),
                ("MARKETSTALL_MG_HEADER", "Управление торговой лавкой"),
                ("MARKETSTALL_MG_UNLOCK", "Разрешить покупки"),
                ("MARKETSTALL_MG_UNLOCKED", "Вы разрешили покупки на этой торговой лавке!"),
                ("MARKETSTALL_MG_LOCK", "Запретить покупки"),
                ("MARKETSTALL_MG_LOCKED",  "Вы запретили покупки на этой торговой лавке!"),
                ("MARKETSTALL_MG_CHOOSE", "Выбрать предметы для продажи"),
                ("MARKETSTALL_MG_SELLHIST", "История продаж"),
                ("MARKETSTALL_MG_CLOSE", "Закрыть лавку"),
                ("MARKETSTALL_MG_HISTEMPTY", "История Ваших продаж на данный момент пуста!"),

                ("BUSINESSMENU_ORDER_STATE_0", "Заказ принят"),
                ("BUSINESSMENU_ORDER_STATE_1", "Заказ в пути"),

                ("ARRESTMENU_FREE_HEADER", "Вы хотите закрыть дело #{0}?"),
                ("ARRESTMENU_FREE_CONTENT", "Введите причину амнистии"),
                ("ARRESTMENU_CHTIME_HEADER", "Изменение срока наказания"),
                ("ARRESTMENU_CHTIME_CONTENT",  "Введите число минут, на которое вы хотите изменить срок по делу #{0} и причину.\n\nПример: -10, Хорошее поведение"),

                ("INTERACTION_L_GEN_0", "для взаимодействия"),

                ("INTERACTION_L_MARKETSTALL_0", "чтобы начать торговать"),
                ("INTERACTION_L_MARKETSTALL_1", "чтобы управлять лавкой"),
                ("INTERACTION_L_MARKETSTALL_2", "чтобы посмотреть товары {0} ({1})"),
            }
        );
    }
}
