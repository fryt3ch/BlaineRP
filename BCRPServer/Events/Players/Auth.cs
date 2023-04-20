using GTANetworkAPI;
using System;
using System.Linq;
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
                        if (tData.AuthTimer != null)
                        {
                            tData.AuthTimer.Dispose();

                            tData.AuthTimer = null;
                        }

                        tData.AccountData = aRes.AccountData;

                        tData.ActualToken = GenerateToken(tData.AccountData, hwid);

                        tData.Characters = new PlayerData.PlayerInfo[3];

                        var cData = new object[3];

                        player.TriggerEvent("Auth::ShowCharacterChoosePage", true, tData.AccountData.Login, tData.AccountData.RegistrationDate.GetUnixTimestamp(), tData.AccountData.BCoins, cData, tData.ActualToken);

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

            if (tData.AuthTimer != null)
            {
                tData.AuthTimer.Dispose();

                tData.AuthTimer = null;
            }

            tData.StepType = TempData.StepTypes.CharacterSelection;

            tData.AccountData.LastIP = player.Address;

            Task.Run(() =>
            {
                tData.AccountData.UpdateOnEnter();
            });

            var cData = new object[3];

            for (int i = 0; i < cData.Length; i++)
            {
                if (tData.Characters[i] != null)
                {
                    var lastBan = tData.Characters[i].Punishments.Where(x => x.Type == Sync.Punishment.Types.Ban && x.IsActive()).FirstOrDefault();

                    var charArr = new object[14]
                    {
                            $"{tData.Characters[i].Name} {tData.Characters[i].Surname}",
                            tData.Characters[i].BankAccount == null ? 0 : tData.Characters[i].BankAccount.Balance,
                            tData.Characters[i].Cash,
                            tData.Characters[i].Sex,
                            tData.Characters[i].BirthDate.GetTotalYears(),
                            (int)tData.Characters[i].Fraction,
                            tData.Characters[i].TimePlayed,
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
                        charArr[10] = lastBan.PunisherID;
                        charArr[11] = lastBan.Reason;
                        charArr[12] = lastBan.StartDate.GetUnixTimestamp();
                        charArr[13] = lastBan.EndDate.GetUnixTimestamp();
                    }

                    cData[i] = charArr;
                }
            }

            var newToken = GenerateToken(tData.AccountData, hwid);

            player.TriggerEvent("Auth::ShowCharacterChoosePage", true, tData.AccountData.Login, tData.AccountData.RegistrationDate.GetUnixTimestamp(), tData.AccountData.BCoins, cData, newToken);
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

                player.TriggerEvent("Auth::SaveLastCharacter", num);

                if (activePunishment != null)
                {
                    if (activePunishment.Type == Sync.Punishment.Types.NRPPrison)
                    {
                        var pos = Utils.Demorgan.GetNextPos();

                        data.LastData.Dimension = Utils.Dimensions.Demorgan;
                        data.LastData.Position.Position = pos;
                    }
                    else if (activePunishment.Type == Sync.Punishment.Types.Arrest)
                    {
                        var fData = Game.Fractions.Fraction.Get(activePunishment.AdditionalData.DeserializeFromJson<Game.Fractions.Types>()) as Game.Fractions.Police;

                        if (fData == null)
                            return;

                        var pos = fData.GetNextArrestCellPosition();

                        data.LastData.Position.Position = pos;
                        data.LastData.Dimension = Utils.Dimensions.Main;
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
        #endregion

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
                    tData.DimensionToSpawn = Utils.Dimensions.Main;

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
                        tData.DimensionToSpawn = Utils.Dimensions.Main;

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
                        tData.DimensionToSpawn = Utils.Dimensions.Main;

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

        #region Stuff
        public static string GenerateToken(AccountData adata, string hwid) => Utils.EncryptString(adata.Password, Utils.ToMD5(hwid));

        private static string DecryptToken(string token, string hwid) => Utils.DecryptString(token, Utils.ToMD5(hwid));
        #endregion
    }
}
