//#define XBDEBUG
namespace XB { // namespace open
using SysCG = System.Collections.Generic;
public enum SettingsPreset {
    Minimum,
    Default,
    Maximum,
}

// layer is which layers the object is in, mask is which layers an object scans for collisions
public struct LayerMasks {               //24--20--16--12--8---4---  
    public static uint EmptyMask       = 0b000000000000000000000000;
    public static uint AimMask         = 0b000001001100001001001001;
    public static uint CamMask         = 0b000000000000000000000011;
    public static uint EnvironmentMask = 0b000000000000000000001001;
    public static uint MovementMask    = 0b100000000000000100000000;
    public static uint MovementLayer   = 0b000000000000000010000000;
    public static uint PlayerMask      = 0b000000000000000110001001;
    public static uint PlayerLayer     = 0b100000000000000000000000;
                                         //24--20--16--12--8---4---
}

public struct Constants {
    public const float Tau     = 6.28318530718f;
    public const float Pi      = 3.14159265359f;
    public const float PiHalf  = 1.57079632679f;
    public const float Sqrt2   = 1.41421356237f;
    public const float Deg2Rad = 0.01745329251f;
    public const float Rad2Deg = 57.2957795131f;
    public static Godot.Transform3D OriginTransform 
        = new Godot.Transform3D(new Godot.Vector3(1.0f, 0.0f, 0.0f),
                                new Godot.Vector3(0.0f, 1.0f, 0.0f),
                                new Godot.Vector3(0.0f, 0.0f, 1.0f),
                                new Godot.Vector3(0.0f, 0.0f, 0.0f));
    public const string TimeFormat = "F6";
}

public struct ScenePaths {
    public static string Player      = "res://assets/player/playerController.tscn";
    public static string ButtonAudio = "res://assets/audio/soundButtonPress.tscn";
}

public struct WorldData {
    public static Godot.Color MsgColor     = new Godot.Color (0.2f, 0.2f, 0.2f, 1.0f);
    public static Godot.Color MsgFadeColor = new Godot.Color (0.1f, 0.1f, 0.1f, 0.0f);
    public static float       LowestPoint  = -128.0f;  // lowest used point used in player falling off,
                                                       // gets updated with terrain updating
    public static float       KillPlane    = -4096.0f; // fallback for the player falling off
}

public class AData {
    public static XB.Input                 Input;
    public static Godot.DirectionalLight3D MainLight;
    public static Godot.Environment        Environment;
    public static Godot.Node               MainRoot;
    public static Godot.Node               TR = new Godot.Node(); //NOTE[ALEX]: necessary to use Tr()

    public static float      CamCollDist    = 0.2f;     // distance from camera to colliders
    public static bool       Controller     = false;
    public static int        Fps            = 60;
    public static bool       ShowFps        = false;
    public static float      FovDef         = 0.0f; // camera fov when not aiming (in mm)
    public static float      FovAim         = 0.0f; // when aiming
    public static float      FovZoom        = 0.0f;
    public static float      FovMin         = 12.0f;
    public static float      FovMax         = 70.0f;
    public static float      CamMinDist     = 0.5f;
    public static float      CamMaxDist     = 0.0f;
    public static float      CamAimDist     = 0.0f;
    public static float      CamXSens       = 2.0f;
    public static float      CamYSens       = 2.0f;
    public static float      Volume         = 0.0f; // audio master volume
    public static string[]   WindowModes    = new string[] {"WINDOWED", "FULLSCREEN"};
    public static bool       FullScreen     = false;
    public static string[]   Languages      = new string[] {"en", "de"};
    public static string     Language       = "en";
    public static bool       VSync          = false;
    public static string[]   MSAA           = new string[] {"DISABLED", "MSAA2", "MSAA4", "MSAA8"};
    public static string     MSAASel        = "DISABLED";
    public static string[]   SSAA           = new string[] {"DISABLED", "FXAA"};
    public static string     SSAASel        = "DISABLED";
    public static bool       TAA            = false;
    public static bool       Debanding      = false;
    public static int[]      ShadowSizes    = new int[] {512, 1024, 2048, 4096};
    public static int        ShadowSize     = 512;
    public static string[]   ShadowFilters  = new string[] {"SHADOWF0", "SHADOWF1", "SHADOWF2",
                                                            "SHADOWF3", "SHADOWF4", "SHADOWF5"};
    public static string     ShadowFilter   = "SHADOWF0";
    public static int        ShadowDistance = 300;
    public static float[]    LOD            = new float[] {4.0f, 2.0f, 1.0f};
    public static float      LODSel         = 1.0f;
    public static string[]   SSAO           = new string[] {"SSAO0", "SSAO1", "SSAO2", "SSAO3"};
    public static string     SSAOSel        = "SSAO0";
    public static bool       SSAOHalf       = false;
    public static string[]   SSIL           = new string[] {"SSIL0", "SSIL1", "SSIL2", "SSIL3"};
    public static string     SSILSel        = "SSIL0";
    public static bool       SSILHalf       = false;
    public static bool       SSR            = false;
    public static string     BaseResolution = "1920x1080";
    public static string     Resolution     = BaseResolution;
    public static SysCG.Dictionary<string, Godot.Vector2I> Resolutions
        = new SysCG.Dictionary<string, Godot.Vector2I>() {
            {"3840x2160", new Godot.Vector2I(3840, 2160)},
            {"2560x1440", new Godot.Vector2I(2560, 1440)},
            {"2048x1152", new Godot.Vector2I(2048, 1152)},
            {"1920x1080", new Godot.Vector2I(1920, 1080)},
            {"1280x720",  new Godot.Vector2I(1280, 720)},
        };
}

public class PersistData {
    private static float _fovAimM = 1.25f;
    private static float _fovZM   = 2.25f;
    private static float _fovDef  = 28.0f;

    public static void UpdateFov(float value = 28.0f) {
        XB.AData.FovDef     = value;
        XB.AData.FovAim     = value*_fovAimM;
        XB.AData.FovAim     = XB.Utils.ClampF(XB.AData.FovAim, XB.AData.FovMin, XB.AData.FovMax);
        XB.AData.FovZoom    = value*_fovZM;
        XB.AData.FovZoom    = XB.Utils.ClampF(XB.AData.FovZoom, XB.AData.FovMin, XB.AData.FovMax);
        XB.AData.CamMaxDist = value*(1.0f/28.0f)*4.2f;
        XB.AData.CamAimDist = value*(1.0f/28.0f)*1.0f;
    }

    public static void UpdateScreen() {
        var window  = XB.AData.MainRoot.GetTree().Root;
        window.Size = XB.AData.Resolutions[XB.AData.Resolution];
        float scale = ((float)XB.AData.Resolutions[XB.AData.Resolution].X) /
                      ((float)XB.AData.Resolutions[XB.AData.BaseResolution].X);
        if (XB.AData.FullScreen) {
            window.Mode               = Godot.Window.ModeEnum.Fullscreen;
            XB.PController.Hud.Scale  = new Godot.Vector2(scale, scale);
            XB.PController.Menu.Scale = new Godot.Vector2(scale, scale);
            window.ContentScaleFactor = 1.0f/scale;
            window.ContentScaleMode   = Godot.Window.ContentScaleModeEnum.Viewport;
        } else {
            window.Mode               = Godot.Window.ModeEnum.Windowed;
            XB.PController.Hud.Scale  = new Godot.Vector2(1.0f, 1.0f);
            XB.PController.Menu.Scale = new Godot.Vector2(1.0f, 1.0f);
            window.ContentScaleFactor = scale;
            window.ContentScaleMode   = Godot.Window.ContentScaleModeEnum.Disabled;
        }

        // VIEWPORT SETTINGS
        var viewport = XB.AData.MainRoot.GetViewport();
        viewport.UseDebanding = XB.AData.Debanding;
        viewport.UseTaa       = XB.AData.TAA;

        //NOTE[ALEX]: I could not figure out the c# naming of the msaa enums in Godot, so I am casting ints:
        //            0 -> disabled, 1 -> 2x, 2 -> 4x, 3 -> 8x
        switch (XB.AData.MSAASel) {
            case "DISABLED": viewport.Msaa3D = (Godot.Viewport.Msaa)0; break;
            case "MSAA2":    viewport.Msaa3D = (Godot.Viewport.Msaa)1; break;
            case "MSAA4":    viewport.Msaa3D = (Godot.Viewport.Msaa)2; break;
            case "MSAA8":    viewport.Msaa3D = (Godot.Viewport.Msaa)3; break;
        }

        switch (XB.AData.SSAASel) {
            case "DISABLED": viewport.ScreenSpaceAA = (Godot.Viewport.ScreenSpaceAAEnum)0; break;
            case "FXAA":     viewport.ScreenSpaceAA = (Godot.Viewport.ScreenSpaceAAEnum)1; break;
        }

        // ENGINE SETTINGS
        Godot.Engine.MaxFps = XB.AData.Fps;

        // DISPLAYSERVER SETTINGS
        if (XB.AData.VSync) {
            Godot.DisplayServer.WindowSetVsyncMode(Godot.DisplayServer.VSyncMode.Enabled);
        } else {
            Godot.DisplayServer.WindowSetVsyncMode(Godot.DisplayServer.VSyncMode.Disabled);
        }

        // RENDERINGSERVER SETTINGS
        Godot.RenderingServer.DirectionalShadowAtlasSetSize(XB.AData.ShadowSize, true);
        var sFQuality = Godot.RenderingServer.ShadowQuality.Hard;
        switch (XB.AData.ShadowFilter) {
            case "SHADOWF0": sFQuality = Godot.RenderingServer.ShadowQuality.Hard;        break;
            case "SHADOWF1": sFQuality = Godot.RenderingServer.ShadowQuality.SoftVeryLow; break;
            case "SHADOWF2": sFQuality = Godot.RenderingServer.ShadowQuality.SoftLow;     break;
            case "SHADOWF3": sFQuality = Godot.RenderingServer.ShadowQuality.SoftMedium;  break;
            case "SHADOWF4": sFQuality = Godot.RenderingServer.ShadowQuality.SoftHigh;    break;
            case "SHADOWF5": sFQuality = Godot.RenderingServer.ShadowQuality.SoftUltra;   break;
        }
        Godot.RenderingServer.DirectionalSoftShadowFilterSetQuality(sFQuality);

        var ssilQuality = Godot.RenderingServer.EnvironmentSsilQuality.VeryLow;
        switch (XB.AData.SSILSel) {
            case "SSIL0": ssilQuality = Godot.RenderingServer.EnvironmentSsilQuality.VeryLow; break; 
            case "SSIL1": ssilQuality = Godot.RenderingServer.EnvironmentSsilQuality.Low;     break; 
            case "SSIL2": ssilQuality = Godot.RenderingServer.EnvironmentSsilQuality.Medium;  break; 
            case "SSIL3": ssilQuality = Godot.RenderingServer.EnvironmentSsilQuality.High;    break; 
        }
        Godot.RenderingServer.EnvironmentSetSsilQuality(ssilQuality, XB.AData.SSILHalf, 
                                                        0.5f, 4, 50.0f, 300.0f);

        var ssaoQuality = Godot.RenderingServer.EnvironmentSsaoQuality.VeryLow;
        switch (XB.AData.SSAOSel) {
            case "SSAO0": ssaoQuality = Godot.RenderingServer.EnvironmentSsaoQuality.VeryLow; break;
            case "SSAO1": ssaoQuality = Godot.RenderingServer.EnvironmentSsaoQuality.Low;     break;
            case "SSAO2": ssaoQuality = Godot.RenderingServer.EnvironmentSsaoQuality.Medium;  break;
            case "SSAO3": ssaoQuality = Godot.RenderingServer.EnvironmentSsaoQuality.High;    break;
        }
        Godot.RenderingServer.EnvironmentSetSsaoQuality(ssaoQuality, XB.AData.SSAOHalf,
                                                        0.5f, 2, 50.0f, 300.0f);

        XB.AData.MainRoot.GetTree().Root.MeshLodThreshold = XB.AData.LODSel;

        if (XB.AData.MainLight != null) {
            XB.AData.MainLight.DirectionalShadowMaxDistance = XB.AData.ShadowDistance;
        }

        if (XB.AData.ShowFps) XB.PController.Hud.LbFps.Show();
        else                  XB.PController.Hud.LbFps.Hide();

        XB.AData.Environment.SsrEnabled = XB.AData.SSR;
    }

    public static void SetApplicationDefaults() {
        XB.AData.Controller = false;
        XB.AData.FullScreen = false;
        XB.AData.Resolution = XB.AData.BaseResolution;
        XB.AData.Fps        = 60;
        XB.AData.ShowFps    = false;
        UpdateFov(_fovDef);
        XB.AData.CamXSens   = 2.0f;
        XB.AData.CamYSens   = 2.0f;
        XB.AData.Volume     = -30.0f;
        XB.AData.VSync      = false;
        XB.AData.Language   = "en";
        Godot.TranslationServer.SetLocale(XB.AData.Language);
    }

    public static void SetPresetSettings(XB.SettingsPreset preset) {
        switch (preset) {
            case XB.SettingsPreset.Minimum: {
                XB.AData.MSAASel        = "DISABLED";
                XB.AData.SSAASel        = "DISABLED";
                XB.AData.TAA            = false;
                XB.AData.Debanding      = false;
                XB.AData.ShadowSize     = 512;
                XB.AData.ShadowFilter   = "SHADOWF0";
                XB.AData.ShadowDistance = 50;
                XB.AData.LODSel         = 4.0f;
                XB.AData.SSAOSel        = "SSAO0";
                XB.AData.SSAOHalf       = true;
                XB.AData.SSILSel        = "SSIL0";
                XB.AData.SSILHalf       = true;
                XB.AData.SSR            = false;
            } break;
            case XB.SettingsPreset.Default: {
                XB.AData.MSAASel        = "MSAA4";
                XB.AData.SSAASel        = "FXAA";
                XB.AData.TAA            = false;
                XB.AData.Debanding      = true;
                XB.AData.ShadowSize     = 2048;
                XB.AData.ShadowFilter   = "SHADOWF3";
                XB.AData.ShadowDistance = 150;
                XB.AData.LODSel         = 2.0f;
                XB.AData.SSAOSel        = "SSAO2";
                XB.AData.SSAOHalf       = true;
                XB.AData.SSILSel        = "SSIL2";
                XB.AData.SSILHalf       = true;
                XB.AData.SSR            = true;
            } break;
            case XB.SettingsPreset.Maximum: {
                XB.AData.MSAASel        = "MSAA8";
                XB.AData.SSAASel        = "FXAA";
                XB.AData.TAA            = true;
                XB.AData.Debanding      = true;
                XB.AData.ShadowSize     = 4096;
                XB.AData.ShadowFilter   = "SHADOWF5";
                XB.AData.ShadowDistance = 300;
                XB.AData.LODSel         = 1.0f;
                XB.AData.SSAOSel        = "SSAO3";
                XB.AData.SSAOHalf       = false;
                XB.AData.SSILSel        = "SSIL3";
                XB.AData.SSILHalf       = false;
                XB.AData.SSR            = true;
            } break;
        }
    }

    public static void UpdateAudio() {
        Godot.AudioServer.SetBusVolumeDb(0, XB.AData.Volume); // 0 is master bus
    }

    public static void UpdateLanguage() {
        Godot.TranslationServer.SetLocale(XB.AData.Language);
    }
}
} // namespace close
