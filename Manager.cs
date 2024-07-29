#define XBDEBUG
namespace XB { // namespace open
public class Manager {
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
} // namespace close
