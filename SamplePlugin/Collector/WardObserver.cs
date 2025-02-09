using System;
using Dalamud.Hooking;
using Dalamud.IoC;
using Dalamud.Plugin.Services;
using Dalamud.Utility.Signatures;
using SamplePlugin.Storage;

namespace SamplePlugin.Collector;

public unsafe class WardObserver {
        private Plugin plugin;
        private readonly PluginDataStorage pluginDataStorage;
        [PluginService] internal static IPluginLog PluginLog { get; private set; } = null!;

        private delegate void HandleHousingWardInfoDelegate(
            void* agentBase,
            IntPtr housingWardInfoPtr
        );

        [Signature("40 55 53 41 54 41 55 41 57 48 8D AC 24 ?? ?? ?? ?? B8", DetourName = nameof(OnHousingWardInfo))]
        private Hook<HandleHousingWardInfoDelegate>? housingWardInfoHook;

        public WardObserver(Plugin plugin, PluginDataStorage pluginDataStorage) {
            this.plugin = plugin;
            this.pluginDataStorage = pluginDataStorage;
            try
            {
                Plugin.InteropProvider.InitializeFromAttributes(this);
                housingWardInfoHook?.Enable();
            }
            catch (Exception e)
            {
                PluginLog.Info("Failed to initialize WardObserver " + e.ToString());
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
        }

        
    }
