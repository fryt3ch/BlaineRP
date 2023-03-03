using RAGE;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BCRPClient.CEF
{
    class Notification : Events.Script
    {
        public const int DefTimeout = 2500;
        public const int MaxNotifications = 5;

        private static DateTime LastAntiSpamShowed { get; set; }

        public static bool IsActive { get => Browser.IsActive(Browser.IntTypes.Notifications); }

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
                this.Title = Title ?? Locale.Notifications.DefHeader;
                this.Message = Message;

                this.Timeout = Timeout;
            }
        }

        private static Dictionary<string, Instance> Prepared = new Dictionary<string, Instance>()
        {
            { "ASP::ARN", new Instance(Types.Error, Locale.Notifications.AntiSpam.ActionRestrictedNow, Locale.Notifications.ErrorHeader) },

            { "Auth::MailNotFree", new Instance(Types.Error, Locale.Notifications.Auth.MailNotFree, Locale.Notifications.ErrorHeader, -1) },
            { "Auth::LoginNotFree", new Instance(Types.Error, Locale.Notifications.Auth.LoginNotFree, Locale.Notifications.ErrorHeader, -1) },

            { "Auth::WrongPassword", new Instance(Types.Error, Locale.Notifications.Auth.WrongPassword, Locale.Notifications.ErrorHeader, -1) },
            { "Auth::WrongLogin", new Instance(Types.Error, Locale.Notifications.Auth.WrongLogin, Locale.Notifications.ErrorHeader, -1) },
            { "Auth::WrongToken", new Instance(Types.Error, Locale.Notifications.Auth.WrongToken, Locale.Notifications.ErrorHeader, -1) },

            { "Engine::On", new Instance(Types.Success, Locale.Notifications.Vehicles.Engine.On, Locale.Notifications.Vehicles.Header) },
            { "Engine::Off", new Instance(Types.Success, Locale.Notifications.Vehicles.Engine.Off, Locale.Notifications.Vehicles.Header) },
            { "Engine::OutOfFuel", new Instance(Types.Information, Locale.Notifications.Vehicles.Engine.OutOfFuel, Locale.Notifications.Vehicles.Header) },

            { "Doors::Locked", new Instance(Types.Success, Locale.Notifications.Vehicles.Doors.Locked, Locale.Notifications.Vehicles.Header) },
            { "Doors::Unlocked", new Instance(Types.Success, Locale.Notifications.Vehicles.Doors.Unlocked, Locale.Notifications.Vehicles.Header) },

            { "Trunk::Locked", new Instance(Types.Success, Locale.Notifications.Vehicles.Trunk.Locked, Locale.Notifications.Vehicles.Header) },
            { "Trunk::Unlocked", new Instance(Types.Success, Locale.Notifications.Vehicles.Trunk.Unlocked, Locale.Notifications.Vehicles.Header) },

            { "Hood::Locked", new Instance(Types.Success, Locale.Notifications.Vehicles.Hood.Locked, Locale.Notifications.Vehicles.Header) },
            { "Hood::Unlocked", new Instance(Types.Success, Locale.Notifications.Vehicles.Hood.Unlocked, Locale.Notifications.Vehicles.Header) },

            { "NP::Set", new Instance(Types.Success, Locale.Notifications.Vehicles.PlateInstalled, Locale.Notifications.Vehicles.Header) },

            { "Vehicle::NotAllowed", new Instance(Types.Error, Locale.Notifications.Vehicles.NotAllowed, Locale.Notifications.Vehicles.Header) },
            { "Vehicle::NFO", new Instance(Types.Error, Locale.Notifications.Vehicles.NotFullOwner, Locale.Notifications.ErrorHeader) },
            { "Vehicle::OVP", new Instance(Types.Error, Locale.Notifications.Vehicles.VehicleOnPound, Locale.Notifications.ErrorHeader) },
            { "Vehicle::KE", new Instance(Types.Error, Locale.Notifications.Vehicles.VehicleKeyError, Locale.Notifications.ErrorHeader) },
            { "Vehicle::KENS", new Instance(Types.Error, Locale.Notifications.Vehicles.VehicleKeyNoSignalError, Locale.Notifications.ErrorHeader) },
            { "Vehicle::RKDE", new Instance(Types.Error, Locale.Notifications.Vehicles.VehicleIsDeadFixError, Locale.Notifications.ErrorHeader) },

            { "Vehicle::AHRV", new Instance(Types.Error, Locale.Notifications.Vehicles.AlreadyHaveRentedVehicle, Locale.Notifications.ErrorHeader) },

            { "Vehicle::FOFP", new Instance(Types.Error, Locale.Notifications.Vehicles.FullOfGasDef, Locale.Notifications.ErrorHeader) },
            { "Vehicle::FOFE", new Instance(Types.Error, Locale.Notifications.Vehicles.FullOfGasElectrical, Locale.Notifications.ErrorHeader) },

            { "Vehicle::OIG", new Instance(Types.Error, "Этот транспорт уже находится в гараже!", Locale.Notifications.ErrorHeader) },

            { "Vehicle::RVAH", new Instance(Types.Error, Locale.Notifications.General.JobRentVehicleAlreadyRented1, Locale.Notifications.ErrorHeader) },

            { "Spam::Warning", new Instance(Types.Information, Locale.Notifications.AntiSpam.Warning, Locale.Notifications.AntiSpam.Header) },

            { "Container::Wait", new Instance(Types.Information, Locale.Notifications.Container.Wait, Locale.Notifications.DefHeader) },

            { "Weapon::InVehicleRestricted", new Instance(Types.Error, Locale.Notifications.Weapon.InVehicleRestricted, Locale.Notifications.Weapon.Header) },

            { "Inventory::ActionRestricted", new Instance(Types.Error, Locale.Notifications.Inventory.ActionRestricted, Locale.Notifications.Inventory.Header) },
            { "Inventory::NoSpace", new Instance(Types.Error, Locale.Notifications.Inventory.NoSpace, Locale.Notifications.Inventory.Header) },
            { "Inventory::PlaceRestricted", new Instance(Types.Error, Locale.Notifications.Inventory.PlaceRestricted, Locale.Notifications.Inventory.Header) },
            { "Inventory::ItemIsTemp", new Instance(Types.Error, Locale.Notifications.Inventory.TempItem, Locale.Notifications.Inventory.Header) },
            { "Inventory::TempItemDeleted", new Instance(Types.Information, Locale.Notifications.Inventory.TempItemDeleted, Locale.Notifications.Inventory.Header) },
            { "Inventory::NoItem", new Instance(Types.Error, Locale.Notifications.Inventory.NoSuchItem, Locale.Notifications.ErrorHeader) },
            { "Inventory::NoItemA", new Instance(Types.Error, Locale.Notifications.Inventory.NoSuchItemAmount, Locale.Notifications.ErrorHeader) },
            { "Inventory::Blocked", new Instance(Types.Error, Locale.Notifications.Inventory.InventoryBlocked, Locale.Notifications.ErrorHeader) },

            { "Inventory::ArmourBroken", new Instance(Types.Information, Locale.Notifications.Inventory.ArmourBroken, Locale.Notifications.Inventory.Header) },
            { "Inventory::Wounded", new Instance(Types.Error, Locale.Notifications.Inventory.Wounded, Locale.Notifications.Inventory.Header) },

            { "Inventory::WHTC", new Instance(Types.Error, Locale.Notifications.Inventory.WeaponHasThisComponent, Locale.Notifications.Inventory.Header) },
            { "Inventory::WWC", new Instance(Types.Error, Locale.Notifications.Inventory.WeaponWrongComponent, Locale.Notifications.Inventory.Header) },

            { "Inventory::FGNC", new Instance(Types.Error, Locale.Notifications.Inventory.FishingNotCatched, Locale.Notifications.ErrorHeader) },

            { "Inv::HMPF", new Instance(Types.Error, Locale.Notifications.Inventory.MaxAmountFurnitureHouse, Locale.Notifications.ErrorHeader) },
            { "Inv::PMPF", new Instance(Types.Error, Locale.Notifications.Inventory.MaxAmountFurnitureOwned, Locale.Notifications.ErrorHeader) },
            { "Inv::CCWUA", new Instance(Types.Error, "Вы не можете поменять эту одежду/аксессуар, пока находитесь в рабочей форме!", Locale.Notifications.ErrorHeader) },
            { "Inv::CDTWUA", new Instance(Types.Error, "Вы не можете сделать это, пока находитесь в рабочей форме!", Locale.Notifications.ErrorHeader) },

            { "IOG::PINA", new Instance(Types.Error, Locale.Notifications.Inventory.PlacedItemOnGroundNotAllowed, Locale.Notifications.ErrorHeader) },

            { "Job::AHJ", new Instance(Types.Error, Locale.Notifications.General.AlreadyHaveJob, Locale.Notifications.ErrorHeader) },

            { "Kick", new Instance(Types.Information, Locale.Notifications.General.Kick, Locale.Notifications.DefHeader) },
            { "TeleportBy", new Instance(Types.Information, Locale.Notifications.General.TeleportBy, Locale.Notifications.DefHeader) },

            { "Report::Start", new Instance(Types.Information, Locale.Notifications.General.Report.Start, Locale.Notifications.General.Report.Header) },
            { "Report::Close", new Instance(Types.Information, Locale.Notifications.General.Report.Close, Locale.Notifications.General.Report.Header) },
            { "Report::Reply", new Instance(Types.Information, Locale.Notifications.General.Report.Reply, Locale.Notifications.General.Report.Header) },

            { "Cmd::IdNotFound", new Instance(Types.Error, Locale.Notifications.Commands.IdNotFound, Locale.Notifications.Commands.Header) },
            { "Cmd::TargetNotFound", new Instance(Types.Error, Locale.Notifications.Commands.TargetNotFound, Locale.Notifications.Commands.Header) },

            { "PayDay::FailTime", new Instance(Types.Error, Locale.Notifications.General.PayDay.FailTime, Locale.Notifications.General.PayDay.Header) },
            { "PayDay::Fail", new Instance(Types.Error, Locale.Notifications.General.PayDay.Fail, Locale.Notifications.General.PayDay.Header) },
            { "PayDay::FailBank", new Instance(Types.Error, Locale.Notifications.General.PayDay.FailBank, Locale.Notifications.General.PayDay.Header) },

            { "Park::NotAllowed", new Instance(Types.Error, Locale.Notifications.Vehicles.Park.NotAllowed, Locale.Notifications.Vehicles.Header) },

            { "Cash::NotEnough", new Instance(Types.Error, Locale.Notifications.Money.Cash.NotEnough, Locale.Notifications.ErrorHeader) },

            { "Bank::NotEnough", new Instance(Types.Error, Locale.Notifications.Money.Bank.NotEnough, Locale.Notifications.ErrorHeader) },
            { "Bank::NoAccount", new Instance(Types.Error, Locale.Notifications.Money.Bank.NoAccount, Locale.Notifications.ErrorHeader) },
            { "Bank::NoAccountTarget", new Instance(Types.Error, Locale.Notifications.Money.Bank.NoAccountTarget, Locale.Notifications.ErrorHeader) },
            { "Bank::TargetNotFound", new Instance(Types.Error, Locale.Notifications.Money.Bank.TargetNotFound, Locale.Notifications.ErrorHeader) },
            { "Bank::DayLimitExceed", new Instance(Types.Error, Locale.Notifications.Money.Bank.DayLimitExceed, Locale.Notifications.ErrorHeader) },
            { "Bank::MaxSavings", new Instance(Types.Error, Locale.Notifications.Money.Bank.SavingsDepositMaxExceed, Locale.Notifications.ErrorHeader) },
            { "Bank::SendApprove", new Instance(Types.Question, Locale.Notifications.Money.Bank.SendApprove, Locale.Notifications.ApproveHeader) },
            { "Bank::SendApproveP", new Instance(Types.Question, Locale.Notifications.Money.Bank.SendApproveP, Locale.Notifications.ApproveHeader) },

            { "Phone::MBA", new Instance(Types.Error, Locale.Notifications.Money.PhoneBalanceMax, Locale.Notifications.ErrorHeader) },
            { "Phone::BNE", new Instance(Types.Error, Locale.Notifications.Money.PhoneBalanceNotEnough, Locale.Notifications.ErrorHeader) },
            { "Phone::CMA", new Instance(Types.Error, "У Вас уже сохранено максимальное кол-во контактов - {0}!", Locale.Notifications.ErrorHeader) },
            { "Phone::BLMA", new Instance(Types.Error, "В Вашем чёрном списке уже находится максимальное кол-во номеров - {0}!", Locale.Notifications.ErrorHeader) },

            { "Business:NoMats", new Instance(Types.Error, Locale.Notifications.Money.NoMaterialsShop, Locale.Notifications.ErrorHeader) },

            { "Business::HMA", new Instance(Types.Error, Locale.Notifications.General.MaxAmountOfBusinesses, Locale.Notifications.ErrorHeader) },
            { "Business::AB", new Instance(Types.Error, Locale.Notifications.General.BusinessAlreadyBought, Locale.Notifications.ErrorHeader) },
            { "Business::MMPO", new Instance(Types.Error, "Вы не можете заказать больше, чем {0} ед. материалов за один заказ!", Locale.Notifications.ErrorHeader) },
            { "Business::MMB", new Instance(Types.Error, "На складе Вашего бизнеса не может находиться больше, чем {0} ед. материалов!\nСейчас там находится {1} ед. материалов", Locale.Notifications.ErrorHeader) },
            { "Business::COIT", new Instance(Types.Error, "Вы не можете отменить этот заказ, так как он уже находится в процессе доставки!", Locale.Notifications.ErrorHeader) },

            { "Tuning::NA", new Instance(Types.Error, Locale.Notifications.General.TuningNotAllowed, Locale.Notifications.ErrorHeader) },

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
            { "Player::Busy", new Instance(Types.Error, Locale.Notifications.Inventory.ActionRestricted, Locale.Notifications.ErrorHeader) },
            { "Offer::TargetHasOffer", new Instance(Types.Error, Locale.Notifications.Offers.TargetHasOffer, Locale.Notifications.Offers.Header) },
            { "Offer::HasOffer", new Instance(Types.Error, Locale.Notifications.Offers.PlayerHasOffer, Locale.Notifications.Offers.Header) },

            { "Trade::PlayerNeedConfirm", new Instance(Types.Error, Locale.Notifications.Offers.PlayerNeedConfirm, Locale.Notifications.ErrorHeader) },
            { "Trade::PlayerConfirmed", new Instance(Types.Information, Locale.Notifications.Offers.PlayerConfirmed, Locale.Notifications.Offers.HeaderTrade) },
            { "Trade::PlayerConfirmedCancel", new Instance(Types.Information, Locale.Notifications.Offers.PlayerConfirmedCancel, Locale.Notifications.Offers.HeaderTrade) },
            { "Trade::Success", new Instance(Types.Success, Locale.Notifications.Offers.TradeCompleted, Locale.Notifications.DefHeader) },
            { "Trade::Error", new Instance(Types.Error, Locale.Notifications.Offers.TradeError, Locale.Notifications.Offers.HeaderTrade) },
            { "Trade::NotEnoughMoney", new Instance(Types.Error, Locale.Notifications.Offers.TradeNotEnoughMoney, Locale.Notifications.ErrorHeader) },
            { "Trade::NotEnoughMoneyOther", new Instance(Types.Error, Locale.Notifications.Offers.TradeNotEnoughMoneyOther, Locale.Notifications.ErrorHeader) },
            { "Trade::NotEnoughSpaceOther", new Instance(Types.Error, Locale.Notifications.Offers.TradeNotEnoughSpaceOther, Locale.Notifications.ErrorHeader) },
            { "Trade::NPSO", new Instance(Types.Error, Locale.Notifications.Offers.TradeNotEnoughPropertySpaceOther, Locale.Notifications.ErrorHeader) },
            { "Trade::EOP", new Instance(Types.Error, "Другой игрок не может что-то передать Вам сейчас или получить от Вас, спросите его об этом", Locale.Notifications.ErrorHeader) },
            { "Trade::MVIT", new Instance(Types.Error, "Вы не можете выбрать больше, чем {0} транспорта для обмена", Locale.Notifications.ErrorHeader) },
            { "Trade::MHBIT", new Instance(Types.Error, "Вы не можете выбрать больше, чем {0} дома/квартиры для обмена", Locale.Notifications.ErrorHeader) },
            { "Trade::MGIT", new Instance(Types.Error, "Вы не можете выбрать больше, чем {0} гаражей для обмена", Locale.Notifications.ErrorHeader) },
            { "Trade::MBIT", new Instance(Types.Error, "Вы не можете выбрать больше, чем {0} бизнеса для обмена", Locale.Notifications.ErrorHeader) },
            { "Trade::MGPH", new Instance(Types.Error, "Чтобы продать/обменять гараж #{0}, на его счете должно быть как минимум столько средств, чтобы было достаточно для оплаты {1} часов налога", Locale.Notifications.ErrorHeader) },
            { "Trade::MBPH", new Instance(Types.Error, "Чтобы продать/обменять бизнес #{0}, на его счете должно быть как минимум столько средств, чтобы было достаточно для оплаты {1} часов аренды", Locale.Notifications.ErrorHeader) },
            { "Trade::MHPH", new Instance(Types.Error, "Чтобы продать/обменять дом #{0}, на его счете должно быть как минимум столько средств, чтобы было достаточно для оплаты {1} часов налога", Locale.Notifications.ErrorHeader) },
            { "Trade::MAPH", new Instance(Types.Error, "Чтобы продать/обменять квартиру #{0}, на ее счете должно быть как минимум столько средств, чтобы было достаточно для оплаты {1} часов налога", Locale.Notifications.ErrorHeader) },

            { "Trade::MVOW", new Instance(Types.Error, "Вы уже владеете максимальным кол-вом транспорта - {0}\nЧтобы увеличить это кол-во, купите дом или гараж с нужным кол-вом гаражных мест", Locale.Notifications.ErrorHeader) },
            { "Trade::MHOW", new Instance(Types.Error, "Вы уже владеете максимальным кол-вом домов - {0}", Locale.Notifications.ErrorHeader) },
            { "Trade::MAOW", new Instance(Types.Error, "Вы уже владеете максимальным кол-вом квартир - {0}", Locale.Notifications.ErrorHeader) },
            { "Trade::MGOW", new Instance(Types.Error, "Вы уже владеете максимальным кол-вом гаражей - {0}", Locale.Notifications.ErrorHeader) },
            { "Trade::MBOW", new Instance(Types.Error, "Вы уже владеете максимальным кол-вом бизнесов - {0}", Locale.Notifications.ErrorHeader) },
            { "Trade::ASH", new Instance(Types.Error, "Вы уже прописаны в каком-то доме, чтобы владеть своим домом, вы не должны быть прописаны в другом!", Locale.Notifications.ErrorHeader) },
            { "Trade::ASA", new Instance(Types.Error, "Вы уже прописаны в какой-то квартире, чтобы владеть своей квартирой, вы не должны быть прописаны в другой!", Locale.Notifications.ErrorHeader) },

            { "House::NotAllowed", new Instance(Types.Error, Locale.Notifications.House.NotAllowed, Locale.Notifications.ErrorHeader) },
            { "House::HL", new Instance(Types.Error, Locale.Notifications.House.IsLocked, Locale.Notifications.ErrorHeader) },
            { "House::CL", new Instance(Types.Error, Locale.Notifications.House.ContainersLocked, Locale.Notifications.ErrorHeader) },
            { "House::LCC", new Instance(Types.Success, Locale.Notifications.House.LightColourChanged, Locale.Notifications.DefHeader) },
            { "House::HMA", new Instance(Types.Error, Locale.Notifications.General.MaxAmountOfHouses, Locale.Notifications.ErrorHeader) },
            { "House::AB", new Instance(Types.Error, "Эта недвижимость уже кем-то приобретена!", Locale.Notifications.ErrorHeader) },

            { "House::ASH", new Instance(Types.Error, Locale.Notifications.House.AlreadySettledHere, Locale.Notifications.ErrorHeader) },
            { "House::ASOH", new Instance(Types.Error, Locale.Notifications.House.AlreadySettledOtherHouse, Locale.Notifications.ErrorHeader) },
            { "House::ASOA", new Instance(Types.Error, Locale.Notifications.House.AlreadySettledOtherApartments, Locale.Notifications.ErrorHeader) },
            { "House::OHSE", new Instance(Types.Error, Locale.Notifications.House.OwnsHouseSettle, Locale.Notifications.ErrorHeader) },
            { "House::OASE", new Instance(Types.Error, Locale.Notifications.House.OwnsApartmentsSettle, Locale.Notifications.ErrorHeader) },

            { "Garage::NVP", new Instance(Types.Error, Locale.Notifications.House.NoVehiclePlacesInGarage, Locale.Notifications.ErrorHeader) },

            { "License::NTB", new Instance(Types.Error, Locale.Notifications.General.NoLicenseToBuy, Locale.Notifications.ErrorHeader) },

            { $"Lic::N_{(int)Sync.Players.LicenseTypes.Business}", new Instance(Types.Error, "У Вас отсутствует лицензия на право владения бизнесом!", Locale.Notifications.ErrorHeader) },
            { $"Lic::N_{(int)Sync.Players.LicenseTypes.Weapons}", new Instance(Types.Error, "У Вас отсутствует лицензия на право владения оружием!", Locale.Notifications.ErrorHeader) },
            { $"Lic::N_{(int)Sync.Players.LicenseTypes.A}", new Instance(Types.Error, "У Вас отсутствует водительская лицензия категории A (мотоциклы)!", Locale.Notifications.ErrorHeader) },
            { $"Lic::N_{(int)Sync.Players.LicenseTypes.B}", new Instance(Types.Error, "У Вас отсутствует водительская лицензия категории B (легковой транспорт)!", Locale.Notifications.ErrorHeader) },
            { $"Lic::N_{(int)Sync.Players.LicenseTypes.C}", new Instance(Types.Error, "У Вас отсутствует водительская лицензия категории C (грузовой транспорт)!", Locale.Notifications.ErrorHeader) },
            { $"Lic::N_{(int)Sync.Players.LicenseTypes.D}", new Instance(Types.Error, "У Вас отсутствует водительская лицензия категории D (маршрутный транспорт)!", Locale.Notifications.ErrorHeader) },
            { $"Lic::N_{(int)Sync.Players.LicenseTypes.Sea}", new Instance(Types.Error, "У Вас отсутствует водительская лицензия категории Sea (водный транспорт)!", Locale.Notifications.ErrorHeader) },
            { $"Lic::N_{(int)Sync.Players.LicenseTypes.Fly}", new Instance(Types.Error, "У Вас отсутствует водительская лицензия категории Air (воздушный транспорт)!", Locale.Notifications.ErrorHeader) },
            { $"Lic::N_{(int)Sync.Players.LicenseTypes.Lawyer}", new Instance(Types.Error, "У Вас отсутствует лицензия на работу адвокатом!", Locale.Notifications.ErrorHeader) },
            { $"Lic::N_{(int)Sync.Players.LicenseTypes.Hunting}", new Instance(Types.Error, "У Вас отсутствует лицензия на охоту!", Locale.Notifications.ErrorHeader) },

            { "CDown::1", new Instance(Types.Error, Locale.Notifications.AntiSpam.CooldownText1, Locale.Notifications.ErrorHeader) },
            { "CDown::2", new Instance(Types.Error, Locale.Notifications.AntiSpam.CooldownText2, Locale.Notifications.ErrorHeader) },
            { "CDown::3", new Instance(Types.Error, Locale.Notifications.AntiSpam.CooldownText3, Locale.Notifications.ErrorHeader) },
        };

        public Notification()
        {
            LastAntiSpamShowed = Sync.World.ServerTime;

            // 0 - type, 1 - title, 2 - text, 3 - timeout
            Events.Add("Notify::Custom", (object[] args) => Show((Types)((int)args[0]), (string)args[1], (string)args[2], args.Length > 3 ? (int)args[3] : -1));

            Events.Add("Notify", (object[] args) =>
            {
                Show((string)args[0], ((Newtonsoft.Json.Linq.JArray)args[1]).ToObject<object[]>());
            });

            Events.Add("Item::Added", (object[] args) =>
            {
                CEF.Notification.Show(CEF.Notification.Types.Item, Locale.Notifications.Inventory.Header, (int)args[1] > 1 ? string.Format(Locale.Notifications.Inventory.Added, Data.Items.GetName((string)args[0]), (int)args[1]) : string.Format(Locale.Notifications.Inventory.AddedOne, Data.Items.GetName((string)args[0])));
            });

            Events.Add("Item::FCN", (object[] args) =>
            {
                CEF.Notification.Show(CEF.Notification.Types.Information, Locale.Notifications.Inventory.FishingHeader, (int)args[1] > 1 ? string.Format(Locale.Notifications.Inventory.FishCatched, Data.Items.GetName((string)args[0]), (int)args[1]) : string.Format(Locale.Notifications.Inventory.FishCatchedOne, Data.Items.GetName((string)args[0])));
            });
        }

        #region Showers
        public static void Show(string type, params object[] args)
        {
            if (!Prepared.ContainsKey(type))
                return;

            var inst = Prepared[type];

            if (args.Length > 0)
                Show(inst.Type, inst.Title, string.Format(inst.Message, args), inst.Timeout);
            else
                Show(inst.Type, inst.Title, inst.Message, inst.Timeout);
        }

        public static void Show(Types type, string title, string content, int timeout = -1)
        {
            if (!IsActive)
                return;

            Browser.Window.ExecuteJs("Notific.draw", timeout <= 0 ? GetTextReadingTime(title + content) : timeout, type.ToString(), Utils.ReplaceNewLineHtml(title), Utils.ReplaceNewLineHtml(content), MaxNotifications, false);
        }

        public static void ShowHint(string content, bool showAnyway = false, int timeout = -1)
        {
            if (!IsActive)
                return;

            if (!showAnyway && Settings.Interface.HideHints)
                return;

            Browser.Window.ExecuteJs("Notific.draw", timeout <= 0 ? GetTextReadingTime(content) : timeout, Types.Information.ToString(), Locale.Notifications.Hints.Header, Utils.ReplaceNewLineHtml(content), MaxNotifications, false);
        }

        public static void ShowOffer(string content, int timeout = 9999999)
        {
            if (!IsActive)
                return;

            Browser.Window.ExecuteJs("Notific.draw", timeout, Types.Offer.ToString(), Locale.Notifications.Offers.Header, Utils.ReplaceNewLineHtml(content), MaxNotifications, true);
        }
        #endregion

        #region Stuff
        public static void ClearAll()
        {
            if (!IsActive)
                return;

            Browser.Window.ExecuteCachedJs("Notific.clearAll();");
        }
        #endregion

        #region Spam Check
        public static bool SpamCheck(ref DateTime dateTime, int timeout = 500, bool updateTime = false, bool notify = false)
        {
            var spam = Sync.World.ServerTime.Subtract(dateTime).TotalMilliseconds < timeout;

            if (spam && notify && Sync.World.ServerTime.Subtract(LastAntiSpamShowed).TotalMilliseconds > 500)
            {
                Notification.Show(Notification.Types.Error, Locale.Notifications.AntiSpam.Header, Locale.Notifications.AntiSpam.DontFlood, 2000);

                LastAntiSpamShowed = Sync.World.ServerTime;
            }

            if (updateTime)
                dateTime = Sync.World.ServerTime;

            return spam;
        }
        #endregion

        public static int GetTextReadingTime(string text)
        {
            var optimalTime = text.Where(x => char.IsLetterOrDigit(x)).Count() * 100;

            return optimalTime < DefTimeout ? DefTimeout : optimalTime;
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
    }
}
