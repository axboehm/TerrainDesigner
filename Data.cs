#define XBDEBUG
namespace XB { // namespace open
using SysCG = System.Collections.Generic;
public enum SettingsPreset {
    Minimum,
    Default,
    Maximum,
}

// layer is which layers the object is in, mask is which layers an object scans for collisions
public struct LayerMasks {                //24--20--16--12--8---4---  
    public static uint EmptyMask        = 0b000000000000000000000000;
    public static uint AimMask          = 0b000000000000000001000001;
    public static uint CamMask          = 0b000000000000000000000011;
    public static uint EnvironmentLayer = 0b000000000000000000000001;
    public static uint EnvironmentMask  = 0b000000000000000000000001;
    public static uint MovementMask     = 0b100000000000000100000000;
    public static uint MovementLayer    = 0b000000000000000010000000;
    public static uint PlayerMask       = 0b000000000000000110000001;
    public static uint PlayerLayer      = 0b100000000000000000000000;
    public static uint SphereLayer      = 0b000000000000010000000010;
    public static uint SphereMask       = 0b000000000000010000000000;
                                          //24--20--16--12--8---4---
}

public struct Constants {
    public const float Tau     = 6.28318530718f;
    public const float Pi      = 3.14159265359f;
    public const float PiHalf  = 1.57079632679f;
    public const float Sqrt2   = 1.41421356237f;
    public const float Deg2Rad = 0.01745329251f;
    public const float Rad2Deg = 57.2957795131f;
    public const float Epsilon = 0.01f; // for floating point comparisons (empiric)
}

//NOTE[ALEX]: struct name intentionally kept short
public struct Col {
    public static Godot.Color Red     = new Godot.Color(1.0f, 0.0f, 0.0f, 1.0f);
    public static Godot.Color Green   = new Godot.Color(0.0f, 1.0f, 0.0f, 1.0f);
    public static Godot.Color Blue    = new Godot.Color(0.0f, 0.0f, 1.0f, 1.0f);
    public static Godot.Color Black   = new Godot.Color(0.0f, 0.0f, 0.0f, 1.0f);
    public static Godot.Color White   = new Godot.Color(1.0f, 1.0f, 1.0f, 1.0f);
    public static Godot.Color Transp  = new Godot.Color(0.0f, 0.0f, 0.0f, 0.0f);
    // UI colors
    public static Godot.Color MPlayer = new Godot.Color(1.0f, 0.22f, 0.0f, 1.0f);
    public static Godot.Color MSphere = new Godot.Color(0.0f, 0.22f, 1.0f, 1.0f);
    public static Godot.Color Hl      = new Godot.Color(0.6f, 1.0f, 0.6f, 1.0f);
    public static Godot.Color Outline = new Godot.Color(0.0f, 0.0f, 0.0f, 0.6f);
    public static Godot.Color LinkBri = new Godot.Color(1.0f, 0.88f, 0.0f, 1.0f);
    public static Godot.Color LinkDim = new Godot.Color(1.0f, 0.63f, 0.0f, 1.0f);
    public static Godot.Color Act     = new Godot.Color(0.87f, 0.87f, 0.87f, 1.0f);
    public static Godot.Color InAct   = new Godot.Color(0.2f, 0.2f, 0.2f, 0.3f);
    public static Godot.Color Msg     = new Godot.Color(0.2f, 0.2f, 0.2f, 1.0f);
    public static Godot.Color MsgFade = new Godot.Color(0.1f, 0.1f, 0.1f, 0.0f);
    // sphere colors
    public static Godot.Color SpHl     = new Godot.Color(0.6f, 1.0f, 0.6f, 1.0f);
    public static Godot.Color SpHlLink = new Godot.Color(1.0f, 0.68f, 0.0f, 1.0f);
    public static Godot.Color SpLink   = new Godot.Color(1.0f, 0.43f, 0.0f, 1.0f);
}

public struct ScenePaths {
    public static string Player        = "res://assets/player/playerController.tscn";
    public static string ButtonAudio   = "res://assets/audio/soundButtonPress.tscn";
    public static string Sphere        = "res://assets/sphere/sphere.tscn";
    public static string TerrainShader = "res://code/shaders/terrain.gdshader";
    public static string TSkirtShader  = "res://code/shaders/terrainSkirt.gdshader";
    public static string BlueNoiseTex  = "res://materials/data/blueNoise64px.png";
    public static string BlockTexture  = "res://materials/data/blockTexture2048.png";
    public static string Terrain1CATex = "res://materials/data/asteroidStone1_CA.png";
    public static string Terrain1RMTex = "res://materials/data/asteroidStone1_RM.png";
    public static string Terrain1NTex  = "res://materials/data/asteroidStone1_N.png";
    public static string Terrain1HTex  = "res://materials/data/asteroidStone1_HEIGHT.png";
    public static string Terrain2CATex = "res://materials/data/grdAsteroid1_C.png";
    public static string Terrain2RMTex = "res://materials/data/grdAsteroid1_RM.png";
    public static string Terrain2NTex  = "res://materials/data/grdAsteroid1_N.png";
    public static string Terrain2HTex  = "res://materials/data/grdAsteroid1_HEIGHT.png";
    public static string FontLibMono   = "res://assets/ui/fonts/LiberationMono-Regular.ttf";
}

public class WorldData {
    public static Godot.Image    ImgMiniMap;
    public static float          LowestPoint  = -1.0f;  // lowest y coordinate in world
    public static float          HighestPoint = +1.0f;  // highest y coordinate in world
    public static float          LowHighExtra = 1.0f;   // buffer amount for high/low updating
    public static float          KillPlane    = -4096.0f; // fallback for the player falling off
    public static float          SphereEdgeLength    = 64.0f;
    public static int            DamSegmentDivisions = 16;
    public static Godot.Vector2  WorldDim;              // world dimensions in meters
    public static Godot.Vector2I WorldVerts;
    public static float          WorldRes            = 0;    // subdivisions per meter
    public static float          CollisionRes        = 1.0f;
    public static float          TerrainTileMinimum  = 8.0f;
    public static float          ColliderSizeMult    = 3.0f; // multiplied with TerrainTileMinimum
    public static int            TerrainDivisionsMax = 6;
    public static float[,]       TerrainHeights;        // height value for each vertex
    public static float[,]       TerrainHeightsMod;     // stores calculated values to add to terrain

    public static float                BlockStrength = 0.6f;
    public static int                  NoiseRes      = 256; // resolution of large scale noise
                                                            // texture for texture bombing
    public static float                AlbedoMult    = 0.6f;
    public static float                Mat1UVScale   = 1.0f/4.0f; // empirical value
    public static float                Mat2UVScale   = 1.0f/4.0f; // empirical value
    public static float                NoisePScale   = 0.1f;  // perlin noise for bombing
    public static float                BlockUVScale  = 1.0f/(2.0f*10.0f); 
                                                            // block texture has 2x2 large squares
                                                            // with 10 subdivisions each per tile
    public static float                BlendDepth    = 0.2f; // height blending edge
    public static Godot.NoiseTexture2D NoiseBombing;
    public static Godot.Texture        BlockTex;
    public static Godot.Texture        Terrain1CATex;
    public static Godot.Texture        Terrain1RMTex;
    public static Godot.Texture        Terrain1NTex;
    public static Godot.Texture        Terrain1HTex;
    public static Godot.Texture        Terrain2CATex;
    public static Godot.Texture        Terrain2RMTex;
    public static Godot.Texture        Terrain2NTex;
    public static Godot.Texture        Terrain2HTex;


    //TODO[ALEX]: z direction edges are off, x direction is correct
    public static void InitializeTerrainMesh(int expX, int expZ) {
#if XBDEBUG
        var debug = new XB.DebugTimedBlock(XB.D.WorldDataInitializeTerrainMesh);
#endif

        float sizeX = System.MathF.Pow(2, expX);
        float sizeZ = System.MathF.Pow(2, expZ);

        var fastNoise = new Godot.FastNoiseLite();
            fastNoise.NoiseType = Godot.FastNoiseLite.NoiseTypeEnum.Perlin;
        NoiseBombing = new Godot.NoiseTexture2D();
        NoiseBombing.Noise           = fastNoise;
        NoiseBombing.Height          = NoiseRes;
        NoiseBombing.Width           = NoiseRes;
        NoiseBombing.Normalize       = true;
        NoiseBombing.Seamless        = true;
        NoiseBombing.GenerateMipmaps = true;
        Terrain1CATex = Godot.ResourceLoader.Load<Godot.Texture>(XB.ScenePaths.Terrain1CATex);
        Terrain1RMTex = Godot.ResourceLoader.Load<Godot.Texture>(XB.ScenePaths.Terrain1RMTex);
        Terrain1NTex  = Godot.ResourceLoader.Load<Godot.Texture>(XB.ScenePaths.Terrain1NTex);
        Terrain1HTex  = Godot.ResourceLoader.Load<Godot.Texture>(XB.ScenePaths.Terrain1HTex);
        Terrain2CATex = Godot.ResourceLoader.Load<Godot.Texture>(XB.ScenePaths.Terrain2CATex);
        Terrain2RMTex = Godot.ResourceLoader.Load<Godot.Texture>(XB.ScenePaths.Terrain2RMTex);
        Terrain2NTex  = Godot.ResourceLoader.Load<Godot.Texture>(XB.ScenePaths.Terrain2NTex);
        Terrain2HTex  = Godot.ResourceLoader.Load<Godot.Texture>(XB.ScenePaths.Terrain2HTex);

        WorldRes          = 8.0f;
        WorldDim          = new Godot.Vector2(sizeX, sizeZ);
        WorldVerts        = new Godot.Vector2I((int)(sizeX*WorldRes) +1, (int)(sizeZ*WorldRes) +1);
        TerrainHeights    = new float[WorldVerts.X, WorldVerts.Y];
        TerrainHeightsMod = new float[WorldVerts.X, WorldVerts.Y];

        ImgMiniMap = Godot.Image.Create((int)(sizeX*WorldRes), (int)(sizeZ*WorldRes),
                                              false, Godot.Image.Format.L8           );
        ImgMiniMap.Fill(XB.Col.Black);

#if XBDEBUG
        debug.End();
#endif 
    }

    public static void GenerateRandomTerrain() {
#if XBDEBUG
        var debug = new XB.DebugTimedBlock(XB.D.WorldDataGenerateRandomTerrain);
#endif

        XB.Terrain.Flat(ref TerrainHeights, WorldVerts.X, WorldVerts.Y, 0.0f); // initialize to flat

        XB.Terrain.SetTerrainParameters(18.0f, 0.0174f, 0.0f, 0.0f, 8, 0.9f, 2.2f, 7.5f);
        XB.Terrain.FBM(WorldVerts.X, WorldVerts.Y, WorldDim.X, WorldDim.Y);
        XB.Terrain.HeightReplace(ref TerrainHeights, ref TerrainHeightsMod, WorldVerts.X, WorldVerts.Y);

        Godot.GD.Print("Generate Terrain: LP: " + LowestPoint + ", HP: " + HighestPoint);

#if XBDEBUG
        debug.End();
#endif 
    }

    public static void UpdateTerrain(bool reInitialize) {
#if XBDEBUG
        var debug = new XB.DebugTimedBlock(XB.D.WorldDataUpdateTerrain);
#endif

        XB.Terrain.UpdateHeightMap(ref TerrainHeights, LowestPoint, HighestPoint, ref ImgMiniMap);
        XB.PController.Hud.UpdateMiniMap(LowestPoint, HighestPoint);

        if (reInitialize) {
            XB.ManagerTerrain.InitializeQuadTree(WorldDim.X, WorldDim.Y, WorldRes,
                                                 CollisionRes, ColliderSizeMult*TerrainTileMinimum,
                                                 TerrainTileMinimum, TerrainDivisionsMax           );
        } else {
            XB.ManagerTerrain.ResampleMeshes(LowestPoint, HighestPoint, ref ImgMiniMap);
        }

        XB.ManagerTerrain.UpdateCollisionTiles(LowestPoint, HighestPoint, ref ImgMiniMap);

#if XBDEBUG
        debug.End();
#endif 
    }

    // takes angle in degrees
    public static void ApplySphereCone(Godot.Vector3 pos, float radius, float angle) {
#if XBDEBUG
        var debug = new XB.DebugTimedBlock(XB.D.WorldDataApplySphereCone);
#endif

        // Godot.GD.Print("ApplySphereCone with p: " + pos + ", r: " + radius + ", a: " + angle);
        
        XB.Terrain.Cone(ref TerrainHeightsMod, WorldVerts.X, WorldVerts.Y,
                        WorldDim.X, WorldDim.Y, pos.X, pos.Z,
                        radius, angle*XB.Constants.Deg2Rad, pos.Y, XB.Direction.Up);
        XB.Terrain.HeightMax(ref TerrainHeights, ref TerrainHeightsMod, WorldVerts.X, WorldVerts.Y);
        XB.Terrain.Cone(ref TerrainHeightsMod, WorldVerts.X, WorldVerts.Y,
                        WorldDim.X, WorldDim.Y, pos.X, pos.Z,
                        radius, angle*XB.Constants.Deg2Rad, pos.Y, XB.Direction.Down);
        XB.Terrain.HeightMin(ref TerrainHeights, ref TerrainHeightsMod, WorldVerts.X, WorldVerts.Y);

#if XBDEBUG
        debug.End();
#endif 
    }

    // takes angle in degrees
    public static void ApplyDamSegment(Godot.Vector3 pos1, float radius1, float angle1,
                                       Godot.Vector3 pos2, float radius2, float angle2 ) {
#if XBDEBUG
        var debug = new XB.DebugTimedBlock(XB.D.WorldDataApplyDamSegment);
#endif

        Godot.GD.Print("ApplyDamSegment with p1: " + pos1 + ", r1: " + radius1 + ", a1: " + angle1
                       + ", p2: " + pos2 + ", r2: " + radius2 + ", a2: " + angle2                 );
        
        XB.Terrain.UnevenCapsule(ref TerrainHeightsMod, WorldVerts.X, WorldVerts.Y,
                                 WorldDim.X, WorldDim.Y,
                                 pos1.X, pos1.Z, radius1, angle1*XB.Constants.Deg2Rad, pos1.Y,
                                 pos2.X, pos2.Z, radius2, angle2*XB.Constants.Deg2Rad, pos2.Y,
                                 XB.Direction.Up                                              );
        XB.Terrain.HeightMax(ref TerrainHeights, ref TerrainHeightsMod, WorldVerts.X, WorldVerts.Y);
        XB.Terrain.UnevenCapsule(ref TerrainHeightsMod, WorldVerts.X, WorldVerts.Y,
                                 WorldDim.X, WorldDim.Y,
                                 pos1.X, pos1.Z, radius1, angle1*XB.Constants.Deg2Rad, pos1.Y,
                                 pos2.X, pos2.Z, radius2, angle2*XB.Constants.Deg2Rad, pos2.Y,
                                 XB.Direction.Down                                            );
        XB.Terrain.HeightMin(ref TerrainHeights, ref TerrainHeightsMod, WorldVerts.X, WorldVerts.Y);

#if XBDEBUG
        debug.End();
#endif 
    }
}

public class AData {
    public static XB.Input                 Input;
    public static Godot.DirectionalLight3D MainLight;
    public static Godot.Environment        Environment;
    public static Godot.Node               MainRoot;
    public static Godot.Node               TR = new Godot.Node(); //NOTE[ALEX]: necessary to use Tr()

    public static uint       InitialSeed    = 0;        // random seed on application startup
    public static float      CamCollDist    = 0.2f;     // distance from camera to colliders
    public static ulong      SetCodeR       = 0;
    public static ulong      SetCodeL       = 0;
    public static int        SetCodeLengthR = 45;
    public static int        SetCodeLengthL = 35;
    public static bool       Controller     = false;
    public static int        Fps            = 60;
    public static int[]      FpsOptions     = new int[] {30, 60, 120, 0};
    public static bool       ShowFps        = false;
    public static float      FovDef         = 0.0f; // camera fov when not aiming (in mm)
    public static float      FovAim         = 0.0f; // when aiming
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
    public static int        ShadowDistance = 256;
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
    public static int        BaseResX       = 1920;
    public static int        BaseResY       = 1080;
    public static string     Resolution     = BaseResolution;
    public static string[]   ResStrings     = new string[] {"3840x2160", "2560x1440", "2048x1152",
                                                            "1920x1080", "1280x720"};
    public static SysCG.Dictionary<string, Godot.Vector2I> Resolutions
        = new SysCG.Dictionary<string, Godot.Vector2I>() {
            {ResStrings[0], new Godot.Vector2I(3840, 2160)},
            {ResStrings[1], new Godot.Vector2I(2560, 1440)},
            {ResStrings[2], new Godot.Vector2I(2048, 1152)},
            {ResStrings[3], new Godot.Vector2I(1920, 1080)},
            {ResStrings[4], new Godot.Vector2I(1280, 720)},
        };
    public static SysCG.Dictionary<string, XB.SettingsPreset> Presets
        = new SysCG.Dictionary<string, XB.SettingsPreset>() {
            {"Lowest",  XB.SettingsPreset.Minimum},
            {"Default", XB.SettingsPreset.Default},
            {"Highest", XB.SettingsPreset.Maximum},
        };
}

public class PersistData {
    private static float _fovAimM = 1.25f;
    private static float _fovDef  = 28.0f;

    public static void UpdateFov(float value = 28.0f) {
        XB.AData.FovDef     = value;
        XB.AData.FovAim     = value*_fovAimM;
        XB.AData.FovAim     = XB.Utils.ClampF(XB.AData.FovAim, XB.AData.FovMin, XB.AData.FovMax);
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

        if (XB.AData.ShowFps) XB.PController.Hud.FpsVisible = true;
        else                  XB.PController.Hud.FpsVisible = false;

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
