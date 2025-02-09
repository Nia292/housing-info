using System;
using System.Numerics;
using Dalamud.Interface.Utility.Raii;
using Dalamud.Interface.Windowing;
using ImGuiNET;
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
                var ward = InterfaceUtils.TranslateLandId(entry.HouseId.LandId);
                ImGui.PushID(entry.HouseId.ToString());
                ImGui.TextUnformatted($"{world} - {ward} - W{entry.HouseId.WardNumber} P{entry.HouseId.PlotNumber} ({entry.HouseMetaData.EstateOwnerName})");
                ImGui.SameLine();
                ImGui.SetCursorPosX(ImGui.GetWindowSize().X - ImGui.CalcTextSize("Visited").X - 20);
                if (ImGui.Button("Visited"))
                {
                    pluginDataStorage.MarkVisit(entry.HouseId, false);
                }
                ImGui.PopID();
            }
        }
    }
}
