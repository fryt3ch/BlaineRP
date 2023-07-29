using System;
using System.Collections.Generic;
using BlaineRP.Client.Extensions.RAGE.Ui;
using BlaineRP.Client.Extensions.System;
using BlaineRP.Client.Game.EntitiesData.Players;
using BlaineRP.Client.Game.Helpers.Colshapes;
using BlaineRP.Client.Game.Helpers.Colshapes.Types;
using RAGE;
using RAGE.Elements;

namespace BlaineRP.Client.Game.UI.CEF
{
    [Script(int.MaxValue)]
    public class AutoschoolTest
    {
        private static DateTime LastSent;

        private static Dictionary<LicenseTypes, (int TestId, int MaxQuestions)> JsLibsData = new Dictionary<LicenseTypes, (int, int)>()
        {
            { LicenseTypes.B, (0, 4) },
            { LicenseTypes.D, (0, 4) },
            { LicenseTypes.C, (0, 4) },
            { LicenseTypes.A, (0, 4) },
            { LicenseTypes.Sea, (0, 4) },
            { LicenseTypes.Fly, (0, 4) },
        };

        public AutoschoolTest()
        {
            Events.Add("AutoSchool::StartText",
                async (args) =>
                {
                    var useCash = (bool)args[0];

                    if (LastSent.IsSpam(500, false, true))
                        return;

                    LastSent = World.Core.ServerTime;

                    (int TestId, int MaxQuestions) data = JsLibsData[CurrentLicenseType];

                    if ((bool)await Events.CallRemoteProc("DrSchool::CHL", CurrentSchoolId, (int)CurrentLicenseType, useCash))
                        ShowTest(data.MaxQuestions);
                }
            );

            Events.Add("AutoSchool::FinishTest",
                async (args) =>
                {
                    var okAmount = (byte)(int)args[0];
                    var allAmount = (byte)(int)args[1];

                    Events.CallRemote("DrSchool::PT", CurrentSchoolId, okAmount, allAmount);

                    Close(true);
                }
            );

            Events.Add("AutoSchool::Close", (args) => Close(false));
        }

        public static bool IsActive => Browser.IsActive(Browser.IntTypes.AutoschoolTest);

        public static bool IsActiveTest { get; set; }

        private static LicenseTypes CurrentLicenseType { get; set; }

        private static int CurrentSchoolId { get; set; }

        private static ExtraColshape CloseColshape { get; set; }

        private static int EscBindIdx { get; set; }

        public static async void Show(int schoolId, LicenseTypes licType, uint price)
        {
            if (IsActive)
                return;

            await Browser.Render(Browser.IntTypes.AutoschoolTest, true, true);

            CloseColshape = new Sphere(Player.LocalPlayer.Position, 2.5f, false, Utils.Misc.RedColor, uint.MaxValue, null)
            {
                OnExit = (cancel) =>
                {
                    if (CloseColshape?.Exists == true)
                        Close(false);
                },
            };

            CurrentLicenseType = licType;

            CurrentSchoolId = schoolId;

            Browser.Window.ExecuteJs("AutoSchool.draw", JsLibsData[licType].TestId, price);

            Cursor.Show(true, true);

            EscBindIdx = Input.Core.Bind(RAGE.Ui.VirtualKeys.Escape, true, () => Close(false));
        }

        public static void ShowTest(int questionsAmount)
        {
            if (!IsActive || IsActiveTest)
                return;

            Input.Core.Unbind(EscBindIdx);

            EscBindIdx = -1;

            IsActiveTest = true;

            Browser.Window.ExecuteJs("AutoSchool.drawTest", questionsAmount);
        }

        public static void Close(bool success)
        {
            if (!IsActive)
                return;

            CloseColshape?.Destroy();

            CloseColshape = null;

            Browser.Render(Browser.IntTypes.AutoschoolTest, false, false);

            Cursor.Show(false, false);

            if (EscBindIdx >= 0)
            {
                Input.Core.Unbind(EscBindIdx);

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