using System;
using System.Linq;
using BlaineRP.Client.Extensions.RAGE.Ui;
using BlaineRP.Client.Extensions.System;
using BlaineRP.Client.Game.World;
using Newtonsoft.Json.Linq;
using RAGE;

namespace BlaineRP.Client.Game.UI.CEF
{
    [Script(int.MaxValue)]
    public class StartPlace
    {
        public enum Types : byte
        {
            /// <summary>Последнее место на сервере</summary>
            Last = 0,

            /// <summary>Спавн (Округ Блэйн)</summary>
            SpawnBlaineCounty,

            /// <summary>Дом</summary>
            House,

            /// <summary>Квартира</summary>
            Apartments,

            /// <summary>Фракция</summary>
            Fraction,

            /// <summary>Организация</summary>
            Organisation,

            /// <summary>Спавн (Лос-Сантос)</summary>
            SpawnLosSantos,

            /// <summary>Фракция (филиал)</summary>
            FractionBranch,
        }

        public static DateTime LastSent;

        public StartPlace()
        {
            LastSent = DateTime.MinValue;

            Events.Add("Auth::StartPlace::Load",
                async (object[] args) =>
                {
                    Auth.CloseAll(true);

                    CurrentTypes = ((JArray)args[0]).ToObject<Types[]>();

                    LastType = CurrentTypes.Contains(Types.Last) ? Types.Last : CurrentTypes.First();

                    LastSent = Core.ServerTime;

                    await Events.CallRemoteProc("Auth::StartPlace", false, (byte)LastType);
                }
            );

            Events.Add("Auth::StartPlace::Select",
                async (object[] args) =>
                {
                    var type = (Types)(int)args[0];

                    if (!IsActive || type == LastType)
                        return;

                    if (!LastSent.IsSpam(500, false, false))
                    {
                        LastSent = Core.ServerTime;

                        if ((bool)await Events.CallRemoteProc("Auth::StartPlace", false, (byte)type))
                        {
                            LastType = type;

                            Browser.Switch(Browser.IntTypes.StartPlace, false);
                        }
                        else
                        {
                            Notification.ShowError(Locale.Get("AUTH_STARTPLACE_CANNOT"), -1);
                        }
                    }
                }
            );

            Events.Add("Auth::StartPlace::Start",
                async (object[] args) =>
                {
                    if (!IsActive)
                        return;

                    if (!LastSent.IsSpam(1000, false, false))
                    {
                        LastSent = Core.ServerTime;

                        if ((bool)await Events.CallRemoteProc("Auth::StartPlace", true, LastType))
                        {
                        }
                        else
                        {
                            Notification.ShowError(Locale.Get("AUTH_STARTPLACE_CANNOT"), -1);
                        }
                    }
                }
            );

            Events.Add("Auth::StartPlace::Allow",
                async (object[] args) =>
                {
                    if (IsRendered)
                    {
                        Browser.Switch(Browser.IntTypes.StartPlace, true);

                        Select(LastType);
                    }
                    else
                    {
                        await Show(CurrentTypes);
                    }
                }
            );
        }

        public static bool IsRendered => Browser.IsRendered(Browser.IntTypes.StartPlace);
        public static bool IsActive => Browser.IsActive(Browser.IntTypes.StartPlace);

        private static Types[] CurrentTypes { get; set; }
        private static Types LastType { get; set; }

        public static async System.Threading.Tasks.Task Show(params Types[] types)
        {
            if (IsRendered)
                return;

            await Browser.Render(Browser.IntTypes.StartPlace, true, true);

            Browser.Window.ExecuteJs("AuthStart.draw",
                new object[]
                {
                    types,
                }
            );
            Select(LastType);

            Browser.Window.ExecuteJs("AuthStart.showConfirm", !true);
        }

        public static void Close()
        {
            if (!IsRendered)
                return;

            Browser.Render(Browser.IntTypes.StartPlace, false);
        }

        private static void Select(Types type)
        {
            Browser.Window.ExecuteJs("AuthStart.selectNav", (int)type);
        }
    }
}