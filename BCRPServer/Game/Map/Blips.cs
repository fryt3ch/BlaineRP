using GTANetworkAPI;
using System.Collections.Generic;

namespace BCRPServer.Game.Map
{
    public class Blips
    {
        public static List<GTANetworkAPI.Blip> BlipList = new List<GTANetworkAPI.Blip>();

        public static int LoadAll()
        {
            // Квесты (блипы будут завадаваться на клиенте)
            BlipList.Add(NAPI.Blip.CreateBlip(792, new Vector3(-749.78f, 5818.21f, 18.9f), 1, 5, name: "Рэймонд", shortRange: true));

            #region Customs
            BlipList.Add(NAPI.Blip.CreateBlip(479, new Vector3(-3018.627f, 1665f, 20f), 1, 29, name: "Таможня", shortRange: true));
            BlipList.Add(NAPI.Blip.CreateBlip(479, new Vector3(-1503.909f, 1665f, 20f), 1, 29, name: "Таможня", shortRange: true));
            BlipList.Add(NAPI.Blip.CreateBlip(479, new Vector3(1975.948f, 1655f, 20f), 1, 29, name: "Таможня", shortRange: true));
            BlipList.Add(NAPI.Blip.CreateBlip(479, new Vector3(1729.99f, 1655f, 20f), 1, 29, name: "Таможня", shortRange: true));
            #endregion

            #region Block-posts
            BlipList.Add(NAPI.Blip.CreateBlip(163, new Vector3(-2605.542f, 1655f, 20f), 1, 1, name: "Блок-пост", shortRange: true));
            BlipList.Add(NAPI.Blip.CreateBlip(163, new Vector3(-780.7f, 1655f, 20f), 1, 1, name: "Блок-пост", shortRange: true));
            BlipList.Add(NAPI.Blip.CreateBlip(163, new Vector3(138.39f, 1655f, 20f), 1, 1, name: "Блок-пост", shortRange: true));
            BlipList.Add(NAPI.Blip.CreateBlip(163, new Vector3(1054.1f, 1655f, 20f), 1, 1, name: "Блок-пост", shortRange: true));
            BlipList.Add(NAPI.Blip.CreateBlip(163, new Vector3(1266.8f, 1655f, 20f), 1, 1, name: "Блок-пост", shortRange: true));
            BlipList.Add(NAPI.Blip.CreateBlip(163, new Vector3(1501.8f, 1655f, 20f), 1, 1, name: "Блок-пост", shortRange: true));
            BlipList.Add(NAPI.Blip.CreateBlip(163, new Vector3(2539.5f, 1655f, 20f), 1, 1, name: "Блок-пост", shortRange: true));
            #endregion

            #region Jobs
            BlipList.Add(NAPI.Blip.CreateBlip(468, new Vector3(-525.37f, 5320f, 20f), 1.5f, 25, name: "Лесопилка", shortRange: true));
            BlipList.Add(NAPI.Blip.CreateBlip(527, new Vector3(2826.65f, 2833.74f, 20f), 1, 25, name: "Карьер", shortRange: true));
            BlipList.Add(NAPI.Blip.CreateBlip(755, new Vector3(1325.16f, 4336.48f, 20f), 1, 38, name: "Порт", shortRange: true));
            BlipList.Add(NAPI.Blip.CreateBlip(566, new Vector3(1030.06f, 2362.33f, 20f), 0.75f, 1, name: "Стройка", shortRange: true));
            #endregion

            #region Pierces (fishing)
            BlipList.Add(NAPI.Blip.CreateBlip(68, new Vector3(-272.71f, 6634.79f, 20f), 1, 3, name: "Пирс", shortRange: true));
            BlipList.Add(NAPI.Blip.CreateBlip(68, new Vector3(-1609.82f, 5255.81f, 20f), 1, 3, name: "Пирс", shortRange: true));
            BlipList.Add(NAPI.Blip.CreateBlip(68, new Vector3(717f, 4101.48f, 20f), 1, 3, name: "Пирс", shortRange: true));
            BlipList.Add(NAPI.Blip.CreateBlip(68, new Vector3(1490.2f, 3919.77f, 20f), 1, 3, name: "Пирс", shortRange: true));
            BlipList.Add(NAPI.Blip.CreateBlip(68, new Vector3(3812.18f, 4464f, 20f), 1, 3, name: "Пирс", shortRange: true));
            #endregion

            #region Relax
            BlipList.Add(NAPI.Blip.CreateBlip(766, new Vector3(-462.62f, 6477.62f, 20f), 1, 61, name: "Пляж BoaBay", shortRange: true));
            BlipList.Add(NAPI.Blip.CreateBlip(766, new Vector3(204.69f, 7397.73f, 20f), 1, 61, name: "Остров Montecristo", shortRange: true));
            #endregion

            #region Airports
            BlipList.Add(NAPI.Blip.CreateBlip(582, new Vector3(1752f, 3258.5f, 20f), 1, 25, name: "Аэродром", shortRange: true));
            BlipList.Add(NAPI.Blip.CreateBlip(582, new Vector3(2138.8f, 4781.19f, 20f), 1, 25, name: "Аэродром", shortRange: true));
            #endregion

            #region Factories
            BlipList.Add(NAPI.Blip.CreateBlip(499, new Vector3(3536.9f, 3723.66f, 20f), 1, 3, name: "Химический завод", shortRange: true));
            BlipList.Add(NAPI.Blip.CreateBlip(361, new Vector3(2906.88f, 4344.87f, 20f), 1, 51, name: "Нефтезавод", shortRange: true));
            #endregion

            #region For Vehicles
            BlipList.Add(NAPI.Blip.CreateBlip(728, new Vector3(340.87f, 3569.55f, 20f), 1, 19, name: "Рынок автомобилей", shortRange: true));
            BlipList.Add(NAPI.Blip.CreateBlip(728, new Vector3(2049.33f, 3436.35f, 20f), 1, 19, name: "Рынок автомобилей", shortRange: true));
            #endregion

            #region Paleto Bay
            BlipList.Add(NAPI.Blip.CreateBlip(436, new Vector3(-371.8f, 6121.9f, 20f), 1, 1, name: "Пожарная станция", shortRange: true));
            #endregion

            #region Sandy Shores
            BlipList.Add(NAPI.Blip.CreateBlip(436, new Vector3(1697f, 3586f, 20f), 1, 1, name: "Пожарная станция", shortRange: true));
            #endregion

            return BlipList.Count;
        }
    }
}
