//#define XBDEBUG
namespace XB { // namespace open
public partial class Initialize : Godot.Node3D {
    [Godot.Export] private Godot.WorldEnvironment   _environment;
    [Godot.Export] private Godot.DirectionalLight3D _mainLight;

    public override void _EnterTree() { // the very first thing that happens in the game
        // create input manager that will persist throughout the running of the game
        XB.AData.Input = new XB.Input();
        XB.AData.Input.ProcessMode = Godot.Node.ProcessModeEnum.Always;
        XB.AData.Input.DefaultInputActions();

        XB.AData.MainRoot    = this;
        XB.AData.Environment = _environment.Environment;
        XB.AData.MainLight   = _mainLight;
        XB.Random.InitializeRandom((uint)System.DateTime.Now.GetHashCode());

        Godot.Input.MouseMode = Godot.Input.MouseModeEnum.Captured;
        GetTree().Paused = false;
    }

    public override void _Ready() { // after all children are ready
        XB.AData.MainRoot.AddChild(XB.AData.Input);
        XB.PersistData.SetPresetSettings(XB.SettingsPreset.Default);
        XB.PersistData.SetApplicationDefaults();
        XB.Manager.InitializeSpheres();
        XB.PController.Hud.InitializeHud();
        XB.PersistData.UpdateScreen();
    }
}
} // namespace close
