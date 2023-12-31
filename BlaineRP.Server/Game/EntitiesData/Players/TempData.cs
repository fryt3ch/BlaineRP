﻿using System.Collections.Generic;
using System.Threading;
using BlaineRP.Server.UtilsT;
using GTANetworkAPI;

namespace BlaineRP.Server.Game.EntitiesData.Players
{
    public partial class TempData
    {
        public static Dictionary<Player, TempData> Players = new Dictionary<Player, TempData>();

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

        public PlayerInfo[] Characters { get; set; }

        public AccountData AccountData { get; set; }

        public PlayerData PlayerData { get; set; }

        public Vector4 PositionToSpawn { get; set; }

        public uint DimensionToSpawn { get; set; }

        public bool BlockRemoteCalls { get; set; }

        public byte SpamCounter { get; set; }

        public string RegistrationConfirmationHash { get => Player.GetData<string>("TData::Reg::ConfirmHash"); set { if (value == null) Player.ResetData("TData::Reg::ConfirmHash"); else Player.SetData("TData::Reg::ConfirmHash", value); } }

        public TempData(Player Player)
        {
            this.Player = Player;

            SpamCounter = 0;

            BlockRemoteCalls = true;

            DimensionToSpawn = Properties.Settings.Static.MainDimension;

            StepType = StepTypes.None;

            LoginAttempts = Properties.Settings.Profile.Current.General.PlayerLoginMaxAttempts;

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
                        Utils.Kick(Player, "Время на вход вышло!");
                });
            }, null, (int)Properties.Settings.Profile.Current.General.PlayerAuthTimeoutTime.TotalMilliseconds, Timeout.Infinite);

            Characters = new PlayerInfo[3];
        }

        public void ShowStartPlace()
        {
            if (PlayerData == null)
                return;

            var sTypes = new HashSet<StartPlaceTypes>() { StartPlaceTypes.SpawnBlaineCounty };

            if (PlayerData.Info.LosSantosAllowed)
            {
                sTypes.Add(StartPlaceTypes.SpawnLosSantos);
            }

            if (PlayerData.LastData.Dimension != Properties.Settings.Static.MainDimension)
            {
                PlayerData.LastData.Position.Position = Utils.DefaultSpawnPosition;
                PlayerData.LastData.Dimension = Properties.Settings.Static.MainDimension;
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

            if (PlayerData.Fraction != Game.Fractions.FractionType.None)
            {
                var fData = Game.Fractions.Fraction.Get(PlayerData.Fraction);

                if (fData != null)
                {
                    sTypes.Add(StartPlaceTypes.Fraction);

                    if (fData.SpawnPositions.Length > 1)
                    {
                        sTypes.Add(StartPlaceTypes.FractionBranch);
                    }
                }
            }

            Player.TriggerEvent("Auth::StartPlace::Load", sTypes);
        }
    }
}