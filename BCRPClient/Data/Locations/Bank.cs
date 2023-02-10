﻿using RAGE;
using RAGE.Elements;
using System;
using System.Collections.Generic;
using System.Text;

namespace BCRPClient.Data
{
    public partial class Locations : Events.Script
    {
        public class Bank
        {
            public static Dictionary<int, Bank> All = new Dictionary<int, Bank>();

            public int Id { get; set; }

            public List<NPC> Workers { get; set; }

            public Blip Blip { get; set; }

            public Bank(int id, Utils.Vector4[] NPCs)
            {
                Id = id;

                Workers = new List<NPC>();

                Vector3 posBlip = new Vector3(0f, 0f, 0f);

                for (int i = 0; i < NPCs.Length; i++)
                {
                    posBlip += NPCs[i].Position;

                    var npc = new NPC($"bank_{Id}_{i}", "Эмили", NPC.Types.Talkable, "csb_anita", NPCs[i].Position, NPCs[i].RotationZ, Settings.MAIN_DIMENSION)
                    {
                        DefaultDialogueId = "bank_preprocess",

                        Data = this,
                    };

                    Workers.Add(npc);
                }

                Blip = new Blip(605, posBlip / NPCs.Length, Locale.Property.BankNameDef, 1f, 0, 255, 0f, true, 0, 0f, Settings.MAIN_DIMENSION);
            }
        }

        public class ATM
        {
            public static Dictionary<int, ATM> All = new Dictionary<int, ATM>();

            public int Id { get; set; }

            public Additional.ExtraColshape Colshape { get; set; }

            public Blip Blip { get; set; }

            public ATM(int Id, Utils.Vector4 PositionParams)
            {
                this.Id = Id;

                All.Add(Id, this);

                Colshape = new Additional.Sphere(PositionParams.Position, PositionParams.RotationZ, false, new Utils.Colour(255, 0, 0, 255), Settings.MAIN_DIMENSION, null)
                {
                    Data = this,

                    InteractionType = Additional.ExtraColshape.InteractionTypes.ATM,
                    ActionType = Additional.ExtraColshape.ActionTypes.ATM,

                    Name = $"atm_{Id}",
                };

                Blip = new Blip(108, PositionParams.Position, Locale.Property.AtmNameDef, 0.4f, 25, 255, 0f, true, 0, 0f, Settings.MAIN_DIMENSION);
            }
        }
    }
}