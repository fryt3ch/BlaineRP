using System.Collections.Generic;

namespace BlaineRP.Client.Game.Animations
{
    public partial class Core
    {
        public static Dictionary<FastTypes, Animation> FastAnimsList { get; private set; } = new Dictionary<FastTypes, Animation>()
        {
            {
                FastTypes.VehLocking, new Animation("anim@mp_player_intmenu@key_fob@", "fob_click", 8f, 1f, 1500, 50, 0f, false, false, false)
                {
                    NameFP = "fob_click_fp",
                }
            },
            { FastTypes.Pickup, new Animation("pickup_object", "pickup_low", 8f, 1f, 750, 48, 0f, false, false, false) },
            { FastTypes.Putdown, new Animation("pickup_object", "putdown_low", 8f, 1f, 750, 48, 0f, false, false, false) },
            { FastTypes.Handshake, new Animation("mp_ped_interaction", "handshake_guy_a", 8f, 1f, 4000, 16, 0f, false, false, false) },
            { FastTypes.Whistle, new Animation("rcmnigel1c", "hailing_whistle_waive_a", 2.7f, 2.7f, 2000, 49, 0f, false, false, false) },
            { FastTypes.SmokePuffCig, new Animation("amb@world_human_aa_smoke@male@idle_a", "idle_a", 8f, 1f, 2800, 49, 0f, false, false, false) },
            { FastTypes.SmokeTransitionCig, new Animation("move_p_m_two_idles@generic", "fidget_sniff_fingers", 8f, 1f, 1000, 49, 0f, false, false, false) },
            { FastTypes.ItemChips, new Animation("amb@code_human_wander_eating_donut@female@idle_a", "idle_c", 8f, 1f, 6000, 49, 0f, false, false, false) },
            { FastTypes.ItemBurger, new Animation("amb@code_human_wander_eating_donut@female@idle_a", "idle_b", 8f, 1f, 6000, 49, 0f, false, false, false) },
            { FastTypes.ItemHotdog, new Animation("mp_player_inteat@burger", "mp_player_int_eat_burger", 8f, 1f, 10000, 49, 0f, false, false, false) },
            { FastTypes.ItemChocolate, new Animation("amb@world_human_seat_wall_eating@male@both_hands@base", "idle_b", 8f, 1f, 6000, 49, 0f, false, false, false) },
            { FastTypes.ItemPizza, new Animation("amb@code_human_wander_eating_donut_fat@male@idle_a", "idle_a", 8f, 1f, 6000, 49, 0f, false, false, false) },
            { FastTypes.ItemCola, new Animation("amb@world_human_drinking@coffee@female@idle_a", "idle_a", 8f, 1f, 6000, 49, 0f, false, false, false) },
            { FastTypes.ItemBeer, new Animation("amb@world_human_drinking@beer@male@idle_a", "idle_b", 8f, 1f, 6000, 49, 0f, false, false, false) },
            { FastTypes.ItemBandage, new Animation("oddjobs@bailbond_hobotwitchy", "base", 8f, 1f, 4000, 49, 0f, false, false, false) },
            { FastTypes.ItemMedKit, new Animation("anim@amb@office@boardroom@crew@female@var_b@base@", "idle_a", 8f, 1f, 7000, 49, 0f, false, false, false) },
        };
    }
}