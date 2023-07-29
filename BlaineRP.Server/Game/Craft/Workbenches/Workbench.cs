using System.Collections.Generic;
using System.Linq;
using BlaineRP.Server.Extensions.System;
using BlaineRP.Server.Game.EntitiesData.Players;
using BlaineRP.Server.Game.Inventory;
using BlaineRP.Server.Game.Items;
using GTANetworkAPI;

namespace BlaineRP.Server.Game.Craft.Workbenches
{
    public abstract partial class Workbench
    {
        private static Dictionary<Types, Dictionary<uint, Workbench>> AllWorkbenches { get; set; } = new Dictionary<Types, Dictionary<uint, Workbench>>()
        {
            { Types.ItemWorkbench, new Dictionary<uint, Workbench>() },
            { Types.FurnitureWorkbench, new Dictionary<uint, Workbench>() },
            { Types.StaticWorkbench, new Dictionary<uint, Workbench>() },
        };

        public static Workbench Get(Types type, uint uid) => AllWorkbenches.GetValueOrDefault(type)?.GetValueOrDefault(uid);

        private static Dictionary<WorkbenchTypes, WorkbenchData> AllWorkbenchData { get; set; } = new Dictionary<WorkbenchTypes, WorkbenchData>()
        {
            { WorkbenchTypes.Grill, new WorkbenchData(new WorkbenchTool[] { Service.FireStaticItem, null, null, null, null }, new Item[] { null, null, null, null, Service.FireStaticItem }) },

            { WorkbenchTypes.GasStove, new WorkbenchData(new WorkbenchTool[] { Service.FireStaticItem, Service.WaterStaticItem, Service.KnifeStaticItem, null, null }, null) },
            { WorkbenchTypes.KitchenSet, new WorkbenchData(new WorkbenchTool[] { Service.FireStaticItem, Service.WaterStaticItem, Service.KnifeStaticItem, Service.WhishStaticItem, Service.RollingPinStaticItem }, null) },
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

            if (PlayersObserving.Count >= Properties.Settings.Static.WORKBENCH_MAX_PLAYERS)
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

        public void ProceedCraft(List<Item> craftItems, Receipt receipt, int amount, int timeout = 0)
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
                    players1[j].InventoryUpdate(GroupTypes.CraftResult, str);

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
                var upd = Game.Items.Item.ToClientJson(Items[updateIdxes[i]], GroupTypes.CraftItems);

                Utils.InventoryUpdate(GroupTypes.CraftItems, updateIdxes[i], upd, players);
            }

            var upd1 = Game.Items.Item.ToClientJson(ResultItem, GroupTypes.CraftResult);

            Utils.InventoryUpdate(GroupTypes.CraftResult, upd1, players);
        }

        public void CancelCraft()
        {
            if (CurrentPendingCraftData != null)
            {
                CurrentPendingCraftData.Cancel();

                CurrentPendingCraftData = null;

                var players = GetPlayersObservingArray();

                var upd = Game.Items.Item.ToClientJson(ResultItem, GroupTypes.CraftResult);

                Utils.InventoryUpdate(GroupTypes.CraftResult, upd, players);
            }
        }

        public void DropAllItemsToGround(Vector3 pos, Vector3 rot, uint dim)
        {
            for (int i = 0; i < Items.Length; i++)
            {
                if (Items[i] == null || Items[i] is Game.Items.WorkbenchTool)
                    continue;

                World.Service.AddItemOnGround(null, Items[i], pos, rot, dim, World.Service.ItemOnGround.Types.Default);


                Items[i] = null;
            }

            if (ResultItem != null)
            {
                World.Service.AddItemOnGround(null, ResultItem, pos, rot, dim, World.Service.ItemOnGround.Types.Default);

                ResultItem = null;
            }
        }

        public List<Item> GetOrderedItems() => Items.Where(x => x != null).OrderBy(x => x.ID).ToList();

        public Player[] GetPlayersObservingArray() => PlayersObserving.Where(x => x.Player?.Exists == true).Select(x => x.Player).ToArray();

        public string ToClientJson() => $"{(int)WorkbenchType}^{string.Join('|', Items.Select(x => Item.ToClientJson(x, GroupTypes.CraftItems)))}^{string.Join('|', StaticData.Tools.Select(x => Item.ToClientJson(x, GroupTypes.CraftTools)))}^{Item.ToClientJson(ResultItem, GroupTypes.CraftResult)}^{(CurrentPendingCraftData != null ? CurrentPendingCraftData.CreationDate.GetUnixTimestamp().ToString() : "")}";
    }
}
