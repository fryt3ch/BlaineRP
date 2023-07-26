using System.Collections.Generic;
using System.Drawing;
using BlaineRP.Client.Game.EntitiesData.Enums;
using Newtonsoft.Json.Linq;

namespace BlaineRP.Client.Settings.App
{
    public static class Static
    {
        /// <summary>Базовый FPS для выравнивания скорости работы некоторых механик</summary>
        /// <remarks>Используется, например, в тире, для обеспечения одинаковой скорости перемещения мишеней у игроков с разным FPS</remarks>
        public const float BaseFps = 185f;

        public const float FINGER_POINT_ENTITY_MAX_DISTANCE = 10f;

        public const int SPEEDOMETER_UPDATE_SPEED = 10;

        public const int RENTED_VEHICLE_TIME_TO_AUTODELETE = 300_000;

        public const int PHONE_SMS_MAX_LENGTH = 120;
        public const int PHONE_SMS_MIN_LENGTH = 5;

        public const byte TAXI_ORDER_MAX_WAIT_RANGE = 10;
        public const byte POLICE_CALL_MAX_WAIT_RANGE = 10;
        public const byte EMS_CALL_MAX_WAIT_RANGE = 10;

        public const double WeaponSystemWoundChance = 0.15d;

        public const bool DisableIdleCamera = true;

        public static float EntityInteractionMaxDistance = 5f;

        public static float MIN_CRUISE_CONTROL_SPEED = 0f;
        public static float MAX_CRUISE_CONTROL_SPEED = 0f;

        public static readonly Color HudColour = Color.FromArgb(255, 255, 0, 0);

        #region TO_REPLACE

        private static JObject _otherSettings;

        #endregion

        public static uint MainDimension => GetOther<uint>("mainDimension");
        public static uint StuffDimension => GetOther<uint>("stuffDimension");
        public static int PlayerMaxHealth => GetOther<int>("playerMaxHealth");

        public static Dictionary<SkillTypes, int> PlayerMaxSkills => GetOther<Dictionary<SkillTypes, int>>("playerMaxSkills");

        public static T GetOther<T>(string key)
        {
            return _otherSettings.GetValue(key).ToObject<T>();
        }
    }
}