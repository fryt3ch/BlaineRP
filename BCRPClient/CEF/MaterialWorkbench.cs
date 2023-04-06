﻿using RAGE;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

namespace BCRPClient.CEF
{
    public class MaterialWorkbench : Events.Script
    {
        public static bool IsActive => CurrentType != Types.None && CEF.Browser.IsActive(Browser.IntTypes.Retail);

        public enum Types : byte
        {
            None = 0,

            Fraction,
        }

        public static Types CurrentType { get; private set; } = Types.None;

        private static int EscBindIdx { get; set; } = -1;

        public MaterialWorkbench()
        {
            Events.Add("Shop::Create", (args) =>
            {
                var itemId = (string)args[0];

                var amountD = args[1].ToDecimal();

                int amount;

                if (!amountD.IsNumberValid(1, int.MaxValue, out amount, true))
                    return;

                var pData = Sync.Players.GetData(RAGE.Elements.Player.LocalPlayer);

                if (pData == null)
                    return;

                if (Shop.LastSent.IsSpam(500, false, true))
                    return;

                if (CurrentType == Types.Fraction)
                {
                    Shop.LastSent = Sync.World.ServerTime;

                    Events.CallRemote("Fraction::CWBC", itemId, amount);
                }
            });
        }

        public static async void Show(Types type, Dictionary<string, uint> prices, decimal materialsAmount)
        {
            if (IsActive)
                return;

            await CEF.Browser.Render(Browser.IntTypes.Retail, true, true);

            CurrentType = type;

            CEF.Browser.Window.ExecuteJs("Retail.draw", "creation", new object[] { prices.Select(x => { var itemData = Data.Items.GetData(x.Key); return new object[] { x.Key, itemData.Name, x.Value, (itemData as Data.Items.Item.ItemData.IStackable)?.MaxAmount ?? 1, itemData.Weight, false }; }) }, materialsAmount, false);

            CEF.Cursor.Show(true, true);

            EscBindIdx = KeyBinds.Bind(RAGE.Ui.VirtualKeys.Escape, true, () => Close());
        }

        public static void Close()
        {
            if (!IsActive)
                return;

            if (EscBindIdx >= 0)
                KeyBinds.Unbind(EscBindIdx);

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