﻿using GTANetworkAPI;
using Newtonsoft.Json;
using System;

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
            public Game.Fractions.Types IssueFraction { get; set; }

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

            public void Show(Player target, PlayerData.PlayerInfo holder)
            {
                target.TriggerEvent("Documents::Show", 3, holder.Name, holder.Surname, Diagnose, IssueFraction, DoctorName, IssueDate.SerializeToJson());
            }
        }
    }
}
