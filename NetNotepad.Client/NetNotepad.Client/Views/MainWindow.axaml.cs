using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Avalonia.Controls;
using Avalonia.Controls.Notifications;
using NetNotepad.Client.Source;
using NetNotepad.Client.Source.Localization;
using NetNotepad.Client.ViewModels;

namespace NetNotepad.Client.Views;

public partial class MainWindow : Window
{
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
        LocalizationData[] datas = FileSystem.LoadAll<LocalizationData>("Localizations");
        foreach (string key in GetLocalizationKeys()) { LocalizationManager.RegisterKey<string>(key); }
        if (datas.Length <= 0) { LocalizationManager.AddLocalization(GenerateLocalizationData()); }
        LocalizationManager.AddLocalization(datas);

        InitializeComponent();
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