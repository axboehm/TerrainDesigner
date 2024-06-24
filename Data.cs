//#define XBDEBUG
namespace XB { // namespace open
using SysCG = System.Collections.Generic;
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
    public static string Player       = "res://assets/player/playerController.tscn";
}

public struct WorldData {
}

public class AData {
    public static XB.Input          Input;
    // public static XB.DebugHUD       DHud;
    public static Godot.DirectionalLight3D MainLight;
    public static Godot.Environment Environment;
    public static Godot.Node        MainRoot;
    public static Godot.Node        TR = new Godot.Node(); //NOTE[ALEX]: necessary to use Tr()
    public static float      CamCollDist    = 0.2f;     // distance from camera to colliders
    public static bool       Controller     = false;
    public static int        Fps            = 60;
    public static bool       ShowFps        = false;
    public static bool       Crosshairs     = true;
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
    // public static string[]   Languages      = new string[] {"en", "de"};
    // public static string     Language       = "en";
    // public static bool       VoxelGI        = false;
    // public static bool       VoxelGIHQ      = false;
    // public static bool       VoxelGIHalf    = false;
    // public static bool       VSync          = false;
    // public static string[]   MSAA           = new string[] {"DISABLED", "MSAA2", "MSAA4", "MSAA8"};
    // public static string     MSAASel        = "DISABLED";
    // public static string[]   SSAA           = new string[] {"DISABLED", "FXAA"};
    // public static string     SSAASel        = "DISABLED";
    // public static bool       TAA            = false;
    // public static bool       Debanding      = false;
    // public static int[]      ShadowSizes    = new int[] {512, 1024, 2048, 4096};
    // public static int        ShadowSize     = 512;
    // public static string[]   ShadowFilters  = new string[] {"SHADOWF0", "SHADOWF1", "SHADOWF2",
    //                                                         "SHADOWF3", "SHADOWF4", "SHADOWF5"};
    // public static string     ShadowFilter   = "SHADOWF0";
    // public static int        ShadowDistance = 300;
    // public static float[]    LOD            = new float[] {4.0f, 2.0f, 1.0f};
    // public static float      LODSel         = 1.0f;
    // public static string[]   SSAO           = new string[] {"SSAO0", "SSAO1", "SSAO2", "SSAO3"};
    // public static string     SSAOSel        = "SSAO0";
    // public static bool       SSAOHalf       = false;
    // public static string[]   SSIL           = new string[] {"SSIL0", "SSIL1", "SSIL2", "SSIL3"};
    // public static string     SSILSel        = "SSIL0";
    // public static bool       SSILHalf       = false;
    // public static bool       SSR            = false;
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
    private static float _fovAimM      = 1.25f;
    private static float _fovZM        = 2.25f;

    public static void UpdateFov(float value = 28.0f) {
        XB.AData.FovDef     = value;
        XB.AData.FovAim     = value*_fovAimM;
        XB.AData.FovAim     = XB.Utils.ClampF(XB.AData.FovAim, XB.AData.FovMin, XB.AData.FovMax);
        XB.AData.FovZoom    = value*_fovZM;
        XB.AData.FovZoom    = XB.Utils.ClampF(XB.AData.FovZoom, XB.AData.FovMin, XB.AData.FovMax);
        XB.AData.CamMaxDist = value*(1.0f/28.0f)*4.2f;
        XB.AData.CamAimDist = value*(1.0f/28.0f)*1.0f;
    }
}
} // namespace close
