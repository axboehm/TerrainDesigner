namespace XB { // namespace open
public partial class Settings {
    public static float CamSliderMult = 25.0f;

    public static void UpdateSettingsTabs(Godot.Slider slCamHor, Godot.Label lbCamHor,
            Godot.Slider slCamVer, Godot.Label lbCamVer,
            Godot.Slider slFov, Godot.Label lbCamFov, Godot.Slider slFrame, Godot.Label lbFrame,
            Godot.OptionButton obRes, 
            Godot.OptionButton obMode, Godot.Button cbFps, Godot.Button cbVSync,
            Godot.OptionButton obMSAA, Godot.OptionButton obSSAA,
            Godot.Button cbTAA, Godot.Button cbDebanding,
            Godot.Label lbShdwSize, Godot.Slider slShdwSize, Godot.OptionButton obShdwFilter,
            Godot.Label lbShdwDist, Godot.Slider slShdwDist,
            Godot.Label lbLOD, Godot.Slider slLOD,
            Godot.OptionButton obSSAO, Godot.Button cbSSAOHalf,
            Godot.OptionButton obSSIL, Godot.Button cbSSILHalf,
            Godot.Button cbSSR,
            Godot.Slider slVolume, Godot.Label lbVolume,
            Godot.OptionButton obLang) {
        slCamHor.Value = XB.AData.CamXSens*XB.Settings.CamSliderMult;
        lbCamHor.Text  = slCamHor.Value.ToString();
        slCamVer.Value = XB.AData.CamYSens*XB.Settings.CamSliderMult;
        lbCamVer.Text  = slCamVer.Value.ToString();
        slFov.Value    = XB.AData.FovDef;
        lbCamFov.Text  = slFov.Value.ToString() + "mm";
        slVolume.Value = XB.AData.Volume;
        lbVolume.Text  = slVolume.Value.ToString() + "dB";
        slShdwDist.Value = XB.AData.ShadowDistance;
        lbShdwDist.Text  = slShdwDist.Value.ToString() + " m";
        switch (XB.AData.Fps) {
            case 30:  slFrame.Value = 0; lbFrame.Text = "30";                        break;
            case 60:  slFrame.Value = 1; lbFrame.Text = "60";                        break;
            case 120: slFrame.Value = 2; lbFrame.Text = "120";                       break;
            case 240: slFrame.Value = 3; lbFrame.Text = "240";                       break;
            case 0:   slFrame.Value = 4; lbFrame.Text = XB.AData.TR.Tr("UNLIMITED"); break;
        }
        switch (XB.AData.ShadowSize) {
            case 512:  slShdwSize.Value = 0; lbShdwSize.Text = "512 px";  break;
            case 1024: slShdwSize.Value = 1; lbShdwSize.Text = "1024 px"; break;
            case 2048: slShdwSize.Value = 2; lbShdwSize.Text = "2048 px"; break;
            case 4096: slShdwSize.Value = 3; lbShdwSize.Text = "4096 px"; break;
        }
        switch (XB.AData.LODSel) {
            case 4.0f: slLOD.Value = 0; lbLOD.Text = "4 px"; break;
            case 2.0f: slLOD.Value = 1; lbLOD.Text = "2 px"; break;
            case 1.0f: slLOD.Value = 2; lbLOD.Text = "1 px"; break;
        }
        cbVSync.ButtonPressed     = XB.AData.VSync;
        cbFps.ButtonPressed       = XB.AData.ShowFps;
        cbTAA.ButtonPressed       = XB.AData.TAA;
        cbDebanding.ButtonPressed = XB.AData.Debanding;
        cbSSAOHalf.ButtonPressed  = XB.AData.SSAOHalf;
        cbSSILHalf.ButtonPressed  = XB.AData.SSILHalf;
        cbSSR.ButtonPressed       = XB.AData.SSR;
        if (XB.AData.FullScreen) obMode.Select(1);
        else                     obMode.Select(0);
        for (int i = 0; i < XB.AData.Resolutions.Count; i++) {
            if (obRes.GetItemText(i) == XB.AData.Resolution) obRes.Select(i);
        }
        for (int i = 0; i < XB.AData.Languages.Length; i++) {
            if (obLang.GetItemText(i) == XB.AData.Language) obLang.Select(i);
        }
        for (int i = 0; i < XB.AData.MSAA.Length; i++) {
            if (obMSAA.GetItemText(i) == XB.AData.MSAASel) obMSAA.Select(i);
        }
        for (int i = 0; i < XB.AData.SSAA.Length; i++) {
            if (obSSAA.GetItemText(i) == XB.AData.SSAASel) obSSAA.Select(i);
        }
        for (int i = 0; i < XB.AData.ShadowFilters.Length; i++) {
            if (obShdwFilter.GetItemText(i) == XB.AData.ShadowFilter) obShdwFilter.Select(i);
        }
        for (int i = 0; i < XB.AData.SSAO.Length; i++) {
            if (obSSAO.GetItemText(i) == XB.AData.SSAOSel) obSSAO.Select(i);
        }
        for (int i = 0; i < XB.AData.SSIL.Length; i++) {
            if (obSSIL.GetItemText(i) == XB.AData.SSILSel) obSSIL.Select(i);
        }
    }

    public static void UpdateSliders(Godot.Slider slFrame, Godot.Label lbFrame,
                                     Godot.Slider slShdwSize, Godot.Label lbShdwSize,
                                     Godot.Slider slLOD, Godot.Label lbLOD) {
        switch (slFrame.Value) {
            case 0: XB.AData.Fps = 30;  lbFrame.Text = "30";                        break;
            case 1: XB.AData.Fps = 60;  lbFrame.Text = "60";                        break;
            case 2: XB.AData.Fps = 120; lbFrame.Text = "120";                       break;
            case 3: XB.AData.Fps = 240; lbFrame.Text = "240";                       break;
            case 4: XB.AData.Fps = 0;   lbFrame.Text = XB.AData.TR.Tr("UNLIMITED"); break;
        }
        switch (slShdwSize.Value) {
            case 0: XB.AData.ShadowSize = 512;  lbShdwSize.Text = "512 px";  break;
            case 1: XB.AData.ShadowSize = 1024; lbShdwSize.Text = "1024 px"; break;
            case 2: XB.AData.ShadowSize = 2048; lbShdwSize.Text = "2048 px"; break;
            case 3: XB.AData.ShadowSize = 4096; lbShdwSize.Text = "4096 px"; break;
        }
        switch (slLOD.Value) {
            case 0: XB.AData.LODSel = 4.0f; lbLOD.Text = "4 px"; break;
            case 1: XB.AData.LODSel = 2.0f; lbLOD.Text = "2 px"; break;
            case 2: XB.AData.LODSel = 1.0f; lbLOD.Text = "1 px"; break;
        }
    }

    public static string ApplySettings(Godot.Slider slFrame, Godot.Label lbFrame,
                                       Godot.Slider slShdwSize, Godot.Label lbShdwSize,
                                       Godot.Slider slShdwDist, Godot.Label lbShdwDist,
                                       Godot.Slider slLOD, Godot.Label lbLOD,
                                       Godot.OptionButton obRes, Godot.OptionButton obMode,
                                       Godot.OptionButton obMSAA, Godot.OptionButton obSSAA,
                                       Godot.OptionButton obShdwFilter,
                                       Godot.OptionButton obSSAO, Godot.OptionButton obSSIL) {
        UpdateSliders(slFrame, lbFrame, slShdwSize, lbShdwSize, slLOD, lbLOD);
        XB.AData.Resolution = obRes.GetItemText(obRes.GetSelectedId());
        if (obMode.GetSelectedId() == 1) XB.AData.FullScreen = true;
        else                             XB.AData.FullScreen = false;
        XB.AData.MSAASel      = XB.AData.MSAA[obMSAA.GetSelectedId()];
        XB.AData.SSAASel      = XB.AData.SSAA[obSSAA.GetSelectedId()];
        XB.AData.ShadowFilter = XB.AData.ShadowFilters[obShdwFilter.GetSelectedId()];
        XB.AData.SSAOSel      = XB.AData.SSAO[obSSAO.GetSelectedId()];
        XB.AData.SSILSel      = XB.AData.SSIL[obSSIL.GetSelectedId()];

        return XB.AData.TR.Tr("SETTINGS_APPLIED");
    }

    public static string ToggleTAA() {
        XB.AData.TAA = !XB.AData.TAA;
        if (!XB.AData.TAA) return XB.AData.TR.Tr("TURNED_TAA_OFF");
        else               return XB.AData.TR.Tr("TURNED_TAA_ON");
    }

    public static string ToggleDebanding() {
        XB.AData.Debanding = !XB.AData.Debanding;
        if (!XB.AData.Debanding) return XB.AData.TR.Tr("TURNED_DEBANDING_OFF");
        else                     return XB.AData.TR.Tr("TURNED_DEBANDING_ON");
    }

    public static string ToggleSSAOHalf() {
        XB.AData.SSAOHalf = !XB.AData.SSAOHalf;
        if (!XB.AData.SSAOHalf) return XB.AData.TR.Tr("TURNED_SSAOHALF_OFF");
        else                    return XB.AData.TR.Tr("TURNED_SSAOHALF_ON");
    }

    public static string ToggleSSILHalf() {
        XB.AData.SSILHalf = !XB.AData.SSILHalf;
        if (!XB.AData.SSILHalf) return XB.AData.TR.Tr("TURNED_SSILHALF_OFF");
        else                    return XB.AData.TR.Tr("TURNED_SSILHALF_ON");
    }

    public static string ToggleSSR() {
        XB.AData.SSR = !XB.AData.SSR;
        if (!XB.AData.SSR) return XB.AData.TR.Tr("TURNED_SSR_OFF");
        else               return XB.AData.TR.Tr("TURNED_SSR_ON");
    }

    public static string ToggleShowFPS() {
        XB.AData.ShowFps = !XB.AData.ShowFps;
        if (XB.AData.ShowFps) return XB.AData.TR.Tr("TURNED_FPS_OFF");
        else                  return XB.AData.TR.Tr("TURNED_FPS_ON");
    }

    public static string ToggleVSync() {
        XB.AData.VSync = !XB.AData.VSync;
        if (XB.AData.VSync) return XB.AData.TR.Tr("TURNED_VSYNC_OFF");
        else                return XB.AData.TR.Tr("TURNED_VSYNC_ON");
    }

    public static string ChangeShadowDistance(Godot.Slider slShdwDist) {
        XB.AData.ShadowDistance = (int)slShdwDist.Value;
        return XB.AData.TR.Tr("CHANGED_SHADOWDIST");
    }

    public static string ChangeFov(Godot.Slider slFov) {
        XB.PersistData.UpdateFov((float)slFov.Value);
        return XB.AData.TR.Tr("CHANGED_FOV");
    }

    public static string ChangeSensitivityHorizontal(Godot.Slider slCamHor) {
        XB.AData.CamXSens = ((float)slCamHor.Value)/XB.Settings.CamSliderMult;
        return XB.AData.TR.Tr("CHANGED_CAM_HOR");
    }

    public static string ChangeSensitivityVertical(Godot.Slider slCamVer) {
        XB.AData.CamYSens = ((float)slCamVer.Value)/XB.Settings.CamSliderMult;
        return XB.AData.TR.Tr("CHANGED_CAM_VER");
    }

    public static string ChangeVolume(Godot.Slider slVolume) {
        XB.AData.Volume = (float)slVolume.Value;
        XB.PersistData.UpdateAudio();
        return XB.AData.TR.Tr("CHANGED_VOLUME");
    }

    public static string ChangeLanguage(Godot.OptionButton obLang) {
        XB.AData.Language = obLang.GetItemText(obLang.GetSelectedId());
        XB.PersistData.UpdateLanguage();
        return XB.AData.TR.Tr("CHANGED_LANGUAGE");
    }

    public static string DefaultSettings() {
        XB.PersistData.SettingsDefault();
        Godot.Input.MouseMode = Godot.Input.MouseModeEnum.Visible;
        return XB.AData.TR.Tr("SETTINGS_DEFAULT");
    }

    public static void AddSeparators(Godot.OptionButton ob) {
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
