#define XBDEBUG
namespace XB { // namespace open

// each input has one InputAction
public class InputAction {
    public string           Name;
    public string           Description;
    public string           Key; // event in plain text
    public Godot.InputEvent Event;

    //NOTE[ALEX]: description is the key for the translation dictionary,
    //            unused inputs have to start with "~" in each translated language so that
    //            their buttons can be hidden in the menu on startup
    public InputAction(string name, string description, string key, Godot.InputEvent iEvent) {
        Name        = name;
        Description = description;
        UpdateKey(key);
        Event       = iEvent;
    }

    // key names in Godot are very long for mouse buttons, so abbreviations are used instead
    public void UpdateKey(string key) {
        switch (key) {
            case "Left Mouse Button":   { Key = "LMB";        break; }
            case "Middle Mouse Button": { Key = "MMB";        break; }
            case "Right Mouse Button":  { Key = "RMB";        break; }
            case "Mouse Wheel Up":      { Key = "Wheel Up";   break; }
            case "Mouse Wheel Down":    { Key = "Wheel Down"; break; }
            default:                    { Key = key;          break; }
        }
    }
}

// cast these to find the id of a specific key in Input arrays
// useful for guide overlay
public enum KeyID {
    Start,
    Select,
    LUp,
    LDown,
    LLeft,
    LRight,
    LIn,
    RUp,
    RDown,
    RLeft,
    RRight,
    RIn,
    DUp,
    DDown,
    DLeft,
    DRight,
    FUp,
    FDown,
    FLeft,
    FRight,
    SLTop,
    SLBot,
    SRTop,
    SRBot,
    ZoomIn,
    ZoomOut,
}

// Input is responsible for collecting and storing each tick's player inputs
// the available inputs are based on a standard controller layout with an additional zoom variable
public partial class Input : Godot.Node {
    public const int Amount = 26;   // number of total input slots

    public bool  Start  = false;
    public bool  Select = false;
    public float CamX   = 0.0f;     // holds x camera input (horizontal)
    public float CamY   = 0.0f;     // holds y camera input (vertical)
    public bool  LIn    = false;    // left stick push in
    public float Zoom   = 0.0f;     // holds camera zoom input
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
            "ZoomIn",
            "ZoomOut",
        };
    public string[] InputDescriptions = new string[Amount] { // keys for translation dictionary
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
            "INP_SRBOT",
            "INP_ZOOMIN",
            "INP_ZOOMOUT",
        };

#if XBDEBUG
    // debug inputs
    public bool Debug1 = false;
    public bool Debug2 = false;
    public bool Debug3 = false;
    public bool Debug4 = false;
    public bool Debug5 = false;
#endif


    public void GetInputs() {
#if XBDEBUG
        var debug = new XB.DebugTimedBlock(XB.D.InputGetInputs);
#endif

        // clear old input values, then get input for current tick
        // IsActionPressed will continually trigger, IsActionJustPressed only on pushing down
        ConsumeAllInputs();

        // menu buttons
        if (Godot.Input.IsActionJustPressed("Start"))   { Start   = true; }  // system menu
        if (Godot.Input.IsActionJustPressed("Select"))  { Select  = true; }  // toggle HUD
        // left analog stick
        if (Godot.Input.IsActionPressed    ("LUp"))     { MoveY  += 1.0f; }  // movement
        if (Godot.Input.IsActionPressed    ("LDown"))   { MoveY  -= 1.0f; }
        if (Godot.Input.IsActionPressed    ("LLeft"))   { MoveX  += 1.0f; }
        if (Godot.Input.IsActionPressed    ("LRight"))  { MoveX  -= 1.0f; }
        if (Godot.Input.IsActionJustPressed("LIn"))     { LIn     = true; }  // unused
        // right analog stick
        if (Godot.Input.IsActionPressed    ("RUp"))     { CamY   += 1.0f; }  // camera
        if (Godot.Input.IsActionPressed    ("RDown"))   { CamY   -= 1.0f; }
        if (Godot.Input.IsActionPressed    ("RLeft"))   { CamX   += 1.0f; }
        if (Godot.Input.IsActionPressed    ("RRight"))  { CamX   -= 1.0f; }
        if (Godot.Input.IsActionJustPressed("RIn"))     { RIn     = true; }  // unused
        // d pad
        if (Godot.Input.IsActionJustPressed("DUp"))     { DUp     = true; }  // run
        if (Godot.Input.IsActionJustPressed("DDown"))   { DDown   = true; }  // toggle 1st/3rd person
        if (Godot.Input.IsActionPressed    ("DLeft"))   { DLeft   = true; }  // sphere radius modifier
        if (Godot.Input.IsActionPressed    ("DRight"))  { DRight  = true; }  // sphere angle modifier
        // face buttons
        if (Godot.Input.IsActionJustPressed("FUp"))     { FUp     = true; }  // link
        if (Godot.Input.IsActionJustPressed("FDown"))   { FDown   = true; }  // jump
        if (Godot.Input.IsActionJustPressed("FLeft"))   { FLeft   = true; }  // unlink highlighted
        if (Godot.Input.IsActionJustPressed("FRight"))  { FRight  = true; }  // toggle linking
        // left shoulder buttons
        if (Godot.Input.IsActionJustPressed("SLTop"))   { SLTop   = true; }  // place sphere
        if (Godot.Input.IsActionPressed    ("SLBot"))   { SLBot   = true; }  // aim
        // right shoulder buttons
        if (Godot.Input.IsActionJustPressed("SRTop"))   { SRTop   = true; }  // remove sphere
        if (Godot.Input.IsActionPressed    ("SRBot"))   { SRBot   = true; }  // move/modify sphere
        // zooming
        if (Godot.Input.IsActionJustReleased("ZoomIn"))  { Zoom   -= 1.0f; }
        if (Godot.Input.IsActionJustReleased("ZoomOut")) { Zoom   += 1.0f; }


#if XBDEBUG
        // DEBUG INPUTS
        Debug1 = false;
        Debug2 = false;
        Debug3 = false;
        Debug4 = false;
        Debug5 = false;

        if (Godot.Input.IsActionJustPressed("Debug1")) { Debug1 = true; }
        if (Godot.Input.IsActionJustPressed("Debug2")) { Debug2 = true; }
        if (Godot.Input.IsActionJustPressed("Debug3")) { Debug3 = true; }
        if (Godot.Input.IsActionJustPressed("Debug4")) { Debug4 = true; }
        if (Godot.Input.IsActionJustPressed("Debug5")) { Debug5 = true; }
#endif

#if XBDEBUG
        debug.End();
#endif 
    }

    // load default key bindings
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
            } else if (name == "SLTop") { // middle mouse button
                var iEvent = new Godot.InputEventMouseButton();
                iEvent.ButtonIndex = Godot.MouseButton.Middle;
                Godot.InputMap.ActionAddEvent(name, iEvent);
                key = iEvent.AsText();
                InputActions[i] = new XB.InputAction(name, desc, key, iEvent);
            } else if (name == "ZoomIn") { // mouse wheel up
                var iEvent = new Godot.InputEventMouseButton();
                iEvent.ButtonIndex = Godot.MouseButton.WheelUp;
                Godot.InputMap.ActionAddEvent(name, iEvent);
                key = iEvent.AsText();
                InputActions[i] = new XB.InputAction(name, desc, key, iEvent);
            } else if (name == "ZoomOut") { // mouse wheel down
                var iEvent = new Godot.InputEventMouseButton();
                iEvent.ButtonIndex = Godot.MouseButton.WheelDown;
                Godot.InputMap.ActionAddEvent(name, iEvent);
                key = iEvent.AsText();
                InputActions[i] = new XB.InputAction(name, desc, key, iEvent);
            } else { // keyboard keys
                var iEvent = new Godot.InputEventKey();
                switch (name) {
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
                        iEvent.Keycode = Godot.Key.X;
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
                        iEvent.Keycode = Godot.Key.E;
                    } break;
                    case "DUp": {
                        iEvent.Keycode = Godot.Key.Shift;
                    } break;
                    case "DDown": {
                        iEvent.Keycode = Godot.Key.Q;
                    } break;
                    case "DLeft": {
                        iEvent.Keycode = Godot.Key.Shift;
                    } break;
                    case "DRight": {
                        iEvent.Keycode = Godot.Key.Ctrl;
                    } break;
                    case "FUp": {
                        iEvent.Keycode = Godot.Key.F;
                    } break;
                    case "FDown": {
                        iEvent.Keycode = Godot.Key.Space;
                    } break;
                    case "FLeft": {
                        iEvent.Keycode = Godot.Key.Key2;
                    } break;
                    case "FRight": {
                        iEvent.Keycode = Godot.Key.Key3;
                    } break;
                    case "SRTop": {
                        iEvent.Keycode = Godot.Key.Key4;
                    } break;
                }
                Godot.InputMap.ActionAddEvent(name, iEvent);
                key = iEvent.AsText();
                InputActions[i] = new XB.InputAction(name, desc, key, iEvent);
            }
        }

#if XBDEBUG
        var iEventD = new Godot.InputEventKey();
        iEventD.Keycode = Godot.Key.Key5;
        AddDebugKeyBinding("Debug1", iEventD);
        iEventD = new Godot.InputEventKey();
        iEventD.Keycode = Godot.Key.Key6;
        AddDebugKeyBinding("Debug2", iEventD);
        iEventD = new Godot.InputEventKey();
        iEventD.Keycode = Godot.Key.Key7;
        AddDebugKeyBinding("Debug3", iEventD);
        iEventD = new Godot.InputEventKey();
        iEventD.Keycode = Godot.Key.Key8;
        AddDebugKeyBinding("Debug4", iEventD);
        iEventD = new Godot.InputEventKey();
        iEventD.Keycode = Godot.Key.Key9;
        AddDebugKeyBinding("Debug5", iEventD);
#endif
    }

#if XBDEBUG
    private void AddDebugKeyBinding(string name, Godot.InputEventKey iEvent) {
        Godot.InputMap.ActionEraseEvents(name);
        Godot.InputMap.ActionAddEvent(name, iEvent);
    }
#endif

    // resets all input variables
    // useful to prevent "sticky" inputs that persist after their input has been used once
    public void ConsumeAllInputs() {
#if XBDEBUG
        var debug = new XB.DebugTimedBlock(XB.D.InputConsumeAllInputs);
#endif

        Start  = false;
        Select = false;
        CamY   = 0.0f;
        CamX   = 0.0f;
        LIn    = false;
        Zoom   = 0.0f;
        MoveY  = 0.0f;
        MoveX  = 0.0f;
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

#if XBDEBUG
        Debug1 = false;
        Debug2 = false;
        Debug3 = false;
        Debug4 = false;
        Debug5 = false;
#endif 

#if XBDEBUG
        debug.End();
#endif 
    }

    public void ConsumeInputStart() {
#if XBDEBUG
        var debug = new XB.DebugTimedBlock(XB.D.InputConsumeInputStart);
#endif

        Start = false;

#if XBDEBUG
        debug.End();
#endif 
    }
}
} // namespace close
