using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BCRPServer.Sync
{
    public class Players
    {
        public static void DisableMicrophone(PlayerData pData)
        {
            if (pData.VoiceRange == 0f)
                return;

            pData.VoiceRange = 0f;

            var player = pData.Player;

            for (int i = 0; i < pData.Listeners.Count; i++)
            {
                var target = pData.Listeners[i];

                if (target == null)
                    continue;

                player.DisableVoiceTo(target);
            }

            pData.Listeners.Clear();
        }

        public static void StopUsePhone(PlayerData pData)
        {
            var player = pData.Player;

            if (!pData.PhoneOn)
                return;

            pData.PhoneOn = false;

            var attachedPhone = pData.AttachedObjects.Where(x => x.Type == AttachSystem.Types.Phone).FirstOrDefault();

            if (attachedPhone != null)
                player.DetachObject(attachedPhone.Id);

            Sync.Chat.SendLocal(Sync.Chat.Types.Me, player, Locale.Chat.Player.PhoneOff);
        }
    }
}
