using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BCRPServer
{
    public class UidHandlerUInt32
    {
        public uint LastAddedMaxUid { get; private set; }

        private HashSet<uint> FreeUids { get; set; } = new HashSet<uint>();

        public int FreeUidsCount => FreeUids.Count;

        public string HandlerStr => $"LastAddedMaxUid: {LastAddedMaxUid}, FreeUids: {string.Join(',', FreeUids)}";

        public bool SetUidAsFree(uint uid) => FreeUids.Add(uid);

        public uint MoveNextUid()
        {
            uint id;

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

        public bool TryUpdateLastAddedMaxUid(uint uid)
        {
            if (uid > LastAddedMaxUid)
            {
                LastAddedMaxUid = uid;

                return true;
            }

            return false;
        }

        public void ResetLastAddedMaxUid() => LastAddedMaxUid = 0;

        public UidHandlerUInt32(uint LastAddedMaxUid = 0)
        {
            this.LastAddedMaxUid = LastAddedMaxUid;
        }
    }

    public class UidHandlerUInt64
    {
        public ulong LastAddedMaxUid { get; private set; }

        private HashSet<ulong> FreeUids { get; set; } = new HashSet<ulong>();

        public bool SetUidAsFree(ulong uid) => FreeUids.Add(uid);

        public int FreeUidsCount => FreeUids.Count;

        public ulong MoveNextUid()
        {
            ulong id;

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

        public bool TryUpdateLastAddedMaxUid(ulong uid)
        {
            if (uid > LastAddedMaxUid)
            {
                LastAddedMaxUid = uid;

                return true;
            }

            return false;
        }

        public UidHandlerUInt64(ulong LastAddedMaxUid = 0)
        {
            this.LastAddedMaxUid = LastAddedMaxUid;
        }
    }
}
