using System;
using System.Collections.Generic;
using System.Linq;
using BlaineRP.Client.Extensions.RAGE.Elements;
using BlaineRP.Client.Game.Input.Enums;
using BlaineRP.Client.Game.UI.CEF;
using BlaineRP.Client.Utils;
using BlaineRP.Client.Utils.Game;
using RAGE;
using RAGE.Elements;

namespace BlaineRP.Client.Game.World
{
    [Script(int.MaxValue)]
    public partial class Core
    {
        private static bool _enabledItemsOnGround;

        private static Dictionary<string, Action<object, object>> DataActions = new Dictionary<string, Action<object, object>>();

        public Core()
        {
            Events.AddDataHandler("IOG",
                (Entity entity, object value, object oldValue) =>
                {
                    if (entity is MapObject obj)
                        obj.NotifyStreaming = true;
                }
            );

            var closestIogTask = new AsyncTask(() =>
                {
                    float minDist = float.MaxValue;
                    int minIdx = -1;

                    for (var i = 0; i < ItemsOnGround.Count; i++)
                    {
                        if (ItemsOnGround[i].Type != ItemOnGround.Types.Default || ItemsOnGround[i].Object?.Exists != true)
                            continue;

                        float dist = Vector3.Distance(Player.LocalPlayer.Position, ItemsOnGround[i].Object.GetCoords(true));

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
                },
                1_000,
                true,
                0
            );

            closestIogTask.Run();
        }

        public static DateTime ServerTime { get; private set; } = DateTime.UtcNow;

        public static DateTime LocalTime { get; private set; } = DateTime.Now;

        public static long ServerTimestampMilliseconds => GetSharedData<long>("cst");

        public static bool Preloaded { get; private set; }

        public static WeatherType CurrentWeatherServer => (WeatherType)GetSharedData<int>("Weather");

        public static WeatherType? CurrentWeatherCustom { get; set; }

        public static WeatherType? CurrentWeatherSpecial { get; set; }

        /// <summary>Ближайший к игроку предмет на земле</summary>
        public static ItemOnGround ClosestItemOnGround { get; set; }

        /// <summary>Включено ли взаимодействие с предметами на земле в данный момент?</summary>
        public static bool EnabledItemsOnGround
        {
            get => _enabledItemsOnGround;
            set
            {
                if (!_enabledItemsOnGround && value)
                {
                    Main.Render -= ItemsOnGroundRender;
                    Main.Render += ItemsOnGroundRender;
                }
                else if (_enabledItemsOnGround && !value)
                {
                    Main.Render -= ItemsOnGroundRender;
                }

                _enabledItemsOnGround = value;
                ClosestItemOnGround = null;
            }
        }

        public static List<ItemOnGround> ItemsOnGround { get; set; } = new List<ItemOnGround>();

        private static Colshape ServerDataColshape { get; set; }

        public static T GetSharedData<T>(string key, T defaultValue = default)
        {
            return ServerDataColshape.GetSharedData<T>(key, defaultValue);
        }

        public static async System.Threading.Tasks.Task<T> GetRetrievableData<T>(string key, T defaultValue = default)
        {
            object obj = await Events.CallRemoteProc("SW::GRD", RAGE.Util.Joaat.Hash(key));

            if (obj is T objT)
                return objT;

            return defaultValue;
        }

        public static void InvokeHandler(string dataKey, object value, object oldValue = null)
        {
            DataActions.GetValueOrDefault(dataKey)?.Invoke(value, oldValue);
        }

        public static void AddDataHandler(string dataKey, Action<object, object> action)
        {
            Events.AddDataHandler(dataKey,
                (Entity entity, object value, object oldValue) =>
                {
                    if (entity is Colshape colshape && colshape == ServerDataColshape)
                        action.Invoke(value, oldValue);
                }
            );

            DataActions.Add(dataKey, action);
        }

        public static void AddDataHandler(string dataKey, Action<string, object, object> action)
        {
            Events.AddDataHandler(dataKey,
                (Entity entity, object value, object oldValue) =>
                {
                    if (entity is Colshape colshape && colshape == ServerDataColshape)
                        action.Invoke(dataKey, value, oldValue);
                }
            );
        }

        public static void LoadServerDataColshape()
        {
            ServerDataColshape = Entities.Colshapes.All.Where(x => x?.HasSharedData("ServerData") == true).FirstOrDefault();
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

        private static void ItemsOnGroundRender()
        {
            float screenX = 0f, screenY = 0f;

            for (var i = 0; i < ItemsOnGround.Count; i++)
            {
                ItemOnGround temp = ItemsOnGround[i];

                if (temp.Type != ItemOnGround.Types.Default || temp.Object?.Exists != true)
                    continue;

                if (temp.Object.GetData<float>("Dist") > Settings.App.Static.EntityInteractionMaxDistance)
                {
                    if (ClosestItemOnGround == temp)
                    {
                        ClosestItemOnGround = null;

                        if (ActionBox.CurrentContextStr == "ItemOnGroundTakeRange")
                            ActionBox.Close(true);
                    }

                    continue;
                }

                if (!Graphics.GetScreenCoordFromWorldCoord(temp.Object.GetCoords(true), ref screenX, ref screenY))
                    continue;

                if (!Settings.User.Interface.HideIOGNames)
                    Graphics.DrawText($"{temp.Name} (x{temp.Amount})", screenX, screenY, 255, 255, 255, 255, 0.4f, RAGE.Game.Font.ChaletComprimeCologne, true);

                if (temp == ClosestItemOnGround && !Settings.User.Interface.HideInteractionBtn)
                    Graphics.DrawText(Input.Core.Get(BindTypes.TakeItem).GetKeyString(),
                        screenX,
                        Settings.User.Interface.HideIOGNames ? screenY : screenY + 0.025f,
                        255,
                        0,
                        0,
                        255,
                        0.4f,
                        RAGE.Game.Font.ChaletComprimeCologne,
                        true
                    );
            }
        }

        public static void SetSpecialWeather(WeatherType? weather)
        {
            if (weather == null)
                InvokeHandler("Weather", GetSharedData<int>("Weather"), null);
            else
                SetWeatherNow((WeatherType)weather);
        }

        public static void SetWeatherNow(WeatherType weather)
        {
            var str = weather.ToString();

            //RAGE.Game.Misc.SetWeatherTypePersist(str);
            //RAGE.Game.Misc.SetWeatherTypeNowPersist(str);
            RAGE.Game.Misc.SetWeatherTypeNow(str);
            //RAGE.Game.Misc.SetOverrideWeather(str);
        }
    }
}