using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System;
using System.Globalization;

namespace BCRPClient.Settings.App
{
    public class Profile
    {
        private static Profile _current;

        public static Profile Current => _current;

        [JsonProperty(PropertyName = "general")]
        public GeneralSettings General { get; private set; }

        [JsonProperty(PropertyName = "game")]
        public GameSettings Game { get; private set; }

        [SettingsSection]
        public class GeneralSettings
        {
            [ClientSync]
            [JsonProperty(PropertyName = "cultureInfo")]
            public CultureInfo CultureInfo { get; private set; }

            [ClientSync]
            [JsonProperty(PropertyName = "serverId")]
            public string ServerId { get; private set; }

            [ClientSync]
            [JsonProperty(PropertyName = "timeUtcOffset")]
            public TimeSpan TimeUtcOffset { get; private set; }
        }

        [SettingsSection]
        public class GameSettings
        {
            [ClientSync]
            [JsonProperty(PropertyName = "mainDimension")]
            public uint MainDimension { get; private set; }

            [ClientSync]
            [JsonProperty(PropertyName = "stuffDimension")]
            public uint StuffDimension { get; private set; }

            [ClientSync]
            [JsonProperty(PropertyName = "streamDistance")]
            public float StreamDistance { get; private set; }

            [ClientSync]
            [JsonProperty(PropertyName = "inventoryMaxWeight")]
            public float InventoryMaxWeight { get; private set; }

            public GameSettings()
            {

            }
        }

        public static Profile LoadProfile(JObject jObject)
        {
            return jObject.ToObject<Profile>();
        }

        public static void SetCurrentProfile(Profile profile)
        {
            _current = profile;
        }

        [AttributeUsage(AttributeTargets.Property)]
        private class ClientSyncAttribute : Attribute
        {
            public string PropertyName { get; set; }

            public ClientSyncAttribute()
            {

            }
        }

        [AttributeUsage(AttributeTargets.Class)]
        private class SettingsSectionAttribute : Attribute
        {
            public SettingsSectionAttribute()
            {

            }
        }
    }
}
