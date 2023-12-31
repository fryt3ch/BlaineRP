﻿using System.Collections.Generic;
using System.Threading;
using GTANetworkAPI;

namespace BlaineRP.Server.Game.World
{
    public partial class Service
    {
        /// <summary>Время для запроса синхронизации у Controller предмета в мс.</summary>
        public const int TimeToSync = 5000; // 5 secs
        /// <summary>Время для принятия обновленных данных выброшенных предметов в мс.</summary>
        public const int TimeToAllowUpdate = 1000;

        /// <summary>Базовый коэфициент отклонения позиции предмета от игрока</summary>
        public const float BaseOffsetCoeff = 0.5f;

        /// <summary>Таймер удаления всех предметов</summary>
        private static Timer ClearItemsTimer { get; set; }

        /// <summary>Все выброшенные предметы на сервере</summary>
        /// <value>Словарь, где ключ - UID предмета, а значение - объект класса ItemOnGround</value>
        private static Dictionary<uint, ItemOnGround> ItemsOnGround { get; set; } = new Dictionary<uint, ItemOnGround>();

        private static ColShape ServerDataColshape { get; set; }

        private static Dictionary<uint, object> RetrievableData { get; } = new Dictionary<uint, object>()
        {

        };

        public static void SetSharedData<T>(string key, T data) => ServerDataColshape.SetSharedData(key, data);

        public static T GetSharedData<T>(string key) => ServerDataColshape.GetSharedData<T>(key);

        public static void ResetSharedData(string key) => ServerDataColshape.ResetSharedData(key);

        public static void SetRetrievableData(uint key, object value)
        {
            if (!RetrievableData.TryAdd(key, value))
                RetrievableData[key] = value;
        }

        public static void SetRetrievableData(string key, object value) => SetRetrievableData(NAPI.Util.GetHashKey(key), value);

        public static T GetRetrievableData<T>(uint key, T otherwise = default(T))
        {
            var value = RetrievableData.GetValueOrDefault(key);

            if (value is T valueT)
                return valueT;

            return otherwise;
        }

        public static T GetRetrievableData<T>(string key, T otherwise = default(T)) => GetRetrievableData<T>(NAPI.Util.GetHashKey(key), otherwise);

        public static void Initialize()
        {
            ServerDataColshape = NAPI.ColShape.CreatCircleColShape(0f, 0f, 0f, Properties.Settings.Static.StuffDimension);

            SetSharedData("ServerData", true);
        }
    }
}
