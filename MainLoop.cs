#define XBDEBUG
namespace XB { // namespace open

// status of the application is used to determine which objects should be updated
public enum AppState {
    Uninit,
    Startup,
    Application,
    Menu,
}

// holds all variables that are made per player 
// and persist throughout the runtime of the application
public class PlayerState {
    public XB.Input       Input; // gathers and holds inputs each frame
    public XB.PController PCtrl; // updates everything related to the player character
    public XB.HUD         Hud;   // on screen display active while not in the menu
    public XB.Menu        Menu;  // pause menu with various settings and functions
    public Godot.Control  NameOverlay; // always active top right corner overlay
    public XB.Settings    Sett;  // holds and updates all settings and applies their effects
    public XB.AppState    AppSt; // status of the application
#if XBDEBUG
    public XB.DebugHUD    DebugHud; // additional, optional overlay for profiling information
#endif
}

// MainLoop is the first non engine code that runs
// initialization of the application and all managers and object 
// that remain over the lifetime of the app
// after that, initial creation of terrain
// MainLoop's _PhysicsProcess function is the only call of this kind in the codebase
// all other object's update functions are called from here to make the control flow obvious
// and to control the order in which functions are called
public partial class MainLoop : Godot.Node3D {
    // global variables that live for the duration of the applcation's lifetime
    public XB.PlayerState           PS; //NOTE[ALEX]: could become an array to allow for multiple
    public Godot.DirectionalLight3D MainLight;   // reference to adjust via settings
    public Godot.Environment        Environment; // reference to adjust via settings
    public Godot.Node               MainRoot;    // reference to allow adding objects at runtime
    public uint                     InitialSeed = 0; // random seed on application startup
                                                     // fixed to get the same starting terrain
    // world dimensions given in exponent for power of 2:
    // 1 - 2m, 2 - 4m, 3 - 8m, 4 - 16m, 5 - 32m, 6 - 64m, 7 - 128m, 8 - 256m, 9 - 512m
    private const int _worldSizeExpX = 5;
    private const int _worldSizeExpZ = 5;

    public static Godot.Node TR = new Godot.Node(); // for translation
    private Godot.Vector2 _qTreeRefPos = new Godot.Vector2(0.0f, 0.0f); // used as reference point
                                                                        // to update the quadtree
                                                                        // every frame


    // the first non engine code that runs
    // all objects that persist throughout the runtime of the application are created
    // and initialized here
    // then the initial terrain is created and the quadtree initialized
    public override void _Ready() {
        PS = new XB.PlayerState();

#if XBDEBUG
        XB.DebugProfiling.StartProfiling();
        PS.DebugHud = new XB.DebugHUD();
        AddChild(PS.DebugHud);
        var debug = new XB.DebugTimedBlock(XB.D.Initialize_Ready);
#endif

        ProcessMode = ProcessModeEnum.Always; // run _PhysicsProcess also when processing is paused

        // create runtime objects
        MainRoot = this;
        var pCtrlScene = Godot.ResourceLoader.Load<Godot.PackedScene>(XB.ResourcePaths.Player);
        PS.PCtrl       = (XB.PController)pCtrlScene.Instantiate();
        var environmentScene = Godot.ResourceLoader.Load<Godot.PackedScene>(XB.ResourcePaths.Environment);
        var environment      = (Godot.WorldEnvironment)environmentScene.Instantiate();
        Environment = environment.Environment; // the node that "holds" the environment
                                               // (WorldEnvironment) does not get interacted 
                                               // with, but rather the environment
                                               // it holds (WorldEnvironment.Environment)
        var mainLightScene = Godot.ResourceLoader.Load<Godot.PackedScene>(XB.ResourcePaths.MainLight);
        MainLight          = (Godot.DirectionalLight3D)mainLightScene.Instantiate();
        AddChild(PS.PCtrl);
        AddChild(environment); // place the node that "holds" the environment
        AddChild(MainLight);

        PS.Input = new XB.Input();
        PS.Input.DefaultInputActions();
        PS.PCtrl.AddChild(PS.Input);

        var hudScene = Godot.ResourceLoader.Load<Godot.PackedScene>(XB.ResourcePaths.Hud);
        PS.Hud       = (XB.HUD)hudScene.Instantiate();
        var menuScene = Godot.ResourceLoader.Load<Godot.PackedScene>(XB.ResourcePaths.Menu);
        PS.Menu       = (XB.Menu)menuScene.Instantiate();
        PS.PCtrl.AddChild(PS.Hud);
        PS.PCtrl.AddChild(PS.Menu);

        var nameOverlayScene = Godot.ResourceLoader.Load<Godot.PackedScene>(XB.ResourcePaths.NameOverlay);
        PS.NameOverlay       = (Godot.Control)nameOverlayScene.Instantiate();
        PS.PCtrl.AddChild(PS.NameOverlay);

        PS.Sett = new XB.Settings();
        PS.Sett.SetMainLoopReferences(MainRoot, MainLight, Environment, PS);

        // initializations
        XB.Random.InitializeRandom(InitialSeed); // fixed startup seed for reproducable runs
        XB.Resources.InitializeTerrainTextures();
        XB.Resources.InitializeSphereTextures();
        XB.ManagerSphere.InitializeSpheres(MainRoot);
        PS.PCtrl.InitializePController(PS.Sett);
        PS.Hud.InitializeHud();

        // load default settings
        PS.Sett.SetPresetSettings(XB.SettingsPreset.Default);
        PS.Sett.SetApplicationDefaults();

        // initialize, then create terrain
        XB.WData.InitializeTerrainMesh(_worldSizeExpX, _worldSizeExpZ);
        XB.WData.GenerateRandomTerrain();
        XB.WData.UpdateTerrain(true, PS.Hud, PS.Menu, MainRoot);

        // initialize menu after terrain initialization for minimap (_imgGenMap size)
        PS.Menu.SetMainLoopReferences(PS, MainRoot);
        PS.Menu.InitializeMenu();

        // place the player in center of terrain and show startup screen
        //NOTE[ALEX]: since spawning happens one tick delayed, the first frame's UpdateQTreeMeshes
        //            uses the incorrect location for distance calculations, consider this when
        //            debugging
        PS.PCtrl.SpawnPlayer(new Godot.Vector2(-XB.WData.WorldDim.X/2.0f,
                                               -XB.WData.WorldDim.Y/2.0f ));
        PS.Menu.ShowStartupScreen();

#if XBDEBUG
        PS.DebugHud.InitializeDebugHUD(); // after random initialization for noise texture size
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
                _qTreeRefPos.X = PS.PCtrl.PModel.GlobalPosition.X;
                _qTreeRefPos.Y = PS.PCtrl.PModel.GlobalPosition.Z;
                XB.ManagerTerrain.UpdateQTreeMeshes(ref _qTreeRefPos, XB.WData.LowestPoint, 
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
        // debug functions
        if (PS.Input.Debug1) { PS.DebugHud.Debug1(); }
        if (PS.Input.Debug2) { PS.DebugHud.Debug2(); }
        if (PS.Input.Debug3) { PS.DebugHud.Debug3(); }
        if (PS.Input.Debug4) { PS.DebugHud.Debug4(); }
        if (PS.Input.Debug5) { PS.DebugHud.Debug5(); }
#endif 
    }
}
} // namespace close
