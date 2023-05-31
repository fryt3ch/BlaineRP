using RAGE;
using RAGE.Elements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BCRPClient.Data
{
    public partial class Locations
    {
        public class MarketStall
        {
            public static uint RentPrice => Utils.ToUInt32(Sync.World.GetSharedData<object>("MARKETSTALL_RP", 0));

            public int Id { get; set; }

            public ushort CurrentRenterRID => Utils.ToUInt16(Sync.World.GetSharedData<object>($"MARKETSTALL_{Id}_R", ushort.MaxValue));

            public Utils.Vector4 Position { get; set; }

            public MarketStall(int Id, Utils.Vector4 Position)
            {
                this.Id = Id;

                this.Position = Position;

                var cs = new Additional.Sphere(new Vector3(Position.X, Position.Y, Position.Z), 2.5f, false, Utils.RedColor, Settings.MAIN_DIMENSION, null)
                {
                    ApproveType = Additional.ExtraColshape.ApproveTypes.OnlyByFoot,

                    ActionType = Additional.ExtraColshape.ActionTypes.MarketStallInteract,

                    InteractionType = Additional.ExtraColshape.InteractionTypes.MarketStallInteract,

                    Data = this,
                };
            }

            public int GetClosestMapObject()
            {
                var hashes = new uint[]
                {
                    RAGE.Util.Joaat.Hash("brp_p_marketstall_0"),
                };

                for (int i = 0; i < hashes.Length; i++)
                {
                    var handle = RAGE.Game.Object.GetClosestObjectOfType(Position.X, Position.Y, Position.Z, 1f, hashes[i], false, false, false);

                    if (handle <= 0)
                        continue;
                }

                return 0;
            }
        }
    }
}