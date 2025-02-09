using System;
using System.Numerics;
using Dalamud.Interface.Windowing;
using Dalamud.Plugin.Services;
using ImGuiNET;
using SamplePlugin.Storage;
using SamplePlugin.Windows.Tabs;
using ImGui = ImGuiNET.ImGui;

namespace SamplePlugin.Windows;

public class MainWindow : Window, IDisposable
{
    private readonly AllDataView AllDataView;

    // We give this window a hidden ID using ##
    // So that the user will see "My Amazing Window" as window title,
    // but for ImGui the ID is "My Amazing Window##With a hidden ID"
    public MainWindow(Plugin plugin, PluginDataStorage pluginDataStorage, IPluginLog log)
        : base("Houses Overview", ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse)
    {
        SizeConstraints = new WindowSizeConstraints
        {
            MinimumSize = new Vector2(1200, 600),
            MaximumSize = new Vector2(float.MaxValue, float.MaxValue)
        };

        AllDataView = new AllDataView(pluginDataStorage, plugin);
    }

    public void Dispose() { }

    public override void Draw()
    {
        ImGui.BeginTabBar("main-window-tabs");
        if (ImGui.BeginTabItem("All Data"))
        {
            AllDataView.Draw();
            ImGui.EndTabItem();
        }
        if (ImGui.BeginTabItem("Favorites"))
        {
            ImGui.EndTabItem();
        }
        ImGui.EndTabBar();
    }
}
