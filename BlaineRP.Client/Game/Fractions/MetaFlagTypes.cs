using System;

namespace BlaineRP.Client.Game.Fractions
{
    [Flags]
    public enum MetaFlagTypes
    {
        None = 0,

        IsGov = 1 << 0,
        MembersHaveDocs = 1 << 1,
    }
}