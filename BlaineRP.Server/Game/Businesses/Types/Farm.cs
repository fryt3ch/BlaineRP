using GTANetworkAPI;
using System;
using System.Collections.Generic;
using BlaineRP.Server.EntityData.Players;

namespace BlaineRP.Server.Game.Businesses
{
    public partial class Farm : Business
    {
        public static BusinessTypes DefaultType => BusinessTypes.Farm;

        public static MaterialsData InitMaterialsData => new MaterialsData(5, 7, 9)
        {
            Prices = new Dictionary<string, uint>()
            {
                { $"crop_{(int)CropField.Types.Cabbage}_0", 2 },
                { $"crop_{(int)CropField.Types.Cabbage}_1", 2 },
                { $"crop_{(int)CropField.Types.Cabbage}_2", 100 },

                { $"crop_{(int)CropField.Types.Pumpkin}_0", 2 },
                { $"crop_{(int)CropField.Types.Pumpkin}_1", 2 },
                { $"crop_{(int)CropField.Types.Pumpkin}_2", 100 },

                { $"crop_{(int)CropField.Types.Wheat}_0", 2 },
                { $"crop_{(int)CropField.Types.Wheat}_1", 3 },
                { $"crop_{(int)CropField.Types.Wheat}_2", 50 },

                { $"crop_{(int)CropField.Types.OrangeTree}_0", 2 },
                { $"crop_{(int)CropField.Types.OrangeTree}_1", 2 },

                { $"crop_{(int)CropField.Types.Cow}_1", 2 },
            }
        };

        public List<CropField> CropFields { get; set; }

        public List<OrangeTreeData> OrangeTrees { get; set; }

        public List<Vector3> OrangeTreeBoxPositions { get; set; }

        public List<Vector3> CowBucketPositions { get; set; }

        public List<CowData> Cows { get; set; }

        public Farm(int ID, Vector3 PositionInfo, Utils.Vector4 PositionInteract) : base(ID, PositionInfo, PositionInteract, BusinessTypes.Farm)
        {

        }

        public override string ClientData => $"{ID}, {PositionInfo.ToCSharpStr()}, {PositionInteract.ToCSharpStr()}, {GovPrice}, {Rent}, {Tax}f, \"{CropFields.SerializeToJson().Replace('"', '\'')}\", \"{OrangeTrees.SerializeToJson().Replace('"', '\'')}\", \"{Cows.SerializeToJson().Replace('"', '\'')}\", \"{OrangeTreeBoxPositions.SerializeToJson().Replace('"', '\'')}\", \"{CowBucketPositions.SerializeToJson().Replace('"', '\'')}\"";

        public bool TryProceedPayment(PlayerData pData, string itemId, decimal salaryCoef, out uint newMats, out ulong newBalance, out uint newPlayerBalance)
        {
            try
            {
                var matData = MaterialsData;

                var matPrice = matData.Prices[itemId];

                var hasMaterials = true;

                if (Owner != null)
                {
                    if (!TryRemoveMaterials(matPrice, out newMats, false, null))
                    {
                        hasMaterials = false;

                        newMats = Materials;
                    }
                }
                else
                {
                    newMats = 0;
                }

                var realPriceP = Math.Floor((decimal)matPrice * matData.RealPrice * (2m - Margin) * salaryCoef);

                if (realPriceP < 0)
                    realPriceP = 0;

                newPlayerBalance = Game.Jobs.Job.GetPlayerTotalCashSalary(pData) + (uint)realPriceP;

                if (hasMaterials)
                {
                    var bizPrice = GetBusinessPrice(matPrice, false);

                    if (!TryAddMoneyCash(bizPrice, out newBalance, true, pData))
                        return false;
                }
                else
                {
                    newBalance = Cash;
                }

                return true;
            }
            catch (Exception)
            {
                newMats = 0;
                newBalance = 0;
                newPlayerBalance = 0;

                return false;
            }
        }

        public void ProceedPayment(PlayerData pData, uint newMats, ulong newBalance, uint newPlayerBalance)
        {
            if (Owner != null)
            {
                if (newMats != Materials)
                    SetMaterials(newMats);

                if (newBalance > Cash)
                    UpdateStatistics(newBalance - Cash);

                SetCash(newBalance);

                MySQL.BusinessUpdateBalances(this, false);
            }

            Game.Jobs.Job.SetPlayerTotalCashSalary(pData, newPlayerBalance, true);
        }
    }
}
