// #define XBDEBUG
namespace XB { // namespace open
public enum SphereTexSt {
    Available,
    Active,
    ActiveLinking,
    ActiveLinked,
}

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
    [Godot.Export] private        Godot.NodePath       _textureRectLinkingNode;
                   private        Godot.TextureRect    _trLinking;
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
    private float       _linkingAlpha   = 0.0f;
    private Godot.Color _colCross       = new Godot.Color(1.0f, 1.0f, 1.0f, 1.0f);
    private Godot.Color _colFps         = new Godot.Color(0.6f, 0.6f, 0.6f, 1.0f);
    private Godot.Color _colSpheres     = new Godot.Color(1.0f, 1.0f, 1.0f, 1.0f);
    private Godot.Color _colLinking     = new Godot.Color(1.0f, 1.0f, 1.0f, 1.0f);
    private Godot.Color _colTrans       = new Godot.Color(0.0f, 0.0f, 0.0f, 0.0f);
    private Godot.Color _colSpAct       = new Godot.Color(0.87f, 0.87f, 0.87f, 1.0f);
    private Godot.Color _colSpAva       = new Godot.Color(0.2f, 0.2f, 0.2f, 0.3f);
    private Godot.Color _colSpActLing   = new Godot.Color(1.0f, 0.88f, 0.0f, 1.0f);
    private Godot.Color _colSpActLind   = new Godot.Color(0.8f, 0.66f, 0.0f, 1.0f);
    private Godot.Color _colOutline     = new Godot.Color(0.0f, 0.0f, 0.0f, 0.6f);
    private Godot.Color _colHighlight   = new Godot.Color(0.6f, 1.0f, 0.6f, 1.0f);
    private Godot.Color _colWhite       = new Godot.Color(1.0f, 1.0f, 1.0f, 1.0f);
    private Godot.Color _colBlack       = new Godot.Color(0.0f, 0.0f, 0.0f, 1.0f);

    private Godot.Image        _imgSpheres;
    private Godot.ImageTexture _texSpheres;
    private int                _texMaxY  = 1024;
    private int                _dimSpX   = 0;
    private int                _dimSpY   = 0;
    private const int          _dimSp    = 32;
    private const int          _dimBord  = 3;
    private const int          _dimThick = 1;
    private int                _rows     = 0;
    private int                _columns  = 0;
    private const int          _topOff   = 52;
    private const int          _rightOff = 4;

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
        _trLinking    =                       GetNode<Godot.TextureRect>(_textureRectLinkingNode);
        _trSpheres    =                       GetNode<Godot.TextureRect>(_textureRectSpheresNode);
        _texSpheres   = (Godot.ImageTexture)_trSpheres.Texture;
        _trCrosshairs =                       GetNode<Godot.TextureRect>(_textureRectCrosshairsNode);
        _matHudEff    = (Godot.ShaderMaterial)GetNode<Godot.TextureRect>(_matHudEffectsNode).Material;
        LbInterAct.Hide();
        LbInterKey.Hide();
        _lbFps.Hide();
        _lbMessage.Text  = "";
        _lbMessage2.Text = "";

        float columns = (float)XB.Manager.MaxSphereAmount/((float)_texMaxY/(float)_dimSp);
        _columns = (int)columns;
        if (columns%1.0f > 0.0f) { _columns += 1; }
        float rows = (float)XB.Manager.MaxSphereAmount/(float)_columns;
        _rows    = (int)rows;
        if (rows%1.0f > 0.0f) { _rows += 1; }
        _dimSpX  = _dimSp*_columns;
        _dimSpY  = _dimSp*_rows;
        _imgSpheres = Godot.Image.Create(_dimSpX, _dimSpY, false, Godot.Image.Format.Rgba8);
        _imgSpheres.Fill(_colTrans);
        _trSpheres.Position = new Godot.Vector2(1920-_rightOff-_dimSpX, _topOff);
        _trSpheres.Size     = new Godot.Vector2(_dimSpX, _dimSpY);
        _texSpheres.SetImage(_imgSpheres);
        CreateSphereTexture();

        _colCross.A            = _crossAlpha;
        _trCrosshairs.Modulate = _colCross;
        _colSpheres.A          = _spheresAlpha;
        _trSpheres.Modulate    = _colSpheres;
        _colLinking.A          = _linkingAlpha;
        _trLinking.Modulate    = _colLinking;
    }

    private void CreateSphereTexture() {
        int xStart  = 0;
        int yStart  = _dimSpY-_dimSp;
        int counter = 0;
        for (int i = 0; i < XB.Manager.MaxSphereAmount; i++ ) {
            xStart   = counter*_dimSp;
            counter += 1;
            counter %= _columns;
            var outline = XB.Utils.BeveledRectangle(xStart, yStart, _dimSp-2*_dimThick);
            var inner   = XB.Utils.BeveledRectangle(xStart+_dimBord, yStart+_dimBord,
                                                    _dimSp-2*_dimBord-2*_dimThick     );
            foreach (var sp in outline) { _imgSpheres.FillRect(sp, _colOutline  ); }
            foreach (var sp in inner)   { _imgSpheres.FillRect(sp, _colSpAva); }
            AddDigitTexture(i, xStart+2*_dimBord, yStart+2*_dimBord, _dimThick, _colWhite);
            if (counter == 0) {
                yStart -= _dimSp;
            }
        }
        _texSpheres.Update(_imgSpheres);
    }

    private void AddDigitTexture(int digit, int xStart, int yStart,
                                 int thickness, Godot.Color digitColor) {
        int ySize = _dimSp -4*_dimBord -2*_dimThick;
        int xSize = ySize/2;
        if (digit > 9) { // decimal digit
            var segmentsD = XB.Utils.DigitRectangles(digit/10, xStart, yStart,
                                                     xSize, ySize, thickness  );
            foreach (var segment in segmentsD) { _imgSpheres.FillRect(segment, digitColor); }
            xStart += xSize;
        }
        var segments = XB.Utils.DigitRectangles(digit%10, xStart, yStart,
                                                xSize, ySize, thickness  );
        foreach (var segment in segments) { _imgSpheres.FillRect(segment, digitColor); }
    }

    public void UpdateSphereTexture(int id, XB.SphereTexSt state) {
        int xStart = (id%_columns)*_dimSp;
        int yOff   = (id/_columns)*_dimSp;
        int yStart = _dimSpY-yOff-_dimSp;
        Godot.Color pColor = new Godot.Color(0.0f, 0.0f, 0.0f, 0.0f);
        Godot.Color dColor = new Godot.Color(0.0f, 0.0f, 0.0f, 0.0f);
        switch (state) {
            case XB.SphereTexSt.Available:     { pColor = _colSpAva;     dColor = _colWhite; break; }
            case XB.SphereTexSt.Active:        { pColor = _colSpAct;     dColor = _colBlack; break; }
            case XB.SphereTexSt.ActiveLinking: { pColor = _colSpActLing; dColor = _colBlack; break; }
            case XB.SphereTexSt.ActiveLinked:  { pColor = _colSpActLind; dColor = _colBlack; break; }
        }
        var inner  = XB.Utils.BeveledRectangle(xStart+_dimBord, yStart+_dimBord,
                                               _dimSp-2*_dimBord-2*_dimThick    );
        foreach (var sp in inner) { _imgSpheres.FillRect(sp, pColor); }
        AddDigitTexture(id, xStart+2*_dimBord, yStart+2*_dimBord, _dimThick, dColor);
        _texSpheres.Update(_imgSpheres);
    }

    public void UpdateSphereTextureHighlight(int from, int to) {
        if (from < XB.Manager.MaxSphereAmount) {
            int xStart = (from%_columns)*_dimSp;
            int yOff   = (from/_columns)*_dimSp;
            int yStart = _dimSpY-yOff-_dimSp;
            var rect = XB.Utils.RectangleOutline(xStart, yStart, _dimSp, _dimThick);
            foreach (var r in rect) { _imgSpheres.FillRect(r, _colTrans); }
        }
        if (to < XB.Manager.MaxSphereAmount) {
            int xStart = (to%_columns)*_dimSp;
            int yOff   = (to/_columns)*_dimSp;
            int yStart = _dimSpY-yOff-_dimSp;
            var rect = XB.Utils.RectangleOutline(xStart, yStart, _dimSp, _dimThick);
            foreach (var r in rect) { _imgSpheres.FillRect(r, _colHighlight); }
        }
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
            if (XB.Manager.Linking) { _linkingAlpha = 1.0f; }
            else                    { _linkingAlpha = 0.0f; }
            _spheresAlpha = 1.0f;
        } else {
            _crossAlpha   = 0.0f;
            _fpsAlpha     = 0.0f;
            _spheresAlpha = 0.0f;
            _linkingAlpha = 0.0f;
        }

        _colCross.A            = XB.Utils.LerpF(_colCross.A, _crossAlpha, _hudSm*dt);
        _trCrosshairs.Modulate = _colCross;
        _colFps.A              = XB.Utils.LerpF(_colFps.A, _fpsAlpha, _hudSm*dt);
        _lbFps.AddThemeColorOverride("font_color", _colFps);
        _colSpheres.A          = XB.Utils.LerpF(_colSpheres.A, _spheresAlpha, _hudSm*dt);
        _trSpheres.Modulate    = _colSpheres;
        _colLinking.A          = XB.Utils.LerpF(_colLinking.A, _linkingAlpha, _hudSm*dt);
        _trLinking.Modulate    = _colLinking;

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
