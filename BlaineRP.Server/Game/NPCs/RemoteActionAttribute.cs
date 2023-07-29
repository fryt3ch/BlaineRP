using System;

namespace BlaineRP.Server.Game.NPCs
{
    [AttributeUsage(AttributeTargets.Method)]
    public class RemoteActionAttribute : Attribute
    {
        public string Id { get; set; }

        public string[] AllowedNpcIds { get; set; }

        public RemoteActionAttribute(string Id, params string[] AllowedNpcIds)
        {
            this.Id = Id;

            this.AllowedNpcIds = AllowedNpcIds;
        }
    }
}