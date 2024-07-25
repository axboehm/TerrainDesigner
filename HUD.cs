// #define XBDEBUG
namespace XB { // namespace open
public enum SphereTexSt {
    Inactive,
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
    [Godot.Export] private        Godot.NodePath       _textureRectMiniMapNode;
                   private        Godot.TextureRect    _trMiniMap;
    [Godot.Export] private        Godot.NodePath       _textureRectMiniMapOverlayNode;
                   private        Godot.TextureRect    _trMiniMapO;
    [Godot.Export] private        Godot.NodePath       _matHudEffectsNode;
                   private        Godot.ShaderMaterial _matHudEff;

    private bool        _hudVisible    = true;
    public  bool        CrossVisible   = true;
    public  bool        FpsVisible     = false;
    public  bool        SpheresVisible = true;
    private float       _hudSm         = 16.0f;
    private float       _crossAlpha    = 0.0f;
    private Godot.Color _colCross      = new Godot.Color(1.0f, 1.0f, 1.0f, 1.0f);
    private float       _fpsAlpha      = 0.0f;
    private Godot.Color _colFps        = new Godot.Color(0.6f, 0.6f, 0.6f, 1.0f);
    private float       _spheresAlpha  = 0.0f;
    private Godot.Color _colSpheres    = new Godot.Color(1.0f, 1.0f, 1.0f, 1.0f);
    private float       _linkingAlpha  = 0.0f;
    private Godot.Color _colLinking    = new Godot.Color(1.0f, 1.0f, 1.0f, 1.0f);
    private float       _miniMapAlpha  = 0.0f;
    private Godot.Color _colMiniMap    = new Godot.Color(1.0f, 1.0f, 1.0f, 1.0f);
    private float       _blockMultCur  = 1.0f;
    private float       _blockMult     = 0.0f;

    private Godot.Vector2I _vect  = new Godot.Vector2I(0, 0); // reusable vector for Rect2I
    private Godot.Rect2I[] _rects = new Godot.Rect2I[XB.Utils.MaxRectSize];
    private int            _rSize = 0; // how many entries in _rects were used by function

    private const int          _offsetT = 52; // offset from top screen edge (at base res 1920x1080)
    private const int          _offsetH = 4;  // horizontal offset from side screen edges

    private Godot.Image        _imgLinking;
    private Godot.ImageTexture _texLinking;
    private const int          _dimLinkBorderX = 128;
    private const int          _dimLinkBorderY = 96;
    private const int          _dimLinkThick   = 16;
    private const int          _dimLinkShadow  = 4; // part of thickness, does not add to total
    private const float        _dimLinkCorX    = 0.18f;
    private const float        _dimLinkCorY    = 0.12f;

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

    public  Godot.Image        ImgMiniMap;
    private Godot.ImageTexture _texMiniMap;
    private Godot.Image        _imgMiniMapO;
    private Godot.ImageTexture _texMiniMapO;
    private const int          _dimMMX  = 256;
    private const int          _dimMMY  = 256;
    private const int          _dimMMPl = 10;
    private const int          _dimMMSp = 6;

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
        _texLinking   = new Godot.ImageTexture();
        _trLinking.Texture = _texLinking;
        _trSpheres    =                       GetNode<Godot.TextureRect>(_textureRectSpheresNode);
        _texSpheres   = new Godot.ImageTexture();
        _trSpheres.Texture = _texSpheres;
        _trCrosshairs =                       GetNode<Godot.TextureRect>(_textureRectCrosshairsNode);
        _trMiniMap    =                       GetNode<Godot.TextureRect>(_textureRectMiniMapNode);
        _texMiniMap   = new Godot.ImageTexture();
        _trMiniMap.Texture = _texMiniMap;
        _trMiniMapO   =                       GetNode<Godot.TextureRect>(_textureRectMiniMapOverlayNode);
        _texMiniMapO  = new Godot.ImageTexture();
        _trMiniMapO.Texture = _texMiniMapO;
        _matHudEff    = (Godot.ShaderMaterial)GetNode<Godot.TextureRect>(_matHudEffectsNode).Material;
        LbInterAct.Hide();
        LbInterKey.Hide();
        _lbFps.Hide();
        _lbMessage.Text  = "";
        _lbMessage2.Text = "";

        for (int i = 0; i < _rects.Length; i++) { _rects[i] = new Godot.Rect2I(0, 0, 0, 0); }

        int sizeLinkingX = XB.AData.BaseResX-2*_dimLinkBorderX;
        int sizeLinkingY = XB.AData.BaseResY-2*_dimLinkBorderY;
        _imgLinking = Godot.Image.Create(sizeLinkingX, sizeLinkingY,
                                         false, Godot.Image.Format.Rgba8);
        _texLinking.SetImage(_imgLinking);
        _trLinking.Position = new Godot.Vector2(_dimLinkBorderX, _dimLinkBorderY);
        _trLinking.Size     = new Godot.Vector2(sizeLinkingX, sizeLinkingY);
        CreateLinkingTexture(_dimLinkThick, _dimLinkShadow,
                             (int)((float)sizeLinkingX*_dimLinkCorX),
                             (int)((float)sizeLinkingY*_dimLinkCorY), 
                             ref _imgLinking, ref _texLinking, ref _vect);

        float columns = (float)XB.Manager.MaxSphereAmount/((float)_texMaxY/(float)_dimSp);
        _columns = (int)columns;
        if (columns%1.0f > 0.0f) { _columns += 1; }
        float rows = (float)XB.Manager.MaxSphereAmount/(float)_columns;
        _rows    = (int)rows;
        if (rows%1.0f > 0.0f) { _rows += 1; }
        _dimSpX  = _dimSp*_columns;
        _dimSpY  = _dimSp*_rows;
        _imgSpheres = Godot.Image.Create(_dimSpX, _dimSpY, false, Godot.Image.Format.Rgba8);
        _imgSpheres.Fill(XB.Col.Transp);
        _trSpheres.Position = new Godot.Vector2(XB.AData.BaseResX-_offsetH-_dimSpX, _offsetT);
        _trSpheres.Size     = new Godot.Vector2(_dimSpX, _dimSpY);
        _texSpheres.SetImage(_imgSpheres);
        CreateSphereTexture(_dimSpY, _dimSp, _columns, _dimBord, _dimThick,
                            ref _imgSpheres, ref _texSpheres, ref _vect, ref _rects, ref _rSize);

        _imgMiniMapO = Godot.Image.Create(_dimMMX, _dimMMY, false, Godot.Image.Format.Rgba8);
        _imgMiniMapO.Fill(XB.Col.Transp);
        _trMiniMap.Position  = new Godot.Vector2(_offsetH, _offsetT);
        _trMiniMapO.Position = new Godot.Vector2(_offsetH, _offsetT);
        _trMiniMap.Size      = new Godot.Vector2(_dimMMX, _dimMMY);
        _trMiniMapO.Size     = new Godot.Vector2(_dimMMX, _dimMMY);
        _texMiniMapO.SetImage(_imgMiniMapO);

        _colCross.A            = _crossAlpha;
        _trCrosshairs.Modulate = _colCross;
        _colSpheres.A          = _spheresAlpha;
        _trSpheres.Modulate    = _colSpheres;
        _colLinking.A          = _linkingAlpha;
        _trLinking.Modulate    = _colLinking;
        _colMiniMap.A          = _miniMapAlpha;
        _trMiniMap.Modulate    = _colMiniMap;
        _trMiniMapO.Modulate   = _colMiniMap;
        _blockMultCur          = _blockMult;
    }

    public void InitializeMiniMap(ref Godot.Vector2I size) {
        ImgMiniMap = Godot.Image.Create(size.X, size.Y, false, Godot.Image.Format.L8);
        ImgMiniMap.Fill (XB.Col.Black);
        _texMiniMap.SetImage(ImgMiniMap);
    }

    public void UpdateMiniMap() {
        _texMiniMap.Update(ImgMiniMap);
    }

    //NOTE[ALEX]: colors are hardcoded because the texture is very specific
    private void CreateLinkingTexture(int thickness, int shadow, int cornerWidth, int cornerHeight,
                                      ref Godot.Image image, ref Godot.ImageTexture tex,
                                      ref Godot.Vector2I vect                                      ) {
        int width  = image.GetWidth();
        int height = image.GetHeight();

        var rect = new Godot.Rect2I(0, 0, width, height);
        image.FillRect(rect, XB.Col.LinkDim);
        XB.Utils.UpdateRect2I(shadow, shadow, width-2*shadow, height-2*shadow, ref rect, ref vect);
        image.FillRect(rect, XB.Col.LinkBri);
        XB.Utils.UpdateRect2I(thickness, thickness, width-2*thickness,
                              height-2*thickness, ref rect, ref vect  );
        image.FillRect(rect, XB.Col.Transp);
        XB.Utils.UpdateRect2I(0, cornerHeight, width, height-2*cornerHeight, ref rect, ref vect);
        image.FillRect(rect, XB.Col.Transp);
        XB.Utils.UpdateRect2I(cornerWidth, 0, width-2*cornerWidth, height, ref rect, ref vect);
        image.FillRect(rect, XB.Col.Transp);

        tex.Update(image);
    }

    private void CreateSphereTexture(int dimSpY, int dimSp, int columns, int dimBord, int dimThick,
                                     ref Godot.Image image, ref Godot.ImageTexture tex, 
                                     ref Godot.Vector2I vect, ref Godot.Rect2I[] rects, ref int rSize) {
        int xStart  = 0;
        int yStart  = dimSpY-dimSp;
        int counter = 0;
        for (int i = 0; i < XB.Manager.MaxSphereAmount; i++ ) {
            xStart   = counter*dimSp;
            counter += 1;
            counter %= columns;

            XB.Utils.BeveledRectangle(xStart, yStart, dimSp-2*dimThick, 
                                      ref rects, ref rSize, ref vect   );
            for (int j = 0; j < rSize; j++ ) { image.FillRect(rects[j], XB.Col.Outline); }

            XB.Utils.BeveledRectangle(xStart+dimBord, yStart+dimBord,
                                      dimSp-2*dimBord-2*dimThick, 
                                      ref rects, ref rSize, ref vect );
            for (int j = 0; j < rSize; j++ ) { image.FillRect(rects[j], XB.Col.InAct); }

            AddDigitTexture(i, xStart+2*dimBord, yStart+2*dimBord, dimThick, ref XB.Col.White);
            if (counter == 0) {
                yStart -= dimSp;
            }
        }
        tex.Update(image);
    }

    private void AddDigitTexture(int digit, int xStart, int yStart,
                                 int thickness, ref Godot.Color digitColor) {
        int ySize = _dimSp -4*_dimBord -2*_dimThick;
        int xSize = ySize/2;
        if (digit > 9) { // decimal digit
            XB.Utils.DigitRectangles(digit/10, xStart, yStart, xSize, ySize, thickness,
                                     ref _rects, ref _rSize, ref _vect                 );
            for (int i = 0; i < _rSize; i++ ) { _imgSpheres.FillRect(_rects[i], digitColor); }
            xStart += xSize;
        }
        XB.Utils.DigitRectangles(digit%10, xStart, yStart, xSize, ySize, thickness,
                                 ref _rects, ref _rSize, ref _vect                 );
        for (int i = 0; i < _rSize; i++ ) { _imgSpheres.FillRect(_rects[i], digitColor); }
    }

    public void UpdateSphereTexture(int id, XB.SphereTexSt state) {
        int xStart = (id%_columns)*_dimSp;
        int yOff   = (id/_columns)*_dimSp;
        int yStart = _dimSpY-yOff-_dimSp;
        Godot.Color pColor = new Godot.Color(0.0f, 0.0f, 0.0f, 0.0f);
        Godot.Color dColor = new Godot.Color(0.0f, 0.0f, 0.0f, 0.0f);
        switch (state) {
            case XB.SphereTexSt.Inactive:      { pColor = XB.Col.InAct;   dColor = XB.Col.White; break; }
            case XB.SphereTexSt.Active:        { pColor = XB.Col.Act;     dColor = XB.Col.Black; break; }
            case XB.SphereTexSt.ActiveLinking: { pColor = XB.Col.LinkBri; dColor = XB.Col.Black; break; }
            case XB.SphereTexSt.ActiveLinked:  { pColor = XB.Col.LinkDim; dColor = XB.Col.Black; break; }
        }

        XB.Utils.BeveledRectangle(xStart+_dimBord, yStart+_dimBord,
                                  _dimSp-2*_dimBord-2*_dimThick, 
                                  ref _rects, ref _rSize, ref _vect);
        for (int i = 0; i < _rSize; i++ ) { _imgSpheres.FillRect(_rects[i], pColor); }

        AddDigitTexture(id, xStart+2*_dimBord, yStart+2*_dimBord, _dimThick, ref dColor);
        _texSpheres.Update(_imgSpheres);
    }

    public void UpdateSphereTextureHighlight(int from, int to) {
        if (from < XB.Manager.MaxSphereAmount) {
            int xStart = (from%_columns)*_dimSp;
            int yOff   = (from/_columns)*_dimSp;
            int yStart = _dimSpY-yOff-_dimSp;

            XB.Utils.RectangleOutline(xStart, yStart, _dimSp, _dimThick, 
                                      ref _rects, ref _rSize, ref _vect );
            for (int i = 0; i < _rSize; i++ ) { _imgSpheres.FillRect(_rects[i], XB.Col.Transp); }
        }
        if (to < XB.Manager.MaxSphereAmount) {
            int xStart = (to%_columns)*_dimSp;
            int yOff   = (to/_columns)*_dimSp;
            int yStart = _dimSpY-yOff-_dimSp;
            
            XB.Utils.RectangleOutline(xStart, yStart, _dimSp, _dimThick, 
                                      ref _rects, ref _rSize, ref _vect );
            for (int i = 0; i < _rSize; i++ ) { _imgSpheres.FillRect(_rects[i], XB.Col.Hl); }
        }
        _texSpheres.Update(_imgSpheres);
    }

    private void UpdateMiniMapOverlayTexture() {
        _imgMiniMapO.Fill(XB.Col.Transp);

        //NOTE[ALEX]: world corner is at 0|0, calculate % offset from corner
        //            texture starts at top right corner with 0|0,
        //            world has z+ goind forward and x+ going left,
        //            so 0|0 in world coordinates would be 1|1 in texture coordinates
        float posX = XB.PController.PModel.GlobalPosition.X/XB.WorldData.WorldDim.X;
        float posZ = XB.PController.PModel.GlobalPosition.Z/XB.WorldData.WorldDim.Y;
        int   x    = (int)((1.0f-posX)*(float)_dimMMX);
        int   z    = (int)((1.0f-posZ)*(float)_dimMMY);
        XB.Utils.PointRectangles(x, z, _dimMMPl, ref _rects, ref _rSize, ref _vect);
        for (int i = 0; i < _rSize; i++) { _imgMiniMapO.FillRect(_rects[i], XB.Col.MPlayer); }

        for (int i = 0; i < XB.Manager.Spheres.Length; i++) {
            if (!XB.Manager.Spheres[i].Active) { continue; }

            posX = XB.Manager.Spheres[i].GlobalPosition.X/XB.WorldData.WorldDim.X;
            posZ = XB.Manager.Spheres[i].GlobalPosition.Z/XB.WorldData.WorldDim.Y;
            x    = (int)((1.0f-posX)*(float)_dimMMX);
            z    = (int)((1.0f-posZ)*(float)_dimMMY);
            XB.Utils.PointRectangles(x, z, _dimMMSp, ref _rects, ref _rSize, ref _vect);

            Godot.Color spColor = new Godot.Color(0.0f, 0.0f, 0.0f, 0.0f);
            switch (XB.Manager.Spheres[i].TexSt) {
                case XB.SphereTexSt.Inactive:      { spColor = XB.Col.InAct;   break; }
                case XB.SphereTexSt.Active:        { spColor = XB.Col.Act;     break; }
                case XB.SphereTexSt.ActiveLinking: { spColor = XB.Col.LinkBri; break; }
                case XB.SphereTexSt.ActiveLinked:  { spColor = XB.Col.LinkDim; break; }
            }
            for (int j = 0; j < _rSize; j++) { _imgMiniMapO.FillRect(_rects[j], spColor); }
        }

        _texMiniMapO.Update(_imgMiniMapO);
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
            _miniMapAlpha = 1.0f;
            _blockMult    = 1.0f;
        } else {
            _crossAlpha   = 0.0f;
            _fpsAlpha     = 0.0f;
            _spheresAlpha = 0.0f;
            _linkingAlpha = 0.0f;
            _miniMapAlpha = 0.0f;
            _blockMult    = 0.0f;
        }

        _colCross.A            = XB.Utils.LerpF(_colCross.A, _crossAlpha, _hudSm*dt);
        _trCrosshairs.Modulate = _colCross;
        _colFps.A              = XB.Utils.LerpF(_colFps.A, _fpsAlpha, _hudSm*dt);
        _lbFps.AddThemeColorOverride("font_color", _colFps);
        _colSpheres.A          = XB.Utils.LerpF(_colSpheres.A, _spheresAlpha, _hudSm*dt);
        _trSpheres.Modulate    = _colSpheres;
        _colLinking.A          = XB.Utils.LerpF(_colLinking.A, _linkingAlpha, _hudSm*dt);
        _trLinking.Modulate    = _colLinking;
        _colMiniMap.A          = XB.Utils.LerpF(_colMiniMap.A, _miniMapAlpha, _hudSm*dt);
        _trMiniMap.Modulate    = _colMiniMap;
        _trMiniMapO.Modulate   = _colMiniMap;
        _blockMultCur          = XB.Utils.LerpF(_blockMultCur, _blockMult, _hudSm*dt); 
        XB.WorldData.UpdateBlockStrength(_blockMultCur);

        UpdateMiniMapOverlayTexture();

        _lbFps.Text = Godot.Engine.GetFramesPerSecond().ToString();

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
