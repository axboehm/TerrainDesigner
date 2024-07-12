namespace XB { // namespace open
using SysCG = System.Collections.Generic;
public partial class Settings {
    public  static float CamSliderMult = 25.0f;
    private static SysCG.Dictionary<string, ulong> sPos =
        new SysCG.Dictionary<string, ulong>() {
                {"Cont",   (ulong)1 << 0 },
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
            };
    private static ulong sFov  = (ulong)63  << 7+7+7+8+0;
    private static ulong sCamX = (ulong)127 <<   7+7+8+0;
    private static ulong sCamY = (ulong)127 <<     7+8+0;
    private static ulong sVol  = (ulong)127 <<       8+0;
    private static ulong sShD  = (ulong)255 <<         0;

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
        slCamHor.Value = XB.AData.CamXSens*CamSliderMult;
        lbCamHor.Text  = slCamHor.Value.ToString();
        slCamVer.Value = XB.AData.CamYSens*CamSliderMult;
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
        XB.AData.CamXSens = ((float)slCamHor.Value)/CamSliderMult;
        return XB.AData.TR.Tr("CHANGED_CAM_HOR");
    }

    public static string ChangeSensitivityVertical(Godot.Slider slCamVer) {
        XB.AData.CamYSens = ((float)slCamVer.Value)/CamSliderMult;
        return XB.AData.TR.Tr("CHANGED_CAM_VER");
    }

    public static string ChangeVolume(Godot.Slider slVolume) {
        XB.AData.Volume = (float)slVolume.Value;
        return XB.AData.TR.Tr("CHANGED_VOLUME");
    }

    public static string ChangeLanguage(Godot.OptionButton obLang) {
        XB.AData.Language = obLang.GetItemText(obLang.GetSelectedId());
        return XB.AData.TR.Tr("CHANGED_LANGUAGE");
    }

    public static string PresetSettings(XB.SettingsPreset preset) {
        XB.PersistData.SetPresetSettings(preset);
        Godot.Input.MouseMode = Godot.Input.MouseModeEnum.Visible;
        string msg = "";
        switch (preset) {
            case XB.SettingsPreset.Minimum: { msg = "SETTINGS_MINIMUM"; break; }
            case XB.SettingsPreset.Default: { msg = "SETTINGS_DEFAULT"; break; }
            case XB.SettingsPreset.Maximum: { msg = "SETTINGS_MAXIMUM"; break; }
        }
        return XB.AData.TR.Tr(msg);
    }

    public static string ApplicationDefaults() {
        XB.PersistData.SetApplicationDefaults();
        Godot.Input.MouseMode = Godot.Input.MouseModeEnum.Visible;
        return XB.AData.TR.Tr("APPLICATION_DEFAULT");
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

    // SetCode is a string representing two bitfields appended.
    // The purpose is to allow reading in settings quickly without the need for a settings file.
    // Internally the code is split in two, as ulong is the largest datatype available with 64 bits
    // and more are needed to store all settings values
    // the right side of the code (XB.AData.SetcodeLengthR bits) is used to store booleans and array
    // values, the left side has the floating point values stored one after another
    public static void SettingsCodeFromSettings(Godot.LineEdit leSetCode) {
        ulong codeR = 0;
        ulong codeL = 0;

        // right side (booleans and arrays)
        // booleans (9 bits)
        if (XB.AData.Controller) { codeR |= sPos["Cont"];  }
        if (XB.AData.ShowFps)    { codeR |= sPos["SFps"];  }
        if (XB.AData.FullScreen) { codeR |= sPos["Full"];  }
        if (XB.AData.VSync)      { codeR |= sPos["VSync"]; }
        if (XB.AData.TAA)        { codeR |= sPos["TAA"];   }
        if (XB.AData.Debanding)  { codeR |= sPos["Deba"];  }
        if (XB.AData.SSAOHalf)   { codeR |= sPos["SSAOH"]; }
        if (XB.AData.SSILHalf)   { codeR |= sPos["SSILH"]; }
        if (XB.AData.SSR)        { codeR |= sPos["SSR"];   }
        // arrays (36 bits)
        if (XB.AData.Fps == XB.AData.FpsOptions[0])             { codeR |= sPos["Fps0"];   }
        if (XB.AData.Fps == XB.AData.FpsOptions[1])             { codeR |= sPos["Fps1"];   }
        if (XB.AData.Fps == XB.AData.FpsOptions[2])             { codeR |= sPos["Fps2"];   }
        if (XB.AData.Fps == XB.AData.FpsOptions[3])             { codeR |= sPos["Fps3"];   }
        if (XB.AData.Language == XB.AData.Languages[0])         { codeR |= sPos["Lang"];   }
        if (XB.AData.SSAASel == XB.AData.SSAA[0])               { codeR |= sPos["SSAA"];   }
        if (XB.AData.MSAASel == XB.AData.MSAA[0])               { codeR |= sPos["MSAA0"];  }
        if (XB.AData.MSAASel == XB.AData.MSAA[1])               { codeR |= sPos["MSAA1"];  }
        if (XB.AData.MSAASel == XB.AData.MSAA[2])               { codeR |= sPos["MSAA2"];  }
        if (XB.AData.MSAASel == XB.AData.MSAA[3])               { codeR |= sPos["MSAA3"];  }
        if (XB.AData.ShadowSize == XB.AData.ShadowSizes[0])     { codeR |= sPos["SSize0"]; }
        if (XB.AData.ShadowSize == XB.AData.ShadowSizes[1])     { codeR |= sPos["SSize1"]; }
        if (XB.AData.ShadowSize == XB.AData.ShadowSizes[2])     { codeR |= sPos["SSize2"]; }
        if (XB.AData.ShadowSize == XB.AData.ShadowSizes[3])     { codeR |= sPos["SSize3"]; }
        if (XB.AData.ShadowFilter == XB.AData.ShadowFilters[0]) { codeR |= sPos["SFilt0"]; }
        if (XB.AData.ShadowFilter == XB.AData.ShadowFilters[1]) { codeR |= sPos["SFilt1"]; }
        if (XB.AData.ShadowFilter == XB.AData.ShadowFilters[2]) { codeR |= sPos["SFilt2"]; }
        if (XB.AData.ShadowFilter == XB.AData.ShadowFilters[3]) { codeR |= sPos["SFilt3"]; }
        if (XB.AData.ShadowFilter == XB.AData.ShadowFilters[4]) { codeR |= sPos["SFilt4"]; }
        if (XB.AData.ShadowFilter == XB.AData.ShadowFilters[5]) { codeR |= sPos["SFilt5"]; }
        if (XB.AData.LODSel == XB.AData.LOD[0])                 { codeR |= sPos["LODS0"];  }
        if (XB.AData.LODSel == XB.AData.LOD[1])                 { codeR |= sPos["LODS1"];  }
        if (XB.AData.LODSel == XB.AData.LOD[2])                 { codeR |= sPos["LODS2"];  }
        if (XB.AData.SSAOSel == XB.AData.SSAO[0])               { codeR |= sPos["SSAO0"];  }
        if (XB.AData.SSAOSel == XB.AData.SSAO[1])               { codeR |= sPos["SSAO1"];  }
        if (XB.AData.SSAOSel == XB.AData.SSAO[2])               { codeR |= sPos["SSAO2"];  }
        if (XB.AData.SSAOSel == XB.AData.SSAO[3])               { codeR |= sPos["SSAO3"];  }
        if (XB.AData.SSILSel == XB.AData.SSIL[0])               { codeR |= sPos["SSIL0"];  }
        if (XB.AData.SSILSel == XB.AData.SSIL[1])               { codeR |= sPos["SSIL1"];  }
        if (XB.AData.SSILSel == XB.AData.SSIL[2])               { codeR |= sPos["SSIL2"];  }
        if (XB.AData.SSILSel == XB.AData.SSIL[3])               { codeR |= sPos["SSIL3"];  }
        if (XB.AData.Resolution == XB.AData.ResStrings[0])      { codeR |= sPos["Res0"];   }
        if (XB.AData.Resolution == XB.AData.ResStrings[1])      { codeR |= sPos["Res1"];   }
        if (XB.AData.Resolution == XB.AData.ResStrings[2])      { codeR |= sPos["Res2"];   }
        if (XB.AData.Resolution == XB.AData.ResStrings[3])      { codeR |= sPos["Res3"];   }
        if (XB.AData.Resolution == XB.AData.ResStrings[4])      { codeR |= sPos["Res4"];   }

        // left side (numerical values)
        // fov (64), camx, camy, volume (100), shadowdist (250) (6+7+7+7+8 = 35bits)
        codeL  = (ulong)XB.AData.FovDef-(ulong)XB.AData.FovMin;
        codeL  = codeL << 7;
        codeL |= (ulong)(XB.AData.CamXSens*CamSliderMult);
        codeL  = codeL << 7;
        codeL |= (ulong)(XB.AData.CamYSens*CamSliderMult);
        codeL  = codeL << 7;
        codeL |= (ulong)(-XB.AData.Volume);
        codeL  = codeL << 8;
        codeL |= (ulong)((uint)XB.AData.ShadowDistance);

        XB.AData.SetCodeR = codeR;
        XB.AData.SetCodeL = codeL;
        leSetCode.Text =   XB.Utils.ULongToBitString(XB.AData.SetCodeL, XB.AData.SetCodeLengthL)
                         //+ " " // visualization
                         + XB.Utils.ULongToBitString(XB.AData.SetCodeR, XB.AData.SetCodeLengthR);
    }

    public static void SettingsFromSettingsCode(string bitString) {
        string bitStringR = "";
        for (int i = XB.AData.SetCodeLengthL;
             i < XB.AData.SetCodeLengthR+XB.AData.SetCodeLengthL; i++) {
            bitStringR += bitString[i];
        }
        string bitStringL = "";
        for (int i = 0; i < XB.AData.SetCodeLengthL; i++) {
            bitStringL += bitString[i];
        }
        ulong codeR = XB.Utils.BitStringToULong(bitStringR, XB.AData.SetCodeLengthR);
        ulong codeL = XB.Utils.BitStringToULong(bitStringL, XB.AData.SetCodeLengthL);

        // right side (booleans and arrays)
        // booleans (9 bits)
        if ((codeR & sPos["Cont"]) > 0)  { XB.AData.Controller = true;  }
        else                             { XB.AData.Controller = false; }
        if ((codeR & sPos["SFps"]) > 0)  { XB.AData.ShowFps = true;  }
        else                             { XB.AData.ShowFps = false; }
        if ((codeR & sPos["Full"]) > 0)  { XB.AData.FullScreen = true;  }
        else                             { XB.AData.FullScreen = false; }
        if ((codeR & sPos["VSync"]) > 0) { XB.AData.VSync = true;  }
        else                             { XB.AData.VSync = false; }
        if ((codeR & sPos["TAA"]) > 0)   { XB.AData.TAA = true;  }
        else                             { XB.AData.TAA = false; }
        if ((codeR & sPos["Deba"]) > 0)  { XB.AData.Debanding = true;  }
        else                             { XB.AData.Debanding = false; }
        if ((codeR & sPos["SSAOH"]) > 0) { XB.AData.SSAOHalf = true;  }
        else                             { XB.AData.SSAOHalf = false; }
        if ((codeR & sPos["SSILH"]) > 0) { XB.AData.SSILHalf = true;  }
        else                             { XB.AData.SSILHalf = false; }
        if ((codeR & sPos["SSR"]) > 0)   { XB.AData.SSR = true;  }
        else                             { XB.AData.SSR = false; }
        // arrays (36 bits)
        if      ((codeR & sPos["Fps0"]) > 0)   { XB.AData.Fps = XB.AData.FpsOptions[0]; }
        else if ((codeR & sPos["Fps1"]) > 0)   { XB.AData.Fps = XB.AData.FpsOptions[1]; }
        else if ((codeR & sPos["Fps2"]) > 0)   { XB.AData.Fps = XB.AData.FpsOptions[2]; }
        else if ((codeR & sPos["Fps3"]) > 0)   { XB.AData.Fps = XB.AData.FpsOptions[3]; }
        if      ((codeR & sPos["Lang"]) > 0)   { XB.AData.Language = XB.AData.Languages[0]; }
        else                                   { XB.AData.Language = XB.AData.Languages[1]; }
        if      ((codeR & sPos["SSAA"]) > 0)   { XB.AData.SSAASel = XB.AData.SSAA[0]; }
        else                                   { XB.AData.SSAASel = XB.AData.SSAA[1]; }
        if      ((codeR & sPos["MSAA0"]) > 0)  { XB.AData.MSAASel = XB.AData.MSAA[0]; }
        else if ((codeR & sPos["MSAA1"]) > 0)  { XB.AData.MSAASel = XB.AData.MSAA[1]; }
        else if ((codeR & sPos["MSAA2"]) > 0)  { XB.AData.MSAASel = XB.AData.MSAA[2]; }
        else if ((codeR & sPos["MSAA3"]) > 0)  { XB.AData.MSAASel = XB.AData.MSAA[3]; }
        if      ((codeR & sPos["SSize0"]) > 0) { XB.AData.ShadowSize = XB.AData.ShadowSizes[0]; }
        else if ((codeR & sPos["SSize1"]) > 0) { XB.AData.ShadowSize = XB.AData.ShadowSizes[1]; }
        else if ((codeR & sPos["SSize2"]) > 0) { XB.AData.ShadowSize = XB.AData.ShadowSizes[2]; }
        else if ((codeR & sPos["SSize3"]) > 0) { XB.AData.ShadowSize = XB.AData.ShadowSizes[3]; }
        if      ((codeR & sPos["SFilt0"]) > 0) { XB.AData.ShadowFilter = XB.AData.ShadowFilters[0]; }
        else if ((codeR & sPos["SFilt1"]) > 0) { XB.AData.ShadowFilter = XB.AData.ShadowFilters[1]; }
        else if ((codeR & sPos["SFilt2"]) > 0) { XB.AData.ShadowFilter = XB.AData.ShadowFilters[2]; }
        else if ((codeR & sPos["SFilt3"]) > 0) { XB.AData.ShadowFilter = XB.AData.ShadowFilters[3]; }
        else if ((codeR & sPos["SFilt4"]) > 0) { XB.AData.ShadowFilter = XB.AData.ShadowFilters[4]; }
        else if ((codeR & sPos["SFilt5"]) > 0) { XB.AData.ShadowFilter = XB.AData.ShadowFilters[5]; }
        if      ((codeR & sPos["LODS0"]) > 0)  { XB.AData.LODSel = XB.AData.LOD[0]; }
        else if ((codeR & sPos["LODS1"]) > 0)  { XB.AData.LODSel = XB.AData.LOD[1]; }
        else if ((codeR & sPos["LODS2"]) > 0)  { XB.AData.LODSel = XB.AData.LOD[2]; }
        if      ((codeR & sPos["SSAO0"]) > 0)  { XB.AData.SSAOSel = XB.AData.SSAO[0]; }
        else if ((codeR & sPos["SSAO1"]) > 0)  { XB.AData.SSAOSel = XB.AData.SSAO[1]; }
        else if ((codeR & sPos["SSAO2"]) > 0)  { XB.AData.SSAOSel = XB.AData.SSAO[2]; }
        else if ((codeR & sPos["SSAO3"]) > 0)  { XB.AData.SSAOSel = XB.AData.SSAO[3]; }
        if      ((codeR & sPos["SSIL0"]) > 0)  { XB.AData.SSILSel = XB.AData.SSIL[0]; }
        else if ((codeR & sPos["SSIL1"]) > 0)  { XB.AData.SSILSel = XB.AData.SSIL[1]; }
        else if ((codeR & sPos["SSIL2"]) > 0)  { XB.AData.SSILSel = XB.AData.SSIL[2]; }
        else if ((codeR & sPos["SSIL3"]) > 0)  { XB.AData.SSILSel = XB.AData.SSIL[3]; }
        if      ((codeR & sPos["Res0"]) > 0)   { XB.AData.Resolution = XB.AData.ResStrings[0]; }
        else if ((codeR & sPos["Res1"]) > 0)   { XB.AData.Resolution = XB.AData.ResStrings[1]; }
        else if ((codeR & sPos["Res2"]) > 0)   { XB.AData.Resolution = XB.AData.ResStrings[2]; }
        else if ((codeR & sPos["Res3"]) > 0)   { XB.AData.Resolution = XB.AData.ResStrings[3]; }
        else if ((codeR & sPos["Res4"]) > 0)   { XB.AData.Resolution = XB.AData.ResStrings[4]; }

        // left side (numerical values)
        ulong temp = codeL & sFov;
        temp = temp >> (7+7+7+8);
        XB.PersistData.UpdateFov((float)temp + XB.AData.FovMin);
        temp = codeL & sCamX;
        temp = temp >> (7+7+8);
        XB.AData.CamXSens = ((float)temp)/CamSliderMult;
        temp = codeL & sCamY;
        temp = temp >> (7+8);
        XB.AData.CamXSens = ((float)temp)/CamSliderMult;
        temp = codeL & sVol;
        temp = temp >> (8);
        XB.AData.Volume = -((float)temp);
        temp = codeL & sShD;
        XB.AData.ShadowDistance = (int)temp;

        XB.AData.SetCodeL = codeL;
        XB.AData.SetCodeR = codeR;
    }
}
} // namespace close
