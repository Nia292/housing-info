using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Dalamud.Game.Addon.Lifecycle;
using Dalamud.Game.Addon.Lifecycle.AddonArgTypes;
using Dalamud.Game.Command;
using Dalamud.Interface.Windowing;
using Dalamud.IoC;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using Dalamud.Utility;
using FFXIVClientStructs.FFXIV.Component.GUI;
using HousingDatabase.Collector;
using HousingDatabase.Storage;
using HousingDatabase.Windows;

namespace HousingDatabase;

public sealed class Plugin : IDalamudPlugin
{
    [PluginService]
    internal static IDalamudPluginInterface PluginInterface { get; private set; } = null!;

    [PluginService]
    internal static ICommandManager CommandManager { get; private set; } = null!;

    [PluginService]
    internal static IDataManager DataManager { get; private set; } = null!;

    [PluginService]
    internal static IPluginLog Log { get; private set; } = null!;

    [PluginService]
    internal static IGameInteropProvider InteropProvider { get; private set; } = null!;

    [PluginService]
    internal static IAddonLifecycle AddonLifecycle { get; private set; } = null!;
    
    [PluginService]
    internal static IObjectTable ObjectTable { get; private set; } = null!;

    private const string CommandName = "/houses";
    private const string CommandNameVisitList = "/visitlist";

    public Configuration Configuration { get; init; }

    public readonly WindowSystem WindowSystem = new("HousingDatabase");
    private ConfigWindow ConfigWindow { get; init; }
    private MainWindow MainWindow { get; init; }
    private VisitListWindow VisitListWindow { get; init; }

    private readonly HousingDataPusher housingDataPusher;
    private readonly WardObserver wardObserver;
    private readonly PluginDataStorage pluginDataStorage;

    private readonly HashSet<string> alreadySeen = [];

    public Plugin()
    {
        pluginDataStorage = PluginDataStorage.Instantiate(Log);
        Configuration = PluginInterface.GetPluginConfig() as Configuration ?? new Configuration();

        ConfigWindow = new ConfigWindow(this, Log);
        MainWindow = new MainWindow(this, pluginDataStorage, Log);
        VisitListWindow = new VisitListWindow(this, pluginDataStorage);

        WindowSystem.AddWindow(ConfigWindow);
        WindowSystem.AddWindow(MainWindow);
        WindowSystem.AddWindow(VisitListWindow);

        CommandManager.AddHandler(CommandName, new CommandInfo(OnMainUiCommand)
        {
            HelpMessage = "Opens the housing list."
        });

        CommandManager.AddHandler(CommandNameVisitList, new CommandInfo(OnVisitListCommand)
        {
            HelpMessage = "Opens the visit list"
        });


        PluginInterface.UiBuilder.Draw += DrawUI;

        // This adds a button to the plugin installer entry of this plugin which allows
        // to toggle the display status of the configuration ui
        PluginInterface.UiBuilder.OpenConfigUi += ToggleConfigUI;

        // Adds another button that is doing the same but for the main ui of the plugin
        PluginInterface.UiBuilder.OpenMainUi += ToggleMainUI;

        housingDataPusher = new HousingDataPusher(Log, Configuration);
        wardObserver = new WardObserver(pluginDataStorage, Log, housingDataPusher);
        AddonLifecycle.RegisterListener(AddonEvent.PostRequestedUpdate, "HousingSignBoard", OnPreDraw);
    }

    private unsafe void OnPreDraw(AddonEvent type, AddonArgs args)
    {
        try
        {
            var addon = (AtkUnitBase*)args.Addon.Address;
            var greetingAddr = addon->GetTextNodeById(28);
            if (greetingAddr != null)
            {
                var greetingText = greetingAddr->NodeText.AsReadOnlySeString().ExtractText();
                var plotTextAddr = addon->GetTextNodeById(21);
                if (plotTextAddr != null)
                {
                    var plotText = plotTextAddr->NodeText.AsReadOnlySeString().ExtractText();
                    var split = plotText.Split(",");
                    if (split.Length == 3)
                    {
                        int.TryParse(split[0].Split(" ")[1], out var plot);
                        short.TryParse(Regex.Match(split[1].Trim(), @"(\d*).*").Groups[1].Value, out var ward);
                        var location = Regex.Match(split[2].Trim(), @"([\w|\s]*) \(.*").Groups[1].Value;
                        var houseId = new HouseId((short)ObjectTable.LocalPlayer.CurrentWorld.RowId,
                                                  InterfaceUtils.ResolveTerritoryId(location), (short)(ward - 1), plot);
                        Log.Info("HouseId: " + houseId);
                        if (houseId.TerritoryTypeId > 0)
                        {
                            pluginDataStorage.AddGreeting(houseId, greetingText);
                            housingDataPusher.PushGreeting(houseId, greetingText);
                        }
                    }
                }
            }
        }
        catch (Exception e)
        {
            Log.Error(e, "Failure in addon hook.");
        }
       
    }

    public void Dispose()
    {
        WindowSystem.RemoveAllWindows();

        ConfigWindow.Dispose();
        MainWindow.Dispose();
        VisitListWindow.Dispose();

        CommandManager.RemoveHandler(CommandName);
        CommandManager.RemoveHandler(CommandNameVisitList);
        wardObserver.Dispose();
    }

    private void OnMainUiCommand(string command, string args)
    {
        ToggleMainUI();
    }

    private void OnVisitListCommand(string command, string args)
    {
        ToggleVisitList();
    }

    private void DrawUI() => WindowSystem.Draw();

    public void ToggleConfigUI() => ConfigWindow.Toggle();
    public void ToggleMainUI() => MainWindow.Toggle();
    public void ToggleVisitList() => VisitListWindow.Toggle();
}
