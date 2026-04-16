using Dalamud.Configuration;
using Dalamud.Plugin;
using System;
using Dalamud.Utility;

namespace SamplePlugin;

[Serializable]
public class Configuration : IPluginConfiguration
{
    public int Version { get; set; } = 0;


    public string RemoteServer { get; set; } = "";
    public string RemoteKey { get; set; } = "";
    
    // the below exist just to make saving less cumbersome
    public void Save()
    {
        Plugin.PluginInterface.SavePluginConfig(this);
    }

    public bool HasRemoteServer()
    {
        return !RemoteServer.IsNullOrEmpty() && !RemoteKey.IsNullOrEmpty();
    }

    public string RemoteUpdateUri()
    {
        if (RemoteServer.Contains("localhost"))
        {
            return $"http://{RemoteServer}:8080/api/update";
        }

        return "https://" + RemoteServer + "/api/update";
    }
}
