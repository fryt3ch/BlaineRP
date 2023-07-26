using BlaineRP.Client.Game.Helpers;
using BlaineRP.Client.Game.Helpers.Blips;
using BlaineRP.Client.Game.Helpers.Colshapes.Enums;
using BlaineRP.Client.Game.Helpers.Colshapes.Types;
using BlaineRP.Client.Game.NPCs;
using BlaineRP.Client.Game.UI.CEF.Phone.Apps;
using BlaineRP.Client.Utils;
using RAGE;

namespace BlaineRP.Client.Game.Businesses
{
    public class WeaponShop : Business
    {
        public static uint ShootingRangePrice => Convert.ToUInt32(World.Core.GetSharedData<object>("SRange::Price", 0));

        public WeaponShop(int id, Vector3 positionInfo, uint price, uint rent, float tax, Vector4 positionInteract, Vector3 shootingRangePosition) : base(id, positionInfo, BusinessTypes.WeaponShop, price, rent, tax)
        {
            Blip = new ExtraBlip(110, positionInteract.Position, Name, 1f, 0, 255, 0f, true, 0, 0f, Settings.App.Static.MainDimension);

            Seller = new NPC($"seller_{id}", "", NPC.Types.Talkable, "s_m_y_ammucity_01", positionInteract.Position, positionInteract.RotationZ, Settings.App.Static.MainDimension)
            {
                SubName = "NPC_SUBNAME_SELLER",

                Data = this,

                DefaultDialogueId = "seller_clothes_greeting_0",
            };

            var tPos = new Vector3(shootingRangePosition.X, shootingRangePosition.Y, shootingRangePosition.Z - 1f);

            var shootingRangeEnterCs = new Cylinder(tPos, 1.5f, 2f, false, Utils.Misc.RedColor, Settings.App.Static.MainDimension, null)
            {
                Data = this,

                ActionType = ActionTypes.ShootingRangeEnter,
                InteractionType = InteractionTypes.ShootingRangeEnter,
            };

            var shootingRangeText = new ExtraLabel(new Vector3(tPos.X, tPos.Y, tPos.Z + 0.75f), Locale.Get("SHOP_WEAPON_SRANGE_L"), new RGBA(255, 255, 255, 255), 10f, 0, true, Settings.App.Static.MainDimension);

            GPS.AddPosition("bizother", "weapon", $"bizother_{id}", $"{Name} #{SubId}", new RAGE.Ui.Cursor.Vector2(positionInteract.X, positionInteract.Y));
        }
    }
}