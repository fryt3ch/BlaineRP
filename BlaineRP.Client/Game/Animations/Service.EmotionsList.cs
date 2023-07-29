using System.Collections.Generic;

namespace BlaineRP.Client.Game.Animations
{
    public partial class Service
    {
        public static Dictionary<EmotionType, string> EmotionsList { get; private set; } = new Dictionary<EmotionType, string>()
        {
            { EmotionType.Angry, "mood_angry_1" },
            //{ EmotionTypes.Drunk, "mood_drunk_1" },
            //{ EmotionTypes.Dumb, "pose_injured_1" },
            { EmotionType.Electrocuted, "electrocuted_1" },
            { EmotionType.Grumpy, "effort_1" },
            { EmotionType.Grumpy2, "mood_drivefast_1" },
            { EmotionType.Grumpy3, "pose_angry_1" },
            { EmotionType.Happy, "mood_happy_1" },
            //{ EmotionTypes.Injured, "mood_injured_1" },
            { EmotionType.Joyful, "mood_dancing_low_1" },
            { EmotionType.Mouthbreather, "smoking_hold_1" },
            { EmotionType.NeverBlink, "pose_normal_1" },
            { EmotionType.OneEye, "pose_aiming_1" },
            { EmotionType.Shocked, "shocked_1" },
            { EmotionType.Shocked2, "shocked_2" },
            { EmotionType.Sleeping, "mood_sleeping_1" },
            { EmotionType.Sleeping2, "dead_1" },
            { EmotionType.Sleeping3, "dead_2" },
            { EmotionType.Smug, "mood_smug_1" },
            { EmotionType.Speculative, "mood_aiming_1" },
            { EmotionType.Stressed, "mood_stressed_1" },
            { EmotionType.Sulking, "mood_sulk_1" },
            { EmotionType.Weird, "effort_2" },
            { EmotionType.Weird2, "effort_3" },
        };
    }
}