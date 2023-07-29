using GTANetworkAPI;
using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using BlaineRP.Server.Extensions.System;
using BlaineRP.Server.Game.EntitiesData.Players;
using BlaineRP.Server.Game.Management;
using BlaineRP.Server.Game.Management.Misc;
using BlaineRP.Server.Game.Management.Punishments;
using BlaineRP.Server.UtilsT;
using BlaineRP.Server.UtilsT.Cryptography;

namespace BlaineRP.Server.Events.Players
{
    class Auth : Script
    {
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

            if (!Properties.Settings.Static.AuthMailRegex.IsMatch(mail) || !Properties.Settings.Static.AuthLoginRegex.IsMatch(login) || !Properties.Settings.Static.AuthPasswordRegex.IsMatch(password))
                return;

            tData.BlockRemoteCalls = true;

            var hwid = player.Serial;
            var scid = player.SocialClubId;
            var ip = player.Address;

            string confirmationHash;

            using (var cts = new CancellationTokenSource(2_500))
            {
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
            else
            {

            }

            tData.BlockRemoteCalls = true;

            AccountData aData;

            using (var cts = new CancellationTokenSource(2_500))
            {
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

                        if (message == "WrongLogin" || message == "WrongPassword")
                        {
                            tData.LoginAttempts--;

                            if (tData.LoginAttempts <= 0)
                            {
                                player.KickSilent();
                            }
                            else
                            {
                                if (message == "WrongLogin")
                                {
                                    player.NotifyError(Language.Strings.Get("NTFC_AUTH_WRONGLOGIN_0", tData.LoginAttempts));
                                }
                                else
                                {
                                    if (isPasswordToken)
                                        player.NotifyError(Language.Strings.Get("NTFC_AUTH_WRONGTOKEN_0", tData.LoginAttempts));
                                    else
                                        player.NotifyError(Language.Strings.Get("NTFC_AUTH_WRONGPASSWORD_0", tData.LoginAttempts));
                                }
                            }
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

                foreach (var pInfo in PlayerInfo.All.Values.Where(x => aData.ID == x.AID))
                {
                    if (idx >= tData.Characters.Length)
                        break;

                    tData.Characters[idx] = pInfo;

                    var lastBan = pInfo.Punishments.Where(x => x.Type == PunishmentType.Ban && x.IsActive()).FirstOrDefault();

                    var charArr = new object[14]
                    {
                            $"{pInfo.Name} {pInfo.Surname}",
                            pInfo.BankAccount == null ? 0 : pInfo.BankAccount.Balance,
                            pInfo.Cash,
                            pInfo.Sex,
                            pInfo.BirthDate.GetTotalYears(),
                            (int)pInfo.Fraction,
                            pInfo.TimePlayed.TotalMinutes,
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

                player.TriggerEvent("Auth::CharSelect::Show", true, tData.AccountData.Login, tData.AccountData.RegistrationDate.GetUnixTimestamp(), tData.AccountData.BCoins, cData, newToken);
            });
        }

        [RemoteProc("Auth::OnCharacterChooseAttempt")]
        private static byte OnCharacterChooseAttempt(Player player, byte charNum)
        {
            var sRes = player.CheckSpamAttackTemp();

            if (sRes.IsSpammer)
                return 0;

            var tData = sRes.Data;

            if ((tData.StepType != TempData.StepTypes.CharacterSelection) || tData.AccountData == null)
                return 0;

            if (charNum < 0 || charNum >= tData.Characters.Length)
                return 0;

            if (tData.Characters[charNum] != null) // character exists
            {
                var activePunishment = tData.Characters[charNum].Punishments.Where(x => (x.Type == PunishmentType.Ban || x.Type == PunishmentType.NRPPrison || x.Type == PunishmentType.FederalPrison || x.Type == PunishmentType.Arrest) && x.IsActive()).FirstOrDefault();

                if (activePunishment?.Type == PunishmentType.Ban || tData.Characters[charNum].IsOnline)
                    return 1;

                var data = PlayerInfo.Get(tData.Characters[charNum].CID);

                if (data == null)
                    return 0;

                tData.StepType = TempData.StepTypes.StartPlace;

                var pData = new PlayerData(player, data);

                tData.PlayerData = pData;

                MySQL.CharacterUpdateOnEnter(data);

                if (activePunishment != null)
                {
                    if (activePunishment.Type == PunishmentType.NRPPrison)
                    {
                        var pos = Utils.Demorgan.GetNextPos();

                        data.LastData.Dimension = Properties.Settings.Static.DemorganDimension;
                        data.LastData.Position.Position = pos;
                    }
                    else if (activePunishment.Type == PunishmentType.Arrest)
                    {
                        var aData = activePunishment.AdditionalData.Split('_');

                        var fData = Game.Fractions.Fraction.Get((Game.Fractions.FractionType)int.Parse(aData[1])) as Game.Fractions.Police;

                        var pos = fData.GetNextArrestCellPosition();

                        data.LastData.Position.Position = pos;
                        data.LastData.Dimension = Properties.Settings.Static.MainDimension;
                    }

                    tData.Delete();

                    pData.SetReady();
                }
                else
                {
                    tData.ShowStartPlace();
                }

                return 255;
            }
            else // create new character
            {
                var charactersCount = tData.Characters.Where(x => x != null).Count();

                if (charNum != charactersCount)
                    return 0;

                tData.StepType = TempData.StepTypes.CharacterCreation;

                CharacterCreation.StartNew(player);

                return 255;
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
                    tData.PositionToSpawn = new Vector4(Utils.DefaultSpawnPosition.X, Utils.DefaultSpawnPosition.Y, Utils.DefaultSpawnPosition.Z, Utils.DefaultSpawnHeading);
                    tData.DimensionToSpawn = Properties.Settings.Static.MainDimension;

                    player.Teleport(tData.PositionToSpawn.Position, true, Utils.GetPrivateDimension(player));
                }
                else if (sType == TempData.StartPlaceTypes.Fraction)
                {
                    if (tData.PlayerData.Fraction != Game.Fractions.FractionType.None)
                    {
                        var fData = Game.Fractions.Fraction.Get(tData.PlayerData.Fraction);

                        var pos = fData.GetSpawnPosition(0);

                        if (pos == null)
                            return false;

                        tData.PositionToSpawn = new Vector4(pos.X, pos.Y, pos.Z, pos.RotationZ);
                        tData.DimensionToSpawn = Properties.Settings.Static.MainDimension;

                        player.Teleport(tData.PositionToSpawn.Position, true, Utils.GetPrivateDimension(player));
                    }
                    else
                        return false;
                }
                else if (sType == TempData.StartPlaceTypes.FractionBranch)
                {
                    if (tData.PlayerData.Fraction != Game.Fractions.FractionType.None)
                    {
                        var fData = Game.Fractions.Fraction.Get(tData.PlayerData.Fraction);

                        var pos = fData.GetSpawnPosition(1);

                        if (pos == null)
                            return false;

                        tData.PositionToSpawn = new Vector4(pos.X, pos.Y, pos.Z, pos.RotationZ);
                        tData.DimensionToSpawn = Properties.Settings.Static.MainDimension;

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

                        tData.PositionToSpawn = new Vector4(hPos.X, hPos.Y, hPos.Z, house.StyleData.Heading);
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

                        tData.PositionToSpawn = new Vector4(hPos.X, hPos.Y, hPos.Z, aps.StyleData.Heading);
                        tData.DimensionToSpawn = aps.Dimension;

                        player.Teleport(aps.Root.EnterParams.Position, true, Utils.GetPrivateDimension(player));
                    }
                    else
                        return false;
                }
                else
                    return false;

                player.SkyCameraMove(SkyCamera.SwitchType.Move, false, "Auth::StartPlace::Allow", type);
            }

            return true;
        }

        public static string GenerateToken(string password, string hwid) => Aes.AesEncryptString(password, MD5.MD5EncryptString(hwid));

        private static string DecryptToken(string token, string hwid) => Aes.AesDecryptString(token, MD5.MD5EncryptString(hwid));
    }
}
