using RAGE;
using RAGE.Elements;
using System;
using System.Collections.Generic;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Threading;

namespace BCRPClient.CEF
{
    class Documents : Events.Script
    {
        public static bool IsActive { get => CEF.Browser.IsActive(Browser.IntTypes.Documents); }

        public enum ReplyTypes
        {
            OK = 0, Cancel = 1,
        }

        private static DateTime LastSent;

        private static List<int> TempBinds { get; set; }

        public Documents()
        {
            LastSent = DateTime.MinValue;

            TempBinds = new List<int>();

            Events.Add("Documents::PoliceBlank::Reply", (object[] args) =>
            {
                ReplyTypes rType = (ReplyTypes)args[0];

                if (rType == ReplyTypes.OK)
                {
                    string text1 = (string)args[1];
                    string text2 = (string)args[2];
                    string text3 = (string)args[3];

                    Close();
                }
                else if (rType == ReplyTypes.Cancel)
                {
                    Close();
                }
                else
                    return;
            });
        }

        public static async System.Threading.Tasks.Task ReadyDefault()
        {
            await CEF.Browser.Render(Browser.IntTypes.Documents, true, true);

            TempBinds.Add(RAGE.Input.Bind(RAGE.Ui.VirtualKeys.Escape, true, () => Close()));

            Cursor.Show(true, true);
        }

        public static async System.Threading.Tasks.Task Show()
        {
            if (IsActive)
                return;

            var pData = Sync.Players.GetData(Player.LocalPlayer);

            if (pData == null)
                return;

            await ReadyDefault();

            var ns = Player.LocalPlayer.GetNameSurname();

            var pas = GetPasportData(ns.Name, ns.Surname, pData.Sex, CEF.Menu.BirthDate, null, pData.CID, CEF.Menu.CreationDate, false, false);
            var lic = GetLicensesData(ns.Name, ns.Surname, pData.Licenses);
            var res = GetResumeData(ns.Name, ns.Surname, null);

            CEF.Browser.Window.ExecuteJs("Docs.show", true, 0, new object[] { pas, lic, null, null, res, null });
        }

        public static async System.Threading.Tasks.Task ShowPasport(string name, string surname, bool sex, DateTime birthDate, string married, uint cid, DateTime dateOfIssue, bool boundToMilitaryService, bool losSantosAllowed)
        {
            if (IsActive)
                return;

            await ReadyDefault();

            CEF.Browser.Window.ExecuteJs("Docs.show", false, 0, new object[] { GetPasportData(name, surname, sex, birthDate, married, cid, dateOfIssue, boundToMilitaryService, losSantosAllowed) });
        }

        public static async System.Threading.Tasks.Task ShowPoliceBlank(bool isCop, string title, string criminal, string officer, string date, string[] completedData)
        {
            if (IsActive)
                return;

            await ReadyDefault();

            CEF.Browser.Window.ExecuteJs("Docs.policeBlank", new object[] { new object[] { isCop, title, criminal, officer, date, completedData } });
        }

        public static void Close()
        {
            if (!IsActive)
                return;

            CEF.Browser.Render(Browser.IntTypes.Documents, false);

            Cursor.Show(false, false);

            for (int i = 0; i < TempBinds.Count; i++)
                RAGE.Input.Unbind(TempBinds[i]);

            TempBinds.Clear();
        }

        public static object[] GetPasportData(string name, string surname, bool sex, DateTime birthDate, string married, uint cid, DateTime dateOfIssue, bool boundToMilitaryService, bool losSantosAllowed) => new object[] { name, surname, sex ? Locale.General.Documents.SexMale : Locale.General.Documents.SexFemale, birthDate.ToString("dd.MM.yyyy"), married ?? (sex ? Locale.General.Documents.NotMarriedMale : Locale.General.Documents.NotMarriedFemale), cid, dateOfIssue.ToString("dd.MM.yyyy"), boundToMilitaryService, losSantosAllowed };
        public static object[] GetResumeData(string name, string surname, string[] data) => new object[] { name, surname, new object[] { new object[] { new object[] { "side1-a", "side1-b" }  }, new object[] { new object[] { "side2-a", "side2-b" } } } };
        public static object[] GetLicensesData(string name, string surname, List<Sync.Players.LicenseTypes> licenses)
        {

            return new object[] { name, surname, new object[] { new object[] { licenses.Contains(Sync.Players.LicenseTypes.M), licenses.Contains(Sync.Players.LicenseTypes.A), licenses.Contains(Sync.Players.LicenseTypes.B), licenses.Contains(Sync.Players.LicenseTypes.C), licenses.Contains(Sync.Players.LicenseTypes.D), licenses.Contains(Sync.Players.LicenseTypes.Fly), licenses.Contains(Sync.Players.LicenseTypes.Sea) },
                new object[] { licenses.Contains(Sync.Players.LicenseTypes.Weapons), licenses.Contains(Sync.Players.LicenseTypes.Business), licenses.Contains(Sync.Players.LicenseTypes.Hunting), licenses.Contains(Sync.Players.LicenseTypes.Lawyer), false, false, false } } };
        }
    }
}
