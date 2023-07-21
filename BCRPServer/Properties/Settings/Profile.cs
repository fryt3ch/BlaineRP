using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;

namespace BCRPServer.Properties.Settings
{
    public class Profile
    {
        private static Profile _current;

        public static Profile Current => _current;

        [JsonProperty(PropertyName = "general")]
        public GeneralSettings General { get; private set; } = new GeneralSettings();

        [JsonProperty(PropertyName = "game")]
        public GameSettings Game { get; private set; } = new GameSettings();

        [JsonProperty(PropertyName = "dataBase")]
        public DataBaseSettings DataBase { get; private set; } = new DataBaseSettings();

        [JsonProperty(PropertyName = "web")]
        public WebSettings Web { get; private set; } = new WebSettings();

        [SettingsSection]
        public class GeneralSettings
{
            [ClientSync]
            [JsonProperty(PropertyName = "cultureInfo")]
            public CultureInfo CultureInfo { get; private set; } = new CultureInfo("ru-RU", false)
            {
                NumberFormat = new NumberFormatInfo()
                {
                    CurrencyDecimalSeparator = ".",
                    NumberDecimalSeparator = ".",
                },
            };

            [ClientSync]
            [JsonProperty(PropertyName = "serverId")]
            public string ServerId { get; private set; } = "brp-server-1";

            [JsonProperty(PropertyName = "playerMaxLoginAttempts")]
            public byte PlayerLoginMaxAttempts { get; private set; } = 3 + 1;

            [JsonProperty(PropertyName = "playerAuthTimeoutTime")]
            public TimeSpan PlayerAuthTimeoutTime { get; private set; } = TimeSpan.FromMinutes(10);

            [ClientSync]
            [JsonProperty(PropertyName = "timeUtcOffset")]
            public TimeSpan TimeUtcOffset { get; private set; } = TimeSpan.FromHours(+3);
        }

        [SettingsSection]
        public class GameSettings
        {
            [ClientSync]
            [JsonProperty(PropertyName = "streamDistance")]
            public float StreamDistance { get; private set; } = 300f;

            [JsonProperty(PropertyName = "playerPrivateDimensionBaseOffset")]
            public uint PlayerPrivateDimensionBaseOffset { get; private set; } = 1_000;

            [JsonProperty(PropertyName = "houseDimensionBaseOffset")]
            public uint HouseDimensionBaseOffset { get; private set; } = 10_000;

            [JsonProperty(PropertyName = "apartmentsDimensionBaseOffset")]
            public uint ApartmentsDimensionBaseOffset { get; private set; } = 20_000;

            [JsonProperty(PropertyName = "apartmentsRootDimensionBaseOffset")]
            public uint ApartmentsRootDimensionBaseOffset { get; private set; } = 30_000;

            [JsonProperty(PropertyName = "garageDimensionBaseOffset")]
            public uint GarageDimensionBaseOffset { get; private set; } = 50_000;

            [JsonProperty(PropertyName = "CIDBaseOffset")]
            public uint CIDBaseOffset { get; private set; } = 3_000;

            [JsonProperty(PropertyName = "VIDBaseOffset")]
            public uint VIDBaseOffset { get; private set; } = 100_000;

            [JsonProperty(PropertyName = "knockedDropWeaponsEnabled")]
            public bool KnockedDropWeaponsEnabled { get; private set; } = true;

            [JsonProperty(PropertyName = "knockedDropAmmoMaxAmount")]
            public ushort KnockedDropAmmoMaxAmount { get; private set; } = 250;

            [JsonProperty(PropertyName = "knockedDropAmmoTotalPercentage")]
            public double KnockedDropAmmoTotalPercentage { get; private set; } = 0.5d;

            [JsonProperty(PropertyName = "payDayMinimalSessionTimeToReceive")]
            public TimeSpan PayDayMinimalSessionTimeToReceive { get; private set; } = TimeSpan.FromSeconds(600);

            [ClientSync]
            [JsonProperty(PropertyName = "inventoryMaxWeight")]
            public float InventoryMaxWeight { get; private set; } = 15f;

            public GameSettings()
            {

            }
        }

        [SettingsSection]
        public class DataBaseSettings
        {
            [JsonProperty(PropertyName = "ownDbHost")]
            public string OwnDbHost { get; private set; } = "localhost";

            [JsonProperty(PropertyName = "ownDbUser")]
            public string OwnDbUser { get; private set; } = "root";

            [JsonProperty(PropertyName = "ownDbPassword")]
            public string OwnDbPassword { get; private set; } = "";

            [JsonProperty(PropertyName = "ownDbName")]
            public string OwnDbName { get; private set; } = "bcrp-1";
        }

        [SettingsSection]
        public class WebSettings
        {
            [JsonProperty(PropertyName = "socketIOHost")]
            public string SocketIOHost { get; private set; } = "http://localhost:7777";

            [JsonProperty(PropertyName = "socketIOUser")]
            public string SocketIOUser { get; private set; } = "brp-server-1";

            [JsonProperty(PropertyName = "socketIOPassword")]
            public string SocketIOPassword { get; private set; } = "63c209c3-3505-443a-b234-91e3046e2894";
        }

        public static Profile LoadProfile(string path)
        {
            return JsonConvert.DeserializeObject<Profile>(File.ReadAllText(path));
        }

        public static void SaveProfile(Profile settProfile, string path)
        {
            using (FileStream fs = File.Open(path, FileMode.OpenOrCreate))
            {
                using (StreamWriter sw = new StreamWriter(fs))
                {
                    using (JsonTextWriter jw = new JsonTextWriter(sw))
                    {
                        jw.Formatting = Formatting.Indented;
                        jw.IndentChar = ' ';
                        jw.Indentation = 4;

                        JObject.FromObject(settProfile).WriteTo(jw);
                    }
                }
            }
        }

        public static Profile GetDefault() => new Profile();

        public static JObject GetClientsideSettings(Profile profile)
        {
            JObject jObj = new JObject();

            foreach (var x in profile.GetType().GetProperties())
            {
                var settSectionAttr = x.PropertyType.GetCustomAttribute<SettingsSectionAttribute>();

                if (settSectionAttr == null)
                    continue;

                var jsonAttrClass = x.GetCustomAttribute<JsonPropertyAttribute>();

                if (jsonAttrClass == null)
                    continue;

                var dict = new Dictionary<string, object>();

                foreach (var y in x.PropertyType.GetProperties())
                {
                    var clientSyncAttr = y.GetCustomAttribute<ClientSyncAttribute>();

                    if (clientSyncAttr == null)
                        continue;

                    var jsonAttr = y.GetCustomAttribute<JsonPropertyAttribute>();

                    if (jsonAttr == null)
                        continue;

                    var value = y.GetValue(x.GetValue(profile, null), null);

                    if (!dict.TryAdd(jsonAttr.PropertyName, value))
                        dict[jsonAttr.PropertyName] = value;

                }

                if (dict.Count > 0)
                {
                    jObj[jsonAttrClass.PropertyName] = JObject.FromObject(dict);
                }
            }

            //Console.WriteLine(jObj.SerializeToJson());

            return jObj;
        }

        public JObject GetClientsideSettings() => GetClientsideSettings(this);


        public static void SetCurrentProfile(Profile profile)
        {
            _current = profile;
        }

        [AttributeUsage(AttributeTargets.Property)]
        private class ClientSyncAttribute : Attribute
        {
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
