﻿using Newtonsoft.Json.Linq;
using RAGE;
using RAGE.Elements;
using System;
using System.Collections.Generic;

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

        private static int EscBindIdx { get; set; } = -1;

        private static Action<object[]> CurrentPoliceBlankAction { get; set; }

        public Documents()
        {
            Events.Add("Docs::Police", (args) =>
            {
                CurrentPoliceBlankAction?.Invoke(args);
            });

            Events.Add("Documents::Show", (args) =>
            {
                var type = (int)args[0];

                if (type == 0)
                {
                    var name = (string)args[1];
                    var surname = (string)args[2];
                    var sex = (bool)args[3];
                    var birthdate = RAGE.Util.Json.Deserialize<DateTime>((string)args[4]);
                    var married = (string)args[5];
                    var cid = (uint)(int)args[6];
                    var issueDate = RAGE.Util.Json.Deserialize<DateTime>((string)args[7]);
                    var mil = (bool)args[8];
                    var lsa = (bool)args[9];

                    ShowPasport(name, surname, sex, birthdate, married, cid, issueDate, mil, lsa);
                }
                else if (type == 1)
                {
                    var name = (string)args[1];
                    var surname = (string)args[2];
                    var lics = ((JArray)args[3]).ToObject<List<Sync.Players.LicenseTypes>>();

                    ShowLicenses(name, surname, lics);
                }
                else if (type == 2)
                {
                    var vName = (string)args[1];
                    var oName = (string)args[2];
                    var oSurname = (string)args[3];
                    var vid = (uint)(int)args[4];
                    var oCount = (uint)(int)args[5];
                    var nPlate = (string)args[6];
                    var issueDate = RAGE.Util.Json.Deserialize<DateTime>((string)args[7]);

                    ShowVehiclePassport(vName, oName, oSurname, vid, oCount, nPlate, issueDate);
                }
                else if (type == 3)
                {
                    var name = (string)args[1];
                    var surname = (string)args[2];

                    var diagnose = (Sync.Players.MedicalCard.DiagnoseTypes)(int)args[3];
                    var issueFraction = (Data.Fractions.Types)(int)args[4];
                    var docName = (string)args[5];
                    var issueDate = RAGE.Util.Json.Deserialize<DateTime>((string)args[6]);

                    ShowMedicalCard(name, surname, diagnose, issueFraction, docName, issueDate);
                }
            });
        }

        public static async System.Threading.Tasks.Task ReadyDefault()
        {
            await CEF.Browser.Render(Browser.IntTypes.Documents, true, true);

            EscBindIdx = KeyBinds.Bind(RAGE.Ui.VirtualKeys.Escape, true, () => Close());

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

            var medCard = pData.MedicalCard;

            var ns = Player.LocalPlayer.GetNameSurname();

            var pas = GetPasportData(ns.Name, ns.Surname, pData.Sex, CEF.Menu.BirthDate, null, pData.CID, CEF.Menu.CreationDate, false, false);
            var lic = GetLicensesData(ns.Name, ns.Surname, pData.Licenses);

            var med = medCard == null ? null : GetMedicalCardData(ns.Name, ns.Surname, medCard.Diagnose, medCard.IssueFraction, medCard.DoctorName, medCard.IssueDate);

            var res = GetResumeData(ns.Name, ns.Surname, null);

            var fractionData = pData.CurrentFraction is Data.Fractions.Fraction fData ? GetFractionData(ns.Name, ns.Surname, fData, Data.Fractions.Fraction.AllMembers.GetValueOrDefault(pData.CID)?.Rank ?? 0) : null;

            CEF.Browser.Window.ExecuteJs("Docs.show", true, 0, new object[] { pas, lic, med, fractionData, res, null });
        }

        public static async System.Threading.Tasks.Task ShowPasport(string name, string surname, bool sex, DateTime birthDate, string married, uint cid, DateTime dateOfIssue, bool boundToMilitaryService, bool losSantosAllowed)
        {
            if (IsActive)
                return;

            await ReadyDefault();

            CEF.Browser.Window.ExecuteJs("Docs.show", false, 0, new object[] { GetPasportData(name, surname, sex, birthDate, married, cid, dateOfIssue, boundToMilitaryService, losSantosAllowed) });
        }

        public static async System.Threading.Tasks.Task ShowLicenses(string name, string surname, List<Sync.Players.LicenseTypes> licenses)
        {
            if (IsActive)
                return;

            await ReadyDefault();

            CEF.Browser.Window.ExecuteJs("Docs.show", false, 1, new object[] { null, GetLicensesData(name, surname, licenses) });
        }

        public static async System.Threading.Tasks.Task ShowMedicalCard(string name, string surname, Sync.Players.MedicalCard.DiagnoseTypes diagnose, Data.Fractions.Types issueFraction, string docName, DateTime dateOfIssue)
        {
            if (IsActive)
                return;

            await ReadyDefault();

            CEF.Browser.Window.ExecuteJs("Docs.show", false, 2, new object[] { null, null, GetMedicalCardData(name, surname, diagnose, issueFraction, docName, dateOfIssue) });
        }

        public static async System.Threading.Tasks.Task ShowVehiclePassport(string vName, string oName, string oSurname, uint vid, uint oCount, string plate, DateTime dateOfIssue)
        {
            if (IsActive)
                return;

            await ReadyDefault();

            CEF.Browser.Window.ExecuteJs("Docs.show", false, 5, new object[] { null, null, null, null, null, GetVehiclePassportData(vName, oName, oSurname, vid, oCount, plate, dateOfIssue) });
        }

        public static async System.Threading.Tasks.Task ShowPoliceBlank(bool isCop, string title, string criminal, string officer, string date, string[] completedData, Action<object[]> action)
        {
            if (IsActive)
                return;

            await ReadyDefault();

            CEF.Browser.Window.ExecuteJs("Docs.policeBlank", new object[] { new object[] { isCop, title, criminal, officer, date, completedData } });

            CurrentPoliceBlankAction = action;
        }

        public static void Close()
        {
            if (!IsActive)
                return;

            CEF.Browser.Render(Browser.IntTypes.Documents, false);

            Cursor.Show(false, false);

            KeyBinds.Unbind(EscBindIdx);

            EscBindIdx = -1;

            CurrentPoliceBlankAction = null;
        }

        public static object[] GetPasportData(string name, string surname, bool sex, DateTime birthDate, string married, uint cid, DateTime dateOfIssue, bool boundToMilitaryService, bool losSantosAllowed) => new object[] { name, surname, Locale.Get(sex ? "DOCS_SEX_MALE" : "DOCS_SEX_FEMALE"), birthDate.ToString("dd.MM.yyyy"), married ?? Locale.Get(sex ? "DOCS_NOTMARRIED_MALE" : "DOCS_NOTMARRIED_FEMALE"), cid, dateOfIssue.ToString("dd.MM.yyyy"), boundToMilitaryService, losSantosAllowed };
        public static object[] GetResumeData(string name, string surname, string[] data1) => new object[] { name, surname, new object[] { new object[] { new object[] { "side1-a", "side1-b" } }, new object[] { new object[] { "side2-a", "side2-b" } } } };
        public static object[] GetLicensesData(string name, string surname, List<Sync.Players.LicenseTypes> licenses)
        {
            return new object[] { name, surname, new object[] { new object[] { licenses.Contains(Sync.Players.LicenseTypes.M), licenses.Contains(Sync.Players.LicenseTypes.A), licenses.Contains(Sync.Players.LicenseTypes.B), licenses.Contains(Sync.Players.LicenseTypes.C), licenses.Contains(Sync.Players.LicenseTypes.D), licenses.Contains(Sync.Players.LicenseTypes.Fly), licenses.Contains(Sync.Players.LicenseTypes.Sea) },
                new object[] { licenses.Contains(Sync.Players.LicenseTypes.Weapons), licenses.Contains(Sync.Players.LicenseTypes.Business), licenses.Contains(Sync.Players.LicenseTypes.Hunting), licenses.Contains(Sync.Players.LicenseTypes.Lawyer), false, false, false } } };
        }

        public static object[] GetVehiclePassportData(string vName, string oName, string oSurname, uint vid, uint oCount, string plate, DateTime dateOfIssue) => new object[] { vName, $"{oName} {oSurname}", $"#{vid}", oCount, plate ?? Locale.Get("DOCS_VEHPASS_NOPLATE"), dateOfIssue.ToString("dd.MM.yyyy") };

        public static object[] GetMedicalCardData(string name, string surname, Sync.Players.MedicalCard.DiagnoseTypes diagnose, Data.Fractions.Types issueFraction, string docName, DateTime dateOfIssue) => new object[] { name, surname, diagnose.ToString(), issueFraction.ToString(), docName, dateOfIssue.ToString("dd.MM.yyyy") };

        public static object[] GetFractionData(string name, string surname, Data.Fractions.Fraction fData, byte rank) => new object[]
        {
            Data.Fractions.Fraction.IsFractionPolice(fData) ? "cop" : Data.Fractions.Fraction.IsFractionMedical(fData) ? "med" : Data.Fractions.Fraction.IsFractionArmy(fData) ? "army" : "press",

            fData.Name,

            name, surname,

            $"{rank} - {fData.GetRankName(rank)}",
        };
    }
}
