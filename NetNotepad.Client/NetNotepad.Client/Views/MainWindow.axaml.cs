using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Avalonia.Controls;
using Avalonia.Controls.Notifications;
using NetNotepad.Client.Source;
using NetNotepad.Client.Source.Localization;
using NetNotepad.Client.ViewModels;
using R3;

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

        LocalizationData[] datas = FileSystem.LoadAll<LocalizationData>("Localizations", "*.json");
        foreach (string key in GetLocalizationKeys()) { LocalizationManager.RegisterKey<string>(key); }
        if (datas.Length <= 0) { LocalizationManager.AddLocalization(GenerateLocalizationData()); }
        LocalizationManager.AddLocalization(datas);

        InitializeComponent();
        NP = NavigationPage;
        NM = NotificationManager;
    }

    private static LocalizationData GenerateLocalizationData() => new("test-Test", [.. GetLocalizationKeys().Select(x => new LocalizationPair<string>(x, x))]);

    private static IEnumerable<string> GetLocalizationKeys()
    {
        return typeof(PageLocalizations).GetFields(BindingFlags.Public | BindingFlags.Static)
                        .Where(x => x.IsLiteral && x.FieldType == typeof(string))
                        .Select(x => x.GetRawConstantValue()?.ToString() ?? "null")
                        .OrderBy(x => x);
    }
}