namespace BlaineRP.Client.Utils.Game
{
    internal static class Audio
    {
        public static void DisableFlightMusic()
        {
            RAGE.Game.Audio.SetAudioFlag("DisableFlightMusic", true);
        }

        public static void LinkStaticEmitterToEntity(string emitterName, int entityHandle)
        {
            RAGE.Game.Invoker.Invoke(0x651D3228960D08AF, emitterName, entityHandle);
        }

        public static async System.Threading.Tasks.Task PrepareAlarm(string alarmName)
        {
            while (!RAGE.Game.Audio.PrepareAlarm(alarmName))
            {
                await RAGE.Game.Invoker.WaitAsync(5);
            }
        }

        public static async System.Threading.Tasks.Task RequestScriptAudioBank(string name, bool p1 = false, int p2 = -1)
        {
            while (!RAGE.Game.Audio.RequestScriptAudioBank(name, p1, p2))
            {
                await RAGE.Game.Invoker.WaitAsync(5);
            }
        }
    }
}