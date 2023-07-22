using RAGE;
using RAGE.Elements;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace BlaineRP.Client.CEF
{
    [Script(int.MaxValue)]
    public class AutoschoolTest 
    {
        public static bool IsActive => CEF.Browser.IsActive(Browser.IntTypes.AutoschoolTest);

        public static bool IsActiveTest { get; set; }

        private static Sync.Players.LicenseTypes CurrentLicenseType { get; set; }

        private static int CurrentSchoolId { get; set; }

        private static DateTime LastSent;

        private static Additional.ExtraColshape CloseColshape { get; set; }

        private static Dictionary<Sync.Players.LicenseTypes, (int TestId, int MaxQuestions)> JsLibsData = new Dictionary<Sync.Players.LicenseTypes, (int, int)>()
        {
            { Sync.Players.LicenseTypes.B, (0, 4) },
            { Sync.Players.LicenseTypes.D, (0, 4) },
            { Sync.Players.LicenseTypes.C, (0, 4) },
            { Sync.Players.LicenseTypes.A, (0, 4) },
            { Sync.Players.LicenseTypes.Sea, (0, 4) },
            { Sync.Players.LicenseTypes.Fly, (0, 4) },
        };

        private static int EscBindIdx { get; set; }

        public AutoschoolTest()
        {
            Events.Add("AutoSchool::StartText", async (args) =>
            {
                var useCash = (bool)args[0];

                if (LastSent.IsSpam(500, false, true))
                    return;

                LastSent = Sync.World.ServerTime;

                var data = JsLibsData[CurrentLicenseType];

                if ((bool)await Events.CallRemoteProc("DrSchool::CHL", CurrentSchoolId, (int)CurrentLicenseType, useCash))
                {
                    ShowTest(data.MaxQuestions);
                }
            });

            Events.Add("AutoSchool::FinishTest", async (args) =>
            {
                var okAmount = (byte)(int)args[0];
                var allAmount = (byte)(int)args[1];

                Events.CallRemote("DrSchool::PT", CurrentSchoolId, okAmount, allAmount);

                Close(true);
            });

            Events.Add("AutoSchool::Close", (args) => Close(false));
        }

        public static async void Show(int schoolId, Sync.Players.LicenseTypes licType, uint price)
        {
            if (IsActive)
                return;

            await CEF.Browser.Render(Browser.IntTypes.AutoschoolTest, true, true);

            CloseColshape = new Additional.Sphere(Player.LocalPlayer.Position, 2.5f, false, Utils.RedColor, uint.MaxValue, null)
            {
                OnExit = (cancel) =>
                {
                    if (CloseColshape?.Exists == true)
                        Close(false);
                }
            };

            CurrentLicenseType = licType;

            CurrentSchoolId = schoolId;

            CEF.Browser.Window.ExecuteJs("AutoSchool.draw", JsLibsData[licType].TestId, price);

            CEF.Cursor.Show(true, true);

            EscBindIdx = KeyBinds.Bind(RAGE.Ui.VirtualKeys.Escape, true, () => Close(false));
        }

        public static void ShowTest(int questionsAmount)
        {
            if (!IsActive || IsActiveTest)
                return;

            KeyBinds.Unbind(EscBindIdx);

            EscBindIdx = -1;

            IsActiveTest = true;

            CEF.Browser.Window.ExecuteJs("AutoSchool.drawTest", questionsAmount);
        }

        public static void Close(bool success)
        {
            if (!IsActive)
                return;

            CloseColshape?.Destroy();

            CloseColshape = null;

            CEF.Browser.Render(Browser.IntTypes.AutoschoolTest, false, false);

            CEF.Cursor.Show(false, false);

            if (EscBindIdx >= 0)
            {
                KeyBinds.Unbind(EscBindIdx);

                EscBindIdx = -1;
            }

            if (IsActiveTest)
            {
                IsActiveTest = false;

                if (!success)
                    Events.CallRemote("DrSchool::PT", CurrentSchoolId, 0, 0);
            }
        }
    }
}
