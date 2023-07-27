using System;

namespace BlaineRP.Server.Game.Fractions
{
    public abstract partial class Fraction
    {
        [Flags]
        public enum FlagTypes : uint
        {
            None = 0,

            /// <summary>Является ли фракция государственной?</summary>
            IsGov = 1 << 0,
            /// <summary>Имеют ли члены фракции удостоверения?</summary>
            MembersHaveDocs = 1 << 1,
        }
    }
}