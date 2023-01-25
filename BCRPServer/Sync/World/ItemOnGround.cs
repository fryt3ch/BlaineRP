using BCRPServer.Game.Items;
using GTANetworkAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace BCRPServer.Sync
{
    public partial class World
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

            /// <summary>CTS автоудаления предмета</summary>
            private CancellationTokenSource DeletionCTS { get; set; }

            public string ID { get => Object.GetSharedData<string>("I"); set => Object.SetSharedData("I", value); }

            public int Amount { get => Object.GetSharedData<int>("A"); set => Object.SetSharedData("A", value); }

            public uint UID { get => Object.GetSharedData<int>("U").ToUInt32(); set => Object.SetSharedData("U", value); }

            public Types Type { get => (Types)Object.GetSharedData<int>("IOG"); set => Object.SetSharedData("IOG", value); }

            public bool IsLocked { get => Object.GetSharedData<bool?>("L") == true; set { if (value) Object.SetSharedData("L", value); else Object.ResetSharedData("L"); } }

            public PlayerData.PlayerInfo Owner { get; set; }

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
                if (this.DeletionCTS != null)
                    return;

                this.DeletionCTS = new CancellationTokenSource();

                Task.Run(async () =>
                {
                    try
                    {
                        await Task.Delay(Settings.IOG_TIME_TO_AUTODELETE, this.DeletionCTS.Token);

                        NAPI.Task.Run(() =>
                        {
                            this.DeletionCTS?.Cancel();

                            var uid = this.Item.UID;

                            this.Object?.Delete();

                            ItemsOnGround.Remove(uid);

                            this.Item.Delete();

                            this.Item = null;
                        });
                    }
                    catch (Exception ex)
                    {
                        NAPI.Task.Run(() =>
                        {
                            this.DeletionCTS?.Cancel();
                        });
                    }
                });
            }

            public void CancelDeletionTask()
            {
                this.DeletionCTS?.Cancel();
                this.DeletionCTS = null;
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
                    var existingAll = ItemsOnGround.Values.Where(x => x.Type == type && x.Item.ID == item.ID && (x.Object.Dimension == dimension || x.Object.Dimension == Utils.Dimensions.Stuff) && Vector3.Distance(x.Object.Position, position) <= Settings.IOG_MAX_DISTANCE_TO_STACK).ToList();

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

            var obj = NAPI.Object.CreateObject(item.Model, position, rotation, 255, Utils.Dimensions.Stuff);

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
            if (ClearItemsCTS != null)
                ClearItemsCTS.Cancel();

            Sync.Chat.SendServer(string.Format(Locale.Chat.Server.ClearItemsSoon, delay));

            ClearItemsCTS = new CancellationTokenSource();

            (new Task(async () =>
            {
                try
                {
                    await Task.Delay(delay * 1000, ClearItemsCTS.Token);
                }
                catch (Exception ex)
                {
                    ClearItemsCTS = null;

                    return;
                }

                NAPI.Task.Run(() =>
                {
                    int counter = 0;

                    foreach (var x in ItemsOnGround.Values)
                    {
                        if (x == null)
                            continue;

                        try
                        {
                            x?.Delete(true);

                            counter++;
                        }
                        catch (Exception ex) { }
                    }

                    Sync.Chat.SendServer(string.Format(Locale.Chat.Server.ClearItems, counter));
                });
            })).Start();
        }

        public static void ClearAllItemsCancel()
        {
            if (ClearItemsCTS != null)
            {
                ClearItemsCTS.Cancel();

                Sync.Chat.SendServer(Locale.Chat.Server.ClearItemsCancelled);
            }
        }

        public static IEnumerable<ItemOnGround> GetItemsOnGroundByOwner(PlayerData.PlayerInfo pInfo) => ItemsOnGround.Values.Where(x => x.Owner == pInfo);
    }
}
