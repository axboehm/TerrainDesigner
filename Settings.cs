namespace XB { // namespace open
using SysCG = System.Collections.Generic;
public class SettingsContainer {
    public bool   FullScreen     = false;
    public string Resolution     = "1920x1080";
    public float  MouseMultX     = -30.0f/1920.0f;
    public float  MouseMultY     = -30.0f/1080.0f;
    public int    Fps            = 60;
    public bool   ShowGuides     = false;
    public bool   ShowFps        = false;
    public bool   BlockGrid      = false;
    public bool   QTreeVis       = false;
    public float  FovDef         = 0.0f; // camera fov when not aiming (in mm)
    public float  FovAim         = 0.0f; // when aiming
    public float  CamXSens       = 2.0f;
    public float  CamYSens       = 2.0f;
    public float  CamMaxDist     = 0.0f;
    public float  CamAimDist     = 0.0f;
    public float  Volume         = -45.0f; // audio master volume
    public bool   VSync          = false;
    public string Language       = "en";
    public string MSAASel        = "DISABLED";
    public string SSAASel        = "DISABLED";
    public bool   TAA            = false;
    public bool   Debanding      = false;
    public int    ShadowSize     = 512;
    public string ShadowFilter   = "SHADOWF0";
    public int    ShadowDistance = 256;
    public float  LODSel         = 1.0f;
    public string SSAOSel        = "SSAO0";
    public bool   SSAOHalf       = false;
    public string SSILSel        = "SSIL0";
    public bool   SSILHalf       = false;
    public bool   SSR            = false;

    public void SetAllFromSettingsContainer(XB.SettingsContainer scFrom) {
        FullScreen     = scFrom.FullScreen;
        Resolution     = scFrom.Resolution;
        MouseMultX     = scFrom.MouseMultX;
        MouseMultY     = scFrom.MouseMultY;
        Fps            = scFrom.Fps;
        ShowGuides     = scFrom.ShowGuides;
        ShowFps        = scFrom.ShowFps;
        BlockGrid      = scFrom.BlockGrid;
        QTreeVis       = scFrom.QTreeVis;
        FovDef         = scFrom.FovDef;
        FovAim         = scFrom.FovAim;
        CamXSens       = scFrom.CamXSens;
        CamYSens       = scFrom.CamYSens;
        CamMaxDist     = scFrom.CamMaxDist;
        CamAimDist     = scFrom.CamAimDist;
        Volume         = scFrom.Volume;
        VSync          = scFrom.VSync;
        Language       = scFrom.Language;
        MSAASel        = scFrom.MSAASel;
        SSAASel        = scFrom.SSAASel;
        TAA            = scFrom.TAA;
        Debanding      = scFrom.Debanding;
        ShadowSize     = scFrom.ShadowSize;
        ShadowFilter   = scFrom.ShadowFilter;
        ShadowDistance = scFrom.ShadowDistance;
        LODSel         = scFrom.LODSel;
        SSAOSel        = scFrom.SSAOSel;
        SSAOHalf       = scFrom.SSAOHalf;
        SSILSel        = scFrom.SSILSel;
        SSILHalf       = scFrom.SSILHalf;
        SSR            = scFrom.SSR;
    }
}

public class SettingsStateChange {
    public bool ChangeCamSens      = false;
    public bool ChangeDebanding    = false;
    public bool ChangeFov          = false;
    public bool ChangeFps          = false;
    public bool ChangeHUD          = false;
    public bool ChangeLanguage     = false;
    public bool ChangeLOD          = false;
    public bool ChangeMSAA         = false;
    public bool ChangeScreen       = false;
    public bool ChangeShadowDist   = false;
    public bool ChangeShadowFilter = false;
    public bool ChangeShadowSize   = false;
    public bool ChangeSSAA         = false;
    public bool ChangeSSAO         = false;
    public bool ChangeSSIL         = false;
    public bool ChangeSSR          = false;
    public bool ChangeTAA          = false;
    public bool ChangeVolume       = false;
    public bool ChangeVSync        = false;

    public void SetAllFalse() {
        ChangeCamSens      = false;
        ChangeDebanding    = false;
        ChangeFov          = false;
        ChangeFps          = false;
        ChangeHUD          = false;
        ChangeLanguage     = false;
        ChangeLOD          = false;
        ChangeMSAA         = false;
        ChangeScreen       = false;
        ChangeShadowDist   = false;
        ChangeShadowFilter = false;
        ChangeShadowSize   = false;
        ChangeSSAA         = false;
        ChangeSSAO         = false;
        ChangeSSIL         = false;
        ChangeSSR          = false;
        ChangeTAA          = false;
        ChangeVolume       = false;
        ChangeVSync        = false;
    }
}

// all active settings live in one SettingsContainer while the app is running
// changes to settings in the menu, etc. are made to a second container and then
// this class updates the active settings and makes all changes required to the application
// the goal was to make every change to settings go through the same function to ensure
// consistency and only one place of truth
public class Settings {
    public  XB.SettingsContainer SC     = new XB.SettingsContainer(); // application settings
    private XB.SettingsContainer _scMod = new XB.SettingsContainer(); // for changing settings
    private static SettingsStateChange _chng = new XB.SettingsStateChange();

    public  int[]        FpsOptions      = new int[]    {30, 60, 120, 0};
    public  string[]     WindowModes     = new string[] {"WINDOWED", "FULLSCREEN"};
    public  string[]     Languages       = new string[] {"en", "de"};
    public  string[]     MSAA            = new string[] {"DISABLED", "MSAA2", "MSAA4", "MSAA8"};
    public  string[]     SSAA            = new string[] {"DISABLED", "FXAA"};
    public  int[]        ShadowSizes     = new int[]    {512, 1024, 2048, 4096};
    public  string[]     ShadowFilters   = new string[] {"SHADOWF0", "SHADOWF1", "SHADOWF2",
                                                         "SHADOWF3", "SHADOWF4", "SHADOWF5"};
    public  float[]      LOD             = new float[]  {4.0f, 2.0f, 1.0f};
    public  string[]     SSAO            = new string[] {"SSAO0", "SSAO1", "SSAO2", "SSAO3"};
    public  string[]     SSIL            = new string[] {"SSIL0", "SSIL1", "SSIL2", "SSIL3"};
    private const string _baseResolution = "1920x1080";
    public  const int    BaseResX        = 1920;
    public  const int    BaseResY        = 1080;
    private string[]     _resStrings     = new string[] {"3840x2160", "2560x1440", "2048x1152",
                                                         "1920x1080", "1280x720"};
    public SysCG.Dictionary<string, Godot.Vector2I> Resolutions
        = new SysCG.Dictionary<string, Godot.Vector2I>() {
            {"3840x2160", new Godot.Vector2I(3840, 2160)},
            {"2560x1440", new Godot.Vector2I(2560, 1440)},
            {"2048x1152", new Godot.Vector2I(2048, 1152)},
            {"1920x1080", new Godot.Vector2I(1920, 1080)},
            {"1280x720",  new Godot.Vector2I(1280, 720)},
        };
    public SysCG.Dictionary<string, XB.SettingsPreset> Presets
        = new SysCG.Dictionary<string, XB.SettingsPreset>() {
            {"Lowest",  XB.SettingsPreset.Minimum},
            {"Default", XB.SettingsPreset.Default},
            {"Highest", XB.SettingsPreset.Maximum},
        };

    public  const float CamSensMax      = 100.0f;
    public  const float VolumeMin       = -60.0f;
    public  const float ShadowDistMin   = 50.0f;
    public  const float ShadowDistMax   = 250.0f;
    public  const float FovMin          = 12.0f;
    public  const float FovMax          = 70.0f;
    public  const float FovMult         = 1.0f/28.0f;
    public  const float CamMinDist      = 0.5f;
    public  const float _camMaxDistMult = 4.2f;
    private const float _camAimDistMult = 1.0f;
    private const float _fovAimM            = 1.25f;
    private const float _ssaoAdaptiveTarget = 0.5f;
    private const int   _ssaoBlurPasses     = 2;
    private const float _ssaoFadeOutFrom    = 50.0f;
    private const float _ssaoFadeOutTo      = 300.0f;
    private const float _ssilAdaptiveTarget = 0.5f;
    private const int   _ssilBlurPasses     = 4;
    private const float _ssilFadeOutFrom    = 50.0f;
    private const float _ssilFadeOutTo      = 300.0f;
    private const float _camSliderMult      = 25.0f;

    // settings code variables
    private static SysCG.Dictionary<string, ulong> _sPos =
        new SysCG.Dictionary<string, ulong>() {
                {"SGuide", (ulong)1 << 0 },
                {"SFps",   (ulong)1 << 1 },
                {"Full",   (ulong)1 << 2 },
                {"VSync",  (ulong)1 << 3 },
                {"TAA",    (ulong)1 << 4 },
                {"Deba",   (ulong)1 << 5 },
                {"SSAOH",  (ulong)1 << 6 },
                {"SSILH",  (ulong)1 << 7 },
                {"SSR",    (ulong)1 << 8 },
                {"Fps0",   (ulong)1 << 9 },
                {"Fps1",   (ulong)1 << 10},
                {"Fps2",   (ulong)1 << 11},
                {"Fps3",   (ulong)1 << 12},
                {"Lang",   (ulong)1 << 13},
                {"SSAA",   (ulong)1 << 14},
                {"MSAA0",  (ulong)1 << 15},
                {"MSAA1",  (ulong)1 << 16},
                {"MSAA2",  (ulong)1 << 17},
                {"MSAA3",  (ulong)1 << 18},
                {"SSize0", (ulong)1 << 19},
                {"SSize1", (ulong)1 << 20},
                {"SSize2", (ulong)1 << 21},
                {"SSize3", (ulong)1 << 22},
                {"SFilt0", (ulong)1 << 23},
                {"SFilt1", (ulong)1 << 24},
                {"SFilt2", (ulong)1 << 25},
                {"SFilt3", (ulong)1 << 26},
                {"SFilt4", (ulong)1 << 27},
                {"SFilt5", (ulong)1 << 28},
                {"LODS0",  (ulong)1 << 29},
                {"LODS1",  (ulong)1 << 30},
                {"LODS2",  (ulong)1 << 31},
                {"SSAO0",  (ulong)1 << 32},
                {"SSAO1",  (ulong)1 << 33},
                {"SSAO2",  (ulong)1 << 34},
                {"SSAO3",  (ulong)1 << 35},
                {"SSIL0",  (ulong)1 << 36},
                {"SSIL1",  (ulong)1 << 37},
                {"SSIL2",  (ulong)1 << 38},
                {"SSIL3",  (ulong)1 << 39},
                {"Res0",   (ulong)1 << 40},
                {"Res1",   (ulong)1 << 41},
                {"Res2",   (ulong)1 << 42},
                {"Res3",   (ulong)1 << 43},
                {"Res4",   (ulong)1 << 44},
                {"SBlock", (ulong)1 << 45},
                {"SQTree", (ulong)1 << 46},
            };
    private ulong        _setCodeR       = 0;
    private ulong        _setCodeL       = 0;
    private const  int   _setCodeLengthR = 47;
    private const  int   _setCodeLengthL = 35;
    private static ulong _sFov  = (ulong)63  << 7+7+7+8+0;
    private static ulong _sCamX = (ulong)127 <<   7+7+8+0;
    private static ulong _sCamY = (ulong)127 <<     7+8+0;
    private static ulong _sVol  = (ulong)127 <<       8+0;
    private static ulong _sShD  = (ulong)255 <<         0;


    public void UpdateSettings(XB.SettingsContainer sc) {
        _chng.SetAllFalse();

        if (SC.FullScreen != sc.FullScreen)         { _chng.ChangeScreen = true; }
        if (SC.Resolution != sc.Resolution)         { _chng.ChangeScreen = true; }
        if (SC.MouseMultX != sc.MouseMultX)         { _chng.ChangeScreen = true; }
        if (SC.MouseMultY != sc.MouseMultY)         { _chng.ChangeScreen = true; }
        if (SC.Fps != sc.Fps)                       { _chng.ChangeFps = true; }
        if (SC.ShowGuides != sc.ShowGuides)         { _chng.ChangeHUD = true; }
        if (SC.ShowFps != sc.ShowFps)               { _chng.ChangeHUD = true; }
        if (SC.BlockGrid != sc.BlockGrid)           { _chng.ChangeHUD = true; }
        if (SC.QTreeVis != sc.QTreeVis)             { _chng.ChangeHUD = true; }
        if (SC.FovDef != sc.FovDef)                 { _chng.ChangeFov = true; }
        if (SC.CamXSens != sc.CamXSens)             { _chng.ChangeCamSens = true; }
        if (SC.CamYSens != sc.CamYSens)             { _chng.ChangeCamSens = true; }
        if (SC.Volume != sc.Volume)                 { _chng.ChangeVolume = true; }
        if (SC.VSync != sc.VSync)                   { _chng.ChangeVSync = true; }
        if (SC.Language != sc.Language)             { _chng.ChangeLanguage = true; }
        if (SC.MSAASel != sc.MSAASel)               { _chng.ChangeMSAA = true; }
        if (SC.SSAASel != sc.SSAASel)               { _chng.ChangeSSAA = true; }
        if (SC.TAA != sc.TAA)                       { _chng.ChangeTAA = true; }
        if (SC.Debanding != sc.Debanding)           { _chng.ChangeDebanding = true; }
        if (SC.ShadowSize != sc.ShadowSize)         { _chng.ChangeShadowSize = true; }
        if (SC.ShadowFilter != sc.ShadowFilter)     { _chng.ChangeShadowFilter = true; }
        if (SC.ShadowDistance != sc.ShadowDistance) { _chng.ChangeShadowDist = true; }
        if (SC.LODSel != sc.LODSel)                 { _chng.ChangeLOD = true; }
        if (SC.SSAOSel != sc.SSAOSel)               { _chng.ChangeSSAO = true; }
        if (SC.SSAOHalf != sc.SSAOHalf)             { _chng.ChangeSSAO = true; }
        if (SC.SSILSel != sc.SSILSel)               { _chng.ChangeSSIL = true; }
        if (SC.SSILHalf != sc.SSILHalf)             { _chng.ChangeSSIL = true; }
        if (SC.SSR != sc.SSR)                       { _chng.ChangeSSR = true; }

        if (_chng.ChangeCamSens)      { UpdateCameraSensitivity(sc.CamXSens, sc.CamYSens); }
        if (_chng.ChangeDebanding)    { UpdateDebanding(sc.Debanding); }
        if (_chng.ChangeFov)          { UpdateFov(sc.FovDef); }
        if (_chng.ChangeFps)          { UpdateFps(sc.Fps); }
        if (_chng.ChangeHUD)          { UpdateHUD(sc.ShowFps, sc.ShowGuides,
                                                  sc.BlockGrid, sc.QTreeVis); }
        if (_chng.ChangeLanguage)     { UpdateLanguage(sc.Language); }
        if (_chng.ChangeLOD)          { UpdateLOD(sc.LODSel); }
        if (_chng.ChangeMSAA)         { UpdateMSAA(sc.MSAASel); }
        if (_chng.ChangeScreen)       { UpdateScreen(sc.FullScreen, sc.Resolution); }
        if (_chng.ChangeShadowDist)   { UpdateShadowDistance(sc.ShadowDistance); }
        if (_chng.ChangeShadowFilter) { UpdateShadowFilter(sc.ShadowFilter); }
        if (_chng.ChangeShadowSize)   { UpdateShadowSize(sc.ShadowSize); }
        if (_chng.ChangeSSAA)         { UpdateSSAA(sc.SSAASel); }
        if (_chng.ChangeSSAO)         { UpdateSSAO(sc.SSAOSel, sc.SSAOHalf); }
        if (_chng.ChangeSSIL)         { UpdateSSIL(sc.SSILSel, sc.SSILHalf); }
        if (_chng.ChangeSSR)          { UpdateSSR(sc.SSR); }
        if (_chng.ChangeTAA)          { UpdateTAA(sc.TAA); }
        if (_chng.ChangeVolume)       { UpdateVolume(sc.Volume); }
        if (_chng.ChangeVSync)        { UpdateVSync(sc.VSync); }
    }

    private void UpdateCameraSensitivity(float camXSens, float camYSens) {
        SC.CamXSens = camXSens;
        SC.CamYSens = camYSens;
    }

    // resolution and window mode
    private void UpdateScreen(bool fullScreen, string resolution) {
        SC.FullScreen = fullScreen;
        SC.Resolution = resolution;

        var window  = XB.AData.MainRoot.GetTree().Root;
        window.Size = Resolutions[SC.Resolution];
        float scale = ((float)Resolutions[SC.Resolution].X) / ((float)Resolutions[_baseResolution].X);

        //TODO[ALEX]: should the mouse multiplier be adjusted in fullscreen?
        //            test if looking around feels the same or it needs adjustment
        if (SC.FullScreen) {
            window.Mode               = Godot.Window.ModeEnum.Fullscreen;
            XB.PController.Hud.Scale  = new Godot.Vector2(scale, scale);
            XB.PController.Menu.Scale = new Godot.Vector2(scale, scale);
            window.ContentScaleFactor = 1.0f/scale;
            window.ContentScaleMode   = Godot.Window.ContentScaleModeEnum.Viewport;
        } else {
            window.Mode               = Godot.Window.ModeEnum.Windowed;
            XB.PController.Hud.Scale  = new Godot.Vector2(1.0f, 1.0f);
            XB.PController.Menu.Scale = new Godot.Vector2(1.0f, 1.0f);
            window.ContentScaleFactor = scale;
            window.ContentScaleMode   = Godot.Window.ContentScaleModeEnum.Disabled;
        }
    }

    private void UpdateFps(int fps) {
        SC.Fps = fps;
        Godot.Engine.MaxFps = SC.Fps;
    }

    private void UpdateHUD(bool showFps, bool showGuides, bool blockGrid, bool qTreeVis) {
        SC.ShowFps    = showFps;
        SC.ShowGuides = showGuides;
        SC.BlockGrid  = blockGrid;
        SC.QTreeVis   = qTreeVis;
        XB.PController.Hud.UpdateHUDElementVisibility(SC.ShowFps, SC.ShowGuides,
                                                      SC.BlockGrid, SC.QTreeVis);
    }

    private void UpdateFov(float fovDef) {
        SC.FovDef = fovDef;
        SC.FovAim = SC.FovDef*_fovAimM;
        SC.FovAim = XB.Utils.ClampF(SC.FovAim, FovMin, FovMax);

        SC.CamMaxDist = SC.FovDef*FovMult*_camMaxDistMult;
        SC.CamAimDist = SC.FovDef*FovMult*_camAimDistMult;
    }

    private void UpdateVolume(float volume) {
        SC.Volume = volume;
        Godot.AudioServer.SetBusVolumeDb(0, SC.Volume); // 0 is master bus
    }

    private void UpdateLanguage(string language) {
        SC.Language = language;
        Godot.TranslationServer.SetLocale(SC.Language);
    }

    private void UpdateVSync(bool vSync) {
        SC.VSync = vSync;
        if (SC.VSync) {
            Godot.DisplayServer.WindowSetVsyncMode(Godot.DisplayServer.VSyncMode.Enabled); 
        } else {
            Godot.DisplayServer.WindowSetVsyncMode(Godot.DisplayServer.VSyncMode.Disabled);
        }
    }

    private void UpdateMSAA(string msaaSel) {
        SC.MSAASel = msaaSel;
        var viewport = XB.AData.MainRoot.GetViewport();
        switch (SC.MSAASel) {
            case "DISABLED": { viewport.Msaa3D = Godot.Viewport.Msaa.Disabled; break; }
            case "MSAA2":    { viewport.Msaa3D = Godot.Viewport.Msaa.Msaa2X;   break; }
            case "MSAA4":    { viewport.Msaa3D = Godot.Viewport.Msaa.Msaa4X;   break; }
            case "MSAA8":    { viewport.Msaa3D = Godot.Viewport.Msaa.Msaa8X;   break; }
        }
    }

    private void UpdateSSAA(string ssaaSel) {
        SC.SSAASel = ssaaSel;
        var viewport = XB.AData.MainRoot.GetViewport();
        switch (SC.SSAASel) {
            case "DISABLED": { viewport.ScreenSpaceAA = (Godot.Viewport.ScreenSpaceAAEnum)0; break; }
            case "FXAA":     { viewport.ScreenSpaceAA = (Godot.Viewport.ScreenSpaceAAEnum)1; break; }
        }
    }

    private void UpdateDebanding(bool debanding) {
        SC.Debanding = debanding;
        var viewport = XB.AData.MainRoot.GetViewport();
        viewport.UseDebanding = SC.Debanding;
    }

    private void UpdateTAA(bool taa) {
        SC.TAA = taa;
        var viewport = XB.AData.MainRoot.GetViewport();
        viewport.UseTaa = SC.TAA;
    }

    private void UpdateShadowSize(int shadowSize) {
        SC.ShadowSize = shadowSize;
        Godot.RenderingServer.DirectionalShadowAtlasSetSize(SC.ShadowSize, true);
    }

    private void UpdateShadowFilter(string shadowFilter) {
        SC.ShadowFilter = shadowFilter;

        var sFQuality = Godot.RenderingServer.ShadowQuality.Hard;
        switch (SC.ShadowFilter) {
            case "SHADOWF0": { sFQuality = Godot.RenderingServer.ShadowQuality.Hard;        break; }
            case "SHADOWF1": { sFQuality = Godot.RenderingServer.ShadowQuality.SoftVeryLow; break; }
            case "SHADOWF2": { sFQuality = Godot.RenderingServer.ShadowQuality.SoftLow;     break; }
            case "SHADOWF3": { sFQuality = Godot.RenderingServer.ShadowQuality.SoftMedium;  break; }
            case "SHADOWF4": { sFQuality = Godot.RenderingServer.ShadowQuality.SoftHigh;    break; }
            case "SHADOWF5": { sFQuality = Godot.RenderingServer.ShadowQuality.SoftUltra;   break; }
        }
        Godot.RenderingServer.DirectionalSoftShadowFilterSetQuality(sFQuality);
    }

    private void UpdateShadowDistance(int shadowDistance) {
        SC.ShadowDistance = shadowDistance;
        if (XB.AData.MainLight != null) {
            XB.AData.MainLight.DirectionalShadowMaxDistance = SC.ShadowDistance;
        }
    }

    private void UpdateLOD(float lodSel) {
        SC.LODSel = lodSel;
        XB.AData.MainRoot.GetTree().Root.MeshLodThreshold = SC.LODSel;
    }

    private void UpdateSSAO(string ssaoSel, bool ssaoHalf) {
        SC.SSAOSel  = ssaoSel;
        SC.SSAOHalf = ssaoHalf;

        var ssaoQuality = Godot.RenderingServer.EnvironmentSsaoQuality.VeryLow;
        switch (SC.SSAOSel) {
            case "SSAO0": { ssaoQuality = Godot.RenderingServer.EnvironmentSsaoQuality.VeryLow; break; }
            case "SSAO1": { ssaoQuality = Godot.RenderingServer.EnvironmentSsaoQuality.Low;     break; }
            case "SSAO2": { ssaoQuality = Godot.RenderingServer.EnvironmentSsaoQuality.Medium;  break; }
            case "SSAO3": { ssaoQuality = Godot.RenderingServer.EnvironmentSsaoQuality.High;    break; }
        }
        Godot.RenderingServer.EnvironmentSetSsaoQuality(ssaoQuality, SC.SSAOHalf,
                                                        _ssaoAdaptiveTarget, _ssaoBlurPasses,
                                                        _ssaoFadeOutFrom, _ssaoFadeOutTo     );
    }

    private void UpdateSSIL(string ssilSel, bool ssilHalf) {
        SC.SSILSel  = ssilSel;
        SC.SSILHalf = ssilHalf;

        var ssilQuality = Godot.RenderingServer.EnvironmentSsilQuality.VeryLow;
        switch (SC.SSILSel) {
            case "SSIL0": { ssilQuality = Godot.RenderingServer.EnvironmentSsilQuality.VeryLow; break; }
            case "SSIL1": { ssilQuality = Godot.RenderingServer.EnvironmentSsilQuality.Low;     break; }
            case "SSIL2": { ssilQuality = Godot.RenderingServer.EnvironmentSsilQuality.Medium;  break; }
            case "SSIL3": { ssilQuality = Godot.RenderingServer.EnvironmentSsilQuality.High;    break; }
        }
        Godot.RenderingServer.EnvironmentSetSsilQuality(ssilQuality, SC.SSILHalf, 
                                                        _ssilAdaptiveTarget, _ssilBlurPasses,
                                                        _ssilFadeOutFrom, _ssilFadeOutTo     );
    }

    private void UpdateSSR(bool ssr) {
        SC.SSR = ssr;
        XB.AData.Environment.SsrEnabled = SC.SSR;
    }

    public void SetApplicationDefaults() {
        _scMod.SetAllFromSettingsContainer(SC);
        _scMod.FullScreen = false;
        _scMod.Resolution = _baseResolution;
        _scMod.Fps        = FpsOptions[1];
        _scMod.ShowFps    = false;
        _scMod.ShowGuides = true;
        _scMod.BlockGrid  = false;
        _scMod.QTreeVis   = false;
        _scMod.FovDef     = 28.0f;
        _scMod.CamXSens   = 2.0f;
        _scMod.CamYSens   = 2.0f;
        _scMod.Volume     = -45.0f;
        _scMod.VSync      = false;
        _scMod.Language   = Languages[0];
        UpdateSettings(_scMod);
    }

    public void SetPresetSettings(XB.SettingsPreset preset) {
        _scMod.SetAllFromSettingsContainer(SC);
        switch (preset) {
            case XB.SettingsPreset.Minimum: {
                _scMod.MSAASel        = MSAA[0];
                _scMod.SSAASel        = SSAA[0];
                _scMod.TAA            = false;
                _scMod.Debanding      = false;
                _scMod.ShadowSize     = ShadowSizes[0];
                _scMod.ShadowFilter   = ShadowFilters[0];
                _scMod.ShadowDistance = 50;
                _scMod.LODSel         = LOD[0];
                _scMod.SSAOSel        = SSAO[0];
                _scMod.SSAOHalf       = true;
                _scMod.SSILSel        = SSIL[0];
                _scMod.SSILHalf       = true;
                _scMod.SSR            = false;
            } break;
            case XB.SettingsPreset.Default: {
                _scMod.MSAASel        = MSAA[2];
                _scMod.SSAASel        = SSAA[1];
                _scMod.TAA            = false;
                _scMod.Debanding      = true;
                _scMod.ShadowSize     = ShadowSizes[2];
                _scMod.ShadowFilter   = ShadowFilters[3];
                _scMod.ShadowDistance = 150;
                _scMod.LODSel         = LOD[1];
                _scMod.SSAOSel        = SSAO[2];
                _scMod.SSAOHalf       = true;
                _scMod.SSILSel        = SSIL[2];
                _scMod.SSILHalf       = true;
                _scMod.SSR            = true;
            } break;
            case XB.SettingsPreset.Maximum: {
                _scMod.MSAASel        = MSAA[3];
                _scMod.SSAASel        = SSAA[1];
                _scMod.TAA            = true;
                _scMod.Debanding      = true;
                _scMod.ShadowSize     = ShadowSizes[3];
                _scMod.ShadowFilter   = ShadowFilters[5];
                _scMod.ShadowDistance = 250;
                _scMod.LODSel         = LOD[3];
                _scMod.SSAOSel        = SSAO[3];
                _scMod.SSAOHalf       = false;
                _scMod.SSILSel        = SSIL[3];
                _scMod.SSILHalf       = false;
                _scMod.SSR            = true;
            } break;
        }
        UpdateSettings(_scMod);
    }

    public bool ValidateSettingsCode(string code) {
        if (code.Length != (_setCodeLengthL + _setCodeLengthR)) {
            return false;
        }
        for (int i = 0; i < (_setCodeLengthL + _setCodeLengthR); i++) {
            if (code[i] != '0' && code[i] != '1') {
                return false;
            }
        }
        return true;
    }

    // _setCode is a string representing two bitfields appended.
    // The purpose is to allow reading in settings quickly without the need for a settings file.
    // Internally the code is split in two, as ulong is the largest datatype available with 64 bits
    // and more are needed to store all settings values
    // the right side of the code (XB.AData.SetcodeLengthR bits) is used to store booleans and array
    // values, the left side has the floating point values stored one after another
    public void SettingsCodeFromSettings(Godot.LineEdit leSetCode) {
        ulong codeR = 0;
        ulong codeL = 0;

        // right side (booleans and arrays)
        // booleans (11 bits)
        if (SC.ShowGuides) { codeR |= _sPos["SGuide"]; }
        if (SC.ShowFps)    { codeR |= _sPos["SFps"];   }
        if (SC.BlockGrid)  { codeR |= _sPos["SBlock"]; }
        if (SC.QTreeVis)   { codeR |= _sPos["SQTree"]; }
        if (SC.FullScreen) { codeR |= _sPos["Full"];   }
        if (SC.VSync)      { codeR |= _sPos["VSync"];  }
        if (SC.TAA)        { codeR |= _sPos["TAA"];    }
        if (SC.Debanding)  { codeR |= _sPos["Deba"];   }
        if (SC.SSAOHalf)   { codeR |= _sPos["SSAOH"];  }
        if (SC.SSILHalf)   { codeR |= _sPos["SSILH"];  }
        if (SC.SSR)        { codeR |= _sPos["SSR"];    }
        // arrays (36 bits)
        if (SC.Fps == FpsOptions[0])             { codeR |= _sPos["Fps0"];   }
        if (SC.Fps == FpsOptions[1])             { codeR |= _sPos["Fps1"];   }
        if (SC.Fps == FpsOptions[2])             { codeR |= _sPos["Fps2"];   }
        if (SC.Fps == FpsOptions[3])             { codeR |= _sPos["Fps3"];   }
        if (SC.Language == Languages[0])         { codeR |= _sPos["Lang"];   }
        if (SC.SSAASel == SSAA[0])               { codeR |= _sPos["SSAA"];   }
        if (SC.MSAASel == MSAA[0])               { codeR |= _sPos["MSAA0"];  }
        if (SC.MSAASel == MSAA[1])               { codeR |= _sPos["MSAA1"];  }
        if (SC.MSAASel == MSAA[2])               { codeR |= _sPos["MSAA2"];  }
        if (SC.MSAASel == MSAA[3])               { codeR |= _sPos["MSAA3"];  }
        if (SC.ShadowSize == ShadowSizes[0])     { codeR |= _sPos["SSize0"]; }
        if (SC.ShadowSize == ShadowSizes[1])     { codeR |= _sPos["SSize1"]; }
        if (SC.ShadowSize == ShadowSizes[2])     { codeR |= _sPos["SSize2"]; }
        if (SC.ShadowSize == ShadowSizes[3])     { codeR |= _sPos["SSize3"]; }
        if (SC.ShadowFilter == ShadowFilters[0]) { codeR |= _sPos["SFilt0"]; }
        if (SC.ShadowFilter == ShadowFilters[1]) { codeR |= _sPos["SFilt1"]; }
        if (SC.ShadowFilter == ShadowFilters[2]) { codeR |= _sPos["SFilt2"]; }
        if (SC.ShadowFilter == ShadowFilters[3]) { codeR |= _sPos["SFilt3"]; }
        if (SC.ShadowFilter == ShadowFilters[4]) { codeR |= _sPos["SFilt4"]; }
        if (SC.ShadowFilter == ShadowFilters[5]) { codeR |= _sPos["SFilt5"]; }
        if (SC.LODSel == LOD[0])                 { codeR |= _sPos["LODS0"];  }
        if (SC.LODSel == LOD[1])                 { codeR |= _sPos["LODS1"];  }
        if (SC.LODSel == LOD[2])                 { codeR |= _sPos["LODS2"];  }
        if (SC.SSAOSel == SSAO[0])               { codeR |= _sPos["SSAO0"];  }
        if (SC.SSAOSel == SSAO[1])               { codeR |= _sPos["SSAO1"];  }
        if (SC.SSAOSel == SSAO[2])               { codeR |= _sPos["SSAO2"];  }
        if (SC.SSAOSel == SSAO[3])               { codeR |= _sPos["SSAO3"];  }
        if (SC.SSILSel == SSIL[0])               { codeR |= _sPos["SSIL0"];  }
        if (SC.SSILSel == SSIL[1])               { codeR |= _sPos["SSIL1"];  }
        if (SC.SSILSel == SSIL[2])               { codeR |= _sPos["SSIL2"];  }
        if (SC.SSILSel == SSIL[3])               { codeR |= _sPos["SSIL3"];  }
        if (SC.Resolution == _resStrings[0])     { codeR |= _sPos["Res0"];   }
        if (SC.Resolution == _resStrings[1])     { codeR |= _sPos["Res1"];   }
        if (SC.Resolution == _resStrings[2])     { codeR |= _sPos["Res2"];   }
        if (SC.Resolution == _resStrings[3])     { codeR |= _sPos["Res3"];   }
        if (SC.Resolution == _resStrings[4])     { codeR |= _sPos["Res4"];   }

        // left side (numerical values)
        // fov (64), camx, camy, volume (100), shadowdist (250) (6+7+7+7+8 = 35bits)
        codeL  = (ulong)SC.FovDef-(ulong)FovMin;
        codeL  = codeL << 7;
        codeL |= (ulong)(SC.CamXSens*_camSliderMult);
        codeL  = codeL << 7;
        codeL |= (ulong)(SC.CamYSens*_camSliderMult);
        codeL  = codeL << 7;
        codeL |= (ulong)(-SC.Volume);
        codeL  = codeL << 8;
        codeL |= (ulong)((uint)SC.ShadowDistance);

        _setCodeR = codeR;
        _setCodeL = codeL;
        leSetCode.Text =   XB.Utils.ULongToBitString(_setCodeL, _setCodeLengthL)
                         + XB.Utils.ULongToBitString(_setCodeR, _setCodeLengthR);
    }

    public void SettingsFromSettingsCode(string bitString) {
        _scMod.SetAllFromSettingsContainer(SC);

        string bitStringR = "";
        for (int i = _setCodeLengthL;
             i < _setCodeLengthR+_setCodeLengthL; i++) {
            bitStringR += bitString[i];
        }
        string bitStringL = "";
        for (int i = 0; i < _setCodeLengthL; i++) {
            bitStringL += bitString[i];
        }
        ulong codeR = XB.Utils.BitStringToULong(bitStringR, _setCodeLengthR);
        ulong codeL = XB.Utils.BitStringToULong(bitStringL, _setCodeLengthL);

        // right side (booleans and arrays)
        // booleans (11 bits)
        if ((codeR & _sPos["SGuide"]) > 0) { _scMod.ShowGuides = true;  }
        else                               { _scMod.ShowGuides = false; }
        if ((codeR & _sPos["SFps"]) > 0) { _scMod.ShowFps = true;  }
        else                             { _scMod.ShowFps = false; }
        if ((codeR & _sPos["SBlock"]) > 0) { _scMod.BlockGrid = true;  }
        else                               { _scMod.BlockGrid = false; }
        if ((codeR & _sPos["SQTree"]) > 0) { _scMod.QTreeVis = true;  }
        else                               { _scMod.QTreeVis = false; }
        if ((codeR & _sPos["Full"]) > 0) { _scMod.FullScreen = true;  }
        else                             { _scMod.FullScreen = false; }
        if ((codeR & _sPos["VSync"]) > 0) { _scMod.VSync = true;  }
        else                              { _scMod.VSync = false; }
        if ((codeR & _sPos["TAA"]) > 0) { _scMod.TAA = true;  }
        else                            { _scMod.TAA = false; }
        if ((codeR & _sPos["Deba"]) > 0) { _scMod.Debanding = true;  }
        else                             { _scMod.Debanding = false; }
        if ((codeR & _sPos["SSAOH"]) > 0) { _scMod.SSAOHalf = true;  }
        else                              { _scMod.SSAOHalf = false; }
        if ((codeR & _sPos["SSILH"]) > 0) { _scMod.SSILHalf = true;  }
        else                              { _scMod.SSILHalf = false; }
        if ((codeR & _sPos["SSR"]) > 0) { _scMod.SSR = true;  }
        else                            { _scMod.SSR = false; }
        // arrays (36 bits)
        if      ((codeR & _sPos["Fps0"]) > 0) { _scMod.Fps = FpsOptions[0]; }
        else if ((codeR & _sPos["Fps1"]) > 0) { _scMod.Fps = FpsOptions[1]; }
        else if ((codeR & _sPos["Fps2"]) > 0) { _scMod.Fps = FpsOptions[2]; }
        else if ((codeR & _sPos["Fps3"]) > 0) { _scMod.Fps = FpsOptions[3]; }
        if      ((codeR & _sPos["Lang"]) > 0) { _scMod.Language = Languages[0]; }
        else                                  { _scMod.Language = Languages[1]; }
        if      ((codeR & _sPos["SSAA"]) > 0) { _scMod.SSAASel = SSAA[0]; }
        else                                  { _scMod.SSAASel = SSAA[1]; }
        if      ((codeR & _sPos["MSAA0"]) > 0) { _scMod.MSAASel = MSAA[0]; }
        else if ((codeR & _sPos["MSAA1"]) > 0) { _scMod.MSAASel = MSAA[1]; }
        else if ((codeR & _sPos["MSAA2"]) > 0) { _scMod.MSAASel = MSAA[2]; }
        else if ((codeR & _sPos["MSAA3"]) > 0) { _scMod.MSAASel = MSAA[3]; }
        if      ((codeR & _sPos["SSize0"]) > 0) { _scMod.ShadowSize = ShadowSizes[0]; }
        else if ((codeR & _sPos["SSize1"]) > 0) { _scMod.ShadowSize = ShadowSizes[1]; }
        else if ((codeR & _sPos["SSize2"]) > 0) { _scMod.ShadowSize = ShadowSizes[2]; }
        else if ((codeR & _sPos["SSize3"]) > 0) { _scMod.ShadowSize = ShadowSizes[3]; }
        if      ((codeR & _sPos["SFilt0"]) > 0) { _scMod.ShadowFilter = ShadowFilters[0]; }
        else if ((codeR & _sPos["SFilt1"]) > 0) { _scMod.ShadowFilter = ShadowFilters[1]; }
        else if ((codeR & _sPos["SFilt2"]) > 0) { _scMod.ShadowFilter = ShadowFilters[2]; }
        else if ((codeR & _sPos["SFilt3"]) > 0) { _scMod.ShadowFilter = ShadowFilters[3]; }
        else if ((codeR & _sPos["SFilt4"]) > 0) { _scMod.ShadowFilter = ShadowFilters[4]; }
        else if ((codeR & _sPos["SFilt5"]) > 0) { _scMod.ShadowFilter = ShadowFilters[5]; }
        if      ((codeR & _sPos["LODS0"]) > 0) { _scMod.LODSel = LOD[0]; }
        else if ((codeR & _sPos["LODS1"]) > 0) { _scMod.LODSel = LOD[1]; }
        else if ((codeR & _sPos["LODS2"]) > 0) { _scMod.LODSel = LOD[2]; }
        if      ((codeR & _sPos["SSAO0"]) > 0) { _scMod.SSAOSel = SSAO[0]; }
        else if ((codeR & _sPos["SSAO1"]) > 0) { _scMod.SSAOSel = SSAO[1]; }
        else if ((codeR & _sPos["SSAO2"]) > 0) { _scMod.SSAOSel = SSAO[2]; }
        else if ((codeR & _sPos["SSAO3"]) > 0) { _scMod.SSAOSel = SSAO[3]; }
        if      ((codeR & _sPos["SSIL0"]) > 0) { _scMod.SSILSel = SSIL[0]; }
        else if ((codeR & _sPos["SSIL1"]) > 0) { _scMod.SSILSel = SSIL[1]; }
        else if ((codeR & _sPos["SSIL2"]) > 0) { _scMod.SSILSel = SSIL[2]; }
        else if ((codeR & _sPos["SSIL3"]) > 0) { _scMod.SSILSel = SSIL[3]; }
        if      ((codeR & _sPos["Res0"]) > 0) { _scMod.Resolution = _resStrings[0]; }
        else if ((codeR & _sPos["Res1"]) > 0) { _scMod.Resolution = _resStrings[1]; }
        else if ((codeR & _sPos["Res2"]) > 0) { _scMod.Resolution = _resStrings[2]; }
        else if ((codeR & _sPos["Res3"]) > 0) { _scMod.Resolution = _resStrings[3]; }
        else if ((codeR & _sPos["Res4"]) > 0) { _scMod.Resolution = _resStrings[4]; }

        // left side (numerical values)
        ulong temp = codeL & _sFov;
        temp = temp >> (7+7+7+8);
        _scMod.FovDef = (float)temp + FovMin;
        temp = codeL & _sCamX;
        temp = temp >> (7+7+8);
        _scMod.CamXSens = ((float)temp)/_camSliderMult;
        temp = codeL & _sCamY;
        temp = temp >> (7+8);
        _scMod.CamXSens = ((float)temp)/_camSliderMult;
        temp = codeL & _sVol;
        temp = temp >> (8);
        _scMod.Volume = -((float)temp);
        temp = codeL & _sShD;
        _scMod.ShadowDistance = (int)temp;

        _setCodeL = codeL;
        _setCodeR = codeR;

        UpdateSettings(_scMod);
    }

    public void UpdateSettingsTabs(
            Godot.Slider slCamHor, Godot.Label lbCamHor, Godot.Slider slCamVer, Godot.Label lbCamVer,
            Godot.Slider slFov, Godot.Label lbCamFov, Godot.Slider slFrame, Godot.Label lbFrame,
            Godot.OptionButton obRes, Godot.OptionButton obMode,
            Godot.Button cbFps, Godot.Button cbGuides, Godot.Button cbVSync, 
            Godot.Button cbBlock, Godot.Button cbQTVis,
            Godot.OptionButton obMSAA, Godot.OptionButton obSSAA,
            Godot.Button cbTAA, Godot.Button cbDebanding,
            Godot.Label lbShdwSize, Godot.Slider slShdwSize, Godot.OptionButton obShdwFilter,
            Godot.Label lbShdwDist, Godot.Slider slShdwDist,
            Godot.Label lbLOD, Godot.Slider slLOD,
            Godot.OptionButton obSSAO, Godot.Button cbSSAOHalf,
            Godot.OptionButton obSSIL, Godot.Button cbSSILHalf,
            Godot.Button cbSSR, Godot.Slider slVolume, Godot.Label lbVolume, Godot.OptionButton obLang) {
        slCamHor.Value   = SC.CamXSens*_camSliderMult;
        lbCamHor.Text    = slCamHor.Value.ToString();
        slCamVer.Value   = SC.CamYSens*_camSliderMult;
        lbCamVer.Text    = slCamVer.Value.ToString();
        slFov.Value      = SC.FovDef;
        lbCamFov.Text    = slFov.Value.ToString() + "mm";
        slVolume.Value   = SC.Volume;
        lbVolume.Text    = slVolume.Value.ToString() + "dB";
        slShdwDist.Value = SC.ShadowDistance;
        lbShdwDist.Text  = slShdwDist.Value.ToString() + " m";
        switch (SC.Fps) {
            case 30:  { slFrame.Value = 0; lbFrame.Text = "30";                              break; }
            case 60:  { slFrame.Value = 1; lbFrame.Text = "60";                              break; }
            case 120: { slFrame.Value = 2; lbFrame.Text = "120";                             break; }
            case 0:   { slFrame.Value = 3; lbFrame.Text = XB.AData.MainRoot.Tr("UNLIMITED"); break; }
        }
        switch (SC.ShadowSize) {
            case 512:  { slShdwSize.Value = 0; lbShdwSize.Text = "512 px";  break; }
            case 1024: { slShdwSize.Value = 1; lbShdwSize.Text = "1024 px"; break; }
            case 2048: { slShdwSize.Value = 2; lbShdwSize.Text = "2048 px"; break; }
            case 4096: { slShdwSize.Value = 3; lbShdwSize.Text = "4096 px"; break; }
        }
        switch (SC.LODSel) {
            case 4.0f: { slLOD.Value = 0; lbLOD.Text = "4 px"; break; }
            case 2.0f: { slLOD.Value = 1; lbLOD.Text = "2 px"; break; }
            case 1.0f: { slLOD.Value = 2; lbLOD.Text = "1 px"; break; }
        }
        cbVSync.ButtonPressed     = SC.VSync;
        cbGuides.ButtonPressed    = SC.ShowGuides;
        cbFps.ButtonPressed       = SC.ShowFps;
        cbBlock.ButtonPressed     = SC.BlockGrid;
        cbQTVis.ButtonPressed     = SC.QTreeVis;
        cbTAA.ButtonPressed       = SC.TAA;
        cbDebanding.ButtonPressed = SC.Debanding;
        cbSSAOHalf.ButtonPressed  = SC.SSAOHalf;
        cbSSILHalf.ButtonPressed  = SC.SSILHalf;
        cbSSR.ButtonPressed       = SC.SSR;
        if (SC.FullScreen) obMode.Select(1);
        else               obMode.Select(0);
        for (int i = 0; i < Resolutions.Count; i++) {
            if (obRes.GetItemText(i) == SC.Resolution) obRes.Select(i);
        }
        for (int i = 0; i < Languages.Length; i++) {
            if (obLang.GetItemText(i) == SC.Language) obLang.Select(i);
        }
        for (int i = 0; i < MSAA.Length; i++) {
            if (obMSAA.GetItemText(i) == SC.MSAASel) obMSAA.Select(i);
        }
        for (int i = 0; i < SSAA.Length; i++) {
            if (obSSAA.GetItemText(i) == SC.SSAASel) obSSAA.Select(i);
        }
        for (int i = 0; i < ShadowFilters.Length; i++) {
            if (obShdwFilter.GetItemText(i) == SC.ShadowFilter) obShdwFilter.Select(i);
        }
        for (int i = 0; i < SSAO.Length; i++) {
            if (obSSAO.GetItemText(i) == SC.SSAOSel) obSSAO.Select(i);
        }
        for (int i = 0; i < SSIL.Length; i++) {
            if (obSSIL.GetItemText(i) == SC.SSILSel) obSSIL.Select(i);
        }
    }

    public void UpdateSliders(Godot.Slider slFrame, Godot.Label lbFrame,
                              Godot.Slider slShdwSize, Godot.Label lbShdwSize,
                              Godot.Slider slLOD, Godot.Label lbLOD           ) {
        _scMod.SetAllFromSettingsContainer(SC);
        switch (slFrame.Value) {
            case 0: _scMod.Fps = 30;  lbFrame.Text = "30";                              break;
            case 1: _scMod.Fps = 60;  lbFrame.Text = "60";                              break;
            case 2: _scMod.Fps = 120; lbFrame.Text = "120";                             break;
            case 3: _scMod.Fps = 0;   lbFrame.Text = XB.AData.MainRoot.Tr("UNLIMITED"); break;
        }
        switch (slShdwSize.Value) {
            case 0: _scMod.ShadowSize = 512;  lbShdwSize.Text = "512 px";  break;
            case 1: _scMod.ShadowSize = 1024; lbShdwSize.Text = "1024 px"; break;
            case 2: _scMod.ShadowSize = 2048; lbShdwSize.Text = "2048 px"; break;
            case 3: _scMod.ShadowSize = 4096; lbShdwSize.Text = "4096 px"; break;
        }
        switch (slLOD.Value) {
            case 0: _scMod.LODSel = 4.0f; lbLOD.Text = "4 px"; break;
            case 1: _scMod.LODSel = 2.0f; lbLOD.Text = "2 px"; break;
            case 2: _scMod.LODSel = 1.0f; lbLOD.Text = "1 px"; break;
        }
        UpdateSettings(_scMod);
    }

    public string ToggleTAA() {
        _scMod.SetAllFromSettingsContainer(SC);
        _scMod.TAA = !SC.TAA;
        UpdateSettings(_scMod);
        if (!SC.TAA) { return XB.AData.MainRoot.Tr("TURNED_TAA_OFF"); }
        else         { return XB.AData.MainRoot.Tr("TURNED_TAA_ON");  }
    }

    public string ToggleDebanding() {
        _scMod.SetAllFromSettingsContainer(SC);
        _scMod.Debanding = !SC.Debanding;
        UpdateSettings(_scMod);
        if (!SC.Debanding) { return XB.AData.MainRoot.Tr("TURNED_DEBANDING_OFF"); }
        else               { return XB.AData.MainRoot.Tr("TURNED_DEBANDING_ON");  }
    }

    public string ToggleSSAOHalf() {
        _scMod.SetAllFromSettingsContainer(SC);
        _scMod.SSAOHalf = !SC.SSAOHalf;
        UpdateSettings(_scMod);
        if (!SC.SSAOHalf) { return XB.AData.MainRoot.Tr("TURNED_SSAOHALF_OFF"); }
        else              { return XB.AData.MainRoot.Tr("TURNED_SSAOHALF_ON");  }
    }

    public string ToggleSSILHalf() {
        _scMod.SetAllFromSettingsContainer(SC);
        _scMod.SSILHalf = !SC.SSILHalf;
        UpdateSettings(_scMod);
        if (!SC.SSILHalf) { return XB.AData.MainRoot.Tr("TURNED_SSILHALF_OFF"); }
        else              { return XB.AData.MainRoot.Tr("TURNED_SSILHALF_ON");  }
    }

    public string ToggleSSR() {
        _scMod.SetAllFromSettingsContainer(SC);
        _scMod.SSR = !SC.SSR;
        UpdateSettings(_scMod);
        if (!SC.SSR) { return XB.AData.MainRoot.Tr("TURNED_SSR_OFF"); }
        else         { return XB.AData.MainRoot.Tr("TURNED_SSR_ON");  }
    }

    public string ToggleShowFPS() {
        _scMod.SetAllFromSettingsContainer(SC);
        _scMod.ShowFps = !SC.ShowFps;
        UpdateSettings(_scMod);
        if (SC.ShowFps) { return XB.AData.MainRoot.Tr("TURNED_FPS_ON");  }
        else            { return XB.AData.MainRoot.Tr("TURNED_FPS_OFF"); }
    }

    public string ToggleShowGuides() {
        _scMod.SetAllFromSettingsContainer(SC);
        _scMod.ShowGuides = !SC.ShowGuides;
        UpdateSettings(_scMod);
        if (SC.ShowGuides) { return XB.AData.MainRoot.Tr("TURNED_GUIDES_ON");  }
        else               { return XB.AData.MainRoot.Tr("TURNED_GUIDES_OFF"); }
    }

    public string ToggleVSync() {
        _scMod.SetAllFromSettingsContainer(SC);
        _scMod.VSync = !SC.VSync;
        UpdateSettings(_scMod);
        if (SC.VSync) { return XB.AData.MainRoot.Tr("TURNED_VSYNC_ON"); }
        else          { return XB.AData.MainRoot.Tr("TURNED_VSYNC_OFF"); }
    }

    public string ToggleBlockGrid() {
        _scMod.SetAllFromSettingsContainer(SC);
        _scMod.BlockGrid = !SC.BlockGrid;
        UpdateSettings(_scMod);
        if (SC.BlockGrid) { return XB.AData.MainRoot.Tr("TURNED_BLOCKGRID_ON"); }
        else              { return XB.AData.MainRoot.Tr("TURNED_BLOCKGRID_OFF"); }
    }

    public string ToggleQuadTreeVis() {
        _scMod.SetAllFromSettingsContainer(SC);
        _scMod.QTreeVis = !SC.QTreeVis;
        UpdateSettings(_scMod);
        if (SC.QTreeVis) { return XB.AData.MainRoot.Tr("TURNED_QTREEVIS_ON"); }
        else             { return XB.AData.MainRoot.Tr("TURNED_QTREEVIS_OFF"); }
    }

    public string ChangeShadowDistance(Godot.Slider slShdwDist) {
        _scMod.SetAllFromSettingsContainer(SC);
        _scMod.ShadowDistance = (int)slShdwDist.Value;
        UpdateSettings(_scMod);
        return XB.AData.MainRoot.Tr("CHANGED_SHADOWDIST");
    }

    public string ChangeFov(Godot.Slider slFov) {
        _scMod.SetAllFromSettingsContainer(SC);
        _scMod.FovDef = (float)slFov.Value;
        UpdateSettings(_scMod);
        return XB.AData.MainRoot.Tr("CHANGED_FOV");
    }

    public string ChangeSensitivityHorizontal(Godot.Slider slCamHor) {
        _scMod.SetAllFromSettingsContainer(SC);
        _scMod.CamXSens = ((float)slCamHor.Value)/_camSliderMult;
        UpdateSettings(_scMod);
        return XB.AData.MainRoot.Tr("CHANGED_CAM_HOR");
    }

    public string ChangeSensitivityVertical(Godot.Slider slCamVer) {
        _scMod.SetAllFromSettingsContainer(SC);
        _scMod.CamYSens = ((float)slCamVer.Value)/_camSliderMult;
        UpdateSettings(_scMod);
        return XB.AData.MainRoot.Tr("CHANGED_CAM_VER");
    }

    public string ChangeVolume(Godot.Slider slVolume) {
        _scMod.SetAllFromSettingsContainer(SC);
        _scMod.Volume = (float)slVolume.Value;
        UpdateSettings(_scMod);
        return XB.AData.MainRoot.Tr("CHANGED_VOLUME");
    }

    public string ChangeLanguage(Godot.OptionButton obLang) {
        _scMod.SetAllFromSettingsContainer(SC);
        _scMod.Language = obLang.GetItemText(obLang.GetSelectedId());
        UpdateSettings(_scMod);
        return XB.AData.MainRoot.Tr("CHANGED_LANGUAGE");
    }
    
    public void ChangeResolution(Godot.OptionButton obRes) {
        _scMod.SetAllFromSettingsContainer(SC);
        _scMod.Resolution = obRes.GetItemText(obRes.GetSelectedId());
        UpdateSettings(_scMod);
    }

    public void ChangeMode(Godot.OptionButton obMode) {
        _scMod.SetAllFromSettingsContainer(SC);
        if (obMode.GetSelectedId() == 1) { _scMod.FullScreen = true;  }
        else                             { _scMod.FullScreen = false; }
        UpdateSettings(_scMod);
    }

    public void ChangeMSAA(Godot.OptionButton obMSAA) {
        _scMod.SetAllFromSettingsContainer(SC);
        _scMod.MSAASel = obMSAA.GetItemText(obMSAA.GetSelectedId());
        UpdateSettings(_scMod);
    }

    public void ChangeSSAA(Godot.OptionButton obSSAA) {
        _scMod.SetAllFromSettingsContainer(SC);
        _scMod.SSAASel = obSSAA.GetItemText(obSSAA.GetSelectedId());
        UpdateSettings(_scMod);
    }

    public void ChangeSSIL(Godot.OptionButton obSSIL) {
        _scMod.SetAllFromSettingsContainer(SC);
        _scMod.SSILSel = obSSIL.GetItemText(obSSIL.GetSelectedId());
        UpdateSettings(_scMod);
    }

    public void ChangeShadowFilter(Godot.OptionButton obShdwFilter) {
        _scMod.SetAllFromSettingsContainer(SC);
        _scMod.ShadowFilter = obShdwFilter.GetItemText(obShdwFilter.GetSelectedId());
        UpdateSettings(_scMod);
    }

    public void ChangeSSAO(Godot.OptionButton obSSAO) {
        _scMod.SetAllFromSettingsContainer(SC);
        _scMod.SSAOSel = obSSAO.GetItemText(obSSAO.GetSelectedId());
        UpdateSettings(_scMod);
    }

    public string PresetSettings(XB.SettingsPreset preset) {
        SetPresetSettings(preset);
        Godot.Input.MouseMode = Godot.Input.MouseModeEnum.Visible;
        string msg = "";
        switch (preset) {
            case XB.SettingsPreset.Minimum: { msg = "SETTINGS_MINIMUM"; break; }
            case XB.SettingsPreset.Default: { msg = "SETTINGS_DEFAULT"; break; }
            case XB.SettingsPreset.Maximum: { msg = "SETTINGS_MAXIMUM"; break; }
        }
        return XB.AData.MainRoot.Tr(msg);
    }

    public string ApplicationDefaults() {
        SetApplicationDefaults();
        Godot.Input.MouseMode = Godot.Input.MouseModeEnum.Visible;
        return XB.AData.MainRoot.Tr("APPLICATION_DEFAULT");
    }

    //NOTE[ALEX]: drop down dialogs are a bit too short with apparently no option to change that
    //            adding separators at the bottom makes all entries show up without scrolling
    //            when that should be the case
    public void AddSeparators(Godot.OptionButton ob) {
        ob.AddSeparator();
        ob.AddSeparator();
        ob.AddSeparator();
        ob.AddSeparator();
        ob.AddSeparator();
        ob.AddSeparator();
        ob.AddSeparator();
        ob.AddSeparator();
    }
}
} // namespace close
