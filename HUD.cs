// #define XBDEBUG
namespace XB { // namespace open
using SysCG = System.Collections.Generic;
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
                   public         Godot.Label          LbFps;
    [Godot.Export] private        Godot.NodePath       _textureCrosshairsNode;
                   private        Godot.TextureRect    _tCrosshairs;
    [Godot.Export] private        Godot.NodePath       _matHudEffectsNode;
                   private        Godot.ShaderMaterial _matHudEff;

    private        float       _t            = 0.0f;
    private        Godot.Color _colLabel     = new Godot.Color(0.54f, 0.55f, 0.6f, 1.0f);
    private        float       _msgDur       = 3.0f;
    private static bool        _receivedMsg;
    private static string[]    _messages     = new string[2];

    public override void _Ready() {
        LbInterAct   =                       GetNode<Godot.Label>      (_labelInteractPromptNode);
        LbInterKey   =                       GetNode<Godot.Label>      (_labelInteractKeyNode);
        _lbMessage   =                       GetNode<Godot.Label>      (_labelMessageNode);
        _lbMessage2  =                       GetNode<Godot.Label>      (_labelMessage2Node);
        LbFps        =                       GetNode<Godot.Label>      (_labelFpsNode);
        _tCrosshairs =                       GetNode<Godot.TextureRect>(_textureCrosshairsNode);
        _matHudEff   = (Godot.ShaderMaterial)GetNode<Godot.TextureRect>(_matHudEffectsNode).Material;
        LbInterAct.Hide();
        LbInterKey.Hide();
        LbFps.RemoveThemeColorOverride("font_color");
        LbFps.AddThemeColorOverride("font_color", _colLabel);
        LbFps.Hide();
        _lbMessage.Text  = "";
        _lbMessage2.Text = "";
        _tCrosshairs.Hide();
    }

    public void UpdateInteractKey() {
        LbInterKey.Text = XB.AData.Input.InputActions[21].Key + " " + Tr("TO_INTERACT");
    }

    public void Initialize() {
    }

    public void UpdateHUD(float dt) {
        if (XB.PController.PlAiming && XB.AData.Crosshairs) _tCrosshairs.Show();
        else                                                _tCrosshairs.Hide();

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

        LbFps.Text = Godot.Engine.GetFramesPerSecond().ToString();

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
