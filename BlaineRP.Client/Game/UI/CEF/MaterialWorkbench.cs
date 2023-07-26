using System.Collections.Generic;
using System.Linq;
using BlaineRP.Client.Extensions.RAGE.Ui;
using BlaineRP.Client.Extensions.System;
using BlaineRP.Client.Game.EntitiesData;
using BlaineRP.Client.Game.Items;
using RAGE;
using Core = BlaineRP.Client.Game.Input.Core;

namespace BlaineRP.Client.Game.UI.CEF
{
    [Script(int.MaxValue)]
    public class MaterialWorkbench
    {
        public static bool IsActive => CurrentType != Types.None && CEF.Browser.IsActive(Browser.IntTypes.Retail);

        public enum Types : byte
        {
            None = 0,

            Fraction,
        }

        public static Types CurrentType { get; private set; } = Types.None;

        private static int EscBindIdx { get; set; } = -1;

        private static object[] TempData { get; set; }

        public MaterialWorkbench()
        {
            Events.Add("Shop::Create", async (args) =>
            {
                var itemId = (string)args[0];

                var amountD = Utils.Convert.ToDecimal(args[1]);

                int amount;

                if (!amountD.IsNumberValid(1, int.MaxValue, out amount, true))
                    return;

                var pData = PlayerData.GetData(RAGE.Elements.Player.LocalPlayer);

                if (pData == null)
                    return;

                if (Shop.LastSent.IsSpam(500, false, true))
                    return;

                if (CurrentType == Types.Fraction)
                {
                    if (TempData == null)
                        return;

                    Shop.LastSent = Game.World.Core.ServerTime;

                    var res = await Events.CallRemoteProc("Fraction::CWBC", TempData[0], TempData[1], itemId, amount);
                }
            });
        }

        public static async void Show(Types type, Dictionary<string, uint> prices, decimal materialsAmount, params object[] tempData)
        {
            if (IsActive)
                return;

            await CEF.Browser.Render(Browser.IntTypes.Retail, true, true);

            TempData = tempData;

            CurrentType = type;

            CEF.Browser.Window.ExecuteJs("Retail.draw", "creation", new object[] { prices.Select(x => { var itemData = Game.Items.Core.GetData(x.Key); return new object[] { x.Key, itemData.Name, x.Value, (itemData as Item.ItemData.IStackable)?.MaxAmount ?? 1, itemData.Weight, null }; }) }, materialsAmount, false);

            CEF.Cursor.Show(true, true);

            EscBindIdx = Core.Bind(RAGE.Ui.VirtualKeys.Escape, true, () => Close());
        }

        public static void Close()
        {
            if (!IsActive)
                return;

            TempData = null;

            if (EscBindIdx >= 0)
                Core.Unbind(EscBindIdx);

            EscBindIdx = -1;

            CEF.Browser.Render(Browser.IntTypes.Retail, false, false);

            CurrentType = Types.None;

            CEF.Cursor.Show(false, false);
        }

        public static void UpdateMateriaslBalance(decimal value)
        {
            if (!IsActive)
                return;

            CEF.Browser.Window.ExecuteJs("Retail.updateMaterials", value);
        }
    }
}
