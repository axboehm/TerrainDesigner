#define XBDEBUG
// #define XBVISUALIZECOLLIDERS
using SysCG = System.Collections.Generic;
namespace XB { // namespace open
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

        //DebugPrint("Called at Creation");
    }

#if XBDEBUG
    public void DebugPrint(string note) {
        string print = "Print Quadtree Node: " + ID.ToString() + ", Is Visible: " + Visible
                       + " " + note + '\n';
        print += "Ctr Pos: " + XPos.ToString() + "m " + ZPos.ToString() + "m, ";
        print += "Size: " + XSize.ToString() + "m " + ZSize.ToString() + "m, ";
        print += "Resolution: " + Res.ToString() + "/m\n";
        print += "Has Parent: ";
        if (Parent == null) { print += "No, "; }
        else                      { print += "Yes, ID: " + Parent.ID.ToString() + ", "; }
        print += '\n' + "Has Children: ";
        if (Children[0] == null) { print += "No"; }
        else                           { 
            print += "Yes, IDs: "; 
            for (int i = 0; i < 4; i++) {
                print += Children[i].ID.ToString() + ", ";
            }
        }
        print += '\n';
        Godot.GD.Print(print);
    }
#endif

    public void AssignMeshContainer(XB.MeshContainer mC) {
#if XBDEBUG
        var debug = new XB.DebugTimedBlock(XB.D.QNodeAssignMeshContainer);
#endif

        MeshContainer = mC;
        Visible       = true;

#if XBDEBUG
        debug.End();
#endif 
    }

    public void ReleaseMeshContainer() {
#if XBDEBUG
        var debug = new XB.DebugTimedBlock(XB.D.QNodeReleaseMeshContainer);
#endif

        if (!Visible) { return; }
        MeshContainer.ReleaseMesh();
        MeshContainer = null;
        Visible       = false;

#if XBDEBUG
        debug.End();
#endif 
    }

    public void UpdateAssignedMesh(float worldXSize, float worldZSize,
                                   float lowestPoint, float highestPoint,
                                   ref Godot.Image imgHeightMap          ) {
#if XBDEBUG
        var debug = new XB.DebugTimedBlock(XB.D.QNodeUpdateAssignedMesh);
#endif

        MeshContainer.SampleTerrainNoise(XPos, ZPos, XSize, ZSize, worldXSize, worldZSize,
                                         Res, lowestPoint, highestPoint, ref imgHeightMap );
        var worldEdge = new bool[4];

        if (worldZSize-(-ZPos+ZSize/2.0f) < XB.Constants.Epsilon) { worldEdge[(int)XB.Sk.ZM] = true;  }
        else                                                      { worldEdge[(int)XB.Sk.ZM] = false; }
        if (            -ZPos-ZSize/2.0f  < XB.Constants.Epsilon) { worldEdge[(int)XB.Sk.ZP] = true;  }
        else                                                      { worldEdge[(int)XB.Sk.ZP] = false; }
        if (worldXSize-(-XPos+XSize/2.0f) < XB.Constants.Epsilon) { worldEdge[(int)XB.Sk.XM] = true;  }
        else                                                      { worldEdge[(int)XB.Sk.XM] = false; }
        if (            -XPos-XSize/2.0f  < XB.Constants.Epsilon) { worldEdge[(int)XB.Sk.XP] = true;  }
        else                                                      { worldEdge[(int)XB.Sk.XP] = false; }

        MeshContainer.ApplyToMesh(worldEdge);

#if XBDEBUG
        debug.End();
#endif 
    }
}

public enum Sk {
    ZM, // bottom edge
    ZP, // top edge
    XM, // right edge
    XP, // left edge
}

// each MeshContainer represents the visible mesh data of one terrain tile
// since the tiles have different resolutions, gaps can appear between the tiles
// to prevent this, each tile gets a mesh skirt, an extension of the edges downwards to hide the gap
public class MeshContainer {
    public int  XAmount;
    public int  ZAmount;
    public int  ID;
    public bool InUse;
    public Godot.MeshInstance3D      MeshInst; // holds tile and skirt meshes
    public Godot.Collections.Array   MeshDataTile;
    public Godot.Collections.Array[] MeshDataSkirt;
    public Godot.ArrayMesh           ArrMesh;
    public Godot.ShaderMaterial      MaterialTile;
    public Godot.ShaderMaterial      MaterialSkirt;
    public Godot.Vector3[]   VerticesTile;
    public Godot.Vector3[][] VerticesSkirt;
    public Godot.Vector2[]   UVsTile;
    public Godot.Vector2[][] UVsSkirt;
    public Godot.Vector3[]   NormalsTile;
    public Godot.Vector3[][] NormalsSkirt;
    public int[]             TrianglesTile;
    public int[][]           TrianglesSkirt;
    private const float      _skirtLength = 8.0f;

    public MeshContainer(Godot.Node root, int id, float lerpRAmount, float lerpGAmount) {
        MeshInst = new Godot.MeshInstance3D();
        root.AddChild(MeshInst);

        MaterialTile = new Godot.ShaderMaterial();
        MaterialTile.Shader = Godot.ResourceLoader.Load<Godot.Shader>(XB.ResourcePaths.TerrainShader);
        MaterialTile.SetShaderParameter("albedoMult", XB.WorldData.AlbedoMult);
        MaterialTile.SetShaderParameter("tBlock",     XB.Resources.BlockTex);
        MaterialTile.SetShaderParameter("blockStr",   XB.WorldData.BlockStrength);
        MaterialTile.SetShaderParameter("tNoiseP",    XB.Resources.NoiseBombing);
        MaterialTile.SetShaderParameter("tAlbedoM1",  XB.Resources.Terrain1CATex);
        MaterialTile.SetShaderParameter("tRMM1",      XB.Resources.Terrain1RMTex);
        MaterialTile.SetShaderParameter("tNormalM1",  XB.Resources.Terrain1NTex );
        MaterialTile.SetShaderParameter("tHeightM1",  XB.Resources.Terrain1HTex );
        MaterialTile.SetShaderParameter("tAlbedoM2",  XB.Resources.Terrain2CATex);
        MaterialTile.SetShaderParameter("tRMM2",      XB.Resources.Terrain2RMTex);
        MaterialTile.SetShaderParameter("tNormalM2",  XB.Resources.Terrain2NTex );
        MaterialTile.SetShaderParameter("tHeightM2",  XB.Resources.Terrain2HTex );
        // visualization colors initially represent somewhat of a gradient 
        // but will quickly get shuffled around as MeshContainers get reused
        float r = 1.0f - XB.Utils.LerpF(0.0f, 1.0f, -lerpRAmount);
        float g =        XB.Utils.LerpF(0.0f, 1.0f, -lerpGAmount);
        float b = XB.Random.RandomInRangeF(0.0f, 1.0f);
        var col = new Godot.Color(r, g, b, 1.0f);
        MaterialTile.SetShaderParameter("albVis",    col);
        MaterialTile.SetShaderParameter("albVisStr", 0.5f);

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
        ArrMesh       = new Godot.ArrayMesh();

        XAmount = 0;
        ZAmount = 0;
        ID      = id;
        InUse   = true;
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

        Godot.Vector2 v2 = new Godot.Vector2(0.0f, 0.0f);
        float uvStartX = -(xPos-xSize/2.0f)/xWorldSize;
        float uvEndX   = -(xPos+xSize/2.0f)/xWorldSize;
        float uvStartY = -(zPos-zSize/2.0f)/zWorldSize;
        float uvEndY   = -(zPos+zSize/2.0f)/zWorldSize;
        for (int i = 0; i < UVsTile.Length; i++) {
            int x = i%XAmount;
            int y = i/XAmount;
            v2.X = XB.Utils.LerpF(uvStartX, uvEndX, (float)x/(float)(XAmount-1));
            v2.Y = XB.Utils.LerpF(uvStartY, uvEndY, (float)y/(float)(ZAmount-1));
            UVsTile[i] = v2;
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
        NormalsSkirt                 = new Godot.Vector3[4][];
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

        SkirtTriangleIndices(ref TrianglesSkirt[(int)XB.Sk.ZM], XAmount-1);
        SkirtTriangleIndices(ref TrianglesSkirt[(int)XB.Sk.ZP], XAmount-1);
        SkirtTriangleIndices(ref TrianglesSkirt[(int)XB.Sk.XM], ZAmount-1);
        SkirtTriangleIndices(ref TrianglesSkirt[(int)XB.Sk.XP], ZAmount-1);

        MeshDataSkirt[(int)XB.Sk.ZM][(int)Godot.Mesh.ArrayType.TexUV] = UVsSkirt[(int)XB.Sk.ZM];
        MeshDataSkirt[(int)XB.Sk.ZP][(int)Godot.Mesh.ArrayType.TexUV] = UVsSkirt[(int)XB.Sk.ZP];
        MeshDataSkirt[(int)XB.Sk.XM][(int)Godot.Mesh.ArrayType.TexUV] = UVsSkirt[(int)XB.Sk.XM];
        MeshDataSkirt[(int)XB.Sk.XP][(int)Godot.Mesh.ArrayType.TexUV] = UVsSkirt[(int)XB.Sk.XP];

        MeshDataSkirt[(int)XB.Sk.ZM][(int)Godot.Mesh.ArrayType.Index] = TrianglesSkirt[(int)XB.Sk.ZM];
        MeshDataSkirt[(int)XB.Sk.ZP][(int)Godot.Mesh.ArrayType.Index] = TrianglesSkirt[(int)XB.Sk.ZP];
        MeshDataSkirt[(int)XB.Sk.XM][(int)Godot.Mesh.ArrayType.Index] = TrianglesSkirt[(int)XB.Sk.XM];
        MeshDataSkirt[(int)XB.Sk.XP][(int)Godot.Mesh.ArrayType.Index] = TrianglesSkirt[(int)XB.Sk.XP];

        MeshInst.Show();
        InUse = true;

#if XBDEBUG
        debug.End();
#endif 
    }

    private void SkirtTriangleIndices(ref int[] triangles, int amount) {
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
    }

    public void SampleTerrainNoise(float xPos, float zPos, float xSize,  float zSize,
                                   float worldXSize, float worldZSize, float res,
                                   float lowestPoint, float highestPoint, 
                                   ref Godot.Image imgHeightMap                      ) {
#if XBDEBUG
        var debug = new XB.DebugTimedBlock(XB.D.MeshContainerSampleTerrainNoise);
#endif

        float height = XB.Utils.AbsF(highestPoint-lowestPoint);
        var pos = new Godot.Vector3(xPos, 0.0f, zPos);
        MeshInst.GlobalPosition = pos; // move MeshInstance to center of tile, mesh is child
        // Godot.GD.Print("MeshContainerSampleTerrainNoise: lp: " + lowestPoint + ", h: " + height);

        // tile (without skirt)
        var   v3   = new Godot.Vector3(0.0f, 0.0f, 0.0f);
        float step = 1.0f/res;
        float sampledNoise = 0.0f;
        int   vNumber = 0;
        for (int j = 0; j < ZAmount; j++) {
            for (int i = 0; i < XAmount; i++) {
                v3.X = (float)i*step - xSize/2.0f;
                v3.Z = (float)j*step - zSize/2.0f;
                sampledNoise = XB.Terrain.HeightMapSample(v3.X + pos.X, v3.Z + pos.Z,
                                                          worldXSize, worldZSize, ref imgHeightMap);
                v3.Y = sampledNoise*height + lowestPoint;
                VerticesTile[vNumber] = v3;
                vNumber += 1;
            }
        }

        XB.Terrain.CalculateNormals(ref NormalsTile, ref VerticesTile, ref TrianglesTile);

        // skirt vertices and normals (copied from edges of tile
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
    public void SetTerrainShaderAttributes() {
#if XBDEBUG
        var debug = new XB.DebugTimedBlock(XB.D.MeshContainerSetTerrainShaderAttributes);
#endif

        MaterialTile.SetShaderParameter("scaleX",      WorldData.WorldDim.X);
        MaterialTile.SetShaderParameter("scaleY",      WorldData.WorldDim.Y);
        MaterialTile.SetShaderParameter("blockScale",  WorldData.BlockUVScale);
        MaterialTile.SetShaderParameter("uv1Scale",    WorldData.Mat1UVScale);
        MaterialTile.SetShaderParameter("uv2Scale",    WorldData.Mat2UVScale);
        MaterialTile.SetShaderParameter("noisePScale", WorldData.NoisePScale);
        MaterialTile.SetShaderParameter("blendDepth",  WorldData.BlendDepth);
        MaterialTile.SetShaderParameter("tHeight",     XB.PController.Hud.TexMiniMap);

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

    public void SetShaderAttribute(string attName, Godot.ImageTexture img) {
#if XBDEBUG
        var debug = new XB.DebugTimedBlock(XB.D.MeshContainerSetShaderAttribute);
#endif

        MaterialTile.SetShaderParameter(attName, img);

#if XBDEBUG
        debug.End();
#endif 
    }

    private void AdjustWorldEdgeSkirt(XB.Sk direction) {
#if XBDEBUG
        var debug = new XB.DebugTimedBlock(XB.D.MeshContainerAdjustWorldEdgeSkirt);
#endif

        int d = (int)direction;
        var n = new Godot.Vector3(0.0f, 0.0f, 0.0f);

        switch (direction) {
            case XB.Sk.ZM: { n.Z = -1.0f; break; }
            case XB.Sk.ZP: { n.Z = +1.0f; break; }
            case XB.Sk.XM: { n.X = -1.0f; break; }
            case XB.Sk.XP: { n.X = +1.0f; break; }
        }

        for (int i = 0; i < VerticesSkirt[d].Length/2; i++) {
            VerticesSkirt[d][2*i+1].Y = XB.WorldData.KillPlane;
            NormalsSkirt [d][2*i+0]   = n;
            NormalsSkirt [d][2*i+1]   = n;
        }

#if XBDEBUG
        debug.End();
#endif 
    }

    public void ApplyToMesh(bool[] worldEdge) {
#if XBDEBUG
        var debug = new XB.DebugTimedBlock(XB.D.MeshContainerApplyToMesh);
#endif

        MeshDataTile [(int)Godot.Mesh.ArrayType.Vertex] = VerticesTile;
        MeshDataTile [(int)Godot.Mesh.ArrayType.Normal] = NormalsTile;

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

public class CollisionTile {
    public int   XAmount;
    public int   ZAmount;
    public float XPos;  // center x coordinate in meter
    public float ZPos;
    public float XSize; // dimensions in meter
    public float ZSize;
    public float Res;   // subdivisions per meter
    public Godot.Collections.Array MeshData;
    public Godot.ArrayMesh         ArrMesh;
    public Godot.StaticBody3D      StatBody;
    public Godot.CollisionShape3D  CollShape;
    public Godot.Vector3[] Vertices;
    public int[]           Triangles;
#if XBVISUALIZECOLLIDERS
    public Godot.MeshInstance3D MeshInstVis;
    public Godot.ShaderMaterial MaterialVis;
#endif 
    
    public CollisionTile(Godot.Node root, float xPos, float zPos, float xSize, float zSize,
                         float res, uint collisionLayer, uint collisionMask                ) {
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

        //DebugPrint();

#if XBVISUALIZECOLLIDERS
        MeshInstVis  = new Godot.MeshInstance3D();
        root.AddChild(MeshInstVis);
        MeshInstVis.GlobalPosition = new Godot.Vector3(xPos, 0.0f, zPos);
        MaterialVis = new Godot.ShaderMaterial();
        MaterialVis.Shader = Godot.ResourceLoader.Load<Godot.Shader>(XB.ResourcePaths.TerrainShader);
        float r = XB.Random.RandomInRangeF(0.0f, 1.0f);
        float g = XB.Random.RandomInRangeF(0.0f, 1.0f);
        float b = XB.Random.RandomInRangeF(0.0f, 1.0f);
        var col = new Godot.Color(r, g, b, 1.0f);
        MaterialVis.SetShaderParameter("albVis",    col);
        MaterialVis.SetShaderParameter("albVisStr", 1.0f);
#endif
    }

#if XBDEBUG
    public void DebugPrint() {
        Godot.GD.Print("pos: " + XPos + " " + ZPos + ", size: " + XSize + " " + ZSize +
                       ", res: " + Res + ", amnt: " + XAmount + " " + ZAmount          );
    }
#endif

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

    public void SampleTerrainNoise(float worldXSize, float worldZSize,
                                   float lowestPoint, float highestPoint,
                                   ref Godot.Image imgHeightMap          ) {
#if XBDEBUG
        var debug = new XB.DebugTimedBlock(XB.D.CollisionTileSampleTerrainNoise);
#endif

        float height = XB.Utils.AbsF(highestPoint-lowestPoint);
        // Godot.GD.Print("xamnt: " + XAmount + ", zamnt: " + ZAmount);
        var   v3   = new Godot.Vector3(0.0f, 0.0f, 0.0f);
        float step = 1.0f/Res;
        float sampledNoise = 0.0f;
        int   vNumber = 0;
        for (int j = 0; j < ZAmount; j++) {
            for (int i = 0; i < XAmount; i++) {
                v3.X = (float)i*step - XSize/2.0f;
                v3.Z = (float)j*step - ZSize/2.0f;
                sampledNoise = XB.Terrain.HeightMapSample(v3.X + XPos, v3.Z + ZPos,
                                                          worldXSize, worldZSize, ref imgHeightMap);
                v3.Y = sampledNoise*height + lowestPoint;
                // Godot.GD.Print("i: " + i + ",j: " + j + ",vN: " + vNumber + " of " + Vertices.Length);
                Vertices[vNumber] = v3;
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

public class ManagerTerrain {
    private static XB.QNode _qRoot;
    private static int      _nextID      = 0;
    private static int      _divisions   = 0;
    private static float    _resolutionM = 0.0f; // highest resolution of visible mesh tiles
    private static float    _resolutionC = 0.0f; // resolution of collider tiles, uniform throughout
    private static float    _sizeCTile   = 0.0f; // size of full size collision tiles
    private static float    _worldXSize  = 0.0f;
    private static float    _worldZSize  = 0.0f;
    private static SysCG.List<XB.MeshContainer> _terrainMeshes;
    private static XB.CollisionTile[,]          _terrainColTiles;

    //NOTE[ALEX]: there is no safequard against overly large worlds with high resolution
    public static void InitializeQuadTree(float xSize, float zSize, float resM, float resC,
                                          float sizeCTile, float sizeMTileMin, int divMax  ) {
#if XBDEBUG
        var debug = new XB.DebugTimedBlock(XB.D.ManagerTerrainInitializeQuadTree);
#endif

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
            if (temp > _worldXSize) {
                _divisions = i;
                break;
            }
        }
        _divisions = XB.Utils.MinI(_divisions, divMax);

        Godot.GD.Print("InitializeQuadTree with Size: " + _worldXSize + " x " + _worldZSize
                       + ", Mesh Resolution: " + _resolutionM + ", Divisions: " + _divisions
                       + ", Coll Resolution: " + _resolutionC + ", CTile Size: " + _sizeCTile);
        
        // the lowest division level (highest detail) should have the specified resolution
        for (int i = 0; i < _divisions; i++) { resM = resM/2.0f; }
        _qRoot = new XB.QNode(ref _nextID, -_worldXSize/2, -_worldZSize/2, 
                              _worldXSize, _worldZSize, resM            );

        DivideQuadNode(ref _qRoot, _divisions);

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
                    (XB.AData.MainRoot, -colXPos, -colZPos, colXSize, colZSize, _resolutionC,
                     XB.LayerMasks.EnvironmentLayer, XB.LayerMasks.EnvironmentMask         );
            }
        }

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

        var q1 = new XB.QNode(ref _nextID, xPos+xSize/2.0f, zPos+zSize/2.0f, xSize, zSize, res, parent);
        var q2 = new XB.QNode(ref _nextID, xPos+xSize/2.0f, zPos-zSize/2.0f, xSize, zSize, res, parent);
        var q3 = new XB.QNode(ref _nextID, xPos-xSize/2.0f, zPos+zSize/2.0f, xSize, zSize, res, parent);
        var q4 = new XB.QNode(ref _nextID, xPos-xSize/2.0f, zPos-zSize/2.0f, xSize, zSize, res, parent);

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

    public static void UpdateCollisionTiles(float lowestPoint, float highestPoint,
                                            ref Godot.Image imgHeightMap          ) {
#if XBDEBUG
        var debug = new XB.DebugTimedBlock(XB.D.ManagerTerrainUpdateCollisionTiles);
#endif

        int colTileAmntX = (int)System.MathF.Ceiling(_worldXSize*_resolutionC / _sizeCTile);
        int colTileAmntZ = (int)System.MathF.Ceiling(_worldZSize*_resolutionC / _sizeCTile);
        for (int i = 0; i < colTileAmntX; i++) {
            for (int j = 0; j < colTileAmntZ; j++) {
                _terrainColTiles[i, j].InitializeCollisionMesh();
                _terrainColTiles[i, j].SampleTerrainNoise(_worldXSize, _worldZSize, lowestPoint,
                                                          highestPoint, ref imgHeightMap        );
                _terrainColTiles[i, j].ApplyToCollisionMesh();
            }
        }

#if XBDEBUG
        debug.End();
#endif 
    }

    // reference position can be the player model or player camera,
    // depending on where the highest resolution mesh should be prioritized
    public static void UpdateQTreeMeshes(Godot.Vector2 refPos, float lowestPoint,
                                         float highestPoint, ref Godot.Image imgHeightMap) {
#if XBDEBUG
        var debug = new XB.DebugTimedBlock(XB.D.ManagerTerrainUpdateQTreeMeshes);
#endif

        UpdateQNodeMesh(refPos, ref _qRoot, lowestPoint, highestPoint, ref imgHeightMap);   

#if XBDEBUG
        debug.End();
#endif 
    }

    private static void UpdateQNodeMesh(Godot.Vector2 refPos, ref XB.QNode qNode,
                                        float lowestPoint, float highestPoint,
                                        ref Godot.Image imgHeightMap             ) {
#if XBDEBUG
        var debug = new XB.DebugTimedBlock(XB.D.ManagerTerrainUpdateQNodeMesh);
#endif
        if (qNode.Children[0] == null) {
            if (!qNode.Visible) {
                RequestMeshContainer(ref qNode);
                qNode.UpdateAssignedMesh(_worldXSize, _worldZSize, lowestPoint, 
                                         highestPoint, ref imgHeightMap        );
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
            UpdateQNodeMesh(refPos, ref qNode.Children[0], lowestPoint, highestPoint, ref imgHeightMap);
            UpdateQNodeMesh(refPos, ref qNode.Children[1], lowestPoint, highestPoint, ref imgHeightMap);
            UpdateQNodeMesh(refPos, ref qNode.Children[2], lowestPoint, highestPoint, ref imgHeightMap);
            UpdateQNodeMesh(refPos, ref qNode.Children[3], lowestPoint, highestPoint, ref imgHeightMap);
        } else {
            if (!qNode.Visible) {
                RequestMeshContainer(ref qNode);
                qNode.UpdateAssignedMesh(_worldXSize, _worldZSize, lowestPoint,
                                         highestPoint, ref imgHeightMap        );
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
                _terrainMeshes[i].UseMesh(qNode.XPos, qNode.ZPos, qNode.XSize, qNode.ZSize, 
                                          _worldXSize, _worldZSize, qNode.Res              );
                _terrainMeshes[i].SetTerrainShaderAttributes();
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
        _terrainMeshes[newID].UseMesh(qNode.XPos, qNode.ZPos, qNode.XSize, qNode.ZSize, 
                                      _worldXSize, _worldZSize, qNode.Res              );
        _terrainMeshes[newID].SetTerrainShaderAttributes();

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

    public static void ResampleMeshes(float lowestPoint, float highestPoint,
                                      ref Godot.Image imgHeightMap          ) {
#if XBDEBUG
        var debug = new XB.DebugTimedBlock(XB.D.ManagerTerrainResampleMeshes);
#endif

        ResampleQNode(ref _qRoot, lowestPoint, highestPoint, ref imgHeightMap);

#if XBDEBUG
        debug.End();
#endif 
    }

    private static void ResampleQNode(ref XB.QNode qNode, float lowestPoint,
                                      float highestPoint, ref Godot.Image imgHeightMap) {
#if XBDEBUG
        var debug = new XB.DebugTimedBlock(XB.D.ManagerTerrainResampleMeshes);
#endif

        if (qNode == null) { //NOTE[ALEX]: this should never happen
            Godot.GD.Print("ResampleQNode with null child");
            return;
        }

        if (qNode.Visible) {
            qNode.UpdateAssignedMesh(_worldXSize, _worldZSize, lowestPoint,
                                     highestPoint, ref imgHeightMap        );
        } else {
            ResampleQNode(ref qNode.Children[0], lowestPoint, highestPoint, ref imgHeightMap);
            ResampleQNode(ref qNode.Children[1], lowestPoint, highestPoint, ref imgHeightMap);
            ResampleQNode(ref qNode.Children[2], lowestPoint, highestPoint, ref imgHeightMap);
            ResampleQNode(ref qNode.Children[3], lowestPoint, highestPoint, ref imgHeightMap);
        }

#if XBDEBUG
        debug.End();
#endif 
    }

    public static void UpdateBlockStrength(float multiplier) {
#if XBDEBUG
        var debug = new XB.DebugTimedBlock(XB.D.ManagerTerrainUpdateBlockStrength);
#endif

        for (int i = 0; i < _terrainMeshes.Count; i++) {
            if (_terrainMeshes[i].InUse) {
                _terrainMeshes[i].SetShaderAttribute("blockStr", multiplier*XB.WorldData.BlockStrength);
            }
        }

#if XBDEBUG
        debug.End();
#endif 
    }

    public static void UpdateQTreeStrength(float multiplier) {
#if XBDEBUG
        var debug = new XB.DebugTimedBlock(XB.D.ManagerTerrainUpdateQTreeStrength);
#endif

        for (int i = 0; i < _terrainMeshes.Count; i++) {
            if (_terrainMeshes[i].InUse) {
                _terrainMeshes[i].SetShaderAttribute("albVisStr", multiplier*XB.WorldData.QTreeStrength);
            }
        }

#if XBDEBUG
        debug.End();
#endif 
    }

#if XBDEBUG
    //prints tree depth first
    private static void PrintQTree(ref XB.QNode qNode) {
        if (qNode == null) { return; }
        qNode.DebugPrint("Called by PrintQTree");

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
            // texture has 0|0 in top left, in world coordinates, "top left" has 0|0 with negative axes
            int xCtr = (int)(-qNode.XPos*scaleFactor);
            int yCtr = (int)(-qNode.ZPos*scaleFactor);
            int dx   = (int)(qNode.XSize*scaleFactor);
            int dy   = (int)(qNode.ZSize*scaleFactor);
            int t    = 1;
            XB.Utils.UpdateRect2I(xCtr-dx/2,      yCtr-dy/2,      dx, t,  ref rects[0], ref vect);
            XB.Utils.UpdateRect2I(xCtr-dx/2,      yCtr-dy/2+dy-t, dx, t,  ref rects[1], ref vect);
            XB.Utils.UpdateRect2I(xCtr-dx/2,      yCtr-dy/2,      t,  dy, ref rects[2], ref vect);
            XB.Utils.UpdateRect2I(xCtr-dx/2+dx-t, yCtr-dy/2,      t,  dy, ref rects[3], ref vect);

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
