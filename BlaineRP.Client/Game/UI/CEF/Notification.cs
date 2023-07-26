using System;
using System.Collections.Generic;
using System.Linq;
using BlaineRP.Client.Extensions.RAGE.Ui;
using BlaineRP.Client.Game.EntitiesData.Enums;
using RAGE;
using Players = BlaineRP.Client.Utils.Game.Players;

namespace BlaineRP.Client.Game.UI.CEF
{
    [Script(int.MaxValue)]
    public class Notification
    {
        private static TimeSpan TextReadMinTime { get; } = TimeSpan.FromSeconds(2.5d);

        public const byte MaxNotifications = 5;

        private static DateTime LastAntiSpamShowed { get; set; }

        private static DateTime ApproveTime;
        private static string ApproveContext;

        public static bool IsActive { get => Browser.IsActive(Browser.IntTypes.Notifications); }

        public static bool IsOnTop { get; private set; } = false;

        public enum Types
        {
            /// <summary>Информация (синий)</summary>
            Information = 0,
            /// <summary>Вопрос (жёлтый)</summary>
            Question,
            /// <summary>Успех (зелёный)</summary>
            Success,
            /// <summary>Ошибка (красный)</summary>
            Error,
            /// <summary>Наличные (зелёный)</summary>
            Cash,
            /// <summary>Банк (фиолетовый)</summary>
            Bank,
            /// <summary>Предложение (жёлтый)</summary>
            Offer,
            /// <summary>Подарок (розовый)</summary>
            Gift,
            /// <summary>Предмет (синий)</summary>
            Item,
            /// <summary>Достижение (золотой)</summary>
            Achievement,
            /// <summary>NPC (синий)</summary>
            NPC,
            Quest,

            /// <summary>Мут (красный)</summary>
            Mute,
            /// <summary>Тюрьма (красный)</summary>
            Jail1,
            /// <summary>Блокировка (красный)</summary>
            Ban,
            /// <summary>Предупреждение (оранжевый)</summary>
            Warn,

            /// <summary>Наручники (оранжевый)</summary>
            Cuffs,
            /// <summary>Тюрьма (оранжевый)</summary>
            Jail2,
        }

        private class Instance
        {
            /// <summary>Тип уведомления</summary>
            public Types Type { get; set; }
            /// <summary>Заголовок</summary>
            public string Title { get; set; }
            /// <summary>Сообщение</summary>
            public string Message { get; set; }
            /// <summary>Время показа уведомления в мс.</summary>
            public int Timeout { get; set; }

            /// <summary>Новое заготовленное уведомление</summary>
            /// <param name="Type">Тип</param>
            /// <param name="Title">Заголовок</param>
            /// <param name="Message">Сообщение</param>
            public Instance(Types Type, string Message, string Title = null, int Timeout = -1)
            {
                this.Type = Type;
                this.Title = Title ?? Locale.Get("NOTIFICATION_HEADER_DEF");
                this.Message = Message;

                this.Timeout = Timeout;
            }
        }

        private static Dictionary<string, Instance> Prepared = new Dictionary<string, Instance>()
        {
            { "ASP::ARN", new Instance(Types.Error, Locale.Get("GEN_ACTION_RESTRICTED_NOW"), Locale.Get("NOTIFICATION_HEADER_ERROR")) },
            { "SA", new Instance(Types.Error, Locale.Get("GEN_ACTION_NO_SELF"), Locale.Get("NOTIFICATION_HEADER_ERROR")) },

            { "ACMD::NA", new Instance(Types.Error, "Для использования данной команды необходим более высокий уровень администрирования!", Locale.Get("NOTIFICATION_HEADER_ERROR")) },

            { "Str::NM", new Instance(Types.Error, "Введенная строка содержит недопустимые символы либо длиннее/короче, чем должна быть!", Locale.Get("NOTIFICATION_HEADER_ERROR")) },

            { "ASTUFF::PIAMT", new Instance(Types.Error, "Данный игрок уже имеет активный мут!", Locale.Get("NOTIFICATION_HEADER_ERROR")) },
            { "ASTUFF::PINMT", new Instance(Types.Error, "У данного игрока нет активного мута!", Locale.Get("NOTIFICATION_HEADER_ERROR")) },
            { "ASTUFF::PIAJL", new Instance(Types.Error, "Данный игрок уже сидит в NonRP-тюрьме!", Locale.Get("NOTIFICATION_HEADER_ERROR")) },
            { "ASTUFF::PINJL", new Instance(Types.Error, "Данный игрок не сидит в NonRP-тюрьме!", Locale.Get("NOTIFICATION_HEADER_ERROR")) },
            { "ASTUFF::PAHMW", new Instance(Types.Error, "У данного игрока уже есть максимальное кол-во предупреждений ({0})!\nЕго следует заблокировать", Locale.Get("NOTIFICATION_HEADER_ERROR")) },
            { "ASTUFF::PWNFOF", new Instance(Types.Error, "У данного игрока отсутствует предупреждение с таким ID!", Locale.Get("NOTIFICATION_HEADER_ERROR")) },
            { "ASTUFF::PIAB", new Instance(Types.Error, "Данный игрок уже заблокирован!", Locale.Get("NOTIFICATION_HEADER_ERROR")) },
            { "ASTUFF::PNB", new Instance(Types.Error, "Данный игрок не заблокирован!", Locale.Get("NOTIFICATION_HEADER_ERROR")) },

            { "Auth::MailNotFree", new Instance(Types.Error, Locale.Notifications.Auth.MailNotFree, Locale.Get("NOTIFICATION_HEADER_ERROR"), -1) },
            { "Auth::LoginNotFree", new Instance(Types.Error, Locale.Notifications.Auth.LoginNotFree, Locale.Get("NOTIFICATION_HEADER_ERROR"), -1) },

            { "Auth::SPWC", new Instance(Types.Error, Locale.Get("AUTH_STARTPLACE_FAULT"), Locale.Get("NOTIFICATION_HEADER_ERROR"), -1) },


            { "NP::Set", new Instance(Types.Success, Locale.Notifications.Vehicles.PlateInstalled, Locale.Notifications.Vehicles.Header) },
            { "NP::NFNRN", new Instance(Types.Error, "На данный момент нет свободных номерных знаков!", Locale.Get("NOTIFICATION_HEADER_ERROR")) },

            { "Vehicle::NotAllowed", new Instance(Types.Error, Locale.Notifications.Vehicles.NotAllowed, Locale.Notifications.Vehicles.Header) },
            { "Vehicle::NFO", new Instance(Types.Error, Locale.Notifications.Vehicles.NotFullOwner, Locale.Get("NOTIFICATION_HEADER_ERROR")) },
            { "Vehicle::OVP", new Instance(Types.Error, Locale.Notifications.Vehicles.VehicleOnPound, Locale.Get("NOTIFICATION_HEADER_ERROR")) },
            { "Vehicle::KE", new Instance(Types.Error, Locale.Notifications.Vehicles.VehicleKeyError, Locale.Get("NOTIFICATION_HEADER_ERROR")) },
            { "Vehicle::KENS", new Instance(Types.Error, Locale.Notifications.Vehicles.VehicleKeyNoSignalError, Locale.Get("NOTIFICATION_HEADER_ERROR")) },
            { "Vehicle::RKDE", new Instance(Types.Error, Locale.Notifications.Vehicles.VehicleIsDeadFixError, Locale.Get("NOTIFICATION_HEADER_ERROR")) },

            { "Vehicle::AHRV", new Instance(Types.Error, Locale.Notifications.Vehicles.AlreadyHaveRentedVehicle, Locale.Get("NOTIFICATION_HEADER_ERROR")) },

            { "Vehicle::FOFP", new Instance(Types.Error, Locale.Notifications.Vehicles.FullOfGasDef, Locale.Get("NOTIFICATION_HEADER_ERROR")) },
            { "Vehicle::FOFE", new Instance(Types.Error, Locale.Notifications.Vehicles.FullOfGasElectrical, Locale.Get("NOTIFICATION_HEADER_ERROR")) },

            { "Vehicle::OIG", new Instance(Types.Error, "Этот транспорт уже находится в гараже!", Locale.Get("NOTIFICATION_HEADER_ERROR")) },

            { "Vehicle::RVAH", new Instance(Types.Error, Locale.Notifications.General.JobRentVehicleAlreadyRented1, Locale.Get("NOTIFICATION_HEADER_ERROR")) },

            { "Vehicle::JCUSP", new Instance(Types.Success, "Вы заправили транспорт с помощью канистры!", Locale.Get("NOTIFICATION_HEADER_DEF")) },
            { "Vehicle::JCUSE", new Instance(Types.Success, "Вы заправили транспорт с помощью аккумулятора!", Locale.Get("NOTIFICATION_HEADER_DEF")) },

            { "Vehicle::HISLE", new Instance(Types.Error, "Капот этого транспорта сейчас закрыт!", Locale.Get("NOTIFICATION_HEADER_ERROR")) },

            { "Spam::Warning", new Instance(Types.Information, Locale.Get("ANTISPAM_TEXT_1"), Locale.Get("NOTIFICATION_HEADER_ASPAM")) },

            { "Container::Wait", new Instance(Types.Information, Locale.Notifications.Container.Wait, Locale.Get("NOTIFICATION_HEADER_DEF")) },
            { "Container::C", new Instance(Types.Error, "Этот контейнер сейчас закрыт!", Locale.Get("NOTIFICATION_HEADER_ERROR")) },
            { "Container::CF", new Instance(Types.Error, "Этот склад сейчас закрыт!", Locale.Get("NOTIFICATION_HEADER_ERROR")) },

            { "Weapon::InVehicleRestricted", new Instance(Types.Error, Locale.Notifications.Weapon.InVehicleRestricted, Locale.Notifications.Weapon.Header) },

            { "Inventory::ActionRestricted", new Instance(Types.Error, Locale.Notifications.Inventory.ActionRestricted, Locale.Notifications.Inventory.Header) },
            { "Inventory::NoSpace", new Instance(Types.Error, Locale.Notifications.Inventory.NoSpace, Locale.Notifications.Inventory.Header) },
            { "Inventory::PlaceRestricted", new Instance(Types.Error, Locale.Notifications.Inventory.PlaceRestricted, Locale.Notifications.Inventory.Header) },
            { "Inventory::ItemIsTemp", new Instance(Types.Error, Locale.Notifications.Inventory.TempItem, Locale.Notifications.Inventory.Header) },
            { "Inventory::TempItemDeleted", new Instance(Types.Information, Locale.Notifications.Inventory.TempItemDeleted, Locale.Notifications.Inventory.Header) },
            { "Inventory::NoItem", new Instance(Types.Error, Locale.Notifications.Inventory.NoSuchItem, Locale.Get("NOTIFICATION_HEADER_ERROR")) },
            { "Inventory::NoItemA", new Instance(Types.Error, Locale.Notifications.Inventory.NoSuchItemAmount, Locale.Get("NOTIFICATION_HEADER_ERROR")) },
            { "Inventory::Blocked", new Instance(Types.Error, Locale.Notifications.Inventory.InventoryBlocked, Locale.Get("NOTIFICATION_HEADER_ERROR")) },

            { "Inventory::ArmourBroken", new Instance(Types.Information, Locale.Notifications.Inventory.ArmourBroken, Locale.Notifications.Inventory.Header) },
            { "Inventory::Wounded", new Instance(Types.Error, Locale.Notifications.Inventory.Wounded, Locale.Notifications.Inventory.Header) },

            { "Inventory::WHTC", new Instance(Types.Error, Locale.Notifications.Inventory.WeaponHasThisComponent, Locale.Notifications.Inventory.Header) },
            { "Inventory::WWC", new Instance(Types.Error, Locale.Notifications.Inventory.WeaponWrongComponent, Locale.Notifications.Inventory.Header) },

            { "Inventory::FGNC", new Instance(Types.Error, Locale.Notifications.Inventory.FishingNotCatched, Locale.Get("NOTIFICATION_HEADER_ERROR")) },

            { "Inv::HMPF", new Instance(Types.Error, Locale.Notifications.Inventory.MaxAmountFurnitureHouse, Locale.Get("NOTIFICATION_HEADER_ERROR")) },
            { "Inv::PMPF", new Instance(Types.Error, Locale.Notifications.Inventory.MaxAmountFurnitureOwned, Locale.Get("NOTIFICATION_HEADER_ERROR")) },
            { "Inv::CCWUA", new Instance(Types.Error, "Вы не можете поменять эту одежду/аксессуар, пока находитесь в рабочей форме!", Locale.Get("NOTIFICATION_HEADER_ERROR")) },
            { "Inv::CDTWUA", new Instance(Types.Error, "Вы не можете сделать это, пока находитесь в рабочей форме!", Locale.Get("NOTIFICATION_HEADER_ERROR")) },

            { "Inv::NWNOTE", new Instance(Types.Error, "Текст этой записки нельзя изменить!", Locale.Get("NOTIFICATION_HEADER_ERROR")) },
            { "Inv::NRNOTE", new Instance(Types.Error, "Эту записку нельзя прочесть!", Locale.Get("NOTIFICATION_HEADER_ERROR")) },

            { "IOG::PINA", new Instance(Types.Error, Locale.Notifications.Inventory.PlacedItemOnGroundNotAllowed, Locale.Get("NOTIFICATION_HEADER_ERROR")) },

            { "Job::AHJ", new Instance(Types.Error, Locale.Notifications.General.AlreadyHaveJob, Locale.Get("NOTIFICATION_HEADER_ERROR")) },

            { "Kick", new Instance(Types.Information, Locale.Notifications.General.Kick, Locale.Get("NOTIFICATION_HEADER_DEF")) },
            { "KickA", new Instance(Types.Information, Locale.Notifications.General.KickAdmin, Locale.Get("NOTIFICATION_HEADER_DEF")) },
            { "KickB", new Instance(Types.Ban, "Вы были заблокированы на сервере!\nАдминистратор: {0}\nПричина: {1}\nДата окончания блокировки: {2}\n\nВы можете обжаловать решение Администрации сервера на нашем форуме!", Locale.Get("NOTIFICATION_HEADER_DEF")) },

            { "TeleportBy", new Instance(Types.Information, Locale.Notifications.General.TeleportBy, Locale.Get("NOTIFICATION_HEADER_DEF")) },

            { "Report::Start", new Instance(Types.Information, Locale.Notifications.General.Report.Start, Locale.Notifications.General.Report.Header) },
            { "Report::Close", new Instance(Types.Information, Locale.Notifications.General.Report.Close, Locale.Notifications.General.Report.Header) },
            { "Report::Reply", new Instance(Types.Information, Locale.Notifications.General.Report.Reply, Locale.Notifications.General.Report.Header) },

            { "Cmd::IdNotFound", new Instance(Types.Error, Locale.Notifications.Commands.IdNotFound, Locale.Notifications.Commands.Header) },
            { "Cmd::TargetNotFound", new Instance(Types.Error, Locale.Notifications.Commands.TargetNotFound, Locale.Notifications.Commands.Header) },

            { "PayDay::FailTime", new Instance(Types.Error, Locale.Notifications.General.PayDay.FailTime, Locale.Notifications.General.PayDay.Header) },
            { "PayDay::Fail", new Instance(Types.Error, Locale.Notifications.General.PayDay.Fail, Locale.Notifications.General.PayDay.Header) },
            { "PayDay::FailBank", new Instance(Types.Error, Locale.Notifications.General.PayDay.FailBank, Locale.Notifications.General.PayDay.Header) },

            { "Park::NotAllowed", new Instance(Types.Error, Locale.Notifications.Vehicles.Park.NotAllowed, Locale.Notifications.Vehicles.Header) },

            { "Cash::NotEnough", new Instance(Types.Error, Locale.Notifications.Money.Cash.NotEnough, Locale.Get("NOTIFICATION_HEADER_ERROR")) },

            { "Bank::NotEnough", new Instance(Types.Error, Locale.Notifications.Money.Bank.NotEnough, Locale.Get("NOTIFICATION_HEADER_ERROR")) },
            { "Bank::NoAccount", new Instance(Types.Error, Locale.Notifications.Money.Bank.NoAccount, Locale.Get("NOTIFICATION_HEADER_ERROR")) },
            { "Bank::NoAccountTarget", new Instance(Types.Error, Locale.Notifications.Money.Bank.NoAccountTarget, Locale.Get("NOTIFICATION_HEADER_ERROR")) },
            { "Bank::TargetNotFound", new Instance(Types.Error, Locale.Notifications.Money.Bank.TargetNotFound, Locale.Get("NOTIFICATION_HEADER_ERROR")) },
            { "Bank::DayLimitExceed", new Instance(Types.Error, Locale.Notifications.Money.Bank.DayLimitExceed, Locale.Get("NOTIFICATION_HEADER_ERROR")) },
            { "Bank::MSB", new Instance(Types.Error, Locale.Notifications.Money.Bank.SavingsDepositMaxExceed, Locale.Get("NOTIFICATION_HEADER_ERROR")) },
            { "Bank::SendApprove", new Instance(Types.Question, Locale.Notifications.Money.Bank.SendApprove, Locale.Get("NOTIFICATION_HEADER_APPROVE")) },
            { "Bank::SendApproveP", new Instance(Types.Question, Locale.Notifications.Money.Bank.SendApproveP, Locale.Get("NOTIFICATION_HEADER_APPROVE")) },

            { "Business::NEMB", new Instance(Types.Error, "На счёте Вашего бизнеса недостаточно средств!\nТекущий баланс: ${0}", Locale.Get("NOTIFICATION_HEADER_ERROR")) },
            { "Business::NEMC", new Instance(Types.Error, "В кассе Вашего бизнеса недостаточно средств!\nТекущий баланс: ${0}", Locale.Get("NOTIFICATION_HEADER_ERROR")) },
            { "Estate::NEMB", new Instance(Types.Error, "На счёте этой недвижимости недостаточно средств!\nТекущий баланс: ${0}", Locale.Get("NOTIFICATION_HEADER_ERROR")) },

            { "Phone::MBA", new Instance(Types.Error, Locale.Notifications.Money.PhoneBalanceMax, Locale.Get("NOTIFICATION_HEADER_ERROR")) },
            { "Phone::BNE", new Instance(Types.Error, Locale.Notifications.Money.PhoneBalanceNotEnough, Locale.Get("NOTIFICATION_HEADER_ERROR")) },
            { "Phone::CMA", new Instance(Types.Error, "У Вас уже сохранено максимальное кол-во контактов - {0}!", Locale.Get("NOTIFICATION_HEADER_ERROR")) },
            { "Phone::BLMA", new Instance(Types.Error, "В Вашем чёрном списке уже находится максимальное кол-во номеров - {0}!", Locale.Get("NOTIFICATION_HEADER_ERROR")) },

            { "Business:NoMats", new Instance(Types.Error, Locale.Notifications.Money.NoMaterialsShop, Locale.Get("NOTIFICATION_HEADER_ERROR")) },

            { "Business::HMA", new Instance(Types.Error, Locale.Notifications.General.MaxAmountOfBusinesses, Locale.Get("NOTIFICATION_HEADER_ERROR")) },
            { "Business::AB", new Instance(Types.Error, Locale.Notifications.General.BusinessAlreadyBought, Locale.Get("NOTIFICATION_HEADER_ERROR")) },
            { "Business::MMPO", new Instance(Types.Error, "Вы не можете заказать больше, чем {0} ед. материалов за один заказ!", Locale.Get("NOTIFICATION_HEADER_ERROR")) },
            { "Business::MMB", new Instance(Types.Error, "На складе Вашего бизнеса не может находиться больше, чем {0} ед. материалов!\nСейчас там находится {1} ед. материалов", Locale.Get("NOTIFICATION_HEADER_ERROR")) },
            { "Business::COIT", new Instance(Types.Error, "Вы не можете отменить этот заказ, так как он уже находится в процессе доставки!", Locale.Get("NOTIFICATION_HEADER_ERROR")) },

            { "Tuning::NA", new Instance(Types.Error, Locale.Notifications.General.TuningNotAllowed, Locale.Get("NOTIFICATION_HEADER_ERROR")) },

/*            { "Mute", new Instance(Types.Mute, Locale.Notifications.Punishments.Mute.Header, Locale.Notifications.Punishments.GotTimed) },
            { "KickBy", new Instance(Types.Information, Locale.Notifications.Punishments.Kick.Header, Locale.Notifications.Punishments.Kick.Got) },
            { "Jail", new Instance(Types.Jail1, Locale.Notifications.Punishments.Jail.Header, Locale.Notifications.Punishments.GotTimed) },
            { "Warn", new Instance(Types.Warn, Locale.Notifications.Punishments.Warn.Header, Locale.Notifications.Punishments.Warn.Got) },
            { "Ban", new Instance(Types.Ban, Locale.Notifications.Punishments.Ban.HeaderCasual, Locale.Notifications.Punishments.GotDated) },
            { "Ban::Hard", new Instance(Types.Ban, Locale.Notifications.Punishments.Ban.HeaderHard, Locale.Notifications.Punishments.GotDated) },*/

            { "Offer::Sent", new Instance(Types.Success, Locale.Notifications.Offers.Sent, Locale.Notifications.Offers.Header) },
            { "Offer::Cancel", new Instance(Types.Error, Locale.Notifications.Offers.Cancel, Locale.Notifications.Offers.Header) },
            { "Offer::CancelBy", new Instance(Types.Error, Locale.Notifications.Offers.CancelBy, Locale.Notifications.Offers.Header) },
            { "Offer::TargetBusy", new Instance(Types.Error, Locale.Notifications.Offers.TargetBusy, Locale.Notifications.Offers.Header) },
            { "Player::Busy", new Instance(Types.Error, Locale.Notifications.Inventory.ActionRestricted, Locale.Get("NOTIFICATION_HEADER_ERROR")) },
            { "Offer::TargetHasOffer", new Instance(Types.Error, Locale.Notifications.Offers.TargetHasOffer, Locale.Notifications.Offers.Header) },
            { "Offer::HasOffer", new Instance(Types.Error, Locale.Notifications.Offers.PlayerHasOffer, Locale.Notifications.Offers.Header) },

            { "Trade::PlayerNeedConfirm", new Instance(Types.Error, Locale.Notifications.Offers.PlayerNeedConfirm, Locale.Get("NOTIFICATION_HEADER_ERROR")) },
            { "Trade::PlayerConfirmed", new Instance(Types.Information, Locale.Notifications.Offers.PlayerConfirmed, Locale.Notifications.Offers.HeaderTrade) },
            { "Trade::PlayerConfirmedCancel", new Instance(Types.Information, Locale.Notifications.Offers.PlayerConfirmedCancel, Locale.Notifications.Offers.HeaderTrade) },
            { "Trade::Success", new Instance(Types.Success, Locale.Notifications.Offers.TradeCompleted, Locale.Get("NOTIFICATION_HEADER_DEF")) },
            { "Trade::Error", new Instance(Types.Error, Locale.Notifications.Offers.TradeError, Locale.Notifications.Offers.HeaderTrade) },
            { "Trade::NotEnoughMoney", new Instance(Types.Error, Locale.Notifications.Offers.TradeNotEnoughMoney, Locale.Get("NOTIFICATION_HEADER_ERROR")) },
            { "Trade::NotEnoughMoneyOther", new Instance(Types.Error, Locale.Notifications.Offers.TradeNotEnoughMoneyOther, Locale.Get("NOTIFICATION_HEADER_ERROR")) },
            { "Trade::NotEnoughSpaceOther", new Instance(Types.Error, Locale.Notifications.Offers.TradeNotEnoughSpaceOther, Locale.Get("NOTIFICATION_HEADER_ERROR")) },
            { "Trade::NPSO", new Instance(Types.Error, Locale.Notifications.Offers.TradeNotEnoughPropertySpaceOther, Locale.Get("NOTIFICATION_HEADER_ERROR")) },
            { "Trade::EOP", new Instance(Types.Error, "Другой игрок не может что-то передать Вам сейчас или получить от Вас, спросите его об этом", Locale.Get("NOTIFICATION_HEADER_ERROR")) },
            { "Trade::MVIT", new Instance(Types.Error, "Вы не можете выбрать больше, чем {0} транспорта для обмена", Locale.Get("NOTIFICATION_HEADER_ERROR")) },
            { "Trade::MHBIT", new Instance(Types.Error, "Вы не можете выбрать больше, чем {0} дома/квартиры для обмена", Locale.Get("NOTIFICATION_HEADER_ERROR")) },
            { "Trade::MGIT", new Instance(Types.Error, "Вы не можете выбрать больше, чем {0} гаражей для обмена", Locale.Get("NOTIFICATION_HEADER_ERROR")) },
            { "Trade::MBIT", new Instance(Types.Error, "Вы не можете выбрать больше, чем {0} бизнеса для обмена", Locale.Get("NOTIFICATION_HEADER_ERROR")) },
            { "Trade::MGPH", new Instance(Types.Error, "Чтобы продать/обменять гараж #{0}, на его счете должно быть как минимум столько средств, чтобы было достаточно для оплаты {1} часов налога", Locale.Get("NOTIFICATION_HEADER_ERROR")) },
            { "Trade::MBPH", new Instance(Types.Error, "Чтобы продать/обменять бизнес #{0}, на его счете должно быть как минимум столько средств, чтобы было достаточно для оплаты {1} часов аренды", Locale.Get("NOTIFICATION_HEADER_ERROR")) },
            { "Trade::MHPH", new Instance(Types.Error, "Чтобы продать/обменять дом #{0}, на его счете должно быть как минимум столько средств, чтобы было достаточно для оплаты {1} часов налога", Locale.Get("NOTIFICATION_HEADER_ERROR")) },
            { "Trade::MAPH", new Instance(Types.Error, "Чтобы продать/обменять квартиру #{0}, на ее счете должно быть как минимум столько средств, чтобы было достаточно для оплаты {1} часов налога", Locale.Get("NOTIFICATION_HEADER_ERROR")) },

            { "Trade::MVOW", new Instance(Types.Error, "Вы уже владеете максимальным кол-вом транспорта - {0}\nЧтобы увеличить это кол-во, купите дом или гараж с нужным кол-вом гаражных мест", Locale.Get("NOTIFICATION_HEADER_ERROR")) },
            { "Trade::MHOW", new Instance(Types.Error, "Вы уже владеете максимальным кол-вом домов - {0}", Locale.Get("NOTIFICATION_HEADER_ERROR")) },
            { "Trade::MAOW", new Instance(Types.Error, "Вы уже владеете максимальным кол-вом квартир - {0}", Locale.Get("NOTIFICATION_HEADER_ERROR")) },
            { "Trade::MGOW", new Instance(Types.Error, "Вы уже владеете максимальным кол-вом гаражей - {0}", Locale.Get("NOTIFICATION_HEADER_ERROR")) },
            { "Trade::MBOW", new Instance(Types.Error, "Вы уже владеете максимальным кол-вом бизнесов - {0}", Locale.Get("NOTIFICATION_HEADER_ERROR")) },
            { "Trade::ASH", new Instance(Types.Error, "Вы уже прописаны в каком-то доме, чтобы владеть своим домом, вы не должны быть прописаны в другом!", Locale.Get("NOTIFICATION_HEADER_ERROR")) },
            { "Trade::ASA", new Instance(Types.Error, "Вы уже прописаны в какой-то квартире, чтобы владеть своей квартирой, вы не должны быть прописаны в другой!", Locale.Get("NOTIFICATION_HEADER_ERROR")) },

            { "House::NotAllowed", new Instance(Types.Error, Locale.Notifications.House.NotAllowed, Locale.Get("NOTIFICATION_HEADER_ERROR")) },
            { "House::HL", new Instance(Types.Error, Locale.Notifications.House.IsLocked, Locale.Get("NOTIFICATION_HEADER_ERROR")) },
            { "House::CL", new Instance(Types.Error, Locale.Notifications.House.ContainersLocked, Locale.Get("NOTIFICATION_HEADER_ERROR")) },
            { "House::LCC", new Instance(Types.Success, Locale.Notifications.House.LightColourChanged, Locale.Get("NOTIFICATION_HEADER_DEF")) },
            { "House::HMA", new Instance(Types.Error, Locale.Notifications.General.MaxAmountOfHouses, Locale.Get("NOTIFICATION_HEADER_ERROR")) },
            { "House::AB", new Instance(Types.Error, "Эта недвижимость уже кем-то приобретена!", Locale.Get("NOTIFICATION_HEADER_ERROR")) },
            { "House::FPE0", new Instance(Types.Error, Locale.Get("HOUSE_FURNPLACE_0"), Locale.Get("NOTIFICATION_HEADER_ERROR")) },

            { "House::ASH", new Instance(Types.Error, Locale.Notifications.House.AlreadySettledHere, Locale.Get("NOTIFICATION_HEADER_ERROR")) },
            { "House::ASOH", new Instance(Types.Error, Locale.Notifications.House.AlreadySettledOtherHouse, Locale.Get("NOTIFICATION_HEADER_ERROR")) },
            { "House::ASOA", new Instance(Types.Error, Locale.Notifications.House.AlreadySettledOtherApartments, Locale.Get("NOTIFICATION_HEADER_ERROR")) },
            { "House::OHSE", new Instance(Types.Error, Locale.Notifications.House.OwnsHouseSettle, Locale.Get("NOTIFICATION_HEADER_ERROR")) },
            { "House::OASE", new Instance(Types.Error, Locale.Notifications.House.OwnsApartmentsSettle, Locale.Get("NOTIFICATION_HEADER_ERROR")) },

            { "Garage::NVP", new Instance(Types.Error, Locale.Notifications.House.NoVehiclePlacesInGarage, Locale.Get("NOTIFICATION_HEADER_ERROR")) },

            { "License::NTB", new Instance(Types.Error, Locale.Notifications.General.NoLicenseToBuy, Locale.Get("NOTIFICATION_HEADER_ERROR")) },

            { $"Lic::N_{(int)LicenseTypes.Business}", new Instance(Types.Error, "У Вас отсутствует лицензия на право владения бизнесом!", Locale.Get("NOTIFICATION_HEADER_ERROR")) },
            { $"Lic::N_{(int)LicenseTypes.Weapons}", new Instance(Types.Error, "У Вас отсутствует лицензия на право владения оружием!", Locale.Get("NOTIFICATION_HEADER_ERROR")) },
            { $"Lic::N_{(int)LicenseTypes.A}", new Instance(Types.Error, "У Вас отсутствует водительская лицензия категории A (мотоциклы)!", Locale.Get("NOTIFICATION_HEADER_ERROR")) },
            { $"Lic::N_{(int)LicenseTypes.B}", new Instance(Types.Error, "У Вас отсутствует водительская лицензия категории B (легковой транспорт)!", Locale.Get("NOTIFICATION_HEADER_ERROR")) },
            { $"Lic::N_{(int)LicenseTypes.C}", new Instance(Types.Error, "У Вас отсутствует водительская лицензия категории C (грузовой транспорт)!", Locale.Get("NOTIFICATION_HEADER_ERROR")) },
            { $"Lic::N_{(int)LicenseTypes.D}", new Instance(Types.Error, "У Вас отсутствует водительская лицензия категории D (маршрутный транспорт)!", Locale.Get("NOTIFICATION_HEADER_ERROR")) },
            { $"Lic::N_{(int)LicenseTypes.Sea}", new Instance(Types.Error, "У Вас отсутствует водительская лицензия категории Sea (водный транспорт)!", Locale.Get("NOTIFICATION_HEADER_ERROR")) },
            { $"Lic::N_{(int)LicenseTypes.Fly}", new Instance(Types.Error, "У Вас отсутствует водительская лицензия категории Air (воздушный транспорт)!", Locale.Get("NOTIFICATION_HEADER_ERROR")) },
            { $"Lic::N_{(int)LicenseTypes.Lawyer}", new Instance(Types.Error, "У Вас отсутствует лицензия на работу адвокатом!", Locale.Get("NOTIFICATION_HEADER_ERROR")) },
            { $"Lic::N_{(int)LicenseTypes.Hunting}", new Instance(Types.Error, "У Вас отсутствует лицензия на охоту!", Locale.Get("NOTIFICATION_HEADER_ERROR")) },

            { "Report::N", new Instance(Types.Success, "Ваше сообщение принято, ответ будет дан администрацией в ближайшее время!\n\nС каждым новым отправленным сообщением Ваш запрос будет перемещен в конец очереди.\n\nНомер Вашего запроса в очереди: {0}", Locale.Get("NOTIFICATION_HEADER_DEF")) },
            { "Report::S", new Instance(Types.Error, "Вы сможете отправить новое сообщение к этому запросу через {0}", Locale.Get("NOTIFICATION_HEADER_ERROR")) },
            { "Report::AT", new Instance(Types.Information, "Администратор {0} начал заниматься Вашим запросом!", Locale.Get("NOTIFICATION_HEADER_DEF")) },

            { "Cuffs::0_0", new Instance(Types.Cuffs, Locale.Get("POLICE_CUFFS_N_2"), Locale.Get("NOTIFICATION_HEADER_DEF")) },
            { "Cuffs::0_1", new Instance(Types.Cuffs, Locale.Get("POLICE_CUFFS_N_3"), Locale.Get("NOTIFICATION_HEADER_DEF")) },

            { "Cuffs::1_0", new Instance(Types.Information, "{0} надел на Вас стяжки!", Locale.Get("NOTIFICATION_HEADER_DEF")) },
            { "Cuffs::1_1", new Instance(Types.Information, "{0} снял с Вас стяжки", Locale.Get("NOTIFICATION_HEADER_DEF")) },

            { "Escort::0_0_0", new Instance(Types.Information, "Вы начали вести за собой {0}", Locale.Get("NOTIFICATION_HEADER_DEF")) },
            { "Escort::0_0_1", new Instance(Types.Information, "Вы перестали вести за собой {0}", Locale.Get("NOTIFICATION_HEADER_DEF")) },

            { "Escort::0_0", new Instance(Types.Information, "{0} начал вести Вас за собой!", Locale.Get("NOTIFICATION_HEADER_DEF")) },
            { "Escort::0_1", new Instance(Types.Information, "{0} перестал вести Вас за собой", Locale.Get("NOTIFICATION_HEADER_DEF")) },

            { "Fraction::NM", new Instance(Types.Error, "Вы не состоите в этой фракции!", Locale.Get("NOTIFICATION_HEADER_ERROR")) },
            { "Fraction::NMA", new Instance(Types.Error, "Вы не состоите ни в одной фракции!", Locale.Get("NOTIFICATION_HEADER_ERROR")) },
            { "Fraction::NA", new Instance(Types.Error, "У Вас недостаточно прав!", Locale.Get("NOTIFICATION_HEADER_ERROR")) },
            { "Fraction::NAL", new Instance(Types.Error, "Только лидер фракции может делать это!", Locale.Get("NOTIFICATION_HEADER_ERROR")) },
            { "Fraction::RE0", new Instance(Types.Error, "Вы не можете редактировать эту должность!", Locale.Get("NOTIFICATION_HEADER_ERROR")) },
            { "Fraction::RE1", new Instance(Types.Error, "Вы не можете самостоятельно изменить название должности!\nОставьте заявку на форуме с желаемыми названиями должностей и ожидайте ответа", Locale.Get("NOTIFICATION_HEADER_ERROR")) },
            { "Fraction::HRIBTY", new Instance(Types.Error, "Должность этого сотрудника равна либо выше Вашей, вы не можете сделать это!", Locale.Get("NOTIFICATION_HEADER_ERROR")) },
            { "Fraction::TGINYF", new Instance(Types.Error, "Данный человек не состоит в Вашей фракции!", Locale.Get("NOTIFICATION_HEADER_ERROR")) },
            { "Fraction::CSTR", new Instance(Types.Error, "Вы не можете назначить этого сотрудника на данную должность!", Locale.Get("NOTIFICATION_HEADER_ERROR")) },

            { "Fraction::RU", new Instance(Types.Information, "{0} повысил Вас до должности {1}!", Locale.Get("NOTIFICATION_HEADER_DEF")) },
            { "Fraction::RD", new Instance(Types.Information, "{0} понизил Вас до должности {1}!", Locale.Get("NOTIFICATION_HEADER_DEF")) },
            { "Fraction::F", new Instance(Types.Information, "{0} уволил Вас из фракции!", Locale.Get("NOTIFICATION_HEADER_DEF")) },
            { "Fraction::FN", new Instance(Types.Information, "Вы были уволены из фракции!", Locale.Get("NOTIFICATION_HEADER_DEF")) },

            { "Fraction::CWL", new Instance(Types.Error, "Создание предметов из материалов на данный момент недоступно!\nОбратитесь к руководству фракции", Locale.Get("NOTIFICATION_HEADER_ERROR")) },

            { "Fraction::NEWSMC", new Instance(Types.Error, "Достигнуто максимальное кол-во активных новостей, удалите любую старую новость!", Locale.Get("NOTIFICATION_HEADER_ERROR")) },
            { "Fraction::NEWSDE", new Instance(Types.Error, "Новость не существует!", Locale.Get("NOTIFICATION_HEADER_ERROR")) },

            { "Fraction::JJ", new Instance(Types.Information, "Теперь Вы состоите во фракции {0}!", Locale.Get("NOTIFICATION_HEADER_DEF")) },

            { "Fraction::NEMB", new Instance(Types.Error, "На счёте фракции недостаточно средств!\nТекущий баланс: ${0}", Locale.Get("NOTIFICATION_HEADER_ERROR")) },
            { "Fraction::NEMA", new Instance(Types.Error, "На складе фракции недостаточно материалов!\nМатериалов имеется: {0} ед.", Locale.Get("NOTIFICATION_HEADER_ERROR")) },
            { "Fraction::NIUF", new Instance(Types.Error, Locale.Get("FRACTION_NIUFE_0"), Locale.Get("NOTIFICATION_HEADER_ERROR")) },

            { "Police::DBS::PNF0", new Instance(Types.Error, "Человек с таким номером телефона не найден!", Locale.Get("NOTIFICATION_HEADER_ERROR")) },
            { "Police::DBS::PNF3", new Instance(Types.Error, "Человек с таким CID/ID не найден!", Locale.Get("NOTIFICATION_HEADER_ERROR")) },
            { "Police::DBS::PNF1", new Instance(Types.Error, "Человек с таким именем и фамилией не найден!", Locale.Get("NOTIFICATION_HEADER_ERROR")) },
            { "Police::DBS::PNF2", new Instance(Types.Error, "Человек, который владеет транспортом с таким гос. номером, не найден!", Locale.Get("NOTIFICATION_HEADER_ERROR")) },
            { "Police::DBS::PAF", new Instance(Types.Error, "Вы сейчас и так просматриваете сведения об этом человеке!", Locale.Get("NOTIFICATION_HEADER_ERROR")) },

            { "Police::APB::NYD", new Instance(Types.Error, "Вы не можете исполнить (удалить) эту ориентировку, т.к. она была создана сотрудником другого департамента!", Locale.Get("NOTIFICATION_HEADER_ERROR")) },
            { "Police::GPSTR::NYD", new Instance(Types.Error, "Вы не можете отключить этот GPS-трекер, т.к. он был активирован сотрудником другого департамента!", Locale.Get("NOTIFICATION_HEADER_ERROR")) },

            { "Police::PMASKOFF_0", new Instance(Types.Error, Locale.Get("POLICE_PMASKOFF_S_1"), Locale.Get("NOTIFICATION_HEADER_ERROR")) },

            { "DriveS::NPTT", new Instance(Types.Error, "Вы не проходили теоритический тест для этого типа транспорта, чтобы сдавать практический!", Locale.Get("NOTIFICATION_HEADER_ERROR")) },
            { "DriveS::AHPT", new Instance(Types.Error, "Вы уже сдали теоретическую часть одного теста, завершите его практическую часть, чтобы начать сдавать другой!", Locale.Get("NOTIFICATION_HEADER_ERROR")) },
            { "DriveS::AHTL", new Instance(Types.Error, "Вы уже владеете лицензией этого типа!", Locale.Get("NOTIFICATION_HEADER_ERROR")) },

            { "DriveS::TTF0", new Instance(Types.Error, "Вы провалили теоретическую часть теста!", Locale.Get("NOTIFICATION_HEADER_DEF")) },
            { "DriveS::TTF1", new Instance(Types.Error, "Вы провалили теоретическую часть теста!\nНабрано баллов: {0} из {1} минимальных", Locale.Get("NOTIFICATION_HEADER_DEF")) },

            { "DriveS::TTS", new Instance(Types.Success, "Вы сдали теоретическую часть теста!\nНабрано баллов: {0} из {1} минимальных", Locale.Get("NOTIFICATION_HEADER_DEF")) },

            { "DriveS::PEF0", new Instance(Types.Error, "Вы провалили практическую часть экзамена!\nЕсли хотите повторить попытку, придется заново оплатить и сдать теорию!", Locale.Get("NOTIFICATION_HEADER_DEF")) },
            { "DriveS::PEF1", new Instance(Types.Error, "Вы провалили практическую часть экзамена!\nПричина: транспорт уничтожен/потерян\nЕсли хотите повторить попытку, придется заново оплатить и сдать теорию!", Locale.Get("NOTIFICATION_HEADER_DEF")) },
            { "DriveS::PEF2", new Instance(Types.Error, "Вы провалили практическую часть экзамена!\nПричина: покидание транспорта\nЕсли хотите повторить попытку, придется заново оплатить и сдать теорию!", Locale.Get("NOTIFICATION_HEADER_DEF")) },
            { "DriveS::PEF3", new Instance(Types.Error, "Вы провалили практическую часть экзамена!\nПричина: езда без ремня безопасности\nЕсли хотите повторить попытку, придется заново оплатить и сдать теорию!", Locale.Get("NOTIFICATION_HEADER_DEF")) },
            { "DriveS::PEF4", new Instance(Types.Error, "Вы провалили практическую часть экзамена!\nПричина: слишком сильное повреждение двигателя\nЕсли хотите повторить попытку, придется заново оплатить и сдать теорию!", Locale.Get("NOTIFICATION_HEADER_DEF")) },
            { "DriveS::PEF5", new Instance(Types.Error, "Вы провалили практическую часть экзамена!\nПричина: закончилось топливо\nЕсли хотите повторить попытку, придется заново оплатить и сдать теорию!", Locale.Get("NOTIFICATION_HEADER_DEF")) },

            { "DriveS::PES", new Instance(Types.Success, "Вы успешно сдали экзамен и получаете права категории {0}!\nНе нарушайте ПДД, за серьезные нарушения Вы можете лишиться прав!", Locale.Get("NOTIFICATION_HEADER_DEF")) },

            { "EMS::HBEDS", new Instance(Types.Success, "Вы завершили процесс лечения!", Locale.Get("NOTIFICATION_HEADER_DEF")) },

            { "Casino::NEC", new Instance(Types.Error, "Недостаточно фишек!\nВаш баланс: {0}", Locale.Get("NOTIFICATION_HEADER_ERROR")) },
            { "Casino::RLTMP", new Instance(Types.Error, "За этим столом уже играет максимальное кол-во человек - {0}!", Locale.Get("NOTIFICATION_HEADER_ERROR")) },
            { "Casino::RLTMB", new Instance(Types.Error, "Вы сделали максимальное кол-во ставок на этом столе!", Locale.Get("NOTIFICATION_HEADER_ERROR")) },

            { "Casino::LCWAS", new Instance(Types.Error, "Кто-то крутит это колесо удачи прямо сейчас!", Locale.Get("NOTIFICATION_HEADER_ERROR")) },
            { "Casino::SLMAS", new Instance(Types.Error, "Кто-то уже сидит за этим автоматом!", Locale.Get("NOTIFICATION_HEADER_ERROR")) },
            { "Casino::BLJAP", new Instance(Types.Error, "Вы не можете сесть, т.к. за этим столом уже идёт игра, в которой Вы не участвуете!", Locale.Get("NOTIFICATION_HEADER_ERROR")) },

            { "Casino::BLMT", new Instance(Types.Error, "Сегодня Вы можете поставить еще не более {0} фишек!", Locale.Get("NOTIFICATION_HEADER_ERROR")) },

            { "Casino::CSB", new Instance(Types.Error, "Вы не можете сделать ставку в данный момент!", Locale.Get("NOTIFICATION_HEADER_ERROR")) },

            { "MarketStall::NO", new Instance(Types.Error, Locale.Get("MARKETSTALL_NOWNER"), Locale.Get("NOTIFICATION_HEADER_ERROR")) },
            { "MarketStall::LN", new Instance(Types.Error, Locale.Get("MARKETSTALL_LOCKED_NOW"), Locale.Get("NOTIFICATION_HEADER_ERROR")) },
            { "MarketStall::BSE0", new Instance(Types.Error, Locale.Get("MARKETSTALL_B_SERROR_0"), Locale.Get("NOTIFICATION_HEADER_ERROR")) },
            { "MarketStall::BSE1", new Instance(Types.Error, Locale.Get("MARKETSTALL_B_SERROR_1"), Locale.Get("NOTIFICATION_HEADER_ERROR")) },
            { "MarketStall::BSE2", new Instance(Types.Error, Locale.Get("MARKETSTALL_B_SERROR_2"), Locale.Get("NOTIFICATION_HEADER_ERROR")) },
            { "MarketStall::BSE3", new Instance(Types.Error, Locale.Get("MARKETSTALL_B_SERROR_3"), Locale.Get("NOTIFICATION_HEADER_ERROR")) },
            { "MarketStall::BSE4", new Instance(Types.Error, Locale.Get("MARKETSTALL_B_SERROR_4"), Locale.Get("NOTIFICATION_HEADER_ERROR")) },

            { "ArrestMenu::E3", new Instance(Types.Error, Locale.Get("ARRESTMENU_E_3"), Locale.Get("NOTIFICATION_HEADER_ERROR")) },
        };

        public Notification()
        {
            LastAntiSpamShowed = Game.World.Core.ServerTime;

            Events.Add("Notify::Custom", (args) =>
            {
                Show((Types)((int)args[0]), (string)args[1], (string)args[2], Utils.Convert.ToInt32(args[3]));
            });

            Events.Add("Notify::CustomE", (args) =>
            {
                ShowError((string)args[0], Utils.Convert.ToInt32(args[1]));
            });

            Events.Add("Notify::CustomS", (args) =>
            {
                ShowSuccess((string)args[0], Utils.Convert.ToInt32(args[1]));
            });

            Events.Add("Notify", (object[] args) =>
            {
                Show((string)args[0], ((Newtonsoft.Json.Linq.JArray)args[1]).ToObject<object[]>());
            });

            Events.Add("Item::Added", (object[] args) =>
            {
                CEF.Notification.Show(CEF.Notification.Types.Item, Locale.Notifications.Inventory.Header, (int)args[1] > 1 ? string.Format(Locale.Notifications.Inventory.Added, Game.Items.Core.GetName((string)args[0]), (int)args[1]) : string.Format(Locale.Notifications.Inventory.AddedOne, Game.Items.Core.GetName((string)args[0])));
            });

            Events.Add("Item::FCN", (object[] args) =>
            {
                CEF.Notification.Show(CEF.Notification.Types.Information, Locale.Notifications.Inventory.FishingHeader, (int)args[1] > 1 ? string.Format(Locale.Notifications.Inventory.FishCatched, Game.Items.Core.GetName((string)args[0]), (int)args[1]) : string.Format(Locale.Notifications.Inventory.FishCatchedOne, Game.Items.Core.GetName((string)args[0])));
            });

            Events.Add("Notify::P", (object[] args) =>
            {
                var args1 = ((Newtonsoft.Json.Linq.JArray)args[2]).ToObject<List<object>>();

                var player = RAGE.Elements.Entities.Players.GetAtRemote(Utils.Convert.ToUInt16(args[1]));

                if (player != null)
                {
                    args1.Insert(0, Players.GetPlayerName(player, true, false, true));
                }
                else
                {
                    args1.Insert(0, "null");
                }

                Show((string)args[0], args1.ToArray());
            });
        }

        public static void Show(string type, params object[] args)
        {
            var inst = Prepared.GetValueOrDefault(type);

            if (inst == null)
                return;

            if (args.Length > 0)
                Show(inst.Type, inst.Title, string.Format(inst.Message, args), inst.Timeout);
            else
                Show(inst.Type, inst.Title, inst.Message, inst.Timeout);
        }

        public static void Show(Types type, string title, string content, int timeout = -1)
        {
            if (!IsActive)
                return;

            Browser.Window.ExecuteJs("Notific.draw", timeout <= 0 ? GetTextReadingTime(title + content) : timeout, type.ToString(), Utils.Misc.ReplaceNewLineHtml(title), Utils.Misc.ReplaceNewLineHtml(content), MaxNotifications, false);
        }

        public static void ShowHint(string content, bool showAnyway = false, int timeout = -1)
        {
            if (!IsActive)
                return;

            if (!showAnyway && Settings.User.Interface.HideHints)
                return;

            Browser.Window.ExecuteJs("Notific.draw", timeout <= 0 ? GetTextReadingTime(content) : timeout, Types.Information.ToString(), Locale.Notifications.Hints.Header, Utils.Misc.ReplaceNewLineHtml(content), MaxNotifications, false);
        }

        public static void ShowOffer(string content, int timeout = 9999999)
        {
            if (!IsActive)
                return;

            Browser.Window.ExecuteJs("Notific.draw", timeout, Types.Offer.ToString(), Locale.Notifications.Offers.Header, Utils.Misc.ReplaceNewLineHtml(content), MaxNotifications, true);
        }

        public static void ShowError(string content, int timeout = -1)
        {
            Show(Types.Error, Locale.Get("NOTIFICATION_HEADER_ERROR"), content, timeout);
        }

        public static void ShowErrorDefault()
        {
            Show(Types.Error, Locale.Get("NOTIFICATION_HEADER_ERROR"), Locale.Get("GEN_ACTION_RESTRICTED_NOW"), -1);
        }

        public static void ShowSuccess(string content, int timeout = -1)
        {
            Show(Types.Success, Locale.Get("NOTIFICATION_HEADER_DEF"), content, timeout);
        }

        public static void ShowInfo(string content, int timeout = -1)
        {
            Show(Types.Information, Locale.Get("NOTIFICATION_HEADER_DEF"), content, timeout);
        }

        public static void ClearAll()
        {
            if (!IsActive)
                return;

            Browser.Window.ExecuteCachedJs("Notific.clearAll();");
        }

        public static bool SpamCheck(ref DateTime dateTime, int timeout = 500, bool updateTime = false, bool notify = false)
        {
            var spam = Game.World.Core.ServerTime.Subtract(dateTime).TotalMilliseconds < timeout;

            if (spam && notify && Game.World.Core.ServerTime.Subtract(LastAntiSpamShowed).TotalMilliseconds > 500)
            {
                Notification.Show(Notification.Types.Error, Locale.Get("NOTIFICATION_HEADER_ASPAM"), Locale.Get("ANTISPAM_TEXT_0"), 2000);

                LastAntiSpamShowed = Game.World.Core.ServerTime;
            }

            if (updateTime)
                dateTime = Game.World.Core.ServerTime;

            return spam;
        }

        public static int GetTextReadingTime(string text)
        {
            var optimalTime = text.Where(x => char.IsLetterOrDigit(x)).Count() * 100;

            return optimalTime < (int)TextReadMinTime.TotalMilliseconds ? (int)TextReadMinTime.TotalMilliseconds : optimalTime;
        }

        public enum FiveNotificImgTypes
        {
            Default = 0,
            Bank,
            DeliveryService,
            Taxi,
            C911,
            IncomingCall,
        }

        private static Dictionary<FiveNotificImgTypes, string> FiveNotificImgNames { get; set; } = new Dictionary<FiveNotificImgTypes, string>()
        {
            { FiveNotificImgTypes.Default, "CHAR_MULTIPLAYER" },
            { FiveNotificImgTypes.Bank, "CHAR_BANK_MAZE" },
            { FiveNotificImgTypes.DeliveryService, "CHAR_PROPERTY_ARMS_TRAFFICKING" },
            { FiveNotificImgTypes.Taxi, "CHAR_TAXI" },
            { FiveNotificImgTypes.C911, "CHAR_CALL911" },
            { FiveNotificImgTypes.IncomingCall, "CHAR_CHAT_CALL" },
        };

        public static void ShowSmsFive(FiveNotificImgTypes smsType, string senderName, string content)
        {
            var tName = FiveNotificImgNames.GetValueOrDefault(smsType);

            if (tName == null)
                return;

            ShowFiveNotification(tName, tName, 2, senderName, Locale.General.FiveNotificationDefSubj, content, 140, 0.5f);
        }

        public static void ShowFiveCallNotification(string phoneNumber, string subject, string content)
        {
            var tName = FiveNotificImgNames.GetValueOrDefault(FiveNotificImgTypes.IncomingCall);

            if (tName == null)
                return;

            ShowFiveNotification(tName, tName, 0, phoneNumber, subject, content, 140, 0.5f);
        }

        public static void ShowFiveNotification(string imageDict, string imageName, int iconType, string label, string subject, string content, int backgroundColour, float durationCoef)
        {
            var curNotific = RAGE.Game.Ui.GetCurrentNotification();

            if (curNotific > 0)
                RAGE.Game.Ui.RemoveNotification(curNotific);

            RAGE.Game.Ui.SetNotificationTextEntry("STRING");

            RAGE.Game.Ui.AddTextComponentSubstringPlayerName(content);

            RAGE.Game.Ui.SetNotificationBackgroundColor(backgroundColour);

            RAGE.Game.Ui.SetNotificationMessage4(imageDict, imageName, false, iconType, label, subject, durationCoef);

            RAGE.Game.Ui.DrawNotification(false, false);
        }

        public static void SetOnTop(bool state)
        {
            if (!CEF.Browser.IsRendered(Browser.IntTypes.Notifications))
                return;

            IsOnTop = state;

            CEF.Browser.Window.ExecuteJs("Notific.switchPos", state);
        }

        public static void SetCurrentApproveContext(string context, DateTime time)
        {
            ApproveContext = context;

            ApproveTime = time;
        }

        public static bool HasApproveTimedOut(string context, DateTime curTime, int timeoutMs) => context != ApproveContext || curTime.Subtract(ApproveTime).TotalMilliseconds >= timeoutMs;
    }
}
