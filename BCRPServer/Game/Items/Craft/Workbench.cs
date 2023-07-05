using GTANetworkAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace BCRPServer.Game.Items.Craft
{
    public abstract partial class Workbench
    {
        public class PendingCraftData
        {
            public Timer Timer { get; private set; }

            public DateTime CreationDate { get; private set; }

            public Craft.Receipt Receipt { get; private set; }

            public int Amount { get; private set; }

            public List<Item> CraftItems { get; private set; }

            public bool IsInProcess => Timer != null;

            public PendingCraftData(List<Item> CraftItems, Craft.Receipt Receipt, int Amount = 1)
            {
                this.CraftItems = CraftItems;

                this.Receipt = Receipt;
                this.Amount = Amount;
            }

            public void Start(Workbench wb, int timeout = 0)
            {
                if (IsInProcess)
                    return;

                CreationDate = Utils.GetCurrentTime().AddMilliseconds(timeout);

                Timer = new Timer((obj) =>
                {
                    NAPI.Task.Run(() =>
                    {
                        if (!wb.Exists)
                            return;

                        wb.ProceedCraft(CraftItems, Receipt, Amount, 0);
                    });
                }, null, timeout, Timeout.Infinite);
            }

            public void Cancel()
            {
                if (!IsInProcess)
                    return;

                Timer.Dispose();

                Timer = null;
            }
        }

        public class WorkbenchData
        {
            public WorkbenchTool[] Tools { get; private set; }

            public Item[] DefaultCraftItems { get; private set; }

            public WorkbenchData(WorkbenchTool[] Tools, Item[] DefaultCraftItems)
            {
                this.Tools = Tools;

                this.DefaultCraftItems = DefaultCraftItems;
            }
        }

        private static Dictionary<Types, Dictionary<uint, Workbench>> AllWorkbenches { get; set; } = new Dictionary<Types, Dictionary<uint, Workbench>>()
        {
            { Types.ItemWorkbench, new Dictionary<uint, Workbench>() },
            { Types.FurnitureWorkbench, new Dictionary<uint, Workbench>() },
            { Types.StaticWorkbench, new Dictionary<uint, Workbench>() },
        };

        public static Workbench Get(Types type, uint uid) => AllWorkbenches.GetValueOrDefault(type)?.GetValueOrDefault(uid);

        public enum WorkbenchTypes
        {
            Grill,

            GasStove,
            KitchenSet,

            CraftTable,
        }

        private static Dictionary<WorkbenchTypes, WorkbenchData> AllWorkbenchData { get; set; } = new Dictionary<WorkbenchTypes, WorkbenchData>()
        {
            { WorkbenchTypes.Grill, new WorkbenchData(new WorkbenchTool[] { Craft.FireStaticItem, null, null, null, null }, new Item[] { null, null, null, null, Craft.FireStaticItem }) },

            { WorkbenchTypes.GasStove, new WorkbenchData(new WorkbenchTool[] { Craft.FireStaticItem, Craft.WaterStaticItem, Craft.KnifeStaticItem, null, null }, null) },
            { WorkbenchTypes.KitchenSet, new WorkbenchData(new WorkbenchTool[] { Craft.FireStaticItem, Craft.WaterStaticItem, Craft.KnifeStaticItem, Craft.WhishStaticItem, Craft.RollingPinStaticItem }, null) },
        };

        public enum Types : byte
        {
            ItemWorkbench = 0,
            FurnitureWorkbench,
            StaticWorkbench,
        }

        public uint Uid { get; set; }

        public WorkbenchTypes WorkbenchType { get; set; }

        public Types Type { get; set; }

        public Item[] Items { get; set; }

        public Item ResultItem { get; set; }

        public WorkbenchData StaticData => AllWorkbenchData[WorkbenchType];

        /// <summary>Игроки, в данный момент использующие верстак</summary>
        /// <value>Список сущностей игроков</value>
        public List<PlayerData> PlayersObserving { get; set; }

        public PendingCraftData CurrentPendingCraftData { get; set; }

        public bool Exists => AllWorkbenches[Type].ContainsValue(this);

        public abstract bool IsNear(PlayerData pData);

        public abstract bool IsAccessableFor(PlayerData pData);

        public bool Delete()
        {
            var res = AllWorkbenches[Type].Remove(Uid);

            CurrentPendingCraftData?.Cancel();

            for (int i = 0; i < Items.Length; i++)
            {
                if (Items[i] == null || Items[i] is Game.Items.WorkbenchTool)
                    continue;

                Items[i].Delete();

                Items[i] = null;
            }

            if (ResultItem != null)
            {
                ResultItem.Delete();

                ResultItem = null;
            }

            var players = new List<Player>();

            PlayersObserving.ForEach(x =>
            {
                x.CurrentWorkbench = null;

                if (x.Player?.Exists == true)
                {
                    players.Add(x.Player);
                }
            });

            if (players.Count > 0)
                NAPI.ClientEvent.TriggerClientEventToPlayers(players.ToArray(), "Inventory::Close");

            return res;
        }

        public bool AddPlayerObserving(PlayerData pData)
        {
            ClearAllWrongObservers();

            if (PlayersObserving.Count >= Settings.WORKBENCH_MAX_PLAYERS)
                return false;

            pData.CurrentWorkbench = this;

            PlayersObserving.Add(pData);

            return true;
        }

        public void RemovePlayerObserving(PlayerData pData, bool callRemoteClose)
        {
            PlayersObserving.Remove(pData);

            pData.CurrentWorkbench = null;

            if (callRemoteClose)
                pData.Player.TriggerEvent("Inventory::Close");
        }

        public void ClearAllWrongObservers()
        {
            if (PlayersObserving.Count == 0)
                return;

            var players = new List<Player>();

            PlayersObserving.ToList().ForEach(x =>
            {
                var target = x.Player;

                if (target?.Exists != true || !IsNear(x) || !IsAccessableFor(x))
                {
                    players.Add(target);

                    x.CurrentWorkbench = null;

                    PlayersObserving.Remove(x);
                }
            });

            if (players.Count > 0)
                NAPI.ClientEvent.TriggerClientEventToPlayers(players.ToArray(), "Inventory::Close");
        }

        public void ClearAllObservers()
        {
            if (PlayersObserving.Count == 0)
                return;

            var players = new List<Player>();

            PlayersObserving.ForEach(x =>
            {
                x.CurrentWorkbench = null;

                players.Add(x.Player);
            });

            PlayersObserving.Clear();

            if (players.Count > 0)
                NAPI.ClientEvent.TriggerClientEventToPlayers(players.ToArray(), "Inventory::Close");
        }

        public Workbench(uint Uid, Types Type, WorkbenchTypes WorkbenchType)
        {
            this.PlayersObserving = new List<PlayerData>();

            this.Uid = Uid;

            this.Type = Type;

            this.WorkbenchType = WorkbenchType;

            this.Items = StaticData.DefaultCraftItems?.ToArray() ?? new Item[5];

            AllWorkbenches[Type].Add(Uid, this);
        }

        public void ProceedCraft(List<Item> craftItems, Craft.Receipt receipt, int amount, int timeout = 0)
        {
            if (CurrentPendingCraftData != null)
            {
                CurrentPendingCraftData.Cancel();

                CurrentPendingCraftData = null;
            }

            if (timeout > 0)
            {

                CurrentPendingCraftData = new PendingCraftData(craftItems, receipt, amount);

                CurrentPendingCraftData.Start(this, timeout);

                var players1 = GetPlayersObservingArray();

                var str = CurrentPendingCraftData.CreationDate.ToString();

                for (int j = 0; j < players1.Length; j++)
                    players1[j].InventoryUpdate(Inventory.GroupTypes.CraftResult, str);

                return;
            }

            var updateIdxes = new List<int>();

            for (int i = 0; i < receipt.CraftNeededItems.Count; i++)
            {
                var item = craftItems[i];

                if (receipt.CraftNeededItems[i].Amount <= 0)
                    continue;

                var realIdx = -1;

                for (int j = 0; j < Items.Length; j++)
                {
                    if (Items[j] == item)
                    {
                        realIdx = j;

                        break;
                    }
                }

                if (item is Game.Items.IStackable itemStackable)
                {
                    itemStackable.Amount -= receipt.CraftNeededItems[i].Amount * amount;

                    if (itemStackable.Amount <= 0)
                    {
                        item.Delete();

                        item = null;
                    }
                    else
                    {
                        item.Update();
                    }
                }
                else
                {
                    item.Delete();

                    item = null;
                }

                if (realIdx >= 0)
                {
                    updateIdxes.Add(realIdx);

                    Items[realIdx] = item;
                }
            }

            var players = GetPlayersObservingArray();

            ResultItem = Game.Items.Stuff.CreateItem(receipt.CraftResultData.ResultItem.Id, 0, receipt.CraftResultData.ResultItem.Amount * amount, false);

            if (players.Length == 0)
                return;

            for (int i = 0; i < updateIdxes.Count; i++)
            {
                var upd = Game.Items.Item.ToClientJson(Items[updateIdxes[i]], Inventory.GroupTypes.CraftItems);

                Utils.InventoryUpdate(Inventory.GroupTypes.CraftItems, updateIdxes[i], upd, players);
            }

            var upd1 = Game.Items.Item.ToClientJson(ResultItem, Inventory.GroupTypes.CraftResult);

            Utils.InventoryUpdate(Inventory.GroupTypes.CraftResult, upd1, players);
        }

        public void CancelCraft()
        {
            if (CurrentPendingCraftData != null)
            {
                CurrentPendingCraftData.Cancel();

                CurrentPendingCraftData = null;

                var players = GetPlayersObservingArray();

                var upd = Game.Items.Item.ToClientJson(ResultItem, Inventory.GroupTypes.CraftResult);

                Utils.InventoryUpdate(Inventory.GroupTypes.CraftResult, upd, players);
            }
        }

        public void DropAllItemsToGround(Vector3 pos, Vector3 rot, uint dim)
        {
            for (int i = 0; i < Items.Length; i++)
            {
                if (Items[i] == null || Items[i] is Game.Items.WorkbenchTool)
                    continue;

                Sync.World.AddItemOnGround(null, Items[i], pos, rot, dim, Sync.World.ItemOnGround.Types.Default);


                Items[i] = null;
            }

            if (ResultItem != null)
            {
                Sync.World.AddItemOnGround(null, ResultItem, pos, rot, dim, Sync.World.ItemOnGround.Types.Default);

                ResultItem = null;
            }
        }

        public List<Item> GetOrderedItems() => Items.Where(x => x != null).OrderBy(x => x.ID).ToList();

        public Player[] GetPlayersObservingArray() => PlayersObserving.Where(x => x.Player?.Exists == true).Select(x => x.Player).ToArray();

        public string ToClientJson() => $"{(int)WorkbenchType}^{string.Join('|', Items.Select(x => Item.ToClientJson(x, Inventory.GroupTypes.CraftItems)))}^{string.Join('|', StaticData.Tools.Select(x => Item.ToClientJson(x, Inventory.GroupTypes.CraftTools)))}^{Item.ToClientJson(ResultItem, Inventory.GroupTypes.CraftResult)}^{(CurrentPendingCraftData != null ? CurrentPendingCraftData.CreationDate.GetUnixTimestamp().ToString() : "")}";
    }

    public class ItemWorkbench : Workbench
    {
        public static ItemWorkbench Get(uint uid) => Workbench.Get(Types.ItemWorkbench, uid) as ItemWorkbench;

        public Sync.World.ItemOnGround OwnerEntity { get; set; }

        public override bool IsNear(PlayerData pData)
        {
            if (OwnerEntity.Object?.Exists != true)
                return false;

            if (pData.Player.Dimension != OwnerEntity.Object.Dimension)
                return false;

            if (pData.Player.Position.DistanceTo(OwnerEntity.Object.Position) > Settings.ENTITY_INTERACTION_MAX_DISTANCE)
                return false;

            return true;
        }

        public override bool IsAccessableFor(PlayerData pData)
        {
            if (OwnerEntity.Object?.Exists != true)
                return false;

            return OwnerEntity.PlayerHasAccess(pData, true, true);
        }

        public ItemWorkbench(uint Uid, WorkbenchTypes Workbenchtype, Sync.World.ItemOnGround OwnerEntity) : base(Uid, Types.ItemWorkbench, Workbenchtype)
        {
            this.OwnerEntity = OwnerEntity;
        }
    }

    public class FurnitureWorkbench : Workbench
    {
        public static FurnitureWorkbench Get(uint uid) => Workbench.Get(Types.FurnitureWorkbench, uid) as FurnitureWorkbench;

        public Estates.HouseBase HouseBase { get; private set; }

        public override bool IsNear(PlayerData pData)
        {
            if (pData.CurrentHouseBase is Estates.HouseBase houseBase)
            {
                if (houseBase != HouseBase)
                    return false;

                return true;
            }

            return false;
        }

        public override bool IsAccessableFor(PlayerData pData)
        {
            if (!HouseBase.ContainersLocked)
                return true;

            if (HouseBase.Owner == pData.Info)
                return true;

            if (HouseBase.Settlers.GetValueOrDefault(pData.Info) != null)
                return true;

            pData.Player.Notify("House::NotAllowed");

            return false;
        }

        public FurnitureWorkbench(uint Uid, Estates.HouseBase HouseBase, WorkbenchTypes Workbenchtype) : base(Uid, Types.FurnitureWorkbench, Workbenchtype)
        {
            this.HouseBase = HouseBase;
        }
    }

    public class StaticWorkbench : Workbench
    {
        public uint Dimension { get; set; }

        public Vector3 Position { get; set; }

        public float InteractionRange { get; set; }

        public override bool IsNear(PlayerData pData)
        {
            if (pData.Player.Dimension != Dimension)
                return false;

            if (pData.Player.Position.DistanceTo(Position) > InteractionRange)
                return false;

            return true;
        }

        public override bool IsAccessableFor(PlayerData pData)
        {
            return true;
        }

        public StaticWorkbench(uint Uid, WorkbenchTypes Workbenchtype) : base(Uid, Types.StaticWorkbench, Workbenchtype)
        {

        }
    }
}
