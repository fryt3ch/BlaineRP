using System.Collections.Generic;
using BlaineRP.Server.UtilsT;
using GTANetworkAPI;

namespace BlaineRP.Server.Game.Businesses
{
    public class MotoShop : VehicleShop
    {
        public static BusinessType DefaultType => BusinessType.MotoShop;

        public static MaterialsData InitMaterialsData => new MaterialsData(5, 7, 50)
        {
            Prices = new Dictionary<string, uint>()
            {
                { "akuma", 100 },
                { "avarus", 100 },
                { "bagger", 100 },
                { "bati", 100 },
                { "bati2", 100 },
                { "bf400", 100 },
                { "carbonrs", 100 },
                { "chimera", 100 },
                { "cliffhanger", 100 },
                { "daemon", 100 },
                { "daemon2", 100 },
                { "deathbike", 100 },
                { "deathbike2", 100 },
                { "deathbike3", 100 },
                { "defiler", 100 },
                { "diablous", 100 },
                { "diablous2", 100 },
                { "double", 100 },
                { "enduro", 100 },
                { "esskey", 100 },
                { "faggio", 100 },
                { "faggio2", 100 },
                { "faggio3", 100 },
                { "fcr", 100 },
                { "fcr2", 100 },
                { "gargoyle", 100 },
                { "hakuchou", 100 },
                { "hakuchou2", 100 },
                { "hexer", 100 },
                { "innovation", 100 },
                { "lectro", 100 },
                { "manchez", 100 },
                { "manchez2", 100 },
                { "nemesis", 100 },
                { "nightblade", 100 },
                { "oppressor", 100 },
                { "oppressor2", 100 },
                { "pcj", 100 },
                { "ratbike", 100 },
                { "ruffian", 100 },
                { "rrocket", 100 },
                { "sanchez", 100 },
                { "sanchez2", 100 },
                { "sanctus", 100 },
                { "shotaro", 100 },
                { "sovereign", 100 },
                { "stryder", 100 },
                { "thrust", 100 },
                { "vader", 100 },
                { "vindicator", 100 },
                { "vortex", 100 },
                { "wolfsbane", 100 },
                { "zombiea", 100 },
                { "zombieb", 100 },
                { "policeb", 100 },
                { "bmx", 100 },
                { "cruiser", 100 },
                { "fixter", 100 },
                { "scorcher", 100 },
                { "tribike", 100 },
                { "tribike2", 100 },
                { "tribike3", 100 },
            }
        };

        public MotoShop(int ID, Vector3 Position, Vector4 EnterProperties, Vector4[] AfterBuyPositions, Vector4 PositionInteract) : base(ID, Position, EnterProperties, DefaultType, AfterBuyPositions, PositionInteract)
        {

        }
    }
}