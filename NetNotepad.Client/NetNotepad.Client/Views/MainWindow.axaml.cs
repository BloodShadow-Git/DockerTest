using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Avalonia.Controls;
using Avalonia.Controls.Notifications;
using NetNotepad.Client.Source;
using NetNotepad.Client.Source.Localization;
using NetNotepad.Client.ViewModels;
using ObservableCollections;
using R3;
using static NetNotepad.Client.Source.DataManager;

namespace NetNotepad.Client.Views;

public partial class MainWindow : Window
{
    public static NavigationPage NP { get; private set; } = null!;
    public static WindowNotificationManager NM { get; private set; } = null!;

    public MainWindow()
    {
        if (Environment.GetCommandLineArgs().Contains("--loc-gen"))
        {
            LocalizationData data = GenerateLocalizationData();
            string templatePath = "template.json";
            FileSystem.Save(templatePath, data);
            Environment.Exit(0);
        }
        AvaloniaProviderInitializer.SetDefaultObservableSystem();

        LocalizationData[] localizationDatas = FileSystem.LoadAll<LocalizationData>("Localizations", "*.json");
        foreach (string key in GetLocalizationKeys()) { LocalizationManager.RegisterKey<string>(key); }
        if (localizationDatas.Length <= 0) { LocalizationManager.AddLocalization(GenerateLocalizationData()); }
        LocalizationManager.AddLocalization(localizationDatas);

        string profilesDir = "Profiles";
        DataManager.LoadUsers(FileSystem.LoadAll<UserProfile>(profilesDir, "*.json"));
        DataManager.UserList.ObserveAdd().Subscribe(e => { FileSystem.Save(profilesDir + GenerateName(e.Value.Key), e.Value.Value); });
        DataManager.UserList.ObserveRemove().Subscribe(e => { FileSystem.Remove(profilesDir + GenerateName(e.Value.Key)); });

        InitializeComponent();
        NP = NavigationPage;
        NM = NotificationManager;
    }

    private static LocalizationData GenerateLocalizationData() => new("test-Test", [.. GetLocalizationKeys().Select(x => new LocalizationPair<string>(x, x))]);
    private static IEnumerable<string> GetLocalizationKeys() => typeof(PageLocalizations).GetFields(BindingFlags.Public | BindingFlags.Static)
                                                                    .Where(x => x.IsLiteral && x.FieldType == typeof(string))
                                                                    .Select(x => x.GetRawConstantValue()?.ToString() ?? "null")
                                                                    .OrderBy(x => x);
    private static string GenerateName(UserID userID) => $"{userID.UserServer.Replace('.', '-').Replace(':', '-')}_{userID.UserGuid}";
}