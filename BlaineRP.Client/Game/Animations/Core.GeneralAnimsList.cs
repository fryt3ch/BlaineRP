using System.Collections.Generic;
using BlaineRP.Client.Game.Animations.Enums;

namespace BlaineRP.Client.Game.Animations
{
    public partial class Core
    {
        public static Dictionary<GeneralTypes, Animation> GeneralAnimsList { get; private set; } =
            new Dictionary<GeneralTypes, Animation>()
            {
                {
                    GeneralTypes.Knocked,
                    new Animation("random@dealgonewrong", "idle_a", 2f, 2f, -1, 1, 0, false, false, false)
                },
                {
                    GeneralTypes.PushingVehicle,
                    new Animation("missfinale_c2ig_11", "pushcar_offcliff_m", 2f, 2f, -1, 35, 0, false, false, false)
                },
                {
                    GeneralTypes.RagdollElectrocute,
                    new Animation("ragdoll@human", "electrocute", 8f, 8f, -1, 39, 0, false, false, false)
                },
                {
                    GeneralTypes.CarryA,
                    new Animation("missfinale_c2mcs_1", "fin_c2_mcs_1_camman", 8f, -8f, -1, 48, 0, false, false, false)
                },
                { GeneralTypes.CarryB, new Animation("nm", "firemans_carry", 2f, 2f, -1, 1, 0, false, false, false) },
                {
                    GeneralTypes.PiggyBackA,
                    new Animation("anim@arena@celeb@flat@paired@no_props@",
                        "piggyback_c_player_a",
                        8f,
                        -8f,
                        -1,
                        33,
                        0,
                        false,
                        false,
                        false)
                },
                {
                    GeneralTypes.PiggyBackB,
                    new Animation("anim@arena@celeb@flat@paired@no_props@",
                        "piggyback_c_player_b",
                        8f,
                        -8f,
                        -1,
                        49,
                        0,
                        false,
                        false,
                        false)
                },
                {
                    GeneralTypes.HostageA,
                    new Animation("anim@gangops@hostage@", "perp_idle", 8f, -8f, -1, 49, 0, false, false, false)
                },
                {
                    GeneralTypes.HostageB,
                    new Animation("anim@gangops@hostage@", "victim_idle", 8f, -8f, -1, 49, 0, false, false, false)
                },
                {
                    GeneralTypes.LieInTrunk,
                    new Animation("timetable@floyd@cryingonbed@base", "base", 8f, -8f, -1, 1, 0, false, false, false)
                },
                {
                    GeneralTypes.FishingIdle0,
                    new Animation("amb@world_human_stand_fishing@base", "base", 2f, 2f, -1, 1, 0, false, false, false)
                },
                {
                    GeneralTypes.FishingProcess0,
                    new Animation("amb@world_human_stand_fishing@idle_a",
                        "idle_b",
                        2f,
                        2f,
                        -1,
                        1,
                        0,
                        false,
                        false,
                        false)
                },
                {
                    GeneralTypes.ShovelProcess0,
                    new Animation("random@burial", "a_burial", 2f, 2f, -1, 1, 0, false, false, false)
                },
                {
                    GeneralTypes.MetalDetectorProcess0,
                    new Animation("mini@golfai", "wood_idle_a", 2f, 2f, -1, 49, 0f, false, false, false)
                },
                {
                    GeneralTypes.CuffedStatic,
                    new Animation("mp_arresting", "idle", 8f, -8f, -1, 49, 1, false, false, false)
                },
                {
                    GeneralTypes.FarmPlantSmallShovelProcess0,
                    new Animation("amb@world_human_gardener_plant@male@base",
                        "base",
                        2f,
                        2f,
                        -1,
                        1,
                        0f,
                        false,
                        false,
                        false)
                },
                {
                    GeneralTypes.BoxCarry0,
                    new Animation("anim@heists@box_carry@", "idle", 2f, 2f, -1, 49, 0f, false, false, false)
                },
                {
                    GeneralTypes.WateringCan0,
                    new Animation("missarmenian3_gardener", "blower_idle_a", 2f, 2f, -1, 1, 0f, false, false, false)
                }, // static - amb@lo_res_idles@ world_human_gardener_leaf_blower_lo_res_base

                {
                    GeneralTypes.TreeCollect0,
                    new Animation("amb@prop_human_movie_bulb@base", "base", 2f, 2f, -1, 1, 0f, false, false, false)
                },
                {
                    GeneralTypes.BucketCarryOneHand0,
                    new Animation("move_bucket", "idle", 2f, 2f, -1, 49, 0f, false, false, false)
                },
                {
                    GeneralTypes.MilkCow0,
                    new Animation("amb@prop_human_parking_meter@female@base",
                        "base_female",
                        2f,
                        2f,
                        -1,
                        17,
                        0f,
                        false,
                        false,
                        false)
                },
                {
                    GeneralTypes.PoliceEscort0,
                    new Animation("amb@world_human_drinking@coffee@female@base",
                        "base",
                        8f,
                        -8f,
                        -1,
                        49,
                        0f,
                        false,
                        false,
                        false)
                },
                {
                    GeneralTypes.BedLie0,
                    new Animation("amb@world_human_bum_slumped@male@laying_on_left_side@base",
                        "base",
                        8f,
                        -8f,
                        -1,
                        1,
                        0,
                        false,
                        false,
                        false)
                },
                {
                    GeneralTypes.CasinoSlotMachineIdle0,
                    new Animation("amb@code_human_in_bus_passenger_idles@male@sit@base",
                        "base",
                        8f,
                        -8f,
                        -1,
                        1,
                        1f,
                        false,
                        false,
                        false)
                },
                {
                    GeneralTypes.CasinoBlackjackIdle0,
                    new Animation("anim_casino_b@amb@casino@games@shared@player@",
                        "idle_a",
                        8f,
                        -8f,
                        -1,
                        1,
                        1f,
                        false,
                        false,
                        false)
                },
                {
                    GeneralTypes.MedicalRevive,
                    new Animation("mini@cpr@char_a@cpr_str", "cpr_pumpchest", 8f, -8f, -1, 1, 1f, false, false, false)
                },
            };
    }
}