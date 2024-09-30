#define XBDEBUG
namespace XB { // namespace opegn
// Menu takes over as the main controlling function when the game gets paused (from PController)
public partial class Menu : Godot.Control {
    [Godot.Export] private Godot.Label        _lbTab;
    [Godot.Export] private Godot.Label        _lbMsg;
    [Godot.Export] private Godot.ColorRect    _crMsg;
    [Godot.Export] private Godot.TabContainer _tabCont;
                   private int                _tabPrev;
                   private bool               _justOpened = true;
                   private float              _t          = 0.0f;
                   private float              _msgDur     = 2.0f; // in seconds
    [Godot.Export] private Godot.Button       _bResume;
                   private bool               _inStartupScreen = false;

                   // references to MainLoop variables
                   private XB.PlayerState _pS;
                   private Godot.Node     _mainRoot; // reference for sound effects and terrain

    // tab numbers
    private const int _tPau    = 0;
    private const int _tSys    = 1;
    private const int _tSysCam = 0;
    private const int _tSysDis = 1;
    private const int _tSysPer = 2;
    private const int _tSysAud = 3;
    private const int _tSysLan = 4;
    private const int _tCon    = 2;

    // startup popup
    [Godot.Export] private Godot.Control     _ctrlPopupS;
    [Godot.Export] private Godot.TextureRect _trStartup0;
    [Godot.Export] private Godot.TextureRect _trStartup1;
    [Godot.Export] private Godot.TextureRect _trStartup2;
    [Godot.Export] private Godot.Label       _lbStartup0;
    [Godot.Export] private Godot.Label       _lbStartup1;
    [Godot.Export] private Godot.Label       _lbStartup2;

    // pause tab
    [Godot.Export] private Godot.Button      _bQuit;
    [Godot.Export] private Godot.Button      _bGenerate;
    [Godot.Export] private Godot.Button      _bApplySpheres;
    [Godot.Export] private Godot.Button      _bClearSpheres;
    [Godot.Export] private Godot.Button      _bShowStartup;
    [Godot.Export] private Godot.TextureRect _trPauseMap;
    [Godot.Export] private Godot.Label       _lbPauseLow;
    [Godot.Export] private Godot.Label       _lbPauseHigh;

    // system tab
    [Godot.Export] private Godot.TabContainer _tabSys;
    [Godot.Export] private Godot.LineEdit     _leSetCode;
    [Godot.Export] private Godot.Button       _bApplyCode;

        // camera
    [Godot.Export] private Godot.Label        _lbCamHor;
    [Godot.Export] private Godot.Slider       _slCamHor;
    [Godot.Export] private Godot.Label        _lbCamVer;
    [Godot.Export] private Godot.Slider       _slCamVer;
    [Godot.Export] private Godot.Label        _lbFov;
    [Godot.Export] private Godot.Slider       _slFov;
    [Godot.Export] private Godot.OptionButton _obPresets;
    [Godot.Export] private Godot.Button       _bApplyPreset;
    [Godot.Export] private Godot.Button       _bAppDefaults;
    [Godot.Export] private Godot.Button       _cbBlock;
    [Godot.Export] private Godot.Button       _cbQTVis;
    [Godot.Export] private Godot.Button       _cbGuides;
        // display
    [Godot.Export] private Godot.OptionButton _obRes;
    [Godot.Export] private Godot.OptionButton _obMode;
    [Godot.Export] private Godot.Label        _lbFrame;
    [Godot.Export] private Godot.Slider       _slFrame;
    [Godot.Export] private Godot.Button       _cbFps;
    [Godot.Export] private Godot.Button       _cbVSync;
        // performance
    [Godot.Export] private Godot.ScrollContainer _scrPerf;
    [Godot.Export] private Godot.OptionButton    _obMSAA;
    [Godot.Export] private Godot.OptionButton    _obSSAA;
    [Godot.Export] private Godot.Button          _cbTAA;
    [Godot.Export] private Godot.Button          _cbDebanding;
    [Godot.Export] private Godot.Label           _lbShdwSize;
    [Godot.Export] private Godot.Slider          _slShdwSize;
    [Godot.Export] private Godot.OptionButton    _obShdwFilter;
    [Godot.Export] private Godot.Label           _lbShdwDist;
    [Godot.Export] private Godot.Slider          _slShdwDist;
    [Godot.Export] private Godot.Label           _lbLOD;
    [Godot.Export] private Godot.Slider          _slLOD;
    [Godot.Export] private Godot.OptionButton    _obSSAO;
    [Godot.Export] private Godot.Button          _cbSSAOHalf;
    [Godot.Export] private Godot.OptionButton    _obSSIL;
    [Godot.Export] private Godot.Button          _cbSSILHalf;
    [Godot.Export] private Godot.Button          _cbSSR;
        // audio
    [Godot.Export] private Godot.Label  _lbVolume;
    [Godot.Export] private Godot.Slider _slVolume;
        // language
    [Godot.Export] private Godot.OptionButton _obLanguage;

    // controls tab
    [Godot.Export] private Godot.Button[] _bCK          = new Godot.Button[XB.Input.Amount];
    [Godot.Export] private Godot.Button   _bDefaultsCK;
    [Godot.Export] private Godot.Control  _chngMsg;
                   private bool           _setKey       = false;
                   private bool           _lockInput    = false;
                   private bool           _mouseRelease = false;
                   private int            _setKeyID     = 0;

    // popup apply spheres
    [Godot.Export] private Godot.Control _ctrlPopupA;
    [Godot.Export] private Godot.Button  _bPopAppSpCancel;
    [Godot.Export] private Godot.Button  _bPopAppSp;

    // popup quit
    [Godot.Export] private Godot.Control _ctrlPopupQ;
    [Godot.Export] private Godot.Button  _bPopQuitCancel;
    [Godot.Export] private Godot.Button  _bPopQuit;

    // popup generation
    [Godot.Export] private Godot.Control  _ctrlPopupG;
    [Godot.Export] private Godot.Button   _bPopGenCancel;
    [Godot.Export] private Godot.Button   _bPopGenApply;
    [Godot.Export] private Godot.LineEdit _leGenSeed;
    [Godot.Export] private Godot.Button   _bGenSeedApply;
    [Godot.Export] private Godot.Label    _lbGenHeight;
    [Godot.Export] private Godot.Slider   _slGenHeight;
    [Godot.Export] private Godot.Label    _lbGenScale;
    [Godot.Export] private Godot.Slider   _slGenScale;
    [Godot.Export] private Godot.Label    _lbGenOffX;
    [Godot.Export] private Godot.Slider   _slGenOffX;
    [Godot.Export] private Godot.Label    _lbGenOffZ;
    [Godot.Export] private Godot.Slider   _slGenOffZ;
    [Godot.Export] private Godot.Label    _lbGenOct;
    [Godot.Export] private Godot.Slider   _slGenOct;
    [Godot.Export] private Godot.Label    _lbGenPers;
    [Godot.Export] private Godot.Slider   _slGenPers;
    [Godot.Export] private Godot.Label    _lbGenLac;
    [Godot.Export] private Godot.Slider   _slGenLac;
    [Godot.Export] private Godot.Label    _lbGenExp;
    [Godot.Export] private Godot.Slider   _slGenExp;
    [Godot.Export] private Godot.Button   _cbGenUpd;
    [Godot.Export] private Godot.Label    _lbGenLow;
    [Godot.Export] private Godot.Label    _lbGenHigh;
    [Godot.Export] private Godot.Label    _lbGenCurSeed;
    [Godot.Export] private Godot.TextureRect  _trGenMap;
                   private Godot.Image        _imgGenMap;
                   private Godot.ImageTexture _texGenMap;
                   private const string       _valueFormat  = "F2";
                   private const string       _scaleFormat  = "F4"; // more precision for scale
                   private const string       _heightFormat = "F2";
                   private       bool         _updateGenTex = true;


    public void SetMainLoopReferences(XB.PlayerState pS, Godot.Node mainRoot) {
        _pS       = pS;
        _mainRoot = mainRoot;
    }

    public void InitializeMenu() {
        _tabCont.CurrentTab = _tPau;
        _tabSys.CurrentTab  = _tSysCam;
        _tabPrev            = _tabCont.CurrentTab;
        _crMsg.Visible      = false;
        _bResume.Pressed   += ButtonResumeOnPressed;

        // startup popup
        _ctrlPopupS.Hide();
        _trStartup0.Texture = Godot.ResourceLoader.Load<Godot.Texture2D>(XB.ResourcePaths.Startup0Tex);
        _trStartup1.Texture = Godot.ResourceLoader.Load<Godot.Texture2D>(XB.ResourcePaths.Startup1Tex);
        _trStartup2.Texture = Godot.ResourceLoader.Load<Godot.Texture2D>(XB.ResourcePaths.Startup2Tex);
        _lbStartup0.HorizontalAlignment = Godot.HorizontalAlignment.Center;
        _lbStartup0.VerticalAlignment   = Godot.VerticalAlignment.Center;
        _lbStartup1.HorizontalAlignment = Godot.HorizontalAlignment.Center;
        _lbStartup1.VerticalAlignment   = Godot.VerticalAlignment.Center;
        _lbStartup2.HorizontalAlignment = Godot.HorizontalAlignment.Center;
        _lbStartup2.VerticalAlignment   = Godot.VerticalAlignment.Center;

        // pause tab
        _bQuit.Pressed          += ButtonPopupQuitOnPressed;
        _bGenerate.Pressed      += ButtonGenerateTerrainOnPressed;
        _bApplySpheres.Pressed  += ButtonPopupApplySpheresOnPressed;
        _bClearSpheres.Pressed  += ButtonClearSpheresOnPressed;
        _bShowStartup.Pressed   += ButtonShowStartupOnPressed;
        var matPauseMap    = new Godot.ShaderMaterial();
        matPauseMap.Shader = Godot.ResourceLoader.Load<Godot.Shader>(XB.ResourcePaths.GreyScaleShader);
        matPauseMap.SetShaderParameter("texToGreyT", _pS.Hud.TexMiniMap);
        _trPauseMap.Texture      = _pS.Hud.TexMiniMap;
        _trPauseMap.ExpandMode   = Godot.TextureRect.ExpandModeEnum.IgnoreSize;
        _trPauseMap.StretchMode  = Godot.TextureRect.StretchModeEnum.Scale;
        _trPauseMap.Material     = matPauseMap;

        // system tab
        _bApplyCode.Pressed    += ButtonApplyCodeOnPressed;
            // camera
        _slFov.MinValue        = XB.Settings.FovMin;
        _slFov.MaxValue        = XB.Settings.FovMax;
        _slFov.Step            = 1;
        _slFov.DragEnded      += SliderFovOnDragEnded;
        _slCamHor.MinValue     = 0.0f;
        _slCamHor.MaxValue     = XB.Settings.CamSensMax;
        _slCamHor.Step         = 1;
        _slCamHor.DragEnded   += SliderCamHorOnDragEnded;
        _slCamVer.MinValue     = 0.0f;
        _slCamVer.MaxValue     = XB.Settings.CamSensMax;
        _slCamVer.Step         = 1;
        _slCamVer.DragEnded   += SliderCamVerOnDragEnded;
        _cbGuides.Pressed     += ButtonShowGuidesOnPressed;
        _cbBlock.Pressed      += ButtonBlockGridOnPressed;
        _cbQTVis.Pressed      += ButtonQuadTreeVisOnPressed;
        _bAppDefaults.Pressed += ButtonAppDefaultsOnPressed;
        _bApplyPreset.Pressed += ButtonApplyOnPressed;
        foreach (var preset in _pS.Sett.Presets) { _obPresets.AddItem(preset.Key); }
        _pS.Sett.AddSeparators(_obPresets);
        _obPresets.Select(1); // select default preset on startup
        _obPresets.ItemSelected += OptionButtonPresetsOnItemSelected;
            // display
        foreach (var resolution in _pS.Sett.Resolutions) { _obRes.AddItem(resolution.Key); }
        _pS.Sett.AddSeparators(_obRes);
        _obRes.ItemSelected += OptionButtonResOnItemSelected;
        foreach (var windowMode in _pS.Sett.WindowModes) { _obMode.AddItem(windowMode); }
        _pS.Sett.AddSeparators(_obMode);
        _obMode.ItemSelected += OptionButtonModeOnItemSelected;
        _cbFps.Pressed       += ButtonShowFPSOnPressed;
        _cbVSync.Pressed     += ButtonVSyncOnPressed;
        _slFrame.MinValue     = 0.0f;
        _slFrame.MaxValue     = _pS.Sett.FpsOptions.Length - 1;
        _slFrame.DragEnded   += SliderFrameRateOnDragEnded;
            // performance
        _scrPerf.ScrollVertical = 0;
        _cbTAA.Pressed         += ButtonTAAOnPressed;
        _cbDebanding.Pressed   += ButtonDebandingOnPressed;
        _cbSSAOHalf.Pressed    += ButtonSSAOHalfOnPressed;
        _cbSSILHalf.Pressed    += ButtonSSILHalfOnPressed;
        _cbSSR.Pressed         += ButtonSSROnPressed;
        foreach (var option in _pS.Sett.MSAA) { _obMSAA.AddItem(option); }
        _pS.Sett.AddSeparators(_obMSAA);
        _obMSAA.ItemSelected += OptionButtonMSAAOnItemSelected;
        foreach (var option in _pS.Sett.SSAA) { _obSSAA.AddItem(option); }
        _pS.Sett.AddSeparators(_obSSAA);
        _obSSAA.ItemSelected += OptionButtonSSAAOnItemSelected;
        foreach (var option in _pS.Sett.ShadowFilters) { _obShdwFilter.AddItem(option); }
        _pS.Sett.AddSeparators(_obShdwFilter);
        _obShdwFilter.ItemSelected += OptionButtonShdwFilterOnItemSelected;
        foreach (var option in _pS.Sett.SSAO) { _obSSAO.AddItem(option); }
        _pS.Sett.AddSeparators(_obSSAO);
        _obSSAO.ItemSelected += OptionButtonSSAOOnItemSelected;
        foreach (var option in _pS.Sett.SSIL) { _obSSIL.AddItem(option); }
        _pS.Sett.AddSeparators(_obSSIL);
        _obSSIL.ItemSelected += OptionButtonSSILOnItemSelected;
        _slShdwDist.MinValue   = XB.Settings.ShadowDistMin;
        _slShdwDist.MaxValue   = XB.Settings.ShadowDistMax;
        _slShdwDist.Step       = 1;
        _slShdwDist.DragEnded += SliderShadowDistanceOnDragEnded;
        _slShdwSize.MinValue   = 0.0f;
        _slShdwSize.MaxValue   = _pS.Sett.ShadowSizes.Length - 1;
        _slShdwSize.Step       = 1;
        _slShdwSize.DragEnded += SliderShadowSizeOnDragEnded;
        _slLOD.MinValue   = 0.0f;
        _slLOD.MaxValue   = _pS.Sett.LOD.Length - 1;
        _slLOD.Step       = 1;
        _slLOD.DragEnded += SliderLODOnDragEnded;
            // audio
        _slVolume.MinValue   = XB.Settings.VolumeMin;
        _slVolume.MaxValue   = 0.0f;
        _slVolume.Step       = 1;
        _slVolume.DragEnded += SliderVolumeOnDragEnded;
            // language
        foreach (var language in _pS.Sett.Languages) { _obLanguage.AddItem(language); }
        _pS.Sett.AddSeparators(_obLanguage);
        _obLanguage.ItemSelected += OptionButtonLanguageOnItemSelected;

        // controls tab
        _bCK[ 0].Pressed     += ButtonCK00OnPressed;
        _bCK[ 1].Pressed     += ButtonCK01OnPressed;
        _bCK[ 2].Pressed     += ButtonCK02OnPressed;
        _bCK[ 3].Pressed     += ButtonCK03OnPressed;
        _bCK[ 4].Pressed     += ButtonCK04OnPressed;
        _bCK[ 5].Pressed     += ButtonCK05OnPressed;
        _bCK[ 6].Pressed     += ButtonCK06OnPressed;
        _bCK[ 7].Pressed     += ButtonCK07OnPressed;
        _bCK[ 8].Pressed     += ButtonCK08OnPressed;
        _bCK[ 9].Pressed     += ButtonCK09OnPressed;
        _bCK[10].Pressed     += ButtonCK10OnPressed;
        _bCK[11].Pressed     += ButtonCK11OnPressed;
        _bCK[12].Pressed     += ButtonCK12OnPressed;
        _bCK[13].Pressed     += ButtonCK13OnPressed;
        _bCK[14].Pressed     += ButtonCK14OnPressed;
        _bCK[15].Pressed     += ButtonCK15OnPressed;
        _bCK[16].Pressed     += ButtonCK16OnPressed;
        _bCK[17].Pressed     += ButtonCK17OnPressed;
        _bCK[18].Pressed     += ButtonCK18OnPressed;
        _bCK[19].Pressed     += ButtonCK19OnPressed;
        _bCK[20].Pressed     += ButtonCK20OnPressed;
        _bCK[21].Pressed     += ButtonCK21OnPressed;
        _bCK[22].Pressed     += ButtonCK22OnPressed;
        _bCK[23].Pressed     += ButtonCK23OnPressed;
        HideUnusedKeys();
        _bDefaultsCK.Pressed += ButtonDefaultsCKOnPressed;
        _chngMsg.Visible      = false;

        // popup apply spheres
        _bPopAppSpCancel.Pressed += ButtonPopupCancelOnPressed;
        _bPopAppSp.Pressed       += ButtonApplySpheresOnPressed;
        _ctrlPopupA.Hide();

        // popup quit
        _bPopQuitCancel.Pressed += ButtonPopupCancelOnPressed;
        _bPopQuit.Pressed       += ButtonQuitOnPressed;
        _ctrlPopupQ.Hide();

        // popup generation
        _bPopGenCancel.Pressed += ButtonPopupCancelOnPressed;
        _bPopGenApply.Pressed  += ButtonPopupGenApplyOnPressed;
        _leGenSeed.TextChanged += LineEditGenerateSeedOnTextChanged;
        _bGenSeedApply.Pressed += ButtonGenSeedApplyOnPressed;
        _slGenHeight.MinValue   = XB.WData.GenHeightMin;
        _slGenHeight.MaxValue   = XB.WData.GenHeightMax;
        _slGenHeight.DragEnded += SliderGenHeightOnDragEnded;
        _slGenHeight.Step       = 0; // step 0 is floating point amount
        _slGenScale.MinValue    = XB.WData.GenScaleMin;
        _slGenScale.MaxValue    = XB.WData.GenScaleMax;
        _slGenScale.DragEnded  += SliderGenScaleOnDragEnded;
        _slGenScale.Step        = 0;
        _slGenOffX.MinValue     = XB.WData.GenOffXMin;
        _slGenOffX.MaxValue     = XB.WData.GenOffXMax;
        _slGenOffX.DragEnded   += SliderGenOffXOnDragEnded;
        _slGenOffX.Step         = 0;
        _slGenOffZ.MinValue     = XB.WData.GenOffZMin;
        _slGenOffZ.MaxValue     = XB.WData.GenOffZMax;
        _slGenOffZ.DragEnded   += SliderGenOffZOnDragEnded;
        _slGenOffZ.Step         = 0;
        _slGenOct.MinValue      = XB.WData.GenOctMin;
        _slGenOct.MaxValue      = XB.WData.GenOctMax;
        _slGenOct.DragEnded    += SliderGenOctOnDragEnded;
        _slGenOct.Step          = 1;
        _slGenPers.MinValue     = XB.WData.GenPersMin;
        _slGenPers.MaxValue     = XB.WData.GenPersMax;
        _slGenPers.DragEnded   += SliderGenPersOnDragEnded;
        _slGenPers.Step         = 0;
        _slGenLac.MinValue      = XB.WData.GenLacMin;
        _slGenLac.MaxValue      = XB.WData.GenLacMax;
        _slGenLac.DragEnded    += SliderGenLacOnDragEnded;
        _slGenLac.Step          = 0;
        _slGenExp.MinValue      = XB.WData.GenExpMin;
        _slGenExp.MaxValue      = XB.WData.GenExpMax;
        _slGenExp.DragEnded    += SliderGenExpOnDragEnded;
        _slGenExp.Step          = 0;
        _cbGenUpd.Pressed      += ButtonGenUpdOnPressed;
        _imgGenMap = Godot.Image.Create(XB.WData.ImgHeightMap.GetWidth(),
                                        XB.WData.ImgHeightMap.GetHeight(),
                                        false, Godot.Image.Format.Rf      );
        _imgGenMap.Fill(XB.Col.Black);
        _texGenMap = new Godot.ImageTexture();
        _texGenMap.SetImage(_imgGenMap);
        var matGenMap    = new Godot.ShaderMaterial();
        matGenMap.Shader = Godot.ResourceLoader.Load<Godot.Shader>(XB.ResourcePaths.GreyScaleShader);
        matGenMap.SetShaderParameter("texToGreyT", _texGenMap);
        _trGenMap.Texture     = _texGenMap;
        _trGenMap.ExpandMode  = Godot.TextureRect.ExpandModeEnum.IgnoreSize;
        _trGenMap.StretchMode = Godot.TextureRect.StretchModeEnum.Scale;
        _trGenMap.Material    = matGenMap;
        _ctrlPopupG.Hide();
    }

    public void InputStartup(Godot.InputEvent @event, XB.PController pCtrl) {
        if (_justOpened) { // prevent immediate closing of startup popup
            _justOpened = false;
            return;
        }
        if (_inStartupScreen && @event is not Godot.InputEventMouseMotion) {
            _inStartupScreen = false;
            Godot.Input.MouseMode = Godot.Input.MouseModeEnum.Captured;
            _ctrlPopupS.Hide();
            Hide();
            _pS.AppSt = XB.AppState.Application;
        }
    }

    // handling input before any other node to reassign controls
    public void Input(Godot.InputEvent @event) {
        if (_tabCont.CurrentTab != _tCon || !_setKey) { return; }

        if        (!_mouseRelease && @event is Godot.InputEventMouseButton) {
            _mouseRelease = true;
        } else if (@event is Godot.InputEventKey || @event is Godot.InputEventMouseButton) {
            _setKey       = false;
            // NOTE[ALEX]: InputEventMouseButton is also triggered on release of the button
            _mouseRelease = false;

            string key = "";
            if (@event is Godot.InputEventKey) {
                string[] keyText = @event.AsText().Split(' ');
                key = keyText[0];
            } else if (@event is Godot.InputEventMouseButton) {
                key = @event.AsText();
            }

            if (@event is Godot.InputEventMouseButton // get rid of double clicks, etc.
                    && key != "Left Mouse Button"
                    && key != "Middle Mouse Button"
                    && key != "Right Mouse Button") {
                ShowMessage(Tr("ILLEGAL_MOUSE_INPUT"));
                _chngMsg.Visible      = false;
                Godot.Input.MouseMode = Godot.Input.MouseModeEnum.Visible;
                return;
            }

            for (int i = 0; i < XB.Input.Amount; i++) {
                if (_pS.Input.InputActions[i].Key == key) {
                    ShowMessage(Tr("INPUT_ALREADY_USED"));
                    _chngMsg.Visible      = false;
                    Godot.Input.MouseMode = Godot.Input.MouseModeEnum.Visible;
                    return;
                }
            }

            var iAction       = _pS.Input.InputActions[_setKeyID];
                iAction.Event = @event;
                iAction.UpdateKey(key);
            Godot.InputMap.ActionEraseEvents(iAction.Name);
            Godot.InputMap.ActionAddEvent(iAction.Name, @event);

            ShowMessage(Tr("KEYBINDINGS_UPDATED") + Tr(iAction.Description) + ".");

            _chngMsg.Visible      = false;
            Godot.Input.MouseMode = Godot.Input.MouseModeEnum.Visible;
            UpdateControlTab();

            _pS.Input.ConsumeAllInputs();
        }
    }

    public void UpdateMenu(float dt) {
        if (_justOpened) {
            _justOpened = false;
            return;
        }

        _t += dt;

        if (_t >= _msgDur) {
            _t          = 0.0f;
            _lbMsg.Text = "";
            _crMsg.Hide();
        } else {
            _crMsg.Color = XB.Col.Msg.Lerp(XB.Col.MsgFade, _t/_msgDur);
        }

        // player input
        if (_pS.Input.Start && !_lockInput) {
            _pS.Input.ConsumeInputStart();
            ButtonResumeOnPressed();
        }
        
        //NOTE[ALEX]: _lockInput is required in addition to _setKey for setting new input keys,
        //            otherwise the pressed input will be read again in this method's GetInputs()
        //            call because the input sticks longer than expected before being cleared
        if (!_setKey && _lockInput) { _lockInput = false; }

        // change tabs
        if (_tabCont.CurrentTab != _tabPrev) {
            switch (_tabCont.CurrentTab) {
                case _tPau: { // pause
                    UpdatePauseTab();
                    break;
                }
                case _tSys: { // system
                    UpdateSystemTabContainer();
                    UpdateSettingsTab();
                    break;
                }
                case _tCon: { // controls
                    UpdateControlTab();
                    break;
                }
            }
            _bResume.GrabFocus();
            _tabPrev = _tabCont.CurrentTab;
        }

        // active tab
        switch (_tabCont.CurrentTab) {
            case _tPau: { // pause
                if (_ctrlPopupG.Visible) { UpdateGenLabels(); }
                break;
            }
            case _tSys: { // system
                UpdateSystemTabContainer();
                break;
            }
            case _tCon: { // controls
                break;
            }
        }
    }

    // application is not paused in startup screen -> terrain updates happen in background
    // when the startup screen is dismissed, the player's camera is reset (called in _Input)
    public void ShowStartupScreen() {
        _justOpened      = true;
        _inStartupScreen = true;
        Godot.Input.MouseMode = Godot.Input.MouseModeEnum.Visible; // show cursor
        Show();
        _ctrlPopupS.Show();
        _pS.AppSt = XB.AppState.Startup;
        _lbStartup0.Text = Tr("STARTUP_0");
        _lbStartup1.Text = Tr("STARTUP_1");
        _lbStartup2.Text = Tr("STARTUP_2");
    }

    public void OpenMenu() {
        _pS.Input.ConsumeInputStart();
        _lbTab.Text = Tr("TAB_PAUSE");
        _bResume.GrabFocus();
        _tabCont.CurrentTab   = _tPau;
        _justOpened           = true;
        Godot.Input.MouseMode = Godot.Input.MouseModeEnum.Visible; // show cursor
        GetTree().Paused      = true;
        Show();

        _setKey       = false;
        _lockInput    = false;
        _mouseRelease = false;

        _lbMsg.Text = "";
        _crMsg.Hide();
        UpdatePauseTab();
        UpdateTabNames();
        _pS.Sett.SettingsCodeFromSettings(_leSetCode);
        _pS.AppSt = XB.AppState.Menu;
    }

    private void UpdatePauseTab() {
        _lbTab.Text = Tr("TAB_PAUSE");
    }

    public void UpdatePauseMiniMap(float low, float high) {
        //NOTE[ALEX]: the minimap texture itself gets updated in HUD.UpdateMiniMap
        _lbPauseLow.Text  =   Tr("MMLEGEND_LOW") + ": "
                            + low.ToString(XB.Constants.HeightFormat) + "m";
        _lbPauseHigh.Text =   Tr("MMLEGEND_HIGH") + ": "
                            + high.ToString(XB.Constants.HeightFormat) + "m";
    }

    private void UpdateTabNames() {
        _tabCont.SetTabTitle(_tPau,    Tr("TAB_PAUSE"));
        _tabCont.SetTabTitle(_tSys,    Tr("TAB_SYSTEM"));
        _tabCont.SetTabTitle(_tCon,    Tr("TAB_CONTROLS"));
        _tabSys.SetTabTitle (_tSysCam, Tr("TAB_CAMERA"));
        _tabSys.SetTabTitle (_tSysDis, Tr("TAB_DISPLAY"));
        _tabSys.SetTabTitle (_tSysPer, Tr("TAB_PERFORMANCE"));
        _tabSys.SetTabTitle (_tSysAud, Tr("TAB_AUDIO"));
        _tabSys.SetTabTitle (_tSysLan, Tr("TAB_LANGUAGE"));
    }

    private void UpdateSystemTabContainer() {
        switch (_tabSys.CurrentTab) {
            case _tSysCam: { _lbTab.Text = Tr("TAB_SYSTEM") + " | " + Tr("TAB_CAMERA");      break; }
            case _tSysDis: { _lbTab.Text = Tr("TAB_SYSTEM") + " | " + Tr("TAB_DISPLAY");     break; }
            case _tSysPer: { _lbTab.Text = Tr("TAB_SYSTEM") + " | " + Tr("TAB_PERFORMANCE"); break; }
            case _tSysAud: { _lbTab.Text = Tr("TAB_SYSTEM") + " | " + Tr("TAB_AUDIO");       break; }
            case _tSysLan: { _lbTab.Text = Tr("TAB_SYSTEM") + " | " + Tr("TAB_LANGUAGE");    break; }
            default:       {                                                                 break; }
        }
    }

    private void UpdateControlTab() {
        _lbTab.Text = Tr("TAB_CONTROLS");
        for (int i = 0; i < XB.Input.Amount; i++) {
            _bCK[i].Text = Tr(_pS.Input.InputActions[i].Description) + " - "
                           + _pS.Input.InputActions[i].Key;
            int buttonFontSize = 22; 
            //NOTE[ALEX]: empirical font size decrease
            switch (_bCK[i].Text.Length) {
                case > 50: { buttonFontSize = 14; break; }
                case > 47: { buttonFontSize = 15; break; }
                case > 44: { buttonFontSize = 16; break; }
                case > 41: { buttonFontSize = 17; break; }
                case > 39: { buttonFontSize = 18; break; }
                case > 37: { buttonFontSize = 19; break; }
                case > 34: { buttonFontSize = 20; break; }
                // jump from 22 to 21 does not add horizontal space
            }
            _bCK[i].AddThemeFontSizeOverride("font_size", buttonFontSize);
            // Godot.GD.Print(_bCK[i].Text + " " + _bCK[i].Text.Length + " " + buttonFontSize);
        }
    }

    // wrapped into a function to avoid large repeated function calls
    private void UpdateSettingsTab() {
        _pS.Sett.UpdateSettingsTabs(_slCamHor, _lbCamHor, _slCamVer, _lbCamVer,
                                    _slFov, _lbFov, _slFrame, _lbFrame,
                                    _obRes, _obMode,
                                    _cbFps, _cbGuides, _cbVSync, _cbBlock, _cbQTVis,
                                    _obMSAA, _obSSAA,
                                    _cbTAA, _cbDebanding,
                                    _lbShdwSize, _slShdwSize, _obShdwFilter,
                                    _lbShdwDist, _slShdwDist,
                                    _lbLOD, _slLOD,
                                    _obSSAO, _cbSSAOHalf,
                                    _obSSIL, _cbSSILHalf,
                                    _cbSSR, _slVolume, _lbVolume, _obLanguage       );
    }

    private void UpdateSettingsSliders() {
        _pS.Sett.UpdateSliders(_slFrame, _lbFrame, _slShdwSize, _lbShdwSize, _slLOD, _lbLOD);
    }

    private void ApplySettings() {
        UpdateSettingsTab();
        _pS.Sett.SettingsCodeFromSettings(_leSetCode);
    }

    public void ShowMessage(string msg) {
        _crMsg.Show();
        _lbMsg.Text  = msg;
        _t           = 0.0f;
        _crMsg.Color = XB.Col.Msg;
    }

    private void ButtonResumeOnPressed() {
        PlayUISound(XB.ResourcePaths.ButtonAudio);
        Godot.Input.MouseMode = Godot.Input.MouseModeEnum.Captured; // hide cursor
        GetTree().Paused      = false;
        Hide();
        _ctrlPopupS.Hide();
        _ctrlPopupA.Hide();
        _ctrlPopupG.Hide();
        _ctrlPopupQ.Hide();
        _pS.AppSt = XB.AppState.Application;
    }

    private void ButtonQuitOnPressed() {
        GetTree().Quit();
    }

    private void SliderShadowDistanceOnDragEnded(bool valueChanged) {
        ShowMessage(_pS.Sett.ChangeShadowDistance(_slShdwDist));
        ApplySettings();
    }

    private void SliderFovOnDragEnded(bool valueChanged) {
        ShowMessage(_pS.Sett.ChangeFov(_slFov));
        ApplySettings();
    }

    private void SliderCamHorOnDragEnded(bool valueChanged) {
        ShowMessage(_pS.Sett.ChangeSensitivityHorizontal(_slCamHor));
        ApplySettings();
    }

    private void SliderCamVerOnDragEnded(bool valueChanged) {
        ShowMessage(_pS.Sett.ChangeSensitivityVertical(_slCamVer));
        ApplySettings();
    }

    private void SliderVolumeOnDragEnded(bool valueChanged) {
        ShowMessage(_pS.Sett.ChangeVolume(_slVolume));
        ApplySettings();
    }

    private void SliderFrameRateOnDragEnded(bool valueChanged) {
        UpdateSettingsSliders();
        ApplySettings();
    }

    private void SliderShadowSizeOnDragEnded(bool valueChanged) {
        UpdateSettingsSliders();
        ApplySettings();
    }

    private void SliderLODOnDragEnded(bool valueChanged) {
        UpdateSettingsSliders();
        ApplySettings();
    }

    private void ButtonAppDefaultsOnPressed() {
        PlayUISound(XB.ResourcePaths.ButtonAudio);
        ShowMessage(_pS.Sett.ApplicationDefaults());
        ApplySettings();
    }

    private void ButtonApplyOnPressed () {
        PlayUISound(XB.ResourcePaths.ButtonAudio);
        string preset = _obPresets.GetItemText(_obPresets.GetSelectedId());
        ShowMessage(_pS.Sett.PresetSettings(_pS.Sett.Presets[preset]));
        ApplySettings();
    }

    private void ButtonTAAOnPressed() {
        ShowMessage(_pS.Sett.ToggleTAA());
        ApplySettings();
    }

    private void ButtonDebandingOnPressed() {
        ShowMessage(_pS.Sett.ToggleDebanding());
        ApplySettings();
    }

    private void ButtonSSAOHalfOnPressed() {
        ShowMessage(_pS.Sett.ToggleSSAOHalf());
        ApplySettings();
    }

    private void ButtonSSILHalfOnPressed() {
        ShowMessage(_pS.Sett.ToggleSSILHalf());
        ApplySettings();
    }

    private void ButtonSSROnPressed() {
        ShowMessage(_pS.Sett.ToggleSSR());
        ApplySettings();
    }

    private void ButtonShowFPSOnPressed() {
        ShowMessage(_pS.Sett.ToggleShowFPS());
        ApplySettings();
    }

    private void ButtonShowGuidesOnPressed() {
        ShowMessage(_pS.Sett.ToggleShowGuides());
        ApplySettings();
    }

    private void ButtonVSyncOnPressed() {
        ShowMessage(_pS.Sett.ToggleVSync());
        ApplySettings();
    }

    private void ButtonBlockGridOnPressed() {
        ShowMessage(_pS.Sett.ToggleBlockGrid());
        ApplySettings();
    }

    private void ButtonQuadTreeVisOnPressed() {
        ShowMessage(_pS.Sett.ToggleQuadTreeVis());
        ApplySettings();
    }

    private void ButtonPopupCancelOnPressed() {
        PlayUISound(XB.ResourcePaths.ButtonAudio);
        _ctrlPopupS.Hide();
        _ctrlPopupA.Hide();
        _ctrlPopupQ.Hide();
        _ctrlPopupG.Hide();
    }

    private void ButtonPopupApplySpheresOnPressed() {
        PlayUISound(XB.ResourcePaths.ButtonAudio);
        _ctrlPopupA.Show();
    }

    private void ButtonPopupQuitOnPressed() {
        PlayUISound(XB.ResourcePaths.ButtonAudio);
        _ctrlPopupQ.Show();
    }

    private void ButtonDefaultsCKOnPressed() {
        PlayUISound(XB.ResourcePaths.ButtonAudio);
        ShowMessage(Tr("DEFAULT_KEYBINDINGS"));
        _pS.Input.DefaultInputActions();
        UpdateControlTab();
    }

    private void ButtonCK00OnPressed() { ControlsChange( 0); }
    private void ButtonCK01OnPressed() { ControlsChange( 1); }
    private void ButtonCK02OnPressed() { ControlsChange( 2); }
    private void ButtonCK03OnPressed() { ControlsChange( 3); }
    private void ButtonCK04OnPressed() { ControlsChange( 4); }
    private void ButtonCK05OnPressed() { ControlsChange( 5); }
    private void ButtonCK06OnPressed() { ControlsChange( 6); }
    private void ButtonCK07OnPressed() { ControlsChange( 7); }
    private void ButtonCK08OnPressed() { ControlsChange( 8); }
    private void ButtonCK09OnPressed() { ControlsChange( 9); }
    private void ButtonCK10OnPressed() { ControlsChange(10); }
    private void ButtonCK11OnPressed() { ControlsChange(11); }
    private void ButtonCK12OnPressed() { ControlsChange(12); }
    private void ButtonCK13OnPressed() { ControlsChange(13); }
    private void ButtonCK14OnPressed() { ControlsChange(14); }
    private void ButtonCK15OnPressed() { ControlsChange(15); }
    private void ButtonCK16OnPressed() { ControlsChange(16); }
    private void ButtonCK17OnPressed() { ControlsChange(17); }
    private void ButtonCK18OnPressed() { ControlsChange(18); }
    private void ButtonCK19OnPressed() { ControlsChange(19); }
    private void ButtonCK20OnPressed() { ControlsChange(20); }
    private void ButtonCK21OnPressed() { ControlsChange(21); }
    private void ButtonCK22OnPressed() { ControlsChange(22); }
    private void ButtonCK23OnPressed() { ControlsChange(23); }

    private void ControlsChange(int id) {
        PlayUISound(XB.ResourcePaths.ButtonAudio);
        _setKeyID        = id;
        _setKey          = true;
        _lockInput       = true;
        _mouseRelease    = false;
        _chngMsg.Visible = true;
        Godot.Input.MouseMode = Godot.Input.MouseModeEnum.Captured; // hide cursor
    }

    private void OptionButtonLanguageOnItemSelected(long id) {
        ShowMessage(_pS.Sett.ChangeLanguage(_obLanguage));
        ApplySettings();
        UpdateSystemTabContainer();
        UpdateTabNames();
    }

    private void OptionButtonPresetsOnItemSelected(long id) {
        //NOTE[ALEX]: settings only get applied when the button is clicked
    }

    private void OptionButtonResOnItemSelected(long id) {
        _pS.Sett.ChangeResolution(_obRes);
        ApplySettings();
    }

    private void OptionButtonModeOnItemSelected(long id) {
        _pS.Sett.ChangeMode(_obMode);
        ApplySettings();
    }

    private void OptionButtonMSAAOnItemSelected(long id) {
        _pS.Sett.ChangeMSAA(_obMSAA);
        ApplySettings();
    }

    private void OptionButtonSSAAOnItemSelected(long id) {
        _pS.Sett.ChangeSSAA(_obSSAA);
        ApplySettings();
    }

    private void OptionButtonSSILOnItemSelected(long id) {
        _pS.Sett.ChangeSSIL(_obSSIL);
        ApplySettings();
    }

    private void OptionButtonShdwFilterOnItemSelected(long id) {
        _pS.Sett.ChangeShadowFilter(_obShdwFilter);
        ApplySettings();
    }

    private void OptionButtonSSAOOnItemSelected(long id) {
        _pS.Sett.ChangeSSAO(_obSSAO);
        ApplySettings();
    }

    private void ButtonApplyCodeOnPressed() {
        string code = _leSetCode.Text;
        if (_pS.Sett.ValidateSettingsCode(code)) {
            _pS.Sett.SettingsFromSettingsCode(code);
            ApplySettings();
            ShowMessage(Tr("SETCODE_APPLIED"));
        } else {
            _pS.Sett.SettingsCodeFromSettings(_leSetCode);
            ShowMessage(Tr("INCORRECT_SETCODE"));
        }
    }

    private void ButtonGenerateTerrainOnPressed() {
        PlayUISound(XB.ResourcePaths.ButtonAudio);
        _ctrlPopupG.Show();
        // restore values to defaults when opening the generation dialog
        _slGenHeight.Value = XB.WData.GenHeightDef;
        _lbGenHeight.Text  = _slGenHeight.Value.ToString();
        _slGenScale.Value  = XB.WData.GenScaleDef;
        _lbGenScale.Text   = _slGenScale.Value.ToString();
        _slGenOffX.Value   = XB.WData.GenOffXDef;
        _lbGenOffX.Text    = _slGenOffX.Value.ToString();
        _slGenOffZ.Value   = XB.WData.GenOffZDef;
        _lbGenOffZ.Text    = _slGenOffZ.Value.ToString();
        _slGenOct.Value    = XB.WData.GenOctDef;
        _lbGenOct.Text     = _slGenOct.Value.ToString();
        _slGenPers.Value   = XB.WData.GenPersDef;
        _lbGenPers.Text    = _slGenPers.Value.ToString();
        _slGenLac.Value    = XB.WData.GenLacDef;
        _lbGenLac.Text     = _slGenLac.Value.ToString();
        _slGenExp.Value    = XB.WData.GenExpDef;
        _lbGenExp.Text     = _slGenExp.Value.ToString();
        _updateGenTex      = true;
        _cbGenUpd.ButtonPressed = _updateGenTex;
        uint seed = (uint)System.DateTime.Now.GetHashCode();
        _leGenSeed.Text = seed.ToString();
        ButtonGenSeedApplyOnPressed();
    }

    private void ButtonApplySpheresOnPressed() {
        XB.ManagerSphere.ApplyTerrain(_pS.Hud);
        XB.WData.UpdateTerrain(false, _pS.Hud, this, _mainRoot);
        _pS.PCtrl.SpawnPlayer(new Godot.Vector2(_pS.PCtrl.GlobalPosition.X,
                                                _pS.PCtrl.GlobalPosition.Z ));
        ShowMessage(Tr("APPLIED_SPHERES"));
        ButtonResumeOnPressed();
    }

    private void ButtonClearSpheresOnPressed() {
        XB.ManagerSphere.ClearSpheres(_pS.Hud);
        ShowMessage(Tr("CLEARED_SPHERES"));
        ButtonResumeOnPressed();
    }

    private void ButtonShowStartupOnPressed() {
        PlayUISound(XB.ResourcePaths.ButtonAudio);
        GetTree().Paused = false;
        Hide();
        _ctrlPopupS.Hide();
        _ctrlPopupA.Hide();
        _ctrlPopupG.Hide();
        _ctrlPopupQ.Hide();
        ShowStartupScreen();
    }

    private void ButtonPopupGenApplyOnPressed() {
        ButtonClearSpheresOnPressed();
        if (!_updateGenTex) { GenerateTerrainHeights(); } // only if heightmap has not been generated yet
        XB.Terrain.HeightReplace(XB.WData.TerrainHeights, XB.WData.TerrainHeightsMod,
                                 XB.WData.WorldVerts.X, XB.WData.WorldVerts.Y,
                                 ref XB.WData.LowestPoint, ref XB.WData.HighestPoint );
        XB.Terrain.HeightScale(XB.WData.TerrainHeights, XB.WData.WorldVerts.X,
                               XB.WData.WorldVerts.Y, (float)_slGenHeight.Value,
                               ref XB.WData.LowestPoint, ref XB.WData.HighestPoint);
        XB.WData.UpdateTerrain(false, _pS.Hud, this, _mainRoot);
        _pS.PCtrl.SpawnPlayer(new Godot.Vector2(_pS.PCtrl.GlobalPosition.X,
                                                _pS.PCtrl.GlobalPosition.Z ));
        ShowMessage(Tr("GENERATED_TERRAIN"));
        _ctrlPopupG.Hide();
        ButtonResumeOnPressed();
    }

    private void LineEditGenerateSeedOnTextChanged(string text) {
        uint seed = 0;
        if (_leGenSeed.Text != "" && !System.UInt32.TryParse(_leGenSeed.Text, out seed)) {
            seed = (uint)System.DateTime.Now.GetHashCode();
            _leGenSeed.Text = seed.ToString();
        }
    }

    private void ButtonGenSeedApplyOnPressed() {
        uint seed = 0;
        if (_leGenSeed.Text != "") { seed = System.UInt32.Parse(_leGenSeed.Text); }
        else                       { seed = (uint)System.DateTime.Now.GetHashCode(); }
        _leGenSeed.Text = seed.ToString();
        XB.Random.InitializeRandom(seed);
        if (_updateGenTex) { GenerateTerrainHeights(); }

        // create new code to make iteration easier
        seed = (uint)System.DateTime.Now.GetHashCode();
        _leGenSeed.Text = seed.ToString();
    }

    private void SliderGenHeightOnDragEnded(bool valueChanged) {
        if (_updateGenTex) { GenerateTerrainHeights(); }
    }

    private void SliderGenScaleOnDragEnded(bool valueChanged) {
        if (_updateGenTex) { GenerateTerrainHeights(); }
    }

    private void SliderGenOffXOnDragEnded(bool valueChanged) {
        if (_updateGenTex) { GenerateTerrainHeights(); }
    }

    private void SliderGenOffZOnDragEnded(bool valueChanged) {
        if (_updateGenTex) { GenerateTerrainHeights(); }
    }

    private void SliderGenOctOnDragEnded(bool valueChanged) {
        if (_updateGenTex) { GenerateTerrainHeights(); }
    }

    private void SliderGenPersOnDragEnded(bool valueChanged) {
        if (_updateGenTex) { GenerateTerrainHeights(); }
    }

    private void SliderGenLacOnDragEnded(bool valueChanged) {
        if (_updateGenTex) { GenerateTerrainHeights(); }
    }

    private void SliderGenExpOnDragEnded(bool valueChanged) {
        if (_updateGenTex) { GenerateTerrainHeights(); }
    }

    private void ButtonGenUpdOnPressed() {
        _updateGenTex = !_updateGenTex;
        if (_updateGenTex) { GenerateTerrainHeights(); }
    }

    private void GenerateTerrainHeights() {
        XB.Terrain.FBM(XB.WData.TerrainHeightsMod,
                       XB.WData.WorldVerts.X, XB.WData.WorldVerts.Y,
                       XB.WData.WorldSize.X, XB.WData.WorldSize.Y,
                       (float)_slGenScale.Value,
                       (float)_slGenOffX.Value, (float)_slGenOffZ.Value,
                       (int)_slGenOct.Value, (float)_slGenPers.Value,
                       (float)_slGenLac.Value, (float)_slGenExp.Value   );
        float lowest  = 0.0f;
        float highest = 0.0f;
        XB.Terrain.FindLowestHighest(XB.WData.TerrainHeightsMod, XB.WData.WorldVerts.X, 
                                     XB.WData.WorldVerts.Y, ref lowest, ref highest    );
        XB.Terrain.UpdateHeightMap(XB.WData.TerrainHeightsMod,
                                   lowest, highest, _imgGenMap);
        _lbGenLow.Text     = "0.0m";
        _lbGenHigh.Text    = _slGenHeight.Value.ToString(_heightFormat) + "m";
        _lbGenCurSeed.Text = XB.Random.RandomSeed.ToString();

        _texGenMap.Update(_imgGenMap);
    }

    // to have the labels show the slider values while sliding, they have to be updated every frame
    private void UpdateGenLabels() {
        _lbGenHeight.Text = _slGenHeight.Value.ToString(_valueFormat);
        _lbGenScale.Text  = _slGenScale.Value.ToString(_scaleFormat);
        _lbGenOffX.Text   = _slGenOffX.Value.ToString(_valueFormat);
        _lbGenOffZ.Text   = _slGenOffZ.Value.ToString(_valueFormat);
        _lbGenOct.Text    = _slGenOct.Value.ToString();
        _lbGenPers.Text   = _slGenPers.Value.ToString(_valueFormat);
        _lbGenLac.Text    = _slGenLac.Value.ToString(_valueFormat);
        _lbGenExp.Text    = _slGenExp.Value.ToString(_valueFormat);
    }

    public void PlayUISound(string path) {
        var sScn  = Godot.ResourceLoader.Load<Godot.PackedScene>(path);
        var sound = sScn.Instantiate();
        _mainRoot.AddChild(sound);
    }

    // look through all inputs and hide the buttons for inputs that are not used
    //NOTE[ALEX]: the translation dictionary (appText.csv) expects unused inputs to begin with '~'
    //            in all languages
    private void HideUnusedKeys() {
        for (int i = 0; i < _bCK.Length; i++) {
            if (Tr(_pS.Input.InputActions[i].Description)[0] == '~') { _bCK[i].Hide(); }
        }
    }
}
} // namespace close 
