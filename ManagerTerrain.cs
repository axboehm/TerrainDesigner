#define XBDEBUG
// #define XBVISUALIZECOLLIDERS
using SysCG = System.Collections.Generic;
namespace XB { // namespace open

// QNode is a node in a quadtree that represents the terrain of the world
// MeshContainer is a separate object that hold all the data of the mesh,
// QNode itself is very light and only has a reference to all the actual mesh data,
// MeshContainer is quite heavy with all the data
// the entire tree gets created at initialization, since MeshContainers are not assigned
// at creation, the tree can be thought of as sparse, as each node only contains a few
// variables and references
// MeshContainers are assigned to QNodes as needed in TerrainManager
// by keeping QNode light, creating the entire tree does not become a performance or memory issue,
// but having the entire tree allows for the distance based calculations in TerrainManager
// to be much simpler and to use recursion when traversing the tree
public class QNode {
    public QNode   Parent;    // reference to parent node
    public QNode[] Children;  // references to four child nodes
    public XB.MeshContainer MeshContainer; // holds all data regarding the visible mesh
    public bool  Active;      // is the tile supposed to be active (based on distance to reference)
    public bool  MeshVisible; // is the mesh of the tile currently visible (independent of Active)
    public bool  MeshReady;   // is the mesh assigned and ready (processing of meshes is staggered)
    public int   ID;    // unique ID for each node assigned at startup
    public float XPos;  // x center coordinate of mesh tile represented by this QNode in meter
    public float ZPos;
    public float XSize; // dimensions of mesh tile represented by this QNode in meter
    public float ZSize;
    public float Res;   // subdivisions per meter of mesh tile represented by this QNode
    private bool[] _worldEdge; // is either side of the tile part of the world edge

    // creates a new quadtree node, increases id for the next node's creation
    public QNode(ref int id, float xPos, float zPos, float xSize, float zSize,
                 float res, QNode parent = null                               ) {
#if XBDEBUG
        var debug = new XB.DebugTimedBlock(XB.D.QNode);
#endif

        Parent   = parent;
        Children = new QNode[4];
        for (int i = 0; i < 4; i++) { Children[i] = null; }

        MeshContainer = null;

        Active      = false;
        MeshVisible = false;
        MeshReady   = false;

        ID    = id;
        id   += 1;
        XPos  = xPos;
        ZPos  = zPos;
        XSize = xSize;
        ZSize = zSize;
        Res   = res;

        _worldEdge = new bool[4];

#if XBDEBUG
        debug.End();
#endif 
    }

    // recursively free all child node's MeshContainers and
    // set them to null so that the C# garbage collector can free the objects
    // root node needs to manually be treated afterwards
    public void DeleteRecursively() {
        if (Children[0] == null) { return; }
        Children[0].DeleteRecursively();
        Children[0].ReleaseMeshContainer();
        Children[0] = null;
        Children[1].DeleteRecursively();
        Children[1].ReleaseMeshContainer();
        Children[1] = null;
        Children[2].DeleteRecursively();
        Children[2].ReleaseMeshContainer();
        Children[2] = null;
        Children[3].DeleteRecursively();
        Children[3].ReleaseMeshContainer();
        Children[3] = null;
    }

    // when a node gets activated, all children should be deactivated
    public void Activate() {
#if XBDEBUG
        var debug = new XB.DebugTimedBlock(XB.D.QNodeActivate);
#endif

        Active = true;
        if (Children[0] == null) { return; }
        Children[0].DeActivate();
        Children[1].DeActivate();
        Children[2].DeActivate();
        Children[3].DeActivate();

#if XBDEBUG
        debug.End();
#endif 
    }

    public void DeActivate() {
#if XBDEBUG
        var debug = new XB.DebugTimedBlock(XB.D.QNodeDeActivate);
#endif

        Active = false;
        if (Children[0] == null) { return; }
        Children[0].DeActivate();
        Children[1].DeActivate();
        Children[2].DeActivate();
        Children[3].DeActivate();

#if XBDEBUG
        debug.End();
#endif 
    }

    // are all immediate children active and have their meshes ready (assigned and updated)
    public bool ChildrenActiveAndReady() {
#if XBDEBUG
        var debug = new XB.DebugTimedBlock(XB.D.QNodeChildrenActiveAndReady);
        debug.End();
#endif

        if (Children[0] == null) { return false; }

        if (   !Children[0].Active && !Children[1].Active
            && !Children[1].Active && !Children[3].Active) { return false; }

        if (   !Children[0].MeshReady || !Children[1].MeshReady
            || !Children[2].MeshReady || !Children[3].MeshReady) { return false; }

        return true;
    }

    // are any children (recursively until leaf node) ready
    // relevant for initialization, as the root node gets shown first for performance reasons,
    // then gets replaced when children are ready
    public bool ChildrenActiveRecursive() {
#if XBDEBUG
        var debug = new XB.DebugTimedBlock(XB.D.QNodeChildrenActiveAndReadyRecursive);
        debug.End();
#endif

        if (Children[0] == null) { return false; }
        if (   Children[0].Active || Children[1].Active
            || Children[2].Active || Children[3].Active) { return true; }

        if (Children[0].ChildrenActiveRecursive()) { return true; }
        if (Children[1].ChildrenActiveRecursive()) { return true; }
        if (Children[2].ChildrenActiveRecursive()) { return true; }
        if (Children[3].ChildrenActiveRecursive()) { return true; }

        return false;
    }

    // are all children (recursively until leaf node) that are active also ready
    public bool ChildrenReadyRecursive() {
#if XBDEBUG
        var debug = new XB.DebugTimedBlock(XB.D.QNodeChildrenActiveAndReadyRecursive);
        debug.End();
#endif
        if (Children[0] == null) {
            if (!Active)              { return true; }
            if (Active && MeshReady)  { return true; }
            if (Active && !MeshReady) { return false; }
        }
        if (   (Children[0].Active && !Children[0].MeshReady)
            || (Children[1].Active && !Children[1].MeshReady)
            || (Children[2].Active && !Children[2].MeshReady)
            || (Children[3].Active && !Children[3].MeshReady)) { return false; }

        if (!Children[0].ChildrenReadyRecursive()) { return false; }
        if (!Children[1].ChildrenReadyRecursive()) { return false; }
        if (!Children[2].ChildrenReadyRecursive()) { return false; }
        if (!Children[3].ChildrenReadyRecursive()) { return false; }

        return true;
    }

#if XBDEBUG
    public void DebugPrint(string note) {
        string print = "Print Quadtree Node: " + ID.ToString() + " " + note + ", ";
        print += "Active: " + Active;
        print += ", MeshVisible: " + MeshVisible;
        print += ", MeshReady: " + MeshReady;
        print += ", Ctr Pos: " + XPos.ToString() + "m, " + ZPos.ToString() + "m, ";
        print += "Size: " + XSize.ToString() + "m, " + ZSize.ToString() + "m, ";
        print += "Resolution: " + Res.ToString() + "/m, ";
        print += "Has Parent: ";
        if (Parent == null) { print += "No, "; }
        else                      { print += "Yes, ID: " + Parent.ID.ToString() + ", "; }
        print += ", Has Children: ";
        if (Children[0] == null) { print += "No"; }
        else                           { 
            print += "Yes, IDs: "; 
            for (int i = 0; i < 4; i++) {
                print += Children[i].ID.ToString() + ", ";
            }
        }
        Godot.GD.Print(print);
    }
#endif

    public void AssignMeshContainer(XB.MeshContainer mC) {
#if XBDEBUG
        var debug = new XB.DebugTimedBlock(XB.D.QNodeAssignMeshContainer);
#endif

        MeshContainer = mC;
        MeshContainer.Use();

#if XBDEBUG
        debug.End();
#endif 
    }

    public void ShowMeshContainer() {
#if XBDEBUG
        var debug = new XB.DebugTimedBlock(XB.D.QNodeShowMeshContainer);
#endif

        // Godot.GD.Print("ShowMeshContainer " + ID);
        MeshVisible = true;
        MeshReady   = false;
        MeshContainer.ShowMesh();

#if XBDEBUG
        debug.End();
#endif 
    }

    public void ReleaseMeshContainer() {
#if XBDEBUG
        var debug = new XB.DebugTimedBlock(XB.D.QNodeReleaseMeshContainer);
#endif

        // Godot.GD.Print("ReleaseMeshContainer " + ID);
        if (MeshContainer != null) {
            MeshContainer.ReleaseMesh();
            MeshContainer = null;
        }
        MeshVisible = false;
        MeshReady   = false;

#if XBDEBUG
        debug.End();
#endif 
    }

    // update the assigned meshcontainers mesh to represent the terrain based on a given heightmap
    // this resamples the heightmap and updates based on that
    // also assigns shaders to that tile's edgeskirt based on whether it is part of the world edge
    public void UpdateAssignedMesh(float worldRes, float worldXSize, float worldZSize,
                                   float lowestPoint, float highestPoint,
                                   Godot.Image imgHeightMap                           ) {
#if XBDEBUG
        var debug = new XB.DebugTimedBlock(XB.D.QNodeUpdateAssignedMesh);
#endif

        MeshContainer.SampleTerrainNoise(XPos, ZPos, XSize, ZSize, worldRes,
                                         Res, lowestPoint, highestPoint, imgHeightMap);

        if (worldZSize-(-ZPos+ZSize/2.0f) < XB.Constants.Epsilon) { _worldEdge[(int)XB.Sk.ZM] = true;  }
        else                                                      { _worldEdge[(int)XB.Sk.ZM] = false; }
        if (            -ZPos-ZSize/2.0f  < XB.Constants.Epsilon) { _worldEdge[(int)XB.Sk.ZP] = true;  }
        else                                                      { _worldEdge[(int)XB.Sk.ZP] = false; }
        if (worldXSize-(-XPos+XSize/2.0f) < XB.Constants.Epsilon) { _worldEdge[(int)XB.Sk.XM] = true;  }
        else                                                      { _worldEdge[(int)XB.Sk.XM] = false; }
        if (            -XPos-XSize/2.0f  < XB.Constants.Epsilon) { _worldEdge[(int)XB.Sk.XP] = true;  }
        else                                                      { _worldEdge[(int)XB.Sk.XP] = false; }

        MeshContainer.ApplyToMesh(_worldEdge);
        MeshReady = true;

#if XBDEBUG
        debug.End();
#endif 
    }
}

// directions of mesh skirt (used to ensure consistency)
public enum Sk {
    ZM, // bottom edge
    ZP, // top edge
    XM, // right edge
    XP, // left edge
}

// each MeshContainer represents the visible mesh data of one terrain tile
// they are created as required but never destroyed, rather they are hidden and made available
// again for when another MeshContainer gets requested
// since the tiles can have different resolutions to their neighbors,
// gaps can appear between the tiles, to prevent this,
// each tile gets a mesh skirt, an extension of the edges downwards to hide the gap
// this also leads to inconsistent normals across tile edges as only that tile's faces
// are considered for its normals, to consider the neighboring tile's faces as well adds a lot
// of code complexity for a barely visual difference so it is not done
public class MeshContainer {
    public int  XAmount; // vertices along x axis
    public int  ZAmount;
    public int  ID;
    public bool InUse;   // currently assigned to a QNode or not
    public Godot.MeshInstance3D      MeshInst; // holds tile and skirt meshes
    public Godot.Collections.Array   MeshDataTile;
    public Godot.Collections.Array[] MeshDataSkirt;
    public Godot.ArrayMesh           ArrMesh;
    public Godot.ShaderMaterial      MaterialTile;
    public Godot.ShaderMaterial      MaterialSkirt;
    public Godot.Vector3[]   VerticesTile;
    public Godot.Vector3[][] VerticesSkirt; // four arrays, one for each side
    public Godot.Vector2[]   UVsTile;
    public Godot.Vector2[][] UVsSkirt;
    public Godot.Vector3[]   NormalsTile;
    public Godot.Vector3[][] NormalsSkirt;
    public int[]             TrianglesTile;
    public int[][]           TrianglesSkirt;
    private const float _skirtLength = 8.0f; // vertical extension of mesh skirt in meter (not world edge)
    private Godot.Vector2 _v2;
    private Godot.Vector3 _v3;

    // when a MeshContainer gets created, the relevant shaders are prepared and mesh arrays
    // get initialized
    // lerpRAmount and lerpGAmount are used to define the color the mesh gets when visualizing it
    // when the MeshContainer gets released and subsequently reused, its color does not get updated
    public MeshContainer(Godot.Node root, int id, float lerpRAmount, float lerpGAmount) {
#if XBDEBUG
        var debug = new XB.DebugTimedBlock(XB.D.MeshContainer);
#endif

        MeshInst = new Godot.MeshInstance3D();
        root.AddChild(MeshInst);

        MaterialTile        = new Godot.ShaderMaterial();
        MaterialTile.Shader = Godot.ResourceLoader.Load<Godot.Shader>(XB.ResourcePaths.TerrainShader);
        MaterialTile.SetShaderParameter("tBlock",     XB.Resources.BlockTex);
        MaterialTile.SetShaderParameter("blockStr",   XB.WData.BlockStrength);
        MaterialTile.SetShaderParameter("tNoiseP1",   XB.Resources.NoiseBombing);
        MaterialTile.SetShaderParameter("tNoiseP2",   XB.Resources.NoiseModulation);
        MaterialTile.SetShaderParameter("tAlbedoM1",  XB.Resources.Terrain1CATex);
        MaterialTile.SetShaderParameter("tRMM1",      XB.Resources.Terrain1RMTex);
        MaterialTile.SetShaderParameter("tNormalM1",  XB.Resources.Terrain1NTex );
        MaterialTile.SetShaderParameter("tHeightM1",  XB.Resources.Terrain1HTex );
        MaterialTile.SetShaderParameter("tAlbedoM2",  XB.Resources.Terrain2CATex);
        MaterialTile.SetShaderParameter("tRMM2",      XB.Resources.Terrain2RMTex);
        MaterialTile.SetShaderParameter("tNormalM2",  XB.Resources.Terrain2NTex );
        MaterialTile.SetShaderParameter("tHeightM2",  XB.Resources.Terrain2HTex );
        MaterialTile.SetShaderParameter("tAlbedoM3",  XB.Resources.Terrain3CATex);
        MaterialTile.SetShaderParameter("tRMM3",      XB.Resources.Terrain3RMTex);
        MaterialTile.SetShaderParameter("tNormalM3",  XB.Resources.Terrain3NTex );
        MaterialTile.SetShaderParameter("tHeightM3",  XB.Resources.Terrain3HTex );
        MaterialTile.SetShaderParameter("tAlbedoM4",  XB.Resources.Terrain4CATex);
        MaterialTile.SetShaderParameter("tRMM4",      XB.Resources.Terrain4RMTex);
        MaterialTile.SetShaderParameter("tNormalM4",  XB.Resources.Terrain4NTex );
        MaterialTile.SetShaderParameter("tHeightM4",  XB.Resources.Terrain4HTex );
        MaterialTile.SetShaderParameter("tColShift",  XB.Resources.ColorShiftTex);
        //MaterialTile.SetShaderParameter("colFog",     XB.Col.Fog); //NOTE[ALEX]: see terrain.gdshader
        //MaterialTile.SetShaderParameter("fogDist",    XB.WData.FogDistance);
        // visualization colors initially represent somewhat of a gradient 
        // but will quickly get shuffled around as MeshContainers get reused
        float r = 1.0f - XB.Utils.LerpF(0.0f, 1.0f, -lerpRAmount);
        float g =        XB.Utils.LerpF(0.0f, 1.0f, -lerpGAmount);
        float b = XB.Random.RandomInRangeF(0.0f, 1.0f);
        var col = new Godot.Color(r, g, b, 1.0f);
        MaterialTile.SetShaderParameter("albVis",      col);
        MaterialTile.SetShaderParameter("albVisStr",   XB.WData.QTreeStrength);
        MaterialTile.SetShaderParameter("blendDepth",  XB.WData.BlendDepth);
        MaterialTile.SetShaderParameter("blendWidth",  XB.WData.BlendWidth);
        MaterialTile.SetShaderParameter("blend12",     XB.WData.Blend12);
        MaterialTile.SetShaderParameter("blend23",     XB.WData.Blend23);
        MaterialTile.SetShaderParameter("blend34",     XB.WData.Blend34);
        MaterialTile.SetShaderParameter("pointyStr",   XB.WData.PointinessStr);
        MaterialTile.SetShaderParameter("pointyPow",   XB.WData.PointinessPow);
        MaterialTile.SetShaderParameter("blendCStr",   XB.WData.BlendColStr);
        MaterialTile.SetShaderParameter("blendCScale", XB.WData.BlendColScale);

        MaterialSkirt = new Godot.ShaderMaterial();
        MaterialSkirt.Shader = Godot.ResourceLoader.Load<Godot.Shader>(XB.ResourcePaths.TSkirtShader);

        MeshDataTile  = new Godot.Collections.Array();
        MeshDataTile.Resize((int)Godot.Mesh.ArrayType.Max);

        MeshDataSkirt                = new Godot.Collections.Array[4];
        MeshDataSkirt[(int)XB.Sk.ZM] = new Godot.Collections.Array();
        MeshDataSkirt[(int)XB.Sk.ZM].Resize((int)Godot.Mesh.ArrayType.Max);
        MeshDataSkirt[(int)XB.Sk.ZP] = new Godot.Collections.Array();
        MeshDataSkirt[(int)XB.Sk.ZP].Resize((int)Godot.Mesh.ArrayType.Max);
        MeshDataSkirt[(int)XB.Sk.XM] = new Godot.Collections.Array();
        MeshDataSkirt[(int)XB.Sk.XM].Resize((int)Godot.Mesh.ArrayType.Max);
        MeshDataSkirt[(int)XB.Sk.XP] = new Godot.Collections.Array();
        MeshDataSkirt[(int)XB.Sk.XP].Resize((int)Godot.Mesh.ArrayType.Max);
        ArrMesh = new Godot.ArrayMesh();

        XAmount = 0;
        ZAmount = 0;
        ID      = id;
        InUse   = true;

        _v2 = new Godot.Vector2(0.0f, 0.0f);
        _v3 = new Godot.Vector3(0.0f, 0.0f, 0.0f);

#if XBDEBUG
        debug.End();
#endif 
    }

    public void ReleaseMesh() {
#if XBDEBUG
        var debug = new XB.DebugTimedBlock(XB.D.MeshContainerReleaseMesh);
#endif

        // Godot.GD.Print("ReleaseMesh");
        MeshInst.Hide();
        InUse = false;

#if XBDEBUG
        debug.End();
#endif 
    }

    // create mesh arrays of the required sizes
    // fill mesh arrays if they do not changed based on the heightmap (all except vertices and normals)
    public void UseMesh(float xPos, float zPos, float xSize, float zSize,
                        float xWorldSize, float zWorldSize, float res    ) {
#if XBDEBUG
        var debug = new XB.DebugTimedBlock(XB.D.MeshContainerUseMesh);
#endif

        int xAmount = (int)(xSize*res) + 1;
        int zAmount = (int)(zSize*res) + 1;

        // tile (without skirt)
        XAmount = xAmount;
        ZAmount = zAmount;
        VerticesTile  = new Godot.Vector3[XAmount*ZAmount];
        UVsTile       = new Godot.Vector2[XAmount*ZAmount];
        NormalsTile   = new Godot.Vector3[XAmount*ZAmount];
        TrianglesTile = new int[(XAmount-1)*(ZAmount-1)*6];

        XB.Utils.ResetV2(ref _v2);
        float uvStartX = -(xPos-xSize/2.0f)/xWorldSize;
        float uvEndX   = -(xPos+xSize/2.0f)/xWorldSize;
        float uvStartY = -(zPos-zSize/2.0f)/zWorldSize;
        float uvEndY   = -(zPos+zSize/2.0f)/zWorldSize;
        for (int i = 0; i < UVsTile.Length; i++) {
            int x = i%XAmount;
            int y = i/XAmount;
            _v2.X = XB.Utils.LerpF(uvStartX, uvEndX, (float)x/(float)(XAmount-1));
            _v2.Y = XB.Utils.LerpF(uvStartY, uvEndY, (float)y/(float)(ZAmount-1));
            UVsTile[i] = _v2;
        }

        int tri  = 0;
        int vert = 0;
        for (int j = 0; j < ZAmount-1; j++) {
            for (int i = 0; i < XAmount-1; i++) {
                TrianglesTile[tri + 0] = vert;
                TrianglesTile[tri + 1] = vert + 1;
                TrianglesTile[tri + 2] = vert + 1 + XAmount;
                TrianglesTile[tri + 3] = vert;
                TrianglesTile[tri + 4] = vert + 1 + XAmount;
                TrianglesTile[tri + 5] = vert +     XAmount;
                tri  += 6;
                vert += 1;
            }
            vert += 1;
        }

        MeshDataTile[(int)Godot.Mesh.ArrayType.TexUV] = UVsTile;
        MeshDataTile[(int)Godot.Mesh.ArrayType.Index] = TrianglesTile;

        // skirt: each edge has two vertices per tile edge vertex, none are shared with the tile
        VerticesSkirt                = new Godot.Vector3[4][];
        VerticesSkirt[(int)XB.Sk.XM] = new Godot.Vector3[2*XAmount];
        VerticesSkirt[(int)XB.Sk.XP] = new Godot.Vector3[2*XAmount];
        VerticesSkirt[(int)XB.Sk.ZM] = new Godot.Vector3[2*ZAmount];
        VerticesSkirt[(int)XB.Sk.ZP] = new Godot.Vector3[2*ZAmount];
        UVsSkirt                = new Godot.Vector2[4][];
        UVsSkirt[(int)XB.Sk.XM] = new Godot.Vector2[2*XAmount];
        UVsSkirt[(int)XB.Sk.XP] = new Godot.Vector2[2*XAmount];
        UVsSkirt[(int)XB.Sk.ZM] = new Godot.Vector2[2*ZAmount];
        UVsSkirt[(int)XB.Sk.ZP] = new Godot.Vector2[2*ZAmount];
        NormalsSkirt                = new Godot.Vector3[4][];
        NormalsSkirt[(int)XB.Sk.XM] = new Godot.Vector3[2*XAmount];
        NormalsSkirt[(int)XB.Sk.XP] = new Godot.Vector3[2*XAmount];
        NormalsSkirt[(int)XB.Sk.ZM] = new Godot.Vector3[2*ZAmount];
        NormalsSkirt[(int)XB.Sk.ZP] = new Godot.Vector3[2*ZAmount];
        TrianglesSkirt                = new int[4][];
        TrianglesSkirt[(int)XB.Sk.XM] = new int[(XAmount-1)*6];
        TrianglesSkirt[(int)XB.Sk.XP] = new int[(XAmount-1)*6];
        TrianglesSkirt[(int)XB.Sk.ZM] = new int[(ZAmount-1)*6];
        TrianglesSkirt[(int)XB.Sk.ZP] = new int[(ZAmount-1)*6];

        //NOTE[ALEX]: uvs for Z- and X- are added in reverse order, also see SampleTerrainNoise
        for (int i = 0; i < XAmount; i++) {
            UVsSkirt[(int)XB.Sk.ZM][2*i+0] = UVsTile[i];
            UVsSkirt[(int)XB.Sk.ZM][2*i+1] = UVsTile[i];

            int inv = XAmount-1-i; // inverted index for Vertices array
            UVsSkirt[(int)XB.Sk.ZP][2*i+0] = UVsTile[inv + XAmount*ZAmount - XAmount];
            UVsSkirt[(int)XB.Sk.ZP][2*i+1] = UVsTile[inv + XAmount*ZAmount - XAmount];
        }
        for (int i = 0; i < ZAmount; i++) {
            int inv = ZAmount-1-i; // inverted index for Vertices array
            UVsSkirt[(int)XB.Sk.XM][2*i+0] = UVsTile[inv*XAmount];
            UVsSkirt[(int)XB.Sk.XM][2*i+1] = UVsTile[inv*XAmount];

            UVsSkirt[(int)XB.Sk.XP][2*i+0] = UVsTile[i*XAmount + XAmount-1];
            UVsSkirt[(int)XB.Sk.XP][2*i+1] = UVsTile[i*XAmount + XAmount-1];
        }

        SkirtTriangleIndices(TrianglesSkirt[(int)XB.Sk.ZM], XAmount-1);
        SkirtTriangleIndices(TrianglesSkirt[(int)XB.Sk.ZP], XAmount-1);
        SkirtTriangleIndices(TrianglesSkirt[(int)XB.Sk.XM], ZAmount-1);
        SkirtTriangleIndices(TrianglesSkirt[(int)XB.Sk.XP], ZAmount-1);

        MeshDataSkirt[(int)XB.Sk.ZM][(int)Godot.Mesh.ArrayType.TexUV] = UVsSkirt[(int)XB.Sk.ZM];
        MeshDataSkirt[(int)XB.Sk.ZP][(int)Godot.Mesh.ArrayType.TexUV] = UVsSkirt[(int)XB.Sk.ZP];
        MeshDataSkirt[(int)XB.Sk.XM][(int)Godot.Mesh.ArrayType.TexUV] = UVsSkirt[(int)XB.Sk.XM];
        MeshDataSkirt[(int)XB.Sk.XP][(int)Godot.Mesh.ArrayType.TexUV] = UVsSkirt[(int)XB.Sk.XP];

        MeshDataSkirt[(int)XB.Sk.ZM][(int)Godot.Mesh.ArrayType.Index] = TrianglesSkirt[(int)XB.Sk.ZM];
        MeshDataSkirt[(int)XB.Sk.ZP][(int)Godot.Mesh.ArrayType.Index] = TrianglesSkirt[(int)XB.Sk.ZP];
        MeshDataSkirt[(int)XB.Sk.XM][(int)Godot.Mesh.ArrayType.Index] = TrianglesSkirt[(int)XB.Sk.XM];
        MeshDataSkirt[(int)XB.Sk.XP][(int)Godot.Mesh.ArrayType.Index] = TrianglesSkirt[(int)XB.Sk.XP];

#if XBDEBUG
        debug.End();
#endif 
    }

    // separate from UseMesh, as the MeshContainer can be assigned but not updated yet
    // for a few frames (waiting in queue)
    public void Use() {
#if XBDEBUG
        var debug = new XB.DebugTimedBlock(XB.D.MeshContainerSampleTerrainNoise);
#endif

        InUse = true;

#if XBDEBUG
        debug.End();
#endif 
    }

    public void ShowMesh() {
#if XBDEBUG
        var debug = new XB.DebugTimedBlock(XB.D.MeshContainerSampleTerrainNoise);
#endif

#if XBVISUALIZECOLLIDERS
        MeshInst.Hide();
#else
        MeshInst.Show();
#endif 

#if XBDEBUG
        debug.End();
#endif 
    }

    // helper function to assign triangle indices for mesh skirt
    private void SkirtTriangleIndices(int[] triangles, int amount) {
#if XBDEBUG
        var debug = new XB.DebugTimedBlock(XB.D.MeshContainerSampleTerrainNoise);
#endif

        int tri  = 0;
        int vert = 0;
        for (int i = 0; i < amount; i++) {
            triangles[tri + 0] = vert;
            triangles[tri + 1] = vert + 1;
            triangles[tri + 2] = vert + 3;
            triangles[tri + 3] = vert;
            triangles[tri + 4] = vert + 3;
            triangles[tri + 5] = vert + 2;
            tri  += 6;
            vert += 2;
        }

#if XBDEBUG
        debug.End();
#endif 
    }

    // iterate thorugh all vertices and assign them correctly, sampling the heightmap
    // then calculate normals and assign mesh skirt vertices and normals
    public void SampleTerrainNoise(float xPos, float zPos, float xSize,  float zSize,
                                   float worldRes, float res,
                                   float lowestPoint, float highestPoint, 
                                   Godot.Image imgHeightMap                          ) {
#if XBDEBUG
        var debug = new XB.DebugTimedBlock(XB.D.MeshContainerSampleTerrainNoise);
#endif

        float height = XB.Utils.AbsF(highestPoint-lowestPoint);
        var pos = new Godot.Vector3(xPos, 0.0f, zPos);
        MeshInst.GlobalPosition = pos; // move MeshInstance to center of tile, mesh is child
        // Godot.GD.Print("MeshContainerSampleTerrainNoise: lp: " + lowestPoint + ", h: " + height);

        // tile (without skirt)
        XB.Utils.ResetV3(ref _v3);
        float step = 1.0f/res;
        float sampledNoise = 0.0f;
        int   vNumber = 0;
        for (int j = 0; j < ZAmount; j++) {
            for (int i = 0; i < XAmount; i++) {
                _v3.X = (float)i*step - xSize/2.0f;
                _v3.Z = (float)j*step - zSize/2.0f;
                sampledNoise = XB.Terrain.HeightMapSample(_v3.X + pos.X, _v3.Z + pos.Z,
                                                          worldRes, imgHeightMap       );
                _v3.Y = sampledNoise*height + lowestPoint;
                VerticesTile[vNumber] = _v3;
                vNumber += 1;
            }
        }

        XB.Terrain.CalculateNormals(NormalsTile, VerticesTile, TrianglesTile);

        // skirt vertices and normals (copied from edges of tile)
        //NOTE[ALEX]: if the vertices for opposing sides are added in the same order 
        //            e.g. for Z- and Z+ from X- to X+, then the triangle indices need to be inverted
        //            to use the same triangle indices for all four sides, the vertices and normals
        //            are added in reverse order
        for (int i = 0; i < XAmount; i++) {
            VerticesSkirt[(int)XB.Sk.ZM][2*i+0]    = VerticesTile[i];
            VerticesSkirt[(int)XB.Sk.ZM][2*i+1]    = VerticesTile[i];
            VerticesSkirt[(int)XB.Sk.ZM][2*i+1].Y -= _skirtLength;

            NormalsSkirt [(int)XB.Sk.ZM][2*i+0]    = NormalsTile [i];
            NormalsSkirt [(int)XB.Sk.ZM][2*i+1]    = NormalsTile [i];

            int inv = XAmount-1-i; // inverted index for Vertices array
            VerticesSkirt[(int)XB.Sk.ZP][2*i+0]    = VerticesTile[inv + XAmount*ZAmount - XAmount];
            VerticesSkirt[(int)XB.Sk.ZP][2*i+1]    = VerticesTile[inv + XAmount*ZAmount - XAmount];
            VerticesSkirt[(int)XB.Sk.ZP][2*i+1].Y -= _skirtLength;

            NormalsSkirt [(int)XB.Sk.ZP][2*i+0]    = NormalsTile [inv + XAmount*ZAmount - XAmount];
            NormalsSkirt [(int)XB.Sk.ZP][2*i+1]    = NormalsTile [inv + XAmount*ZAmount - XAmount];
        }
        for (int i = 0; i < ZAmount; i++) {
            int inv = ZAmount-1-i; // inverted index for Vertices array
            VerticesSkirt[(int)XB.Sk.XM][2*i+0]    = VerticesTile[inv*XAmount];
            VerticesSkirt[(int)XB.Sk.XM][2*i+1]    = VerticesTile[inv*XAmount];
            VerticesSkirt[(int)XB.Sk.XM][2*i+1].Y -= _skirtLength;

            NormalsSkirt [(int)XB.Sk.XM][2*i+0]    = NormalsTile [inv*XAmount];
            NormalsSkirt [(int)XB.Sk.XM][2*i+1]    = NormalsTile [inv*XAmount];

            VerticesSkirt[(int)XB.Sk.XP][2*i+0]    = VerticesTile[i * XAmount + XAmount-1];
            VerticesSkirt[(int)XB.Sk.XP][2*i+1]    = VerticesTile[i * XAmount + XAmount-1];
            VerticesSkirt[(int)XB.Sk.XP][2*i+1].Y -= _skirtLength;

            NormalsSkirt [(int)XB.Sk.XP][2*i+0]    = NormalsTile [i*XAmount + XAmount-1];
            NormalsSkirt [(int)XB.Sk.XP][2*i+1]    = NormalsTile [i*XAmount + XAmount-1];
        }

#if XBDEBUG
        debug.End();
#endif 
    }

    //NOTE[ALEX]: hard coded because it is very specific to the shader used
    public void SetTerrainShaderAttributes(Godot.ImageTexture miniMap) {
#if XBDEBUG
        var debug = new XB.DebugTimedBlock(XB.D.MeshContainerSetTerrainShaderAttributes);
#endif

        MaterialTile.SetShaderParameter("scaleX",      WData.WorldSize.X);
        MaterialTile.SetShaderParameter("scaleY",      WData.WorldSize.Y);
        MaterialTile.SetShaderParameter("blockScale",  WData.BlockUVScale);
        MaterialTile.SetShaderParameter("uv1Scale",    WData.Mat1UVScale);
        MaterialTile.SetShaderParameter("uv2Scale",    WData.Mat2UVScale);
        MaterialTile.SetShaderParameter("uv3Scale",    WData.Mat3UVScale);
        MaterialTile.SetShaderParameter("uv4Scale",    WData.Mat4UVScale);
        MaterialTile.SetShaderParameter("noisePScale", WData.NoisePScale);
        MaterialTile.SetShaderParameter("tHeight",     miniMap);
        MaterialTile.SetShaderParameter("tPointy",     XB.WData.TexPointiness);
        MaterialTile.SetShaderParameter("axisBlendSharpen", XB.WData.AxisBlSharpen);
        MaterialTile.SetShaderParameter("axisBlendWidth",   XB.WData.AxisBlWidth);
        MaterialTile.SetShaderParameter("axisBlendDepth",   XB.WData.AxisBlDepth);

#if XBDEBUG
        debug.End();
#endif 
    }

    public void SetShaderAttribute(string attName, float value) {
#if XBDEBUG
        var debug = new XB.DebugTimedBlock(XB.D.MeshContainerSetShaderAttribute);
#endif

        MaterialTile.SetShaderParameter(attName, value);

#if XBDEBUG
        debug.End();
#endif 
    }

    // adjust vertical position of lower vertices of edge skirt if they are part of the world edge,
    // also adjust their normals to face exactly outwards, creating a hard edge to the terrain
    private void AdjustWorldEdgeSkirt(XB.Sk direction) {
#if XBDEBUG
        var debug = new XB.DebugTimedBlock(XB.D.MeshContainerAdjustWorldEdgeSkirt);
#endif

        int d = (int)direction;
        XB.Utils.ResetV3(ref _v3);

        switch (direction) {
            case XB.Sk.ZM: { _v3.Z = -1.0f; break; }
            case XB.Sk.ZP: { _v3.Z = +1.0f; break; }
            case XB.Sk.XM: { _v3.X = -1.0f; break; }
            case XB.Sk.XP: { _v3.X = +1.0f; break; }
        }

        for (int i = 0; i < VerticesSkirt[d].Length/2; i++) {
            VerticesSkirt[d][2*i+1].Y = XB.WData.KillPlane;
            NormalsSkirt [d][2*i+0]   = _v3;
            NormalsSkirt [d][2*i+1]   = _v3;
        }

#if XBDEBUG
        debug.End();
#endif 
    }

    // apply all calculated arrays to the mesh instance
    // then apply relevant materials to tile and four skirt sides
    // this step is very slow so should definitely be staggered
    public void ApplyToMesh(bool[] worldEdge) {
#if XBDEBUG
        var debug = new XB.DebugTimedBlock(XB.D.MeshContainerApplyToMesh);
#endif

        MeshDataTile[(int)Godot.Mesh.ArrayType.Vertex] = VerticesTile;
        MeshDataTile[(int)Godot.Mesh.ArrayType.Normal] = NormalsTile;

        if (worldEdge[(int)XB.Sk.ZM]) { AdjustWorldEdgeSkirt(XB.Sk.ZM); }
        if (worldEdge[(int)XB.Sk.ZP]) { AdjustWorldEdgeSkirt(XB.Sk.ZP); }
        if (worldEdge[(int)XB.Sk.XM]) { AdjustWorldEdgeSkirt(XB.Sk.XM); }
        if (worldEdge[(int)XB.Sk.XP]) { AdjustWorldEdgeSkirt(XB.Sk.XP); }

        MeshDataSkirt[(int)XB.Sk.ZM][(int)Godot.Mesh.ArrayType.Vertex] = VerticesSkirt[(int)XB.Sk.ZM];
        MeshDataSkirt[(int)XB.Sk.ZP][(int)Godot.Mesh.ArrayType.Vertex] = VerticesSkirt[(int)XB.Sk.ZP];
        MeshDataSkirt[(int)XB.Sk.XM][(int)Godot.Mesh.ArrayType.Vertex] = VerticesSkirt[(int)XB.Sk.XM];
        MeshDataSkirt[(int)XB.Sk.XP][(int)Godot.Mesh.ArrayType.Vertex] = VerticesSkirt[(int)XB.Sk.XP];

        MeshDataSkirt[(int)XB.Sk.ZM][(int)Godot.Mesh.ArrayType.Normal] = NormalsSkirt [(int)XB.Sk.ZM];
        MeshDataSkirt[(int)XB.Sk.ZP][(int)Godot.Mesh.ArrayType.Normal] = NormalsSkirt [(int)XB.Sk.ZP];
        MeshDataSkirt[(int)XB.Sk.XM][(int)Godot.Mesh.ArrayType.Normal] = NormalsSkirt [(int)XB.Sk.XM];
        MeshDataSkirt[(int)XB.Sk.XP][(int)Godot.Mesh.ArrayType.Normal] = NormalsSkirt [(int)XB.Sk.XP];

        ArrMesh.ClearSurfaces();
        ArrMesh.AddSurfaceFromArrays(Godot.Mesh.PrimitiveType.Triangles, MeshDataTile);

        ArrMesh.AddSurfaceFromArrays(Godot.Mesh.PrimitiveType.Triangles, MeshDataSkirt[(int)XB.Sk.ZM]);
        ArrMesh.AddSurfaceFromArrays(Godot.Mesh.PrimitiveType.Triangles, MeshDataSkirt[(int)XB.Sk.ZP]);
        ArrMesh.AddSurfaceFromArrays(Godot.Mesh.PrimitiveType.Triangles, MeshDataSkirt[(int)XB.Sk.XM]);
        ArrMesh.AddSurfaceFromArrays(Godot.Mesh.PrimitiveType.Triangles, MeshDataSkirt[(int)XB.Sk.XP]);

        MeshInst.Mesh = ArrMesh;
        MeshInst.Mesh.SurfaceSetMaterial(0, MaterialTile);

        if (worldEdge[(int)XB.Sk.ZM]) {
            MeshInst.Mesh.SurfaceSetMaterial(1 + (int)XB.Sk.ZM, MaterialSkirt);
        } else {
            MeshInst.Mesh.SurfaceSetMaterial(1 + (int)XB.Sk.ZM, MaterialTile);
        }
        if (worldEdge[(int)XB.Sk.ZP]) {
            MeshInst.Mesh.SurfaceSetMaterial(1 + (int)XB.Sk.ZP, MaterialSkirt);
        } else {
            MeshInst.Mesh.SurfaceSetMaterial(1 + (int)XB.Sk.ZP, MaterialTile);
        }
        if (worldEdge[(int)XB.Sk.XM]) {
            MeshInst.Mesh.SurfaceSetMaterial(1 + (int)XB.Sk.XM, MaterialSkirt);
        } else {
            MeshInst.Mesh.SurfaceSetMaterial(1 + (int)XB.Sk.XM, MaterialTile);
        }
        if (worldEdge[(int)XB.Sk.XP]) {
            MeshInst.Mesh.SurfaceSetMaterial(1 + (int)XB.Sk.XP, MaterialSkirt);
        } else {
            MeshInst.Mesh.SurfaceSetMaterial(1 + (int)XB.Sk.XP, MaterialTile);
        }

#if XBVISUALIZECOLLIDERS
        MeshInst.Hide();
#endif 

#if XBDEBUG
        debug.End();
#endif 
    }
}

// collision for the world is also split into tiles
// they have a uniform resolution (as opposed to the visible mesh tiles)
// collision tiles are created at terrain generation for the entire terrain
// then when the heightmap changes they are all updated at once
// their updates can not be staggered as the player or other objects might potentially fall
// through the terrain if they are not available there yet
// to keep performance high, collision tiles have a lower resolution than the mesh tiles
// this can lead to the player clipping through the ground but the speedup is worth the trade off
public class CollisionTile {
    public int   XAmount; // vertices in x direction
    public int   ZAmount;
    public float XPos;  // x center coordinate of collision tile in meter
    public float ZPos;
    public float XSize; // x dimension in meter
    public float ZSize;
    public float Res;   // subdivisions per meter
    public Godot.Collections.Array MeshData;
    public Godot.ArrayMesh         ArrMesh;
    public Godot.StaticBody3D      StatBody;
    public Godot.CollisionShape3D  CollShape;
    public Godot.Vector3[] Vertices;
    public int[]           Triangles;
    private Godot.Vector3 _v3;
#if XBVISUALIZECOLLIDERS
    public Godot.MeshInstance3D MeshInstVis;
    public Godot.ShaderMaterial MaterialVis;
#endif 
    
    public CollisionTile(Godot.Node root, float xPos, float zPos, float xSize, float zSize,
                         float res, uint collisionLayer, uint collisionMask                ) {
#if XBDEBUG
        var debug = new XB.DebugTimedBlock(XB.D.CollisionTile);
#endif

        XPos    = xPos;
        ZPos    = zPos;
        XSize   = xSize;
        ZSize   = zSize;
        Res     = res;
        XAmount = (int)(XSize*Res) + 1;
        ZAmount = (int)(ZSize*Res) + 1;

        StatBody  = new Godot.StaticBody3D();
        CollShape = new Godot.CollisionShape3D();
        root.AddChild(StatBody);
        StatBody.AddChild(CollShape);
        StatBody.CollisionLayer = collisionLayer;
        StatBody.CollisionMask  = collisionMask;
        StatBody.GlobalPosition = new Godot.Vector3(xPos, 0.0f, zPos);

        MeshData = new Godot.Collections.Array();
        MeshData.Resize((int)Godot.Mesh.ArrayType.Max);
        ArrMesh  = new Godot.ArrayMesh();

        _v3 = new Godot.Vector3(0.0f, 0.0f, 0.0f);

        //DebugPrint();

#if XBVISUALIZECOLLIDERS
        MeshInstVis  = new Godot.MeshInstance3D();
        root.AddChild(MeshInstVis);
        MeshInstVis.GlobalPosition = new Godot.Vector3(xPos, 0.0f, zPos);
        MaterialVis        = new Godot.ShaderMaterial();
        MaterialVis.Shader = Godot.ResourceLoader.Load<Godot.Shader>(XB.ResourcePaths.TerrainShader);
        float r = XB.Random.RandomInRangeF(0.0f, 1.0f);
        float g = XB.Random.RandomInRangeF(0.0f, 1.0f);
        float b = XB.Random.RandomInRangeF(0.0f, 1.0f);
        var col = new Godot.Color(r, g, b, 1.0f);
        MaterialVis.SetShaderParameter("albVis",    col);
        MaterialVis.SetShaderParameter("albVisStr", 1.0f);
#endif

#if XBDEBUG
        debug.End();
#endif 
    }

#if XBDEBUG
    public void DebugPrint() {
        Godot.GD.Print("pos: " + XPos + " " + ZPos + ", size: " + XSize + " " + ZSize +
                       ", res: " + Res + ", amnt: " + XAmount + " " + ZAmount          );
    }
#endif

    // initialize vertices and triangle arrays, normals or uvs are not required as collision
    // tiles are not visible
    // triangle array can be filled, as its content is independent of the height of the vertices
    public void InitializeCollisionMesh() {
#if XBDEBUG
        var debug = new XB.DebugTimedBlock(XB.D.CollisionTileInitializeCollisionMesh);
#endif

        Vertices  = new Godot.Vector3[XAmount*ZAmount];
        Triangles = new int[(XAmount-1)*(ZAmount-1)*6];

        int tri  = 0;
        int vert = 0;
        for (int j = 0; j < ZAmount-1; j++) {
            for (int i = 0; i < XAmount-1; i++) {
                Triangles[tri + 0] = vert;
                Triangles[tri + 1] = vert + 1;
                Triangles[tri + 2] = vert + 1 + XAmount;
                Triangles[tri + 3] = vert;
                Triangles[tri + 4] = vert + 1 + XAmount;
                Triangles[tri + 5] = vert +     XAmount;
                tri  += 6;
                vert += 1;
            }
            vert += 1;
        }

        MeshData[(int)Godot.Mesh.ArrayType.Index] = Triangles;

#if XBDEBUG
        debug.End();
#endif 
    }

    // update the vertices array by sampling the heightmap at the corresponding position
    // for each vertex
    public void SampleTerrainNoise(float worldRes, float lowestPoint, float highestPoint, 
                                   Godot.Image imgHeightMap                              ) {
#if XBDEBUG
        var debug = new XB.DebugTimedBlock(XB.D.CollisionTileSampleTerrainNoise);
#endif

        float height = XB.Utils.AbsF(highestPoint-lowestPoint);
        // Godot.GD.Print("xamnt: " + XAmount + ", zamnt: " + ZAmount);
        XB.Utils.ResetV3(ref _v3);
        float step = 1.0f/Res;
        float sampledNoise = 0.0f;
        int   vNumber = 0;
        for (int j = 0; j < ZAmount; j++) {
            for (int i = 0; i < XAmount; i++) {
                _v3.X = (float)i*step - XSize/2.0f;
                _v3.Z = (float)j*step - ZSize/2.0f;
                sampledNoise = XB.Terrain.HeightMapSample(_v3.X + XPos, _v3.Z + ZPos,
                                                          worldRes, imgHeightMap     );
                _v3.Y = sampledNoise*height + lowestPoint;
                // Godot.GD.Print("i: " + i + ",j: " + j + ",vN: " + vNumber + " of " + Vertices.Length);
                Vertices[vNumber] = _v3;
                vNumber += 1;
            }
        }

#if XBDEBUG
        debug.End();
#endif 
    }

    public void ApplyToCollisionMesh() {
#if XBDEBUG
        var debug = new XB.DebugTimedBlock(XB.D.CollisionTileApplyToCollisionMesh);
#endif

        MeshData[(int)Godot.Mesh.ArrayType.Vertex] = Vertices;

        ArrMesh.ClearSurfaces();
        ArrMesh.AddSurfaceFromArrays(Godot.Mesh.PrimitiveType.Triangles, MeshData);

        CollShape.Shape = ArrMesh.CreateTrimeshShape();

#if XBVISUALIZECOLLIDERS
        MeshInstVis.Mesh = ArrMesh;
        MeshInstVis.Mesh.SurfaceSetMaterial(0, MaterialVis);
#endif

#if XBDEBUG
        debug.End();
#endif 
    }
}

// ManagerTerrain manages the terrain mesh tiles and collision tiles along with the terrain quadtree
// updates are handled here and changes to the nodes of the quadtree are queued to be processed
// and assigned over multiple frames to avoid lag spikes
// mesh allocation is the longest step in the whole process, and a queue is used to spread 
// allocation across multiple frames
// the quadtree always has the correct calculated state of which nodes should have their tiles
// visible and which nodes should have their tiles hidden
// because tiles can take a few frames to be processed and have their meshes allocated,
// the state of which tiles should be visible and which are actually visible can be different
// keeping track of these changes is part of this class as well
public class ManagerTerrain {
    private static XB.QNode _qRoot;
    private static int      _nextID      = 0;    // used in creation of quadtree
    private static int      _divisions   = 0;    // divisions of the terrain = depth of quadtree
    private static float    _resolutionM = 0.0f; // highest resolution of visible mesh tiles (/m)
    private static float    _resolutionC = 0.0f; // resolution of collider tiles, uniform throughout
    private static float    _sizeCTile   = 0.0f; // size of full size collision tiles in meter
    private static float    _worldXSize  = 0.0f; // world size in meter
    private static float    _worldZSize  = 0.0f;
    private static int      _maxVerts    = 65536; // 256*256 vertices per tile (technical maximum)
    private static Godot.Vector2                _qNodeCtr = new Godot.Vector2(0.0f, 0.0f);
    private static SysCG.List<XB.MeshContainer> _terrainMeshes; // variable length
    private static XB.CollisionTile[,]   _terrainColTiles; // fixed size array of collision tiles
    private static SysCG.Queue<XB.QNode> _reqQueue;        // requests for MeshContainers go here
    private static XB.QNode              _reqNode;
    private static int                   _queueBudget = 1; // queue processing amount per tick


    // calculates the required depth of the quadtree and populates the whole (sparse) tree
    // then creates and populates the fixed size array of collision tiles
    // creates the request queue
    // creates and show the larges tile (root node of quadtree) so that the ground is not
    // invisible while the "correct" tiles based on distance to the reference are prepared
    public static void InitializeQuadTree(float xSize, float zSize, float resM, float resC,
                                          float sizeCTile, float sizeMTileMin, int divMax,
                                          float lowest, float highest, Godot.Image imgHeightMap,
                                          Godot.Node mainRoot, Godot.ImageTexture miniMap       ) {
#if XBDEBUG
        var debug = new XB.DebugTimedBlock(XB.D.ManagerTerrainInitializeQuadTree);
#endif

        // clear variables if they are already initialized (for ReInitialization)
        if (_qRoot != null) {
            _qRoot.DeleteRecursively();
            _qRoot.ReleaseMeshContainer();
            _qRoot = null;
        }
        if (_terrainMeshes != null) {
            _terrainMeshes.Clear();
            _terrainMeshes = null;
        }
        if (_terrainColTiles != null) {
            _terrainColTiles = null;
        }
        if (_reqQueue != null) {
            _reqQueue.Clear();
            _reqQueue = null;
        }

        _nextID      = 0;
        _resolutionM = resM;
        _resolutionC = resC;
        _sizeCTile   = sizeCTile;
        _worldXSize  = xSize;
        _worldZSize  = zSize;

        float temp = sizeMTileMin;
        _divisions = 0;
        for (int i = 0; i < divMax; i++) {
            temp *= 2;
            if (temp > XB.Utils.MaxF(_worldXSize, _worldZSize)) {
                _divisions = i;
                break;
            }
        }
        _divisions = XB.Utils.MinI(_divisions, divMax);
        
        // the lowest division level (highest detail) should have the specified resolution
        for (int i = 0; i < _divisions; i++) { resM = resM/2.0f; }

        // check for overly high resolution for tile size
        int vertPerTile = (int)(resM * (-_worldXSize) * resM * (-_worldZSize));
        if (vertPerTile > _maxVerts/4) { //NOTE[ALEX]: divided to get better performance
            Godot.GD.Print("WARNING: Resolution for given tile size is too big.");
        }

        // Godot.GD.Print("InitializeQuadTree with Size: " + _worldXSize + " x " + _worldZSize
        //                + ", Mesh Resolution: " + _resolutionM + ", Divisions: " + _divisions
        //                + ", Coll Resolution: " + _resolutionC + ", CTile Size: " + _sizeCTile
        //                + ", Vertices per Tile: " + vertPerTile                               );

        _qRoot = new XB.QNode(ref _nextID, -_worldXSize/2.0f, -_worldZSize/2.0f, 
                              _worldXSize, _worldZSize, resM                    );

        DivideQuadNode(_qRoot, _divisions);

        _terrainMeshes = new SysCG.List<XB.MeshContainer>();

        int colTileAmntX = (int)System.MathF.Ceiling(_worldXSize*_resolutionC / _sizeCTile);
        int colTileAmntZ = (int)System.MathF.Ceiling(_worldZSize*_resolutionC / _sizeCTile);
        _terrainColTiles = new XB.CollisionTile[colTileAmntX, colTileAmntZ];
        float colXPos = 0.0f;
        float colZPos = 0.0f;
        for (int i = 0; i < colTileAmntX; i++) {
            float colXSize = _sizeCTile * _resolutionC;
            if ((i+1)*colXSize > _worldXSize) { colXSize =  _worldXSize - i*colXSize; }
            colXPos = i*_sizeCTile*_resolutionC + colXSize/2.0f;

            for (int j = 0; j < colTileAmntZ; j++) {
                float colZSize = _sizeCTile * _resolutionC;
                if ((j+1)*colZSize > _worldZSize) { colZSize = _worldZSize - j*colZSize; }
                colZPos = j*_sizeCTile*_resolutionC + colZSize/2.0f;

                _terrainColTiles[i, j] = new XB.CollisionTile
                    (mainRoot, -colXPos, -colZPos, colXSize, colZSize, _resolutionC,
                     XB.LayerMasks.EnvironmentLayer, XB.LayerMasks.EnvironmentMask  );
            }
        }

        _reqQueue = new SysCG.Queue<XB.QNode>();

        ShowLargestTile(_qRoot, lowest, highest, imgHeightMap, mainRoot, miniMap);

#if XBDEBUG
        debug.End();
#endif 
    }

    // take a quadtree node and create 4 children with correct parameters,
    // then recursively repeat until specified amount of divisions is reached
    // the result of calling this once with a previously created QNode is 
    // a (sparse) quadtree without any assigned MeshContainers
    private static void DivideQuadNode(XB.QNode parent, int divisions) {
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

        var q1 = new XB.QNode(ref _nextID, xPos+xSize/2.0f, zPos+zSize/2.0f, xSize, zSize, res, parent);
        var q2 = new XB.QNode(ref _nextID, xPos+xSize/2.0f, zPos-zSize/2.0f, xSize, zSize, res, parent);
        var q3 = new XB.QNode(ref _nextID, xPos-xSize/2.0f, zPos+zSize/2.0f, xSize, zSize, res, parent);
        var q4 = new XB.QNode(ref _nextID, xPos-xSize/2.0f, zPos-zSize/2.0f, xSize, zSize, res, parent);

        parent.Children[0] = q1;
        parent.Children[1] = q2;
        parent.Children[2] = q3;
        parent.Children[3] = q4;

        DivideQuadNode(parent.Children[0], divisions);
        DivideQuadNode(parent.Children[1], divisions);
        DivideQuadNode(parent.Children[2], divisions);
        DivideQuadNode(parent.Children[3], divisions);

#if XBDEBUG
        debug.End();
#endif 
    }

    // iterate through all initialized collision tiles and update their geometry
    // by sampling the heightmap
    public static void UpdateCollisionTiles(float lowestPoint, float highestPoint,
                                            Godot.Image imgHeightMap              ) {
#if XBDEBUG
        var debug = new XB.DebugTimedBlock(XB.D.ManagerTerrainUpdateCollisionTiles);
#endif

        int colTileAmntX = (int)System.MathF.Ceiling(_worldXSize*_resolutionC / _sizeCTile);
        int colTileAmntZ = (int)System.MathF.Ceiling(_worldZSize*_resolutionC / _sizeCTile);
        for (int i = 0; i < colTileAmntX; i++) {
            for (int j = 0; j < colTileAmntZ; j++) {
                _terrainColTiles[i, j].InitializeCollisionMesh();
                _terrainColTiles[i, j].SampleTerrainNoise(_resolutionM, lowestPoint,
                                                          highestPoint, imgHeightMap);
                _terrainColTiles[i, j].ApplyToCollisionMesh();
            }
        }

#if XBDEBUG
        debug.End();
#endif 
    }

    // traverse the tree to find mesh tiles which should be active,
    // queue requests for a MeshContainer where necessary
    // process the queue
    // see if any tiles should and can be replaced
    public static void UpdateQTreeMeshes(ref Godot.Vector2 refPos, float lowestPoint,
                                         float highestPoint, Godot.Image imgHeightMap,
                                         Godot.Node mainRoot, Godot.ImageTexture miniMap) {
#if XBDEBUG
        var debug = new XB.DebugTimedBlock(XB.D.ManagerTerrainUpdateQTreeMeshes);
#endif

        // Godot.GD.Print("UpdateQTreeMeshes");
        UpdateQNodeMesh(ref refPos, _qRoot, mainRoot);
        QueueRequestProcess(_queueBudget, lowestPoint, highestPoint, imgHeightMap, miniMap);
        QNodeShowReadyMeshes(_qRoot);

#if XBDEBUG
        debug.End();
#endif 
    }

    // traverse the tree starting from a given QNode and see if the mesh tile that that 
    // node is representing should be active or if its childrens mesh tiles should be active instead
    // recursively travel one layer deeper until either a leaf is hit or the appropriate
    // resolution is reached
    // to decide if a tile should be visible,
    // a reference position is compared to the tile's dimension
    // if a node is set active and was previously not active, a MeshContainer is requested (queued)
    private static void UpdateQNodeMesh(ref Godot.Vector2 refPos, XB.QNode qNode,
                                        Godot.Node mainRoot                      ) {
#if XBDEBUG
        var debug = new XB.DebugTimedBlock(XB.D.ManagerTerrainUpdateQNodeMesh);
#endif
        if (qNode.Children[0] == null) { // no further divisions possible
            if (!qNode.Active) {
                // Godot.GD.Print("UpdateQNodeMesh no children act: " + qNode.ID);
                qNode.Activate();
                RequestMeshContainer(qNode, mainRoot);
                QueueRequestMeshUpdate(qNode);
            }
            // Godot.GD.Print("UpdateQNodeMesh tile already active, ends at: " + qNode.ID);
#if XBDEBUG
        debug.End();
#endif 
            return;
        }

        _qNodeCtr.X = qNode.XPos;
        _qNodeCtr.Y = qNode.ZPos;
        float dist = (refPos-_qNodeCtr).Length();
        float comp = (qNode.XSize + qNode.ZSize) / 2.0f;

        if (dist < comp) { // close enough to replace with higher resolution tile
            if (qNode.Active) {
                // Godot.GD.Print("UpdateQNodeMesh close enough, deact: " + qNode.ID + " " + refPos);
                qNode.DeActivate();
                // recycling happens in QNodeShowReadyMeshes after updating the request queue
            }
            // Godot.GD.Print("UpdateQNodeMesh go one level deeper " + qNode.ID);
            UpdateQNodeMesh(ref refPos, qNode.Children[0], mainRoot);
            UpdateQNodeMesh(ref refPos, qNode.Children[1], mainRoot);
            UpdateQNodeMesh(ref refPos, qNode.Children[2], mainRoot);
            UpdateQNodeMesh(ref refPos, qNode.Children[3], mainRoot);
        } else { // reached correct resolution
            if (!qNode.Active) {
                // Godot.GD.Print("UpdateQNodeMesh correct resolution " + qNode.ID);
                qNode.Activate();
                RequestMeshContainer(qNode, mainRoot);
                QueueRequestMeshUpdate(qNode);
            }
        }

#if XBDEBUG
        debug.End();
#endif 
    }

    // traverse the tree and check whether mesh tiles of nodes that are active but not visible
    // are ready to be shown
    // if a parent node should have its tile replaced by the tiles of its child nodes,
    // then that tile only gets replaced if all child tiles are ready to avoid gaps in the terrain
    private static void QNodeShowReadyMeshes(XB.QNode qNode) {
        if (qNode == null) { return; }
#if XBDEBUG
        var debug = new XB.DebugTimedBlock(XB.D.ManagerTerrainQNodeShowReadyMeshes);
#endif

        if (qNode.Active) { // should be visible
            if (!qNode.MeshVisible && qNode.MeshReady) {
                // Godot.GD.Print("QNodeShowReadyMeshes show node " + qNode.ID);
                qNode.ShowMeshContainer();
            }
        } else { // should not be visible
            if (qNode.MeshVisible) {
                if (qNode.ChildrenActiveAndReady()) {
                    // Godot.GD.Print("QNodeShowReadyMeshes off, now showing children " + qNode.ID);
                    RecycleMeshContainer(qNode);
                    qNode.Children[0].ShowMeshContainer();
                    qNode.Children[1].ShowMeshContainer();
                    qNode.Children[2].ShowMeshContainer();
                    qNode.Children[3].ShowMeshContainer();
                } else if (qNode.ChildrenActiveRecursive()) {
                    if (qNode.ChildrenReadyRecursive()) {
                    // Godot.GD.Print("QNodeShowReadyMeshes off rec, now showing children " + qNode.ID);
                    RecycleMeshContainer(qNode);
                    QNodeShowReadyMeshes(qNode.Children[0]);
                    QNodeShowReadyMeshes(qNode.Children[1]);
                    QNodeShowReadyMeshes(qNode.Children[2]);
                    QNodeShowReadyMeshes(qNode.Children[3]);
                    }
                } else {
                    // Godot.GD.Print("QNodeShowReadyMeshes waiting for children " + qNode.ID);
                }
            } else { // go one division level deeper
                // Godot.GD.Print("QNodeShowReadyMeshes go one level deeper " + qNode.ID);
                RecycleMeshContainer(qNode);
                QNodeShowReadyMeshes(qNode.Children[0]);
                QNodeShowReadyMeshes(qNode.Children[1]);
                QNodeShowReadyMeshes(qNode.Children[2]);
                QNodeShowReadyMeshes(qNode.Children[3]);
            }
        }

#if XBDEBUG
        debug.End();
#endif 
    }

    // when a parent node's mesh tile gets loaded in and replaces its children's node's mesh tiles,
    // those node's MeshContainers and their children's, etc. have to be freed up to be reused again
    private static void RecycleChildMesh(XB.QNode qNode) {
        if (qNode == null) { return; }
        if (qNode.Active)  { return; }
#if XBDEBUG
        var debug = new XB.DebugTimedBlock(XB.D.ManagerTerrainRecycleChildMesh);
#endif

        RecycleMeshContainer(qNode);
        RecycleChildMesh(qNode.Children[0]);
        RecycleChildMesh(qNode.Children[1]);
        RecycleChildMesh(qNode.Children[2]);
        RecycleChildMesh(qNode.Children[3]);

#if XBDEBUG
        debug.End();
#endif 
    }

    // when enqueueing a request, first check whether that request has already been made
    // and is simply waiting to be processed
    private static void QueueRequestMeshUpdate(XB.QNode qNode) {
#if XBDEBUG
        var debug = new XB.DebugTimedBlock(XB.D.ManagerTerrainQueueRequestMeshUpdate);
#endif

        if (!_reqQueue.Contains(qNode)) {
            _reqQueue.Enqueue(qNode);
            //Godot.GD.Print("Add to Request Queue: " + qNode.ID);
        }

#if XBDEBUG
        debug.End();
#endif 
    }

    // each update tick, a fixed amount of request in the queue can be processed
    // only the assignment of the newly sampled mesh data is limited to processAmount tiles per tick
    // using the queue, other parts are much faster to do and can be done immediately
    private static void QueueRequestProcess(int processAmount, float lowest, float highest,
                                            Godot.Image imgHeightMap, Godot.ImageTexture miniMap) {
#if XBDEBUG
        var debug = new XB.DebugTimedBlock(XB.D.ManagerTerrainQueueRequestProcess);
#endif

        // Godot.GD.Print("QueueRequestProcess queue amount: " + _reqQueue.Count);
        for (int i = 0; i < XB.Utils.MinF(processAmount, _reqQueue.Count); i++) {
            _reqNode = _reqQueue.Dequeue();
            if (!_reqNode.Active) { // if a node gets deactivated while in the queue
                //Godot.GD.Print("inactive node skipped " + _reqNode.ID);
                i--;
                continue; 
            }
            _reqNode.MeshContainer.UseMesh(_reqNode.XPos, _reqNode.ZPos,
                                           _reqNode.XSize, _reqNode.ZSize, 
                                           _worldXSize, _worldZSize, _reqNode.Res);
            _reqNode.MeshContainer.SetTerrainShaderAttributes(miniMap);
            _reqNode.UpdateAssignedMesh(_resolutionM, _worldXSize, _worldZSize,
                                        lowest, highest, imgHeightMap          );
            // Godot.GD.Print("Queue Assigned " + _reqNode.ID + " recycling children");
            RecycleChildMesh(_reqNode.Children[0]);
            RecycleChildMesh(_reqNode.Children[1]);
            RecycleChildMesh(_reqNode.Children[2]);
            RecycleChildMesh(_reqNode.Children[3]);
        }

#if XBDEBUG
        debug.End();
#endif 
    }

    // releases a node's MeshContainer if it is not already null
    private static void RecycleMeshContainer(XB.QNode qNode) {
#if XBDEBUG
        var debug = new XB.DebugTimedBlock(XB.D.ManagerTerrainRecycleMeshContainer);
#endif

        // Godot.GD.Print("RecycleMeshContainer, ID: " + qNode.ID);
        if (qNode.MeshContainer == null) { return; }
        qNode.ReleaseMeshContainer();

#if XBDEBUG
        debug.End();
#endif 
    }

    // see if a MeshContainer is available, if not, create a new MeshContainer
    // then assign it to the requesting node
    //NOTE[ALEX]: simply creating the MeshContainer is not performance heavy, but rather
    //            the creation and especially assignment of the appropriate mesh are
    private static void RequestMeshContainer(XB.QNode qNode, Godot.Node mainRoot) {
#if XBDEBUG
        var debug = new XB.DebugTimedBlock(XB.D.ManagerTerrainRequestMeshContainer);
#endif
        
        // Godot.GD.Print("RequestTerrainMeshContainer qNodeID: " + qNode.ID);

        for (int i = 0; i < _terrainMeshes.Count; i++) {
            if (!_terrainMeshes[i].InUse) {
                // Godot.GD.Print("RequestTerrainMeshContainer reuse MeshContainer array ID: " + i);
                qNode.AssignMeshContainer(_terrainMeshes[i]);
#if XBDEBUG
                debug.End();
#endif 
                return;
            }
        }

        int newID = _terrainMeshes.Count;
        var mC    = new XB.MeshContainer(mainRoot, newID, qNode.XPos/_worldXSize,
                                         qNode.ZPos/_worldZSize                  );
        _terrainMeshes.Add(mC);
        // Godot.GD.Print("RequestTerrainMeshContainer new MeshContainer array ID: " + newID);
        qNode.AssignMeshContainer(_terrainMeshes[newID]);

#if XBDEBUG
        debug.End();
#endif 
    }

    // request a MeshContainer for the root node, which has the largest, lowest resolution
    // mesh tile of all nodes
    // then prepare the mesh and show it
    private static void ShowLargestTile(XB.QNode qNode, float lowest, float highest,
                                        Godot.Image imgHeightMap, Godot.Node mainRoot,
                                        Godot.ImageTexture miniMap                    ) {
#if XBDEBUG
        var debug = new XB.DebugTimedBlock(XB.D.ManagerTerrainShowLargestTile);
#endif

        // Godot.GD.Print("ShowLargestTile " + qNode.ID);
        qNode.Activate();
        RequestMeshContainer(qNode, mainRoot);
        qNode.MeshContainer.UseMesh(qNode.XPos, qNode.ZPos,
                                    qNode.XSize, qNode.ZSize, 
                                    _worldXSize, _worldZSize, qNode.Res);
        qNode.MeshContainer.SetTerrainShaderAttributes(miniMap);
        qNode.UpdateAssignedMesh(_resolutionM, _worldXSize, _worldZSize,
                                 lowest, highest, imgHeightMap          );
        qNode.ShowMeshContainer();

#if XBDEBUG
        debug.End();
#endif 
    }

    // deactivate all nodes of the tree recursively and recalculate and show the largest tile
    // used when the heightmap changes, essentially "starting over"
    public static void ResetQuadTree(float lowest, float highest, Godot.Image imgHeightMap,
                                     Godot.Node mainRoot, Godot.ImageTexture miniMap       ) {
#if XBDEBUG
        var debug = new XB.DebugTimedBlock(XB.D.ManagerTerrainResetQuadTree);
#endif

        ResetQNode(_qRoot);
        ShowLargestTile(_qRoot, lowest, highest, imgHeightMap, mainRoot, miniMap);

#if XBDEBUG
        debug.End();
#endif 
    }

    // reset given node, then recursively call on all child nodes until a leaf node is reached
    private static void ResetQNode(XB.QNode qNode) {
#if XBDEBUG
        var debug = new XB.DebugTimedBlock(XB.D.ManagerTerrainResetQNode);
#endif

        if (qNode == null) {
#if XBDEBUG
        debug.End();
#endif 
            return;
        }

        qNode.DeActivate();
        qNode.ReleaseMeshContainer();
        ResetQNode(qNode.Children[0]);
        ResetQNode(qNode.Children[1]);
        ResetQNode(qNode.Children[2]);
        ResetQNode(qNode.Children[3]);

#if XBDEBUG
        debug.End();
#endif 
    }

    // update the shader of each MeshContainer that is in use
    public static void UpdateBlockStrength(float multiplier) {
#if XBDEBUG
        var debug = new XB.DebugTimedBlock(XB.D.ManagerTerrainUpdateBlockStrength);
#endif

        for (int i = 0; i < _terrainMeshes.Count; i++) {
            if (_terrainMeshes[i].InUse) {
                _terrainMeshes[i].SetShaderAttribute("blockStr", multiplier*XB.WData.BlockStrength);
            }
        }

#if XBDEBUG
        debug.End();
#endif 
    }

    // update the shader of each MeshContainer that is in use
    public static void UpdateQTreeStrength(float multiplier) {
#if XBDEBUG
        var debug = new XB.DebugTimedBlock(XB.D.ManagerTerrainUpdateQTreeStrength);
#endif

        for (int i = 0; i < _terrainMeshes.Count; i++) {
            if (_terrainMeshes[i].InUse) {
                _terrainMeshes[i].SetShaderAttribute("albVisStr", multiplier*XB.WData.QTreeStrength);
            }
        }

#if XBDEBUG
        debug.End();
#endif 
    }

    // update overlay texture to minimap with quadtree visualization
    public static void UpdateQTreeTexture(Godot.Image tex, float scaleFactor,
                                          Godot.Rect2I[] rects               ) {
#if XBDEBUG
        var debug = new XB.DebugTimedBlock(XB.D.ManagerTerrainUpdateQTreeTexture);
#endif

        tex.Fill(XB.Col.Transp); // clear texture before drawing quadtree tiles

        DrawQNode(_qRoot, tex, scaleFactor, _divisions, rects);

#if XBDEBUG
        debug.End();
#endif 
    }

    // traverse tree recursively to draw outline of all active nodes
    // scaleFactor adjust meter to pixel ratio
    private static void DrawQNode(XB.QNode qNode, Godot.Image tex,
                                  float scaleFactor, int iteration, Godot.Rect2I[] rects) {
#if XBDEBUG
        var debug = new XB.DebugTimedBlock(XB.D.ManagerTerrainDrawQNode);
#endif

        if (qNode == null) {
#if XBDEBUG
            debug.End();
#endif 
            return; 
        }

        if (qNode.Active) { // shows active tiles which are not necessarily processed yet
            // texture has 0|0 in top left, in world coordinates, "top left" has 0|0 with negative axes
            int xCtr = (int)(-qNode.XPos*scaleFactor);
            int yCtr = (int)(-qNode.ZPos*scaleFactor);
            int dx   = (int)(qNode.XSize*scaleFactor);
            int dy   = (int)(qNode.ZSize*scaleFactor);
            int t    = 1;
            XB.Utils.UpdateRect2I(xCtr-dx/2,      yCtr-dy/2,      dx, t,  ref rects[0]);
            XB.Utils.UpdateRect2I(xCtr-dx/2,      yCtr-dy/2+dy-t, dx, t,  ref rects[1]);
            XB.Utils.UpdateRect2I(xCtr-dx/2,      yCtr-dy/2,      t,  dy, ref rects[2]);
            XB.Utils.UpdateRect2I(xCtr-dx/2+dx-t, yCtr-dy/2,      t,  dy, ref rects[3]);

            var col = XB.Col.Red.Lerp(XB.Col.Green, (float)iteration/(float)_divisions);
            for (int i = 0; i < 4; i++) { tex.FillRect(rects[i], col); }
        } else {
            DrawQNode(qNode.Children[0], tex, scaleFactor, iteration-1, rects);
            DrawQNode(qNode.Children[1], tex, scaleFactor, iteration-1, rects);
            DrawQNode(qNode.Children[2], tex, scaleFactor, iteration-1, rects);
            DrawQNode(qNode.Children[3], tex, scaleFactor, iteration-1, rects);
        }

#if XBDEBUG
        debug.End();
#endif 
    }

#if XBDEBUG
    //prints tree depth first
    private static void PrintQTree(XB.QNode qNode) {
        if (qNode == null) { return; }
        qNode.DebugPrint("PrintQTree");

        PrintQTree(qNode.Children[0]);
        PrintQTree(qNode.Children[1]);
        PrintQTree(qNode.Children[2]);
        PrintQTree(qNode.Children[3]);
    }

    public static void PrintQTreeExternal() {
        Godot.GD.Print("PrintQTreeExternal");
        string temp = "";
        PrintQNodeActive(_qRoot, ref temp);
        Godot.GD.Print("Active QNodes:             " + temp);
        temp = "";
        PrintQNodeMeshContainer(_qRoot, ref temp);
        Godot.GD.Print("QNodes with MeshContainer: " + temp);
        Godot.GD.Print("MeshContainers available total: " + _terrainMeshes.Count);
        temp = "";
        for (int i = 0; i < _terrainMeshes.Count; i++) {
            temp += "i: " + i.ToString();
            temp += ", ID: " + _terrainMeshes[i].ID.ToString();
            temp += ", InUse: " + _terrainMeshes[i].InUse.ToString();
            temp += ", MInst: " + _terrainMeshes[i].MeshInst.Visible.ToString();
            temp += "; ";
        }
        Godot.GD.Print("MeshContainers: " + temp);
        Godot.GD.Print('\n');
    }

    private static void PrintQNodeActive(XB.QNode qNode, ref string s) {
        if (qNode.Active) {
            s += qNode.ID.ToString() + " ";
        }
        if (qNode.Children[0] != null) {
            PrintQNodeActive(qNode.Children[0], ref s);
            PrintQNodeActive(qNode.Children[1], ref s);
            PrintQNodeActive(qNode.Children[2], ref s);
            PrintQNodeActive(qNode.Children[3], ref s);
        }
    }

    private static void PrintQNodeMeshContainer(XB.QNode qNode, ref string s) {
        if (qNode.MeshContainer != null) {
            s += qNode.ID.ToString() + " ";
        }
        if (qNode.Children[0] != null) {
            PrintQNodeMeshContainer(qNode.Children[0], ref s);
            PrintQNodeMeshContainer(qNode.Children[1], ref s);
            PrintQNodeMeshContainer(qNode.Children[2], ref s);
            PrintQNodeMeshContainer(qNode.Children[3], ref s);
        }
    }
#endif
}
} // namespace close
