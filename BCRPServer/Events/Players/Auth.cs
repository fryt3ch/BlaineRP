﻿using GTANetworkAPI;
using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace BCRPServer.Events.Players
{
    class Auth : Script
    {
        public static Regex MailRegex { get; } = new Regex(@"^(?("")(""[^""]+?""@)|(([0-9a-z]((\.(?!\.))|[-!#\$%&'\*\+/=\?\^`\{\}\|~\w])*)(?<=[0-9a-z])@))(?(\[)(\[(\d{1,3}\.){3}\d{1,3}\])|(([0-9a-z][-\w]*[0-9a-z]*\.)+[a-z0-9]{2,17}))$", RegexOptions.Compiled);
        public static Regex LoginRegex { get; } = new Regex(@"^(?=.*[a-zA-Z0-9])[0-9a-zA-Z!@#$%^&*]{6,12}$", RegexOptions.Compiled);
        public static Regex PasswordRegex { get; } = new Regex(@"^(?=.*[a-zA-Z0-9])[0-9a-zA-Z!@#$%^&*]{6,64}$", RegexOptions.Compiled);

        [RemoteEvent("Auth::OnRegistrationAttempt")]
        private static async Task OnRegistrationAttempt(Player player, string login, string mail, string password)
        {
            var sRes = player.CheckSpamAttackTemp();

            if (sRes.IsSpammer)
                return;

            var tData = sRes.Data;

            if (tData.StepType != TempData.StepTypes.AuthRegistration)
                return;

            if (password == null || login == null || password == null)
                return;

            mail = mail.ToLower();

            if (!MailRegex.IsMatch(mail) || !LoginRegex.IsMatch(login) || !PasswordRegex.IsMatch(password))
                return;

            tData.BlockRemoteCalls = true;

            var hwid = player.Serial;
            var scid = player.SocialClubId;
            var ip = player.Address;

            string confirmationHash;

            var cts = new CancellationTokenSource(2_500);

            try
            {
                confirmationHash = await Web.SocketIO.Methods.Account.Add(cts.Token, scid, hwid, login, password, mail, ip);
            }
            catch (Web.SocketIO.Exceptions.SocketIOResultException ioEx)
            {
                NAPI.Task.Run(() =>
                {
                    if (player?.Exists != true)
                        return;

                    tData.BlockRemoteCalls = false;

                    var msg = ioEx.Message;

                    if (msg == "SCIDExists")
                    {
                        player.NotifyError(Language.Strings.Get("NTFC_AUTH_SCIDEXISTS_0"));
                    }
                    else if (msg == "MailExists")
                    {
                        player.NotifyError(Language.Strings.Get("NTFC_AUTH_MAILEXISTS_0"));
                    }
                    else if (msg == "LoginExists")
                    {
                        player.NotifyError(Language.Strings.Get("NTFC_AUTH_LOGINEXISTS_0"));
                    }
                    else if (msg == "ConfirmSendTimeout")
                    {
                        TimeSpan nextSendTryTime;

                        if (ioEx.Data["NextSendTryTime"] is TimeSpan ioExDataT)
                            nextSendTryTime = ioExDataT;
                        else
                            nextSendTryTime = TimeSpan.Zero;

                        player.NotifyError(Language.Strings.Get("NTFC_AUTH_REGCONFIRM_SENT_E_0", nextSendTryTime.GetBeautyString()));
                    }
                });

                return;
            }
            catch (Exception ex)
            {
                NAPI.Task.Run(() =>
                {
                    if (player?.Exists != true)
                        return;

                    tData.BlockRemoteCalls = false;

                    player.NotifyError(Language.Strings.Get("NTFC_GEN_ERROR_0", ex.Message ?? ""));
                });

                return;
            }

            NAPI.Task.Run(() =>
            {
                if (player?.Exists != true)
                    return;

                tData.BlockRemoteCalls = false;

                tData.RegistrationConfirmationHash = confirmationHash;

                player.NotifySuccess(Language.Strings.Get("NTFC_AUTH_REGCONFIRM_SENT_0"));
            });
        }

        [RemoteEvent("Auth::OnLoginAttempt")]
        private static async Task OnLoginAttempt(Player player, string login, string password, bool isPasswordToken)
        {
            var sRes = player.CheckSpamAttackTemp();

            if (sRes.IsSpammer)
                return;

            var tData = sRes.Data;

            if (tData.StepType != TempData.StepTypes.AuthLogin)
                return;

            if (login == null || password == null)
                return;

            var hwid = player.Serial;
            var scid = player.SocialClubId;
            var ip = player.Address;

            if (isPasswordToken)
            {
                password = DecryptToken(password, hwid.ToString());
            }

            tData.LoginAttempts--;

            if (tData.LoginAttempts <= 0)
            {
                player.KickSilent();

                return;
            }

            tData.BlockRemoteCalls = true;

            AccountData aData;

            var cts = new CancellationTokenSource(2_500);

            try
            {
                aData = await Web.SocketIO.Methods.Account.Login(cts.Token, scid, login, password, ip);
            }
            catch (Web.SocketIO.Exceptions.SocketIOResultException ioEx)
            {
                NAPI.Task.Run(() =>
                {
                    if (player?.Exists != true)
                        return;

                    tData.BlockRemoteCalls = false;

                    var message = ioEx.Message;

                    if (message == "WrongLogin")
                    {
                        player.NotifyError(Language.Strings.Get("NTFC_AUTH_WRONGLOGIN_0", tData.LoginAttempts));
                    }
                    else if (message == "WrongPassword")
                    {
                        if (isPasswordToken)
                            player.NotifyError(Language.Strings.Get("NTFC_AUTH_WRONGTOKEN_0", tData.LoginAttempts));
                        else
                            player.NotifyError(Language.Strings.Get("NTFC_AUTH_WRONGPASSWORD_0", tData.LoginAttempts));
                    }
                    else
                    {
                        player.NotifyError(Language.Strings.Get("NTFC_GEN_ERROR_0", message ?? ""));
                    }
                });

                return;
            }
            catch (Exception ex)
            {
                NAPI.Task.Run(() =>
                {
                    if (player?.Exists != true)
                        return;

                    tData.BlockRemoteCalls = false;

                    player.NotifyError(Language.Strings.Get("NTFC_GEN_ERROR_0", ex.Message ?? ""));
                });

                return;
            }

            NAPI.Task.Run(() =>
            {
                if (player?.Exists != true)
                    return;

                tData.BlockRemoteCalls = false;

                tData.AccountData = aData;

                if (tData.AuthTimer != null)
                {
                    tData.AuthTimer.Dispose();

                    tData.AuthTimer = null;
                }

                tData.StepType = TempData.StepTypes.CharacterSelection;

                //tData.AccountData.LastIP = player.Address;

                var cData = new object[3];

                int idx = 0;

                foreach (var pInfo in PlayerData.PlayerInfo.GetAllByAID(aData.ID))
                {
                    if (idx >= tData.Characters.Length)
                        break;

                    tData.Characters[idx] = pInfo;

                    var lastBan = pInfo.Punishments.Where(x => x.Type == Sync.Punishment.Types.Ban && x.IsActive()).FirstOrDefault();

                    var charArr = new object[14]
                    {
                            $"{pInfo.Name} {pInfo.Surname}",
                            pInfo.BankAccount == null ? 0 : pInfo.BankAccount.Balance,
                            pInfo.Cash,
                            pInfo.Sex,
                            pInfo.BirthDate.GetTotalYears(),
                            (int)pInfo.Fraction,
                            pInfo.TimePlayed,
                            pInfo.CID,
                            lastBan != null,
                            pInfo.IsOnline,
                            null,
                            null,
                            null,
                            null,
                    };

                    if (lastBan != null)
                    {
                        charArr[10] = lastBan.PunisherID;
                        charArr[11] = lastBan.Reason;
                        charArr[12] = lastBan.StartDate.GetUnixTimestamp();
                        charArr[13] = lastBan.EndDate.GetUnixTimestamp();
                    }

                    cData[idx] = charArr;

                    idx++;
                }

                var newToken = GenerateToken(password, hwid);

                player.TriggerEvent("Auth::ShowCharacterChoosePage", true, tData.AccountData.Login, tData.AccountData.RegistrationDate.GetUnixTimestamp(), tData.AccountData.BCoins, cData, newToken);
            });
        }

        [RemoteEvent("Auth::OnCharacterChooseAttempt")]
        private static void OnCharacterChooseAttempt(Player player, byte charNum)
        {
            var sRes = player.CheckSpamAttackTemp();

            if (sRes.IsSpammer)
                return;

            var tData = sRes.Data;

            if ((tData.StepType != TempData.StepTypes.CharacterSelection) || tData.AccountData == null)
                return;

            if (charNum < 0 || charNum > 2)
                return;

            if (tData.Characters[charNum] != null) // character exists
            {
                var activePunishment = tData.Characters[charNum].Punishments.Where(x => (x.Type == Sync.Punishment.Types.Ban || x.Type == Sync.Punishment.Types.NRPPrison || x.Type == Sync.Punishment.Types.FederalPrison || x.Type == Sync.Punishment.Types.Arrest) && x.IsActive()).FirstOrDefault();

                if (activePunishment?.Type == Sync.Punishment.Types.Ban || tData.Characters[charNum].IsOnline)
                    return;

                var data = PlayerData.PlayerInfo.Get(tData.Characters[charNum].CID);

                if (data == null)
                    return;

                tData.StepType = TempData.StepTypes.StartPlace;

                var pData = new PlayerData(player, data);

                tData.PlayerData = pData;

                MySQL.CharacterUpdateOnEnter(data);

                player.TriggerEvent("Auth::SaveLastCharacter", charNum);

                if (activePunishment != null)
                {
                    if (activePunishment.Type == Sync.Punishment.Types.NRPPrison)
                    {
                        var pos = Utils.Demorgan.GetNextPos();

                        data.LastData.Dimension = Settings.DEMORGAN_DIMENSION;
                        data.LastData.Position.Position = pos;
                    }
                    else if (activePunishment.Type == Sync.Punishment.Types.Arrest)
                    {
                        var aData = activePunishment.AdditionalData.Split('_');

                        var fData = Game.Fractions.Fraction.Get((Game.Fractions.Types)int.Parse(aData[1])) as Game.Fractions.Police;

                        if (fData == null)
                            return;

                        var pos = fData.GetNextArrestCellPosition();

                        data.LastData.Position.Position = pos;
                        data.LastData.Dimension = Settings.MAIN_DIMENSION;
                    }

                    tData.Delete();

                    pData.SetReady();
                }
                else
                {
                    tData.ShowStartPlace();
                }
            }
            else // create new character
            {
                var charactersCount = tData.Characters.Where(x => x != null).Count();

                if (charNum != charactersCount)
                    return;

                tData.StepType = TempData.StepTypes.CharacterCreation;

                CharacterCreation.StartNew(player);
            }
        }

        [RemoteProc("Auth::StartPlace")]
        private static bool StartPlaceSelect(Player player, bool start, byte type)
        {
            var sRes = player.CheckSpamAttackTemp();

            if (sRes.IsSpammer)
                return false;

            var tData = sRes.Data;

            if (tData.StepType != TempData.StepTypes.StartPlace)
                return false;

            if (start)
            {
                if (tData.PositionToSpawn == null)
                    return false;

                var pData = tData.PlayerData;

                pData.LastData.Position = tData.PositionToSpawn;
                pData.LastData.Dimension = tData.DimensionToSpawn;

                pData.AccountData = tData.AccountData;

                tData.Delete();

                pData.SetReady();
            }
            else
            {
                if (!Enum.IsDefined(typeof(TempData.StartPlaceTypes), type))
                    return false;

                var sType = (TempData.StartPlaceTypes)type;

                if (sType == TempData.StartPlaceTypes.Last)
                {
                    tData.PositionToSpawn = tData.PlayerData.LastData.Position;
                    tData.DimensionToSpawn = tData.PlayerData.LastData.Dimension;

                    player.Teleport(tData.PositionToSpawn.Position, true, Utils.GetPrivateDimension(player));
                }
                else if (sType == TempData.StartPlaceTypes.SpawnBlaineCounty)
                {
                    tData.PositionToSpawn = new Utils.Vector4(Utils.DefaultSpawnPosition.X, Utils.DefaultSpawnPosition.Y, Utils.DefaultSpawnPosition.Z, Utils.DefaultSpawnHeading);
                    tData.DimensionToSpawn = Settings.MAIN_DIMENSION;

                    player.Teleport(tData.PositionToSpawn.Position, true, Utils.GetPrivateDimension(player));
                }
                else if (sType == TempData.StartPlaceTypes.Fraction)
                {
                    if (tData.PlayerData.Fraction != Game.Fractions.Types.None)
                    {
                        var fData = Game.Fractions.Fraction.Get(tData.PlayerData.Fraction);

                        var pos = fData.GetSpawnPosition(0);

                        if (pos == null)
                            return false;

                        tData.PositionToSpawn = new Utils.Vector4(pos.X, pos.Y, pos.Z, pos.RotationZ);
                        tData.DimensionToSpawn = Settings.MAIN_DIMENSION;

                        player.Teleport(tData.PositionToSpawn.Position, true, Utils.GetPrivateDimension(player));
                    }
                    else
                        return false;
                }
                else if (sType == TempData.StartPlaceTypes.FractionBranch)
                {
                    if (tData.PlayerData.Fraction != Game.Fractions.Types.None)
                    {
                        var fData = Game.Fractions.Fraction.Get(tData.PlayerData.Fraction);

                        var pos = fData.GetSpawnPosition(1);

                        if (pos == null)
                            return false;

                        tData.PositionToSpawn = new Utils.Vector4(pos.X, pos.Y, pos.Z, pos.RotationZ);
                        tData.DimensionToSpawn = Settings.MAIN_DIMENSION;

                        player.Teleport(tData.PositionToSpawn.Position, true, Utils.GetPrivateDimension(player));
                    }
                    else
                        return false;
                }
                else if (sType == TempData.StartPlaceTypes.House)
                {
                    var house = tData.PlayerData.OwnedHouses.FirstOrDefault() ?? tData.PlayerData.SettledHouseBase as Game.Estates.House;

                    if (house != null)
                    {
                        var hPos = house.StyleData.Position;

                        tData.PositionToSpawn = new Utils.Vector4(hPos.X, hPos.Y, hPos.Z, house.StyleData.Heading);
                        tData.DimensionToSpawn = house.Dimension;

                        player.Teleport(house.PositionParams.Position, true, Utils.GetPrivateDimension(player));
                    }
                    else
                        return false;
                }
                else if (sType == TempData.StartPlaceTypes.Apartments)
                {
                    var aps = tData.PlayerData.OwnedApartments.FirstOrDefault() ?? tData.PlayerData.SettledHouseBase as Game.Estates.Apartments;

                    if (aps != null)
                    {
                        var hPos = aps.StyleData.Position;

                        tData.PositionToSpawn = new Utils.Vector4(hPos.X, hPos.Y, hPos.Z, aps.StyleData.Heading);
                        tData.DimensionToSpawn = aps.Dimension;

                        player.Teleport(aps.Root.EnterParams.Position, true, Utils.GetPrivateDimension(player));
                    }
                    else
                        return false;
                }
                else
                    return false;

                player.SkyCameraMove(Additional.SkyCamera.SwitchTypes.Move, false, "Auth::StartPlace::Allow", type);
            }

            return true;
        }

        public static string GenerateToken(string password, string hwid) => Cryptography.AesEncryptString(password, Cryptography.MD5EncryptString(hwid));

        private static string DecryptToken(string token, string hwid) => Cryptography.AesDecryptString(token, Cryptography.MD5EncryptString(hwid));
    }
}
