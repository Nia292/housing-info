using Dalamud.Game.Command;
using Dalamud.Interface.Windowing;
using Dalamud.IoC;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using SamplePlugin.Collector;
using SamplePlugin.Storage;
using SamplePlugin.Windows;

namespace SamplePlugin;

public sealed class Plugin : IDalamudPlugin
{
    [PluginService] internal static IDalamudPluginInterface PluginInterface { get; private set; } = null!;
    [PluginService] internal static ITextureProvider TextureProvider { get; private set; } = null!;
    [PluginService] internal static ICommandManager CommandManager { get; private set; } = null!;
    [PluginService] internal static IClientState ClientState { get; private set; } = null!;
    [PluginService] internal static IDataManager DataManager { get; private set; } = null!;
    [PluginService] internal static IPluginLog Log { get; private set; } = null!;
    [PluginService] internal static IGameInteropProvider InteropProvider { get; private set; } = null!;

    private const string CommandName = "/houses";
    private const string CommandNameVisitList = "/visitlist";

    public Configuration Configuration { get; init; }

    public readonly WindowSystem WindowSystem = new("SamplePlugin");
    private ConfigWindow ConfigWindow { get; init; }
    private MainWindow MainWindow { get; init; }
    private VisitListWindow VisitListWindow { get; init; }

    private readonly WardObserver wardObserver;
    private readonly PluginDataStorage pluginDataStorage;

    public Plugin()
    {
        pluginDataStorage = PluginDataStorage.Instantiate();
        Configuration = PluginInterface.GetPluginConfig() as Configuration ?? new Configuration();

        ConfigWindow = new ConfigWindow(this);
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

        wardObserver = new WardObserver(this, pluginDataStorage);
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
