using System.Collections.Generic;
using System.Linq;

namespace BlaineRP.Client.Utils.UidHandlers
{
    public class UInt16
    {
        public UInt16(ushort LastAddedMaxUid = 0)
        {
            this.LastAddedMaxUid = LastAddedMaxUid;
        }

        public ushort LastAddedMaxUid { get; private set; }

        private HashSet<ushort> FreeUids { get; set; } = new HashSet<ushort>();

        public int FreeUidsCount => FreeUids.Count;

        public string HandlerStr => $"LastAddedMaxUid: {LastAddedMaxUid}, FreeUids: {string.Join(',', FreeUids)}";

        public bool SetUidAsFree(ushort uid)
        {
            return FreeUids.Add(uid);
        }

        public ushort MoveNextUid()
        {
            ushort id;

            if (FreeUids.Count == 0)
            {
                id = ++LastAddedMaxUid;
            }
            else
            {
                id = FreeUids.First();

                FreeUids.Remove(id);

                TryUpdateLastAddedMaxUid(id);
            }

            return id;
        }

        public bool TryUpdateLastAddedMaxUid(ushort uid)
        {
            if (uid > LastAddedMaxUid)
            {
                LastAddedMaxUid = uid;

                return true;
            }

            return false;
        }

        public void ResetLastAddedMaxUid()
        {
            LastAddedMaxUid = 0;
        }
    }
}