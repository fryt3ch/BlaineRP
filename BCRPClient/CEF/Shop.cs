using Newtonsoft.Json.Linq;
using RAGE;
using RAGE.Elements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace BCRPClient.CEF
{
    public class Shop : Events.Script
    {
        //        Стало : //items[i] = [id, 'name', cash, variants || maxspeed, chageable(t|f) || [slots, weight] || maxtank, cruise, autopilot, maxtrunk, maxweight] 
        //(если магаз не транспортный последние 4 параметра можно либо не передавать вообще, либо передавать как null)
        //- добавлены разделы магазинов машин(6, 7, 8, 9)

        public static bool IsActive => Browser.IsActiveOr(Browser.IntTypes.Shop, Browser.IntTypes.Retail, Browser.IntTypes.Tuning, Browser.IntTypes.Salon, Browser.IntTypes.TattooSalon);

        public static bool IsRenderedShop => Browser.IsRendered(Browser.IntTypes.Shop);

        public static bool IsRenderedSalon => Browser.IsRendered(Browser.IntTypes.Salon);

        public static bool IsRenderedTattooSalon => Browser.IsRendered(Browser.IntTypes.TattooSalon);

        public static bool IsRenderedRetail => Browser.IsRendered(Browser.IntTypes.Retail);

        public static bool IsRenderedTuning => Browser.IsRendered(Browser.IntTypes.Tuning);

        private static Additional.Camera.StateTypes[] AllowedCameraStates;

        public static DateTime LastSent;

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

        public static Vehicle TempVehicle { get; private set; }

        private static GameEntity TempEntity { get; set; }

        private static bool TestDriveActive { get; set; }

        private static bool RetailPreviewActive { get; set; }

        private static decimal CurrentMargin { get; set; }

        /// <summary>Тип текущего магазина</summary>
        private static Types CurrentType { get; set; } = Types.None;

        private static Additional.ExtraColshape CloseColshape { get; set; }

        public enum Types
        {
            None = -1,

            ClothesShop1 = 0,
            ClothesShop2,
            ClothesShop3,

            JewelleryShop,

            MaskShop,

            BagShop,

            BarberShop,

            TattooShop,

            CarShop1,
            CarShop2,
            CarShop3,
            MotoShop,
            BoatShop,
            AeroShop,

            FurnitureShop,

            Market,

            GasStation,

            TuningShop,

            WeaponShop,

            Farm,
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
        }

        public enum FurnitureSubTypes
        {
            /// <summary>Кресла, стулья</summary>
            Chairs = 0,
            /// <summary>Столы</summary>
            Tables,
            /// <summary>Кровати</summary>
            Beds,
            /// <summary>Шкафы</summary>
            Closets,
            /// <summary>Растения</summary>
            Plants,
            /// <summary>Лампы</summary>
            Lamps,
            /// <summary>Электроника</summary>
            Electronics,
            /// <summary>Все для кухни</summary>
            Kitchen,
            /// <summary>Все для ванной</summary>
            Bath,
            /// <summary>Картины</summary>
            Pictures,
            /// <summary>Прочий декор</summary>
            Decores,
        }

        private static List<int> TempBinds { get; set; } = new List<int>();

        private static float DefaultHeading { get; set; }
        private static Vector3 DefaultPosition { get; set; }

        private static RAGE.Ui.Cursor.Vector2 LastCursorPos { get; set; }
        private static AsyncTask CursorTask { get; set; }

        private static int CurrentCameraStateNum { get; set; }

        private static Dictionary<int, (int, int)> RealClothes;
        private static Dictionary<int, (int, int)> RealAccessories;

        private static Dictionary<Types, string> RetailJsTypes = new Dictionary<Types, string>()
        {
            { Types.Market, "convenience" },

            { Types.GasStation, "gas" },

            { Types.WeaponShop, "weapon" },

/*            { Types.FishingShop, "fish" },

            { Types.Bar, "bar" },*/

            { Types.FurnitureShop, "furniture" },
        };

        private static Dictionary<Types, int> ShopJsTypes = new Dictionary<Types, int>()
        {
            { Types.ClothesShop1, 0 },
            { Types.ClothesShop2, 1 },
            { Types.ClothesShop3, 2 },

            { Types.JewelleryShop, 4 },

            { Types.MaskShop, 3 },

            { Types.BagShop, 5 },

            { Types.CarShop1, 6 },
            { Types.CarShop2, 7 },
            { Types.CarShop3, 8 },

            { Types.MotoShop, 9 },

            { Types.BoatShop, 10 },

            { Types.AeroShop, 11 },
        };

        private static Dictionary<Types, Dictionary<string, uint>> Prices = new Dictionary<Types, Dictionary<string, uint>>();

        private static Dictionary<FurnitureSubTypes, Data.Furniture.Types[]> FurnitureSections = new Dictionary<FurnitureSubTypes, Data.Furniture.Types[]>()
        {
            { FurnitureSubTypes.Chairs, new Data.Furniture.Types[] { Data.Furniture.Types.Chair, } },
            { FurnitureSubTypes.Tables, new Data.Furniture.Types[] { Data.Furniture.Types.Table, } },
            { FurnitureSubTypes.Beds, new Data.Furniture.Types[] { Data.Furniture.Types.Bed, } },
            { FurnitureSubTypes.Closets, new Data.Furniture.Types[] { Data.Furniture.Types.Locker, Data.Furniture.Types.Wardrobe, } },
            { FurnitureSubTypes.Plants, new Data.Furniture.Types[] { Data.Furniture.Types.Plant, } },
            { FurnitureSubTypes.Lamps, new Data.Furniture.Types[] { Data.Furniture.Types.Lamp, } },
            { FurnitureSubTypes.Electronics, new Data.Furniture.Types[] { Data.Furniture.Types.Washer, Data.Furniture.Types.TV, Data.Furniture.Types.Electronics, } },
            { FurnitureSubTypes.Kitchen, new Data.Furniture.Types[] { Data.Furniture.Types.Fridge, Data.Furniture.Types.KitchenSet, Data.Furniture.Types.KitchenStuff } },
            { FurnitureSubTypes.Bath, new Data.Furniture.Types[] { Data.Furniture.Types.Bath, Data.Furniture.Types.Toilet, Data.Furniture.Types.BathStuff } },
            { FurnitureSubTypes.Pictures, new Data.Furniture.Types[] { Data.Furniture.Types.Painting } },
            { FurnitureSubTypes.Decores, new Data.Furniture.Types[] { Data.Furniture.Types.Decor } },
        };

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

            {
                Types.WeaponShop,

                new Dictionary<SectionTypes, string[]>()
                {
                    {
                        SectionTypes.Melee,

                        new string[]
                        {
                            "w_bat",
                            "w_bottle",
                            "w_crowbar",
                            "w_dagger",
                            "w_flashlight",
                            "w_golfclub",
                            "w_hammer",
                            "w_hatchet",
                            "w_knuckles",
                            "w_machete",
                            "w_nightstick",
                            "w_poolcue",
                            "w_switchblade",
                            "w_wrench",
                        }
                    },

                    {
                        SectionTypes.Pistols,

                        new string[]
                        {
                            "w_pistol",
                            "w_pistol_mk2",
                            "w_appistol",
                            "w_combpistol",
                            "w_heavypistol",
                            "w_machpistol",
                            "w_markpistol",
                            "w_vintpistol",

                            "w_revolver",
                            "w_revolver_mk2",
                        }
                    },

                    {
                        SectionTypes.Rifles,

                        new string[]
                        {
                            "w_asrifle",
                            "w_asrifle_mk2",
                            "w_advrifle",
                            "w_carbrifle",
                            "w_comprifle",
                            "w_heavyrifle",
                        }
                    },

                    {
                        SectionTypes.Submachines,

                        new string[]
                        {
                            "w_microsmg",
                            "w_minismg",
                            "w_smg",
                            "w_smg_mk2",
                            "w_asmsg",
                            "w_combpdw",
                        }
                    },

                    {
                        SectionTypes.Shotguns,


                        new string[]
                        {
                            "w_assgun",
                            "w_heavysgun",
                            "w_pumpsgun",
                            "w_pumpsgun_mk2",
                            "w_sawnsgun",
                        }
                    },

                    {
                        SectionTypes.Ammo,


                        new string[]
                        {
                            "am_5.56",
                            "am_7.62",
                            "am_9",
                            "am_11.43",
                            "am_12",
                            "am_12.7",
                        }
                    },

                    {
                        SectionTypes.Components,


                        new string[]
                        {
                            "wc_s",
                            "wc_f",
                            "wc_g",
                            "wc_sc",
                        }
                    },
                }
            },
        };

        public Shop()
        {
            #region TO_REPLACE

            #endregion

            Events.Add("Shop::Show", async (object[] args) =>
            {
                Types type = (Types)(int)args[0];
                var margin = Utils.ToDecimal(args[1]);

                CurrentMargin = margin;

                float? heading = args.Length > 2 ? (float?)args[2] : null;

                await Show(type, heading, args.Skip(args.Length > 2 ? 3 : 2).ToArray());
            });

            Events.Add("Shop::UM", (args) =>
            {
                CurrentMargin = Utils.ToDecimal(args[0]);

                if (CurrentType == Types.None)
                    return;

                UpdateMargin();
            });

            Events.Add("Shop::Close::Server", (object[] args) => Close(true, false));

            Events.Add("Shop::Close", (object[] args) => { Close(false, true); });

            Events.Add("Retail::Action", (args) =>
            {
                //var itemId = (string)args[0];
            });

            Events.Add("Shop::UpdateColor", (object[] args) =>
            {
                var id = (string)args[0];
                var colour = ((string)args[1]).ToColour();

                //Utils.ConsoleOutputLimited(id);

                if (CurrentType >= Types.CarShop1 && CurrentType <= Types.AeroShop)
                {
                    if (TempVehicle == null)
                        return;

                    if (id == "main")
                    {
                        CurrentColor1 = colour;

                        TempVehicle.SetCustomPrimaryColour(colour.Red, colour.Green, colour.Blue);
                    }
                    else if (id == "extra")
                    {
                        CurrentColor2 = colour;

                        TempVehicle.SetCustomSecondaryColour(colour.Red, colour.Green, colour.Blue);
                    }
                }
                else if (CurrentType == Types.TuningShop)
                {
                    if (TempVehicle == null)
                        return;

                    if (id == "colour-main")
                    {
                        TempVehicle.SetCustomPrimaryColour(colour.Red, colour.Green, colour.Blue);
                    }
                    else if (id == "colour-extra")
                    {
                        TempVehicle.SetCustomSecondaryColour(colour.Red, colour.Green, colour.Blue);
                    }
                    else if (id == "tsmoke")
                    {
                        TempVehicle.SetTyreSmokeColor(colour.Red, colour.Green, colour.Blue);
                    }
                    else if (id == "neon")
                    {
                        if (!TempVehicle.IsNeonLightEnabled(0))
                            TempVehicle.SetNeonEnabled(true);

                        TempVehicle.SetNeonLightsColour(colour.Red, colour.Green, colour.Blue);
                    }

                    if (CurrentItem != id)
                        ChangeView(0);

                    CurrentItem = id;
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

                    GameEvents.DisableAllControls(false);

                    GameEvents.Render -= TestDriveRender;
                    GameEvents.Render += TestDriveRender;

                    TempBinds.Add(KeyBinds.Bind(RAGE.Ui.VirtualKeys.F4, true, () => Additional.TuningMenu.Show()));
                }
            });

            Events.Add("Shop::Choose", async (object[] args) =>
            {
                var pData = Sync.Players.GetData(Player.LocalPlayer);

                if (pData == null)
                    return;

                if (CurrentType >= Types.ClothesShop1 && CurrentType <= Types.BagShop)
                {
                    bool newItem = false;

                    if (CurrentItem != (string)args[0])
                        newItem = true;

                    CurrentItem = (string)args[0];
                    CurrentVariation = (int)args[1];

                    if (CurrentItem?.StartsWith("ring") == true)
                    {
                        Data.Clothes.Wear(CurrentItem, CurrentVariation, Player.LocalPlayer.GetData<bool?>("Temp::JewelShop::RingIsLeft") ?? false);
                    }
                    else
                    {
                        Data.Clothes.Wear(CurrentItem, CurrentVariation);
                    }

                    var type = Data.Items.GetType(CurrentItem, true);

                    if (type == null)
                        return;

                    var data = Data.Items.GetData(CurrentItem, type);

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
                else if (CurrentType == Types.TattooShop)
                {
                    var mainId = (string)args[0];

                    var subId = (string)args[1];

                    var decors = Player.LocalPlayer.GetData<Dictionary<int, Data.Customization.TattooData>>("TempDecorations");

                    if (Data.Customization.TattooData.GetZoneTypeById(mainId) is Data.Customization.TattooData.ZoneTypes zType)
                    {
                        Data.Customization.TattooData.ClearAll(Player.LocalPlayer);

                        var idx = decors.Where(x => x.Value.ZoneType == zType).Select(x => (int?)x.Key).FirstOrDefault() ?? -1;

                        decors.Remove(idx);

                        if (subId != "none")
                        {
                            var tattoIdx = int.Parse(subId.Split('_')[1]);

                            var tattoData = Data.Customization.GetTattooData(tattoIdx);

                            decors.TryAdd(tattoIdx, tattoData);
                        }

                        foreach (var x in decors.Values)
                            x.TryApply(Player.LocalPlayer);
                    }
                }
                else if (CurrentType == Types.BarberShop)
                {
                    if (args.Length == 1)
                    {
                        var hairId = (string)args[0];

                        var hairData = hairId.Split('_');

                        var variation = int.Parse(hairData[1]) - 1;

                        if (hairData[0] == "hair")
                        {
                            var curHair = Player.LocalPlayer.GetData<Data.Customization.HairStyle>("TempAppearance::Hair");

                            curHair.Id = variation;

                            Player.LocalPlayer.SetComponentVariation(2, Data.Customization.GetHair(pData.Sex, curHair.Id), 0, 2);
                        }
                        else if (hairData[0] == "beard")
                        {
                            var curBeard = Player.LocalPlayer.GetData<Data.Customization.HeadOverlay>("TempAppearance::Beard");

                            if (variation == 0)
                                curBeard.Index = 255;
                            else
                                curBeard.Index = (byte)(variation - 1);

                            Player.LocalPlayer.SetHeadOverlay(1, curBeard.Index, curBeard.Opacity);
                        }
                        else if (hairData[0] == "chest")
                        {
                            var curChest = Player.LocalPlayer.GetData<Data.Customization.HeadOverlay>("TempAppearance::Chest");

                            curChest.Index = byte.Parse(hairData[1]);

                            if (variation == 0)
                                curChest.Index = 255;
                            else
                                curChest.Index = (byte)(variation - 1);

                            Player.LocalPlayer.SetHeadOverlay(10, curChest.Index, curChest.Opacity);
                        }
                        else if (hairData[0] == "eyebrows")
                        {
                            var curEyebrows = Player.LocalPlayer.GetData<Data.Customization.HeadOverlay>("TempAppearance::Eyebrows");

                            curEyebrows.Index = byte.Parse(hairData[1]);

                            if (variation == 0)
                                curEyebrows.Index = 255;
                            else
                                curEyebrows.Index = (byte)(variation - 1);

                            Player.LocalPlayer.SetHeadOverlay(2, curEyebrows.Index, curEyebrows.Opacity);
                        }
                    }
                    else
                    {
                        var type = (string)args[0];

                        if (type == "colour")
                        {
                            var colourNum = (byte)(int)args[1];

                            var apType = (string)args[2];

                            if (apType == "hair")
                            {
                                var curHair = Player.LocalPlayer.GetData<Data.Customization.HairStyle>("TempAppearance::Hair");

                                if (args.Length > 3 && args[3] is string str)
                                {
                                    if (str == "main")
                                        curHair.Color = colourNum;
                                    else if (str == "extra")
                                        curHair.Color2 = colourNum;

                                    Player.LocalPlayer.SetHairColor(curHair.Color, curHair.Color2);

                                    Data.Customization.HairOverlay.ClearAll(Player.LocalPlayer);

                                    if (Data.Customization.GetHairOverlay(pData.Sex, curHair.Overlay) is Data.Customization.HairOverlay overlay)
                                        overlay.Apply(Player.LocalPlayer);
                                }
                            }
                            else if (apType == "beard")
                            {
                                var curBeard = Player.LocalPlayer.GetData<Data.Customization.HeadOverlay>("TempAppearance::Beard");

                                curBeard.Color = colourNum;
                                curBeard.SecondaryColor = curBeard.Color;

                                Player.LocalPlayer.SetHeadOverlayColor(1, 1, curBeard.Color, curBeard.SecondaryColor);
                            }
                            else if (apType == "chest")
                            {
                                var curChest = Player.LocalPlayer.GetData<Data.Customization.HeadOverlay>("TempAppearance::Chest");

                                curChest.Color = colourNum;
                                curChest.SecondaryColor = curChest.Color;

                                Player.LocalPlayer.SetHeadOverlayColor(10, 1, curChest.Color, curChest.SecondaryColor);
                            }
                            else if (apType == "eyebrows")
                            {
                                var curEyebrows = Player.LocalPlayer.GetData<Data.Customization.HeadOverlay>("TempAppearance::Eyebrows");

                                curEyebrows.Color = colourNum;
                                curEyebrows.SecondaryColor = curEyebrows.Color;

                                Player.LocalPlayer.SetHeadOverlayColor(2, 1, curEyebrows.Color, curEyebrows.SecondaryColor);
                            }
                            else if (apType == "lipstick")
                            {
                                var curLipstick = Player.LocalPlayer.GetData<Data.Customization.HeadOverlay>("TempAppearance::Lipstick");

                                curLipstick.Color = colourNum;
                                curLipstick.SecondaryColor = curLipstick.Color;

                                Player.LocalPlayer.SetHeadOverlayColor(8, 2, curLipstick.Color, curLipstick.SecondaryColor);
                            }
                            else if (apType == "blush")
                            {
                                var curBlush = Player.LocalPlayer.GetData<Data.Customization.HeadOverlay>("TempAppearance::Blush");

                                curBlush.Color = colourNum;
                                curBlush.SecondaryColor = curBlush.Color;

                                Player.LocalPlayer.SetHeadOverlayColor(5, 2, curBlush.Color, curBlush.SecondaryColor);
                            }
                        }
                        else if (type == "opacity")
                        {
                            var value = Utils.ToSingle(args[1]);

                            var fullId = (string)args[2];

                            if (fullId == "lipstick")
                            {
                                var curLipstick = Player.LocalPlayer.GetData<Data.Customization.HeadOverlay>("TempAppearance::Lipstick");

                                curLipstick.Opacity = value;

                                Player.LocalPlayer.SetHeadOverlay(8, curLipstick.Index, curLipstick.Opacity);
                            }
                            else if (fullId == "blush")
                            {
                                var curBlush = Player.LocalPlayer.GetData<Data.Customization.HeadOverlay>("TempAppearance::Blush");

                                curBlush.Opacity = value;

                                Player.LocalPlayer.SetHeadOverlay(5, curBlush.Index, curBlush.Opacity);
                            }
                            else if (fullId == "makeup")
                            {
                                var curMakeup = Player.LocalPlayer.GetData<Data.Customization.HeadOverlay>("TempAppearance::Makeup");

                                curMakeup.Opacity = value;

                                Player.LocalPlayer.SetHeadOverlay(4, curMakeup.Index, curMakeup.Opacity);
                            }
                        }
                        else if (type == "variant")
                        {
                            var fullId = (string)args[1];

                            var apData = fullId.Split('_');

                            var variation = int.Parse(apData[1]);

                            if (apData[0] == "hairoverlay")
                            {
                                var curHair = Player.LocalPlayer.GetData<Data.Customization.HairStyle>("TempAppearance::Hair");

                                curHair.Overlay = (byte)variation;

                                Data.Customization.HairOverlay.ClearAll(Player.LocalPlayer);

                                if (Data.Customization.GetHairOverlay(pData.Sex, curHair.Overlay) is Data.Customization.HairOverlay overlay)
                                    overlay.Apply(Player.LocalPlayer);
                            }
                            else if (apData[0] == "lipstick")
                            {
                                var curLipstick = Player.LocalPlayer.GetData<Data.Customization.HeadOverlay>("TempAppearance::Lipstick");

                                if (variation == 0)
                                    curLipstick.Index = 255;
                                else
                                    curLipstick.Index = (byte)(variation - 1);

                                Player.LocalPlayer.SetHeadOverlay(8, curLipstick.Index, curLipstick.Opacity);
                            }
                            else if (apData[0] == "blush")
                            {
                                var curBlush = Player.LocalPlayer.GetData<Data.Customization.HeadOverlay>("TempAppearance::Blush");

                                curBlush.Index = byte.Parse(apData[1]);

                                if (variation == 0)
                                    curBlush.Index = 255;
                                else
                                    curBlush.Index = (byte)(variation - 1);

                                Player.LocalPlayer.SetHeadOverlay(5, curBlush.Index, curBlush.Opacity);
                            }
                            else if (apData[0] == "makeup")
                            {
                                var curMakeup = Player.LocalPlayer.GetData<Data.Customization.HeadOverlay>("TempAppearance::Makeup");

                                if (variation == 0)
                                    curMakeup.Index = 255;
                                else
                                    curMakeup.Index = (byte)(variation - 1);

                                Player.LocalPlayer.SetHeadOverlay(4, curMakeup.Index, curMakeup.Opacity);
                            }
                        }
                    }
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

                    var pDef = Additional.Camera.States[AllowedCameraStates[CurrentCameraStateNum]].Position;

                    var pOff = new Vector3(pDef.X, pDef.Y, pDef.Z * TempVehicle.GetModelSize().Z);

                    Additional.Camera.FromState(AllowedCameraStates[CurrentCameraStateNum], TempVehicle, TempVehicle, -1, t, null, pOff);

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
                else if (CurrentType == Types.TuningShop)
                {
                    if (TempVehicle == null)
                        return;

                    var data = ((string)args[0]).Split('_');

                    var p = int.Parse(data[1]);

                    if (p < 0)
                    {
                        var id = data[0] + "_0";

                        await CEF.ActionBox.ShowMoney
                        (
                            "TuningShopDeleteMod", Locale.Get("SHOP_TUNING_MODDEL_HEADER"), Locale.Get("SHOP_TUNING_MODDEL_CONTENT", data[0] == "neon" ? Locale.Get("SHOP_TUNING_NEON_L") : data[0] == "pearl" ? Locale.Get("SHOP_TUNING_PEARL_L") : data[0] == "tsmoke" ? Locale.Get("SHOP_TUNING_TSMOKEC_L") : Locale.Get("SHOP_TUNING_WHEELC_L")),
                            
                            null,

                            async (rType) =>
                            {
                                if (rType == CEF.ActionBox.ReplyTypes.OK || rType == CEF.ActionBox.ReplyTypes.Cancel)
                                {
                                    if ((bool)await Events.CallRemoteProc("Shop::Buy", id, rType == CEF.ActionBox.ReplyTypes.OK))
                                    {
                                        var idData = id.Split('_');

                                        if (CEF.Shop.IsRenderedTuning)
                                        {
                                            CEF.Browser.Window.ExecuteJs("Tuning.switchColor", false, idData[0]);

                                            if (idData[0] == "neon")
                                            {
                                                CEF.Shop.TempVehicle?.SetNeonEnabled(false);
                                            }
                                            else if (idData[0] == "tsmoke")
                                            {
                                                CEF.Shop.TempVehicle?.SetTyreSmokeColor(255, 255, 255);
                                            }
                                            else if (idData[0] == "wcolour")
                                            {
                                                CEF.Shop.TempVehicle?.SetWheelsColour(0);
                                            }
                                            else if (idData[0] == "pearl")
                                            {
                                                CEF.Shop.TempVehicle?.SetPearlColour(0);
                                            }
                                        }
                                    }

                                    CEF.ActionBox.Close(false);
                                }
                                else if (rType == CEF.ActionBox.ReplyTypes.Additional1)
                                {
                                    CEF.ActionBox.Close(false);
                                }
                                else
                                {
                                    CEF.ActionBox.Close(false);
                                }
                            },

                            null
                        );

                        return;
                    }

                    if (data[0] == "colourt")
                    {
                        TempVehicle.SetColourType(p);
                    }
                    else if (data[0] == "pearl")
                    {
                        TempVehicle.SetPearlColour(p);
                    }
                    else if (data[0] == "wcolour")
                    {
                        TempVehicle.SetWheelsColour(p);
                    }
                    else if (data[0] == "tt")
                    {
                        TempVehicle.ToggleMod(18, p > 0);
                    }
                    else if (data[0] == "xenon")
                    {
                        TempVehicle.SetLights(2);

                        TempVehicle.SetXenonColour(p == 0 ? null : (int?)p - 2);
                    }
                    else if (data[0] == "wtint")
                    {
                        TempVehicle.SetWindowTint(p);
                    }
                    else if (data[0] == "wheel" || data[0] == "rwheel")
                    {
                        var wt = p;
                        var wn = int.Parse(data[2]);

                        if (wt == 0)
                        {
                            wn = -1;
                            wt = 6; // for bikes normal behaviour | for cars - no sense if it's 6, stock anyway
                        }
                        else
                        {
                            wt -= 1;
                        }

                        TempVehicle.SetWheels(wt, wn, data[0] == "wheel");
                    }
                    else if (data[0] != "fix" && data[0] != "keys")
                    {
                        var t = Additional.TuningMenu.Slots.Where(x => x.Value.Id == data[0]).FirstOrDefault();

                        if (t.Value.Id == null)
                            return;

                        TempVehicle.SetMod(t.Key, p - 1, false);

                        if (data[0] == "horn")
                        {
                            TempVehicle.StartHorn(2500, RAGE.Util.Joaat.Hash("HELDDOWN"), false);
                        }
                    }

                    if (data[0] != CurrentItem)
                    {
                        if (data[0] == "spoiler")
                            ChangeView(Array.IndexOf(AllowedCameraStates, Additional.Camera.StateTypes.BackVehicleUpAngle));
                        else if (data[0] == "fbump" || data[0] == "xenon")
                            ChangeView(Array.IndexOf(AllowedCameraStates, Additional.Camera.StateTypes.FrontVehicle));
                        else if (data[0] == "rbump" || data[0] == "exh")
                            ChangeView(Array.IndexOf(AllowedCameraStates, Additional.Camera.StateTypes.BackVehicle));
                        else
                            ChangeView(0);
                    }

                    CurrentItem = data[0];
                    CurrentVariation = p;
                }
            });

            Events.Add("Shop::Action", (object[] args) =>
            {
                if (CurrentType >= Types.ClothesShop1 && CurrentType <= Types.ClothesShop3)
                {
                    Data.Clothes.Action(CurrentItem, CurrentVariation);
                }
                else if (CurrentType == Types.JewelleryShop)
                {
                    if (CurrentItem?.StartsWith("ring") == true)
                    {
                        var isLeft = Player.LocalPlayer.GetData<bool>("Temp::JewelShop::RingIsLeft");

                        isLeft = !isLeft;

                        if (isLeft)
                        {
                            AllowedCameraStates = new Additional.Camera.StateTypes[] { Additional.Camera.StateTypes.Head, Additional.Camera.StateTypes.Body, Additional.Camera.StateTypes.LeftHandFingers, Additional.Camera.StateTypes.WholePed };

                            ChangeView(Array.IndexOf(AllowedCameraStates, Additional.Camera.StateTypes.LeftHandFingers));
                        }
                        else
                        {
                            AllowedCameraStates = new Additional.Camera.StateTypes[] { Additional.Camera.StateTypes.Head, Additional.Camera.StateTypes.Body, Additional.Camera.StateTypes.RightHandFingers, Additional.Camera.StateTypes.WholePed };

                            ChangeView(Array.IndexOf(AllowedCameraStates, Additional.Camera.StateTypes.RightHandFingers));
                        }

                        Player.LocalPlayer.SetData("Temp::JewelShop::RingIsLeft", isLeft);

                        Data.Clothes.Action(CurrentItem, CurrentVariation, isLeft);
                    }
                }
            });

            Events.Add("Shop::NavChange", (object[] args) =>
            {
                var id = (int)args[0];

                CurrentNavigation = id;

                if (AllowedCameraStates == null)
                    return;

                var pData = Sync.Players.GetData(Player.LocalPlayer);

                if (pData == null)
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
                else if (CurrentType == Types.JewelleryShop)
                {
                    if (id == 0)
                        ChangeView(Array.IndexOf(AllowedCameraStates, Additional.Camera.StateTypes.Body));
                    else if (id == 1)
                        ChangeView(Array.IndexOf(AllowedCameraStates, Additional.Camera.StateTypes.Head));
                    else if (id == 2)
                        ChangeView(Array.IndexOf(AllowedCameraStates, Player.LocalPlayer.GetData<bool>("Temp::JewelShop::RingIsLeft") ? Additional.Camera.StateTypes.LeftHandFingers : Additional.Camera.StateTypes.RightHandFingers));
                }
                else if (CurrentType == Types.TattooShop)
                {
                    if (id >= 0 && id <= 3)
                    {
                        ChangeView(Array.IndexOf(AllowedCameraStates, Additional.Camera.StateTypes.Head));
                    }
                    else if (id >= 4 && id <= 5)
                    {
                        ChangeView(Array.IndexOf(AllowedCameraStates, Additional.Camera.StateTypes.BodyUpper));
                    }
                    else if (id == 6)
                    {
                        ChangeView(Array.IndexOf(AllowedCameraStates, Additional.Camera.StateTypes.BodyBackUpper));
                    }
                    else if (id == 7)
                    {
                        ChangeView(Array.IndexOf(AllowedCameraStates, Additional.Camera.StateTypes.LeftHandUpper));
                    }
                    else if (id == 8)
                    {
                        ChangeView(Array.IndexOf(AllowedCameraStates, Additional.Camera.StateTypes.RightHandUpper));
                    }
                    else if (id == 9)
                    {
                        ChangeView(Array.IndexOf(AllowedCameraStates, Additional.Camera.StateTypes.LeftLeg));
                    }
                    else if (id == 10)
                    {
                        ChangeView(Array.IndexOf(AllowedCameraStates, Additional.Camera.StateTypes.RightLeg));
                    }
                }
                else if (CurrentType == Types.BarberShop)
                {
                    if (Player.LocalPlayer.HasData("TempAppearance::Chest"))
                    {
                        if (id == 3)
                        {
                            ChangeView(Array.IndexOf(AllowedCameraStates, Additional.Camera.StateTypes.Body));

                            if (RealClothes != null)
                            {
                                Player.LocalPlayer.SetComponentVariation(3, 15, 0, 2);
                                Player.LocalPlayer.SetComponentVariation(5, 0, 0, 2);
                                Player.LocalPlayer.SetComponentVariation(7, 0, 0, 2);
                                Player.LocalPlayer.SetComponentVariation(8, 15, 0, 2);
                                Player.LocalPlayer.SetComponentVariation(9, 0, 0, 2);
                                Player.LocalPlayer.SetComponentVariation(10, 0, 0, 2);
                                Player.LocalPlayer.SetComponentVariation(11, 15, 0, 2);
                            }
                        }
                        else
                        {
                            ChangeView(Array.IndexOf(AllowedCameraStates, Additional.Camera.StateTypes.Head));

                            if (RealClothes != null)
                            {
                                Player.LocalPlayer.SetComponentVariation(3, RealClothes[3].Item1, RealClothes[3].Item2, 2);
                                Player.LocalPlayer.SetComponentVariation(5, RealClothes[5].Item1, RealClothes[5].Item2, 2);
                                Player.LocalPlayer.SetComponentVariation(7, RealClothes[7].Item1, RealClothes[7].Item2, 2);
                                Player.LocalPlayer.SetComponentVariation(8, RealClothes[8].Item1, RealClothes[8].Item2, 2);
                                Player.LocalPlayer.SetComponentVariation(9, RealClothes[9].Item1, RealClothes[9].Item2, 2);
                                Player.LocalPlayer.SetComponentVariation(10, RealClothes[10].Item1, RealClothes[10].Item2, 2);
                                Player.LocalPlayer.SetComponentVariation(11, RealClothes[11].Item1, RealClothes[11].Item2, 2);
                            }
                        }
                    }
                    else
                    {
                        ChangeView(Array.IndexOf(AllowedCameraStates, Additional.Camera.StateTypes.Head));
                    }
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
                else if (CurrentType == Types.JewelleryShop)
                {
                    if (CurrentNavigation == 0)
                        Data.Clothes.Unwear(typeof(Data.Items.Accessory));
                    else if (CurrentNavigation == 1)
                        Data.Clothes.Unwear(typeof(Data.Items.Earrings));
                    else if (CurrentNavigation == 2)
                        Data.Clothes.Unwear(typeof(Data.Items.Ring));
                }
            });

            Events.Add("Shop::Buy", async (object[] args) =>
            {
                if (CurrentType == Types.None)
                    return;

                var pData = Sync.Players.GetData(Player.LocalPlayer);

                if (pData == null)
                    return;

                bool useCash = (bool)args[0];

                if (LastSent.IsSpam(250, false, false))
                    return;

                LastSent = Sync.World.ServerTime;

                if (CurrentType == Types.TuningShop)
                {
                    if (TempVehicle == null)
                        return;

                    var vData = Sync.Vehicles.GetData(TempVehicle);

                    if (vData == null)
                        return;

                    var id = (string)args[1];

                    if (id == "colour")
                    {
                        var rgb1 = TempVehicle.GetPrimaryColour();
                        var rgb2 = TempVehicle.GetSecondaryColour();

                        id += $"_{rgb1.Red}_{rgb1.Green}_{rgb1.Blue}_{rgb2.Red}_{rgb2.Green}_{rgb1.Blue}";

                        if ((bool)await Events.CallRemoteProc("Shop::Buy", id, useCash))
                        {
                            if (IsRenderedTuning)
                            {
                                CurrentColor1 = rgb1;
                                CurrentColor2 = rgb2;

                                CEF.Browser.Window.ExecuteJs("Tuning.switchColor", true, "colour", $"{rgb1.HEXNoAlpha}_{rgb2.HEXNoAlpha}");
                            }
                        }
                    }
                    else if (id == "neon")
                    {
                        var rgb = TempVehicle.GetNeonColour();

                        id += $"_{rgb.Red}_{rgb.Green}_{rgb.Blue}";

                        if ((bool)await Events.CallRemoteProc("Shop::Buy", id, useCash))
                        {
                            if (IsRenderedTuning)
                            {
                                CEF.Browser.Window.ExecuteJs("Tuning.switchColor", true, "neon", rgb.HEXNoAlpha);
                            }
                        }
                    }
                    else if (id == "tsmoke")
                    {
                        var rgb = TempVehicle.GetTyreSmokeColour();

                        id += $"_{rgb.Red}_{rgb.Green}_{rgb.Blue}";

                        if ((bool)await Events.CallRemoteProc("Shop::Buy", id, useCash))
                        {
                            if (IsRenderedTuning)
                            {
                                CEF.Browser.Window.ExecuteJs("Tuning.switchColor", true, "tsmoke", rgb.HEXNoAlpha);
                            }
                        }
                    }
                    else if (id == "pearl")
                    {
                        var p = TempVehicle.GetPearlColour();

                        if (p == null)
                            return;

                        id += $"_{p}";

                        if ((bool)await Events.CallRemoteProc("Shop::Buy", id, useCash))
                        {
                            if (IsRenderedTuning)
                                CEF.Browser.Window.ExecuteJs("Tuning.switchColor", true, "pearl", p);
                        }
                    }
                    else if (id == "wcolour")
                    {
                        var p = TempVehicle.GetWheelsColour();

                        if (p == null)
                            return;

                        id += $"_{p}";

                        if ((bool)await Events.CallRemoteProc("Shop::Buy", id, useCash))
                        {
                            if (IsRenderedTuning)
                                CEF.Browser.Window.ExecuteJs("Tuning.switchColor", true, "wcolour", p);
                        }
                    }
                    else if (id == "colourt")
                    {
                        id += $"_{TempVehicle.GetColourType()}";

                        if ((bool)await Events.CallRemoteProc("Shop::Buy", id, useCash))
                        {
                            if (IsRenderedTuning)
                                CEF.Browser.Window.ExecuteJs("Tuning.buyVariant", id);
                        }
                    }
                    else if (id == "tt")
                    {
                        id += $"_{(TempVehicle.IsToggleModOn(18) ? 1 : 0)}";

                        if ((bool)await Events.CallRemoteProc("Shop::Buy", id, useCash))
                        {
                            if (IsRenderedTuning)
                                CEF.Browser.Window.ExecuteJs("Tuning.buyVariant", id);
                        }
                    }
                    else if (id == "xenon")
                    {
                        id += $"_{(TempVehicle.GetXenonColour() ?? -2) + 2}";

                        if ((bool)await Events.CallRemoteProc("Shop::Buy", id, useCash))
                        {
                            if (IsRenderedTuning)
                                CEF.Browser.Window.ExecuteJs("Tuning.buyVariant", id);
                        }
                    }
                    else if (id == "wtint")
                    {
                        id += $"_{TempVehicle.GetWindowTint()}";

                        if ((bool)await Events.CallRemoteProc("Shop::Buy", id, useCash))
                        {
                            if (IsRenderedTuning)
                                CEF.Browser.Window.ExecuteJs("Tuning.buyVariant", id);
                        }
                    }
                    else if (id.Contains("wheel_"))
                    {
                        var idt = id.Split('_');

                        id = idt[0];

                        var n = TempVehicle.GetMod(id == "wheel" ? 23 : 24);
                        var t = int.Parse(idt[1]);

                        if (n < 0)
                        {
                            t = 0;
                            n = 0;
                        }

                        id += $"_{t}_{n}";

                        if ((bool)await Events.CallRemoteProc("Shop::Buy", id, useCash))
                        {
                            if (IsRenderedTuning)
                                CEF.Browser.Window.ExecuteJs("Tuning.buyVariant", id);
                        }
                    }
                    else if (id == "fix")
                    {
                        id += $"_{CurrentVariation}";

                        if ((bool)await Events.CallRemoteProc("Shop::Buy", id, useCash))
                        {

                        }
                    }
                    else if (id == "keys")
                    {
                        if (CurrentVariation == 0)
                        {
                            var approveContext = "TuningShopKeysChange";
                            var approveTime = 5_000;

                            if (CEF.Notification.HasApproveTimedOut(approveContext, Sync.World.ServerTime, approveTime))
                            {
                                CEF.Notification.SetCurrentApproveContext(approveContext, Sync.World.ServerTime);

                                CEF.Notification.Show(CEF.Notification.Types.Question, Locale.Get("NOTIFICATION_HEADER_APPROVE"), Locale.Get("SHOP_TUNING_KEYS_CHANGE_APPROVE"), approveTime);
                            }
                            else
                            {
                                CEF.Notification.ClearAll();

                                CEF.Notification.SetCurrentApproveContext(null, DateTime.MinValue);

                                if ((bool)await Events.CallRemoteProc("Shop::Buy", $"{id}_0", useCash))
                                {

                                }
                            }
                        }
                        else if (CurrentVariation == 1)
                        {
                            await CEF.ActionBox.ShowInputWithText
                            (
                                "TuningShopKeysTag",

                                Locale.Get("SHOP_TUNING_KEYDUBL_HEADER"),

                                Locale.Get("SHOP_TUNING_KEYDUBL_CONTENT"),

                                18, Data.Items.GetName("vk_0") ?? "null", null, null,

                                null,

                                async (CEF.ActionBox.ReplyTypes rType, string text) =>
                                {
                                    if (rType == ActionBox.ReplyTypes.Cancel)
                                    {
                                        CEF.ActionBox.Close(false);

                                        return;
                                    }

                                    text = text?.Trim();

                                    if (text == null || !(new Regex(@"^[0-9a-zA-Zа-яА-Я\-\s]{1,18}$")).IsMatch(text))
                                    {
                                        CEF.Notification.Show(CEF.Notification.Types.Error, Locale.Get("NOTIFICATION_HEADER_ERROR"), Locale.Get("GEN_TEXT_NOTMATCH_0"), -1);

                                        return;
                                    }

                                    if ((bool)await Events.CallRemoteProc("Shop::Buy", $"{id}_1_{text}", useCash))
                                    {
                                        CEF.ActionBox.Close(false);

                                        return;
                                    }
                                },

                                null
                            );
                        }
                    }
                    else
                    {
                        var slots = Additional.TuningMenu.Slots.Where(x => x.Value.Id == id).Select(x => x.Key).ToList();

                        if (slots.Count == 0)
                            return;

                        id += $"_{TempVehicle.GetMod(slots[0]) + 1}";

                        if ((bool)await Events.CallRemoteProc("Shop::Buy", id, useCash))
                        {
                            if (IsRenderedTuning)
                                CEF.Browser.Window.ExecuteJs("Tuning.buyVariant", id);
                        }
                    }
                }
                else if (CurrentType >= Types.CarShop1 && CurrentType <= Types.AeroShop)
                {
                    if (TempVehicle == null)
                        return;

                    var vehData = Data.Vehicles.GetByModel(TempVehicle.Model);

                    if (vehData == null)
                        return;

                    if ((bool)await Events.CallRemoteProc("Shop::Buy", $"{vehData.ID}_{CurrentColor1.Red}_{CurrentColor1.Green}_{CurrentColor1.Blue}_{CurrentColor2.Red}_{CurrentColor2.Green}_{CurrentColor2.Blue}", useCash))
                    {

                    }
                }
                else if (CurrentType == Types.TattooShop)
                {
                    var decors = Player.LocalPlayer.GetData<Dictionary<int, Data.Customization.TattooData>>("TempDecorations");

                    var mainId = (string)args[1];
                    var boughtItemId = mainId;
                    var serverItemId = mainId;

                    if (Data.Customization.TattooData.GetZoneTypeById(mainId) is Data.Customization.TattooData.ZoneTypes zType)
                    {
                        var idx = decors.Where(x => x.Value.ZoneType == zType).Select(x => (int?)x.Key).FirstOrDefault() ?? -1;

                        if (idx < 0)
                        {
                            boughtItemId = "none";

                            serverItemId = $"tat_-1_{(int)zType}";
                        }
                        else
                        {
                            boughtItemId = $"tat_{idx}";
                            serverItemId = boughtItemId;
                        }

                        if ((bool)await Events.CallRemoteProc("Shop::Buy", serverItemId, useCash))
                        {
                            if (IsRenderedTattooSalon)
                            {
                                CEF.Browser.Window.ExecuteJs("Tattoo.buyVariant", mainId, boughtItemId);
                            }
                        }
                    }
                }
                else if (CurrentType == Types.BarberShop)
                {
                    var itemMainId = (string)args[1];

                    var itemMainIdBase = itemMainId;
                    object boughtItemParams = null;

                    if (itemMainId == "hair")
                    {
                        var curHair = Player.LocalPlayer.GetData<Data.Customization.HairStyle>("TempAppearance::Hair");

                        itemMainId = $"{itemMainId}_{(pData.Sex ? "m" : "f")}_{curHair.Id}&{curHair.Overlay}&{curHair.Color}&{curHair.Color2}";

                        boughtItemParams = new object[] { $"hairoverlay_{curHair.Overlay}", $"hair_{curHair.Id + 1}", curHair.Color, curHair.Color2 };
                    }
                    else if (itemMainId == "beard" || itemMainId == "chest" || itemMainId == "eyebrows")
                    {
                        var curBeard = Player.LocalPlayer.GetData<Data.Customization.HeadOverlay>($"TempAppearance::{char.ToUpperInvariant(itemMainId[0]) + itemMainId.Substring(1)}");

                        itemMainId = $"{itemMainId}_{curBeard.Index}&{curBeard.Color}";

                        boughtItemParams = new object[] { curBeard.Color, $"{itemMainIdBase}_{(curBeard.Index == 255 ? 1 : curBeard.Index + 2)}" };
                    }
                    else
                    {
                        var curBeard = Player.LocalPlayer.GetData<Data.Customization.HeadOverlay>($"TempAppearance::{char.ToUpperInvariant(itemMainId[0]) + itemMainId.Substring(1)}");

                        itemMainId = $"{itemMainId}_{curBeard.Index}&{curBeard.Color}&{curBeard.Opacity}";

                        boughtItemParams = new object[] { itemMainIdBase == "makeup" ? -1 : curBeard.Color, $"{itemMainIdBase}_{(curBeard.Index == 255 ? 0 : curBeard.Index + 1)}", curBeard.Opacity };
                    }

                    if ((bool)await Events.CallRemoteProc("Shop::Buy", itemMainId, useCash))
                    {
                        if (IsRenderedSalon)
                        {
                            CEF.Browser.Window.ExecuteJs("Salon.buyVariant", itemMainIdBase, boughtItemParams);
                        }
                    }
                }
                else
                {
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

                    if (CurrentType == Types.FurnitureShop)
                    {
                        if ((bool)await Events.CallRemoteProc("Shop::Buy", $"{itemId}", useCash))
                        {

                        }
                    }
                    else
                    {
                        if ((bool)await Events.CallRemoteProc("Shop::Buy", $"{itemId}&{variation}&{amount}", useCash))
                        {

                        }
                    }
                }
            });

            // in fact - retail
            Events.Add("Shop::Try", (args) =>
            {
                if (!IsRenderedRetail)
                    return;

                if (CurrentType == Types.FurnitureShop)
                {
                    var itemId = (string)args[0];

                    if (RetailPreviewActive)
                        return;

                    var furnData = Data.Furniture.GetData(itemId);

                    if (furnData == null)
                        return;

                    CurrentItem = itemId;

                    var mapObj = Utils.CreateObjectNoOffsetImmediately(furnData.Model, 2742.275f, 3485.742f, 55.6959f);

                    mapObj.SetCollision(false, false);

                    mapObj.PlaceOnGroundProperly();

                    mapObj.SetTotallyInvincible(true);

                    mapObj.FreezePosition(true);

                    StartRetailPreview(mapObj, 194.5612f + 180f, 1, Additional.Camera.StateTypes.WholeFurniture, Additional.Camera.StateTypes.FrontFurniture, Additional.Camera.StateTypes.TopFurniture);
                }
            });
        }

        public static async System.Threading.Tasks.Task Show(Types type, float? heading, object[] args)
        {
            var pData = Sync.Players.GetData(Player.LocalPlayer);

            if (pData == null)
                return;

            CurrentType = type;

            if (heading != null)
            {
                AsyncTask task = null;

                task = new AsyncTask(async () =>
                {
                    while (Additional.SkyCamera.IsFadedOut)
                        await RAGE.Game.Invoker.WaitAsync(250);

                    if (!Utils.IsTaskStillPending("Shop::Loading", task))
                        return;

                    DefaultHeading = (float)heading;

                    if (!Settings.Interface.HideHUD)
                        CEF.HUD.ShowHUD(false);

                    CEF.Chat.Show(false);

                    BCRPClient.Interaction.Enabled = false;

                    GameEvents.Render -= CharacterCreation.ClearTasksRender;
                    GameEvents.Render += CharacterCreation.ClearTasksRender;

                    GameEvents.DisableAllControls(true);

                    KeyBinds.DisableAll(KeyBinds.Types.Cursor);

                    if (type == Types.BarberShop)
                    {
                        var prices = GetPrices(type);

                        await Browser.Render(Browser.IntTypes.Salon, true);

                        var shopData = new List<object>();

                        var hairStyle = ((JObject)args[0]).ToObject<Data.Customization.HairStyle>();

                        var beard = ((JObject)args[1]).ToObject<Data.Customization.HeadOverlay>();
                        var chestHair = ((JObject)args[2]).ToObject<Data.Customization.HeadOverlay>();

                        var eyebrows = ((JObject)args[3]).ToObject<Data.Customization.HeadOverlay>();
                        var lipstick = ((JObject)args[4]).ToObject<Data.Customization.HeadOverlay>();
                        var blush = ((JObject)args[5]).ToObject<Data.Customization.HeadOverlay>();
                        var makeup = ((JObject)args[6]).ToObject<Data.Customization.HeadOverlay>();

                        if (pData.Sex)
                        {
                            AllowedCameraStates = new Additional.Camera.StateTypes[] { Additional.Camera.StateTypes.Head, Additional.Camera.StateTypes.Body };

                            shopData.Add(new object[] { "hair", prices.Where(x => x.Key.StartsWith("hair_m")).Select(x => { var hairNum = int.Parse(x.Key.Replace("hair_m_", "")); return new object[] { hairNum + 1, x.Value, Data.Customization.GetDefaultHairOverlayId(true, hairNum) }; }), new object[] { $"hairoverlay_{hairStyle.Overlay}", $"hair_{hairStyle.Id + 1}", hairStyle.Color, hairStyle.Color2 }, Enumerable.Range(0, Data.Customization.MaleHairOverlays.Count) });
                        }
                        else
                        {
                            AllowedCameraStates = new Additional.Camera.StateTypes[] { Additional.Camera.StateTypes.Head };

                            shopData.Add(new object[] { "hair", prices.Where(x => x.Key.StartsWith("hair_f")).Select(x => { var hairNum = int.Parse(x.Key.Replace("hair_f_", "")); return new object[] { hairNum + 1, x.Value, Data.Customization.GetDefaultHairOverlayId(false, hairNum) }; }), new object[] { $"hairoverlay_{hairStyle.Overlay}", $"hair_{hairStyle.Id + 1}", hairStyle.Color, hairStyle.Color2 }, Enumerable.Range(0, Data.Customization.FemaleHairOverlays.Count) });
                        }

                        shopData.Add(new object[] { "eyebrows", Enumerable.Range(1, 33 + 1).Select(x => new object[] { x, x == 1 ? prices["eyebrows_255"] : prices["eyebrows"] }), new object[] { eyebrows.Color, $"eyebrows_{(eyebrows.Index == 255 ? 1 : eyebrows.Index + 2)}" } });

                        if (pData.Sex)
                        {
                            shopData.Add(new object[] { "beard", prices.Where(x => x.Key.StartsWith("beard")).Select(x => { var bNum = int.Parse(x.Key.Replace("beard_", "")); return new object[] { bNum == 255 ? 1 : bNum + 2, x.Value }; }), new object[] { beard.Color, $"beard_{(beard.Index == 255 ? 1 : beard.Index + 2)}" } });

                            shopData.Add(new object[] { "chest", Enumerable.Range(1, 16 + 1).Select(x => new object[] { x, x == 1 ? prices["chest_255"] : prices["chest"] }), new object[] { chestHair.Color, $"chest_{(chestHair.Index == 255 ? 1 : chestHair.Index + 2)}" } });

                            Player.LocalPlayer.SetData("TempAppearance::Beard", beard);
                            Player.LocalPlayer.SetData("TempAppearance::Chest", chestHair);
                        }

                        RealClothes = Data.Clothes.GetAllRealClothes(Player.LocalPlayer);

                        RealAccessories = Data.Clothes.GetAllRealAccessories(Player.LocalPlayer);

                        Player.LocalPlayer.SetComponentVariation(1, 0, 0, 2);
                        Player.LocalPlayer.ClearProp(0);
                        Player.LocalPlayer.ClearProp(1);

                        Player.LocalPlayer.SetData("TempAppearance::Hair", hairStyle);
                        Player.LocalPlayer.SetData("TempAppearance::Eyebrows", eyebrows);
                        Player.LocalPlayer.SetData("TempAppearance::Lipstick", lipstick);
                        Player.LocalPlayer.SetData("TempAppearance::Blush", blush);
                        Player.LocalPlayer.SetData("TempAppearance::Makeup", makeup);

                        var subData = new List<object[]>();

                        subData.Add(new object[] { "lipstick", Locale.Shop.BarberShopLipstickLabel, Enumerable.Range(0, 9 + 2).Select(x => new object[] { x == 1 ? prices["lipstick_255"] : prices["lipstick"], Locale.Shop.BarberShopNames.GetValueOrDefault($"lipstick_{x}") ?? $"lipstick_{x}" }), new object[] { lipstick.Color, $"lipstick_{(lipstick.Index == 255 ? 0 : lipstick.Index + 1)}", lipstick.Opacity } });

                        subData.Add(new object[] { "blush", Locale.Shop.BarberShopBlushLabel, Enumerable.Range(0, 6 + 28).Select(x => new object[] { x == 1 ? prices["blush_255"] : prices["blush"], Locale.Shop.BarberShopNames.GetValueOrDefault($"blush_{x}") ?? $"blush_{x}" }), new object[] { blush.Color, $"blush_{(blush.Index == 255 ? 0 : blush.Index + 1)}", blush.Opacity } });

                        subData.Add(new object[] { "makeup", Locale.Shop.BarberShopMakeupLabel, Enumerable.Range(0, 74 + 22).Select(x => new object[] { x == 1 ? prices["makeup_255"] : prices["makeup"], Locale.Shop.BarberShopNames.GetValueOrDefault($"makeup_{x}") ?? $"makeup_{x}" }), new object[] { -1, $"makeup_{(makeup.Index == 255 ? 0 : makeup.Index + 1)}", makeup.Opacity } });

                        shopData.Add(subData);

                        Browser.Switch(Browser.IntTypes.Salon, true);

                        Browser.Window.ExecuteJs("Salon.draw", new object[] { shopData });

                        Additional.Camera.Enable(Additional.Camera.StateTypes.Head, Player.LocalPlayer, Player.LocalPlayer, 0);
                    }
                    else if (type == Types.TattooShop)
                    {
                        var prices = GetPrices(type);

                        AllowedCameraStates = new Additional.Camera.StateTypes[] { Additional.Camera.StateTypes.Head, Additional.Camera.StateTypes.BodyUpper, Additional.Camera.StateTypes.RightHandUpper, Additional.Camera.StateTypes.LeftHandUpper, Additional.Camera.StateTypes.LeftLeg, Additional.Camera.StateTypes.RightLeg, Additional.Camera.StateTypes.BodyBackUpper, Additional.Camera.StateTypes.WholePed };

                        Additional.Camera.Enable(Additional.Camera.StateTypes.Head, Player.LocalPlayer, Player.LocalPlayer, 0);

                        await Browser.Render(Browser.IntTypes.TattooSalon, true);

                        var tattoos = (pData.Decorations ?? new List<int>()).ToDictionary(x => x, x => Data.Customization.GetTattooData(x));

                        var allTattoos = new Dictionary<Data.Customization.TattooData.ZoneTypes, List<object>>();

                        foreach (var x in prices)
                        {
                            var tattooIdx = int.Parse(x.Key.Split('_')[1]);

                            if (tattooIdx < 0)
                                continue;

                            var tattooData = Data.Customization.GetTattooData(tattooIdx);

                            if (tattooData == null || (tattooData.Sex is bool tSex && tSex != pData.Sex))
                                continue;

                            if (!allTattoos.ContainsKey(tattooData.ZoneType))
                                allTattoos.Add(tattooData.ZoneType, new List<object>() { new object[] { "none", Locale.Get("SHOP_SHARED_NOTHINGITEM_L"), prices[$"tat_-1_{(int)tattooData.ZoneType}"] } });

                            allTattoos[tattooData.ZoneType].Add(new object[] { x.Key, tattooData.Name, x.Value });
                        }

                        Player.LocalPlayer.SetData("TempDecorations", tattoos);

                        RealClothes = Data.Clothes.GetAllRealClothes(Player.LocalPlayer);

                        RealAccessories = Data.Clothes.GetAllRealAccessories(Player.LocalPlayer);

                        Data.Clothes.UndressAll();

                        Browser.Switch(Browser.IntTypes.TattooSalon, true);

                        Browser.Window.ExecuteJs("Tattoo.draw", allTattoos.OrderBy(x => x.Key).Select(x => new object[] { Data.Customization.TattooData.GetZoneTypeId(x.Key), Data.Customization.TattooData.GetZoneTypeName(x.Key), x.Value, tattoos.Where(y => y.Value?.ZoneType == x.Key).Select(y => $"tat_{y.Key}").FirstOrDefault() ?? "none" }));
                    }
                    else if (type >= Types.ClothesShop1 && type <= Types.ClothesShop3)
                    {
                        await Browser.Render(Browser.IntTypes.Shop, true);

                        Browser.Window.ExecuteJs("Shop.draw", ShopJsTypes[type]);

                        Additional.Camera.Enable(Additional.Camera.StateTypes.WholePed, Player.LocalPlayer, Player.LocalPlayer, 0);

                        CEF.Notification.ShowHint(Locale.Notifications.CharacterCreation.CtrlMovePed, true);

                        AllowedCameraStates = new Additional.Camera.StateTypes[] { Additional.Camera.StateTypes.WholePed, Additional.Camera.StateTypes.Head, Additional.Camera.StateTypes.Body, Additional.Camera.StateTypes.RightHand, Additional.Camera.StateTypes.LeftHand, Additional.Camera.StateTypes.Legs, Additional.Camera.StateTypes.Foots };

                        RealClothes = Data.Clothes.GetAllRealClothes(Player.LocalPlayer);

                        RealAccessories = Data.Clothes.GetAllRealAccessories(Player.LocalPlayer);

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

                        CEF.Notification.ShowHint(Locale.Notifications.Hints.ClothesShopOrder, false);

                        var prices = GetPrices(CurrentType);

                        if (prices == null)
                            return;

                        var clearingItem = new object[] { "clear", Locale.Get("SHOP_SHARED_NOTHINGITEM_L"), 0, 0, false };

                        List<object[]> hats = new List<object[]>() { clearingItem };
                        List<object[]> tops = new List<object[]>() { clearingItem };
                        List<object[]> unders = new List<object[]>() { clearingItem };
                        List<object[]> pants = new List<object[]>() { clearingItem };
                        List<object[]> shoes = new List<object[]>() { clearingItem };
                        List<object[]> accs = new List<object[]>() { clearingItem };
                        List<object[]> glasses = new List<object[]>() { clearingItem };
                        List<object[]> gloves = new List<object[]>() { clearingItem };
                        List<object[]> watches = new List<object[]>() { clearingItem };
                        List<object[]> bracelets = new List<object[]>() { clearingItem };

                        foreach (var x in prices)
                        {
                            var iType = Data.Items.GetType(x.Key, true);

                            if (iType == null)
                                continue;

                            var data = (Data.Items.Clothes.ItemData)Data.Items.GetData(x.Key, iType);

                            if (data == null || data.Sex != pData.Sex)
                                continue;

                            var obj = new object[] { x.Key, Data.Items.GetName(x.Key), x.Value, data.Textures.Length, (data as Data.Items.Clothes.ItemData.IToggleable)?.ExtraData != null };

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

                        Browser.Switch(Browser.IntTypes.Shop, true);
                    }
                    else if (type == Types.BagShop)
                    {
                        await Browser.Render(Browser.IntTypes.Shop, true);

                        Browser.Window.ExecuteJs("Shop.draw", ShopJsTypes[type]);

                        Additional.Camera.Enable(Additional.Camera.StateTypes.Body, Player.LocalPlayer, Player.LocalPlayer, 0);

                        CEF.Notification.ShowHint(Locale.Notifications.CharacterCreation.CtrlMovePed, true);

                        AllowedCameraStates = new Additional.Camera.StateTypes[] { Additional.Camera.StateTypes.Body, Additional.Camera.StateTypes.BodyBack, Additional.Camera.StateTypes.WholePed };

                        RealClothes = Data.Clothes.GetAllRealClothes(Player.LocalPlayer);

                        RealAccessories = Data.Clothes.GetAllRealAccessories(Player.LocalPlayer);

                        Player.LocalPlayer.SetComponentVariation(5, 0, 0, 2);

                        var prices = GetPrices(CurrentType);

                        var bags = new List<object>();

                        foreach (var x in prices)
                        {
                            var data = (Data.Items.Bag.ItemData)Data.Items.GetData(x.Key, typeof(Data.Items.Bag));

                            if (data == null || data.Sex != pData.Sex)
                                continue;

                            bags.Add(new object[] { x.Key, data.Name, x.Value, data.Textures.Length, new object[] { data.MaxSlots, data.MaxWeight } });
                        }

                        Browser.Switch(Browser.IntTypes.Shop, true);

                        Browser.Window.ExecuteJs("Shop.fillContainer", 0, bags);
                    }
                    else if (type == Types.JewelleryShop)
                    {
                        await Browser.Render(Browser.IntTypes.Shop, true);

                        Browser.Window.ExecuteJs("Shop.draw", ShopJsTypes[type]);

                        Additional.Camera.Enable(Additional.Camera.StateTypes.WholePed, Player.LocalPlayer, Player.LocalPlayer, 0);

                        CEF.Notification.ShowHint(Locale.Notifications.CharacterCreation.CtrlMovePed, true);

                        if (pData.WearedRing is Sync.AttachSystem.AttachmentObject atObj)
                        {
                            Player.LocalPlayer.SetData("Temp::JewelShop::RingIsLeft", atObj.Type == Sync.AttachSystem.Types.PedRingLeft3);
                        }
                        else
                        {
                            Player.LocalPlayer.SetData("Temp::JewelShop::RingIsLeft", false);
                        }

                        if (Player.LocalPlayer.GetData<bool>("Temp::JewelShop::RingIsLeft"))
                            AllowedCameraStates = new Additional.Camera.StateTypes[] { Additional.Camera.StateTypes.Head, Additional.Camera.StateTypes.Body, Additional.Camera.StateTypes.LeftHandFingers, Additional.Camera.StateTypes.WholePed };
                        else
                            AllowedCameraStates = new Additional.Camera.StateTypes[] { Additional.Camera.StateTypes.Head, Additional.Camera.StateTypes.Body, Additional.Camera.StateTypes.RightHandFingers, Additional.Camera.StateTypes.WholePed };

                        RealClothes = Data.Clothes.GetAllRealClothes(Player.LocalPlayer);

                        RealAccessories = Data.Clothes.GetAllRealAccessories(Player.LocalPlayer);

                        var prices = GetPrices(CurrentType);

                        var clearingItem = new object[] { "clear", Locale.Get("SHOP_SHARED_NOTHINGITEM_L"), 0, 0, false };

                        var necklaces = new List<object>() { clearingItem };
                        var earrings = new List<object>() { clearingItem };
                        var rings = new List<object>() { clearingItem };

                        foreach (var x in prices)
                        {
                            var data = (Data.Items.Clothes.ItemData)Data.Items.GetData(x.Key, null);

                            if (data == null || data.Sex != pData.Sex)
                                continue;

                            if (data is Data.Items.Accessory.ItemData acc)
                                necklaces.Add(new object[] { x.Key, data.Name, x.Value, data.Textures.Length, false });
                            else if (data is Data.Items.Earrings.ItemData ear)
                                earrings.Add(new object[] { x.Key, data.Name, x.Value, data.Textures.Length, false });
                            else if (data is Data.Items.Ring.ItemData ring)
                                rings.Add(new object[] { x.Key, data.Name, x.Value, data.Textures.Length, true });
                        }

                        Browser.Switch(Browser.IntTypes.Shop, true);

                        Browser.Window.ExecuteJs("Shop.fillContainer", 0, necklaces);
                        Browser.Window.ExecuteJs("Shop.fillContainer", 1, earrings);
                        Browser.Window.ExecuteJs("Shop.fillContainer", 2, rings);
                    }
                    else if (type == Types.MaskShop)
                    {
                        await Browser.Render(Browser.IntTypes.Shop, true);

                        Browser.Window.ExecuteJs("Shop.draw", ShopJsTypes[type]);

                        Additional.Camera.Enable(Additional.Camera.StateTypes.Head, Player.LocalPlayer, Player.LocalPlayer, 0);

                        CEF.Notification.ShowHint(Locale.Notifications.CharacterCreation.CtrlMovePed, true);

                        AllowedCameraStates = new Additional.Camera.StateTypes[] { Additional.Camera.StateTypes.Head, Additional.Camera.StateTypes.Body, Additional.Camera.StateTypes.WholePed };

                        RealClothes = Data.Clothes.GetAllRealClothes(Player.LocalPlayer);

                        RealAccessories = Data.Clothes.GetAllRealAccessories(Player.LocalPlayer);

                        Player.LocalPlayer.SetComponentVariation(1, 0, 0, 2);

                        var prices = GetPrices(CurrentType);

                        var masks = new List<object>();

                        foreach (var x in prices)
                        {
                            var data = (Data.Items.Mask.ItemData)Data.Items.GetData(x.Key, typeof(Data.Items.Mask));

                            if (data == null || data.Sex != pData.Sex)
                                continue;

                            masks.Add(new object[] { x.Key, data.Name, x.Value, data.Textures.Length, false });
                        }

                        Browser.Switch(Browser.IntTypes.Shop, true);

                        Browser.Window.ExecuteJs("Shop.fillContainer", 0, masks);
                    }
                    else if (type >= Types.CarShop1 && type <= Types.AeroShop)
                    {
                        await Browser.Render(Browser.IntTypes.Shop, true);

                        Browser.Window.ExecuteJs("Shop.draw", ShopJsTypes[type]);

                        var prices = GetPrices(type);

                        if (prices == null)
                            return;

                        Player.LocalPlayer.FreezePosition(true);

                        Player.LocalPlayer.SetVisible(false, false);

                        CurrentColor1 = new Utils.Colour(255, 255, 255, 255);
                        CurrentColor2 = new Utils.Colour(255, 255, 255, 255);

                        Additional.Camera.Enable(Additional.Camera.StateTypes.WholeVehicle, Player.LocalPlayer, Player.LocalPlayer, 0);

                        AllowedCameraStates = new Additional.Camera.StateTypes[] { Additional.Camera.StateTypes.WholeVehicle, Additional.Camera.StateTypes.WholeVehicleOpen, Additional.Camera.StateTypes.FrontVehicle, Additional.Camera.StateTypes.FrontVehicleOpenHood, Additional.Camera.StateTypes.RightVehicle, Additional.Camera.StateTypes.BackVehicle, Additional.Camera.StateTypes.BackVehicleOpenTrunk, Additional.Camera.StateTypes.TopVehicle };

                        Browser.Switch(Browser.IntTypes.Shop, true);

                        Browser.Window.ExecuteJs("Shop.fillContainer", 0, prices.Select(x =>
                        {
                            var data = Data.Vehicles.GetById(x.Key);

                            return new object[] { x.Key, data.Name, x.Value, Math.Floor(3.6f * RAGE.Game.Vehicle.GetVehicleModelMaxSpeed(data.Model)), data.Tank, data.HasCruiseControl, data.HasAutoPilot, data.TrunkData?.Slots ?? 0, data.TrunkData?.MaxWeight ?? 0f };
                        }));
                    }
                    else if (type == Types.TuningShop)
                    {
                        var vClassMargin = (float)args[0];

                        var prices = GetPrices(CurrentType).ToDictionary(x => x.Key, x => x.Value * vClassMargin);

                        Player.LocalPlayer.ClearTasksImmediately();

                        var veh = (Vehicle)args[1];

                        var vData = Sync.Vehicles.GetData(veh);

                        if (vData == null)
                            return;

                        var tData = new List<object>();

                        var techData = Additional.TuningMenu.Slots.ToDictionary(x => x.Value.Id, x => (x.Value.Name, x.Value.ModNames));

                        TempVehicle = veh;

                        GameEvents.Render -= RenderTuning;
                        GameEvents.Render += RenderTuning;

                        await Browser.Render(Browser.IntTypes.Tuning, true, false);

                        // tech
                        var subData = new List<object>();

                        if (vData.Data.Type != Data.Vehicles.Vehicle.Types.Boat)
                        {
                            subData.Add(new object[] { "engine", techData["engine"].Name, "variants-list", techData["engine"].ModNames.Select(x => new object[] { prices[$"engine_{x.Key + 1}"], x.Value }), $"engine_{veh.GetMod(11) + 1}" });

                            subData.Add(new object[] { "brakes", techData["brakes"].Name, "variants-list", techData["brakes"].ModNames.Select(x => new object[] { prices[$"brakes_{x.Key + 1}"], x.Value }), $"brakes_{veh.GetMod(12) + 1}" });

                            subData.Add(new object[] { "trm", techData["trm"].Name, "variants-list", techData["trm"].ModNames.Select(x => new object[] { prices[$"trm_{x.Key + 1}"], x.Value }), $"trm_{veh.GetMod(13) + 1}" });
                        }

                        if (vData.Data.Type == Data.Vehicles.Vehicle.Types.Car)
                            subData.Add(new object[] { "susp", techData["susp"].Name, "variants-list", techData["susp"].ModNames.Select(x => new object[] { prices[$"susp_{x.Key + 1}"], x.Value }), $"susp_{veh.GetMod(15) + 1}" });

                        subData.Add(new object[] { "tt", techData["tt"].Name, "variants-list", techData["tt"].ModNames.Select(x => new object[] { prices[$"susp_{x.Key + 1}"], x.Value }), $"tt_{(veh.IsToggleModOn(18) ? 1 : 0)}" });

                        tData.Add(subData);

                        // style
                        subData = new List<object>();

                        subData.Add(new object[] { "wtint", techData["wtint"].Name, "variants-list", techData["wtint"].ModNames.Select(x => new object[] { prices[$"wtint_{x.Key + 1}"], x.Value }), $"wtint_{veh.GetWindowTint()}" });

                        subData.Add(new object[] { "xenon", techData["xenon"].Name, "variants-list", techData["xenon"].ModNames.Select(x => new object[] { prices[$"xenon_{x.Key + 1}"], x.Value }), $"xenon_{(veh.GetXenonColour() ?? -2) + 2}" });

                        if (vData.Data.Type == Data.Vehicles.Vehicle.Types.Car)
                        {
                            var curNeon = vData.HasNeonMod ? veh.GetNeonColour().HEXNoAlpha : null;

                            subData.Add(new object[] { "neon", Locale.Get("SHOP_TUNING_NEON_L"), "color-selection-1", new object[] { Locale.Get("SHOP_TUNING_NEONCOL_0"), prices["neon"], prices["neon_0"] }, curNeon });
                        }

                        foreach (var x in Additional.TuningMenu.Slots.Where(x => x.Value.Id == "spoiler" || x.Value.Id == "fbump" || x.Value.Id == "rbump" || x.Value.Id == "skirt" || x.Value.Id == "exh" || x.Value.Id == "frame" || x.Value.Id == "grill" || x.Value.Id == "hood" || x.Value.Id == "roof" || x.Value.Id == "seats" || x.Value.Id == "swheel" || x.Value.Id == "livery" || x.Value.Id == "horn").ToDictionary(x => x.Value.Id, x => (x.Value.Name, x.Key)))
                        {
                            var count = veh.GetNumMods(x.Value.Key);

                            if (count > 0)
                            {
                                subData.Add(new object[] { x.Key, x.Value.Name, "variants-list", Enumerable.Range(0, count + 1).Select(y => prices[x.Key]), $"{x.Key}_{veh.GetMod(x.Value.Key) + 1}" });
                            }
                        }

                        tData.Add(subData);

                        // colours
                        subData = new List<object>();

                        CurrentColor1 = veh.GetPrimaryColour();
                        CurrentColor2 = veh.GetSecondaryColour();

                        if (vData.Data.Type != Data.Vehicles.Vehicle.Types.Boat)
                            subData.Add(new object[] { "colourt", techData["colourt"].Name, "variants-list", techData["colourt"].ModNames.Select(x => new object[] { prices[$"colourt_{x.Key}"], x.Value }), $"colourt_{veh.GetColourType()}" });

                        subData.Add(new object[] { "colour", Locale.Get("SHOP_TUNING_COLOURS_L"), "color-selection-2", new object[] { CurrentColor1.HEXNoAlpha, CurrentColor2.HEXNoAlpha, prices["colour"] } });

                        subData.Add(new object[] { "pearl", Locale.Get("SHOP_TUNING_PEARL_L"), "color-selection-many", new object[] { true, prices["pearl"], prices["pearl_0"] }, veh.GetPearlColour() });

                        if (vData.Data.Type != Data.Vehicles.Vehicle.Types.Boat)
                        {
                            subData.Add(new object[] { "wcolour", Locale.Get("SHOP_TUNING_WHEELC_L"), "color-selection-many", new object[] { true, prices["wcolour"], prices["wcolour_0"] }, veh.GetWheelsColour() });

                            var curTsCol = vData.TyreSmokeColour?.HEXNoAlpha;

                            var tyreSmokeColourLabel = Locale.Get("SHOP_TUNING_TSMOKEC_L");

                            subData.Add(new object[] { "tsmoke", tyreSmokeColourLabel, "color-selection-1", new object[] { tyreSmokeColourLabel, prices["tsmoke"], prices["tsmoke_0"] }, curTsCol });
                        }

                        tData.Add(subData);

                        // wheels

                        if (vData.Data.Type != Data.Vehicles.Vehicle.Types.Boat)
                        {
                            subData = new List<object>();

                            if (vData.Data.Type == Data.Vehicles.Vehicle.Types.Motorcycle)
                            {
                                veh.SetWheelType(6);

                                var currentWheel = veh.GetMod(23);

                                subData.Add(new object[] { $"wheel_0", Additional.TuningMenu.WheelTypes[0], "variants-list", new object[] { prices[$"wheel_0"] }, currentWheel < 0 ? $"wheel_0_0" : null });

                                var wAmount = veh.GetNumMods(23);

                                if (wAmount > 0)
                                    subData.Add(new object[] { $"wheel_7", Additional.TuningMenu.WheelTypes[7], "variants-list", Enumerable.Range(0, wAmount).Select(y => prices[$"wheel_7"]), currentWheel >= 0 ? $"wheel_0_{currentWheel}" : null });

                                tData.Add(subData);

                                subData = new List<object>();

                                currentWheel = veh.GetMod(24);

                                subData.Add(new object[] { $"rwheel_0", Additional.TuningMenu.WheelTypes[0], "variants-list", new object[] { prices[$"rwheel_0"] }, currentWheel < 0 ? $"rwheel_0_0" : null });

                                wAmount = veh.GetNumMods(24);

                                if (wAmount > 0)
                                    subData.Add(new object[] { $"rwheel_7", Additional.TuningMenu.WheelTypes[7], "variants-list", Enumerable.Range(0, wAmount).Select(y => prices[$"rwheel_7"]), currentWheel >= 0 ? $"rwheel_0_{currentWheel}" : null });

                                tData.Add(subData);
                            }
                            else
                            {
                                var currentWheels = veh.GetMod(23);
                                var currentWheelsType = veh.GetWheelType() + 1;

                                if (currentWheels < 0)
                                    currentWheelsType = 0;

                                foreach (var x in Additional.TuningMenu.WheelTypes)
                                {
                                    if (x.Key == 7)
                                        continue;

                                    if (x.Key == 0)
                                    {
                                        subData.Add(new object[] { $"wheel_{x.Key}", x.Value, "variants-list", new object[] { prices[$"wheel_{x.Key}"] }, currentWheelsType == x.Key ? $"wheel_{x.Key}_0" : null });
                                    }
                                    else
                                    {
                                        veh.SetWheelType(x.Key - 1);

                                        var wAmount = veh.GetNumMods(23);

                                        if (wAmount > 0)
                                            subData.Add(new object[] { $"wheel_{x.Key}", x.Value, "variants-list", Enumerable.Range(0, wAmount).Select(y => prices[$"wheel_{x.Key}"]), currentWheelsType == x.Key ? $"wheel_{x.Key}_{currentWheels}" : null });
                                    }
                                }

                                veh.SetWheelType(currentWheels < 0 ? 0 : currentWheelsType - 1);
                                veh.SetMod(23, currentWheels, false);

                                tData.Add(subData);
                            }
                        }

                        // misc
                        subData = new List<object>();

                        subData.Add(new object[] { "fix", Locale.Get("SHOP_TUNING_FIX_L"), "variants-list", new object[] { new object[] { prices["fix_0"], Locale.Get("SHOP_TUNING_FIX_0") }, new object[] { prices["fix_1"], Locale.Get("SHOP_TUNING_FIX_1") } }, null });

                        subData.Add(new object[] { "keys", Locale.Get("SHOP_TUNING_KEYS_L"), "variants-list", new object[] { new object[] { prices["keys_0"], Locale.Get("SHOP_TUNING_KEYS_0") }, new object[] { prices["keys_1"], Locale.Get("SHOP_TUNING_KEYS_1") } }, null });

                        tData.Add(subData);

                        Browser.Switch(Browser.IntTypes.Tuning, true);

                        Browser.Window.ExecuteJs("Tuning.draw", new object[] { tData });

                        Player.LocalPlayer.SetVisible(false, false);

                        AllowedCameraStates = new Additional.Camera.StateTypes[] { Additional.Camera.StateTypes.WholeVehicle, Additional.Camera.StateTypes.FrontVehicle, Additional.Camera.StateTypes.BackVehicleUpAngle, Additional.Camera.StateTypes.BackVehicle, Additional.Camera.StateTypes.TopVehicle };

                        ChangeView(0);
                    }

                    OnShowFinish();
                }, 0, false, 0);

                Utils.SetTaskAsPending("Shop::Loading", task);
            }
            else
            {
                //Sync.Players.CloseAll(true);

                var prices = GetPrices(type);

                if (prices == null)
                    return;

                if (type == Types.FurnitureShop)
                {
                    var subTypeNum = (int)args[0];

                    var subType = (FurnitureSubTypes)subTypeNum;

                    await CEF.Browser.Render(Browser.IntTypes.Retail, true, true);

                    CloseColshape = new Additional.Sphere(Player.LocalPlayer.Position, 2.5f, false, Utils.RedColor, uint.MaxValue, null)
                    {
                        OnExit = (cancel) =>
                        {
                            if (CloseColshape?.Exists == true)
                                Close(false);
                        }
                    };

                    var viewText = Locale.Get("SHOP_RET_VIEW_L");

                    CEF.Browser.Window.ExecuteJs("Retail.draw", $"{RetailJsTypes[type]}-{subTypeNum}", new object[] { Data.Furniture.All.Where(x => FurnitureSections[subType].Contains(x.Value.Type)).Where(x => prices.ContainsKey(x.Key)).Select(x => new object[] { x.Key, x.Value.Name, prices[x.Key], 1, 0f, viewText }) }, null, false);
                }
                else
                {
                    var sections = GetSections(type);

                    if (sections == null)
                        return;

                    await CEF.Browser.Render(Browser.IntTypes.Retail, true, true);

                    CloseColshape = new Additional.Sphere(Player.LocalPlayer.Position, 2.5f, false, Utils.RedColor, uint.MaxValue, null)
                    {
                        OnExit = (cancel) =>
                        {
                            if (CloseColshape?.Exists == true)
                                Close(false);
                        }
                    };

                    CEF.Browser.Window.ExecuteJs("Retail.draw", RetailJsTypes[type], sections.Select(x => x.Value.Select(y => { var itemData = Data.Items.GetData(y); return new object[] { y, itemData.Name, prices[y], (itemData as Data.Items.Item.ItemData.IStackable)?.MaxAmount ?? 1, itemData.Weight, null }; })), null, false);
                }

                OnShowFinish();
            }
        }

        private static void OnShowFinish()
        {
            UpdateMargin();

            CEF.Cursor.Show(true, true);

            CurrentCameraStateNum = 0;

            TempBinds.Add(KeyBinds.Bind(RAGE.Ui.VirtualKeys.Control, true, () =>
            {
                if (CursorTask != null)
                    return;

                LastCursorPos = RAGE.Ui.Cursor.Position;

                CursorTask = new AsyncTask(() => OnTickMouse(), 10, true);
                CursorTask.Run();
            }));

            TempBinds.Add(KeyBinds.Bind(RAGE.Ui.VirtualKeys.Control, false, () =>
            {
                if (CursorTask == null)
                    return;

                CursorTask.Cancel();

                CursorTask = null;
            }));

            TempBinds.Add(KeyBinds.Bind(RAGE.Ui.VirtualKeys.V, true, () =>
            {
                ChangeView(CurrentCameraStateNum + 1);
            }));

            TempBinds.Add(KeyBinds.Bind(RAGE.Ui.VirtualKeys.Escape, true, () =>
            {
                if (CEF.ActionBox.IsActive)
                {
                    CEF.ActionBox.Close(false);

                    return;
                }
                else if (TestDriveActive)
                {
                    StopTestDrive();
                }
                else if (RetailPreviewActive)
                {
                    StopRetailPreview();
                }
                else
                {
                    if (!IsActive)
                        return;

                    Close(false, true);
                }
            }));
        }

        public static async void Close(bool ignoreTimeout = false, bool request = true)
        {
            if (CurrentType == Types.None)
                return;

            var pData = Sync.Players.GetData(Player.LocalPlayer);

            if (pData == null)
                return;

            if (CurrentType == Types.TuningShop && CEF.ActionBox.CurrentContextStr != null && (CEF.ActionBox.CurrentContextStr == "TuningShopDeleteMod" || CEF.ActionBox.CurrentContextStr == "TuningShopKeysTag"))
            {
                CEF.ActionBox.Close(true);
            }

            if (request)
            {
                GameEvents.Render -= RenderTuning;

                Events.CallRemote("Business::Exit");
            }
            else
            {
                Utils.CancelPendingTask("Shop::Loading");

                if (CursorTask != null)
                {
                    CursorTask.Cancel();

                    CursorTask = null;
                }

                if (RealClothes != null)
                {
                    foreach (var x in RealClothes)
                    {
                        Player.LocalPlayer.SetComponentVariation(x.Key, x.Value.Item1, x.Value.Item2, 2);

                        if (x.Key == 2)
                            pData.HairOverlay?.Apply(Player.LocalPlayer);
                    }

                    RealClothes = null;
                }

                if (RealAccessories != null)
                {
                    Player.LocalPlayer.ClearAllProps();

                    foreach (var x in RealAccessories)
                    {
                        Player.LocalPlayer.SetPropIndex(x.Key, x.Value.Item1, x.Value.Item2, true);
                    }

                    RealAccessories = null;
                }

                if (Player.LocalPlayer.HasData("TempDecorations"))
                {
                    Data.Customization.TattooData.ClearAll(Player.LocalPlayer);

                    if (pData.Decorations is List<int> decors)
                        foreach (var x in decors)
                            Data.Customization.GetTattooData(x)?.TryApply(Player.LocalPlayer);

                    Player.LocalPlayer.ResetData("TempDecorations");
                }

                if (CurrentType == Types.JewelleryShop)
                {
                    Player.LocalPlayer.ResetData("Temp::JewelShop::RingIsLeft");

                    Data.Clothes.Unwear(typeof(Data.Items.Ring));
                }
                if (CurrentType == Types.ClothesShop1 || CurrentType == Types.ClothesShop2 || CurrentType == Types.ClothesShop3)
                {
                    Player.LocalPlayer.ResetData("TempClothes::Top");
                    Player.LocalPlayer.ResetData("TempClothes::Under");
                    Player.LocalPlayer.ResetData("TempClothes::Gloves");
                    Player.LocalPlayer.ResetData("TempClothes::Hat");
                }
                else if (CurrentType == Types.BarberShop)
                {
                    Player.LocalPlayer.ResetData("TempAppearance::Hair");
                    Player.LocalPlayer.ResetData("TempAppearance::Beard");
                    Player.LocalPlayer.ResetData("TempAppearance::Chest");
                    Player.LocalPlayer.ResetData("TempAppearance::Eyebrows");
                    Player.LocalPlayer.ResetData("TempAppearance::Lipstick");
                    Player.LocalPlayer.ResetData("TempAppearance::Blush");
                    Player.LocalPlayer.ResetData("TempAppearance::Makeup");
                }
                else if (CurrentType >= Types.CarShop1 && CurrentType <= Types.AeroShop)
                {
                    StopTestDrive();

                    Player.LocalPlayer.FreezePosition(false);

                    TempVehicle?.Destroy();

                    TempVehicle = null;
                }
                else if (CurrentType == Types.TuningShop)
                {
                    TempVehicle = null;
                }

                AllowedCameraStates = null;

                if (Browser.IsRendered(Browser.IntTypes.Shop) || Browser.IsRendered(Browser.IntTypes.Tuning) || Browser.IsRendered(Browser.IntTypes.Salon) || Browser.IsRendered(Browser.IntTypes.TattooSalon))
                {
                    Browser.Render(Browser.IntTypes.Shop, false);
                    Browser.Render(Browser.IntTypes.Tuning, false);
                    Browser.Render(Browser.IntTypes.Salon, false);
                    Browser.Render(Browser.IntTypes.TattooSalon, false);

                    /*                    while (Additional.SkyCamera.IsFadedOut)
                                            await RAGE.Game.Invoker.WaitAsync(250);*/

                    Player.LocalPlayer.SetVisible(true, false);

                    GameEvents.Render -= CharacterCreation.ClearTasksRender;

                    GameEvents.DisableAllControls(false);

                    BCRPClient.Interaction.Enabled = true;

                    CEF.Chat.Show(true);

                    if (!Settings.Interface.HideHUD)
                        CEF.HUD.ShowHUD(true);

                    KeyBinds.EnableAll();

                    Additional.Camera.Disable();
                }
                else if (Browser.IsRendered(Browser.IntTypes.Retail))
                {
                    CloseColshape?.Destroy();

                    CloseColshape = null;

                    Browser.Render(Browser.IntTypes.Retail, false);

                    StopRetailPreview();
                }

                CurrentType = Types.None;

                foreach (var x in TempBinds)
                    KeyBinds.Unbind(x);

                TempBinds.Clear();

                Cursor.Show(false, false);
            }
        }

        /// <summary>Получить цены по типу магазина</summary>
        /// <typeparam name="T">Тип выходных данных (Dictionary(string, int) or Dictionary(SectionTypes, Dictionary(string, int))></typeparam>
        /// <param name="type">Тип магазина</param>
        public static Dictionary<string, uint> GetPrices(Types type) => Prices.GetValueOrDefault(type);

        private static Dictionary<SectionTypes, string[]> GetSections(Types type) => RetailSections.GetValueOrDefault(type);

        private static void UpdateMargin()
        {
            if (IsRenderedShop)
            {
                CEF.Browser.Window.ExecuteJs("Shop.priceCoef", CurrentMargin);
            }
            else if (IsRenderedRetail)
            {
                CEF.Browser.Window.ExecuteJs("Retail.priceCoef", CurrentMargin);
            }
            else if (IsRenderedTuning)
            {
                CEF.Browser.Window.ExecuteJs("Tuning.priceCoef", CurrentMargin);
            }
            else if (IsRenderedTattooSalon)
            {
                CEF.Browser.Window.ExecuteJs("Tattoo.priceCoef", CurrentMargin);
            }
            else if (IsRenderedSalon)
            {
                CEF.Browser.Window.ExecuteJs("Salon.priceCoef", CurrentMargin);
            }
        }

        private static async void StopTestDrive()
        {
            if (!TestDriveActive)
                return;

            Additional.TuningMenu.Close();

            KeyBinds.Unbind(TempBinds[TempBinds.Count - 1]);
            TempBinds.RemoveAt(TempBinds.Count - 1);

            CEF.HUD.ShowHUD(false);

            GameEvents.Render -= CharacterCreation.ClearTasksRender;
            GameEvents.Render += CharacterCreation.ClearTasksRender;

            GameEvents.DisableAllControls(true);

            TestDriveActive = false;

            Player.LocalPlayer.SetVisible(false, false);

            Additional.AntiCheat.LastPosition = DefaultPosition;

            Player.LocalPlayer.Position = Additional.AntiCheat.LastPosition;

            Player.LocalPlayer.SetHeading(DefaultHeading);

            Player.LocalPlayer.FreezePosition(true);

            CurrentCameraStateNum = 0;

            if (TempVehicle != null)
            {
                var model = TempVehicle.Model;

                TempVehicle.Destroy();

                TempVehicle = new Vehicle(model, Player.LocalPlayer.Position, DefaultHeading, "SHOP", 255, false, 0, 0, Player.LocalPlayer.Dimension);

                while (TempVehicle?.Exists != true)
                    await RAGE.Game.Invoker.WaitAsync(25);

                var cs = Additional.Camera.StateTypes.WholeVehicle;

                var t = new float[] { (Additional.Camera.States[cs].SourceParams as float[])?[0] ?? 0f, TempVehicle.GetModelRange() };

                var pDef = Additional.Camera.States[cs].Position;

                var pOff = new Vector3(pDef.X, pDef.Y, pDef.Z * TempVehicle.GetModelSize().Z);

                Additional.Camera.Enable(Additional.Camera.StateTypes.WholeVehicle, TempVehicle, TempVehicle, 0, t, null, pOff);

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
                Utils.DrawText(Locale.Get("SHOP_TESTDRIVE_HELP_0"), 0.5f, 0.925f, 255, 255, 255, 255, 0.45f, RAGE.Game.Font.ChaletComprimeCologne, false, true);
                Utils.DrawText(Locale.Get("SHOP_TESTDRIVE_HELP_1"), 0.5f, 0.95f, 255, 255, 255, 255, 0.45f, RAGE.Game.Font.ChaletComprimeCologne, false, true);
            }
            else
            {
                Utils.DrawText(Locale.Get("SHOP_TESTDRIVE_HELP_0"), 0.5f, 0.95f, 255, 255, 255, 255, 0.45f, RAGE.Game.Font.ChaletComprimeCologne, false, true);
            }
        }

        private static void OnTickMouse()
        {
            if (AllowedCameraStates == null)
                return;

            if (CurrentType >= Types.CarShop1 && CurrentType <= Types.AeroShop)
            {
                if (TempVehicle == null || TestDriveActive)
                    return;
            }

            var curPos = RAGE.Ui.Cursor.Position;
            var dist = curPos.Distance(LastCursorPos);
            var newHeading = 0f;

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

            if (TempEntity != null)
                RAGE.Game.Entity.SetEntityHeading(TempEntity.Handle, RAGE.Game.Entity.GetEntityHeading(TempEntity.Handle) + newHeading);
            else if (TempVehicle != null)
                TempVehicle.SetHeading(TempVehicle.GetHeading() + newHeading);
            else
                Player.LocalPlayer.SetHeading(Player.LocalPlayer.GetHeading() + newHeading);

            LastCursorPos = curPos;
        }

        private static void RenderTuning()
        {
            if (TempVehicle?.Exists != true || TempVehicle.IsDead(0))
            {
                Close(true, true);

                return;
            }
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

            if (IsRenderedRetail)
            {
                if (TempEntity != null)
                {
                    RAGE.Game.Entity.SetEntityHeading(TempEntity.Handle, DefaultHeading);

                    Additional.Camera.FromState(AllowedCameraStates[camStateNum], TempEntity, TempEntity, -1);
                }
                else
                {
                    Player.LocalPlayer.SetHeading(DefaultHeading);

                    Additional.Camera.FromState(AllowedCameraStates[camStateNum], Player.LocalPlayer, Player.LocalPlayer, -1);
                }
            }
            else
            {
                if (TempVehicle != null)
                {
                    TempVehicle.SetHeading(DefaultHeading);

                    //var t = new float[] { (Additional.Camera.States[AllowedCameraStates[camStateNum]].SourceParams as float[])?[0] ?? 0f, TempVehicle.GetModelRange() };
                    var t = new float[] { (Additional.Camera.States[AllowedCameraStates[camStateNum]].SourceParams as float[])?[0] ?? 0f, 5f };

                    var pDef = Additional.Camera.States[AllowedCameraStates[camStateNum]].Position;

                    //var pOff = new Vector3(pDef.X, pDef.Y, pDef.Z * TempVehicle.GetModelSize().Z);
                    var pOff = new Vector3(pDef.X, pDef.Y, pDef.Z * 1.5f);

                    Additional.Camera.FromState(AllowedCameraStates[camStateNum], TempVehicle, TempVehicle, -1, t, null, pOff);
                }
                else
                {
                    Player.LocalPlayer.SetHeading(DefaultHeading);

                    Additional.Camera.FromState(AllowedCameraStates[camStateNum], Player.LocalPlayer, Player.LocalPlayer, -1);
                }
            }
        }

        private static void StartRetailPreview(GameEntity gEntity, float defaultHeading, byte renderType, params Additional.Camera.StateTypes[] cameraStates)
        {
            if (RetailPreviewActive)
                return;

            RetailPreviewActive = true;

            CEF.Browser.SwitchTemp(Browser.IntTypes.Retail, false);

            GameEvents.DisableAllControls(true);

            KeyBinds.DisableAll(KeyBinds.Types.MicrophoneOn, KeyBinds.Types.MicrophoneOff);

            CEF.HUD.ShowHUD(false);
            CEF.Chat.Show(false);

            DefaultHeading = defaultHeading;

            RAGE.Game.Entity.SetEntityHeading(gEntity.Handle, defaultHeading);

            if (gEntity.Handle != Player.LocalPlayer.Handle)
            {
                TempEntity = gEntity;
            }

            AllowedCameraStates = cameraStates;

            ChangeView(0);

            if (renderType == 1)
            {
                GameEvents.Render -= RetailPreviewFurnitureRender;
                GameEvents.Render += RetailPreviewFurnitureRender;
            }
        }

        private static void StopRetailPreview()
        {
            if (!RetailPreviewActive)
                return;

            RetailPreviewActive = false;

            if (IsRenderedRetail)
            {
                CEF.Browser.SwitchTemp(Browser.IntTypes.Retail, true);
            }

            if (TempEntity != null)
            {
                TempEntity.Destroy();

                TempEntity = null;
            }

            AllowedCameraStates = null;

            Additional.Camera.Disable(0);

            GameEvents.DisableAllControls(false);

            KeyBinds.EnableAll();

            if (!Settings.Interface.HideHUD)
                CEF.HUD.ShowHUD(true);

            CEF.Chat.Show(true);

            GameEvents.Render -= RetailPreviewFurnitureRender;
        }

        private static void RetailPreviewFurnitureRender()
        {
            if (CurrentItem != null)
            {
                var furnData = Data.Furniture.GetData(CurrentItem);

                if (furnData == null)
                    return;

                var text = furnData.Name;

                Utils.DrawText(text, 0.5f, 0.850f, 255, 255, 255, 255, 0.5f, RAGE.Game.Font.ChaletComprimeCologne, true, true);

                text = Locale.Get("SHOP_RET_PREVIEW_HELP_0", KeyBinds.ExtraBind.GetKeyString(RAGE.Ui.VirtualKeys.Escape));

                Utils.DrawText(text, 0.5f, 0.920f, 255, 255, 255, 255, 0.5f, RAGE.Game.Font.ChaletComprimeCologne, true, true);

                text = Locale.Get("SHOP_RET_PREVIEW_HELP_1");

                Utils.DrawText(text, 0.5f, 0.950f, 255, 255, 255, 255, 0.5f, RAGE.Game.Font.ChaletComprimeCologne, true, true);
            }
        }
    }
}
