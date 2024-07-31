#define XBDEBUG
namespace XB { // namespace open
public class Utils {
    public static int MaxRectSize = 5;

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

    public static int MaxI(int a, int b) {
        if (a > b) return a;
        else       return b;
    }

    public static double MaxD(double a, double b) {
        if (a > b) return a;
        else       return b;
    }

    public static float MaxF(float a, float b) {
        if (a > b) return a;
        else       return b;
    }

    public static float MaxInArrayF(float[] a) {
        float res = 0;
        for (int i = 0; i < a.Length; i++) {
            res = XB.Utils.MaxF(res, a[i]);
        }
        return res;
    }

    public static Godot.Vector2I MinV2I(Godot.Vector2I a, Godot.Vector2I b) {
        Godot.Vector2I res = new Godot.Vector2I(0, 0);
        if (a.X > b.X) { res.X = b.X; }
        else           { res.X = a.X; }
        if (a.Y > b.Y) { res.Y = b.Y; }
        else           { res.Y = a.Y; }

        return res;
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
#if XBDEBUG
        var debug = new XB.DebugTimedBlock(XB.D.UtilsIntersectRayPlaneV3);
#endif

        var res   = new Godot.Vector3(0.0f, 0.0f, 0.0f);
        var diff  = rPoint-pPoint;
        var prod1 = diff.Dot(pNormal);
        var prod2 = rDir.Dot(pNormal);
        if (prod2 == 0) return res; // ray is parallel to plane
        var prod3 = prod1/prod2;
            res   = rPoint - rDir*prod3;

#if XBDEBUG
        debug.End();
#endif 

        return res;
    }

    public static Godot.Collections.Dictionary Raycast(Godot.PhysicsDirectSpaceState3D spaceState,
                                                       Godot.Vector3 from, Godot.Vector3 to,
                                                       uint layerMask) {
#if XBDEBUG
        var debug = new XB.DebugTimedBlock(XB.D.UtilsRaycast);
#endif

        var res =  spaceState.IntersectRay
            (Godot.PhysicsRayQueryParameters3D.Create(from, to, layerMask));

#if XBDEBUG
        debug.End();
#endif 

        return res;
    }

    public static void PlayUISound(string path) {
#if XBDEBUG
        var debug = new XB.DebugTimedBlock(XB.D.UtilsPlayUISound);
#endif

        var sScn  = Godot.ResourceLoader.Load<Godot.PackedScene>(path);
        var sound = sScn.Instantiate();
        XB.AData.MainRoot.AddChild(sound);

#if XBDEBUG
        debug.End();
#endif 
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

    // reassign size and position of a Rect2I without any new allocations
    public static void UpdateRect2I(int posX, int posY, int sizeX, int sizeY,
                                    ref Godot.Rect2I rect, ref Godot.Vector2I vect) {
#if XBDEBUG
        var debug = new XB.DebugTimedBlock(XB.D.UtilsUpdateRect2I);
#endif

        vect.X = posX;
        vect.Y = posY;
        rect.Position = vect;
        vect.X = sizeX;
        vect.Y = sizeY;
        rect.Size = vect;

#if XBDEBUG
        debug.End();
#endif 
    }

    // returns an array of rectangles that represent digits from 0-9 in a segment display style
    // representation of the display, each letter represents one pixel, origin in top left corner
    // X demarks the outer border, the texture starts "inside" the Xs
    // a, b, c, d, e represent positions in the segment
    // the thickness t of the letters and the border around them is the same
    // the width and height describe the final size of the segment (inside the Xs)
    // 
    // XXXXXXXXXXXXXX
    // Xa           X
    // X            X
    // X  b-----e-  X
    // X  --------  X
    // X  --    --  X
    // X  --    --  X
    // X  c-----f-  X
    // X  --------  X
    // X  --    --  X
    // X  --    --  X
    // X  d-------  X
    // X  --------  X
    // X            X
    // X            X
    // XXXXXXXXXXXXXX
    //
    //NOTE[ALEX]: some of the coordinates are duplicates, they are kept to make debugging easier
    public static void DigitRectangles(int digit, int aX, int aY, int width, int height, int t,
                                       ref Godot.Rect2I[] rect,
                                       ref int rectSize, ref Godot.Vector2I vect      ) {
#if XBDEBUG
        var debug = new XB.DebugTimedBlock(XB.D.UtilsDigitRectangles);
#endif

        rectSize  = 0; // failsafe
        int xFull = width  - 2*t;
        int yFull = height - 2*t;
        int yBot  = yFull - height/2;
        int yTop  = yFull - yBot; // avoid vertical gaps
        int bX    = aX + t;
        int bY    = aY + t;
        int cX    = bX;
        int cY    = bY + yTop - t;
        int dX    = bX;
        int dY    = bY + yFull - t;
        int eX    = bX + xFull - t;
        int eY    = bY;
        int fX    = eX;
        int fY    = cY;
        switch (digit) {
            case 0: 
                rectSize = 4;
                UpdateRect2I(bX, bY, t,     yFull, ref rect[0], ref vect); // left full
                UpdateRect2I(eX, eY, t,     yFull, ref rect[1], ref vect); // right full
                UpdateRect2I(dX, dY, xFull, t,     ref rect[2], ref vect); // bottom
                UpdateRect2I(bX, bY, xFull, t,     ref rect[3], ref vect); // top
                break;
            case 1:
                rectSize = 1;
                UpdateRect2I(eX, eY, t,     yFull, ref rect[0], ref vect); // right full
                break;
            case 2:
                rectSize = 5;
                UpdateRect2I(cX, cY, xFull, t,     ref rect[0], ref vect); // middle
                UpdateRect2I(cX, cY, t,     yBot,  ref rect[1], ref vect); // left bottom
                UpdateRect2I(eX, eY, t,     yTop,  ref rect[2], ref vect); // right top
                UpdateRect2I(dX, dY, xFull, t,     ref rect[3], ref vect); // bottom
                UpdateRect2I(bX, bY, xFull, t,     ref rect[4], ref vect); // top
                break;
            case 3:
                rectSize = 4;
                UpdateRect2I(eX, eY, t,     yFull, ref rect[0], ref vect); // right full
                UpdateRect2I(cX, cY, xFull, t,     ref rect[1], ref vect); // middle
                UpdateRect2I(dX, dY, xFull, t,     ref rect[2], ref vect); // bottom
                UpdateRect2I(bX, bY, xFull, t,     ref rect[3], ref vect); // top
                break;
            case 4:
                rectSize = 3;
                UpdateRect2I(eX, eY, t,     yFull, ref rect[0], ref vect); // right full
                UpdateRect2I(cX, cY, xFull, t,     ref rect[1], ref vect); // middle
                UpdateRect2I(bX, bY, t,     yTop,  ref rect[2], ref vect); // left top
                break;
            case 5:
                rectSize = 5;
                UpdateRect2I(dX, dY, xFull, t,     ref rect[0], ref vect); // bottom
                UpdateRect2I(cX, cY, xFull, t,     ref rect[1], ref vect); // middle
                UpdateRect2I(bX, bY, xFull, t,     ref rect[2], ref vect); // top
                UpdateRect2I(fX, fY, t,     yBot,  ref rect[3], ref vect); // right bottom
                UpdateRect2I(bX, bY, t,     yTop,  ref rect[4], ref vect); // left top
                break;
            case 6:
                rectSize = 5;
                UpdateRect2I(bX, bY, t,     yFull, ref rect[0], ref vect); // left full
                UpdateRect2I(dX, dY, xFull, t,     ref rect[1], ref vect); // bottom
                UpdateRect2I(cX, cY, xFull, t,     ref rect[2], ref vect); // middle
                UpdateRect2I(bX, bY, xFull, t,     ref rect[3], ref vect); // top
                UpdateRect2I(fX, fY, t,     yBot,  ref rect[4], ref vect); // right bottom
                break;
            case 7:
                rectSize = 2;
                UpdateRect2I(eX, eY, t,     yFull, ref rect[0], ref vect); // right full
                UpdateRect2I(bX, bY, xFull, t,     ref rect[1], ref vect); // top
                break;
            case 8:
                rectSize = 5;
                UpdateRect2I(eX, eY, t,     yFull, ref rect[0], ref vect); // right full
                UpdateRect2I(bX, bY, t,     yFull, ref rect[1], ref vect); // left full
                UpdateRect2I(dX, dY, xFull, t,     ref rect[2], ref vect); // bottm
                UpdateRect2I(cX, cY, xFull, t,     ref rect[3], ref vect); // middle
                UpdateRect2I(bX, bY, xFull, t,     ref rect[4], ref vect); // top
                break;
            case 9:
                rectSize = 5;
                UpdateRect2I(eX, eY, t,     yFull, ref rect[0], ref vect); // right full
                UpdateRect2I(dX, dY, xFull, t,     ref rect[1], ref vect); // bottm
                UpdateRect2I(cX, cY, xFull, t,     ref rect[2], ref vect); // middle
                UpdateRect2I(bX, bY, xFull, t,     ref rect[3], ref vect); // top
                UpdateRect2I(bX, bY, t,     yTop,  ref rect[4], ref vect); // left top
                break;
        }

#if XBDEBUG
        debug.End();
#endif 
    }

    //NOTE[ALEX]: hardcoded step factor
    public static void BeveledRectangle(int xStart, int yStart, int d, ref Godot.Rect2I[] rect,
                                        ref int rectSize, ref Godot.Vector2I vect              ) {
#if XBDEBUG
        var debug = new XB.DebugTimedBlock(XB.D.UtilsBeveledRectangle);
#endif

        rectSize = 3;
        int step = (int)(0.1f*(float)d);
        UpdateRect2I(xStart+step, yStart,      d-2*step, d,        ref rect[0], ref vect);
        UpdateRect2I(xStart+step, yStart+step, d-2*step, d-2*step, ref rect[1], ref vect);
        UpdateRect2I(xStart,      yStart+step, d       , d-2*step, ref rect[2], ref vect);

#if XBDEBUG
        debug.End();
#endif 
    }

    public static void RectangleOutline(int xStart, int yStart, int d, int t,
                                        ref Godot.Rect2I[] rect, 
                                        ref int rectSize, ref Godot.Vector2I vect) {
#if XBDEBUG
        var debug = new XB.DebugTimedBlock(XB.D.UtilsRectangleOutline);
#endif

        rectSize = 4;
        UpdateRect2I(xStart,     yStart,     d, t, ref rect[0], ref vect); // top
        UpdateRect2I(xStart,     yStart+d-t, d, t, ref rect[1], ref vect); // bottom
        UpdateRect2I(xStart,     yStart,     t, d, ref rect[2], ref vect); // left
        UpdateRect2I(xStart+d-t, yStart,     t, d, ref rect[3], ref vect); // right

#if XBDEBUG
        debug.End();
#endif 
    }

    // draws three step point with center xPos | yPos
    public static void PointRectangles(int xPos, int yPos, int d, ref Godot.Rect2I[] rect,
                                       ref int rectSize, ref Godot.Vector2I vect   ) {
#if XBDEBUG
        var debug = new XB.DebugTimedBlock(XB.D.UtilsPointRectangles);
#endif

        rectSize = 3;
        int ws = (int)(0.4f*(float)d);
        int wl = (int)(0.8f*(float)d);
        UpdateRect2I(xPos - ws/2, yPos - d/2,  ws, d,  ref rect[0], ref vect);
        UpdateRect2I(xPos - d/2,  yPos - ws/2, d , ws, ref rect[1], ref vect);
        UpdateRect2I(xPos - wl/2, yPos - wl/2, wl, wl, ref rect[2], ref vect);

#if XBDEBUG
        debug.End();
#endif 
    }
}
} // namespace close
