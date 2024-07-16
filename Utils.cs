//#define XBDEBUG
namespace XB { // namespace open
public class Utils {
    public static float ClampF(float a, float b, float c) {
        if (a < b) return b;
        if (a > c) return c;
        return a;
    }

    public static int ClampI(int a, int b, int c) {
        if (a < b) return b;
        if (a > c) return c;
        return a;
    }

    public static uint ClampU(uint a, uint b, uint c) {
        if (a < b) return b;
        if (a > c) return c;
        return a;
    }

    public static int MinI(int a, int b) {
        if (a < b) return a;
        else       return b;
    }

    public static float MinF(float a, float b) {
        if (a < b) return a;
        else       return b;
    }

    public static float MaxF(float a, float b) {
        if (a > b) return a;
        else       return b;
    }

    public static float LerpF(float a, float b, float t) {
        t = XB.Utils.ClampF(t, 0.0f, 1.0f);
        return (a + (b-a)*t);
    }

    public static Godot.Vector2 LerpV2(Godot.Vector2 a, Godot.Vector2 b, float t) {
        t = XB.Utils.ClampF(t, 0.0f, 1.0f);
        return (a + (b-a)*t);
    }

    public static float AbsF(float a) {
        if (a < 0.0f) return (-1.0f*a);
        else          return a;
    }

    public static int AbsI(int a) {
        if (a < 0) return (-1*a);
        else       return a;
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

    public static void PlayUISound(string path) {
        var sScn  = Godot.ResourceLoader.Load<Godot.PackedScene>(path);
        var sound = sScn.Instantiate();
        XB.AData.MainRoot.AddChild(sound);
    }

    public static ulong BitStringToULong(string bitString, int length) {
        ulong result = 0;
        for (int i = 0; i < length; i++) {
            if (bitString[length-1-i] == '0') continue;
            ulong temp = 1;
            temp = temp << i;
            result |= temp;
        }
        return result;
    }

    public static string ULongToBitString(ulong bitCode, int length) {
        string result = "";
        for (int i = 0; i < length; i++) {
            ulong temp = 1;
            temp = temp << length-1-i;
            if (bitCode >= temp) {
                result += "1";
                bitCode -= temp;
            } else {
                result += "0";
            }
        }
        return result;
    }
}
} // namespace close
