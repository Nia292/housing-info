using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Dalamud.Interface.Utility.Raii;
using Dalamud.Bindings.ImGui;
using SamplePlugin.Collector;
using SamplePlugin.Storage;

namespace SamplePlugin.Windows.Tabs;

public class AllDataView
{
    private readonly PluginDataStorage pluginDataStorage;
    private readonly Plugin plugin;

    public AllDataView(PluginDataStorage pluginDataStorage, Plugin plugin)
    {
        this.pluginDataStorage = pluginDataStorage;
        this.plugin = plugin;
    }

    public void Draw()
    {
        using (var child = ImRaii.Child("SomeChildWithAScrollbar", Vector2.Zero, true))
        {
            // Check if this child is drawing
            if (child.Success)
            {
                if (ImGui.Button("Visit List"))
                {
                    plugin.ToggleVisitList();
                }
                var flags = ImGuiTableFlags.RowBg & ImGuiTableFlags.Borders
                                                  & ImGuiTableFlags.BordersH
                                                  & ImGuiTableFlags.BordersOuterH
                                                  & ImGuiTableFlags.BordersInnerH;
                var houses = pluginDataStorage.EntriesToDisplay;
                if (ImGui.BeginTable("houses-table", 10, flags))
                {
                    ImGui.TableSetupColumn("World", ImGuiTableColumnFlags.WidthFixed, 80);
                    ImGui.TableSetupColumn("Area", ImGuiTableColumnFlags.WidthFixed, 120);
                    ImGui.TableSetupColumn("Ward", ImGuiTableColumnFlags.WidthFixed, 45);
                    ImGui.TableSetupColumn("Plot", ImGuiTableColumnFlags.WidthFixed, 45);
                    ImGui.TableSetupColumn("Tags", ImGuiTableColumnFlags.WidthFixed, 400);
                    ImGui.TableSetupColumn("Open", ImGuiTableColumnFlags.WidthFixed, 30);
                    ImGui.TableSetupColumn("Owner", ImGuiTableColumnFlags.WidthFixed, 200);
                    ImGui.TableSetupColumn("Fav", ImGuiTableColumnFlags.WidthFixed, 25);
                    ImGui.TableSetupColumn("Visit", ImGuiTableColumnFlags.WidthFixed, 25);
                    ImGui.TableSetupColumn("Comment", ImGuiTableColumnFlags.WidthFixed, 400);
                    ImGui.TableNextRow(ImGuiTableRowFlags.Headers);

                    ImGui.PushID("world");
                    ImGui.TableSetColumnIndex(0);
                    ImGui.TableHeader("World");
                    ImGui.SetNextItemWidth(80);
                    DrawWorldSelectBox("world-filter", "", pluginDataStorage.WorldIdFilter,
                                       pluginDataStorage.SetWorldIdFilter);
                    ImGui.PopID();

                    ImGui.PushID("Area");
                    ImGui.TableSetColumnIndex(1);
                    ImGui.TableHeader("Area");
                    ImGui.SetNextItemWidth(120);
                    var availableWards = pluginDataStorage.GetAvailableWards().ToList();
                    availableWards.Add(-1);
                    DrawWardSelectBox("ward-filter", pluginDataStorage.WardFilter, availableWards, pluginDataStorage.SetWardFilter);
                    ImGui.PopID();

                    ImGui.PushID("Ward");
                    ImGui.TableSetColumnIndex(2);
                    ImGui.TableHeader("Ward");
                    ImGui.SetNextItemWidth(45);
                    ImGui.PopID();

                    ImGui.PushID("Plot");
                    ImGui.TableSetColumnIndex(3);
                    ImGui.TableHeader("Plot");
                    ImGui.PopID();

                    ImGui.PushID("Tags");
                    ImGui.TableSetColumnIndex(4);
                    ImGui.TableHeader("Tags");
                    ImGui.SetNextItemWidth(130);
                    DrawTagSelectBox("tag-filter-1", "", pluginDataStorage.TagFilter,
                                     tag => pluginDataStorage.SetTagFilter(tag));
                    ImGui.SameLine();
                    ImGui.SetNextItemWidth(130);
                    DrawTagSelectBox("tag-filter-2", "", pluginDataStorage.TagFilter2,
                                     tag => pluginDataStorage.SetTagFilter2(tag));
                    ImGui.SameLine();
                    ImGui.SetNextItemWidth(130);
                    DrawTagSelectBox("tag-filter-3", "", pluginDataStorage.TagFilter3,
                                     tag => pluginDataStorage.SetTagFilter3(tag));
                    ImGui.PopID();
                    
                    var onlyOpen = pluginDataStorage.OnlyOpen;
                    ImGui.PushID("Open");
                    {
                        ImGui.TableSetColumnIndex(5);
                        ImGui.TableHeader("Open");
                        ImGui.Checkbox("", ref onlyOpen);
                        if (onlyOpen != pluginDataStorage.OnlyOpen)
                        {
                            pluginDataStorage.SetOpenFilter(onlyOpen);
                        }
                    }
                    ImGui.PopID();

                    InterfaceUtils.WithinId("Owner", () =>
                    {
                        ImGui.TableSetColumnIndex(6);
                        ImGui.TableHeader("Owner");
                        var ownerFilter = pluginDataStorage.OwnerFilter;
                        ImGui.InputText("", ref ownerFilter, 20);
                        if (ownerFilter != pluginDataStorage.OwnerFilter)
                        {
                            pluginDataStorage.SetOwnerFilter(ownerFilter);
                        }
                    });
                    
                    var onlyFavorites = pluginDataStorage.OnlyFavorites;
                    InterfaceUtils.WithinId("Favorite", () =>
                    {
                        ImGui.TableSetColumnIndex(7);
                        ImGui.TableHeader("Fav");
                        ImGui.Checkbox("", ref onlyFavorites);
                        if (onlyFavorites != pluginDataStorage.OnlyFavorites)
                        {
                            pluginDataStorage.SetOnlyFavoritesFilter(onlyFavorites);
                        }
                    });
                    
                    ImGui.PushID("Visit");
                    ImGui.TableSetColumnIndex(8);
                    ImGui.TableHeader("Visit");
                    ImGui.PopID();

                    ImGui.PushID("Comment");
                    ImGui.TableSetColumnIndex(9);
                    ImGui.TableHeader("Comment");
                    ImGui.PopID();


                    for (int row = 0; row < houses.Count; row++)
                    {
                        var house = houses[row];
                        List<HousingTag> tags =
                            [house.HouseMetaData.TagA, house.HouseMetaData.TagB, house.HouseMetaData.TagC];
                        var tagsToRender = tags.Where(tag => tag != HousingTag.None)
                                               .Select(tag => InterfaceUtils.TranslateHousingTag(tag))
                                               .ToList();
                        ImGui.PushID("row-" + house.HouseId);
                        ImGui.TableNextRow();
                        ImGui.TableSetColumnIndex(0);
                        ImGui.TextUnformatted(InterfaceUtils.TranslateWorld(house.HouseId.WorldId));
                        ImGui.TableSetColumnIndex(1);
                        ImGui.TextUnformatted(InterfaceUtils.TranslateTerritoryTypeId(house.HouseId.TerritoryTypeId));
                        ImGui.TableSetColumnIndex(2);
                        ImGui.TextUnformatted((house.HouseId.WardNumber + 1).ToString());
                        ImGui.TableSetColumnIndex(3);
                        ImGui.TextUnformatted(house.HouseId.PlotNumber.ToString());
                        
                        ImGui.TableSetColumnIndex(4);
                        ImGui.TextUnformatted(string.Join(", ", tagsToRender));

                        ImGui.TableSetColumnIndex(5);
                        ImGui.TextUnformatted(house.IsOpen() ? "x" : "");
                        
                        ImGui.TableSetColumnIndex(6);
                        ImGui.TextUnformatted(house.GetFormattedOwnerName());
                        
                        
                        ImGui.TableSetColumnIndex(7);
                        var favorite = house.Favorite;
                        ImGui.PushID("-favorite-checkbox");
                        ImGui.Checkbox("", ref favorite);
                        ImGui.PopID();
                        if (favorite != house.Favorite)
                        {
                            pluginDataStorage.MarkFavorite(house.HouseId, favorite);
                        }

                        ImGui.TableSetColumnIndex(8);
                        var visit = house.Visit;
                        ImGui.PushID("-visit-checkbox");
                        ImGui.Checkbox("", ref visit);
                        ImGui.PopID();
                        if (visit != house.Visit)
                        {
                            pluginDataStorage.MarkVisit(house.HouseId, visit);
                        }

                        ImGui.TableSetColumnIndex(9);
                        var comment = house.Comment;
                        ImGui.PushID("-comment-input");
                        ImGui.SetNextItemWidth(400);
                        ImGui.InputText("", ref comment, 4000);
                        if (comment != house.Comment)
                        {
                            pluginDataStorage.SetComment(house.HouseId, comment);
                        }

                        ImGui.PopID();
                        ImGui.PopID();
                    }

                    ImGui.EndTable();
                }

                var page1Index = pluginDataStorage.GetCurrentPage() + 1;

                DrawButton("first-page", "<<", !pluginDataStorage.HasPreviousPage(), pluginDataStorage.FirstPage);
                ImGui.SameLine();
                DrawButton("prev-page", "<", !pluginDataStorage.HasPreviousPage(), pluginDataStorage.PreviousPage);
                ImGui.SameLine();
                DrawButton("index",
                           $"Page {page1Index} of {pluginDataStorage.CurrentPageCount} (Total {pluginDataStorage.CurrentAvailableEntries})",
                           false, () => { });
                ImGui.SameLine();
                DrawButton("next-page", ">", !pluginDataStorage.HasNextPage(), pluginDataStorage.NextPage);
                ImGui.SameLine();
                DrawButton("last-page", ">>", !pluginDataStorage.HasNextPage(), pluginDataStorage.LastPage);
            }
        }
    }

    public void DrawButton(string id, string label, bool disabled, Action onClick)
    {
        ImGui.PushID(id);
        if (disabled)
        {
            ImGui.BeginDisabled();
        }

        if (ImGui.Button(label))
        {
            onClick.Invoke();
        }

        if (disabled)
        {
            ImGui.EndDisabled();
        }

        ImGui.PopID();
    }

    public void DrawSelectBox<TSelectBoxValue>(string id, string label, TSelectBoxValue value, List<TSelectBoxValue> availableValues, Func<TSelectBoxValue?, string> renderFn, Action<TSelectBoxValue> onChange)
    {
        ImGui.PushID(id);
        if (ImGui.BeginCombo(label, renderFn(value)))
        {
            foreach (var tag in availableValues)
            {
                var isSelected = tag != null && tag.Equals(value);
                if (ImGui.Selectable(renderFn(tag), isSelected))
                {
                    onChange.Invoke(tag);
                }
        
                if (isSelected)
                {
                    ImGui.SetItemDefaultFocus();
                }
            }
        
            ImGui.EndCombo();
        }

        ImGui.PopID();
    }

    public void DrawTagSelectBox(string id, string label, HousingTag? selectedTag, Action<HousingTag> onChange)
    {
        var toDisplay = ((HousingTag[])Enum.GetValues(typeof(HousingTag))).ToList()
                                                                           .OrderBy(tag => InterfaceUtils.TranslateHousingTag(tag))
                                                                           .ToList();
        var value = selectedTag ?? HousingTag.None;
        DrawSelectBox(id, label, value, toDisplay, tag => InterfaceUtils.TranslateHousingTag(tag), onChange.Invoke);
    }
    
    public void DrawWardSelectBox(string id, short? selectedWard, List<short> wards, Action<short> onChange)
    {
        var value = selectedWard ?? -1;
        DrawSelectBox(id, "", value, wards, InterfaceUtils.TranslateTerritoryTypeId, onChange.Invoke);
    }

    public void DrawWorldSelectBox(string id, string label, short selectedWorld, Action<short> onSelectedWorldChange)
    {
        ImGui.PushID(id);
        var availableWorlds = pluginDataStorage.GetAvailableWorlds()
                                               .OrderBy(InterfaceUtils.TranslateWorld)
                                               .ToList();
        availableWorlds.Add(-1);
        if (ImGui.BeginCombo(label, InterfaceUtils.TranslateWorld(selectedWorld)))
        {
            foreach (short world in availableWorlds)
            {
                var isSelected = world == selectedWorld;
                if (ImGui.Selectable(InterfaceUtils.TranslateWorld(world), isSelected))
                {
                    onSelectedWorldChange.Invoke(world);
                }

                if (isSelected)
                {
                    ImGui.SetItemDefaultFocus();
                }
            }

            ImGui.EndCombo();
        }

        ImGui.PopID();
    }
}
