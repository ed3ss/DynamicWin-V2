using System;
using System.Runtime.InteropServices;
using FenUISharp;
using FenUISharp.WinFeatures;
using SkiaSharp;

namespace DynamicWin;

internal static class ComfortRuntime
{
    internal static volatile bool GameMode = true;
    internal static volatile bool Economy = true;
    internal static volatile bool Shortcuts = true;
    internal static volatile bool Suspended;
    internal static volatile bool ManualHidden;
    internal static string[] Exclusions = Array.Empty<string>();
    internal static float OffsetX, OffsetY, Opacity = 1;
    internal static SKColor Background = SKColors.Black;
    private static FWindow? window;
    private static long lastToggle;

    internal static void Load()
    {
        GameMode = Settings.GetSetting("comfort.game-mode", true);
        Economy = Settings.GetSetting("comfort.economy", true);
        Shortcuts = Settings.GetSetting("comfort.shortcuts", true);
        Exclusions = ComfortOptions.ParseExclusions(Settings.GetSetting("comfort.exclusions", ""));
        OffsetX = Math.Clamp(Settings.GetSetting("comfort.x", 0f), -500, 500);
        OffsetY = Math.Clamp(Settings.GetSetting("comfort.y", 0f), 0, 250);
        Opacity = Math.Clamp(Settings.GetSetting("comfort.opacity", 1f), .3f, 1f);
        string color = Settings.GetSetting("comfort.color", "#000000");
        Background = ComfortOptions.ValidColor(color) ? SKColor.Parse(color) : SKColors.Black;
        LocalDiagnostics.Enabled = Settings.GetSetting("comfort.diagnostics", true);
    }

    internal static void Initialize(FWindow target)
    {
        window = target;
        Load();
        LocalDiagnostics.Initialize();
        void Bind(int vk, Action action) => target.WindowKeyboardInput.RegisterKeybind(new KeyBind
        {
            Flags = KeyBindFlags.Control | KeyBindFlags.Alt, VKCode = vk, IgnoreFocus = true,
            OnKeybindExecuted = () =>
            {
                if (!Shortcuts || Environment.TickCount64 - lastToggle < 300) return;
                lastToggle = Environment.TickCount64;
                action();
            }
        });
        Bind('D', () => { ManualHidden = !ManualHidden; LocalDiagnostics.Write("manual visibility changed"); });
        Bind('P', () => WindowsMediaControls.TriggerMediaControl(MediaControlTrigger.PlayPauseToggle));
        Bind(0x27, () => WindowsMediaControls.TriggerMediaControl(MediaControlTrigger.SkipNext));
        Bind(0x25, () => WindowsMediaControls.TriggerMediaControl(MediaControlTrigger.SkipPrevious));
    }

    internal static void Tick(bool suspended)
    {
        if (Suspended != suspended) LocalDiagnostics.Write(suspended ? "overlay paused" : "overlay resumed");
        Suspended = suspended;
        var info = new LastInput { Size = (uint)Marshal.SizeOf<LastInput>() };
        bool idle = GetLastInputInfo(ref info) && unchecked((uint)Environment.TickCount - info.Tick) > 10000;
        bool playing = WindowsMediaControls.CachedInfo.playbackState == Windows.Media.Control.GlobalSystemMediaTransportControlsSessionPlaybackStatus.Playing;
        if (window != null) window.Loop.MaxFramesPerSecond = ComfortOptions.FrameRate(Economy, idle, playing);
    }
    [StructLayout(LayoutKind.Sequential)] private struct LastInput { public uint Size, Tick; }
    [DllImport("user32.dll")] private static extern bool GetLastInputInfo(ref LastInput info);
}
