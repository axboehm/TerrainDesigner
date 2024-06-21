//#define XBDEBUG
namespace XB { // namespace open
public class Utils {
    public static float ClampF(float a, float b, float c) {
        if (a < b) return b;
        if (a > c) return c;
        return a;
    }

    public static float LerpF(float a, float b, float t) {
        t = XB.Utils.ClampF(t, 0.0f, 1.0f);
        return (a + (b-a)*t);
    }

    public static Godot.Vector3 IntersectRayPlaneV3(Godot.Vector3 rPoint, Godot.Vector3 rDir,
                                                    Godot.Vector3 pPoint, Godot.Vector3 pNormal) {
        var res   = new Godot.Vector3(0.0f, 0.0f, 0.0f);
        var diff  = rPoint-pPoint;
        var prod1 = diff.Dot(pNormal);
        var prod2 = rDir.Dot(pNormal);
        if (prod2 == 0) return res; // ray is parallel to plane
        var prod3 = prod1/prod2;
            res   = rPoint - rDir*prod3;

        return res;
    }

    public static Godot.Collections.Dictionary Raycast(Godot.PhysicsDirectSpaceState3D spaceState,
                                                       Godot.Vector3 from, Godot.Vector3 to,
                                                       uint layerMask) {
        var res =  spaceState.IntersectRay
            (Godot.PhysicsRayQueryParameters3D.Create(from, to, layerMask));

        return res;
    }
}
} // namespace close
