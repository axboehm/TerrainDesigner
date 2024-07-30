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
    public bool Visible; // for testing
    public int     ID;
    public int     XPos; // center x coordinate in meter
    public int     ZPos;
    public int     XSize; // dimensions in meter
    public int     ZSize;
    public int     Res; // subdivisions per meter

    public QNode(ref int id, int xPos, int zPos, int xSize, int zSize, int res, QNode parent = null) {
        Parent   = parent;
        Children = new QNode[4];
        for (int i = 0; i < 4; i++) { Children[i] = null; }
        MeshContainer = null;

        ID    = id;
        id   += 1;
        XPos  = xPos;
        ZPos  = zPos;
        XSize = xSize;
        ZSize = zSize;
        Res   = res;
    }

    public void AssignMeshContainer(XB.MeshContainer mC) {
        Visible = true;
        MeshContainer = mC;
    }

    public void ReleaseMeshContainer() {
        if (!Visible) { return; }
        Visible = false;
        MeshContainer.ReleaseMesh();
        MeshContainer = null;
    }

    public void UpdateAssignedMesh() {
        Godot.GD.Print("updateAssigned");
        var pos = new Godot.Vector3(XPos, 0.0f, ZPos);
        MeshContainer.MeshInst.GlobalPosition = pos;

        var verts = new Godot.Vector3[4]; 
        var norms = new Godot.Vector3[4]; 
        var uvs   = new Godot.Vector2[4];
        var tris  = new int[6];
        verts[0] = new Godot.Vector3(-XSize/2,        1.1f, -ZSize/2       );
        verts[1] = new Godot.Vector3(-XSize/2,        1.2f, -ZSize/2 +ZSize);
        verts[2] = new Godot.Vector3(-XSize/2 +XSize, 0.8f, -ZSize/2       );
        verts[3] = new Godot.Vector3(-XSize/2 +XSize, 0.9f, -ZSize/2 +ZSize);
        norms[0] = new Godot.Vector3(0.0f, 1.0f, 0.0f);
        norms[1] = new Godot.Vector3(0.0f, 1.0f, 0.0f);
        norms[2] = new Godot.Vector3(0.0f, 1.0f, 0.0f);
        norms[3] = new Godot.Vector3(0.0f, 1.0f, 0.0f);
        uvs[0] = new Godot.Vector2(0.0f, 0.0f);
        uvs[1] = new Godot.Vector2(0.0f, 1.0f);
        uvs[2] = new Godot.Vector2(1.0f, 0.0f);
        uvs[3] = new Godot.Vector2(1.0f, 1.0f);
        tris[0] = 0;
        tris[1] = 3;
        tris[2] = 1;
        tris[3] = 0;
        tris[4] = 2;
        tris[5] = 3;

        var MeshData = new Godot.Collections.Array();
        MeshData.Resize((int)Godot.Mesh.ArrayType.Max);
        MeshData[(int)Godot.Mesh.ArrayType.Vertex] = verts;
        MeshData[(int)Godot.Mesh.ArrayType.Normal] = norms;
        MeshData[(int)Godot.Mesh.ArrayType.TexUV]  = uvs;
        MeshData[(int)Godot.Mesh.ArrayType.Index]  = tris;
        var ArrMesh  = new Godot.ArrayMesh();
        ArrMesh.AddSurfaceFromArrays(Godot.Mesh.PrimitiveType.Triangles, MeshData);
        MeshContainer.MeshInst.Mesh = ArrMesh;
        //col.Shape = arrMesh.CreateTrimeshShape();
    }
}

public class MeshContainer {
    public Godot.MeshInstance3D MeshInst;
    public float[,]             Heights;
    public int                  XAmount;
    public int                  ZAmount;
    public int                  ID;
    public bool                 InUse;

    public MeshContainer(Godot.Node root, int id) {
        MeshInst = new Godot.MeshInstance3D();
        root.AddChild(MeshInst);
        ID    = id;
        InUse = true;
    }

    public void ReleaseMesh() {
        MeshInst.Hide();
        InUse = false;
    }

    public void UseMesh() {
        MeshInst.Show();
        InUse = true;
    }

    public void CalculateHeights(int xSize, int zSize, int res) {
        XAmount = xSize*res + 1;
        ZAmount = xSize*res + 1;
        Heights = new float[XAmount, ZAmount];
        for (int i = 0; i < XAmount; i++) {
            for (int j = 0; j < ZAmount; j++) {
                Heights[i, j] = 2.0f; //TODO[ALEX]: height sampling
            }
        }
    }
}

//TODO[ALEX]: debug timing code once it works
public class ManagerTerrain {
    private static XB.QNode _qRoot;
    private static int      _nextID    = 0;
    private static int      _divisions = 0;
    private static SysCG.List<XB.MeshContainer> _terrainMeshes;

    public static void InitializeQuadTree(int xSize, int zSize, int res, int divisions) {
        _nextID    = 0;
        _divisions = divisions;
        _qRoot     = new XB.QNode(ref _nextID, xSize/2, zSize/2, xSize, zSize, res);

        _terrainMeshes = new SysCG.List<XB.MeshContainer>();

        DivideQuadNode(ref _qRoot, _divisions);

        //TODO[ALEX]: create inital meshes (first subdiv?)
    }

    // create a sparse quad tree without any mesh data
    private static void DivideQuadNode(ref XB.QNode parent, int divisions) {
        if (divisions == 0) { return; }
        divisions -= 1;
        
        int xPos  = parent.XPos;
        int zPos  = parent.ZPos;
        int xSize = parent.XSize/2;
        int zSize = parent.ZSize/2;

        var q1 = new XB.QNode(ref _nextID, xPos-xSize/2, zPos-zSize/2, xSize, zSize, parent.Res*2, parent);
        var q2 = new XB.QNode(ref _nextID, xPos-xSize/2, zPos+zSize/2, xSize, zSize, parent.Res*2, parent);
        var q3 = new XB.QNode(ref _nextID, xPos+xSize/2, zPos-zSize/2, xSize, zSize, parent.Res*2, parent);
        var q4 = new XB.QNode(ref _nextID, xPos+xSize/2, zPos+zSize/2, xSize, zSize, parent.Res*2, parent);

        parent.Children[0] = q1;
        parent.Children[1] = q2;
        parent.Children[2] = q3;
        parent.Children[3] = q4;

        DivideQuadNode(ref parent.Children[0], divisions);
        DivideQuadNode(ref parent.Children[1], divisions);
        DivideQuadNode(ref parent.Children[2], divisions);
        DivideQuadNode(ref parent.Children[3], divisions);
    }

    // since textures have 0|0 in the top left corner, make sure to pass the correct position here
    public static void UpdateQTreeMeshes(Godot.Vector2 camPos) {
        UpdateQNodeMesh(camPos, ref _qRoot);   
    }

    //TODO[ALEX]: updates and recycles all every frame, that is not necessary, check for changes only
    private static void UpdateQNodeMesh(Godot.Vector2 camPos, ref XB.QNode qNode) {
        if (qNode.Children[0] == null) {
            if (qNode.MeshContainer == null) {
                RequestTerrainMeshContainer(ref qNode);
                qNode.UpdateAssignedMesh();
            }
            return;
        } else {
            if (qNode.MeshContainer != null) {
                RecycleTerrainMeshContainer(ref qNode);
            }
        }

        var   qNodeCtr = new Godot.Vector2(qNode.XPos, qNode.ZPos);
        float dist     = (camPos-qNodeCtr).Length();
        float comp     = (qNode.XSize + qNode.ZSize) / 2;

        if (dist < comp) {
            UpdateQNodeMesh(camPos, ref qNode.Children[0]);
            UpdateQNodeMesh(camPos, ref qNode.Children[1]);
            UpdateQNodeMesh(camPos, ref qNode.Children[2]);
            UpdateQNodeMesh(camPos, ref qNode.Children[3]);
        } else {
            RequestTerrainMeshContainer(ref qNode);
            qNode.UpdateAssignedMesh();
        }
        return;
    }

    private static void RequestTerrainMeshContainer(ref XB.QNode qNode) {
        for (int i = 0; i < _terrainMeshes.Count; i++) {
            if (!_terrainMeshes[i].InUse) {
                qNode.AssignMeshContainer(_terrainMeshes[i]);
                _terrainMeshes[i].UseMesh();
                return;
            }
        }

        int newID = _terrainMeshes.Count;
        var mC    = new XB.MeshContainer(XB.AData.MainRoot, newID);
        _terrainMeshes.Add(mC);
        qNode.AssignMeshContainer(_terrainMeshes[newID]);
        _terrainMeshes[newID].UseMesh();
    }

    private static void RecycleTerrainMeshContainer(ref XB.QNode qNode) {
        qNode.ReleaseMeshContainer();
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

    public static void UpdateQTreeTexture(ref Godot.Image tex, int scaleFactor) {
        var rects = new Godot.Rect2I[4];
        var vect  = new Godot.Vector2I(0, 0);
        for (int i = 0; i < 4; i++) { rects[i] = new Godot.Rect2I(0, 0, 0, 0); }
        tex.Fill(XB.Col.Transp); // clear texture before drawing quadtree tiles

        DrawQNode(ref _qRoot, ref tex, scaleFactor, _divisions, ref rects, ref vect);
    }

    // scaleFactor adjust meter to pixel ratio
    private static void DrawQNode(ref XB.QNode qNode, ref Godot.Image tex,
                                  int scaleFactor, int iteration,
                                  ref Godot.Rect2I[] rects, ref Godot.Vector2I vect    ) {
        if (qNode == null) { return; }

        if (qNode.Visible) {
            int xCtr = qNode.XPos*scaleFactor;
            int yCtr = qNode.ZPos*scaleFactor;
            int dx   = qNode.XSize*scaleFactor;
            int dy   = qNode.ZSize*scaleFactor;
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
