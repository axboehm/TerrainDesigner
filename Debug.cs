namespace XB { // namespace open
using SysCG = System.Collections.Generic;
public partial class DebugHUD : Godot.Control {
    private bool _visible    = false;
    private bool _pauseDebug = false;

    private const int    _sizeMMMax             = 512;
    private const int    _dimSpacer             = 16;
    private const int    _debugLabelSpacing     = 18; // between each line of text
    private const int    _debugLabelFontSize    = 18;
    private const int    _debugLabelOutlineSize = 4;
    private Godot.Color  _debugLabelFontOutline = new Godot.Color(0.0f, 0.0f, 0.0f, 1.0f);
    private const string _timeFormat            = "F6";

    private SysCG.Dictionary<XB.D, Godot.Color> _debugColors
        = new SysCG.Dictionary<XB.D, Godot.Color>();

    private Godot.TextureRect  _trBlueNoise;
    private Godot.ImageTexture _texBlueNoise;
    private Godot.TextureRect  _trMiniMap;
    private Godot.ImageTexture _texMiniMap;
    private Godot.TextureRect  _trMiniMapO;
    private Godot.Image        _imgMiniMapO;
    private Godot.ImageTexture _texMiniMapO;
    private Godot.Label[]      _lbDebugStats;

    public void InitializeDebugHUD() {
        _trBlueNoise          = new Godot.TextureRect();
        _trBlueNoise.Position = new Godot.Vector2I(_dimSpacer, _dimSpacer);
        Godot.Vector2I sizeBN = XB.Random.BlueNoise.GetSize();
        _trBlueNoise.Size     = sizeBN;
        _texBlueNoise         = new Godot.ImageTexture();
        _texBlueNoise.SetImage(XB.Random.BlueNoise);
        _trBlueNoise.Texture  = _texBlueNoise;
        AddChild(_trBlueNoise);

        var sizeMM   = new Godot.Vector2I(XB.HUD.ImgMiniMap.GetWidth(), XB.HUD.ImgMiniMap.GetHeight());
            sizeMM   = XB.Utils.MinV2I(sizeMM, new Godot.Vector2I(_sizeMMMax, _sizeMMMax));
        _imgMiniMapO = Godot.Image.Create(sizeMM.X, sizeMM.Y, false, Godot.Image.Format.Rgba8);
        var posMM    = new Godot.Vector2I(XB.AData.BaseResX -_dimSpacer - _imgMiniMapO.GetWidth(),
                                          _dimSpacer                                              );

        _texMiniMap          = new Godot.ImageTexture();
        _texMiniMap.SetImage(XB.HUD.ImgMiniMap);
        _trMiniMap           = new Godot.TextureRect();
        _trMiniMap.Position  = posMM;
        _trMiniMap.Size      = sizeMM;
        _trMiniMap.Texture   = _texMiniMap;
        _trMiniMap.ExpandMode = Godot.TextureRect.ExpandModeEnum.IgnoreSize;
        _trMiniMap.StretchMode = Godot.TextureRect.StretchModeEnum.Scale;
        AddChild(_trMiniMap);

        _texMiniMapO         = new Godot.ImageTexture();
        _imgMiniMapO.Fill(XB.Col.Transp);
        _texMiniMapO.SetImage(_imgMiniMapO);
        _trMiniMapO          = new Godot.TextureRect();
        _trMiniMapO.Position = posMM;
        _trMiniMapO.Size     = sizeMM;
        _trMiniMapO.Texture  = _texMiniMapO;
        _trMiniMapO.ExpandMode = Godot.TextureRect.ExpandModeEnum.IgnoreSize;
        _trMiniMapO.StretchMode = Godot.TextureRect.StretchModeEnum.Scale;
        AddChild(_trMiniMapO);

        int colCounter = 1;
        foreach (XB.D d in System.Enum.GetValues(typeof(XB.D))) {
            float r = XB.Random.RandomInRangeF(0.5f, 1.0f)*colCounter;
            float g = XB.Random.RandomInRangeF(0.5f, 1.0f)*colCounter;
            float b = XB.Random.RandomInRangeF(0.5f, 1.0f)*colCounter;
            r *= r;
            g *= g;
            b *= b;
            float largest = XB.Utils.MaxInArrayF(new float[] {r, g, b});
            r /= largest;
            g /= largest;
            b /= largest;
            _debugColors[d] = new Godot.Color(r, g, b, 1.0f);
            colCounter += 1;
        }

        _lbDebugStats = new Godot.Label[System.Enum.GetValues(typeof(XB.D)).Length];
        for (int i = 0; i < _lbDebugStats.Length; i++) {
            _lbDebugStats[i] = new Godot.Label();
            _lbDebugStats[i].Text = "";
            var tPos = new Godot.Vector2I(_dimSpacer, 2*_dimSpacer+sizeBN.Y + _debugLabelSpacing*i);
            _lbDebugStats[i].Position = tPos;
            var font = Godot.ResourceLoader.Load<Godot.Font>(XB.ScenePaths.FontLibMono);
            _lbDebugStats[i].AddThemeFontOverride    ("font",               font                  );
            _lbDebugStats[i].AddThemeFontSizeOverride("font_size",          _debugLabelFontSize   );
            _lbDebugStats[i].AddThemeConstantOverride("outline_size",       _debugLabelOutlineSize);
            _lbDebugStats[i].AddThemeColorOverride   ("font_outline_color", _debugLabelFontOutline);
            AddChild(_lbDebugStats[i]);
        }

        ProcessMode = ProcessModeEnum.Always;
        _visible    = false;
        Hide();
    }

    public void UpdateDebugHUD(float dt) {
        if (!_pauseDebug) { XB.DebugProfiling.ProcessCollectedTimes(dt); }
        if (!_visible)    { return; }

        // debug stats text
        for (int i = 0; i < _lbDebugStats.Length; i++) {
            if (XB.DebugProfiling.DebugStatEntries[i].HitsTot == 0) {
                _lbDebugStats[i].Text = "";
            } else {
                _lbDebugStats[i].AddThemeColorOverride("font_color", 
                    _debugColors[XB.DebugProfiling.DebugStatEntries[i].D]);
                string spacerName = "";
                int diff = XB.DebugProfiling.DebugFunctionNameMax -
                           XB.DebugProfiling.DebugStatEntries[i].D.ToString().Length;
                for (int j = 0; j < diff; j++) { spacerName += " "; }
                string spacerHits = ""; 
                if      (XB.DebugProfiling.DebugStatEntries[i].Hits < 10)  { spacerHits += "  "; }
                else if (XB.DebugProfiling.DebugStatEntries[i].Hits < 100) { spacerHits += " ";  }
                _lbDebugStats[i].Text = 
                    XB.DebugProfiling.DebugStatEntries[i].D.ToString() + 
                    spacerName + ": " +
                    XB.DebugProfiling.DebugStatEntries[i].TimeTot.ToString(_timeFormat) + 
                    " ms across " + 
                    XB.DebugProfiling.DebugStatEntries[i].Hits.ToString() +
                    spacerHits + " hits, max: " +
                    XB.DebugProfiling.DebugStatEntries[i].TimeMax.ToString(_timeFormat) +
                    " ms, avg: " +
                    XB.DebugProfiling.DebugStatEntries[i].TimeAvg.ToString(_timeFormat) +
                    " ms";
            }
        }

        // minimap
        XB.ManagerTerrain.UpdateQTreeTexture(ref _imgMiniMapO, XB.WorldData.WorldRes);
        _texMiniMapO.Update(_imgMiniMapO);
        _texMiniMap.Update(XB.HUD.ImgMiniMap);
    }

    public void ToggleDebugHUD() {
        _visible = !_visible;
        if (_visible) {
            Show();
        } else {
            Hide();
        }
    }

    public void TogglePauseDebug() {
        _pauseDebug = !_pauseDebug;
    }
}

// reference
#if XBDEBUG
        var debug = new XB.DebugTimedBlock(XB.D.ClassFunction);
#endif


#if XBDEBUG
        debug.End();
#endif 
//

public enum D { // unique debug identifier, naming scheme: "ClassFunction"
    Uninit,
    HUDAddDigitTexture,
    HUDCreateLinkingTexture,
    HUDCreateSphereTexture,
    HUDInitializeHud,
    HUDInitializeMiniMap,
    HUDUpdateHUD,
    HUDUpdateMiniMap,
    HUDUpdateMiniMapOverlayTexture,
    HUDUpdateSphereTexture,
    HUDUpdateSphereTextureHighlight,
    Initialize_Ready,
    ManagerApplyTerrain,
    ManagerChangeHighlightSphere,
    ManagerInitializeSpheres,
    ManagerLinkSpheres,
    ManagerRequestSphere,
    ManagerToggleLinking,
    ManagerUnlinkSpheres,
    ManagerUnsetLinkingID,
    ManagerUpdateActiveSpheres,
    ManagerUpdateSpheres,
    PController_Input,
    PController_PhysicsProcess,
    PControllerInitializePController,
    PControllerSpawnPlayer,
    RandomInitializeRandom,
    SphereInitializeSphere,
    SphereLinkSphere,
    SphereMoveSphere,
    SpherePlaceSphere,
    SphereRemoveSphere,
    SphereSphereTextureAddLinked,
    SphereSphereTextureRemoveLinked,
    SphereUnlinkFromAllSpheres,
    SphereUnlinkSphere,
    SphereUpdateSphere,
    TerrainCalculateNormals,
    TerrainFBM,
    TerrainFlat,
    TerrainGradientX,
    TerrainGradientY,
    TerrainHeightMax,
    TerrainHeightReplace,
    TerrainHeightsToMesh,
    TerrainResetLowestHighest,
    TerrainSetTerrainParameters,
    TerrainSkirtMesh,
    TerrainUpdateHeightMap,
    TerrainUpdateLowestHighest,
    UtilsBeveledRectangle,
    UtilsDigitRectangles,
    UtilsIntersectRayPlaneV3,
    UtilsPlayUISound,
    UtilsPointRectangles,
    UtilsRaycast,
    UtilsRectangleOutline,
    UtilsUpdateRect2I,
    WorldDataInitializeTerrainMesh,
    WorldDataGenerateRandomTerrain,
    WorldDataUpdateBlockStrength,
    WorldDataUpdateTerrainShader,
}

public struct DebugEntry {
    public XB.D   D         = XB.D.Uninit;
    public double TimeStart = 0.0;
    public double TimeEnd   = 0.0;

    public DebugEntry() {}
}

public class DebugStatistic {
    public double                               FrameTime = 0.0; // time of this monitored frame
    public SysCG.Dictionary<XB.D, XB.DebugData> FrameData;

    public DebugStatistic() {
        FrameData = new SysCG.Dictionary<XB.D, XB.DebugData>();
        foreach (XB.D d in System.Enum.GetValues(typeof(XB.D))) {
            FrameData.Add(d, new XB.DebugData());
        }
    }

    public void IncreaseValues(XB.D d, XB.D dParent, double time) {
        FrameData[d].IncreaseValues(dParent, time);
    }
}

// captured data for 1 frame
public class DebugData {
    public XB.D   NestParent = XB.D.Uninit; // which function called this function if nested (or self)
    public int    HitCount   = 0;   // function hits this frame
    public double Time       = 0.0; // time this frame
    public double TimeMax    = -9999.9; // maximum this frame

    public void IncreaseValues(XB.D d, double time) {
        NestParent  = d;
        HitCount   += 1;
        Time       += time;
        TimeMax     = XB.Utils.MaxD(time, TimeMax);
    }

    public void ResetValues() {
        HitCount = 0;
        Time     = 0.0;
        TimeMax  = -9999.9;
    }
}

// statistic entry that gets sorted to be displayed on screen
public class DebugStatisticEntry {
    public XB.D   D       = XB.D.Uninit;
    public int    Hits    = 0;
    public int    HitsTot = 0;
    public double TimeTot = 0.0;
    public double TimeMax = 0.0;
    public double TimeAvg = 0.0;
}

public class DebugProfiling {
    public static System.Diagnostics.Stopwatch   StopWatch;
    public const  int                            FramesToStore = 480;
    public const  int                            DebugEntryMax = 65536;
    public static int                            DebugFunctionNameMax = 0;
    public static int                            EntryCounter = 0; // position of highest used index
    public static XB.DebugEntry[]                DebugEntries; // one entry per TimedDebugBlock
    public static XB.DebugStatisticEntry[]       DebugStatEntries; // sorted array of debug data
    public static int                            DebugPos = 0; // position of current frame in the array
    public static XB.DebugStatistic[]            DebugStats; // one statistic per frame

    public static void StartProfiling() {
        StopWatch = new System.Diagnostics.Stopwatch();
        StopWatch.Start();

        foreach (var d in System.Enum.GetValues(typeof(XB.D))) {
            if (d.ToString().Length > DebugFunctionNameMax) {
                DebugFunctionNameMax = d.ToString().Length;
            }
        }
        DebugStatEntries = new XB.DebugStatisticEntry[System.Enum.GetValues(typeof(XB.D)).Length];
        for (int i = 0; i < DebugStatEntries.Length; i++) {
            DebugStatEntries[i] = new XB.DebugStatisticEntry();
        }
        DebugEntries = new XB.DebugEntry[DebugEntryMax];
        for (int i = 0; i < DebugEntries.Length; i++) {
            DebugEntries[i] = new XB.DebugEntry();
        }
        DebugStats = new XB.DebugStatistic[FramesToStore];
        for (int i = 0; i < DebugStats.Length; i++) {
            DebugStats[i] = new XB.DebugStatistic();
        }
    }

    // processing happens after each frame is done
    // the duration of this function does not impact the measured times,
    // speed is not as critical with this function
    public static void ProcessCollectedTimes(double delta) {
        DebugStats[DebugPos].FrameTime = delta;

        // add new frame's data to collected statistics
        for (int i = 0; i <= EntryCounter; i++) {
            XB.D   d    = DebugEntries[i].D;
            double time = DebugEntries[i].TimeEnd - DebugEntries[i].TimeStart;

            DebugStats[DebugPos].IncreaseValues(d, d, time);
        }

        foreach (XB.D d in System.Enum.GetValues(typeof(XB.D))) {
            // maximum and average time per hit during monitored period
            double max       = 0.0;
            double avg       = 0.0;
            int    hitsTotal = 0;
            for (int i = DebugPos+DebugStats.Length; i > (DebugPos+1)%DebugStats.Length; i--) {
                int pos = i%DebugStats.Length;
                max        = XB.Utils.MaxD(max, DebugStats[pos].FrameData[d].TimeMax);
                avg       += DebugStats[pos].FrameData[d].Time;
                hitsTotal += DebugStats[pos].FrameData[d].HitCount;
            }
            DebugStatEntries[(int)d].D       = d;
            DebugStatEntries[(int)d].Hits    = DebugStats[DebugPos].FrameData[d].HitCount;
            DebugStatEntries[(int)d].HitsTot = hitsTotal;
            DebugStatEntries[(int)d].TimeTot = DebugStats[DebugPos].FrameData[d].Time;
            DebugStatEntries[(int)d].TimeMax = max;
            DebugStatEntries[(int)d].TimeAvg = avg/hitsTotal;
        }

        int highestEntry = 0; // last position in the array that has hitsTot > 0
        highestEntry = PreSortDebugStatisticEntries(DebugStatEntries);
        QuicksortDebugStatisticEntries(DebugStatEntries, 0, highestEntry);

        DebugPos += 1;
        DebugPos %= DebugStats.Length;

        // remove values from oldest entry that will be overwritten next time
        foreach (XB.D d in System.Enum.GetValues(typeof(XB.D))) {
            DebugStats[DebugPos].FrameData[d].ResetValues();
        }
        EntryCounter = 0; // start from beginning of array to overwrite previous data
    }

    // move all elements with total hits > 0 to the front to reduce the number of elements to sort
    private static int PreSortDebugStatisticEntries(XB.DebugStatisticEntry[] unsorted) {
        int swapID  = -1;
        int highest = 0;
        for (int i = 0; i < unsorted.Length; i++) {
            if (unsorted[i].HitsTot == 0) {
                swapID = -1;
                for (int j = unsorted.Length-1; j > i; j--) {
                    if (unsorted[j].HitsTot != 0) {
                        swapID = j;
                        break;
                    }
                }
                if (swapID != -1) {
                    SwapDebugStatisticEntriesInArray(unsorted, i, swapID);
                    highest = i;
                } else {
                    break;
                }
            }
        }
        return highest;
    }

    // quicksort array entries from lo to hi positions in the array
    private static void QuicksortDebugStatisticEntries(XB.DebugStatisticEntry[] unsorted,
                                                       int lo, int hi) {
        if (hi-lo < 2) { return; }

        int pivot  = hi;
        int swapID = lo-1;

        for (int i = lo; i < hi; i++) {
            if (unsorted[i].TimeAvg >= unsorted[pivot].TimeAvg) {
                swapID += 1;
                SwapDebugStatisticEntriesInArray(unsorted, i, swapID);
            }
        }
        pivot = swapID+1;
        SwapDebugStatisticEntriesInArray(unsorted, hi, pivot);
        
        QuicksortDebugStatisticEntries(unsorted, lo, pivot-1);
        QuicksortDebugStatisticEntries(unsorted, pivot+1, hi);

        return;
    }

    private static void SwapDebugStatisticEntriesInArray(XB.DebugStatisticEntry[] array, int a, int b) {
        DebugStatisticEntry tmpEntry = array[a];
        array[a] = array[b];
        array[b] = tmpEntry;
    }
}

// an object of this gets created every time a function needs to be timed
// keep this as light as possible
public struct DebugTimedBlock {
    private XB.D   _d         = XB.D.Uninit;
    private double _timeStart = 0.0;
    private double _timeEnd   = 0.0;

    public DebugTimedBlock(XB.D d) {
        _d         = d;
        _timeStart = XB.DebugProfiling.StopWatch.Elapsed.TotalMilliseconds;
    }

    //NOTE[ALEX]: if End does not get called for some reason, the data does never get written
    public void End() {
        _timeEnd = XB.DebugProfiling.StopWatch.Elapsed.TotalMilliseconds;
        XB.DebugProfiling.DebugEntries[XB.DebugProfiling.EntryCounter].D         = _d;
        XB.DebugProfiling.DebugEntries[XB.DebugProfiling.EntryCounter].TimeStart = _timeStart;
        XB.DebugProfiling.DebugEntries[XB.DebugProfiling.EntryCounter].TimeEnd   = _timeEnd;
        XB.DebugProfiling.EntryCounter += 1;

        if (XB.DebugProfiling.EntryCounter == XB.DebugProfiling.DebugEntryMax) {
            XB.DebugProfiling.EntryCounter = 0;
        }
    }
}
} // namespace close