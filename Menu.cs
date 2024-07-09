namespace XB { // namespace opegn
using SysCG = System.Collections.Generic;
public enum MenuType {
    None,
    Pause,
    Save,
}
//TODO[ALEX]: controller inputs just as alternative to keyboard, work around that toggle
public partial class Menu : Godot.Control {
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
    [Godot.Export] private Godot.Button _bQuit;

    // system tab
    [Godot.Export] private Godot.TabContainer _tabSys;
    [Godot.Export] private Godot.Button       _bAppDefaults;
    [Godot.Export] private Godot.Button       _bDefaults;
    [Godot.Export] private Godot.Button       _bMinimum;
    [Godot.Export] private Godot.Button       _bMaximum;
    [Godot.Export] private Godot.Button       _bApply;
        // camera
    [Godot.Export] private Godot.Label  _lbCamHor;
    [Godot.Export] private Godot.Slider _slCamHor;
    [Godot.Export] private Godot.Label  _lbCamVer;
    [Godot.Export] private Godot.Slider _slCamVer;
    [Godot.Export] private Godot.Label  _lbFov;
    [Godot.Export] private Godot.Slider _slFov;
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
    [Godot.Export] private Godot.TabContainer _tabCtrl;
    [Godot.Export] private Godot.Button[]     _bCK          = new Godot.Button[26];
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
        _tabCont.CurrentTab = _tPau;
        _tabSys.CurrentTab  = _tSysCam;
        _tabCtrl.CurrentTab = _tConKey;
        _tabPrev            = _tabCont.CurrentTab;
        _crMsg.Visible      = false;
        _bResume.Pressed   += ButtonResumeOnPressed;

        // pause tab
        _bQuit.Pressed += ButtonPopupQuitOnPressed;

        // system tab
        _bAppDefaults.Pressed += ButtonAppDefaultsOnPressed;
        _bDefaults.Pressed    += ButtonDefaultsOnPressed;
        _bMinimum.Pressed     += ButtonMinimumOnPressed;
        _bMaximum.Pressed     += ButtonMaximumOnPressed;
        _bApply.Pressed       += ButtonApplyOnPressed;
        // camera
        _slFov.MinValue      = XB.AData.FovMin;
        _slFov.MaxValue      = XB.AData.FovMax;
        _slFov.DragEnded    += SliderFovOnDragEnded;
        _slCamHor.DragEnded += SliderCamHorOnDragEnded;
        _slCamVer.DragEnded += SliderCamVerOnDragEnded;
        // display
        foreach (var resolution in XB.AData.Resolutions) { _obRes.AddItem(resolution.Key); }
        XB.Settings.AddSeparators(_obRes);
        foreach (var windowMode in XB.AData.WindowModes) { _obMode.AddItem(windowMode); }
        XB.Settings.AddSeparators(_obMode);
        _cbFps.Pressed     += ButtonShowFPSOnPressed;
        _cbVSync.Pressed   += ButtonVSyncOnPressed;
        _slFrame.DragEnded += SliderFrameRateOnDragEnded;
        // performance
        _scrPerf.ScrollVertical = 0;
        _cbTAA.Pressed         += ButtonTAAOnPressed;
        _cbDebanding.Pressed   += ButtonDebandingOnPressed;
        _cbSSAOHalf.Pressed    += ButtonSSAOHalfOnPressed;
        _cbSSILHalf.Pressed    += ButtonSSILHalfOnPressed;
        _cbSSR.Pressed         += ButtonSSROnPressed;
        foreach (var option in XB.AData.MSAA) { _obMSAA.AddItem(option); }
        XB.Settings.AddSeparators(_obMSAA);
        foreach (var option in XB.AData.SSAA) { _obSSAA.AddItem(option); }
        XB.Settings.AddSeparators(_obSSAA);
        foreach (var option in XB.AData.ShadowFilters) { _obShdwFilter.AddItem(option); }
        XB.Settings.AddSeparators(_obShdwFilter);
        foreach (var option in XB.AData.SSAO) { _obSSAO.AddItem(option); }
        XB.Settings.AddSeparators(_obSSAO);
        foreach (var option in XB.AData.SSIL) { _obSSIL.AddItem(option); }
        XB.Settings.AddSeparators(_obSSIL);
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
        _bCK[24].Pressed     += ButtonCK24OnPressed;
        _bCK[25].Pressed     += ButtonCK25OnPressed;
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

                _hud.UpdateInteractKey();
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
            _crMsg.Color = XB.WorldData.MsgColor.Lerp(XB.WorldData.MsgFadeColor, _t/_msgDur);
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
                    _bResume.GrabFocus();
                    UpdatePauseTab();
                    break;
                }
                case _tSys: { // system
                    _bApply.GrabFocus();
                    UpdateSystemTabContainer();
                    UpdateSettingsTab();
                    break;
                }
                case _tCon: { // controls
                    UpdateControlTabContainer();
                    UpdateControlTab();
                    _bResume.GrabFocus();
                    break;
                }
            }
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
    }

    private void UpdatePauseTab() {
        Godot.GD.Print("UpdatePauseTab currently does nothing.");
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
            default:                                                                break;
        }
    }

    private void UpdateControlTabContainer() {
        switch (_tabCtrl.CurrentTab) {
            case _tConKey: _lbTab.Text = Tr("TAB_CONTROLS") + " | " + Tr("TAB_KEYBOARD");   break;
            case _tConCon: _lbTab.Text = Tr("TAB_CONTROLS") + " | " + Tr("TAB_CONTROLLER"); break;
        }
    }

    private void UpdateControlTab() {
        for (int i = 0; i < _bCK.Length; i++) {
            _bCK[i].Text = Tr(XB.AData.Input.InputActions[i].Description) + " - " + 
                           XB.AData.Input.InputActions[i].Key;
        }
    }

    private void UpdateSettingsTab() {
        XB.Settings.UpdateSettingsTabs(_slCamHor, _lbCamHor, _slCamVer, _lbCamVer, _slFov, _lbFov, 
                                       _slFrame, _lbFrame,  _obRes, _obMode, _cbFps,
                                       _cbVSync, 
                                       _obMSAA, _obSSAA, _cbTAA, _cbDebanding,
                                       _lbShdwSize, _slShdwSize, _obShdwFilter,
                                       _lbShdwDist, _slShdwDist,
                                       _lbLOD, _slLOD, _obSSAO, _cbSSAOHalf, _obSSIL, _cbSSILHalf,
                                       _cbSSR,
                                       _slVolume, _lbVolume, _obLanguage);
    }

    private void UpdateSettingsSliders() {
        XB.Settings.UpdateSliders(_slFrame, _lbFrame, _slShdwSize, _lbShdwSize, _slLOD, _lbLOD);
    }

    private void ShowMessage(string msg) {
        _crMsg.Show();
        _lbMsg.Text  = msg;
        _t           = 0.0f;
        _crMsg.Color = XB.WorldData.MsgColor;
    }

    public void ButtonResumeOnPressed() {
        XB.Utils.PlayUISound(XB.ScenePaths.ButtonAudio);
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
        UpdateSettingsTab();
        XB.PersistData.UpdateScreen();
    }

    public void SliderFovOnDragEnded(bool valueChanged) {
        if (!valueChanged) return;
        ShowMessage(XB.Settings.ChangeFov(_slFov));
        UpdateSettingsTab();
    }

    public void SliderCamHorOnDragEnded(bool valueChanged) {
        if (!valueChanged) return;
        ShowMessage(XB.Settings.ChangeSensitivityHorizontal(_slCamHor));
        UpdateSettingsTab();
    }

    public void SliderCamVerOnDragEnded(bool valueChanged) {
        if (!valueChanged) return;
        ShowMessage(XB.Settings.ChangeSensitivityVertical(_slCamVer));
        UpdateSettingsTab();
    }

    public void SliderVolumeOnDragEnded(bool valueChanged) {
        if (!valueChanged) return;
        ShowMessage(XB.Settings.ChangeVolume(_slVolume));
        UpdateSettingsTab();
    }

    public void SliderFrameRateOnDragEnded(bool valueChanged) {
        UpdateSettingsSliders();
    }

    public void SliderShadowSizeOnDragEnded(bool valueChanged) {
        UpdateSettingsSliders();
    }

    public void SliderLODOnDragEnded(bool valueChanged) {
        UpdateSettingsSliders();
    }

    public void ButtonAppDefaultsOnPressed() {
        XB.Utils.PlayUISound(XB.ScenePaths.ButtonAudio);
        ShowMessage(XB.Settings.ApplicationDefaults());
        XB.PersistData.UpdateScreen();
        UpdateSettingsTab();
    }

    public void ButtonDefaultsOnPressed() {
        SettingsPresetShared(XB.SettingsPreset.Default);
    }

    public void ButtonMinimumOnPressed() {
        SettingsPresetShared(XB.SettingsPreset.Minimum);
    }

    public void ButtonMaximumOnPressed() {
        SettingsPresetShared(XB.SettingsPreset.Maximum);
    }

    private void SettingsPresetShared(XB.SettingsPreset preset) {
        XB.Utils.PlayUISound(XB.ScenePaths.ButtonAudio);
        ShowMessage(XB.Settings.PresetSettings(preset));
        XB.PersistData.UpdateScreen();
        UpdateSettingsTab();
    }

    public void ButtonApplyOnPressed () {
        XB.Utils.PlayUISound(XB.ScenePaths.ButtonAudio);
        ShowMessage(XB.Settings.ApplySettings(_slFrame, _lbFrame, _slShdwSize, _lbShdwSize,
                                              _slShdwDist, _lbShdwDist, _slLOD, _lbLOD, _obRes, _obMode,
                                              _obMSAA,_obSSAA, _obShdwFilter, _obSSAO, _obSSIL));
        XB.PersistData.UpdateScreen();
    }

    public void ButtonTAAOnPressed() {
        ShowMessage(XB.Settings.ToggleTAA());
        UpdateSettingsTab();
    }

    public void ButtonDebandingOnPressed() {
        ShowMessage(XB.Settings.ToggleDebanding());
        UpdateSettingsTab();
    }

    public void ButtonSSAOHalfOnPressed() {
        ShowMessage(XB.Settings.ToggleSSAOHalf());
        UpdateSettingsTab();
    }

    public void ButtonSSILHalfOnPressed() {
        ShowMessage(XB.Settings.ToggleSSILHalf());
        UpdateSettingsTab();
    }

    public void ButtonSSROnPressed() {
        ShowMessage(XB.Settings.ToggleSSR());
        UpdateSettingsTab();
    }

    public void ButtonVSyncOnPressed() {
        ShowMessage(XB.Settings.ToggleVSync());
        UpdateSettingsTab();
    }

    public void ButtonShowFPSOnPressed() {
        ShowMessage(XB.Settings.ToggleShowFPS());
        XB.PersistData.UpdateScreen();
        UpdateSettingsTab();
    }

    public void ButtonPopupCancelOnPressed() {
        XB.Utils.PlayUISound(XB.ScenePaths.ButtonAudio);
        _ctrlPopup.Hide();
    }

    public void ButtonPopupQuitOnPressed() {
        XB.Utils.PlayUISound(XB.ScenePaths.ButtonAudio);
        _ctrlPopup.Show();
        _lbPopup.Text = Tr("QUIT_QUESTION");
    }

    public void ButtonDefaultsCKOnPressed() {
        XB.Utils.PlayUISound(XB.ScenePaths.ButtonAudio);
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
    public void ButtonCK24OnPressed() {ControlsChange(24);}
    public void ButtonCK25OnPressed() {ControlsChange(25);}

    private void ControlsChange(int id) {
        XB.Utils.PlayUISound(XB.ScenePaths.ButtonAudio);
        _setKeyID             = id;
        _setKey               = true;
        _lockInput            = true;
        _mouseRelease         = false;
        _chngMsg.Visible      = true;
        Godot.Input.MouseMode = Godot.Input.MouseModeEnum.Captured;
    }

    private void OptionButtonLanguageOnItemSelected(long id) {
        ShowMessage(XB.Settings.ChangeLanguage(_obLanguage));
        UpdateSystemTabContainer();
        UpdateSettingsTab();
        UpdateTabNames();
    }
}
} // namespace close 
