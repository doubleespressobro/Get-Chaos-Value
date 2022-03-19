using Ninja_Price.Enums;
using SharpDX;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using ExileCore.PoEMemory;
using ExileCore.PoEMemory.Elements;
using ExileCore.PoEMemory.Elements.InventoryElements;
using ExileCore.Shared.Enums;
using Color = SharpDX.Color;
using RectangleF = SharpDX.RectangleF;
using ImGuiNET;
using static Ninja_Price.Enums.HaggleTypes.HaggleType;

namespace Ninja_Price.Main
{
    public partial class Main
    {
        // TODO: Clean this shit up later
        public Stopwatch ValueUpdateTimer = Stopwatch.StartNew();
        public double StashTabValue { get; set; }
        public double InventoryTabValue { get; set; }
        public double? ExaltedValue { get; set; }
        public List<NormalInventoryItem> HaggleItemList { get; set; } = new List<NormalInventoryItem>();
        public List<CustomItem> FormattedHaggleItemList { get; set; } = new List<CustomItem>();
        public List<NormalInventoryItem> ItemList { get; set; } = new List<NormalInventoryItem>();
        public List<CustomItem> FormattedItemList { get; set; } = new List<CustomItem>();

        public List<NormalInventoryItem> InventoryItemList { get; set; } = new List<NormalInventoryItem>();
        public List<CustomItem> FormattedInventoryItemList { get; set; } = new List<CustomItem>();

        public List<CustomItem> ItemsToDrawList { get; set; } = new List<CustomItem>();
        public List<CustomItem> InventoryItemsToDrawList { get; set; } = new List<CustomItem>();
        public StashElement StashPanel { get; set; }
        public InventoryElement InventoryPanel { get; set; }
        public Element HagglePanel { get; set; }

        public CustomItem HoveredItem { get; set; }

        // TODO: Get hovered items && items from inventory - Getting hovered item  will become useful later on

        public override void Render()
        {
            #region Reset All Data

            StashTabValue = 0;
            InventoryTabValue = 0;
            HoveredItem = null;

            StashPanel = GameController.Game.IngameState.IngameUi.StashElement;
            InventoryPanel = GameController.Game.IngameState.IngameUi.InventoryPanel;
            HagglePanel = GameController.Game.IngameState.IngameUi.HaggleWindow;

            #endregion

            if (CollectedData == null)
            {
                //nothing loaded yet, don't waste time
                return;
            }

            try // Im lazy and just want to surpress all errors produced
            {
                // only update if the time between last update is more than AutoReloadTimer interval
                if (Settings.AutoReload && Settings.LastUpDateTime.AddMinutes(Settings.ReloadTimer.Value) < DateTime.Now)
                {
                    StartDataReload(Settings.LeagueList.Value, true);
                    Settings.LastUpDateTime = DateTime.Now;
                }

                if (Settings.Debug) LogMessage($"{GetCurrentMethod()}.Loop() is Alive", 5, Color.LawnGreen);

                if (Settings.Debug)
                    LogMessage($"{GetCurrentMethod()}: Selected League: {Settings.LeagueList.Value}", 5, Color.White);

                var tabType = StashPanel.VisibleStash?.InvType;

                // Everything is updated, lets check if we should draw
                if (StashPanel.IsVisible)
                {
                    if (ShouldUpdateValues())
                    {
                        // Format stash items
                        ItemList = new List<NormalInventoryItem>();
                        switch (tabType)
                        {
                            case InventoryType.BlightStash:
                                ItemList = StashPanel.VisibleStash.VisibleInventoryItems.ToList();
                                ItemList.RemoveAt(0);
                                ItemList.RemoveAt(ItemList.Count()-1);
                                break;
                            case InventoryType.MetamorphStash:
                                ItemList = StashPanel.VisibleStash.VisibleInventoryItems.ToList();
                                ItemList.RemoveAt(0);
                                break;
                            default:
                                ItemList = StashPanel.VisibleStash.VisibleInventoryItems.ToList();
                                break;
                        }
                        if (ItemList.Count == 0)
                        {
                            ItemList = (List<NormalInventoryItem>)GameController.Game.IngameState.IngameUi.RitualWindow.Items;
                        }
                        FormattedItemList = new List<CustomItem>();
                        FormattedItemList = FormatItems(ItemList);

                        // Format Inventory Items
                        InventoryItemList = new List<NormalInventoryItem>();
                        InventoryItemList = GetInventoryItems();
                        FormattedInventoryItemList = new List<CustomItem>();
                        FormattedInventoryItemList = FormatItems(InventoryItemList);

                        if (Settings.Debug)
                            LogMessage($"{GetCurrentMethod()}.Render() Looping if (ShouldUpdateValues())", 5,
                                Color.LawnGreen);

                        foreach (var item in FormattedItemList)
                            GetValue(item);
                        foreach (var item in FormattedInventoryItemList)
                            GetValue(item);
                    }

                    // Gather all information needed before rendering as we only want to itterate through the list once

                    ItemsToDrawList = new List<CustomItem>();
                    foreach (var item in FormattedItemList)
                    {
                        if (item == null || item.Item.Address == 0) continue; // Item is fucked, skip
                        if (!item.Item.IsVisible && item.ItemType != ItemTypes.None)
                            continue; // Disregard non visable items as that usually means they arnt part of what we want to look at

                        StashTabValue += item.PriceData.MinChaosValue;
                        ItemsToDrawList.Add(item);
                    }

                    InventoryItemsToDrawList = new List<CustomItem>();
                    foreach (var item in FormattedInventoryItemList)
                    {

                        if (item == null || item.Item.Address == 0) continue; // Item is fucked, skip
                        if (!item.Item.IsVisible && item.ItemType != ItemTypes.None)
                            continue; // Disregard non visable items as that usually means they arnt part of what we want to look at

                        InventoryTabValue += item.PriceData.MinChaosValue;
                        InventoryItemsToDrawList.Add(item);
                    }
                }
                else if (InventoryPanel.IsVisible)
                {
                    if (ShouldUpdateValuesInventory())
                    {
                        // Format Inventory Items
                        InventoryItemList = new List<NormalInventoryItem>();
                        InventoryItemList = GetInventoryItems();
                        FormattedInventoryItemList = new List<CustomItem>();
                        FormattedInventoryItemList = FormatItems(InventoryItemList);

                        if (Settings.Debug)
                            LogMessage($"{GetCurrentMethod()}.Render() Looping if (ShouldUpdateValues())", 5,
                                Color.LawnGreen);

                        foreach (var item in FormattedInventoryItemList)
                            GetValue(item);
                    }

                    // Gather all information needed before rendering as we only want to itterate through the list once
                    InventoryItemsToDrawList = new List<CustomItem>();
                    foreach (var item in FormattedInventoryItemList)
                    {

                        if (item == null || item.Item.Address == 0) continue; // Item is fucked, skip
                        if (!item.Item.IsVisible && item.ItemType != ItemTypes.None)
                            continue; // Disregard non visable items as that usually means they arnt part of what we want to look at

                        InventoryTabValue += item.PriceData.MinChaosValue;
                        InventoryItemsToDrawList.Add(item);
                    }
                }

                if (HagglePanel.IsVisible)
                {

                    var haggleType = None;

                    // Return Haggle Window Type
                    var haggleText = HagglePanel.GetChildAtIndex(6).GetChildAtIndex(2).GetChildAtIndex(0).Text;

                    switch (haggleText)
                    {
                        case "Exchange":
                            haggleType = Exchange;
                            break;
                        case "Gamble":
                            haggleType = Gamble;
                            break;
                        case "Deal":
                            haggleType = Deal;
                            break;
                        case "Haggle":
                            haggleType = Haggle;
                            break;
                    }

                    if (haggleType == Gamble)
                    {
                        HaggleItemList = new List<NormalInventoryItem>();
                        var itemChild = HagglePanel.GetChildAtIndex(8).GetChildAtIndex(1).GetChildAtIndex(0).GetChildAtIndex(0);

                        for (var i = 1; i < itemChild.ChildCount; ++i)
                        {
                            var item = itemChild.Children[i].AsObject<NormalInventoryItem>();
                            HaggleItemList.Add(item);

                            if (Settings.Debug)
                            {
                                LogMessage(
                                    $"Haggle Item[{HaggleItemList.Count}]: {GameController.Files.BaseItemTypes.Translate(item.Item.Path).BaseName}");
                            }
                        }

                        // Format Haggle Items
                        FormattedHaggleItemList = new List<CustomItem>();
                        FormattedHaggleItemList = FormatItems(HaggleItemList);

                        foreach (var item in FormattedHaggleItemList)
                            GetValueHaggle(item);
                    }
                    else
                    {
                        HaggleItemList = new List<NormalInventoryItem>();
                        FormattedHaggleItemList = new List<CustomItem>();
                    }
                }

                // Expedition Gamble Func
                ExpeditionGamble();

                GetHoveredItem(); // Get information for the hovered item
                DrawGraphics();
            }
            catch (Exception e)
            {
                // ignored
                if (Settings.Debug)
                {
                    LogMessage("Error in: Main Render Loop, restart PoEHUD.", 5, Color.Red);
                    LogMessage(e.ToString(), 5, Color.Orange);
                }
            }
        }

        public void DrawGraphics()
        {
            // Hovered Item
            if (HoveredItem != null && HoveredItem.ItemType != ItemTypes.None && Settings.HoveredItem.Value)
            {
                var text = $"Change in last 7 Days: {HoveredItem.PriceData.ChangeInLast7Days}%%";
                var changeTextLength = text.Length-1;
                text += $"\n\r{String.Concat(Enumerable.Repeat('-', changeTextLength))}";

                switch (HoveredItem.ItemType)
                {
                    case ItemTypes.Currency:
                    case ItemTypes.Essence:
                    case ItemTypes.Fragment:
                    case ItemTypes.Scarab:
                    case ItemTypes.Resonator:
                    case ItemTypes.Fossil:
                    case ItemTypes.Oil:
                    case ItemTypes.Artifact:
                    case ItemTypes.Catalyst:
                    case ItemTypes.DeliriumOrbs:
                    case ItemTypes.Vials:
                    case ItemTypes.DivinationCard:
                        if (HoveredItem.PriceData.MinChaosValue / HoveredItem.PriceData.ExaltedPrice >= 0.1)
                        {
                            text += $"\n\rExalt: {HoveredItem.PriceData.MinChaosValue / HoveredItem.PriceData.ExaltedPrice:0.##}ex";
                            text += $"\n\r{String.Concat(Enumerable.Repeat('-', changeTextLength))}";
                        }
                        text += $"\n\rChaos: {HoveredItem.PriceData.MinChaosValue / HoveredItem.CurrencyInfo.StackSize}c";
                        text += $"\n\rTotal: {HoveredItem.PriceData.MinChaosValue}c";
                        break;
                    case ItemTypes.UniqueAccessory:
                    case ItemTypes.UniqueArmour:
                    case ItemTypes.UniqueFlask:
                    case ItemTypes.UniqueJewel:
                    case ItemTypes.UniqueMap:
                    case ItemTypes.UniqueWeapon:
                        if (HoveredItem.PriceData.MinChaosValue / HoveredItem.PriceData.ExaltedPrice >= 0.1)
                        {
                            text += $"\n\rExalt: {HoveredItem.PriceData.MinChaosValue / HoveredItem.PriceData.ExaltedPrice:0.##}ex - {HoveredItem.PriceData.MaxChaosValue / HoveredItem.PriceData.ExaltedPrice:0.##}ex";
                            text += $"\n\r{String.Concat(Enumerable.Repeat('-', changeTextLength))}";
                        }
                        text += $"\n\rChaos: {HoveredItem.PriceData.MinChaosValue}c - {HoveredItem.PriceData.MaxChaosValue}c";
                        break;
                    case ItemTypes.Map:
                    case ItemTypes.Incubator:
                    case ItemTypes.MavenInvitation:
                        if (HoveredItem.PriceData.MinChaosValue / HoveredItem.PriceData.ExaltedPrice >= 0.1)
                        {
                            text += $"\n\rExalt: {HoveredItem.PriceData.MinChaosValue / HoveredItem.PriceData.ExaltedPrice:0.##}ex";
                            text += $"\n\r{String.Concat(Enumerable.Repeat('-', changeTextLength))}";
                        }
                        text += $"\n\rChaos: {HoveredItem.PriceData.MinChaosValue}c";
                        break;
                }
                
                if (Settings.Debug)
                {
                    text += $"\n\rUniqueName: {HoveredItem.UniqueName}";
                    text += $"\n\rBaseName: {HoveredItem.BaseName}";
                    text += $"\n\rItemType: {HoveredItem.ItemType}";
                    text += $"\n\rMapType: {HoveredItem.MapInfo.MapType}";
                } 
                
                if (Settings.ArtifactChaosPrices)
                {
                    var artifactChaosPrice = TryGetArtifactToChaosPrice(HoveredItem);
                    if (artifactChaosPrice > 0)
                    {
                        var exaltString = ExaltedValue != null && artifactChaosPrice >= 0.5 * ExaltedValue
                            ? $" ({artifactChaosPrice / ExaltedValue:F2} ex)"
                            : string.Empty;
                        text += $"\n\rArtifact price: {artifactChaosPrice:F1}c{exaltString}";
                    }
                }

                // var textMeasure = Graphics.MeasureText(text, 15);
                //Graphics.DrawBox(new RectangleF(0, 0, textMeasure.Width, textMeasure.Height), Color.Black);
                //Graphics.DrawText(text, new Vector2(50, 50), Color.White);

                ImGui.BeginTooltip();
                ImGui.SetTooltip(text);
                ImGui.EndTooltip();
            }

            // Inventory Value
            VisibleInventoryValue();

            if (Settings.HelmetEnchantPrices)
                ShowHelmetEnchantPrices();

            if (!StashPanel.IsVisible)
                return;

            // Stash Tab Value
            VisibleStashValue();

            var tabType = StashPanel.VisibleStash.InvType;
            foreach (var customItem in ItemsToDrawList)
            {
                if (customItem.ItemType == ItemTypes.None) continue;

                if (Settings.CurrencyTabSpecificToggle &&
                    (!Settings.DoNotDrawCurrencyTabSpecificWhileItemHovered || HoveredItem == null))
                {
                    switch (tabType)
                    {
                        case InventoryType.CurrencyStash:
                        case InventoryType.FragmentStash:
                        case InventoryType.DelveStash:
                        case InventoryType.DeliriumStash:
                        case InventoryType.MetamorphStash:
                        case InventoryType.BlightStash:
                            PriceBoxOverItem(customItem);
                            break;
                    }
                }

                if (Settings.HighlightUniqueJunk)
                {
                    if (customItem.ItemType == ItemTypes.None || customItem.ItemType == ItemTypes.Currency) continue;
                    HighlightJunkUniques(customItem);
                }
            }

            if (Settings.HighlightUniqueJunk)
                foreach (var customItem in InventoryItemsToDrawList)
                {
                    if (customItem.ItemType == ItemTypes.None || customItem.ItemType == ItemTypes.Currency) continue;

                    HighlightJunkUniques(customItem);
                }
        }

        private void VisibleStashValue()
        {
            try
            {
                //var StashType = GameController.Game.IngameState.IngameUi.StashElement.VisibleStash.InvType;
                if (!Settings.VisibleStashValue.Value || !StashPanel.IsVisible) return;
                {
                    var pos = new Vector2(Settings.StashValueX.Value, Settings.StashValueY.Value);
                    //Graphics.DrawText(
                    //    DrawImage($"{DirectoryFullName}//images//Chaos_Orb_inventory_icon.png",
                    //        new RectangleF(Settings.StashValueX.Value - Settings.StashValueFontSize.Value,
                    //            Settings.StashValueY.Value,
                    //            Settings.StashValueFontSize.Value, Settings.StashValueFontSize.Value))
                    //        ? $"{significantDigits}"
                    //        : $"{significantDigits} Chaos", Settings.StashValueFontSize.Value, pos,
                    //    Settings.UniTextColor);

                    var chaosValue = StashTabValue;
                    DrawWorthWidget(chaosValue, pos);
                }
            }
            catch (Exception e)
            {
                // ignored
                if (Settings.Debug)
                {
                    LogMessage("Error in: VisibleStashValue, restart PoEHUD.", 5, Color.Red);
                    LogMessage(e.ToString(), 5, Color.Orange);
                }
            }
        }

        private void DrawWorthWidget(double chaosValue, Vector2 pos)
        {
            Graphics.DrawText($"Chaos: {Math.Round(chaosValue, Settings.StashValueSignificantDigits.Value):#,##0.################}" +
                              (ExaltedValue != null
                                   ? $"\nExalt: {Math.Round(chaosValue / ExaltedValue.Value, Settings.StashValueSignificantDigits.Value):#,##0.################}"
                                   : ""),
                pos, Settings.UniTextColor, FontAlign.Left);
        }

        private void VisibleInventoryValue()
        {
            try
            {
                var inventory = GameController.Game.IngameState.IngameUi.InventoryPanel;
                if (!Settings.VisibleInventoryValue.Value || !inventory.IsVisible) return;
                {
                    var pos = new Vector2(Settings.InventoryValueX.Value, Settings.InventoryValueY.Value);
                    DrawWorthWidget(InventoryTabValue, pos);
                }
            }
            catch (Exception e)
            {
                // ignored
                if (Settings.Debug)
                {

                    LogMessage("Error in: VisibleInventoryValue, restart PoEHUD.", 5, Color.Red);
                    LogMessage(e.ToString(), 5, Color.Orange);
                }
            }
        }

        private void PriceBoxOverItem(CustomItem item)
        {
            var box = item.Item.GetClientRect();
            var drawBox = new RectangleF(box.X, box.Y - 2, box.Width, -Settings.CurrencyTabBoxHeight);
            var position = new Vector2(drawBox.Center.X, drawBox.Center.Y - Settings.CurrencyTabFontSize.Value / 2);
           
            Graphics.DrawText(Math.Round((decimal) item.PriceData.MinChaosValue, Settings.CurrencyTabSigDigits.Value).ToString(), position, Settings.CurrencyTabFontColor, FontAlign.Center);
            Graphics.DrawBox(drawBox, Settings.CurrencyTabBackgroundColor);
            //Graphics.DrawFrame(drawBox, 1, Settings.CurrencyTabBorderColor);
        }

        private void PriceBoxOverItemHaggle(CustomItem item)
        {
            var box = item.Item.GetClientRect();
            var drawBox = new RectangleF(box.X, box.Y + 2, box.Width, +Settings.CurrencyTabBoxHeight);
            var position = new Vector2(drawBox.Center.X, drawBox.Center.Y - Settings.CurrencyTabFontSize.Value / 2);

            if (item.PriceData.ItemBasePrices.Count == 0)
                return;

            // Sort base unique price from High -> Low
            item.PriceData.ItemBasePrices.Sort((a, b) => b.CompareTo(a));

            if (Settings.Debug)
                Graphics.DrawText(string.Join(",", item.PriceData.ItemBasePrices), position, Settings.CurrencyTabFontColor, FontAlign.Center);


            Graphics.DrawText(Math.Round((decimal)item.PriceData.ItemBasePrices.FirstOrDefault(), Settings.CurrencyTabSigDigits.Value).ToString(CultureInfo.InvariantCulture), position, Settings.CurrencyTabFontColor, FontAlign.Center);
            Graphics.DrawBox(drawBox, Settings.CurrencyTabBackgroundColor);
            //Graphics.DrawFrame(drawBox, 1, Settings.CurrencyTabBorderColor);
        }

        private void ExpeditionGamble()
        {
            var window = HagglePanel;
            if (!window.IsVisible) return;
            foreach (var customItem in FormattedHaggleItemList)
            {
                try
                {
                    PriceBoxOverItemHaggle(customItem);
                }
                catch (Exception e)
                {
                    // ignored
                    if (Settings.Debug)
                    {
                        LogMessage("Error in: ExpeditionGamble, restart PoEHUD.", 5, Color.Red);
                        LogMessage(e.ToString(), 5, Color.Orange);
                    }
                }
            }
        }

        /// <summary>
        ///     Displays price for unique items, and highlights the uniques under X value by drawing a border arround them.
        /// </summary>
        /// <param name="item"></param>
        private void HighlightJunkUniques(CustomItem item)
        {
            var hoverUi = GameController.Game.IngameState.UIHoverTooltip.Tooltip;
            if (hoverUi != null && (item.Rarity != ItemRarity.Unique || hoverUi.GetClientRect().Intersects(item.Item.GetClientRect()) && hoverUi.IsVisibleLocal)) return;

            var chaosValueSignificantDigits = Math.Round((decimal) item.PriceData.MinChaosValue, Settings.HighlightSignificantDigits.Value);
            if (chaosValueSignificantDigits >= Settings.InventoryValueCutOff.Value) return;
            var rec = item.Item.GetClientRect();
            var fontSize = Settings.HighlightFontSize.Value;
            // var backgroundBox = Graphics.MeasureText($"{chaosValueSignificanDigits}", fontSize);
            var position = new Vector2(rec.TopRight.X - fontSize, rec.TopRight.Y);

            //Graphics.DrawBox(new RectangleF(position.X - backgroundBox.Width, position.Y, backgroundBox.Width, backgroundBox.Height), Color.Black);
            Graphics.DrawText($"{chaosValueSignificantDigits}", position, Settings.UniTextColor, FontAlign.Center);
            //Graphics.DrawFrame(item.Item.GetClientRect(), 2, Settings.HighlightColor.Value);
        }


        private void ShowHelmetEnchantPrices()
        {
            ExileCore.PoEMemory.Element GetElementByString(ExileCore.PoEMemory.Element element, string str)
            {
                if (string.IsNullOrWhiteSpace(str))
                    return null;

                if (element.Text != null && element.Text.Contains(str))
                    return element;

                return element.Children.Select(c => GetElementByString(c, str)).FirstOrDefault(e => e != null);
            }
            
            var ingameUi = GameController.Game.IngameState.IngameUi;
            if (!ingameUi.LabyrinthDivineFontPanel.IsVisible) return;
            var triggerEnchantment = GetElementByString(ingameUi.LabyrinthDivineFontPanel, "lvl ");
            var enchantmentContainer = triggerEnchantment?.Parent?.Parent;
            if(enchantmentContainer == null) return;
            var enchants = enchantmentContainer.Children.Select(c => new {Name = c.Children[1].Text, ContainerElement = c});
            foreach (var enchant in enchants)
            {
                var data = GetHelmetEnchantValue(enchant.Name);
                if (data == null) continue;
                var box = enchant.ContainerElement.GetClientRect();
                var drawBox = new RectangleF(box.X + box.Width, box.Y - 2, 65, box.Height);
                var position = new Vector2(drawBox.Center.X, drawBox.Center.Y - 7);

                var textColor = data.PriceData.ExaltedPrice >= 1 ? Color.Black : Color.White;
                var bgColor = data.PriceData.ExaltedPrice >= 1 ? Color.Goldenrod : Color.Black;
                Graphics.DrawText(Math.Round((decimal) data.PriceData.MinChaosValue, 2) + "c", position, textColor, FontAlign.Center);
                Graphics.DrawBox(drawBox, bgColor);
                Graphics.DrawFrame(drawBox, Color.Black, 1);
            }
        }

        private double TryGetArtifactToChaosPrice(CustomItem item)
        {
            if (item == null || item.Item == null)
                return -1;

            Element GetElementByString(Element element, string str)
            {
                if (element == null || string.IsNullOrWhiteSpace(str))
                    return null;

                if (element.Text != null && element.Text.Contains(str))
                    return element;

                return element.Children.Select(c => GetElementByString(c, str)).FirstOrDefault(e => e != null);
            }

            var costElement = GetElementByString(item.Item?.AsObject<HoverItemIcon>()?.Tooltip, "Cost");
            if (costElement == null || costElement.Parent == null || costElement.Parent.ChildCount < 2 ||
                costElement.Parent.Children[1].ChildCount < 3)
                return -1;
            var amountText = costElement.Parent.Children[1].Children[0].Text;
            var artifactName = costElement.Parent.Children[1].Children[2].Text;
            if (string.IsNullOrWhiteSpace(amountText) || string.IsNullOrWhiteSpace(artifactName))
                return -1;
            var artifactSearch = CollectedData.Artifacts.Lines.Find(x => x.Name == artifactName);
            if (artifactSearch == null)
                return -1;
            if (costElement.Text.Equals("Cost:")) // Tujen haggling
            {
                var amount = int.Parse(amountText.Substring(0, amountText.Length - 1));
                return artifactSearch.ChaosValue.Value * amount;
            }
            if (costElement.Text.Equals("Cost Per Unit:")) // Artifact stacks (Dannig)
            {
                var costPerUnit = double.Parse(amountText);
                return item.CurrencyInfo.StackSize * costPerUnit * artifactSearch.ChaosValue.Value;
            }
            return -1;
        }
    }
}
