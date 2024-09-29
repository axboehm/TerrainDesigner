#define XBDEBUG
namespace XB { // namespace open

    //TODO[ALEX]: rethink this in regards to multiplayer (no pause in menu?)
public enum AppState {
    Uninit,
    Startup,
    Application,
    Menu,
}

// holds all variables that are made per player
public class PlayerState {
    public XB.Input       Input;
    public XB.PController PCtrl;
    public XB.HUD         Hud;
    public XB.Menu        Menu;
    public Godot.Control  NameOverlay;
    public XB.Settings    Sett;
    public XB.AppState    AppSt;
#if XBDEBUG
    public XB.DebugHUD    DebugHud;
#endif
}

// MainLoop is the first non engine code that runs
// initialization of the application and all managers and object 
// that remain over the lifetime of the app
// after that, initial creation of a world terrain
// MainLoop's _PhysicsProcess function is the only call of this kind in the codebase
// all other object's update functions are called from here to make the control flow obvious
// and to control the order in which functions are called
public partial class MainLoop : Godot.Node3D {
    // global variables that live for the duration of the applcation's lifetime
    public XB.PlayerState           PS; //NOTE[ALEX]: could become an array to allow for multiple
    public Godot.DirectionalLight3D MainLight;
    public Godot.Environment        Environment;
    public Godot.Node               MainRoot;
    public uint                     InitialSeed = 0; // random seed on application startup
                                                     // fixed to get the same starting terrain
    // world dimensions given in exponent for power of 2:
    // 1 - 2m, 2 - 4m, 3 - 8m, 4 - 16m, 5 - 32m, 6 - 64m, 7 - 128m, 8 - 256m, 9 - 512m
    private const int _worldSizeExpX = 8;
    private const int _worldSizeExpZ = 8;

    public static Godot.Node TR = new Godot.Node(); // for translation

    private Godot.Vector2 _pModelPos = new Godot.Vector2(0.0f, 0.0f);

    // the very first thing that happens, sets up variables that live for runtime
    // and loads default settings, etc.
    public override void _EnterTree() {
        PS = new XB.PlayerState();

#if XBDEBUG
        XB.DebugProfiling.StartProfiling();
        PS.DebugHud = new XB.DebugHUD();
        AddChild(PS.DebugHud);
#endif

        ProcessMode = ProcessModeEnum.Always;

        MainRoot = this;
        var pCtrlScene = Godot.ResourceLoader.Load<Godot.PackedScene>(XB.ResourcePaths.Player);
        PS.PCtrl = (XB.PController)pCtrlScene.Instantiate();
        var environmentScene = Godot.ResourceLoader.Load<Godot.PackedScene>(XB.ResourcePaths.Environment);
        var environment = (Godot.WorldEnvironment)environmentScene.Instantiate();
        Environment = environment.Environment; // the node that "holds" the environment
                                               // (WorldEnvironment) does not get interacted 
                                               // with, but rather the environment
                                               // it holds (WorldEnvironment.Environment)
        var mainLightScene = Godot.ResourceLoader.Load<Godot.PackedScene>(XB.ResourcePaths.MainLight);
        MainLight = (Godot.DirectionalLight3D)mainLightScene.Instantiate();
        AddChild(PS.PCtrl);
        AddChild(environment); // place the node that "holds" the environment
        AddChild(MainLight);

        PS.Input = new XB.Input();
        PS.Input.DefaultInputActions();
        PS.PCtrl.AddChild(PS.Input);

        var hudScene = Godot.ResourceLoader.Load<Godot.PackedScene>(XB.ResourcePaths.Hud);
        PS.Hud = (XB.HUD)hudScene.Instantiate();
        var menuScene = Godot.ResourceLoader.Load<Godot.PackedScene>(XB.ResourcePaths.Menu);
        PS.Menu = (XB.Menu)menuScene.Instantiate();
        PS.PCtrl.AddChild(PS.Hud);
        PS.PCtrl.AddChild(PS.Menu);
        var nameOverlayScene = Godot.ResourceLoader.Load<Godot.PackedScene>(XB.ResourcePaths.NameOverlay);
        PS.NameOverlay = (Godot.Control)nameOverlayScene.Instantiate();
        PS.PCtrl.AddChild(PS.NameOverlay);

        PS.Sett = new XB.Settings();
        PS.Sett.SetMainLoopReferences(MainRoot, MainLight, Environment, PS);

        XB.Random.InitializeRandom(InitialSeed); // fixed startup seed for reproducable runs

        XB.Resources.InitializeTerrainTextures();
        XB.Resources.InitializeSphereTextures();

        Godot.Input.MouseMode = Godot.Input.MouseModeEnum.Captured;
    }

    // after all children are ready, so all objects that are placed, so the player, lights, etc.
    // but not the terrain or spheres, those will be created here
    public override void _Ready() {
#if XBDEBUG
        var debug = new XB.DebugTimedBlock(XB.D.Initialize_Ready);
#endif

        XB.ManagerSphere.InitializeSpheres(MainRoot);

        PS.PCtrl.InitializePController(PS.Sett);
        PS.Hud.InitializeHud();

        PS.Sett.SetPresetSettings(XB.SettingsPreset.Default);
        PS.Sett.SetApplicationDefaults();

        XB.WData.InitializeTerrainMesh(_worldSizeExpX, _worldSizeExpZ);
        XB.WData.GenerateRandomTerrain();
        XB.WData.UpdateTerrain(true, PS.Hud, PS.Menu, MainRoot);

        PS.Menu.SetMainLoopReferences(PS, MainRoot);
        PS.Menu.InitializeMenu(); // after terrain init for _imgGenMap size

        //NOTE[ALEX]: since spawning happens one tick delayed, the first frame's UpdateQTreeMeshes
        //            uses the incorrect location for distance calculations, consider this when
        //            debugging
        PS.PCtrl.SpawnPlayer(new Godot.Vector2(-XB.WData.WorldDim.X/2.0f,
                                               -XB.WData.WorldDim.Y/2.0f ));
        PS.Menu.ShowStartupScreen();

#if XBDEBUG
        PS.DebugHud.InitializeDebugHUD();

        debug.End();
#endif 
    }

    // get input using godot's system, used for mouse movement input, 
    // general input is handled at beginning of _PhysicsProcess below
    // similarly to _PhysicsProcess below, there is only one _Input in the entire code base
    public override void _Input(Godot.InputEvent @event) {
        switch (PS.AppSt) {
            case XB.AppState.Application: { PS.PCtrl.Input(@event, PS.Sett);        break; }
            case XB.AppState.Menu:        { PS.Menu.Input(@event);                  break; }
            case XB.AppState.Startup:     { PS.Menu.InputStartup(@event, PS.PCtrl); break; }
        }
    }

    // Called every frame at fixed time steps (_PhysicsProcess is a Godot built in that every
    // object has, however the order of their calling is not fixed among all the objects),
    // to make the update order deterministic, _PhysicsProcess is only called here and
    // all other update functions are called from this function instead
    public override void _PhysicsProcess(double delta) {
        float dt = (float)delta;
#if XBDEBUG
        PS.DebugHud.UpdateDebugHUD(dt, PS.PCtrl);
#endif
        PS.Input.GetInputs();
        PS.Hud.UpdateHUD(dt, PS.PCtrl, PS.Sett, PS.Input);
        switch (PS.AppSt) {
            case XB.AppState.Startup: // continue updating all things behind the startup graphic
            case XB.AppState.Application: {
                XB.ManagerSphere.UpdateSpheres(dt);
                _pModelPos.X = PS.PCtrl.PModel.GlobalPosition.X;
                _pModelPos.Y = PS.PCtrl.PModel.GlobalPosition.Z;
                XB.ManagerTerrain.UpdateQTreeMeshes(ref _pModelPos, XB.WData.LowestPoint, 
                                                    XB.WData.HighestPoint, XB.WData.ImgMiniMap,
                                                    MainRoot, PS.Hud.TexMiniMap                );
                PS.PCtrl.UpdatePlayer(dt, PS.Hud, PS.Menu, PS.Input, PS.Sett, MainRoot);
                break;
            }
            case XB.AppState.Menu: {
                PS.Menu.UpdateMenu(dt);
                break;
            }
        }

#if XBDEBUG
        if (PS.Input.Debug1) { PS.DebugHud.Debug1(); }
        if (PS.Input.Debug2) { PS.DebugHud.Debug2(); }
        if (PS.Input.Debug3) { PS.DebugHud.Debug3(); }
        if (PS.Input.Debug4) { PS.DebugHud.Debug4(); }
        if (PS.Input.Debug5) { PS.DebugHud.Debug5(); }
#endif 
    }
}
} // namespace close
