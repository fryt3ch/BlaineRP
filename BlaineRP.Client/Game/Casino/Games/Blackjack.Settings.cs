using BlaineRP.Client.Utils;

namespace BlaineRP.Client.Game.Casino.Games
{
    public partial class Blackjack
    {
        /// <summary>Сумма значений карт, при достижении которой дилер перестает брать новые по завершению игры</summary>
        private const byte DEALER_STOPS_ON = 17;

        /// <summary>Сумма значений карт, при превышении которой следует проигрыш</summary>
        private const byte LOOSE_AFTER = 21;

        /// <summary>Сумма значений карт, дающей блэкджек</summary>
        /// <remarks>Имеет значение только при раздаче карт</remarks>
        private const byte BLACKJACK_ON = 21;

        private static Vector4[][] CardOffsets = new Vector4[][] // after 7 for players - split offsets
        {
            new Vector4[]
            {
                new Vector4(0.0436f, 0.21205f, 0.948875f, 178.92f),
                new Vector4(-0.0636f, 0.213825f, 0.9496f, -180f),
                new Vector4(-0.0806f, 0.2137f, 0.950225f, -178.92f),
                new Vector4(-0.1006f, 0.21125f, 0.950875f, -177.12f),
                new Vector4(-0.1256f, 0.21505f, 0.951875f, 180f),
                new Vector4(-0.1416f, 0.21305f, 0.953f, 178.56f),
                new Vector4(-0.1656f, 0.21205f, 0.954025f, 180f),
                new Vector4(-0.1836f, 0.21255f, 0.95495f, 178.2f),
                new Vector4(-0.2076f, 0.21105f, 0.956025f, -177.12f),
                new Vector4(-0.2246f, 0.21305f, 0.957f, 180f),
            },
            new Vector4[]
            {
                new Vector4(-0.5765f, 0.2229f, 0.9482f, -67.03f),
                new Vector4(-0.558925f, 0.2197f, 0.949175f, -69.12f),
                new Vector4(-0.5425f, 0.213025f, 0.9499f, -64.44f),
                new Vector4(-0.525925f, 0.21105f, 0.95095f, -67.68f),
                new Vector4(-0.509475f, 0.20535f, 0.9519f, -63.72f),
                new Vector4(-0.491775f, 0.204075f, 0.952825f, -68.4f),
                new Vector4(-0.4752f, 0.197525f, 0.9543f, -64.44f),
                new Vector4(-0.5205f, 0.1122f, 0.9478f, -67.03f),
                new Vector4(-0.503175f, 0.108525f, 0.94865f, -67.6f),
                new Vector4(-0.485125f, 0.10475f, 0.949175f, -69.4f),
                new Vector4(-0.468275f, 0.099175f, 0.94995f, -69.04f),
                new Vector4(-0.45155f, 0.09435f, 0.95085f, -68.68f),
                new Vector4(-0.434475f, 0.089725f, 0.95145f, -66.16f),
                new Vector4(-0.415875f, 0.0846f, 0.9523f, -63.28f),
            },
            new Vector4[]
            {
                new Vector4(-0.2359f, -0.1091f, 0.9483f, -21.43f),
                new Vector4(-0.221025f, -0.100675f, 0.949f, -20.16f),
                new Vector4(-0.20625f, -0.092875f, 0.949725f, -16.92f),
                new Vector4(-0.193225f, -0.07985f, 0.950325f, -23.4f),
                new Vector4(-0.1776f, -0.072f, 0.951025f, -21.24f),
                new Vector4(-0.165f, -0.060025f, 0.951825f, -23.76f),
                new Vector4(-0.14895f, -0.05155f, 0.95255f, -19.44f),
                new Vector4(-0.116f, -0.1501f, 0.947875f, -14.04f),
                new Vector4(-0.102725f, -0.13795f, 0.948525f, -15.48f),
                new Vector4(-0.08975f, -0.12665f, 0.949175f, -16.56f),
                new Vector4(-0.075025f, -0.1159f, 0.949875f, -15.84f),
                new Vector4(-0.0614f, -0.104775f, 0.9507f, -16.92f),
                new Vector4(-0.046275f, -0.095025f, 0.9516f, -14.4f),
                new Vector4(-0.031425f, -0.0846f, 0.952675f, -14.28f),
            },
            new Vector4[]
            {
                new Vector4(0.2325f, -0.1082f, 0.94805f, 22.11f),
                new Vector4(0.23645f, -0.0918f, 0.949f, 22.32f),
                new Vector4(0.2401f, -0.074475f, 0.950225f, 20.8f),
                new Vector4(0.244625f, -0.057675f, 0.951125f, 19.8f),
                new Vector4(0.249675f, -0.041475f, 0.95205f, 19.44f),
                new Vector4(0.257575f, -0.0256f, 0.9532f, 26.28f),
                new Vector4(0.2601f, -0.008175f, 0.954375f, 22.68f),
                new Vector4(0.3431f, -0.0527f, 0.94855f, 22.11f),
                new Vector4(0.348575f, -0.0348f, 0.949425f, 22.0f),
                new Vector4(0.35465f, -0.018825f, 0.9502f, 24.44f),
                new Vector4(0.3581f, -0.001625f, 0.95115f, 21.08f),
                new Vector4(0.36515f, 0.015275f, 0.952075f, 25.96f),
                new Vector4(0.368525f, 0.032475f, 0.95335f, 26.16f),
                new Vector4(0.373275f, 0.0506f, 0.9543f, 28.76f),
            },
            new Vector4[]
            {
                new Vector4(0.5737f, 0.2376f, 0.948025f, 69.12f),
                new Vector4(0.562975f, 0.2523f, 0.94875f, 67.8f),
                new Vector4(0.553875f, 0.266325f, 0.94955f, 66.6f),
                new Vector4(0.5459f, 0.282075f, 0.9501f, 70.44f),
                new Vector4(0.536125f, 0.29645f, 0.95085f, 70.84f),
                new Vector4(0.524975f, 0.30975f, 0.9516f, 67.88f),
                new Vector4(0.515775f, 0.325325f, 0.95235f, 69.56f),
                new Vector4(0.6083f, 0.3523f, 0.94795f, 68.57f),
                new Vector4(0.598475f, 0.366475f, 0.948925f, 67.52f),
                new Vector4(0.589525f, 0.3807f, 0.94975f, 67.76f),
                new Vector4(0.58045f, 0.39435f, 0.950375f, 67.04f),
                new Vector4(0.571975f, 0.4092f, 0.951075f, 68.84f),
                new Vector4(0.5614f, 0.4237f, 0.951775f, 65.96f),
                new Vector4(0.554325f, 0.4402f, 0.952525f, 67.76f),
            },
        };

        public static Vector4[][] BetOffsets = new Vector4[][]
        {
            new Vector4[]
            {
                new Vector4(-0.72855f, 0.17345f, 0.95f, -79.2f),
            },
            new Vector4[]
            {
                new Vector4(-0.30305f, -0.2464f, 0.95f, -18.36f),
            },
            new Vector4[]
            {
                new Vector4(0.278125f, -0.2571f, 0.95f, 12.96f),
            },
            new Vector4[]
            {
                new Vector4(0.712625f, 0.170625f, 0.95f, 72f),
            },
        };
    }
}