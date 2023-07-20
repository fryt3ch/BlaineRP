using Newtonsoft.Json.Linq;
using RAGE;
using RAGE.Elements;
using System;

namespace BCRPClient.CEF
{
    [Script(int.MaxValue)]
    public class Auth 
    {
        private static DateTime LastTime;

        public static void CloseAll(bool forStartPlace = false)
        {
            Browser.Render(Browser.IntTypes.Login, false);
            Browser.Render(Browser.IntTypes.Registration, false);
            Browser.Render(Browser.IntTypes.CharacterSelection, false);

            if (!forStartPlace)
            {
                Browser.Render(Browser.IntTypes.StartPlace, false);

                Cursor.Show(false, false);
            }

            CEF.Audio.StopAuthPlaylist();
        }

        public Auth()
        {
            Events.Add("Auth::Start::Show", async (args) =>
            {
                var authData = (JObject)args[0];

                var authDataType = authData["Type"].ToObject<byte>();
                var socialClubName = authData["SCName"].ToObject<string>();

                while (Browser.Window == null || !Browser.Window.Active)
                    await RAGE.Game.Invoker.WaitAsync(0);

                CEF.Audio.StartAuthPlaylist();

                Player.LocalPlayer.SetData("SocialClub::Name", socialClubName);

                if (authDataType == 0)
                {
                    await Browser.Render(Browser.IntTypes.Login, true, true);

                    GameEvents.OnReady();

                    var login = Additional.Storage.GetData<string>("Auth::Login");
                    var token = Additional.Storage.GetData<string>("Auth::Token");

                    Browser.Window.ExecuteJs("AuthLogin.fillPanel", Browser.DefaultServer, socialClubName, login, token);

                    Cursor.Show(true, true);
                }
                else if (authDataType == 1)
                {
                    await Browser.Render(Browser.IntTypes.Registration, true, true);

                    GameEvents.OnReady();

                    Browser.Window.ExecuteJs("AuthReg.fillPanel", Browser.DefaultServer, socialClubName);

                    Cursor.Show(true, true);
                }
            });

            Events.Add("Auth::CharSelect::Show", async (args) =>
            {
                if ((bool)args[0])
                {
                    Browser.Render(Browser.IntTypes.Login, false);
                    Browser.Render(Browser.IntTypes.Registration, false);

                    var login = (string)args[1];
                    var regDate = DateTimeOffset.FromUnixTimeSeconds(Utils.ToInt64(args[2])).DateTime;
                    var bCoins = Utils.ToDecimal(args[3]);

                    var data = ((JArray)args[4]).ToObject<object[]>();

                    var token = (string)args[5];

                    Additional.Storage.SetData("Auth::Login", login);
                    Additional.Storage.SetData("Auth::Token", token);

                    for (int i = 0; i < data.Length; i++)
                    {
                        var cData = ((JArray)data[i])?.ToObject<object[]>();

                        if (cData == null)
                        {
                            data[i] = new object[14];

                            continue;
                        }

                        data[i] = cData;

                        var fractionType = (Data.Fractions.Types)Utils.ToDecimal(cData[5]);

                        cData[5] = Data.Fractions.Fraction.Get(fractionType)?.Name ?? Data.Fractions.Fraction.NoFractionStr;
                        cData[6] = TimeSpan.FromMinutes(Convert.ToDouble(cData[6])).TotalHours.ToString("0.0");

                        if (cData[10] != null)
                        {
                            cData[12] = DateTimeOffset.FromUnixTimeSeconds(Utils.ToInt64(cData[12])).DateTime.ToString("dd.MM.yyyy HH:mm");
                            cData[13] = DateTimeOffset.FromUnixTimeSeconds(Utils.ToInt64(cData[13])).DateTime.ToString("dd.MM.yyyy HH:mm");
                        }
                    }

                    await Browser.Render(Browser.IntTypes.CharacterSelection, true, false);

                    var lastCharIdx = Additional.Storage.GetData<int?>("Auth::LastCharacter");

                    if (lastCharIdx is int lastCharIdxI)
                    {
                        if (lastCharIdxI < 0 || lastCharIdxI >= data.Length)
                            lastCharIdx = 0;

                        if (lastCharIdxI > 0 && ((object[])data[lastCharIdxI])[0] == null)
                            lastCharIdx = lastCharIdxI - 1;
                    }

                    Browser.Window.ExecuteJs("AuthSelect.fillPanel", login, regDate.ToString("dd.MM.yyyy"), bCoins, (lastCharIdx ?? 0) + 1, RAGE.Util.Json.Serialize(data));

                    Browser.Switch(Browser.IntTypes.CharacterSelection, true);
                }
                else
                {
                    await Browser.Switch(Browser.IntTypes.CharacterSelection, true);
                }

                Cursor.Show(true, true);
            });

            Events.Add("Auth::RegistrationAttempt", (args) =>
            {
                if (LastTime.IsSpam(500, false, true))
                    return;

                LastTime = Sync.World.ServerTime;

                var login = (string)args[0];
                var mail = ((string)args[1]).ToLower();
                var pass1 = (string)args[2];
                var pass2 = (string)args[3];

                if (login.Length < 6 || login.Length > 12)
                {
                    CEF.Notification.ShowError(login.Length > 12 ? Locale.Notifications.Auth.LoginTooLong : Locale.Notifications.Auth.LoginTooShort);

                    return;
                }

                if (!Utils.IsLoginValid(login))
                {
                    CEF.Notification.ShowError(Locale.Notifications.Auth.InvalidLogin);

                    return;
                }

                if (!Utils.IsMailValid(mail))
                {
                    CEF.Notification.ShowError(Locale.Notifications.Auth.InvalidMail);

                    return;
                }

                if (pass1.Length < 6 || pass1.Length > 64)
                {
                    CEF.Notification.ShowError(pass1.Length > 64 ? Locale.Notifications.Auth.PasswordTooLong : Locale.Notifications.Auth.PasswordTooShort);

                    return;
                }

                if (!Utils.IsPasswordValid(pass1))
                {
                    CEF.Notification.ShowError(Locale.Notifications.Auth.InvalidPassword);

                    return;
                }

                if (!pass1.Equals(pass2))
                {
                    CEF.Notification.ShowError(Locale.Notifications.Auth.PassNotMatch);

                    return;
                }

                RAGE.Events.CallRemote("Auth::OnRegistrationAttempt", login, mail, pass1);
            });

            Events.Add("Auth::LoginAttempt", (args) =>
            {
                if (LastTime.IsSpam(500, false, true))
                    return;

                var login = (string)args[0];
                var pass = (string)args[1];

                if (login.Length < 1 || pass.Length < 1)
                    return;

                LastTime = Sync.World.ServerTime;

                RAGE.Events.CallRemote("Auth::OnLoginAttempt", login, pass, pass == Additional.Storage.GetData<string>("Auth::Token"));
            });

            Events.Add("Auth::CharacterChooseAttempt", async (args) =>
            {
                if (LastTime.IsSpam(500, false, false))
                    return;

                var charNum = (int)args[0] - 1;

                LastTime = Sync.World.ServerTime;

                var res = (int)await RAGE.Events.CallRemoteProc("Auth::OnCharacterChooseAttempt", (byte)charNum);

                if (res == 0)
                {
                    CEF.Notification.ShowErrorDefault();
                }
                else if (res == 255)
                {
                    Additional.Storage.SetData("Auth::LastCharacter", charNum);
                }
            });
        }
    }
}
