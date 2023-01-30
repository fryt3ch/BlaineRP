using BCRPClient.Data;
using RAGE;
using RAGE.Elements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;

namespace BCRPClient.CEF
{
    public class Inventory : Events.Script
    {
        public static bool IsActive { get => Browser.IsActiveOr(Browser.IntTypes.Inventory, Browser.IntTypes.CratesInventory, Browser.IntTypes.Trade, Browser.IntTypes.Workbench) || ActionBox.CurrentContext == ActionBox.Contexts.Inventory; }

        private static int FreezeCounter { get; set; }

        public enum Types
        {
            None = -1,
            Inventory = 0,
            Container = 1,
            Trade = 3,
            Workbench = 4,
        }

        public enum ContainerTypes
        {
            None = -1,
            Trunk,
            Locker,
            Storage,
            Crate,
            Fridge,
            Wardrobe,
        }

        public enum WorkbenchTypes
        {
            None = -1,

            Grill,

            GasStove,
            KitchenSet,

            CraftTable,
        }

        private static Dictionary<string, int> Groups = new Dictionary<string, int>()
        {
            { "pockets", 0 }, { "bag", 1 }, { "weapon", 2 }, { "holster", 3 }, { "clothes", 4 }, { "accessories", 5 }, { "bagItem", 6 }, { "holsterItem", 7 }, { "armour", 8 },
            { "crate", 9 }, { "give", 10 }, { "craft", 20 }, { "tool", 21 }, { "result", 22 },
        };

        private static int GetTooltipGroup()
        {
            if (CurrentType == Types.Container)
            {
                return 0;
            }
            else if (CurrentType == Types.Inventory)
            {
                return (BagData != null ? 1 : 2);
            }
            else
                return 3;
        }

        public class ItemParams
        {
            public string Id { get; set; }

            public bool InUse { get; set; }

            public ItemParams(string Id)
            {
                this.Id = Id;
            }
        }

        #region Variables
        private static bool FirstOpenInv { get => Player.LocalPlayer.HasData("Inv::Temp::FOI"); set { if (value) Player.LocalPlayer.SetData("Inv::Temp::FOI", true); else Player.LocalPlayer.ResetData("Inv::Temp::FOI"); } }
        private static bool FirstOpenCrate { get => Player.LocalPlayer.HasData("Inv::Temp::FOC"); set { if (value) Player.LocalPlayer.SetData("Inv::Temp::FOC", true); else Player.LocalPlayer.ResetData("Inv::Temp::FOC"); } }
        private static bool FirstOpenWorkbench { get => Player.LocalPlayer.HasData("Inv::Temp::FOW"); set { if (value) Player.LocalPlayer.SetData("Inv::Temp::FOW", true); else Player.LocalPlayer.ResetData("Inv::Temp::FOW"); } }

        private static Types CurrentType { get; set; }
        private static ContainerTypes CurrentContainerType { get; set; }
        private static WorkbenchTypes CurrentWorkbenchType { get; set; }

        private static DateTime LastShowed;
        public static DateTime LastSent;

        private static ItemParams[] ItemsParams { get; set; }
        private static Data.Craft.ItemPrototype[] WorkbenchCraftParams { get; set; }
        private static Data.Craft.ItemPrototype[] WorkbenchToolsParams { get; set; }

        private static object[][] WeaponsData { get; set; }
        private static object[] ArmourData { get; set; }
        private static object[][] ItemsData { get; set; }
        private static object[][] ClothesData { get; set; }
        private static object[][] AccessoriesData { get; set; }
        private static object[][] BagData { get; set; }
        private static object[][] ContainerData { get; set; }
        private static object[][] WorkbenchCraftData { get; set; }
        private static object[][] WorkbenchToolsData { get; set; }
        private static object[][] WorkbenchResultData { get; set; }

        private static float BagWeight { get; set; } = 0f;

        private static (string, int)? CurrentSlotFrom { get; set; }
        private static (string, int)? CurrentSlotTo { get; set; }
        private static int? CurrentAction { get; set; }

        private static Entity CurrentEntity { get; set; }

        private static List<int> TempBinds { get; set; }
        private static int TempBindEsc { get; set; }

        private static HashSet<int> ItemSlotsToUpdate { get; set; }
        private static HashSet<int> ItemSlotsToUpdateCrate { get; set; }
        private static HashSet<int> ItemSlotsToUpdateWorkbench { get; set; }

        private static HashSet<int> WeaponsSlotsToUpdate { get; set; }
        private static HashSet<int> ClothesSlotsToUpdate { get; set; }
        private static HashSet<int> AccessoriesSlotsToUpdate { get; set; }
        private static HashSet<int> BagSlotsToUpdate { get; set; }
        private static HashSet<int> BagSlotsToUpdateCrate { get; set; }

        private static bool UpdateBag { get; set; }
        private static bool UpdateBagCrate { get; set; }
        private static bool UpdateArmour { get; set; }

        private static bool UpdateTooltips { get; set; } = false;

        private static int CurrentGiveMoney { get; set; } = 0;

        private static object[][] GiveItemsData { get; set; }

        private static List<string> CurrentGiveProperties { get; set; }
        private static List<string> CurrentGetProperties { get; set; }
        #endregion

        #region Fillers
        private static object[] FillWeapon(string type, int ammo, bool inUse, string tag, string wcStr)
        {
            var iType = Data.Items.GetType(type);
            var imgId = Data.Items.GetImageId(type, iType);

            var name = (tag == null || tag.Length == 0) ? Data.Items.GetName(type) : Data.Items.GetName(type) + $" [{tag}]";

            var tooltips = new List<object>();

            tooltips.AddRange(new object[] { new object[] { 4, Locale.General.Inventory.Actions.TakeOff }, new object[] { 5, inUse ? Locale.General.Inventory.Actions.FromHands : Locale.General.Inventory.Actions.ToHands }, new object[] { 6, Locale.General.Inventory.Actions.Load }, new object[] { 1, Locale.General.Inventory.Actions.Unload } });

            if (wcStr.Length > 0)
            {
                var wcData = wcStr.Split('_');

                foreach (var x in wcData.Select(x => (Sync.WeaponSystem.Weapon.ComponentTypes)int.Parse(x)))
                {
                    tooltips.Add(new object[] { 7 + (int)x,  Locale.General.Inventory.Actions.WeaponComponentsTakeOffStrings.GetValueOrDefault(x) ?? "null" });
                }
            }

            tooltips.Add(new object[] { 2, Locale.General.Inventory.Actions.Drop });

            if (inUse && Additional.AntiCheat.LastAllowedAmmo < ammo)
                ammo = Additional.AntiCheat.LastAllowedAmmo;

            return new object[] { imgId, name, tooltips.ToArray(), ammo, inUse };
        }
        
        private static object[] FillArmour(string type, int strength)
        {
            var iType = Data.Items.GetType(type);
            var imgId = Data.Items.GetImageId(type, iType);

            return new object[] { imgId, Data.Items.GetName(type), new object[] { new object[] { 4, Locale.General.Inventory.Actions.TakeOff }, new object[] { 2, Locale.General.Inventory.Actions.Drop } }, strength };
        }

        private static object[] FillCraftResultItem(string id, int amount, float weight, string tag, bool isReady)
        {
            var iType = Data.Items.GetType(id);
            var imgId = Data.Items.GetImageId(id, iType);

            var name = (tag == null || tag.Length < 1) ? Data.Items.GetName(id) : Data.Items.GetName(id) + $" [{tag}]";

            if (!isReady)
                return new object[] { imgId, name, null, amount, weight };

            return new object[] { imgId, name, Data.Items.GetActions(iType, id, amount, true, false, true, true, false), amount, weight };
        }

        private static object[] FillCraftToolsItem(string id, int amount, float weight, string tag)
        {
            var iType = Data.Items.GetType(id);
            var imgId = Data.Items.GetImageId(id, iType);

            var name = (tag == null || tag.Length < 1) ? Data.Items.GetName(id) : Data.Items.GetName(id) + $" [{tag}]";

            return new object[] { imgId, name, Data.Items.GetActions(iType, id, amount, true, false, true, true, false, false), null, null };
        }

        private static object[] FillCraftItem(string id, int amount, float weight, string tag)
        {
            var iType = Data.Items.GetType(id);
            var imgId = Data.Items.GetImageId(id, iType);

            var name = (tag == null || tag.Length < 1) ? Data.Items.GetName(id) : Data.Items.GetName(id) + $" [{tag}]";

            if (iType == typeof(Data.Items.WorkbenchTool))
                return new object[] { imgId, name, Data.Items.GetActions(iType, id, amount, true, false, true, true, false, false), null, null };
            else
                return new object[] { imgId, name, Data.Items.GetActions(iType, id, amount, true, false, true, true, false, true), amount, weight };
        }

        private static object[] FillItem(string id, int amount, float weight, string tag, bool inUse, bool inBag, bool inContainer, bool inTrade)
        {
            var iType = Data.Items.GetType(id);
            var imgId = Data.Items.GetImageId(id, iType);

            var name = (tag == null || tag.Length < 1) ? Data.Items.GetName(id) : Data.Items.GetName(id) + $" [{tag}]";

            if (inUse)
                name = name.Insert(0, "[A] ");

            if (inContainer)
                return new object[] { imgId, name, Data.Items.GetActions(iType, id, amount, inBag, inUse, true, true), amount, weight };
            else if (inBag)
                return new object[] { new object[] { imgId, name, Data.Items.GetActions(iType, id, amount, inBag, inUse, true, true), amount, weight }, new object[] { imgId, name, Data.Items.GetActions(iType, id, amount, inBag, inUse, true, false), amount, weight } };
            else if (inTrade)
                return new object[] { imgId, name, new object[] { 4, Locale.General.Inventory.Actions.ShiftOutOfTrade }, amount, weight };

            var item1 = new object[] { imgId, name, Data.Items.GetActions(iType, id, amount, inBag, inUse, true, true), amount, weight };
            var item2 = new object[] { imgId, name, Data.Items.GetActions(iType, id, amount, inBag, inUse, true, false), amount, weight };
            var item3 = new object[] { imgId, name, Data.Items.GetActions(iType, id, amount, inBag, inUse, false, false), amount, weight };
            var item4 = new object[] { imgId, name, new object[] { 4, Locale.General.Inventory.Actions.ShiftTrade }, amount, weight };

            return new object[] { item1, item2, item3, item4 };
        }
        private static object[] FillClothes(string type)
        {
            var iType = Data.Items.GetType(type);
            var imgId = Data.Items.GetImageId(type, iType);

            var actions = new List<object[]>() { new object[] { 4, Locale.General.Inventory.Actions.TakeOff }, new object[] { 2, Locale.General.Inventory.Actions.Drop } };

            if (typeof(Data.Items.Clothes.IToggleable).IsAssignableFrom(iType) && Data.Items.GetData(type, iType) is Data.Items.Clothes.ItemData.IToggleable data && data.ExtraData != null)
            {
                actions.Insert(1, new object[] { 5, Locale.General.Inventory.Actions.Reset });
            }

            return new object[] { imgId, Data.Items.GetName(type), actions.ToArray() };
        }
        private static object[] FillAccessories(string type)
        {
            var iType = Data.Items.GetType(type);
            var imgId = Data.Items.GetImageId(type, iType);

            var actions = new List<object[]>() { new object[] { 4, Locale.General.Inventory.Actions.TakeOff }, new object[] { 2, Locale.General.Inventory.Actions.Drop } };

            if (typeof(Data.Items.Clothes.IToggleable).IsAssignableFrom(iType))
            {
                actions.Insert(1, new object[] { 5, Locale.General.Inventory.Actions.Reset });
            }

            return new object[] { imgId, Data.Items.GetName(type), actions.ToArray() };
        }
        #endregion

        public Inventory()
        {
            FirstOpenInv = true;
            FirstOpenCrate = true;
            FirstOpenWorkbench = true;

            ItemSlotsToUpdate = new HashSet<int>();
            ItemSlotsToUpdateCrate = new HashSet<int>();
            ItemSlotsToUpdateWorkbench = new HashSet<int>();

            WeaponsSlotsToUpdate = new HashSet<int>();
            ClothesSlotsToUpdate = new HashSet<int>();
            AccessoriesSlotsToUpdate = new HashSet<int>();
            BagSlotsToUpdate = new HashSet<int>();
            BagSlotsToUpdateCrate = new HashSet<int>();
            UpdateArmour = false;
            UpdateBag = false;
            UpdateBagCrate = false;

            LastShowed = DateTime.Now;
            LastSent = DateTime.Now;

            CurrentType = Types.None;
            CurrentContainerType = ContainerTypes.None;

            CurrentEntity = null;

            TempBinds = new List<int>();
            TempBindEsc = -1;

            #region Events

            Events.Add("Workbench::TryCraft", (args) =>
            {
                if (CurrentType != Types.Workbench)
                    return;

                if (LastSent.IsSpam(1000, false, false))
                    return;

                if (!Player.LocalPlayer.HasData("Inv::Temp::WBCPT"))
                {
                    var notNullItems = WorkbenchCraftParams.Where(x => x != null).OrderBy(x => x.Id).ToList();

                    var receipt = Data.Craft.Receipt.GetByIngredients(notNullItems);

                    if (receipt == null)
                    {
                        return;
                    }

                    if (WorkbenchResultData != null && WorkbenchResultData[0] != null && WorkbenchResultData[0][2] != null)
                    {
                        CEF.Notification.Show(Notification.Types.Error, Locale.Notifications.ErrorHeader, Locale.Notifications.Inventory.WorkbenchResultItemExists);

                        return;
                    }

                    Events.CallRemote("Workbench::Craft", receipt.Index);

                    LastSent = DateTime.Now;
                }
                else
                {
                    Events.CallRemote("Workbench::Craft", -1);

                    LastSent = DateTime.Now;
                }
            });

            #region Show
            Events.Add("Inventory::Show", async (object[] args) =>
            {
                if (IsActive)
                    return;

                CurrentType = (Types)(int)args[0];

                SetWindowReady();

                if (CurrentType == Types.Inventory)
                {
                    bool ammoUpdated = false;
                    int activeWeapon = -1;

                    if (WeaponsData != null)
                    {
                        for (int i = 0; i < WeaponsData.Length; i++)
                            if ((bool?)WeaponsData[i]?[4] == true)
                            {
                                activeWeapon = i;

                                break;
                            }

                        if (activeWeapon != -1)
                        {
                            var selectedWeapon = Player.LocalPlayer.GetSelectedWeapon();

                            if (selectedWeapon == Sync.WeaponSystem.UnarmedHash || selectedWeapon == Sync.WeaponSystem.MobileHash)
                            {
                                WeaponsData[activeWeapon][4] = false;

                                ammoUpdated = true;
                            }
                            else if ((int)WeaponsData[activeWeapon][3] != Additional.AntiCheat.LastAllowedAmmo)
                            {
                                WeaponsData[activeWeapon][3] = Additional.AntiCheat.LastAllowedAmmo;

                                ammoUpdated = true;
                            }
                        }
                    }

                    Browser.Window.ExecuteJs("Inventory.switchThirdWeapon", AccessoriesData[9] != null);

                    if (FirstOpenInv)
                    {
                        Browser.Window.ExecuteJs("Inventory.fillPockets", new object[] { "inv", ItemsData.Select(x => x?[GetTooltipGroup()]), Settings.MAX_INVENTORY_WEIGHT });
                        Browser.Window.ExecuteJs("Inventory.fillVest", new object[] { ArmourData });
                        Browser.Window.ExecuteJs("Inventory.fillBag", new object[] { "inv", BagData == null ? null : BagData.Select(x => x?[GetTooltipGroup()]), BagWeight });
                        Browser.Window.ExecuteJs("Inventory.fillWeapon", new object[] { WeaponsData });
                        Browser.Window.ExecuteJs("Inventory.fillClothes", new object[] { ClothesData });
                        Browser.Window.ExecuteJs("Inventory.fillAccessories", new object[] { AccessoriesData });

                        FirstOpenInv = false;
                    }
                    else
                    {
                        if (UpdateArmour)
                        {
                            Browser.Window.ExecuteJs("Inventory.fillVest", new object[] { ArmourData });

                            UpdateArmour = false;
                        }

                        foreach (var slot in ItemSlotsToUpdate)
                            Browser.Window.ExecuteJs("Inventory.updatePocketsSlot", new object[] { slot, "inv", ItemsData[slot]?[GetTooltipGroup()] });

                        ItemSlotsToUpdate.Clear();

                        if (BagData != null)
                        {
                            if (UpdateBag)
                            {
                                Browser.Window.ExecuteJs("Inventory.fillBag", new object[] { "inv", BagData == null ? null : BagData.Select(x => x?[GetTooltipGroup()]), BagWeight });

                                UpdateBag = false;
                            }
                            else
                            {
                                foreach (var slot in BagSlotsToUpdate)
                                    Browser.Window.ExecuteJs("Inventory.updateBagSlot", new object[] { slot, "inv", BagData[slot]?[GetTooltipGroup()] });
                            }
                        }
                        else
                            Browser.Window.ExecuteJs("Inventory.fillBag", new object[] { "inv", null, null });

                        BagSlotsToUpdate.Clear();

                        if (ammoUpdated && !WeaponsSlotsToUpdate.Contains(activeWeapon))
                            Browser.Window.ExecuteJs("Inventory.updateWeaponSlot", new object[] { activeWeapon, WeaponsData[activeWeapon] });

                        foreach (var slot in WeaponsSlotsToUpdate)
                            Browser.Window.ExecuteJs("Inventory.updateWeaponSlot", new object[] { slot, WeaponsData[slot] });

                        WeaponsSlotsToUpdate.Clear();

                        foreach (var slot in ClothesSlotsToUpdate)
                            Browser.Window.ExecuteJs("Inventory.updateClothesSlot", new object[] { slot, ClothesData[slot] });

                        ClothesSlotsToUpdate.Clear();

                        foreach (var slot in AccessoriesSlotsToUpdate)
                            Browser.Window.ExecuteJs("Inventory.updateAccessoriesSlot", new object[] { slot, AccessoriesData[slot] });

                        AccessoriesSlotsToUpdate.Clear();

                        if (UpdateTooltips)
                        {
                            for (int i = 0; i < ItemsData.Length; i++)
                                if (ItemsData[i] != null)
                                    Browser.Window.ExecuteJs("Inventory.updatePocketsTooltip", new object[] { i, "inv", ((object[])ItemsData[i][GetTooltipGroup()])[2] });

                            UpdateTooltips = false;
                        }
                    }

                    UpdateBinds();
                }
                else if (CurrentType == Types.Container)
                {
                    if (FirstOpenCrate)
                    {
                        Browser.Window.ExecuteJs("Inventory.fillPockets", new object[] { "crate", ItemsData.Select(x => x?[0]), Settings.MAX_INVENTORY_WEIGHT });
                        Browser.Window.ExecuteJs("Inventory.fillBag", new object[] { "crate", BagData == null ? null : BagData.Select(x => x?[0]), BagWeight });

                        FirstOpenCrate = false;
                    }
                    else
                    {
                        foreach (var slot in ItemSlotsToUpdateCrate)
                            Browser.Window.ExecuteJs("Inventory.updatePocketsSlot", new object[] { slot, "crate", ItemsData[slot]?[0] });

                        ItemSlotsToUpdateCrate.Clear();

                        if (BagData != null)
                        {
                            if (UpdateBagCrate)
                            {
                                Browser.Window.ExecuteJs("Inventory.fillBag", new object[] { "crate", BagData == null ? null : BagData.Select(x => x?[0]), BagWeight });

                                UpdateBagCrate = false;
                            }
                            else
                            {
                                foreach (var slot in BagSlotsToUpdateCrate)
                                    Browser.Window.ExecuteJs("Inventory.updateBagSlot", new object[] { slot, "crate", BagData[slot]?[0] });
                            }
                        }
                        else
                            Browser.Window.ExecuteJs("Inventory.fillBag", new object[] { "crate", null, null });

                        BagSlotsToUpdateCrate.Clear();
                    }

                    var contData = ((string)args[1]).Split('|');

                    var contItems = contData.Skip(1).Select(x => x.Length == 0 ? null : x.Split('&')).ToArray();

                    contData = contData[0].Split('&');

                    CurrentContainerType = (ContainerTypes)int.Parse(contData[0]);

                    ContainerData = new object[contItems.Length][];

                    for (int i = 0; i < ContainerData.Length; i++)
                        if (contItems[i] != null)
                            ContainerData[i] = FillItem(contItems[i][0], int.Parse(contItems[i][1]), float.Parse(contItems[i][2]), contItems[i][3], false, true, true, false);

                    Browser.Window.ExecuteJs("Inventory.fillCrate", new object[] { Locale.General.Containers.Names[CurrentContainerType], ContainerData, float.Parse(contData[1]) });
                }
                else if (CurrentType == Types.Trade)
                {
                    var pData = Sync.Players.GetData(Player.LocalPlayer);

                    if (pData == null)
                        return;

                    GiveItemsData = new object[5][];

                    CurrentGiveProperties = new List<string>();
                    CurrentGetProperties = new List<string>();

                    Browser.Window.ExecuteCachedJs("Inventory.switchUnderline", 0);

                    Browser.Window.ExecuteCachedJs("Inventory.fillCheckBox", "last", false);
                    Browser.Window.ExecuteCachedJs("Inventory.switchTradeBtn", false);

                    var properties = new List<string>();

                    var propIds = new List<(Sync.Players.PropertyTypes, uint)>();

                    foreach (var x in pData.OwnedVehicles)
                    {
                        properties.Add(string.Format(Locale.Property.VehicleTradeInfoStr, x.Data.TypeName, x.Data.Name, x.VID));

                        propIds.Add((Sync.Players.PropertyTypes.Vehicle, x.VID));
                    }

                    foreach (var x in pData.OwnedHouses)
                    {
                        properties.Add(string.Format(Locale.Property.HouseTradeInfoStr, x.Id));

                        propIds.Add((Sync.Players.PropertyTypes.House, x.Id));
                    }

                    foreach (var x in pData.OwnedApartments)
                    {
                        properties.Add(string.Format(Locale.Property.ApartmentsTradeInfoStr, Data.Locations.ApartmentsRoot.All[x.RootType].Name, x.NumberInRoot + 1));

                        propIds.Add((Sync.Players.PropertyTypes.Apartments, x.Id));
                    }

                    foreach (var x in pData.OwnedGarages)
                    {
                        properties.Add(string.Format(Locale.Property.GarageTradeInfoStr, Data.Locations.GarageRoot.All[x.RootType].Name, x.NumberInRoot + 1));

                        propIds.Add((Sync.Players.PropertyTypes.Garage, x.Id));
                    }

                    foreach (var x in pData.OwnedBusinesses)
                    {
                        properties.Add(string.Format(Locale.Property.BusinessTradeInfoStr, x.Name, x.SubId));

                        propIds.Add((Sync.Players.PropertyTypes.Business, (uint)x.Id));
                    }

                    Player.LocalPlayer.SetData("Trade::Temp::PropIds", propIds);

                    Browser.Window.ExecuteJs("Inventory.fillTradeLProperties", new object[] { properties });

                    Browser.Window.ExecuteJs("Inventory.fillPockets", new object[] { "trade", ItemsData.Select(x => x?[3]), Settings.MAX_INVENTORY_WEIGHT });

                    Browser.Window.ExecuteJs("Inventory.updateReceiveMoney", 0);
                    Browser.Window.ExecuteJs("Inventory.updateGiveMoney", 0);

                    Browser.Window.ExecuteJs("Inventory.fillTradeReceive", new object[] { new object[] { null, null, null, null, null } });
                    Browser.Window.ExecuteJs("Inventory.fillTradeGive", new object[] { new object[] { null, null, null, null, null } });

                    Browser.Window.ExecuteJs("Inventory.fillTradeRProperties", new object[] { "give", new object[] { } });
                    Browser.Window.ExecuteJs("Inventory.fillTradeRProperties", new object[] { "receive", new object[] { } });
                }
                else if (CurrentType == Types.Workbench)
                {
                    if (FirstOpenWorkbench)
                    {
                        Browser.Window.ExecuteJs("Inventory.fillPockets", new object[] { "wb", ItemsData.Select(x => x?[0]), Settings.MAX_INVENTORY_WEIGHT });

                        FirstOpenWorkbench = false;
                    }
                    else
                    {
                        foreach (var slot in ItemSlotsToUpdateWorkbench)
                            Browser.Window.ExecuteJs("Inventory.updatePocketsSlot", new object[] { slot, "wb", ItemsData[slot]?[0] });

                        ItemSlotsToUpdateWorkbench.Clear();
                    }

                    var currentDate = Utils.GetServerTime();

                    var benchData = ((string)args[1]).Split('^');

                    var benchItems = benchData[1].Split('|').Select(x => x.Length == 0 ? null : x.Split('&')).ToArray();
                    var benchTools = benchData[2].Split('|').Select(x => x.Length == 0 ? null : x.Split('&')).ToArray();
                    var benchResultItem = benchData[3].Length == 0 ? null : benchData[3].Split('&');

                    var benchCraftEndDate = benchData[4].Length == 0 ? currentDate : DateTime.Parse(benchData[4]);

                    var craftTimeLeft = benchData[4].Length == 0 ? 0 : benchCraftEndDate.Subtract(currentDate).TotalMilliseconds;

                    benchData = benchData[0].Split('&');

                    var benchType = (WorkbenchTypes)int.Parse(benchData[0]);

                    CurrentWorkbenchType = benchType;

                    if (craftTimeLeft > 0)
                    {
                        StartPendingCraftTask(benchCraftEndDate);
                    }

                    WorkbenchCraftData = new object[benchItems.Length][];
                    WorkbenchCraftParams = new Data.Craft.ItemPrototype[benchItems.Length];

                    for (int i = 0; i < WorkbenchCraftData.Length; i++)
                    {
                        if (benchItems[i] != null)
                        {
                            var amount = int.Parse(benchItems[i][1]);

                            WorkbenchCraftData[i] = FillCraftItem(benchItems[i][0], amount, float.Parse(benchItems[i][2]), benchItems[i][3]);

                            WorkbenchCraftParams[i] = new Data.Craft.ItemPrototype(benchItems[i][0], amount);
                        }
                    }

                    WorkbenchToolsData = new object[benchTools.Length][];
                    WorkbenchToolsParams = new Craft.ItemPrototype[WorkbenchToolsData.Length];

                    for (int i = 0; i < WorkbenchToolsData.Length; i++)
                    {
                        if (benchTools[i] != null)
                        {
                            if (!WorkbenchCraftParams.Where(x => x != null && x.Id == benchTools[i][0]).Any())
                                WorkbenchToolsData[i] = FillCraftToolsItem(benchTools[i][0], 1, 0f, null);

                            WorkbenchToolsParams[i] = new Craft.ItemPrototype(benchTools[i][0], 1);
                        }
                    }

                    WorkbenchResultData = new object[1][];

                    if (benchResultItem != null)
                    {
                        WorkbenchResultData[0] = FillCraftResultItem(benchResultItem[0], int.Parse(benchResultItem[1]), float.Parse(benchResultItem[2]), benchResultItem[3], true);

                        Browser.Window.ExecuteJs("Inventory.updateResultSlot", new object[] { WorkbenchResultData[0] });

                        ResetCraftButton();
                    }
                    else
                    {
                        UpdateCraftItemVisualization();
                    }

                    Browser.Window.ExecuteJs("Inventory.fillWbCraft", new object[] { Locale.General.Containers.WorkbenchNames[benchType].Name, WorkbenchCraftData });
                    Browser.Window.ExecuteJs("Inventory.fillWbTools", new object[] { WorkbenchToolsData });
                }
            });
            #endregion

            #region Update
            // 0 - pockets, 1 - bag, 2 - weapons, 3 - holster, 4 - clothes, 5 - accessories, 6 - bagItem, 7 - holsterItem, 8 - armour, 9 - container, 10 - trade-give (items), 11 - trade-give (properties/money), 12 - trade-get (items), 13 - trade-get (properties/money)
            Events.Add("Inventory::Update", (object[] args) =>
            {
                var usedGroups = new HashSet<int>();

                for (int k = 0; k < args.Length / 3; k++)
                {
                    var curArgs = args.Skip(k * 3).ToArray();

                    int id = (int)curArgs[0];

                    usedGroups.Add(id);

                    if (id == 0)
                    {
                        int slot = (int)curArgs[1];

                        var data = ((string)curArgs[2]).Length == 0 ? null : ((string)curArgs[2]).Split('&');

                        if (data == null)
                        {
                            ItemsData[slot] = null;
                            ItemsParams[slot] = null;
                        }
                        else
                        {
                            var inUse = int.Parse(data[4]) == 1;

                            ItemsData[slot] = data == null ? null : FillItem(data[0], int.Parse(data[1]), float.Parse(data[2]), data[3], inUse, false, false, false);
                            ItemsParams[slot] = new ItemParams(data[0]) { InUse = inUse };
                        }

                        if (CurrentType == Types.Inventory)
                        {
                            Browser.Window.ExecuteJs("Inventory.updatePocketsSlot", new object[] { slot, "inv", ItemsData[slot]?[GetTooltipGroup()] });

                            ItemSlotsToUpdateCrate.Add(slot);
                            ItemSlotsToUpdateWorkbench.Add(slot);
                        }
                        else if (CurrentType == Types.Container)
                        {
                            Browser.Window.ExecuteJs("Inventory.updatePocketsSlot", new object[] { slot, "crate", ItemsData[slot]?[0] });

                            ItemSlotsToUpdate.Add(slot);
                            ItemSlotsToUpdateWorkbench.Add(slot);
                        }
                        else if (CurrentType == Types.Workbench)
                        {
                            Browser.Window.ExecuteJs("Inventory.updatePocketsSlot", new object[] { slot, "wb", ItemsData[slot]?[0] });

                            ItemSlotsToUpdate.Add(slot);
                            ItemSlotsToUpdateCrate.Add(slot);
                        }
                        else
                        {
                            ItemSlotsToUpdate.Add(slot);
                            ItemSlotsToUpdateCrate.Add(slot);
                            ItemSlotsToUpdateWorkbench.Add(slot);
                        }
                    }
                    else if (id == 1)
                    {
                        int slot = (int)curArgs[1];
                        var data = ((string)curArgs[2]).Length == 0 ? null : ((string)curArgs[2]).Split('&');

                        BagData[slot] = data == null ? null : FillItem(data[0], int.Parse(data[1]), float.Parse(data[2]), data[3], false, true, false, false);

                        if (CurrentType == Types.Inventory)
                        {
                            Browser.Window.ExecuteJs("Inventory.updateBagSlot", new object[] { slot, "inv", BagData[slot]?[GetTooltipGroup()] });

                            BagSlotsToUpdateCrate.Add(slot);
                        }
                        else if (CurrentType == Types.Container)
                        {
                            Browser.Window.ExecuteJs("Inventory.updateBagSlot", new object[] { slot, "crate", BagData[slot]?[0] });

                            BagSlotsToUpdate.Add(slot);
                        }
                        else
                        {
                            BagSlotsToUpdate.Add(slot);
                            BagSlotsToUpdateCrate.Add(slot);
                        }
                    }
                    else if (id == 2 || id == 3)
                    {
                        int slot = (int)curArgs[1];
                        var data = ((string)curArgs[2]).Length == 0 ? null : ((string)curArgs[2]).Split('&');

                        WeaponsData[slot] = data == null ? null : FillWeapon(data[0], int.Parse(data[1]), int.Parse(data[2]) == 1, data[3], data[4]);

                        if (CurrentType == Types.Inventory)
                            Browser.Window.ExecuteJs("Inventory.updateWeaponSlot", new object[] { slot, WeaponsData[slot] });
                        else
                            WeaponsSlotsToUpdate.Add(slot);
                    }
                    else if (id == 4)
                    {
                        int slot = (int)curArgs[1];
                        var data = ((string)curArgs[2]).Length == 0 ? null : ((string)curArgs[2]).Split('&');

                        ClothesData[slot] = data == null ? null : FillClothes(data[0]);

                        if (CurrentType == Types.Inventory)
                            Browser.Window.ExecuteJs("Inventory.updateClothesSlot", new object[] { slot, ClothesData[slot] });
                        else
                            ClothesSlotsToUpdate.Add(slot);
                    }
                    else if (id == 5)
                    {
                        int slot = (int)curArgs[1];
                        var data = ((string)curArgs[2]).Length == 0 ? null : ((string)curArgs[2]).Split('&');

                        AccessoriesData[slot] = data == null ? null : FillAccessories(data[0]);

                        if (CurrentType == Types.Inventory)
                            Browser.Window.ExecuteJs("Inventory.updateAccessoriesSlot", new object[] { slot, AccessoriesData[slot] });
                        else
                            AccessoriesSlotsToUpdate.Add(slot);
                    }
                    else if (id == 6)
                    {
                        var data = ((string)curArgs[2]).Length == 0 ? null : ((string)curArgs[2]).Split('|');

                        if (data == null)
                        {
                            AccessoriesData[8] = null;
                            BagData = null;

                            if (CurrentType == Types.Inventory)
                            {
                                Browser.Window.ExecuteJs("Inventory.fillBag", new object[] { "inv", null, null });

                                for (int i = 0; i < ItemsData.Length; i++)
                                    if (ItemsData[i] != null)
                                        Browser.Window.ExecuteJs("Inventory.updatePocketsTooltip", new object[] { i, "inv", ((object[])ItemsData[i][2])[2] });
                            }
                            else if (CurrentType == Types.Container)
                            {
                                Browser.Window.ExecuteJs("Inventory.fillBag", new object[] { "crate", null, null });

                                UpdateTooltips = true;
                            }
                            else
                                UpdateTooltips = true;

                            UpdateBag = false;
                            UpdateBagCrate = false;
                        }
                        else
                        {
                            var items = data.Skip(1).Select(x => x.Length == 0 ? null : x.Split('&')).ToArray();

                            data = data[0].Split('&');

                            AccessoriesData[8] = FillAccessories(data[0]);
                            BagData = new object[items.Length][];

                            for (int i = 0; i < BagData.Length; i++)
                                if (items[i] != null)
                                    BagData[i] = FillItem(items[i][0], int.Parse(items[i][1]), float.Parse(items[i][2]), items[i][3], false, true, false, false);

                            BagWeight = float.Parse(data[1]);

                            if (CurrentType == Types.Inventory)
                            {
                                Browser.Window.ExecuteJs("Inventory.fillBag", new object[] { "inv", BagData.Select(x => x?[1]), BagWeight });

                                for (int i = 0; i < ItemsData.Length; i++)
                                    if (ItemsData[i] != null)
                                        Browser.Window.ExecuteJs("Inventory.updatePocketsTooltip", new object[] { i, "inv", ((object[])ItemsData[i][1])[2] });

                                UpdateTooltips = false;
                                UpdateBagCrate = true;
                            }
                            else if (CurrentType == Types.Container)
                            {
                                Browser.Window.ExecuteJs("Inventory.fillBag", new object[] { "crate", BagData.Select(x => x?[0]), BagWeight });

                                UpdateTooltips = true;
                                UpdateBag = true;
                            }
                            else
                            {
                                UpdateBag = true;
                                UpdateBagCrate = true;

                                UpdateTooltips = true;
                            }
                        }

                        if (CurrentType == Types.Inventory)
                            Browser.Window.ExecuteJs("Inventory.updateAccessoriesSlot", new object[] { 8, AccessoriesData[8] });
                        else
                            AccessoriesSlotsToUpdate.Add(8);
                    }
                    else if (id == 7)
                    {
                        var data = ((string)curArgs[2]).Length == 0 ? null : ((string)curArgs[2]).Split('|');

                        if (data == null)
                        {
                            AccessoriesData[9] = null;
                            WeaponsData[2] = null;
                        }
                        else
                        {
                            var item = data[1].Length == 0 ? null : data[1].Split('&');

                            data = data[0].Split('&');

                            AccessoriesData[9] = FillAccessories(data[0]);

                            if (item != null)
                                WeaponsData[2] = FillWeapon(item[0], int.Parse(item[1]), int.Parse(item[2]) == 1, item[3], item[4]);
                            else
                                WeaponsData[2] = null;
                        }

                        if (CurrentType == Types.Inventory)
                        {
                            Browser.Window.ExecuteJs("Inventory.updateAccessoriesSlot", new object[] { 9, AccessoriesData[9] });
                            Browser.Window.ExecuteJs("Inventory.updateWeaponSlot", new object[] { 2, WeaponsData[2] });

                            Browser.Window.ExecuteJs("Inventory.switchThirdWeapon", data != null);
                        }
                        else
                        {
                            AccessoriesSlotsToUpdate.Add(9);
                            WeaponsSlotsToUpdate.Add(2);
                        }
                    }
                    else if (id == 8)
                    {
                        var data = ((string)curArgs[2]).Length == 0 ? null : ((string)curArgs[2]).Split('&');

                        ArmourData = data == null ? null : FillArmour(data[0], int.Parse(data[1]));

                        if (CurrentType == Types.Inventory)
                            Browser.Window.ExecuteJs("Inventory.fillVest", new object[] { ArmourData });
                        else
                            UpdateArmour = true;
                    }
                    else if (id == 9)
                    {
                        if (CurrentType != Types.Container)
                            return;

                        int slot = (int)curArgs[1];
                        var data = ((string)curArgs[2]).Length == 0 ? null : ((string)curArgs[2]).Split('&');

                        ContainerData[slot] = data == null ? null : FillItem(data[0], int.Parse(data[1]), float.Parse(data[2]), data[3], false, true, true, false);

                        Browser.Window.ExecuteJs("Inventory.updateCrateSlot", new object[] { slot, ContainerData[slot] });
                    }
                    else if (id == 10)
                    {
                        if (CurrentType != Types.Trade)
                            return;

                        int realSlot = (int)curArgs[1];
                        int slot = (int)curArgs[2];

                        var data = ((string)curArgs[3]).Length == 0 ? null : ((string)curArgs[3]).Split('&');

                        var item = data == null ? null : FillItem(data[0], int.Parse(data[1]), float.Parse(data[2]), data[3], false, false, false, true);

                        if (item == null)
                        {
                            if (GiveItemsData[slot] != null)
                                Browser.Window.ExecuteJs("Inventory.updatePocketsSlot", new object[] { (int)GiveItemsData[slot][0], "trade", ItemsData[(int)GiveItemsData[slot][0]]?[3] });

                            GiveItemsData[slot] = null;
                        }
                        else
                        {
                            if (GiveItemsData[slot] == null)
                                GiveItemsData[slot] = new object[4];
                            else
                            {
                                if (realSlot == -1)
                                    realSlot = (int)GiveItemsData[slot][0];
                                else
                                    Browser.Window.ExecuteJs("Inventory.updatePocketsSlot", new object[] { (int)GiveItemsData[slot][0], "trade", ItemsData[(int)GiveItemsData[slot][0]]?[3] });
                            }

                            GiveItemsData[slot][0] = realSlot;
                            GiveItemsData[slot][1] = item;

                            if (ItemsData[realSlot] != null)
                            {
                                var newRealItem = ((object[])ItemsData[realSlot][3]).ToArray();

                                newRealItem[3] = (int)newRealItem[3] - (int)item[3];

                                if ((int)newRealItem[3] <= 0)
                                    newRealItem = null;

                                Browser.Window.ExecuteJs("Inventory.updatePocketsSlot", new object[] { realSlot, "trade", newRealItem });
                            }
                        }

                        Browser.Window.ExecuteJs("Inventory.updateGiveSlot", new object[] { slot, GiveItemsData[slot]?[1] });

                    }
                    else if (id == 11)
                    {
                        if (CurrentType != Types.Trade)
                            return;

                        if ((bool)curArgs[1])
                        {
                            CurrentGiveMoney = (int)curArgs[2];

                            Browser.Window.ExecuteJs("Inventory.updateGiveMoney", CurrentGiveMoney);
                        }
                        else
                        {
                            var pType = (Sync.Players.PropertyTypes)(int)curArgs[3];
                            var propId = (uint)(int)curArgs[4];

                            string text = null;

                            if (pType == Sync.Players.PropertyTypes.Vehicle)
                            {
                                var vData = Data.Vehicles.GetById((string)curArgs[5]);

                                text = string.Format(Locale.Property.VehicleTradeInfoStr1, vData.Name, propId);
                            }
                            else if (pType == Sync.Players.PropertyTypes.House)
                            {
                                text = string.Format(Locale.Property.HouseTradeInfoStr, propId);
                            }
                            else if (pType == Sync.Players.PropertyTypes.Apartments)
                            {
                                var aps = Data.Locations.Apartments.All[propId];

                                text = string.Format(Locale.Property.ApartmentsTradeInfoStr, Data.Locations.ApartmentsRoot.All[aps.RootType].Name, aps.NumberInRoot + 1);
                            }
                            else if (pType == Sync.Players.PropertyTypes.Garage)
                            {
                                var garage = Data.Locations.Garage.All[propId];

                                text = string.Format(Locale.Property.GarageTradeInfoStr, Data.Locations.GarageRoot.All[garage.RootType].Name, garage.NumberInRoot + 1);
                            }
                            else if (pType == Sync.Players.PropertyTypes.Business)
                            {
                                var biz = Data.Locations.Business.All[(int)propId];

                                text = string.Format(Locale.Property.BusinessTradeInfoStr, biz.Name, biz.SubId);
                            }

                            if ((bool)curArgs[2])
                            {
                                if (!CurrentGiveProperties.Contains(text))
                                    CurrentGiveProperties.Add(text);
                            }
                            else
                            {
                                CurrentGiveProperties.Remove(text);
                            }

                            Browser.Window.ExecuteJs("Inventory.fillTradeRProperties", new object[] { "give", CurrentGiveProperties });
                        }

                    }
                    else if (id == 12)
                    {
                        if (CurrentType != Types.Trade)
                            return;

                        int slot = (int)curArgs[1];
                        var data = ((string)curArgs[2]).Length == 0 ? null : ((string)curArgs[2]).Split('&');

                        var item = data == null ? null : FillItem(data[0], int.Parse(data[1]), float.Parse(data[2]), data[3], false, false, false, true);

                        Browser.Window.ExecuteJs("Inventory.updateReceiveSlot", new object[] { slot, item });

                    }
                    else if (id == 13)
                    {
                        if (CurrentType != Types.Trade)
                            return;

                        if ((bool)curArgs[1])
                        {
                            Browser.Window.ExecuteJs("Inventory.updateReceiveMoney", (int)curArgs[2]);
                        }
                        else
                        {
                            var pType = (Sync.Players.PropertyTypes)(int)curArgs[3];
                            var propId = (uint)(int)curArgs[4];

                            string text = null;

                            if (pType == Sync.Players.PropertyTypes.Vehicle)
                            {
                                var vData = Data.Vehicles.GetById((string)curArgs[5]);

                                text = string.Format(Locale.Property.VehicleTradeInfoStr1, vData.Name, propId);
                            }
                            else if (pType == Sync.Players.PropertyTypes.House)
                            {
                                text = string.Format(Locale.Property.HouseTradeInfoStr, propId);
                            }
                            else if (pType == Sync.Players.PropertyTypes.Apartments)
                            {
                                var aps = Data.Locations.Apartments.All[propId];

                                text = string.Format(Locale.Property.ApartmentsTradeInfoStr, Data.Locations.ApartmentsRoot.All[aps.RootType].Name, aps.NumberInRoot + 1);
                            }
                            else if (pType == Sync.Players.PropertyTypes.Garage)
                            {
                                var garage = Data.Locations.Garage.All[propId];

                                text = string.Format(Locale.Property.GarageTradeInfoStr, Data.Locations.GarageRoot.All[garage.RootType].Name, garage.NumberInRoot + 1);
                            }
                            else if (pType == Sync.Players.PropertyTypes.Business)
                            {
                                var biz = Data.Locations.Business.All[(int)propId];

                                text = string.Format(Locale.Property.BusinessTradeInfoStr, biz.Name, biz.SubId);
                            }

                            if ((bool)curArgs[2])
                            {
                                if (!CurrentGetProperties.Contains(text))
                                    CurrentGetProperties.Add(text);
                            }
                            else
                            {
                                CurrentGetProperties.Remove(text);
                            }

                            Browser.Window.ExecuteJs("Inventory.fillTradeRProperties", new object[] { "receive", CurrentGetProperties });
                        }
                    }
                    else if (id == 14)
                    {
                        if (CurrentType != Types.Trade)
                            return;

                        if ((bool)curArgs[1])
                        {
                            Browser.Window.ExecuteJs("Inventory.fillCheckBox", "last", (bool)curArgs[2]);
                            Browser.Window.ExecuteJs("Inventory.switchTradeBtn", (bool)curArgs[2]);

                            if (!(bool)curArgs[2])
                            {
                                if ((bool)curArgs[3])
                                    CEF.Inventory.FreezeInterface(true, false);
                            }
                        }
                        else
                        {
                            if ((bool)curArgs[2])
                            {
                                if (!(bool)curArgs[3])
                                    CEF.Inventory.FreezeInterface(true, false);

                                CEF.Notification.Show("Trade::PlayerConfirmed");
                            }
                            else
                            {
                                if (!(bool)curArgs[3])
                                    CEF.Inventory.FreezeInterface(false, false);

                                CEF.Notification.Show("Trade::PlayerConfirmedCancel");
                            }
                        }
                    }
                    else if (id == 20)
                    {
                        if (CurrentType != Types.Workbench)
                            return;

                        int slot = (int)curArgs[1];
                        var data = ((string)curArgs[2]).Length == 0 ? null : ((string)curArgs[2]).Split('&');

                        if (data == null)
                        {
                            WorkbenchCraftData[slot] = null;
                            WorkbenchCraftParams[slot] = null;
                        }
                        else
                        {
                            var amount = int.Parse(data[1]);

                            WorkbenchCraftData[slot] = FillCraftItem(data[0], amount, float.Parse(data[2]), data[3]);
                            WorkbenchCraftParams[slot] = new Craft.ItemPrototype(data[0], amount);
                        }

                        for (int i = 0; i < WorkbenchToolsParams.Length; i++)
                        {
                            if (!WorkbenchCraftParams.Where(x => x != null && WorkbenchToolsParams[i] != null && x.Id == WorkbenchToolsParams[i].Id).Any())
                            {
                                if (WorkbenchToolsParams[i] != null)
                                {
                                    var newData = FillCraftToolsItem(WorkbenchToolsParams[i].Id, 1, 0f, null);

                                    if (WorkbenchToolsData[i] == null || WorkbenchToolsData[i][0] != newData[0] || WorkbenchToolsData[i][1] != newData[1])
                                    {
                                        WorkbenchToolsData[i] = newData;

                                        Browser.Window.ExecuteJs("Inventory.updateToolSlot", new object[] { i, newData });
                                    }
                                }
                            }
                            else
                            {
                                if (WorkbenchToolsData[i] != null)
                                {
                                    WorkbenchToolsData[i] = null;

                                    Browser.Window.ExecuteJs("Inventory.updateToolSlot", new object[] { i, null });
                                }
                            }
                        }

                        Browser.Window.ExecuteJs("Inventory.updateCraftSlot", new object[] { slot, WorkbenchCraftData[slot] });
                    }
                    else if (id == 22)
                    {
                        if (CurrentType != Types.Workbench)
                            return;

                        var data = ((string)curArgs[2]).Length == 0 ? null : ((string)curArgs[2]).Split('&');

                        if (data == null || data.Length > 1)
                        {
                            CancelPendingCraftTask();

                            ResetCraftButton();

                            WorkbenchResultData[0] = data == null ? null : FillCraftResultItem(data[0], int.Parse(data[1]), float.Parse(data[2]), data[3], true);

                            Browser.Window.ExecuteJs("Inventory.updateResultSlot", new object[] { WorkbenchResultData[0] });

                            if (data == null)
                                UpdateCraftItemVisualization();
                        }
                        else
                        {
                            var craftEndDate = DateTime.Parse(data[0]);

                            CancelPendingCraftTask();

                            StartPendingCraftTask(craftEndDate);
                        }
                    }
                }

                if (CurrentType == Types.Inventory)
                {
                    UpdateBinds();
                }
                else if (CurrentType == Types.Workbench)
                {
                    if (usedGroups.Contains(20))
                        UpdateCraftItemVisualization();
                }
            });
            #endregion

            Events.Add("Trade::UpdateLocal", async (object[] args) =>
            {
                if (CurrentType != Types.Trade)
                    return;

                var pData = Sync.Players.GetData(Player.LocalPlayer);

                if (pData == null)
                    return;

                int type = (int)args[0];

                // Money
                if (type == 0)
                {
                    if (CurrentGiveMoney < 0)
                        CurrentGiveMoney = 0;

                    var newAmount = (int)args[1];

                    if (newAmount < 0)
                        newAmount = 0;

                    if (CurrentGiveMoney == newAmount)
                        return;

                    if (!LastSent.IsSpam(250, false, false))
                    {
                        CurrentGiveMoney = newAmount;

                        Events.CallRemote("Trade::UpdateMoney", CurrentGiveMoney);

                        LastSent = DateTime.Now;
                    }
                    else
                        Browser.Window.ExecuteJs("Inventory.updateGiveMoney", CurrentGiveMoney);
                }
                // Property
                else if (type == 1)
                {
                    if (LastSent.IsSpam(500, false, false))
                        return;

                    int idx = (int)args[1];
                    bool state = (bool)args[2];

                    var propIds = Player.LocalPlayer.GetData<List<(Sync.Players.PropertyTypes, uint)>>("Trade::Temp::PropIds");

                    if (propIds == null)
                        return;

                    if (idx >= propIds.Count)
                        return;

                    var prop = propIds[idx];

                    if ((bool)await Events.CallRemoteProc("Trade::UpdateProperty", prop.Item1, prop.Item2, state))
                    {
                        if (CurrentType != Types.Trade)
                            return;

                        CEF.Browser.Window.ExecuteJs("Inventory.fillCheckBox", idx, state);
                    }
                }
                // Ready
                else if (type == 2)
                {
                    bool state = (bool)args[1];

                    if (!LastSent.IsSpam(1000, false, false))
                    {
                        Events.CallRemote("Trade::Confirm", state);

                        LastSent = DateTime.Now;
                    }
                }
                // Accept
                else if (type == 3)
                {
                    if (!LastSent.IsSpam(1000, false, false))
                    {
                        Events.CallRemote("Trade::Accept");

                        LastSent = DateTime.Now;
                    }
                }
                else
                    return;
            });

            #region Replace
            Events.Add("Inventory::Replace", (object[] args) =>
            {
                var toStr = (string)args[0];
                var toSlot = ((int)args[1]);

                var fromStr = (string)args[2];
                var fromSlot = (int)args[3];

                var amount = ((int)args[4]);

                Replace(toStr, toSlot, fromStr, fromSlot, amount);
            });
            #endregion

            #region Action
            // 0 - action, 1 - slotStrFrom, 2 - slotFrom, 3 slotStrTo, 4 - slotTo, 5 - amount
            // actions: -1 - cancelAction, 0 - bind, 1 - split, 2 - throw, 3 - takeByGround, 4 - fastUse, 5 - use2

            Events.Add("Inventory::Action", (object[] args) =>
            {
                Action(args);
            });
            #endregion

            #region Bind
            Events.Add("Inventory::Bind", (object[] args) =>
            {
                if (!IsActive)
                    return;

                RAGE.Ui.VirtualKeys keyCode = (RAGE.Ui.VirtualKeys)(int)args[0];

                string slotStr = (string)args[1];
                int slot = (int)args[2];

                KeyBinds.Types type = (KeyBinds.Types)Enum.Parse(typeof(KeyBinds.Types), slotStr + slot.ToString());

                if (keyCode == RAGE.Ui.VirtualKeys.Escape)
                    return;

                if (keyCode == RAGE.Ui.VirtualKeys.Back)
                {
                    KeyBinds.Binds[type].ChangeKeys(new RAGE.Ui.VirtualKeys[] { });

                    return;
                }

                KeyBinds.Binds[type].ChangeKeys(new RAGE.Ui.VirtualKeys[] { keyCode });
                CEF.Notification.Show(Notification.Types.Success, Locale.Notifications.Bind.Header, string.Format(Locale.Notifications.Bind.Binded, KeyBinds.Binds[type].GetKeyString()));
            });
            #endregion

            Events.Add("Inventory::Close", (object[] args) => Close(false));
            #endregion
        }

        #region Show
        public static void Show(Types type, params object[] args)
        {
            if (IsActive)
                return;

            if (LastShowed.IsSpam(1000, false, false) || Utils.IsAnyCefActive())
                return;

            if (!Utils.CanDoSomething(true, Utils.Actions.Shooting, Utils.Actions.Reloading) || Sync.WeaponSystem.LastWeaponShot.IsSpam(250, false, false) || Sync.WeaponSystem.LastArmourLoss.IsSpam(250, false, false))
                return;

            CurrentEntity = BCRPClient.Interaction.CurrentEntity;

            LastShowed = DateTime.Now;

            if (type == Types.Inventory)
            {
                Events.CallLocal("Inventory::Show", (int)Types.Inventory);
            }
            else if (type == Types.Container)
            {
                Events.CallRemote("Container::Show", (uint)args[0]);
            }
            else if (type == Types.Workbench)
            {
                Events.CallRemote("Workbench::Show", (int)args[0], (uint)args[1]);
            }
        }
        #endregion

        #region Set Window Ready
        private static void SetWindowReady()
        {
            if (IsActive)
                return;

            GameEvents.Update -= OnTickCheck;
            GameEvents.Update += OnTickCheck;

            Sync.World.EnabledItemsOnGround = false;
            BCRPClient.Interaction.EnabledVisual = false;

            if (CurrentType == Types.Inventory)
            {
                Browser.Switch(Browser.IntTypes.Inventory, true);

                UpdateStates();
            }
            else if (CurrentType == Types.Container)
            {
                Browser.Switch(Browser.IntTypes.CratesInventory, true);
            }
            else if (CurrentType == Types.Workbench)
            {
                Browser.Switch(Browser.IntTypes.Workbench, true);
            }
            else if (CurrentType == Types.Trade)
            {
                Browser.Switch(Browser.IntTypes.Trade, true);
            }

            TempBinds.Add(RAGE.Input.Bind(RAGE.Ui.VirtualKeys.Control, true, () => Browser.Window.ExecuteCachedJs("Inventory.switchCtrl", true)));
            TempBinds.Add(RAGE.Input.Bind(RAGE.Ui.VirtualKeys.Control, false, () => Browser.Window.ExecuteCachedJs("Inventory.switchCtrl", false)));

            TempBindEsc = RAGE.Input.Bind(RAGE.Ui.VirtualKeys.Escape, true, () =>
            {
                if (CEF.ActionBox.CurrentContext == ActionBox.Contexts.Inventory)
                {
                    CEF.ActionBox.Close(false);

                    CurrentAction = null;
                    CurrentSlotFrom = null;
                    CurrentSlotTo = null;
                }
                else
                    Close(true);
            });

            GameEvents.DisableAllControls(true);

            if (!Settings.Interface.HideHUD)
                CEF.HUD.ShowHUD(false);

            if (!Settings.Interface.HideNames)
                BCRPClient.NameTags.Enabled = false;

            Chat.Show(false);

            RAGE.Game.Graphics.TransitionToBlurred(250);

            Cursor.Show(true, true);
        }
        #endregion

        #region Close
        public static void Close(bool callRemote = true)
        {
            if (!IsActive)
                return;

            Browser.Window.ExecuteCachedJs("Inventory.switchCtrl", false);

            ContainerData = null;

            GiveItemsData = null;

            WorkbenchCraftData = null;
            WorkbenchResultData = null;
            WorkbenchToolsData = null;
            WorkbenchCraftParams = null;

            if (ActionBox.CurrentContext == ActionBox.Contexts.Inventory)
                ActionBox.Close(true);

            if (CurrentType == Types.Workbench)
            {
                CancelPendingCraftTask();
            }

            CEF.Inventory.FreezeInterface(false, true);

            CurrentContainerType = ContainerTypes.None;

            if (callRemote && CurrentType == Types.Container)
                Events.CallRemote("Container::Close");
            else if (callRemote && CurrentType == Types.Workbench)
                Events.CallRemote("Workbench::Close");
            else if (callRemote && CurrentType == Types.Trade)
                Sync.Offers.Reply(Sync.Offers.ReplyTypes.AutoCancel);

            Browser.Switch(Browser.IntTypes.Inventory, false);
            Browser.Switch(Browser.IntTypes.CratesInventory, false);
            Browser.Switch(Browser.IntTypes.Workbench, false);
            Browser.Switch(Browser.IntTypes.Trade, false);

            Player.LocalPlayer.ResetData("Trade::Temp::PropIds");

            CurrentEntity = null;

            Browser.Window.ExecuteJs("Inventory.defaultSlot();");

            foreach (var bind in TempBinds.ToList())
                RAGE.Input.Unbind(bind);

            TempBinds.Clear();

            if (TempBindEsc != -1)
                RAGE.Input.Unbind(TempBindEsc);

            TempBindEsc = -1;

            GameEvents.Update -= OnTickCheck;

            Sync.World.EnabledItemsOnGround = true;
            BCRPClient.Interaction.EnabledVisual = !Settings.Interface.HideInteractionBtn;

            GameEvents.DisableAllControls(false);

            RAGE.Game.Graphics.TransitionFromBlurred(250);

            AsyncTask.RunSlim(() => RAGE.Game.Graphics.TransitionFromBlurred(0), 300);

            if (!Settings.Interface.HideHUD)
                CEF.HUD.ShowHUD(true);

            if (!Settings.Interface.HideNames)
                BCRPClient.NameTags.Enabled = true;

            Chat.Show(true);

            Cursor.Show(false, false);

            CurrentType = Types.None;

            CurrentSlotFrom = null;
            CurrentSlotTo = null;
            CurrentAction = null;

            LastShowed = DateTime.Now;
        }
        #endregion

        #region Stuff

        public static void SwitchHint(bool state) => Browser.Window?.ExecuteJs("Inventory.switchHelp", state);

        public static void BindedAction(int action, string slotStr, int slot, params string[] args)
        {
            if (action < 5)
                return;

            if (Utils.IsAnyCefActive() || Sync.WeaponSystem.LastWeaponShot.IsSpam(250, false, false) || !Utils.CanDoSomething(true, Utils.Actions.Reloading, Utils.Actions.Shooting))
                return;

            Action(action, slotStr, slot, args);
        }

        public static void Action(params object[] args)
        {
            int id = (int)args[0];

            //Utils.ConsoleOutput(id);

            if (id == -1)
            {
                CurrentSlotFrom = null;
                CurrentSlotTo = null;

                CurrentAction = null;

                ActionBox.Close(false);

                return;
            }

            if (CurrentAction != null)
            {
                ActionBox.Close(false);

                if (CurrentAction == 1) // split
                {
                    Replace(CurrentSlotTo.Value.Item1, CurrentSlotTo.Value.Item2, CurrentSlotFrom.Value.Item1, CurrentSlotFrom.Value.Item2, id); // id = amount

                    CurrentAction = null;
                    CurrentSlotFrom = null;
                    CurrentSlotTo = null;

                    return;
                }
                else if (CurrentAction == 2) // drop
                {
                    var slotStrToThrow = CurrentSlotFrom.Value.Item1;
                    var slotToThrow = CurrentSlotFrom.Value.Item2;

                    GetRealSlot(ref slotStrToThrow, ref slotToThrow);

                    CurrentAction = null;
                    CurrentSlotFrom = null;
                    CurrentSlotTo = null;

                    if (slotStrToThrow == null)
                        return;

                    if (id > 0)
                    {
                        if (LastSent.IsSpam(500, false, false))
                            return;

                        if (Player.LocalPlayer.Vehicle != null)
                        {
                            Notification.Show(Notification.Types.Error, Locale.Notifications.Inventory.Header, Locale.Notifications.Inventory.ActionRestricted);

                            return;
                        }

                        if (CurrentType == Types.Inventory)
                        {
                            Events.CallRemote("Inventory::Drop", Groups[slotStrToThrow], slotToThrow, id); // id = amount
                        }
                        else if (CurrentType == Types.Container)
                        {
                            if (slotStrToThrow == "crate")
                                Events.CallRemote("Container::Drop", slotToThrow, id); // id = amount
                            else
                                Events.CallRemote("Inventory::Drop", Groups[slotStrToThrow], slotToThrow, id); // id = amount
                        }
                        else if (CurrentType == Types.Workbench)
                        {
                            if (slotStrToThrow == "pockets")
                                Events.CallRemote("Inventory::Drop", Groups[slotStrToThrow], slotToThrow, id); // id = amount
                            else
                                Events.CallRemote("Workbench::Drop", Groups[slotStrToThrow], slotToThrow, id); // id = amount
                        }

                        LastSent = DateTime.Now;
                    }

                    return;
                }
            }

            string slotStr = (string)args[1];
            int slot = (int)args[2];

            if (id == 1 || id == 2)
            {
                if (CurrentAction == null)
                    CurrentAction = id;

                if (CurrentSlotFrom == null)
                    CurrentSlotFrom = (slotStr, slot);

                if (CurrentSlotTo == null && args.Length > 4)
                    CurrentSlotTo = ((string)args[3], (int)args[4]);
            }

            if (id < 4 && !IsActive)
            {
                CurrentAction = null;
                CurrentSlotFrom = null;
                CurrentSlotTo = null;

                return;
            }

            // Bind
            if (id == 0)
            {
                if (CurrentType != Types.Inventory || slotStr != "pockets" && slotStr != "weapon" || (slotStr == "weapon" && slot == 3))
                    return;

                Browser.Window.ExecuteJs("Inventory.bindSlot", $"{slot}-inv-{slotStr}");

                CEF.Notification.ShowHint(Locale.Notifications.Bind.Hint, true);

                return;
            }

            string name = null;
            int amount = 0;

            if (id < 4)
            {
                if (slotStr == "pockets")
                {
                    name = (string)((object[])ItemsData[slot]?[0])[1];
                    amount = (int)(((object[])ItemsData[slot]?[0])[3] ?? 0);

                    if (CurrentType == Types.Trade)
                    {
                        var actualItem = GiveItemsData.Where(x => x != null && (int)x[0] == slot).FirstOrDefault();

                        if (actualItem != null)
                        {
                            amount -= (int)(((object[])actualItem[1])[3] ?? 0);
                        }
                    }
                }
                else if (slotStr == "bag")
                {
                    name = (string)(((object[])BagData[slot]?[0])[1]);
                    amount = (int)(((object[])BagData[slot]?[0])[3] ?? 0);
                }
                else if (slotStr == "weapon")
                {
                    if (slot == 3)
                    {
                        name = (string)ArmourData?[1];
                        amount = 1;
                    }
                    else
                    {
                        name = (string)WeaponsData[slot]?[1];
                        amount = id == 2 ? 1 : (int)(WeaponsData[slot]?[3] ?? 0);
                    }
                }
                else if (slotStr == "clothes")
                {
                    name = (string)ClothesData[slot]?[1];
                    amount = 1;
                }
                else if (slotStr == "accessories")
                {
                    name = (string)AccessoriesData[slot]?[1];
                    amount = 1;
                }
                else if (slotStr == "crate")
                {
                    name = (string)ContainerData[slot]?[1];
                    amount = (int)(ContainerData[slot]?[3] ?? 0);
                }
                else if (slotStr == "craft")
                {
                    name = (string)WorkbenchCraftData[slot]?[1];
                    amount = (int)(WorkbenchCraftParams[slot]?.Amount ?? 0);
                }
                else if (slotStr == "result")
                {
                    name = (string)WorkbenchResultData[slot]?[1];
                    amount = (int)(WorkbenchResultData[slot]?[3] ?? 0);
                }
                else if (slotStr == "give")
                {
                    name = (string)((object[])GiveItemsData[slot]?[1])[1];
                    amount = (int)(((object[])GiveItemsData[slot]?[1])[3] ?? 0);
                }

                if (name == null || amount <= 0)
                {
                    CurrentAction = null;
                    CurrentSlotFrom = null;
                    CurrentSlotTo = null;
                }
            }

            if (id == 1) // Split
            {
                if (CurrentSlotTo == null)
                {
                    if (CurrentSlotFrom.Value.Item1 == "weapon" || CurrentSlotFrom.Value.Item1 == "pockets")
                    {
                        int slotTo = Array.IndexOf(ItemsData, null);

                        if (slotTo == -1)
                        {
                            CurrentAction = null;
                            CurrentSlotFrom = null;
                            CurrentSlotTo = null;

                            return;
                        }
                        else
                            CurrentSlotTo = ("pockets", slotTo);
                    }
                    else if (CurrentSlotFrom.Value.Item1 == "bag")
                    {
                        int slotTo = Array.IndexOf(BagData, null);

                        if (slotTo == -1)
                        {
                            CurrentAction = null;
                            CurrentSlotFrom = null;
                            CurrentSlotTo = null;

                            return;
                        }
                        else
                            CurrentSlotTo = ("bag", slotTo);
                    }
                    else if (CurrentSlotFrom.Value.Item1 == "crate")
                    {
                        int slotTo = Array.IndexOf(ContainerData, null);

                        if (slotTo == -1)
                        {
                            CurrentAction = null;
                            CurrentSlotFrom = null;
                            CurrentSlotTo = null;

                            return;
                        }
                        else
                            CurrentSlotTo = ("crate", slotTo);
                    }
                }
                else
                {
                    if (CurrentType == Types.Workbench)
                    {
                        if (CurrentSlotTo.Value.Item1 == "result" || (CurrentSlotTo.Value.Item1 == "tool" && (slotStr == "pockets" || slotStr == "result" || (slotStr == "craft" && !(Data.Items.GetData(WorkbenchCraftParams[slot].Id, null) is Data.Items.WorkbenchTool.ItemData)))))
                        {
                            CurrentAction = null;
                            CurrentSlotFrom = null;
                            CurrentSlotTo = null;

                            return;
                        }
                    }
                }

                if ((slotStr != "weapon" && amount == 1) || (slotStr == "weapon" && amount == 0))
                {
                    if ((slotStr != "weapon" && amount == 1))
                        Replace(CurrentSlotTo.Value.Item1, CurrentSlotTo.Value.Item2, CurrentSlotFrom.Value.Item1, CurrentSlotFrom.Value.Item2, -1);

                    CurrentAction = null;
                    CurrentSlotFrom = null;
                    CurrentSlotTo = null;

                    return;
                }

                ActionBox.ShowRange(ActionBox.Contexts.Inventory, string.Format(slotStr == "weapon" ? Locale.Actions.GetAmmo : Locale.Actions.Split, name), 1, amount, amount / 2, -1, ActionBox.RangeSubTypes.Default);

                CurrentAction = 1;

                return;
            }

            if (id == 2) // throw
            {
                if (CurrentType == Types.Workbench)
                {
                    if (slotStr == "tool")
                        return;
                }

                ActionBox.ShowRange(ActionBox.Contexts.Inventory, string.Format(Locale.Actions.Drop, name), 1, amount, amount, -1, ActionBox.RangeSubTypes.Default);

                CurrentAction = 2;

                return;
            }

            if (id > 3) // action
            {
                if (id == 4) // replace
                {
                    if (slotStr == "clothes" || slotStr == "accessories" || slotStr == "weapon" || slotStr == "bag" || slotStr == "crate" || slotStr == "result")
                    {
                        var idx = GetFreeIdx(slotStr == "bag" ? (string)((object[])BagData[slot][0])[0] : slotStr == "crate" ? (string)ContainerData[slot][0] : slotStr == "result" ? (string)WorkbenchResultData[slot][0] : "", ItemsData);

                        if (idx == -1)
                            return;

                        Replace("pockets", idx, slotStr, slot, -1);

                        return;
                    }
                    else if (slotStr == "pockets")
                    {
                        if (CurrentType == Types.Inventory)
                        {
                            if (BagData != null)
                            {
                                var idx = GetFreeIdx(ItemsParams[slot].Id, BagData);

                                if (idx == -1)
                                    return;

                                Replace("bag", idx, slotStr, slot, -1);
                            }
                        }
                        else if (CurrentType == Types.Container)
                        {
                            if (ContainerData != null)
                            {
                                var idx = GetFreeIdx(ItemsParams[slot].Id, ContainerData);

                                if (idx == -1)
                                    return;

                                Replace("crate", idx, slotStr, slot, -1);
                            }
                        }
                        else if (CurrentType == Types.Workbench)
                        {
                            if (WorkbenchCraftData != null)
                            {
                                var idx = GetFreeIdx(ItemsParams[slot].Id, WorkbenchCraftData);

                                if (idx == -1)
                                    return;

                                Replace("craft", idx, slotStr, slot, -1);
                            }
                        }
                        else if (CurrentType == Types.Trade)
                        {
                            if (GiveItemsData != null)
                            {
                                int idx = Array.IndexOf(GiveItemsData, null);

                                if (idx == -1)
                                    return;

                                Replace("give", idx, slotStr, slot, -1);
                            }
                        }

                        return;
                    }
                    else if (slotStr == "give")
                    {
                        if (CurrentType == Types.Trade)
                        {
                            Replace("pockets", 0, slotStr, slot, -1);
                        }
                    }
                    else if (slotStr == "tool")
                    {
                        if (CurrentType == Types.Workbench)
                        {
                            if (WorkbenchCraftData != null)
                            {
                                int idx = Array.IndexOf(WorkbenchCraftData, null);

                                if (idx == -1)
                                    return;

                                Replace("craft", idx, slotStr, slot, -1);
                            }
                        }
                    }
                    else if (slotStr == "craft")
                    {
                        if (CurrentType == Types.Workbench)
                        {
                            if (WorkbenchCraftParams != null && WorkbenchCraftParams[slot] != null)
                            {
                                if (Data.Items.GetData(WorkbenchCraftParams[slot].Id, null) is Data.Items.WorkbenchTool.ItemData)
                                {
                                    if (WorkbenchToolsParams != null)
                                    {
                                        int idx = WorkbenchToolsParams.ToList().FindIndex(x => x?.Id == WorkbenchCraftParams[slot].Id);

                                        if (idx == -1)
                                            return;

                                        Replace("tool", idx, slotStr, slot, -1);
                                    }
                                }
                                else
                                {
                                    if (ItemsData != null)
                                    {
                                        var idx = GetFreeIdx(WorkbenchCraftParams[slot].Id, ItemsData);

                                        if (idx == -1)
                                            return;

                                        Replace("pockets", idx, slotStr, slot, -1);
                                    }
                                }
                            }
                        }
                    }

                    return;
                }

                GetRealSlot(ref slotStr, ref slot);

                if (slotStr == null)
                    return;

                var eData = new List<string>();

                if (slotStr == "pockets")
                {
                    var iParams = ItemsParams[slot];

                    if (iParams == null)
                        return;

                    var type = Data.Items.GetType(iParams.Id, false);

                    if (type == null)
                        return;

                    bool isPreActionNeeded = true;

                    if (args.Length > 3 && args[3] is string[] strArgs && strArgs.Length > 0)
                    {
                        isPreActionNeeded = false;

                        eData.AddRange(strArgs);
                    }

                    if (!iParams.InUse)
                    {
                        var vAction = Data.Items.GetActionToValidate(type);

                        if (vAction != null)
                        {
                            if (vAction.Invoke() is List<string> eDataI)
                            {
                                eData.AddRange(eDataI);
                            }
                            else
                            {
                                return;
                            }
                        }

                        if (isPreActionNeeded)
                        {
                            var preAction = Data.Items.GetActionToPreAction(type);

                            if (preAction != null)
                            {
                                preAction.Invoke(slot, iParams.Id);

                                return;
                            }
                        }
                    }
                }

                if (LastSent.IsSpam(1000, false, false))
                    return;

                Events.CallRemote("Inventory::Action", Groups[slotStr], slot, id, string.Join('&', eData));

                LastSent = DateTime.Now;
            }
        }

        public static void Replace(string toStr, int toSlot, string fromStr, int fromSlot, int amount)
        {
            if (LastSent.IsSpam(500, false, false))
                return;

            GetRealSlot(ref toStr, ref toSlot);

            if (toStr == null)
                return;

            GetRealSlot(ref fromStr, ref fromSlot);

            if (fromStr == null)
                return;

            if (CurrentType == Types.Inventory || CurrentType == Types.Container)
            {
                if (fromStr == "bagItem" && toStr != "pockets")
                    return;
                else if (fromStr == "clothes" || fromStr == "accessories" || fromStr == "armour" || fromStr == "holsterItem")
                {
                    if (toStr != "pockets" && toStr != "bag")
                        return;
                }
                else if (fromStr == "weapon" || fromStr == "holster")
                {
                    if (toStr == "clothes" || toStr == "accessories")
                        return;
                }

                if (toStr == "bagItem" && fromStr != "pockets")
                    return;
                else if (toStr == "clothes" || toStr == "accessories" || toStr == "armour" || toStr == "holsterItem")
                {
                    if (fromStr != "pockets" && fromStr != "bag")
                        return;
                }
                else if (toStr == "weapon" || toStr == "holster")
                {
                    if (fromStr == "clothes" || fromStr == "accessories")
                        return;
                }

                if (toStr == "crate" || fromStr == "crate")
                {
                    if (toStr == "crate")
                    {
                        if (fromStr != "bag" && fromStr != "pockets" && fromStr != "crate")
                            return;
                    }
                    else if (fromStr == "crate")
                    {
                        if (toStr != "bag" && toStr != "pockets" && toStr != "crate")
                            return;
                    }

                    Events.CallRemote("Container::Replace", Groups[toStr], toSlot, Groups[fromStr], fromSlot, amount);
                }
                else
                    Events.CallRemote("Inventory::Replace", Groups[toStr], toSlot, Groups[fromStr], fromSlot, amount);
            }
            else if (CurrentType == Types.Workbench)
            {
                if (toStr == "result" || (toStr == "tool" && (fromStr == "pockets" || fromStr == "result" || (fromStr == "craft" && !(Data.Items.GetData(WorkbenchCraftParams[fromSlot].Id, null) is Data.Items.WorkbenchTool.ItemData)))))
                    return;

                if (toStr == "pockets" && fromStr == "pockets")
                    Events.CallRemote("Inventory::Replace", Groups[toStr], toSlot, Groups[fromStr], fromSlot, amount);
                else
                    Events.CallRemote("Workbench::Replace", Groups[toStr], toSlot, Groups[fromStr], fromSlot, amount);
            }
            else if (CurrentType == Types.Trade)
            {
                if (fromStr == toStr)
                    return;

                Events.CallRemote("Trade::UpdateItem", Groups[fromStr] == 0, toSlot, fromSlot, amount);
            }

            //Utils.ConsoleOutput($"{Groups[toStr]}, {toSlot}, {Groups[fromStr]}, {fromSlot}, {amount}");

            LastSent = DateTime.Now;
        }

        public static void UpdateBinds()
        {
            if (!IsActive || CurrentType != Types.Inventory)
                return;

            Browser.Window.ExecuteJs("Inventory.fillBind", KeyBinds.Binds.Where(x => x.Value.InvOnly).Select(x => x.Value.Keys.Length == 0 ? null : (int?)x.Value.Keys[0]));
        }

        public static void UpdateStates()
        {
            if (CurrentType != Types.Inventory)
                return;

            var data = Sync.Players.GetData(Player.LocalPlayer);

            if (data == null)
                return;

            // Is needed because of bug ???
            Player.LocalPlayer.GetHealth();

            var statuses = new object[3][] { new object[] { 0, Player.LocalPlayer.GetRealHealth() }, new object[] { 1, data.Satiety }, new object[] { 2, data.Mood } };

            Browser.Window.ExecuteJs("Inventory.fillAllStatus", new object[] { statuses });

            var curArm = Player.LocalPlayer.GetArmour();

            if (ArmourData != null && (int)ArmourData[3] != curArm)
            {
                ArmourData[3] = curArm;

                Browser.Window.ExecuteJs("Inventory.fillVest", new object[] { ArmourData });
            }
        }

        private static void OnTickCheck()
        {
            if (CurrentType == Types.Container)
            {
                if (CurrentContainerType == ContainerTypes.Trunk)
                {
                    if (Player.LocalPlayer.Vehicle != null || CurrentEntity?.IsNull != false || !CurrentEntity.IsEntityNear(Settings.ENTITY_INTERACTION_MAX_DISTANCE))
                        Close();
                }
            }
        }

        private static void GetRealSlot(ref string slotStr, ref int slot)
        {
            if (slotStr == "weapon")
            {
                if (slot == 3)
                {
                    slotStr = "armour";

                    return;
                }
                else if (slot == 2)
                    slotStr = "holster";

                return;
            }
            else if (slotStr == "accessories")
            {
                if (slot == 8)
                {
                    slotStr = "bagItem";

                    return;
                }
                else if (slot == 9)
                {
                    slotStr = "holsterItem";

                    return;
                }
            }

            return;
        }

        public static void Load(Newtonsoft.Json.Linq.JArray args)
        {
            var Weapons = RAGE.Util.Json.Deserialize<string[]>((string)args[0]).Select(x => x.Length == 0 ? null : x.Split('&')).ToArray();

            var Armour = ((string)args[1]).Length == 0 ? null : ((string)args[1]).Split('&');

            var Items = RAGE.Util.Json.Deserialize<string[]>((string)args[2]).Select(x => x.Length == 0 ? null : x.Split('&')).ToArray();

            var Clothes = RAGE.Util.Json.Deserialize<string[]>((string)args[3]).Select(x => x.Length == 0 ? null : x.Split('&')).ToArray();

            var Accessories = RAGE.Util.Json.Deserialize<string[]>((string)args[4]).Select(x => x.Length == 0 ? null : x.Split('&')).ToArray();

            var Bag = ((string)args[5]).Length == 0 ? null : ((string)args[5]).Split('|');

            var Holster = ((string)args[6]).Length == 0 ? null : ((string)args[6]).Split('|');

            WeaponsData = new object[Weapons.Length + 1][];

            for (int i = 0; i < WeaponsData.Length - 1; i++)
                if (Weapons[i] != null)
                    WeaponsData[i] = FillWeapon(Weapons[i][0], int.Parse(Weapons[i][1]), int.Parse(Weapons[i][2]) == 1, Weapons[i][3], Weapons[i][4]);

            if (Armour != null)
                ArmourData = FillArmour(Armour[0], int.Parse(Armour[1]));

            ItemsData = new object[Items.Length][];
            ItemsParams = new ItemParams[Items.Length];

            for (int i = 0; i < ItemsData.Length; i++)
            {
                if (Items[i] != null)
                {
                    var inUse = int.Parse(Items[i][4]) == 1;

                    ItemsData[i] = FillItem(Items[i][0], int.Parse(Items[i][1]), float.Parse(Items[i][2]), Items[i][3], inUse, false, false, false);

                    ItemsParams[i] = new ItemParams(Items[i][0]) { InUse = inUse };
                }
            }

            ClothesData = new object[Clothes.Length][];

            for (var i = 0; i < ClothesData.Length; i++)
                if (Clothes[i] != null)
                    ClothesData[i] = FillClothes(Clothes[i][0]);

            AccessoriesData = new object[Accessories.Length + 2][];

            for (var i = 0; i < AccessoriesData.Length - 2; i++)
                if (Accessories[i] != null)
                    AccessoriesData[i] = FillAccessories(Accessories[i][0]);

            if (Bag != null)
            {
                var items = Bag.Skip(1).Select(x => x.Length == 0 ? null : x.Split('&')).ToArray();

                Bag = Bag[0].Split('&');

                AccessoriesData[AccessoriesData.Length - 2] = FillAccessories(Bag[0]);

                BagData = new object[items.Length][];

                for (int i = 0; i < BagData.Length; i++)
                    if (items[i] != null)
                        BagData[i] = FillItem(items[i][0], int.Parse(items[i][1]), float.Parse(items[i][2]), items[i][3], false, true, false, false);

                BagWeight = float.Parse(Bag[1]);
            }

            if (Holster != null)
            {
                var item = Holster[1].Length == 0 ? null : Holster[1].Split('&');

                Holster = Holster[0].Split('&');

                AccessoriesData[AccessoriesData.Length - 1] = FillAccessories(Holster[0]);

                if (item != null)
                    WeaponsData[WeaponsData.Length - 1] = FillWeapon(item[0], int.Parse(item[1]), int.Parse(item[2]) == 1, item[3], item[4]);
            }
        }

        private static void UpdateCraftItemVisualization()
        {
            if (CurrentType != Types.Workbench)
                return;

            if (WorkbenchCraftParams == null || (WorkbenchResultData != null && WorkbenchResultData[0] != null && WorkbenchResultData[0][2] != null))
                return;

            var notNullItems = WorkbenchCraftParams.Where(x => x != null).OrderBy(x => x.Id).ToList();

            var receipt = Data.Craft.Receipt.GetByIngredients(notNullItems);

            if (receipt != null)
            {
                var itemData = Data.Items.GetData(receipt.CraftResultData.ResultItem.Id, null);

                if (itemData == null)
                    return;

                int realAmount = 1;

                if (itemData is Data.Items.Item.ItemData.IStackable itemDataStackable)
                {
                    realAmount = receipt.GetExpectedAmountByIngredients(notNullItems);

                    if (realAmount <= 0)
                        return;

                    var newData = FillCraftResultItem(receipt.CraftResultData.ResultItem.Id, realAmount, itemData.Weight, null, false);

                    if (WorkbenchResultData[0] == null || (string)WorkbenchResultData[0][0] != (string)newData[0] || (string)WorkbenchResultData[0][1] != (string)newData[1] || (int)WorkbenchResultData[0][3] != (int)newData[3] || (float)WorkbenchResultData[0][4] != (float)newData[4])
                    {
                        WorkbenchResultData[0] = newData;

                        CEF.Browser.Window.ExecuteJs("Inventory.updateResultSlot", new object[] { newData });
                    }
                }
                else
                {
                    var newData = FillCraftResultItem(receipt.CraftResultData.ResultItem.Id, receipt.CraftResultData.ResultItem.Amount, itemData.Weight, null, false);

                    if (WorkbenchResultData[0] == null || (string)WorkbenchResultData[0][0] != (string)newData[0] || (string)WorkbenchResultData[0][1] != (string)newData[1] || (int)WorkbenchResultData[0][3] != (int)newData[3] || (float)WorkbenchResultData[0][4] != (float)newData[4])
                    {
                        WorkbenchResultData[0] = newData;

                        CEF.Browser.Window.ExecuteJs("Inventory.updateResultSlot", new object[] { newData });
                    }
                }

                if (!Player.LocalPlayer.HasData("Inv::Temp::WBCPT"))
                    CEF.Browser.Window.ExecuteJs("Inventory.fillCraftBtn", new object[] { new object[] { Locale.General.Containers.WorkbenchNames[CurrentWorkbenchType].CraftBtnText, (new TimeSpan(0, 0, 0, 0, receipt.CraftResultData.CraftTime * (realAmount / receipt.CraftResultData.ResultItem.Amount))).GetBeautyString() } });
            }
            else
            {
                if (WorkbenchResultData[0] != null)
                {
                    WorkbenchResultData[0] = null;

                    CEF.Browser.Window.ExecuteJs("Inventory.updateResultSlot", new object[] { null });
                }

                ResetCraftButton();
            }
        }

        private static void ResetCraftButton()
        {
            if (CurrentType != Types.Workbench)
                return;

            if (!Player.LocalPlayer.HasData("Inv::Temp::WBCPT"))
                CEF.Browser.Window.ExecuteJs("Inventory.fillCraftBtn", new object[] { new object[] { Locale.General.Containers.WorkbenchNames[CurrentWorkbenchType].CraftBtnText } });
        }

        private static void CancelPendingCraftTask()
        {
            CEF.Inventory.FreezeInterface(false, false);

            var task = Player.LocalPlayer.GetData<AsyncTask>("Inv::Temp::WBCPT");

            if (task != null)
            {
                task.Cancel();

                Player.LocalPlayer.ResetData("Inv::Temp::WBCPT");
            }
        }

        private static void StartPendingCraftTask(DateTime endDate)
        {
            var timeOffset = new TimeSpan(0, 0, 0, 1, 0);

            CEF.Inventory.FreezeInterface(true, false);

            var task = new AsyncTask(() =>
            {
                if (CurrentType != Types.Workbench)
                    return true;

                var currentDate = Utils.GetServerTime();

                var timeLeft = endDate.Subtract(currentDate).Add(timeOffset);

                CEF.Browser.Window.ExecuteJs("Inventory.fillCraftBtn", timeLeft.GetBeautyString());

                if (timeLeft.TotalMilliseconds <= 0)
                    return true;

                return false;
            }, 500, true, 0);

            task.Run();

            Player.LocalPlayer.SetData("Inv::Temp::WBCPT", task);
        }

        public static void FreezeInterface(bool state, bool clearCounter = false)
        {
            if (clearCounter)
                FreezeCounter = 0;

            if (!IsActive)
                return;

            if (state)
            {
                FreezeCounter++;
            }
            else
            {
                if (FreezeCounter > 0)
                {
                    FreezeCounter--;

                    if (FreezeCounter > 0)
                        return;
                }
            }

            if (CurrentType == Types.Inventory)
            {
                if (state)
                    CEF.Browser.Window.ExecuteCachedJs("document.querySelector('.Inventory').style.pointerEvents = 'none';");
                else
                    CEF.Browser.Window.ExecuteCachedJs("document.querySelector('.Inventory').style.pointerEvents = 'unset';");
            }
            else if (CurrentType == Types.Container)
            {
                if (state)
                    CEF.Browser.Window.ExecuteCachedJs("document.querySelector('.crates-Inventory').style.pointerEvents = 'none';");
                else
                    CEF.Browser.Window.ExecuteCachedJs("document.querySelector('.crates-Inventory').style.pointerEvents = 'unset';");
            }
            else if (CurrentType == Types.Trade)
            {
                if (state)
                    CEF.Browser.Window.ExecuteCachedJs("document.querySelector('.trade').style.pointerEvents = 'none';");
                else
                    CEF.Browser.Window.ExecuteCachedJs("document.querySelector('.trade').style.pointerEvents = 'unset';");
            }
            else if (CurrentType == Types.Workbench)
            {
                if (state)
                    CEF.Browser.Window.ExecuteCachedJs("document.querySelector('.workbench').firstElementChild.style.pointerEvents = 'none';");
                else
                    CEF.Browser.Window.ExecuteCachedJs("document.querySelector('.workbench').firstElementChild.style.pointerEvents = 'unset';");
            }
        }

        private static int GetFreeIdx(string itemId, object[][] arr)
        {
            int eIdx = -1, idx = -1;
            var minAmount = int.MaxValue;

            var iData = itemId.Length == 0 ? null : Data.Items.GetData(itemId, null) as Data.Items.Item.ItemData.IStackable;

            for (int i = 0; i < arr.Length; i++)
            {
                if (arr[i] == null)
                {
                    if (eIdx < 0)
                    {
                        eIdx = i;

                        break;
                    }
                }
                else
                {
                    if (iData != null)
                    {
                        if (arr[i][0] is object[] tarr)
                        {
                            if (itemId == (string)tarr[0])
                            {
                                var amount = (int)tarr[3];

                                if (amount < minAmount)
                                {
                                    idx = i;

                                    minAmount = amount;
                                }
                            }
                        }
                        else
                        {
                            if (itemId == (string)arr[i][0])
                            {
                                var amount = (int)arr[i][3];

                                if (amount < minAmount)
                                {
                                    idx = i;

                                    minAmount = amount;
                                }
                            }
                        }
                    }
                }
            }

            if (eIdx >= 0)
                return eIdx;

            return idx;
        }
        #endregion
    }
}
