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
            None = -1,
            CharacterSelection,
            CharacterCreation,
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

            TempData existing;

            if (Players.TryGetValue(player, out existing))
                existing = data;
            else
                Players.Add(player, data);
        }

        public void Remove()
        {
            if (Player == null)
                return;

            Players.Remove(Player);
        }

        public void Delete()
        {
            if (LoginCTS != null)
            {
                LoginCTS.Cancel();
            }

            Semaphore?.Dispose();
        }

        /// <summary>Сущность игрока</summary>
        public Player Player { get; set; }

        public StepTypes StepType { get; set; }

        public CancellationTokenSource LoginCTS { get; set; }
        public int LoginAttempts { get; set; }
        public string ActualToken { get; set; }

        public SemaphoreSlim Semaphore { get; set; }

        public PlayerData.Prototype[] Characters { get; set; }
        public PlayerData PlayerData { get; set; }

        public Vector3 PositionToSpawn { get; set; }
        public uint DimensionToSpawn { get; set; }

        public TempData(Player Player)
        {
            this.Player = Player;

            DimensionToSpawn = Utils.Dimensions.Main;

            StepType = StepTypes.None;

            LoginAttempts = Settings.AUTH_ATTEMPTS;

            Semaphore = new SemaphoreSlim(1, 1);

            LoginCTS = new CancellationTokenSource();

            Task.Run(async () =>
            {
                try
                {
                    await Task.Delay(Settings.AUTH_TIMEOUT_TIME, LoginCTS.Token);

                    if (!await this.WaitAsync())
                        return;

                    if (LoginCTS?.IsCancellationRequested == false)
                    {
                        LoginCTS.Cancel();
                        LoginCTS.Dispose();
                        LoginCTS = null;

                        await NAPI.Task.RunAsync(() =>
                        {
                            if (StepType < StepTypes.CharacterSelection)
                                Utils.KickSilent(Player, "Время на вход вышло!");
                        });
                    }

                    this.Release();
                }
                catch (Exception ex)
                {
                    if (LoginCTS != null)
                    {
                        LoginCTS.Cancel();
                        LoginCTS.Dispose();
                    }

                    LoginCTS = null;
                }
            });
        }

        public void ShowStartPlace()
        {
            if (PlayerData == null)
                return;

            List<StartPlaceTypes> sTypes = new List<StartPlaceTypes>() { StartPlaceTypes.Last, StartPlaceTypes.Spawn };

            if (PlayerData.LastData.Dimension != Utils.Dimensions.Main)
            {
                PlayerData.LastData.Position = Utils.DefaultSpawnPosition;
                PlayerData.LastData.Dimension = Utils.Dimensions.Main;
            }

            Player.TriggerEvent("Auth::StartPlace::Load", sTypes.SerializeToJson());
        }
    }
}
