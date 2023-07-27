using System;
using GTANetworkAPI;
using Newtonsoft.Json;

namespace BlaineRP.Server.EntityData.Players
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

            public void Show(Player target, PlayerInfo holder)
            {
                target.TriggerEvent("Documents::Show", 3, holder.Name, holder.Surname, Diagnose, IssueFraction, DoctorName, IssueDate.SerializeToJson());
            }

            public static DiagnoseTypes GetPlayerDiagnose(PlayerData pData)
            {
                if (pData.DrugAddiction >= Properties.Settings.Static.PlayerDrugAddictionStage1)
                {
                    if (pData.DrugAddiction >= Properties.Settings.Static.PlayerDrugAddictionStage2)
                    {
                        if (pData.DrugAddiction >= Properties.Settings.Static.PlayerDrugAddictionStage3)
                        {
                            return DiagnoseTypes.DrugAddicted3;
                        }
                        else
                        {
                            return DiagnoseTypes.DrugAddicted3;
                        }
                    }
                    else
                    {
                        return DiagnoseTypes.DrugAddicted1;
                    }
                }
                else
                {
                    return DiagnoseTypes.Healthy;
                }
            }
    }
}
