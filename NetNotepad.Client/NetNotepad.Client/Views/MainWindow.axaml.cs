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

        string tokensFile = "token.list";
        List<string> tokensList = [.. (FileSystem.Load<string[]>(tokensFile) ?? [])];
        DataManager.LoadUsers([.. tokensList.Select(e => TokenStore.Load<UserProfile>(e))]);
        DataManager.UserList.ObserveAdd().Subscribe(e =>
        {
            string name = GenerateName(e.Value.Key);
            TokenStore.Save(name, e.Value.Value);
            tokensList.Add(name);
            FileSystem.Save(tokensFile, tokensList);
        });
        DataManager.UserList.ObserveRemove().Subscribe(e =>
        {
            string name = GenerateName(e.Value.Key);
            TokenStore.Remove(name);
            tokensList.Remove(name);
            FileSystem.Save(tokensFile, tokensList);
        });

        InitializeComponent();
        NP = NavigationPage;
        NM = NotificationManager;
    }

    private static LocalizationData GenerateLocalizationData() => new("test-TEST", [.. GetLocalizationKeys().Select(x => new LocalizationPair<string>(x, x))]);
    private static IEnumerable<string> GetLocalizationKeys() => typeof(PageLocalizations).GetFields(BindingFlags.Public | BindingFlags.Static)
                                                                    .Where(x => x.IsLiteral && x.FieldType == typeof(string))
                                                                    .Select(x => x.GetRawConstantValue()?.ToString() ?? "null")
                                                                    .OrderBy(x => x);
    private static string GenerateName(UserID userID) => $"Profiles/{new Uri(userID.UserServer).Host}_{userID.UserGuid}";
}