using GTANetworkAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace BCRPServer.Game
{
    class World : Script
    {
        /// <summary>Время для запроса синхронизации у Controller предмета в мс.</summary>
        public const int TimeToSync = 5000; // 5 secs
        /// <summary>Время для принятия обновленных данных выброшенных предметов в мс.</summary>
        public const int TimeToAllowUpdate = 1000;

        /// <summary>Базовый коэфициент отклонения позиции предмета от игрока</summary>
        public const float BaseOffsetCoeff = 0.5f;

        /// <summary>Текущая задача удаления предметов</summary>
        public static Task ClearItemsTask { get; set; }
        /// <summary>CTS текущей задачи удаления предметов</summary>
        private static CancellationTokenSource ClearItemsCTS { get; set; }

        /// <summary>Все выброшенные предметы на сервере</summary>
        /// <value>Словарь, где ключ - UID предмета, а значение - объект класса ItemOnGround</value>
        public static Dictionary<uint, ItemOnGround> ItemsOnGround { get; set; }

        public World()
        {
            ItemsOnGround = new Dictionary<uint, ItemOnGround>();
        }

        #region Item On Ground
        #region Item On Ground Class
        public class ItemOnGround
        {
            /// <summary>Объект самого предмета</summary>
            public Items.Item Item { get; set; }

            /// <summary>Объект модели предмета на сервере</summary>
            public GTANetworkAPI.Object Object { get; set; }

            /// <summary>CTS автоудаления предмета</summary>
            private CancellationTokenSource DeletionCTS { get; set; }

            public string ID { get => Object.GetSharedData<string>("ID"); set => Object.SetSharedData("ID", value); }

            public int Amount { get => Object.GetSharedData<int>("Amount"); set => Object.SetSharedData("Amount", value); }

            public uint UID { get => Object.GetSharedData<int>("UID").ToUInt32(); set => Object.SetSharedData("UID", value); }

            public ItemOnGround(Items.Item Item, GTANetworkAPI.Object Object)
            {
                this.Item = Item;
                this.Object = Object;

                this.Object.SetSharedData("IOG", true);

                this.ID = Item.ID;
                this.Amount = Item is Game.Items.IStackable ? (Item as Game.Items.IStackable).Amount : 1;
                this.UID = Item.UID;

                StartDeletionTask();
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
                if (!(this.Item is Game.Items.IStackable))
                    return;

                var uid = this.Item.UID;
                var newAmount = (this.Item as Game.Items.IStackable).Amount;

                Amount = newAmount;

                if (ItemsOnGround.ContainsKey(uid))
                    ItemsOnGround[uid] = this;

                this.Item.Update();
            }

            public void Delete(bool completely = false)
            {
                var uid = this.Item.UID;

                if (!ItemsOnGround.ContainsKey(uid))
                    return;

                CancelDeletionTask();

                ItemsOnGround.Remove(uid);

                NAPI.Task.Run(() =>
                {
                    this.Object.Delete();
                });

                if (completely)
                    Item.Delete();
            }
        }
        #endregion

        #region Create
        public static async Task AddItemOnGround(Items.Item item, Vector3 position, Vector3 rotation, uint dimension) => await AddItemOnGround(null, item, position, rotation, dimension);

        public static async Task AddItemOnGround(Player controller, Items.Item item)
        {
            if (controller == null)
                return;

            await AddItemOnGround(controller, item, controller.GetFrontOf(BaseOffsetCoeff), controller.Rotation, controller.Dimension);
        }

        public static async Task AddItemOnGround(Player controller, Items.Item item, Vector3 position, Vector3 rotation, uint dimension)
        {
            if (item == null)
                return;

            // Если предмет был временным - на землю не выбрасываем
            if (item.IsTemp)
            {
                item = null;

                return;
            }

            if (item is Game.Items.IStackable)
            {
                var existingAll = (await NAPI.Task.RunAsync(() => ItemsOnGround.Where(x => x.Value.Item.ID == item.ID && (x.Value.Object.Dimension == dimension || x.Value.Object.Dimension == Utils.Dimensions.Stuff) && Vector3.Distance(x.Value.Object.Position, position) <= Settings.IOG_MAX_DISTANCE_TO_STACK).Select(x => x.Value))).ToList();

                if (existingAll != null && existingAll.Any())
                {
                    int minAmount = (existingAll[0].Item as Game.Items.IStackable).Amount;
                    int minIdx = 0;

                    for (int i = 1; i < existingAll.Count(); i++)
                    {
                        var amount = (existingAll[i].Item as Game.Items.IStackable).Amount;

                        if (amount < minAmount)
                        {
                            minAmount = amount;
                            minIdx = i;
                        }
                    }

                    var existing = existingAll[minIdx];

                    existingAll.Clear();

                if ((existing.Item as Game.Items.IStackable).Amount + (item as Game.Items.IStackable).Amount <= (existing.Item as Game.Items.IStackable).MaxAmount)
                    {
                        existing.CancelDeletionTask();

                        var newAmount = (existing.Item as Game.Items.IStackable).Amount + (item as Game.Items.IStackable).Amount;
                        (existing.Item as Game.Items.IStackable).Amount = newAmount;

                        item.Delete();
                        existing.UpdateAmount();

                        existing.StartDeletionTask();

                        return;
                    }
                }
            }

            var obj = await NAPI.Task.RunAsync(() =>
            {
                return NAPI.Object.CreateObject(item.Model, position, rotation, 255, Utils.Dimensions.Stuff);
            });

            var iog = new ItemOnGround(item, obj);

            ItemsOnGround.Add(item.UID, iog);

            NAPI.Task.Run(() =>
            {
                if (obj?.Exists == true)
                    obj.Dimension = dimension;
            }, 500);
        }
        #endregion

        public static ItemOnGround GetItemOnGround(uint UID) => ItemsOnGround.ContainsKey(UID) ? ItemsOnGround[UID] : null;

        public static void ClearAllItems(int delay)
        {
            if (ClearItemsTask != null)
                ClearItemsCTS?.Cancel();

            NAPI.Task.Run(() =>
            {
                Sync.Chat.SendServer(string.Format(Locale.Chat.Server.ClearItemsSoon, delay));
            });

            ClearItemsCTS = new CancellationTokenSource();

            ClearItemsTask = new Task(async () =>
            {
                try
                {
                    await Task.Delay(delay * 1000, ClearItemsCTS.Token);
                }
                catch (Exception ex)
                {
                    ClearItemsTask = null;

                    ClearItemsCTS = null;

                    return;
                }

                NAPI.Task.Run(() =>
                {
                    int counter = 0;

                    foreach (var x in Game.World.ItemsOnGround.Values)
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
            });

            ClearItemsTask.Start();
        }

        public static void ClearAllItemsCancel()
        {
            if (ClearItemsTask != null)
            {
                ClearItemsCTS?.Cancel();

                NAPI.Task.Run(() =>
                {
                    Sync.Chat.SendServer(Locale.Chat.Server.ClearItemsCancelled);
                });
            }
        }

        #endregion
    }
}
