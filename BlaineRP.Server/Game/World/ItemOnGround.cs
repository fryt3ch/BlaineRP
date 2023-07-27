using System.Collections.Generic;
using System.Linq;
using System.Threading;
using BlaineRP.Server.EntitiesData.Players;
using BlaineRP.Server.Game.Items;
using BlaineRP.Server.Game.Management.Chat;
using GTANetworkAPI;

namespace BlaineRP.Server.Game.World
{
    public partial class Service
    {
        public class ItemOnGround
        {
            public enum Types : byte
            {
                /// <summary>Стандартный тип предмета на земле</summary>
                /// <remarks>Автоматически удаляется с определенными условиями, может быть подобран кем угодно</remarks>
                Default = 0,

                /// <summary>Тип предмета на земле, который был намеренно установлен игроком (предметы, наследующие вбстрактный класс PlaceableItem)</summary>
                /// <remarks>Предметы данного типа не удаляется автоматически, так же не могут быть подобраны кем угодно (пока действуют определенные условия)</remarks>
                PlacedItem,
            }

            /// <summary>Объект самого предмета</summary>
            public Item Item { get; set; }

            /// <summary>Объект модели предмета на сервере</summary>
            public GTANetworkAPI.Object Object { get; set; }

            /// <summary>Таймер автоудаления предмета</summary>
            private Timer DeletionTImer { get; set; }

            public string ID { get => Object.GetSharedData<string>("I"); set => Object.SetSharedData("I", value); }

            public int Amount { get => Object.GetSharedData<int>("A"); set => Object.SetSharedData("A", value); }

            public uint UID { get => Object.GetSharedData<int>("U").ToUInt32(); set => Object.SetSharedData("U", value); }

            public Types Type { get => (Types)Object.GetSharedData<int>("IOG"); set => Object.SetSharedData("IOG", value); }

            public bool IsLocked { get => Object.GetSharedData<bool?>("L") == true; set { if (value) Object.SetSharedData("L", value); else Object.ResetSharedData("L"); } }

            public PlayerInfo Owner { get; set; }

            public ItemOnGround(Item Item, GTANetworkAPI.Object Object, Types Type)
            {
                this.Item = Item;
                this.Object = Object;

                this.Type = Type;

                this.ID = Item.ID;
                this.Amount = Game.Items.Stuff.GetItemAmount(Item);
                this.UID = Item.UID;

                if (Type == Types.Default)
                {
                    StartDeletionTask();
                }
                else if (Type == Types.PlacedItem)
                {
                    this.IsLocked = true;
                }
            }

            public void StartDeletionTask()
            {
                if (DeletionTImer != null)
                    return;

                DeletionTImer = new Timer((obj) =>
                {
                    NAPI.Task.Run(() =>
                    {
                        Delete(true);
                    });
                }, null, Properties.Settings.Static.IOG_TIME_TO_AUTODELETE, Timeout.Infinite);
            }

            public void CancelDeletionTask()
            {
                if (DeletionTImer != null)
                {
                    DeletionTImer.Dispose();

                    DeletionTImer = null;
                }
            }

            public void UpdateAmount()
            {
                if (Item is Game.Items.IStackable itemStackable)
                {
                    var newAmount = itemStackable.Amount;

                    Amount = newAmount;

                    Item.Update();
                }
            }

            public void Delete(bool completely = false)
            {
                var uid = Item.UID;

                if (!ItemsOnGround.ContainsKey(uid))
                    return;

                CancelDeletionTask();

                ItemsOnGround.Remove(uid);

                Object.Delete();

                if (completely)
                {
                    Item.Delete();
                }
            }

            public bool PlayerHasAccess(PlayerData pData, bool interact, bool notifyIfNot)
            {
                if (Owner == null)
                {
                    Owner = pData.Info;

                    return true;
                }

                if (interact)
                {
                    if (Owner != pData.Info && IsLocked)
                    {
                        if (notifyIfNot)
                        {
                            pData.Player.Notify("IOG::PINA");
                        }

                        return false;
                    }

                    return true;
                }

                if (Owner != pData.Info)
                {
                    if (notifyIfNot)
                    {
                        pData.Player.Notify("IOG::PINA");
                    }

                    return false;
                }

                return true;
            }
        }

        public static ItemOnGround AddItemOnGround(PlayerData pData, Item item, Vector3 position, Vector3 rotation, uint dimension, ItemOnGround.Types type = ItemOnGround.Types.Default)
        {
            if (item == null)
                return null;

            // Если предмет был временным - на землю не выбрасываем
            if (item.IsTemp)
                return null;

            if (type == ItemOnGround.Types.Default)
            {
                if (item is IStackable itemStackable)
                {
                    var existingAll = ItemsOnGround.Values.Where(x => x.Type == type && x.Item.ID == item.ID && (x.Object.Dimension == dimension || x.Object.Dimension == Properties.Settings.Static.StuffDimension) && Vector3.Distance(x.Object.Position, position) <= Properties.Settings.Static.IOG_MAX_DISTANCE_TO_STACK).ToList();

                    if (existingAll.Count > 0)
                    {
                        int minAmount = ((IStackable)existingAll[0].Item).Amount;
                        int minIdx = 0;

                        for (int i = 1; i < existingAll.Count; i++)
                        {
                            var amount = ((IStackable)existingAll[i].Item).Amount;

                            if (amount < minAmount)
                            {
                                minAmount = amount;
                                minIdx = i;
                            }
                        }

                        var existing = existingAll[minIdx];

                        var existingItem = (IStackable)existing.Item;

                        if (existingItem.Amount + itemStackable.Amount <= existingItem.MaxAmount)
                        {
                            existing.CancelDeletionTask();

                            var newAmount = existingItem.Amount + itemStackable.Amount;
                            existingItem.Amount = newAmount;

                            item.Delete();
                            existing.UpdateAmount();

                            existing.StartDeletionTask();

                            return existing;
                        }
                    }
                }
            }

            var obj = NAPI.Object.CreateObject(item.Model, position, rotation, 255, Properties.Settings.Static.StuffDimension);

            var iog = new ItemOnGround(item, obj, type);

            if (pData != null)
                iog.Owner = pData.Info;

            ItemsOnGround.Add(item.UID, iog);

            NAPI.Task.Run(() =>
            {
                if (obj?.Exists == true)
                    obj.Dimension = dimension;
            }, 500);

            return iog;
        }

        public static ItemOnGround GetItemOnGround(uint UID) => ItemsOnGround.GetValueOrDefault(UID);

        public static void ClearAllItems(int delay)
        {
            if (ClearItemsTimer != null)
                ClearItemsTimer.Dispose();

            Management.Chat.Service.SendServer(string.Format(Language.Strings.Get("CHAT_SERVER_WORLD_CLEARITEMS_0"), delay));

            ClearItemsTimer = new Timer((obj) =>
            {
                NAPI.Task.Run(() =>
                {
                    if (ClearItemsTimer != null)
                    {
                        ClearItemsTimer.Dispose();

                        ClearItemsTimer = null;
                    }

                    int counter = 0;

                    foreach (var x in ItemsOnGround.Values.ToList())
                    {
                        x?.Delete(true);

                        counter++;
                    }

                    Management.Chat.Service.SendServer(string.Format(Language.Strings.Get("CHAT_SERVER_WORLD_CLEARITEMS_1"), counter));
                });
            }, null, delay * 1000, Timeout.Infinite);
        }

        public static void ClearAllItemsCancel()
        {
            if (ClearItemsTimer != null)
            {
                ClearItemsTimer.Dispose();

                ClearItemsTimer = null;

                Management.Chat.Service.SendServer(Language.Strings.Get("CHAT_SERVER_WORLD_CLEARITEMS_2"));
            }
        }

        public static IEnumerable<ItemOnGround> GetItemsOnGroundByOwner(PlayerInfo pInfo) => ItemsOnGround.Values.Where(x => x.Owner == pInfo);
    }
}
