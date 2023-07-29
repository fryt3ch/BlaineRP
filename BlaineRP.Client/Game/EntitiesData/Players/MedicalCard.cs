using System;
using BlaineRP.Client.Game.Fractions;
using Newtonsoft.Json;

namespace BlaineRP.Client.Game.EntitiesData.Players
{
    public class MedicalCard
    {
        public enum DiagnoseTypes
        {
            [Language.Localized("MEDCARD_DIAGNOSIS_0", "DIAGNOSE_NAME")]
            Healthy = 0,

            [Language.Localized("MEDCARD_DIAGNOSIS_1", "DIAGNOSE_NAME")]
            DrugAddicted1,

            [Language.Localized("MEDCARD_DIAGNOSIS_2", "DIAGNOSE_NAME")]
            DrugAddicted2,

            [Language.Localized("MEDCARD_DIAGNOSIS_3", "DIAGNOSE_NAME")]
            DrugAddicted3,
        }

        public MedicalCard()
        {
        }

        [JsonProperty(PropertyName = "I")]
        public DateTime IssueDate { get; set; }

        [JsonProperty(PropertyName = "F")]
        public FractionTypes IssueFraction { get; set; }

        [JsonProperty(PropertyName = "N")]
        public string DoctorName { get; set; }

        [JsonProperty(PropertyName = "D")]
        public DiagnoseTypes Diagnose { get; set; }

        public static string GetDiagnoseNameKey(DiagnoseTypes diagType)
        {
            return Language.Strings.GetKeyFromTypeByMemberName(typeof(DiagnoseTypes), diagType.ToString(), "DIAGNOSE_NAME");
        }
    }
}