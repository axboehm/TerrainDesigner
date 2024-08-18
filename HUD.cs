#define XBDEBUG
namespace XB { // namespace open
public enum SphereTexSt {
    Inactive,
    Active,
    ActiveLinking,
    ActiveLinked,
}

public partial class HUD : Godot.Control {
    [Godot.Export] private Godot.NodePath       _matHudEffectsNode;
                   private Godot.ShaderMaterial _matHudEff;

    private Godot.Label       _lbFps;
    private Godot.TextureRect _trCrosshairs;
    private Godot.TextureRect _trLinking;
    private Godot.TextureRect _trSpheres;
    private Godot.TextureRect _trSpheresBG;
    private Godot.TextureRect _trMiniMap;
    private Godot.TextureRect _trMiniMapQT;
    private Godot.TextureRect _trMiniMapO;
    private Godot.TextureRect _trMiniMapG;
    private Godot.TextureRect _trMiniMapBG;
    private Godot.TextureRect _trGuideBG;
    private Godot.Label       _lbHeightL;
    private Godot.Label       _lbHeightH;

    private bool        _hudVisible      = true;
    public  bool        CrossVisible     = true;
    public  bool        FpsVisible       = false;
    public  bool        BlockGridVisible = false;
    public  bool        QTreeVisible     = false;
    public  bool        SpheresVisible   = true;
    private float       _hudSm           = 16.0f;
    private float       _crossAlpha      = 0.0f;
    private Godot.Color _colCross        = new Godot.Color(1.0f, 1.0f, 1.0f, 1.0f);
    private float       _fpsAlpha        = 0.0f;
    private Godot.Color _colFps          = new Godot.Color(1.0f, 1.0f, 1.0f, 1.0f);
    private Godot.Color _colMMLegend     = new Godot.Color(1.0f, 1.0f, 1.0f, 1.0f);
    private Godot.Color _colMMLegendOut  = new Godot.Color(1.0f, 1.0f, 1.0f, 1.0f);
    private float       _spheresAlpha    = 0.0f;
    private Godot.Color _colSpheres      = new Godot.Color(1.0f, 1.0f, 1.0f, 1.0f);
    private float       _linkingAlpha    = 0.0f;
    private Godot.Color _colLinking      = new Godot.Color(1.0f, 1.0f, 1.0f, 1.0f);
    private float       _miniMapAlpha    = 0.0f;
    private Godot.Color _colMiniMap      = new Godot.Color(1.0f, 1.0f, 1.0f, 1.0f);
    private float       _blockMultCur    = 1.0f;
    private float       _blockMult       = 0.0f;
    private float       _qtreeMultCur    = 1.0f;
    private float       _qtreeMult       = 0.0f;
    private Godot.Color _colMiniMapQT    = new Godot.Color(1.0f, 1.0f, 1.0f, 1.0f);

    private Godot.Vector2I _vect  = new Godot.Vector2I(0, 0); // reusable vector for Rect2I
    private Godot.Rect2I[] _rects = new Godot.Rect2I[XB.Utils.MaxRectSize];
    private int            _rSize = 0; // how many entries in _rects were used by function

    private const int          _offsetT = 52; // offset from top screen edge (at base res 1920x1080)
    private const int          _offsetH = 4;  // horizontal offset from side screen edges

    private const int          _dimCrosshairs = 8;

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
    private Godot.Image        _imgSpheresBG;
    private Godot.ImageTexture _texSpheresBG;
    private int                _texMaxY  = 1024;
    private int                _dimSpX   = 0;
    private int                _dimSpY   = 0;
    private const int          _dimSp    = 32;
    private const int          _dimBord  = 3;
    private const int          _dimThick = 1;
    private int                _rows     = 0;
    private int                _columns  = 0;

    public  Godot.ImageTexture TexMiniMap;
    private Godot.Image        _imgMiniMapQT;
    private Godot.ImageTexture _texMiniMapQT;
    private Godot.Image        _imgMiniMapO;
    private Godot.ImageTexture _texMiniMapO;
    private Godot.Image        _imgMiniMapG;
    private Godot.ImageTexture _texMiniMapG;
    private Godot.Image        _imgMiniMapBG;
    private Godot.ImageTexture _texMiniMapBG;
    private const int          _dimMMX  = 256;
    private const int          _dimMMY  = 256;
    private const int          _dimMMGY = 16;
    private const int          _dimMMPl = 10;
    private const int          _dimMMSp = 6;
    private const float        _playerTriWidth  = 6.0f;
    private const float        _playerTriHeight = 10.0f;
    private const float        _sphereCirRadius = 3.0f;
    private const string       _heightFormat    = "F2";
    private const int          _gradLbFontSize  = 16;
    private const int          _gradLbOutlSize  = 2;
    private Godot.Vector2[]    _spherePositions; // to pass to the shader
    private Godot.Color[]      _sphereColors;

    private Godot.ShaderMaterial _matMiniMapO;

    //TODO[ALEX]: on screen guide
    private Godot.Image        _imgGuideBG;
    private Godot.ImageTexture _texGuideBG;


    public void InitializeHud() {
#if XBDEBUG
        var debug = new XB.DebugTimedBlock(XB.D.HUDInitializeHud);
#endif

        var font = Godot.ResourceLoader.Load<Godot.Font>(XB.ResourcePaths.FontLibMono);

        _matHudEff = (Godot.ShaderMaterial)GetNode<Godot.TextureRect>(_matHudEffectsNode).Material;

        _trCrosshairs = new Godot.TextureRect();
        _trLinking    = new Godot.TextureRect();
        _trSpheres    = new Godot.TextureRect();
        _trSpheresBG  = new Godot.TextureRect();
        _trMiniMap    = new Godot.TextureRect();
        _trMiniMapQT  = new Godot.TextureRect();
        _trMiniMapO   = new Godot.TextureRect();
        _trMiniMapG   = new Godot.TextureRect();
        _trMiniMapBG  = new Godot.TextureRect();
        // order matters (back to front)
        AddChild(_trLinking);
        AddChild(_trSpheresBG);
        AddChild(_trSpheres);
        AddChild(_trMiniMapBG);
        AddChild(_trMiniMap);
        AddChild(_trMiniMapQT);
        AddChild(_trMiniMapO);
        AddChild(_trMiniMapG);
        AddChild(_trCrosshairs);
        _texLinking   = new Godot.ImageTexture();
        _texSpheres   = new Godot.ImageTexture();
        _texSpheresBG = new Godot.ImageTexture();
        TexMiniMap    = new Godot.ImageTexture();
        _texMiniMapQT = new Godot.ImageTexture();
        _texMiniMapO  = new Godot.ImageTexture();
        _texMiniMapG  = new Godot.ImageTexture();
        _texMiniMapBG = new Godot.ImageTexture();
        _lbHeightL    = new Godot.Label();
        _lbHeightH    = new Godot.Label();
        _lbFps        = new Godot.Label();
        AddChild(_lbHeightL);
        AddChild(_lbHeightH);
        AddChild(_lbFps);

        for (int i = 0; i < _rects.Length; i++) { _rects[i] = new Godot.Rect2I(0, 0, 0, 0); }

        var texCrosshairs = Godot.ResourceLoader.Load<Godot.Texture2D>(XB.ResourcePaths.CrosshairsTex);
        _trCrosshairs.Texture     = texCrosshairs;
        var crosshairsSize = new Godot.Vector2I(_dimCrosshairs, _dimCrosshairs);
        _trCrosshairs.Position    = new Godot.Vector2I(XB.AData.BaseResX/2 - crosshairsSize.X/2,
                                                       XB.AData.BaseResY/2 - crosshairsSize.Y/2);
        _trCrosshairs.ExpandMode  = Godot.TextureRect.ExpandModeEnum.IgnoreSize;
        _trCrosshairs.StretchMode = Godot.TextureRect.StretchModeEnum.Scale;
        _trCrosshairs.Size        = crosshairsSize;

        int sizeLinkingX = XB.AData.BaseResX-2*_dimLinkBorderX;
        int sizeLinkingY = XB.AData.BaseResY-2*_dimLinkBorderY;
        _imgLinking = Godot.Image.Create(sizeLinkingX, sizeLinkingY,
                                         false, Godot.Image.Format.Rgba8);
        _texLinking.SetImage(_imgLinking);
        _trLinking.Position    = new Godot.Vector2I(_dimLinkBorderX, _dimLinkBorderY);
        _trLinking.ExpandMode  = Godot.TextureRect.ExpandModeEnum.IgnoreSize;
        _trLinking.StretchMode = Godot.TextureRect.StretchModeEnum.Scale;
        _trLinking.Size        = new Godot.Vector2I(sizeLinkingX, sizeLinkingY);
        _trLinking.Texture     = _texLinking;
        CreateLinkingTexture(_dimLinkThick, _dimLinkShadow,
                             (int)((float)sizeLinkingX*_dimLinkCorX),
                             (int)((float)sizeLinkingY*_dimLinkCorY), 
                             ref _imgLinking, ref _texLinking, ref _vect);

        float columns = (float)XB.ManagerSphere.MaxSphereAmount/((float)_texMaxY/(float)_dimSp);
             _columns = (int)columns;
        if (columns%1.0f > 0.0f) { _columns += 1; }
        float rows = (float)XB.ManagerSphere.MaxSphereAmount/(float)_columns;
             _rows = (int)rows;
        if (rows%1.0f > 0.0f) { _rows += 1; }
        _dimSpX = _dimSp*_columns;
        _dimSpY = _dimSp*_rows;
        _imgSpheres   = Godot.Image.Create(_dimSpX, _dimSpY, false, Godot.Image.Format.Rgba8);
        _imgSpheres.Fill(XB.Col.Transp);
        _imgSpheresBG = Godot.Image.Create(_dimSpX, _dimSpY, false, Godot.Image.Format.Rgba8);
        _imgSpheresBG.Fill(XB.Col.BG);
        var spheresPosition = new Godot.Vector2I(XB.AData.BaseResX - 2*_offsetH - _dimSpX,
                                                 _offsetT - _offsetH                      );
        _trSpheres.Position      = spheresPosition;
        _trSpheresBG.Position    = spheresPosition - new Godot.Vector2I(_offsetH, _offsetH);
        _trSpheres.ExpandMode    = Godot.TextureRect.ExpandModeEnum.IgnoreSize;
        _trSpheresBG.ExpandMode  = Godot.TextureRect.ExpandModeEnum.IgnoreSize;
        _trSpheres.StretchMode   = Godot.TextureRect.StretchModeEnum.Scale;
        _trSpheresBG.StretchMode = Godot.TextureRect.StretchModeEnum.Scale;
        _trSpheres.Size          = new Godot.Vector2I(_dimSpX, _dimSpY);
        _trSpheresBG.Size        = _trSpheres.Size + new Godot.Vector2I(2*_offsetH, 2*_offsetH);
        _trSpheres.Texture       = _texSpheres;
        _trSpheresBG.Texture     = _texSpheresBG;
        _texSpheres.SetImage(_imgSpheres);
        _texSpheresBG.SetImage(_imgSpheresBG);
        CreateSphereTexture(_dimSpY, _dimSp, _columns, _dimBord, _dimThick,
                            ref _imgSpheres, ref _texSpheres, ref _vect, ref _rects, ref _rSize);

        _imgMiniMapQT = Godot.Image.Create(_dimMMX, _dimMMY, false, Godot.Image.Format.Rgba8);
        _imgMiniMapQT.Fill(XB.Col.Transp);
        _imgMiniMapO  = Godot.Image.Create(_dimMMX, _dimMMY, false, Godot.Image.Format.Rgba8);
        _imgMiniMapO.Fill(XB.Col.Transp);
        _imgMiniMapG  = Godot.Image.Create(_dimMMX, _dimMMGY, false, Godot.Image.Format.L8);
        _imgMiniMapG.Fill(XB.Col.Black);
        int dimMMBGX  = _dimMMX + 2*_offsetH;
        int dimMMBGY  = _dimMMY + 2*_offsetH + 3*_dimMMSp + _dimMMGY + _gradLbFontSize;
        _imgMiniMapBG = Godot.Image.Create(dimMMBGX, dimMMBGY, false, Godot.Image.Format.Rgba8);
        _imgMiniMapBG.Fill(XB.Col.BG);
        var miniMapPosition = new Godot.Vector2I(2*_offsetH, XB.AData.BaseResY/2 - dimMMBGY/2);
        _trMiniMap.Position      = miniMapPosition;
        _trMiniMapQT.Position    = miniMapPosition;
        _trMiniMapO.Position     = miniMapPosition;
        _trMiniMapG.Position     = miniMapPosition + new Godot.Vector2I(0, _dimMMY + _dimMMSp);
        _trMiniMapBG.Position    = miniMapPosition - new Godot.Vector2I(_offsetH, _offsetH);
        _trMiniMap.ExpandMode    = Godot.TextureRect.ExpandModeEnum.IgnoreSize;
        _trMiniMapQT.ExpandMode  = Godot.TextureRect.ExpandModeEnum.IgnoreSize;
        _trMiniMapO.ExpandMode   = Godot.TextureRect.ExpandModeEnum.IgnoreSize;
        _trMiniMapG.ExpandMode   = Godot.TextureRect.ExpandModeEnum.IgnoreSize;
        _trMiniMapBG.ExpandMode  = Godot.TextureRect.ExpandModeEnum.IgnoreSize;
        _trMiniMap.StretchMode   = Godot.TextureRect.StretchModeEnum.Scale;
        _trMiniMapQT.StretchMode = Godot.TextureRect.StretchModeEnum.Scale;
        _trMiniMapO.StretchMode  = Godot.TextureRect.StretchModeEnum.Scale;
        _trMiniMapG.StretchMode  = Godot.TextureRect.StretchModeEnum.Scale;
        _trMiniMapBG.StretchMode = Godot.TextureRect.StretchModeEnum.Scale;
        _trMiniMap.Size          = new Godot.Vector2I(_dimMMX, _dimMMY);
        _trMiniMapQT.Size        = new Godot.Vector2I(_dimMMX, _dimMMY);
        _trMiniMapO.Size         = new Godot.Vector2I(_dimMMX, _dimMMY);
        _trMiniMapG.Size         = new Godot.Vector2I(_dimMMX, _dimMMGY);
        _trMiniMapBG.Size        = new Godot.Vector2I(dimMMBGX, dimMMBGY);
        _trMiniMap.Texture       = TexMiniMap;
        _trMiniMapQT.Texture     = _texMiniMapQT;
        _trMiniMapO.Texture      = _texMiniMapO;
        _trMiniMapG.Texture      = _texMiniMapG;
        _trMiniMapBG.Texture     = _texMiniMapBG;
        _texMiniMapQT.SetImage(_imgMiniMapQT);
        _texMiniMapO.SetImage (_imgMiniMapO);
        _texMiniMapG.SetImage (_imgMiniMapG);
        _texMiniMapBG.SetImage(_imgMiniMapBG);
        CreateGradientTexture(ref _imgMiniMapG, ref _texMiniMapG, ref _vect, ref _rects);
        _matMiniMapO = new Godot.ShaderMaterial();
        _matMiniMapO.Shader = Godot.ResourceLoader.Load<Godot.Shader>(XB.ResourcePaths.MiniMapOShader);
        _matMiniMapO.SetShaderParameter("plColor", XB.Col.MPlayer);
        _matMiniMapO.SetShaderParameter("squareRatio", (float)_dimMMY/(float)_dimMMX);
        _matMiniMapO.SetShaderParameter("halfWidth",  (_playerTriWidth/_dimMMX)/2.0f);
        _matMiniMapO.SetShaderParameter("halfHeight", (_playerTriHeight/_dimMMY)/2.0f);
        _matMiniMapO.SetShaderParameter("spRadius", (_sphereCirRadius/_dimMMY));
        _spherePositions = new Godot.Vector2[XB.ManagerSphere.MaxSphereAmount];
        _sphereColors    = new Godot.Color[XB.ManagerSphere.MaxSphereAmount];
        _trMiniMapO.Material = _matMiniMapO;

        var mmLabelPosition = miniMapPosition + new Godot.Vector2I(0, _dimMMY + 2*_dimMMSp + _dimMMGY);
        var mmLabelSize     = new Godot.Vector2I(_dimMMX, _dimMMY);
        _lbHeightL.Position = mmLabelPosition;
        _lbHeightH.Position = mmLabelPosition;
        _lbHeightL.Size     = mmLabelSize;
        _lbHeightH.Size     = mmLabelSize;
        _lbHeightL.HorizontalAlignment = Godot.HorizontalAlignment.Left;
        _lbHeightH.HorizontalAlignment = Godot.HorizontalAlignment.Right;
        _lbHeightL.VerticalAlignment   = Godot.VerticalAlignment.Top;
        _lbHeightL.VerticalAlignment   = Godot.VerticalAlignment.Top;
        _lbHeightL.Text = "low";
        _lbHeightH.Text = "high";
        _lbHeightL.AddThemeFontOverride    ("font",               font           );
        _lbHeightH.AddThemeFontOverride    ("font",               font           );
        _lbHeightL.AddThemeFontSizeOverride("font_size",          _gradLbFontSize);
        _lbHeightH.AddThemeFontSizeOverride("font_size",          _gradLbFontSize);
        _lbHeightL.AddThemeConstantOverride("outline_size",       _gradLbOutlSize);
        _lbHeightH.AddThemeConstantOverride("outline_size",       _gradLbOutlSize);
        _colMMLegend    = XB.Col.White;
        _colMMLegendOut = XB.Col.Black;
        _lbHeightL.AddThemeColorOverride   ("font_color",         _colMMLegend   );
        _lbHeightH.AddThemeColorOverride   ("font_color",         _colMMLegend   );
        _lbHeightL.AddThemeColorOverride   ("font_outline_color", _colMMLegendOut);
        _lbHeightH.AddThemeColorOverride   ("font_outline_color", _colMMLegendOut);

        _lbFps.Position = new Godot.Vector2I(_offsetH, _offsetT);
        _lbFps.Size     = new Godot.Vector2I(_dimMMX, _gradLbFontSize);
        _lbFps.HorizontalAlignment = Godot.HorizontalAlignment.Left;
        _lbFps.VerticalAlignment   = Godot.VerticalAlignment.Top;
        _colFps   = XB.Col.White;
        _colFps.A = 0.0f;
        _lbFps.AddThemeColorOverride("font_color", _colFps);

        _colCross.A            = _crossAlpha;
        _trCrosshairs.Modulate = _colCross;
        _colSpheres.A          = _spheresAlpha;
        _trSpheres.Modulate    = _colSpheres;
        _trSpheresBG.Modulate  = _colSpheres;
        _colLinking.A          = _linkingAlpha;
        _trLinking.Modulate    = _colLinking;
        _colMiniMap.A          = _miniMapAlpha;
        _trMiniMap.Modulate    = _colMiniMap;
        _trMiniMapO.Modulate   = _colMiniMap;
        _trMiniMapG.Modulate   = _colMiniMap;
        _trMiniMapBG.Modulate  = _colMiniMap;
        _blockMultCur          = _blockMult;
        _qtreeMultCur          = _qtreeMult;

#if XBDEBUG
        debug.End();
#endif 
    }

    public void UpdateMiniMap(float low, float high) {
#if XBDEBUG
        var debug = new XB.DebugTimedBlock(XB.D.HUDUpdateMiniMap);
#endif

        TexMiniMap.SetImage(XB.WorldData.ImgMiniMap);
        TexMiniMap.Update(XB.WorldData.ImgMiniMap);
        _lbHeightL.Text = low.ToString (_heightFormat) + "m";
        _lbHeightH.Text = high.ToString(_heightFormat) + "m";

#if XBDEBUG
        debug.End();
#endif 
    }

    //NOTE[ALEX]: colors are hardcoded because the texture is very specific
    private void CreateLinkingTexture(int thickness, int shadow, int cornerWidth, int cornerHeight,
                                      ref Godot.Image image, ref Godot.ImageTexture tex,
                                      ref Godot.Vector2I vect                                      ) {
#if XBDEBUG
        var debug = new XB.DebugTimedBlock(XB.D.HUDCreateLinkingTexture);
#endif

        image.Fill(XB.Col.Transp);

        int radius = 100;
        int thick = 20;
        int width  = image.GetWidth();
        int height = image.GetHeight();

        for (int i = width/2-radius; i < width/2+radius; i++) {
            for (int j = height/2-radius; j < height/2+radius; j++) {
                float d = XB.Utils.CircleSDF(i, j, width/2, height/2, radius);
                if (d < 0 && d > -thick) {
                    image.SetPixel(i, j, XB.Col.LinkBri);
                } else {
                    image.SetPixel(i, j, XB.Col.Transp);
                }
            }
        }

        tex.Update(image);

#if XBDEBUG
        debug.End();
#endif 
    }

    private void CreateSphereTexture(int dimSpY, int dimSp, int columns, int dimBord, int dimThick,
                                     ref Godot.Image image, ref Godot.ImageTexture tex, 
                                     ref Godot.Vector2I vect, ref Godot.Rect2I[] rects, ref int rSize) {
#if XBDEBUG
        var debug = new XB.DebugTimedBlock(XB.D.HUDCreateSphereTexture);
#endif

        int xStart  = 0;
        int yStart  = dimSpY-dimSp;
        int counter = 0;
        for (int i = 0; i < XB.ManagerSphere.MaxSphereAmount; i++ ) {
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

#if XBDEBUG
        debug.End();
#endif 
    }

    private void CreateGradientTexture(ref Godot.Image image, ref Godot.ImageTexture tex,
                                       ref Godot.Vector2I vect, ref Godot.Rect2I[] rects ) {
#if XBDEBUG
        var debug = new XB.DebugTimedBlock(XB.D.HUDCreateGradientTexture);
#endif

        float step = 1.0f/(tex.GetWidth()-1);
        var col = new Godot.Color(0.0f, 0.0f, 0.0f, 1.0f);
        for (int i = 0; i < tex.GetWidth(); i++) {
            int xStart = (int)( (float)i/(tex.GetWidth()-1) * (tex.GetWidth()-1) );
            XB.Utils.UpdateRect2I(xStart, 0, 1, tex.GetHeight(), ref rects[0], ref vect);
            col.B = i*step; // in L8 image, only blue channel is used
            image.FillRect(rects[0], col);
        }
        tex.Update(image);

#if XBDEBUG
        debug.End();
#endif 
    }

    private void AddDigitTexture(int digit, int xStart, int yStart,
                                 int thickness, ref Godot.Color digitColor) {
#if XBDEBUG
        var debug = new XB.DebugTimedBlock(XB.D.HUDAddDigitTexture);
#endif

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

#if XBDEBUG
        debug.End();
#endif 
    }

    public void UpdateSphereTexture(int id, XB.SphereTexSt state) {
#if XBDEBUG
        var debug = new XB.DebugTimedBlock(XB.D.HUDUpdateSphereTexture);
#endif

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

#if XBDEBUG
        debug.End();
#endif 
    }

    public void UpdateSphereTextureHighlight(int from, int to) {
#if XBDEBUG
        var debug = new XB.DebugTimedBlock(XB.D.HUDUpdateSphereTextureHighlight);
#endif

        if (from < XB.ManagerSphere.MaxSphereAmount) {
            int xStart = (from%_columns)*_dimSp;
            int yOff   = (from/_columns)*_dimSp;
            int yStart = _dimSpY-yOff-_dimSp;

            XB.Utils.RectangleOutline(xStart, yStart, _dimSp, _dimThick, 
                                      ref _rects, ref _rSize, ref _vect );
            for (int i = 0; i < _rSize; i++ ) { _imgSpheres.FillRect(_rects[i], XB.Col.Transp); }
        }
        if (to < XB.ManagerSphere.MaxSphereAmount) {
            int xStart = (to%_columns)*_dimSp;
            int yOff   = (to/_columns)*_dimSp;
            int yStart = _dimSpY-yOff-_dimSp;
            
            XB.Utils.RectangleOutline(xStart, yStart, _dimSp, _dimThick, 
                                      ref _rects, ref _rSize, ref _vect );
            for (int i = 0; i < _rSize; i++ ) { _imgSpheres.FillRect(_rects[i], XB.Col.Hl); }
        }
        _texSpheres.Update(_imgSpheres);

#if XBDEBUG
        debug.End();
#endif 
    }

    private void UpdateMiniMapOverlayTexture() {
#if XBDEBUG
        var debug = new XB.DebugTimedBlock(XB.D.HUDUpdateMiniMapOverlayTexture);
#endif

        _imgMiniMapO.Fill(XB.Col.Transp);

        //NOTE[ALEX]: world corner is at 0|0, calculate % offset from corner
        //            texture starts at top right corner with 0|0,
        //            world has z+ going forward and x+ going left,
        //            so 0|0 in world coordinates is 0|0 in world coordinates
        //            but the axes of the terrain in world space go in negative direction
        float posX = -XB.PController.PModel.GlobalPosition.X/XB.WorldData.WorldDim.X;
        float posZ = -XB.PController.PModel.GlobalPosition.Z/XB.WorldData.WorldDim.Y;
        float xDir = XB.PController.CCtrH.GlobalTransform.Basis.Z.X; // X coordinate of Z basis vector
        float zDir = XB.PController.CCtrH.GlobalTransform.Basis.Z.Z;

        _matMiniMapO.SetShaderParameter("plPosX", posX);
        _matMiniMapO.SetShaderParameter("plPosZ", posZ);
        _matMiniMapO.SetShaderParameter("plDirX", xDir);
        _matMiniMapO.SetShaderParameter("plDirZ", zDir);

        for (int i = 0; i < XB.ManagerSphere.Spheres.Length; i++) {
            if (!XB.ManagerSphere.Spheres[i].Active) { 
                _spherePositions[i].X = -1.0f; // move sphere out of bounds of texture
                _spherePositions[i].Y = -1.0f; // to effectively ignore it
                _sphereColors[i]      = XB.Col.Transp;
            } else {
                posX = -XB.ManagerSphere.Spheres[i].GlobalPosition.X/XB.WorldData.WorldDim.X;
                posZ = -XB.ManagerSphere.Spheres[i].GlobalPosition.Z/XB.WorldData.WorldDim.Y;
                _spherePositions[i].X = posX;
                _spherePositions[i].Y = posZ;

                switch (XB.ManagerSphere.Spheres[i].TexSt) {
                    case XB.SphereTexSt.Inactive:      { _sphereColors[i] = XB.Col.InAct;   break; }
                    case XB.SphereTexSt.Active:        { _sphereColors[i] = XB.Col.Act;     break; }
                    case XB.SphereTexSt.ActiveLinking: { _sphereColors[i] = XB.Col.LinkBri; break; }
                    case XB.SphereTexSt.ActiveLinked:  { _sphereColors[i] = XB.Col.LinkDim; break; }
                }
            }
        }

        _matMiniMapO.SetShaderParameter("spPos", _spherePositions);
        _matMiniMapO.SetShaderParameter("spCol", _sphereColors);

        _texMiniMapO.Update(_imgMiniMapO);

#if XBDEBUG
        debug.End();
#endif 
    }

    public void ToggleHUD() {
        _hudVisible = !_hudVisible;
    }

    public void UpdateHUD(float dt) {
#if XBDEBUG
        var debug = new XB.DebugTimedBlock(XB.D.HUDUpdateHUD);
#endif

        if (_hudVisible) {
            if (CrossVisible) { _crossAlpha = 1.0f; }
            else              { _crossAlpha = 0.0f; }
            if (FpsVisible)   { _fpsAlpha   = 1.0f; }
            else              { _fpsAlpha   = 0.0f; }
            if (XB.ManagerSphere.Linking) { _linkingAlpha = 1.0f; }
            else                          { _linkingAlpha = 0.0f; }
            if (BlockGridVisible) { _blockMult = 1.0f; }
            else                  { _blockMult = 0.0f; }
            if (QTreeVisible)     { _qtreeMult = 1.0f; }
            else                  { _qtreeMult = 0.0f; }
            _spheresAlpha = 1.0f;
            _miniMapAlpha = 1.0f;
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
        _trSpheresBG.Modulate  = _colSpheres;
        _colLinking.A          = XB.Utils.LerpF(_colLinking.A, _linkingAlpha, _hudSm*dt);
        _trLinking.Modulate    = _colLinking;
        _colMiniMap.A          = XB.Utils.LerpF(_colMiniMap.A, _miniMapAlpha, _hudSm*dt);
        _trMiniMap.Modulate    = _colMiniMap;
        _trMiniMapO.Modulate   = _colMiniMap;
        _trMiniMapG.Modulate   = _colMiniMap;
        _trMiniMapBG.Modulate  = _colMiniMap;
        _colMMLegend.A         = XB.Utils.LerpF(_colMMLegend.A,    _miniMapAlpha, _hudSm*dt);
        _colMMLegendOut.A      = XB.Utils.LerpF(_colMMLegendOut.A, _miniMapAlpha, _hudSm*dt);
        _lbHeightL.AddThemeColorOverride   ("font_color",         _colMMLegend   );
        _lbHeightH.AddThemeColorOverride   ("font_color",         _colMMLegend   );
        _lbHeightL.AddThemeColorOverride   ("font_outline_color", _colMMLegendOut);
        _lbHeightH.AddThemeColorOverride   ("font_outline_color", _colMMLegendOut);
        _blockMultCur          = XB.Utils.LerpF(_blockMultCur, _blockMult, _hudSm*dt); 
        _qtreeMultCur          = XB.Utils.LerpF(_qtreeMultCur, _qtreeMult, _hudSm*dt); 
        XB.ManagerTerrain.UpdateBlockStrength(_blockMultCur);
        XB.ManagerTerrain.UpdateQTreeStrength(_qtreeMultCur);
        _colMiniMapQT.A        = XB.Utils.LerpF(_colMiniMapQT.A, _qtreeMult, _hudSm*dt);
        _trMiniMapQT.Modulate  = _colMiniMapQT;

        UpdateMiniMapOverlayTexture();

        if (XB.AData.QTreeVis) {
            XB.ManagerTerrain.UpdateQTreeTexture(ref _imgMiniMapQT,
                                                 _imgMiniMapQT.GetWidth()/XB.WorldData.WorldDim.X);
            _texMiniMapQT.Update(_imgMiniMapQT);
        }

        _lbFps.Text = Godot.Engine.GetFramesPerSecond().ToString();

#if XBDEBUG
        debug.End();
#endif 
    }
}
} // namespace close
