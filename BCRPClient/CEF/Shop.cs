using RAGE;
using RAGE.Elements;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using static BCRPClient.Locale.Notifications.Money;

namespace BCRPClient.CEF
{
    class Shop : Events.Script
    {
        //        Стало : //items[i] = [id, 'name', cash, bank, variants || maxspeed, chageable(t|f) || [slots, weight] || maxtank, cruise, autopilot, maxtrunk, maxweight] 
        //(если магаз не транспортный последние 4 параметра можно либо не передавать вообще, либо передавать как null)
        //- добавлены разделы магазинов машин(6, 7, 8, 9)

        public static bool IsActive { get => Browser.IsActiveOr(Browser.IntTypes.Shop, Browser.IntTypes.Retail); }

        public static bool IsActiveShop { get => Browser.IsActive(Browser.IntTypes.Shop); }

        public static bool IsActiveRetail { get => Browser.IsActive(Browser.IntTypes.Retail); }

        private static Additional.Camera.StateTypes[] AllowedCameraStates;

        private static DateTime LastSent;

        /// <summary>ID текущего предмета</summary>
        private static string CurrentItem;

        /// <summary>Номер вариации текущего предмета</summary>
        /// <remarks>Используется только для одежды, тюнинга, причесок, татуировок</remarks>
        private static int CurrentVariation;

        /// <summary>Номер навигации</summary>
        /// <remarks>Используется только для одежды, тюнинга, причесок, татуировок</remarks>
        private static int CurrentNavigation;

        /// <summary>Цвет текущего предмета</summary>
        /// <remarks>Используется только для транспорта</remarks>
        private static Utils.Colour CurrentColor1;

        private static Utils.Colour CurrentColor2;

        private static Vehicle TempVehicle { get; set; }

        private static bool TestDriveActive { get; set; }

        /// <summary>Тип текущего магазина</summary>
        private static Types CurrentType;

        public enum Types
        {
            None = -1,

            ClothesShop1 = 0,
            ClothesShop2,
            ClothesShop3,

            BagShop,

            CarShop1,
            CarShop2,
            CarShop3,
            MotoShop,
            BoatShop,
            AeroShop,

            Market,

            GasStation,

            WeaponShop,

            FishingShop,

            Bar,

            FurnitureShop,
        }

        public enum SectionTypes
        {
            // Market
            Food = 0,
            Drink,
            Ingredients,
            Medical,
            Other,

            // Weapons
            Melee,
            Pistols,
            Rifles,
            Submachines,
            Shotguns,
            Ammo,
            Components,

            // Furniture
            Chairs,
            Beds,
            Closets,
            Plants,
            Lamps,
            Electrics,
            Kitchens,
            Baths,
            Pics,
            Decors,
        }

        private static List<int> TempBinds;

        private static float DefaultHeading;
        private static Vector3 DefaultPosition;

        private static RAGE.Ui.Cursor.Vector2 LastCursorPos;
        private static AsyncTask CursorTask;

        private static int CurrentCameraStateNum;

        private static Dictionary<int, (int, int)> RealClothes;
        private static Dictionary<int, (int, int)> RealAccessories;

        private static Dictionary<Types, string> RetailJsTypes = new Dictionary<Types, string>()
        {
            { Types.Market, "convenience" },

            { Types.GasStation, "gas" },

            { Types.WeaponShop, "weapon" },

            { Types.FishingShop, "fish" },

            { Types.Bar, "bar" },

            { Types.FurnitureShop, "furniture" },
        };

        private static Dictionary<Types, int> ShopJsTypes = new Dictionary<Types, int>()
        {
            { Types.ClothesShop1, 0 },
            { Types.ClothesShop2, 1 },
            { Types.ClothesShop3, 2 },

            { Types.CarShop1, 6 },
            { Types.CarShop2, 7 },
            { Types.CarShop3, 8 },

            { Types.MotoShop, 9 },

            { Types.BoatShop, 10 },

            { Types.AeroShop, 11 },
        };

        private static Dictionary<Types, Dictionary<string, int>> Prices = new Dictionary<Types, Dictionary<string, int>>();

        private static Dictionary<Types, Dictionary<SectionTypes, string[]>> RetailSections = new Dictionary<Types, Dictionary<SectionTypes, string[]>>()
        {
            {
                Types.Market,

                new Dictionary<SectionTypes, string[]>()
                {
                    {
                        SectionTypes.Food,

                        new string[]
                        {
                            "f_burger",
                        }
                    },

                    {
                        SectionTypes.Drink,

                        new string[]
                        {

                        }
                    },

                    {
                        SectionTypes.Ingredients,

                        new string[]
                        {

                        }
                    },

                    {
                        SectionTypes.Medical,


                        new string[]
                        {
                            "med_b_0",

                            "med_kit_0",
                            "med_kit_1",
                        }
                    },

                    {
                        SectionTypes.Other,

                        new string[]
                        {
                            "cigs_0",
                            "cigs_1",

                            "cig_0",
                            "cig_1",
                        }
                    },
                }
            },
        };

        public Shop()
        {
            TempBinds = new List<int>();

            LastSent = DateTime.Now;

            CurrentType = Types.None;

            #region TO_REPLACE

            #endregion

            Events.Add("Shop::Show", async (object[] args) =>
            {
                Types type = (Types)(int)args[0];
                float margin = (float)args[1];
                float? heading = args.Length > 2 ? (float?)args[2] : null;

                await Show(type, margin, heading);
            });

            Events.Add("Shop::Close::Server", (object[] args) => Close(true, false));

            Events.Add("Shop::Close", (object[] args) => { Close(false, true); });

            Events.Add("Shop::UpdateColor", (object[] args) =>
            {
                string id = (string)args[0];
                Utils.Colour colour = ((string)args[1]).ToColor();

                //Utils.ConsoleOutputLimited(id);

                if (CurrentType >= Types.CarShop1 && CurrentType <= Types.AeroShop)
                {
                    if (id == "main")
                    {
                        CurrentColor1 = colour;

                        TempVehicle?.SetCustomPrimaryColour(colour.Red, colour.Green, colour.Blue);
                    }
                    else if (id == "extra")
                    {
                        CurrentColor2 = colour;

                        TempVehicle?.SetCustomSecondaryColour(colour.Red, colour.Green, colour.Blue);
                    }
                }
            });

            Events.Add("Shop::TestDrive", async (object[] args) =>
            {
                if (CurrentType >= Types.CarShop1 && CurrentType <= Types.AeroShop)
                {
                    if (TempVehicle?.Exists != true || TestDriveActive)
                        return;

                    DefaultPosition = Player.LocalPlayer.Position;

                    TestDriveActive = true;

                    var model = TempVehicle.Model;

                    TempVehicle.Destroy();

                    TempVehicle = new Vehicle(model, DefaultPosition, DefaultHeading, "TDRIVE", 255, false, 0, 0, Player.LocalPlayer.Dimension);

                    while (TempVehicle?.Exists != true)
                        await RAGE.Game.Invoker.WaitAsync(25);

                    TempVehicle.SetCustomPrimaryColour(CurrentColor1.Red, CurrentColor1.Green, CurrentColor1.Blue);

                    TempVehicle.SetCustomSecondaryColour(CurrentColor2.Red, CurrentColor2.Green, CurrentColor2.Blue);

                    TempVehicle.SetDirtLevel(0f);

                    TempVehicle.SetData("IsTestDrive", true);

                    CEF.Browser.Switch(Browser.IntTypes.Shop, false);

                    Player.LocalPlayer.SetIntoVehicle(TempVehicle.Handle, -1);

                    Player.LocalPlayer.FreezePosition(false);

                    Player.LocalPlayer.SetVisible(true, false);

                    Additional.Camera.Disable(750);

                    CEF.HUD.ShowHUD(true);

                    CEF.Cursor.Show(false, false);

                    GameEvents.Render -= CharacterCreation.ClearTasksRender;

                    GameEvents.Render -= GameEvents.DisableAllControls;

                    GameEvents.Render -= TestDriveRender;
                    GameEvents.Render += TestDriveRender;

                    TempBinds.Add(RAGE.Input.Bind(RAGE.Ui.VirtualKeys.Escape, true, () => StopTestDrive()));
                    TempBinds.Add(RAGE.Input.Bind(RAGE.Ui.VirtualKeys.F4, true, () => Additional.TuningMenu.Show()));
                }
            });

            Events.Add("Shop::Choose", async (object[] args) =>
            {
                if (CurrentType >= Types.ClothesShop1 && CurrentType <= Types.BagShop)
                {
                    bool newItem = false;

                    if (CurrentItem != (string)args[0])
                        newItem = true;

                    CurrentItem = (string)args[0];
                    CurrentVariation = (int)args[1];

                    Data.Clothes.Wear(CurrentItem, CurrentVariation);

                    var type = Data.Items.GetType(CurrentItem, true);

                    if (type == null)
                        return;

                    var data = Data.Items.GetData(CurrentItem,  type);

                    if (data == null)
                        return;

                    if (newItem)
                    {
                        if (data is Data.Items.Under.ItemData uData)
                        {
                            if (uData.ExtraData == null && uData.BestTop != null && uData.BestTop.ExtraData != null)
                                CEF.Notification.ShowHint(Locale.Notifications.Hints.ClothesShopUnderExtraNotNeedTop, false);
                            else if (uData.ExtraData != null && uData.BestTop != null && uData.BestTop.ExtraData == null)
                                CEF.Notification.ShowHint(Locale.Notifications.Hints.ClothesShopUnderExtraNeedTop, false);
                        }
                    }

/*                    var variation = CurrentVariation < data.Textures.Length && CurrentVariation >= 0 ? data.Textures[CurrentVariation] : 0;

                    Utils.ConsoleOutput($"ID: {CurrentItem}, Var: {CurrentVariation}, Drawable: {data.Drawable}, Texture: {variation}");*/
                }
                else if (CurrentType >= Types.CarShop1 && CurrentType <= Types.AeroShop)
                {
                    TempVehicle?.Destroy();

                    CurrentItem = (string)args[0];

                    var data = Data.Vehicles.GetById(CurrentItem);

                    TempVehicle = new Vehicle(data.Model, Player.LocalPlayer.Position, DefaultHeading, "SHOP", 255, false, 0, 0, Player.LocalPlayer.Dimension);

                    while (TempVehicle?.Exists != true)
                        await RAGE.Game.Invoker.WaitAsync(25);

                    CurrentCameraStateNum = 0;

                    var t = new float[] { (Additional.Camera.States[AllowedCameraStates[CurrentCameraStateNum]].SourceParams as float[])?[0] ?? 0f, TempVehicle.GetModelRange() };

                    Additional.Camera.FromState(AllowedCameraStates[CurrentCameraStateNum], TempVehicle, TempVehicle, 0, t, null);

                    TempVehicle.SetCustomPrimaryColour(CurrentColor1.Red, CurrentColor1.Green, CurrentColor1.Blue);

                    TempVehicle.SetCustomSecondaryColour(CurrentColor2.Red, CurrentColor2.Green, CurrentColor2.Blue);

                    TempVehicle.SetDirtLevel(0f);

                    TempVehicle.SetInvincible(true);
                    TempVehicle.SetCanBeDamaged(false);
                    TempVehicle.SetCanBeVisiblyDamaged(false);

                    for (int i = 0; i < 8; i++)
                        if (TempVehicle.DoesHaveDoor(i) > 0)
                            TempVehicle.SetDoorCanBreak(i, false);

                    if (data.Type != Data.Vehicles.Vehicle.Types.Boat)
                        TempVehicle.SetOnGroundProperly(0);

                    TempVehicle.FreezePosition(true);
                }
            });

            Events.Add("Shop::Action", (object[] args) =>
            {
                if (CurrentType >= Types.ClothesShop1 && CurrentType <= Types.ClothesShop3)
                {
                    Data.Clothes.Action(CurrentItem, CurrentVariation);
                }
            });

            Events.Add("Shop::NavChange", (object[] args) =>
            {
                var id = (int)args[0];

                CurrentNavigation = id;

                if (AllowedCameraStates == null)
                    return;

                if (CurrentType >= Types.ClothesShop1 && CurrentType <= Types.ClothesShop3)
                {
                    if (id == 0 || id == 1)
                        ChangeView(Array.IndexOf(AllowedCameraStates, Additional.Camera.StateTypes.Head));
                    else if (id == 2 || id == 3 || id == 4)
                        ChangeView(Array.IndexOf(AllowedCameraStates, Additional.Camera.StateTypes.Body));
                    else if (id == 6 || id == 5)
                        ChangeView(Array.IndexOf(AllowedCameraStates, Additional.Camera.StateTypes.Legs));
                    else if (id == 7)
                        ChangeView(Array.IndexOf(AllowedCameraStates, Additional.Camera.StateTypes.Foots));
                    else if (id == 8)
                        ChangeView(Array.IndexOf(AllowedCameraStates, Additional.Camera.StateTypes.LeftHand));
                    else if (id == 9)
                        ChangeView(Array.IndexOf(AllowedCameraStates, Additional.Camera.StateTypes.RightHand));
                }
            });

            Events.Add("Shop::ClearChoice", (object[] args) =>
            {
                CurrentItem = null;
                CurrentVariation = 0;

                if (CurrentType >= Types.ClothesShop1 && CurrentType <= Types.ClothesShop3)
                {
                    if (CurrentNavigation == 0)
                        Data.Clothes.Unwear(typeof(Data.Items.Hat));
                    else if (CurrentNavigation == 1)
                        Data.Clothes.Unwear(typeof(Data.Items.Glasses));
                    else if (CurrentNavigation == 2)
                        Data.Clothes.Unwear(typeof(Data.Items.Top));
                    else if (CurrentNavigation == 3)
                        Data.Clothes.Unwear(typeof(Data.Items.Under));
                    else if (CurrentNavigation == 4)
                        Data.Clothes.Unwear(typeof(Data.Items.Accessory));
                    else if (CurrentNavigation == 5)
                        Data.Clothes.Unwear(typeof(Data.Items.Gloves));
                    else if (CurrentNavigation == 6)
                        Data.Clothes.Unwear(typeof(Data.Items.Pants));
                    else if (CurrentNavigation == 7)
                        Data.Clothes.Unwear(typeof(Data.Items.Shoes));
                    else if (CurrentNavigation == 8)
                        Data.Clothes.Unwear(typeof(Data.Items.Watches));
                    else if (CurrentNavigation == 9)
                        Data.Clothes.Unwear(typeof(Data.Items.Bracelet));
                }
            });

            Events.Add("Shop::Buy", (object[] args) =>
            {
                var pData = Sync.Players.GetData(Player.LocalPlayer);

                if (pData == null)
                    return;

                bool useCash = (bool)args[0];

                string itemId = CurrentItem;
                int variation = CurrentVariation;
                int amount = 1;

                if (args.Length > 1)
                {
                    itemId = (string)args[1];
                    amount = (int)args[2];
                }

                if (itemId == null)
                    return;

                if (LastSent.IsSpam(1000, false, false))
                    return;

                Events.CallRemote("Shop::Buy", itemId, variation, amount, useCash);
            });

            var t = 1f;

            RAGE.Input.Bind(RAGE.Ui.VirtualKeys.X, true, () =>
            {
                SetPricesCoef(t += 0.25f);
            });
        }

        public static async System.Threading.Tasks.Task Show(Types type, float margin, float? heading = null)
        {
            var pData = Sync.Players.GetData(Player.LocalPlayer);

            if (pData == null)
                return;

            CurrentType = type;

            if (heading != null)
            {
                await Browser.Render(Browser.IntTypes.Shop, true);

                DefaultHeading = (float)heading;

                Additional.ExtraColshape.InteractionColshapesAllowed = false;

                Additional.SkyCamera.FadeScreen(true);

                CEF.HUD.ShowHUD(false);
                CEF.Chat.Show(false);

                BCRPClient.Interaction.Enabled = false;

                GameEvents.Render -= CharacterCreation.ClearTasksRender;
                GameEvents.Render += CharacterCreation.ClearTasksRender;

                GameEvents.Render -= GameEvents.DisableAllControls;
                GameEvents.Render += GameEvents.DisableAllControls;

                KeyBinds.DisableAll(KeyBinds.Types.Cursor);

                (new AsyncTask(async () =>
                {
                    Additional.SkyCamera.FadeScreen(false);

                    CurrentCameraStateNum = 0;

                    TempBinds.Add(RAGE.Input.Bind(RAGE.Ui.VirtualKeys.Control, true, () =>
                    {
                        if (CursorTask != null)
                            return;

                        LastCursorPos = RAGE.Ui.Cursor.Position;

                        CursorTask = new AsyncTask(() => OnTickMouse(), 10, true);
                        CursorTask.Run();
                    }));

                    TempBinds.Add(RAGE.Input.Bind(RAGE.Ui.VirtualKeys.Control, false, () =>
                    {
                        if (CursorTask == null)
                            return;

                        CursorTask.Cancel();

                        CursorTask = null;
                    }));

                    TempBinds.Add(RAGE.Input.Bind(RAGE.Ui.VirtualKeys.V, true, () =>
                    {
                        ChangeView(++CurrentCameraStateNum);
                    }));

                    Browser.Window.ExecuteJs("Shop.draw", ShopJsTypes[type]);

                    if (type >= Types.ClothesShop1 && type <= Types.BagShop)
                    {
                        Additional.Camera.Enable(Additional.Camera.StateTypes.WholePed, Player.LocalPlayer, Player.LocalPlayer, 0);

                        CEF.Notification.ShowHint(Locale.Notifications.CharacterCreation.CtrlMovePed, true, 5000);

                        AllowedCameraStates = new Additional.Camera.StateTypes[] { Additional.Camera.StateTypes.WholePed, Additional.Camera.StateTypes.Head, Additional.Camera.StateTypes.Body, Additional.Camera.StateTypes.RightHand, Additional.Camera.StateTypes.LeftHand, Additional.Camera.StateTypes.Legs, Additional.Camera.StateTypes.Foots };

                        RealClothes = new Dictionary<int, (int, int)>()
                        {
                            { 1, (Player.LocalPlayer.GetDrawableVariation(1), Player.LocalPlayer.GetTextureVariation(1)) },
                            { 2, (Player.LocalPlayer.GetDrawableVariation(2), 0) },
                            { 3, (Player.LocalPlayer.GetDrawableVariation(3), Player.LocalPlayer.GetTextureVariation(3)) },
                            { 4, (Player.LocalPlayer.GetDrawableVariation(4), Player.LocalPlayer.GetTextureVariation(4)) },
                            { 5, (Player.LocalPlayer.GetDrawableVariation(5), Player.LocalPlayer.GetTextureVariation(5)) },
                            { 6, (Player.LocalPlayer.GetDrawableVariation(6), Player.LocalPlayer.GetTextureVariation(6)) },
                            { 7, (Player.LocalPlayer.GetDrawableVariation(7), Player.LocalPlayer.GetTextureVariation(7)) },
                            { 8, (Player.LocalPlayer.GetDrawableVariation(8), Player.LocalPlayer.GetTextureVariation(8)) },
                            { 9, (Player.LocalPlayer.GetDrawableVariation(9), Player.LocalPlayer.GetTextureVariation(9)) },
                            { 10, (Player.LocalPlayer.GetDrawableVariation(10), Player.LocalPlayer.GetTextureVariation(10)) },
                            { 11, (Player.LocalPlayer.GetDrawableVariation(11), Player.LocalPlayer.GetTextureVariation(11)) },
                        };

                        RealAccessories = new Dictionary<int, (int, int)>()
                        {
                            { 0, (Player.LocalPlayer.GetPropIndex(0), Player.LocalPlayer.GetPropTextureIndex(0)) },
                            { 1, (Player.LocalPlayer.GetPropIndex(1), Player.LocalPlayer.GetPropTextureIndex(1)) },
                            { 2, (Player.LocalPlayer.GetPropIndex(2), Player.LocalPlayer.GetPropTextureIndex(2)) },
                            { 6, (Player.LocalPlayer.GetPropIndex(6), Player.LocalPlayer.GetPropTextureIndex(6)) },
                            { 7, (Player.LocalPlayer.GetPropIndex(7), Player.LocalPlayer.GetPropTextureIndex(7)) },
                        };

                        Player.LocalPlayer.SetComponentVariation(5, 0, 0, 2);
                        Player.LocalPlayer.SetComponentVariation(9, 0, 0, 2);

                        var currentTop = Data.Items.AllData[typeof(Data.Items.Top)].Where(x => ((Data.Items.Top.ItemData)x.Value).Sex == pData.Sex && ((Data.Items.Top.ItemData)x.Value).Drawable == RealClothes[11].Item1).Select(x => x.Key).FirstOrDefault();
                        var currentUnder = Data.Items.AllData[typeof(Data.Items.Under)].Where(x => ((Data.Items.Under.ItemData)x.Value).Sex == pData.Sex && ((Data.Items.Under.ItemData)x.Value).Drawable == RealClothes[8].Item1).Select(x => x.Key).FirstOrDefault();
                        var currentGloves = Data.Items.AllData[typeof(Data.Items.Gloves)].Where(x => ((Data.Items.Gloves.ItemData)x.Value).Sex == pData.Sex && ((Data.Items.Gloves.ItemData)x.Value).BestTorsos.ContainsValue(RealClothes[3].Item1)).Select(x => x.Key).FirstOrDefault();

                        if (currentTop != null)
                            Player.LocalPlayer.SetData("TempClothes::Top", new Data.Clothes.TempClothes(currentTop, RealClothes[11].Item2));

                        if (currentUnder != null)
                            Player.LocalPlayer.SetData("TempClothes::Under", new Data.Clothes.TempClothes(currentUnder, RealClothes[8].Item2));

                        if (currentGloves != null)
                            Player.LocalPlayer.SetData("TempClothes::Gloves", new Data.Clothes.TempClothes(currentGloves, RealClothes[3].Item2));

                        CEF.Notification.ShowHint(Locale.Notifications.Hints.ClothesShopOrder, false, 7500);

                        var prices = GetPrices(CurrentType);

                        if (prices == null)
                            return;

                        List<object[]> hats = new List<object[]>();
                        List<object[]> tops = new List<object[]>();
                        List<object[]> unders = new List<object[]>();
                        List<object[]> pants = new List<object[]>();
                        List<object[]> shoes = new List<object[]>();
                        List<object[]> accs = new List<object[]>();
                        List<object[]> glasses = new List<object[]>();
                        List<object[]> gloves = new List<object[]>();
                        List<object[]> watches = new List<object[]>();
                        List<object[]> bracelets = new List<object[]>();

                        var clearingItem = new object[] { "clear", Locale.General.Business.NothingItem, 0, 0, 0, false };

                        hats.Add(clearingItem);
                        tops.Add(clearingItem);
                        unders.Add(clearingItem);
                        pants.Add(clearingItem);
                        shoes.Add(clearingItem);
                        accs.Add(clearingItem);
                        glasses.Add(clearingItem);
                        gloves.Add(clearingItem);
                        watches.Add(clearingItem);
                        bracelets.Add(clearingItem);

                        foreach (var x in prices)
                        {
                            var type = Data.Items.GetType(x.Key, true);

                            if (type == null)
                                continue;

                            var data = (Data.Items.Clothes.ItemData)Data.Items.GetData(x.Key, type);

                            if (data == null || data.Sex != pData.Sex)
                                continue;

                            var obj = new object[] { x.Key, Data.Items.GetName(x.Key), x.Value * margin, x.Value * margin, data.Textures.Length, (data as Data.Items.Clothes.ItemData.IToggleable)?.ExtraData != null };

                            if (data is Data.Items.Hat.ItemData)
                                hats.Add(obj);
                            else if (data is Data.Items.Top.ItemData)
                                tops.Add(obj);
                            else if (data is Data.Items.Under.ItemData)
                                unders.Add(obj);
                            else if (data is Data.Items.Pants.ItemData)
                                pants.Add(obj);
                            else if (data is Data.Items.Shoes.ItemData)
                                shoes.Add(obj);
                            else if (data is Data.Items.Accessory.ItemData)
                                accs.Add(obj);
                            else if (data is Data.Items.Glasses.ItemData)
                                glasses.Add(obj);
                            else if (data is Data.Items.Gloves.ItemData)
                                gloves.Add(obj);
                            else if (data is Data.Items.Watches.ItemData)
                                watches.Add(obj);
                            else if (data is Data.Items.Bracelet.ItemData)
                                bracelets.Add(obj);
                        }

                        Browser.Window.ExecuteJs("Shop.fillContainer", 0, hats);
                        Browser.Window.ExecuteJs("Shop.fillContainer", 1, glasses);
                        Browser.Window.ExecuteJs("Shop.fillContainer", 2, tops);
                        Browser.Window.ExecuteJs("Shop.fillContainer", 3, unders);
                        Browser.Window.ExecuteJs("Shop.fillContainer", 4, accs);
                        Browser.Window.ExecuteJs("Shop.fillContainer", 5, gloves);
                        Browser.Window.ExecuteJs("Shop.fillContainer", 6, pants);
                        Browser.Window.ExecuteJs("Shop.fillContainer", 7, shoes);
                        Browser.Window.ExecuteJs("Shop.fillContainer", 8, watches);
                        Browser.Window.ExecuteJs("Shop.fillContainer", 9, bracelets);
                    }
                    else if (type >= Types.CarShop1 && type <= Types.AeroShop)
                    {
                        var prices = GetPrices(type);

                        if (prices == null)
                            return;

                        Player.LocalPlayer.FreezePosition(true);

                        Player.LocalPlayer.SetVisible(false, false);

                        CurrentColor1 = new Utils.Colour(255, 255, 255, 255);
                        CurrentColor2 = new Utils.Colour(255, 255, 255, 255);

                        Additional.Camera.Enable(Additional.Camera.StateTypes.WholeVehicle, Player.LocalPlayer, Player.LocalPlayer, 0);

                        AllowedCameraStates = new Additional.Camera.StateTypes[] { Additional.Camera.StateTypes.WholeVehicle, Additional.Camera.StateTypes.WholeVehicleOpen, Additional.Camera.StateTypes.FrontVehicle, Additional.Camera.StateTypes.FrontVehicleOpenHood, Additional.Camera.StateTypes.RightVehicle, Additional.Camera.StateTypes.BackVehicle, Additional.Camera.StateTypes.BackVehicleOpenTrunk, Additional.Camera.StateTypes.TopVehicle };

                        Browser.Window.ExecuteJs("Shop.fillContainer", 0, prices.Select(x =>
                        {
                            var data = Data.Vehicles.GetById(x.Key);

                            return new object[] { x.Key, data.Name, x.Value, x.Value, Math.Floor(3.6f * RAGE.Game.Vehicle.GetVehicleModelMaxSpeed(data.Model)), data.Tank, data.HasCruiseControl, data.HasAutoPilot, data.TrunkData?.Slots ?? 0, data.TrunkData?.MaxWeight ?? 0f };
                        }));
                    }

                    Browser.Switch(Browser.IntTypes.Shop, true);

                    Cursor.Show(true, true);
                }, 1500, false, 0)).Run();
            }
            else
            {
                var prices = GetPrices(type);

                if (prices == null)
                    return;

                var sections = GetSections(type);

                if (sections == null)
                    return;

                await CEF.Browser.Render(Browser.IntTypes.Retail, true, true);

                CEF.Cursor.Show(true, true);

                CEF.Browser.Window.ExecuteJs("Retail.draw", RetailJsTypes[type], sections.Select(x => x.Value.Select(y => new object[] { y, Data.Items.GetName(y), prices[y], (Data.Items.GetData(y) as Data.Items.Item.ItemData.IStackable)?.MaxAmount ?? 1, Data.Items.GetData(y).Weight, false })), null, false);

                TempBinds.Add(RAGE.Input.Bind(RAGE.Ui.VirtualKeys.Escape, true, () => Close(false, true)));
            }
        }

        public static void Close(bool ignoreTimeout = false, bool request = true)
        {
            if (CurrentType == Types.None)
                return;

            var pData = Sync.Players.GetData(Player.LocalPlayer);

            if (pData == null)
                return;

            if (request)
            {
                if (LastSent.IsSpam(1000, false, false))
                    return;

                Events.CallRemote("Business::Exit");

                LastSent = DateTime.Now;
            }
            else
            {
                if (CurrentType == Types.ClothesShop1 || CurrentType == Types.ClothesShop2 || CurrentType == Types.ClothesShop2)
                {
                    Player.LocalPlayer.ResetData("TempClothes::Top");
                    Player.LocalPlayer.ResetData("TempClothes::Under");
                    Player.LocalPlayer.ResetData("TempClothes::Gloves");
                    Player.LocalPlayer.ResetData("TempClothes::Hat");

                    foreach (var x in RealClothes)
                    {
                        Player.LocalPlayer.SetComponentVariation(x.Key, x.Value.Item1, x.Value.Item2, 2);

                        if (x.Key == 2)
                        {
                            pData.HairOverlay = pData.HairOverlay;
                        }
                    }

                    Player.LocalPlayer.ClearAllProps();

                    foreach (var x in RealAccessories)
                    {
                        Player.LocalPlayer.SetPropIndex(x.Key, x.Value.Item1, x.Value.Item2, true);
                    }

                    RealClothes.Clear();
                    RealAccessories.Clear();
                }
                else if (CurrentType >= Types.CarShop1 && CurrentType <= Types.AeroShop)
                {
                    StopTestDrive();

                    Player.LocalPlayer.FreezePosition(false);

                    TempVehicle?.Destroy();

                    TempVehicle = null;
                }

                AllowedCameraStates = null;

                if (Browser.IsRendered(Browser.IntTypes.Shop))
                {
                    Browser.Render(Browser.IntTypes.Shop, false);

                    Additional.ExtraColshape.InteractionColshapesAllowed = false;

                    Additional.SkyCamera.FadeScreen(true);

                    (new AsyncTask(() =>
                    {
                        Player.LocalPlayer.SetVisible(true, false);

                        GameEvents.Render -= CharacterCreation.ClearTasksRender;
                        GameEvents.Render -= GameEvents.DisableAllControls;

                        BCRPClient.Interaction.Enabled = true;

                        CEF.Chat.Show(true);

                        if (!Settings.Interface.HideHUD)
                            CEF.HUD.ShowHUD(true);

                        KeyBinds.EnableAll();

                        Additional.Camera.Disable();

                        Additional.SkyCamera.FadeScreen(false);
                    }, 1500, false, 0)).Run();

                    (new AsyncTask(() =>
                    {
                        Additional.ExtraColshape.InteractionColshapesAllowed = true;
                    }, 2500, false, 0)).Run();
                }
                else
                {
                    Browser.Render(Browser.IntTypes.Retail, false);
                }

                CurrentType = Types.None;

                foreach (var x in TempBinds)
                    RAGE.Input.Unbind(x);

                TempBinds.Clear();

                Cursor.Show(false, false);
            }
        }

        /// <summary>Получить цены по типу магазина</summary>
        /// <typeparam name="T">Тип выходных данных (Dictionary(string, int) or Dictionary(SectionTypes, Dictionary(string, int))></typeparam>
        /// <param name="type">Тип магазина</param>
        private static Dictionary<string, int> GetPrices(Types type) => Prices.GetValueOrDefault(type);

        private static Dictionary<SectionTypes, string[]> GetSections(Types type) => RetailSections.GetValueOrDefault(type);

        private static void SetPricesCoef(float newCoef)
        {
            if (IsActiveShop)
            {
                CEF.Browser.Window.ExecuteJs("Shop.priceCoef", newCoef);
            }
            else if (IsActiveRetail)
            {
                CEF.Browser.Window.ExecuteJs("Retail.priceCoef", newCoef);
            }
        }

        private static async void StopTestDrive()
        {
            if (!TestDriveActive)
                return;

            Additional.TuningMenu.Close();

            RAGE.Input.Unbind(TempBinds[TempBinds.Count - 1]);
            TempBinds.RemoveAt(TempBinds.Count - 1);

            RAGE.Input.Unbind(TempBinds[TempBinds.Count - 1]);
            TempBinds.RemoveAt(TempBinds.Count - 1);

            CEF.HUD.ShowHUD(false);

            GameEvents.Render -= CharacterCreation.ClearTasksRender;
            GameEvents.Render += CharacterCreation.ClearTasksRender;

            GameEvents.Render -= GameEvents.DisableAllControls;
            GameEvents.Render += GameEvents.DisableAllControls;

            TestDriveActive = false;

            Player.LocalPlayer.SetVisible(false, false);

            Additional.AntiCheat.LastPosition = DefaultPosition;

            Player.LocalPlayer.Position = Additional.AntiCheat.LastPosition;

            Player.LocalPlayer.SetHeading(DefaultHeading);

            Player.LocalPlayer.FreezePosition(true);

            CurrentCameraStateNum = 0;

            if (TempVehicle?.Exists == true)
            {
                var model = TempVehicle.Model;

                TempVehicle.Destroy();

                TempVehicle = new Vehicle(model, Player.LocalPlayer.Position, DefaultHeading, "SHOP", 255, false, 0, 0, Player.LocalPlayer.Dimension);

                while (TempVehicle?.Exists != true)
                    await RAGE.Game.Invoker.WaitAsync(25);

                var t = new float[] { (Additional.Camera.States[Additional.Camera.StateTypes.WholeVehicle].SourceParams as float[])?[0] ?? 0f, TempVehicle.GetModelRange() };

                Additional.Camera.Enable(Additional.Camera.StateTypes.WholeVehicle, TempVehicle, TempVehicle, 0, t, null);

                TempVehicle.SetCustomPrimaryColour(CurrentColor1.Red, CurrentColor1.Green, CurrentColor1.Blue);

                TempVehicle.SetCustomSecondaryColour(CurrentColor2.Red, CurrentColor2.Green, CurrentColor2.Blue);

                TempVehicle.SetDirtLevel(0f);

                TempVehicle.SetInvincible(true);
                TempVehicle.SetCanBeDamaged(false);
                TempVehicle.SetCanBeVisiblyDamaged(false);

                for (int i = 0; i < 8; i++)
                    if (TempVehicle.DoesHaveDoor(i) > 0)
                        TempVehicle.SetDoorCanBreak(i, false);

                TempVehicle.FreezePosition(true);
            }
            else
            {
                TempVehicle = null;

                Additional.Camera.Enable(Additional.Camera.StateTypes.WholeVehicle, Player.LocalPlayer, Player.LocalPlayer, 0);
            }

            CEF.Cursor.Show(true, true);

            CEF.Browser.Switch(Browser.IntTypes.Shop, true);

            GameEvents.Render -= TestDriveRender;
        }

        private static void TestDriveRender()
        {
            RAGE.Game.Pad.DisableControlAction(32, 200, true);

            if (TempVehicle?.Exists != true || TempVehicle.IsDead(0))
            {
                StopTestDrive();

                return;
            }

            if (!Additional.TuningMenu.IsActive && Player.LocalPlayer.Vehicle != null)
            {
                Utils.DrawText(Locale.TestDrive.CloseText, 0.5f, 0.925f, 255, 255, 255, 255, 0.45f, Utils.ScreenTextFontTypes.CharletComprimeColonge, false, true);
                Utils.DrawText(Locale.TestDrive.TuningText, 0.5f, 0.95f, 255, 255, 255, 255, 0.45f, Utils.ScreenTextFontTypes.CharletComprimeColonge, false, true);
            }
            else
            {
                Utils.DrawText(Locale.TestDrive.CloseText, 0.5f, 0.95f, 255, 255, 255, 255, 0.45f, Utils.ScreenTextFontTypes.CharletComprimeColonge, false, true);
            }
        }

        private static void OnTickMouse()
        {
            if (CurrentType >= Types.CarShop1 && CurrentType <= Types.AeroShop)
            {
                if (TempVehicle == null || TestDriveActive)
                    return;
            }

            var curPos = RAGE.Ui.Cursor.Position;
            var dist = curPos.Distance(LastCursorPos);
            var newHeading = TempVehicle == null ? Player.LocalPlayer.GetHeading() : TempVehicle.GetHeading();

            if (curPos.X > LastCursorPos.X)
                newHeading += dist / 10;
            else if (curPos.X < LastCursorPos.X)
                newHeading -= dist / 10;
            else if (curPos.X == LastCursorPos.X)
            {
                if (curPos.X == 0)
                    newHeading -= 5;
                else if (curPos.X + 10 >= GameEvents.ScreenResolution.X)
                    newHeading += 5;
            }

            if (RAGE.Game.Pad.GetDisabledControlNormal(0, 241) == 1f)
            {
                Additional.Camera.Fov -= 1;
            }
            else if (RAGE.Game.Pad.GetDisabledControlNormal(0, 242) == 1f)
            {
                Additional.Camera.Fov += 1;
            }

            if (TempVehicle != null)
                TempVehicle.SetHeading(newHeading);
            else
                Player.LocalPlayer.SetHeading(newHeading);

            LastCursorPos = curPos;
        }

        private static void ChangeView(int camStateNum)
        {
            if (AllowedCameraStates == null)
                return;

            if (CurrentType >= Types.CarShop1 && CurrentType <= Types.AeroShop)
            {
                if (TempVehicle == null || TestDriveActive)
                    return;
            }

            if (camStateNum >= AllowedCameraStates.Length || AllowedCameraStates.Length < camStateNum)
                camStateNum = 0;

            CurrentCameraStateNum = camStateNum;

            if (TempVehicle != null)
            {
                TempVehicle.SetHeading(DefaultHeading);

                var t = new float[] { (Additional.Camera.States[AllowedCameraStates[camStateNum]].SourceParams as float[])?[0] ?? 0f, TempVehicle.GetModelRange() };

                Additional.Camera.FromState(AllowedCameraStates[camStateNum], TempVehicle, TempVehicle, -1, t, null);
            }
            else
            {
                Player.LocalPlayer.SetHeading(DefaultHeading);

                Additional.Camera.FromState(AllowedCameraStates[camStateNum], Player.LocalPlayer, Player.LocalPlayer, -1);
            }
        }
    }
}
