using BCRPServer.Game.Bank;
using GTANetworkAPI;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace BCRPServer.CEF
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

            if (!await tData.WaitAsync())
                return;

            await NAPI.Task.RunAsync(async () =>
            {
                if (player?.Exists != true)
                    return;

                if ((tData.StepType != TempData.StepTypes.None) || player.GetAccountData() != null)
                    return;

                if (password == null || login == null || password == null)
                    return;

                var hwid = player.Serial;

                mail = mail.ToLower();

                if (!Utils.IsMailValid(mail) || !Utils.IsLoginValid(login) || !Utils.IsPasswordValid(password))
                    return;

                await MySQL.WaitGlobal();

                await Task.Run(async () =>
                {
                    if (!MySQL.IsLoginFree(login))
                    {
                        NAPI.Task.RunSafe(() => player?.Notify("Auth::LoginNotFree"));

                        return;
                    }

                    if (!MySQL.IsMailFree(mail))
                    {
                        NAPI.Task.RunSafe(() => player?.Notify("Auth::MailNotFree"));

                        return;
                    }

                    password = Utils.ToMD5(password);

                    tData.LoginCTS?.Cancel();

                    tData.StepType = TempData.StepTypes.CharacterSelection;

                    await NAPI.Task.RunAsync(async () =>
                    {
                        if (player?.Exists != true)
                            return;

                        var aData = new AccountData(player)
                        {
                            SCID = player.SocialClubId.ToString(),
                            HWID = player.Serial,
                            Login = login,
                            Password = password,
                            Mail = mail,
                            RegistrationDate = Utils.GetCurrentTime(),
                            RegistrationIP = player.Address,
                            LastIP = player.Address,
                            AdminLevel = -1,
                            BCoins = 0
                        };

                        player.SetAccountData(aData);

                        aData.ID = await Task.Run(() => MySQL.AddNewAccount(aData));

                        tData.ActualToken = GenerateToken(aData, hwid);

                        tData.Characters = new PlayerData.Prototype[3];

                        var cData = new object[3];

                        for (int i = 0; i < cData.Length; i++)
                            cData[i] = new object[14];

                        NAPI.Task.RunSafe(() =>
                        {
                            if (player?.Exists != true)
                                return;

                            player.TriggerEvent("Auth::CloseRegistrationPage", true);
                            player.TriggerEvent("Auth::ShowCharacterChoosePage", true, aData.Login, aData.RegistrationDate.ToString("d"), aData.BCoins, cData.SerializeToJson());

                            player.TriggerEvent("Auth::SaveLogin", aData.Login);
                            player.TriggerEvent("Auth::SaveToken", tData.ActualToken);
                        });
                    });
                });

                MySQL.ReleaseGlobal();
            });

            tData.Release();
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

            if (!await tData.WaitAsync())
                return;

            await NAPI.Task.RunAsync(async () =>
            {
                if (player?.Exists != true)
                    return;

                var aData = player.GetAccountData();

                if (aData == null || (tData.StepType != TempData.StepTypes.None))
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

                if (login != aData.Login)
                {
                    player.Notify("Auth::WrongLogin", tData.LoginAttempts);

                    return;
                }

                if (password == tData.ActualToken)
                    password = DecryptToken(password, hwid);
                else
                    password = Utils.ToMD5(password);

                if (password != aData.Password)
                {
                    player.Notify("Auth::WrongPassword", tData.LoginAttempts);

                    return;
                }

                tData.LoginCTS?.Cancel();

                tData.StepType = TempData.StepTypes.CharacterSelection;

                aData.LastIP = player.Address;

                await Task.Run(async () =>
                {
                    aData.UpdateOnEnter();

                    tData.Characters = MySQL.GetAllCharacters(aData.ID);

                    var cData = new object[3][];

                    for (int i = 0; i < cData.Length; i++)
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

                    var newToken = GenerateToken(aData, hwid);
                    var jsonData = cData.SerializeToJson();

                    NAPI.Task.RunSafe(() =>
                    {
                        if (player?.Exists != true)
                            return;

                        player.TriggerEvent("Auth::CloseLoginPage", true);
                        player.TriggerEvent("Auth::ShowCharacterChoosePage", true, aData.Login, aData.RegistrationDate.ToString("d"), aData.BCoins, jsonData);

                        player.TriggerEvent("Auth::SaveLogin", aData.Login);
                        player.TriggerEvent("Auth::SaveToken", newToken);
                    });
                });
            });

            tData.Release();
        }
        #endregion

        #region Character Choose Attempt
        [RemoteEvent("Auth::OnCharacterChooseAttempt")]
        private async Task OnCharacterChooseAttempt(Player player, int num)
        {
            var sRes = player.CheckSpamAttackTemp();

            if (sRes.IsSpammer)
                return;

            var tData = sRes.Data;

            if (!await tData.WaitAsync())
                return;

            await Task.Run(async () =>
            {
                var aData = player.GetAccountData();

                if ((tData.StepType != TempData.StepTypes.CharacterSelection)|| aData == null)
                    return;

                int charNum = num - 1;

                if (charNum < 0 || charNum > 2)
                    return;

                if (tData.Characters[charNum] != null) // character exists
                {
                    var activeBan = tData.Characters[charNum].Punishments.Where(x => x.Type == PlayerData.Punishment.Types.Ban && x.GetSecondsLeft() > 0).FirstOrDefault();

                    if (activeBan != null || tData.Characters[charNum].IsOnline)
                        return;

                    var data = MySQL.GetCharacterByCID(player, tData.Characters[charNum].CID);

                    if (data == null)
                        return;

                    tData.StepType = TempData.StepTypes.StartPlace;

                    tData.PlayerData = data;

                    await NAPI.Task.RunAsync(() =>
                    {
                        if (player?.Exists != true)
                            return;

                        player.TriggerEvent("Auth::SaveLastCharacter", num);

                        tData.ShowStartPlace();
                    });
                }
                else // create new character
                {
                    int charactersCount = tData.Characters.Where(x => x != null).Count();

                    if (charNum != charactersCount)
                        return;

                    tData.StepType = TempData.StepTypes.CharacterCreation;

                    await NAPI.Task.RunAsync(() =>
                    {
                        if (player?.Exists != true)
                            return;

                        player.TriggerEvent("Auth::CloseCharacterChoosePage", false);
                        player.SetData("CharacterCreation::New", true);

                        CharacterCreation.StartNew(player);
                    });
                }
            });

            tData.Release();
        }
        #endregion

        [RemoteEvent("Auth::StartPlace")]
        private async Task StartPlaceSelect(Player player, bool start, int type)
        {
            var sRes = player.CheckSpamAttackTemp();

            if (sRes.IsSpammer)
                return;

            var tData = sRes.Data;

            if (!await tData.WaitAsync())
                return;

            await Task.Run(async () =>
            {
                if (tData.StepType != TempData.StepTypes.StartPlace)
                    return;

                if (start)
                {
                    if (tData.PositionToSpawn == null)
                        return;

                    await NAPI.Task.RunAsync(() =>
                    {
                        if (player?.Exists != true)
                            return;

                        tData.Remove();

                        Task.Run(async () =>
                        {
                            if (!await tData.WaitAsync())
                                return;

                            tData.Delete();
                        });

                        tData.PlayerData.LastData.Position = tData.PositionToSpawn;
                        tData.PlayerData.LastData.Dimension = tData.DimensionToSpawn;

                        player.SetMainData(tData.PlayerData);

                        tData.PlayerData.SetReady();
                    });
                }
                else
                {
                    if (!Enum.IsDefined(typeof(TempData.StartPlaceTypes), type))
                        return;

                    var sType = (TempData.StartPlaceTypes)type;

                    if (sType == TempData.StartPlaceTypes.Last)
                    {
                        tData.PositionToSpawn = tData.PlayerData.LastData.Position;
                        tData.DimensionToSpawn = tData.PlayerData.LastData.Dimension;
                    }
                    else if (sType == TempData.StartPlaceTypes.Spawn)
                    {
                        tData.PositionToSpawn = Utils.DefaultSpawnPosition;
                        tData.DimensionToSpawn = Utils.Dimensions.Main;
                    }
                    else
                        return;

                    await NAPI.Task.RunAsync(() =>
                    {
                        if (player?.Exists != true)
                            return;

                        player.Teleport(tData.PositionToSpawn, true, Utils.GetPrivateDimension(player));

                        player.SkyCameraMove(Additional.SkyCamera.SwitchTypes.Move, false, "Auth::StartPlace::Allow", type);
                    });
                }
            });

            tData.Release();
        }

        #region Stuff
        public static string GenerateToken(AccountData adata, string hwid) => Utils.EncryptString(adata.Password, Utils.ToMD5(hwid));

        private static string DecryptToken(string token, string hwid) => Utils.DecryptString(token, Utils.ToMD5(hwid));
        #endregion
    }
}
