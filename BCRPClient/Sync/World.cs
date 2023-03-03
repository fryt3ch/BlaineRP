using RAGE;
using RAGE.Elements;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BCRPClient.Sync
{
    class World : Events.Script
    {
        public static DateTime ServerTime { get; private set; } = DateTime.UtcNow.AddHours(Settings.SERVER_TIME_UTC_OFFSET);

        public static DateTime LocalTime { get; private set; } = DateTime.Now;

        public static long ServerTimestampMilliseconds => GetSharedData<long>("cst");

        public class ItemOnGround
        {
            public static DateTime LastShowed;
            public static DateTime LastSent;

            public enum Types : byte
            {
                /// <summary>Стандартный тип предмета на земле</summary>
                /// <remarks>Автоматически удаляется с определенными условиями, может быть подобран кем угодно</remarks>
                Default = 0,

                /// <summary>Тип предмета на земле, который был намеренно установлен игроком (предметы, наследующие вбстрактный класс PlaceableItem)</summary>
                /// <remarks>Предметы данного типа не удаляется автоматически, так же не могут быть подобраны кем угодно (пока действуют определенные условия)</remarks>
                PlacedItem,
            }

            public MapObject Object { get; set; }

            public Types Type => (Types)(byte)Object.GetSharedData<int>("IOG", 0);

            public int Amount => Object.GetSharedData<int>("A", 0);

            public string Id => Object.GetSharedData<string>("I", null);

            public uint Uid => Object.GetSharedData<object>("U", 0).ToUInt32();

            public bool IsLocked => Object.GetSharedData<bool>("L", false);

            public string Name { get; private set; }

            private ItemOnGround(MapObject Object)
            {
                this.Object = Object;

                this.Name = Data.Items.GetName(Id);
            }

            public static ItemOnGround GetItemOnGroundObject(MapObject obj)
            {
                if (obj == null)
                    return null;

                if (ItemsOnGround.Where(x => x.Object == obj).FirstOrDefault() is ItemOnGround existingIog)
                    return existingIog;

                return new ItemOnGround(obj);
            }

            public void TakeItem()
            {
                if (Utils.IsAnyCefActive(true))
                    return;

                if (Amount == 1)
                {
                    if (LastSent.IsSpam(500, false, false))
                        return;

                    Events.CallRemote("Inventory::Take", Uid, 1);

                    LastSent = Sync.World.ServerTime;
                }
                else
                {
                    if (LastShowed.IsSpam(500, false, false))
                        return;

                    CEF.ActionBox.ShowRange(CEF.ActionBox.Contexts.ItemOnGroundTakeRange, string.Format(Locale.Actions.Take, Name), 1, Amount, Amount, 1, CEF.ActionBox.RangeSubTypes.Default, this);

                    LastShowed = Sync.World.ServerTime;
                }
            }
        }

        public static bool Preloaded { get; private set; }

        public enum WeatherTypes : byte
        {
            BLIZZARD = 0,
            CLEAR,
            CLEARING,
            CLOUDS,
            EXTRASUNNY,
            FOGGY,
            HALLOWEEN,
            NEUTRAL,
            OVERCAST,
            RAIN,
            SMOG,
            SNOW,
            SNOWLIGHT,
            THUNDER,
            XMAS,
        }

        public static WeatherTypes CurrentWeatherServer => (WeatherTypes)GetSharedData<int>("Weather");

        public static WeatherTypes? CurrentWeatherCustom { get; set; }

        public static WeatherTypes? CurrentWeatherSpecial { get; set; }

        /// <summary>Ближайший к игроку предмет на земле</summary>
        public static ItemOnGround ClosestItemOnGround { get; set; }

        private static bool _EnabledItemsOnGround;

        /// <summary>Включено ли взаимодействие с предметами на земле в данный момент?</summary>
        public static bool EnabledItemsOnGround { get => _EnabledItemsOnGround; set { if (!_EnabledItemsOnGround && value) { GameEvents.Render -= ItemsOnGroundRender; GameEvents.Render += ItemsOnGroundRender; } else if (_EnabledItemsOnGround && !value) GameEvents.Render -= ItemsOnGroundRender; _EnabledItemsOnGround = value; ClosestItemOnGround = null; } }

        public static List<ItemOnGround> ItemsOnGround { get; set; }

        private static RAGE.Elements.Colshape ServerDataColshape { get; set; }

        public static T GetSharedData<T>(string key, T defaultValue = default(T)) => ServerDataColshape.GetSharedData<T>(key, defaultValue);

        private static Dictionary<string, Action<object, object>> DataActions = new Dictionary<string, Action<object, object>>();

        private static void InvokeHandler(string dataKey, object value, object oldValue = null) => DataActions.GetValueOrDefault(dataKey)?.Invoke(value, oldValue);

        public static void AddDataHandler(string dataKey, Action<object, object> action)
        {
            Events.AddDataHandler(dataKey, (Entity entity, object value, object oldValue) =>
            {
                if (entity is Colshape colshape && colshape == ServerDataColshape)
                {
                    action.Invoke(value, oldValue);
                }
            });

            DataActions.Add(dataKey, action);
        }

        public static void AddDataHandler(string dataKey, Action<string, object, object> action)
        {
            Events.AddDataHandler(dataKey, (Entity entity, object value, object oldValue) =>
            {
                if (entity is Colshape colshape && colshape == ServerDataColshape)
                {
                    action.Invoke(dataKey, value, oldValue);
                }
            });
        }

        public static void Preload()
        {
            if (Preloaded)
                return;

            Preloaded = true;

            AddDataHandler("cst", (value, oldValue) =>
            {
                var of = DateTimeOffset.FromUnixTimeMilliseconds((long)value);

                ServerTime = of.DateTime;
                LocalTime = of.AddHours(-Settings.SERVER_TIME_UTC_OFFSET).LocalDateTime;
            });

            for (int i = 0; i < RAGE.Elements.Entities.Objects.All.Count; i++)
            {
                var x = RAGE.Elements.Entities.Objects.All[i];

                if (x.GetSharedData<int>("IOG", -1) >= 0)
                {
                    x.NotifyStreaming = true;
                }
            }

            for (int i = 0; i < RAGE.Elements.Entities.Colshapes.All.Count; i++)
            {
                var x = RAGE.Elements.Entities.Colshapes.All[i];

                if (x == null)
                    continue;

                if (x.HasSharedData("ServerData"))
                {
                    ServerDataColshape = x;

                    continue;
                }

                if (x.HasSharedData("Type") != true)
                    continue;

                Events.CallLocal("ExtraColshape::New", x);
            }

            AddDataHandler("Weather", (value, oldValue) =>
            {
                var weather = (WeatherTypes)(int)value;

                if (CurrentWeatherCustom != null || CurrentWeatherSpecial != null)
                    return;

                SetWeatherNow(weather);
            });

            InvokeHandler("Weather", GetSharedData<int>("Weather"), null);

            foreach (var x in Data.Locations.Business.All.Values)
            {
                var id = x.Id;
                var obj = x;

                AddDataHandler($"Business::{id}::OName", (value, oldValue) =>
                {
                    var name = (string)value;

                    obj.UpdateOwnerName(name);
                });

                InvokeHandler($"Business::{id}::OName", GetSharedData<string>($"Business::{id}::OName"), null);
            }

            foreach (var x in Data.Locations.House.All.Values)
            {
                var id = x.Id;
                var obj = x;

                AddDataHandler($"House::{id}::OName", (value, oldValue) =>
                {
                    var name = (string)value;

                    obj.UpdateOwnerName(name);
                });

                InvokeHandler($"House::{id}::OName", GetSharedData<string>($"House::{id}::OName"), null);
            }

            foreach (var x in Data.Locations.Apartments.All.Values)
            {
                var id = x.Id;
                var obj = x;

                AddDataHandler($"Apartments::{id}::OName", (value, oldValue) =>
                {
                    var name = (string)value;

                    obj.UpdateOwnerName(name);
                });

                //InvokeHandler($"Apartments::{id}::OName", GetSharedData<string>($"Apartments::{id}::OName"), null);
            }

            foreach (var x in Data.Locations.ApartmentsRoot.All.Values)
                x.UpdateTextLabel();

            foreach (var x in Data.Locations.Garage.All.Values)
            {
                var id = x.Id;
                var obj = x;

                AddDataHandler($"Garages::{id}::OName", (value, oldValue) =>
                {
                    var name = (string)value;

                    obj.UpdateOwnerName(name);
                });

                InvokeHandler($"Garages::{id}::OName", GetSharedData<string>($"Garages::{id}::OName"), null);
            }
        }

        public static async System.Threading.Tasks.Task OnMapObjectStreamIn(MapObject obj)
        {
            if (obj.IsLocal)
                return;

            if (obj.GetSharedData<int>("IOG", -1) is int iogTypeNum && iogTypeNum >= 0)
            {
                if (iogTypeNum == 0)
                {
                    obj.PlaceOnGroundProperly();

                    obj.SetCollision(false, true);

                    obj.SetData("Dist", float.MaxValue);
                }
                else if (iogTypeNum == 1)
                {
                    obj.SetData("Interactive", true);
                }

                obj.SetDisableFragDamage(true);
                obj.SetCanBeDamaged(false);
                obj.SetInvincible(true);

                obj.FreezePosition(true);

                var iogObj = ItemOnGround.GetItemOnGroundObject(obj);

                if (!ItemsOnGround.Contains(iogObj))
                    ItemsOnGround.Add(iogObj);
            }
        }

        public static async System.Threading.Tasks.Task OnMapObjectStreamOut(MapObject obj)
        {
            if (obj.IsLocal)
                return;

            if (obj.GetSharedData<int>("IOG", -1) >= 0)
            {
                var iogObj = ItemOnGround.GetItemOnGroundObject(obj);

                ItemsOnGround.Remove(iogObj);

                if (ClosestItemOnGround != null && ClosestItemOnGround.Object == obj)
                    ClosestItemOnGround = null;
            }

            obj.ResetData();
        }

        public World()
        {
            ItemOnGround.LastShowed = Sync.World.ServerTime;
            ItemOnGround.LastSent = Sync.World.ServerTime;

            Preloaded = false;

            ItemsOnGround = new List<ItemOnGround>();

            ClosestItemOnGround = null;

            _EnabledItemsOnGround = false;

            #region Events
            Events.AddDataHandler("IOG", (Entity entity, object value, object oldValue) =>
            {
                if (entity is MapObject obj)
                {
                    obj.NotifyStreaming = true;
                }
            });

            (new AsyncTask(() =>
            {
                var minDist = float.MaxValue;
                var minIdx = -1;

                for (int i = 0; i < ItemsOnGround.Count; i++)
                {
                    if (ItemsOnGround[i].Type != ItemOnGround.Types.Default || ItemsOnGround[i].Object?.Exists != true)
                        continue;

                    var dist = Vector3.Distance(Player.LocalPlayer.Position, ItemsOnGround[i].Object.GetCoords(true));

                    if (dist < minDist)
                    {
                        minDist = dist;

                        minIdx = i;
                    }

                    ItemsOnGround[i].Object.SetData("Dist", dist);
                }

                if (minIdx < 0)
                    ClosestItemOnGround = null;
                else
                    ClosestItemOnGround = ItemsOnGround[minIdx];
            }, 1000, true, 2500)).Run();
            #endregion
        }

        #region IOG Render
        private static void ItemsOnGroundRender()
        {
            float screenX = 0f, screenY = 0f;

            for (int i = 0; i < ItemsOnGround.Count; i++)
            {
                var temp = ItemsOnGround[i];

                if (temp.Type != ItemOnGround.Types.Default || temp.Object?.Exists != true)
                    continue;

                if (temp.Object.GetData<float>("Dist") > Settings.ENTITY_INTERACTION_MAX_DISTANCE_RENDER)
                {
                    if (ClosestItemOnGround == temp)
                    {
                        ClosestItemOnGround = null;

                        if (CEF.ActionBox.CurrentContext == CEF.ActionBox.Contexts.ItemOnGroundTakeRange)
                        {
                            CEF.ActionBox.Close(true);
                        }
                    }

                    continue;
                }

                if (!Utils.GetScreenCoordFromWorldCoord(temp.Object.GetCoords(true), ref screenX, ref screenY))
                    continue;

                if (!Settings.Interface.HideIOGNames)
                {
                    Utils.DrawText($"{temp.Name} (x{temp.Amount})", screenX, screenY, 255, 255, 255, 255, 0.4f, Utils.ScreenTextFontTypes.CharletComprimeColonge, true);
                }

                if (temp == ClosestItemOnGround && !Settings.Interface.HideInteractionBtn)
                    Utils.DrawText(KeyBinds.Binds[KeyBinds.Types.TakeItem].GetKeyString(), screenX, Settings.Interface.HideIOGNames ? screenY : screenY + 0.025f, 255, 0, 0, 255, 0.4f, Utils.ScreenTextFontTypes.CharletComprimeColonge, true);
            }
        }
        #endregion

        public static void SetSpecialWeather(WeatherTypes? weather)
        {
            if (weather == null)
            {
                InvokeHandler("Weather", GetSharedData<int>("Weather"), null);
            }
            else
            {
                SetWeatherNow((WeatherTypes)weather);
            }
        }

        public static void SetWeatherNow(WeatherTypes weather)
        {
            var str = weather.ToString();

            //RAGE.Game.Misc.SetWeatherTypePersist(str);
            //RAGE.Game.Misc.SetWeatherTypeNowPersist(str);
            RAGE.Game.Misc.SetWeatherTypeNow(str);
            //RAGE.Game.Misc.SetOverrideWeather(str);
        }


    }
}
