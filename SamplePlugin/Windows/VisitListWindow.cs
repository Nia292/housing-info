using System;
using System.Numerics;
using Dalamud.Interface.Utility.Raii;
using Dalamud.Interface.Windowing;
using Dalamud.Bindings.ImGui;
using SamplePlugin.Storage;

namespace SamplePlugin.Windows;

public class VisitListWindow : Window, IDisposable
{
    private readonly PluginDataStorage pluginDataStorage;
    private readonly Plugin plugin;

    public VisitListWindow(Plugin plugin, PluginDataStorage pluginDataStorage) : base(
        "Visit List", ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse)
    {
        this.pluginDataStorage = pluginDataStorage;
        this.plugin = plugin;
        SizeConstraints = new WindowSizeConstraints
        {
            MinimumSize = new Vector2(300, 600),
            MaximumSize = new Vector2(float.MaxValue, float.MaxValue)
        };
    }

    public void Dispose() { }

    public override void Draw()
    {
        using var child = ImRaii.Child("visit-list", Vector2.Zero, true);
        if (child.Success)
        {
            foreach (var entry in pluginDataStorage.GetHousesToVisit())
            {
                var world = InterfaceUtils.TranslateWorld(entry.HouseId.WorldId);
                var ward = InterfaceUtils.TranslateTerritoryTypeId(entry.HouseId.TerritoryTypeId);
                ImGui.PushID(entry.HouseId.ToString());
                ImGui.TextUnformatted($"{world} - {ward} - W{entry.HouseId.WardNumber + 1} P{entry.HouseId.PlotNumber} ({entry.HouseMetaData.EstateOwnerName})");
                if (!entry.Favorite)
                {
                    ImGui.SameLine();
                    ImGui.SetCursorPosX(ImGui.GetWindowSize().X - ImGui.CalcTextSize("Visited").X - 30 - ImGui.CalcTextSize("Favorite").X);
                    if (ImGui.Button("Favorite"))
                    {
                        pluginDataStorage.MarkFavorite(entry.HouseId, true);
                    }
                }
                ImGui.SameLine();
                ImGui.SetCursorPosX(ImGui.GetWindowSize().X - ImGui.CalcTextSize("Favorite").X - 10);
                if (ImGui.Button("Visited"))
                {
                    pluginDataStorage.MarkVisit(entry.HouseId, false);
                }
                ImGui.PopID();
            }
        }
    }
}
