using System;
using Dalamud.Hooking;
using Dalamud.Plugin.Services;
using Dalamud.Utility.Signatures;
using HousingDatabase.Storage;

namespace HousingDatabase.Collector;

public unsafe class WardObserver {
    private readonly PluginDataStorage pluginDataStorage;
    private readonly IPluginLog pluginLog;
    private readonly HousingDataPusher housingDataPusher;

    private delegate void HandleHousingWardInfoDelegate(
        void* agentBase,
        IntPtr housingWardInfoPtr
    );

    [Signature("40 55 53 41 54 41 55 41 57 48 8D AC 24 ?? ?? ?? ?? B8", DetourName = nameof(OnHousingWardInfo))]
    private Hook<HandleHousingWardInfoDelegate>? housingWardInfoHook;

    public WardObserver(PluginDataStorage pluginDataStorage, IPluginLog pluginLog, HousingDataPusher housingDataPusher) {
        this.pluginDataStorage = pluginDataStorage;
        this.pluginLog = pluginLog;
        this.housingDataPusher = housingDataPusher;
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
        housingDataPusher.PushToRemote(result);

    }
}
