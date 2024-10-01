#define XBDEBUG
namespace XB { // namespace open

public enum SphereTexSt {
    Uninit,
    Inactive,
    Active,
    ActiveLinking,
    ActiveLinked,
}

public partial class HUD : Godot.Control {
    private Godot.Label       _lbFps;
    private Godot.TextureRect _trCrosshairs;
    private Godot.TextureRect _trLinking;   // overlay when in linking mode
    private Godot.TextureRect _trSpheres;   // shows status of all available spheres
    private Godot.TextureRect _trSpheresBG; // background for _trSpheres
    private Godot.TextureRect _trMiniMap;   // shows heightmap of terrain, player and spheres
    private Godot.TextureRect _trMiniMapQT; // shows quadtree visualization of terrain
    private Godot.TextureRect _trMiniMapG;  // black to white gradient below minimap
    private Godot.TextureRect _trMiniMapBG; // background for minimap information
    private Godot.TextureRect _trGuideName;
    private Godot.TextureRect _trGuideBG;
    private Godot.Label       _lbHeightL;
    private Godot.Label       _lbHeightH;

    private bool        _hudVisible      = true;
    public  bool        CrossVisible     = true;
    public  bool        FpsVisible       = false;
    public  bool        BlockGridVisible = false;
    public  bool        QTreeVisible     = false;
    public  bool        SpheresVisible   = true;
    public  bool        GuideVisible     = true;
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
    private float       _miniMapLinking  = 0.0f;
    private Godot.Color _colMiniMap      = new Godot.Color(1.0f, 1.0f, 1.0f, 1.0f);
    private float       _blockMultCur    = 1.0f;
    private float       _blockMult       = 0.0f;
    private float       _qtreeMultCur    = 1.0f;
    private float       _qtreeMult       = 0.0f;
    private Godot.Color _colMiniMapQT    = new Godot.Color(1.0f, 1.0f, 1.0f, 1.0f);
    private float       _guideAlpha      = 0.0f;
    private Godot.Color _colGuideBG      = new Godot.Color(1.0f, 1.0f, 1.0f, 1.0f);
    private Godot.Color _colGuideText    = new Godot.Color(1.0f, 1.0f, 1.0f, 1.0f);

    private Godot.Rect2I[] _rects = new Godot.Rect2I[XB.Utils.MaxRectSize];
    private int            _rSize = 0; // how many entries in _rects were used by function

    private const int          _offsetT = 52; // offset from top screen edge (at base res 1920x1080)
    private const int          _offsetE = 4;  // offset from bottom and side screen edges

    private const int          _dimCrosshairs = 8;

    private Godot.Image        _imgLinking;
    private Godot.ImageTexture _texLinking;

    private Godot.ShaderMaterial _matLinking;
    private const float          _linkingRingRadius = 0.5f;

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
    private Godot.Color        _dColor   = new Godot.Color(0.0f, 0.0f, 0.0f, 0.0f);
    private Godot.Color        _pColor   = new Godot.Color(0.0f, 0.0f, 0.0f, 0.0f);

    public  Godot.ImageTexture TexMiniMap;
    private Godot.Image        _imgMiniMapQT;
    private Godot.ImageTexture _texMiniMapQT;
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
    private const float        _playerOutlThick = 1.5f;
    private const float        _sphereCirRadius = 2.0f;
    private const int          _gradLbFontSize  = 16;
    private const int          _gradLbOutlSize  = 2;

    private Godot.ShaderMaterial _matMiniMap;
    private Godot.Vector2[]      _spherePositions;
    private Godot.Color[]        _sphereColors;

    private Godot.Image        _imgGuideName;
    private Godot.ImageTexture _texGuideName;
    private Godot.Label        _lbGuideName;
    private const string       _guideName   = "GUIDE";
    private const string       _guideUnused = "-";
    private const string       _guideSp     = " - ";
    private Godot.Image        _imgGuideBG;
    private Godot.ImageTexture _texGuideBG;
    private Godot.Label[]      _lbGuide;
    private string[,]          _guide;
    private const int          _dimGSegDiv = 3; // amount of segments
    private const int          _dimGY      = 128; // works well with five lines of text
    private const int          _guideLbFontSpacing  = 0;
    private const int          _guideNameLbFontSize = 32;
    private const int          _guideNameLbOutlSize = 6;
    private const int          _guideLbFontSize     = 20;
    private const int          _guideLbOutlSize     = 1;
    private const int          _offsetEGuide        = 8; // distance between sphere BG and guide BG
    private const int          _guideBordDetThick   = 1; // thickness of guide border detail


    public void InitializeHud() {
#if XBDEBUG
        var debug = new XB.DebugTimedBlock(XB.D.HUDInitializeHud);
#endif

        var font = Godot.ResourceLoader.Load<Godot.Font>(XB.ResourcePaths.FontLibMono);

        _trCrosshairs = new Godot.TextureRect();
        _trLinking    = new Godot.TextureRect();
        _texLinking   = new Godot.ImageTexture();
        _trSpheres    = new Godot.TextureRect();
        _texSpheres   = new Godot.ImageTexture();
        _trSpheresBG  = new Godot.TextureRect();
        _texSpheresBG = new Godot.ImageTexture();
        _trMiniMap    = new Godot.TextureRect();
        TexMiniMap    = new Godot.ImageTexture();
        _trMiniMapQT  = new Godot.TextureRect();
        _texMiniMapQT = new Godot.ImageTexture();
        _trMiniMapG   = new Godot.TextureRect();
        _texMiniMapG  = new Godot.ImageTexture();
        _trMiniMapBG  = new Godot.TextureRect();
        _texMiniMapBG = new Godot.ImageTexture();
        _trGuideName  = new Godot.TextureRect();
        _texGuideName = new Godot.ImageTexture();
        _trGuideBG    = new Godot.TextureRect();
        _texGuideBG   = new Godot.ImageTexture();
        _lbHeightL    = new Godot.Label();
        _lbHeightH    = new Godot.Label();
        _lbFps        = new Godot.Label();
        _lbGuide      = new Godot.Label[_dimGSegDiv];
        _lbGuideName  = new Godot.Label();
        for (int i = 0; i < _dimGSegDiv; i++) { _lbGuide[i] = new Godot.Label(); }
        // order matters (back to front)
        AddChild(_trLinking);
        AddChild(_trSpheresBG);
        AddChild(_trSpheres);
        AddChild(_trMiniMapBG);
        AddChild(_trMiniMap);
        AddChild(_trMiniMapQT);
        AddChild(_trMiniMapG);
        AddChild(_trCrosshairs);
        AddChild(_lbHeightL);
        AddChild(_lbHeightH);
        AddChild(_lbFps);
        AddChild(_trGuideName);
        _trGuideName.AddChild(_lbGuideName);
        AddChild(_trGuideBG);
        for (int i = 0; i < _dimGSegDiv; i++) { _trGuideBG.AddChild(_lbGuide[i]); }

        for (int i = 0; i < _rects.Length; i++) { _rects[i] = new Godot.Rect2I(0, 0, 0, 0); }

        var texCrosshairs = Godot.ResourceLoader.Load<Godot.Texture2D>(XB.ResourcePaths.CrosshairsTex);
        _trCrosshairs.Texture  = texCrosshairs;
        var crosshairsSize     = new Godot.Vector2I(_dimCrosshairs, _dimCrosshairs);
        _trCrosshairs.Position = new Godot.Vector2I(XB.Settings.BaseResX/2 - crosshairsSize.X/2,
                                                    XB.Settings.BaseResY/2 - crosshairsSize.Y/2 );
        _trCrosshairs.ExpandMode  = Godot.TextureRect.ExpandModeEnum.IgnoreSize;
        _trCrosshairs.StretchMode = Godot.TextureRect.StretchModeEnum.Scale;
        _trCrosshairs.Size        = crosshairsSize;

        _imgLinking = Godot.Image.Create(XB.Settings.BaseResX, XB.Settings.BaseResY,
                                         false, Godot.Image.Format.Rgba8      );
        _texLinking.SetImage(_imgLinking);
        _trLinking.Position    = new Godot.Vector2I(0, 0);
        _trLinking.ExpandMode  = Godot.TextureRect.ExpandModeEnum.IgnoreSize;
        _trLinking.StretchMode = Godot.TextureRect.StretchModeEnum.Scale;
        _trLinking.Size        = new Godot.Vector2I(XB.Settings.BaseResX, XB.Settings.BaseResY);
        _trLinking.Texture     = _texLinking;
        _matLinking            = new Godot.ShaderMaterial();
        _matLinking.Shader     = Godot.ResourceLoader.Load<Godot.Shader>(XB.ResourcePaths.LinkingShader);
        _matLinking.SetShaderParameter("colInnerBright", XB.Col.LinkBri);
        _matLinking.SetShaderParameter("colInnerDim",    XB.Col.LinkDim);
        _matLinking.SetShaderParameter("colOutline",     XB.Col.Black);
        _matLinking.SetShaderParameter("squareRatio",    (float)XB.Settings.BaseResX/
                                                         (float)XB.Settings.BaseResY );
        _matLinking.SetShaderParameter("ringRad",        _linkingRingRadius);
        _matLinking.SetShaderParameter("alphaMult",      1.0f);
        _trLinking.Material    = _matLinking;

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
        var spPosition = new Godot.Vector2I(XB.Settings.BaseResX - 2*_offsetE - _dimSpX,
                                            _offsetT - _offsetE                         );
        _trSpheres.Position      = spPosition;
        _trSpheresBG.Position    = spPosition - new Godot.Vector2I(_offsetE, _offsetE);
        _trSpheres.ExpandMode    = Godot.TextureRect.ExpandModeEnum.IgnoreSize;
        _trSpheresBG.ExpandMode  = Godot.TextureRect.ExpandModeEnum.IgnoreSize;
        _trSpheres.StretchMode   = Godot.TextureRect.StretchModeEnum.Scale;
        _trSpheresBG.StretchMode = Godot.TextureRect.StretchModeEnum.Scale;
        _trSpheres.Size          = new Godot.Vector2I(_dimSpX, _dimSpY);
        _trSpheresBG.Size        = _trSpheres.Size + new Godot.Vector2I(2*_offsetE, 2*_offsetE);
        _trSpheres.Texture       = _texSpheres;
        _trSpheresBG.Texture     = _texSpheresBG;
        _texSpheres.SetImage(_imgSpheres);
        _texSpheresBG.SetImage(_imgSpheresBG);
        CreateSphereTexture(_dimSpY, _dimSp, _columns, _dimBord, _dimThick,
                            _imgSpheres, _texSpheres, _rects, ref _rSize   );

        _imgMiniMapQT = Godot.Image.Create(_dimMMX, _dimMMY, false, Godot.Image.Format.Rgba8);
        _imgMiniMapQT.Fill(XB.Col.Transp);
        _imgMiniMapG  = Godot.Image.Create(_dimMMX, _dimMMGY, false, Godot.Image.Format.L8);
        _imgMiniMapG.Fill(XB.Col.Black);
        int dimMMBGX  = _dimMMX + 2*_offsetE;
        int dimMMBGY  = _dimMMY + 2*_offsetE + 3*_dimMMSp + _dimMMGY + _gradLbFontSize;
        _imgMiniMapBG = Godot.Image.Create(dimMMBGX, dimMMBGY, false, Godot.Image.Format.Rgba8);
        _imgMiniMapBG.Fill(XB.Col.BGDark);
        var miniMapPosition = new Godot.Vector2I(2*_offsetE, XB.Settings.BaseResY/2 - dimMMBGY/2);
        _trMiniMap.Position      = miniMapPosition;
        _trMiniMapQT.Position    = miniMapPosition;
        _trMiniMapG.Position     = miniMapPosition + new Godot.Vector2I(0, _dimMMY + _dimMMSp);
        _trMiniMapBG.Position    = miniMapPosition - new Godot.Vector2I(_offsetE, _offsetE);
        _trMiniMap.ExpandMode    = Godot.TextureRect.ExpandModeEnum.IgnoreSize;
        _trMiniMapQT.ExpandMode  = Godot.TextureRect.ExpandModeEnum.IgnoreSize;
        _trMiniMapG.ExpandMode   = Godot.TextureRect.ExpandModeEnum.IgnoreSize;
        _trMiniMapBG.ExpandMode  = Godot.TextureRect.ExpandModeEnum.IgnoreSize;
        _trMiniMap.StretchMode   = Godot.TextureRect.StretchModeEnum.Scale;
        _trMiniMapQT.StretchMode = Godot.TextureRect.StretchModeEnum.Scale;
        _trMiniMapG.StretchMode  = Godot.TextureRect.StretchModeEnum.Scale;
        _trMiniMapBG.StretchMode = Godot.TextureRect.StretchModeEnum.Scale;
        _trMiniMap.Size          = new Godot.Vector2I(_dimMMX, _dimMMY);
        _trMiniMapQT.Size        = new Godot.Vector2I(_dimMMX, _dimMMY);
        _trMiniMapG.Size         = new Godot.Vector2I(_dimMMX, _dimMMGY);
        _trMiniMapBG.Size        = new Godot.Vector2I(dimMMBGX, dimMMBGY);
        _trMiniMap.Texture       = TexMiniMap;
        _trMiniMapQT.Texture     = _texMiniMapQT;
        _trMiniMapG.Texture      = _texMiniMapG;
        _trMiniMapBG.Texture     = _texMiniMapBG;
        _texMiniMapQT.SetImage(_imgMiniMapQT);
        _texMiniMapG.SetImage (_imgMiniMapG);
        _texMiniMapBG.SetImage(_imgMiniMapBG);
        CreateGradientTexture(_imgMiniMapG, _texMiniMapG, _rects);
        _matMiniMap        = new Godot.ShaderMaterial();
        _matMiniMap.Shader = Godot.ResourceLoader.Load<Godot.Shader>(XB.ResourcePaths.MiniMapShader);
        _matMiniMap.SetShaderParameter("heightMapT",  TexMiniMap);
        _matMiniMap.SetShaderParameter("plColor",     XB.Col.MPlayer);
        _matMiniMap.SetShaderParameter("plColorLink", XB.Col.MPlayerL);
        _matMiniMap.SetShaderParameter("squareRatio", (float)_dimMMY/(float)_dimMMX);
        _matMiniMap.SetShaderParameter("halfWidth",   (_playerTriWidth/_dimMMX)/2.0f);
        _matMiniMap.SetShaderParameter("halfHeight",  (_playerTriHeight/_dimMMY)/2.0f);
        _matMiniMap.SetShaderParameter("outlThick",   (_playerOutlThick/_dimMMY));
        _matMiniMap.SetShaderParameter("spRadius",    (_sphereCirRadius/_dimMMY));
        _matMiniMap.SetShaderParameter("alphaMult",   _miniMapAlpha);
        _spherePositions = new Godot.Vector2[XB.ManagerSphere.MaxSphereAmount];
        _sphereColors    = new Godot.Color[XB.ManagerSphere.MaxSphereAmount];
        _trMiniMap.Material = _matMiniMap;

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
        _lbHeightL.Text = Tr("MMLEGEND_LOW");
        _lbHeightH.Text = Tr("MMLEGEND_HIGH");
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

        _lbFps.Position = new Godot.Vector2I(_offsetE, _offsetT);
        _lbFps.Size     = new Godot.Vector2I(_dimMMX, _gradLbFontSize);
        _lbFps.HorizontalAlignment = Godot.HorizontalAlignment.Left;
        _lbFps.VerticalAlignment   = Godot.VerticalAlignment.Top;
        _colFps   = XB.Col.White;
        _lbFps.AddThemeFontOverride ("font",       font   );
        _lbFps.AddThemeColorOverride("font_color", _colFps);

        // centered, symmetrical with fixed distance on right side to sphere BG texture
        int dimGX = XB.Settings.BaseResX 
                        - 2*(XB.Settings.BaseResX - spPosition.X + _offsetE + _offsetEGuide);
        int dimGSegX = (dimGX - (2 + _dimGSegDiv)*_offsetE) / _dimGSegDiv;
        _imgGuideBG = Godot.Image.Create(dimGX, _dimGY, false, Godot.Image.Format.Rgba8);
        _imgGuideBG.Fill(XB.Col.BGDark);
        DrawBorderDetail(_imgGuideBG, _dimBord, _guideBordDetThick, ref XB.Col.Teal);
        _texGuideBG.SetImage(_imgGuideBG);
        _trGuideBG.Size        = new Godot.Vector2I(dimGX, _dimGY);
        _trGuideBG.Position    = new Godot.Vector2I((XB.Settings.BaseResX/2) - (dimGX/2),
                                                    XB.Settings.BaseResY - _dimGY - _offsetE);
        _trGuideBG.ExpandMode  = Godot.TextureRect.ExpandModeEnum.IgnoreSize;
        _trGuideBG.StretchMode = Godot.TextureRect.StretchModeEnum.Scale;
        _trGuideBG.Texture     = _texGuideBG;

        _imgGuideName = Godot.Image.Create(_dimGY, _dimSpX, false, Godot.Image.Format.Rgba8);
        _imgGuideName.Fill(XB.Col.BGDark);
        DrawBorderDetail(_imgGuideName, _dimBord, _guideBordDetThick, ref XB.Col.Teal);
        _texGuideName.SetImage(_imgGuideName);
        _trGuideName.Size        = new Godot.Vector2I(_dimGY, _dimSpX + 2*_offsetE);
        _trGuideName.Position    = new Godot.Vector2I(3*_offsetE +  _dimSpX,
                                                      XB.Settings.BaseResY - _dimGY - _offsetE);
        _trGuideName.ExpandMode  = Godot.TextureRect.ExpandModeEnum.IgnoreSize;
        _trGuideName.StretchMode = Godot.TextureRect.StretchModeEnum.Scale;
        _trGuideName.Texture     = _texGuideName;
        _trGuideName.Rotation    = XB.Constants.PiHalf;
        _lbGuideName.Position    = new Godot.Vector2I(_dimBord, _dimBord);
        _lbGuideName.Size        = new Godot.Vector2I(_dimGY, _dimSpX + 2*_offsetE);
        _lbGuideName.HorizontalAlignment = Godot.HorizontalAlignment.Center;
        _lbGuideName.VerticalAlignment   = Godot.VerticalAlignment.Center;
        _lbGuideName.AddThemeFontOverride    ("font",               font                );
        _lbGuideName.AddThemeFontSizeOverride("font_size",          _guideNameLbFontSize);
        _lbGuideName.AddThemeConstantOverride("outline_size",       _guideNameLbOutlSize);
        _lbGuideName.AddThemeColorOverride   ("font_color",         XB.Col.White        );
        _lbGuideName.AddThemeColorOverride   ("font_outline_color", XB.Col.Black        );
        _lbGuideName.Text = _guideName;

        // _lbGuide* are children of _trGuide, so position is relative to top left corner of _trGuide
        for (int i = 0; i < _dimGSegDiv; i++) {
            _lbGuide[i].Position = new Godot.Vector2I((2+i)*_offsetE + i*dimGSegX, 2*_offsetE);
            _lbGuide[i].Size     = new Godot.Vector2I(dimGSegX, _dimGY - 2*_dimBord);
            _lbGuide[i].HorizontalAlignment = Godot.HorizontalAlignment.Left;
            _lbGuide[i].VerticalAlignment   = Godot.VerticalAlignment.Top;
            _lbGuide[i].AddThemeFontOverride    ("font",               font               );
            _lbGuide[i].AddThemeConstantOverride("line_spacing",       _guideLbFontSpacing);
            _lbGuide[i].AddThemeFontSizeOverride("font_size",          _guideLbFontSize   );
            _lbGuide[i].AddThemeConstantOverride("outline_size",       _guideLbOutlSize   );
            _lbGuide[i].AddThemeColorOverride   ("font_color",         XB.Col.White       );
            _lbGuide[i].AddThemeColorOverride   ("font_outline_color", XB.Col.Black       );
        }
        _guide = new string[3, 5];

        _colCross.A            = _crossAlpha;
        _trCrosshairs.Modulate = _colCross;
        _colSpheres.A          = _spheresAlpha;
        _trSpheres.Modulate    = _colSpheres;
        _trSpheresBG.Modulate  = _colSpheres;
        _colMiniMap.A          = _miniMapAlpha;
        _trMiniMap.Modulate    = _colMiniMap;
        _trMiniMapG.Modulate   = _colMiniMap;
        _trMiniMapBG.Modulate  = _colMiniMap;
        _blockMultCur          = _blockMult;
        _qtreeMultCur          = _qtreeMult;
        _colGuideBG.A          = _guideAlpha;
        _trGuideBG.Modulate    = _colGuideBG;
        _trGuideName.Modulate  = _colGuideBG;

#if XBDEBUG
        debug.End();
#endif 
    }

    public void UpdateMiniMap(Godot.Image imgHeightMap, float low, float high) {
#if XBDEBUG
        var debug = new XB.DebugTimedBlock(XB.D.HUDUpdateMiniMap);
#endif

        TexMiniMap.SetImage(imgHeightMap); // required because the heightmap gets created
                                        // after hud gets initialized
        TexMiniMap.Update(imgHeightMap);
        _lbHeightL.Text = low.ToString (XB.Constants.HeightFormat) + "m";
        _lbHeightH.Text = high.ToString(XB.Constants.HeightFormat) + "m";

#if XBDEBUG
        debug.End();
#endif 
    }

    private void CreateSphereTexture(int dimSpY, int dimSp, int columns, int dimBord, int dimThick,
                                     Godot.Image image, Godot.ImageTexture tex, 
                                     Godot.Rect2I[] rects, ref int rSize                           ) {
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

            XB.Utils.BeveledRectangle(xStart, yStart, dimSp-2*dimThick, rects, ref rSize);
            XB.Utils.FillRectanglesInImage(image, rects, rSize, ref XB.Col.Outline);

            XB.Utils.BeveledRectangle(xStart+dimBord, yStart+dimBord,
                                      dimSp-2*dimBord-2*dimThick, rects, ref rSize);
            XB.Utils.FillRectanglesInImage(image, rects, rSize, ref XB.Col.InAct);

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

    private void CreateGradientTexture(Godot.Image image, Godot.ImageTexture tex,
                                       Godot.Rect2I[] rects                          ) {
#if XBDEBUG
        var debug = new XB.DebugTimedBlock(XB.D.HUDCreateGradientTexture);
#endif

        float step = 1.0f/(tex.GetWidth()-1);
        var col = new Godot.Color(0.0f, 0.0f, 0.0f, 1.0f);
        for (int i = 0; i < tex.GetWidth(); i++) {
            int xStart = (int)( (float)i/(tex.GetWidth()-1) * (tex.GetWidth()-1) );
            XB.Utils.UpdateRect2I(xStart, 0, 1, tex.GetHeight(), ref rects[0]);
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
                                     _rects, ref _rSize                                );
            XB.Utils.FillRectanglesInImage(_imgSpheres, _rects, _rSize, ref digitColor);
        }
        xStart += xSize; //NOTE[ALEX]: moved out of loop so that 0-9 are right aligned as well
        XB.Utils.DigitRectangles(digit%10, xStart, yStart, xSize, ySize, thickness,
                                 _rects, ref _rSize                                );
        XB.Utils.FillRectanglesInImage(_imgSpheres, _rects, _rSize, ref digitColor);

#if XBDEBUG
        debug.End();
#endif 
    }

    // draw four rectangles that resemble lines offset from the edges of img
    //NOTE[ALEX]: does not check if the image is large enough to correctly draw these rectangles
    private void DrawBorderDetail(Godot.Image img, int offset, int thickness, ref Godot.Color col) {
        var v2 = new Godot.Vector2I(0, 0);
        int iW = img.GetWidth();
        int iH = img.GetHeight();

        // top rectangle
        v2.X = offset;
        v2.Y = offset;
        _rects[0].Position = v2;
        v2.X = iW - 2*offset;
        v2.Y = thickness;
        _rects[0].Size     = v2;

        // bottom rectangle
        v2.X = offset;
        v2.Y = iH - offset - thickness;
        _rects[1].Position = v2;
        v2.X = iW - 2*offset;
        v2.Y = thickness;
        _rects[1].Size     = v2;

        // left rectangle
        v2.X = offset;
        v2.Y = offset;
        _rects[2].Position = v2;
        v2.X = thickness;
        v2.Y = iH - 2*offset;
        _rects[2].Size     = v2;

        // right rectangle
        v2.X = iW - offset - thickness;
        v2.Y = offset;
        _rects[3].Position = v2;
        v2.X = thickness;
        v2.Y = iH - 2*offset;
        _rects[3].Size     = v2;

        XB.Utils.FillRectanglesInImage(img, _rects, 4, ref col);
    }

    public void UpdateSphereTexture(int id, XB.SphereTexSt state) {
#if XBDEBUG
        var debug = new XB.DebugTimedBlock(XB.D.HUDUpdateSphereTexture);
#endif

        int xStart = (id%_columns)*_dimSp;
        int yOff   = (id/_columns)*_dimSp;
        int yStart = _dimSpY-yOff-_dimSp;
        switch (state) {
            case XB.SphereTexSt.Inactive:      { _pColor = XB.Col.InAct;   
                                                 _dColor = XB.Col.White;   break; }
            case XB.SphereTexSt.Active:        { _pColor = XB.Col.Act;    
                                                 _dColor = XB.Col.Black;   break; }
            case XB.SphereTexSt.ActiveLinking: { _pColor = XB.Col.LinkBri; 
                                                 _dColor = XB.Col.Black;   break; }
            case XB.SphereTexSt.ActiveLinked:  { _pColor = XB.Col.LinkDim; 
                                                 _dColor = XB.Col.Black;   break; }
        }

        XB.Utils.BeveledRectangle(xStart+_dimBord, yStart+_dimBord,
                                  _dimSp-2*_dimBord-2*_dimThick, _rects, ref _rSize);
        XB.Utils.FillRectanglesInImage(_imgSpheres, _rects, _rSize, ref _pColor);

        AddDigitTexture(id, xStart+2*_dimBord, yStart+2*_dimBord, _dimThick, ref _dColor);
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

            XB.Utils.RectangleOutline(xStart, yStart, _dimSp, _dimThick, _rects, ref _rSize);
            XB.Utils.FillRectanglesInImage(_imgSpheres, _rects, _rSize, ref XB.Col.Transp);
        }
        if (to < XB.ManagerSphere.MaxSphereAmount) {
            int xStart = (to%_columns)*_dimSp;
            int yOff   = (to/_columns)*_dimSp;
            int yStart = _dimSpY-yOff-_dimSp;
            
            XB.Utils.RectangleOutline(xStart, yStart, _dimSp, _dimThick, _rects, ref _rSize);
            XB.Utils.FillRectanglesInImage(_imgSpheres, _rects, _rSize, ref XB.Col.Hl);
        }
        _texSpheres.Update(_imgSpheres);

#if XBDEBUG
        debug.End();
#endif 
    }

    private void UpdateMiniMapTexture(XB.PController pCtrl) {
#if XBDEBUG
        var debug = new XB.DebugTimedBlock(XB.D.HUDUpdateMiniMapTexture);
#endif

        //NOTE[ALEX]: world corner is at 0|0, calculate % offset from corner
        //            texture starts at top right corner with 0|0,
        //            world has z+ going forward and x+ going left,
        //            so 0|0 in world coordinates is 0|0 in world coordinates
        //            but the axes of the terrain in world space go in negative direction
        float posX = -pCtrl.PModel.GlobalPosition.X/XB.WData.WorldSize.X;
        float posZ = -pCtrl.PModel.GlobalPosition.Z/XB.WData.WorldSize.Y;
        float xDir = +pCtrl.CCtrH.GlobalTransform.Basis.Z.X; // X coordinate of Z basis vector
        float zDir = +pCtrl.CCtrH.GlobalTransform.Basis.Z.Z;

        _matMiniMap.SetShaderParameter("plPosX", posX);
        _matMiniMap.SetShaderParameter("plPosZ", posZ);
        _matMiniMap.SetShaderParameter("plDirX", xDir);
        _matMiniMap.SetShaderParameter("plDirZ", zDir);

        for (int i = 0; i < XB.ManagerSphere.Spheres.Length; i++) {
            if (!XB.ManagerSphere.Spheres[i].Active) { 
                _spherePositions[i].X = -1.0f; // move sphere out of bounds of texture
                _spherePositions[i].Y = -1.0f; // to effectively ignore it
                _sphereColors[i]      = XB.Col.Transp;
            } else {
                posX = -XB.ManagerSphere.Spheres[i].GlobalPosition.X/XB.WData.WorldSize.X;
                posZ = -XB.ManagerSphere.Spheres[i].GlobalPosition.Z/XB.WData.WorldSize.Y;
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

        _matMiniMap.SetShaderParameter("spPos", _spherePositions);
        _matMiniMap.SetShaderParameter("spCol", _sphereColors);

#if XBDEBUG
        debug.End();
#endif 
    }

    public void ToggleHUD() {
        _hudVisible = !_hudVisible;
    }

    public void UpdateHUDElementVisibility(bool showFps, bool showGuides,
                                           bool showBlockGrid, bool showQTreeVis) {
        FpsVisible       = showFps;
        GuideVisible     = showGuides;
        BlockGridVisible = showBlockGrid;
        QTreeVisible     = showQTreeVis;
    }

    public void UpdateHUD(float dt, XB.PController pCtrl, XB.Settings sett, XB.Input input) {
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
            if (QTreeVisible) { _qtreeMult = 1.0f; }
            else              { _qtreeMult = 0.0f; }
            if (GuideVisible) { _guideAlpha = 1.0f; }
            else              { _guideAlpha = 0.0f; }
            _spheresAlpha = 1.0f;
            _miniMapAlpha = 1.0f;
        } else {
            _crossAlpha   = 0.0f;
            _fpsAlpha     = 0.0f;
            _spheresAlpha = 0.0f;
            _linkingAlpha = 0.0f;
            _miniMapAlpha = 0.0f;
            _blockMult    = 0.0f;
            _qtreeMult    = 0.0f;
            _guideAlpha   = 0.0f;
        }

        if (XB.ManagerSphere.Linking) { 
            _miniMapLinking = 1.0f;
            _colGuideText   = _colGuideText.Lerp(XB.Col.LinkBri, _hudSm*dt);
        } else {
            _miniMapLinking = 0.0f;
            _colGuideText   = _colGuideText.Lerp(XB.Col.White, _hudSm*dt);
        }

        _colCross.A            = XB.Utils.LerpF(_colCross.A, _crossAlpha, _hudSm*dt);
        _trCrosshairs.Modulate = _colCross;
        _colFps.A              = XB.Utils.LerpF(_colFps.A, _fpsAlpha, _hudSm*dt);
        _lbFps.AddThemeColorOverride("font_color", _colFps);
        _colSpheres.A          = XB.Utils.LerpF(_colSpheres.A, _spheresAlpha, _hudSm*dt);
        _trSpheres.Modulate    = _colSpheres;
        _trSpheresBG.Modulate  = _colSpheres;
        _matLinking.SetShaderParameter("alphaMult", _linkingAlpha);
        _colMiniMap.A          = XB.Utils.LerpF(_colMiniMap.A, _miniMapAlpha, _hudSm*dt);
        _trMiniMap.Modulate    = _colMiniMap;
        _matMiniMap.SetShaderParameter("alphaMult", _miniMapAlpha);
        _matMiniMap.SetShaderParameter("linking",   _miniMapLinking);
        _trMiniMapG.Modulate   = _colMiniMap;
        _trMiniMapBG.Modulate  = _colMiniMap;
        _colMMLegend.A         = XB.Utils.LerpF(_colMMLegend.A,    _miniMapAlpha, _hudSm*dt);
        _colMMLegendOut.A      = XB.Utils.LerpF(_colMMLegendOut.A, _miniMapAlpha, _hudSm*dt);
        _lbHeightL.AddThemeColorOverride("font_color",         _colMMLegend   );
        _lbHeightH.AddThemeColorOverride("font_color",         _colMMLegend   );
        _lbHeightL.AddThemeColorOverride("font_outline_color", _colMMLegendOut);
        _lbHeightH.AddThemeColorOverride("font_outline_color", _colMMLegendOut);
        _blockMultCur          = XB.Utils.LerpF(_blockMultCur, _blockMult, _hudSm*dt); 
        _qtreeMultCur          = XB.Utils.LerpF(_qtreeMultCur, _qtreeMult, _hudSm*dt); 
        XB.ManagerTerrain.UpdateBlockStrength(_blockMultCur);
        XB.ManagerTerrain.UpdateQTreeStrength(_qtreeMultCur);
        _colMiniMapQT.A        = XB.Utils.LerpF(_colMiniMapQT.A, _qtreeMult, _hudSm*dt);
        _trMiniMapQT.Modulate  = _colMiniMapQT;
        _colGuideBG.A          = XB.Utils.LerpF(_colGuideBG.A, _guideAlpha, _hudSm*dt);
        _trGuideBG.Modulate    = _colGuideBG;
        _trGuideName.Modulate  = _colGuideBG;
        for (int i = 0; i < _dimGSegDiv; i++) {
            _lbGuide[i].AddThemeColorOverride("font_color", _colGuideText);
        }
        _lbGuideName.AddThemeColorOverride("font_color", _colGuideText);

        UpdateMiniMapTexture(pCtrl);

        if (sett.SC.QTreeVis) {
            XB.ManagerTerrain.UpdateQTreeTexture(_imgMiniMapQT,
                                                 _imgMiniMapQT.GetWidth()/XB.WData.WorldSize.X,
                                                 _rects                                        );
            _texMiniMapQT.Update(_imgMiniMapQT);
        }

        if (sett.SC.ShowGuides) { UpdateGuides(pCtrl, input); }

        _lbFps.Text = Godot.Engine.GetFramesPerSecond().ToString();

#if XBDEBUG
        debug.End();
#endif 
    }

    // on screen guide has three columns:
    // 0: menu
    //    aim
    //    run
    //    toggle 1st/3rd
    //    toggle overlay
    //
    // 1: in linking
    //    place sphere
    //    remove highlighted sphere
    //    move highlighted sphere
    //    enter linking
    //
    // 2: linking id
    //    set linking id / link
    //    change radius of highlighted sphere
    //    change angle of highlighted sphere
    //    unlink
    private void UpdateGuides(XB.PController pCtrl, XB.Input input) {
        _guide[0, 0] = Tr("GUIDE_OPENMENU")
                       + _guideSp + input.InputActions[(int)XB.KeyID.Start].Key;
        if (pCtrl.Move == XB.MoveSt.Walk) {
            _guide[0, 2] = Tr("GUIDE_RUN")
                           + _guideSp + input.InputActions[(int)XB.KeyID.DUp].Key;
        } else {
            _guide[0, 2] = _guideUnused;
        }
        if (pCtrl.ThirdP) {
            _guide[0, 1] = Tr("GUIDE_AIM")
                           + _guideSp + input.InputActions[(int)XB.KeyID.SLBot].Key;
            _guide[0, 3] = Tr("GUIDE_FIRSTPERSON")
                           + _guideSp + input.InputActions[(int)XB.KeyID.DDown].Key;
        } else {
            _guide[0, 1] = _guideUnused;
            _guide[0, 3] = Tr("GUIDE_THIRDPERSON")
                           + _guideSp + input.InputActions[(int)XB.KeyID.DDown].Key;
        }
        _guide[0, 4] = Tr("GUIDE_OVERLAY")
                           + _guideSp + input.InputActions[(int)XB.KeyID.Select].Key;

        if (XB.ManagerSphere.NextSphere == 0) {
            if (!pCtrl.PlAiming && pCtrl.ThirdP) {
                _guide[1, 1] = Tr("GUIDE_AIMTOINTERACT");
            } else {
                if (XB.ManagerSphere.LinkingID == XB.ManagerSphere.MaxSphereAmount) {
                    _guide[1, 1] = Tr("GUIDE_SPHEREPLACE")
                                   + _guideSp + input.InputActions[(int)XB.KeyID.SLTop].Key;
                }
                else {
                    _guide[1, 1] = Tr("GUIDE_SPHEREPLACELINK")
                                   + _guideSp + input.InputActions[(int)XB.KeyID.SLTop].Key;
                }
            }
        } else {
            if (!pCtrl.PlAiming && pCtrl.ThirdP) {
                _guide[1, 1] = Tr("GUIDE_AIMTOINTERACT");
            } else {
                if (XB.ManagerSphere.NextSphere == XB.ManagerSphere.MaxSphereAmount+1) { 
                    _guide[1, 1] = Tr("GUIDE_SPHERESMAX");
                } else {
                    if (XB.ManagerSphere.LinkingID == XB.ManagerSphere.MaxSphereAmount) {
                        _guide[1, 1] = Tr("GUIDE_SPHEREPLACE")
                                       + _guideSp + input.InputActions[(int)XB.KeyID.SLTop].Key;
                    } else {
                        _guide[1, 1] = Tr("GUIDE_SPHEREPLACELINK")
                                       + _guideSp + input.InputActions[(int)XB.KeyID.SLTop].Key;
                    }
                }
            }
        }
        if (XB.ManagerSphere.HLSphereID < XB.ManagerSphere.MaxSphereAmount) {
            _guide[1, 2] = Tr("GUIDE_SPHEREREMOVE")
                           + _guideSp + input.InputActions[(int)XB.KeyID.SRTop].Key;
            _guide[1, 3] = Tr("GUIDE_SPHEREMOVE")
                           + _guideSp + input.InputActions[(int)XB.KeyID.SRBot].Key;
            _guide[2, 2] = Tr("GUIDE_SPHERECHNGRADIUS")
                           + _guideSp + input.InputActions[(int)XB.KeyID.DLeft].Key
                           + "+" + input.InputActions[(int)XB.KeyID.SRBot].Key;
            _guide[2, 3] = Tr("GUIDE_SPHERECHNGANGLE")
                           + _guideSp + input.InputActions[(int)XB.KeyID.DRight].Key
                           + "+" + input.InputActions[(int)XB.KeyID.SRBot].Key;
        } else {
            if (!pCtrl.PlAiming && pCtrl.ThirdP) {
                _guide[1, 2] = _guideUnused;
                _guide[1, 3] = _guideUnused;
                _guide[2, 2] = _guideUnused;
                _guide[2, 3] = _guideUnused;
            } else {
                _guide[1, 2] = Tr("GUIDE_NOHIGHLIGHT");
                _guide[1, 3] = Tr("GUIDE_NOHIGHLIGHT");
                _guide[2, 2] = Tr("GUIDE_NOHIGHLIGHT");
                _guide[2, 3] = Tr("GUIDE_NOHIGHLIGHT");
            }
        }

            if (XB.ManagerSphere.Linking) {
                _guide[2, 1] = Tr("GUIDE_LINKINGSET")
                               + _guideSp + input.InputActions[(int)XB.KeyID.FUp].Key;
            } else {
                _guide[2, 1] = _guideUnused;
            }

        if (XB.ManagerSphere.Linking) {
            _guide[1, 0] = Tr("GUIDE_INLINKING");
            _guide[1, 4] = Tr("GUIDE_LINKINGEXIT")
                           + _guideSp + input.InputActions[(int)XB.KeyID.FRight].Key;

            if (XB.ManagerSphere.LinkingID == XB.ManagerSphere.MaxSphereAmount) {
                _guide[2, 0] = _guideUnused;
                if (XB.ManagerSphere.HLSphereID < XB.ManagerSphere.MaxSphereAmount) {
                    _guide[2, 1] = Tr("GUIDE_LINKINGSET")
                                   + _guideSp + input.InputActions[(int)XB.KeyID.FUp].Key;
                } else {
                    _guide[2, 1] = _guideUnused;
                }
                _guide[2, 4] = _guideUnused;
            } else if (   XB.ManagerSphere.LinkingID == XB.ManagerSphere.HLSphereID
                       && XB.ManagerSphere.Spheres[XB.ManagerSphere.LinkingID].Linked) {
                _guide[2, 0] = Tr("GUIDE_LINKINGID") + " " + XB.ManagerSphere.LinkingID.ToString();
                _guide[2, 1] = Tr("GUIDE_LINKINGUNSET")
                               + _guideSp + input.InputActions[(int)XB.KeyID.FUp].Key;
                _guide[2, 4] = Tr("GUIDE_LINKINGUNLINK")
                               + _guideSp + input.InputActions[(int)XB.KeyID.FLeft].Key;
            } else {
                _guide[2, 0] = Tr("GUIDE_LINKINGID") + " " + XB.ManagerSphere.LinkingID.ToString();
                if (XB.ManagerSphere.HLSphereID < XB.ManagerSphere.MaxSphereAmount) {
                    if (XB.ManagerSphere.HLSphereID != XB.ManagerSphere.LinkingID) {
                        //NOTE[ALEX]: does not reflect updating linking id
                        _guide[2, 1] = Tr("GUIDE_LINKINGLINK")
                                       + _guideSp + input.InputActions[(int)XB.KeyID.FUp].Key;
                    } else {
                        _guide[2, 1] = Tr("GUIDE_LINKINGUNSET")
                                       + _guideSp + input.InputActions[(int)XB.KeyID.FUp].Key;
                    }
                    if (XB.ManagerSphere.Spheres[XB.ManagerSphere.HLSphereID].Linked) {
                        _guide[2, 4] = Tr("GUIDE_LINKINGUNLINK")
                                       + _guideSp + input.InputActions[(int)XB.KeyID.FLeft].Key;
                    } else {
                        _guide[2, 4] = _guideUnused;
                    }
                } else {
                    _guide[2, 1] = Tr("GUIDE_LINKINGUNSET")
                                   + _guideSp + input.InputActions[(int)XB.KeyID.FUp].Key;
                    _guide[2, 4] = _guideUnused;
                }
            }
        } else {
            _guide[1, 0] = _guideUnused;
            _guide[1, 4] = Tr("GUIDE_LINKINGENTER")
                           + _guideSp + input.InputActions[(int)XB.KeyID.FRight].Key;
            _guide[2, 0] = _guideUnused;
            _guide[2, 4] = _guideUnused;
        }

        _lbGuide[0].Text =   _guide[0, 0] + '\n' + _guide[0, 1] + '\n' + _guide[0, 2] + '\n'
                           + _guide[0, 3] + '\n' + _guide[0, 4];
        _lbGuide[1].Text =   _guide[1, 0] + '\n' + _guide[1, 1] + '\n' + _guide[1, 2] + '\n'
                           + _guide[1, 3] + '\n' + _guide[1, 4];
        _lbGuide[2].Text =   _guide[2, 0] + '\n' + _guide[2, 1] + '\n' + _guide[2, 2] + '\n'
                           + _guide[2, 3] + '\n' + _guide[2, 4];
    }
}
} // namespace close
