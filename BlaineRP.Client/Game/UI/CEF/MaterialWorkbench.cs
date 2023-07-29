using System.Collections.Generic;
using System.Linq;
using BlaineRP.Client.Extensions.RAGE.Ui;
using BlaineRP.Client.Extensions.System;
using BlaineRP.Client.Game.EntitiesData;
using BlaineRP.Client.Game.EntitiesData.Players;
using BlaineRP.Client.Game.Items;
using RAGE;

namespace BlaineRP.Client.Game.UI.CEF
{
    [Script(int.MaxValue)]
    public class MaterialWorkbench
    {
        public enum Types : byte
        {
            None = 0,

            Fraction,
        }

        public MaterialWorkbench()
        {
            Events.Add("Shop::Create",
                async (args) =>
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

                        Shop.LastSent = World.Core.ServerTime;

                        object res = await Events.CallRemoteProc("Fraction::CWBC", TempData[0], TempData[1], itemId, amount);
                    }
                }
            );
        }

        public static bool IsActive => CurrentType != Types.None && Browser.IsActive(Browser.IntTypes.Retail);

        public static Types CurrentType { get; private set; } = Types.None;

        private static int EscBindIdx { get; set; } = -1;

        private static object[] TempData { get; set; }

        public static async void Show(Types type, Dictionary<string, uint> prices, decimal materialsAmount, params object[] tempData)
        {
            if (IsActive)
                return;

            await Browser.Render(Browser.IntTypes.Retail, true, true);

            TempData = tempData;

            CurrentType = type;

            Browser.Window.ExecuteJs("Retail.draw",
                "creation",
                new object[]
                {
                    prices.Select(x =>
                        {
                            Item.ItemData itemData = Core.GetData(x.Key);
                            return new object[]
                            {
                                x.Key,
                                itemData.Name,
                                x.Value,
                                (itemData as Item.ItemData.IStackable)?.MaxAmount ?? 1,
                                itemData.Weight,
                                null,
                            };
                        }
                    ),
                },
                materialsAmount,
                false
            );

            Cursor.Show(true, true);

            EscBindIdx = Input.Core.Bind(RAGE.Ui.VirtualKeys.Escape, true, () => Close());
        }

        public static void Close()
        {
            if (!IsActive)
                return;

            TempData = null;

            if (EscBindIdx >= 0)
                Input.Core.Unbind(EscBindIdx);

            EscBindIdx = -1;

            Browser.Render(Browser.IntTypes.Retail, false, false);

            CurrentType = Types.None;

            Cursor.Show(false, false);
        }

        public static void UpdateMateriaslBalance(decimal value)
        {
            if (!IsActive)
                return;

            Browser.Window.ExecuteJs("Retail.updateMaterials", value);
        }
    }
}