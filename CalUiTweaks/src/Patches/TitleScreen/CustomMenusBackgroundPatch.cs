using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;

using CalApi.API;
using CalApi.Patches;

using HarmonyLib;

using UnityEngine;
using UnityEngine.Video;

namespace CalUiTweaks.Patches.TitleScreen;

// ReSharper disable once UnusedType.Global
public class CustomMenusBackgroundPatch : IPatch {
    private const string RootName = "Menus";
    private const string ReadmeName = "CustomMenusBackground-README.txt";
    private const string BackgroundVideoName = "titleBg.mp4";
    private const string BackgroundName = "background.txt";

    private static string? _rootPath;
    private static string? _videoBackgroundPath;
    private static int _forceTheme;
    private static bool _backgroundElements;
    private static VideoAspectRatio _aspectRatio;

    private static bool _wasUnloaded = true;

    private static GameObject? _backgroundContainer;
    //private static RawImage? _imageBackground;
    private static VideoPlayer? _videoBackground;

    private static AudioSource? _audio;

    public void Apply() {
        On.ThemeSystem.Awake += (orig, self) => {
            orig(self);
            ThemeSystem.ApplyTheme(_forceTheme);
        };

        BackgroundElements? backgroundElements = null;
        FloaterContainer? floaterContainer = null;

        On.TitleScreen.Awake += (orig, self) => {
            orig(self);
            CreateBackgroundContainer();
            EnterBackground(_backgroundContainer!, ref backgroundElements, ref floaterContainer);
        };

        On.PolyMap.MapManager.LoadPolyMap += (orig, name, state, path) => {
            LeaveBackground(backgroundElements, floaterContainer);
            return orig(name, state, path);
        };

        On.Music.Update += (orig, self) => {
            orig(self);
            Update();
        };

        UpdateSettings(RootName, BackgroundVideoName, BackgroundName);
        CustomizationProfiles.profileChanged += (_, _) => {
            LeaveBackground(backgroundElements, floaterContainer);
            UpdateSettings(RootName, BackgroundVideoName, BackgroundName);
            CreateBackgroundContainer();
            EnterBackground(_backgroundContainer!, ref backgroundElements, ref floaterContainer);
            ThemeSystem.ApplyTheme(_forceTheme);
        };

        Directory.CreateDirectory(Path.Combine(CustomizationProfiles.defaultPath, RootName));
        CreateReadme(Path.Combine(CustomizationProfiles.defaultPath, RootName, ReadmeName));
    }

    private static void CreateReadme(string path) {
        if(File.Exists(path)) return;
        File.WriteAllText(path, @"If a file named `titleBg.mp4` exists, it will be shown as the background instead of the default one.
background.txt contains some settings about the background:
line 1: theme - the ID of the theme which you can select in the editor
  you can get it by counting where is it in the list and subtracting 1, for example World 1 theme would have an ID of 0
line 2: background elements - fog and vignette (possible values: true, false | default: true) (*not* case sensitive)
line 3: video aspect ratio - https://docs.unity3d.com/2018.4/Documentation/ScriptReference/Video.VideoAspectRatio.html | default: no idea
if a line is left blank or doesn't exist in the file at all, the default value would be used");
    }

    private static void CreateBackgroundContainer() {
        if(_backgroundContainer) return;
        _backgroundContainer = new GameObject("Background Container");
        _backgroundContainer.transform.SetParent(CanvasManager.Instance.transform);
        _backgroundContainer.transform.SetAsFirstSibling();
    }

    private static void UpdateSettings(string rootName, string videoName, string backgroundName) {
        _rootPath = Path.Combine(CustomizationProfiles.currentPath!, rootName);
        _videoBackgroundPath = Path.Combine(_rootPath, videoName);
        string backgroundPath = Path.Combine(_rootPath, backgroundName);
        ResetSettings();
        if(!File.Exists(backgroundPath)) return;
        ParseSettings(File.ReadAllLines(backgroundPath));
    }

    private static void ResetSettings() {
        _forceTheme = -1;
        _backgroundElements = true;
        _aspectRatio = VideoAspectRatio.FitHorizontally;
    }

    [SuppressMessage("ReSharper", "InvertIf")]
    private static void ParseSettings(IReadOnlyList<string> settings) {
        if(settings.Count >= 1 && int.TryParse(settings[0], out int theme)) _forceTheme = theme;
        if(settings.Count >= 2 && bool.TryParse(settings[1], out bool bgElements)) _backgroundElements = bgElements;
        if(settings.Count >= 3 && Enum.TryParse(settings[2], true, out VideoAspectRatio ar)) {
            _aspectRatio = ar;
            if(_videoBackground) _videoBackground!.aspectRatio = ar;
        }
    }

    private static void EnterBackground(GameObject parent, ref BackgroundElements? backgroundElements,
        ref FloaterContainer? floaterContainer) {
        if(!_wasUnloaded) return;
        _wasUnloaded = false;

        backgroundElements = backgroundElements ? backgroundElements :
            Camera.main!.transform.GetComponentInChildren<BackgroundElements>(true);
        floaterContainer = floaterContainer ? floaterContainer :
            (FloaterContainer)AccessTools.Field(typeof(FloaterContainer), "instance").GetValue(null);

        if(File.Exists(_videoBackgroundPath)) {
            floaterContainer!.gameObject.SetActive(false);
            if(!_videoBackground) CreateVideoBackground(parent);
            _videoBackground!.enabled = true;
            LoadVideoBackground(_videoBackgroundPath!);
        }

        backgroundElements!.gameObject.SetActive(_backgroundElements);
    }

    [SuppressMessage("ReSharper", "SuggestBaseTypeForParameter")]
    private static void LeaveBackground(BackgroundElements? backgroundElements, FloaterContainer? floaterContainer) {
        _wasUnloaded = true;
        if(backgroundElements) backgroundElements!.gameObject.SetActive(true);
        //if(_imageBackground) _imageBackground!.enabled = false;
        if(_videoBackground) {
            _videoBackground!.Stop();
            _videoBackground.enabled = false;
        }
        if(!floaterContainer || floaterContainer!.gameObject.activeSelf) return;
        floaterContainer.gameObject.SetActive(true);
        // ReSharper disable once HeapView.BoxingAllocation
        AccessTools.Field(typeof(FloaterContainer), "progress").SetValue(floaterContainer, 1f);
    }

    private static void Update() {
        if(!_videoBackground || !_videoBackground!.enabled) return;
        UpdateAudioSource();
        if(!_audio) return;
        float time = _audio!.time;
        _videoBackground.externalReferenceTime = time;
    }

    //private static void CreateImageBackground(GameObject parent) => _imageBackground = parent.AddComponent<RawImage>();

    // ah yes, settings
    private static void CreateVideoBackground(GameObject parent) {
        _videoBackground = parent.AddComponent<VideoPlayer>();
        _videoBackground.playOnAwake = false;
        _videoBackground.renderMode = VideoRenderMode.CameraFarPlane;
        _videoBackground.targetCamera = Camera.main;
        _videoBackground.audioOutputMode = VideoAudioOutputMode.None;
        _videoBackground.isLooping = true;
        _videoBackground.timeReference = VideoTimeReference.ExternalTime;
        _videoBackground.skipOnDrop = true;
        _videoBackground.aspectRatio = _aspectRatio;
    }

    /*private static void LoadImageBackground(string path) {
        if(!File.Exists(path)) return;

        Texture2D texture = new(1, 1);
        if(!texture.LoadImage(File.ReadAllBytes(path))) return;

        if(_imageBackground) _imageBackground!.texture = texture;
    }*/

    private static void LoadVideoBackground(string path) {
        if(!_videoBackground) return;
        _videoBackground!.url = path;
        _videoBackground.Play();
        UpdateAudioSource();
        if(_audio) _videoBackground.playbackSpeed = _audio!.pitch;
    }

    private static void UpdateAudioSource() {
        if(!_audio) _audio = (AudioSource)AccessTools.Field(typeof(Music), "audioSource").GetValue(null);
    }
}
