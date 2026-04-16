using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using Dalamud.Hooking;
using Dalamud.Plugin.Services;
using Dalamud.Utility.Signatures;
using Newtonsoft.Json;
using HousingDatabase.Storage;
using static System.Text.Encoding;
using Convert = System.Convert;

namespace HousingDatabase.Collector;

public unsafe class WardObserver {
    private readonly PluginDataStorage pluginDataStorage;
    private readonly HttpClient httpClient;
    private readonly Configuration configuration;
    private readonly IPluginLog pluginLog;

    private delegate void HandleHousingWardInfoDelegate(
        void* agentBase,
        IntPtr housingWardInfoPtr
    );

    [Signature("40 55 53 41 54 41 55 41 57 48 8D AC 24 ?? ?? ?? ?? B8", DetourName = nameof(OnHousingWardInfo))]
    private Hook<HandleHousingWardInfoDelegate>? housingWardInfoHook;

    public WardObserver(PluginDataStorage pluginDataStorage, Configuration configuration, IPluginLog pluginLog) {
        this.pluginDataStorage = pluginDataStorage;
        this.configuration = configuration;
        this.pluginLog = pluginLog;
        httpClient = new HttpClient();
        try
        {
            Plugin.InteropProvider.InitializeFromAttributes(this);
            housingWardInfoHook?.Enable();
        }
        catch (Exception e)
        {
            this.pluginLog.Info("Failed to initialize WardObserver " + e);
        }
           
    }

    public void Dispose() {
        housingWardInfoHook?.Dispose();
    }

    public void OnHousingWardInfo(
        void* agentBase,
        IntPtr dataPtr
    ) {
        housingWardInfoHook!.Original(agentBase, dataPtr);

        var result = HouseInfoEntryParser.Parse(dataPtr);
        pluginDataStorage.AddInfoEntries(result);
        PushToRemote(result);

    }

    private void PushToRemote(List<HouseInfoEntry> entries)
    {
        try
        {
            if (!configuration.HasRemoteServer())
            {
                return;
            }
            var uri = configuration.RemoteUpdateUri();
            var payload = JsonConvert.SerializeObject(entries);
            var httpRequestMessage = new HttpRequestMessage(HttpMethod.Post, uri);
            httpRequestMessage.Content = new StringContent(payload, UTF8, "application/json");
            httpRequestMessage.Headers.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(ASCII.GetBytes(configuration.RemoteKey)));
            pluginLog.Debug("Pushing housing data to " + uri);
            httpClient
                .SendAsync(httpRequestMessage)
                .ConfigureAwait(false);
        }
        catch (Exception e)
        {
            pluginLog.Error(e, "Failed to publish to server");
        }
    }
        
}
