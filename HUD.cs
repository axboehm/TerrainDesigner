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
    [Godot.Export] private        Godot.NodePath       _textureRectSpheresNode;
                   private        Godot.TextureRect    _trSpheres;
    [Godot.Export] private        Godot.NodePath       _textureRectCrosshairsNode;
                   private        Godot.TextureRect    _trCrosshairs;
    [Godot.Export] private        Godot.NodePath       _matHudEffectsNode;
                   private        Godot.ShaderMaterial _matHudEff;

    private bool        _hudVisible     = true;
    public  bool         CrossVisible   = true;
    public  bool         FpsVisible     = false;
    public  bool         SpheresVisible = true;
    private float       _hudSm          = 16.0f;
    private float       _crossAlpha     = 0.0f;
    private float       _fpsAlpha       = 0.0f;
    private float       _spheresAlpha   = 0.0f;
    private Godot.Color _colCross       = new Godot.Color(1.0f, 1.0f, 1.0f, 1.0f);
    private Godot.Color _colFps         = new Godot.Color(0.6f, 0.6f, 0.6f, 1.0f);
    private Godot.Color _colSpheres     = new Godot.Color(1.0f, 1.0f, 1.0f, 1.0f);
    private Godot.Color _colTrans       = new Godot.Color(0.0f, 0.0f, 0.0f, 0.0f);
    private Godot.Color _colActive      = new Godot.Color(0.87f, 0.87f, 0.87f, 1.0f);
    private Godot.Color _colAvailable   = new Godot.Color(0.2f, 0.2f, 0.2f, 0.3f);
    private Godot.Color _colOutline     = new Godot.Color(0.0f, 0.0f, 0.0f, 0.6f);

    private Godot.Image        _imgSpheres;
    private Godot.ImageTexture _texSpheres;
    private int                _dimSpX  = 0;
    private int                _dimSpY  = 0;
    private const int          _dimSp   = 32;
    private const int          _dimBord = 3;
    private const int          _dimSpac = 1;
    private int                _rows    = 0;
    private int                _columns = 0;

    private        float       _t            = 0.0f;
    private        float       _msgDur       = 3.0f;
    private static bool        _receivedMsg;
    private static string[]    _messages     = new string[2];

    public void InitializeHud() {
        LbInterAct    =                       GetNode<Godot.Label>      (_labelInteractPromptNode);
        LbInterKey    =                       GetNode<Godot.Label>      (_labelInteractKeyNode);
        _lbMessage    =                       GetNode<Godot.Label>      (_labelMessageNode);
        _lbMessage2   =                       GetNode<Godot.Label>      (_labelMessage2Node);
        _lbFps        =                       GetNode<Godot.Label>      (_labelFpsNode);
        _trSpheres    =                       GetNode<Godot.TextureRect>(_textureRectSpheresNode);
        _texSpheres   = (Godot.ImageTexture)_trSpheres.Texture;
        _trCrosshairs =                       GetNode<Godot.TextureRect>(_textureRectCrosshairsNode);
        _matHudEff    = (Godot.ShaderMaterial)GetNode<Godot.TextureRect>(_matHudEffectsNode).Material;
        LbInterAct.Hide();
        LbInterKey.Hide();
        _lbFps.Hide();
        _lbMessage.Text  = "";
        _lbMessage2.Text = "";

        _columns = XB.Manager.MaxSphereAmount/(1024/32); //NOTE[ALEX]: works only with powers of 2
        _rows    = XB.Manager.MaxSphereAmount/_columns;
        _dimSpX  = _dimSp*_columns;
        _dimSpY  = _dimSp*_rows;
        _imgSpheres = Godot.Image.Create(_dimSpX, _dimSpY, false, Godot.Image.Format.Rgba8);
        _imgSpheres.Fill(_colTrans);
        _texSpheres.SetImage(_imgSpheres);
        CreateSphereTexture();

        _colCross.A            = _crossAlpha;
        _trCrosshairs.Modulate = _colCross;
        _colSpheres.A          = _spheresAlpha;
        _trSpheres.Modulate    = _colSpheres;
    }

    public void CreateSphereTexture() {
        int xStart = 0;
        int yStart = _dimSpY-_dimSp;
        int counter = 0;
        for (int i = 0; i < XB.Manager.MaxSphereAmount; i++ ) {
            xStart   = counter*_dimSp;
            counter += 1;
            counter %= _columns;
            var outline = XB.Utils.BeveledRectangle(xStart, yStart, _dimSp-2*_dimSpac);
            var inner   = XB.Utils.BeveledRectangle(xStart+_dimBord, yStart+_dimBord,
                                                    _dimSp-2*_dimBord-2*_dimSpac     );
            foreach (var sp in outline) { _imgSpheres.FillRect(sp, _colOutline); }
            foreach (var sp in inner) { _imgSpheres.FillRect(sp, _colAvailable); }
            //TODO[ALEX]: digits
            if (counter == 0) {
                yStart -= _dimSp;
            }
        }
        _texSpheres.Update(_imgSpheres);
    }

    public void UpdateSphereTexture(int id) {
        int xStart = (id%_columns)*_dimSp;
        int yOff   = (id/_columns)*_dimSp;
        int yStart = _dimSpY-yOff-_dimSp;
        var inner  = XB.Utils.BeveledRectangle(xStart+_dimBord, yStart+_dimBord,
                                               _dimSp-2*_dimBord-2*_dimSpac     );
        foreach (var sp in inner) { _imgSpheres.FillRect(sp, _colActive); }
        //TODO[ALEX]: digits
        _texSpheres.Update(_imgSpheres);
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

        _colCross.A            = XB.Utils.LerpF(_colCross.A, _crossAlpha, _hudSm*dt);
        _trCrosshairs.Modulate = _colCross;
        _colFps.A              = XB.Utils.LerpF(_colFps.A, _fpsAlpha, _hudSm*dt);
        _lbFps.AddThemeColorOverride("font_color", _colFps);
        _colSpheres.A          = XB.Utils.LerpF(_colSpheres.A, _spheresAlpha, _hudSm*dt);
        _trSpheres.Modulate    = _colSpheres;

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

        _lbFps.Text = Godot.Engine.GetFramesPerSecond().ToString();

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
