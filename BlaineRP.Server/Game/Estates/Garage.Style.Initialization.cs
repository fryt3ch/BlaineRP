using System.Collections.Generic;
using BlaineRP.Server.UtilsT;

namespace BlaineRP.Server.Game.Estates
{
    public partial class Garage
    {
        public partial class Style
        {
            public static void LoadAll()
            {
                new Style(Types.Two,
                    0,
                    new Vector4(179.0708f, -1005.729f, -98.99996f, 80.5f),
                    new List<Vector4>()
                    {
                        new Vector4(171.2562f, -1004.826f, -99.38025f, 182f),
                        new Vector4(174.7562f, -1004.826f, -99.38025f, 182f),
                    }
                );

                new Style(Types.Six,
                    0,
                    new Vector4(207.0894f, -998.9854f, -98.99996f, 90f),
                    new List<Vector4>()
                    {
                        new Vector4(192.987f, -1004.135f, -99.38025f, 182f),
                        new Vector4(196.487f, -1004.135f, -99.38025f, 182f),

                        new Vector4(199.987f, -1004.135f, -99.38025f, 182f),
                        new Vector4(203.487f, -1004.135f, -99.38025f, 182f),

                        new Vector4(192.987f, -997.135f, -99.38025f, 182f),
                        new Vector4(196.487f, -997.135f, -99.38025f, 182f),
                    }
                );

                new Style(Types.Ten,
                    0,
                    new Vector4(238.0103f, -1004.861f, -98.99996f, 78f),
                    new List<Vector4>()
                    {
                        new Vector4(233.536f, -1001.264f, -99.38025f, 125f),
                        new Vector4(233.536f, -996.764f, -99.38025f, 125f),
                        new Vector4(233.536f, -992.264f, -99.38025f, 125f),
                        new Vector4(233.536f, -987.764f, -99.38025f, 125f),
                        new Vector4(233.536f, -983.264f, -99.38025f, 125f),

                        new Vector4(223.536f, -1001.264f, -99.38025f, 250f),
                        new Vector4(223.536f, -996.764f, -99.38025f, 250f),
                        new Vector4(223.536f, -992.264f, -99.38025f, 250f),
                        new Vector4(223.536f, -987.764f, -99.38025f, 250f),
                        new Vector4(223.536f, -983.264f, -99.38025f, 250f),
                    }
                );
            }
        }
    }
}