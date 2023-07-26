using System;
using System.Globalization;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace BlaineRP.Client.Settings.App
{
    public class Profile
    {
        private static Profile _current;

        public static Profile Current => _current;

        [JsonProperty(PropertyName = "general")]
        public GeneralSettings General { get; private set; }

        [JsonProperty(PropertyName = "game")]
        public GameSettings Game { get; private set; }

        public static Profile LoadProfile(JObject jObject)
        {
            return jObject.ToObject<Profile>();
        }

        public static void SetCurrentProfile(Profile profile)
        {
            _current = profile;
        }

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
            public GameSettings()
            {
            }

            [ClientSync]
            [JsonProperty(PropertyName = "streamDistance")]
            public float StreamDistance { get; private set; }

            [ClientSync]
            [JsonProperty(PropertyName = "inventoryMaxWeight")]
            public float InventoryMaxWeight { get; private set; }
        }

        [AttributeUsage(AttributeTargets.Property)]
        private class ClientSyncAttribute : Attribute
        {
            public ClientSyncAttribute()
            {
            }

            public string PropertyName { get; set; }
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