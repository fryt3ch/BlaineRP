using BCRPServer.Game.Items;
using GTANetworkAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace BCRPServer.Sync
{
    public partial class Offers
    {
        public enum Types
        {
            /// <summary>Рукопожатие</summary>
            Handshake = 0,
            /// <summary>Обмен</summary>
            Exchange,
            /// <summary>Нести игркока</summary>
            Carry,
            /// <summary>Сыграть в орел и решка</summary>
            HeadsOrTails,
            /// <summary>Приглашение во фракцию</summary>
            InviteFraction,
            /// <summary>Приглашение в организацию</summary>
            InviteOrganisation,
            /// <summary>Передать наличные</summary>
            Cash,
            /// <summary>Показать паспорт</summary>
            ShowPassport,
            /// <summary>Показать мед. карту</summary>
            ShowMedicalCard,
            /// <summary>Показать лицензии</summary>
            ShowLicenses,
            /// <summary>Показать тех. паспорт</summary>
            ShowVehiclePassport,
            /// <summary>Показать резюме</summary>
            ShowResume,
            /// <summary>Продажа имущества</summary>
            PropertySell,
            /// <summary>Поделиться меткой</summary>
            WaypointShare,
            /// <summary>Подселить в дом/квартиру</summary>
            Settle,
            /// <summary>Продать недвижимость</summary>
            SellEstate,
            /// <summary>Продать транспорт</summary>
            SellVehicle,
            /// <summary>Продать бизнес</summary>
            SellBusiness,
        }

        private static Dictionary<Types, Dictionary<bool, Action<PlayerData, PlayerData, Offer>>> OfferActions = new Dictionary<Types, Dictionary<bool, Action<PlayerData, PlayerData, Offer>>>()
        {
            {
                Types.Handshake,

                new Dictionary<bool, Action<PlayerData, PlayerData, Offer>>()
                {
                    {
                        true,

                        (pData, tData, offer) =>
                        {
                            offer.Cancel(true, false, ReplyTypes.AutoCancel, false);

                            if (pData == null || tData == null)
                                return;

                            var sPlayer = pData.Player;
                            var tPlayer = tData.Player;

                            if (sPlayer?.Exists != true || tPlayer?.Exists != true)
                                return;

                            if (!sPlayer.AreEntitiesNearby(tPlayer, Settings.ENTITY_INTERACTION_MAX_DISTANCE))
                                return;

                            if (tPlayer.Vehicle == null && sPlayer.Vehicle == null && pData.CanPlayAnimNow() && tData.CanPlayAnimNow())
                            {
                                tPlayer.Position = sPlayer.GetFrontOf(0.85f);
                                tPlayer.Heading = Utils.GetOppositeAngle(sPlayer.Heading);

                                pData.PlayAnim(Animations.FastTypes.Handshake);
                                tData.PlayAnim(Animations.FastTypes.Handshake);
                            }

                            pData.AddFamiliar(tData.Info);
                            tData.AddFamiliar(pData.Info);
                        }
                    }
                }
            },

            {
                Types.HeadsOrTails,

                new Dictionary<bool, Action<PlayerData, PlayerData, Offer>>()
                {
                    {
                        true,

                        (pData, tData, offer) =>
                        {
                            offer.Cancel(true, false, ReplyTypes.AutoCancel, false);

                            if (pData == null || tData == null)
                                return;

                            var sPlayer = pData.Player;
                            var tPlayer = tData.Player;

                            if (sPlayer?.Exists != true || tPlayer?.Exists != true)
                                return;

                            if (!sPlayer.AreEntitiesNearby(tPlayer, Settings.ENTITY_INTERACTION_MAX_DISTANCE))
                                return;

                            Sync.Chat.SendLocal(Chat.Types.Me, sPlayer, Locale.Chat.Player.HeadsOrTails1);
                            Sync.Chat.SendLocal(Chat.Types.Me, tPlayer, Locale.Chat.Player.HeadsOrTails1);

                            var res = Utils.Randoms.Chat.Next(0, 2) == 0;

                            Sync.Chat.SendLocal(Chat.Types.Do, sPlayer, res ? Locale.Chat.Player.HeadsOrTails2 : Locale.Chat.Player.HeadsOrTails3);
                        }
                    }
                }
            },

            {
                Types.Carry,

                new Dictionary<bool, Action<PlayerData, PlayerData, Offer>>()
                {
                    {
                        true,

                        (pData, tData, offer) =>
                        {
                            offer.Cancel(true, false, ReplyTypes.AutoCancel, false);

                            if (pData == null || tData == null)
                                return;

                            var sPlayer = pData.Player;
                            var tPlayer = tData.Player;

                            if (sPlayer?.Exists != true || tPlayer?.Exists != true)
                                return;

                            if (!sPlayer.AreEntitiesNearby(tPlayer, Settings.ENTITY_INTERACTION_MAX_DISTANCE))
                                return;

                            if (tPlayer.Vehicle == null && sPlayer.Vehicle == null && pData.CanPlayAnimNow() && tData.CanPlayAnimNow())
                            {
                                sPlayer.AttachEntity(tPlayer, AttachSystem.Types.Carry);
                            }
                        }
                    }
                }
            },

            {
                Types.Cash,

                new Dictionary<bool, Action<PlayerData, PlayerData, Offer>>()
                {
                    {
                        true,

                        (pData, tData, offer) =>
                        {
                            offer.Cancel(true, false, ReplyTypes.AutoCancel, false);

                            if (pData == null || tData == null)
                                return;

                            var sPlayer = pData.Player;
                            var tPlayer = tData.Player;

                            if (sPlayer?.Exists != true || tPlayer?.Exists != true)
                                return;

                            if (!sPlayer.AreEntitiesNearby(tPlayer, Settings.ENTITY_INTERACTION_MAX_DISTANCE))
                                return;

                            var cash = (offer.Data as uint?) ?? 0;

                            if (cash == 0)
                                return;

                            ulong pNewCash, tNewCash;

                            if (!pData.TryRemoveCash(cash, out pNewCash, true, tData))
                                return;

                            if (!tData.TryAddCash(cash, out tNewCash, true, pData))
                                return;

                            pData.SetCash(pNewCash);
                            tData.SetCash(tNewCash);
                        }
                    }
                }
            },

            {
                Types.Exchange,

                new Dictionary<bool, Action<PlayerData, PlayerData, Offer>>()
                {
                    {
                        true,

                        (pData, tData, offer) =>
                        {
                            offer.Cancel(true, false, ReplyTypes.AutoCancel, true);

                            if (pData == null || tData == null)
                                return;

                            var sPlayer = pData.Player;
                            var tPlayer = tData.Player;

                            if (sPlayer?.Exists != true || tPlayer?.Exists != true)
                                return;

                            if (!sPlayer.AreEntitiesNearby(tPlayer, Settings.ENTITY_INTERACTION_MAX_DISTANCE))
                                return;

                            sPlayer.CloseAll();
                            tPlayer.CloseAll();

                            sPlayer.TriggerEvent("Inventory::Show", 3);
                            tPlayer.TriggerEvent("Inventory::Show", 3);

                            offer.TradeData = new Offer.Trade();
                        }
                    },

                    {
                        false,

                        (pData, tData, offer) =>
                        {
                            offer.TradeData = null;

                            if (pData != null)
                            {
                                var sPlayer = pData.Player;

                                sPlayer?.CloseAll();
                            }

                            if (tData != null)
                            {
                                var tPlayer = tData.Player;

                                tPlayer?.CloseAll();
                            }
                        }
                    }
                }
            },

            {
                Types.SellVehicle,

                new Dictionary<bool, Action<PlayerData, PlayerData, Offer>>()
                {
                    {
                        true,

                        (pData, tData, offer) =>
                        {
                            offer.Cancel(true, false, ReplyTypes.AutoCancel, true);

                            if (pData == null || tData == null)
                                return;

                            var sPlayer = pData.Player;
                            var tPlayer = tData.Player;

                            if (sPlayer?.Exists != true || tPlayer?.Exists != true)
                                return;

                            if (!sPlayer.AreEntitiesNearby(tPlayer, Settings.ENTITY_INTERACTION_MAX_DISTANCE))
                                return;

                            if (offer.Data is Offer.PropertySellData psData)
                            {
                                if (psData.Data is VehicleData.VehicleInfo vInfo)
                                {
                                    if (!pData.OwnedVehicles.Contains(vInfo))
                                        return;

                                    tPlayer.CloseAll();

                                    tPlayer.TriggerEvent("Estate::Show", 1, 0, vInfo.ID, vInfo.VID, sPlayer, psData.Price, vInfo.Numberplate?.Tag);

                                    offer.TradeData = new Offer.Trade()
                                    {
                                        SenderReady = true,

                                        ReceiverMoney = psData.Price,
                                    };

                                    offer.TradeData.SenderVehicles.Add(vInfo);
                                }
                            }
                        }
                    },

                    {
                        false,

                        (pData, tData, offer) =>
                        {
                            offer.TradeData = null;

                            if (tData != null)
                            {
                                var tPlayer = tData.Player;

                                tPlayer?.CloseAll();
                            }
                        }
                    }
                }
            },

            {
                Types.SellBusiness,

                new Dictionary<bool, Action<PlayerData, PlayerData, Offer>>()
                {
                    {
                        true,

                        (pData, tData, offer) =>
                        {
                            offer.Cancel(true, false, ReplyTypes.AutoCancel, true);

                            if (pData == null || tData == null)
                                return;

                            var sPlayer = pData.Player;
                            var tPlayer = tData.Player;

                            if (sPlayer?.Exists != true || tPlayer?.Exists != true)
                                return;

                            if (!sPlayer.AreEntitiesNearby(tPlayer, Settings.ENTITY_INTERACTION_MAX_DISTANCE))
                                return;

                            if (offer.Data is Offer.PropertySellData psData)
                            {
                                if (psData.Data is Game.Businesses.Business businessInfo)
                                {
                                    if (!pData.OwnedBusinesses.Contains(businessInfo))
                                        return;

                                    tPlayer.CloseAll();

                                    tPlayer.TriggerEvent("Estate::Show", 1, 1, businessInfo.ID, sPlayer, psData.Price);

                                    offer.TradeData = new Offer.Trade()
                                    {
                                        SenderReady = true,

                                        ReceiverMoney = psData.Price,
                                    };

                                    offer.TradeData.SenderBusinesses.Add(businessInfo);
                                }
                            }
                        }
                    },

                    {
                        false,

                        (pData, tData, offer) =>
                        {
                            offer.TradeData = null;

                            if (tData != null)
                            {
                                var tPlayer = tData.Player;

                                tPlayer?.CloseAll();
                            }
                        }
                    }
                }
            },

            {
                Types.SellEstate,

                new Dictionary<bool, Action<PlayerData, PlayerData, Offer>>()
                {
                    {
                        true,

                        (pData, tData, offer) =>
                        {
                            offer.Cancel(true, false, ReplyTypes.AutoCancel, true);

                            if (pData == null || tData == null)
                                return;

                            var sPlayer = pData.Player;
                            var tPlayer = tData.Player;

                            if (sPlayer?.Exists != true || tPlayer?.Exists != true)
                                return;

                            if (!sPlayer.AreEntitiesNearby(tPlayer, Settings.ENTITY_INTERACTION_MAX_DISTANCE))
                                return;

                            if (offer.Data is Offer.PropertySellData psData)
                            {
                                if (psData.Data is Game.Estates.HouseBase houseBase)
                                {
                                    if (!pData.OwnedHouses.Contains(houseBase) && !pData.OwnedApartments.Contains(houseBase))
                                        return;

                                    tPlayer.CloseAll();

                                    tPlayer.TriggerEvent("Estate::Show", 1, houseBase.Type == Game.Estates.HouseBase.Types.House ? 2 : 3, houseBase.Id, sPlayer, psData.Price);

                                    offer.TradeData = new Offer.Trade()
                                    {
                                        SenderReady = true,

                                        ReceiverMoney = psData.Price,
                                    };

                                    offer.TradeData.SenderHouseBases.Add(houseBase);
                                }
                                else if (psData.Data is Game.Estates.Garage garage)
                                {
                                    if (!pData.OwnedGarages.Contains(garage))
                                        return;

                                    tPlayer.CloseAll();

                                    tPlayer.TriggerEvent("Estate::Show", 1, 4, garage.Id, sPlayer, psData.Price);

                                    offer.TradeData = new Offer.Trade()
                                    {
                                        SenderReady = true,

                                        ReceiverMoney = psData.Price,
                                    };

                                    offer.TradeData.SenderGarages.Add(garage);
                                }
                            }
                        }
                    },

                    {
                        false,

                        (pData, tData, offer) =>
                        {
                            offer.TradeData = null;

                            if (tData != null)
                            {
                                var tPlayer = tData.Player;

                                tPlayer?.CloseAll();
                            }
                        }
                    }
                }
            },

            {
                Types.WaypointShare,

                new Dictionary<bool, Action<PlayerData, PlayerData, Offer>>()
                {
                    {
                        true,

                        (pData, tData, offer) =>
                        {
                            offer.Cancel(true, false, ReplyTypes.AutoCancel, false);

                            if (pData == null || tData == null)
                                return;

                            var sPlayer = pData.Player;
                            var tPlayer = tData.Player;

                            if (sPlayer?.Exists != true || tPlayer?.Exists != true)
                                return;

                            if (!sPlayer.AreEntitiesNearby(tPlayer, Settings.ENTITY_INTERACTION_MAX_DISTANCE))
                                return;

                            var pos = offer.Data as Vector3;

                            if (pos == null)
                                return;

                            tPlayer.TriggerEvent("Player::Waypoint::Set", pos.X, pos.Y);
                        }
                    }
                }
            },

            {
                Types.Settle,

                new Dictionary<bool, Action<PlayerData, PlayerData, Offer>>()
                {
                    {
                        true,

                        (pData, tData, offer) =>
                        {
                            offer.Cancel(true, false, ReplyTypes.AutoCancel, false);

                            if (pData == null || tData == null)
                                return;

                            var sPlayer = pData.Player;
                            var tPlayer = tData.Player;

                            if (sPlayer?.Exists != true || tPlayer?.Exists != true)
                                return;

                            if (!sPlayer.AreEntitiesNearby(tPlayer, Settings.ENTITY_INTERACTION_MAX_DISTANCE))
                                return;

                            var houseBase = pData.CurrentHouseBase;

                            if (houseBase == null || houseBase.Owner != pData.Info)
                                return;

                            if (!tData.CanBeSettled(houseBase, true))
                                return;

                            houseBase.SettlePlayer(tData.Info, true, pData);
                        }
                    }
                }
            },

            {
                Types.ShowPassport,

                new Dictionary<bool, Action<PlayerData, PlayerData, Offer>>()
                {
                    {
                        true,

                        (pData, tData, offer) =>
                        {
                            offer.Cancel(true, false, ReplyTypes.AutoCancel, false);

                            if (pData == null || tData == null)
                                return;

                            var sPlayer = pData.Player;
                            var tPlayer = tData.Player;

                            if (sPlayer?.Exists != true || tPlayer?.Exists != true)
                                return;

                            if (!sPlayer.AreEntitiesNearby(tPlayer, Settings.ENTITY_INTERACTION_MAX_DISTANCE))
                                return;

                            tPlayer.TriggerEvent("Documents::Show", 0, pData.Name, pData.Surname, pData.Sex, pData.BirthDate.SerializeToJson(), null, pData.CID, pData.CreationDate.SerializeToJson(), false, pData.Info.LosSantosAllowed);

                            pData.AddFamiliar(tData.Info);
                            tData.AddFamiliar(pData.Info);
                        }
                    }
                }
            },

            {
                Types.ShowLicenses,

                new Dictionary<bool, Action<PlayerData, PlayerData, Offer>>()
                {
                    {
                        true,

                        (pData, tData, offer) =>
                        {
                            offer.Cancel(true, false, ReplyTypes.AutoCancel, false);

                            if (pData == null || tData == null)
                                return;

                            var sPlayer = pData.Player;
                            var tPlayer = tData.Player;

                            if (sPlayer?.Exists != true || tPlayer?.Exists != true)
                                return;

                            if (!sPlayer.AreEntitiesNearby(tPlayer, Settings.ENTITY_INTERACTION_MAX_DISTANCE))
                                return;

                            tPlayer.TriggerEvent("Documents::Show", 1, pData.Name, pData.Surname, pData.Licenses);

                            pData.AddFamiliar(tData.Info);
                            tData.AddFamiliar(pData.Info);
                        }
                    }
                }
            },

            {
                Types.ShowVehiclePassport,

                new Dictionary<bool, Action<PlayerData, PlayerData, Offer>>()
                {
                    {
                        true,

                        (pData, tData, offer) =>
                        {
                            offer.Cancel(true, false, ReplyTypes.AutoCancel, false);

                            if (pData == null || tData == null)
                                return;

                            var sPlayer = pData.Player;
                            var tPlayer = tData.Player;

                            if (sPlayer?.Exists != true || tPlayer?.Exists != true)
                                return;

                            if (!sPlayer.AreEntitiesNearby(tPlayer, Settings.ENTITY_INTERACTION_MAX_DISTANCE))
                                return;

                            if (offer.Data is VehicleData.VehicleInfo vInfo)
                            {
                                if (!pData.OwnedVehicles.Contains(vInfo))
                                    return;

                                vInfo.ShowPassport(tPlayer);
                            }
                        }
                    }
                }
            },

            {
                Types.ShowMedicalCard,

                new Dictionary<bool, Action<PlayerData, PlayerData, Offer>>()
                {
                    {
                        true,

                        (pData, tData, offer) =>
                        {
                            offer.Cancel(true, false, ReplyTypes.AutoCancel, false);

                            if (pData == null || tData == null)
                                return;

                            var sPlayer = pData.Player;
                            var tPlayer = tData.Player;

                            if (sPlayer?.Exists != true || tPlayer?.Exists != true)
                                return;

                            if (!sPlayer.AreEntitiesNearby(tPlayer, Settings.ENTITY_INTERACTION_MAX_DISTANCE))
                                return;

                            if (pData.Info.MedicalCard == null)
                                return;

                            tPlayer.TriggerEvent("Documents::Show", 3, pData.Name, pData.Surname, pData.Info.MedicalCard.Diagnose, pData.Info.MedicalCard.IssueFraction, pData.Info.MedicalCard.DoctorName, pData.Info.MedicalCard.IssueDate.SerializeToJson());
                        }
                    }
                }
            },
        };

        public enum ReturnTypes
        {
            Error = -1,
            TargetBusy,
            SourceHasOffer, TargetHasOffer,
            NotEnoughMoneySource, NotEnoughMoneyTarget,
            Success,
        }

        public enum ReplyTypes
        {
            Deny = 0,
            Accept,
            Busy,
            AutoCancel,
        }

        public partial class Offer
        {
            private static List<Offer> AllOffers { get; set; } = new List<Offer>();

            /// <summary>Сущность игрока, который отправил предложение</summary>
            public PlayerData Sender { get; set; }

            /// <summary>Сущность игрока, которому отправлено предложение</summary>
            public PlayerData Receiver { get; set; }

            /// <summary>Тип предложения</summary>
            public Types Type { get; set; }

            public Trade TradeData { get; set; }

            /// <summary>Timer предложения</summary>
            private Timer Timer { get; set; }

            public object Data { get; set; }

            /// <summary>Новое предложение</summary>
            /// <param name="Sender">Сущность игрока, который отправил предложение</param>
            /// <param name="Receiver">Сущность игрока, которому отправлено предложение</param>
            /// <param name="Type">Тип предложения</param>
            /// <param name="Duration">Время действия предложения (если -1 - будет использовано стандартное время)</param>
            public Offer(PlayerData Sender, PlayerData Receiver, Types Type, int Duration = -1, object Data = null)
            {
                this.Sender = Sender;
                this.Receiver = Receiver;

                this.Type = Type;

                this.Data = Data;

                if (Duration == -1)
                    Duration = Settings.OFFER_DEFAULT_DURATION;

                this.Timer = new Timer((obj) =>
                {
                    NAPI.Task.Run(() =>
                    {
                        if (Timer == null)
                            return;

                        Cancel(false, false, ReplyTypes.AutoCancel, false);
                    });
                }, null, Duration, Timeout.Infinite);

                AllOffers.Add(this);
            }

            /// <summary>Метод для отмены предложения и удаления его из списка активных предложения</summary>
            public void Cancel(bool success = false, bool isSender = false, ReplyTypes rType = ReplyTypes.AutoCancel, bool justCancelCts = false)
            {
                var ctsNull = Timer == null;

                if (ctsNull)
                {
                    OfferActions[Type].GetValueOrDefault(false)?.Invoke(Sender, Receiver, this);
                }
                else
                {
                    Timer.Dispose();

                    Timer = null;
                }

                var sender = Sender?.Player;
                var receiver = Receiver?.Player;

                if (sender?.Exists == true)
                {
                    sender.TriggerEvent("Offer::Reply::Server", false, justCancelCts, ctsNull);

                    if (!success)
                    {
                        if (rType == ReplyTypes.Deny && !isSender)
                            sender.Notify("Offer::CancelBy");
                        else if (isSender || rType == ReplyTypes.AutoCancel)
                            sender.Notify("Offer::Cancel");
                        else if (rType == ReplyTypes.Busy)
                            sender.Notify("Offer::TargetBusy");
                    }
                }

                if (receiver?.Exists == true)
                {
                    receiver.TriggerEvent("Offer::Reply::Server", false, justCancelCts, ctsNull);

                    if (!success)
                    {
                        if (rType == ReplyTypes.Deny && isSender)
                            receiver.Notify("Offer::CancelBy");
                        else if (rType != ReplyTypes.Busy)
                            receiver.Notify("Offer::Cancel");
                    }
                }

                if (justCancelCts)
                    return;

                AllOffers.Remove(this);
            }

            public void Execute()
            {
                if (Timer == null)
                    return;

                OfferActions[Type][true].Invoke(Sender, Receiver, this);
            }

            public static Offer Create(PlayerData pData, PlayerData tData, Types type, int duration = -1, object data = null)
            {
                var offer = new Offer(pData, tData, type, duration, data);

                return offer;
            }

            public static Offer GetByPlayer(PlayerData pData) => AllOffers.Where(x => x.Sender == pData || x.Receiver == pData).FirstOrDefault();

            public static Offer GetBySender(PlayerData pData) => AllOffers.Where(x => x.Sender == pData).FirstOrDefault();

            public static Offer GetByReceiver(PlayerData pData) => AllOffers.Where(x => x.Receiver == pData).FirstOrDefault();
        }
    }
}
