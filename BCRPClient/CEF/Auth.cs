using RAGE;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json.Linq;

namespace BCRPClient.CEF
{
    class Auth : Events.Script
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
        }

        public Auth()
        {
            LastTime = DateTime.Now;

            #region Events
            #region Savers
            Events.Add("Auth::SaveLogin", (object[] args) => Additional.Storage.SetData("Auth::Login", (string)args[0]));
            Events.Add("Auth::SaveToken", (object[] args) => Additional.Storage.SetData("Auth::Token", (string)args[0]));
            Events.Add("Auth::SaveLastCharacter", (object[] args) => Additional.Storage.SetData("Auth::LastCharacter", (int)args[0]));
            #endregion

            #region Showers
            Events.Add("Auth::ShowLoginPage", async (object[] args) =>
            {
                while (Browser.Window == null || !Browser.Window.Active)
                    await RAGE.Game.Invoker.WaitAsync(0);

                await Browser.Render(Browser.IntTypes.Login, true);

                GameEvents.OnReady();

                var login = Additional.Storage.GetData<string>("Auth::Login");
                var token = Additional.Storage.GetData<string>("Auth::Token");

                Browser.Window.ExecuteJs("AuthLogin.fillPanel", Browser.DefaultServer, (string)args[0], login, token);

                await Browser.Switch(Browser.IntTypes.Login, true);

                Cursor.Show(true, true);
            });

            Events.Add("Auth::ShowRegistrationPage", async (object[] args) =>
            {
                while (Browser.Window == null || !Browser.Window.Active)
                    await RAGE.Game.Invoker.WaitAsync(0);

                await Browser.Render(Browser.IntTypes.Registration, true);

                GameEvents.OnReady();

                Browser.Window.ExecuteJs("AuthReg.fillPanel", Browser.DefaultServer, (string)args[0]);
                Browser.Switch(Browser.IntTypes.Registration, true);

                Cursor.Show(true, true);
            });

            Events.Add("Auth::ShowCharacterChoosePage", async (object[] args) =>
            {
                if ((bool)args[0])
                {
                    await Browser.Render(Browser.IntTypes.CharacterSelection, true);

                    var login = (string)args[1];
                    var regDate = (string)args[2];
                    var bCoins = (int)args[3];

                    var data = (string)args[4];

                    Browser.Window.ExecuteJs("AuthSelect.fillPanel", login, regDate, bCoins, Additional.Storage.GetData<int?>("Auth::LastCharacter") ?? 1, data);

                    await Browser.Switch(Browser.IntTypes.CharacterSelection, true);
                }
                else
                {
                    await Browser.Switch(Browser.IntTypes.CharacterSelection, true);
                }

                Cursor.Show(true, true);
            });
            #endregion

            #region Closers
            Events.Add("Auth::CloseLoginPage", (object[] args) =>
            {
                if ((bool)args[0])
                {
                    Browser.Render(Browser.IntTypes.Login, false);
                }
                else
                    Browser.Switch(Browser.IntTypes.Login, false);
            });

            Events.Add("Auth::CloseRegistrationPage", (object[] args) =>
            {
                if ((bool)args[0])
                {
                    Browser.Render(Browser.IntTypes.Registration, false);
                }
                else
                    Browser.Switch(Browser.IntTypes.Registration, false);
            });

            Events.Add("Auth::CloseCharacterChoosePage", (object[] args) =>
            {
                if ((bool)args[0])
                {
                    Browser.Render(Browser.IntTypes.CharacterSelection, false);
                }
                else
                    Browser.Switch(Browser.IntTypes.CharacterSelection, false);
            });
            #endregion

            #region Attempt Managers
            Events.Add("Auth::RegistrationAttempt", (object[] args) =>
            {
                if (LastTime.IsSpam(500, false, false))
                    return;

                LastTime = DateTime.Now;

                var login = (string)args[0];
                var mail = ((string)args[1]).ToLower();
                var pass1 = (string)args[2];
                var pass2 = (string)args[3];

                if (login.Length < 6 || login.Length > 12)
                {
                    CEF.Notification.Show(Notification.Types.Error, Locale.Notifications.ErrorHeader, login.Length > 12 ? Locale.Notifications.Auth.LoginTooLong : Locale.Notifications.Auth.LoginTooShort);

                    return;
                }

                if (!Utils.IsLoginValid(login))
                {
                    CEF.Notification.Show(Notification.Types.Error, Locale.Notifications.ErrorHeader, Locale.Notifications.Auth.InvalidLogin);

                    return;
                }

                if (!Utils.IsMailValid(mail))
                {
                    CEF.Notification.Show(Notification.Types.Error, Locale.Notifications.ErrorHeader, Locale.Notifications.Auth.InvalidMail);

                    return;
                }

                if (pass1.Length < 6 || pass1.Length > 64)
                {
                    CEF.Notification.Show(Notification.Types.Error, Locale.Notifications.ErrorHeader, pass1.Length > 64 ? Locale.Notifications.Auth.PasswordTooLong : Locale.Notifications.Auth.PasswordTooShort);

                    return;
                }

                if (!Utils.IsPasswordValid(pass1))
                {
                    CEF.Notification.Show(Notification.Types.Error, Locale.Notifications.ErrorHeader, Locale.Notifications.Auth.InvalidPassword);

                    return;
                }

                if (!pass1.Equals(pass2))
                {
                    CEF.Notification.Show(Notification.Types.Error, Locale.Notifications.ErrorHeader, Locale.Notifications.Auth.PassNotMatch);

                    return;
                }

                RAGE.Events.CallRemote("Auth::OnRegistrationAttempt", login, mail, pass1);
            });

            Events.Add("Auth::LoginAttempt", (object[] args) =>
            {
                if (LastTime.IsSpam(1000, false, false))
                    return;

                var login = (string)args[0];
                var pass = (string)args[1];

                if (login.Length < 1 || pass.Length < 1)
                    return;

                LastTime = DateTime.Now;

                RAGE.Events.CallRemote("Auth::OnLoginAttempt", login, pass);
            });

            Events.Add("Auth::CharacterChooseAttempt", (object[] args) =>
            {
                if (LastTime.IsSpam(1000, false, false))
                    return;

                LastTime = DateTime.Now;

                RAGE.Events.CallRemote("Auth::OnCharacterChooseAttempt", args);
            });
            #endregion
            #endregion
        }
    }
}
