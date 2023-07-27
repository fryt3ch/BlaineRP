using System;
using System.Net.Http;
using System.Threading;
using BlaineRP.Server.UtilsT;
using GTANetworkAPI;
using Newtonsoft.Json.Linq;

namespace BlaineRP.Server.Game.World
{
    public static class Weather
    {
        private static Timer UpdateTimer { get; set; }

        private const int UpdateTimeout = 1000 * 60 * 30;

        // https://www.weatherapi.com/docs/weather_conditions.json
        private const string UrlFormatString = "http://api.weatherapi.com/v1/current.json?key={0}&q={1}";

        private const string APIKey = "8dd49447c2374d32a2874253230706";

        private static ChancePicker<WeatherType> RandomWeatherChances = new ChancePicker<WeatherType>
        (
            new ChancePicker<WeatherType>.Item<WeatherType>(0.25d, WeatherType.CLEAR),
            new ChancePicker<WeatherType>.Item<WeatherType>(0.10d, WeatherType.CLEARING),
            new ChancePicker<WeatherType>.Item<WeatherType>(0.15d, WeatherType.EXTRASUNNY),
            new ChancePicker<WeatherType>.Item<WeatherType>(0.10d, WeatherType.OVERCAST),
            new ChancePicker<WeatherType>.Item<WeatherType>(0.10d, WeatherType.CLOUDS),
            new ChancePicker<WeatherType>.Item<WeatherType>(0.05d, WeatherType.SMOG),
            new ChancePicker<WeatherType>.Item<WeatherType>(0.05d, WeatherType.FOGGY),
            new ChancePicker<WeatherType>.Item<WeatherType>(0.15d, WeatherType.RAIN),
            new ChancePicker<WeatherType>.Item<WeatherType>(0.05d, WeatherType.THUNDER)
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

                WeatherType weatherTypeToSet;

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

                WeatherType weatherTypeToSet = RandomWeatherChances.GetNextItem(out chance);

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

        public static void SetWeather(WeatherType weather)
        {
            if (weather == WeatherType.XMAS)
                NAPI.World.SetWeather("XMAS");

            Service.SetSharedData("Weather", (byte)weather);
        }

        private static bool TryGetWeatherTypeByCode(uint code, out WeatherType weatherType)
        {
            switch (code)
            {
                case 1000:
                    weatherType = WeatherType.EXTRASUNNY;

                    return true;

                case 1003:
                case 1006:
                    weatherType = WeatherType.CLOUDS;

                    return true;

                case 1009:
                    weatherType = WeatherType.OVERCAST;

                    return true;

                case 1066:
                    weatherType = WeatherType.SNOW;

                    return true;

                case 1030:
                case 1135:
                    weatherType = WeatherType.FOGGY;

                    return true;

                case 1183:
                case 1189:
                case 1198:
                    weatherType = WeatherType.RAIN;

                    return true;

                case 1273:
                case 1192:
                case 1195:
                    weatherType = WeatherType.THUNDER;

                    return true;

                default:
                    weatherType = WeatherType.CLEAR;

                    return false;
            }
        }
    }
}
