using GTANetworkAPI;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace BCRPServer
{
    public class TempData
    {
        private static Dictionary<Player, TempData> Players = new Dictionary<Player, TempData>();

        public enum StepTypes
        {
            /// <summary>Регистрация/вход</summary>
            None = -1,
            /// <summary>Выбор персонажа</summary>
            CharacterSelection,
            /// <summary>Создание персонажа</summary>
            CharacterCreation,
            /// <summary>Выбор места спавна</summary>
            StartPlace,
        }

        public enum StartPlaceTypes
        {
            /// <summary>Последнее место на сервере</summary>
            Last = 0,
            /// <summary>Спавн</summary>
            Spawn,
            /// <summary>Дом</summary>
            House,
            /// <summary>Квартира</summary>
            Apartments,
            /// <summary>Фракция</summary>
            Fraction,
            /// <summary>Организация</summary>
            Organisation,
        }

        /// <summary>Получить TempData игрока</summary>
        /// <returns>Объект класса TempData если существует, иначе - null</returns>
        public static TempData Get(Player player)
        {
            if (player == null)
                return null;

            return Players.GetValueOrDefault(player);
        }

        /// <summary>Назначить объект класса TempData игроку</summary>
        public static void Set(Player player, TempData data)
        {
            if (player == null)
                return;

            if (Players.ContainsKey(player))
            {
                Players[player] = data;
            }
            else
            {
                Players.Add(player, data);
            }
        }

        public void Delete()
        {
            if (Player != null)
            {
                Players.Remove(Player);
            }

            if (AuthTimer != null)
            {
                AuthTimer.Dispose();

                AuthTimer = null;
            }
        }

        public Player Player { get; set; }

        public StepTypes StepType { get; set; }

        public Timer AuthTimer { get; set; }

        public int LoginAttempts { get; set; }

        public string ActualToken { get; set; }

        public PlayerData.PlayerInfo[] Characters { get; set; }

        public AccountData AccountData { get; set; }

        public PlayerData PlayerData { get; set; }

        public Vector3 PositionToSpawn { get; set; }

        public uint DimensionToSpawn { get; set; }

        public bool BlockRemoteCalls { get; set; }

        public byte SpamCounter { get; set; }

        public TempData(Player Player)
        {
            this.Player = Player;

            SpamCounter = 0;

            BlockRemoteCalls = true;

            DimensionToSpawn = Utils.Dimensions.Main;

            StepType = StepTypes.None;

            LoginAttempts = Settings.AUTH_ATTEMPTS;

            AuthTimer = new Timer((obj) =>
            {
                NAPI.Task.Run(() =>
                {
                    if (AuthTimer != null)
                    {
                        AuthTimer.Dispose();

                        AuthTimer = null;
                    }

                    if (Player?.Exists != true)
                        return;

                    if (StepType < StepTypes.CharacterSelection)
                        Utils.KickSilent(Player, "Время на вход вышло!");
                });
            }, null, Settings.AUTH_TIMEOUT_TIME, Timeout.Infinite);

            Characters = new PlayerData.PlayerInfo[3];
        }

        public void ShowStartPlace()
        {
            if (PlayerData == null)
                return;

            var sTypes = new List<StartPlaceTypes>() { StartPlaceTypes.Spawn };

            if (PlayerData.LastData.Dimension != Utils.Dimensions.Main)
            {
                PlayerData.LastData.Position.Position = Utils.DefaultSpawnPosition;
                PlayerData.LastData.Dimension = Utils.Dimensions.Main;
            }
            else
            {
                sTypes.Add(StartPlaceTypes.Last);
            }

            if (PlayerData.OwnedHouses.Count > 0)
            {
                sTypes.Add(StartPlaceTypes.House);
            }

            if (PlayerData.OwnedApartments.Count > 0)
            {
                sTypes.Add(StartPlaceTypes.Apartments);
            }

            if (PlayerData.SettledHouseBase != null)
            {
                sTypes.Add(PlayerData.SettledHouseBase.Type == Game.Estates.HouseBase.Types.House ? StartPlaceTypes.House : StartPlaceTypes.Apartments);
            }

            if (PlayerData.Fraction != Game.Fractions.Types.None)
            {
                var fData = Game.Fractions.Fraction.Get(PlayerData.Fraction);

                if (fData != null)
                    sTypes.Add(StartPlaceTypes.Fraction);
            }

            Player.TriggerEvent("Auth::StartPlace::Load", sTypes);
        }
    }
}