using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;

namespace BCRPServer.Properties
{
    public class SettingsProfile
    {
        [JsonProperty(PropertyName = "general")]
        public GeneralSettings General { get; private set; } = new GeneralSettings();

        [JsonProperty(PropertyName = "game")]
        public GameSettings Game { get; private set; } = new GameSettings();

        [JsonProperty(PropertyName = "dataBase")]
        public DataBaseSettings DataBase { get; private set; } = new DataBaseSettings();
        [JsonProperty(PropertyName = "web")]
        public WebSettings Web { get; private set; } = new WebSettings();

        public class GeneralSettings
        {
            [JsonProperty(PropertyName = "cultureInfo")]
            public CultureInfo CultureInfo { get; private set; } = new CultureInfo("ru-RU", false)
            {
                NumberFormat = new NumberFormatInfo()
                {
                    CurrencyDecimalSeparator = ".",
                    NumberDecimalSeparator = ".",
                },
            };

            [JsonProperty(PropertyName = "serverId")]
            public string ServerId { get; private set; } = "brp-server-1";

            [JsonProperty(PropertyName = "playerMaxLoginAttempts")]
            public byte PlayerLoginMaxAttempts { get; private set; } = 3 + 1;
            [JsonProperty(PropertyName = "playerAuthTimeoutTime")]
            public TimeSpan PlayerAuthTimeoutTime { get; private set; } = TimeSpan.FromMinutes(10);
        }

        public class GameSettings
        {
            [JsonProperty(PropertyName = "mainDimension")]
            public uint MainDimension { get; private set; } = 7;

            [JsonProperty(PropertyName = "stuffDimension")]
            public uint StuffDimension { get; private set; } = 1;

            [JsonProperty(PropertyName = "demorganDimension")]
            public uint DemorganDimension { get; private set; } = 2;

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

            public GameSettings()
            { 

            }

            [JsonProperty(PropertyName = "payDay")]
            public PayDaySettings PayDay { get; private set; } = new PayDaySettings();

            public class PayDaySettings
            {
                [JsonProperty(PropertyName = "minimalSessionTimeToReceive")]
                public TimeSpan MinimalSessionTimeToReceive { get; private set; } = TimeSpan.FromSeconds(600);
            }
        }

        public class DataBaseSettings
        {
            [JsonProperty(PropertyName = "ownDbCredentials")]
            public MySQLDbCredentials OwnDbCredentials { get; private set; } = new MySQLDbCredentials();

            public class MySQLDbCredentials
            {
                [JsonProperty(PropertyName = "host")]
                public string Host { get; private set; } = "localhost";
                [JsonProperty(PropertyName = "user")]
                public string User { get; private set; } = "root";
                [JsonProperty(PropertyName = "password")]
                public string Password { get; private set; } = "";
                [JsonProperty(PropertyName = "name")]
                public string Name { get; private set; } = "bcrp-1";
            }
        }

        public class WebSettings
        {
            [JsonProperty(PropertyName = "socketIOCredentials")]
            public SocketIOSettings SocketIOCredentials { get; private set; } = new SocketIOSettings();

            public class SocketIOSettings
            {
                [JsonProperty(PropertyName = "host")]
                public string Host { get; private set; } = "http://localhost:7777";
                [JsonProperty(PropertyName = "user")]
                public string User { get; private set; } = "brp-server-1";
                [JsonProperty(PropertyName = "password")]
                public string Password { get; private set; } = "63c209c3-3505-443a-b234-91e3046e2894";
            }
        }

        public static SettingsProfile LoadProfile(string path)
        {
            return JsonConvert.DeserializeObject<SettingsProfile>(File.ReadAllText(path));
        }

        public static void SaveProfile(SettingsProfile settProfile, string path)
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

        public static SettingsProfile GetDefault() => new SettingsProfile();
    }
}
