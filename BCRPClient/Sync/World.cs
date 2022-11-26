using RAGE;
using RAGE.Elements;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.Security.Cryptography.X509Certificates;

namespace BCRPClient.Sync
{
    class World : Events.Script
    {
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
        public static MapObject ClosestItemOnGround { get; set; }

        private static bool _EnabledItemsOnGround;

        /// <summary>Включено ли взаимодействие с предметами на земле в данный момент?</summary>
        public static bool EnabledItemsOnGround { get => _EnabledItemsOnGround; set { if (!_EnabledItemsOnGround && value) { GameEvents.Render -= ItemsOnGroundRender; GameEvents.Render += ItemsOnGroundRender; } else if (_EnabledItemsOnGround && !value) GameEvents.Render -= ItemsOnGroundRender; _EnabledItemsOnGround = value; ClosestItemOnGround = null; } }

        public static List<MapObject> ItemsOnGround { get; set; }

        private static List<float> Distances { get; set; }

        private static RAGE.Elements.Colshape ServerDataColshape { get; set; }

        public static T GetSharedData<T>(string key, T defaultValue = default(T)) => ServerDataColshape.GetSharedData<T>(key, defaultValue);

        private static Dictionary<string, Action<object, object>> DataActions = new Dictionary<string, Action<object, object>>();

        private static void InvokeHandler(string dataKey, object value, object oldValue = null) => DataActions.GetValueOrDefault(dataKey)?.Invoke(value, oldValue);

        private static void AddDataHandler(string dataKey, Action<object, object> action)
        {
            Events.AddDataHandler(dataKey, (Entity entity, object value, object oldValue) =>
            {
                if (ServerDataColshape == null)
                    return;

                if (entity is Colshape colshape)
                {
                    if (colshape.IsLocal || colshape.RemoteId != ServerDataColshape.RemoteId)
                        return;

                    action.Invoke(value, oldValue);
                }
            });

            DataActions.Add(dataKey, action);
        }

        public static void Preload()
        {
            if (Preloaded)
                return;

            for (int i = 0; i < RAGE.Elements.Entities.Objects.All.Count; i++)
            {
                var x = RAGE.Elements.Entities.Objects.All[i];

                if (x.GetSharedData<bool>("IOG", false))
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

                AddDataHandler($"Business::{id}::OName", (value, oldValue) =>
                {
                    var name = (string)value;

                    var biz = Data.Locations.Business.All[id];

                    biz.UpdateOwnerName(name);
                });

                InvokeHandler($"Business::{id}::OName", GetSharedData<string>($"Business::{id}::OName"), null);
            }

            foreach (var x in Data.Locations.House.All.Values)
            {
                var id = x.Id;

                AddDataHandler($"House::{id}::OName", (value, oldValue) =>
                {
                    var name = (string)value;

                    var house = Data.Locations.House.All[id];

                    house.UpdateOwnerName(name);
                });

                InvokeHandler($"House::{id}::OName", GetSharedData<string>($"House::{id}::OName"), null);
            }

            Preloaded = true;
        }

        public World()
        {
            Preloaded = false;

            ItemsOnGround = new List<MapObject>();
            Distances = new List<float>();

            ClosestItemOnGround = null;

            _EnabledItemsOnGround = false;

            #region Events
            Events.AddDataHandler("IOG", (Entity entity, object value, object oldValue) =>
            {
                if (entity.Type != RAGE.Elements.Type.Object)
                    return;

                MapObject obj = entity as MapObject;

                obj.NotifyStreaming = true;
            });

            Events.AddDataHandler("Amount", (Entity entity, object value, object oldValue) =>
            {
                if (entity.Type != RAGE.Elements.Type.Object || !entity.HasSharedData("IOG"))
                    return;

                MapObject obj = entity as MapObject;

                obj.SetData("Amount", (int)value);
            });
            #endregion

            #region New IOG Stream
            Events.OnEntityStreamIn += (async (Entity entity) =>
            {
                if (entity is MapObject obj)
                {
                    if (obj.IsLocal || !obj.GetSharedData<bool>("IOG", false))
                        return;

                    obj.SetCanBeDamaged(false);
                    obj.SetInvincible(true);

                    obj.PlaceOnGroundProperly();

                    obj.FreezePosition(true);

                    obj.SetCollision(false, true);

                    obj.SetData("Name", Data.Items.GetName(obj.GetSharedData<string>("ID", null)));
                    obj.SetData("UID", obj.GetSharedData<int>("UID").ToUInt32());
                    obj.SetData("Amount", obj.GetSharedData<int>("Amount", -1));

                    if (obj.GetData<string>("Name") == null)
                        return;

                    if (!ItemsOnGround.Contains(obj))
                        ItemsOnGround.Add(obj);
                }
            });

            Events.OnEntityStreamOut += ((Entity entity) =>
            {
                if (entity is MapObject obj)
                {
                    if (obj.IsLocal || !obj.GetSharedData<bool>("IOG", false))
                        return;

                    obj.ResetData();

                    ItemsOnGround.Remove(obj);
                }
            });
            #endregion
        }

        #region IOG Render
        private static void ItemsOnGroundRender()
        {
            Distances.Clear();

            float minDist = Settings.ENTITY_INTERACTION_MAX_DISTANCE_RENDER;
            int minIdx = -1;

            var allItems = ItemsOnGround.ToList();

            for (int i = 0; i < allItems.Count; i++)
            {
                if (allItems[i]?.Exists != true)
                    continue;

                float dist = Vector3.Distance(Player.LocalPlayer.Position, allItems[i].GetCoords(true));

                if (dist < minDist)
                {
                    minDist = dist;
                    minIdx = i;
                }

                Distances.Add(dist);
            }

            if (minIdx == -1)
            {
                ClosestItemOnGround = null;

                return;
            }

            ClosestItemOnGround = allItems[minIdx];

            float screenX = 0, screenY = 0;

            for (int i = 0; i < allItems.Count; i++)
            {
                if (allItems[i]?.Exists != true)
                    continue;

                var temp = allItems[i];

                if (Distances[i] > Settings.ENTITY_INTERACTION_MAX_DISTANCE_RENDER)
                    continue;

                if (!Utils.GetScreenCoordFromWorldCoord(temp.GetCoords(true), ref screenX, ref screenY))
                    continue;

                if (!Settings.Interface.HideIOGNames)
                {
                    Utils.DrawText($"{temp.GetData<string>("Name")} (x{temp.GetData<int>("Amount")})", screenX, screenY, 255, 255, 255, 255, 0.4f, Utils.ScreenTextFontTypes.CharletComprimeColonge, true);
                }

                if (i == minIdx && !Settings.Interface.HideInteractionBtn)
                    Utils.DrawText(KeyBinds.Binds[KeyBinds.Types.TakeItem].GetKeyString(), screenX, screenY + 0.025f, 255, 0, 0, 255, 0.4f, Utils.ScreenTextFontTypes.CharletComprimeColonge, true);
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
