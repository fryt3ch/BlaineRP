﻿using GTANetworkAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BCRPServer.Events.Players
{
    class Auth : Script
    {
        #region Registration Attempt
        [RemoteEvent("Auth::OnRegistrationAttempt")]
        private static async Task OnRegistrationAttempt(Player player, string login, string mail, string password)
        {
            var sRes = player.CheckSpamAttackTemp();

            if (sRes.IsSpammer)
                return;

            var tData = sRes.Data;

            if ((tData.StepType != TempData.StepTypes.None) || tData.AccountData != null)
                return;

            if (password == null || login == null || password == null)
                return;

            mail = mail.ToLower();

            if (!Utils.IsMailValid(mail) || !Utils.IsLoginValid(login) || !Utils.IsPasswordValid(password))
                return;

            tData.BlockRemoteCalls = true;

            var hwid = player.Serial;
            var scid = player.SocialClubId.ToString();
            var ip = player.Address;

            await MySQL.WaitGlobal();

            await Task.Run(async () =>
            {
                password = Utils.ToMD5(password);

                var aRes = await MySQL.AccountAdd(scid, hwid, login, password, mail, Utils.GetCurrentTime(), ip);

                NAPI.Task.Run(() =>
                {
                    if (player?.Exists != true)
                        return;

                    tData.BlockRemoteCalls = false;

                    if (aRes.Result == MySQL.AuthResults.RegOk)
                    {
                        tData.LoginCTS?.Cancel();

                        tData.AccountData = aRes.AccountData;

                        tData.ActualToken = GenerateToken(tData.AccountData, hwid);

                        tData.Characters = new PlayerData.PlayerInfo[3];

                        var cData = new object[3];

                        for (int i = 0; i < cData.Length; i++)
                            cData[i] = new object[14];

                        player.TriggerEvent("Auth::CloseRegistrationPage", true);
                        player.TriggerEvent("Auth::ShowCharacterChoosePage", true, tData.AccountData.Login, tData.AccountData.RegistrationDate.ToString("d"), tData.AccountData.BCoins, cData.SerializeToJson());

                        player.TriggerEvent("Auth::SaveLogin", tData.AccountData.Login);
                        player.TriggerEvent("Auth::SaveToken", tData.ActualToken);

                        tData.StepType = TempData.StepTypes.CharacterSelection;
                    }
                    else
                    {
                        if (aRes.Result == MySQL.AuthResults.RegMailNotFree)
                        {
                            player.Notify("Auth::MailNotFree");

                            return;
                        }
                        else if (aRes.Result == MySQL.AuthResults.RegLoginNotFree)
                        {
                            player.Notify("Auth::LoginNotFree");

                            return;
                        }
                    }
                });
            });

            MySQL.ReleaseGlobal();
        }
        #endregion

        #region Login Attempt
        [RemoteEvent("Auth::OnLoginAttempt")]
        private static async Task OnLoginAttempt(Player player, string login, string password)
        {
            var sRes = player.CheckSpamAttackTemp();

            if (sRes.IsSpammer)
                return;

            var tData = sRes.Data;

            if (tData.AccountData == null || (tData.StepType != TempData.StepTypes.None))
                return;

            if (login == null || password == null)
                return;

            var hwid = player.Serial;

            tData.LoginAttempts--;

            if (tData.LoginAttempts <= 0)
            {
                player.KickSilent();

                return;
            }

            if (login != tData.AccountData.Login)
            {
                player.Notify("Auth::WrongLogin", tData.LoginAttempts);

                return;
            }

            if (password == tData.ActualToken)
                password = DecryptToken(password, hwid);
            else
                password = Utils.ToMD5(password);

            if (password != tData.AccountData.Password)
            {
                player.Notify("Auth::WrongPassword", tData.LoginAttempts);

                return;
            }

            tData.LoginCTS?.Cancel();

            tData.StepType = TempData.StepTypes.CharacterSelection;

            tData.AccountData.LastIP = player.Address;

            Task.Run(() =>
            {
                tData.AccountData.UpdateOnEnter();
            });

            var cData = new object[3][];

            for (int i = 0; i < cData.Length; i++)
            {
                if (tData.Characters[i] != null)
                {
                    var lastBan = tData.Characters[i].Punishments.Where(x => x.Type == PlayerData.Punishment.Types.Ban && x.GetSecondsLeft() > 0).FirstOrDefault();

                    cData[i] = new object[14]
                    {
                            tData.Characters[i].Name + " " + tData.Characters[i].Surname,
                            tData.Characters[i].BankAccount == null ? 0 : tData.Characters[i].BankAccount.Balance,
                            tData.Characters[i].Cash,
                            tData.Characters[i].Sex,
                            tData.Characters[i].BirthDate.GetTotalYears(),
                            tData.Characters[i].Fraction == PlayerData.FractionTypes.None ? Locale.General.Auth.NoFraction : tData.Characters[i].Fraction.ToString(),
                            (tData.Characters[i].TimePlayed / 60f).ToString("0.0"),
                            tData.Characters[i].CID,
                            lastBan != null,
                            tData.Characters[i].IsOnline,
                            null,
                            null,
                            null,
                            null,
                    };

                    if (lastBan != null)
                    {
                        cData[i][10] = lastBan.Reason;
                        cData[i][11] = lastBan.AdminID.ToString();
                        cData[i][12] = lastBan.StartDate.ToString();
                        cData[i][13] = lastBan.EndDate.ToString();
                    }
                }
                else
                    cData[i] = new object[14] { null, null, null, null, null, null, null, null, null, null, null, null, null, null };
            }

            var newToken = GenerateToken(tData.AccountData, hwid);
            var jsonData = cData.SerializeToJson();

            player.TriggerEvent("Auth::CloseLoginPage", true);
            player.TriggerEvent("Auth::ShowCharacterChoosePage", true, tData.AccountData.Login, tData.AccountData.RegistrationDate.ToString("d"), tData.AccountData.BCoins, jsonData);

            player.TriggerEvent("Auth::SaveLogin", tData.AccountData.Login);
            player.TriggerEvent("Auth::SaveToken", newToken);
        }
        #endregion

        #region Character Choose Attempt
        [RemoteEvent("Auth::OnCharacterChooseAttempt")]
        private static async Task OnCharacterChooseAttempt(Player player, int num)
        {
            var sRes = player.CheckSpamAttackTemp();

            if (sRes.IsSpammer)
                return;

            var tData = sRes.Data;

            if ((tData.StepType != TempData.StepTypes.CharacterSelection) || tData.AccountData == null)
                return;

            int charNum = num - 1;

            if (charNum < 0 || charNum > 2)
                return;

            if (tData.Characters[charNum] != null) // character exists
            {
                var activeBan = tData.Characters[charNum].Punishments.Where(x => x.Type == PlayerData.Punishment.Types.Ban && x.GetSecondsLeft() > 0).FirstOrDefault();

                if (activeBan != null || tData.Characters[charNum].IsOnline)
                    return;

                var data = PlayerData.PlayerInfo.Get(tData.Characters[charNum].CID);

                if (data == null)
                    return;

                tData.StepType = TempData.StepTypes.StartPlace;

                tData.PlayerData = new PlayerData(player, data);

                MySQL.CharacterUpdateOnEnter(data);

                player.TriggerEvent("Auth::SaveLastCharacter", num);

                tData.ShowStartPlace();
            }
            else // create new character
            {
                int charactersCount = tData.Characters.Where(x => x != null).Count();

                if (charNum != charactersCount)
                    return;

                tData.StepType = TempData.StepTypes.CharacterCreation;

                player.TriggerEvent("Auth::CloseCharacterChoosePage", false);

                CharacterCreation.StartNew(player);
            }
        }
        #endregion

        [RemoteEvent("Auth::StartPlace")]
        private static void StartPlaceSelect(Player player, bool start, int type)
        {
            var sRes = player.CheckSpamAttackTemp();

            if (sRes.IsSpammer)
                return;

            var tData = sRes.Data;

            if (tData.StepType != TempData.StepTypes.StartPlace)
                return;

            if (start)
            {
                if (tData.PositionToSpawn == null)
                    return;

                var pData = tData.PlayerData;

                pData.LastData.Position.Position = tData.PositionToSpawn;
                pData.LastData.Dimension = tData.DimensionToSpawn;

                pData.AccountData = tData.AccountData;

                tData.Delete();

                player.SetMainData(pData);

                pData.SetReady();
            }
            else
            {
                if (!Enum.IsDefined(typeof(TempData.StartPlaceTypes), type))
                    return;

                var sType = (TempData.StartPlaceTypes)type;

                if (sType == TempData.StartPlaceTypes.Last)
                {
                    tData.PositionToSpawn = tData.PlayerData.LastData.Position.Position;
                    tData.DimensionToSpawn = tData.PlayerData.LastData.Dimension;
                }
                else if (sType == TempData.StartPlaceTypes.Spawn)
                {
                    tData.PositionToSpawn = Utils.DefaultSpawnPosition;
                    tData.DimensionToSpawn = Utils.Dimensions.Main;
                }
                else if (sType == TempData.StartPlaceTypes.House)
                {
                    if (tData.PlayerData.OwnedHouses.Count > 0)
                    {
                        tData.PositionToSpawn = tData.PlayerData.OwnedHouses[0].PositionParams.Position;
                        tData.DimensionToSpawn = Utils.Dimensions.Main;
                    }
                    else
                        return;
                }
                else
                    return;

                player.Teleport(tData.PositionToSpawn, true, Utils.GetPrivateDimension(player));

                player.SkyCameraMove(Additional.SkyCamera.SwitchTypes.Move, false, "Auth::StartPlace::Allow", type);
            }
        }

        #region Stuff
        public static string GenerateToken(AccountData adata, string hwid) => Utils.EncryptString(adata.Password, Utils.ToMD5(hwid));

        private static string DecryptToken(string token, string hwid) => Utils.DecryptString(token, Utils.ToMD5(hwid));
        #endregion
    }
}