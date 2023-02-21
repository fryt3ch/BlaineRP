using RAGE;
using RAGE.Elements;
using System;

namespace BCRPClient
{
    class Interaction : Events.Script
    {
        private static bool _Enabled = false;

        public static bool Enabled { get => _Enabled; set { if (!_Enabled && value) { GameEvents.Render -= Render; GameEvents.Render += Render; _Enabled = value; } else if (_Enabled && !value) { GameEvents.Render -= Render; _Enabled = value; CurrentEntity = null; } } }

        public static bool EnabledVisual { get; set; }

        public static RAGE.Elements.Entity CurrentEntity { get; set; }

        public Interaction()
        {

        }

        private static void Render()
        {
            if (RAGE.Elements.Player.LocalPlayer.Vehicle != null)
            {
                CurrentEntity = RAGE.Elements.Player.LocalPlayer.Vehicle;

                return;
            }

            var entity = Utils.GetEntityPlayerLookAt(Settings.ENTITY_INTERACTION_MAX_DISTANCE_RENDER);

            if (entity == null)
            {
                CurrentEntity = null;

                return;
            }

            float x = 0f, y = 0f;

            if (entity.Type == RAGE.Elements.Type.Player || entity.Type == RAGE.Elements.Type.Vehicle || entity.Type == RAGE.Elements.Type.Ped)
            {
                if ((entity.Type == RAGE.Elements.Type.Vehicle && entity.IsLocal) || !entity.GetScreenPosition(ref x, ref y))
                {
                    CurrentEntity = null;

                    return;
                }

                CurrentEntity = entity;
            }
            else if (entity is MapObject mObj)
            {
                if (!entity.HasData("Interactive"))
                {
                    CurrentEntity = null;

                    return;
                }

                if (!entity.GetScreenPosition(ref x, ref y))
                {
                    CurrentEntity = null;

                    return;
                }

                if (entity.IsLocal)
                {
                    CurrentEntity = entity;

                    if (entity.HasData("Furniture"))
                    {
                        var furnData = entity.GetData<Data.Furniture>("Furniture");

                        if (furnData != null)
                        {
                            if (EnabledVisual)
                                Utils.DrawText(furnData.Name, x, y - NameTags.Interval, 255, 255, 255, 255, 0.4f, Utils.ScreenTextFontTypes.CharletComprimeColonge, true);
                        }
                    }
                    else if (entity.HasData("CustomText"))
                    {
                        if (EnabledVisual)
                        {
                            var ctAction = entity.GetData<Action<float, float>>("CustomText");

                            if (ctAction != null)
                            {
                                ctAction.Invoke(x, y);
                            }
                        }
                    }
                }
                else
                {
                    CurrentEntity = entity;

                    if (mObj.GetSharedData<int>("IOG", -1) == 1)
                    {
                        var iogData = Sync.World.ItemOnGround.GetItemOnGroundObject(mObj);

                        if (iogData != null)
                        {
                            if (EnabledVisual)
                                Utils.DrawText(iogData.Name, x, y - NameTags.Interval, 255, 255, 255, 255, 0.4f, Utils.ScreenTextFontTypes.CharletComprimeColonge, true);
                        }
                    }
                }
            }

            if (!EnabledVisual)
                return;

            Utils.DrawText(KeyBinds.Binds[KeyBinds.Types.Interaction].GetKeyString(), x, y, 255, 255, 255, 255, 0.4f, Utils.ScreenTextFontTypes.CharletComprimeColonge, true);
        }
    }
}
