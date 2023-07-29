using System.Collections.Generic;
using BlaineRP.Client.Game.Animations;
using RAGE.Elements;

namespace BlaineRP.Client.Game.Casino
{
    public partial class CasinoEntity
    {
        [Script]
        public class Initialization
        {
            public Initialization()
            {
                #region TO_REPLACE

                #endregion
                
                Animation casinoSlotMachineIdle0Anim = Service.GeneralAnimsList.GetValueOrDefault(GeneralType.CasinoSlotMachineIdle0);

                if (casinoSlotMachineIdle0Anim != null)
                {
                    casinoSlotMachineIdle0Anim.StartAction = (entity, anim) =>
                    {
                        var ped = entity as PedBase;

                        if (ped?.Exists != true)
                            return;

                        ped.FreezePosition(true);
                        ped.SetCollision(false, true);
                    };

                    casinoSlotMachineIdle0Anim.StopAction = (entity, anim) =>
                    {
                        var ped = entity as PedBase;

                        if (ped?.Exists != true)
                            return;

                        ped.FreezePosition(false);
                        ped.SetCollision(true, false);
                    };
                }
            }
        }
    }
}