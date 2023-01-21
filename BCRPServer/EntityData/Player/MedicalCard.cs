using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace BCRPServer
{
    public partial class PlayerData
    {
        public class MedicalCard
        {
            public enum DiagnoseTypes
            {
                Healthy = 0,
            }

            [JsonProperty(PropertyName = "I")]
            public DateTime IssueDate { get; set; }

            [JsonProperty(PropertyName = "F")]
            public FractionTypes IssueFraction { get; set; }

            [JsonProperty(PropertyName = "N")]
            public string DoctorName { get; set; }

            [JsonProperty(PropertyName = "D")]
            public DiagnoseTypes Diagnose { get; set; }

            public MedicalCard() { }

            public MedicalCard(DateTime IssueDate, PlayerInfo DoctorInfo, DiagnoseTypes Diagnose)
            {
                this.IssueDate = IssueDate;

                this.IssueFraction = DoctorInfo.Fraction;
                this.DoctorName = $"{DoctorInfo.Name} {DoctorInfo.Surname}";

                this.Diagnose = Diagnose;
            }
        }
    }
}
