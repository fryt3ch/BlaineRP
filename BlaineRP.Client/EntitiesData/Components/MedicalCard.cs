using System;
using Newtonsoft.Json;

namespace BlaineRP.Client.EntitiesData.Components
{
    public class MedicalCard
    {
        public enum DiagnoseTypes
        {
            Healthy = 0,
            DrugAddicted1,
            DrugAddicted2,
            DrugAddicted3,
        }

        [JsonProperty(PropertyName = "I")]
        public DateTime IssueDate { get; set; }

        [JsonProperty(PropertyName = "F")]
        public Data.Fractions.Types IssueFraction { get; set; }

        [JsonProperty(PropertyName = "N")]
        public string DoctorName { get; set; }

        [JsonProperty(PropertyName = "D")]
        public DiagnoseTypes Diagnose { get; set; }

        public MedicalCard() { }

        public static string GetDiagnoseNameId(DiagnoseTypes diagType) => $"MEDCARD_DIAGNOSIS_{(int)diagType}";
    }
}