using System.Collections.Generic;

namespace BlaineRP.Client.Game.Animations
{
    public partial class Service
    {
        public static Dictionary<FastType, Animation> FastAnimsList { get; private set; } = new Dictionary<FastType, Animation>()
        {
            {
                FastType.VehLocking, new Animation("anim@mp_player_intmenu@key_fob@", "fob_click", 8f, 1f, 1500, 50, 0f, false, false, false)
                {
                    NameFP = "fob_click_fp",
                }
            },
            { FastType.Pickup, new Animation("pickup_object", "pickup_low", 8f, 1f, 750, 48, 0f, false, false, false) },
            { FastType.Putdown, new Animation("pickup_object", "putdown_low", 8f, 1f, 750, 48, 0f, false, false, false) },
            { FastType.Handshake, new Animation("mp_ped_interaction", "handshake_guy_a", 8f, 1f, 4000, 16, 0f, false, false, false) },
            { FastType.Whistle, new Animation("rcmnigel1c", "hailing_whistle_waive_a", 2.7f, 2.7f, 2000, 49, 0f, false, false, false) },
            { FastType.SmokePuffCig, new Animation("amb@world_human_aa_smoke@male@idle_a", "idle_a", 8f, 1f, 2800, 49, 0f, false, false, false) },
            { FastType.SmokeTransitionCig, new Animation("move_p_m_two_idles@generic", "fidget_sniff_fingers", 8f, 1f, 1000, 49, 0f, false, false, false) },
            { FastType.ItemChips, new Animation("amb@code_human_wander_eating_donut@female@idle_a", "idle_c", 8f, 1f, 6000, 49, 0f, false, false, false) },
            { FastType.ItemBurger, new Animation("amb@code_human_wander_eating_donut@female@idle_a", "idle_b", 8f, 1f, 6000, 49, 0f, false, false, false) },
            { FastType.ItemHotdog, new Animation("mp_player_inteat@burger", "mp_player_int_eat_burger", 8f, 1f, 10000, 49, 0f, false, false, false) },
            { FastType.ItemChocolate, new Animation("amb@world_human_seat_wall_eating@male@both_hands@base", "idle_b", 8f, 1f, 6000, 49, 0f, false, false, false) },
            { FastType.ItemPizza, new Animation("amb@code_human_wander_eating_donut_fat@male@idle_a", "idle_a", 8f, 1f, 6000, 49, 0f, false, false, false) },
            { FastType.ItemCola, new Animation("amb@world_human_drinking@coffee@female@idle_a", "idle_a", 8f, 1f, 6000, 49, 0f, false, false, false) },
            { FastType.ItemBeer, new Animation("amb@world_human_drinking@beer@male@idle_a", "idle_b", 8f, 1f, 6000, 49, 0f, false, false, false) },
            { FastType.ItemBandage, new Animation("oddjobs@bailbond_hobotwitchy", "base", 8f, 1f, 4000, 49, 0f, false, false, false) },
            { FastType.ItemMedKit, new Animation("anim@amb@office@boardroom@crew@female@var_b@base@", "idle_a", 8f, 1f, 7000, 49, 0f, false, false, false) },
        };
    }
}