using System;
using System.Collections.Generic;
using System.Linq;
using BlaineRP.Server.EntitiesData.Players;
using BlaineRP.Server.Game.Items;
using Newtonsoft.Json;

namespace BlaineRP.Server.Game.Management.Achievements
{
    public partial class Achievement
    {
        private static Dictionary<AchievementType, Data> TypesData = new Dictionary<AchievementType, Data>()
        {
            { AchievementType.SR1, new Data(80, Gift.Prototype.CreateAchievement(Gift.Types.Money, null, 0, 10_000)) },
            { AchievementType.SR2, new Data(100, Gift.Prototype.CreateAchievement(Gift.Types.Money, null, 0, 10_000)) },
        };

        public static Dictionary<AchievementType, Achievement> GetNewDict()
        {
            return Enum.GetValues(typeof(AchievementType)).Cast<AchievementType>().ToDictionary(x => x, y => new Achievement(y));
        }

        [JsonProperty(PropertyName = "IR")]
        public bool IsRecieved { get; set; }

        [JsonProperty(PropertyName = "P")]
        public uint Progress { get; set; }

        [JsonIgnore]
        public AchievementType Type { get; set; }

        [JsonIgnore]
        public Data TypeData => TypesData[Type];

        public Achievement(AchievementType Type)
        {
            this.Type = Type;
        }

        public Achievement(AchievementType Type, uint Progress, bool IsRecieved) : this(Type)
        {
            this.Progress = Progress;
            this.IsRecieved = IsRecieved;
        }

        public bool UpdateProgress(PlayerInfo pInfo, uint newProgress)
        {
            if (IsRecieved)
                return true;

            if (newProgress <= Progress)
                return false;

            Data data = TypeData;

            if (newProgress > data.Goal)
                newProgress = data.Goal;

            if (newProgress == Progress)
                return false;

            Progress = newProgress;

            if (pInfo.PlayerData != null)
                pInfo.PlayerData.Player.TriggerEvent("Player::Achievements::Update", (int)Type, Progress, data.Goal);

            if (Progress >= data.Goal)
            {
                if (Progress > data.Goal)
                    Progress = data.Goal;

                IsRecieved = true;

                if (data.Reward != null)
                    Gift.Give(pInfo, data.Reward, true);

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