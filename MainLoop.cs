#define XBDEBUG
namespace XB { // namespace open

public enum AppState {
    Uninit,
    Startup,
    Application,
    Menu,
}

//TODO[ALEX]: this should not be necessary, but just passing the enum variable by ref does not work
//NOTE[ALEX]: passing an enum as a reference does not seem to work, wrapping it in a class works
public class AppStWrapper {
    public XB.AppState St = XB.AppState.Uninit;
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
    public XB.Input                 Input;
    public XB.PController           PCtrl;
    public XB.HUD                   Hud;
    public XB.Menu                  Menu;
    public XB.Settings              Sett;
    public Godot.DirectionalLight3D MainLight;
    public Godot.Environment        Environment;
    public Godot.Node               MainRoot;
    public uint                     InitialSeed = 0; // random seed on application startup
                                                     // fixed to get the same starting terrain
#if XBDEBUG
    public XB.DebugHUD DebugHud;
#endif

    public static Godot.Node TR = new Godot.Node(); // for translation
    public XB.AppStWrapper _appSt;

    private Godot.Vector2 _pModelPos = new Godot.Vector2(0.0f, 0.0f);

    // the very first thing that happens, sets up variables that live for runtime
    // and loads default settings, etc.
    public override void _EnterTree() {
#if XBDEBUG
        XB.DebugProfiling.StartProfiling();
#endif

        ProcessMode = ProcessModeEnum.Always;

        _appSt = new XB.AppStWrapper();

        Input = new XB.Input();
        Input.ProcessMode = Godot.Node.ProcessModeEnum.Always;
        Input.DefaultInputActions();

        MainRoot = this;
        var pCtrlScene = Godot.ResourceLoader.Load<Godot.PackedScene>(XB.ResourcePaths.Player);
        PCtrl = (XB.PController)pCtrlScene.Instantiate();
        var environmentScene = Godot.ResourceLoader.Load<Godot.PackedScene>(XB.ResourcePaths.Environment);
        var environment = (Godot.WorldEnvironment)environmentScene.Instantiate();
        Environment = environment.Environment; // the node that "holds" the environment
                                               // (WorldEnvironment) does not get interacted 
                                               // with, but rather the environment
                                               // it holds (WorldEnvironment.Environment)
        var mainLightScene = Godot.ResourceLoader.Load<Godot.PackedScene>(XB.ResourcePaths.MainLight);
        MainLight = (Godot.DirectionalLight3D)mainLightScene.Instantiate();
        AddChild(PCtrl);
        AddChild(environment); // place the node that "holds" the environment
        AddChild(MainLight);

        var hudScene = Godot.ResourceLoader.Load<Godot.PackedScene>(XB.ResourcePaths.Hud);
        Hud = (XB.HUD)hudScene.Instantiate();
        var menuScene = Godot.ResourceLoader.Load<Godot.PackedScene>(XB.ResourcePaths.Menu);
        Menu = (XB.Menu)menuScene.Instantiate();
        AddChild(Hud);
        AddChild(Menu);
        var nameOverlayScene = Godot.ResourceLoader.Load<Godot.PackedScene>(XB.ResourcePaths.NameOverlay);
        var nameOverlay = (Godot.Control)nameOverlayScene.Instantiate();
        AddChild(nameOverlay);

        Sett = new XB.Settings();
        Sett.SetMainLoopReferences(MainRoot, MainLight, Environment, Hud, Menu, PCtrl);

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

        AddChild(Input);
        XB.ManagerSphere.InitializeSpheres(MainRoot);

        PCtrl.InitializePController(Sett);
        Hud.InitializeHud();

        Sett.SetPresetSettings(XB.SettingsPreset.Default);
        Sett.SetApplicationDefaults();

        // world dimensions given in exponent for power of 2:
        // 1 - 2m, 2 - 4m, 3 - 8m, 4 - 16m, 5 - 32m, 6 - 64m, 7 - 128m, 8 - 256m, 9 - 512m
        int worldSizeExpX = 8;
        int worldSizeExpZ = 8;
        XB.WData.InitializeTerrainMesh(worldSizeExpX, worldSizeExpZ);
        XB.WData.GenerateRandomTerrain();
        XB.WData.UpdateTerrain(true, Hud, Menu, MainRoot);

        Menu.SetMainLoopReferences(Hud, Sett, Input, PCtrl, MainRoot, ref _appSt);
        Menu.InitializeMenu(); // after terrain init for _imgGenMap size

        //NOTE[ALEX]: since spawning happens one tick delayed, the first frame's UpdateQTreeMeshes
        //            uses the incorrect location for distance calculations, consider this when
        //            debugging
        PCtrl.SpawnPlayer(new Godot.Vector2(-XB.WData.WorldDim.X/2.0f, -XB.WData.WorldDim.Y/2.0f));
        Menu.ShowStartupScreen();

#if XBDEBUG
        DebugHud = new XB.DebugHUD();
        AddChild(DebugHud);
        DebugHud.InitializeDebugHUD();

        debug.End();
#endif 
    }

    // get input using godot's system, used for mouse movement input, 
    // general input is handled at beginning of _PhysicsProcess below
    // similarly to _PhysicsProcess below, there is only one _Input in the entire code base
    public override void _Input(Godot.InputEvent @event) {
        switch (_appSt.St) {
            case XB.AppState.Application: { PCtrl.Input(@event, Sett);        break; }
            case XB.AppState.Menu:        { Menu.Input(@event);               break; }
            case XB.AppState.Startup:     { Menu.InputStartup(@event, PCtrl); break; }
        }
    }

    // Called every frame at fixed time steps (_PhysicsProcess is a Godot built in that every
    // object has, however the order of their calling is not fixed among all the objects),
    // to make the update order deterministic, _PhysicsProcess is only called here and
    // all other update functions are called from this function instead
    public override void _PhysicsProcess(double delta) {
        float dt = (float)delta;
#if XBDEBUG
        DebugHud.UpdateDebugHUD(dt, PCtrl);
#endif
        Input.GetInputs();
        Hud.UpdateHUD(dt, PCtrl, Sett, Input);
        switch (_appSt.St) {
            case XB.AppState.Startup: // continue updating all things behind the startup graphic
            case XB.AppState.Application: {
                XB.ManagerSphere.UpdateSpheres(dt);
                _pModelPos.X = PCtrl.PModel.GlobalPosition.X;
                _pModelPos.Y = PCtrl.PModel.GlobalPosition.Z;
                XB.ManagerTerrain.UpdateQTreeMeshes(ref _pModelPos, XB.WData.LowestPoint, 
                                                    XB.WData.HighestPoint, XB.WData.ImgMiniMap,
                                                    MainRoot, Hud.TexMiniMap                   );
                PCtrl.UpdatePlayer(dt, Hud, Menu, Input, Sett, MainRoot);
                break;
            }
            case XB.AppState.Menu: {
                Menu.UpdateMenu(dt);
                break;
            }
        }

#if XBDEBUG
        if (Input.Debug1) { DebugHud.Debug1(); }
        if (Input.Debug2) { DebugHud.Debug2(); }
        if (Input.Debug3) { DebugHud.Debug3(); }
        if (Input.Debug4) { DebugHud.Debug4(); }
        if (Input.Debug5) { DebugHud.Debug5(); }
#endif 
    }
}
} // namespace close
