using System;
using System.Collections.Generic;
using System.Text;

namespace BCRPClient
{
    public static partial class Locale
    {
        // restricted chars for regex _|&^
        public static Language RussianLanguage => new Language
        (
            Texts: new (string Key, string Value)[]
            {
                ("HOUSE_STYLE_0@Name", "Вар. 1, Стиль: 1"),

                ("HOUSEMENU_LAMPS_SET", "Набор ламп #{0}"),
                ("HOUSEMENU_LAMPS_SINGLE", "Лампа #{0}"),
                ("HOUSE_FURNPLACE_0", "Мебель находится вне интерьера помещения, невозможно установить в текущем месте!"),
                ("HOUSE_STYLE_APPROVE_0", "Вы уверены, что хотите купить этот стиль интерьера? Нажмите еще раз для подтверждения."),
                ("HOUSE_STYLE_APPROVE_1", "Вы уверены, что хотите купить этот стиль интерьера? Вся размещенная раннее мебель, блокировка дверей, настройка освещения не будут сохранены, Вам придется размещать и настраивать все заново! Нажмите еще раз для подтверждения."),

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
                ("AUTH_STARTPLACE_FAULT", "Что-то пошло не так, выберите другое место Вашего появления!"),

                ("POLICETABLET_CALL_NOTAKEN", "Вы не принимались ни за один вызов!"),
                ("POLICETABLET_CALL_FINISH_0", "Для того, чтобы завершить текущий вызов, вы должны находиться не более, чем в {0} метрах от него!"),
                ("POLICETABLET_APB_FINISH", "Ориентировка №{0} исполнена (удалена из базы)!"),
                ("POLICETABLET_APB_ADD", "Ориентировка успешно добавлена в базу!\nОна будет активна {0} часа, после чего удалится"),
                ("POLICETABLET_CALL_NOTEXISTS", "Такой вызов не существует!"),
                ("POLICETABLET_GPSTR_NOTEXISTS", "Такой GPS-трекер не существует!"),
                ("POLICETABLET_RFIND_INHOUSENOW", "Вы уже находитесь в этом доме!"),
                ("POLICETABLET_RFIND_INFLATNOW", "Вы уже находитесь в этой квартире!"),

                ("SHOP_SHARED_NOTHINGITEM_L", "Ничего"),
                ("SHOP_WEAPON_SRANGE_L", "Тир"),
                ("SHOP_TUNING_NEONCOL_0", "Цвет неона"),
                ("SHOP_TUNING_FIX_L", "Ремонт"),
                ("SHOP_TUNING_FIX_0", "Полноценный"),
                ("SHOP_TUNING_FIX_1", "Косметический (визуальный)"),
                ("SHOP_TUNING_KEYS_L", "Ключи"),
                ("SHOP_TUNING_KEYS_0", "Смена замков"),
                ("SHOP_TUNING_KEYS_1", "Дубликат ключей"),
                ("SHOP_TUNING_MODDEL_HEADER", "Удаление модификации"),
                ("SHOP_TUNING_MODDEL_CONTENT", "Вы собираетесь удалить {0} со своего транспорта.\n\nДанное действие необратимо!\nВыберите способ оплаты, чтобы продолжить."),
                ("SHOP_TUNING_KEYS_CHANGE_APPROVE", "Вы собираетесь сменить замки на своем транспорте. После этой операции все ранее созданные дубликаты ключей перестанут функционировать. Нажмите еще раз, чтобы продолжить."),
                ("SHOP_TUNING_NEON_L", "Неон"),
                ("SHOP_TUNING_COLOURS_L", "Цвета покраски"),
                ("SHOP_TUNING_PEARL_L", "Перламутр"),
                ("SHOP_TUNING_WHEELC_L", "Цвет покрышек"),
                ("SHOP_TUNING_TSMOKEC_L", "Цвет дыма от колес"),
                ("SHOP_TUNING_KEYDUBL_HEADER", "Название ключа"),
                ("SHOP_TUNING_KEYDUBL_CONTENT", "Введите название вашего ключа\n\nРазрешенные символы: цифры, буквы (рус. и англ.), пробел, -\n\nКол-во символов: от 1 до 18"),

                ("SHOP_TESTDRIVE_HELP_0", "Нажмите {0}, чтобы закончить тест-драйв"),
                ("SHOP_TESTDRIVE_HELP_1", "Нажмите {0}, чтобы открыть меню тюнинга"),

                ("SHOP_RET_PREVIEW_HELP_0", "{0} - вернуться к ассортименту"),
                ("SHOP_RET_PREVIEW_HELP_1", "V - смена вида, Ctrl + колесико - зум"),
                ("SHOP_RET_PREVIEW_HELP_2", "ПКМ по предмету - предпросмотр"),
                ("SHOP_RET_DRESS_L", "Примерить"),
                ("SHOP_RET_VIEW_L", "Посмотреть"),

                ("DOCS_SEX_MALE", "мужской"),
                ("DOCS_SEX_FEMALE", "женский"),
                ("DOCS_NOTMARRIED_MALE", "не женат"),
                ("DOCS_NOTMARRIED_FEMALE", "не замужем"),
                ("DOCS_VEHPASS_NOPLATE", "отсутствует"),

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
                ("MARKETSTALL_NOWNER", "Вы не арендуете эту торговую лавку!"),
                ("MARKETSTALL_LOCKED_NOW", "Эта торговая лавка в данный момент закрыта!"),
                ("MARKETSTALL_NOITEMS_SELL", "У Вас в инвентаре нет ни одного предмета, который можно продать!"),
                ("MARKETSTALL_NOITEMS_BUY", "В этой торговой лавке не выставлено на продажу ни одного предмета!"),
                ("MARKETSTALL_R_ODIST_0", "Если Вы отойдете еще на {0} м. от Вашей торговой лавки, то лишитесь аренды!"),
                ("MARKETSTALL_R_ODIST_1", "Вы лишились аренды торговой лавки, т.к. ушли слишком далеко от нее!"),
                ("MARKETSTALL_MG_ITEMCH_0", "Цена, которую вы указали за 1 ед. предмета для '{0}' не соответствует диапазону (от {1} до {2})"),
                ("MARKETSTALL_MG_ITEMCH_1", "Произошла ошибка, попробуйте выбрать предметы для продажи еще раз!"),
                ("MARKETSTALL_MG_ITEMCH_2", "Содержимое Вашего инвентаря было изменено, зайдите в это меню еще раз!"),
                ("MARKETSTALL_MG_ITEMCH_3", "Вы выставили выбранные предметы на продажу в этой торговой лавке!"),
                ("MARKETSTALL_MG_ITEMCH_H_0", "ЛКМ - выбрать/редактировать предмет"),
                ("MARKETSTALL_MG_ITEMCH_H_1", "ПКМ - убрать предмет"),
                ("MARKETSTALL_B_SERROR_0", "В данный момент продавец не может передать Вам этот товар, попробуйте позже!"),
                ("MARKETSTALL_B_SERROR_1", "Этот предмет в данный момент не находится у продавца в инвентаре, свяжитесь с ним или попробуйте позже!"),
                ("MARKETSTALL_B_SERROR_2", "Продавец находится слишком далеко от данной торговой лавки, поэтому он был лишен аренды!"),
                ("MARKETSTALL_B_SERROR_3", "У продавца закончился данный товар!"),
                ("MARKETSTALL_B_SERROR_4", "У продавца осталось только {0} ед. данного товара!"),
                ("MARKETSTALL_B_SERROR_5", "У продавца нет банковского счета, используйте наличные!"),
                ("MARKETSTALL_TRY_ERROR_0", "В данный момент примерка товаров не работает!"),

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

                ("NPC_NOTFAM_MALE", "Незнакомец"),
                ("NPC_NOTFAM_FEMALE", "Незнакомка"),

                ("PLAYER_DEFNAME_MALE", "Гражданин"),
                ("PLAYER_DEFNAME_FEMALE", "Гражданка"),
                ("PLAYER_ADMIN_L", "Администратор"),
                ("PLAYER_QUIT_TEXT", "Игрок вышел {0} в {1}\nCID: #{2} | ID: {3}"),

                ("GEN_PAUSEMENU_MONEY_T", "Наличные: {0} | Банк: {1}"),
                ("GEN_PAUSEMENU_HUDM_T", "Blaine RP"),

                ("GEN_ACTION_RESTRICTED_NOW", "Сейчас вы не можете сделать это!"),
                ("GEN_ACTION_NO_SELF", "Вы не можете делать это с самим собой!"),
                ("GEN_TEXT_NOTMATCH_0", "Введенное Вами значение не соответствует правилу!"),

                ("NOTIFICATION_HEADER_DEF", "Уведомление"),
                ("NOTIFICATION_HEADER_ERROR", "Ошибка"),
                ("NOTIFICATION_HEADER_WARN", "Предупреждение"),
                ("NOTIFICATION_HEADER_APPROVE", "Подтверждение"),
                ("NOTIFICATION_HEADER_ASPAM", "Анти-спам"),

                ("SCALEFORM_WASTED_HEADER", "~r~Вы при смерти"),
                ("SCALEFORM_WASTED_ATTACKER_P", "Атакующий: {0} | CID: #{1}"),
                ("SCALEFORM_WASTED_ATTACKER_S", "Несчастный случай"),
                ("SCALEFORM_SRANGE_CDOWN_HEADER", "~g~Приготовьтесь!"),
                ("SCALEFORM_SRANGE_CDOWN_CONTENT", "Начало через: {0}"),
                ("SCALEFORM_SRANGE_SCORE_T", "Счёт: {0} / {1}"),
                ("SCALEFORM_SRANGE_ACC_T", "Точность: {0}%"),
                ("SCALEFORM_SRANGE_H_EXIT", "{0} - покинуть тир"),
                ("SCALEFORM_JOB_BUSDRIVER_WAIT_HEADER", "~g~Ожидание пассажиров"),
                ("SCALEFORM_JOB_COLLECTOR_WAIT_0_HEADER", "~g~Загрузка денег"),
                ("SCALEFORM_JOB_COLLECTOR_WAIT_1_HEADER", "~g~Выгрузка денег"),
                ("SCALEFORM_JOB_TRUCKER_WAIT_0_HEADER", "~g~Загрузка материалов"),
                ("SCALEFORM_JOB_TRUCKER_WAIT_1_HEADER", "~g~Выгрузка материалов"),
                ("SCALEFORM_JOB_TRUCKER_WAIT_CONTENT", "Подождите еще {0} сек."),

                ("BLIP_MARKETSTALLS", "Рынок"),
            }
        );
    }
}
