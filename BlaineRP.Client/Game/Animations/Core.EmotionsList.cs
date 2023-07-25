using System.Collections.Generic;
using BlaineRP.Client.Game.Animations.Enums;

namespace BlaineRP.Client.Game.Animations
{
    public partial class Core
    {
        public static Dictionary<EmotionTypes, string> EmotionsList { get; private set; } =
            new Dictionary<EmotionTypes, string>()
            {
                { EmotionTypes.Angry, "mood_angry_1" },
                //{ EmotionTypes.Drunk, "mood_drunk_1" },
                //{ EmotionTypes.Dumb, "pose_injured_1" },
                { EmotionTypes.Electrocuted, "electrocuted_1" },
                { EmotionTypes.Grumpy, "effort_1" },
                { EmotionTypes.Grumpy2, "mood_drivefast_1" },
                { EmotionTypes.Grumpy3, "pose_angry_1" },
                { EmotionTypes.Happy, "mood_happy_1" },
                //{ EmotionTypes.Injured, "mood_injured_1" },
                { EmotionTypes.Joyful, "mood_dancing_low_1" },
                { EmotionTypes.Mouthbreather, "smoking_hold_1" },
                { EmotionTypes.NeverBlink, "pose_normal_1" },
                { EmotionTypes.OneEye, "pose_aiming_1" },
                { EmotionTypes.Shocked, "shocked_1" },
                { EmotionTypes.Shocked2, "shocked_2" },
                { EmotionTypes.Sleeping, "mood_sleeping_1" },
                { EmotionTypes.Sleeping2, "dead_1" },
                { EmotionTypes.Sleeping3, "dead_2" },
                { EmotionTypes.Smug, "mood_smug_1" },
                { EmotionTypes.Speculative, "mood_aiming_1" },
                { EmotionTypes.Stressed, "mood_stressed_1" },
                { EmotionTypes.Sulking, "mood_sulk_1" },
                { EmotionTypes.Weird, "effort_2" },
                { EmotionTypes.Weird2, "effort_3" },
            };
    }
}