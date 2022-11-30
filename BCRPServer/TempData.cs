using GTANetworkAPI;
using Org.BouncyCastle.Asn1.Mozilla;
using System;
using System.Collections.Generic;
using System.Text;
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

            if (LoginCTS != null)
            {
                LoginCTS.Cancel();

                LoginCTS = null;
            }
        }

        public Player Player { get; set; }

        public StepTypes StepType { get; set; }

        public CancellationTokenSource LoginCTS { get; set; }

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

            LoginCTS = new CancellationTokenSource();

            Characters = new PlayerData.PlayerInfo[3];

            Task.Run(async () =>
            {
                try
                {
                    await Task.Delay(Settings.AUTH_TIMEOUT_TIME, LoginCTS.Token);

                    NAPI.Task.Run(() =>
                    {
                        if (Player?.Exists != true)
                            return;

                        if (StepType < StepTypes.CharacterSelection)
                            Utils.KickSilent(Player, "Время на вход вышло!");
                    });
                }
                catch (Exception ex)
                {

                }
                finally
                {
                    if (LoginCTS != null)
                    {
                        LoginCTS.Cancel();

                        LoginCTS = null;
                    }
                }
            });
        }

        public void ShowStartPlace()
        {
            if (PlayerData == null)
                return;

            List<StartPlaceTypes> sTypes = new List<StartPlaceTypes>() { StartPlaceTypes.Last };

            if (PlayerData.LastData.Dimension != Utils.Dimensions.Main)
            {
                PlayerData.LastData.Position = Utils.DefaultSpawnPosition;
                PlayerData.LastData.Dimension = Utils.Dimensions.Main;
            }

            if (PlayerData.OwnedHouses.Count > 0)
            {
                sTypes.Add(StartPlaceTypes.House);
            }

            sTypes.Add(StartPlaceTypes.Spawn);

            Player.TriggerEvent("Auth::StartPlace::Load", sTypes.SerializeToJson());
        }
    }
}
