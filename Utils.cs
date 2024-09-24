#define XBDEBUG
namespace XB { // namespace open
// Utils provides various basic math functions using as few allocations as possible
// also included are functions used when drawing to textures using Godot internal rectangles,
// references are used for the results of those functions to again reduce allocations as they
// are potentially called multiple times every frame
public class Utils {
    public  static int MaxRectSize = 5; // maximum amount of Rect2I used in functions in Utils
    private static Godot.Vector3  _v0 = new Godot.Vector3(0.0f, 0.0f, 0.0f);
    private static Godot.Vector2I _v1 = new Godot.Vector2I(0, 0);

    public static float ClampF(float a, float b, float c) {
        if (a < b) { return b; }
        if (a > c) { return c; }
        return a;
    }

    public static int ClampI(int a, int b, int c) {
        if (a < b) { return b; }
        if (a > c) { return c; }
        return a;
    }

    public static uint ClampU(uint a, uint b, uint c) {
        if (a < b) { return b; }
        if (a > c) { return c; }
        return a;
    }

    public static int MinI(int a, int b) {
        if (a < b) { return a; }
        else       { return b; }
    }

    public static float MinF(float a, float b) {
        if (a < b) { return a; }
        else       { return b; }
    }

    public static int MaxI(int a, int b) {
        if (a > b) { return a; }
        else       { return b; }
    }

    public static double MaxD(double a, double b) {
        if (a > b) { return a; }
        else       { return b; }
    }

    public static float MaxF(float a, float b) {
        if (a > b) { return a; }
        else       { return b; }
    }

    public static float MaxInArrayF(float[] a) {
#if XBDEBUG
        var debug = new XB.DebugTimedBlock(XB.D.UtilsMaxInArrayF);
        debug.End();
#endif 

        float res = 0;
        for (int i = 0; i < a.Length; i++) {
            res = XB.Utils.MaxF(res, a[i]);
        }
        return res;
    }

    public static float LerpF(float a, float b, float t) {
        t = XB.Utils.ClampF(t, 0.0f, 1.0f);
        return (a + (b-a)*t);
    }

    public static void LerpV2(ref Godot.Vector2 a, ref Godot.Vector2 b, float t,
                              ref Godot.Vector2 result                          ) {
#if XBDEBUG
        var debug = new XB.DebugTimedBlock(XB.D.UtilsLerpV2);
        debug.End();
#endif 

        t = XB.Utils.ClampF(t, 0.0f, 1.0f);
        result = a + (b-a)*t;
    }

    public static void LerpV3(ref Godot.Vector3 a, ref Godot.Vector3 b, float t,
                              ref Godot.Vector3 result                          ) {
#if XBDEBUG
        var debug = new XB.DebugTimedBlock(XB.D.UtilsLerpV3);
        debug.End();
#endif 

        t = XB.Utils.ClampF(t, 0.0f, 1.0f);
        result = a + (b-a)*t;
    }

    public static float AbsF(float a) {
        if (a < 0.0f) { return (-1.0f*a); }
        else          { return a;         }
    }

    public static int AbsI(int a) {
        if (a < 0) { return (-1*a); }
        else       { return a;      }
    }

    public static void ResetV2(ref Godot.Vector2 a) {
        a.X = 0.0f;
        a.Y = 0.0f;
    }

    public static void ResetV3(ref Godot.Vector3 a) {
        a.X = 0.0f;
        a.Y = 0.0f;
        a.Z = 0.0f;
    }

    public static void IntersectRayPlaneV3(ref Godot.Vector3 rOrig, ref Godot.Vector3 rDir,
                                           ref Godot.Vector3 pOrig, ref Godot.Vector3 pNormal,
                                           ref Godot.Vector3 result                           ) {
#if XBDEBUG
        var debug = new XB.DebugTimedBlock(XB.D.UtilsIntersectRayPlaneV3);
#endif

        ResetV3(ref _v0);
        _v0 = rOrig - pOrig;
        float prod1 = _v0.Dot(pNormal);
        float prod2 = rDir.Dot(pNormal);
        if (prod2 == 0) { // ray is parallel to plane
            result.X = 0;
            result.Y = 0;
            result.Z = 0;
        }
        float prod3 = prod1/prod2;
        result = rOrig - rDir*prod3;

#if XBDEBUG
        debug.End();
#endif 
    }

    public static void Raycast(ref Godot.PhysicsDirectSpaceState3D spaceState,
                               ref Godot.Vector3 from, ref Godot.Vector3 to, uint layerMask,
                               ref Godot.Collections.Dictionary result                      ) {
#if XBDEBUG
        var debug = new XB.DebugTimedBlock(XB.D.UtilsRaycast);
#endif

        result = spaceState.IntersectRay
            (Godot.PhysicsRayQueryParameters3D.Create(from, to, layerMask));

#if XBDEBUG
        debug.End();
#endif 
    }

    public static ulong BitStringToULong(string bitString, int length) {
        ulong result = 0;
        for (int i = 0; i < length; i++) {
            if (bitString[length-1-i] == '0') { continue; }
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
                                    ref Godot.Rect2I rect                    ) {
#if XBDEBUG
        var debug = new XB.DebugTimedBlock(XB.D.UtilsUpdateRect2I);
#endif

        _v1.X = posX;
        _v1.Y = posY;
        rect.Position = _v1;
        _v1.X = sizeX;
        _v1.Y = sizeY;
        rect.Size = _v1;

#if XBDEBUG
        debug.End();
#endif 
    }

    public static void FillRectanglesInImage(Godot.Image image, Godot.Rect2I[] rects,
                                             int rSize, ref Godot.Color color        ) {
#if XBDEBUG
        var debug = new XB.DebugTimedBlock(XB.D.UtilsFillRectanglesInImage);
#endif

        for (int i = 0; i < rSize; i++ ) { image.FillRect(rects[i], color); }

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
                                       Godot.Rect2I[] resultRect, ref int resultRectSize   ) {
#if XBDEBUG
        var debug = new XB.DebugTimedBlock(XB.D.UtilsDigitRectangles);
#endif

        resultRectSize = 0; // failsafe
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
                resultRectSize = 4;
                UpdateRect2I(bX, bY, t,     yFull, ref resultRect[0]); // left full
                UpdateRect2I(eX, eY, t,     yFull, ref resultRect[1]); // right full
                UpdateRect2I(dX, dY, xFull, t,     ref resultRect[2]); // bottom
                UpdateRect2I(bX, bY, xFull, t,     ref resultRect[3]); // top
                break;
            case 1:
                resultRectSize = 1;
                UpdateRect2I(eX, eY, t,     yFull, ref resultRect[0]); // right full
                break;
            case 2:
                resultRectSize = 5;
                UpdateRect2I(cX, cY, xFull, t,     ref resultRect[0]); // middle
                UpdateRect2I(cX, cY, t,     yBot,  ref resultRect[1]); // left bottom
                UpdateRect2I(eX, eY, t,     yTop,  ref resultRect[2]); // right top
                UpdateRect2I(dX, dY, xFull, t,     ref resultRect[3]); // bottom
                UpdateRect2I(bX, bY, xFull, t,     ref resultRect[4]); // top
                break;
            case 3:
                resultRectSize = 4;
                UpdateRect2I(eX, eY, t,     yFull, ref resultRect[0]); // right full
                UpdateRect2I(cX, cY, xFull, t,     ref resultRect[1]); // middle
                UpdateRect2I(dX, dY, xFull, t,     ref resultRect[2]); // bottom
                UpdateRect2I(bX, bY, xFull, t,     ref resultRect[3]); // top
                break;
            case 4:
                resultRectSize = 3;
                UpdateRect2I(eX, eY, t,     yFull, ref resultRect[0]); // right full
                UpdateRect2I(cX, cY, xFull, t,     ref resultRect[1]); // middle
                UpdateRect2I(bX, bY, t,     yTop,  ref resultRect[2]); // left top
                break;
            case 5:
                resultRectSize = 5;
                UpdateRect2I(dX, dY, xFull, t,     ref resultRect[0]); // bottom
                UpdateRect2I(cX, cY, xFull, t,     ref resultRect[1]); // middle
                UpdateRect2I(bX, bY, xFull, t,     ref resultRect[2]); // top
                UpdateRect2I(fX, fY, t,     yBot,  ref resultRect[3]); // right bottom
                UpdateRect2I(bX, bY, t,     yTop,  ref resultRect[4]); // left top
                break;
            case 6:
                resultRectSize = 5;
                UpdateRect2I(bX, bY, t,     yFull, ref resultRect[0]); // left full
                UpdateRect2I(dX, dY, xFull, t,     ref resultRect[1]); // bottom
                UpdateRect2I(cX, cY, xFull, t,     ref resultRect[2]); // middle
                UpdateRect2I(bX, bY, xFull, t,     ref resultRect[3]); // top
                UpdateRect2I(fX, fY, t,     yBot,  ref resultRect[4]); // right bottom
                break;
            case 7:
                resultRectSize = 2;
                UpdateRect2I(eX, eY, t,     yFull, ref resultRect[0]); // right full
                UpdateRect2I(bX, bY, xFull, t,     ref resultRect[1]); // top
                break;
            case 8:
                resultRectSize = 5;
                UpdateRect2I(eX, eY, t,     yFull, ref resultRect[0]); // right full
                UpdateRect2I(bX, bY, t,     yFull, ref resultRect[1]); // left full
                UpdateRect2I(dX, dY, xFull, t,     ref resultRect[2]); // bottm
                UpdateRect2I(cX, cY, xFull, t,     ref resultRect[3]); // middle
                UpdateRect2I(bX, bY, xFull, t,     ref resultRect[4]); // top
                break;
            case 9:
                resultRectSize = 5;
                UpdateRect2I(eX, eY, t,     yFull, ref resultRect[0]); // right full
                UpdateRect2I(dX, dY, xFull, t,     ref resultRect[1]); // bottm
                UpdateRect2I(cX, cY, xFull, t,     ref resultRect[2]); // middle
                UpdateRect2I(bX, bY, xFull, t,     ref resultRect[3]); // top
                UpdateRect2I(bX, bY, t,     yTop,  ref resultRect[4]); // left top
                break;
        }

#if XBDEBUG
        debug.End();
#endif 
    }

    //NOTE[ALEX]: hardcoded step factor
    public static void BeveledRectangle(int xStart, int yStart, int d,
                                        Godot.Rect2I[] resultRect, ref int resultRectSize) {
#if XBDEBUG
        var debug = new XB.DebugTimedBlock(XB.D.UtilsBeveledRectangle);
#endif

        resultRectSize = 3;
        int step = (int)(0.1f*(float)d);
        UpdateRect2I(xStart+step, yStart,      d-2*step, d,        ref resultRect[0]);
        UpdateRect2I(xStart+step, yStart+step, d-2*step, d-2*step, ref resultRect[1]);
        UpdateRect2I(xStart,      yStart+step, d       , d-2*step, ref resultRect[2]);

#if XBDEBUG
        debug.End();
#endif 
    }

    public static void RectangleOutline(int xStart, int yStart, int d, int t,
                                        Godot.Rect2I[] resultRect, ref int resultRectSize) {
#if XBDEBUG
        var debug = new XB.DebugTimedBlock(XB.D.UtilsRectangleOutline);
#endif

        resultRectSize = 4;
        UpdateRect2I(xStart,     yStart,     d, t, ref resultRect[0]); // top
        UpdateRect2I(xStart,     yStart+d-t, d, t, ref resultRect[1]); // bottom
        UpdateRect2I(xStart,     yStart,     t, d, ref resultRect[2]); // left
        UpdateRect2I(xStart+d-t, yStart,     t, d, ref resultRect[3]); // right

#if XBDEBUG
        debug.End();
#endif 
    }

    // draws three step point with center xPos | yPos
    public static void PointRectangles(int xPos, int yPos, int d, 
                                       Godot.Rect2I[] resultRect, ref int resultRectSize) {
#if XBDEBUG
        var debug = new XB.DebugTimedBlock(XB.D.UtilsPointRectangles);
#endif

        // Godot.GD.Print("PointRectangles around: " + xPos + ", " + yPos);

        resultRectSize = 3;
        int ws = (int)(0.4f*(float)d);
        int wl = (int)(0.8f*(float)d);
        UpdateRect2I(xPos - ws/2, yPos - d/2,  ws, d,  ref resultRect[0]);
        UpdateRect2I(xPos - d/2,  yPos - ws/2, d , ws, ref resultRect[1]);
        UpdateRect2I(xPos - wl/2, yPos - wl/2, wl, wl, ref resultRect[2]);

#if XBDEBUG
        debug.End();
#endif 
    }
}
} // namespace close
