#define XBDEBUG
namespace XB { // namespace open
// Initialize is the first non-engine code that runs
// initialization of the application and all managers and object 
// that remain over the lifetime of the app
// after that, initial creation of a world terrain
public partial class Initialize : Godot.Node3D {
    [Godot.Export] private Godot.WorldEnvironment   _environment;
    [Godot.Export] private Godot.DirectionalLight3D _mainLight;
    [Godot.Export] private XB.PController           _player;

    // the very first thing that happens, sets up variables that live for runtime
    // and loads default settings, etc.
    public override void _EnterTree() {
#if XBDEBUG
        XB.DebugProfiling.StartProfiling();
#endif

        XB.AData.Input = new XB.Input();
        XB.AData.Input.ProcessMode = Godot.Node.ProcessModeEnum.Always;
        XB.AData.Input.DefaultInputActions();

        XB.AData.MainRoot    = this;
        XB.AData.Environment = _environment.Environment;
        XB.AData.MainLight   = _mainLight;

        XB.AData.S = new XB.Settings();

        XB.Random.InitializeRandom(XB.AData.InitialSeed); // fixed startup seed for reproducable runs

        XB.Resources.InitializeTerrainTextures();
        XB.Resources.InitializeSphereTextures();

        Godot.Input.MouseMode = Godot.Input.MouseModeEnum.Captured;
        GetTree().Paused = false;
    }

    // after all children are ready, so all objects that are placed, so the player, lights, etc.
    // but not the terrain or spheres, those will be created here
    public override void _Ready() {
#if XBDEBUG
        var debug = new XB.DebugTimedBlock(XB.D.Initialize_Ready);
#endif

        XB.AData.MainRoot.AddChild(XB.AData.Input);
        XB.ManagerSphere.InitializeSpheres();

        _player.InitializePController();
        _player.InitializeHud();

        XB.AData.S.SetPresetSettings(XB.SettingsPreset.Default);
        XB.AData.S.SetApplicationDefaults();

        // world dimensions given in exponent for power of 2:
        // 1 - 2m, 2 - 4m, 3 - 8m, 4 - 16m, 5 - 32m, 6 - 64m, 7 - 128m, 8 - 256m, 9 - 512m
        int worldSizeExpX = 8;
        int worldSizeExpZ = 8;
        XB.WData.InitializeTerrainMesh(worldSizeExpX, worldSizeExpZ);
        XB.WData.GenerateRandomTerrain();
        XB.WData.UpdateTerrain(true);

        _player.InitializeMenu(); // after terrain init for _imgGenMap size

        //NOTE[ALEX]: since spawning happens one tick delayed, the first frame's UpdateQTreeMeshes
        //            uses the incorrect location for distance calculations, consider this when
        //            debugging
        _player.SpawnPlayer(new Godot.Vector2(-XB.WData.WorldDim.X/2.0f, -XB.WData.WorldDim.Y/2.0f));

#if XBDEBUG
        _player.InitializeDebugHud();

        debug.End();
#endif 
    }
}
} // namespace close
