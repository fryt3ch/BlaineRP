using System;
using System.Collections.Generic;
using System.Text;

namespace BCRPClient
{
    public static partial class Locale
    {
        public static partial class Notifications
        {
            public static string DefHeader = "Уведомление";
            public static string ErrorHeader = "Ошибка";
            public static string ApproveHeader = "Подтверждение";

            public static class Blip
            {
                public static string Header = "GPS";

                public static string ReachedGPS = "Вы достигли точки маршрута!";

                public static Dictionary<Additional.ExtraBlip.Types, string> TypesText = new Dictionary<Additional.ExtraBlip.Types, string>()
                {
                    { Additional.ExtraBlip.Types.GPS, "Местоположение отмечено у вас на карте!" }
                };
            }

            public static class AntiSpam
            {
                public static string Header = "Анти-спам";

                public static string DontFlood = "Не так быстро!";
                public static string Warning = "Прекратите спаммить!\nВ случае продолжения, вы будете кикнуты!\nВаш лимит: {0}/{1}";

                public static string CooldownText1 = "Вы должны подождать некоторое время, прежде чем вновь сможете сделать это!";
                public static string CooldownText2 = "Вы устали, подождите некоторое время и возвращайтесь!";
                public static string CooldownText3 = "Подождите еще {0}, прежде чем сделать это!";

                public static string CooldownText4 = "Сейчас вы не можете сделать это, приходите завтра!";

                public static string ActionRestrictedNow = "Сейчас вы не можете сделать это!";
            }

            public static class General
            {
                public static string Kick = "Вы были кикнуты!\nПричина: {0}";
                public static string TeleportBy = "Вы были телепортированы!\nАдминистратор: {0}";

                public static string KickOnServerRestart = "Сервер перезапускается!\nПерезайдите позднее";

                public static string LessThanMinValue = "Введенное значение должно быть больше, чем {0}!";
                public static string BiggerThanMaxValue = "Введенное значение должно быть меньше либо равно ({0})!";

                public static string BusinessNewMarginOwner0 = "Теперь наценка на продукцию Вашего бизнеса составляет {0}%";
                public static string BusinessIncassationNowOn = "Теперь наличные из кассы Вашего бизнеса будут доставляться в банк!";
                public static string BusinessIncassationNowOff = "Теперь наличные из кассы Вашего бизнеса не будут доставляться в банк!";

                public static string MinimalCharactersCount = "Минимальная длина текста должна составлять {0} символов!";
                public static string MaximalCharactersCount = "Максимальная длина текста должна составлять {0} символов!";

                public static string SmsDeleteConfirmText = "Данное действие удалит весь чат с данным абонентом!\nЕсли вы уверены, что хотите это сделать, нажмите еще раз";

                public static string StringOnlyLettersNumbersW = "Строка должна содержать только буквы, цифры или пробел!";
                public static string StringOnlyLettersNumbers = "Строка должна содержать только буквы или цифры!";

                public static string SmsSendAttachPosOn = "Вы прикрепили свою геолокацию к сообщению!";
                public static string SmsSendAttachPosOff = "Вы открепили свою геолокацию от сообщения!";

                public static string AlreadyInPhoneBlacklist = "Этот номер уже находится в Вашем черном списке!";

                public static string ScreenshotSaved = "Фото сохранено в папку RAGEMP/screenshots/server_ip под именем {0}";

                public static class PayDay
                {
                    public static string Header = "Зарплата";

                    public static string FailTime = "Вы отыграли {0} из {1} мин. на сервере за последний час, поэтому вы не получили зарплату!";
                    public static string Fail = "Сейчас вы не можете получить зарплату!";
                    public static string FailBank = "У вас нет банковского счёта!\nЗарплата не была начислена";
                }

                public static class Report
                {
                    public static string Header = "Администрация";

                    public static string Start = "{0} начал заниматься вашим вопросом!";
                    public static string Close = "{0} закрыл ваш вопрос!\nЕсли остались вы недовольны работой администратора - обратитесь на форум";

                    public static string Reply = "Администратор {0} ответил вам:\n{1}";
                }

                public static class Death
                {
                    public static string Header = "EMS";

                    public static string EMSNotified = "Службы скорой помощи получили ваш вызов!";
                }

                public static string SkillUp = "Навык {0} повышен! [+{1}]\nТекущий уровень: {2}/{3}]";
                public static string SkillDown = "Навык {0} понижен! [-{1}]\nТекущий уровень: {2}/{3}";

                public static string MaxAmountOfBusinesses = "Вы уже владеете максимальным количеством бизнесов!";
                public static string MaxAmountOfHouses = "Вы уже владеете максимальным количеством домов!";
                public static string MaxAmountOfApartments = "Вы уже владеете максимальным количеством квартир!";

                public static string BusinessAlreadyBought = "Этот бизнес уже кем-то приобретен!";

                public static string NoLicenseToBuy = "У вас нет необходимой лицензии для совершения покупки!";

                public static string TuningNotAllowed = "Этот транспорт нельзя тюнинговать!";

                public static string ShootingRangeHint1 = "Будьте внимательны, вы проиграете, если ваша меткость будет ниже {0}%!";

                public static string AchievementUnlockedText = "Достижение разблокировано!";

                public static string NoMedicalCard = "У вас нет мед. карты!";

                public static string NoOwnedBusiness = "Вы не владеете ни одним бизнесом!";
                public static string NoOwnedHouse = "Вы не владеете ни одним домом!";
                public static string NoOwnedApartments = "Вы не владеете ни одной квартирой!";
                public static string NoOwnedGarage = "Вы не владеете ни одним гаражом!";
                public static string NoOwnedEstate = "Вы не владеете ни одним видом недвижимости!";

                public static string ElevatorCurrentFloor = "Вы и так находитесь на этом этаже!";
            }

            public static class House
            {
                public static string Header = "Дом";

                public static string NotInAnyHouse = "Вы не находитесь в доме!";
                public static string NotInAnyHouseOrApartments = "Вы не находитесь в доме/квартире!";

                public static string NotAllowed = "У вас недостаточно прав!";

                public static string NotNearGarage = "Вы не находитесь рядом с гаражом!";

                public static string LightColourChanged = "Цвет света лампы был изменен!";

                public static string IsLocked = "Дверь закрыта владельцем!";
                public static string ContainersLocked = "Контейнеры закрыты владельцем!";

                public static string ExpelledHouse = "{0} выписал вас из своего дома!";
                public static string SettledHouse = "{0} прописал вас в своем доме!";
                public static string ExpelledApartments = "{0} выписал вас из своей квартиры!";
                public static string SettledApartments = "{0} прописал вас в своей квартире!";

                public static string ExpelledHouseSelf = "Вы выписались из этого дома!";
                public static string ExpelledApartmentsSelf = "Вы выписались из этой квартиры!";

                public static string ExpelledHouseAuto = "Вы были выписаны из дома!";
                public static string SettledHouseAuto = "Вы были прописаны в доме!";

                public static string ExpelledApartmentsAuto = "Вы были выписаны из квартиры!";
                public static string SettledApartmentsAuto = "Вы были прописаны в квартире!";

                public static string AlreadySettledHere = "Вы и так здесь прописаны!";
                public static string AlreadySettledOtherHouse = "Вы уже прописаны в другом доме!";
                public static string AlreadySettledOtherApartments = "Вы уже прописаны в другой квартире!";

                public static string OwnsHouseSettle = "У вас уже есть свой дом, вы не можете прописаться!";
                public static string OwnsApartmentsSettle = "У вас уже есть своя квартира, вы не можете прописаться!";

                public static string NoVehiclePlacesInGarage = "В гараже нет свободных мест для транспорта!";
            }

            public static class Commands
            {
                public static string Header = "Команда";

                public static string NotFound = "Такой команды не существует!";
                public static string WrongUsing = "Неверные параметры! Используйте:\n{0}";

                public static string IdNotFound = "Такой ID не существует!";
                public static string TargetNotFound = "Цель не существует!";

                public static string Position = "Позиция: {0}, {1}, {2} | Поворот: {3}";

                public static string Enabled = "{0} - включено!";
                public static string Disabled = "{0} - выключено!";

                public static class Teleport
                {
                    public static string Header = "Телепортация";

                    public static string NoWaypoint = "Вы не поставили метку!";
                }

                public static class Chat
                {
                    public static string Header = "Чат";

                    public static string WrongValue = "Такое значение нельзя использовать!";
                }

                public static class Item
                {
                    public static string Header = "Предмет";

                    public static string Info = "ID: {0}\nНазвание: {1}\nТип: {2}\nПодтип: {3}\nИнтерфейсы: {4}";
                }
            }

            public static class Punishments
            {
                public static class Mute
                {
                    public static string Header = "Мут";
                }

                public static class Ban
                {
                    public static string HeaderCasual = "Блокировка";
                    public static string HeaderHard = "Hard-блокировка";
                }

                public static class Jail
                {
                    public static string Header = "NonRP-тюрьма";
                }

                public static class Warn
                {
                    public static string Header = "Предупреждение";

                    public static string Got = "Администратор: {0}\nПричина: {1}\nУ вас предупреждений: {2}/{3}";
                }

                public static class Kick
                {
                    public static string Header = "Кик";

                    public static string Got = "Администратор: {0}\nПричина: {1}";
                }

                public static string GotTimed = "Администратор: {0}\nВремя: {1} сек.\nПричина: {2}";
                public static string GotDated = "Администратор: {0}\nСрок: {1} мин.\nПричина: {2}";

                public static string TimeLeft = "До снятия: {0} сек.";

                public static string PlayerOnlineInfo = "Игрок в сети!\nCID: #{0} | ID: {1}\nИмя: {2}\n\nВведите команду еще раз, чтобы подтвердить!";
                public static string PlayerOfflineInfo = "Игрок НЕ в сети!\nCID: #{0} | ID: {1}\nИмя: {2}\n\nВведите команду еще раз, чтобы подтвердить!";
            }

            public static class Money
            {
                public static string Header = "Деньги";

                public static string NotEnough = "У Вас нет денег!";

                public static class Cash
                {
                    public static string AddHeader = "+${0}";
                    public static string LossHeader = "-${0}";

                    public static string Balance = "Всего наличных: ${0}";

                    public static string NotEnough = "Недостаточно средств!\nУ вас наличных: ${0}";
                }

                public static class Bank
                {
                    public static string AddHeader = "+${0}";
                    public static string LossHeader = "-${0}";

                    public static string Balance = "Всего на счёте: ${0}";

                    public static string NotEnough = "Недостаточно средств!\nУ вас на счёте: ${0}";
                    public static string NoAccount = "У вас нет банковского счёта!";
                    public static string NoAccountTarget = "У получателя нет банковского счёта!";
                    public static string TargetNotFound = "Неверный идентификатор получателя!";

                    public static string DayLimitExceed = "Превышение максимальной суммы средств для отправки в день по тарифу!";
                    public static string SavingsDepositMaxExceed = "Превышение максимально возможного баланса сберегательного счёта по тарифу!";

                    public static string SendApprove = "Вы собираетесь отправить ${0} {1} {2}.\nНажмите еще раз для подтверждения";
                    public static string SendApproveP = "Вы собираетесь отправить ${0} {1} {2}. Комиссия за операцию составит {3}%\nНажмите еще раз для подтверждения";
                }

                public static string MaximalBalanceAlready = "У Вас на счете уже максимальное кол-во средств!";

                public static string MaximalBalanceNear = "На этот счет еще можно положить не более {0}!";

                public static string MinimalBalanceHouse = "На этом счете должно оставаться минимум {0}, Вы не можете снять этот остаток средств частично или целиком!";

                public static string AdmitToBuy = "Вы уверены? Нажмите еще раз,\nчтобы совершить покупку";

                public static string AdmitToSellGov1 = "Вы уверены? Вы получите {0}\nНажмите еще раз для подтверждения";

                public static string NoMaterialsShop = "В этом магазине недостаточно материалов!";

                public static string PhoneBalanceMax = "На балансе вашего телефона не может быть больше, чем ${0}";

                public static string PhoneBalanceInfo = "Ваш номер: {0}\nБаланс: {1}\nСтоимость 1 мин. исходящего вызова: {2}\nСтоимость 1 символа SMS: {3}";
            }

            public static class Hints
            {
                public static string Header = "Подсказка";

                public static string ClothesShopOrder = "Не вся нижняя одежда сочетается с верхней!\nЕсли хотите носить верхнюю одежду с нижней,\nто лучше сначала выберите верхнюю,\nа потом нижнюю";
                public static string ClothesShopUnderExtraNotNeedTop = "Данная нижняя одежда может изменять состояние только если на вас нет верхней одежды!";
                public static string ClothesShopUnderExtraNeedTop = "Данная нижняя одежда может изменять состояние только если на вас есть верхняя одежда!";

                public static string GasStationColshape = "В этой зоне вы можете заправить свой транспорт";

                public static string AuthCursor = "Не видно курсор? Нажми {0}";
            }

            public static class Gifts
            {
                public static string Header = "Подарки";

                public static string Added = "{0} у вас в подарках!\n\nЗабрать: {1} - Меню - Подарки";

                public static Dictionary<CEF.Menu.GiftSourceTypes, string> SourceNames = new Dictionary<CEF.Menu.GiftSourceTypes, string>()
                {
                    { CEF.Menu.GiftSourceTypes.Server, "Сервер" },
                    { CEF.Menu.GiftSourceTypes.Shop, "Магазин" },
                    { CEF.Menu.GiftSourceTypes.Achievement, "Достижение" },
                };
            }

            public static class CharacterCreation
            {
                public static string CtrlMovePed = "Удерживайте CTRL и двигайте мышкой,\nчтобы повернуть персонажа";
                public static string PressAgainToExit = "Нажмите еще раз,\n чтобы отменить создание персонажа";
                public static string PressAgainToCreate = "Вы уверены? Нажмите еще раз,\n чтобы создать персонажа";

                public static string WrongAge = "Укажите возраст от 18 до 99";
                public static string WrongName = "Имя должно содержать от 2 до X букв";
                public static string WrongSurname = "Фамилия должна содержать от 2 до X букв";
            }

            public static class Bind
            {
                public static string Header = "Привязка клавиш";
                public static string Hint = "Нажмите клавишу, чтобы назначить её на слот\nBackspace - удалить привязку";
                public static string HintCombo = "Нажмите комбинацию клавиш, чтобы назначить их на слот\nBackspace - удалить привязку";
                public static string Binded = "Действие назначено на клавишу {0}";
                public static string BindedCombo = "Действие назначено на комбинацию {0}";
            }

            public static class Interaction
            {
                public static string Header = "Взаимодействие";
                public static string DistanceTooLarge = "Цель слишком далеко!";
                public static string NotFound = "Цель не найдена!";
            }

            public static class Weapon
            {
                public static string Header = "Оружие";

                public static string InVehicleRestricted = "Данное оружие нельзя использовать в транспорте!";
            }

            public static class Auth
            {
                public static string MailNotFree = "Такая почта занята!\nПопробуйте другую";
                public static string LoginNotFree = "Такой логин занят!\nПопробуйте другой";

                public static string WrongPassword = "Неверный пароль!\nОсталось попыток: {0}";
                public static string WrongLogin = "Неверный логин!\nОсталось попыток: {0}";
                public static string WrongToken = "Неверный токен!\nОсталось попыток: {0}";

                public static string LoginTooShort = "Логин слишком короткий!";
                public static string LoginTooLong = "Логин слишком длинный!";
                public static string InvalidLogin = "Логин не соответствует правилу!";

                public static string PasswordTooShort = "Пароль слишком короткий!";
                public static string PasswordTooLong = "Пароль слишком длинный!";
                public static string InvalidPassword = "Пароль не соответствует правилу!";
                public static string PassNotMatch = "Введенные пароли не совпадают!";

                public static string InvalidMail = "Неверная почта!";

                public static string AlreadyOnline = "Этот персонаж уже в сети!";
            }

            public static class Players
            {
                public static class Microphone
                {
                    public static string Reloaded = "Голосовой чат был перезагружен!";
                }

                public static class Administrator
                {
                    public static string FreezedBy = "Вы были заморожены администратором #{0}";
                    public static string UnfreezedBy = "Вы были разморожены администратором #{0}";
                }

                public static class States
                {
                    public static string Header = "Состояние";

                    public static string Wounded = "Вы сильно ранены!\nВы будете терять здоровье, пока вам не окажут помощь";

                    public static string LowMood = "У вас плохое настроение!\nПоднимите его чем-нибудь приятным :)";
                    public static string LowSatiety = "Вы проголодались!\nЕсли вы не поедите в ближайшее время,\nто начнёте терять здоровье";
                }

                public static string PhoneNumberWrong0 = "Неправильно набран номер!";
                public static string PhoneNumberWrong1 = "Абонент вне зоны доступа!\nПопробуйте позвонить позднее";
                public static string PhoneIncomingCall0 = "Вам звонит {0}";
                public static string PhoneOpenHelp = "Откройте телефон ({0}) и примите/отклоните вызов";
            }
        }
    }
}
