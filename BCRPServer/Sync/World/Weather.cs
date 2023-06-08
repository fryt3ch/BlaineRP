using GTANetworkAPI;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading;

namespace BCRPServer.Sync
{
    public static class Weather
    {
        public enum Types : byte
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

        private static Timer UpdateTimer { get; set; }

        private const int UpdateTimeout = 1000 * 60 * 30;

        // https://www.weatherapi.com/docs/weather_conditions.json
        private const string UrlFormatString = "http://api.weatherapi.com/v1/current.json?key={0}&q={1}";

        private const string APIKey = "8dd49447c2374d32a2874253230706";

        private static ChancePicker<Types> RandomWeatherChances = new ChancePicker<Types>
        (
            new ChancePicker<Types>.Item<Types>(0.25d, Types.CLEAR),
            new ChancePicker<Types>.Item<Types>(0.10d, Types.CLEARING),
            new ChancePicker<Types>.Item<Types>(0.15d, Types.EXTRASUNNY),
            new ChancePicker<Types>.Item<Types>(0.10d, Types.OVERCAST),
            new ChancePicker<Types>.Item<Types>(0.10d, Types.CLOUDS),
            new ChancePicker<Types>.Item<Types>(0.05d, Types.SMOG),
            new ChancePicker<Types>.Item<Types>(0.05d, Types.FOGGY),
            new ChancePicker<Types>.Item<Types>(0.15d, Types.RAIN),
            new ChancePicker<Types>.Item<Types>(0.05d, Types.THUNDER)
        );

        public static void StartRealWeatherSync(string[] citiesStr, bool updateNow, int currentCityIdx = 0, int maxCallsPerCity = -1)
        {
            if (UpdateTimer != null)
            {
                UpdateTimer.Dispose();
            }

            if (currentCityIdx == -1)
            {
                currentCityIdx = SRandom.NextInt32(0, citiesStr.Length);
            }

            int callsMade = 0;

            UpdateTimer = new Timer(async (obj) =>
            {
                if (currentCityIdx < 0)
                    currentCityIdx = citiesStr.Length - 1;
                else if (currentCityIdx >= citiesStr.Length)
                    currentCityIdx = 0;

                Types weatherTypeToSet;

                string cityStr = null;

                uint conditionCode = 0;

                bool success = false;

                try
                {
                    using (var httpClient = new HttpClient())
                    {
                        cityStr = citiesStr[currentCityIdx];

                        var response = await httpClient.GetAsync(string.Format(UrlFormatString, APIKey, cityStr));

                        response.EnsureSuccessStatusCode();

                        var jsonObj = JObject.Parse(await response.Content.ReadAsStringAsync());

                        conditionCode = jsonObj["current"]["condition"]["code"].ToObject<uint>();

                        if (!TryGetWeatherTypeByCode(conditionCode, out weatherTypeToSet))
                        {
                           weatherTypeToSet = RandomWeatherChances.GetNextItem(out _);

                           success = false;
                        }
                        else
                        {
                            if (maxCallsPerCity > 0)
                            {
                                callsMade++;

                                if (callsMade >= maxCallsPerCity)
                                {
                                    callsMade = 0;

                                    currentCityIdx++;
                                }
                            }

                            success = true;
                        }
                    }
                }
                catch (Exception ex)
                {
                    weatherTypeToSet = RandomWeatherChances.GetNextItem(out _);
                }

                NAPI.Task.Run(() =>
                {
                    if (success)
                    {
                        Utils.ConsoleOutput($"~Yellow~[WeatherSync]~/~ AUTO-SET WEATHER TO ~Yellow~{weatherTypeToSet.ToString()}~/~, RootCity: ~Yellow~{cityStr}~/~, RootConditionCode: ~Yellow~{conditionCode}~/~ | ~Green~SUCCESS GET BY API");
                    }
                    else
                    {
                        Utils.ConsoleOutput($"~Yellow~[WeatherSync]~/~ AUTO-SET WEATHER TO ~Yellow~{weatherTypeToSet.ToString()}~/~, RootCity: ~Yellow~{cityStr}~/~, RootConditionCode: ~Yellow~{conditionCode}~/~ | ~Red~FAILED GET BY API");
                    }

                    SetWeather(weatherTypeToSet);
                }, 0);
            }, null, updateNow ? 0 : Timeout.Infinite, UpdateTimeout);
        }

        public static void StartRandomWeatherSync(bool updateNow)
        {
            if (UpdateTimer != null)
            {
                UpdateTimer.Dispose();
            }

            UpdateTimer = new Timer(async (obj) =>
            {
                double chance;

                Types weatherTypeToSet = RandomWeatherChances.GetNextItem(out chance);

                NAPI.Task.Run(() =>
                {
                    Console.WriteLine($"[WeatherSync] AUTO-SET WEATHER TO \"{weatherTypeToSet.ToString()}\", Chance was: {chance * 100d}%");

                    SetWeather(weatherTypeToSet);
                }, 0);
            }, null, updateNow ? 0 : Timeout.Infinite, UpdateTimeout);
        }

        public static void StopAll()
        {
            if (UpdateTimer != null)
            {
                UpdateTimer.Dispose();

                UpdateTimer = null;
            }
        }

        public static void SetWeather(Types weather)
        {
            if (weather == Types.XMAS)
                NAPI.World.SetWeather("XMAS");

            Sync.World.SetSharedData("Weather", (byte)weather);
        }

        private static bool TryGetWeatherTypeByCode(uint code, out Types weatherType)
        {
            switch (code)
            {
                case 1000:
                    weatherType = Types.EXTRASUNNY;

                    return true;

                case 1003:
                case 1006:
                    weatherType = Types.CLOUDS;

                    return true;

                case 1009:
                    weatherType = Types.OVERCAST;

                    return true;

                case 1066:
                    weatherType = Types.SNOW;

                    return true;

                case 1030:
                case 1135:
                    weatherType = Types.FOGGY;

                    return true;

                case 1183:
                case 1189:
                case 1198:
                    weatherType = Types.RAIN;

                    return true;

                case 1273:
                case 1192:
                case 1195:
                    weatherType = Types.THUNDER;

                    return true;

                default:
                    weatherType = Types.CLEAR;

                    return false;
            }
        }
    }
}
