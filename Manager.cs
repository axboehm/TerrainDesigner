#define XBDEBUG
using SysCG = System.Collections.Generic;
namespace XB { // namespace open
public class ManagerSphere {
    public const  int  MaxSphereAmount = 64; // limit to <= 99 because of sphere texture sizes,
    public static int  ActiveSpheres   = 0;
    public static int  NextSphere      = MaxSphereAmount;
    public static int  HLSphereID      = MaxSphereAmount;
    public static int  LinkingID       = MaxSphereAmount; // id of sphere to link with
    public static bool Linking         = false;

    public static XB.Sphere[] Spheres = new XB.Sphere[MaxSphereAmount];

    public static void InitializeSpheres() {
#if XBDEBUG
        var debug = new XB.DebugTimedBlock(XB.D.ManagerInitializeSpheres);
#endif

        var sphereScn = Godot.ResourceLoader.Load<Godot.PackedScene>(XB.ScenePaths.Sphere);
        XB.Sphere sphere;
        var rects = new Godot.Rect2I[XB.Utils.MaxRectSize];
        int rSize = 0;
        var vect  = new Godot.Vector2I(0, 0);
        for (int i = 0; i < MaxSphereAmount; i++) {
            sphere = (XB.Sphere)sphereScn.Instantiate();
            sphere.InitializeSphere(i, ref rects, ref rSize, ref vect);
            XB.AData.MainRoot.AddChild(sphere);
            Spheres[i] = sphere;
        }
        UpdateActiveSpheres();

#if XBDEBUG
        debug.End();
#endif 
    }

    public static void UpdateSpheres(float dt) {
#if XBDEBUG
        var debug = new XB.DebugTimedBlock(XB.D.ManagerUpdateSpheres);
#endif

        for (int i = 0; i < MaxSphereAmount; i++) { Spheres[i].UpdateSphere(dt); }

        if (HLSphereID < MaxSphereAmount) { Spheres[HLSphereID].Highlighted = 1.0f; }
        if (LinkingID < MaxSphereAmount)  { Spheres[LinkingID].Highlighted  = 1.0f; }

#if XBDEBUG
        debug.End();
#endif 
    }

    public static void ChangeHighlightSphere(int newHLSphereID) {
#if XBDEBUG
        var debug = new XB.DebugTimedBlock(XB.D.ManagerChangeHighlightSphere);
#endif

        if (HLSphereID == newHLSphereID) { return; }
        XB.PController.Hud.UpdateSphereTextureHighlight(HLSphereID, newHLSphereID);
        HLSphereID = newHLSphereID;

#if XBDEBUG
        debug.End();
#endif 
    }

    public static void UpdateActiveSpheres() {
#if XBDEBUG
        var debug = new XB.DebugTimedBlock(XB.D.ManagerUpdateActiveSpheres);
#endif

        ActiveSpheres = 0;
        NextSphere = MaxSphereAmount+1;
        for (int i = 0; i < MaxSphereAmount; i++) {
            if (Spheres[i].Active) { ActiveSpheres += 1;                        }
            else                   { NextSphere = XB.Utils.MinI(i, NextSphere); }
        }

#if XBDEBUG
        debug.End();
#endif 
    }

    public static void ToggleLinking() {
#if XBDEBUG
        var debug = new XB.DebugTimedBlock(XB.D.ManagerToggleLinking);
#endif

        Linking = !Linking;
        if (!Linking) { 
            if (LinkingID < MaxSphereAmount) {
                Spheres[LinkingID].SphereTextureRemoveLinked();
            }
            LinkingID = MaxSphereAmount; 
        }

#if XBDEBUG
        debug.End();
#endif 
    }

    public static void LinkSpheres() {
#if XBDEBUG
        var debug = new XB.DebugTimedBlock(XB.D.ManagerLinkSpheres);
#endif

        // Godot.GD.Print("linking: " + LinkingID + " with " + HLSphereID);
        if        (LinkingID == HLSphereID) {
            Spheres[LinkingID].SphereTextureRemoveLinked();
            LinkingID = MaxSphereAmount;
        } else if (LinkingID == MaxSphereAmount) {
            LinkingID = HLSphereID;
            Spheres[LinkingID].SphereTextureAddLinked();
        } else {
            Spheres[LinkingID].SphereTextureRemoveLinked();
            Spheres[HLSphereID].LinkSphere(LinkingID);
            Spheres[LinkingID].LinkSphere(HLSphereID);
            LinkingID = HLSphereID;
            Spheres[LinkingID].SphereTextureAddLinked();
        }

#if XBDEBUG
        debug.End();
#endif 
    }

    public static void UnsetLinkingID() {
#if XBDEBUG
        var debug = new XB.DebugTimedBlock(XB.D.ManagerUnsetLinkingID);
#endif

        if (LinkingID == MaxSphereAmount) { return; }
        Spheres[LinkingID].SphereTextureRemoveLinked();
        LinkingID = MaxSphereAmount;

#if XBDEBUG
        debug.End();
#endif 
    }

    public static void UnlinkSpheres() {
#if XBDEBUG
        var debug = new XB.DebugTimedBlock(XB.D.ManagerUnlinkSpheres);
#endif

        // Godot.GD.Print("unlinking: " + HLSphereID);
        Spheres[HLSphereID].SphereTextureRemoveLinked();
        Spheres[HLSphereID].UnlinkFromAllSpheres();
        LinkingID = MaxSphereAmount;

#if XBDEBUG
        debug.End();
#endif 
    }

    public static void ApplyTerrain() {
#if XBDEBUG
        var debug = new XB.DebugTimedBlock(XB.D.ManagerApplyTerrain);
#endif

        // recalculate and assign terrain
        for (int i = 0; i < MaxSphereAmount; i++) {
            Spheres[i].RemoveSphere();
        }

#if XBDEBUG
        debug.End();
#endif 
    }

    public static bool RequestSphere(Godot.Vector3 pos) {
#if XBDEBUG
        var debug = new XB.DebugTimedBlock(XB.D.ManagerRequestSphere);
#endif

        if (NextSphere == MaxSphereAmount+1) { 
#if XBDEBUG
            debug.End();
#endif 

            return false; 
        }

        XB.PController.Hud.UpdateSphereTextureHighlight(HLSphereID, NextSphere);
        HLSphereID = NextSphere; // so that linking immediately works
        Spheres[NextSphere].PlaceSphere(pos);
        if (Linking && LinkingID < MaxSphereAmount) { LinkSpheres(); }

#if XBDEBUG
        debug.End();
#endif 

        return true;
    }
}

public class QNode {
    public QNode   Parent;
    public QNode[] Children;
    public XB.MeshContainer MeshContainer;
    public bool  Visible;
    public int   ID;
    public float XPos;  // center x coordinate in meter
    public float ZPos;
    public float XSize; // dimensions in meter
    public float ZSize;
    public float Res;   // subdivisions per meter

    public QNode(ref int id, float xPos, float zPos, float xSize, float zSize,
                 float res, QNode parent = null                               ) {
        Parent   = parent;
        Children = new QNode[4];
        for (int i = 0; i < 4; i++) { Children[i] = null; }

        MeshContainer = null;
        Visible       = false;

        ID    = id;
        id   += 1;
        XPos  = xPos;
        ZPos  = zPos;
        XSize = xSize;
        ZSize = zSize;
        Res   = res;
    }

    public void AssignMeshContainer(XB.MeshContainer mC) {
#if XBDEBUG
        var debug = new XB.DebugTimedBlock(XB.D.QNodeAssignMeshContainer);
#endif


#if XBDEBUG
        debug.End();
#endif 
        MeshContainer = mC;
        Visible       = true;
    }

    public void ReleaseMeshContainer() {
#if XBDEBUG
        var debug = new XB.DebugTimedBlock(XB.D.QNodeReleaseMeshContainer);
#endif


#if XBDEBUG
        debug.End();
#endif 
        if (!Visible) { return; }
        MeshContainer.ReleaseMesh();
        MeshContainer = null;
        Visible       = false;
    }

    public void UpdateAssignedMesh(float worldXSize, float worldZSize, float height,
                                   ref Godot.Image imgHeightMap                     ) {
#if XBDEBUG
        var debug = new XB.DebugTimedBlock(XB.D.QNodeUpdateAssignedMesh);
#endif

        MeshContainer.SampleTerrainNoise(XPos, ZPos, XSize, ZSize, worldXSize, worldZSize,
                                         Res, height, ref imgHeightMap                    );
        MeshContainer.ApplyToMesh();

#if XBDEBUG
        debug.End();
#endif 
    }
}

public class MeshContainer {
    public Godot.MeshInstance3D    MeshInst;
    public Godot.Collections.Array MeshData;
    public Godot.ArrayMesh         ArrMesh;
    public Godot.StaticBody3D      StatBody;
    public Godot.CollisionShape3D  CollShape;
    public Godot.ShaderMaterial    Material;
    public int   XAmount;
    public int   ZAmount;
    public int   ID;
    public bool  InUse;
    public float LowestPoint;  // every MeshContainer tracks its own lowest and highest
    public float HighestPoint;
    public Godot.Vector3[] Vertices;
    public Godot.Vector2[] UVs;
    public Godot.Vector3[] Normals;
    public int[]           Triangles;

    //TODO[ALEX]: colliders updating at the same time introduces twitchiness
    //            maybe just generate colliders at startup at lower resolution and keep those throughout
    //            or make respawn very exact and respawn on every change in meshtiles?
    public MeshContainer(Godot.Node root, int id, float lerpRAmount, float lerpGAmount) {
        MeshInst  = new Godot.MeshInstance3D();
        StatBody  = new Godot.StaticBody3D();
        CollShape = new Godot.CollisionShape3D();
        root.AddChild(StatBody);
        StatBody.AddChild(MeshInst);
        StatBody.AddChild(CollShape);
        StatBody.CollisionLayer = XB.LayerMasks.EnvironmentLayer;
        StatBody.CollisionMask  = XB.LayerMasks.EnvironmentMask; //TODO[ALEX]: necessary?

        Material  = new Godot.ShaderMaterial();
        Material.Shader = Godot.ResourceLoader.Load<Godot.Shader>(XB.ScenePaths.TerrainShader);
        // colors initially represent somewhat of a gradient 
        // but will quickly get shuffled around as MeshContainers get reused
        float r = 1.0f - XB.Utils.LerpF(0.0f, 1.0f, lerpRAmount);
        float g =        XB.Utils.LerpF(0.0f, 1.0f, lerpGAmount);
        float b = XB.Random.RandomInRangeF(0.0f, 1.0f);
        var col = new Godot.Color(r, g, b, 1.0f);
        Material.SetShaderParameter("albVis", col);
        //TODO[ALEX]: set shader up

        MeshData = new Godot.Collections.Array();
        MeshData.Resize((int)Godot.Mesh.ArrayType.Max);
        ArrMesh  = new Godot.ArrayMesh();

        XAmount = 0;
        ZAmount = 0;
        ID      = id;
        InUse   = true;

        ResetLowestHighest();
    }

    public void ReleaseMesh() {
#if XBDEBUG
        var debug = new XB.DebugTimedBlock(XB.D.MeshContainerReleaseMesh);
#endif

        MeshInst.Hide();
        InUse = false;

#if XBDEBUG
        debug.End();
#endif 
    }

    public void UseMesh(float xSize, float zSize, float res) {
#if XBDEBUG
        var debug = new XB.DebugTimedBlock(XB.D.MeshContainerUseMesh);
#endif

        int xAmount = (int)(xSize*res) + 1;
        int zAmount = (int)(zSize*res) + 1;
        if (XAmount != xAmount || ZAmount != zAmount) {
            XAmount = xAmount;
            ZAmount = zAmount;
            Vertices  = new Godot.Vector3[XAmount*ZAmount];
            UVs       = new Godot.Vector2[XAmount*ZAmount];
            Normals   = new Godot.Vector3[XAmount*ZAmount];
            Triangles = new int          [XAmount*ZAmount*6];

            Godot.Vector2 v2 = new Godot.Vector2(0.0f, 0.0f);
            for (int i = 0; i < UVs.Length; i++) {
                int x = i%XAmount;
                int z = i/XAmount;
                v2.X = 1.0f - (float)x/(float)(XAmount-1);
                v2.Y = 1.0f - (float)z/(float)(ZAmount-1);
                UVs[i] = v2;
            }

            int tri  = 0;
            int vert = 0;
            for (int i = 0; i < XAmount-1; i++) {
                for (int j = 0; j < ZAmount-1; j++) {
                    Triangles[tri + 0] = vert;
                    Triangles[tri + 1] = vert + 1 + xAmount;
                    Triangles[tri + 2] = vert + 1;
                    Triangles[tri + 3] = vert;
                    Triangles[tri + 4] = vert +     XAmount;
                    Triangles[tri + 5] = vert + 1 + XAmount;
                    tri  += 6;
                    vert += 1;
                }
                vert += 1;
            }

            MeshData[(int)Godot.Mesh.ArrayType.TexUV] = UVs;
            MeshData[(int)Godot.Mesh.ArrayType.Index] = Triangles;
        }

        MeshInst.Show();
        InUse = true;

#if XBDEBUG
        debug.End();
#endif 
    }

    public void SampleTerrainNoise(float xPos, float zPos, float xSize,  float zSize,
                                   float worldXSize, float worldZSize, float res, float height,
                                   ref Godot.Image imgHeightMap                                ) {
#if XBDEBUG
        var debug = new XB.DebugTimedBlock(XB.D.MeshContainerSampleTerrainNoise);
#endif

        var pos = new Godot.Vector3(xPos, 0.0f, zPos);
        StatBody.GlobalPosition = pos; // move MeshInstance to center of tile, mesh is child

        ResetLowestHighest();

        var   v3   = new Godot.Vector3(0.0f, 0.0f, 0.0f);
        float step = 1.0f/res;
        float sampledNoise = 0.0f;
        for (int i = 0; i < XAmount; i++) {
            for (int j = 0; j < ZAmount; j++) {
                int vNumber = i*XAmount + j;
                v3.X = (float)i*step - xSize/2.0f;
                v3.Z = (float)j*step - zSize/2.0f;
                sampledNoise = XB.Terrain.HeightMapSample(v3.X + pos.X, v3.Z + pos.Z,
                                                          worldXSize, worldZSize, ref imgHeightMap);
                v3.Y = sampledNoise*height;
                UpdateLowestHighest(v3.Y);
                Vertices[vNumber] = v3;
            }
        }

        XB.Terrain.CalculateNormals(ref Normals, ref Vertices, ref Triangles);

#if XBDEBUG
        debug.End();
#endif 
    }

    private void ResetLowestHighest() {
#if XBDEBUG
        var debug = new XB.DebugTimedBlock(XB.D.MeshContainerResetLowestHighest);
#endif

        LowestPoint  = float.MaxValue;
        HighestPoint = float.MinValue;

#if XBDEBUG
        debug.End();
#endif 
    }

    private void UpdateLowestHighest(float value) {
#if XBDEBUG
        var debug = new XB.DebugTimedBlock(XB.D.MeshContainerUpdateLowestHighest);
#endif

        LowestPoint  = float.MaxValue;
        HighestPoint = float.MinValue;

#if XBDEBUG
        debug.End();
#endif 
    }

    public void ApplyToMesh() {
#if XBDEBUG
        var debug = new XB.DebugTimedBlock(XB.D.MeshContainerApplyToMesh);
#endif

        MeshData[(int)Godot.Mesh.ArrayType.Vertex] = Vertices;
        MeshData[(int)Godot.Mesh.ArrayType.Normal] = Normals;

        ArrMesh.ClearSurfaces();
        ArrMesh.AddSurfaceFromArrays(Godot.Mesh.PrimitiveType.Triangles, MeshData);

        MeshInst.Mesh   = ArrMesh;
        CollShape.Shape = ArrMesh.CreateTrimeshShape();

        MeshInst.Mesh.SurfaceSetMaterial(0, Material);

#if XBDEBUG
        debug.End();
#endif 
    }
}

public enum Sk {
    B, // bottom (Z-)
    T, // top    (Z+)
    L, // left   (X-)
    R, // right  (X+)
}

public class ManagerTerrain {
    private static XB.QNode _qRoot;
    private static int      _nextID      = 0;
    private static int      _divisions   = 0;
    private static float    _resolution  = 0.0f; // highest resolution
    private static float    _worldXSize  = 0.0f;
    private static float    _worldZSize  = 0.0f;
    private static float    _worldHeight = 0.0f;
    private static SysCG.List<XB.MeshContainer> _terrainMeshes;

    private static Godot.MeshInstance3D[]    _terrainSkirtMesh; // black sides
    private static float[][]                 _edgeHeights; //TODO
    private static Godot.Vector3[][]         _skVertices;
    private static Godot.Vector3[][]         _skNormals;
    private static int[][]                   _skTriangles;
    private static Godot.ShaderMaterial      _terrainSkirtMat;
    private static Godot.Collections.Array[] _meshDataSk;
    private static Godot.ArrayMesh[]         _arrMeshSk;

    // takes resolution at highest detail and number of quadtree divisions
    public static void InitializeQuadTree(float xSize, float zSize, float height, float res, int div) {
#if XBDEBUG
        var debug = new XB.DebugTimedBlock(XB.D.ManagerTerrainInitializeQuadTree);
#endif

        //TODO[ALEX]: adjust divisions according to size to prevent overly large tiles
        _nextID     = 0;
        _divisions  = div;
        _resolution = res;
        float temp  = 1;
        // limit divisions so that tiles do not get too small 
        // so that they would have < 2 vertices per side
        for (int i = 0; i < div; i++) {
            temp *= 2;
            if (temp > xSize) {
                _divisions = i;
                Godot.GD.Print("Divisions set to high at : " + div + ", reduced to: " + _divisions);
                break;
            }
        }
        _worldXSize  = xSize;
        _worldZSize  = zSize;
        _worldHeight = height;

        // Godot.GD.Print("InitializeQuadTree with Size: " + xSize + " x " + zSize
        //                + ", Resolution: " + res + ", Divisions: " + _divisions );
        
        // the lowest division level (highest detail) should have the specified resolution
        for (int i = 0; i < _divisions; i++) { res = res/2.0f; }
        _qRoot = new XB.QNode(ref _nextID, xSize/2, zSize/2, xSize, zSize, res);

        DivideQuadNode(ref _qRoot, _divisions);
        //TODO[ALEX]: colliders, node tile skirts

        _terrainMeshes = new SysCG.List<XB.MeshContainer>();

        _terrainSkirtMesh = new Godot.MeshInstance3D[4];
        _terrainSkirtMat  = new Godot.ShaderMaterial();
        _terrainSkirtMat.Shader = Godot.ResourceLoader.Load<Godot.Shader>(XB.ScenePaths.TSkirtShader);
        _meshDataSk = new Godot.Collections.Array[4];
        _arrMeshSk  = new Godot.ArrayMesh[4];
        for (int i = 0; i < 4; i++) {
            _terrainSkirtMesh[i] = new Godot.MeshInstance3D();
            XB.AData.MainRoot.AddChild(_terrainSkirtMesh[i]);
            _meshDataSk[i] = new Godot.Collections.Array();
            _meshDataSk[i].Resize((int)Godot.Mesh.ArrayType.Max);
            _arrMeshSk[i]  = new Godot.ArrayMesh();
        }
        _skVertices = new Godot.Vector3[4][];
        _skVertices[(int)XB.Sk.B] = new Godot.Vector3[2*((int)(_worldXSize*_resolution) + 1)];
        _skVertices[(int)XB.Sk.T] = new Godot.Vector3[2*((int)(_worldXSize*_resolution) + 1)];
        _skVertices[(int)XB.Sk.L] = new Godot.Vector3[2*((int)(_worldZSize*_resolution) + 1)];
        _skVertices[(int)XB.Sk.R] = new Godot.Vector3[2*((int)(_worldZSize*_resolution) + 1)];
        _skNormals = new Godot.Vector3[4][];
        _skNormals[(int)XB.Sk.B] = new Godot.Vector3[2*((int)(_worldXSize*_resolution) + 1)];
        _skNormals[(int)XB.Sk.T] = new Godot.Vector3[2*((int)(_worldXSize*_resolution) + 1)];
        _skNormals[(int)XB.Sk.L] = new Godot.Vector3[2*((int)(_worldZSize*_resolution) + 1)];
        _skNormals[(int)XB.Sk.R] = new Godot.Vector3[2*((int)(_worldZSize*_resolution) + 1)];
        _skTriangles = new int[4][];
        _skTriangles[(int)XB.Sk.B] = new int[(int)(_worldXSize*_resolution) * 6];
        _skTriangles[(int)XB.Sk.T] = new int[(int)(_worldXSize*_resolution) * 6];
        _skTriangles[(int)XB.Sk.L] = new int[(int)(_worldZSize*_resolution) * 6];
        _skTriangles[(int)XB.Sk.R] = new int[(int)(_worldZSize*_resolution) * 6];

        InitializeQTreeWorldSkirt();

#if XBDEBUG
        debug.End();
#endif 
    }

    // create a sparse quad tree without any mesh data
    private static void DivideQuadNode(ref XB.QNode parent, int divisions) {
        if (divisions == 0) { return; }

#if XBDEBUG
        var debug = new XB.DebugTimedBlock(XB.D.ManagerTerrainDivideQuadNode);
#endif

        divisions -= 1;
        
        float xPos  = parent.XPos;
        float zPos  = parent.ZPos;
        float xSize = parent.XSize/2.0f;
        float zSize = parent.ZSize/2.0f;
        float res   = parent.Res*2.0f;

        var q1 = new XB.QNode(ref _nextID, xPos-xSize/2, zPos-zSize/2, xSize, zSize, res, parent);
        var q2 = new XB.QNode(ref _nextID, xPos-xSize/2, zPos+zSize/2, xSize, zSize, res, parent);
        var q3 = new XB.QNode(ref _nextID, xPos+xSize/2, zPos-zSize/2, xSize, zSize, res, parent);
        var q4 = new XB.QNode(ref _nextID, xPos+xSize/2, zPos+zSize/2, xSize, zSize, res, parent);

        parent.Children[0] = q1;
        parent.Children[1] = q2;
        parent.Children[2] = q3;
        parent.Children[3] = q4;

        DivideQuadNode(ref parent.Children[0], divisions);
        DivideQuadNode(ref parent.Children[1], divisions);
        DivideQuadNode(ref parent.Children[2], divisions);
        DivideQuadNode(ref parent.Children[3], divisions);

#if XBDEBUG
        debug.End();
#endif 
    }

    // reference position can be the player model or player camera,
    // depending on where the highest resolution mesh should be prioritized
    public static void UpdateQTreeMeshes(Godot.Vector2 refPos, ref Godot.Image imgHeightMap) {
#if XBDEBUG
        var debug = new XB.DebugTimedBlock(XB.D.ManagerTerrainUpdateQTreeMeshes);
#endif

        UpdateQNodeMesh(refPos, ref _qRoot, ref imgHeightMap);   
        UpdateQTreeWorldSkirt();

#if XBDEBUG
        debug.End();
#endif 
    }

    private static void UpdateQNodeMesh(Godot.Vector2 refPos, ref XB.QNode qNode,
                                        ref Godot.Image imgHeightMap             ) {
#if XBDEBUG
        var debug = new XB.DebugTimedBlock(XB.D.ManagerTerrainUpdateQNodeMesh);
#endif
        if (qNode.Children[0] == null) {
            if (!qNode.Visible) {
                RequestMeshContainer(ref qNode);
                qNode.UpdateAssignedMesh(_worldXSize, _worldZSize, _worldHeight, ref imgHeightMap);
                XB.Terrain.UpdateLowestHighest(qNode.MeshContainer.LowestPoint,
                                               qNode.MeshContainer.HighestPoint);
                RecycleChildMesh(ref qNode.Children[0]);
                RecycleChildMesh(ref qNode.Children[1]);
                RecycleChildMesh(ref qNode.Children[2]);
                RecycleChildMesh(ref qNode.Children[3]);
            }
            return;
        }

        var   qNodeCtr = new Godot.Vector2(qNode.XPos, qNode.ZPos);
        float dist     = (refPos-qNodeCtr).Length();
        float comp     = (qNode.XSize + qNode.ZSize) / 2;

        if (dist < comp) {
            if (qNode.Visible) {
                RecycleMeshContainer(ref qNode);
            }
            UpdateQNodeMesh(refPos, ref qNode.Children[0], ref imgHeightMap);
            UpdateQNodeMesh(refPos, ref qNode.Children[1], ref imgHeightMap);
            UpdateQNodeMesh(refPos, ref qNode.Children[2], ref imgHeightMap);
            UpdateQNodeMesh(refPos, ref qNode.Children[3], ref imgHeightMap);
        } else {
            if (!qNode.Visible) {
                RequestMeshContainer(ref qNode);
                qNode.UpdateAssignedMesh(_worldXSize, _worldZSize, _worldHeight, ref imgHeightMap);
                XB.Terrain.UpdateLowestHighest(qNode.MeshContainer.LowestPoint,
                                               qNode.MeshContainer.HighestPoint);
                RecycleChildMesh(ref qNode.Children[0]);
                RecycleChildMesh(ref qNode.Children[1]);
                RecycleChildMesh(ref qNode.Children[2]);
                RecycleChildMesh(ref qNode.Children[3]);
            }
        }

#if XBDEBUG
        debug.End();
#endif 
    }

    private static void InitializeQTreeWorldSkirt() {
#if XBDEBUG
        var debug = new XB.DebugTimedBlock(XB.D.ManagerTerrainInitializeQTreeWorldSkirt);
#endif

        int amountX = (int)(_worldXSize*_resolution) + 1;
        int amountZ = (int)(_worldZSize*_resolution) + 1;
        var v3 = new Godot.Vector3(0.0f, 0.0f, -1.0f);
        for (int i = 0; i < amountX; i++) { // bottom
            _skNormals[(int)XB.Sk.B][2*i +0] = v3;
            _skNormals[(int)XB.Sk.B][2*i +1] = v3;
        }
        v3.Z = 1.0f;
        for (int i = 0; i < amountX; i++) { // top
            _skNormals[(int)XB.Sk.T][2*i +0] = v3;
            _skNormals[(int)XB.Sk.T][2*i +1] = v3;
        }
        v3.Z = 0.0f;
        v3.X = -1.0f;
        for (int i = 0; i < amountZ; i++) { // left
            _skNormals[(int)XB.Sk.L][2*i +0] = v3;
            _skNormals[(int)XB.Sk.L][2*i +1] = v3;
        }
        v3.X = 1.0f;
        for (int i = 0; i < amountZ; i++) { // right
            _skNormals[(int)XB.Sk.R][2*i +0] = v3;
            _skNormals[(int)XB.Sk.R][2*i +1] = v3;
        }

        int tri  = 0;
        int vert = 0;
        for (int i = 0; i < amountX-1; i++) {
            _skTriangles[(int)XB.Sk.B][tri + 0] = vert;
            _skTriangles[(int)XB.Sk.B][tri + 1] = vert + 1;
            _skTriangles[(int)XB.Sk.B][tri + 2] = vert + 3;
            _skTriangles[(int)XB.Sk.B][tri + 3] = vert;
            _skTriangles[(int)XB.Sk.B][tri + 4] = vert + 3;
            _skTriangles[(int)XB.Sk.B][tri + 5] = vert + 2;

            _skTriangles[(int)XB.Sk.T][tri + 0] = vert + 2;
            _skTriangles[(int)XB.Sk.T][tri + 1] = vert + 3;
            _skTriangles[(int)XB.Sk.T][tri + 2] = vert;
            _skTriangles[(int)XB.Sk.T][tri + 3] = vert + 1;
            _skTriangles[(int)XB.Sk.T][tri + 4] = vert;
            _skTriangles[(int)XB.Sk.T][tri + 5] = vert + 3;
            tri  += 6;
            vert += 2;
        }
        tri  = 0;
        vert = 0;
        for (int i = 0; i < amountZ-1; i++) {
            _skTriangles[(int)XB.Sk.L][tri + 0] = vert;
            _skTriangles[(int)XB.Sk.L][tri + 1] = vert + 1;
            _skTriangles[(int)XB.Sk.L][tri + 2] = vert + 3;
            _skTriangles[(int)XB.Sk.L][tri + 3] = vert;
            _skTriangles[(int)XB.Sk.L][tri + 4] = vert + 3;
            _skTriangles[(int)XB.Sk.L][tri + 5] = vert + 2;

            _skTriangles[(int)XB.Sk.R][tri + 0] = vert + 2;
            _skTriangles[(int)XB.Sk.R][tri + 1] = vert + 3;
            _skTriangles[(int)XB.Sk.R][tri + 2] = vert;
            _skTriangles[(int)XB.Sk.R][tri + 3] = vert + 1;
            _skTriangles[(int)XB.Sk.R][tri + 4] = vert;
            _skTriangles[(int)XB.Sk.R][tri + 5] = vert + 3;
            tri  += 6;
            vert += 2;
        }

        foreach(XB.Sk sk in System.Enum.GetValues(typeof(XB.Sk))) {
            _meshDataSk[(int)sk][(int)Godot.Mesh.ArrayType.Normal] = _skNormals  [(int)sk];
            _meshDataSk[(int)sk][(int)Godot.Mesh.ArrayType.Index]  = _skTriangles[(int)sk];
        }

#if XBDEBUG
        debug.End();
#endif 
    }

    //TODO[ALEX]: how to read skirt mesh vertex y information?
    //            update an array everytime an edge tile gets calculated?
    // the world skirt mesh is always at full resolution, but adopts to the actual tile edges
    private static void UpdateQTreeWorldSkirt() {
#if XBDEBUG
        var debug = new XB.DebugTimedBlock(XB.D.ManagerTerrainUpdateQTreeWorldSkirt);
#endif

        int   amountX = (int)(_worldXSize*_resolution) + 1;
        int   amountZ = (int)(_worldZSize*_resolution) + 1;
        var   v3      = new Godot.Vector3(0.0f, 0.0f, 0.0f);
        float step    = 1.0f/_resolution;

        for (int i = 0; i < amountX; i++) {
            v3.X = i*step;

            v3.Z = 0.0f;
            v3.Y = 0.0f;
            _skVertices[(int)XB.Sk.B][2*i+0] = v3;
            v3.Y = XB.WorldData.KillPlane;
            _skVertices[(int)XB.Sk.B][2*i+1] = v3;

            v3.Z = _worldZSize;
            v3.Y = 0.0f;
            _skVertices[(int)XB.Sk.T][2*i+0] = v3;
            v3.Y = XB.WorldData.KillPlane;
            _skVertices[(int)XB.Sk.T][2*i+1] = v3;
        }

        for (int i = 0; i < amountZ; i++) {
            v3.Z = i*step;

            v3.X = _worldXSize;
            v3.Y = 0.0f;
            _skVertices[(int)XB.Sk.L][2*i+0] = v3;
            v3.Y = XB.WorldData.KillPlane;
            _skVertices[(int)XB.Sk.L][2*i+1] = v3;

            v3.X = 0.0f;
            v3.Y = 0.0f;
            _skVertices[(int)XB.Sk.R][2*i+0] = v3;
            v3.Y = XB.WorldData.KillPlane;
            _skVertices[(int)XB.Sk.R][2*i+1] = v3;
        }
        
        foreach(XB.Sk sk in System.Enum.GetValues(typeof(XB.Sk))) {
            _meshDataSk[(int)sk][(int)Godot.Mesh.ArrayType.Vertex] = _skVertices[(int)sk];

            _arrMeshSk[(int)sk].ClearSurfaces();
            _arrMeshSk[(int)sk].AddSurfaceFromArrays(Godot.Mesh.PrimitiveType.Triangles,
                                                     _meshDataSk[(int)sk]               );
            _terrainSkirtMesh[(int)sk].Mesh = _arrMeshSk[(int)sk];
            _terrainSkirtMesh[(int)sk].Mesh.SurfaceSetMaterial(0, _terrainSkirtMat);
        }

#if XBDEBUG
        debug.End();
#endif 
    }

    private static void RecycleChildMesh(ref XB.QNode qNode) {
        if (qNode == null) { return; }
#if XBDEBUG
        var debug = new XB.DebugTimedBlock(XB.D.ManagerTerrainRecycleChildMesh);
#endif

        RecycleMeshContainer(ref qNode);
        RecycleChildMesh(ref qNode.Children[0]);
        RecycleChildMesh(ref qNode.Children[1]);
        RecycleChildMesh(ref qNode.Children[2]);
        RecycleChildMesh(ref qNode.Children[3]);

#if XBDEBUG
        debug.End();
#endif 
    }

    private static void RequestMeshContainer(ref XB.QNode qNode) {
#if XBDEBUG
        var debug = new XB.DebugTimedBlock(XB.D.ManagerTerrainRequestMeshContainer);
#endif

        for (int i = 0; i < _terrainMeshes.Count; i++) {
            if (!_terrainMeshes[i].InUse) {
                qNode.AssignMeshContainer(_terrainMeshes[i]);
                _terrainMeshes[i].UseMesh(qNode.XSize, qNode.ZSize, qNode.Res);
#if XBDEBUG
                debug.End();
#endif 
                return;
            }
        }

        int newID = _terrainMeshes.Count;
        var mC    = new XB.MeshContainer(XB.AData.MainRoot, newID,
                                         qNode.XPos/_worldXSize, qNode.ZPos/_worldZSize);
        _terrainMeshes.Add(mC);
        qNode.AssignMeshContainer(_terrainMeshes[newID]);
        _terrainMeshes[newID].UseMesh(qNode.XSize, qNode.ZSize, qNode.Res);

#if XBDEBUG
        debug.End();
#endif 
    }

    private static void RecycleMeshContainer(ref XB.QNode qNode) {
#if XBDEBUG
        var debug = new XB.DebugTimedBlock(XB.D.ManagerTerrainRecycleMeshContainer);
#endif

        //Godot.GD.Print("RecycleTerrainMeshContainer " + qNode.ID);
        qNode.ReleaseMeshContainer();

#if XBDEBUG
        debug.End();
#endif 
    }

#if XBDEBUG
    private static void PrintQNode(ref XB.QNode qNode) {
        string print = "Print Quadtree Node: " + qNode.ID.ToString() + '\n';
        print += "Ctr Pos: " + qNode.XPos.ToString() + "m " + qNode.ZPos.ToString() + "m, ";
        print += "Size: " + qNode.XSize.ToString() + "m " + qNode.ZSize.ToString() + "m, ";
        print += "Resolution: " + qNode.Res.ToString() + "/m\n";
        print += "Has Parent: ";
        if (qNode.Parent == null) { print += "No, "; }
        else                      { print += "Yes, ID: " + qNode.Parent.ID.ToString() + ", "; }
        print += '\n' + "Has Children: ";
        if (qNode.Children[0] == null) { print += "No"; }
        else                           { 
            print += "Yes, IDs: "; 
            for (int i = 0; i < 4; i++) {
                print += qNode.Children[i].ID.ToString() + ", ";
            }
        }
        print += '\n';
        Godot.GD.Print(print);
    }

    //prints tree depth first
    private static void PrintQTree(ref XB.QNode qNode) {
        if (qNode == null) { return; }
        PrintQNode(ref qNode);

        PrintQTree(ref qNode.Children[0]);
        PrintQTree(ref qNode.Children[1]);
        PrintQTree(ref qNode.Children[2]);
        PrintQTree(ref qNode.Children[3]);
    }

    public static void UpdateQTreeTexture(ref Godot.Image tex, float scaleFactor) {
        var rects = new Godot.Rect2I[4];
        var vect  = new Godot.Vector2I(0, 0);
        for (int i = 0; i < 4; i++) { rects[i] = new Godot.Rect2I(0, 0, 0, 0); }
        tex.Fill(XB.Col.Transp); // clear texture before drawing quadtree tiles

        DrawQNode(ref _qRoot, ref tex, scaleFactor, _divisions, ref rects, ref vect);
    }

    // scaleFactor adjust meter to pixel ratio
    private static void DrawQNode(ref XB.QNode qNode, ref Godot.Image tex,
                                  float scaleFactor, int iteration,
                                  ref Godot.Rect2I[] rects, ref Godot.Vector2I vect    ) {
        if (qNode == null) { return; }

        if (qNode.Visible) {
            // texture has 0|0 in top left, in world coordinates, "bottom right" has 0|0
            int xSz  = tex.GetWidth(); 
            int ySz  = tex.GetHeight(); 
            int xCtr = (int)(qNode.XPos*scaleFactor);
            int yCtr = (int)(qNode.ZPos*scaleFactor);
            int dx   = (int)(qNode.XSize*scaleFactor);
            int dy   = (int)(qNode.ZSize*scaleFactor);
            int t    = 1;
            XB.Utils.UpdateRect2I(xSz-xCtr-dx/2,      ySz-yCtr-dy/2,      dx, t,  ref rects[0], ref vect);
            XB.Utils.UpdateRect2I(xSz-xCtr-dx/2,      ySz-yCtr-dy/2+dy-t, dx, t,  ref rects[1], ref vect);
            XB.Utils.UpdateRect2I(xSz-xCtr-dx/2,      ySz-yCtr-dy/2,      t,  dy, ref rects[2], ref vect);
            XB.Utils.UpdateRect2I(xSz-xCtr-dx/2+dx-t, ySz-yCtr-dy/2,      t,  dy, ref rects[3], ref vect);

            var col = XB.Col.Red.Lerp(XB.Col.Green, (float)iteration/(float)_divisions);
            for (int i = 0; i < 4; i++) { tex.FillRect(rects[i], col); }
        } else {
            DrawQNode(ref qNode.Children[0], ref tex, scaleFactor, iteration-1, ref rects, ref vect);
            DrawQNode(ref qNode.Children[1], ref tex, scaleFactor, iteration-1, ref rects, ref vect);
            DrawQNode(ref qNode.Children[2], ref tex, scaleFactor, iteration-1, ref rects, ref vect);
            DrawQNode(ref qNode.Children[3], ref tex, scaleFactor, iteration-1, ref rects, ref vect);
        }
    }
#endif
}
} // namespace close
