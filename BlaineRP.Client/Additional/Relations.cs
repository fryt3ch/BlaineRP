using RAGE;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BlaineRP.Client.Additional
{
    [Script(int.MaxValue)]
    public class Relations 
    {
        public enum Types
        {
            Companion = 0,
            Respect = 1,
            Like = 2,
            Neutral = 3,
            Dislike = 4,
            Hate = 5,
            Pedestrians = 255
        }

        public enum Groups
        {
            Friendly = 0,
            Enemy,
        }

        private static Dictionary<Groups, uint> GroupHashes = new Dictionary<Groups, uint>();

        public Relations()
        {
            foreach (Groups group in (Groups[])Enum.GetValues(typeof(Groups)))
            {
                int hash = -1;

                RAGE.Game.Ped.AddRelationshipGroup(group.ToString(), ref hash);

                GroupHashes.Add(group, (uint)hash);
            }

            RAGE.Game.Ped.SetRelationshipBetweenGroups((int)Types.Companion, GroupHashes[Groups.Friendly], GroupHashes[Groups.Friendly]);
            RAGE.Game.Ped.SetRelationshipBetweenGroups((int)Types.Hate, GroupHashes[Groups.Friendly], GroupHashes[Groups.Enemy]);

            //SetRelationshipGroup(RAGE.Elements.Player.LocalPlayer, Groups.Friendly);
        }

        public static void SetRelationshipGroup(RAGE.Elements.Player player, Groups group) => player.SetRelationshipGroupHash(GroupHashes[group]);
        public static Groups GetRelationshipGroup(RAGE.Elements.Player player) => GroupHashes.Where(x => x.Value == player.GetRelationshipGroupHash()).Select(x => x.Key).FirstOrDefault();
    }
}
