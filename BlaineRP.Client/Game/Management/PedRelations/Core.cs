using System;
using System.Collections.Generic;
using System.Linq;

namespace BlaineRP.Client.Game.Management.PedRelations
{
    [Script(int.MaxValue)]
    public class Core
    {
        public enum Types
        {
            Companion = 0,
            Respect = 1,
            Like = 2,
            Neutral = 3,
            Dislike = 4,
            Hate = 5,
            Pedestrians = 255,
        }

        public enum Groups
        {
            Friendly = 0,
            Enemy,
        }

        private static readonly Dictionary<Groups, uint> _groupHashes = new Dictionary<Groups, uint>();

        public Core()
        {
            foreach (Groups group in (Groups[])Enum.GetValues(typeof(Groups)))
            {
                int hash = -1;

                RAGE.Game.Ped.AddRelationshipGroup(group.ToString(), ref hash);

                _groupHashes.Add(group, (uint)hash);
            }

            RAGE.Game.Ped.SetRelationshipBetweenGroups((int)Types.Companion, _groupHashes[Groups.Friendly], _groupHashes[Groups.Friendly]);
            RAGE.Game.Ped.SetRelationshipBetweenGroups((int)Types.Hate, _groupHashes[Groups.Friendly], _groupHashes[Groups.Enemy]);

            //SetRelationshipGroup(RAGE.Elements.Player.LocalPlayer, Groups.Friendly);
        }

        public static void SetRelationshipGroup(RAGE.Elements.Player player, Groups group) => player.SetRelationshipGroupHash(_groupHashes[group]);
        public static Groups GetRelationshipGroup(RAGE.Elements.Player player) => _groupHashes.Where(x => x.Value == player.GetRelationshipGroupHash()).Select(x => x.Key).FirstOrDefault();
    }
}
