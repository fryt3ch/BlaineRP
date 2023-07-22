using GTANetworkAPI;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace BlaineRP.Server.Game.Estates
{
	public abstract class HouseBase
	{
		public static Utils.Colour DefaultLightColour => new Utils.Colour(255, 187, 96, 255);

		public class Style
		{
			public class DoorInfo
			{
                [JsonProperty(PropertyName = "M")]
                public uint Model { get; set; }

                [JsonProperty(PropertyName = "P")]
                public Vector3 Position { get; set; }

				public DoorInfo(string Model, Vector3 Position) : this(NAPI.Util.GetHashKey(Model), Position)
				{

				}

				public DoorInfo(uint Model, Vector3 Position)
				{
					this.Model = Model;
					this.Position = Position;
				}
			}

			public class LightInfo
			{
                [JsonProperty(PropertyName = "M")]
                public uint Model { get; set; }

                [JsonProperty(PropertyName = "P")]
                public Vector3 Position { get; set; }

				public LightInfo(string Model, Vector3 Position) : this(NAPI.Util.GetHashKey(Model), Position)
				{

				}

				public LightInfo(uint Model, Vector3 Position)
				{
					this.Model = Model;
					this.Position = Position;
				}
			}

			/// <summary>Типы комнат</summary>
			public enum RoomTypes : byte
			{
				One = 1,
				Two = 2,
				Three = 3,
				Four = 4,
				Five = 5,
			}

			private HashSet<RoomTypes> SupportedRoomTypes { get; }
            private HashSet<HouseBase.Types> SupportedHouseTypes { get; }

            private HashSet<ushort> FamiliarTypes { get; }

            public Utils.Vector4 InteriorPosition { get; }

            public Vector3 Position { get; }

            public float Heading { get; }

            private DoorInfo[] Doors { get; }

            private LightInfo[][] Lights { get; }

			public int LightsAmount => Lights.Length;

			public int DoorsAmount => Doors.Length;

            public uint Price { get; }

			public ushort ParentType { get; }

			/// <summary>Словарь планировок</summary>
			private static Dictionary<ushort, Style> All { get; set; }

			public static Style Get(ushort sType) => All.GetValueOrDefault(sType);

			public bool IsPositionInsideInterior(Vector3 position) => InteriorPosition.Position.DistanceTo(position) <= InteriorPosition.RotationZ;

            public Style(ushort Type, Vector3 Position, float Heading, Utils.Vector4 InteriorPosition, DoorInfo[] Doors, LightInfo[][] Lights, uint Price, HashSet<RoomTypes> SupportedRoomTypes, HashSet<HouseBase.Types> SupportedHouseTypes, Vector3 Offset = null)
			{
				All.Add(Type, this);

				this.FamiliarTypes = new HashSet<ushort>();

				this.SupportedHouseTypes = SupportedHouseTypes;
				this.SupportedRoomTypes = SupportedRoomTypes;

				this.Position = Position;
				this.Heading = Heading;
				this.InteriorPosition = InteriorPosition;
				this.Price = Price;

				this.Doors = Doors;
				this.Lights = Lights;

				this.ParentType = Type;

				for (int i = 0; i < Doors.Length; i++)
				{
					Doors[i].Position += InteriorPosition.Position;
				}

				for (int i = 0; i < Lights.Length; i++)
				{
					for (int j = 0; j < Lights[i].Length; j++)
						Lights[i][j].Position += InteriorPosition.Position;
				}

				if (Offset != null)
				{
					for (int i = 0; i < Doors.Length; i++)
					{
						Doors[i].Position += Offset;
					}

					for (int i = 0; i < Lights.Length; i++)
					{
						for (int j = 0; j < Lights[i].Length; j++)
							Lights[i][j].Position += Offset;
					}
				}
			}

			public Style(ushort Type, ushort ParentType, Vector3 Offset, uint Price)
			{
				All.Add(Type, this);

				this.ParentType = ParentType;

				var parent = Get(ParentType);

				this.FamiliarTypes = new HashSet<ushort>() { ParentType };

				foreach (var x in parent.FamiliarTypes)
				{
					var s = Get(x);

					s.FamiliarTypes.Add(Type);

					this.FamiliarTypes.Add(x);
				}

				parent.FamiliarTypes.Add(Type);

				this.Heading = parent.Heading;
				this.Position = parent.Position + Offset;
				this.InteriorPosition = new Utils.Vector4(parent.InteriorPosition.Position + Offset, parent.InteriorPosition.RotationZ);

				this.Doors = new DoorInfo[parent.Doors.Length];

				for (int i = 0; i < parent.Doors.Length; i++)
				{
					var x = parent.Doors[i];

					this.Doors[i] = new DoorInfo(x.Model, x.Position - parent.InteriorPosition.Position + this.InteriorPosition.Position);
				}

				this.Lights = new LightInfo[parent.Lights.Length][];

				for (int i = 0; i < parent.Lights.Length; i++)
				{
					var x = parent.Lights[i];

					this.Lights[i] = new LightInfo[x.Length];

					for (int j = 0; j < x.Length; j++)
					{
						var y = x[j];

						this.Lights[i][j] = new LightInfo(y.Model, y.Position - parent.InteriorPosition.Position + this.InteriorPosition.Position);
					}
				}

				this.SupportedRoomTypes = parent.SupportedRoomTypes.ToHashSet();
				this.SupportedHouseTypes = parent.SupportedHouseTypes.ToHashSet();

				this.Price = Price;
			}

			public bool IsHouseTypeSupported(HouseBase.Types hType) => SupportedHouseTypes.Contains(hType);
			public bool IsRoomTypeSupported(RoomTypes rType) => SupportedRoomTypes.Contains(rType);
			public bool IsTypeFamiliar(ushort type) => FamiliarTypes.Contains(type);

			public static void LoadAll()
			{
				if (All != null)
					return;

				All = new Dictionary<ushort, Style>();

				new Style
				(
					Type: 0,

					Position: new Vector3(-951.7298f, 3439.978f, -179f),
					Heading: 271.0717f,

					InteriorPosition: new Utils.Vector4(-950f, 3440f, -180f, 10.3465f),

					Doors: new DoorInfo[]
					{
						new DoorInfo("v_ilev_ra_door3", new Vector3(-0.6799995f, -1.300002f, 1.152f)),
						new DoorInfo("v_ilev_ra_door3", new Vector3(2.550006f, -5.550001f, 1.152f)),
						new DoorInfo("v_ilev_ra_door3", new Vector3(3.25f, -2.800003f, 1.152f)),
						new DoorInfo("v_ilev_ra_door3", new Vector3(4.850007f, -0.2000022f, 1.152f)),
						new DoorInfo("v_ilev_ra_door3", new Vector3(3.249999f, 1.3f, 1.152f)),
					},

					Lights: new LightInfo[][]
					{
						new LightInfo[] { new LightInfo("brp_p_light_3_1", new Vector3(0f, 0f, 3.976f)) },
						new LightInfo[] { new LightInfo("brp_p_light_3_1", new Vector3(0f, -5.349999f, 3.976f)) },
						new LightInfo[] { new LightInfo("brp_p_light_3_1", new Vector3(5.400002f, -6.099998f, 3.976f)) },
						new LightInfo[] { new LightInfo("brp_p_light_3_1", new Vector3(5.641205f, 4.258736f, 3.976f)) },
						new LightInfo[] { new LightInfo("brp_p_light_3_1", new Vector3(3.800003f, -0.75f, 3.976f)) },
						new LightInfo[] { new LightInfo("brp_p_light_2_1", new Vector3(6.75f, -0.75f, 3.95f)) },
					},

					SupportedHouseTypes: new HashSet<HouseBase.Types>() { HouseBase.Types.House, },

					SupportedRoomTypes: new HashSet<RoomTypes>() { RoomTypes.Two, },

					Price: 10_000
				);

				new Style
				(
					Type: 10,

					Position: new Vector3(-849.6476f, 3442.679f, -180.5f),
					Heading: 177.6222f,

					InteriorPosition: new Utils.Vector4(-850f, 3440f, -180f, 6.94992f),

					Doors: new DoorInfo[]
					{
						new DoorInfo("prop_ret_door", new Vector3(-0.661008f, 2.153585f, -0.4080915f)),
						new DoorInfo("prop_ret_door_02", new Vector3(-0.6673489f, 0.1417937f, -0.4082555f)),
						new DoorInfo("prop_ret_door_02", new Vector3(1.345982f, 0.2633806f, -0.4082556f)),
						new DoorInfo("prop_ret_door", new Vector3(1.005982f, -1.482248f, -0.4080915f)),
					},

					Lights: new LightInfo[][]
					{
						new LightInfo[] { new LightInfo("brp_p_light_4_0", new Vector3(0.3443137f, 1.061796f, 1.483612f)) },
						new LightInfo[] { new LightInfo("brp_p_light_4_0", new Vector3(-2.582341f, 2.22257f, 1.483612f)) },
						new LightInfo[] { new LightInfo("brp_p_light_4_0", new Vector3(-3.406205f, -1.783329f, 1.483612f)) },
						new LightInfo[] { new LightInfo("brp_p_light_4_0", new Vector3(3.372307f, -1.298711f, 1.483612f)) },
						new LightInfo[] { new LightInfo("brp_p_light_4_0", new Vector3(-0.1508973f, -2.840963f, 1.483612f)) },
					},

					SupportedHouseTypes: new HashSet<HouseBase.Types>() { HouseBase.Types.House, },

					SupportedRoomTypes: new HashSet<RoomTypes>() { RoomTypes.Two, },

					Price: 10_000
				);

				new Style
				(
					Type: 20,

					Position: new Vector3(-847.1672f, 3478.561f, -180.5f),
					Heading: 177.7124f,

					InteriorPosition: new Utils.Vector4(-850f, 3440f + 35f, -180f, 7.52497f),

					Doors: new DoorInfo[]
					{
						new DoorInfo("prop_ret_door", new Vector3(1.776493f, 38.60274f, -0.4080915f)),
						new DoorInfo("prop_ret_door_02", new Vector3(-0.4096897f, 34.64424f, -0.4082555f)),
						new DoorInfo("prop_ret_door_02", new Vector3(1.334396f, 32.99568f, -0.4082555f)),
						new DoorInfo("prop_ret_door", new Vector3(1.34976f, 35.02336f, -0.4080915f)),
					},

					Lights: new LightInfo[][]
					{
						new LightInfo[] { new LightInfo("brp_p_light_4_0", new Vector3(-0.9984746f, 38.96366f, 1.483612f)) },
						new LightInfo[] { new LightInfo("brp_p_light_4_0", new Vector3(2.943951f, 36.38088f, 1.483612f)) },
						new LightInfo[] { new LightInfo("brp_p_light_4_0", new Vector3(1.71268f, 33.98152f, 1.483612f)) },
						new LightInfo[] { new LightInfo("brp_p_light_4_0", new Vector3(-2.398272f, 34.93951f, 1.483612f)) },
						new LightInfo[] { new LightInfo("brp_p_light_4_0", new Vector3(-1.027865f, 30.95322f, 1.483612f)) },
						new LightInfo[] { new LightInfo("brp_p_light_4_0", new Vector3(0.4503248f, 36.07869f, 1.483612f)) },
					},

					SupportedHouseTypes: new HashSet<HouseBase.Types>() { HouseBase.Types.House, },

					SupportedRoomTypes: new HashSet<RoomTypes>() { RoomTypes.Two, },

					Price: 10_000,

					Offset: new Vector3(0f, -35f, 0f)
				);

				new Style
				(
					Type: 30,

					Position: new Vector3(-845.6879f, 3513.75f, -180.5001f),
					Heading: 86.09361f,

					InteriorPosition: new Utils.Vector4(-850f, 3440f + 70f, -180f, 7.0415f),

					Doors: new DoorInfo[]
					{
						new DoorInfo("prop_ret_door", new Vector3(2.816763f, 73.85589f, -0.4180914f)),
						new DoorInfo("prop_ret_door_02", new Vector3(-0.9265923f, 69.08482f, -0.4082555f)),
						new DoorInfo("prop_ret_door_02", new Vector3(4.319703f, 69.13847f, -0.4082555f)),
					},

					Lights: new LightInfo[][]
					{
						new LightInfo[]
						{
							new LightInfo("brp_p_light_4_0", new Vector3(2.642129f, 70.13357f, 1.483612f)),
                            new LightInfo("brp_p_light_4_0", new Vector3(3.846375f, 72.22765f, 1.483612f)),
                        },

						new LightInfo[] { new LightInfo("brp_p_light_4_0", new Vector3(1.832659f, 72.88146f, 1.483612f)) },
						new LightInfo[] { new LightInfo("brp_p_light_4_0", new Vector3(2.793969f, 67.21001f, 1.483612f)) },
						new LightInfo[] { new LightInfo("brp_p_light_4_0", new Vector3(-2.349123f, 67.70047f, 1.483612f)) },
						new LightInfo[] { new LightInfo("brp_p_light_4_0", new Vector3(-1.305994f, 72.57049f, 1.483612f)) },
					},

					SupportedHouseTypes: new HashSet<HouseBase.Types>() { HouseBase.Types.House, },

					SupportedRoomTypes: new HashSet<RoomTypes>() { RoomTypes.Two, },

					Price: 10_000,

					Offset: new Vector3(0f, -70f, 0f)
				);

				new Style
				(
					Type: 40,

					Position: new Vector3(-853.1857f, 3545.175f, -182.1146f),
					Heading: 273.0863f,

					InteriorPosition: new Utils.Vector4(-850f, 3440f + 105f, -180f, 9.53456f),

					Doors: new DoorInfo[]
					{
						new DoorInfo("prop_ret_door", new Vector3(-0.1510415f, 103.5986f, -2.022577f)),
						new DoorInfo("prop_ret_door", new Vector3(0.5698756f, 103.5593f, -2.022577f)),
						new DoorInfo("prop_ret_door", new Vector3(-0.8160014f, 103.5784f, 1.57737f)),
						new DoorInfo("prop_ret_door", new Vector3(0.2111053f, 99.09834f, 1.57737f)),

						new DoorInfo("prop_ret_door_02", new Vector3(-3.135704f, 103.5642f, 1.577206f)),
						new DoorInfo("prop_ret_door_02", new Vector3(-3.855967f, 105.7998f, 1.577206f)),
						new DoorInfo("prop_ret_door_02", new Vector3(-3.127736f, 106.7351f, 1.577206f)),
						new DoorInfo("prop_ret_door_02", new Vector3(1.396887f, 106.7426f, 1.577206f)),
						new DoorInfo("prop_ret_door_02", new Vector3(-1.152958f, 106.7211f, -2.022741f)),
						new DoorInfo("prop_ret_door_02", new Vector3(5.140837f, 103.5809f, -2.693746f)),
					},

					Lights: new LightInfo[][]
					{
						new LightInfo[] { new LightInfo("brp_p_light_4_0", new Vector3(-1.065282f, 105.1964f, 0.1691258f)) },
						new LightInfo[] { new LightInfo("brp_p_light_4_0", new Vector3(-1.950482f, 101.1643f, 0.1691258f)) },
						new LightInfo[] { new LightInfo("brp_p_light_4_0", new Vector3(1.822946f, 102.3438f, 0.1691258f)) },
						new LightInfo[] { new LightInfo("brp_p_light_4_0", new Vector3(-1.478506f, 109.2319f, 0.1691258f)) },
						new LightInfo[] { new LightInfo("brp_p_light_4_0", new Vector3(3.212504f, 109.1722f, 0.1691258f)) },
						new LightInfo[] { new LightInfo("brp_p_light_4_0", new Vector3(-2.036492f, 108.8396f, 3.769073f)) },
						new LightInfo[] { new LightInfo("brp_p_light_4_0", new Vector3(2.648248f, 108.9481f, 3.769073f)) },
						new LightInfo[] { new LightInfo("brp_p_light_4_0", new Vector3(0.9117524f, 102.3212f, 3.769073f)) },
						new LightInfo[] { new LightInfo("brp_p_light_4_0", new Vector3(-2.188939f, 100.9081f, 3.769073f)) },
						new LightInfo[] { new LightInfo("brp_p_light_4_0", new Vector3(1.877401f, 99.74334f, 3.769073f)) },
						new LightInfo[] { new LightInfo("brp_p_light_4_0", new Vector3(-1.16269f, 105.1631f, 3.769073f)) },
						new LightInfo[] { new LightInfo("brp_p_light_4_0", new Vector3(4.060811f, 99.72443f, -0.501879f)) },
					},

					SupportedHouseTypes: new HashSet<HouseBase.Types>() { HouseBase.Types.House, },

					SupportedRoomTypes: new HashSet<RoomTypes>() { RoomTypes.Two, },

					Price: 10_000,

					Offset: new Vector3(0f, -105f, 0f)
				);

				new Style
				(
					Type: 50,

					Position: new Vector3(-846.2768f, 3579.112f, -180.5f),
					Heading: 85.65966f,

					InteriorPosition: new Utils.Vector4(-850f, 3440f + 140f, -180f, 7.81977f),

					Doors: new DoorInfo[]
					{
						new DoorInfo("v_ilev_fh_door02", new Vector3(1.816878f, 136.163f, -0.4082561f)),
						new DoorInfo("v_ilev_fib_door1", new Vector3(1.37214f, 138.0012f, -0.3449999f)),
						new DoorInfo("v_ilev_fib_door1", new Vector3(1.37214f, 138.0012f, -0.3449999f)),
						new DoorInfo("v_ilev_fh_door02", new Vector3(1.40801f, 140.3409f, -0.4082563f)),
						new DoorInfo("v_ilev_fib_door1", new Vector3(-2.660797f, 140.3393f, -0.3449997f)),
						new DoorInfo("v_ilev_fib_door1", new Vector3(-2.616023f, 142.5336f, -0.3449999f)),
						new DoorInfo("v_ilev_fib_door1", new Vector3(3.1124f, 142.0498f, -0.3449999f)),
					},

					Lights: new LightInfo[][]
					{
						new LightInfo[] { new LightInfo("brp_p_light_3_1", new Vector3(2.972252f, 138.9892f, 1.481776f)) },
						new LightInfo[] { new LightInfo("brp_p_light_3_1", new Vector3(3.027311f, 134.9848f, 1.481776f)) },
						new LightInfo[] { new LightInfo("brp_p_light_3_1", new Vector3(-1.603372f, 136.0336f, 1.481776f)) },
						new LightInfo[] { new LightInfo("brp_p_light_3_1", new Vector3(0.8066364f, 144.0647f, 1.481776f)) },
						new LightInfo[] { new LightInfo("brp_p_light_3_1", new Vector3(-3.526964f, 142.1393f, 1.481777f)) },
						new LightInfo[] { new LightInfo("brp_p_light_3_1", new Vector3(-0.5216536f, 140.2679f, 1.481777f)) },
					},

					SupportedHouseTypes: new HashSet<HouseBase.Types>() { HouseBase.Types.House, },

					SupportedRoomTypes: new HashSet<RoomTypes>() { RoomTypes.Two, },

					Price: 10_000,

					Offset: new Vector3(0f, -140f, 0f)
				);

				new Style
				(
					Type: 60,

					Position: new Vector3(-847.5582f, 3615.727f, -180.525f),
					Heading: 127.9297f,

					InteriorPosition: new Utils.Vector4(-850f, 3440f + 175f, -180f, 8.46709f),

					Doors: new DoorInfo[]
					{
						new DoorInfo("v_ilev_fh_door02", new Vector3(3.634691f, 174.8662f, -0.433256f)),
						new DoorInfo("v_ilev_fh_door02", new Vector3(0.1666245f, 175.6444f, -0.433256f)),

						new DoorInfo("v_ilev_fib_door1", new Vector3(0.06170309f, 176.9257f, -0.37f)),
						new DoorInfo("v_ilev_fib_door1", new Vector3(-4.288092f, 173.7819f, -0.37f)),
						new DoorInfo("v_ilev_fib_door1", new Vector3(0.06170309f, 173.0459f, -0.37f)),
					},

					Lights: new LightInfo[][]
					{
						new LightInfo[] { new LightInfo("brp_p_light_3_1", new Vector3(1.640707f, 174.8707f, 1.456776f)) },
						new LightInfo[] { new LightInfo("brp_p_light_3_1", new Vector3(4.936249f, 173.578f, 1.456777f)) },
						new LightInfo[] { new LightInfo("brp_p_light_3_1", new Vector3(-1.695213f, 178.8944f, 1.456776f)) },
						new LightInfo[] { new LightInfo("brp_p_light_3_1", new Vector3(-1.814014f, 171.1266f, 1.456776f)) },
						new LightInfo[] { new LightInfo("brp_p_light_3_1", new Vector3(-2.059127f, 174.9795f, 1.456776f)) },
					},

					SupportedHouseTypes: new HashSet<HouseBase.Types>() { HouseBase.Types.House, },

					SupportedRoomTypes: new HashSet<RoomTypes>() { RoomTypes.Two, },

					Price: 10_000,

					Offset: new Vector3(0f, -175f, 0f)
				);

				new Style
				(
					Type: 70,

					Position: new Vector3(-846.0022f, 3654.428f, -180.5001f),
					Heading: 175.8804f,

					InteriorPosition: new Utils.Vector4(-850f, 3440f + 210f, -180f, 7.58965f),

					Doors: new DoorInfo[]
					{
						new DoorInfo("v_ilev_fh_door02", new Vector3(2.795463f, 210.6533f, -0.4082561f)),
						new DoorInfo("v_ilev_fh_door02", new Vector3(1.090689f, 208.7606f, -0.4082561f)),

						new DoorInfo("v_ilev_fib_door1", new Vector3(0.6610495f, 209.359f, -0.3449999f)),
						new DoorInfo("v_ilev_fib_door1", new Vector3(0.6593362f, 211.5502f, -0.3449999f)),
					},

					Lights: new LightInfo[][]
					{
						new LightInfo[]
						{
							new LightInfo("brp_p_light_3_1", new Vector3(3.039308f, 213.7278f, 1.481776f)),
                            new LightInfo("brp_p_light_3_1", new Vector3(1.711056f, 210.8099f, 1.481776f)),
                        },

						new LightInfo[] { new LightInfo("brp_p_light_3_1", new Vector3(3.934543f, 210.5374f, 1.481776f)) },
						new LightInfo[] { new LightInfo("brp_p_light_3_1", new Vector3(2.929501f, 206.8474f, 1.481777f)) },
						new LightInfo[] { new LightInfo("brp_p_light_3_1", new Vector3(-1.382281f, 208.2117f, 1.481776f)) },
						new LightInfo[] { new LightInfo("brp_p_light_3_1", new Vector3(-2.141615f, 213.1263f, 1.481777f)) },
					},

					Price: 10_000,

					SupportedHouseTypes: new HashSet<HouseBase.Types>() { HouseBase.Types.House, },

					SupportedRoomTypes: new HashSet<RoomTypes>() { RoomTypes.Two, },

					Offset: new Vector3(0f, -210f, 0f)
				);

				new Style
				(
					Type: 80,

					Position: new Vector3(-848.4805f, 3678.419f, -182.2f),
					Heading: 356.7289f,

					InteriorPosition: new Utils.Vector4(-850f, 3440f + 245f, -180f, 10.7366f),

					Doors: new DoorInfo[]
					{
						new DoorInfo("v_ilev_fh_door02", new Vector3(3.103763f, 243.2373f, -2.108256f)),
						new DoorInfo("v_ilev_fh_door02", new Vector3(3.166764f, 249.6447f, -2.108256f)),
						new DoorInfo("v_ilev_fh_door02", new Vector3(1.256672f, 249.3925f, 1.19175f)),
						new DoorInfo("v_ilev_fh_door02", new Vector3(2.613898f, 247.7522f, 1.19175f)),

						new DoorInfo("v_ilev_fib_door1", new Vector3(2.635777f, 240.5598f, 1.255006f)),
						new DoorInfo("v_ilev_fib_door1", new Vector3(-0.4513416f, 240.5625f, 1.255006f)),
						new DoorInfo("v_ilev_fib_door1", new Vector3(-0.4445041f, 245.5483f, 1.255006f)),
						new DoorInfo("v_ilev_fib_door1", new Vector3(-0.4087986f, 247.7394f, 1.255006f)),
						new DoorInfo("v_ilev_fib_door1", new Vector3(3.618469f, 247.88f, -2.045f)),
						new DoorInfo("v_ilev_fib_door1", new Vector3(3.13644f, 240.4389f, -2.045f)),
						new DoorInfo("v_ilev_fib_door1", new Vector3(-0.08900129f, 240.4405f, -2.045f)),
					},

					Lights: new LightInfo[][]
					{
						new LightInfo[]
						{
							new LightInfo("brp_p_light_3_1", new Vector3(1.530113f, 239.623f, -0.2182242f)),
							new LightInfo("brp_p_light_3_1", new Vector3(1.811421f, 247.0652f, -0.218224f)),
							new LightInfo("brp_p_light_3_1", new Vector3(1.568148f, 243.7543f, -0.2182242f)),
						},

						new LightInfo[]
						{
							new LightInfo("brp_p_light_3_1", new Vector3(-3.650046f, 245.3317f, -0.2182242f)),
							new LightInfo("brp_p_light_3_1", new Vector3(-2.531029f, 249.9022f, -0.218224f)),
						},

						new LightInfo[]
						{
							new LightInfo("brp_p_light_3_1", new Vector3(1.076304f, 241.2551f, 3.081781f)),
							new LightInfo("brp_p_light_3_1", new Vector3(1.132537f, 245.6533f, 3.081782f)),
                            new LightInfo("brp_p_light_3_1", new Vector3(4.70255f, 245.6533f, 3.081782f)),
                        },

						new LightInfo[] { new LightInfo("brp_p_light_3_1", new Vector3(4.969859f, 239.3361f, -0.218224f)) },
						new LightInfo[] { new LightInfo("brp_p_light_3_1", new Vector3(-2.635627f, 239.8996f, -0.218224f)) },
						new LightInfo[] { new LightInfo("brp_p_light_3_1", new Vector3(1.801483f, 251.0864f, -0.2182245f)) },
						new LightInfo[] { new LightInfo("brp_p_light_3_1", new Vector3(5.23355f, 249.9244f, -0.2182245f)) },
						new LightInfo[] { new LightInfo("brp_p_light_3_1", new Vector3(4.926069f, 242.2638f, -0.2182242f)) },
						new LightInfo[] { new LightInfo("brp_p_light_3_1", new Vector3(1.140518f, 250.7868f, 3.081782f)) },
						new LightInfo[] { new LightInfo("brp_p_light_3_1", new Vector3(-2.784307f, 249.9406f, 3.081782f)) },
						new LightInfo[] { new LightInfo("brp_p_light_3_1", new Vector3(-2.745191f, 244.9624f, 3.081782f)) },
						new LightInfo[] { new LightInfo("brp_p_light_3_1", new Vector3(-2.710855f, 239.9792f, 3.081782f)) },
						new LightInfo[] { new LightInfo("brp_p_light_3_1", new Vector3(4.610734f, 240.6396f, 3.081782f)) },
						new LightInfo[] { new LightInfo("brp_p_light_3_1", new Vector3(4.77532f, 249.9269f, 3.081782f)) },
					},

					SupportedHouseTypes: new HashSet<HouseBase.Types>() { HouseBase.Types.House, },

					SupportedRoomTypes: new HashSet<RoomTypes>() { RoomTypes.Two, },

					Price: 10_000,

					Offset: new Vector3(0f, -245f, 0f)
				);

				new Style
				(
					Type: 90,

					Position: new Vector3(-846.1027f, 3718.751f, -180.775f),
					Heading: 89.03065f,

					InteriorPosition: new Utils.Vector4(-850f, 3440f + 280f, -180f, 9.00488f),

					Doors: new DoorInfo[]
					{
						new DoorInfo("apa_p_mp_door_apart_door", new Vector3(3.981887f, 280.2249f, -0.6252492f)),
						new DoorInfo("apa_p_mp_door_apart_door", new Vector3(1.862078f, 283.4269f, -0.6252492f)),

						new DoorInfo("apa_p_mp_yacht_door_01", new Vector3(0.196035f, 281.7054f, -0.62448f)),
						new DoorInfo("apa_p_mp_yacht_door_01", new Vector3(0.1971457f, 279.4926f, -0.62448f)),
						new DoorInfo("apa_p_mp_yacht_door_01", new Vector3(0.7391281f, 277.2619f, -0.62448f)),
						new DoorInfo("apa_p_mp_yacht_door_01", new Vector3(-3.089636f, 275.3045f, -0.62448f)),
					},

					Lights: new LightInfo[][]
					{
						new LightInfo[]
						{
							new LightInfo("brp_p_light_11_0", new Vector3(-2.358837f, 278.8042f, 1.661687f)),

							new LightInfo("brp_p_light_7_0", new Vector3(-2.316423f, 277.6817f, 1.70866f)),
							new LightInfo("brp_p_light_7_0", new Vector3(-1.131693f, 278.7681f, 1.708659f)),
							new LightInfo("brp_p_light_7_0", new Vector3(-2.331957f, 279.9564f, 1.70866f)),
							new LightInfo("brp_p_light_7_0", new Vector3(-3.571777f, 278.7996f, 1.708659f)),
						},

						new LightInfo[]
						{
							new LightInfo("brp_p_light_11_0", new Vector3(-2.366597f, 284.1059f, 1.661687f)),

							new LightInfo("brp_p_light_7_0", new Vector3(-3.725451f, 284.1629f, 1.70866f)),
							new LightInfo("brp_p_light_7_0", new Vector3(-2.22726f, 282.613f, 1.708659f)),
							new LightInfo("brp_p_light_7_0", new Vector3(-0.9662268f, 284.2278f, 1.708659f)),
							new LightInfo("brp_p_light_7_0", new Vector3(-2.257765f, 285.7301f, 1.70866f)),
						},

						new LightInfo[]
						{
							new LightInfo("brp_p_light_7_0", new Vector3(-0.7175725f, 274.7214f, 1.708659f)),
							new LightInfo("brp_p_light_7_0", new Vector3(2.667315f, 275.1362f, 1.70866f)),
						},

						new LightInfo[]
						{
							new LightInfo("brp_p_light_7_0", new Vector3(3.720678f, 285.3074f, 1.708659f)),
							new LightInfo("brp_p_light_7_0", new Vector3(1.519767f, 285.2246f, 1.708659f)),
						},

						new LightInfo[]
						{
							new LightInfo("brp_p_light_7_0", new Vector3(1.207969f, 281.3058f, 1.70866f)),
							new LightInfo("brp_p_light_7_0", new Vector3(2.335105f, 278.8039f, 1.708659f)),
						},

						new LightInfo[] { new LightInfo("brp_p_light_7_0", new Vector3(-3.993486f, 274.6962f, 1.70866f)) },
						new LightInfo[] { new LightInfo("brp_p_light_7_0", new Vector3(3.686812f, 281.8657f, 1.70866f)) },
					},

					SupportedHouseTypes: new HashSet<HouseBase.Types>() { HouseBase.Types.House, },

					SupportedRoomTypes: new HashSet<RoomTypes>() { RoomTypes.Two, },

					Price: 10_000,

					Offset: new Vector3(0f, -280f, 0f)
				);

				new Style
				(
					Type: 100,

					Position: new Vector3(-845.1092f, 3753.724f, -180.7951f),
					Heading: 81.4196f,

					InteriorPosition: new Utils.Vector4(-850f, 3440f + 315f, -180f, 9.42834f),

					Doors: new DoorInfo[]
					{
						new DoorInfo("apa_p_mp_door_apart_door", new Vector3(-1.679748f, 315.9545f, -0.6452492f)),
						new DoorInfo("apa_p_mp_door_apart_door", new Vector3(2.33556f, 311.996f, -0.6452492f)),

						new DoorInfo("apa_p_mp_yacht_door_01", new Vector3(1.583649f, 312.022f, -0.64448f)),
						new DoorInfo("apa_p_mp_yacht_door_01", new Vector3(1.052425f, 315.4702f, -0.64448f)),
						new DoorInfo("apa_p_mp_yacht_door_01", new Vector3(0.07279678f, 317.7578f, -0.64448f)),
						new DoorInfo("apa_p_mp_yacht_door_01", new Vector3(-3.897955f, 311.6848f, -0.64448f)),
					},

					Lights: new LightInfo[][]
					{
						new LightInfo[]
						{
                            new LightInfo("brp_p_light_7_0", new Vector3(-0.5462933f, 316.2869f, 1.68866f)),
							new LightInfo("brp_p_light_1_0", new Vector3(2.501907f, 313.8606f, 1.693291f)),
                        },

						new LightInfo[]
						{
							new LightInfo("brp_p_light_11_0", new Vector3(3.206917f, 318.8151f, 1.691686f)),

                            new LightInfo("brp_p_light_7_0", new Vector3(3.189486f, 320.4447f, 1.77866f)),
							new LightInfo("brp_p_light_7_0", new Vector3(3.171264f, 316.9838f, 1.77866f)),
                        },

						new LightInfo[]
						{
                            new LightInfo("brp_p_light_11_0", new Vector3(-0.9743146f, 310.0256f, 1.691686f)),

                            new LightInfo("brp_p_light_7_0", new Vector3(-2.550541f, 309.9674f, 1.77866f)),
							new LightInfo("brp_p_light_7_0", new Vector3(0.5905386f, 310.0841f, 1.77866f)),
                        },

						new LightInfo[]
						{
							new LightInfo("brp_p_light_11_0", new Vector3(-2.477737f, 319.9074f, 1.691687f)),

                            new LightInfo("brp_p_light_7_0", new Vector3(-4.179114f, 319.8979f, 1.77866f)),
							new LightInfo("brp_p_light_7_0", new Vector3(-0.8496855f, 319.9404f, 1.77866f)),
                        },

                        new LightInfo[] { new LightInfo("brp_p_light_1_0", new Vector3(-3.677651f, 314.9938f, 1.693291f)) },
                        new LightInfo[] { new LightInfo("brp_p_light_1_0", new Vector3(3.751418f, 309.8597f, 1.693291f)) },

                        new LightInfo[] { new LightInfo("brp_p_light_7_0", new Vector3(-4.760465f, 309.9926f, 1.68866f)) },
                    },

					SupportedHouseTypes: new HashSet<HouseBase.Types>() { HouseBase.Types.House, },

					SupportedRoomTypes: new HashSet<RoomTypes>() { RoomTypes.Two, },

					Price: 10_000,

					Offset: new Vector3(0f, -315f, 0f)
				);

				new Style
				(
					Type: 110,

					Position: new Vector3(-844.476f, 3789.712f, -180.7799f),
					Heading: 83.89639f,

					InteriorPosition: new Utils.Vector4(-850f, 3440f + 350f, -180f, 10.0227f),

					Doors: new DoorInfo[]
					{
						new DoorInfo("apa_p_mp_door_apart_door", new Vector3(5.250995f, 351.9758f, -0.6302485f)),
						new DoorInfo("apa_p_mp_door_apart_door", new Vector3(3.964564f, 347.3526f, -0.6302483f)),

						new DoorInfo("apa_p_mp_yacht_door_01", new Vector3(2.324154f, 347.373f, -0.6294789f)),
						new DoorInfo("apa_p_mp_yacht_door_01", new Vector3(0.3166523f, 350.982f, -0.6294791f)),
						new DoorInfo("apa_p_mp_yacht_door_01", new Vector3(0.3073617f, 353.3655f, -0.6294791f)),
						new DoorInfo("apa_p_mp_yacht_door_01", new Vector3(-4.310765f, 356.5187f, -0.6294791f)),
					},

					Lights: new LightInfo[][]
					{
						new LightInfo[]
						{
							new LightInfo("brp_p_light_1_0", new Vector3(3.897215f, 349.7984f, 1.698291f)),

							new LightInfo("brp_p_light_7_0", new Vector3(1.703947f, 351.3096f, 1.713661f)),
						},

                        new LightInfo[]
                        {
                            new LightInfo("brp_p_light_11_0", new Vector3(-1.529435f, 344.9614f, 1.676688f)),

                            new LightInfo("brp_p_light_7_0", new Vector3(-4.000854f, 344.9742f, 1.76366f)),
                            new LightInfo("brp_p_light_7_0", new Vector3(0.7727992f, 345.0595f, 1.763661f)),
                        },

                        new LightInfo[]
                        {
                            new LightInfo("brp_p_light_11_0", new Vector3(-2.817852f, 349.3704f, 1.676688f)),

                            new LightInfo("brp_p_light_7_0", new Vector3(-4.631263f, 349.3846f, 1.763661f)),
                            new LightInfo("brp_p_light_7_0", new Vector3(-1.106217f, 349.3983f, 1.763661f)),
                        },

                        new LightInfo[]
                        {
                            new LightInfo("brp_p_light_11_0", new Vector3(-1.990419f, 354.488f, 1.676688f)),

                            new LightInfo("brp_p_light_7_0", new Vector3(-2.004944f, 356.123f, 1.763661f)),
                            new LightInfo("brp_p_light_7_0", new Vector3(-1.952244f, 352.8014f, 1.76366f)),
                        },

                        new LightInfo[] { new LightInfo("brp_p_light_1_0", new Vector3(3.813694f, 355.0768f, 1.698292f)) },
						new LightInfo[] { new LightInfo("brp_p_light_1_0", new Vector3(4.604296f, 344.9529f, 1.698292f)) },

						new LightInfo[] { new LightInfo("brp_p_light_7_0", new Vector3(-5.114809f, 354.458f, 1.703661f)) },
					},

					SupportedHouseTypes: new HashSet<HouseBase.Types>() { HouseBase.Types.House, },

					SupportedRoomTypes: new HashSet<RoomTypes>() { RoomTypes.Two, },

					Price: 10_000,

					Offset: new Vector3(0f, -350f, 0f)
				);

				new Style
				(
					Type: 120,

					Position: new Vector3(-856.2227f, 3824.961f, -182.4751f),
					Heading: 269.9142f,

					InteriorPosition: new Utils.Vector4(-850f, 3440f + 385f, -180f, 12.0572f),

					Doors: new DoorInfo[]
					{
						new DoorInfo("apa_p_mp_door_apart_door", new Vector3(0.01823944f, 382.828f, -2.325255f)),
						new DoorInfo("apa_p_mp_door_apart_door", new Vector3(1.754448f, 388.647f, -2.325255f)),
						new DoorInfo("apa_p_mp_door_apart_door", new Vector3(-2.5278, 381.0526, -2.325255)),
						new DoorInfo("apa_p_mp_door_apart_door", new Vector3(-0.4144659f, 377.7114f, -2.325255f)),
						new DoorInfo("apa_p_mp_door_apart_door", new Vector3(1.854466f, 388.1587f, 1.27479f)),
						new DoorInfo("apa_p_mp_door_apart_door", new Vector3(-2.560574f, 388.5428f, 1.27479f)),

						new DoorInfo("apa_p_mp_yacht_door_01", new Vector3(-4.749583f, 382.8409f, -2.324486f)),
						new DoorInfo("apa_p_mp_yacht_door_01", new Vector3(1.749034f, 384.4548f, -2.324486f)),
						new DoorInfo("apa_p_mp_yacht_door_01", new Vector3(-0.3158976f, 382.8257f, 1.275559f)),
						new DoorInfo("apa_p_mp_yacht_door_01", new Vector3(-4.079145f, 382.8347f, 1.275559f)),
						new DoorInfo("apa_p_mp_yacht_door_01", new Vector3(-4.617629f, 383.9258f, 1.275559f)),
						new DoorInfo("apa_p_mp_yacht_door_01", new Vector3(3.516185f, 383.282f, 1.275559f)),
						new DoorInfo("apa_p_mp_yacht_door_01", new Vector3(3.14104f, 382.131f, 1.275559f)),
					},

					Lights: new LightInfo[][]
					{
						new LightInfo[]
						{
							new LightInfo("brp_p_light_11_0", new Vector3(-4.818128f, 380.6407f, -0.2783198f)),

							new LightInfo("brp_p_light_7_0", new Vector3(-4.818793f, 381.9157f, -0.1913468f)),
							new LightInfo("brp_p_light_7_0", new Vector3(-4.805385f, 379.357f, -0.1913468f)),
						},

						new LightInfo[]
						{
							new LightInfo("brp_p_light_7_0", new Vector3(-1.145573f, 383.8226f, -0.1913468f)),
							new LightInfo("brp_p_light_7_0", new Vector3(-4.715909f, 387.2929f, -0.191347f)),
							new LightInfo("brp_p_light_7_0", new Vector3(-4.788708f, 389.4898f, -0.1913468f)),
							new LightInfo("brp_p_light_7_0", new Vector3(-4.757474f, 384.7742f, -0.191347f)),
						},

						new LightInfo[]
						{
							new LightInfo("brp_p_light_7_0", new Vector3(5.878274f, 385.1228f, -0.1313468f)),
							new LightInfo("brp_p_light_7_0", new Vector3(3.970784f, 384.9812f, -0.1413472f)),
							new LightInfo("brp_p_light_7_0", new Vector3(3.85214f, 381.1817f, -0.1413468f)),
							new LightInfo("brp_p_light_7_0", new Vector3(3.985691f, 388.741f, -0.1413468f)),
						},

						new LightInfo[]
						{
							new LightInfo("brp_p_light_7_0", new Vector3(2.529257f, 385.1851f, 3.408698f)),
							new LightInfo("brp_p_light_7_0", new Vector3(-2.00676f, 384.282f, 3.408698f)),
						},

						new LightInfo[]
						{
							new LightInfo("brp_p_light_1_0", new Vector3(-4.841751f, 390.007f, 3.405036f)),
							new LightInfo("brp_p_light_1_0", new Vector3(-6.43726f, 384.348f, 3.405036f)),
							new LightInfo("brp_p_light_1_0", new Vector3(3.86345f, 390.0189f, 3.405036f)),
						},

						new LightInfo[]
						{
							new LightInfo("brp_p_light_11_0", new Vector3(-4.696994f, 380.6493f, 3.321725f)),

							new LightInfo("brp_p_light_7_0", new Vector3(-3.298198f, 380.6977f, 3.408698f)),
							new LightInfo("brp_p_light_7_0", new Vector3(-6.018672f, 380.6259f, 3.408699f)),
						},

						new LightInfo[]
						{
							new LightInfo("brp_p_light_11_0", new Vector3(-0.389662f, 380.2239f, 3.321725f)),

							new LightInfo("brp_p_light_7_0", new Vector3(-0.3769767f, 378.577f, 3.408699f)),
							new LightInfo("brp_p_light_7_0", new Vector3(-0.3682517f, 381.6945f, 3.408698f)),
						},

						new LightInfo[]
						{
							new LightInfo("brp_p_light_11_0", new Vector3(3.867332f, 380.2135f, 3.321726f)),

							new LightInfo("brp_p_light_7_0", new Vector3(5.171535f, 380.2224f, 3.408698f)),
							new LightInfo("brp_p_light_7_0", new Vector3(2.548948f, 380.2066f, 3.408698f)),
						},

						new LightInfo[]
						{
							new LightInfo("brp_p_light_11_0", new Vector3(5.552965f, 385.0905f, 3.321725f)),

							new LightInfo("brp_p_light_7_0", new Vector3(5.510437f, 386.5676f, 3.408698f)),
							new LightInfo("brp_p_light_7_0", new Vector3(5.588992f, 383.4707f, 3.408699f)),
						},

						new LightInfo[] { new LightInfo("brp_p_light_7_0", new Vector3(0.6668818f, 381.1875f, -0.1913468f)) },
						new LightInfo[] { new LightInfo("brp_p_light_7_0", new Vector3(0.6495017f, 378.3827f, -0.1913468f)) },
						new LightInfo[] { new LightInfo("brp_p_light_7_0", new Vector3(-1.485061f, 380.1133f, -0.1913468f)) },

						new LightInfo[] { new LightInfo("brp_p_light_1_0", new Vector3(-0.5480608f, 390.4648f, 3.405036f)) },
						new LightInfo[] { new LightInfo("brp_p_light_1_0", new Vector3(3.86345f, 390.0189f, 3.405036f)) },
						new LightInfo[] { new LightInfo("brp_p_light_1_0", new Vector3(-0.3946362f, 390.4435f, -0.1950091f)) },
					},

					SupportedHouseTypes: new HashSet<HouseBase.Types>() { HouseBase.Types.House, },

					SupportedRoomTypes: new HashSet<RoomTypes>() { RoomTypes.Two, },

					Price: 10_000,

					Offset: new Vector3(0f, -385f, 0f)
				);

				new Style
				(
					Type: 130,

					Position: new Vector3(-842.799f, 3861.573f, -180.7238f),
					Heading: 87.02471f,

					InteriorPosition: new Utils.Vector4(-850f, 3440f + 420f, -180f, 12.1327f),

					Doors: new DoorInfo[]
					{
						new DoorInfo("apa_p_mp_door_apart_door", new Vector3(2.907555f, 419.6196f, -0.5739562f)),
						new DoorInfo("apa_p_mp_door_apart_door", new Vector3(2.316894f, 416.8317f, -0.5739562f)),

						new DoorInfo("apa_p_mp_yacht_door_01", new Vector3(3.533487f, 423.0842f, -0.573187f)),
						new DoorInfo("apa_p_mp_yacht_door_01", new Vector3(2.631699f, 423.0973f, -0.573187f)),
						new DoorInfo("apa_p_mp_yacht_door_01", new Vector3(-3.04984f, 421.312f, -0.573187f)),
					},

					Lights: new LightInfo[][]
					{
						new LightInfo[]
						{
							new LightInfo("brp_p_light_11_0", new Vector3(5.496925f, 426.0089f, 1.62298f)),

							new LightInfo("brp_p_light_7_0", new Vector3(5.476704f, 427.5124f, 1.709953f)),
							new LightInfo("brp_p_light_7_0", new Vector3(5.480489f, 424.3639f, 1.709952f)),
						},

                        new LightInfo[]
                        {
                            new LightInfo("brp_p_light_11_0", new Vector3(-0.00141817f, 425.934f, 1.62298f)),

                            new LightInfo("brp_p_light_7_0", new Vector3(1.77305, 425.9396, 1.709953)),
                            new LightInfo("brp_p_light_7_0", new Vector3(-1.696062f, 425.9713f, 1.709953f)),
                        },

                        new LightInfo[]
                        {
                            new LightInfo("brp_p_light_11_0", new Vector3(-5.4013f, 424.7136f, 1.62298f)),

                            new LightInfo("brp_p_light_7_0", new Vector3(-5.420548f, 422.6524f, 1.709952f)),
                            new LightInfo("brp_p_light_7_0", new Vector3(-5.395589f, 426.8822f, 1.709953f)),
                        },

                        new LightInfo[]
                        {
                            new LightInfo("brp_p_light_11_0", new Vector3(-5.425754f, 417.5864f, 1.47298f)),

                            new LightInfo("brp_p_light_7_0", new Vector3(-5.392357f, 415.7531f, 1.569953f)),
                            new LightInfo("brp_p_light_7_0", new Vector3(-5.42395f, 419.3454f, 1.569953f)),
                        },

                        new LightInfo[]
						{
							new LightInfo("brp_p_light_7_0", new Vector3(5.111981f, 421.5154f, 1.569953f)),

                            new LightInfo("brp_p_light_1_0", new Vector3(-0.2325937f, 419.9339f, 1.564584f)),
                        },

                        new LightInfo[] { new LightInfo("brp_p_light_1_0", new Vector3(5.325827f, 418.4982f, 1.564584f)) },
                        new LightInfo[] { new LightInfo("brp_p_light_1_0", new Vector3(0.5055502f, 413.9978f, 1.564584f)) },
					},

					SupportedHouseTypes: new HashSet<HouseBase.Types>() { HouseBase.Types.House, },

					SupportedRoomTypes: new HashSet<RoomTypes>() { RoomTypes.Two, },

					Price: 10_000,

					Offset: new Vector3(0f, -420f, 0f)
				);

				new Style
				(
					Type: 140,

					Position: new Vector3(-854.6521f, 3894.84f, -182.196f),
					Heading: 271.9679f,

					InteriorPosition: new Utils.Vector4(-850f, 3440f + 455f, -180f, 13.569f),

					Doors: new DoorInfo[]
					{
						new DoorInfo("apa_p_mp_door_apart_door_black", new Vector3(-2.207603f, 455.7861f, 1.553843f)),
						new DoorInfo("apa_p_mp_door_apart_door_black", new Vector3(-2.603649f, 459.2207f, 1.553843f)),
						new DoorInfo("apa_p_mp_door_apart_door_black", new Vector3(-7.42378f, 459.2212f, 1.553843f)),
						new DoorInfo("apa_p_mp_door_apart_door_black", new Vector3(3.266553f, 460.954f, -2.046162f)),
						new DoorInfo("apa_p_mp_door_apart_door_black", new Vector3(-0.2029644f, 453.1087f, -2.046162f)),
						new DoorInfo("apa_p_mp_door_apart_door_black", new Vector3(-3.539188f, 459.2407f, -2.717209f)),

						new DoorInfo("apa_p_mp_yacht_door_01", new Vector3(1.056287f, 453.097f, -2.045393f)),
						new DoorInfo("apa_p_mp_yacht_door_01", new Vector3(3.803128f, 459.1953f, 1.554612f)),
						new DoorInfo("apa_p_mp_yacht_door_01", new Vector3(1.056072f, 453.11f, 1.554612f)),
						new DoorInfo("apa_p_mp_yacht_door_01", new Vector3(-0.3900837f, 453.1115f, 1.554612f)),
						new DoorInfo("apa_p_mp_yacht_door_01", new Vector3(-0.3869764f, 459.239f, 1.554612f)),
					},

					Lights: new LightInfo[][]
					{
						new LightInfo[]
						{
							new LightInfo("brp_p_light_7_0", new Vector3(-2.169753f, 461.9752f, 0.08669864f)),
							new LightInfo("brp_p_light_7_0", new Vector3(-5.567911f, 462.0701f, 0.08669888f)),
						},

						new LightInfo[]
						{
							new LightInfo("brp_p_light_11_0", new Vector3(3.346321f, 449.1016f, 0.00077373f)),

							new LightInfo("brp_p_light_7_0", new Vector3(3.202268f, 446.246f, 0.08774698f)),
							new LightInfo("brp_p_light_7_0", new Vector3(3.195077f, 451.9853f, 0.08774674f)),
							new LightInfo("brp_p_light_7_0", new Vector3(1.381593f, 449.0968f, 0.08774674f)),
							new LightInfo("brp_p_light_7_0", new Vector3(4.991682f, 449.1079f, 0.08774674f)),
						},

						new LightInfo[]
						{
							new LightInfo("brp_p_light_11_0", new Vector3(5.776524f, 456.2266f, 0.00077373f)),

							new LightInfo("brp_p_light_7_0", new Vector3(5.769058f, 454.5159f, 0.08774674f)),
							new LightInfo("brp_p_light_7_0", new Vector3(5.760062f, 457.743f, 0.08774698f)),
						},

						new LightInfo[]
						{
							new LightInfo("brp_p_light_11_0", new Vector3(-4.010991f, 462.0704f, 3.650779f)),

							new LightInfo("brp_p_light_7_0", new Vector3(-5.904768f, 463.402f, 3.687751f)),
							new LightInfo("brp_p_light_7_0", new Vector3(-5.840635f, 460.7006f, 3.687751f)),
							new LightInfo("brp_p_light_7_0", new Vector3(-2.074727f, 463.402f, 3.687751f)),
							new LightInfo("brp_p_light_7_0", new Vector3(-2.090594f, 460.7006f, 3.687751f)),
						},

						new LightInfo[]
						{
							new LightInfo("brp_p_light_11_0", new Vector3(-2.589798f, 450.1341f, 3.760778f)),

							new LightInfo("brp_p_light_7_0", new Vector3(-2.57673f, 448.5091f, 3.687751f)),
							new LightInfo("brp_p_light_7_0", new Vector3(-2.595134f, 451.8058f, 3.687751f)),
						},

						new LightInfo[]
						{
							new LightInfo("brp_p_light_11_0", new Vector3(3.225483f, 449.0952f, 3.700778f)),

							new LightInfo("brp_p_light_7_0", new Vector3(3.200975f, 446.5159f, 3.687751f)),
							new LightInfo("brp_p_light_7_0", new Vector3(3.199283f, 451.6571f, 3.687751f)),
						},

						new LightInfo[]
						{
							new LightInfo("brp_p_light_11_0", new Vector3(5.751761f, 456.1617f, 3.600778f)),

							new LightInfo("brp_p_light_7_0", new Vector3(5.784851f, 454.4858f, 3.687752f)),
							new LightInfo("brp_p_light_7_0", new Vector3(5.720947f, 457.853f, 3.687751f)),
						},

						new LightInfo[]
						{
							new LightInfo("brp_p_light_11_0", new Vector3(5.76047f, 462.1011f, 3.600778f)),

							new LightInfo("brp_p_light_7_0", new Vector3(5.807197f, 460.4768f, 3.687751f)),
							new LightInfo("brp_p_light_7_0", new Vector3(5.706869f, 463.6979f, 3.687751f)),
						},

						new LightInfo[]
						{
							new LightInfo("brp_p_light_1_0", new Vector3(0.6403403f, 456.3575f, 3.692382f)),
							new LightInfo("brp_p_light_1_0", new Vector3(1.655561f, 461.0978f, 3.692382f)),
						},

						new LightInfo[] { new LightInfo("brp_p_light_7_0", new Vector3(-3.836375f, 457.7079f, 3.687751f)) },
						new LightInfo[] { new LightInfo("brp_p_light_7_0", new Vector3(-3.84143f, 454.6393f, 3.687751f)) },
						new LightInfo[] { new LightInfo("brp_p_light_7_0", new Vector3(-6.735017f, 456.1489f, 3.307752f)) },
						new LightInfo[] { new LightInfo("brp_p_light_1_0", new Vector3(-2.642789f, 456.0628f, 0.09237802f)) },
						new LightInfo[] { new LightInfo("brp_p_light_1_0", new Vector3(1.765093f, 456.4812f, 0.09237779f)) },
						new LightInfo[] { new LightInfo("brp_p_light_1_0", new Vector3(5.769177f, 462.0398f, 0.09237779f)) },
						new LightInfo[] { new LightInfo("brp_p_light_1_0", new Vector3(-2.553523f, 450.0591f, 0.09237802f)) },
					},

					SupportedHouseTypes: new HashSet<HouseBase.Types>() { HouseBase.Types.House, },

					SupportedRoomTypes: new HashSet<RoomTypes>() { RoomTypes.Two, },

					Price: 10_000,

					Offset: new Vector3(0f, -455f, 0f)
				);

				new Style
				(
					Type: 150,

					Position: new Vector3(-854.1147f, 3932.486f, -182.5f),
					Heading: 269.0663f,

					InteriorPosition: new Utils.Vector4(-850f, 3440f + 490f, -180f, 14.7842f),

					Doors: new DoorInfo[]
					{
						new DoorInfo("apa_p_mp_yacht_door_01", new Vector3(-0.3869764f, 459.239f, 1.554612f)),
						new DoorInfo("apa_p_mp_yacht_door_01", new Vector3(-2.460922f, 492.9905f, 1.250502f)),
						new DoorInfo("apa_p_mp_yacht_door_01", new Vector3(-2.461515f, 493.797f, 1.250502f)),
						new DoorInfo("apa_p_mp_yacht_door_01", new Vector3(3.752901f, 494.9835f, 1.250502f)),
						new DoorInfo("apa_p_mp_yacht_door_01", new Vector3(5.701312f, 493.2313f, 1.250502f)),
						new DoorInfo("apa_p_mp_yacht_door_01", new Vector3(3.726109f, 492.6924f, -2.34948f)),
						new DoorInfo("apa_p_mp_yacht_door_01", new Vector3(3.754722f, 484.5643f, -2.34948f)),
						new DoorInfo("apa_p_mp_yacht_door_01", new Vector3(-2.468299f, 493.6937f, -2.34948f)),

						new DoorInfo("apa_p_mp_door_apart_door_black", new Vector3(-2.462869f, 497.9169f, -2.350249f)),
						new DoorInfo("apa_p_mp_door_apart_door_black", new Vector3(-1.871128f, 491.6913f, -2.350249f)),
						new DoorInfo("apa_p_mp_door_apart_door_black", new Vector3(-2.987341f, 490.5945f, -2.350249f)),
						new DoorInfo("apa_p_mp_door_apart_door_black", new Vector3(-2.478989f, 485.8532f, -2.350249f)),
						new DoorInfo("apa_p_mp_door_apart_door_black", new Vector3(-0.573443f, 491.3009f, 1.249732f)),
						new DoorInfo("apa_p_mp_door_apart_door_black", new Vector3(0.7877403f, 495.4812f, 1.249732f)),
						new DoorInfo("apa_p_mp_door_apart_door_black", new Vector3(-2.462869f, 495.3344f, -2.350249f)),
					},

					Lights: new LightInfo[][]
					{
						new LightInfo[]
						{
							new LightInfo("brp_p_light_1_0", new Vector3(-6.099768f, 488.5618f, -0.1517097f)),
							new LightInfo("brp_p_light_1_0", new Vector3(-6.09789f, 484.6024f, -0.1517097f)),
						},

						new LightInfo[]
						{
							new LightInfo("brp_p_light_11_0", new Vector3(0.6243751f, 483.5532f, -0.2533138f)),

							new LightInfo("brp_p_light_7_0", new Vector3(0.6276543f, 481.3264f, -0.1563407f)),
							new LightInfo("brp_p_light_7_0", new Vector3(0.628985f, 485.8341f, -0.1563407f)),
							new LightInfo("brp_p_light_7_0", new Vector3(-1.33784f, 483.5514f, -0.1563407f)),
							new LightInfo("brp_p_light_7_0", new Vector3(2.597163f, 483.5668f, -0.1663407f)),
						},

						new LightInfo[]
						{
							new LightInfo("brp_p_light_11_0", new Vector3(6.904156f, 488.2478f, -0.1533137f)),

							new LightInfo("brp_p_light_7_0", new Vector3(6.802312f, 490.892f, -0.06634068f)),
							new LightInfo("brp_p_light_7_0", new Vector3(6.826692f, 485.7371f, -0.06634068f)),
							new LightInfo("brp_p_light_7_0", new Vector3(5.163073f, 488.227f, -0.06634092f)),
							new LightInfo("brp_p_light_7_0", new Vector3(8.579486f, 488.3089f, -0.06634068f)),
						},

						new LightInfo[]
						{
							new LightInfo("brp_p_light_7_0", new Vector3(8.239394f, 496.4323f, -0.2163405f)),
							new LightInfo("brp_p_light_7_0", new Vector3(5.16218f, 496.5278f, -0.2163407f)),
						},

						new LightInfo[]
						{
							new LightInfo("brp_p_light_7_0", new Vector3(0.1951773f, 493.9633f, -0.2163407f)),
							new LightInfo("brp_p_light_7_0", new Vector3(0.1871416f, 497.6697f, -0.2163407f)),
						},

						new LightInfo[]
						{
							new LightInfo("brp_p_light_1_0", new Vector3(0.5618586f, 493.3743f, 3.388273f)),
							new LightInfo("brp_p_light_1_0", new Vector3(1.791857f, 490.6617f, 3.388273f)),
						},

						new LightInfo[]
						{
							new LightInfo("brp_p_light_11_0", new Vector3(6.447819f, 496.3773f, 3.396668f)),

							new LightInfo("brp_p_light_7_0", new Vector3(6.411898f, 498.1527f, 3.483641f)),
							new LightInfo("brp_p_light_7_0", new Vector3(6.497867f, 494.6928f, 3.483642f)),
						},

						new LightInfo[]
						{
							new LightInfo("brp_p_light_11_0", new Vector3(6.462771f, 490.1754f, 3.396668f)),

							new LightInfo("brp_p_light_7_0", new Vector3(6.468739f, 488.5619f, 3.483641f)),
							new LightInfo("brp_p_light_7_0", new Vector3(6.432286f, 491.8756f, 3.483641f)),
						},

						new LightInfo[]
						{
							new LightInfo("brp_p_light_11_0", new Vector3(-4.510116f, 496.7023f, 3.396668f)),

							new LightInfo("brp_p_light_7_0", new Vector3(-4.440294f, 494.8429f, 3.483641f)),
							new LightInfo("brp_p_light_7_0", new Vector3(-4.544689f, 498.5695f, 3.483641f)),
						},

						new LightInfo[]
						{
							new LightInfo("brp_p_light_11_0", new Vector3(-4.55655f, 489.7246f, 3.396668f)),

							new LightInfo("brp_p_light_7_0", new Vector3(-4.565982f, 491.7129f, 3.483641f)),
							new LightInfo("brp_p_light_7_0", new Vector3(-4.521025f, 488.0248f, 3.483641f)),
						},

						new LightInfo[] { new LightInfo("brp_p_light_7_0", new Vector3(-1.245679f, 489.4554f, -0.2163407f)) },
						new LightInfo[] { new LightInfo("brp_p_light_7_0", new Vector3(-3.635203f, 492.3578f, -0.2163407f)) },
						new LightInfo[] { new LightInfo("brp_p_light_7_0", new Vector3(-1.216342f, 489.2183f, 3.383641f)) },
						new LightInfo[] { new LightInfo("brp_p_light_1_0", new Vector3(-4.622376f, 497.2843f, -0.2117097f)) },
						new LightInfo[] { new LightInfo("brp_p_light_1_0", new Vector3(0.1963161f, 497.8428f, 3.388272f)) },
					},

					SupportedHouseTypes: new HashSet<HouseBase.Types>() { HouseBase.Types.House, },

					SupportedRoomTypes: new HashSet<RoomTypes>() { RoomTypes.Two, },

					Price: 10_000,

					Offset: new Vector3(0f, -490f, 0f)
				);

				new Style
				(
					Type: 160,

					Position: new Vector3(-857.3874f, 3964.743f, -181.5365f),
					Heading: 269.621f,

					InteriorPosition: new Utils.Vector4(-850f, 3440f + 525f, -180f, 16.2984f),

					Doors: new DoorInfo[]
					{
						new DoorInfo("apa_p_mp_door_apart_door_black", new Vector3(2.083806f, 524.2892f, -1.386743f)),
						new DoorInfo("apa_p_mp_door_apart_door_black", new Vector3(0.6717576f, 523.8997f, 2.213262f)),
						new DoorInfo("apa_p_mp_door_apart_door_black", new Vector3(-4.688732f, 525.2108f, 2.213262f)),
						new DoorInfo("apa_p_mp_door_apart_door_black", new Vector3(2.53278f, 529.405f, -1.386743f)),
						new DoorInfo("apa_p_mp_door_apart_door_black", new Vector3(-0.8186592f, 529.6409f, -1.386743f)),

						new DoorInfo("apa_p_mp_yacht_door_01", new Vector3(3.352888f, 521.9554f, 2.214031f)),
						new DoorInfo("apa_p_mp_yacht_door_01", new Vector3(3.381983f, 518.0277f, 2.214031f)),
						new DoorInfo("apa_p_mp_yacht_door_01", new Vector3(5.239776f, 522.6587f, -1.385974f)),
						new DoorInfo("apa_p_mp_yacht_door_01", new Vector3(4.835387f, 523.8979f, -1.385974f)),

						new DoorInfo("apa_prop_apa_bankdoor_new", new Vector3(-4.676546f, 523.4527f, -1.414499f)),
						new DoorInfo("apa_prop_apa_bankdoor_new", new Vector3(-4.676546f, 526.0452f, -1.414499f)),
					},

					Lights: new LightInfo[][]
					{
						new LightInfo[]
						{
							new LightInfo("brp_p_light_7_0", new Vector3(-2.72358f, 521.959f, 0.747166f)),
							new LightInfo("brp_p_light_7_0", new Vector3(-2.68463f, 529.9005f, 0.747166f)),
						},

						new LightInfo[]
						{
							new LightInfo("brp_p_light_1_0", new Vector3(-8.43482f, 532.8275f, 0.7507652f)),
							new LightInfo("brp_p_light_1_0", new Vector3(-2.43472f, 532.8275f, 0.7507652f)),
						},

						new LightInfo[]
						{
							new LightInfo("brp_p_light_7_0", new Vector3(3.939576f, 522.0867f, 0.7471663f)),
							new LightInfo("brp_p_light_7_0", new Vector3(1.043539f, 522.0633f, 0.747166f)),
						},

						new LightInfo[]
						{
							new LightInfo("brp_p_light_11_0", new Vector3(6.157589f, 526.6196f, 0.710193f)),

							new LightInfo("brp_p_light_7_0", new Vector3(8.289794f, 527.9811f, 0.747166f)),
							new LightInfo("brp_p_light_7_0", new Vector3(4.066166f, 527.9244f, 0.747166f)),
							new LightInfo("brp_p_light_7_0", new Vector3(3.958413f, 525.3485f, 0.747166f)),
							new LightInfo("brp_p_light_7_0", new Vector3(8.116397f, 525.3379f, 0.7471663f)),
						},

						new LightInfo[]
						{
							new LightInfo("brp_p_light_7_0", new Vector3(-2.709936f, 528.7472f, 4.347178f)),
							new LightInfo("brp_p_light_7_0", new Vector3(-2.58771f, 521.71f, 4.347179f)),
							new LightInfo("brp_p_light_7_0", new Vector3(1.316126f, 520.674f, 4.347178f)),
							new LightInfo("brp_p_light_7_0", new Vector3(-2.709936f, 525.0236f, 4.347178f)),
						},

						new LightInfo[]
						{
							new LightInfo("brp_p_light_11_0", new Vector3(6.771834f, 517.3571f, 4.310204f)),

							new LightInfo("brp_p_light_7_0", new Vector3(6.711686f, 519.5197f, 4.347179f)),
							new LightInfo("brp_p_light_7_0", new Vector3(6.774369f, 515.1024f, 4.347179f)),
						},

						new LightInfo[]
						{
							new LightInfo("brp_p_light_11_0", new Vector3(6.750028f, 525.5585f, 4.310205f)),

							new LightInfo("brp_p_light_7_0", new Vector3(6.75043f, 523.464f, 4.347179f)),
							new LightInfo("brp_p_light_7_0", new Vector3(6.73293f, 527.8218f, 4.347179f)),
						},

						new LightInfo[]
						{
							new LightInfo("brp_p_light_1_0", new Vector3(3.950593f, 516.7975f, 0.761797f)),
							new LightInfo("brp_p_light_1_0", new Vector3(7.590529f, 519.2474f, 0.761797f)),
                        },

						new LightInfo[] { new LightInfo("brp_p_light_1_0", new Vector3(-3.17395f, 516.8767f, 4.538254f)) },
						new LightInfo[] { new LightInfo("brp_p_light_1_0", new Vector3(0.6622602f, 526.7348f, 0.7517968f)) },
						new LightInfo[] { new LightInfo("brp_p_light_1_0", new Vector3(1.283101f, 526.8842f, 4.35181f)) },

						new LightInfo[] { new LightInfo("brp_p_light_1_0", new Vector3(-0.09641814f, 530.6565f, 0.7417971f)) },
						new LightInfo[] { new LightInfo("brp_p_light_7_0", new Vector3(-5.79264f, 524.5608f, 4.347178f)) },
						new LightInfo[] { new LightInfo("brp_p_light_7_0", new Vector3(3.472058f, 532.0806f, 0.747166f)) },
						new LightInfo[] { new LightInfo("brp_p_light_7_0", new Vector3(-6.451055f, 524.6136f, 0.747166f)) },
					},

					SupportedHouseTypes: new HashSet<HouseBase.Types>() { HouseBase.Types.House, },

					SupportedRoomTypes: new HashSet<RoomTypes>() { RoomTypes.Two, },

					Price: 10_000,

					Offset: new Vector3(0f, -525f, 0f)
				);

				new Style
				(
					Type: 1010,

					Position: new Vector3(-903.6582f, 3442.644f, -180.5001f),
					Heading: 265.6042f,

					InteriorPosition: new Utils.Vector4(-900f, 3440f, -180f, 7.09981f),

					Doors: new DoorInfo[]
					{
						new DoorInfo("v_ilev_fa_dinedoor", new Vector3(-0.4098042f, 3.401391f, -0.4082556f)),

						new DoorInfo("apa_p_mp_door_stilt_door", new Vector3(0.4312974f, -0.04905067f, -0.3602486f)),

						new DoorInfo("prop_ret_door_02", new Vector3(-2.925214f, 1.240639f, -0.4082546f)),
					},

					Lights: new LightInfo[][]
					{
						new LightInfo[] { new LightInfo("brp_p_light_3_1", new Vector3(-2.263044f, 3.407688f, 1.481788f)) },
						new LightInfo[] { new LightInfo("brp_p_light_3_1", new Vector3(1.42442f, -2.815429f, 1.491786f)) },
						new LightInfo[] { new LightInfo("brp_p_light_3_1", new Vector3(1.551695f, 2.69418f, 1.491788f)) },
						new LightInfo[] { new LightInfo("brp_p_light_3_1", new Vector3(-2.263044f, -0.5923127f, 1.481788f)) },
					},

					SupportedHouseTypes: new HashSet<HouseBase.Types>() { HouseBase.Types.Apartments, },

					SupportedRoomTypes: new HashSet<RoomTypes>() { RoomTypes.Two, },

					Price: 10_000
				);

				new Style
				(
					Type: 1020,

					Position: new Vector3(-903.5726f, 3476.603f, -180.5001f),
					Heading: 265.2024f,

					InteriorPosition: new Utils.Vector4(-900f, 3440f + 35f, -180f, 7.360031f),

					Doors: new DoorInfo[]
					{
						new DoorInfo("prop_door_01", new Vector3(-1.405509f, 38.54019f, -0.4082565f)),

						new DoorInfo("v_ilev_fa_dinedoor", new Vector3(-2.694798f, 34.13317f, -0.4082565f)),

						new DoorInfo("v_ilev_fa_roomdoor", new Vector3(-0.7145147f, 35.32466f, -0.4082565f)),
					},

					Lights: new LightInfo[][]
					{
						new LightInfo[] { new LightInfo("brp_p_light_3_1", new Vector3(-2.497479f, 36.39978f, 1.491785f)) },

						new LightInfo[]
						{
							new LightInfo("brp_p_light_3_1", new Vector3(1.753863f, 39.39698f, 1.481785f)),
                            new LightInfo("brp_p_light_3_1", new Vector3(1.753863f, 35.99698f, 1.481785f)),
                        },

						new LightInfo[]
						{
							new LightInfo("brp_p_light_5_0", new Vector3(-1.610097f, 31.95196f, 1.497232f)),
                            new LightInfo("brp_p_light_5_0", new Vector3(2.086908f, 31.74992f, 1.497242f)),
                        },

						new LightInfo[] { new LightInfo("brp_p_light_5_0", new Vector3(-2.482678f, 39.65707f, 1.495396f)) },
					},

					SupportedHouseTypes: new HashSet<HouseBase.Types>() { HouseBase.Types.Apartments, },

					SupportedRoomTypes: new HashSet<RoomTypes>() { RoomTypes.Two, },

					Price: 10_000,

					Offset: new Vector3(0f, -35f, 0f)
				);

				new Style
				(
					Type: 1030,

					Position: new Vector3(-903.665f, 3504.762f, -180.5001f),
					Heading: 265.8553f,

					InteriorPosition: new Utils.Vector4(-900f, 3440f + 70f, -180f, 7.91285f),

					Doors: new DoorInfo[]
					{
						new DoorInfo("apa_p_mp_yacht_door_01", new Vector3(3.442998f, 69.67144f, -0.349474f)),
						new DoorInfo("apa_p_mp_yacht_door_01", new Vector3(-0.5560716f, 64.13625f, -0.349474f)),

						new DoorInfo("apa_p_mp_yacht_door_02", new Vector3(-1.028391f, 66.02565f, -0.3497582f)),
					},

					Lights: new LightInfo[][]
					{
                        new LightInfo[] { new LightInfo("brp_p_light_7_0", new Vector3(-2.485538f, 64.77189f, 1.496979f)) },

                        new LightInfo[]
						{
							new LightInfo("brp_p_light_7_0", new Vector3(-1.585538f, 68.7719f, 1.496979f)),
                            new LightInfo("brp_p_light_7_0", new Vector3(-3.185537f, 67.37189f, 1.496979f)),
                        },

                        new LightInfo[]
						{
							new LightInfo("brp_p_light_1_0", new Vector3(1.662424f, 67.87781f, 1.497011f)),
                            new LightInfo("brp_p_light_1_0", new Vector3(1.662423f, 64.87782f, 1.497011f)),
                        },

                        new LightInfo[]
						{
							new LightInfo("brp_p_light_1_0", new Vector3(2.992423f, 75.25782f, 1.497011f)),
                            new LightInfo("brp_p_light_1_0", new Vector3(-1.007577f, 75.25782f, 1.497011f)),
                            new LightInfo("brp_p_light_1_0", new Vector3(-1.007577f, 70.93782f, 1.497011f)),
                            new LightInfo("brp_p_light_1_0", new Vector3(2.992423f, 70.93782f, 1.497011f)),
                            new LightInfo("brp_p_light_1_0", new Vector3(0.9924229f, 73.03782f, 1.497011f)),
                        },
					},

					SupportedHouseTypes: new HashSet<HouseBase.Types>() { HouseBase.Types.Apartments, },

					SupportedRoomTypes: new HashSet<RoomTypes>() { RoomTypes.Two, },

					Price: 10_000,

					Offset: new Vector3(0f, -70f, 0f)
				);

				new Style
				(
					Type: 1040,

					Position: new Vector3(-905.8348f, 3543.652f, -180.5001f),
					Heading: 267.7038f,

					InteriorPosition: new Utils.Vector4(-900f, 3440f + 105f, -180f, 9.25402f),

					Doors: new DoorInfo[]
					{
						new DoorInfo("apa_p_mp_door_stilt_door", new Vector3(-1.254941f, 105.5985f, -0.3502483f)),
						new DoorInfo("apa_p_mp_door_stilt_door", new Vector3(1.25506f, 105.5985f, -0.3502483f)),
					},

					Lights: new LightInfo[][]
					{
						new LightInfo[]
						{
							new LightInfo("brp_p_light_3_1", new Vector3(-4.636493f, 103.6411f, 1.491739f)),
							new LightInfo("brp_p_light_3_1", new Vector3(-2.636492f, 103.6411f, 1.491739f)),
							new LightInfo("brp_p_light_3_1", new Vector3(-0.6364918f, 103.6411f, 1.491739f)),
                        },

						new LightInfo[]
						{
							new LightInfo("brp_p_light_3_1", new Vector3(-4.636493f, 106.6411f, 1.491739f)),
							new LightInfo("brp_p_light_3_1", new Vector3(-2.636492f, 108.6411f, 1.491739f)),
                        },

						new LightInfo[]
						{
                            new LightInfo("brp_p_light_3_1", new Vector3(4.363507f, 109.7411f, 1.491739f)),
							new LightInfo("brp_p_light_3_1", new Vector3(2.763509f, 108.1411f, 1.491739f)),
							new LightInfo("brp_p_light_3_1", new Vector3(3.863508f, 106.2411f, 1.491739f)),
                        },

						new LightInfo[]
						{
                            new LightInfo("brp_p_light_3_1", new Vector3(3.242906f, 103.5503f, 1.494738f)),

                            new LightInfo("brp_p_light_5_0", new Vector3(2.370064f, 100.706f, 1.505358f)),
                        },

                        new LightInfo[] { new LightInfo("brp_p_light_1_0", new Vector3(-0.02290444f, 106.1517f, 1.500002f)) },
                        new LightInfo[] { new LightInfo("brp_p_light_3_1", new Vector3(-1.636492f, 100.6411f, 1.491739f)) },
                        new LightInfo[] { new LightInfo("brp_p_light_7_0", new Vector3(0.01108941f, 109.2589f, 1.495928f)) },
                    },

					SupportedHouseTypes: new HashSet<HouseBase.Types>() { HouseBase.Types.Apartments, },

					SupportedRoomTypes: new HashSet<RoomTypes>() { RoomTypes.Two, },

					Price: 10_000,

					Offset: new Vector3(0f, -105f, 0f)
				);

				new Style
				(
					Type: 1050,

					Position: new Vector3(-905.8141f, 3584.104f, -180.5001f),
					Heading: 265.9755f,

					InteriorPosition: new Utils.Vector4(-900f, 3440f + 140f, -180f, 10.4071f),

					Doors: new DoorInfo[]
					{
						new DoorInfo("apa_p_mp_yacht_door_01", new Vector3(-1.438152f, 137.3335f, -0.3494778f)),
						new DoorInfo("apa_p_mp_yacht_door_01", new Vector3(0.2887057f, 139.3858f, -0.3494778f)),
						new DoorInfo("apa_p_mp_yacht_door_01", new Vector3(0.3093196f, 145.5758f, -0.3494778f)),

						new DoorInfo("apa_p_mp_yacht_door_02", new Vector3(-1.852429f, 141.5582f, -0.3497639f)),
					},

					Lights: new LightInfo[][]
					{
						new LightInfo[]
						{
                            new LightInfo("brp_p_light_1_0", new Vector3(-0.7177288f, 140.7123f, 1.497004f)),
							new LightInfo("brp_p_light_1_0", new Vector3(-0.7177288f, 137.9123f, 1.497004f)),
							new LightInfo("brp_p_light_1_0", new Vector3(-5.19773f, 143.9123f, 1.497004f)),
							new LightInfo("brp_p_light_1_0", new Vector3(-5.19773f, 146.7123f, 1.497004f)),
							new LightInfo("brp_p_light_1_0", new Vector3(-0.7177288f, 143.9123f, 1.497004f)),
							new LightInfo("brp_p_light_1_0", new Vector3(-0.7177288f, 146.7123f, 1.497004f)),
                        },

						new LightInfo[]
						{
                            new LightInfo("brp_p_light_6_0", new Vector3(-5.319083f, 137.5245f, 0.1927967f)),
							new LightInfo("brp_p_light_6_0", new Vector3(-1.953239f, 138.3401f, 0.1904049f)),
							new LightInfo("brp_p_light_6_0", new Vector3(-1.949531f, 142.1122f, 0.01664162f)),

							new LightInfo("brp_p_light_7_0", new Vector3(-5.287843f, 140.0756f, 1.489592f)),
							new LightInfo("brp_p_light_7_0", new Vector3(-3.287843f, 140.0756f, 1.489592f)),
                        },

						new LightInfo[]
						{
                            new LightInfo("brp_p_light_1_0", new Vector3(-0.7191803f, 135.2013f, 1.496958f)),
							new LightInfo("brp_p_light_1_0", new Vector3(2.28082f, 135.2013f, 1.496958f)),
							new LightInfo("brp_p_light_1_0", new Vector3(5.281544f, 135.202f, 1.496271f)),
                        },

						new LightInfo[]
						{
                            new LightInfo("brp_p_light_1_0", new Vector3(1.28082f, 140.0013f, 1.496958f)),
                            new LightInfo("brp_p_light_1_0", new Vector3(5.278643f, 140.0011f, 1.497271f)),

                            new LightInfo("brp_p_light_6_0", new Vector3(4.800152f, 137.5245f, 0.229517f)),
							new LightInfo("brp_p_light_6_0", new Vector3(1.800154f, 137.5245f, 0.229517f)),

							new LightInfo("brp_p_light_7_0", new Vector3(3.312158f, 140.0756f, 1.489592f)),
                        },

						new LightInfo[]
						{
                            new LightInfo("brp_p_light_1_0", new Vector3(1.28082f, 145.4013f, 1.496958f)),
							new LightInfo("brp_p_light_1_0", new Vector3(5.278643f, 145.4011f, 1.497271f)),

							new LightInfo("brp_p_light_7_0", new Vector3(3.312158f, 145.3756f, 1.489592f)),
                        },
					},

					SupportedHouseTypes: new HashSet<HouseBase.Types>() { HouseBase.Types.Apartments, },

					SupportedRoomTypes: new HashSet<RoomTypes>() { RoomTypes.Two, },

					Price: 10_000,

					Offset: new Vector3(0f, -140f, 0f)
				);

				new Style
				(
					Type: 1060,

					Position: new Vector3(-905.9375f, 3617.076f, -180.5001f),
					Heading: 272.7087f,

					InteriorPosition: new Utils.Vector4(-900f, 3440f + 175f, -180f, 11.1925f),

					Doors: new DoorInfo[]
					{
						new DoorInfo("apa_p_mp_yacht_door_02", new Vector3(-5.395699f, 180.2851f, -0.3497677f)),

						new DoorInfo("apa_p_mp_yacht_door_01", new Vector3(-1.423927f, 178.4041f, -0.3494816f)),
						new DoorInfo("apa_p_mp_yacht_door_01", new Vector3(-1.43786f, 175.5864f, -0.3494816f)),
					},

					Lights: new LightInfo[][]
					{
						new LightInfo[]
						{
                            new LightInfo("brp_p_light_6_0", new Vector3(-1.544488f, 180.6005f, 0.3466968f)),
                            new LightInfo("brp_p_light_6_0", new Vector3(-1.544488f, 183.6315f, 0.3511677f)),

                            new LightInfo("brp_p_light_7_0", new Vector3(-3.00092f, 182.161f, 1.496904f)),
							new LightInfo("brp_p_light_7_0", new Vector3(-5.400918f, 182.161f, 1.496904f)),

							new LightInfo("brp_p_light_11_0", new Vector3(-4.329724f, 182.1355f, 1.396646f)),
                        },

						new LightInfo[]
						{
                            new LightInfo("brp_p_light_6_0", new Vector3(-1.344603f, 183.5446f, 0.2681332f)),
							new LightInfo("brp_p_light_6_0", new Vector3(-1.344603f, 180.5446f, 0.2681332f)),
							new LightInfo("brp_p_light_6_0", new Vector3(5.579499f, 178.0592f, 0.269804f)),

							new LightInfo("brp_p_light_7_0", new Vector3(0.6990807f, 182.961f, 1.496904f)),
							new LightInfo("brp_p_light_7_0", new Vector3(0.6990806f, 178.961f, 1.496904f)),
							new LightInfo("brp_p_light_7_0", new Vector3(4.691546f, 182.962f, 1.497648f)),
							new LightInfo("brp_p_light_7_0", new Vector3(4.691546f, 178.962f, 1.497648f)),
                        },

						new LightInfo[]
						{
                            new LightInfo("brp_p_light_7_0", new Vector3(0.6990806f, 176.961f, 1.496904f)),
							new LightInfo("brp_p_light_7_0", new Vector3(0.6990806f, 172.961f, 1.496904f)),
							new LightInfo("brp_p_light_7_0", new Vector3(4.691546f, 176.962f, 1.497648f)),
							new LightInfo("brp_p_light_7_0", new Vector3(4.691546f, 172.961f, 1.497648f)),
                        },

						new LightInfo[]
						{
                            new LightInfo("brp_p_light_1_0", new Vector3(-4.28703f, 167.8151f, 1.497004f)),
							new LightInfo("brp_p_light_1_0", new Vector3(-0.28703f, 167.8151f, 1.497004f)),
							new LightInfo("brp_p_light_1_0", new Vector3(3.71297f, 167.8151f, 1.497004f)),
							new LightInfo("brp_p_light_1_0", new Vector3(3.71297f, 170.8151f, 1.497004f)),
							new LightInfo("brp_p_light_1_0", new Vector3(-0.28703f, 170.8151f, 1.497004f)),
							new LightInfo("brp_p_light_1_0", new Vector3(-4.28703f, 170.8151f, 1.497004f)),

							new LightInfo("brp_p_light_11_0", new Vector3(-2.359723f, 170.7755f, 1.396646f)),
							new LightInfo("brp_p_light_11_0", new Vector3(5.640278f, 170.7755f, 1.396646f)),
							new LightInfo("brp_p_light_11_0", new Vector3(5.640278f, 167.7755f, 1.396646f)),
							new LightInfo("brp_p_light_11_0", new Vector3(-2.359723f, 167.7755f, 1.396646f)),
							new LightInfo("brp_p_light_11_0", new Vector3(5.640278f, 169.2755f, 1.396646f)),
							new LightInfo("brp_p_light_11_0", new Vector3(1.640278f, 167.7755f, 1.396646f)),
							new LightInfo("brp_p_light_11_0", new Vector3(1.640278f, 170.7755f, 1.396646f)),
							new LightInfo("brp_p_light_11_0", new Vector3(1.640278f, 169.2755f, 1.396646f)),
							new LightInfo("brp_p_light_11_0", new Vector3(-5.359723f, 167.7755f, 1.396646f)),
							new LightInfo("brp_p_light_11_0", new Vector3(-5.359723f, 170.7755f, 1.396646f)),
							new LightInfo("brp_p_light_11_0", new Vector3(-5.359723f, 169.2755f, 1.396646f)),
                        },

						new LightInfo[]
						{
                            new LightInfo("brp_p_light_7_0", new Vector3(-2.40092f, 178.161f, 1.496904f)),
							new LightInfo("brp_p_light_7_0", new Vector3(-2.40092f, 173.961f, 1.496904f)),
							new LightInfo("brp_p_light_7_0", new Vector3(-5.40092f, 173.961f, 1.496904f)),
							new LightInfo("brp_p_light_7_0", new Vector3(-5.40092f, 178.161f, 1.496904f)),

							new LightInfo("brp_p_light_11_0", new Vector3(-3.359723f, 178.6755f, 1.396646f)),
							new LightInfo("brp_p_light_11_0", new Vector3(-4.529722f, 178.6755f, 1.396646f)),
							new LightInfo("brp_p_light_11_0", new Vector3(-3.359723f, 173.4755f, 1.396646f)),
							new LightInfo("brp_p_light_11_0", new Vector3(-4.529722f, 173.4755f, 1.396646f)),
							new LightInfo("brp_p_light_11_0", new Vector3(-4.529722f, 177.3755f, 1.396646f)),
							new LightInfo("brp_p_light_11_0", new Vector3(-3.359723f, 177.3755f, 1.396646f)),
							new LightInfo("brp_p_light_11_0", new Vector3(-4.529722f, 176.0755f, 1.396646f)),
							new LightInfo("brp_p_light_11_0", new Vector3(-3.359723f, 176.0755f, 1.396646f)),
							new LightInfo("brp_p_light_11_0", new Vector3(-4.529722f, 174.7755f, 1.396646f)),
							new LightInfo("brp_p_light_11_0", new Vector3(-3.359723f, 174.7755f, 1.396646f)),
                        },
					},

					SupportedHouseTypes: new HashSet<HouseBase.Types>() { HouseBase.Types.Apartments, },

					SupportedRoomTypes: new HashSet<RoomTypes>() { RoomTypes.Two, },

					Price: 10_000,

					Offset: new Vector3(0f, -175f, 0f)
				);

				new Style
				(
					Type: 1070,

					Position: new Vector3(-905.9075f, 3649.897f, -180.4993f),
					Heading: 268.0993f,

					InteriorPosition: new Utils.Vector4(-900f, 3440f + 210f, -180f, 10.7189f),

					Doors: new DoorInfo[]
					{
						new DoorInfo("apa_heist_apart2_door", new Vector3(-1.660513f, 211.5338f, -0.3495083f)),
						new DoorInfo("apa_heist_apart2_door", new Vector3(0.1694885f, 211.5338f, -0.3495083f)),
						new DoorInfo("apa_heist_apart2_door", new Vector3(2.339487f, 209.4938f, -0.3495083f)),

						new DoorInfo("v_ilev_fh_door02", new Vector3(-1.867892f, 204.5072f, -0.4089871f)),
					},

					Lights: new LightInfo[][]
					{
						new LightInfo[]
						{
                            new LightInfo("brp_p_light_3_1", new Vector3(4.704434f, 202.9523f, 1.48247f)),
							new LightInfo("brp_p_light_3_1", new Vector3(1.976834f, 203.2924f, 1.482523f)),
							new LightInfo("brp_p_light_3_1", new Vector3(1.976834f, 207.2924f, 1.482523f)),
							new LightInfo("brp_p_light_3_1", new Vector3(4.704434f, 208.9523f, 1.48247f)),
                        },

						new LightInfo[]
						{
                            new LightInfo("brp_p_light_3_1", new Vector3(4.704434f, 213.9522f, 1.48247f)),
							new LightInfo("brp_p_light_3_1", new Vector3(2.704434f, 215.9523f, 1.48247f)),
                        },

						new LightInfo[]
						{
                            new LightInfo("brp_p_light_3_1", new Vector3(-3.623166f, 213.2924f, 1.482523f)),
							new LightInfo("brp_p_light_3_1", new Vector3(-3.395565f, 216.2522f, 1.48247f)),
                        },

						new LightInfo[]
						{
                            new LightInfo("brp_p_light_3_1", new Vector3(0.6768188f, 210.0923f, 1.482529f)),

							new LightInfo("brp_p_light_5_0", new Vector3(-3.78965f, 203.6294f, 1.504824f)),
							new LightInfo("brp_p_light_5_0", new Vector3(-3.78965f, 206.6294f, 1.504824f)),
							new LightInfo("brp_p_light_5_0", new Vector3(-0.7896497f, 206.6294f, 1.504824f)),
                        },

						new LightInfo[] { new LightInfo("brp_p_light_5_0", new Vector3(-0.7896497f, 203.6294f, 1.504824f)), },

/*						new LightInfo[]
						{
                            new LightInfo("brp_p_light_5_0", new Vector3(-3.78965f, 34.62939f, 1.504824f)),
                        },*/
					},

					SupportedHouseTypes: new HashSet<HouseBase.Types>() { HouseBase.Types.Apartments, },

					SupportedRoomTypes: new HashSet<RoomTypes>() { RoomTypes.Two, },

					Price: 10_000,

					Offset: new Vector3(0f, -210f, 0f)
				);

				new Style // no lights in 2 rooms, but fake light from window, prob change?
				(
					Type: 1080,

					Position: new Vector3(-905.9802f, 3684.474f, -180.5001f),
					Heading: 268.7205f,

					InteriorPosition: new Utils.Vector4(-900f, 3440f + 245f, -180f, 11.213f),

					Doors: new DoorInfo[]
					{
						new DoorInfo("apa_p_mp_yacht_door_01", new Vector3(0.2705393f, 248.3371f, -0.3494778f)),
						new DoorInfo("apa_p_mp_yacht_door_01", new Vector3(0.2309114f, 246.9336f, -0.3494778f)),
						new DoorInfo("apa_p_mp_yacht_door_01", new Vector3(-0.5667678f, 250.2692f, -0.3494778f)),

						new DoorInfo("apa_p_mp_yacht_door_02", new Vector3(-2.632566f, 249.6747f, -0.34976f)),

						new DoorInfo("v_ilev_fib_door2", new Vector3(0.2463773f, 237.9085f, -0.3499966f)),
						new DoorInfo("v_ilev_fib_door2", new Vector3(0.2463773f, 240.5055f, -0.3499966f)),
					},

					Lights: new LightInfo[][]
					{
						new LightInfo[]
						{
                            new LightInfo("brp_p_light_1_0", new Vector3(-5.093377f, 244.7984f, 1.490641f)),
							new LightInfo("brp_p_light_1_0", new Vector3(-1.193377f, 244.7984f, 1.490641f)),
							new LightInfo("brp_p_light_1_0", new Vector3(-1.193377f, 247.7984f, 1.490641f)),
                        },

						new LightInfo[]
						{
                            new LightInfo("brp_p_light_1_0", new Vector3(-1.093377f, 237.7984f, 1.490641f)),
							new LightInfo("brp_p_light_1_0", new Vector3(-1.093377f, 241.7984f, 1.490641f)),
							new LightInfo("brp_p_light_1_0", new Vector3(-4.093377f, 241.7984f, 1.490641f)),
							new LightInfo("brp_p_light_1_0", new Vector3(-4.093377f, 237.7984f, 1.490641f)),
                        },

						new LightInfo[]
						{
                            new LightInfo("brp_p_light_6_0", new Vector3(-4.658952f, 246.7252f, 0.2307453f)),

                            new LightInfo("brp_p_light_7_0", new Vector3(-4.449864f, 248.4461f, 1.279993f)),
                        },

						new LightInfo[]
						{
                            new LightInfo("brp_p_light_7_0", new Vector3(-5.350868f, 252.1451f, 1.279947f)),
							new LightInfo("brp_p_light_7_0", new Vector3(-3.350868f, 252.1451f, 1.279947f)),
							new LightInfo("brp_p_light_7_0", new Vector3(-1.350868f, 252.1451f, 1.279947f)),

							new LightInfo("brp_p_light_11_1", new Vector3(-5.349224f, 252.144f, 1.390802f)),
							new LightInfo("brp_p_light_11_1", new Vector3(-3.349224f, 252.144f, 1.390802f)),
							new LightInfo("brp_p_light_11_1", new Vector3(-1.349224f, 252.144f, 1.390802f)),
                        },

						new LightInfo[] { new LightInfo("brp_p_light_6_0", new Vector3(4.289703f, 253.9034f, 0.08818245f)) },
					},

					SupportedHouseTypes: new HashSet<HouseBase.Types>() { HouseBase.Types.Apartments, },

					SupportedRoomTypes: new HashSet<RoomTypes>() { RoomTypes.Two, },

					Price: 10_000,

					Offset: new Vector3(0f, -245f, 0f)
				);

				new Style
				(
					Type: 1090,

					Position: new Vector3(-905.9459f, 3716.544f, -180.5001f),
					Heading: 263.868f,

					InteriorPosition: new Utils.Vector4(-900f, 3440f + 280f, -180f, 12.3893f),

					Doors: new DoorInfo[]
					{
						new DoorInfo("apa_p_mp_yacht_door_02", new Vector3(-3.264183f, 278.3035f, -0.34976f)),

						new DoorInfo("apa_p_mp_yacht_door_01", new Vector3(-0.5665947f, 274.2459f, -0.3494778f)),
						new DoorInfo("apa_p_mp_yacht_door_01", new Vector3(0.2414839f, 275.8889f, -0.3494778f)),
						new DoorInfo("apa_p_mp_yacht_door_01", new Vector3(0.2392138f, 278.769f, -0.3494778f)),
					},

					Lights: new LightInfo[][]
					{
						new LightInfo[]
						{
                            new LightInfo("brp_p_light_6_0", new Vector3(-5.638455f, 274.7072f, 0.1355228f)),
							new LightInfo("brp_p_light_6_0", new Vector3(-3.488455f, 274.7072f, 0.1355228f)),
							new LightInfo("brp_p_light_6_0", new Vector3(-5.638455f, 269.7072f, 0.1355228f)),
							new LightInfo("brp_p_light_6_0", new Vector3(-3.638455f, 269.7072f, 0.1355228f)),
							new LightInfo("brp_p_light_6_0", new Vector3(-1.638455f, 269.7072f, 0.1355228f)),

							new LightInfo("brp_p_light_11_1", new Vector3(-4.815666f, 271.2173f, 1.406649f)),
                        },

						new LightInfo[]
						{
                            new LightInfo("brp_p_light_7_0", new Vector3(0.8511442f, 275.3337f, 1.490053f)),
							new LightInfo("brp_p_light_7_0", new Vector3(1.791144f, 270.0337f, 1.490053f)),
							new LightInfo("brp_p_light_7_0", new Vector3(4.791145f, 270.0337f, 1.490053f)),
							new LightInfo("brp_p_light_7_0", new Vector3(4.786145f, 275.2637f, 1.296053f)),
							new LightInfo("brp_p_light_7_0", new Vector3(1.785144f, 275.2637f, 1.290056f)),

							new LightInfo("brp_p_light_11_1", new Vector3(4.784331f, 275.3373f, 1.406649f)),
							new LightInfo("brp_p_light_11_1", new Vector3(4.784331f, 272.3673f, 1.406649f)),
							new LightInfo("brp_p_light_11_1", new Vector3(4.784331f, 273.8673f, 1.406649f)),
							new LightInfo("brp_p_light_11_1", new Vector3(1.784333f, 273.8673f, 1.406649f)),
							new LightInfo("brp_p_light_11_1", new Vector3(1.784333f, 272.3673f, 1.406649f)),
							new LightInfo("brp_p_light_11_1", new Vector3(1.784333f, 275.3373f, 1.406649f)),
							new LightInfo("brp_p_light_11_1", new Vector3(3.284332f, 273.8673f, 1.406649f)),
							new LightInfo("brp_p_light_11_1", new Vector3(3.284332f, 272.3673f, 1.406649f)),
							new LightInfo("brp_p_light_11_1", new Vector3(3.284332f, 275.3373f, 1.406649f)),
                        },

						new LightInfo[]
						{
                            new LightInfo("brp_p_light_1_0", new Vector3(-4.536665f, 280.3985f, 1.489977f)),

                            new LightInfo("brp_p_light_6_0", new Vector3(-4.559184f, 282.4691f, 0.04358101f)),

                            new LightInfo("brp_p_light_11_1", new Vector3(-5.625668f, 281.6973f, 1.406649f)),
							new LightInfo("brp_p_light_11_1", new Vector3(-3.425669f, 281.6973f, 1.406649f)),
							new LightInfo("brp_p_light_11_1", new Vector3(-3.425669f, 279.1173f, 1.406649f)),
							new LightInfo("brp_p_light_11_1", new Vector3(-5.625668f, 279.1173f, 1.406649f)),
                        },

						new LightInfo[]
						{
                            new LightInfo("brp_p_light_1_0", new Vector3(-5.357264f, 283.4746f, 1.890642f)),
							new LightInfo("brp_p_light_1_0", new Vector3(-2.757266f, 283.4746f, 1.890642f)),
							new LightInfo("brp_p_light_1_0", new Vector3(-0.1572667f, 283.4746f, 1.890642f)),
							new LightInfo("brp_p_light_1_0", new Vector3(2.442733f, 283.4746f, 1.890642f)),
							new LightInfo("brp_p_light_1_0", new Vector3(5.042731f, 283.4746f, 1.890642f)),
							new LightInfo("brp_p_light_1_0", new Vector3(5.042731f, 288.6746f, 1.890642f)),
							new LightInfo("brp_p_light_1_0", new Vector3(2.442733f, 288.6746f, 1.890642f)),
							new LightInfo("brp_p_light_1_0", new Vector3(-0.1572667f, 288.6746f, 1.890642f)),
							new LightInfo("brp_p_light_1_0", new Vector3(-2.757266f, 288.6746f, 1.890642f)),
							new LightInfo("brp_p_light_1_0", new Vector3(-5.357264f, 288.6746f, 1.890642f)),
							new LightInfo("brp_p_light_1_0", new Vector3(5.042731f, 286.0746f, 1.890642f)),
							new LightInfo("brp_p_light_1_0", new Vector3(2.442733f, 286.0746f, 1.890642f)),
							new LightInfo("brp_p_light_1_0", new Vector3(-0.1572667f, 286.0746f, 1.890642f)),
							new LightInfo("brp_p_light_1_0", new Vector3(-2.757266f, 286.0746f, 1.890642f)),
							new LightInfo("brp_p_light_1_0", new Vector3(-5.357264f, 286.0746f, 1.890642f)),

							new LightInfo("brp_p_light_7_0", new Vector3(-3.958856f, 289.7937f, 2.089968f)),

							new LightInfo("brp_p_light_11_0", new Vector3(5.040808f, 283.4724f, 1.99933f)),
							new LightInfo("brp_p_light_11_0", new Vector3(5.040808f, 284.7725f, 1.99933f)),
							new LightInfo("brp_p_light_11_0", new Vector3(5.040808f, 286.0724f, 1.99933f)),
							new LightInfo("brp_p_light_11_0", new Vector3(5.040808f, 287.3725f, 1.99933f)),
							new LightInfo("brp_p_light_11_0", new Vector3(5.040808f, 288.6724f, 1.99933f)),
							new LightInfo("brp_p_light_11_0", new Vector3(-5.359194f, 288.6724f, 1.99933f)),
							new LightInfo("brp_p_light_11_0", new Vector3(-5.359194f, 287.3725f, 1.99933f)),
							new LightInfo("brp_p_light_11_0", new Vector3(-5.359194f, 286.0724f, 1.99933f)),
							new LightInfo("brp_p_light_11_0", new Vector3(-5.359194f, 284.7725f, 1.99933f)),
							new LightInfo("brp_p_light_11_0", new Vector3(-5.359194f, 283.4724f, 1.99933f)),
							new LightInfo("brp_p_light_11_0", new Vector3(-4.059193f, 288.6724f, 1.99933f)),
							new LightInfo("brp_p_light_11_0", new Vector3(-4.059193f, 287.3725f, 1.99933f)),
							new LightInfo("brp_p_light_11_0", new Vector3(-4.059193f, 286.0724f, 1.99933f)),
							new LightInfo("brp_p_light_11_0", new Vector3(-4.059193f, 284.7725f, 1.99933f)),
							new LightInfo("brp_p_light_11_0", new Vector3(-4.059193f, 283.4724f, 1.99933f)),
							new LightInfo("brp_p_light_11_0", new Vector3(-2.759194f, 288.6724f, 1.99933f)),
							new LightInfo("brp_p_light_11_0", new Vector3(-2.759194f, 287.3725f, 1.99933f)),
							new LightInfo("brp_p_light_11_0", new Vector3(-2.759194f, 286.0724f, 1.99933f)),
							new LightInfo("brp_p_light_11_0", new Vector3(-2.759194f, 284.7725f, 1.99933f)),
							new LightInfo("brp_p_light_11_0", new Vector3(-2.759194f, 283.4724f, 1.99933f)),
							new LightInfo("brp_p_light_11_0", new Vector3(-1.459193f, 288.6724f, 1.99933f)),
							new LightInfo("brp_p_light_11_0", new Vector3(-1.459193f, 287.3725f, 1.99933f)),
							new LightInfo("brp_p_light_11_0", new Vector3(-1.459193f, 286.0724f, 1.99933f)),
							new LightInfo("brp_p_light_11_0", new Vector3(-1.459193f, 284.7725f, 1.99933f)),
							new LightInfo("brp_p_light_11_0", new Vector3(-1.459193f, 283.4724f, 1.99933f)),
							new LightInfo("brp_p_light_11_0", new Vector3(-0.1591928f, 288.6724f, 1.99933f)),
							new LightInfo("brp_p_light_11_0", new Vector3(-0.1591928f, 287.3725f, 1.99933f)),
							new LightInfo("brp_p_light_11_0", new Vector3(-0.1591928f, 286.0724f, 1.99933f)),
							new LightInfo("brp_p_light_11_0", new Vector3(-0.1591928f, 284.7725f, 1.99933f)),
							new LightInfo("brp_p_light_11_0", new Vector3(-0.1591928f, 283.4724f, 1.99933f)),
							new LightInfo("brp_p_light_11_0", new Vector3(1.140807f, 288.6724f, 1.99933f)),
							new LightInfo("brp_p_light_11_0", new Vector3(1.140807f, 287.3725f, 1.99933f)),
							new LightInfo("brp_p_light_11_0", new Vector3(1.140807f, 286.0724f, 1.99933f)),
							new LightInfo("brp_p_light_11_0", new Vector3(1.140807f, 284.7725f, 1.99933f)),
							new LightInfo("brp_p_light_11_0", new Vector3(1.140807f, 283.4724f, 1.99933f)),
							new LightInfo("brp_p_light_11_0", new Vector3(2.440807f, 288.6724f, 1.99933f)),
							new LightInfo("brp_p_light_11_0", new Vector3(2.440807f, 287.3725f, 1.99933f)),
							new LightInfo("brp_p_light_11_0", new Vector3(2.440807f, 286.0724f, 1.99933f)),
							new LightInfo("brp_p_light_11_0", new Vector3(2.440807f, 284.7725f, 1.99933f)),
							new LightInfo("brp_p_light_11_0", new Vector3(2.440807f, 283.4724f, 1.99933f)),
							new LightInfo("brp_p_light_11_0", new Vector3(3.740808f, 288.6724f, 1.99933f)),
							new LightInfo("brp_p_light_11_0", new Vector3(3.740808f, 287.3725f, 1.99933f)),
							new LightInfo("brp_p_light_11_0", new Vector3(3.740808f, 286.0724f, 1.99933f)),
							new LightInfo("brp_p_light_11_0", new Vector3(3.740808f, 284.7725f, 1.99933f)),
							new LightInfo("brp_p_light_11_0", new Vector3(3.740808f, 283.4724f, 1.99933f)),
                        },

						new LightInfo[]
						{
                            new LightInfo("brp_p_light_6_0", new Vector3(-2.549184f, 281.5291f, 0.04358101f)),
							new LightInfo("brp_p_light_6_0", new Vector3(-2.549184f, 278.5291f, 0.04358101f)),
							new LightInfo("brp_p_light_6_0", new Vector3(0.1408161f, 281.5291f, 0.04358101f)),
							new LightInfo("brp_p_light_6_0", new Vector3(0.1408161f, 277.3291f, 0.04358101f)),
							new LightInfo("brp_p_light_6_0", new Vector3(-0.1591839f, 274.3391f, 0.04358101f)),
							new LightInfo("brp_p_light_6_0", new Vector3(-2.259184f, 274.3391f, 0.04358101f)),
							new LightInfo("brp_p_light_6_0", new Vector3(-5.659184f, 274.9091f, 0.04358101f)),
							new LightInfo("brp_p_light_6_0", new Vector3(-3.159184f, 274.9091f, 0.04358101f)),
							new LightInfo("brp_p_light_6_0", new Vector3(-5.659184f, 278.1991f, 0.04358101f)),

							new LightInfo("brp_p_light_7_0", new Vector3(-1.208856f, 278.4937f, 1.490053f)),
							new LightInfo("brp_p_light_7_0", new Vector3(-1.208856f, 276.4937f, 1.490053f)),
							new LightInfo("brp_p_light_7_0", new Vector3(-4.208856f, 276.4937f, 1.490053f)),
							new LightInfo("brp_p_light_7_0", new Vector3(-1.208856f, 280.4937f, 1.490053f)),

							new LightInfo("brp_p_light_11_1", new Vector3(-1.215668f, 277.4973f, 1.406649f)),
							new LightInfo("brp_p_light_11_1", new Vector3(-2.715668f, 276.4973f, 1.406649f)),
							new LightInfo("brp_p_light_11_1", new Vector3(-5.615668f, 276.4973f, 1.406649f)),
							new LightInfo("brp_p_light_11_1", new Vector3(-5.615668f, 275.4973f, 1.406649f)),
							new LightInfo("brp_p_light_11_1", new Vector3(-5.615668f, 277.4973f, 1.406649f)),
							new LightInfo("brp_p_light_11_1", new Vector3(-1.215668f, 279.4973f, 1.406649f)),
							new LightInfo("brp_p_light_11_1", new Vector3(-1.215668f, 281.4973f, 1.406649f)),
							new LightInfo("brp_p_light_11_1", new Vector3(-2.215668f, 281.4973f, 1.406649f)),
							new LightInfo("brp_p_light_11_1", new Vector3(-0.2156675f, 281.4973f, 1.406649f)),
                        },
					},

					SupportedHouseTypes: new HashSet<HouseBase.Types>() { HouseBase.Types.Apartments, },

					SupportedRoomTypes: new HashSet<RoomTypes>() { RoomTypes.Two, },

					Price: 10_000,

					Offset: new Vector3(0f, -280f, 0f)
				);

				new Style(1, 0, new Vector3(0f, 0f, 30f), 10_000);

				new Style(11, 10, new Vector3(0f, 0f, 30f), 10_000);
				new Style(12, 10, new Vector3(0f, 0f, 60f), 10_000);
				new Style(13, 10, new Vector3(0f, 0f, 90f), 10_000);
				new Style(14, 10, new Vector3(0f, 0f, 120f), 10_000);

				new Style(21, 20, new Vector3(0f, 0f, 30f), 10_000);
				new Style(22, 20, new Vector3(0f, 0f, 60f), 10_000);
				new Style(23, 20, new Vector3(0f, 0f, 90f), 10_000);
				new Style(24, 20, new Vector3(0f, 0f, 120f), 10_000);

				new Style(31, 30, new Vector3(0f, 0f, 30f), 10_000);
				new Style(32, 30, new Vector3(0f, 0f, 60f), 10_000);
				new Style(33, 30, new Vector3(0f, 0f, 90f), 10_000);
				new Style(34, 30, new Vector3(0f, 0f, 120f), 10_000);

				new Style(41, 40, new Vector3(0f, 0f, 30f), 10_000);
				new Style(42, 40, new Vector3(0f, 0f, 60f), 10_000);
				new Style(43, 40, new Vector3(0f, 0f, 90f), 10_000);
				new Style(44, 40, new Vector3(0f, 0f, 120f), 10_000);

				new Style(51, 50, new Vector3(0f, 0f, 30f), 10_000);
				new Style(52, 50, new Vector3(0f, 0f, 60f), 10_000);
				new Style(53, 50, new Vector3(0f, 0f, 90f), 10_000);
				new Style(54, 50, new Vector3(0f, 0f, 120f), 10_000);

				new Style(61, 60, new Vector3(0f, 0f, 30f), 10_000);
				new Style(62, 60, new Vector3(0f, 0f, 60f), 10_000);
				new Style(63, 60, new Vector3(0f, 0f, 90f), 10_000);
				new Style(64, 60, new Vector3(0f, 0f, 120f), 10_000);

				new Style(71, 70, new Vector3(0f, 0f, 30f), 10_000);
				new Style(72, 70, new Vector3(0f, 0f, 60f), 10_000);
				new Style(73, 70, new Vector3(0f, 0f, 90f), 10_000);
				new Style(74, 70, new Vector3(0f, 0f, 120f), 10_000);

				new Style(81, 80, new Vector3(0f, 0f, 30f), 10_000);
				new Style(82, 80, new Vector3(0f, 0f, 60f), 10_000);
				new Style(83, 80, new Vector3(0f, 0f, 90f), 10_000);
				new Style(84, 80, new Vector3(0f, 0f, 120f), 10_000);

				new Style(91, 90, new Vector3(0f, 0f, 30f), 10_000);
				new Style(92, 90, new Vector3(0f, 0f, 60f), 10_000);
				new Style(93, 90, new Vector3(0f, 0f, 90f), 10_000);
				new Style(94, 90, new Vector3(0f, 0f, 120f), 10_000);

				new Style(101, 100, new Vector3(0f, 0f, 30f), 10_000);
				new Style(102, 100, new Vector3(0f, 0f, 60f), 10_000);
				new Style(103, 100, new Vector3(0f, 0f, 90f), 10_000);
				new Style(104, 100, new Vector3(0f, 0f, 120f), 10_000);

				new Style(111, 110, new Vector3(0f, 0f, 30f), 10_000);
				new Style(112, 110, new Vector3(0f, 0f, 60f), 10_000);
				new Style(113, 110, new Vector3(0f, 0f, 90f), 10_000);
				new Style(114, 110, new Vector3(0f, 0f, 120f), 10_000);

				new Style(121, 120, new Vector3(0f, 0f, 30f), 10_000);
				new Style(122, 120, new Vector3(0f, 0f, 60f), 10_000);
				new Style(123, 120, new Vector3(0f, 0f, 90f), 10_000);
				new Style(124, 120, new Vector3(0f, 0f, 120f), 10_000);

				new Style(131, 130, new Vector3(0f, 0f, 30f), 10_000);
				new Style(132, 130, new Vector3(0f, 0f, 60f), 10_000);
				new Style(133, 130, new Vector3(0f, 0f, 90f), 10_000);
				new Style(134, 130, new Vector3(0f, 0f, 120f), 10_000);

				new Style(141, 140, new Vector3(0f, 0f, 30f), 10_000);
				new Style(142, 140, new Vector3(0f, 0f, 60f), 10_000);
				new Style(143, 140, new Vector3(0f, 0f, 90f), 10_000);
				new Style(144, 140, new Vector3(0f, 0f, 120f), 10_000);

				new Style(151, 150, new Vector3(0f, 0f, 30f), 10_000);
				new Style(152, 150, new Vector3(0f, 0f, 60f), 10_000);
				new Style(153, 150, new Vector3(0f, 0f, 90f), 10_000);
				new Style(154, 150, new Vector3(0f, 0f, 120f), 10_000);

				new Style(161, 160, new Vector3(0f, 0f, 30f), 10_000);
				new Style(162, 160, new Vector3(0f, 0f, 60f), 10_000);
				new Style(163, 160, new Vector3(0f, 0f, 90f), 10_000);
				new Style(164, 160, new Vector3(0f, 0f, 120f), 10_000);

				new Style(1011, 1010, new Vector3(0f, 0f, 30f), 10_000);
				new Style(1012, 1010, new Vector3(0f, 0f, 60f), 10_000);
				new Style(1013, 1010, new Vector3(0f, 0f, 90f), 10_000);
				new Style(1014, 1010, new Vector3(0f, 0f, 120f), 10_000);

				new Style(1021, 1020, new Vector3(0f, 0f, 30f), 10_000);
				new Style(1022, 1020, new Vector3(0f, 0f, 60f), 10_000);
				new Style(1023, 1020, new Vector3(0f, 0f, 90f), 10_000);
				new Style(1024, 1020, new Vector3(0f, 0f, 120f), 10_000);

				new Style(1031, 1030, new Vector3(0f, 0f, 30f), 10_000);
				new Style(1032, 1030, new Vector3(0f, 0f, 60f), 10_000);
				new Style(1033, 1030, new Vector3(0f, 0f, 90f), 10_000);
				new Style(1034, 1030, new Vector3(0f, 0f, 120f), 10_000);

				new Style(1041, 1040, new Vector3(0f, 0f, 30f), 10_000);
				new Style(1042, 1040, new Vector3(0f, 0f, 60f), 10_000);
				new Style(1043, 1040, new Vector3(0f, 0f, 90f), 10_000);
				new Style(1044, 1040, new Vector3(0f, 0f, 120f), 10_000);

				new Style(1051, 1050, new Vector3(0f, 0f, 30f), 10_000);
				new Style(1052, 1050, new Vector3(0f, 0f, 60f), 10_000);
				new Style(1053, 1050, new Vector3(0f, 0f, 90f), 10_000);
				new Style(1054, 1050, new Vector3(0f, 0f, 120f), 10_000);

				new Style(1061, 1060, new Vector3(0f, 0f, 30f), 10_000);
				new Style(1062, 1060, new Vector3(0f, 0f, 60f), 10_000);
				new Style(1063, 1060, new Vector3(0f, 0f, 90f), 10_000);
				new Style(1064, 1060, new Vector3(0f, 0f, 120f), 10_000);

				new Style(1071, 1070, new Vector3(0f, 0f, 30f), 10_000);
				new Style(1072, 1070, new Vector3(0f, 0f, 60f), 10_000);
				new Style(1073, 1070, new Vector3(0f, 0f, 90f), 10_000);
				new Style(1074, 1070, new Vector3(0f, 0f, 120f), 10_000);

				new Style(1081, 1080, new Vector3(0f, 0f, 30f), 10_000);
				new Style(1082, 1080, new Vector3(0f, 0f, 60f), 10_000);
				new Style(1083, 1080, new Vector3(0f, 0f, 90f), 10_000);
				new Style(1084, 1080, new Vector3(0f, 0f, 120f), 10_000);

				new Style(1091, 1090, new Vector3(0f, 0f, 30f), 10_000);
				new Style(1092, 1090, new Vector3(0f, 0f, 60f), 10_000);
				new Style(1093, 1090, new Vector3(0f, 0f, 90f), 10_000);
				new Style(1094, 1090, new Vector3(0f, 0f, 120f), 10_000);

				Game.Items.Container.AllSIDs.Add("h_locker", new Items.Container.Data(50, 150f, Items.Container.AllowedItemTypes.All, Items.Container.ContainerTypes.Locker));
				Game.Items.Container.AllSIDs.Add("h_wardrobe", new Items.Container.Data(50, 80f, Items.Container.AllowedItemTypes.Wardrobe, Items.Container.ContainerTypes.Wardrobe));
				Game.Items.Container.AllSIDs.Add("h_fridge", new Items.Container.Data(50, 100f, Items.Container.AllowedItemTypes.Fridge, Items.Container.ContainerTypes.Fridge));

				Game.Items.Container.AllSIDs.Add("a_locker", new Items.Container.Data(50, 150f, Items.Container.AllowedItemTypes.All, Items.Container.ContainerTypes.Locker));
				Game.Items.Container.AllSIDs.Add("a_wardrobe", new Items.Container.Data(50, 80f, Items.Container.AllowedItemTypes.Wardrobe, Items.Container.ContainerTypes.Wardrobe));
				Game.Items.Container.AllSIDs.Add("a_fridge", new Items.Container.Data(50, 100f, Items.Container.AllowedItemTypes.Fridge, Items.Container.ContainerTypes.Fridge));

				var lines = new List<string>();

				foreach (var x in Style.All)
				{
					lines.Add($"new Style({x.Key}, {x.Value.Position.ToCSharpStr()}, {x.Value.InteriorPosition.ToCSharpStr()}, {x.Value.Price}, \"{x.Value.Doors.SerializeToJson().Replace('\"', '\'')}\", \"{x.Value.Lights.SerializeToJson().Replace('\"', '\'')}\", \"{x.Value.SupportedRoomTypes.SerializeToJson().Replace('\"', '\'')}\", \"{x.Value.SupportedHouseTypes.SerializeToJson().Replace('\"', '\'')}\", \"{x.Value.FamiliarTypes.SerializeToJson().Replace('\"', '\'')}\");");
				}

				Utils.FillFileToReplaceRegion(Directory.GetCurrentDirectory() + Properties.Settings.Static.ClientScriptsTargetPath + @"\Sync\House.cs", "STYLES_TO_REPLACE", lines);
			}
		}

		public class Light
		{
			[JsonProperty(PropertyName = "S")]
			public bool State { get; set; }

			[JsonProperty(PropertyName = "C")]
			public Utils.Colour Colour { get; set; }

			public Light(bool State, Utils.Colour Colour)
			{
				this.State = State;
				this.Colour = Colour;
			}

			public Light() { }
		}

		/// <summary>Типы домов</summary>
		public enum Types
		{
			/// <summary>Дом</summary>
			House = 0,
			/// <summary>Квартира</summary>
			Apartments,
		}

		public enum ClassTypes
		{
			A = 0,
			B,
			C,
			D,

			FA,
			FB,
			FC,
			FD,
		}

		private static Dictionary<ClassTypes, uint> Taxes = new Dictionary<ClassTypes, uint>()
		{
			{ ClassTypes.A, 50 },
			{ ClassTypes.B, 75 },
			{ ClassTypes.C, 90 },
			{ ClassTypes.D, 100 },

			{ ClassTypes.FA, 50 },
			{ ClassTypes.FB, 75 },
			{ ClassTypes.FC, 90 },
			{ ClassTypes.FD, 100 },
		};

		public static uint GetTax(ClassTypes cType) => Taxes.GetValueOrDefault(cType, uint.MinValue);

		public static ClassTypes GetClass(HouseBase house)
		{
			if (house.Type == Types.House)
			{
				if (house.Price <= 100_000)
					return ClassTypes.A;

				if (house.Price <= 500_000)
					return ClassTypes.B;

				if (house.Price <= 1_000_000)
					return ClassTypes.C;

				return ClassTypes.D;
			}
			else
			{
				if (house.Price <= 100_000)
					return ClassTypes.FA;

				if (house.Price <= 500_000)
					return ClassTypes.FB;

				if (house.Price <= 1_000_000)
					return ClassTypes.FC;

				return ClassTypes.FD;
			}
		}

		/// <summary>ID дома</summary>
		public uint Id { get; set; }

		/// <summary>Тип дома</summary>
		public Types Type { get; set; }

		public Style.RoomTypes RoomType { get; set; }

		public ushort StyleType { get; set; }

		public Style StyleData => Style.Get(StyleType);

		/// <summary>Владелец</summary>
		public PlayerData.PlayerInfo Owner { get; set; }

		/// <summary>Список сожителей</summary>
		/// <remarks>0 - свет, 1 - двери, 2 - шкаф, 3 - гардероб, 4 - холодильник</remarks>
		public Dictionary<PlayerData.PlayerInfo, bool[]> Settlers { get; set; }

		/// <summary>Баланс дома</summary>
		public ulong Balance { get; set; }

		/// <summary>Заблокированы ли двери?</summary>
		public bool IsLocked { get; set; }

		public bool ContainersLocked { get; set; }

		/// <summary>Налог</summary>
		public uint Tax => GetTax(Class);

		public abstract Utils.Vector4 PositionParams { get; }

		public uint Locker { get; set; }

		public uint Wardrobe { get; set; }

		public uint Fridge { get; set; }

		/// <summary>Список FID мебели в доме</summary>
		public List<Furniture> Furniture { get; set; }

		public Light[] LightsStates { get; set; }

		public bool[] DoorsStates { get; set; }

		/// <summary>Стандартная цена дома</summary>
		public uint Price { get; set; }

		public uint Dimension { get; set; }

		public ClassTypes Class { get; set; }

		public HouseBase(uint ID, Types Type, Style.RoomTypes RoomType)
		{
			this.Type = Type;

			this.RoomType = RoomType;

			this.Id = ID;

			this.Class = GetClass(this);
		}

		public void TriggerEventForHouseOwners(string eventName, params object[] args)
		{
			var players = Settlers.Keys.Where(x => x?.PlayerData?.Player.Dimension == Dimension).Select(x => x.PlayerData.Player).ToList();

			if (players.Count > 0)
			{
				if (Owner?.PlayerData?.Player.Dimension == Dimension)
					players.Add(Owner.PlayerData.Player);

				NAPI.ClientEvent.TriggerClientEventToPlayers(players.ToArray(), eventName, args);
			}
			else
			{
				if (Owner?.PlayerData?.Player.Dimension == Dimension)
					Owner.PlayerData.Player.TriggerEvent(eventName, args);
			}
		}

		public virtual void UpdateOwner(PlayerData.PlayerInfo pInfo)
		{
			Owner = pInfo;
		}

		public string ToClientJson()
		{
			var data = new JObject();

			data.Add("I", Id);
			data.Add("T", (int)Type);
			data.Add("S", StyleType);

			data.Add("LI", Locker);
			data.Add("WI", Wardrobe);
			data.Add("FI", Fridge);

			data.Add("DS", DoorsStates.SerializeToJson());
			data.Add("LS", LightsStates.SerializeToJson());
			data.Add("F", Furniture.SerializeToJson());

			return data.SerializeToJson();
		}

		public abstract void SetPlayersInside(bool teleport, params Player[] players);

		public abstract void SetPlayersOutside(bool teleport, params Player[] players);

		public abstract bool IsEntityNearEnter(Entity entity);

		public abstract void ChangeOwner(PlayerData.PlayerInfo pInfo, bool buyGov = false);

		public void SellToGov(bool balancesBack = true, bool govHalfPriceBack = true)
		{
			if (Owner == null)
				return;

			ulong newBalance;

			if (balancesBack)
			{
				var totalMoney = Balance;

				if (totalMoney > 0)
				{
					if (Owner.BankAccount != null)
					{
						if (Owner.BankAccount.TryAddMoneyDebit(totalMoney, out newBalance, true))
						{
							Owner.BankAccount.SetDebitBalance(newBalance, null);
						}
					}
					else
					{
						if (Owner.TryAddCash(totalMoney, out newBalance, true))
						{
							Owner.SetCash(newBalance);
						}
					}
				}
			}

			if (govHalfPriceBack)
			{
				var totalMoney = Price / 2;

				if (Owner.BankAccount != null)
				{
					if (Owner.BankAccount.TryAddMoneyDebit(totalMoney, out newBalance, true))
					{
						Owner.BankAccount.SetDebitBalance(newBalance, null);
					}
				}
				else
				{
					if (Owner.TryAddCash(totalMoney, out newBalance, true))
					{
						Owner.SetCash(newBalance);
					}
				}
			}

			ContainersLocked = true;
			IsLocked = false;

			for (int i = 0; i < DoorsStates.Length; i++)
				DoorsStates[i] = false;

			for (int i = 0; i < LightsStates.Length; i++)
			{
				LightsStates[i].Colour = DefaultLightColour;
				LightsStates[i].State = true;
            }

			MySQL.HouseUpdateLockState(this);
			MySQL.HouseUpdateContainersLockState(this);

			SetBalance(0, null);

			ChangeOwner(null);
		}

		public bool TryAddMoneyBalance(ulong amount, out ulong newBalance, bool notifyOnFault = true, PlayerData tData = null)
		{
			if (!Balance.TryAdd(amount, out newBalance))
			{
				if (notifyOnFault)
				{

				}

				return false;
			}

			return true;
		}

		public bool TryRemoveMoneyBalance(ulong amount, out ulong newBalance, bool notifyOnFault = true, PlayerData tData = null)
		{
			if (!Balance.TrySubtract(amount, out newBalance))
			{
				if (notifyOnFault)
				{
					if (tData != null)
					{
						tData.Player.Notify("Estate::NEMB", Balance);
					}
				}

				return false;
			}

			return true;
		}

		public void SetBalance(ulong value, string reason)
		{
			Balance = value;

			MySQL.HouseUpdateBalance(this);
		}

		public void SettlePlayer(PlayerData.PlayerInfo pInfo, bool state, PlayerData pDataInit = null)
		{
			if (state)
			{
				if (Settlers.ContainsKey(pInfo))
					return;

				TriggerEventForHouseOwners("HouseMenu::SettlerUpd", $"{pInfo.CID}_{pInfo.Name}_{pInfo.Surname}");

				Settlers.Add(pInfo, new bool[5]);

				if (pInfo.PlayerData != null)
				{
					pInfo.PlayerData.SettledHouseBase = this;

					if (pDataInit != null)
						pInfo.PlayerData.Player.TriggerEvent("Player::SettledHB", (int)Type, Id, true, pDataInit.Player.Id);
					else
						pInfo.PlayerData.Player.TriggerEvent("Player::SettledHB", (int)Type, Id, true);
				}
			}
			else
			{
				Settlers.Remove(pInfo);

				if (pDataInit?.Info == pInfo)
				{
					pDataInit.Player.CloseAll(true);
				}

				TriggerEventForHouseOwners("HouseMenu::SettlerUpd", pInfo.CID);

				if (pInfo.PlayerData != null)
				{
					pInfo.PlayerData.SettledHouseBase = null;

					if (pDataInit != null)
						pInfo.PlayerData.Player.TriggerEvent("Player::SettledHB", (int)Type, Id, false, pDataInit.Player.Id);
					else
						pInfo.PlayerData.Player.TriggerEvent("Player::SettledHB", (int)Type, Id, false);
				}
			}

			MySQL.HouseUpdateSettlers(this);
		}

		public bool BuyFromGov(PlayerData pData)
		{
			ulong newCash;

			if (!pData.TryRemoveCash(Price, out newCash, true))
				return false;

			if (pData.SettledHouseBase?.Type == Type)
			{
				pData.Player.Notify(Type == Types.House ? "Trade::ASH" : "Trade::ASA");

				return false;
			}

			if (Type == Types.House)
			{
				if (pData.FreeHouseSlots <= 0)
				{
					pData.Player.Notify("Trade::MHOW", pData.OwnedHouses.Count);

					return false;
				}
			}
			else
			{
				if (pData.FreeApartmentsSlots <= 0)
				{
					pData.Player.Notify("Trade::MAOW", pData.OwnedApartments.Count);

					return false;
				}
			}

			pData.SetCash(newCash);

			ChangeOwner(pData.Info, true);

			return true;
		}

		public void SetStyle(ushort styleId, Style style, bool furnitureRefund)
		{
			var ownerInfo = Owner;

			var oldStyleId = StyleType;
			var oldStyle = StyleData;

			StyleType = styleId;

			if (style.IsTypeFamiliar(oldStyleId))
			{
				var parentStyle = Style.Get(style.ParentType);

				var offsetOld = oldStyle.InteriorPosition.Position - parentStyle.InteriorPosition.Position;
				var offsetNew = style.InteriorPosition.Position - parentStyle.InteriorPosition.Position;

				foreach (var x in Furniture)
				{
					x.Data.Position = x.Data.Position - offsetOld + offsetNew;

					MySQL.FurnitureUpdate(x);
				}
			}
			else
			{
				if (furnitureRefund && ownerInfo != null)
				{
                    foreach (var x in Furniture)
                    {
                        x.Delete(this);
                    }

                    ownerInfo.AddFurniture(Furniture.ToArray());

                    Furniture.Clear();
                }
				else
				{
                    foreach (var x in Furniture)
                    {
                        x.Delete(this);

						Game.Estates.Furniture.Remove(x);
                    }

                    Furniture.Clear();
                }

				LightsStates = new Light[style.LightsAmount];

				for (int i = 0; i < LightsStates.Length; i++)
				{
					LightsStates[i] = new Light(true, DefaultLightColour);
				}

				DoorsStates = new bool[style.DoorsAmount];

                MySQL.HouseFurnitureUpdate(this);
				MySQL.HouseUpdateDoorsStates(this);
				MySQL.HouseUpdateLightsStates(this);
            }

            MySQL.HouseUpdateStyleType(this);

			var hDim = Dimension;

			var playersInside = PlayerData.All.Keys.Where(x => x.Dimension == hDim).ToArray();

			SetPlayersInside(true, playersInside);
			SetPlayersInside(false, playersInside);
        }
	}
}
