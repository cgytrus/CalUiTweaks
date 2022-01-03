using System;
using System.IO;

using CalApi.API;
using CalApi.Patches;

using UnityEngine;
using UnityEngine.UI;

namespace CalUiTweaks.Patches.TitleScreen;

// ReSharper disable once UnusedType.Global
internal class CustomizableTitlePatch : IPatch {
    private const string RootName = "Title Screen";
    private const string ReadmeName = "CustomizeableTitle-README.txt";
    private const string TitleName = "title.txt";
    private const string SubtitleName = "subtitle.txt";

    private static string? _rootPath;

    private static Text? _title;
    private static Text? _subtitle;

    public void Apply() {
        On.TitleScreen.Awake += (orig, self) => {
            orig(self);
            Transform parent = self.transform.Find("Title Screen Stuff").Find("Title Parent");
            _title = parent.Find("Title").GetComponent<Text>();
            _subtitle = parent.Find("Subtitle").GetComponent<Text>();
            UpdateSettings(RootName, TitleName, SubtitleName);
        };

        CustomizationProfiles.profileChanged += (_, _) => UpdateSettings(RootName, TitleName, SubtitleName);

        Directory.CreateDirectory(Path.Combine(CustomizationProfiles.defaultPath, RootName));
        CreateReadme(Path.Combine(CustomizationProfiles.defaultPath, RootName, ReadmeName));
    }

    private static void CreateReadme(string path) {
        if(File.Exists(path)) return;
        File.WriteAllText(path, @"Files title.txt and subtitle.txt change the title and subtitle respectively.
They both have the following format (*not* case sensitive):
line 1: horizontal overflow - possible values: wrap, overflow | default: wrap
line 2: vertical overflow - possible values: truncate, overflow | default: truncate
line 3: font size - default title: 240, default subtitle: 74
line 4: text - supports multiple lines; default title: Cats are Liquid, default subtitle: A Better Place");
    }

    private static void UpdateSettings(string rootName, string titleName, string subtitleName) {
        _rootPath = Path.Combine(CustomizationProfiles.currentPath!, rootName);
        string titlePath = Path.Combine(_rootPath, titleName);
        string subtitlePath = Path.Combine(_rootPath, subtitleName);
        ResetSettings();
        UpdateTitle(titlePath);
        UpdateSubtitle(subtitlePath);
    }

    private static void ResetSettings() {
        if(_title) {
            _title!.horizontalOverflow = HorizontalWrapMode.Wrap;
            _title.verticalOverflow = VerticalWrapMode.Truncate;
            _title.fontSize = 240;
            _title.text = "Cats are Liquid";
        }

        // ReSharper disable once InvertIf
        if(_subtitle) {
            _subtitle!.horizontalOverflow = HorizontalWrapMode.Wrap;
            _subtitle.verticalOverflow = VerticalWrapMode.Truncate;
            _subtitle.fontSize = 74;
            _subtitle.text = "A Better Place";
        }
    }

    private static void UpdateTitle(string path) {
        if(!_title || !File.Exists(path)) return;
        string[] settings = File.ReadAllLines(path);
        if(settings.Length >= 1 && Enum.TryParse(settings[0], true, out HorizontalWrapMode horizontalWrap))
            _title!.horizontalOverflow = horizontalWrap;
        if(settings.Length >= 2 && Enum.TryParse(settings[1], true, out VerticalWrapMode verticalWrap))
            _title!.verticalOverflow = verticalWrap;
        if(settings.Length >= 3 && int.TryParse(settings[2], out int fontSize)) _title!.fontSize = fontSize;
        _title!.text = settings.Length >= 4 ? string.Join("\n", settings, 3, settings.Length - 3) : "";
    }

    private static void UpdateSubtitle(string path) {
        if(!_subtitle || !File.Exists(path)) return;
        string[] settings = File.ReadAllLines(path);
        if(settings.Length >= 1 && Enum.TryParse(settings[0], true, out HorizontalWrapMode horizontalWrap))
            _subtitle!.horizontalOverflow = horizontalWrap;
        if(settings.Length >= 2 && Enum.TryParse(settings[1], true, out VerticalWrapMode verticalWrap))
            _subtitle!.verticalOverflow = verticalWrap;
        if(settings.Length >= 3 && int.TryParse(settings[2], out int fontSize)) _subtitle!.fontSize = fontSize;
        _subtitle!.text = settings.Length >= 4 ? string.Join("\n", settings, 3, settings.Length - 3) : "";
    }
}