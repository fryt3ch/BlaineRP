using BCRPServer.Game.Items;
using GTANetworkAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace BCRPServer.Sync
{
    public class Offers
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

                            pData.AddFamiliar(tData);
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

                        async (pData, tData, offer) =>
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

                        async (pData, tData, offer) =>
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

                        async (pData, tData, offer) =>
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

                        async (pData, tData, offer) =>
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

                            tData.AddFamiliar(pData);
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

                        async (pData, tData, offer) =>
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

                            tData.AddFamiliar(pData);
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

                        async (pData, tData, offer) =>
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

                        async (pData, tData, offer) =>
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

        public class Offer
        {
            private static List<Offer> AllOffers { get; set; } = new List<Offer>();

            #region Trade Subclass
            public class PropertySellData
            {
                public object Data { get; set; }

                public ulong Price { get; set; }

                public PropertySellData(object Data, ulong Price)
                {
                    this.Data = Data;
                    this.Price = Price;
                }
            }

            public class Trade
            {
                public class TradeItem
                {
                    public Game.Items.Item ItemRoot { get; set; }
                    public int Amount { get; set; }

                    public TradeItem(Game.Items.Item ItemRoot, int Amount)
                    {
                        this.ItemRoot = ItemRoot;
                        this.Amount = Amount;
                    }

                    public string ToClientJson() => ItemRoot == null ? "" : $"{ItemRoot.ID}&{Amount}&{(ItemRoot is IStackable ? ItemRoot.BaseWeight : ItemRoot.Weight)}&{Game.Items.Stuff.GetItemTag(ItemRoot)}";
                }

                public TradeItem[] SenderItems { get; set; }
                public TradeItem[] ReceiverItems { get; set; }

                public List<VehicleData.VehicleInfo> SenderVehicles { get; set; }
                public List<VehicleData.VehicleInfo> ReceiverVehicles { get; set; }

                public List<Game.Businesses.Business> SenderBusinesses { get; set; }
                public List<Game.Businesses.Business> ReceiverBusinesses { get; set; }

                public List<Game.Estates.HouseBase> SenderHouseBases { get; set; }
                public List<Game.Estates.HouseBase> ReceiverHouseBases { get; set; }

                public List<Game.Estates.Garage> SenderGarages { get; set; }
                public List<Game.Estates.Garage> ReceiverGarages { get; set; }

                public ulong SenderMoney { get; set; }
                public ulong ReceiverMoney { get; set; }

                public bool SenderReady { get; set; }
                public bool ReceiverReady { get; set; }

                public (Game.Items.Inventory.Results Result, PlayerData PlayerError) Execute(PlayerData pData, PlayerData tData)
                {
                    var senderFreeSlots = pData.Items.Where(x => x == null).Count();

                    var senderItems = SenderItems.Where(x => x != null && x.ItemRoot != null).ToList();
                    var receiverItems = ReceiverItems.Where(x => x != null && x.ItemRoot != null).ToList();

                    var senderRemoveSlots = senderItems.Where(x => (((x.ItemRoot as Game.Items.IStackable)?.Amount ?? 1) - x.Amount) == 0).Count();
                    var receiverRemoveSlots = receiverItems.Where(x => (((x.ItemRoot as Game.Items.IStackable)?.Amount ?? 1) - x.Amount) == 0).Count();

                    if (senderFreeSlots + senderRemoveSlots < receiverItems.Count)
                        return (Game.Items.Inventory.Results.NoSpace, pData);

                    var receiverFreeSlots = tData.Items.Where(x => x == null).Count();

                    if (receiverFreeSlots + receiverRemoveSlots < senderItems.Count)
                        return (Game.Items.Inventory.Results.NoSpace, pData);

                    var senderCurrentWeight = pData.Items.Sum(x => x?.Weight ?? 0f);

                    var senderRemoveWeight = senderItems.Sum(x => x.Amount * x.ItemRoot.BaseWeight);
                    var receiverRemoveWeight = receiverItems.Sum(x => x.Amount * x.ItemRoot.BaseWeight);

                    if (senderCurrentWeight - senderRemoveWeight + receiverRemoveWeight > Settings.MAX_INVENTORY_WEIGHT)
                        return (Game.Items.Inventory.Results.NoSpace, pData);

                    var receiverCurrentWeight = tData.Items.Sum(x => x?.Weight ?? 0f);

                    if (receiverCurrentWeight - receiverRemoveWeight + senderRemoveWeight > Settings.MAX_INVENTORY_WEIGHT)
                        return (Game.Items.Inventory.Results.NoSpace, tData);

                    if (pData.Cash < SenderMoney)
                        return (Game.Items.Inventory.Results.NotEnoughMoney, pData);

                    if (tData.Cash < ReceiverMoney)
                        return (Game.Items.Inventory.Results.NotEnoughMoney, tData);

                    foreach (var x in SenderVehicles)
                    {
                        if (x.OwnerID != pData.CID)
                            return (Game.Items.Inventory.Results.Error, null);
                    }

                    foreach (var x in ReceiverVehicles)
                    {
                        if (x.OwnerID != tData.CID)
                            return (Game.Items.Inventory.Results.Error, null);
                    }

                    foreach (var x in SenderBusinesses)
                    {
                        if (x.Owner != pData.Info)
                            return (Game.Items.Inventory.Results.Error, null);
                    }

                    foreach (var x in ReceiverBusinesses)
                    {
                        if (x.Owner != pData.Info)
                            return (Game.Items.Inventory.Results.Error, null);
                    }

                    var sHCount = 0;

                    foreach (var x in SenderHouseBases)
                    {
                        if (x.Owner != pData.Info)
                            return (Game.Items.Inventory.Results.Error, null);

                        if (x.Type == Game.Estates.HouseBase.Types.House)
                            sHCount++;
                    }

                    var rHCount = 0;

                    foreach (var x in ReceiverHouseBases)
                    {
                        if (x.Owner != pData.Info)
                            return (Game.Items.Inventory.Results.Error, null);

                        if (x.Type == Game.Estates.HouseBase.Types.House)
                            rHCount++;
                    }

                    foreach (var x in SenderGarages)
                    {
                        if (x.Owner != pData.Info)
                            return (Game.Items.Inventory.Results.Error, null);
                    }

                    foreach (var x in ReceiverGarages)
                    {
                        if (x.Owner != pData.Info)
                            return (Game.Items.Inventory.Results.Error, null);
                    }

                    var pFreeVehSlots = pData.VehicleSlots;

                    if (pFreeVehSlots < 0)
                        pFreeVehSlots = 0;

                    if (ReceiverVehicles.Count > 0 && (pFreeVehSlots + SenderVehicles.Count - ReceiverVehicles.Count) < 0)
                    {
                        pData.Player.Notify("Trade::MVOW", pData.OwnedVehicles.Count);
                        tData.Player.Notify("Trade::EOP");

                        return (Game.Items.Inventory.Results.NotEnoughVehicleSlots, pData);
                    }

                    var tFreeVehSlots = tData.VehicleSlots;

                    if (tFreeVehSlots < 0)
                        tFreeVehSlots = 0;

                    if (SenderVehicles.Count > 0 && (tFreeVehSlots + ReceiverVehicles.Count - SenderVehicles.Count) < 0)
                    {
                        tData.Player.Notify("Trade::MVOW", tData.OwnedVehicles.Count);
                        pData.Player.Notify("Trade::EOP");

                        return (Game.Items.Inventory.Results.NotEnoughVehicleSlots, tData);
                    }

                    if (ReceiverBusinesses.Count > 0)
                    {
                        if (!pData.HasLicense(PlayerData.LicenseTypes.Business, true))
                        {
                            tData.Player.Notify("Trade::EOP");

                            return (Game.Items.Inventory.Results.NoBusinessLicense, pData);
                        }

                        if ((pData.BusinessesSlots + SenderBusinesses.Count - ReceiverBusinesses.Count) < 0)
                        {
                            pData.Player.Notify("Trade::MBOW", pData.OwnedBusinesses.Count);
                            tData.Player.Notify("Trade::EOP");

                            return (Game.Items.Inventory.Results.NotEnoughBusinessSlots, pData);
                        }
                    }

                    if (SenderBusinesses.Count > 0)
                    {
                        if (!tData.HasLicense(PlayerData.LicenseTypes.Business, true))
                        {
                            pData.Player.Notify("Trade::EOP");

                            return (Game.Items.Inventory.Results.NoBusinessLicense, tData);
                        }

                        if ((tData.BusinessesSlots + ReceiverBusinesses.Count - SenderBusinesses.Count) < 0)
                        {
                            tData.Player.Notify("Trade::MBOW", tData.OwnedBusinesses.Count);
                            pData.Player.Notify("Trade::EOP");

                            return (Game.Items.Inventory.Results.NotEnoughBusinessSlots, tData);
                        }
                    }

                    if (ReceiverGarages.Count > 0 && (pData.GaragesSlots + SenderGarages.Count - ReceiverGarages.Count) < 0)
                    {
                        pData.Player.Notify("Trade::MGOW", pData.OwnedGarages.Count);
                        tData.Player.Notify("Trade::EOP");

                        return (Game.Items.Inventory.Results.NotEnoughGarageSlots, pData);
                    }

                    if (SenderGarages.Count > 0 && (tData.GaragesSlots + ReceiverGarages.Count - SenderGarages.Count) < 0)
                    {
                        tData.Player.Notify("Trade::MGOW", tData.OwnedGarages.Count);
                        pData.Player.Notify("Trade::EOP");

                        return (Game.Items.Inventory.Results.NotEnoughGarageSlots, tData);
                    }

                    if (rHCount > 0)
                    {
                        if (pData.SettledHouseBase?.Type == Game.Estates.HouseBase.Types.House)
                        {
                            pData.Player.Notify("Trade::ASH");
                            tData.Player.Notify("Trade::EOP");

                            return (Game.Items.Inventory.Results.SettledToHouse, pData);
                        }

                        if ((pData.HouseSlots + sHCount - rHCount) < 0)
                        {
                            pData.Player.Notify("Trade::MHOW", pData.OwnedHouses.Count);
                            tData.Player.Notify("Trade::EOP");

                            return (Game.Items.Inventory.Results.NotEnoughHouseSlots, pData);
                        }
                    }

                    if (sHCount > 0)
                    {
                        if (pData.SettledHouseBase?.Type == Game.Estates.HouseBase.Types.House)
                        {
                            tData.Player.Notify("Trade::ASH");
                            pData.Player.Notify("Trade::EOP");

                            return (Game.Items.Inventory.Results.SettledToHouse, tData);
                        }

                        if ((tData.HouseSlots + rHCount - sHCount) < 0)
                        {
                            tData.Player.Notify("Trade::MHOW", tData.OwnedHouses.Count);
                            pData.Player.Notify("Trade::EOP");

                            return (Game.Items.Inventory.Results.NotEnoughHouseSlots, tData);
                        }
                    }

                    rHCount = ReceiverHouseBases.Count - rHCount;
                    sHCount = SenderHouseBases.Count - sHCount;

                    if (rHCount > 0)
                    {
                        if (pData.SettledHouseBase?.Type == Game.Estates.HouseBase.Types.Apartments)
                        {
                            pData.Player.Notify("Trade::ASA");
                            tData.Player.Notify("Trade::EOP");

                            return (Game.Items.Inventory.Results.SettledToApartments, pData);
                        }

                        if ((pData.ApartmentsSlots + sHCount - rHCount) < 0)
                        {
                            pData.Player.Notify("Trade::MAOW", pData.OwnedApartments.Count);
                            tData.Player.Notify("Trade::EOP");

                            return (Game.Items.Inventory.Results.NotEnoughApartmentsSlots, pData);
                        }
                    }

                    if (sHCount > 0)
                    {
                        if (tData.SettledHouseBase?.Type == Game.Estates.HouseBase.Types.Apartments)
                        {
                            tData.Player.Notify("Trade::ASA");
                            pData.Player.Notify("Trade::EOP");

                            return (Game.Items.Inventory.Results.SettledToApartments, tData);
                        }

                        if ((tData.ApartmentsSlots + rHCount - sHCount) < 0)
                        {
                            tData.Player.Notify("Trade::MAOW", tData.OwnedApartments.Count);
                            pData.Player.Notify("Trade::EOP");

                            return (Game.Items.Inventory.Results.NotEnoughApartmentsSlots, tData);
                        }
                    }

                    foreach (var x in SenderVehicles)
                    {
                        x.OwnerID = tData.CID;

                        pData.RemoveVehicleProperty(x);
                        tData.AddVehicleProperty(x);

                        MySQL.VehicleOwnerUpdate(x);
                    }

                    foreach (var x in ReceiverVehicles)
                    {
                        x.OwnerID = pData.CID;

                        tData.RemoveVehicleProperty(x);
                        pData.AddVehicleProperty(x);

                        MySQL.VehicleOwnerUpdate(x);
                    }

                    foreach (var x in SenderBusinesses)
                    {
                        x.ChangeOwner(tData.Info);
                    }

                    foreach (var x in ReceiverBusinesses)
                    {
                        x.ChangeOwner(pData.Info);
                    }

                    foreach (var x in SenderHouseBases)
                    {
                        x.ChangeOwner(tData.Info);
                    }

                    foreach (var x in ReceiverHouseBases)
                    {
                        x.ChangeOwner(pData.Info);
                    }

                    foreach (var x in SenderGarages)
                    {
                        x.ChangeOwner(tData.Info);
                    }

                    foreach (var x in ReceiverGarages)
                    {
                        x.ChangeOwner(pData.Info);
                    }

                    if (ReceiverMoney > 0)
                    {
                        ulong pNewCash;

                        if (tData.TryRemoveCash(ReceiverMoney, out pNewCash, true, pData))
                        {
                            tData.SetCash(pNewCash);

                            if (pData.TryAddCash(ReceiverMoney, out pNewCash, true, tData))
                                pData.SetCash(pNewCash);
                        }
                    }

                    if (SenderMoney > 0)
                    {
                        ulong pNewCash;

                        if (pData.TryRemoveCash(SenderMoney, out pNewCash, true, pData))
                        {
                            pData.SetCash(pNewCash);

                            if (tData.TryAddCash(SenderMoney, out pNewCash, true, tData))
                                tData.SetCash(pNewCash);
                        }
                    }

                    var senderSlotsToUpdate = new List<(int, string)>();
                    var receiverSlotsToUpdate = new List<(int, string)>();

                    for (int i = 0; i < senderItems.Count; i++)
                    {
                        if (senderItems[i].ItemRoot is Game.Items.IStackable senderItemS && senderItems[i].Amount < senderItemS.Amount)
                        {
                            for (int j = 0; j < pData.Items.Length; j++)
                                if (pData.Items[j] == senderItems[i].ItemRoot)
                                {
                                    senderItemS.Amount -= senderItems[i].Amount;

                                    senderSlotsToUpdate.Add((j, Game.Items.Item.ToClientJson(pData.Items[j], Game.Items.Inventory.Groups.Items)));

                                    break;
                                }

                            senderItems[i].ItemRoot = Game.Items.Stuff.CreateItem(senderItems[i].ItemRoot.ID, 0, senderItems[i].Amount, false);

                            if (senderItems[i].ItemRoot == null)
                                return (Game.Items.Inventory.Results.Error, null);
                        }
                        else
                        {
                            for (int j = 0; j < pData.Items.Length; j++)
                                if (pData.Items[j] == senderItems[i].ItemRoot)
                                {
                                    pData.Items[j] = null;

                                    senderSlotsToUpdate.Add((j, Game.Items.Item.ToClientJson(pData.Items[j], Game.Items.Inventory.Groups.Items)));

                                    break;
                                }
                        }
                    }

                    for (int i = 0; i < receiverItems.Count; i++)
                    {
                        if (receiverItems[i].ItemRoot is Game.Items.IStackable receiverItemS && receiverItems[i].Amount < receiverItemS.Amount)
                        {
                            for (int j = 0; j < tData.Items.Length; j++)
                                if (tData.Items[j] == receiverItems[i].ItemRoot)
                                {
                                    receiverItemS.Amount -= receiverItems[i].Amount;

                                    receiverSlotsToUpdate.Add((j, Game.Items.Item.ToClientJson(tData.Items[j], Game.Items.Inventory.Groups.Items)));

                                    break;
                                }

                            receiverItems[i].ItemRoot = Game.Items.Stuff.CreateItem(receiverItems[i].ItemRoot.ID, 0, receiverItems[i].Amount, false);

                            if (receiverItems[i].ItemRoot == null)
                                return (Game.Items.Inventory.Results.Error, null);
                        }
                        else
                        {
                            for (int j = 0; j < tData.Items.Length; j++)
                                if (tData.Items[j] == receiverItems[i].ItemRoot)
                                {
                                    tData.Items[j] = null;

                                    receiverSlotsToUpdate.Add((j, Game.Items.Item.ToClientJson(tData.Items[j], Game.Items.Inventory.Groups.Items)));

                                    break;
                                }
                        }
                    }

                    for (int i = 0; i < senderItems.Count; i++)
                    {
                        for (int j = 0; j < tData.Items.Length; j++)
                        {
                            if (tData.Items[j] == null)
                            {
                                tData.Items[j] = senderItems[i].ItemRoot;

                                receiverSlotsToUpdate.Add((j, Game.Items.Item.ToClientJson(tData.Items[j], Game.Items.Inventory.Groups.Items)));

                                break;
                            }
                        }
                    }

                    for (int i = 0; i < receiverItems.Count; i++)
                    {
                        for (int j = 0; j < pData.Items.Length; j++)
                        {
                            if (pData.Items[j] == null)
                            {
                                pData.Items[j] = receiverItems[i].ItemRoot;

                                senderSlotsToUpdate.Add((j, Game.Items.Item.ToClientJson(pData.Items[j], Game.Items.Inventory.Groups.Items)));

                                break;
                            }
                        }
                    }

                    MySQL.CharacterItemsUpdate(pData.Info);
                    MySQL.CharacterItemsUpdate(tData.Info);

                    if (senderSlotsToUpdate.Count % 2 != 0)
                    {
                        pData.Player.InventoryUpdate(Game.Items.Inventory.Groups.Items, senderSlotsToUpdate[0].Item1, senderSlotsToUpdate[0].Item2);

                        for (int i = 1; i < senderSlotsToUpdate.Count; i += 2)
                            pData.Player.InventoryUpdate(Game.Items.Inventory.Groups.Items, senderSlotsToUpdate[i].Item1, senderSlotsToUpdate[i].Item2, Game.Items.Inventory.Groups.Items, senderSlotsToUpdate[i + 1].Item1, senderSlotsToUpdate[i + 1].Item2);
                    }
                    else
                    {
                        for (int i = 0; i < senderSlotsToUpdate.Count; i += 2)
                            pData.Player.InventoryUpdate(Game.Items.Inventory.Groups.Items, senderSlotsToUpdate[i].Item1, senderSlotsToUpdate[i].Item2, Game.Items.Inventory.Groups.Items, senderSlotsToUpdate[i + 1].Item1, senderSlotsToUpdate[i + 1].Item2);
                    }

                    if (receiverSlotsToUpdate.Count % 2 != 0)
                    {
                        tData.Player.InventoryUpdate(Game.Items.Inventory.Groups.Items, receiverSlotsToUpdate[0].Item1, receiverSlotsToUpdate[0].Item2);

                        for (int i = 1; i < receiverSlotsToUpdate.Count; i += 2)
                            tData.Player.InventoryUpdate(Game.Items.Inventory.Groups.Items, receiverSlotsToUpdate[i].Item1, receiverSlotsToUpdate[i].Item2, Game.Items.Inventory.Groups.Items, receiverSlotsToUpdate[i + 1].Item1, receiverSlotsToUpdate[i + 1].Item2);
                    }
                    else
                    {
                        for (int i = 0; i < receiverSlotsToUpdate.Count; i += 2)
                            tData.Player.InventoryUpdate(Game.Items.Inventory.Groups.Items, receiverSlotsToUpdate[i].Item1, receiverSlotsToUpdate[i].Item2, Game.Items.Inventory.Groups.Items, receiverSlotsToUpdate[i + 1].Item1, receiverSlotsToUpdate[i + 1].Item2);
                    }

                    return (Game.Items.Inventory.Results.Success, null);
                }

                public Trade()
                {
                    SenderItems = new TradeItem[5];
                    ReceiverItems = new TradeItem[5];

                    SenderVehicles = new List<VehicleData.VehicleInfo>();
                    ReceiverVehicles = new List<VehicleData.VehicleInfo>();

                    SenderBusinesses = new List<Game.Businesses.Business>();
                    ReceiverBusinesses = new List<Game.Businesses.Business>();

                    SenderHouseBases = new List<Game.Estates.HouseBase>();
                    ReceiverHouseBases = new List<Game.Estates.HouseBase>();

                    SenderGarages = new List<Game.Estates.Garage>();
                    ReceiverGarages = new List<Game.Estates.Garage>();

                    SenderMoney = 0;
                    ReceiverMoney = 0;

                    SenderReady = false;
                    ReceiverReady = false;
                }
            }
            #endregion

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
