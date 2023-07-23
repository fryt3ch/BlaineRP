using RAGE.Elements;

namespace BlaineRP.Client.Extensions.RAGE.Elements
{
    public static class PedBaseExtensions
    {
        public static void SetVoiceGroup(this PedBase ped, uint voiceGroupHash) => global::RAGE.Game.Invoker.Invoke(0x7CDC8C3B89F661B3, ped.Handle, voiceGroupHash);

        public static void PlaySpeech(this PedBase ped, string speechName, string speechParam = "SPEECH_PARAMS_FORCE_NORMAL_CLEAR", int p3 = 1, bool stopCurrentSpeech = true)
        {
            if (stopCurrentSpeech)
                ped.StopCurrentPlayingAmbientSpeech();

            ped.PlayAmbientSpeech2(speechName, speechParam, p3);
        }

        public static int GetScriptTaskStatus(this PedBase ped, uint taskHash) => global::RAGE.Game.Invoker.Invoke<int>(global::RAGE.Game.Natives.GetScriptTaskStatus, ped.Handle, (int)taskHash);

        public static void SetFlashLightEnabled(this PedBase ped, bool state) => global::RAGE.Game.Invoker.Invoke(0x988DB6FE9B3AC000, ped.Handle, state);

        public static void TaskLookAtCoord2(this PedBase ped, float posX, float posY, float posZ, int duration, int flags = 2048, int p2 = 3) => global::RAGE.Game.Invoker.Invoke(0x6FA46612594F7973, ped.Handle, posX, posY, posZ, duration, flags, p2);
    }
}