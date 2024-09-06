#define XBDEBUG
using SysCG = System.Collections.Generic;
namespace XB { // namespace open
// DamSegment holds all the mesh data required to display a segment between two linked spheres
public class DamSegment {
    public int[] LinkedIDs; // ids of linked spheres(2)
    public int   ID;        // id of segment, not of linked spheres
    public bool  InUse;
    public Godot.MeshInstance3D    MeshInst;
    public Godot.Collections.Array MeshDataDam;
    public Godot.ArrayMesh         ArrMesh;
    public Godot.ShaderMaterial    MaterialDam;
    public Godot.ShaderMaterial    MaterialDamU;
    public Godot.Vector3[]         VerticesDam;
    public Godot.Vector2[]         UVsDam;
    public Godot.Vector3[]         NormalsDam;
    public int[]                   TrianglesDam;
    public int                     SegmentDivisions; // same amount of divisions, independent of length
                                                     // to reduce calculations and setup every frame
    public const int VAmnt = 7;
    private Godot.Vector3 _dirCU   = new Godot.Vector3(0.0f, 0.0f, 0.0f);
    private Godot.Vector3 _ort     = new Godot.Vector3(0.0f, 0.0f, 0.0f);
    private Godot.Vector3 _nrmU    = new Godot.Vector3(0.0f, 0.0f, 0.0f);
    private Godot.Vector3 _posSp2L = new Godot.Vector3(0.0f, 0.0f, 0.0f);
    private Godot.Vector3 _posSp1L = new Godot.Vector3(0.0f, 0.0f, 0.0f);
    private Godot.Vector3 _dirLU   = new Godot.Vector3(0.0f, 0.0f, 0.0f);
    private Godot.Vector3 _posSp2R = new Godot.Vector3(0.0f, 0.0f, 0.0f);
    private Godot.Vector3 _posSp1R = new Godot.Vector3(0.0f, 0.0f, 0.0f);
    private Godot.Vector3 _dirRU   = new Godot.Vector3(0.0f, 0.0f, 0.0f);
    private Godot.Vector3 _dirLUF  = new Godot.Vector3(0.0f, 0.0f, 0.0f);
    private Godot.Vector3 _dirRUF  = new Godot.Vector3(0.0f, 0.0f, 0.0f);
    private Godot.Vector3 _dirL1   = new Godot.Vector3(0.0f, 0.0f, 0.0f);
    private Godot.Vector3 _nrmL1   = new Godot.Vector3(0.0f, 0.0f, 0.0f);
    private Godot.Vector3 _dirL2   = new Godot.Vector3(0.0f, 0.0f, 0.0f);
    private Godot.Vector3 _nrmL2   = new Godot.Vector3(0.0f, 0.0f, 0.0f);
    private Godot.Vector3 _dirR1   = new Godot.Vector3(0.0f, 0.0f, 0.0f);
    private Godot.Vector3 _nrmR1   = new Godot.Vector3(0.0f, 0.0f, 0.0f);
    private Godot.Vector3 _dirR2   = new Godot.Vector3(0.0f, 0.0f, 0.0f);
    private Godot.Vector3 _nrmR2   = new Godot.Vector3(0.0f, 0.0f, 0.0f);
    private Godot.Vector3 _dirL    = new Godot.Vector3(0.0f, 0.0f, 0.0f);
    private Godot.Vector3 _nrmL    = new Godot.Vector3(0.0f, 0.0f, 0.0f);
    private Godot.Vector3 _dirR    = new Godot.Vector3(0.0f, 0.0f, 0.0f);
    private Godot.Vector3 _nrmR    = new Godot.Vector3(0.0f, 0.0f, 0.0f);

    // geometry layout of vertices and triangles (X is a single vertex, Y are two separate vertices)
    //
    // from above:
    //  |/|/|/|/|   8 triangles
    //  X-Y-X-Y-X   7 vertices
    //  |/|/|/|/|
    //
    // in section:
    //    Y-X-Y
    //   /     \
    //  X       X
    //
    //  1/2 3 4/5
    //  0       6
    //
    public DamSegment(Godot.Node root, int id, int segmentDivisions) {
#if XBDEBUG
        var debug = new XB.DebugTimedBlock(XB.D.DamSegment);
#endif

        MeshInst = new Godot.MeshInstance3D();
        root.AddChild(MeshInst);

        MaterialDam        = new Godot.ShaderMaterial();
        MaterialDam.Shader = Godot.ResourceLoader.Load<Godot.Shader>(XB.ResourcePaths.ConeDamShader);
        MaterialDam.SetShaderParameter("cTopInner", XB.Col.DamTI);
        MaterialDam.SetShaderParameter("cTopOuter", XB.Col.DamTO);
        MaterialDam.SetShaderParameter("cBotUpper", XB.Col.DamBU);
        MaterialDam.SetShaderParameter("cBotLower", XB.Col.DamBL);
        MaterialDam.RenderPriority = -1; // draw main material behind
        MaterialDamU        = new Godot.ShaderMaterial();
        MaterialDamU.Shader = Godot.ResourceLoader.Load<Godot.Shader>(XB.ResourcePaths.ConeDamUShader);
        MaterialDamU.SetShaderParameter("cTopInner", XB.Col.DamTI);
        MaterialDamU.SetShaderParameter("cTopOuter", XB.Col.DamTO);
        MaterialDamU.SetShaderParameter("cBotUpper", XB.Col.DamBU);
        MaterialDamU.SetShaderParameter("cBotLower", XB.Col.DamBL);
        MaterialDam.NextPass = MaterialDamU;

        MeshDataDam = new Godot.Collections.Array();
        MeshDataDam.Resize((int)Godot.Mesh.ArrayType.Max);
        ArrMesh     = new Godot.ArrayMesh();

        LinkedIDs = new int[2];
        ID        = id;
        InUse     = true;

        SegmentDivisions = segmentDivisions;
        VerticesDam      = new Godot.Vector3[7*SegmentDivisions];
        UVsDam           = new Godot.Vector2[7*SegmentDivisions];
        NormalsDam       = new Godot.Vector3[7*SegmentDivisions];
        TrianglesDam     = new int[8*(SegmentDivisions-1) * 3];

        var v2 = new Godot.Vector2(0.0f, 0.0f);
        for (int i = 0; i < SegmentDivisions; i++) {
            v2.X = i/(float)(SegmentDivisions-1);
            v2.Y = 1.0f;
            UVsDam[i*VAmnt + 0] = v2;
            UVsDam[i*VAmnt + 6] = v2;
            v2.Y = 0.5f;
            UVsDam[i*VAmnt + 1] = v2;
            UVsDam[i*VAmnt + 2] = v2;
            UVsDam[i*VAmnt + 4] = v2;
            UVsDam[i*VAmnt + 5] = v2;
            v2.Y = 0.0f;
            UVsDam[i*VAmnt + 3] = v2;
        }

        int tri  = 0;
        int vert = 0;
        for (int i = 0; i < SegmentDivisions-1; i++) {
            // side A
            TrianglesDam[tri + 0] = vert;
            TrianglesDam[tri + 1] = vert + VAmnt;
            TrianglesDam[tri + 2] = vert + VAmnt + 1;
            TrianglesDam[tri + 3] = vert;
            TrianglesDam[tri + 4] = vert + VAmnt + 1;
            TrianglesDam[tri + 5] = vert         + 1;
            tri  += 6;
            vert += 2;
            // top
            TrianglesDam[tri + 0] = vert;
            TrianglesDam[tri + 1] = vert + VAmnt;
            TrianglesDam[tri + 2] = vert + VAmnt + 1;
            TrianglesDam[tri + 3] = vert;
            TrianglesDam[tri + 4] = vert + VAmnt + 1;
            TrianglesDam[tri + 5] = vert         + 1;
            tri  += 6;
            vert += 1;
            TrianglesDam[tri + 0] = vert;
            TrianglesDam[tri + 1] = vert + VAmnt;
            TrianglesDam[tri + 2] = vert + VAmnt + 1;
            TrianglesDam[tri + 3] = vert;
            TrianglesDam[tri + 4] = vert + VAmnt + 1;
            TrianglesDam[tri + 5] = vert         + 1;
            tri  += 6;
            vert += 2;
            // side B
            TrianglesDam[tri + 0] = vert;
            TrianglesDam[tri + 1] = vert + VAmnt;
            TrianglesDam[tri + 2] = vert + VAmnt + 1;
            TrianglesDam[tri + 3] = vert;
            TrianglesDam[tri + 4] = vert + VAmnt + 1;
            TrianglesDam[tri + 5] = vert         + 1;
            tri  += 6;
            vert += 2;
        }

        MeshDataDam[(int)Godot.Mesh.ArrayType.TexUV] = UVsDam;
        MeshDataDam[(int)Godot.Mesh.ArrayType.Index] = TrianglesDam;

#if XBDEBUG
        debug.End();
#endif 
    }

    // make available again to manager (reuse instead of deleting)
    public void ReleaseMesh() {
#if XBDEBUG
        var debug = new XB.DebugTimedBlock(XB.D.DamSegmentReleaseMesh);
#endif

        LinkedIDs[0] = -1;
        LinkedIDs[1] = -1;
        MeshInst.Hide();
        InUse = false;

#if XBDEBUG
        debug.End();
#endif 
    }

    public void UseMesh(int linkedSphere1ID, int linkedSphere2ID) {
#if XBDEBUG
        var debug = new XB.DebugTimedBlock(XB.D.DamSegmentUseMesh);
#endif

        LinkedIDs[0] = linkedSphere1ID;
        LinkedIDs[1] = linkedSphere2ID;
        MeshInst.Show();
        InUse = true;

#if XBDEBUG
        debug.End();
#endif 
    }

    // takes values of two spheres and a length to update the corresponding connecting geometry
    //NOTE[ALEX]: the sphere positions are not ref because GlobalPosition does not work with ref
    public void UpdateMesh(float edgeLength,
                           Godot.Vector3 posSp1, float rSp1, float angSp1,
                           Godot.Vector3 posSp2, float rSp2, float angSp2 ) {
#if XBDEBUG
        var debug = new XB.DebugTimedBlock(XB.D.DamSegmentUpdateMesh);
#endif

        // Godot.GD.Print("UpdateMesh: Sp1: " + posSp1 + ", r: " + rSp1 + ", ang: " + angSp1
        //                         + ",Sp2: " + posSp2 + ", r: " + rSp2 + ", ang: " + angSp2);
        
        _dirCU = posSp2-posSp1; // direction in center of upper area
        _ort.X = _dirCU.X;
        _ort.Y = 0.0f;
        _ort.Z = _dirCU.Z;
        _ort   = _ort.Rotated(Godot.Vector3.Up, -90.0f*XB.Constants.Deg2Rad);
        _ort   = _ort.Normalized();
        _nrmU  = _dirCU.Rotated(_ort, 90.0f*XB.Constants.Deg2Rad); // normal for upper area
        _posSp2L = (posSp2 - rSp2*_ort);
        _posSp1L = (posSp1 - rSp1*_ort);
        _dirLU   = _posSp2L - _posSp1L; // direction at left upper edge
        _posSp2R = (posSp2 + rSp2*_ort);
        _posSp1R = (posSp1 + rSp1*_ort);
        _dirRU   = _posSp2R - _posSp1R; // direction at right upper edge
        float stepC = _dirCU.Length() / (float)(SegmentDivisions-1);
        float stepL = _dirLU.Length() / (float)(SegmentDivisions-1);
        float stepR = _dirRU.Length() / (float)(SegmentDivisions-1);
        _dirCU = _dirCU.Normalized();
        _dirLU = _dirLU.Normalized();
        _dirRU = _dirRU.Normalized();
        _dirLUF.X = _dirLU.X;
        _dirLUF.Y = 0.0f;
        _dirLUF.Z = _dirLU.Z;
        _dirLUF   = _dirLUF.Normalized();
        _dirRUF.X = _dirRU.X;
        _dirRUF.Y = 0.0f;
        _dirRUF.Z = _dirRU.Z;
        _dirRUF   = _dirRUF.Normalized();
        _dirL1 = (-_ort).Rotated(_dirLUF, -angSp1*XB.Constants.Deg2Rad); // direction of angle of Sp1
        _nrmL1 = _nrmU.Rotated  (_dirLUF, -angSp1*XB.Constants.Deg2Rad); // angled normal
        _dirL2 = (-_ort).Rotated(_dirLUF, -angSp2*XB.Constants.Deg2Rad);
        _nrmL2 = _nrmU.Rotated  (_dirLUF, -angSp2*XB.Constants.Deg2Rad);
        _dirR1 = _ort.Rotated   (_dirRUF, +angSp1*XB.Constants.Deg2Rad);
        _nrmR1 = _nrmU.Rotated  (_dirRUF, +angSp1*XB.Constants.Deg2Rad);
        _dirR2 = _ort.Rotated   (_dirRUF, +angSp2*XB.Constants.Deg2Rad);
        _nrmR2 = _nrmU.Rotated  (_dirRUF, +angSp2*XB.Constants.Deg2Rad);

        XB.Utils.ResetV3(ref _dirL);
        XB.Utils.ResetV3(ref _nrmL);
        XB.Utils.ResetV3(ref _dirR);
        XB.Utils.ResetV3(ref _nrmR);

        for (int i = 0; i < SegmentDivisions; i++) {
            float t = (float)i / (float)(SegmentDivisions-1);
            XB.Utils.LerpV3(ref _dirL1, ref _dirL2, t, ref _dirL);
            XB.Utils.LerpV3(ref _nrmL1, ref _nrmL2, t, ref _nrmL);
            XB.Utils.LerpV3(ref _dirR1, ref _dirR2, t, ref _dirR);
            XB.Utils.LerpV3(ref _nrmR1, ref _nrmR2, t, ref _nrmR);

            VerticesDam[i*VAmnt + 0] = _posSp1L + _dirLU*((float)i*stepL) + _dirL*edgeLength;
            VerticesDam[i*VAmnt + 1] = _posSp1L + _dirLU*((float)i*stepL);
            VerticesDam[i*VAmnt + 2] = _posSp1L + _dirLU*((float)i*stepL);
            VerticesDam[i*VAmnt + 3] = posSp1   + _dirCU*((float)i*stepC);
            VerticesDam[i*VAmnt + 4] = _posSp1R + _dirRU*((float)i*stepR);
            VerticesDam[i*VAmnt + 5] = _posSp1R + _dirRU*((float)i*stepR);
            VerticesDam[i*VAmnt + 6] = _posSp1R + _dirRU*((float)i*stepR) + _dirR*edgeLength;

            NormalsDam[i*VAmnt + 0] = _nrmL;
            NormalsDam[i*VAmnt + 1] = _nrmL;
            NormalsDam[i*VAmnt + 2] = _nrmU;
            NormalsDam[i*VAmnt + 3] = _nrmU;
            NormalsDam[i*VAmnt + 4] = _nrmU;
            NormalsDam[i*VAmnt + 5] = _nrmR;
            NormalsDam[i*VAmnt + 6] = _nrmR;
        }

#if XBDEBUG
        debug.End();
#endif 
    }

    // applying geometry is very performance intensive, so it is split into a separate function
    // if performance is not good enough it can be split over multiple frames
    // like is done with the terrain tiles
    public void ApplyToMesh() {
#if XBDEBUG
        var debug = new XB.DebugTimedBlock(XB.D.DamSegmentApplyToMesh);
#endif

        MeshDataDam[(int)Godot.Mesh.ArrayType.Vertex] = VerticesDam;
        MeshDataDam[(int)Godot.Mesh.ArrayType.Normal] = NormalsDam;

        ArrMesh.ClearSurfaces();
        ArrMesh.AddSurfaceFromArrays(Godot.Mesh.PrimitiveType.Triangles, MeshDataDam);

        MeshInst.Mesh = ArrMesh;
        MeshInst.Mesh.SurfaceSetMaterial(0, MaterialDam);

#if XBDEBUG
        debug.End();
#endif 
    }

#if XBDEBUG
    public void DebugPrint(string note) {
        string print = "Print DamSegment: " + ID.ToString() + ", In Use: " + InUse
                       + " " + note + '\n';
        print += "Linked Spheres: " + LinkedIDs[0].ToString() + ", " + LinkedIDs[1].ToString();
        print += '\n';
        Godot.GD.Print(print);
    }
#endif 
}

// ManagerSphere keeps track of all the spheres that the player can place and modify
// all spheres are initialized at startup and hidden when not in use, 
// no new sphere allocations are made during runtime
// linking or modifying spheres also passes through ManagerSphere
// cone meshes for each sphere are managed by each sphere itself, 
// whereas dam meshes are managed by ManagerSphere, they are allocated as necessary but not deleted,
// instead re-used if available
public class ManagerSphere {
    public const  int  MaxSphereAmount = 64; // limit to <= 99 because of sphere texture sizes,
                                             //NOTE[ALEX]: change this manually in miniMapO.gdshader
    public static int  NextSphere      = MaxSphereAmount;
    public static int  HLSphereID      = MaxSphereAmount;
    public static int  LinkingID       = MaxSphereAmount; // id of sphere to link with
    public static bool Linking         = false;
    public static bool Highlight       = false; // only used for guide in hud

    public  static XB.Sphere[] Spheres = new XB.Sphere[MaxSphereAmount];
    private static SysCG.List<XB.DamSegment> _damSegments;

    // at startup, all spheres are created,
    // dam segments are not created here but when they are required
    public static void InitializeSpheres() {
#if XBDEBUG
        var debug = new XB.DebugTimedBlock(XB.D.ManagerSphereInitializeSpheres);
#endif

        _damSegments = new SysCG.List<XB.DamSegment>();

        var rects = new Godot.Rect2I[XB.Utils.MaxRectSize]; // only required for initialization
        int rSize = 0;

        var sphereScn = Godot.ResourceLoader.Load<Godot.PackedScene>(XB.ResourcePaths.Sphere);
        XB.Sphere sphere;
        for (int i = 0; i < MaxSphereAmount; i++) {
            sphere = (XB.Sphere)sphereScn.Instantiate();
            sphere.InitializeSphere(i, ref rects, ref rSize);
            XB.AData.MainRoot.AddChild(sphere);
            Spheres[i] = sphere;
        }
        FindNextAvailableSphere();

#if XBDEBUG
        debug.End();
#endif 
    }

    public static void UpdateSpheres(float dt) {
#if XBDEBUG
        var debug = new XB.DebugTimedBlock(XB.D.ManagerSphereUpdateSpheres);
#endif

        for (int i = 0; i < MaxSphereAmount; i++) { Spheres[i].UpdateSphere(dt); }

        if (HLSphereID < MaxSphereAmount) { // for sphere under crosshairs
            Spheres[HLSphereID].Highlighted = 1.0f;
            Highlight = true;
        } else {
            Highlight = false;
        }
        if (LinkingID < MaxSphereAmount) {  // for spheres that are linked to
            Spheres[LinkingID].Highlighted  = 1.0f;
        }

#if XBDEBUG
        debug.End();
#endif 
    }

    public static void ChangeHighlightSphere(int newHLSphereID) {
#if XBDEBUG
        var debug = new XB.DebugTimedBlock(XB.D.ManagerSphereChangeHighlightSphere);
#endif

        if (HLSphereID == newHLSphereID) { return; }

        XB.PController.Hud.UpdateSphereTextureHighlight(HLSphereID, newHLSphereID);
        HLSphereID = newHLSphereID;

#if XBDEBUG
        debug.End();
#endif 
    }

    public static void FindNextAvailableSphere() {
#if XBDEBUG
        var debug = new XB.DebugTimedBlock(XB.D.ManagerSphereFindeNextAvailableSphere);
#endif

        NextSphere = MaxSphereAmount+1;
        for (int i = 0; i < MaxSphereAmount; i++) {
            if (!Spheres[i].Active) { NextSphere = XB.Utils.MinI(i, NextSphere); }
        }

#if XBDEBUG
        debug.End();
#endif 
    }

    public static void ToggleLinking() {
#if XBDEBUG
        var debug = new XB.DebugTimedBlock(XB.D.ManagerSphereToggleLinking);
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
        var debug = new XB.DebugTimedBlock(XB.D.ManagerSphereLinkSpheres);
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

            int damID = -1; //NOTE[ALEX]: deliberately set to -1 to catch errors
            RequestDamSegment(ref damID, LinkingID, HLSphereID);
            _damSegments[damID].UpdateMesh(XB.WData.SphereEdgeLength,
                                          Spheres[LinkingID].GlobalPosition,
                                          Spheres[LinkingID].Radius, Spheres[LinkingID].Angle,
                                          Spheres[HLSphereID].GlobalPosition,
                                          Spheres[HLSphereID].Radius, Spheres[HLSphereID].Angle);
            _damSegments[damID].ApplyToMesh();

            LinkingID = HLSphereID;
            Spheres[LinkingID].SphereTextureAddLinked();
        }

#if XBDEBUG
        debug.End();
#endif 
    }

    public static void UnsetLinkingID() {
#if XBDEBUG
        var debug = new XB.DebugTimedBlock(XB.D.ManagerSphereUnsetLinkingID);
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
        var debug = new XB.DebugTimedBlock(XB.D.ManagerSphereUnlinkSpheres);
#endif

        // Godot.GD.Print("unlinking: " + HLSphereID);
        Spheres[HLSphereID].SphereTextureRemoveLinked();
        Spheres[HLSphereID].UnlinkFromAllSpheres();
        RecycleDamSegment(HLSphereID);
        LinkingID = MaxSphereAmount;

#if XBDEBUG
        debug.End();
#endif 
    }

    // update all dam segments that are connected to sphere with sphereID
    public static void UpdateDam(int sphereID) {
#if XBDEBUG
        var debug = new XB.DebugTimedBlock(XB.D.ManagerSphereUpdateDam);
#endif

        // Godot.GD.Print("UpdateDam: " + sphereID);
        for (int i = 0; i < _damSegments.Count; i++) {
            if (!_damSegments[i].InUse) { continue; }

            if (   _damSegments[i].LinkedIDs[0] == sphereID
                || _damSegments[i].LinkedIDs[1] == sphereID) {
                _damSegments[i].UpdateMesh(XB.WData.SphereEdgeLength,
                                           Spheres[_damSegments[i].LinkedIDs[0]].GlobalPosition,
                                           Spheres[_damSegments[i].LinkedIDs[0]].Radius,
                                           Spheres[_damSegments[i].LinkedIDs[0]].Angle,
                                           Spheres[_damSegments[i].LinkedIDs[1]].GlobalPosition,
                                           Spheres[_damSegments[i].LinkedIDs[1]].Radius,
                                           Spheres[_damSegments[i].LinkedIDs[1]].Angle          );
                _damSegments[i].ApplyToMesh();
            }
        }

#if XBDEBUG
        debug.End();
#endif 
    }

    // change terrain heightmap by applying all spheres, then all dam segments
    // afterwards spheres and dam segments are removed from the world and made available for reuse
    //NOTE[ALEX]: this method of applying the modifications has some limitations:
    //            1 - spheres are always applied in the order of their index, which will produce
    //                different results depending on how they are placed to each other
    //            2 - dam segments are processed in the order they were initially created
    //                after several spheres have been linked and/or unlinked, this will not
    //                be easily apparent to the user anymore
    //                also the order of operation matters for intersections of dam segments
    //            accepting these limitations allows the dam segments and also this function
    //            to be much more simple and easier to understand, so for now this approach is used
    public static void ApplyTerrain() {
#if XBDEBUG
        var debug = new XB.DebugTimedBlock(XB.D.ManagerSphereApplyTerrain);
#endif

            Godot.GD.Print("testkkPre");
        for (int i = 0; i < MaxSphereAmount; i++) {
            if (!Spheres[i].Active) { continue; }

            XB.WData.ApplySphereCone(Spheres[i].GlobalPosition, Spheres[i].Radius, Spheres[i].Angle);
            XB.PController.Menu.ShowMessage("testpost"); //TODO[ALEX]: this does not show up when I want it to
        }
        for (int i = 0; i < _damSegments.Count; i++) {
            if (!_damSegments[i].InUse) { continue; }

            XB.WData.ApplyDamSegment(Spheres[_damSegments[i].LinkedIDs[0]].GlobalPosition,
                                     Spheres[_damSegments[i].LinkedIDs[0]].Radius,
                                     Spheres[_damSegments[i].LinkedIDs[0]].Angle,
                                     Spheres[_damSegments[i].LinkedIDs[1]].GlobalPosition,
                                     Spheres[_damSegments[i].LinkedIDs[1]].Radius,
                                     Spheres[_damSegments[i].LinkedIDs[1]].Angle          );
            _damSegments[i].ReleaseMesh();
        }
        for (int i = 0; i < MaxSphereAmount; i++) {
            if (!Spheres[i].Active) { continue; }

            Spheres[i].RemoveSphere();
        }

#if XBDEBUG
        debug.End();
#endif 
    }

    // remove all spheres and dam segments without applying them
    public static void ClearSpheres() {
#if XBDEBUG
        var debug = new XB.DebugTimedBlock(XB.D.ManagerSphereClearSpheres);
#endif

        for (int i = 0; i < _damSegments.Count; i++) {
            if (!_damSegments[i].InUse) { continue; }

            _damSegments[i].ReleaseMesh();
        }
        for (int i = 0; i < MaxSphereAmount; i++) {
            if (!Spheres[i].Active) { continue; }

            Spheres[i].RemoveSphere();
        }

#if XBDEBUG
        debug.End();
#endif 
    }

    // places the next available sphere at the requested position
    // if all spheres are in use, none are placed
    public static bool RequestSphere(ref Godot.Vector3 pos) {
#if XBDEBUG
        var debug = new XB.DebugTimedBlock(XB.D.ManagerSphereRequestSphere);
#endif

        if (NextSphere == MaxSphereAmount+1) { 
#if XBDEBUG
            debug.End();
#endif 
            return false; 
        }

        XB.PController.Hud.UpdateSphereTextureHighlight(HLSphereID, NextSphere);
        HLSphereID = NextSphere; // so that linking immediately works
        Spheres[NextSphere].PlaceSphere(ref pos);
        if (Linking && LinkingID < MaxSphereAmount) { LinkSpheres(); }

#if XBDEBUG
        debug.End();
#endif 

        return true;
    }

    // finds an unused dam segment or allocates a new one if none are available
    // writes the index of the segment that can be used into damID
    private static void RequestDamSegment(ref int damID, int linkedToID1, int linkedToID2) {
#if XBDEBUG
        var debug = new XB.DebugTimedBlock(XB.D.ManagerSphereRequestDamSegment);
#endif

        damID = -1;
        // Godot.GD.Print("RequestDamSegment: " + linkedToID1 + ", " + linkedToID2);
        for (int i = 0; i < _damSegments.Count; i++) {
            if (!_damSegments[i].InUse) {
                _damSegments[i].UseMesh(linkedToID1, linkedToID2);
                damID = i;

#if XBDEBUG
        debug.End();
#endif 

            return;
            }
        }

        int newID = _damSegments.Count;
        var dS    = new XB.DamSegment(XB.AData.MainRoot, newID, XB.WData.DamSegmentDivisions);
        _damSegments.Add(dS);
        _damSegments[newID].UseMesh(linkedToID1, linkedToID2);
        damID = newID;

#if XBDEBUG
        debug.End();
#endif 
    }

    // makes all dam segment that were connected to the sphere with sphereID available again
    // dam segments can not individually be disabled, but rather a sphere gets unlinked,
    // then all dam segments connected to that sphere are released
    public static void RecycleDamSegment(int sphereID) {
#if XBDEBUG
        var debug = new XB.DebugTimedBlock(XB.D.ManagerSphereRecycleDamSegment);
#endif

        // Godot.GD.Print("RecycleDamSegment: " + sphereID);
        for (int i = 0; i < _damSegments.Count; i++) {
            if (!_damSegments[i].InUse) { continue; }

            if (   _damSegments[i].LinkedIDs[0] == sphereID
                || _damSegments[i].LinkedIDs[1] == sphereID) {
                _damSegments[i].ReleaseMesh();;
            }
        }

#if XBDEBUG
        debug.End();
#endif 
    }
}
} // namespace end
