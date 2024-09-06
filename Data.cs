#define XBDEBUG
namespace XB { // namespace open
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
    // terrain colors
    public static Godot.Color Fog     = new Godot.Color(0.686f, 0.871f, 0.878f, 1.0f);
    // UI colors
    public static Godot.Color MPlayer = new Godot.Color(0.22f, 0.44f, 1.0f, 1.0f);
    public static Godot.Color MSphere = new Godot.Color(0.0f, 0.22f, 1.0f, 1.0f);
    public static Godot.Color Hl      = new Godot.Color(0.6f, 1.0f, 0.6f, 1.0f);
    public static Godot.Color Outline = new Godot.Color(0.0f, 0.0f, 0.0f, 0.6f);
    public static Godot.Color LinkBri = new Godot.Color(1.0f, 0.88f, 0.0f, 1.0f);
    public static Godot.Color LinkDim = new Godot.Color(1.0f, 0.63f, 0.0f, 1.0f);
    public static Godot.Color Act     = new Godot.Color(0.87f, 0.87f, 0.87f, 1.0f);
    public static Godot.Color InAct   = new Godot.Color(0.2f, 0.2f, 0.2f, 0.3f);
    public static Godot.Color Msg     = new Godot.Color(0.2f, 0.2f, 0.2f, 1.0f);
    public static Godot.Color MsgFade = new Godot.Color(0.1f, 0.1f, 0.1f, 0.0f);
    public static Godot.Color BG      = new Godot.Color(0.0f, 0.0f, 0.0f, 0.34f);
    // sphere colors
    public static Godot.Color SpHl     = new Godot.Color(0.6f, 1.0f, 0.6f, 1.0f);
    public static Godot.Color SpHlLink = new Godot.Color(1.0f, 0.68f, 0.0f, 1.0f);
    public static Godot.Color SpLink   = new Godot.Color(1.0f, 0.43f, 0.0f, 1.0f);
    // cone dam colors
    public static Godot.Color ConeTI = new Godot.Color(0.0f, 0.01f, 0.03f, 1.0f);
    public static Godot.Color ConeTO = new Godot.Color(0.0f, 0.55f, 0.66f, 1.0f);
    public static Godot.Color ConeBU = new Godot.Color(0.84f, 0.4f, 0.0f, 1.0f);
    public static Godot.Color ConeBL = new Godot.Color(0.084f, 0.04f, 0.0f, 1.0f);
    public static Godot.Color DamTI  = new Godot.Color(0.0f, 0.01f, 0.03f, 1.0f);
    public static Godot.Color DamTO  = new Godot.Color(0.0f, 0.55f, 0.66f, 1.0f);
    public static Godot.Color DamBU  = new Godot.Color(0.84f, 0.4f, 0.0f, 1.0f);
    public static Godot.Color DamBL  = new Godot.Color(0.084f, 0.04f, 0.0f, 1.0f);
}

public struct ResourcePaths {
    public static string Player           = "res://assets/player/playerController.tscn";
    public static string FootStep01       = "res://assets/audio/footStep01.tscn";
    public static string FootStep02       = "res://assets/audio/footStep02.tscn";
    public static string FootStep03       = "res://assets/audio/footStep03.tscn";
    public static string FootStep04       = "res://assets/audio/footStep04.tscn";
    public static string FootStep05       = "res://assets/audio/footStep05.tscn";
    public static string FootStep06       = "res://assets/audio/footStep06.tscn";
    public static string ButtonAudio      = "res://assets/audio/soundButtonPress.tscn";
    public static string Sphere           = "res://assets/sphere/sphere.tscn";
    public static string CrosshairsTex    = "res://assets/ui/crosshairsDot.png";
    public static string TerrainShader    = "res://code/shaders/terrain.gdshader";
    public static string TSkirtShader     = "res://code/shaders/terrainSkirt.gdshader";
    public static string BlueNoiseTex     = "res://materials/data/blueNoise64px.png";
    public static string BlockTexture     = "res://materials/data/blockTexture2048.png";
    public static string Terrain1CATex    = "res://materials/data/stoneVeinsMatStone2_CA.png";
    public static string Terrain1RMTex    = "res://materials/data/stoneVeinsMatStone2_RM.png";
    public static string Terrain1NTex     = "res://materials/data/stoneVeinsMatStone2_N.png";
    public static string Terrain1HTex     = "res://materials/data/stoneVeinsMatStone2_HEIGHT.png";
    public static string Terrain2CATex    = "res://materials/data/stoneVeinsFlat_CA.png";
    public static string Terrain2RMTex    = "res://materials/data/stoneVeinsFlat_RM.png";
    public static string Terrain2NTex     = "res://materials/data/stoneVeinsFlat_N.png";
    public static string Terrain2HTex     = "res://materials/data/stoneVeinsFlat_HEIGHT.png";
    public static string Terrain3CATex    = "res://materials/data/grdAsteroid1_C.png";
    public static string Terrain3RMTex    = "res://materials/data/grdAsteroid1_RM.png";
    public static string Terrain3NTex     = "res://materials/data/grdAsteroid1_N.png";
    public static string Terrain3HTex     = "res://materials/data/grdAsteroid1_HEIGHT.png";
    public static string Terrain4CATex    = "res://materials/data/stoneVeinsGravel_CA.png";
    public static string Terrain4RMTex    = "res://materials/data/stoneVeinsGravel_RM.png";
    public static string Terrain4NTex     = "res://materials/data/stoneVeinsGravel_N.png";
    public static string Terrain4HTex     = "res://materials/data/stoneVeinsGravel_HEIGHT.png";
    public static string TerrainCShiftTex = "res://materials/data/terrainCol.png";
    public static string FontLibMono      = "res://assets/ui/fonts/LiberationMono-Regular.ttf";
    public static string SpShellShader    = "res://code/shaders/sphereShell.gdshader";
    public static string SpScreenShader   = "res://code/shaders/sphereScreen.gdshader";
    public static string SpScrGhostShader = "res://code/shaders/sphereScreenBehind.gdshader";
    public static string SpShellCATex     = "res://assets/sphere/data/sphereLP_C.png";
    public static string SpShellRMTex     = "res://assets/sphere/data/sphereLP_RM.png";
    public static string SpShellNTex      = "res://assets/sphere/data/sphereLP_NOpenGL.png";
    public static string SpShellETex      = "res://assets/sphere/data/sphereLP_E.png";
    public static string SpScreenCATex    = "res://assets/sphere/data/sphereLP_C_1.png";
    public static string SpScreenRMTex    = "res://assets/sphere/data/sphereLP_RM_3.png";
    public static string SpScreenNTex     = "res://assets/sphere/data/sphereLP_NOpenGL_2.png";
    public static string SpScreenETex     = "res://assets/sphere/data/sphereLP_E_4.png";
    public static string SpEMaskTex       = "res://assets/sphere/data/sphereEmissionMask.png";
    public static string ConeDamShader    = "res://code/shaders/spConeDam.gdshader";
    public static string ConeDamUShader   = "res://code/shaders/spConeDamU.gdshader";
    public static string MiniMapOShader   = "res://code/shaders/miniMapO.gdshader";
    public static string LinkingShader    = "res://code/shaders/linkingOverlay.gdshader";
}

// holds loaded resources for shaders like textures
public struct Resources {
    public static Godot.Texture SpShellCA;
    public static Godot.Texture SpShellRM;
    public static Godot.Texture SpShellN;
    public static Godot.Texture SpShellE;
    public static Godot.Texture SpScreenCA;
    public static Godot.Texture SpScreenRM;
    public static Godot.Texture SpScreenN;
    public static Godot.Texture SpScreenE;
    public static Godot.Texture SpEMask;

    public static int                  NoiseRes = 1024;
    public static Godot.NoiseTexture2D NoiseBombing;
    public static Godot.NoiseTexture2D NoiseModulation;
    public static Godot.Texture        BlockTex;
    public static Godot.Texture        Terrain1CATex;
    public static Godot.Texture        Terrain1RMTex;
    public static Godot.Texture        Terrain1NTex;
    public static Godot.Texture        Terrain1HTex;
    public static Godot.Texture        Terrain2CATex;
    public static Godot.Texture        Terrain2RMTex;
    public static Godot.Texture        Terrain2NTex;
    public static Godot.Texture        Terrain2HTex;
    public static Godot.Texture        Terrain3CATex;
    public static Godot.Texture        Terrain3RMTex;
    public static Godot.Texture        Terrain3NTex;
    public static Godot.Texture        Terrain3HTex;
    public static Godot.Texture        Terrain4CATex;
    public static Godot.Texture        Terrain4RMTex;
    public static Godot.Texture        Terrain4NTex;
    public static Godot.Texture        Terrain4HTex;
    public static Godot.Texture        ColorShiftTex;

    public static void InitializeTerrainTextures() {
        var fastNoiseB           = new Godot.FastNoiseLite();
            fastNoiseB.NoiseType = Godot.FastNoiseLite.NoiseTypeEnum.Perlin;
        NoiseBombing = new Godot.NoiseTexture2D();
        NoiseBombing.Noise           = fastNoiseB;
        NoiseBombing.Height          = NoiseRes;
        NoiseBombing.Width           = NoiseRes;
        NoiseBombing.Normalize       = true;
        NoiseBombing.Seamless        = true;
        NoiseBombing.GenerateMipmaps = true;
        var fastNoiseM           = new Godot.FastNoiseLite();
            fastNoiseM.NoiseType = Godot.FastNoiseLite.NoiseTypeEnum.Cellular;
            fastNoiseM.CellularDistanceFunction = 
                Godot.FastNoiseLite.CellularDistanceFunctionEnum.EuclideanSquared;
            fastNoiseM.Frequency      = 0.03f;
            fastNoiseM.FractalOctaves = 5;
        NoiseModulation = new Godot.NoiseTexture2D();
        NoiseModulation.Noise           = fastNoiseM;
        NoiseModulation.Height          = NoiseRes;
        NoiseModulation.Width           = NoiseRes;
        NoiseModulation.Normalize       = true;
        NoiseModulation.Seamless        = true;
        NoiseModulation.GenerateMipmaps = true;
        BlockTex      = Godot.ResourceLoader.Load<Godot.Texture>(XB.ResourcePaths.BlockTexture);
        Terrain1CATex = Godot.ResourceLoader.Load<Godot.Texture>(XB.ResourcePaths.Terrain1CATex);
        Terrain1RMTex = Godot.ResourceLoader.Load<Godot.Texture>(XB.ResourcePaths.Terrain1RMTex);
        Terrain1NTex  = Godot.ResourceLoader.Load<Godot.Texture>(XB.ResourcePaths.Terrain1NTex);
        Terrain1HTex  = Godot.ResourceLoader.Load<Godot.Texture>(XB.ResourcePaths.Terrain1HTex);
        Terrain2CATex = Godot.ResourceLoader.Load<Godot.Texture>(XB.ResourcePaths.Terrain2CATex);
        Terrain2RMTex = Godot.ResourceLoader.Load<Godot.Texture>(XB.ResourcePaths.Terrain2RMTex);
        Terrain2NTex  = Godot.ResourceLoader.Load<Godot.Texture>(XB.ResourcePaths.Terrain2NTex);
        Terrain2HTex  = Godot.ResourceLoader.Load<Godot.Texture>(XB.ResourcePaths.Terrain2HTex);
        Terrain3CATex = Godot.ResourceLoader.Load<Godot.Texture>(XB.ResourcePaths.Terrain3CATex);
        Terrain3RMTex = Godot.ResourceLoader.Load<Godot.Texture>(XB.ResourcePaths.Terrain3RMTex);
        Terrain3NTex  = Godot.ResourceLoader.Load<Godot.Texture>(XB.ResourcePaths.Terrain3NTex);
        Terrain3HTex  = Godot.ResourceLoader.Load<Godot.Texture>(XB.ResourcePaths.Terrain3HTex);
        Terrain4CATex = Godot.ResourceLoader.Load<Godot.Texture>(XB.ResourcePaths.Terrain4CATex);
        Terrain4RMTex = Godot.ResourceLoader.Load<Godot.Texture>(XB.ResourcePaths.Terrain4RMTex);
        Terrain4NTex  = Godot.ResourceLoader.Load<Godot.Texture>(XB.ResourcePaths.Terrain4NTex);
        Terrain4HTex  = Godot.ResourceLoader.Load<Godot.Texture>(XB.ResourcePaths.Terrain4HTex);
        ColorShiftTex = Godot.ResourceLoader.Load<Godot.Texture>(XB.ResourcePaths.TerrainCShiftTex);
    }

    public static void InitializeSphereTextures() {
        SpShellCA  = Godot.ResourceLoader.Load<Godot.Texture>(XB.ResourcePaths.SpShellCATex);
        SpShellRM  = Godot.ResourceLoader.Load<Godot.Texture>(XB.ResourcePaths.SpShellRMTex);
        SpShellN   = Godot.ResourceLoader.Load<Godot.Texture>(XB.ResourcePaths.SpShellNTex);
        SpShellE   = Godot.ResourceLoader.Load<Godot.Texture>(XB.ResourcePaths.SpShellETex);
        SpScreenCA = Godot.ResourceLoader.Load<Godot.Texture>(XB.ResourcePaths.SpScreenCATex);
        SpScreenRM = Godot.ResourceLoader.Load<Godot.Texture>(XB.ResourcePaths.SpScreenRMTex);
        SpScreenN  = Godot.ResourceLoader.Load<Godot.Texture>(XB.ResourcePaths.SpScreenNTex);
        SpScreenE  = Godot.ResourceLoader.Load<Godot.Texture>(XB.ResourcePaths.SpScreenETex);
        SpEMask    = Godot.ResourceLoader.Load<Godot.Texture>(XB.ResourcePaths.SpEMaskTex);
    }
}

// WData holds variables and values for world data, so everything related to the heightmap
// and terrain generation and terrain materials
// terrain/heightmap related functions are also included
public class WData {
    public static Godot.Image    ImgMiniMap;
    public static Godot.Image    ImgPointiness;
    public static Godot.ImageTexture TexPointiness;
    public static float          LowestPoint  = -1.0f;    // lowest y coordinate in world
    public static float          HighestPoint = +1.0f;    // highest y coordinate in world
    public static float          LowHighExtra = 1.0f;     // buffer amount for high/low updating
    public static float          KillPlane    = -4096.0f; // fallback for the player falling off
    public static float          SphereEdgeLength    = 64.0f;
    public static int            DamSegmentDivisions = 16;
    public static Godot.Vector2  WorldDim;                   // world dimensions in meters
    public static Godot.Vector2I WorldVerts;
    public static float          WorldRes            = 0;    // subdivisions per meter
    public static float          CollisionRes        = 1.0f;
    public static float          TerrainTileMinimum  = 8.0f;
    public static float          ColliderSizeMult    = 3.0f; // multiplied with TerrainTileMinimum
    public static int            TerrainDivisionsMax = 6;
    public static float[,]       TerrainHeights;        // height value for each vertex
    public static float[,]       TerrainHeightsMod;     // stores calculated values to add to terrain

    public static float GenHeightMin = 0.0f;
    public static float GenHeightMax = 100.0f;
    public static float GenHeightDef = 18.0f;
    public static float GenScaleMin  = 0.0001f;
    public static float GenScaleMax  = 0.1f;
    public static float GenScaleDef  = 0.0174f;
    public static float GenOffXMin   = 0.0f;
    public static float GenOffXMax   = 5.0f;
    public static float GenOffXDef   = 0.46f;
    public static float GenOffZMin   = 0.0f;
    public static float GenOffZMax   = 5.0f;
    public static float GenOffZDef   = 0.33f;
    public static int   GenOctMin    = 1;
    public static int   GenOctMax    = 10;
    public static int   GenOctDef    = 6;
    public static float GenPersMin   = 0.0f;
    public static float GenPersMax   = 5.0f;
    public static float GenPersDef   = 1.2f;
    public static float GenLacMin    = 0.0f;
    public static float GenLacMax    = 8.0f;
    public static float GenLacDef    = 2.15f;
    public static float GenExpMin    = 0.0f;
    public static float GenExpMax    = 16.0f;
    public static float GenExpDef    = 5.21f;

    public static float BlockStrength = 0.8f; // for visualizing grid
    public static float QTreeStrength = 0.6f; // for visualizing quad tree tiles
    public static float AlbedoMult    = 0.6f;
    public static float Mat1UVScale   = 1.0f/8.0f;          // empirical value
    public static float Mat2UVScale   = 1.0f/8.0f;
    public static float Mat3UVScale   = 1.0f/8.0f;
    public static float Mat4UVScale   = 1.0f/8.0f;
    public static float NoisePScale   = 0.1f;               // perlin noise for bombing
    public static float BlockUVScale  = 1.0f/(2.0f*10.0f);  // block texture has 2x2 large squares
                                                            // with 10 subdivisions each per tile
    public static float BlendDepth    = 0.2f;               // height blending edge
    public static float BlendWidth    = 0.25f;
    public static float Blend12       = 0.75f;
    public static float Blend23       = 0.38f;
    public static float Blend34       = 0.08f;
    public static float AlbVisStr     = 0.5f;
    public static float PointinessStr = 0.11f;
    public static float PointinessPow = 3.0f;
    public static float BlendColStr   = 0.2f;
    public static float BlendColScale = 4.0f;
    //public static float FogDistance   = 4000.0f; // terrain shader depth albedo blend far distance (in m)


    public static void InitializeTerrainMesh(int expX, int expZ) {
#if XBDEBUG
        var debug = new XB.DebugTimedBlock(XB.D.WorldDataInitializeTerrainMesh);
#endif

        float sizeX = System.MathF.Pow(2, expX);
        float sizeZ = System.MathF.Pow(2, expZ);

        WorldRes          = 8.0f;
        WorldDim          = new Godot.Vector2(sizeX, sizeZ);
        WorldVerts        = new Godot.Vector2I((int)(sizeX*WorldRes) +1, (int)(sizeZ*WorldRes) +1);
        TerrainHeights    = new float[WorldVerts.X, WorldVerts.Y];
        TerrainHeightsMod = new float[WorldVerts.X, WorldVerts.Y];

        ImgMiniMap = Godot.Image.Create((int)(sizeX*WorldRes), (int)(sizeZ*WorldRes),
                                        false, Godot.Image.Format.L8                 );
        ImgMiniMap.Fill(XB.Col.Black);
        ImgPointiness = Godot.Image.Create((int)(sizeX*WorldRes), (int)(sizeZ*WorldRes),
                                           false, Godot.Image.Format.L8           );
        ImgPointiness.Fill(XB.Col.Black);
        TexPointiness = new Godot.ImageTexture();
        TexPointiness.SetImage(ImgPointiness);

#if XBDEBUG
        debug.End();
#endif 
    }

    // generate a random terrain using Fractal Brownian Motion and the default generation parameters
    // then scales the world to go from 0 to the default generation height
    public static void GenerateRandomTerrain() {
#if XBDEBUG
        var debug = new XB.DebugTimedBlock(XB.D.WorldDataGenerateRandomTerrain);
#endif

        XB.Terrain.FBM(TerrainHeights, WorldVerts.X, WorldVerts.Y, WorldDim.X, WorldDim.Y,
                       GenScaleDef, GenOffXDef, GenOffZDef,
                       GenOctDef, GenPersDef, GenLacDef, GenExpDef                        );
        XB.Terrain.FindLowestHighest(TerrainHeights, WorldVerts.X, WorldVerts.Y,
                                     ref LowestPoint, ref HighestPoint          );
        XB.Terrain.HeightScale(TerrainHeights, WorldVerts.X, WorldVerts.Y,
                               GenHeightDef, ref LowestPoint, ref HighestPoint);

        // Godot.GD.Print("Generate Terrain: LP: " + LowestPoint + ", HP: " + HighestPoint);

#if XBDEBUG
        debug.End();
#endif 
    }

    // resamples the heightmap from the generated TerrainHeights array and assigns it to minimap
    // recalculates pointiness (for appearance only)
    // if reInitialize is true, the quadtree holding the terrain tiles is also created,
    // otherwise merely reset to have all tiles recalculated using the assigned heightmap
    // then the terrain collision is updated
    public static void UpdateTerrain(bool reInitialize) {
#if XBDEBUG
        var debug = new XB.DebugTimedBlock(XB.D.WorldDataUpdateTerrain);
#endif

        XB.Terrain.UpdateHeightMap(TerrainHeights, LowestPoint, HighestPoint, ImgMiniMap);
        XB.PController.Hud.UpdateMiniMap(LowestPoint, HighestPoint);

        XB.Terrain.BakePointiness(TerrainHeights, WorldVerts.X, WorldVerts.Y, ImgPointiness);
        TexPointiness.Update(ImgPointiness);

        if (reInitialize) {
            XB.ManagerTerrain.InitializeQuadTree(WorldDim.X, WorldDim.Y, WorldRes,
                                                 CollisionRes, ColliderSizeMult*TerrainTileMinimum,
                                                 TerrainTileMinimum, TerrainDivisionsMax,
                                                 LowestPoint, HighestPoint, ImgMiniMap             );
        } else {
            XB.ManagerTerrain.ResetQuadTree(LowestPoint, HighestPoint, ImgMiniMap);
        }

        XB.ManagerTerrain.UpdateCollisionTiles(LowestPoint, HighestPoint, ImgMiniMap);

#if XBDEBUG
        debug.End();
#endif 
    }

    // calculated two heightmaps representing truncated cones using signed distance field math
    // the two cones have the center of their truncated plateaus (or their tips if not truncated)
    // in the same position, then one gets larger in negative y direction, and the other is mirrored
    // vertically
    // after each the difference between the cone heightmaps and the terrain heightmap are calculated
    // the result is that any part of the cone that was above the terrain will create a 
    // cone shaped platform (add to the terrain) whereas if the inverted cone was underground
    // (also partially), those parts are cut into the terrain
    // takes angle in degrees
    public static void ApplySphereCone(Godot.Vector3 pos, float radius, float angle) {
#if XBDEBUG
        var debug = new XB.DebugTimedBlock(XB.D.WorldDataApplySphereCone);
#endif

        // Godot.GD.Print("ApplySphereCone with p: " + pos + ", r: " + radius + ", a: " + angle);
        
        XB.Terrain.Cone(TerrainHeightsMod, WorldVerts.X, WorldVerts.Y,
                        WorldDim.X, WorldDim.Y, pos.X, pos.Z,
                        radius, angle*XB.Constants.Deg2Rad, pos.Y, XB.Direction.Up);
        XB.Terrain.HeightMax(TerrainHeights, TerrainHeightsMod, WorldVerts.X, WorldVerts.Y,
                             ref LowestPoint, ref HighestPoint                             );
        XB.Terrain.Cone(TerrainHeightsMod, WorldVerts.X, WorldVerts.Y,
                        WorldDim.X, WorldDim.Y, pos.X, pos.Z,
                        radius, angle*XB.Constants.Deg2Rad, pos.Y, XB.Direction.Down);
        XB.Terrain.HeightMin(TerrainHeights, TerrainHeightsMod, WorldVerts.X, WorldVerts.Y,
                             ref LowestPoint, ref HighestPoint                             );

#if XBDEBUG
        debug.End();
#endif 
    }

    // similar to ApplySphereCone
    // takes angle in degrees
    //NOTE[ALEX]: when multiple dam segments are applied, their intersection is not solved
    //            properly with this method, also see ManagerSphere.ApplyTerrain
    public static void ApplyDamSegment(Godot.Vector3 pos1, float radius1, float angle1,
                                       Godot.Vector3 pos2, float radius2, float angle2 ) {
#if XBDEBUG
        var debug = new XB.DebugTimedBlock(XB.D.WorldDataApplyDamSegment);
#endif

        // Godot.GD.Print("ApplyDamSegment with p1: " + pos1 + ", r1: " + radius1 + ", a1: " + angle1
        //                + ", p2: " + pos2 + ", r2: " + radius2 + ", a2: " + angle2                 );
        
        XB.Terrain.UnevenCapsule(TerrainHeightsMod, WorldVerts.X, WorldVerts.Y,
                                 WorldDim.X, WorldDim.Y,
                                 pos1.X, pos1.Z, radius1, angle1*XB.Constants.Deg2Rad, pos1.Y,
                                 pos2.X, pos2.Z, radius2, angle2*XB.Constants.Deg2Rad, pos2.Y,
                                 XB.Direction.Up                                              );
        XB.Terrain.HeightMax(TerrainHeights, TerrainHeightsMod, WorldVerts.X, WorldVerts.Y,
                             ref LowestPoint, ref HighestPoint                             );
        XB.Terrain.UnevenCapsule(TerrainHeightsMod, WorldVerts.X, WorldVerts.Y,
                                 WorldDim.X, WorldDim.Y,
                                 pos1.X, pos1.Z, radius1, angle1*XB.Constants.Deg2Rad, pos1.Y,
                                 pos2.X, pos2.Z, radius2, angle2*XB.Constants.Deg2Rad, pos2.Y,
                                 XB.Direction.Down                                            );
        XB.Terrain.HeightMin(TerrainHeights, TerrainHeightsMod, WorldVerts.X, WorldVerts.Y,
                             ref LowestPoint, ref HighestPoint                             );

#if XBDEBUG
        debug.End();
#endif 
    }
}

// holds objects that live for the duration of the applications lifetime
public class AData {
    public static XB.Input                 Input;
    public static XB.Settings              S;
    public static Godot.DirectionalLight3D MainLight;
    public static Godot.Environment        Environment;
    public static Godot.Node               MainRoot;
    public static uint                     InitialSeed = 0; // random seed on application startup
                                                            // set to get the same starting terrain
}
} // namespace close
