// #define XBDEBUG
namespace XB { // namespace open
public partial class HUD : Godot.Control {
    [Godot.Export] private        Godot.NodePath       _labelInteractPromptNode;
                   public  static Godot.Label          LbInterAct;
    [Godot.Export] private        Godot.NodePath       _labelInteractKeyNode;
                   public  static Godot.Label          LbInterKey;
    [Godot.Export] private        Godot.NodePath       _labelMessageNode;
                   private        Godot.Label          _lbMessage;
    [Godot.Export] private        Godot.NodePath       _labelMessage2Node;
                   private        Godot.Label          _lbMessage2;
    [Godot.Export] private        Godot.NodePath       _labelFpsNode;
                   public         Godot.Label          _lbFps;
    [Godot.Export] private        Godot.NodePath       _labelSpheresNode;
                   public         Godot.Label          _lbSpheres;
    [Godot.Export] private        Godot.NodePath       _textureCrosshairsNode;
                   private        Godot.TextureRect    _tCrosshairs;
    [Godot.Export] private        Godot.NodePath       _matHudEffectsNode;
                   private        Godot.ShaderMaterial _matHudEff;

    private bool        _hudVisible   = true;
    public  bool         CrossVisible = true; //NOTE[ALEX]: additionaly track each hud element separately
    public  bool         FpsVisible = false; //NOTE[ALEX]: additionaly track each hud element separately
    public  bool         SpheresVisible = true; //NOTE[ALEX]: additionaly track each hud element separately
    private float       _hudSm        = 16.0f;
    private float       _crossAlpha   = 0.0f;
    private float       _fpsAlpha     = 0.0f;
    private float       _spheresAlpha = 0.0f;
    private Godot.Color _colCross     = new Godot.Color(1.0f, 1.0f, 1.0f, 1.0f);
    private Godot.Color _colFps       = new Godot.Color(0.6f, 0.6f, 0.6f, 1.0f);
    private Godot.Color _colSpheres   = new Godot.Color(0.6f, 0.6f, 0.6f, 1.0f);

    private        float       _t            = 0.0f;
    private        float       _msgDur       = 3.0f;
    private static bool        _receivedMsg;
    private static string[]    _messages     = new string[2];

    public override void _Ready() {
        LbInterAct   =                       GetNode<Godot.Label>      (_labelInteractPromptNode);
        LbInterKey   =                       GetNode<Godot.Label>      (_labelInteractKeyNode);
        _lbMessage   =                       GetNode<Godot.Label>      (_labelMessageNode);
        _lbMessage2  =                       GetNode<Godot.Label>      (_labelMessage2Node);
        _lbFps       =                       GetNode<Godot.Label>      (_labelFpsNode);
        _lbSpheres   =                       GetNode<Godot.Label>      (_labelSpheresNode);
        _tCrosshairs =                       GetNode<Godot.TextureRect>(_textureCrosshairsNode);
        _matHudEff   = (Godot.ShaderMaterial)GetNode<Godot.TextureRect>(_matHudEffectsNode).Material;
        LbInterAct.Hide();
        LbInterKey.Hide();
        _lbFps.Hide();
        _lbMessage.Text  = "";
        _lbMessage2.Text = "";

        _colCross.A           = _crossAlpha;
        _tCrosshairs.Modulate = _colCross;
    }

    public void ToggleHUD() {
        _hudVisible = !_hudVisible;
    }

    public void UpdateInteractKey() {
        LbInterKey.Text = XB.AData.Input.InputActions[21].Key + " " + Tr("TO_INTERACT");
    }

    public void UpdateHUD(float dt) {
        if (_hudVisible) {
            if (CrossVisible) { _crossAlpha = 1.0f; }
            else              { _crossAlpha = 0.0f; }
            if (FpsVisible)   { _fpsAlpha   = 1.0f; }
            else              { _fpsAlpha   = 0.0f; }
            _spheresAlpha = 1.0f;
        } else {
            _crossAlpha   = 0.0f;
            _fpsAlpha     = 0.0f;
            _spheresAlpha = 0.0f;
        }

        _colCross.A           = XB.Utils.LerpF(_colCross.A, _crossAlpha, _hudSm*dt);
        _tCrosshairs.Modulate = _colCross;
        _colFps.A             = XB.Utils.LerpF(_colFps.A, _fpsAlpha, _hudSm*dt);
        _lbFps.AddThemeColorOverride("font_color", _colFps);
        _colSpheres.A         = XB.Utils.LerpF(_colSpheres.A, _spheresAlpha, _hudSm*dt);
        _lbSpheres.AddThemeColorOverride("font_color", _colSpheres);

        _t += dt;
        if (_receivedMsg) {
            if (_messages[1] != "") {
                _lbMessage2.Text = _messages[0];
                _messages[0]     = _messages[1];
                _messages[1]     = "";
            }
            _t              = 0.0f;
            _lbMessage.Text = _messages[0];
            _receivedMsg    = false;
        } 
        if (_t > _msgDur) {
            _lbMessage.Text  = "";
            _lbMessage2.Text = "";
            _messages[0]     = "";
        }

        _lbFps.Text     = Godot.Engine.GetFramesPerSecond().ToString();
        _lbSpheres.Text =   XB.Manager.ActiveSpheres.ToString() + "/"
                          + XB.Manager.MaxSphereAmount.ToString();

        // LbInterAct.Text = ""; //NOTE[ALEX]: placeholder for now
        // LbInterAct.Show();
        // LbInterKey.Show();

        // if (!XB.AData.Prompts) {
        //     LbInterAct.Hide();
        //     LbInterKey.Hide();
        // }
    }

    public void AddMessage(string message) {
        if (_messages[0] != "") _messages[1] = message;
        else                    _messages[0] = message;
        _receivedMsg = true;
    }
}
} // namespace close
