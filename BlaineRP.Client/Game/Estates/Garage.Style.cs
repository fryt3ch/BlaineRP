using System;
using System.Collections.Generic;
using RAGE;

namespace BlaineRP.Client.Game.Estates
{
    public partial class Garage
    {
        public partial class Style
        {
            public Style(Types Type, byte Variation, Vector3 EnterPosition, Action OnAction = null, Action OffAction = null)
            {
                this.Variation = Variation;

                this.EnterPosition = EnterPosition;

                this.OnAction = OnAction;
                this.OffAction = OffAction;

                if (!All.ContainsKey(Type))
                    All.Add(Type,
                        new Dictionary<byte, Style>()
                        {
                            { Variation, this },
                        }
                    );
                else
                    All[Type].Add(Variation, this);
            }

            private static Dictionary<Types, Dictionary<byte, Style>> All { get; set; } = new Dictionary<Types, Dictionary<byte, Style>>();

            public Types Type { get; set; }

            public Vector3 EnterPosition { get; set; }

            public byte Variation { get; set; }

            public Action OnAction { get; set; }

            public Action OffAction { get; set; }

            public static Style Get(Types type, byte variation)
            {
                return All.GetValueOrDefault(type).GetValueOrDefault(variation);
            }
        }
    }
}