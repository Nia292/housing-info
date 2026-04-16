using System;
using System.Numerics;
using Dalamud.Interface.Windowing;
using Dalamud.Bindings.ImGui;
using Dalamud.Plugin.Services;
using Serilog;

namespace HousingDatabase.Windows;

public class ConfigWindow : Window, IDisposable
{
    private Configuration Configuration;
    private IPluginLog Log;

    public ConfigWindow(Plugin plugin, IPluginLog log) : base("Housing Configuration")
    {
        Size = new Vector2(400, 90);
        Log = log;

        Configuration = plugin.Configuration;
    }

    public void Dispose() { }

    public override void PreDraw()
    {
    }

    public override void Draw()
    {
        var remoteServer = Configuration.RemoteServer;
        ImGui.InputText("Remote Server", ref remoteServer, 200);

        var remoteKey = Configuration.RemoteKey;
        ImGui.InputText("Remote Key", ref remoteKey, 200);
        
        if (remoteServer != Configuration.RemoteServer || remoteKey != Configuration.RemoteKey)
        {
            Configuration.RemoteServer = remoteServer;
            Configuration.RemoteKey = remoteKey;
            Configuration.Save();
        }
    }
}
