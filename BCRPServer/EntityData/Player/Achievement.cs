using BCRPServer.Game.Items;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BCRPServer
{
    public partial class PlayerData
    {
        public class Achievement
        {
            public enum Types
            {
                SR1,
                SR2,
            }

            private static Dictionary<Types, Data> TypesData = new Dictionary<Types, Data>()
            {
                { Types.SR1, new Data(80, Game.Items.Gift.Prototype.CreateAchievement(Gift.Types.Money, null, 0, 10_000)) },
                { Types.SR2, new Data(100, Game.Items.Gift.Prototype.CreateAchievement(Gift.Types.Money, null, 0, 10_000)) },
            };

            public class Data
            {
                public int Goal { get; set; }

                public bool IsHidden { get; set; }

                public Game.Items.Gift.Prototype Reward { get; set; }

                public Data(int Goal, Game.Items.Gift.Prototype Reward)
                {
                    this.Goal = Goal;
                    this.Reward = Reward;
                }
            }

            public static Dictionary<Types, Achievement> GetNewDict() => Enum.GetValues(typeof(Types)).Cast<Types>().ToDictionary(x => x, y => new Achievement(y));

            [JsonProperty(PropertyName = "IR")]
            public bool IsRecieved { get; set; }

            [JsonProperty(PropertyName = "P")]
            public int Progress { get; set; }

            [JsonIgnore]
            public Types Type { get; set; }

            [JsonIgnore]
            public Data TypeData => TypesData[Type];

            public Achievement(Types Type)
            {
                this.Type = Type;
            }

            public Achievement(Types Type, int Progress, bool IsRecieved) : this(Type)
            {
                this.Progress = Progress;
                this.IsRecieved = IsRecieved;
            }

            public bool UpdateProgress(PlayerInfo pInfo, int newProgress)
            {
                if (IsRecieved)
                    return true;

                if (newProgress <= Progress)
                    return false;

                var data = TypeData;

                if (newProgress > data.Goal)
                    newProgress = data.Goal;

                if (newProgress == Progress)
                    return false;

                Progress = newProgress;

                if (pInfo.PlayerData != null)
                {
                    pInfo.PlayerData.Player.TriggerEvent("Player::Achievements::Update", (int)Type, Progress, data.Goal);
                }

                if (Progress >= data.Goal)
                {
                    if (Progress > data.Goal)
                        Progress = data.Goal;

                    IsRecieved = true;

                    if (data.Reward != null)
                        Game.Items.Gift.Give(pInfo, data.Reward, true);

                    MySQL.CharacterAchievementUpdate(pInfo, this);

                    return true;
                }
                else
                {
                    MySQL.CharacterAchievementUpdate(pInfo, this);

                    return false;
                }
            }
        }
    }
}
