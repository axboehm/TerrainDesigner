#define XBDEBUG
using SysCG = System.Collections.Generic;
namespace XB { // namespace open
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
    public int                     SegmentDivisions;
    public const int VAmnt = 7;

    // geometry layout of vertices (X is a single vertex, Y are two separate vertices)
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
    //    2 3 4
    //  0 1   5 6
    //
    public DamSegment(Godot.Node root, int id, int segmentDivisions) {
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
    }

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

    public void UpdateMesh(float edgeLength,
                           Godot.Vector3 posSp1, float rSp1, float angSp1,
                           Godot.Vector3 posSp2, float rSp2, float angSp2 ) {
#if XBDEBUG
        var debug = new XB.DebugTimedBlock(XB.D.DamSegmentUpdateMesh);
#endif

        // Godot.GD.Print("UpdateMesh: Sp1: " + posSp1 + ", r: " + rSp1 + ", ang: " + angSp1
        //                         + ",Sp2: " + posSp2 + ", r: " + rSp2 + ", ang: " + angSp2);
        
        var dirCU = posSp2-posSp1; // direction in center of upper area
        var ort   = new Godot.Vector3(dirCU.X, 0.0f, dirCU.Z);
            ort   = ort.Rotated(Godot.Vector3.Up, -90.0f*XB.Constants.Deg2Rad);
            ort   = ort.Normalized();
        var nrmU  = dirCU.Rotated(ort, 90.0f*XB.Constants.Deg2Rad); // normal for upper area
        var posSp2L = (posSp2 - rSp2*ort);
        var posSp1L = (posSp1 - rSp1*ort);
        var dirLU   = posSp2L - posSp1L; // direction at left upper edge
        var posSp2R = (posSp2 + rSp2*ort);
        var posSp1R = (posSp1 + rSp1*ort);
        var dirRU   = posSp2R - posSp1R; // direction at right upper edge
        float stepC = dirCU.Length() / (float)(SegmentDivisions-1);
        float stepL = dirLU.Length() / (float)(SegmentDivisions-1);
        float stepR = dirRU.Length() / (float)(SegmentDivisions-1);
        dirCU = dirCU.Normalized();
        dirLU = dirLU.Normalized();
        dirRU = dirRU.Normalized();
        var dirLUF = new Godot.Vector3(dirLU.X, 0.0f, dirLU.Z);
            dirLUF = dirLUF.Normalized();
        var dirRUF = new Godot.Vector3(dirRU.X, 0.0f, dirRU.Z);
            dirRUF = dirRUF.Normalized();
        var dirL1 = (-ort).Rotated(dirLUF, -angSp1*XB.Constants.Deg2Rad); // direction of angle of Sp1
        var nrmL1 = nrmU.Rotated  (dirLUF, -angSp1*XB.Constants.Deg2Rad); // angled normal
        var dirL2 = (-ort).Rotated(dirLUF, -angSp2*XB.Constants.Deg2Rad);
        var nrmL2 = nrmU.Rotated  (dirLUF, -angSp2*XB.Constants.Deg2Rad);
        var dirR1 = ort.Rotated   (dirRUF, +angSp1*XB.Constants.Deg2Rad);
        var nrmR1 = nrmU.Rotated  (dirRUF, +angSp1*XB.Constants.Deg2Rad);
        var dirR2 = ort.Rotated   (dirRUF, +angSp2*XB.Constants.Deg2Rad);
        var nrmR2 = nrmU.Rotated  (dirRUF, +angSp2*XB.Constants.Deg2Rad);

        var dirL = new Godot.Vector3(0.0f, 0.0f, 0.0f);
        var nrmL = new Godot.Vector3(0.0f, 0.0f, 0.0f);
        var dirR = new Godot.Vector3(0.0f, 0.0f, 0.0f);
        var nrmR = new Godot.Vector3(0.0f, 0.0f, 0.0f);

        for (int i = 0; i < SegmentDivisions; i++) {
            float t = (float)i / (float)(SegmentDivisions-1);
            dirL = XB.Utils.LerpV3(dirL1, dirL2, t);
            nrmL = XB.Utils.LerpV3(nrmL1, nrmL2, t);
            dirR = XB.Utils.LerpV3(dirR1, dirR2, t);
            nrmR = XB.Utils.LerpV3(nrmR1, nrmR2, t);

            VerticesDam[i*VAmnt + 0] = posSp1L + dirLU*((float)i*stepL) + dirL*edgeLength;
            VerticesDam[i*VAmnt + 1] = posSp1L + dirLU*((float)i*stepL);
            VerticesDam[i*VAmnt + 2] = posSp1L + dirLU*((float)i*stepL);
            VerticesDam[i*VAmnt + 3] = posSp1  + dirCU*((float)i*stepC);
            VerticesDam[i*VAmnt + 4] = posSp1R + dirRU*((float)i*stepR);
            VerticesDam[i*VAmnt + 5] = posSp1R + dirRU*((float)i*stepR);
            VerticesDam[i*VAmnt + 6] = posSp1R + dirRU*((float)i*stepR) + dirR*edgeLength;

            NormalsDam[i*VAmnt + 0] = nrmL;
            NormalsDam[i*VAmnt + 1] = nrmL;
            NormalsDam[i*VAmnt + 2] = nrmU;
            NormalsDam[i*VAmnt + 3] = nrmU;
            NormalsDam[i*VAmnt + 4] = nrmU;
            NormalsDam[i*VAmnt + 5] = nrmR;
            NormalsDam[i*VAmnt + 6] = nrmR;
        }

#if XBDEBUG
        debug.End();
#endif 
    }

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

    public void DebugPrint(string note) {
        string print = "Print DamSegment: " + ID.ToString() + ", In Use: " + InUse
                       + " " + note + '\n';
        print += "Linked Spheres: " + LinkedIDs[0].ToString() + ", " + LinkedIDs[1].ToString();
        print += '\n';
        Godot.GD.Print(print);
    }
}

public class ManagerSphere {
    public const  int  MaxSphereAmount = 64; // limit to <= 99 because of sphere texture sizes,
                                             //NOTE[ALEX]: change this manually in miniMapO.gdshader
    public static int  ActiveSpheres   = 0;
    public static int  NextSphere      = MaxSphereAmount;
    public static int  HLSphereID      = MaxSphereAmount;
    public static int  LinkingID       = MaxSphereAmount; // id of sphere to link with
    public static bool Linking         = false;

    public static XB.Sphere[] Spheres = new XB.Sphere[MaxSphereAmount];
    public static SysCG.List<XB.DamSegment> DamSegments;

    public static void InitializeSpheres() {
#if XBDEBUG
        var debug = new XB.DebugTimedBlock(XB.D.ManagerSphereInitializeSpheres);
#endif

        DamSegments = new SysCG.List<XB.DamSegment>();

        var rects = new Godot.Rect2I[XB.Utils.MaxRectSize];
        int rSize = 0;
        var vect  = new Godot.Vector2I(0, 0);

        var sphereScn = Godot.ResourceLoader.Load<Godot.PackedScene>(XB.ResourcePaths.Sphere);
        XB.Sphere sphere;
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
        var debug = new XB.DebugTimedBlock(XB.D.ManagerSphereUpdateSpheres);
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
        var debug = new XB.DebugTimedBlock(XB.D.ManagerSphereChangeHighlightSphere);
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
        var debug = new XB.DebugTimedBlock(XB.D.ManagerSphereUpdateActiveSpheres);
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

            int damID = -1;
            RequestDamSegment(ref damID, LinkingID, HLSphereID);
            DamSegments[damID].UpdateMesh(XB.WData.SphereEdgeLength,
                                          Spheres[LinkingID].GlobalPosition,
                                          Spheres[LinkingID].Radius, Spheres[LinkingID].Angle,
                                          Spheres[HLSphereID].GlobalPosition,
                                          Spheres[HLSphereID].Radius, Spheres[HLSphereID].Angle);
            DamSegments[damID].ApplyToMesh();

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

    public static void UpdateDam(int sphereID) {
#if XBDEBUG
        var debug = new XB.DebugTimedBlock(XB.D.ManagerSphereUpdateDam);
#endif

        // Godot.GD.Print("UpdateDam: " + sphereID);
        for (int i = 0; i < DamSegments.Count; i++) {
            if (!DamSegments[i].InUse) { continue; }

            if (   DamSegments[i].LinkedIDs[0] == sphereID
                || DamSegments[i].LinkedIDs[1] == sphereID) {
                DamSegments[i].UpdateMesh(XB.WData.SphereEdgeLength,
                                          Spheres[DamSegments[i].LinkedIDs[0]].GlobalPosition,
                                          Spheres[DamSegments[i].LinkedIDs[0]].Radius,
                                          Spheres[DamSegments[i].LinkedIDs[0]].Angle,
                                          Spheres[DamSegments[i].LinkedIDs[1]].GlobalPosition,
                                          Spheres[DamSegments[i].LinkedIDs[1]].Radius,
                                          Spheres[DamSegments[i].LinkedIDs[1]].Angle          );
                DamSegments[i].ApplyToMesh();
            }
        }

#if XBDEBUG
        debug.End();
#endif 
    }

    public static void ApplyTerrain() {
#if XBDEBUG
        var debug = new XB.DebugTimedBlock(XB.D.ManagerSphereApplyTerrain);
#endif

        for (int i = 0; i < MaxSphereAmount; i++) {
            if (!Spheres[i].Active) { continue; }

            XB.WData.ApplySphereCone(Spheres[i].GlobalPosition, Spheres[i].Radius, Spheres[i].Angle);
        }
        for (int i = 0; i < DamSegments.Count; i++) {
            if (!DamSegments[i].InUse) { continue; }

            XB.WData.ApplyDamSegment(Spheres[DamSegments[i].LinkedIDs[0]].GlobalPosition,
                                     Spheres[DamSegments[i].LinkedIDs[0]].Radius,
                                     Spheres[DamSegments[i].LinkedIDs[0]].Angle,
                                     Spheres[DamSegments[i].LinkedIDs[1]].GlobalPosition,
                                     Spheres[DamSegments[i].LinkedIDs[1]].Radius,
                                     Spheres[DamSegments[i].LinkedIDs[1]].Angle          );
            DamSegments[i].ReleaseMesh();
        }
        for (int i = 0; i < MaxSphereAmount; i++) {
            if (!Spheres[i].Active) { continue; }

            Spheres[i].RemoveSphere();
        }

#if XBDEBUG
        debug.End();
#endif 
    }

    public static void ClearSpheres() {
#if XBDEBUG
        var debug = new XB.DebugTimedBlock(XB.D.ManagerSphereClearSpheres);
#endif

        for (int i = 0; i < DamSegments.Count; i++) {
            if (!DamSegments[i].InUse) { continue; }

            DamSegments[i].ReleaseMesh();
        }
        for (int i = 0; i < MaxSphereAmount; i++) {
            if (!Spheres[i].Active) { continue; }

            Spheres[i].RemoveSphere();
        }

#if XBDEBUG
        debug.End();
#endif 
    }

    public static bool RequestSphere(Godot.Vector3 pos) {
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
        Spheres[NextSphere].PlaceSphere(pos);
        if (Linking && LinkingID < MaxSphereAmount) { LinkSpheres(); }

#if XBDEBUG
        debug.End();
#endif 

        return true;
    }

    private static void RequestDamSegment(ref int damID, int linkedToID1, int linkedToID2) {
#if XBDEBUG
        var debug = new XB.DebugTimedBlock(XB.D.ManagerSphereRequestDamSegment);
#endif

        // Godot.GD.Print("RequestDamSegment: " + linkedToID1 + ", " + linkedToID2);
        for (int i = 0; i < DamSegments.Count; i++) {
            if (!DamSegments[i].InUse) {
                DamSegments[i].UseMesh(linkedToID1, linkedToID2);
                damID = i;

#if XBDEBUG
        debug.End();
#endif 

            return;
            }
        }

        int newID = DamSegments.Count;
        var dS    = new XB.DamSegment(XB.AData.MainRoot, newID, XB.WData.DamSegmentDivisions);
        DamSegments.Add(dS);
        DamSegments[newID].UseMesh(linkedToID1, linkedToID2);
        damID = newID;

#if XBDEBUG
        debug.End();
#endif 
    }

    public static void RecycleDamSegment(int sphereID) {
#if XBDEBUG
        var debug = new XB.DebugTimedBlock(XB.D.ManagerSphereRecycleDamSegment);
#endif

        // Godot.GD.Print("RecycleDamSegment: " + sphereID);
        for (int i = 0; i < DamSegments.Count; i++) {
            if (!DamSegments[i].InUse) { continue; }

            if (   DamSegments[i].LinkedIDs[0] == sphereID
                || DamSegments[i].LinkedIDs[1] == sphereID) {
                DamSegments[i].ReleaseMesh();;
            }
        }

#if XBDEBUG
        debug.End();
#endif 
    }
}
} // namespace end
