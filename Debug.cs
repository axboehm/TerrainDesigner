namespace XB { // namespace open
using SysCG = System.Collections.Generic;
// DebugHUD is an optional hud that shows the times used by each function that is being monitored
// monitoring has to be set up in each function manually but will be shown here automatically
// also visible is the currently used blue noise texture and the player coordinates
// used primarily for debugging and performance evaluation
public partial class DebugHUD : Godot.Control {
    private bool _visible    = false;
    private bool _pauseDebug = false;

    private const int    _dimSpacer             = 16;
    private const int    _debugLabelSpacing     = 18; // between each line of text
    private const int    _debugLabelFontSize    = 18;
    private const int    _debugLabelOutlineSize = 4;
    private const int    _edgeOff               = 100; // distance to right edge
    private const int    _bgOffset              = 350; // distance of background to left edge
    private Godot.Color  _debugLabelFontOutline = new Godot.Color(0.0f, 0.0f, 0.0f, 1.0f);
    private const string _timeFormat            = "F6";
    private const string _playerPosFormat       = "F3";

    private SysCG.Dictionary<XB.D, Godot.Color> _debugColors
        = new SysCG.Dictionary<XB.D, Godot.Color>();

    private Godot.TextureRect  _trBlueNoise;
    private Godot.ImageTexture _texBlueNoise;
    private Godot.TextureRect  _trPointiness;
    private Godot.Label[]      _lbDebugStats;
    private Godot.TextureRect  _trDebugStatsBG;
    private Godot.ImageTexture _texDebugStatsBG;
    private Godot.Image        _imgDebugStatsBG;
    private Godot.Label        _lbPlayerPos;

    public void InitializeDebugHUD() {
        MouseFilter = Godot.Control.MouseFilterEnum.Ignore; // do not catch any clicks

        _trBlueNoise          = new Godot.TextureRect();
        Godot.Vector2I sizeBN = XB.Random.BlueNoise.GetSize();
        _trBlueNoise.Size     = sizeBN;
        _trBlueNoise.Position = new Godot.Vector2I(XB.Settings.BaseResX
                                                   - _dimSpacer - _edgeOff - sizeBN.X,
                                                   _dimSpacer                         );
        _trBlueNoise.ExpandMode  = Godot.TextureRect.ExpandModeEnum.IgnoreSize;
        _trBlueNoise.StretchMode = Godot.TextureRect.StretchModeEnum.Scale;
        _texBlueNoise        = new Godot.ImageTexture();
        _texBlueNoise.SetImage(XB.Random.BlueNoise);
        _trBlueNoise.Texture = _texBlueNoise;
        AddChild(_trBlueNoise);
        _trBlueNoise.MouseFilter = Godot.Control.MouseFilterEnum.Ignore;

        _trPointiness          = new Godot.TextureRect();
        _trPointiness.Size     = sizeBN; // same size as blue noise texture
        _trPointiness.Position = new Godot.Vector2I(XB.Settings.BaseResX
                                                    - 2*_dimSpacer - _edgeOff - 2*sizeBN.X,
                                                    _dimSpacer                             );
        _trPointiness.ExpandMode  = Godot.TextureRect.ExpandModeEnum.IgnoreSize;
        _trPointiness.StretchMode = Godot.TextureRect.StretchModeEnum.Scale;
        _trPointiness.Texture = XB.WData.TexPointiness;
        AddChild(_trPointiness);
        _trPointiness.MouseFilter = Godot.Control.MouseFilterEnum.Ignore;

        // initialize random colors for function labels
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

        // darker background for easier reading of debug stats
        _trDebugStatsBG = new Godot.TextureRect();
        var bGSize      = new Godot.Vector2I(XB.Settings.BaseResX-2*_dimSpacer-_edgeOff-_bgOffset,
                                             XB.Settings.BaseResY-3*_dimSpacer-sizeBN.Y);
        _trDebugStatsBG.Size     = bGSize;
        _trDebugStatsBG.Position = new Godot.Vector2I(_dimSpacer+_bgOffset, 2*_dimSpacer+sizeBN.Y);
        _texDebugStatsBG = new Godot.ImageTexture();
        _imgDebugStatsBG = Godot.Image.Create(bGSize.X, bGSize.Y, false, Godot.Image.Format.Rgba8  );
        _imgDebugStatsBG.Fill(XB.Col.BG);
        _texDebugStatsBG.SetImage(_imgDebugStatsBG);
        _trDebugStatsBG.Texture = _texDebugStatsBG;
        AddChild(_trDebugStatsBG);
        _trDebugStatsBG.MouseFilter = Godot.Control.MouseFilterEnum.Ignore;

        var font = Godot.ResourceLoader.Load<Godot.Font>(XB.ResourcePaths.FontLibMono);
        for (int i = 0; i < _lbDebugStats.Length; i++) {
            _lbDebugStats[i] = new Godot.Label();
            _lbDebugStats[i].Text = "";
            var tPos = new Godot.Vector2I(_dimSpacer, 2*_dimSpacer+sizeBN.Y + _debugLabelSpacing*i);
            _lbDebugStats[i].HorizontalAlignment = Godot.HorizontalAlignment.Right;
            _lbDebugStats[i].Position = tPos;
            _lbDebugStats[i].Size     = new Godot.Vector2I(XB.Settings.BaseResX-2*_dimSpacer-_edgeOff,
                                                           _debugLabelSpacing+_debugLabelFontSize     );
            _lbDebugStats[i].AddThemeFontOverride    ("font",               font                  );
            _lbDebugStats[i].AddThemeFontSizeOverride("font_size",          _debugLabelFontSize   );
            _lbDebugStats[i].AddThemeConstantOverride("outline_size",       _debugLabelOutlineSize);
            _lbDebugStats[i].AddThemeColorOverride   ("font_outline_color", _debugLabelFontOutline);
            AddChild(_lbDebugStats[i]);
            _lbDebugStats[i].MouseFilter = Godot.Control.MouseFilterEnum.Ignore;
        }

        _lbPlayerPos = new Godot.Label();
        _lbPlayerPos.Position = new Godot.Vector2I(_dimSpacer, _dimSpacer);
        _lbPlayerPos.Size     = new Godot.Vector2I((int)_trPointiness.Position.X - 2*_dimSpacer, 
                                                   3*_debugLabelSpacing + 3*_debugLabelFontSize );
        _lbPlayerPos.HorizontalAlignment = Godot.HorizontalAlignment.Right;
        _lbPlayerPos.AddThemeFontOverride    ("font",               font                  );
        _lbPlayerPos.AddThemeFontSizeOverride("font_size",          _debugLabelFontSize   );
        _lbPlayerPos.AddThemeConstantOverride("outline_size",       _debugLabelOutlineSize);
        _lbPlayerPos.AddThemeColorOverride   ("font_outline_color", _debugLabelFontOutline);
        AddChild(_lbPlayerPos);
        _lbPlayerPos.MouseFilter = Godot.Control.MouseFilterEnum.Ignore;

        ProcessMode = ProcessModeEnum.Always;
        _visible    = false;
        Hide();
    }

    public void UpdateDebugHUD(float dt) {
        if (!_pauseDebug) { XB.DebugProfiling.ProcessCollectedTimes(dt); }
        if (!_visible)    { return; }

        // debug stats text
        //NOTE[ALEX]: if values stay within reasonable ranges, the debug strings are
        //            vertically aligned with spaces inbetween for easier comparing
        XB.DebugStatisticEntry[] stats = XB.DebugProfiling.DebugStatEntries;
        for (int i = 0; i < _lbDebugStats.Length; i++) {
            if (stats[i].HitsTot == 0) {
                _lbDebugStats[i].Text = "";
            } else {
                _lbDebugStats[i].AddThemeColorOverride("font_color", 
                    _debugColors[stats[i].D]);
                string spacerName = "";
                int diff = XB.DebugProfiling.DebugFunctionNameMax -
                           stats[i].D.ToString().Length;
                for (int j = 0; j < diff; j++) { spacerName += " "; }
                string spacerTimeTot = "";
                if      (stats[i].TimeTot < 10.0f)   { spacerTimeTot += "   "; }
                else if (stats[i].TimeTot < 100.0f)  { spacerTimeTot += "  ";  }
                else if (stats[i].TimeTot < 1000.0f) { spacerTimeTot += " ";   }
                string spacerTimeAvg = "";
                if      (stats[i].TimeAvg < 10.0f)   { spacerTimeAvg += "   "; }
                else if (stats[i].TimeAvg < 100.0f)  { spacerTimeAvg += "  ";  }
                else if (stats[i].TimeAvg < 1000.0f) { spacerTimeAvg += " ";   }
                string spacerTimeMax = "";
                if      (stats[i].TimeMax < 10.0f)   { spacerTimeMax += "   "; }
                else if (stats[i].TimeMax < 100.0f)  { spacerTimeMax += "  ";  }
                else if (stats[i].TimeMax < 1000.0f) { spacerTimeMax += " ";   }
                string spacerHits = ""; 
                if      (stats[i].Hits < 10)    { spacerHits += "    "; }
                else if (stats[i].Hits < 100)   { spacerHits += "   ";  }
                else if (stats[i].Hits < 1000)  { spacerHits += "  ";   }
                else if (stats[i].Hits < 10000) { spacerHits += " ";    }
                string spacerHitsMax = ""; 
                if      (stats[i].HitsMax < 10)    { spacerHitsMax += "    "; }
                else if (stats[i].HitsMax < 100)   { spacerHitsMax += "   ";  }
                else if (stats[i].HitsMax < 1000)  { spacerHitsMax += "  ";   }
                else if (stats[i].HitsMax < 10000) { spacerHitsMax += " ";    }
                _lbDebugStats[i].Text = 
                    stats[i].D.ToString()
                    + ": " + spacerName + spacerTimeTot + stats[i].TimeTot.ToString(_timeFormat)
                    + " ms across " + spacerHits + stats[i].Hits.ToString()
                    + " hits, max: " + spacerTimeMax + stats[i].TimeMax.ToString(_timeFormat)
                    + " ms, max hits: " + spacerHitsMax + stats[i].HitsMax.ToString()
                    + " avg: " + spacerTimeAvg + stats[i].TimeAvg.ToString(_timeFormat) + " ms";
            }
        }

        // player position
        string plPos  = "X: " + (-XB.PController.PModel.GlobalPosition.X).ToString(_playerPosFormat) + '\n';
               plPos += "Y: " + XB.PController.PModel.GlobalPosition.Y.ToString(_playerPosFormat) + '\n';
               plPos += "Z: " + (-XB.PController.PModel.GlobalPosition.Z).ToString(_playerPosFormat) + '\n';
        _lbPlayerPos.Text = plPos;

        _texBlueNoise.Update(XB.Random.BlueNoise);
    }

    // debug functions are moved here so they can be called from PController and Menu
    public void Debug1() {
        Godot.GD.Print("Debug1 - Toggle DebugHUD");
        ToggleDebugHUD();
    }

    public void Debug2() {
        Godot.GD.Print("Debug2 - Toggle PauseDebug");
        TogglePauseDebug();
    }

    public void Debug3() {
        Godot.GD.Print("Debug3");
        XB.ManagerTerrain.PrintQTReeExternal();
    }

    public void Debug4() {
        Godot.GD.Print("Debug4");
    }

    public void Debug5() {
        Godot.GD.Print("Debug5");
    }

    public void ToggleDebugHUD() {
        _visible = !_visible;
        if (_visible) { Show(); }
        else          { Hide(); }
    }

    public void TogglePauseDebug() {
        _pauseDebug = !_pauseDebug;
    }
}

// reference for adding a timed block to functions
// place the first three lines at the beginning of the function block to be timed
// and the last three at the end (before any return statements!)
// if End is not called, the block will not be considered by the profiler
#if XBDEBUG
        var debug = new XB.DebugTimedBlock(XB.D.ClassFunction);
#endif


#if XBDEBUG
        debug.End();
#endif 
//

// every function to be timed has to be added to this enum, the name shown in DebugHUD uses 
// the enum name
// naming scheme: "ClassFunction"
//NOTE[ALEX]: current implementation does not work with parallel for loops
public enum D {
    Uninit,
    CollisionTile,
    CollisionTileInitializeCollisionMesh,
    CollisionTileSampleTerrainNoise,
    CollisionTileApplyToCollisionMesh,
    DamSegment,
    DamSegmentApplyToMesh,
    DamSegmentReleaseMesh,
    DamSegmentUpdateMesh,
    DamSegmentUseMesh,
    HUDAddDigitTexture,
    HUDCreateGradientTexture,
    HUDCreateSphereTexture,
    HUDInitializeHud,
    HUDUpdateHUD,
    HUDUpdateMiniMap,
    HUDUpdateMiniMapOverlayTexture,
    HUDUpdateSphereTexture,
    HUDUpdateSphereTextureHighlight,
    Initialize_Ready,
    InputConsumeAllInputs,
    InputConsumeInputStart,
    InputGetInputs,
    ManagerSphereApplyTerrain,
    ManagerSphereChangeHighlightSphere,
    ManagerSphereClearSpheres,
    ManagerSphereInitializeSpheres,
    ManagerSphereLinkSpheres,
    ManagerSphereRecycleDamSegment,
    ManagerSphereRequestDamSegment,
    ManagerSphereRequestSphere,
    ManagerSphereToggleLinking,
    ManagerSphereUnlinkSpheres,
    ManagerSphereUnsetLinkingID,
    ManagerSphereFindeNextAvailableSphere,
    ManagerSphereUpdateDam,
    ManagerSphereUpdateSpheres,
    ManagerTerrainDivideQuadNode,
    ManagerTerrainInitializeQuadTree,
    ManagerTerrainQNodeShowReadyMeshes,
    ManagerTerrainQueueRequestProcess,
    ManagerTerrainQueueRequestMeshUpdate,
    ManagerTerrainRecycleChildMesh,
    ManagerTerrainRecycleMeshContainer,
    ManagerTerrainRequestMeshContainer,
    ManagerTerrainResetQuadTree,
    ManagerTerrainResetQNode,
    ManagerTerrainShowLargestTile,
    ManagerTerrainUpdateCollisionTiles,
    ManagerTerrainUpdateBlockStrength,
    ManagerTerrainUpdateQNodeMesh,
    ManagerTerrainUpdateQTreeMeshes,
    ManagerTerrainUpdateQTreeStrength,
    MeshContainer,
    MeshContainerAdjustWorldEdgeSkirt,
    MeshContainerApplyToMesh,
    MeshContainerReleaseMesh,
    MeshContainerSampleTerrainNoise,
    MeshContainerSetShaderAttribute,
    MeshContainerSetTerrainShaderAttributes,
    MeshContainerShowMesh,
    MeshContainerSkirtTriangleIndices,
    MeshContainerUse,
    MeshContainerUseMesh,
    PController_Input,
    PController_PhysicsProcess,
    PControllerInitializePController,
    PControllerPlacePlayer,
    PControllerSpawnPlayer,
    PControllerSpawnPlayerDelayed,
    PControllerUpdateGeneral,
    PControllerUpdateMovement,
    PControllerUpdateCamera,
    PControllerUpdateAiming,
    PControllerUpdateSphereInteractions,
    PControllerUpdateAnimations,
    PControllerUpdatePlayerMaterial,
    PControllerUpdateInputs,
    QNode,
    QNodeActivate,
    QNodeAssignMeshContainer,
    QNodeChildrenActiveAndReady,
    QNodeChildrenActiveAndReadyRecursive,
    QNodeDeActivate,
    QNodeReleaseMeshContainer,
    QNodeShowMeshContainer,
    QNodeUpdateAssignedMesh,
    RandomInitializeRandom,
    RandomRandomInRangeF,
    RandomRandomInRangeI,
    RandomRandomInRangeU,
    RandomRandomUInt,
    RandomXorshift,
    SphereChangeSphereAngle,
    SphereChangeSphereRadius,
    SphereInitializeSphere,
    SphereLinkSphere,
    SphereMoveSphere,
    SpherePlaceSphere,
    SphereRemoveSphere,
    SphereSphereTextureAddLinked,
    SphereSphereTextureRemoveLinked,
    SphereUnlinkFromAllSpheres,
    SphereUnlinkSphere,
    SphereUpdateConeMesh,
    SphereUpdateSphere,
    TerrainCalculateNormals,
    TerrainCone,
    TerrainFBM,
    TerrainFindLowestHighest,
    TerrainFlat,
    TerrainGradientX,
    TerrainGradientY,
    TerrainHeightMapSample,
    TerrainHeightMax,
    TerrainHeightMin,
    TerrainHeightReplace,
    TerrainHeightScale,
    TerrainResetLowestHighest,
    TerrainUnevenCapsule,
    TerrainUpdateHeightMap,
    TerrainUpdateLowestHighest,
    UtilsBeveledRectangle,
    UtilsLerpV2,
    UtilsLerpV3,
    UtilsMaxInArrayF,
    UtilsDigitRectangles,
    UtilsFillRectanglesInImage,
    UtilsIntersectRayPlaneV3,
    UtilsPlayUISound,
    UtilsPointRectangles,
    UtilsRaycast,
    UtilsRectangleOutline,
    UtilsUpdateRect2I,
    WorldDataApplyDamSegment,
    WorldDataApplySphereCone,
    WorldDataInitializeTerrainMesh,
    WorldDataGenerateRandomTerrain,
    WorldDataUpdateTerrain,
}

// single data point for profiling times
public struct DebugEntry {
    public XB.D   D         = XB.D.Uninit;
    public double TimeStart = 0.0;
    public double TimeEnd   = 0.0;

    public DebugEntry() {}
}

// all profiling data of one frame for each profiled function
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
    public int    HitCount   = 0;       // function hits this frame
    public double Time       = 0.0;     // time this frame
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
    public int    HitsMax = 0;
    public double TimeTot = 0.0;
    public double TimeMax = 0.0;
    public double TimeAvg = 0.0;
}

// main profiling class that holds profiling data and processes it when called
public class DebugProfiling {
    public static System.Diagnostics.Stopwatch   StopWatch;
    public const  int                            FramesToStore = 480;
    public const  int                            DebugEntryMax = 65536;
    public static int                            DebugFunctionNameMax = 0;
    public static int                            EntryCounter = 0; // position of highest used index
    public static XB.DebugEntry[]                DebugEntries;     // one entry per TimedDebugBlock
    public static XB.DebugStatisticEntry[]       DebugStatEntries; // sorted array of debug data
    public static int                            DebugPos = 0; // position of current frame in the array
    public static XB.DebugStatistic[]            DebugStats;   // one statistic per frame

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
            double timeMax   = 0.0;
            double timeAvg   = 0.0;
            int    hitsMax   = 0;
            int    hitsTotal = 0;
            for (int i = DebugPos+DebugStats.Length; i > (DebugPos+1)%DebugStats.Length; i--) {
                int pos = i%DebugStats.Length;
                timeMax    = XB.Utils.MaxD(timeMax, DebugStats[pos].FrameData[d].TimeMax);
                timeAvg   += DebugStats[pos].FrameData[d].Time;
                hitsMax    = XB.Utils.MaxI(hitsMax, DebugStats[pos].FrameData[d].HitCount);
                hitsTotal += DebugStats[pos].FrameData[d].HitCount;
            }
            DebugStatEntries[(int)d].D       = d;
            DebugStatEntries[(int)d].Hits    = DebugStats[DebugPos].FrameData[d].HitCount;
            DebugStatEntries[(int)d].HitsMax = hitsMax;
            DebugStatEntries[(int)d].HitsTot = hitsTotal;
            DebugStatEntries[(int)d].TimeTot = DebugStats[DebugPos].FrameData[d].Time;
            DebugStatEntries[(int)d].TimeMax = timeMax;
            DebugStatEntries[(int)d].TimeAvg = timeAvg/hitsTotal;
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
                                                       int lo, int hi                    ) {
        if (hi-lo < 2) { return; }

        int pivot  = hi;
        int swapID = lo - 1;

        for (int i = lo; i < hi; i++) {
            if (unsorted[i].TimeAvg >= unsorted[pivot].TimeAvg) {
                swapID += 1;
                SwapDebugStatisticEntriesInArray(unsorted, i, swapID);
            }
        }
        pivot = swapID + 1;
        SwapDebugStatisticEntriesInArray(unsorted, hi, pivot);
        
        QuicksortDebugStatisticEntries(unsorted, lo,        pivot - 1);
        QuicksortDebugStatisticEntries(unsorted, pivot + 1, hi       );

        return;
    }

    private static void SwapDebugStatisticEntriesInArray(XB.DebugStatisticEntry[] array, int a, int b) {
        DebugStatisticEntry tmpEntry = array[a];
        array[a] = array[b];
        array[b] = tmpEntry;
    }
}

// an object of this gets created every time a function needs to be timed
// a lot of these are allocated each frame and not recycled, this has a lot of overhead
// creating and processing these happens outside of the profiling of other functions,
// so the additional overhead is noticeable by the user but not shown in the times in DebugHUD
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
