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

    // returns an array of rectangles that represent digits from 0-9 in a segment display style
    // representation of the display, each letter represents one pixel, origin in top left corner
    // X demarks the outer border, the texture starts "inside" the Xs
    // A, B, C, D, E represent positions in the segment
    // the thickness T of the letters and the border around them is the same
    // the width W and height H describe the final size of the segment (inside the Xs)
    // 
    // XXXXXXXXXXXXXX
    // XA           X
    // X            X
    // X  B-----E-  X
    // X  --------  X
    // X  --    --  X
    // X  --    --  X
    // X  C-----F-  X
    // X  --------  X
    // X  --    --  X
    // X  --    --  X
    // X  D-------  X
    // X  --------  X
    // X            X
    // X            X
    // XXXXXXXXXXXXXX
    //
    public static Godot.Rect2I[] DigitRectangles(int digit, int aX, int aY, int W, int H, int T) {
        Godot.Rect2I[] res = new Godot.Rect2I[0];
        int xFull = W - 2*T;
        int yFull = H - 2*T;
        int yBot  = yFull - H/2;
        int yTop  = yFull - yBot;
        int bX    = aX + T;
        int bY    = aY + T;
        int cX    = bX;
        int cY    = bY + yTop - T;
        int dX    = bX;
        int dY    = bY + yFull - T;
        int eX    = bX + xFull - T;
        int eY    = bY;
        int fX    = eX;
        int fY    = cY;
        switch (digit) {
            case 0: 
                res    = new Godot.Rect2I[4];
                res[0] = new Godot.Rect2I(bX, bY, T, yFull); // left full
                res[1] = new Godot.Rect2I(eX, eY, T, yFull); // right full
                res[2] = new Godot.Rect2I(dX, dY, xFull, T); // bot
                res[3] = new Godot.Rect2I(bX, bY, xFull, T); // top
                break;
            case 1:
                res    = new Godot.Rect2I[1];
                res[0] = new Godot.Rect2I(eX, eY, T, yFull); // right full
                break;
            case 2:
                res    = new Godot.Rect2I[5];
                res[0] = new Godot.Rect2I(cX, cY, xFull, T); // middle
                res[1] = new Godot.Rect2I(cX, cY, T, yBot);  // left bot
                res[2] = new Godot.Rect2I(eX, eY, T, yTop);  // right top
                res[3] = new Godot.Rect2I(dX, dY, xFull, T); // bot
                res[4] = new Godot.Rect2I(bX, bY, xFull, T); // top
                break;
            case 3:
                res    = new Godot.Rect2I[4];
                res[0] = new Godot.Rect2I(eX, eY, T, yFull); // right full
                res[1] = new Godot.Rect2I(cX, cY, xFull, T); // middle
                res[2] = new Godot.Rect2I(dX, dY, xFull, T); // bot
                res[3] = new Godot.Rect2I(bX, bY, xFull, T); // top
                break;
            case 4:
                res    = new Godot.Rect2I[3];
                res[0] = new Godot.Rect2I(eX, eY, T, yFull); // right full
                res[1] = new Godot.Rect2I(cX, cY, xFull, T); // middle
                res[2] = new Godot.Rect2I(bX, bY, T, yTop);  // left top
                break;
            case 5:
                res    = new Godot.Rect2I[5];
                res[0] = new Godot.Rect2I(dX, dY, xFull, T); // bot
                res[1] = new Godot.Rect2I(cX, cY, xFull, T); // middle
                res[2] = new Godot.Rect2I(bX, bY, xFull, T); // top
                res[3] = new Godot.Rect2I(fX, fY, T, yBot);  // right bot
                res[4] = new Godot.Rect2I(bX, bY, T, yTop);  // left top
                break;
            case 6:
                res    = new Godot.Rect2I[5];
                res[0] = new Godot.Rect2I(bX, bY, T, yFull); // left full
                res[1] = new Godot.Rect2I(dX, dY, xFull, T); // bot
                res[2] = new Godot.Rect2I(cX, cY, xFull, T); // middle
                res[3] = new Godot.Rect2I(bX, bY, xFull, T); // top
                res[4] = new Godot.Rect2I(fX, fY, T, yBot);  // right bot
                break;
            case 7:
                res    = new Godot.Rect2I[2];
                res[0] = new Godot.Rect2I(eX, eY, T, yFull); // right full
                res[1] = new Godot.Rect2I(bX, bY, xFull, T); // top
                break;
            case 8:
                res    = new Godot.Rect2I[5];
                res[0] = new Godot.Rect2I(eX, eY, T, yFull); // right full
                res[1] = new Godot.Rect2I(bX, bY, T, yFull); // left full
                res[2] = new Godot.Rect2I(dX, dY, xFull, T); // bot
                res[3] = new Godot.Rect2I(cX, cY, xFull, T); // middle
                res[4] = new Godot.Rect2I(bX, bY, xFull, T); // top
                break;
            case 9:
                res    = new Godot.Rect2I[5];
                res[0] = new Godot.Rect2I(eX, eY, T, yFull); // right full
                res[1] = new Godot.Rect2I(dX, dY, xFull, T); // bot
                res[2] = new Godot.Rect2I(cX, cY, xFull, T); // middle
                res[3] = new Godot.Rect2I(bX, bY, xFull, T); // top
                res[4] = new Godot.Rect2I(bX, bY, T, yTop);  // left top
                break;
            default:
                break;
        }
        return res;
    }

    public static Godot.Rect2I[] BeveledRectangle(int xStart, int yStart, int D) {
        Godot.Rect2I[] res = new Godot.Rect2I[3];
        int step = (int)(0.1f*(float)D);
        res[0] = new Godot.Rect2I(xStart+step, yStart,      D-2*step, D       );
        res[1] = new Godot.Rect2I(xStart+step, yStart+step, D-2*step, D-2*step);
        res[2] = new Godot.Rect2I(xStart,      yStart+step, D       , D-2*step);
        return res;
    }
}
} // namespace close
