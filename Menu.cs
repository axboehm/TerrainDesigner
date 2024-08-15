#define XBDEBUG
namespace XB { // namespace opegn
public enum MenuType {
    None,
    Pause,
    Save,
}
public partial class Menu : Godot.Control {
    [Godot.Export] private XB.PController     _player;
    [Godot.Export] private XB.HUD             _hud;
    [Godot.Export] private Godot.Label        _lbTab;
    [Godot.Export] private Godot.Label        _lbMsg;
    [Godot.Export] private Godot.ColorRect    _crMsg;
    [Godot.Export] private Godot.TabContainer _tabCont;
                   private int                _tabPrev;
                   private XB.MenuType        _menuType;
                   private bool               _justOpened = true;
                   private int                _scroll     = 100;  // amount to scroll (multiplier)
                   private float              _t          = 0.0f;
                   private float              _msgDur     = 2.0f; // in seconds
    [Godot.Export] private Godot.Button       _bResume;

    // tab numbers
    private const int _tPau    = 0;
    private const int _tSys    = 1;
    private const int _tSysCam = 0;
    private const int _tSysDis = 1;
    private const int _tSysPer = 2;
    private const int _tSysAud = 3;
    private const int _tSysLan = 4;
    private const int _tCon    = 2;
    private const int _tConKey = 0;
    private const int _tConCon = 1;

    // pause tab
    [Godot.Export] private Godot.Button   _bQuit;
    [Godot.Export] private Godot.Button   _bGenerate;
    [Godot.Export] private Godot.LineEdit _leGenSeed;
    [Godot.Export] private Godot.Button   _bApplySpheres;

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
        // display
    [Godot.Export] private Godot.OptionButton _obRes;
    [Godot.Export] private Godot.OptionButton _obMode;
    [Godot.Export] private Godot.Label        _lbFrame;
    [Godot.Export] private Godot.Slider       _slFrame;
    [Godot.Export] private Godot.Button       _cbFps;
    [Godot.Export] private Godot.Button       _cbVSync;
    [Godot.Export] private Godot.Button       _cbBlock;
    [Godot.Export] private Godot.Button       _cbQTVis;
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
    [Godot.Export] private Godot.TabContainer _tabCtrl;
    [Godot.Export] private Godot.Button[]     _bCK          = new Godot.Button[XB.Input.Amount];
    [Godot.Export] private Godot.Button       _bDefaultsCK;
    [Godot.Export] private Godot.Control      _chngMsg;
                   private bool               _setKey       = false;
                   private bool               _lockInput    = false;
                   private bool               _mouseRelease = false;
                   private int                _setKeyID     = 0;

    // confirmation popup
    [Godot.Export] private Godot.Control _ctrlPopup;
    [Godot.Export] private Godot.Label   _lbPopup;
    [Godot.Export] private Godot.Button  _bPopCancel;
    [Godot.Export] private Godot.Button  _bPopQuit;

    public override void _Ready() {
        ProcessMode = ProcessModeEnum.WhenPaused;

        _tabCont.CurrentTab = _tPau;
        _tabSys.CurrentTab  = _tSysCam;
        _tabCtrl.CurrentTab = _tConKey;
        _tabPrev            = _tabCont.CurrentTab;
        _crMsg.Visible      = false;
        _bResume.Pressed   += ButtonResumeOnPressed;

        // pause tab
        _bQuit.Pressed         += ButtonPopupQuitOnPressed;
        _bGenerate.Pressed     += ButtonGenerateTerrainOnPressed;
        _leGenSeed.TextChanged += LineEditGenerateSeedOnTextChanged;
        _leGenSeed.MaxLength    = 8;
        _bApplySpheres.Pressed += ButtonApplySpheresOnPressed;

        // system tab
        _bApplyCode.Pressed    += ButtonApplyCodeOnPressed;
        // camera
        _slFov.MinValue        = XB.AData.FovMin;
        _slFov.MaxValue        = XB.AData.FovMax;
        _slFov.DragEnded      += SliderFovOnDragEnded;
        _slCamHor.DragEnded   += SliderCamHorOnDragEnded;
        _slCamVer.DragEnded   += SliderCamVerOnDragEnded;
        _bAppDefaults.Pressed += ButtonAppDefaultsOnPressed;
        _bApplyPreset.Pressed += ButtonApplyOnPressed;
        foreach (var preset in XB.AData.Presets) { _obPresets.AddItem(preset.Key); }
        XB.Settings.AddSeparators(_obPresets);
        _obPresets.Select(1); // select default preset on startup
        _obPresets.ItemSelected += OptionButtonPresetsOnItemSelected;
        // display
        foreach (var resolution in XB.AData.Resolutions) { _obRes.AddItem(resolution.Key); }
        XB.Settings.AddSeparators(_obRes);
        _obRes.ItemSelected += OptionButtonResOnItemSelected;
        foreach (var windowMode in XB.AData.WindowModes) { _obMode.AddItem(windowMode); }
        XB.Settings.AddSeparators(_obMode);
        _obMode.ItemSelected += OptionButtonModeOnItemSelected;
        _cbFps.Pressed       += ButtonShowFPSOnPressed;
        _cbVSync.Pressed     += ButtonVSyncOnPressed;
        _cbBlock.Pressed     += ButtonBlockGridOnPressed;
        _cbQTVis.Pressed     += ButtonQuadTreeVisOnPressed;
        _slFrame.DragEnded   += SliderFrameRateOnDragEnded;
        // performance
        _scrPerf.ScrollVertical = 0;
        _cbTAA.Pressed         += ButtonTAAOnPressed;
        _cbDebanding.Pressed   += ButtonDebandingOnPressed;
        _cbSSAOHalf.Pressed    += ButtonSSAOHalfOnPressed;
        _cbSSILHalf.Pressed    += ButtonSSILHalfOnPressed;
        _cbSSR.Pressed         += ButtonSSROnPressed;
        foreach (var option in XB.AData.MSAA) { _obMSAA.AddItem(option); }
        XB.Settings.AddSeparators(_obMSAA);
        _obMSAA.ItemSelected += OptionButtonMSAAOnItemSelected;
        foreach (var option in XB.AData.SSAA) { _obSSAA.AddItem(option); }
        XB.Settings.AddSeparators(_obSSAA);
        _obSSAA.ItemSelected += OptionButtonSSAAOnItemSelected;
        foreach (var option in XB.AData.ShadowFilters) { _obShdwFilter.AddItem(option); }
        XB.Settings.AddSeparators(_obShdwFilter);
        _obShdwFilter.ItemSelected += OptionButtonShdwFilterOnItemSelected;
        foreach (var option in XB.AData.SSAO) { _obSSAO.AddItem(option); }
        XB.Settings.AddSeparators(_obSSAO);
        _obSSAO.ItemSelected += OptionButtonSSAOOnItemSelected;
        foreach (var option in XB.AData.SSIL) { _obSSIL.AddItem(option); }
        XB.Settings.AddSeparators(_obSSIL);
        _obSSIL.ItemSelected += OptionButtonSSILOnItemSelected;
        _slShdwSize.DragEnded += SliderShadowSizeOnDragEnded;
        _slShdwDist.DragEnded += SliderShadowDistanceOnDragEnded;
        _slLOD.DragEnded      += SliderLODOnDragEnded;
        // audio
        _slVolume.DragEnded += SliderVolumeOnDragEnded;
        // language
        foreach (var language in XB.AData.Languages) { _obLanguage.AddItem(language); }
        XB.Settings.AddSeparators(_obLanguage);
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
        _bDefaultsCK.Pressed += ButtonDefaultsCKOnPressed;
        _chngMsg.Visible      = false;

        // confirmation popup
        _bPopCancel.Pressed += ButtonPopupCancelOnPressed;
        _bPopQuit.Pressed   += ButtonQuitOnPressed;
        _ctrlPopup.Hide();
    }

    // handling input before any other node to reassign controls
    public override void _Input(Godot.InputEvent @event) {
        if (_tabCont.CurrentTab != _tCon || !_setKey) return;
        if (!XB.AData.Controller) {
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
                    if (XB.AData.Input.InputActions[i].Key == key) {
                        ShowMessage(Tr("INPUT_ALREADY_USED"));
                        _chngMsg.Visible      = false;
                        Godot.Input.MouseMode = Godot.Input.MouseModeEnum.Visible;
                        return;
                    }
                }

                var iAction       = XB.AData.Input.InputActions[_setKeyID];
                    iAction.Event = @event;
                    iAction.Key   = key;
                Godot.InputMap.ActionEraseEvents(iAction.Name);
                Godot.InputMap.ActionAddEvent(iAction.Name, @event);

                ShowMessage(Tr("KEYBINDINGS_UPDATED") + Tr(iAction.Description) + ".");

                _chngMsg.Visible      = false;
                Godot.Input.MouseMode = Godot.Input.MouseModeEnum.Visible;
                UpdateControlTab();

                XB.AData.Input.ConsumeAllInputs();
            }
        } 
        //TODO[ALEX]: implement controller keybinding
        //            does this make sense for changing controls or 
        //            does it not really matter and I do it by showing the available buttons 
        //            and mapping actions to them
    }

    public override void _PhysicsProcess(double delta) {
        if (_justOpened) {
            _justOpened = false;
            return;
        }

        XB.AData.Input.GetInputs();

        _t     += (float)delta;

        if (_t >= _msgDur) {
            _t          = 0.0f;
            _lbMsg.Text = "";
            _crMsg.Hide();
        } else {
            _crMsg.Color = XB.Col.Msg.Lerp(XB.Col.MsgFade, _t/_msgDur);
        }

        // player input
        if        (XB.AData.Input.Start && !_lockInput) {
            XB.AData.Input.ConsumeInputStart();
            if (_menuType == XB.MenuType.Pause) ButtonResumeOnPressed();
        } else if (XB.AData.Input.CamY > 0.0f) {
            //NOTE[ALEX]: reference for scrollcontainers
            // _scrADesc.ScrollVertical -= XB.Utils.MaxI(1, (int)(delta*_scroll));
            // _scrGDesc.ScrollVertical -= XB.Utils.MaxI(1, (int)(delta*_scroll));
        } else if (XB.AData.Input.CamY < 0.0f) {
            // _scrADesc.ScrollVertical += XB.Utils.MaxI(1, (int)(delta*_scroll));
            // _scrGDesc.ScrollVertical += XB.Utils.MaxI(1, (int)(delta*_scroll));
        } else {
            if (XB.AData.Controller) {
                int tabCount = _tabCont.GetTabCount();
                if        (XB.AData.Input.SRTop || XB.AData.Input.SRBot) {
                    if (_tabCont.CurrentTab < tabCount-1) _tabCont.CurrentTab += 1;
                    else                                  _tabCont.CurrentTab  = 0;
                } else if (XB.AData.Input.SLTop || XB.AData.Input.SLBot) {
                    if (_tabCont.CurrentTab > 0)         _tabCont.CurrentTab -= 1;
                    else                                 _tabCont.CurrentTab  = tabCount-1;
                }
            }
        }
        //TODO[ALEX]: deal with full controller control (subtabs, etc)
        
        //NOTE[ALEX]: _lockInput is required in addition to _setKey for setting new input keys,
        //            otherwise the pressed input will be read again in this method's GetInputs()
        //            call because the input sticks longer than expected before being cleared
        if (!_setKey && _lockInput) {
            _lockInput = false;
        }

        // change tabs
        if (_tabCont.CurrentTab != _tabPrev) {
            switch (_tabCont.CurrentTab) {
                case _tPau: { // pause
                    _lbTab.Text = Tr("TAB_PAUSE");
                    UpdatePauseTab();
                    break;
                }
                case _tSys: { // system
                    UpdateSystemTabContainer();
                    UpdateSettingsTab();
                    break;
                }
                case _tCon: { // controls
                    UpdateControlTabContainer();
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
                break;
            }
            case _tSys: { // system
                UpdateSystemTabContainer();
                //NOTE[ALEX]: the dragEnded slider signal does not get emitted when
                //            clicking a value on the slider (not dragging)
                if (_slFov.Value != XB.AData.FovDef) {
                    SliderFovOnDragEnded(true);
                }
                if ((float)_slCamHor.Value != XB.AData.CamXSens*XB.Settings.CamSliderMult) {
                    SliderCamHorOnDragEnded(true);
                }
                if ((float)_slCamVer.Value != XB.AData.CamYSens*XB.Settings.CamSliderMult) {
                    SliderCamVerOnDragEnded(true);
                }
                if ((float)_slVolume.Value != (float)Godot.AudioServer.GetBusVolumeDb(0)) {
                    SliderVolumeOnDragEnded(true);
                }
                if (_slShdwDist.Value != XB.AData.ShadowDistance) {
                    SliderShadowDistanceOnDragEnded(true);
                }
                break;
            }
            case _tCon: { // controls
                UpdateControlTabContainer();
                break;
            }
        }
    }

    public void OpenMenu() {
        XB.AData.Input.ConsumeInputStart();
        _menuType   = XB.MenuType.Pause;
        _lbTab.Text = Tr("TAB_PAUSE");
        _bResume.Show();
        _bResume.GrabFocus();
        _tabCont.Show();
        _tabCont.CurrentTab   = _tPau;
        _justOpened           = true;
        Godot.Input.MouseMode = Godot.Input.MouseModeEnum.Visible;
        GetTree().Paused      = true;
        Show();

        _setKey       = false;
        _lockInput    = false;
        _mouseRelease = false;

        _lbMsg.Text = "";
        _crMsg.Hide();
        UpdatePauseTab();
        UpdateTabNames();
        XB.Settings.SettingsCodeFromSettings(_leSetCode);
    }

    private void UpdatePauseTab() {
        _leGenSeed.Text = ((uint)System.DateTime.Now.GetHashCode()).ToString();
    }

    private void UpdateTabNames() {
        _tabCont.SetTabTitle (_tPau,    Tr("TAB_PAUSE"));
        _tabCont.SetTabTitle (_tSys,    Tr("TAB_SYSTEM"));
        _tabCont.SetTabTitle (_tCon,    Tr("TAB_CONTROLS"));
        _tabSys.SetTabTitle  (_tSysCam, Tr("TAB_CAMERA"));
        _tabSys.SetTabTitle  (_tSysDis, Tr("TAB_DISPLAY"));
        _tabSys.SetTabTitle  (_tSysPer, Tr("TAB_PERFORMANCE"));
        _tabSys.SetTabTitle  (_tSysAud, Tr("TAB_AUDIO"));
        _tabSys.SetTabTitle  (_tSysLan, Tr("TAB_LANGUAGE"));
        _tabCtrl.SetTabTitle (_tConKey, Tr("TAB_KEYBOARD"));
        _tabCtrl.SetTabTitle (_tConCon, Tr("TAB_CONTROLLER"));
    }

    private void UpdateSystemTabContainer() {
        switch (_tabSys.CurrentTab) {
            case _tSysCam: _lbTab.Text = Tr("TAB_SYSTEM") + " | " + Tr("TAB_CAMERA");      break;
            case _tSysDis: _lbTab.Text = Tr("TAB_SYSTEM") + " | " + Tr("TAB_DISPLAY");     break;
            case _tSysPer: _lbTab.Text = Tr("TAB_SYSTEM") + " | " + Tr("TAB_PERFORMANCE"); break;
            case _tSysAud: _lbTab.Text = Tr("TAB_SYSTEM") + " | " + Tr("TAB_AUDIO");       break;
            case _tSysLan: _lbTab.Text = Tr("TAB_SYSTEM") + " | " + Tr("TAB_LANGUAGE");    break;
            default:                                                                       break;
        }
    }

    private void UpdateControlTabContainer() {
        switch (_tabCtrl.CurrentTab) {
            case _tConKey: _lbTab.Text = Tr("TAB_CONTROLS") + " | " + Tr("TAB_KEYBOARD");   break;
            case _tConCon: _lbTab.Text = Tr("TAB_CONTROLS") + " | " + Tr("TAB_CONTROLLER"); break;
        }
    }

    private void UpdateControlTab() {
        for (int i = 0; i < XB.Input.Amount; i++) {
            _bCK[i].Text = Tr(XB.AData.Input.InputActions[i].Description) + " - " + 
                           XB.AData.Input.InputActions[i].Key;
        }
    }

    private void UpdateSettingsTab() {
        XB.Settings.UpdateSettingsTabs(_slCamHor, _lbCamHor, _slCamVer, _lbCamVer,
                                       _slFov, _lbFov, _slFrame, _lbFrame,
                                       _obRes, _obMode,
                                       _cbFps, _cbVSync, _cbBlock, _cbQTVis,
                                       _obMSAA, _obSSAA,
                                       _cbTAA, _cbDebanding,
                                       _lbShdwSize, _slShdwSize, _obShdwFilter,
                                       _lbShdwDist, _slShdwDist,
                                       _lbLOD, _slLOD,
                                       _obSSAO, _cbSSAOHalf,
                                       _obSSIL, _cbSSILHalf,
                                       _cbSSR, _slVolume, _lbVolume, _obLanguage  );
    }

    private void UpdateSettingsSliders() {
        XB.Settings.UpdateSliders(_slFrame, _lbFrame, _slShdwSize, _lbShdwSize, _slLOD, _lbLOD);
    }

    private void ApplySettings() {
        UpdateSettingsTab();
        XB.PersistData.UpdateScreen();
        XB.PersistData.UpdateAudio();
        XB.PersistData.UpdateLanguage();
        XB.Settings.SettingsCodeFromSettings(_leSetCode);
    }

    private void ShowMessage(string msg) {
        _crMsg.Show();
        _lbMsg.Text  = msg;
        _t           = 0.0f;
        _crMsg.Color = XB.Col.Msg;
    }

    public void ButtonResumeOnPressed() {
        XB.Utils.PlayUISound(XB.ResourcePaths.ButtonAudio);
        Godot.Input.MouseMode = Godot.Input.MouseModeEnum.Captured;
        GetTree().Paused      = false;
        Hide();
        _menuType = XB.MenuType.None;
    }

    public void ButtonQuitOnPressed() {
        GetTree().Quit();
    }

    public void SliderShadowDistanceOnDragEnded(bool valueChanged) {
        if (!valueChanged) return;
        ShowMessage(XB.Settings.ChangeShadowDistance(_slShdwDist));
        ApplySettings();
    }

    public void SliderFovOnDragEnded(bool valueChanged) {
        if (!valueChanged) return;
        ShowMessage(XB.Settings.ChangeFov(_slFov));
        ApplySettings();
    }

    public void SliderCamHorOnDragEnded(bool valueChanged) {
        if (!valueChanged) return;
        ShowMessage(XB.Settings.ChangeSensitivityHorizontal(_slCamHor));
        ApplySettings();
    }

    public void SliderCamVerOnDragEnded(bool valueChanged) {
        if (!valueChanged) return;
        ShowMessage(XB.Settings.ChangeSensitivityVertical(_slCamVer));
        ApplySettings();
    }

    public void SliderVolumeOnDragEnded(bool valueChanged) {
        if (!valueChanged) return;
        ShowMessage(XB.Settings.ChangeVolume(_slVolume));
        ApplySettings();
    }

    public void SliderFrameRateOnDragEnded(bool valueChanged) {
        UpdateSettingsSliders();
        ApplySettings();
    }

    public void SliderShadowSizeOnDragEnded(bool valueChanged) {
        UpdateSettingsSliders();
        ApplySettings();
    }

    public void SliderLODOnDragEnded(bool valueChanged) {
        UpdateSettingsSliders();
        ApplySettings();
    }

    public void ButtonAppDefaultsOnPressed() {
        XB.Utils.PlayUISound(XB.ResourcePaths.ButtonAudio);
        ShowMessage(XB.Settings.ApplicationDefaults());
        ApplySettings();
    }

    public void ButtonApplyOnPressed () {
        XB.Utils.PlayUISound(XB.ResourcePaths.ButtonAudio);
        string preset = _obPresets.GetItemText(_obPresets.GetSelectedId());
        ShowMessage(XB.Settings.PresetSettings(XB.AData.Presets[preset]));
        ApplySettings();
    }

    public void ButtonTAAOnPressed() {
        ShowMessage(XB.Settings.ToggleTAA());
        ApplySettings();
    }

    public void ButtonDebandingOnPressed() {
        ShowMessage(XB.Settings.ToggleDebanding());
        ApplySettings();
    }

    public void ButtonSSAOHalfOnPressed() {
        ShowMessage(XB.Settings.ToggleSSAOHalf());
        ApplySettings();
    }

    public void ButtonSSILHalfOnPressed() {
        ShowMessage(XB.Settings.ToggleSSILHalf());
        ApplySettings();
    }

    public void ButtonSSROnPressed() {
        ShowMessage(XB.Settings.ToggleSSR());
        ApplySettings();
    }

    public void ButtonShowFPSOnPressed() {
        ShowMessage(XB.Settings.ToggleShowFPS());
        ApplySettings();
    }

    public void ButtonVSyncOnPressed() {
        ShowMessage(XB.Settings.ToggleVSync());
        ApplySettings();
    }

    public void ButtonBlockGridOnPressed() {
        ShowMessage(XB.Settings.ToggleBlockGrid());
        ApplySettings();
    }

    public void ButtonQuadTreeVisOnPressed() {
        ShowMessage(XB.Settings.ToggleQuadTreeVis());
        ApplySettings();
    }

    public void ButtonPopupCancelOnPressed() {
        XB.Utils.PlayUISound(XB.ResourcePaths.ButtonAudio);
        _ctrlPopup.Hide();
    }

    public void ButtonPopupQuitOnPressed() {
        XB.Utils.PlayUISound(XB.ResourcePaths.ButtonAudio);
        _ctrlPopup.Show();
        _lbPopup.Text = Tr("QUIT_QUESTION");
    }

    public void ButtonDefaultsCKOnPressed() {
        XB.Utils.PlayUISound(XB.ResourcePaths.ButtonAudio);
        ShowMessage(Tr("DEFAULT_KEYBINDINGS"));
        XB.AData.Input.DefaultInputActions();
        UpdateControlTab();
    }

    public void ButtonCK00OnPressed() {ControlsChange( 0);}
    public void ButtonCK01OnPressed() {ControlsChange( 1);}
    public void ButtonCK02OnPressed() {ControlsChange( 2);}
    public void ButtonCK03OnPressed() {ControlsChange( 3);}
    public void ButtonCK04OnPressed() {ControlsChange( 4);}
    public void ButtonCK05OnPressed() {ControlsChange( 5);}
    public void ButtonCK06OnPressed() {ControlsChange( 6);}
    public void ButtonCK07OnPressed() {ControlsChange( 7);}
    public void ButtonCK08OnPressed() {ControlsChange( 8);}
    public void ButtonCK09OnPressed() {ControlsChange( 9);}
    public void ButtonCK10OnPressed() {ControlsChange(10);}
    public void ButtonCK11OnPressed() {ControlsChange(11);}
    public void ButtonCK12OnPressed() {ControlsChange(12);}
    public void ButtonCK13OnPressed() {ControlsChange(13);}
    public void ButtonCK14OnPressed() {ControlsChange(14);}
    public void ButtonCK15OnPressed() {ControlsChange(15);}
    public void ButtonCK16OnPressed() {ControlsChange(16);}
    public void ButtonCK17OnPressed() {ControlsChange(17);}
    public void ButtonCK18OnPressed() {ControlsChange(18);}
    public void ButtonCK19OnPressed() {ControlsChange(19);}
    public void ButtonCK20OnPressed() {ControlsChange(20);}
    public void ButtonCK21OnPressed() {ControlsChange(21);}
    public void ButtonCK22OnPressed() {ControlsChange(22);}
    public void ButtonCK23OnPressed() {ControlsChange(23);}

    private void ControlsChange(int id) {
        XB.Utils.PlayUISound(XB.ResourcePaths.ButtonAudio);
        _setKeyID             = id;
        _setKey               = true;
        _lockInput            = true;
        _mouseRelease         = false;
        _chngMsg.Visible      = true;
        Godot.Input.MouseMode = Godot.Input.MouseModeEnum.Captured;
    }

    private void OptionButtonLanguageOnItemSelected(long id) {
        ShowMessage(XB.Settings.ChangeLanguage(_obLanguage));
        ApplySettings();
        UpdateSystemTabContainer();
        UpdateTabNames();
    }

    private void OptionButtonPresetsOnItemSelected(long id) {
        ApplySettings();
    }

    private void OptionButtonResOnItemSelected(long id) {
        XB.AData.Resolution = _obRes.GetItemText(_obRes.GetSelectedId());
        ApplySettings();
    }

    private void OptionButtonModeOnItemSelected(long id) {
        if (_obMode.GetSelectedId() == 1) XB.AData.FullScreen = true;
        else                              XB.AData.FullScreen = false;
        ApplySettings();
    }

    private void OptionButtonMSAAOnItemSelected(long id) {
        XB.AData.MSAASel = XB.AData.MSAA[_obMSAA.GetSelectedId()];
        ApplySettings();
    }

    private void OptionButtonSSAAOnItemSelected(long id) {
        XB.AData.SSAASel = XB.AData.SSAA[_obSSAA.GetSelectedId()];
        ApplySettings();
    }

    private void OptionButtonSSILOnItemSelected(long id) {
        XB.AData.SSILSel = XB.AData.SSIL[_obSSIL.GetSelectedId()];
        ApplySettings();
    }

    private void OptionButtonShdwFilterOnItemSelected(long id) {
        XB.AData.ShadowFilter = XB.AData.ShadowFilters[_obShdwFilter.GetSelectedId()];
        ApplySettings();
    }

    private void OptionButtonSSAOOnItemSelected(long id) {
        XB.AData.SSAOSel = XB.AData.SSAO[_obSSAO.GetSelectedId()];
        ApplySettings();
    }

    private void ButtonApplyCodeOnPressed() {
        string code = _leSetCode.Text;
        if (code.Length != (XB.AData.SetCodeLengthL+XB.AData.SetCodeLengthR)) {
            XB.Settings.SettingsCodeFromSettings(_leSetCode);
            ShowMessage(Tr("INCORRECT_SETCODE"));
            return;
        }
        for (int i = 0; i < (XB.AData.SetCodeLengthL+XB.AData.SetCodeLengthR); i++) {
            if (code[i] != '0' && code[i] != '1') {
                XB.Settings.SettingsCodeFromSettings(_leSetCode);
                ShowMessage(Tr("INCORRECT_SETCODE"));
                return;
            }
        }

        XB.Settings.SettingsFromSettingsCode(code);
        ApplySettings();
        ShowMessage(Tr("SETCODE_APPLIED"));
    }

    private void ButtonGenerateTerrainOnPressed() {
        uint seed = 0;
        if (System.UInt32.TryParse(_leGenSeed.Text, out seed)) {
        } else {
            seed = (uint)System.DateTime.Now.GetHashCode();
        }
        XB.Random.InitializeRandom(seed);
        XB.WorldData.GenerateRandomTerrain();
        XB.WorldData.UpdateTerrain(false);

        _player.SpawnPlayer(new Godot.Vector2(-XB.WorldData.WorldDim.X/2.0f,
                                              -XB.WorldData.WorldDim.Y/2.0f));
        ShowMessage(Tr("GENERATED_TERRAIN"));
        ButtonResumeOnPressed();
    }

    private void ButtonApplySpheresOnPressed() {
        XB.ManagerSphere.ApplyTerrain();
        XB.WorldData.UpdateTerrain(false);
        _player.SpawnPlayer(new Godot.Vector2(_player.GlobalPosition.X,
                                              _player.GlobalPosition.Z));
        ShowMessage(Tr("APPLIED_SPHERES"));
        ButtonResumeOnPressed();
    }

    private void LineEditGenerateSeedOnTextChanged(string text) {
        uint seed = 0;
        if (System.UInt32.TryParse(_leGenSeed.Text, out seed)) {
            ShowMessage(Tr("SEED_VALID"));
        } else {
            ShowMessage(Tr("SEED_INVALID"));
            _leGenSeed.Text = ((uint)System.DateTime.Now.GetHashCode()).ToString();
        }
    }
}
} // namespace close 
