namespace XB { // namespace opegn
using SysCG = System.Collections.Generic;
public enum MenuType {
    None,
    Pause,
    Save,
}
//TODO[ALEX]: controller inputs just as alternative to keyboard, work around that toggle
public partial class Menu : Godot.Control {
    // [Godot.Export] private XB.PController     _player;
    // [Godot.Export] private XB.PBot            _bot;
    // [Godot.Export] private XB.HUD             _hud;
    // [Godot.Export] private Godot.Label        _lbTab;
    // [Godot.Export] private Godot.Label        _lbMsg;
    // [Godot.Export] private Godot.ColorRect    _crMsg;
    // [Godot.Export] private Godot.TabContainer _tabCont;
    // [Godot.Export] private Godot.TabContainer _tabContSave;
    //                private int                _tabPrev;
    //                private int                _tabSavePrev;
    //                private XB.MenuType        _menuType;
    //                private bool               _justOpened = true;
    //                private int                _scroll     = 100;  // amount to scroll (multiplier)
    //                private float              _t          = 0.0f;
    //                private float              _msgDur     = 2.0f; // in seconds
    // [Godot.Export] private Godot.Button       _bResume;
    // [Godot.Export] private Godot.Button       _bSaveResume;
    //
    // // pause tab
    // [Godot.Export] private Godot.Button       _bSave;
    // [Godot.Export] private Godot.Button       _bQuit;
    // [Godot.Export] private Godot.Label        _lbFrags;
    // [Godot.Export] private Godot.Label        _lbHealth;
    // [Godot.Export] private Godot.Label        _lbHeals;
    // [Godot.Export] private Godot.Label        _lbPU0;
    // [Godot.Export] private Godot.Label        _lbPU1;
    // [Godot.Export] private Godot.Label        _lbPU2;
    // [Godot.Export] private Godot.OptionButton _obBS0;
    // [Godot.Export] private Godot.OptionButton _obBS1;
    // [Godot.Export] private Godot.OptionButton _obBS2;
    //
    // // inventory tab
    // [Godot.Export] private Godot.ItemList    _iItem;
    // [Godot.Export] private Godot.Label       _lbIDesc;
    // [Godot.Export] private Godot.Label       _lbIAmount;
    // [Godot.Export] private Godot.Button      _bUse;
    // [Godot.Export] private Godot.Control     _ctrlUnlim;
    // [Godot.Export] private Godot.TextureRect _trItemIcon;
    //
    // // archive tab
    // [Godot.Export] private Godot.ItemList        _iArchive;
    // [Godot.Export] private Godot.TextureRect     _tArchive;
    // [Godot.Export] private Godot.Label           _lbAName;
    // [Godot.Export] private Godot.ScrollContainer _scrADesc;
    // [Godot.Export] private Godot.Label           _lbADesc;
    //
    // // system tab
    // [Godot.Export] private Godot.TabContainer _tabSys;
    // [Godot.Export] private Godot.Button       _bDefaults;
    // [Godot.Export] private Godot.Button       _bApply;
    //     // camera
    // [Godot.Export] private Godot.Label        _lbCamHor;
    // [Godot.Export] private Godot.Slider       _slCamHor;
    // [Godot.Export] private Godot.Label        _lbCamVer;
    // [Godot.Export] private Godot.Slider       _slCamVer;
    // [Godot.Export] private Godot.Label        _lbFov;
    // [Godot.Export] private Godot.Slider       _slFov;
    // [Godot.Export] private Godot.Button       _cbCross;
    // [Godot.Export] private Godot.Button       _cbGProm;
    // [Godot.Export] private Godot.Button       _cbProm;
    // [Godot.Export] private Godot.Button       _cbBlood;
    // [Godot.Export] private Godot.Button       _cbSave;
    //     // display
    // [Godot.Export] private Godot.OptionButton _obRes;
    // [Godot.Export] private Godot.OptionButton _obMode;
    // [Godot.Export] private Godot.Label        _lbFrame;
    // [Godot.Export] private Godot.Slider       _slFrame;
    // [Godot.Export] private Godot.Button       _cbFps;
    // [Godot.Export] private Godot.Button       _cbVSync;
    //     // performance
    // [Godot.Export] private Godot.ScrollContainer _scrPerf;
    // [Godot.Export] private Godot.OptionButton _obMSAA;
    // [Godot.Export] private Godot.OptionButton _obSSAA;
    // [Godot.Export] private Godot.Button       _cbTAA;
    // [Godot.Export] private Godot.Button       _cbDebanding;
    // [Godot.Export] private Godot.Label        _lbShdwSize;
    // [Godot.Export] private Godot.Slider       _slShdwSize;
    // [Godot.Export] private Godot.OptionButton _obShdwFilter;
    // [Godot.Export] private Godot.Label        _lbShdwDist;
    // [Godot.Export] private Godot.Slider       _slShdwDist;
    // [Godot.Export] private Godot.Button       _cbVoxel;
    // [Godot.Export] private Godot.Button       _cbVoxelHQ;
    // [Godot.Export] private Godot.Button       _cbVoxelHQHalf;
    // [Godot.Export] private Godot.Label        _lbLOD;
    // [Godot.Export] private Godot.Slider       _slLOD;
    // [Godot.Export] private Godot.OptionButton _obSSAO;
    // [Godot.Export] private Godot.Button       _cbSSAOHalf;
    // [Godot.Export] private Godot.OptionButton _obSSIL;
    // [Godot.Export] private Godot.Button       _cbSSILHalf;
    // [Godot.Export] private Godot.Button       _cbSSR;
    //     // audio
    // [Godot.Export] private Godot.Label        _lbVolume;
    // [Godot.Export] private Godot.Slider       _slVolume;
    // [Godot.Export] private Godot.OptionButton _obAudioMd;
    //     // language
    // [Godot.Export] private Godot.OptionButton _obLanguage;
    // 
    // // controls tab
    // [Godot.Export] private Godot.TabContainer _tabCtrl;
    // [Godot.Export] private Godot.Button[]     _bCK          = new Godot.Button[26];
    // [Godot.Export] private Godot.Button       _bDefaultsCK;
    // [Godot.Export] private Godot.Control      _chngMsg;
    //                private bool               _setKey       = false;
    //                private bool               _mouseRelease = false;
    //                private int                _setKeyID     = 0;
    //
    // // guide tab
    // [Godot.Export] private Godot.ItemList        _iGuide;
    // [Godot.Export] private Godot.TextureRect     _tGuide;
    // [Godot.Export] private Godot.Label           _lbGName;
    // [Godot.Export] private Godot.ScrollContainer _scrGDesc;
    // [Godot.Export] private Godot.Label           _lbGDesc;
    //
    // // stats tab
    // [Godot.Export] private Godot.Label _lbSSave;
    // [Godot.Export] private Godot.Label _lbSTime;
    // [Godot.Export] private Godot.Label _lbSDeaths;
    // [Godot.Export] private Godot.Label _lbSDestroyed;
    // [Godot.Export] private Godot.Label _lbVer;
    //
    // // save main tab
    // [Godot.Export] private Godot.Button _bSSave;
    // [Godot.Export] private Godot.Button _bSQuit;
    // [Godot.Export] private Godot.Label  _lbSLevel;
    // [Godot.Export] private Godot.Label  _lbSFrags;
    // [Godot.Export] private Godot.Label  _lbSFragsLU;
    // [Godot.Export] private Godot.Label  _lbSHealth;
    // [Godot.Export] private Godot.Label  _lbSHealthLU;
    // [Godot.Export] private Godot.Label  _lbSDmg;
    // [Godot.Export] private Godot.Label  _lbSDmgLU;
    // [Godot.Export] private Godot.Button _bSLevelUp;
    //
    // // save shop tab
    // [Godot.Export] private Godot.ItemList _iStore;
    // [Godot.Export] private Godot.Label    _lbStDesc;
    // [Godot.Export] private Godot.Label    _lbStAmount;
    // [Godot.Export] private Godot.Label    _lbStFrags;
    // [Godot.Export] private Godot.Label    _lbStPrice;
    // [Godot.Export] private Godot.Label    _lbStValue;
    // [Godot.Export] private Godot.Button   _bBuy;
    // [Godot.Export] private Godot.Button   _bSell;
    //
    // // save weapon tab
    // [Godot.Export] private Godot.ItemList        _iUpgrade;
    // [Godot.Export] private Godot.Label           _lbUShards;
    // [Godot.Export] private Godot.Label           _lbUFrags;
    // [Godot.Export] private Godot.ColorRect       _crUPrice;
    // [Godot.Export] private Godot.ScrollContainer _scrUDesc;
    // [Godot.Export] private Godot.Label           _lbUDesc;
    // [Godot.Export] private Godot.TextureRect     _trGun;
    // [Godot.Export] private Godot.OptionButton    _obGun;
    // [Godot.Export] private Godot.Label           _lbU0;
    // [Godot.Export] private Godot.Label           _lbU1;
    // [Godot.Export] private Godot.Label           _lbU2;
    // [Godot.Export] private Godot.Label           _lb0;
    // [Godot.Export] private Godot.Label           _lb1;
    // [Godot.Export] private Godot.Label           _lb2;
    // [Godot.Export] private Godot.Button          _bUBuy;
    // [Godot.Export] private Godot.Control         _ctrlUPopup;
    // [Godot.Export] private Godot.Label           _lbUPopup;
    // [Godot.Export] private Godot.Button          _bUPop0;
    // [Godot.Export] private Godot.Button          _bUPop1;
    // [Godot.Export] private Godot.Button          _bUPop2;
    // [Godot.Export] private Godot.Button          _bUPopRem;
    // [Godot.Export] private Godot.Button          _bUPopBack;
    //                private string                _artGunTex  = "res://assets/ui/artGunARender.png";
    //                private string                _corpGunTex = "res://assets/ui/corpGunARender.png";
    //
    //
    // // save transport tab
    // [Godot.Export] private Godot.ItemList        _iLvl;
    // [Godot.Export] private Godot.ItemList        _iTerminals;
    // [Godot.Export] private Godot.Label           _lbLvl;
    // [Godot.Export] private Godot.Label           _lbTerminal;
    // [Godot.Export] private Godot.ScrollContainer _scrLvlDesc;
    // [Godot.Export] private Godot.Label           _lbLvlDesc;
    // [Godot.Export] private Godot.Button          _bTransport;
    //                private XB.Level              _lastLvl     = XB.Level.Uninit;
    //                private XB.Level              _selLvl      = XB.Level.Uninit;
    //                private XB.SaveTerm           _selTerm     = XB.SaveTerm.Uninit;
    //                private int                   _termClicked = -1;
    //                private float                 _tTerm       = 0.0f;
    //                private float                 _tTermWindow = 0.6f;
    // private SysCG.Dictionary<int, XB.Level>      _listLvl     = new SysCG.Dictionary<int, XB.Level>();
    // private SysCG.Dictionary<int, XB.SaveTerm>  _listTerm    = new SysCG.Dictionary<int, XB.SaveTerm>();
    //
    // // confirmation popup
    // [Godot.Export] private Godot.Control _ctrlPopup;
    // [Godot.Export] private Godot.Label   _lbPopup;
    // [Godot.Export] private Godot.Button  _bPopBack;
    // [Godot.Export] private Godot.Button  _bPopRet;
    // [Godot.Export] private Godot.Button  _bPopQuit;
    //
    // public override void _Ready() {
    //     _tabPrev      = _tabCont.CurrentTab;
    //     _tabSavePrev  = _tabContSave.CurrentTab;
    //
    //     _bResume.Pressed     += ButtonResumeOnPressed;
    //     _bSaveResume.Pressed += ButtonSaveResumeOnPressed;
    //
    //     // pause tab
    //     _bSave.Pressed      += ButtonSaveOnPressed;
    //     _bQuit.Pressed      += ButtonPopupQuitOnPressed;
    //     _obBS0.ItemSelected += OptionButtonBotSlot0OnItemSelected;
    //     _obBS1.ItemSelected += OptionButtonBotSlot1OnItemSelected;
    //     _obBS2.ItemSelected += OptionButtonBotSlot2OnItemSelected;
    //
    //     // inventory tab
    //     _bUse.Pressed += ButtonUseOnPressed;
    //
    //     // archive tab
    //
    //     // system tab
    //     _bDefaults.Pressed  += ButtonDefaultsOnPressed;
    //     _bApply.Pressed     += ButtonApplyOnPressed;
    //     // camera
    //     _slFov.MinValue      = XB.AData.FovMin;
    //     _slFov.MaxValue      = XB.AData.FovMax;
    //     _slFov.DragEnded    += SliderFovOnDragEnded;
    //     _slCamHor.DragEnded += SliderCamHorOnDragEnded;
    //     _slCamVer.DragEnded += SliderCamVerOnDragEnded;
    //     _cbCross.Pressed    += ButtonCrosshairsOnPressed;
    //     _cbGProm.Pressed    += ButtonGuidePromptsOnPressed;
    //     _cbProm.Pressed     += ButtonPromptsOnPressed;
    //     _cbBlood.Pressed    += ButtonBloodOnPressed;
    //     _cbSave.Pressed     += ButtonSaveLabelOnPressed;
    //     // display
    //     foreach (var resolution in XB.AData.Resolutions) _obRes.AddItem(resolution.Key);
    //     XB.Settings.AddSeparators(_obRes);
    //     foreach (var windowMode in XB.AData.WindowModes) _obMode.AddItem(windowMode);
    //     XB.Settings.AddSeparators(_obMode);
    //     _cbFps.Pressed     += ButtonShowFPSOnPressed;
    //     _cbVSync.Pressed   += ButtonVSyncOnPressed;
    //     _slFrame.DragEnded += SliderFrameRateOnDragEnded;
    //     // performance
    //     _scrPerf.ScrollVertical = 0;
    //     _cbVoxel.Pressed       += ButtonVoxelGIOnPressed;
    //     _cbVoxelHQ.Pressed     += ButtonVoxelGIHQOnPressed;
    //     _cbVoxelHQHalf.Pressed += ButtonVoxelGIHalfOnPressed;
    //     _cbTAA.Pressed         += ButtonTAAOnPressed;
    //     _cbDebanding.Pressed   += ButtonDebandingOnPressed;
    //     _cbSSAOHalf.Pressed    += ButtonSSAOHalfOnPressed;
    //     _cbSSILHalf.Pressed    += ButtonSSILHalfOnPressed;
    //     _cbSSR.Pressed         += ButtonSSROnPressed;
    //     foreach (var option in XB.AData.MSAA) _obMSAA.AddItem(option);
    //     XB.Settings.AddSeparators(_obMSAA);
    //     foreach (var option in XB.AData.SSAA) _obSSAA.AddItem(option);
    //     XB.Settings.AddSeparators(_obSSAA);
    //     foreach (var option in XB.AData.ShadowFilters) _obShdwFilter.AddItem(option);
    //     XB.Settings.AddSeparators(_obShdwFilter);
    //     foreach (var option in XB.AData.SSAO) _obSSAO.AddItem(option);
    //     XB.Settings.AddSeparators(_obSSAO);
    //     foreach (var option in XB.AData.SSIL) _obSSIL.AddItem(option);
    //     XB.Settings.AddSeparators(_obSSIL);
    //     _slShdwSize.DragEnded += SliderShadowSizeOnDragEnded;
    //     _slShdwDist.DragEnded += SliderShadowDistanceOnDragEnded;
    //     _slLOD.DragEnded      += SliderLODOnDragEnded;
    //     // audio
    //     foreach (var audioMode in XB.AData.AudioModes) _obAudioMd.AddItem(audioMode);
    //     XB.Settings.AddSeparators(_obAudioMd);
    //     _slVolume.DragEnded += SliderVolumeOnDragEnded;
    //     // language
    //     foreach (var language in XB.AData.Languages) _obLanguage.AddItem(language);
    //     XB.Settings.AddSeparators(_obLanguage);
    //     _obLanguage.ItemSelected += OptionButtonLanguageOnItemSelected;
    //
    //     // controls tab
    //     _bCK[ 0].Pressed     += ButtonCK00OnPressed;
    //     _bCK[ 1].Pressed     += ButtonCK01OnPressed;
    //     _bCK[ 2].Pressed     += ButtonCK02OnPressed;
    //     _bCK[ 3].Pressed     += ButtonCK03OnPressed;
    //     _bCK[ 4].Pressed     += ButtonCK04OnPressed;
    //     _bCK[ 5].Pressed     += ButtonCK05OnPressed;
    //     _bCK[ 6].Pressed     += ButtonCK06OnPressed;
    //     _bCK[ 7].Pressed     += ButtonCK07OnPressed;
    //     _bCK[ 8].Pressed     += ButtonCK08OnPressed;
    //     _bCK[ 9].Pressed     += ButtonCK09OnPressed;
    //     _bCK[10].Pressed     += ButtonCK10OnPressed;
    //     _bCK[11].Pressed     += ButtonCK11OnPressed;
    //     _bCK[12].Pressed     += ButtonCK12OnPressed;
    //     _bCK[13].Pressed     += ButtonCK13OnPressed;
    //     _bCK[14].Pressed     += ButtonCK14OnPressed;
    //     _bCK[15].Pressed     += ButtonCK15OnPressed;
    //     _bCK[16].Pressed     += ButtonCK16OnPressed;
    //     _bCK[17].Pressed     += ButtonCK17OnPressed;
    //     _bCK[18].Pressed     += ButtonCK18OnPressed;
    //     _bCK[19].Pressed     += ButtonCK19OnPressed;
    //     _bCK[20].Pressed     += ButtonCK20OnPressed;
    //     _bCK[21].Pressed     += ButtonCK21OnPressed;
    //     _bCK[22].Pressed     += ButtonCK22OnPressed;
    //     _bCK[23].Pressed     += ButtonCK23OnPressed;
    //     _bCK[24].Pressed     += ButtonCK24OnPressed;
    //     _bCK[25].Pressed     += ButtonCK25OnPressed;
    //     _bDefaultsCK.Pressed += ButtonDefaultsCKOnPressed;
    //     _chngMsg.Visible      = false;
    //
    //     // guide tab
    //
    //     // stats tab
    //     _lbVer.Text   = "v" + XB.AData.VersionNumber;
    //
    //     // save main tab
    //     _bSSave.Pressed    += ButtonSaveOnPressed;
    //     _bSQuit.Pressed    += ButtonQuitOnPressed;
    //     _bSLevelUp.Pressed += ButtonLevelUpOnPressed;
    //     
    //     // save store tab
    //     _bBuy.Pressed  += ButtonBuyOnPressed;
    //     _bSell.Pressed += ButtonSellOnPressed;
    //
    //     // save weapon tab
    //     _obGun.ItemSelected += OptionButtonGunOnItemSelected;
    //     _bUBuy.Pressed      += ButtonUpgradeBuyOnPressed;
    //     _bUPop0.Pressed     += ButtonUpgradePopup0OnPressed;
    //     _bUPop1.Pressed     += ButtonUpgradePopup1OnPressed;
    //     _bUPop2.Pressed     += ButtonUpgradePopup2OnPressed;
    //     _bUPopRem.Pressed   += ButtonUpgradePopupRemoveOnPressed;
    //     _bUPopBack.Pressed  += ButtonUpgradePopupBackOnPressed;
    //     _ctrlUPopup.Hide();
    //
    //     // save transport tab
    //     _bTransport.Pressed     += ButtonTransportOnPressed;
    //     _iLvl.ItemClicked       += ItemListLevelsItemClicked;
    //     _iTerminals.ItemClicked += ItemListTerminalsItemClicked;
    //
    //     // confirmation popup
    //     _bPopBack.Pressed += ButtonPopupBackOnPressed;
    //     _bPopQuit.Pressed += ButtonQuitOnPressed;
    //     _ctrlPopup.Hide();
    //
    //     Hide();
    // }
    //
    // // handling input before any other node to reassign controls
    // public override void _Input(Godot.InputEvent @event) {
    //     if (_tabCont.CurrentTab != 4 || !_setKey) return;
    //     if (!XB.AData.Controller) {
    //         if        (!_mouseRelease && @event is Godot.InputEventMouseButton) {
    //             _mouseRelease = true;
    //         } else if (@event is Godot.InputEventKey || @event is Godot.InputEventMouseButton) {
    //             _setKey       = false;
    //             // NOTE[ALEX]: InputEventMouseButton is also triggered on release of the button
    //             _mouseRelease = false;
    //
    //             string key = "";
    //             if (@event is Godot.InputEventKey) {
    //                 string[] keyText = @event.AsText().Split(' ');
    //                 key = keyText[0];
    //             } else if (@event is Godot.InputEventMouseButton) {
    //                 key = @event.AsText();
    //             }
    //
    //             if (@event is Godot.InputEventMouseButton // get rid of double clicks, etc.
    //                     && key != "Left Mouse Button"
    //                     && key != "Middle Mouse Button"
    //                     && key != "Right Mouse Button") {
    //                 ShowMessage(Tr("ILLEGAL_MOUSE_INPUT"));
    //                 _chngMsg.Visible      = false;
    //                 Godot.Input.MouseMode = Godot.Input.MouseModeEnum.Visible;
    //                 return;
    //             }
    //
    //             for (int i = 0; i < XB.Input.Amount; i++) {
    //                 if (XB.AData.Input.InputActions[i].Key == key) {
    //                     ShowMessage(Tr("INPUT_ALREADY_USED"));
    //                     _chngMsg.Visible = false;
    //                     Godot.Input.MouseMode = Godot.Input.MouseModeEnum.Visible;
    //                     return;
    //                 }
    //             }
    //
    //             var iAction       = XB.AData.Input.InputActions[_setKeyID];
    //                 iAction.Event = @event;
    //                 iAction.Key   = key;
    //             Godot.InputMap.ActionEraseEvents(iAction.Name);
    //             Godot.InputMap.ActionAddEvent(iAction.Name, @event);
    //
    //             XB.PersistData.ControlBindingsWrite();
    //             _hud.UpdateInteractKey();
    //             ShowMessage(Tr("KEYBINDINGS_UPDATED") + Tr(iAction.Description) + ".");
    //
    //             _chngMsg.Visible      = false;
    //             Godot.Input.MouseMode = Godot.Input.MouseModeEnum.Visible;
    //             UpdateControlTab();
    //         }
    //     } 
    //     //TODO[ALEX]: implement controller keybinding
    //     //            does this make sense for changing controls or 
    //     //            does it not really matter and I do it by showing the available buttons 
    //     //            and mapping actions to them
    // }
    //
    // public override void _PhysicsProcess(double delta) {
    //     if (XB.DeathScreen.PlayerDead) return;
    //
    //     if (_justOpened) {
    //         _justOpened = false;
    //         return;
    //     }
    //
    //     //NOTE[ALEX]: if I add to the global time, then enemy sight can get cheesed
    //     XB.PersistData.PlayTime += delta;
    //
    //     _t     += (float)delta;
    //     _tTerm += (float)delta;
    //
    //     if (_t >= _msgDur) {
    //         _t          = 0.0f;
    //         _lbMsg.Text = "";
    //         _crMsg.Hide();
    //     } else {
    //         _crMsg.Color = XB.WorldData.MsgColor.Lerp(XB.WorldData.MsgFadeColor, _t/_msgDur);
    //     }
    //
    //     // player input
    //     if        (XB.AData.Input.Start) {
    //         XB.AData.Input.ConsumeInputStart();
    //         if (_menuType == XB.MenuType.Pause) ButtonResumeOnPressed();
    //         if (_menuType == XB.MenuType.Save)  ButtonSaveResumeOnPressed();
    //     } else if (XB.AData.Input.CamY > 0.0f) {
    //         _scrADesc.ScrollVertical -= XB.Utils.MaxI(1, (int)(delta*_scroll));
    //         _scrGDesc.ScrollVertical -= XB.Utils.MaxI(1, (int)(delta*_scroll));
    //     } else if (XB.AData.Input.CamY < 0.0f) {
    //         _scrADesc.ScrollVertical += XB.Utils.MaxI(1, (int)(delta*_scroll));
    //         _scrGDesc.ScrollVertical += XB.Utils.MaxI(1, (int)(delta*_scroll));
    //     } else {
    //         if (XB.AData.Controller) {
    //             int tabCount = _tabCont.GetTabCount();
    //             if        (XB.AData.Input.SRTop || XB.AData.Input.SRBot) {
    //                 if (_tabCont.CurrentTab < tabCount-1) _tabCont.CurrentTab += 1;
    //                 else                                  _tabCont.CurrentTab  = 0;
    //             } else if (XB.AData.Input.SLTop || XB.AData.Input.SLBot) {
    //                 if (_tabCont.CurrentTab > 0)         _tabCont.CurrentTab -= 1;
    //                 else                                 _tabCont.CurrentTab  = tabCount-1;
    //             }
    //         }
    //     }
    //     //TODO[ALEX]: deal with full controller control (subtabs, etc)
    //
    //     // change tabs
    //     switch (_menuType) {
    //         case XB.MenuType.Pause:
    //             if (_tabCont.CurrentTab == _tabPrev) break;
    //             switch (_tabCont.CurrentTab) {
    //                 case 0: { // pause
    //                     _lbTab.Text = Tr("TAB_PAUSE");
    //                     _bResume.GrabFocus();
    //                     UpdatePauseTab();
    //                     break;
    //                 }
    //                 case 1: { // inventory
    //                     UpdateItemTabContainer(0);
    //                     _lbTab.Text = Tr("TAB_INVENTORY");
    //                     break;
    //                 }
    //                 case 2: { //archive
    //                     _iArchive.Clear();
    //                     foreach (int i in System.Enum.GetValues(typeof(XB.Archive))) {
    //                         if (XB.Entries.ArchiveEntries[(XB.Archive)i].Discovered) {
    //                             string name = Tr(XB.Entries.ArchiveEntries[(XB.Archive)i].Name);
    //                             _iArchive.AddItem(name);
    //                         }
    //                     }
    //                     _lbTab.Text = Tr("TAB_ARCHIVE");
    //                     if (_iArchive.ItemCount > 0) {
    //                         _iArchive.Select(0);
    //                         _iArchive.GrabFocus();
    //                     }
    //                     break;
    //                 }
    //                 case 3: { // system
    //                     _bApply.GrabFocus();
    //                     UpdateSystemTabContainer();
    //                     UpdateSettingsTab();
    //                     break;
    //                 }
    //                 case 4: { // controls
    //                     UpdateControlTabContainer();
    //                     UpdateControlTab();
    //                     _bResume.GrabFocus();
    //                     break;
    //                 }
    //                 case 5: { // guide
    //                     UpdateGuideContainer(0);
    //                     _lbTab.Text = Tr("TAB_GUIDE");
    //                     break;
    //                 } 
    //                 case 6: { // stats
    //                     _lbTab.Text = Tr("TAB_STATS");
    //                     _bResume.GrabFocus();
    //                     break;
    //                 }
    //             }
    //             _tabPrev = _tabCont.CurrentTab;
    //             break;
    //         case XB.MenuType.Save:
    //             if (_tabContSave.CurrentTab == _tabSavePrev) break;
    //             switch (_tabContSave.CurrentTab) {
    //                 case 0: {
    //                     _lbTab.Text = Tr("TAB_SAVE_TERMINAL");
    //                     _bSaveResume.GrabFocus();
    //                     UpdateSaveTab();
    //                     break;
    //                 }
    //                 case 1: {
    //                     _lbTab.Text = Tr("TAB_STORE");
    //                     UpdateStoreTab();
    //                     _bSaveResume.GrabFocus();
    //                     break;
    //                 }
    //                 case 2: {
    //                     _lbTab.Text = Tr("TAB_WEAPON");
    //                     UpdateUpgradeTab();
    //                     _bSaveResume.GrabFocus();
    //                     break;
    //                 }
    //                 case 3: {
    //                     _lbTab.Text = Tr("TAB_TRANSPORT");
    //                     UpdateTransportTab();
    //                     if (_iLvl.ItemCount > 0)       _iLvl.Select(0);
    //                     if (_iTerminals.ItemCount > 0) _iTerminals.Select(0);
    //                     _bSaveResume.GrabFocus();
    //                     break;
    //                 }
    //             }
    //             _tabSavePrev = _tabContSave.CurrentTab;
    //             break;
    //     }
    //
    //     // active tab
    //     switch (_menuType) {
    //         case XB.MenuType.Pause: {
    //             switch (_tabCont.CurrentTab) {
    //                 case 0: { // pause
    //                     break;
    //                 }
    //                 case 1: { // inventory
    //                     if (!_iItem.IsAnythingSelected()) {
    //                         _bUse.Disabled = true;
    //                         break; //TODO[ALEX]: does this break out of the outer loop correctly?
    //                     }
    //                     int     selIDI   = _iItem.GetSelectedItems()[0];
    //                     string  selNameI = _iItem.GetItemText(selIDI);
    //                     XB.Item item     = XB.Entries.ItemNames[selNameI];
    //                     _lbIDesc.Text    = Tr(XB.Entries.ItemEntries[item].Description);
    //                     _lbIAmount.Text  = XB.Entries.ItemEntries[item].Amount.ToString() + "/" +
    //                                        XB.Entries.ItemEntries[item].AmountMax.ToString();
    //                     if (XB.Entries.ItemEntries[item].UnlimitedUse) _ctrlUnlim.Show();
    //                     else                                           _ctrlUnlim.Hide();
    //                     Godot.Texture2D icon = Godot.ResourceLoader.Load<Godot.Texture2D>
    //                                                (XB.Entries.ItemEntries[item].IconPath);
    //                     _trItemIcon.Texture  = icon;
    //                     switch (XB.Entries.ItemEntries[item].Type) {
    //                         case XB.ItemType.Uninit: {
    //                             _bUse.Disabled = true;
    //                             _bUse.Text     = "";
    //                             break;
    //                         }
    //                         case XB.ItemType.Outfit: {
    //                             _bUse.Disabled = false;
    //                             _bUse.Text     = Tr("CHANGE_OUTFIT");
    //                             break;
    //                         }
    //                         case XB.ItemType.Consumable: {
    //                             _bUse.Disabled = true;
    //                             _bUse.Text     = Tr("ASSIGN_TO_BOT");
    //                             break;
    //                         }
    //                         case XB.ItemType.ConsumableMenu: {
    //                             if (XB.Entries.ItemEntries[item].Item == XB.Item.CylinderUpgrade &&
    //                                 XB.PersistData.CylAmountMax >= XB.AData.CylUpgradeMax) {
    //                                 _bUse.Disabled = true;
    //                                 _bUse.Text     = Tr("CYLINDER_CAPACITY_REACHED");
    //                                 break;
    //                             }
    //                             _bUse.Disabled = false;
    //                             _bUse.Text     = Tr("USE_ITEM");
    //                             break;
    //                         }
    //                         case XB.ItemType.Key: {
    //                             _bUse.Disabled = true;
    //                             _bUse.Text     = Tr("KEY_ITEM");
    //                             break;
    //                         }
    //                     }
    //                     break;
    //                 }
    //                 case 2: { // archive
    //                     if (!_iArchive.IsAnythingSelected()) break;
    //                     int    selIDA     = _iArchive.GetSelectedItems()[0];
    //                     string selNameA   = _iArchive.GetItemText(selIDA);
    //                     var    entryA     = XB.Entries.ArchiveNames[selNameA];
    //                     _tArchive.Texture = Godot.ResourceLoader.Load<Godot.Texture2D>
    //                         (XB.Entries.ArchiveEntries[entryA].IconPath);
    //                     _lbAName.Text     = selNameA;
    //                     _lbADesc.Text     = Tr(XB.Entries.ArchiveEntries[entryA].Description);
    //                     break;
    //                 }
    //                 case 3: { // system
    //                     UpdateSystemTabContainer();
    //                     //NOTE[ALEX]: the dragEnded slider signal does not get emitted when
    //                     //            clicking a value on the slider (not dragging)
    //                     if (_slFov.Value != XB.AData.FovDef) {
    //                         SliderFovOnDragEnded(true);
    //                     }
    //                     if ((float)_slCamHor.Value != XB.AData.CamXSens*XB.Settings.CamSliderMult) {
    //                         SliderCamHorOnDragEnded(true);
    //                     }
    //                     if ((float)_slCamVer.Value != XB.AData.CamYSens*XB.Settings.CamSliderMult) {
    //                         SliderCamVerOnDragEnded(true);
    //                     }
    //                     if ((float)_slVolume.Value != (float)Godot.AudioServer.GetBusVolumeDb(0)) {
    //                         SliderVolumeOnDragEnded(true);
    //                     }
    //                     if (_slShdwDist.Value != XB.AData.ShadowDistance) {
    //                         SliderShadowDistanceOnDragEnded(true);
    //                     }
    //                     break;
    //                 }
    //                 case 4: { // controls
    //                     UpdateControlTabContainer();
    //                     break;
    //                 }
    //                 case 5: { // guide
    //                     if (!_iGuide.IsAnythingSelected()) break;
    //                     int    selIDG   = _iGuide.GetSelectedItems()[0];
    //                     string selNameG = _iGuide.GetItemText(selIDG);
    //                     var    entryG   = XB.Entries.GuideNames[selNameG];
    //                     _tGuide.Texture = Godot.ResourceLoader.Load<Godot.Texture2D>
    //                         (XB.Entries.GuideEntries[entryG].IconPath);
    //                     _lbGName.Text   = selNameG;
    //                     _lbGDesc.Text   = Tr(XB.Entries.GuideEntries[entryG].Description);
    //                     break;
    //                 }
    //                 case 6: { // stats
    //                     int seconds  = (int)XB.PersistData.PlayTime;
    //                     int hours    = seconds/3600; 
    //                     int minutes  = seconds/60;
    //                         minutes %= 60;
    //                         seconds  = seconds%60;
    //                     string time  = hours.ToString() + ":";
    //                     if (minutes < 10) time += "0";
    //                            time += minutes.ToString() + ":";
    //                     if (seconds < 10) time += "0";
    //                            time += seconds.ToString();
    //                     _lbSTime.Text      = time;
    //                     _lbSDeaths.Text    = XB.PersistData.PlayerDeaths.ToString();
    //                     _lbSDestroyed.Text = XB.PersistData.EnemiesKilled.ToString();
    //                     _lbSSave.Text      = XB.AData.SaveID;
    //                     break;
    //                 }
    //             }
    //             break;
    //         }
    //         case XB.MenuType.Save: {
    //             switch (_tabContSave.CurrentTab) {
    //                 case 0: { // save main
    //                     break;
    //                 }
    //                 case 1: { // save store
    //                     if (!_iStore.IsAnythingSelected()) {
    //                         _bBuy.Disabled = true;
    //                         break;
    //                     }
    //                     int     selIDS   = _iStore.GetSelectedItems()[0];
    //                     string  selNameS = _iStore.GetItemText(selIDS);
    //                     XB.Item itemS    = XB.Entries.ItemNames[selNameS];
    //                     var     iS       = XB.Entries.ItemEntries[itemS];
    //                     _lbStDesc.Text   = Tr(iS.Description);
    //                     _lbStFrags.Text  = Tr("FRAGMENTS") + ": " + 
    //                                        XB.PersistData.Fragments.ToString();
    //                     _lbStAmount.Text = iS.Amount.ToString() + "/" + iS.AmountMax.ToString() +
    //                                        " " + Tr("IN_INVENTORY");
    //                     _lbStPrice.Text  = iS.Price.ToString() + " " + Tr("FRAGMENTS_TO_BUY");
    //                     _lbStValue.Text  = (iS.Price/2).ToString() + " " + Tr("FRAGMENTS_TO_SELL");
    //                     if       (iS.StoreReq == XB.State.Uninit) {
    //                         _bBuy.Disabled  = true;
    //                         _bBuy.Text      = Tr("CANNOT_BUY");
    //                         _lbStPrice.Text = "";
    //                     } else if (XB.PersistData.Fragments < iS.Price) {
    //                         _bBuy.Disabled  = true;
    //                         _bBuy.Text      = Tr("INSUFFICIENT_FRAGMENTS");
    //                     } else if (iS.Amount >= iS.AmountMax) {
    //                         _bBuy.Disabled  = true;
    //                         _bBuy.Text      = Tr("REACHED_CAPACITY");
    //                     } else {
    //                         _bBuy.Disabled  = false;
    //                         _bBuy.Text      = Tr("BUY_ITEM");
    //                     }
    //                     if (iS.Amount > 0) {
    //                         _bSell.Disabled = false;
    //                         _bSell.Text     = Tr("SELL_ITEM");
    //                     } else {
    //                         _bSell.Disabled = true;
    //                         _bSell.Text     = Tr("INSUFFICIENT_AMOUNT");
    //                     }
    //                     if (iS.Item == XB.PersistData.Outfit) {
    //                         _bSell.Disabled = true;
    //                         _bSell.Text     = Tr("CANNOT_SELL_OUTFIT");
    //                     }
    //                     break;
    //                 }
    //                 case 2: { // save weapon
    //                     if (XB.PersistData.ImpactWpn.GunName == XB.WName.CorpImpact) {
    //                         _lbUDesc.Text   = "";
    //                         _lbUShards.Text = "";
    //                         _lbUFrags.Text  = "";
    //                         _crUPrice.Hide();
    //                         _bUBuy.Text     = Tr("NO_UPGRADE");
    //                         _bUBuy.Disabled = true;
    //                         _lbU0.Text      = "";
    //                         _lbU1.Text      = "";
    //                         _lbU2.Text      = "";
    //                         _lb0.Text       = "";
    //                         _lb1.Text       = "";
    //                         _lb2.Text       = "";
    //                     } else {
    //                         int        selIDU   = _iUpgrade.GetSelectedItems()[0];
    //                         string     selNameU = _iUpgrade.GetItemText(selIDU);
    //                         XB.Upgrade upg      = XB.WUData.UpgradeNames[selNameU];
    //                         var        u        = XB.WUData.Upgrades[upg];
    //                         _lbUDesc.Text   = Tr(u.Description);
    //                         _lbUShards.Text = u.PrShards.ToString();
    //                         _lbUFrags.Text  = u.PrFragments.ToString();
    //                         if (XB.WUData.Upgrades[upg].Bought) {
    //                             _crUPrice.Hide();
    //                             _bUBuy.Text     = Tr("EQUIP_UPGRADE");
    //                             _bUBuy.Disabled = false;
    //                         } else {
    //                             _crUPrice.Show();
    //                             _bUBuy.Text = Tr("BUY_UPGRADE");
    //                             if (XB.PersistData.Fragments < u.PrFragments ||
    //                                 XB.Entries.ItemEntries[XB.Item.ArtefactShard].Amount < u.PrShards) {
    //                                 _bUBuy.Disabled = true;
    //                             } else {
    //                                 _bUBuy.Disabled = false;
    //                             }
    //                         }
    //                     }
    //                     break;
    //                 }
    //                 case 3: { // transport
    //                     _lbLvl.Text          = "";
    //                     _lbTerminal.Text     = "";
    //                     _bTransport.Disabled = true;
    //                     _bTransport.Text     = "BTN_NO_TERMINAL_SELECTED";
    //                     //TODO[ALEX]: transportation tab has issues
    //
    //                     // if (!_iLvl.IsAnythingSelected()) break;
    //                     // int selIDLvl    = _iLvl.GetSelectedItems()[0];
    //                     // var lv          = _listLvl[selIDLvl];
    //                     // _lbLvl.Text     = Tr(XB.Entries.LevelEntries[lv].LevelName);
    //                     // _lbLvlDesc.Text = Tr(XB.Entries.LevelEntries[lv].Description);
    //                     //
    //                     // // if (lv != XB.Level.Uninit && lv != _lastLvl)
    //                     // //     UpdateTransportTabTerminals(_listLvl[selIDLvl]);
    //                     // // _lastLvl = lv;
    //                     //
    //                     // if (!_iTerminals.IsAnythingSelected() && _iTerminals.ItemCount > 0) {
    //                     //     _iTerminals.Select(0);
    //                     //     int selIDTerm = _iTerminals.GetSelectedItems()[0];
    //                     //     var tm        = _listTerm[selIDTerm];
    //                     //     for (int i = 0; i < XB.Entries.LevelEntries[lv].LevelTerms.Length; i++) {
    //                     //         if (tm != XB.Entries.LevelEntries[lv].LevelTerms[i]) continue;
    //                     //         _lbTerminal.Text = XB.Entries.LevelEntries[lv].TermNames[i];
    //                     //     }
    //                     //     _bTransport.Disabled = false;
    //                     //     _bTransport.Text     = "BTN_TRANSPORT_TO_AREA";
    //                     //     _selLvl              = lv;
    //                     //     _selTerm             = tm;
    //                     // }
    //                     break;
    //                 }
    //             }
    //             break;
    //         }
    //     }
    // }
    //
    // public void OpenMenu() {
    //     _menuType   = XB.MenuType.Pause;
    //     _lbTab.Text = Tr("TAB_PAUSE");
    //     _bResume.Show();
    //     _bSaveResume.Hide();
    //     _bResume.GrabFocus();
    //     _tabCont.Show();
    //     _tabContSave.Hide();
    //     _tabCont.CurrentTab = 0;
    //     OpenShared();
    //     UpdatePauseTab();
    //     UpdateTabNames();
    // }
    //
    // public void OpenSaveMenu() {
    //     _menuType   = XB.MenuType.Save;
    //     _lbTab.Text = Tr("TAB_SAVE_TERMINAL");
    //     _bResume.Hide();
    //     _bSaveResume.Show();
    //     _bSaveResume.GrabFocus();
    //     _tabCont.Hide();
    //     _tabContSave.Show();
    //     _tabContSave.CurrentTab = 0;
    //     OpenShared();
    //     UpdateSaveTab();
    // }
    //
    // private void OpenShared() {
    //     _justOpened           = true;
    //     Godot.Input.MouseMode = Godot.Input.MouseModeEnum.Visible;
    //     GetTree().Paused      = true;
    //     Show();
    //
    //     _setKey       = false;
    //     _mouseRelease = false;
    //
    //     _lbMsg.Text = "";
    //     _crMsg.Hide();
    // }
    //
    // private void UpdatePauseTab() {
    //     _lbFrags.Text  = Tr("FRAGMENTS") + ": " + XB.PersistData.Fragments.ToString();
    //     _lbHealth.Text = Tr("HEALTH")    + ": " + XB.PersistData.PlHealth.ToString() + " / "
    //                                             + XB.PersistData.PlHealthMax.ToString();
    //     _lbHeals.Text  = Tr("HEALS")     + ": " + XB.PersistData.CylAmount.ToString() + " / "
    //                                             + XB.PersistData.CylAmountMax.ToString();
    //     _lbPU0.Text = "0.. " + Tr(XB.WUData.Upgrades[XB.PersistData.Upgrades[0]].Name);
    //     _lbPU1.Text = "1.. " + Tr(XB.WUData.Upgrades[XB.PersistData.Upgrades[1]].Name);
    //     _lbPU2.Text = "2.. " + Tr(XB.WUData.Upgrades[XB.PersistData.Upgrades[2]].Name);
    //     _obBS0.Clear();
    //     _obBS1.Clear();
    //     _obBS2.Clear();
    //     _obBS0.AddItem(Tr("NONE"));
    //     _obBS1.AddItem(Tr("NONE"));
    //     _obBS2.AddItem(Tr("NONE"));
    //     foreach (int i in System.Enum.GetValues(typeof(XB.Item))) {
    //         var item = XB.Entries.ItemEntries[(XB.Item)i];
    //         if (item.Amount > 0 && item.Type == XB.ItemType.Consumable) {
    //             _obBS0.AddItem(Tr(item.Name));
    //             _obBS1.AddItem(Tr(item.Name));
    //             _obBS2.AddItem(Tr(item.Name));
    //         }
    //     }
    //     XB.Settings.AddSeparators(_obBS0);
    //     XB.Settings.AddSeparators(_obBS1);
    //     XB.Settings.AddSeparators(_obBS2);
    //     for (int i = 0; i < _obBS0.ItemCount; i++) {
    //         if (_obBS0.GetItemText(i) == Tr(XB.Entries.ItemEntries[XB.PersistData.BotSlot[0]].Name))
    //             _obBS0.Select(i);
    //         if (_obBS1.GetItemText(i) == Tr(XB.Entries.ItemEntries[XB.PersistData.BotSlot[1]].Name))
    //             _obBS1.Select(i);
    //         if (_obBS2.GetItemText(i) == Tr(XB.Entries.ItemEntries[XB.PersistData.BotSlot[2]].Name))
    //             _obBS2.Select(i);
    //     }
    // }
    //
    // private void UpdateSaveTab() {
    //     XB.PersistData.CalculateNextLevelUp();
    //     string[] dmg   = (100.0f*XB.PersistData.PlayerMult).ToString().Split('.');
    //     string[] dmgL  = (100.0f*XB.PersistData.LvlUpDamage).ToString().Split('.');
    //     string   dmgT  = dmg[0]  + ".";
    //     string   dmgLT = dmgL[0] + ".";
    //     if (dmg.Length > 1)  dmgT  += dmg[1].Remove(2);
    //     else                 dmgT  += "00";
    //     if (dmgL.Length > 1) dmgLT += dmgL[1].Remove(2);
    //     else                 dmgLT += "00";
    //     _lbSDmg.Text      =       dmgT  + "%";
    //     _lbSDmgLU.Text    = "+" + dmgLT + "%";
    //     _lbSLevel.Text    =       XB.PersistData.PlayerLevel.ToString();
    //     _lbSFrags.Text    =       XB.PersistData.Fragments.ToString();
    //     _lbSFragsLU.Text  =       XB.PersistData.LvlUpCost.ToString();
    //     _lbSHealth.Text   =       XB.PersistData.PlHealthMaxMod.ToString();
    //     _lbSHealthLU.Text = "+" + XB.PersistData.LvlUpHealth.ToString();
    //     if (XB.PersistData.Fragments < XB.PersistData.LvlUpCost) {
    //         _bSLevelUp.Disabled = true;
    //         _bSLevelUp.Text     = "INSUFFICIENT_FRAGMENTS";
    //     } else {
    //         _bSLevelUp.Disabled = false;
    //         _bSLevelUp.Text     = "LEVELUP";
    //     }
    // }
    //
    // private void UpdateItemTabContainer(int selectedID) {
    //     _iItem.Clear();
    //     int itemCounter = 0;
    //     for (int i = 0; i < XB.Entries.ItemOrder.Length; i++) {
    //         var item = XB.Entries.ItemOrder[i];
    //         if (XB.Entries.ItemEntries[item].Amount > 0) {
    //             _iItem.AddItem(Tr(XB.Entries.ItemEntries[item].Name));
    //             var icon = Godot.ResourceLoader.Load<Godot.Texture2D>
    //                            (XB.Entries.ItemEntries[item].IconPath);
    //             _iItem.SetItemIcon(itemCounter, icon);
    //             itemCounter += 1;
    //         }
    //     }
    //     if (selectedID < _iItem.ItemCount) {
    //         _iItem.Select(selectedID);
    //         _iItem.GrabFocus();
    //     } else { 
    //         _iItem.Select(_iItem.ItemCount-1);
    //         _iItem.GrabFocus();
    //     }
    // }
    //
    // private void UpdateGuideContainer(int selectedID) {
    //     _iGuide.Clear();
    //     int guideCounter = 0;
    //     for (int i = 0; i < XB.Entries.GuideOrder.Length; i++) {
    //         var guide = XB.Entries.GuideOrder[i];
    //         if (XB.Entries.GuideEntries[guide].Discovered) {
    //             _iGuide.AddItem(Tr(XB.Entries.GuideEntries[guide].Name));
    //             guideCounter += 1;
    //         }
    //     }
    //     if (selectedID < _iGuide.ItemCount) {
    //         _iGuide.Select(selectedID);
    //         _iGuide.GrabFocus();
    //     } else { 
    //         _iGuide.Select(_iGuide.ItemCount-1);
    //         _iGuide.GrabFocus();
    //     }
    // }
    //
    // private void UpdateStoreTab() {
    //     _iStore.Clear();
    //     int storeCounter = 0;
    //     foreach (int i in System.Enum.GetValues(typeof(XB.Item))) {
    //         if ((XB.PersistData.States[XB.Entries.ItemEntries[(XB.Item)i].StoreReq] ||
    //              XB.Entries.ItemEntries[(XB.Item)i].Amount > 0) &&
    //              XB.Entries.ItemEntries[(XB.Item)i].Price > 0) {
    //             _iStore.AddItem(Tr(XB.Entries.ItemEntries[(XB.Item)i].Name));
    //             var icon = Godot.ResourceLoader.Load<Godot.Texture2D>
    //                            (XB.Entries.ItemEntries[(XB.Item)i].IconPath);
    //             _iStore.SetItemIcon(storeCounter, icon);
    //             storeCounter += 1;
    //         }
    //     }
    //     if (_iStore.ItemCount > 0) {
    //         _iStore.Select(0);
    //         _iStore.GrabFocus();
    //     }
    // }
    //
    // private void UpdateUpgradeTab() {
    //     _obGun.Clear();
    //     _obGun.AddItem(Tr("CORPORATE"));
    //     _obGun.AddItem(Tr("ARTEFACT"));
    //     XB.Settings.AddSeparators(_obGun);
    //     if (XB.PersistData.ImpactWpn.GunName == XB.WName.CorpImpact) {
    //         _obGun.Select(0);
    //         _trGun.Texture = Godot.ResourceLoader.Load<Godot.Texture2D>(_corpGunTex);
    //     } else {
    //         _obGun.Select(1);
    //         _trGun.Texture = Godot.ResourceLoader.Load<Godot.Texture2D>(_artGunTex);
    //     }
    //     for (int i = 0; i < XB.PersistData.Upgrades.Length; i++) {
    //         UpdateUpgradeSlotLabels(i);
    //     }
    //     _iUpgrade.Clear();
    //     if (XB.PersistData.ImpactWpn.GunName == XB.WName.CorpImpact) return;
    //     foreach (int i in System.Enum.GetValues(typeof(XB.Upgrade))) {
    //         if (XB.PersistData.States[XB.WUData.Upgrades[(XB.Upgrade)i].StoreReq]) {
    //             _iUpgrade.AddItem(Tr(XB.WUData.Upgrades[(XB.Upgrade)i].Name));
    //         }
    //     }
    //     _iUpgrade.Select(0);
    //     _iUpgrade.GrabFocus();
    //     _lb0.Text = "..0";
    //     _lb1.Text = "..1";
    //     _lb2.Text = "..2";
    // }
    //
    // private void UpdateTransportTab() {
    //     _iLvl.Clear();
    //     _listLvl  = new SysCG.Dictionary<int, XB.Level>();
    //     int lvlCounter = 0;
    //     for (int i = 1; i < XB.Entries.LevelEntries.Count; i++) { // skip uninit
    //         int termCounter = 0;
    //         for (int j = 0; j < XB.Entries.LevelEntries[(XB.Level)i].TermsDiscovered.Length; j++) {
    //             if (!XB.Entries.LevelEntries[(XB.Level)i].TermsDiscovered[j]) continue;
    //             termCounter += 1;
    //         }
    //         if (termCounter > 0) {
    //             _listLvl.Add(lvlCounter, (XB.Level)i);
    //             _iLvl.AddItem("");
    //             var icon = Godot.ResourceLoader.Load<Godot.Texture2D>
    //                         (XB.Entries.LevelEntries[(XB.Level)i].LevelIconPath);
    //             _iLvl.SetItemIcon(lvlCounter, icon);
    //             lvlCounter += 1;
    //         }
    //     }
    //     UpdateTransportTabTerminals((XB.Level)0);
    // }
    //
    // private void UpdateTransportTabTerminals(XB.Level lvl) {
    //     _iTerminals.Clear();
    //     _listTerm = new SysCG.Dictionary<int, XB.SaveTerm>();
    //     int termCounter = 0;
    //     for (int i = 0; i < XB.Entries.LevelEntries[lvl].TermsDiscovered.Length; i++) {
    //         if (!XB.Entries.LevelEntries[lvl].TermsDiscovered[i]) continue;
    //         _listTerm.Add(termCounter, XB.Entries.LevelEntries[lvl].LevelTerms[i]);
    //         _iTerminals.AddItem("");
    //         var icon = Godot.ResourceLoader.Load<Godot.Texture2D>
    //                     (XB.Entries.LevelEntries[lvl].TermIconPaths[i]);
    //         _iTerminals.SetItemIcon(termCounter, icon);
    //         termCounter += 1;
    //     }
    // }
    //
    // private void UpdateTabNames() {
    //     _tabCont.SetTabTitle    (0, Tr("TAB_PAUSE"));
    //     _tabCont.SetTabTitle    (1, Tr("TAB_INVENTORY"));
    //     _tabCont.SetTabTitle    (2, Tr("TAB_ARCHIVE"));
    //     _tabCont.SetTabTitle    (3, Tr("TAB_SYSTEM"));
    //     _tabCont.SetTabTitle    (4, Tr("TAB_CONTROLS"));
    //     _tabCont.SetTabTitle    (5, Tr("TAB_GUIDE"));
    //     _tabCont.SetTabTitle    (6, Tr("TAB_STATS"));
    //     _tabContSave.SetTabTitle(0, Tr("TAB_SAVE_TERMINAL"));
    //     _tabContSave.SetTabTitle(1, Tr("TAB_STORE"));
    //     _tabContSave.SetTabTitle(2, Tr("TAB_WEAPON"));
    //     _tabContSave.SetTabTitle(3, Tr("TAB_TRANSPORT"));
    //     _tabSys.SetTabTitle     (0, Tr("TAB_CAMERA"));
    //     _tabSys.SetTabTitle     (1, Tr("TAB_DISPLAY"));
    //     _tabSys.SetTabTitle     (2, Tr("TAB_PERFORMANCE"));
    //     _tabSys.SetTabTitle     (3, Tr("TAB_AUDIO"));
    //     _tabSys.SetTabTitle     (4, Tr("TAB_LANGUAGE"));
    //     _tabCtrl.SetTabTitle    (0, Tr("TAB_KEYBOARD"));
    //     _tabCtrl.SetTabTitle    (1, Tr("TAB_CONTROLLER"));
    // }
    //
    // private void UpdateSystemTabContainer() {
    //     switch (_tabSys.CurrentTab) {
    //         case 0: _lbTab.Text = Tr("TAB_SYSTEM") + " | " + Tr("TAB_CAMERA");      break;
    //         case 1: _lbTab.Text = Tr("TAB_SYSTEM") + " | " + Tr("TAB_DISPLAY");     break;
    //         case 2: _lbTab.Text = Tr("TAB_SYSTEM") + " | " + Tr("TAB_PERFORMANCE"); break;
    //         case 3: _lbTab.Text = Tr("TAB_SYSTEM") + " | " + Tr("TAB_AUDIO");       break;
    //         case 4: _lbTab.Text = Tr("TAB_SYSTEM") + " | " + Tr("TAB_LANGUAGE");    break;
    //         default:                                                                break;
    //     }
    // }
    //
    // private void UpdateControlTabContainer() {
    //     if (_tabCtrl.CurrentTab == 0) _lbTab.Text = Tr("TAB_CONTROLS") + " | " + Tr("TAB_KEYBOARD");
    //     else                          _lbTab.Text = Tr("TAB_CONTROLS") + " | " + Tr("TAB_CONTROLLER");
    // }
    //
    // private void UpdateControlTab() {
    //     for (int i = 0; i < _bCK.Length; i++) {
    //         _bCK[i].Text = Tr(XB.AData.Input.InputActions[i].Description) + " - " + 
    //                        XB.AData.Input.InputActions[i].Key;
    //     }
    // }
    //
    // private void UpdateSettingsTab() {
    //     XB.Settings.UpdateSettingsTabs(_slCamHor, _lbCamHor, _slCamVer, _lbCamVer, _slFov, _lbFov, 
    //                                    _slFrame, _lbFrame,  _obRes, _obMode, _cbFps, _cbCross,
    //                                    _cbGProm, _cbProm, _cbBlood, _cbSave, _cbVSync, 
    //                                    _cbVoxel, _cbVoxelHQ, _cbVoxelHQHalf,
    //                                    _obMSAA, _obSSAA, _cbTAA, _cbDebanding,
    //                                    _lbShdwSize, _slShdwSize, _obShdwFilter,
    //                                    _lbShdwDist, _slShdwDist,
    //                                    _lbLOD, _slLOD, _obSSAO, _cbSSAOHalf, _obSSIL, _cbSSILHalf,
    //                                    _cbSSR, _obAudioMd,
    //                                    _slVolume, _lbVolume, _obLanguage);
    // }
    //
    // private void UpdateSettingsSliders() {
    //     XB.Settings.UpdateSliders(_slFrame, _lbFrame, _slShdwSize, _lbShdwSize, _slLOD, _lbLOD);
    // }
    //
    // private void ShowMessage(string msg) {
    //     _crMsg.Show();
    //     _lbMsg.Text  = msg;
    //     _t           = 0.0f;
    //     _crMsg.Color = XB.WorldData.MsgColor;
    // }
    //
    // public void ButtonResumeOnPressed() {
    //     XB.Utils.PlayUISound(XB.ScenePaths.ButtonAudio);
    //     Godot.Input.MouseMode = Godot.Input.MouseModeEnum.Captured;
    //     GetTree().Paused      = false;
    //     Hide();
    //     _menuType = XB.MenuType.None;
    // }
    //
    // public void ButtonSaveResumeOnPressed() {
    //     XB.Utils.PlayUISound(XB.ScenePaths.ButtonAudio);
    //     XB.PersistData.Respawn(false);
    //     Hide();
    //     _menuType = XB.MenuType.None;
    // }
    //
    // public void ButtonSaveOnPressed() {
    //     XB.Utils.PlayUISound(XB.ScenePaths.ButtonAudio);
    //     XB.PersistData.SaveDataWrite("ButtonSaveOnPressed");
    //     ShowMessage(Tr("GAME_SAVED"));
    // }
    //
    // public void ButtonLevelUpOnPressed() {
    //     XB.Utils.PlayUISound(XB.ScenePaths.ButtonAudio);
    //     XB.PersistData.PlayerLevel    += 1;
    //     XB.PersistData.Fragments      -= XB.PersistData.LvlUpCost;
    //     XB.PersistData.PlHealthMax    += XB.PersistData.LvlUpHealth;
    //     XB.PersistData.PlHealthMaxMod  = XB.PersistData.PlHealthMax;
    //     XB.PersistData.PlayerMult     += XB.PersistData.LvlUpDamage;
    //     UpdateSaveTab();
    //     XB.PersistData.SaveDataWrite("ButtonLevelUpOnPressed");
    // }
    //
    // public void ButtonQuitOnPressed() {
    //     XB.Utils.PlayUISound(XB.ScenePaths.ButtonAudio);
    //     XB.PersistData.SaveDataWrite("ButtonQuitOnPressed");
    //     XB.PersistData.ChangeScene(XB.ScenePaths.TitlePath);
    // }
    //
    // public void ButtonUseOnPressed() {
    //     XB.Utils.PlayUISound(XB.ScenePaths.ButtonAudio);
    //     int    id      = _iItem.GetSelectedItems()[0];
    //     string name    = _iItem.GetItemText(id);
    //     var    item    = XB.Entries.ItemEntries[XB.Entries.ItemNames[name]];
    //     string message = "";
    //     switch (item.Type) {
    //         case XB.ItemType.Outfit: {
    //             message = _player.ChangeOutfit(item.Item);
    //             XB.PersistData.SaveDataWrite("ChangeOutfit");
    //             break;
    //         }
    //         case XB.ItemType.Consumable: {
    //             message = Tr("ASSIGN") + " " + name + " " + Tr("TO_USE_WHILE_EXPLORING");
    //             break;
    //         }
    //         case XB.ItemType.ConsumableMenu: {
    //             message = ConsumeItem(item);
    //             break;
    //         }
    //         default: {
    //             XB.Log.Err("unhandled item type in Menu", XB.D.MenuButtonUseOnPressed);
    //             break;
    //         }
    //     }
    //     UpdateItemTabContainer(id);
    //     ShowMessage(message);
    // }
    //
    // public void ButtonBuyOnPressed() {
    //     XB.Utils.PlayUISound(XB.ScenePaths.ButtonAudio);
    //     int    id      = _iStore.GetSelectedItems()[0];
    //     string name    = _iStore.GetItemText(id);
    //     var    item    = XB.Entries.ItemEntries[XB.Entries.ItemNames[name]];
    //     XB.PersistData.Fragments -= item.Price;
    //     item.Amount           += 1;
    //     ShowMessage(Tr("BOUGHT") + " 1 " + Tr(item.Name) + ".");
    //     XB.PersistData.SaveDataWrite("Buy");
    // }
    //
    // public void ButtonSellOnPressed() {
    //     XB.Utils.PlayUISound(XB.ScenePaths.ButtonAudio);
    //     int    id      = _iStore.GetSelectedItems()[0];
    //     string name    = _iStore.GetItemText(id);
    //     var    item    = XB.Entries.ItemEntries[XB.Entries.ItemNames[name]];
    //     XB.PersistData.Fragments += item.Price/2;
    //     item.Amount -= 1;
    //     if (item.Amount == 0) UpdateStoreTab();
    //     ShowMessage(Tr("SOLD") + " 1 " + Tr(item.Name) + ".");
    //     XB.PersistData.SaveDataWrite("Sell");
    // }
    //
    // public void SliderShadowDistanceOnDragEnded(bool valueChanged) {
    //     if (!valueChanged) return;
    //     ShowMessage(XB.Settings.ChangeShadowDistance(_slShdwDist));
    //     UpdateSettingsTab();
    //     XB.PersistData.UpdateScreen();
    // }
    //
    // public void SliderFovOnDragEnded(bool valueChanged) {
    //     if (!valueChanged) return;
    //     ShowMessage(XB.Settings.ChangeFov(_slFov));
    //     UpdateSettingsTab();
    // }
    //
    // public void SliderCamHorOnDragEnded(bool valueChanged) {
    //     if (!valueChanged) return;
    //     ShowMessage(XB.Settings.ChangeSensitivityHorizontal(_slCamHor));
    //     UpdateSettingsTab();
    // }
    //
    // public void SliderCamVerOnDragEnded(bool valueChanged) {
    //     if (!valueChanged) return;
    //     ShowMessage(XB.Settings.ChangeSensitivityVertical(_slCamVer));
    //     UpdateSettingsTab();
    // }
    //
    // public void SliderVolumeOnDragEnded(bool valueChanged) {
    //     if (!valueChanged) return;
    //     ShowMessage(XB.Settings.ChangeVolume(_slVolume));
    //     UpdateSettingsTab();
    // }
    //
    // public void SliderFrameRateOnDragEnded(bool valueChanged) {
    //     UpdateSettingsSliders();
    // }
    //
    // public void SliderShadowSizeOnDragEnded(bool valueChanged) {
    //     UpdateSettingsSliders();
    // }
    //
    // public void SliderLODOnDragEnded(bool valueChanged) {
    //     UpdateSettingsSliders();
    // }
    //
    // public void ButtonDefaultsOnPressed() {
    //     XB.Utils.PlayUISound(XB.ScenePaths.ButtonAudio);
    //     ShowMessage(XB.Settings.DefaultSettings());
    //     XB.PersistData.UpdateScreen();
    //     UpdateSettingsTab();
    // }
    //
    // public void ButtonApplyOnPressed () {
    //     XB.Utils.PlayUISound(XB.ScenePaths.ButtonAudio);
    //     ShowMessage(XB.Settings.ApplySettings(_slFrame, _lbFrame, _slShdwSize, _lbShdwSize,
    //                                           _slShdwDist, _lbShdwDist, _slLOD, _lbLOD, _obRes, _obMode,
    //                                           _obMSAA,_obSSAA, _obShdwFilter, _obSSAO, _obSSIL));
    //     XB.PersistData.UpdateScreen();
    // }
    //
    // public void ButtonVoxelGIOnPressed() {
    //     ShowMessage(XB.Settings.ToggleVoxelGI());
    //     UpdateSettingsTab();
    // }
    //
    // public void ButtonVoxelGIHQOnPressed() {
    //     ShowMessage(XB.Settings.ToggleVoxelGIHQ());
    //     UpdateSettingsTab();
    // }
    //
    // public void ButtonVoxelGIHalfOnPressed() {
    //     ShowMessage(XB.Settings.ToggleVoxelGIHalf());
    //     UpdateSettingsTab();
    // }
    //
    // public void ButtonTAAOnPressed() {
    //     ShowMessage(XB.Settings.ToggleTAA());
    //     UpdateSettingsTab();
    // }
    // 
    // public void ButtonDebandingOnPressed() {
    //     ShowMessage(XB.Settings.ToggleDebanding());
    //     UpdateSettingsTab();
    // }
    //
    // public void ButtonSSAOHalfOnPressed() {
    //     ShowMessage(XB.Settings.ToggleSSAOHalf());
    //     UpdateSettingsTab();
    // }
    //
    // public void ButtonSSILHalfOnPressed() {
    //     ShowMessage(XB.Settings.ToggleSSILHalf());
    //     UpdateSettingsTab();
    // }
    //
    // public void ButtonSSROnPressed() {
    //     ShowMessage(XB.Settings.ToggleSSR());
    //     UpdateSettingsTab();
    // }
    //
    // public void ButtonVSyncOnPressed() {
    //     ShowMessage(XB.Settings.ToggleVSync());
    //     UpdateSettingsTab();
    // }
    //
    // public void ButtonShowFPSOnPressed() {
    //     ShowMessage(XB.Settings.ToggleShowFPS());
    //     XB.PersistData.UpdateScreen();
    //     UpdateSettingsTab();
    // }
    //
    // public void ButtonCrosshairsOnPressed() {
    //     ShowMessage(XB.Settings.ToggleCrosshairs());
    //     UpdateSettingsTab();
    // }
    //
    // public void ButtonGuidePromptsOnPressed() {
    //     ShowMessage(XB.Settings.ToggleGuidePrompts());
    //     UpdateSettingsTab();
    // }
    //
    // public void ButtonPromptsOnPressed() {
    //     ShowMessage(XB.Settings.TogglePrompts());
    //     UpdateSettingsTab();
    // }
    //
    // public void ButtonBloodOnPressed() {
    //     ShowMessage(XB.Settings.ToggleBlood());
    //     UpdateSettingsTab();
    // }
    //
    // public void ButtonSaveLabelOnPressed() {
    //     ShowMessage(XB.Settings.ToggleSaveLabel());
    //     XB.PersistData.UpdateScreen();
    //     UpdateSettingsTab();
    // }
    //
    // public void ButtonPopupBackOnPressed() {
    //     XB.Utils.PlayUISound(XB.ScenePaths.ButtonAudio);
    //     _ctrlPopup.Hide();
    // }
    //
    // public void ButtonPopupRespawnOnPressed() {
    //     XB.Utils.PlayUISound(XB.ScenePaths.ButtonAudio);
    //     _ctrlPopup.Show();
    //     _lbPopup.Text = Tr("RETURN_TO_LAST_TERMINAL");
    //     _bPopRet.Show();
    //     _bPopQuit.Hide();
    // }
    //
    // public void ButtonPopupQuitOnPressed() {
    //     XB.Utils.PlayUISound(XB.ScenePaths.ButtonAudio);
    //     _ctrlPopup.Show();
    //     _lbPopup.Text = Tr("QUIT_TO_TITLE");
    //     _bPopRet.Hide();
    //     _bPopQuit.Show();
    // }
    //
    // public void ButtonDefaultsCKOnPressed() {
    //     XB.Utils.PlayUISound(XB.ScenePaths.ButtonAudio);
    //     ShowMessage(Tr("DEFAULT_KEYBINDINGS"));
    //     XB.PersistData.ControlBindingsLoad(true);
    //     UpdateControlTab();
    // }
    //
    // public void ButtonCK00OnPressed() {ControlsChange( 0);}
    // public void ButtonCK01OnPressed() {ControlsChange( 1);}
    // public void ButtonCK02OnPressed() {ControlsChange( 2);}
    // public void ButtonCK03OnPressed() {ControlsChange( 3);}
    // public void ButtonCK04OnPressed() {ControlsChange( 4);}
    // public void ButtonCK05OnPressed() {ControlsChange( 5);}
    // public void ButtonCK06OnPressed() {ControlsChange( 6);}
    // public void ButtonCK07OnPressed() {ControlsChange( 7);}
    // public void ButtonCK08OnPressed() {ControlsChange( 8);}
    // public void ButtonCK09OnPressed() {ControlsChange( 9);}
    // public void ButtonCK10OnPressed() {ControlsChange(10);}
    // public void ButtonCK11OnPressed() {ControlsChange(11);}
    // public void ButtonCK12OnPressed() {ControlsChange(12);}
    // public void ButtonCK13OnPressed() {ControlsChange(13);}
    // public void ButtonCK14OnPressed() {ControlsChange(14);}
    // public void ButtonCK15OnPressed() {ControlsChange(15);}
    // public void ButtonCK16OnPressed() {ControlsChange(16);}
    // public void ButtonCK17OnPressed() {ControlsChange(17);}
    // public void ButtonCK18OnPressed() {ControlsChange(18);}
    // public void ButtonCK19OnPressed() {ControlsChange(19);}
    // public void ButtonCK20OnPressed() {ControlsChange(20);}
    // public void ButtonCK21OnPressed() {ControlsChange(21);}
    // public void ButtonCK22OnPressed() {ControlsChange(22);}
    // public void ButtonCK23OnPressed() {ControlsChange(23);}
    // public void ButtonCK24OnPressed() {ControlsChange(24);}
    // public void ButtonCK25OnPressed() {ControlsChange(25);}
    //
    // private void ControlsChange(int id) {
    //     XB.Utils.PlayUISound(XB.ScenePaths.ButtonAudio);
    //     _setKeyID             = id;
    //     _setKey               = true;
    //     _mouseRelease         = false;
    //     _chngMsg.Visible      = true;
    //     Godot.Input.MouseMode = Godot.Input.MouseModeEnum.Captured;
    // }
    //
    // //NOTE[ALEX]: godot documentation is wrong, parameter has to be long, not int
    // private void OptionButtonBotSlot0OnItemSelected(long id) {
    //     if (id == 0) {
    //         ResetBotSlot(0);
    //         return;
    //     }
    //     XB.PersistData.BotSlot[0] = XB.Entries.ItemNames[_obBS0.GetItemText((int)id)];
    //     ShowMessage(Tr("ASSIGNED") + " " + _obBS0.GetItemText((int)id) + " " + Tr("TO_THE_BOT"));
    //     _bot.SetBotDisplayIcons(0);
    //     XB.PersistData.SaveDataWrite("BotSlot0");
    // }
    //
    // private void OptionButtonBotSlot1OnItemSelected(long id) {
    //     if (id == 0) {
    //         ResetBotSlot(1);
    //         return;
    //     }
    //     XB.PersistData.BotSlot[1] = XB.Entries.ItemNames[_obBS1.GetItemText((int)id)];
    //     ShowMessage(Tr("ASSIGNED") + " " + _obBS1.GetItemText((int)id) + " " + Tr("TO_THE_BOT"));
    //     _bot.SetBotDisplayIcons(1);
    //     XB.PersistData.SaveDataWrite("BotSlot1");
    // }
    //
    // private void OptionButtonBotSlot2OnItemSelected(long id) {
    //     if (id == 0) {
    //         ResetBotSlot(2);
    //         return;
    //     }
    //     XB.PersistData.BotSlot[2] = XB.Entries.ItemNames[_obBS2.GetItemText((int)id)];
    //     ShowMessage(Tr("ASSIGNED") + " " + _obBS2.GetItemText((int)id) + " " + Tr("TO_THE_BOT"));
    //     _bot.SetBotDisplayIcons(2);
    //     XB.PersistData.SaveDataWrite("BotSlot2");
    // }
    //
    // private void OptionButtonGunOnItemSelected(long id) {
    //     switch (id) {
    //         case 0: {
    //             XB.PersistData.ImpactWpn = new XB.WData(XB.WName.CorpImpact);
    //             XB.PersistData.ProjWpn   = new XB.WData(XB.WName.CorpProjectile);
    //             _trGun.Texture           = Godot.ResourceLoader.Load<Godot.Texture2D>(_corpGunTex);
    //             break;
    //         }
    //         case 1: {
    //             XB.PersistData.ImpactWpn = new XB.WData(XB.WName.ArtImpact);
    //             XB.PersistData.ProjWpn   = new XB.WData(XB.WName.ArtProjectile);
    //             _trGun.Texture           = Godot.ResourceLoader.Load<Godot.Texture2D>(_artGunTex);
    //             break;
    //         }
    //     }
    //     _player.SetGuns();
    //     for (int i = 0; i < XB.PersistData.Upgrades.Length; i++) {
    //         XB.PersistData.Upgrades[i] = XB.Upgrade.None;
    //         UpdateUpgradeSlotLabels(i);
    //     }
    //     UpdateUpgradeTab();
    //     XB.PersistData.SaveDataWrite("GunOnItemSelected");
    // }
    //
    // private void OptionButtonLanguageOnItemSelected(long id) {
    //     ShowMessage(XB.Settings.ChangeLanguage(_obLanguage));
    //     UpdateSystemTabContainer();
    //     UpdateSettingsTab();
    //     UpdateTabNames();
    // }
    //
    // private void ButtonUpgradeBuyOnPressed() {
    //     XB.Utils.PlayUISound(XB.ScenePaths.ButtonAudio);
    //     int        selIDU   = _iUpgrade.GetSelectedItems()[0];
    //     string     selNameU = _iUpgrade.GetItemText(selIDU);
    //     XB.Upgrade upg      = XB.WUData.UpgradeNames[selNameU];
    //     var        u        = XB.WUData.Upgrades[upg];
    //     if (u.Bought) {
    //         _lbUPopup.Text = Tr(u.Name);
    //         // show popup012 buttons as appropriate
    //         _ctrlUPopup.Show();
    //     } else {
    //         XB.PersistData.Fragments                             -= u.PrFragments;
    //         XB.Entries.ItemEntries[XB.Item.ArtefactShard].Amount -= u.PrShards;
    //         u.Bought        = true;
    //         _bUBuy.Text     = Tr("EQUIP_UPGRADE");
    //         _bUBuy.Disabled = false;
    //     }
    // }
    //
    // private void ButtonUpgradePopup0OnPressed() {
    //     XB.Utils.PlayUISound(XB.ScenePaths.ButtonAudio);
    //     EquipUpgrade(0);
    //     _ctrlUPopup.Hide();
    // }
    //
    // private void ButtonUpgradePopup1OnPressed() {
    //     XB.Utils.PlayUISound(XB.ScenePaths.ButtonAudio);
    //     EquipUpgrade(1);
    //     _ctrlUPopup.Hide();
    // }
    //
    // private void ButtonUpgradePopup2OnPressed() {
    //     XB.Utils.PlayUISound(XB.ScenePaths.ButtonAudio);
    //     EquipUpgrade(2);
    //     _ctrlUPopup.Hide();
    // }
    //
    // private void EquipUpgrade(int slot) {
    //     int        selIDU   = _iUpgrade.GetSelectedItems()[0];
    //     string     selNameU = _iUpgrade.GetItemText(selIDU);
    //     XB.Upgrade upg      = XB.WUData.UpgradeNames[selNameU];
    //     if (upg != XB.Upgrade.None) {
    //         for (int i = 0; i < XB.PersistData.Upgrades.Length; i++) {
    //             if (XB.PersistData.Upgrades[i] == upg) {
    //                 XB.PersistData.Upgrades[i] = XB.Upgrade.None;
    //                 UpdateUpgradeSlotLabels(i);
    //             }
    //         }
    //     }
    //     XB.PersistData.Upgrades[slot] = upg;
    //     UpdateUpgradeSlotLabels(slot);
    //     XB.PersistData.SaveDataWrite("EquipUpgrade");
    // }
    //
    // private void ButtonUpgradePopupRemoveOnPressed() {
    //     XB.Utils.PlayUISound(XB.ScenePaths.ButtonAudio);
    //     int        selIDU   = _iUpgrade.GetSelectedItems()[0];
    //     string     selNameU = _iUpgrade.GetItemText(selIDU);
    //     XB.Upgrade upg      = XB.WUData.UpgradeNames[selNameU];
    //     for (int i = 0; i < XB.PersistData.Upgrades.Length; i++) {
    //         if (XB.PersistData.Upgrades[i] == upg) {
    //             XB.PersistData.Upgrades[i] = XB.Upgrade.None;
    //             UpdateUpgradeSlotLabels(i);
    //         }
    //     }
    //     XB.PersistData.SaveDataWrite("RemoveUpgrade");
    //     _ctrlUPopup.Hide();
    // }
    //
    // private void UpdateUpgradeSlotLabels(int slot) {
    //     string name  = XB.WUData.Upgrades[XB.PersistData.Upgrades[slot]].Name;
    //     switch (slot) {
    //         case 0: _lbU0.Text = name; break;
    //         case 1: _lbU1.Text = name; break;
    //         case 2: _lbU2.Text = name; break;
    //     }
    // }
    //
    // private void ButtonUpgradePopupBackOnPressed() {
    //     XB.Utils.PlayUISound(XB.ScenePaths.ButtonAudio);
    //     _ctrlUPopup.Hide();
    // }
    //
    // private void ButtonTransportOnPressed() {
    //     XB.Utils.PlayUISound(XB.ScenePaths.ButtonAudio);
    //     XB.AData.SceneID              = _selLvl;
    //     XB.PersistData.LoadSpawnPoint = XB.SpawnPoint.Terminal;
    //     XB.PersistData.LastSaveTerm   = _selTerm;
    //     XB.PersistData.SaveDataWrite("MenuTransport");
    //     XB.PersistData.ChangeScene(XB.Entries.LevelEntries[XB.AData.SceneID].ScenePath);
    // }
    //
    // public void ItemListTerminalsItemClicked(long id, Godot.Vector2 atPos, long mButton) {
    //     if (mButton != 1) return;
    //
    //     //if (!_iLvl.IsAnythingSelected()) break;
    //     int selIDLvl    = _iLvl.GetSelectedItems()[0];
    //     var lv          = _listLvl[selIDLvl];
    //     _lbLvl.Text     = Tr(XB.Entries.LevelEntries[lv].LevelName);
    //     _lbLvlDesc.Text = Tr(XB.Entries.LevelEntries[lv].Description);
    //
    //     int selIDTerm = _iTerminals.GetSelectedItems()[0];
    //     var tm        = _listTerm[selIDTerm];
    //     for (int i = 0; i < XB.Entries.LevelEntries[lv].LevelTerms.Length; i++) {
    //         if (tm != XB.Entries.LevelEntries[lv].LevelTerms[i]) continue;
    //         _lbTerminal.Text = XB.Entries.LevelEntries[lv].TermNames[i];
    //     }
    //     _bTransport.Disabled = false;
    //     _bTransport.Text     = "BTN_TRANSPORT_TO_AREA";
    //     _selLvl              = lv;
    //     _selTerm             = tm;
    //
    //     if (_termClicked == (int)id && _tTerm <= _tTermWindow) {
    //         ButtonTransportOnPressed();
    //     }
    //     _termClicked = (int)id;
    //     _tTerm       = 0.0f;
    // }
    //
    // public void ItemListLevelsItemClicked(long id, Godot.Vector2 atPos, long mButton) {
    //     if (mButton != 1) return;
    //     int selIDLvl    = _iLvl.GetSelectedItems()[0];
    //     var lv          = _listLvl[selIDLvl];
    //
    //     if (_lbLvl.Text != Tr(XB.Entries.LevelEntries[lv].LevelName)) _termClicked = -1;
    //     UpdateTransportTabTerminals(lv);
    // }
    //
    // private void ResetBotSlot(int id) {
    //     XB.PersistData.BotSlot[id] = XB.Item.Uninit;
    //     _bot.SetBotDisplayIcons(id);
    //     ShowMessage(Tr("REMOVED_ITEM_FROM_THE_BOT"));
    //     XB.PersistData.SaveDataWrite("BotslotReset");
    // }
    //
    // private string ConsumeItem(XB.ItemEntry item) {
    //     if (!item.UnlimitedUse) item.Amount -= 1;
    //     string message = Tr("ITEM_USED");
    //     switch (item.Item) {
    //         case XB.Item.Home:
    //             _player.InitiateCallHome();
    //             ButtonResumeOnPressed();
    //             return message; //NOTE[ALEX]: if we let it go like other items, markers get overwritten
    //         case XB.Item.Fragment0:
    //         case XB.Item.Fragment1:
    //         case XB.Item.Fragment2:
    //         case XB.Item.Fragment3:
    //         case XB.Item.Fragment4:
    //         case XB.Item.Fragment5:
    //         case XB.Item.Fragment6:
    //         case XB.Item.Fragment7:
    //         case XB.Item.Fragment8:
    //         case XB.Item.Fragment9: {
    //             XB.PersistData.Fragments += item.Price;
    //             message = item.Price.ToString() + " " + Tr("FRAGMENTS_ADDED");
    //             break;
    //         }
    //         case XB.Item.KosmosEssence: {
    //             XB.PersistData.States[XB.State.Kosmos] = !XB.PersistData.States[XB.State.Kosmos];
    //             _player.UpdateEyes();
    //             if (XB.PersistData.States[XB.State.Kosmos]) message = Tr("TOOK_IN_KOSMOS");
    //             else                                        message = Tr("REMOVED_KOSMOS");
    //             break;
    //         }
    //         case XB.Item.ScmSwimSuitRed: {
    //             XB.PersistData.States[XB.State.ScmSwimSuitRed] = true;
    //             message = Tr("SCHEMATIC_USED") + " " + 
    //                       "(" + Tr(XB.Entries.ItemEntries[XB.Item.SwimSuitRed].Name) + ")";
    //             break;
    //         }
    //         case XB.Item.ScmSwimSuitGold: {
    //             XB.PersistData.States[XB.State.ScmSwimSuitGold] = true;
    //             message = Tr("SCHEMATIC_USED") + " " + 
    //                       "(" + Tr(XB.Entries.ItemEntries[XB.Item.SwimSuitGold].Name) + ")";
    //             break;
    //         }
    //         case XB.Item.ScmSwimSuitBlack: {
    //             XB.PersistData.States[XB.State.ScmSwimSuitBlack] = true;
    //             message = Tr("SCHEMATIC_USED") + " " + 
    //                       "(" + Tr(XB.Entries.ItemEntries[XB.Item.SwimSuitBlack].Name) + ")";
    //             break;
    //         }
    //         case XB.Item.ScmSwimSuitLaser: {
    //             XB.PersistData.States[XB.State.ScmSwimSuitLaser] = true;
    //             message = Tr("SCHEMATIC_USED") + " " + 
    //                       "(" + Tr(XB.Entries.ItemEntries[XB.Item.SwimSuitLaser].Name) + ")";
    //             break;
    //         }
    //         case XB.Item.CylinderUpgrade: {
    //             XB.PersistData.CylAmountMax += 1;
    //             message = Tr("CYLINDER_UPGRADED") + " " + XB.PersistData.CylAmountMax.ToString() + ".";
    //             break;
    //         }
    //         case XB.Item.DifficultyUp: {
    //             XB.PersistData.IncreaseDifficulty();
    //             message = Tr("DIFFICULTY_UP");
    //             break;
    //         }
    //         case XB.Item.TripleEdge: {
    //             if (XB.PersistData.PlHealthMax == 1.0f) {
    //                 item.Amount = 1;
    //                 return Tr("TRIPLE_EDGE_ACTIVE");
    //             }
    //             XB.PersistData.PlHealthMaxMod     = XB.PersistData.PlHealthMax;
    //             XB.PersistData.PlHealthMax        = 1.0f;
    //             XB.PersistData.PlHealth           = 1.0f;
    //             XB.PersistData.ImpactWpn.ShotDmg *= 3.0f;
    //             XB.PersistData.ProjWpn.ShotDmg   *= 3.0f;
    //             break;
    //         }
    //         default: {
    //             XB.Log.Err("ConsumeItem in Menu has unhandled case.", XB.D.MenuConsumeItem);
    //             break;
    //         }
    //     }
    //     XB.PersistData.SaveDataWrite("ConsumeItem");
    //     return message;
    // }
}
} // namespace close 
