using BlaineRP.Client.Extensions.RAGE.Ui;
using BlaineRP.Client.Extensions.System;
using Newtonsoft.Json.Linq;
using RAGE;
using System;
using System.Linq;

namespace BlaineRP.Client.CEF
{
    [Script(int.MaxValue)]
    public class StartPlace
    {
        public static bool IsRendered { get => CEF.Browser.IsRendered(CEF.Browser.IntTypes.StartPlace); }
        public static bool IsActive { get => CEF.Browser.IsActive(CEF.Browser.IntTypes.StartPlace); }

        public static DateTime LastSent;

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

        private static Types[] CurrentTypes { get; set; }
        private static Types LastType { get; set; }

        public StartPlace()
        {
            LastSent = DateTime.MinValue;

            Events.Add("Auth::StartPlace::Load", async (object[] args) =>
            {
                CEF.Auth.CloseAll(true);

                CurrentTypes = ((JArray)args[0]).ToObject<Types[]>();

                LastType = CurrentTypes.Contains(Types.Last) ? Types.Last : CurrentTypes.First();

                LastSent = Sync.World.ServerTime;

                await Events.CallRemoteProc("Auth::StartPlace", false, (byte)LastType);
            });

            Events.Add("Auth::StartPlace::Select", async (object[] args) =>
            {
                Types type = (Types)(int)args[0];

                if (!IsActive || type == LastType)
                    return;

                if (!LastSent.IsSpam(500, false, false))
                {
                    LastSent = Sync.World.ServerTime;

                    if ((bool)await Events.CallRemoteProc("Auth::StartPlace", false, (byte)type))
                    {
                        LastType = type;

                        CEF.Browser.Switch(Browser.IntTypes.StartPlace, false);
                    }
                    else
                    {
                        CEF.Notification.ShowError(Locale.Get("AUTH_STARTPLACE_CANNOT"), -1);
                    }
                }
            });

            Events.Add("Auth::StartPlace::Start", async (object[] args) =>
            {
                if (!IsActive)
                    return;

                if (!LastSent.IsSpam(1000, false, false))
                {
                    LastSent = Sync.World.ServerTime;

                    if ((bool)await Events.CallRemoteProc("Auth::StartPlace", true, LastType))
                    {

                    }
                    else
                    {
                        CEF.Notification.ShowError(Locale.Get("AUTH_STARTPLACE_CANNOT"), -1);
                    }
                }
            });

            Events.Add("Auth::StartPlace::Allow", async (object[] args) =>
            {
                if (IsRendered)
                {
                    CEF.Browser.Switch(Browser.IntTypes.StartPlace, true);

                    Select(LastType);
                }
                else
                    await Show(CurrentTypes);
            });
        }

        public static async System.Threading.Tasks.Task Show(params Types[] types)
        {
            if (IsRendered)
                return;

            await CEF.Browser.Render(Browser.IntTypes.StartPlace, true, true);

            CEF.Browser.Window.ExecuteJs("AuthStart.draw", new object[] { types });
            Select(LastType);

            CEF.Browser.Window.ExecuteJs("AuthStart.showConfirm", !true);
        }

        public static void Close()
        {
            if (!IsRendered)
                return;

            CEF.Browser.Render(Browser.IntTypes.StartPlace, false);
        }

        private static void Select(Types type)
        {
            CEF.Browser.Window.ExecuteJs("AuthStart.selectNav", (int)type);
        }
    }
}
