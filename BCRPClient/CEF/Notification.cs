using BCRPClient.Sync;
using RAGE;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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

                this.Timeout = Timeout == -1 ? DefTimeout : Timeout;
            }
        }

        private static Dictionary<string, Instance> Prepared = new Dictionary<string, Instance>()
        {
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

            { "Spam::Warning", new Instance(Types.Information, Locale.Notifications.AntiSpam.Warning, Locale.Notifications.AntiSpam.Header) },

            { "Container::Wait", new Instance(Types.Information, Locale.Notifications.Container.Wait, Locale.Notifications.DefHeader) },
            { "Container::ReadOnly", new Instance(Types.Information, Locale.Notifications.Container.Wait, Locale.Notifications.DefHeader) },

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

            { "Business:NoMats", new Instance(Types.Error, Locale.Notifications.Money.NoMaterialsShop, Locale.Notifications.ErrorHeader) },

            { "Business::HMA", new Instance(Types.Error, Locale.Notifications.General.MaxAmountOfBusinesses, Locale.Notifications.ErrorHeader) },
            { "Business::AB", new Instance(Types.Error, Locale.Notifications.General.BusinessAlreadyBought, Locale.Notifications.ErrorHeader) },

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

            { "House::NotAllowed", new Instance(Types.Error, Locale.Notifications.House.NotAllowed, Locale.Notifications.ErrorHeader) },
            { "House::HL", new Instance(Types.Error, Locale.Notifications.House.IsLocked, Locale.Notifications.ErrorHeader) },
            { "House::CL", new Instance(Types.Error, Locale.Notifications.House.ContainersLocked, Locale.Notifications.ErrorHeader) },
            { "House::LCC", new Instance(Types.Success, Locale.Notifications.House.LightColourChanged, Locale.Notifications.DefHeader) },
            { "House::EH", new Instance(Types.Information, Locale.Notifications.House.ExpelledHouse, Locale.Notifications.DefHeader) },
            { "House::EA", new Instance(Types.Information, Locale.Notifications.House.ExpelledApartments, Locale.Notifications.DefHeader) },
            { "House::HMA", new Instance(Types.Error, Locale.Notifications.General.MaxAmountOfHouses, Locale.Notifications.ErrorHeader) },

            { "License::NTB", new Instance(Types.Error, Locale.Notifications.General.NoLicenseToBuy, Locale.Notifications.ErrorHeader) },
        };

        public Notification()
        {
            LastAntiSpamShowed = DateTime.Now;

            // 0 - type, 1 - title, 2 - text, 3 - timeout
            Events.Add("Notify::Custom", (object[] args) => Show((Types)((int)args[0]), (string)args[1], (string)args[2], (int)args[3]));

            Events.Add("Notify", (object[] args) =>
            {
                Show((string)args[0], ((Newtonsoft.Json.Linq.JArray)args[1]).ToObject<object[]>());
            });

            Events.Add("Item::Added", (object[] args) =>
            {
                CEF.Notification.Show(CEF.Notification.Types.Item, Locale.Notifications.Inventory.Header, (int)args[1] > 1 ? string.Format(Locale.Notifications.Inventory.Added, Data.Items.GetName((string)args[0]), (int)args[1]) : string.Format(Locale.Notifications.Inventory.AddedOne, Data.Items.GetName((string)args[0])), 5000);
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

        public static void Show(Types type, string title, string content, int timeout = 2500)
        {
            if (!IsActive)
                return;

            Browser.Window.ExecuteJs("Notific.draw", timeout, type.ToString(), Utils.ReplaceNewLineHtml(title), Utils.ReplaceNewLineHtml(content), MaxNotifications, false);
        }

        public static void ShowHint(string content, bool showAnyway = false, int timeout = 2500)
        {
            if (!IsActive)
                return;

            if (!showAnyway && Settings.Interface.HideHints)
                return;

            Browser.Window.ExecuteJs("Notific.draw", timeout, Types.Information.ToString(), Locale.Notifications.Hints.Header, Utils.ReplaceNewLineHtml(content), MaxNotifications, false);
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
            var spam = DateTime.Now.Subtract(dateTime).TotalMilliseconds < timeout;

            if (spam && notify && DateTime.Now.Subtract(LastAntiSpamShowed).TotalMilliseconds > 500)
            {
                Notification.Show(Notification.Types.Error, Locale.Notifications.AntiSpam.Header, Locale.Notifications.AntiSpam.DontFlood, 2000);

                LastAntiSpamShowed = DateTime.Now;
            }

            if (updateTime)
                dateTime = DateTime.Now;

            return spam;
        }
        #endregion
    }
}
