//#define XBDEBUG
namespace XB { // namespace open
public class InputAction {
    public string           Name;
    public string           Description;
    public string           Key; // event in plain text
    public Godot.InputEvent Event;

    public InputAction(string name, string description, string key, Godot.InputEvent iEvent) {
        Name        = name;
        Description = description;
        Key         = key;
        Event       = iEvent;
    }
}

//NOTE[ALEX]: only one Input exists, that persists throughout the running of the game
//            when the game or a level is initialized, the input node is attached to the root
//            when the scene gets changed, it gets de-attached but not deleted
public partial class Input : Godot.Node {
    public const int Amount = 26;       // number of total input slots

    public bool  Mode1  = false;
    public bool  Mode2  = false;
    public bool  Start  = false;
    public bool  Select = false;
    public float CamX   = 0.0f;     // holds x camera input (horizontal)
    public float CamY   = 0.0f;     // holds y camera input (vertical)
    public bool  LIn    = false;    // left stick push in
    public float MoveX  = 0.0f;     // holds x movement input (left to right)
    public float MoveY  = 0.0f;     // holds y movement input (forwards and backwards)
    public bool  RIn    = false;    // right stick push in
    public bool  DUp    = false;    // D-Pad Up
    public bool  DDown  = false;
    public bool  DLeft  = false;
    public bool  DRight = false;
    public bool  FUp    = false;    // top face button
    public bool  FDown  = false;
    public bool  FLeft  = false;
    public bool  FRight = false;
    public bool  SLTop  = false;    // left upper shoulder button
    public bool  SLBot  = false;
    public bool  SRTop  = false;
    public bool  SRBot  = false;

    public XB.InputAction[] InputActions = new XB.InputAction[Amount];
    public string[]         InputNames   = new string[Amount] {
            "Mode1",
            "Mode2",
            "Start",
            "Select",
            "LUp",
            "LDown",
            "LLeft",
            "LRight",
            "LIn",
            "RUp",
            "RDown",
            "RLeft",
            "RRight",
            "RIn",
            "DUp",
            "DDown",
            "DLeft",
            "DRight",
            "FUp",
            "FDown",
            "FLeft",
            "FRight",
            "SLTop",
            "SLBot",
            "SRTop",
            "SRBot",
        };
    public string[] InputDescriptions = new string[Amount] {
            "INP_MODE1",
            "INP_MODE2",
            "INP_START",
            "INP_SELECT",
            "INP_LUP",
            "INP_LDOWN",
            "INP_LLEFT",
            "INP_LRIGHT",
            "INP_LIN",
            "INP_RUP",
            "INP_RDOWN",
            "INP_RLEFT",
            "INP_RRIGHT",
            "INP_RIN",
            "INP_DUP",
            "INP_DDOWN",
            "INP_DLEFT",
            "INP_DRIGHT",
            "INP_FUP",
            "INP_FDOWN",
            "INP_FLEFT",
            "INP_FRIGHT",
            "INP_SLTOP",
            "INP_SLBOT",
            "INP_SRTOP",
            "INP_SLBOT",
        };

    // debug inputs
    public bool DebugMenu  = false;
    public bool DebugHud   = false;
    public bool DebugPause = false;
    public bool Debug1     = false;
    public bool Debug2     = false;
    public bool Debug3     = false;
    public bool Debug4     = false;
    public bool Debug5     = false;


    public void GetInputs() {
        // clear old input values, then get input for current tick
        // IsActionPressed will continually trigger, IsActionJustPressed only on pushing down
        ConsumeAllInputs();

        //NOTE[ALEX]: analog sticks are hacked to work with keyboard for now
        // mouse and keyboard
        if (Godot.Input.IsActionJustPressed("Mode1"))  Mode1   = true;    // unused
        if (Godot.Input.IsActionJustPressed("Mode2"))  Mode2   = true;    // unused
        // menu buttons
        if (Godot.Input.IsActionJustPressed("Start"))  Start   = true;    // system menu
        if (Godot.Input.IsActionJustPressed("Select")) Select  = true;    // toggle HUD
        // left analog stick
        if (Godot.Input.IsActionPressed    ("LUp"))    MoveY  += 1.0f;    // movement
        if (Godot.Input.IsActionPressed    ("LDown"))  MoveY  -= 1.0f;
        if (Godot.Input.IsActionPressed    ("LLeft"))  MoveX  += 1.0f;
        if (Godot.Input.IsActionPressed    ("LRight")) MoveX  -= 1.0f;
        if (Godot.Input.IsActionJustPressed("LIn"))    LIn     = true;    // toggle walk/run
        // right analog stick
        if (Godot.Input.IsActionPressed    ("RUp"))    CamY   += 1.0f;    // camera
        if (Godot.Input.IsActionPressed    ("RDown"))  CamY   -= 1.0f;
        if (Godot.Input.IsActionPressed    ("RLeft"))  CamX   += 1.0f;
        if (Godot.Input.IsActionPressed    ("RRight")) CamX   -= 1.0f;
        if (Godot.Input.IsActionJustPressed("RIn"))    RIn     = true;    // unused
        // d pad
        if (Godot.Input.IsActionJustPressed("DUp"))    DUp     = true;    // unused
        if (Godot.Input.IsActionJustPressed("DDown"))  DDown   = true;    // unused
        if (Godot.Input.IsActionJustPressed("DLeft"))  DLeft   = true;    // unused
        if (Godot.Input.IsActionJustPressed("DRight")) DRight  = true;    // unused
        // face buttons
        if (Godot.Input.IsActionJustPressed("FUp"))    FUp     = true;    // unused
        if (Godot.Input.IsActionJustPressed("FDown"))  FDown   = true;    // jump
        if (Godot.Input.IsActionJustPressed("FLeft"))  FLeft   = true;    // unused
        if (Godot.Input.IsActionJustPressed("FRight")) FRight  = true;    // unused
        // left shoulder buttons
        if (Godot.Input.IsActionJustPressed("SLTop"))  SLTop   = true;    // unused
        if (Godot.Input.IsActionPressed    ("SLBot"))  SLBot   = true;    // aim
        // right shoulder buttons
        if (Godot.Input.IsActionJustPressed("SRTop"))  SRTop   = true;    // toggle 1st/3rd person
        if (Godot.Input.IsActionPressed    ("SRBot"))  SRBot   = true;    // shoot / unused


        // DEBUG INPUTS
        DebugMenu  = false;
        DebugHud   = false;
        DebugPause = false;
        Debug1     = false;
        Debug2     = false;
        Debug3     = false;
        Debug4     = false;
        Debug5     = false;

        if (Godot.Input.IsActionJustPressed("DebugMenu"))  DebugMenu  = true;
        if (Godot.Input.IsActionJustPressed("DebugHud"))   DebugHud   = true;
        if (Godot.Input.IsActionJustPressed("DebugPause")) DebugPause = true;
        if (Godot.Input.IsActionJustPressed("Debug1"))     Debug1     = true;
        if (Godot.Input.IsActionJustPressed("Debug2"))     Debug2     = true;
        if (Godot.Input.IsActionJustPressed("Debug3"))     Debug3     = true;
        if (Godot.Input.IsActionJustPressed("Debug4"))     Debug4     = true;
        if (Godot.Input.IsActionJustPressed("Debug5"))     Debug5     = true;
    }

    public void DefaultInputActions() {
        for (int i = 0; i < Amount; i++ ) {
            string name = InputNames[i];
            string desc = InputDescriptions[i];
            string key  = "UNDEFINED";
            Godot.InputMap.ActionEraseEvents(name);
            if        (name == "SLBot") { // right mouse button
                var iEvent = new Godot.InputEventMouseButton();
                iEvent.ButtonIndex = Godot.MouseButton.Right;
                Godot.InputMap.ActionAddEvent(name, iEvent);
                key = iEvent.AsText();
                InputActions[i] = new XB.InputAction(name, desc, key, iEvent);
            } else if (name == "SRBot") { // left mouse button
                var iEvent = new Godot.InputEventMouseButton();
                iEvent.ButtonIndex = Godot.MouseButton.Left;
                Godot.InputMap.ActionAddEvent(name, iEvent);
                key = iEvent.AsText();
                InputActions[i] = new XB.InputAction(name, desc, key, iEvent);
            } else { // keyboard keys
                var iEvent = new Godot.InputEventKey();
                switch (name) {
                    case "Mode1": {
                        iEvent.Keycode = Godot.Key.Key1;
                    } break;
                    case "Mode2": {
                        iEvent.Keycode = Godot.Key.Key2;
                    } break;
                    case "Start": {
                        iEvent.Keycode = Godot.Key.Escape;
                    } break;
                    case "Select": {
                        iEvent.Keycode = Godot.Key.Tab;
                    } break;
                    case "LUp": {
                        iEvent.Keycode = Godot.Key.W;
                    } break;
                    case "LDown": {
                        iEvent.Keycode = Godot.Key.S;
                    } break;
                    case "LLeft": {
                        iEvent.Keycode = Godot.Key.A;
                    } break;
                    case "LRight": {
                        iEvent.Keycode = Godot.Key.D;
                    } break;
                    case "LIn": {
                        iEvent.Keycode = Godot.Key.Shift;
                    } break;
                    case "RUp": {
                        iEvent.Keycode = Godot.Key.Up;
                    } break;
                    case "RDown": {
                        iEvent.Keycode = Godot.Key.Down;
                    } break;
                    case "RLeft": {
                        iEvent.Keycode = Godot.Key.Left;
                    } break;
                    case "RRight": {
                        iEvent.Keycode = Godot.Key.Right;
                    } break;
                    case "RIn": {
                        iEvent.Keycode = Godot.Key.L;
                    } break;
                    case "DUp": {
                        iEvent.Keycode = Godot.Key.F;
                    } break;
                    case "DDown": {
                        iEvent.Keycode = Godot.Key.X;
                    } break;
                    case "DLeft": {
                        iEvent.Keycode = Godot.Key.Z;
                    } break;
                    case "DRight": {
                        iEvent.Keycode = Godot.Key.C;
                    } break;
                    case "FUp": {
                        iEvent.Keycode = Godot.Key.Q;
                    } break;
                    case "FDown": {
                        iEvent.Keycode = Godot.Key.Space;
                    } break;
                    case "FLeft": {
                        iEvent.Keycode = Godot.Key.H;
                    } break;
                    case "FRight": {
                        iEvent.Keycode = Godot.Key.E;
                    } break;
                    case "SLTop": {
                        iEvent.Keycode = Godot.Key.P;
                    } break;
                    case "SRTop": {
                        iEvent.Keycode = Godot.Key.Key3;
                    } break;
                }
                Godot.InputMap.ActionAddEvent(name, iEvent);
                key = iEvent.AsText();
                InputActions[i] = new XB.InputAction(name, desc, key, iEvent);
            }
        }
    }

    public void ConsumeAllInputs() {
        Mode1  = false;
        Mode2  = false;
        Start  = false;
        Select = false;
        MoveY  = 0.0f;
        MoveX  = 0.0f;
        LIn    = false;
        CamY   = 0.0f;
        CamX   = 0.0f;
        RIn    = false;
        DUp    = false;
        DDown  = false;
        DLeft  = false;
        DRight = false;
        FUp    = false;
        FDown  = false;
        FLeft  = false;
        FRight = false;
        SLTop  = false;
        SLBot  = false;
        SRTop  = false;
        SRBot  = false;
    }

    public void ConsumeInputStart() {
        Start = false;
    }

    public void ConsumeInputFRight() {
        FRight = false;
    }

    public void ConsumeInputDebugMenu() {
        DebugMenu = false;
    }

    public void ConsumeInputDebugHud() {
        DebugHud = false;
    }

    public void ConsumeInputDebugPause() {
        DebugPause = false;
    }
}
} // namespace close
