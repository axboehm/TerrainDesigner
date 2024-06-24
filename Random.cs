#define XBDEBUG
namespace XB { // namespace open
public class Random {
    private static uint[] _randomValues = new uint[4] {1, 1, 1, 1};
    private static uint   _rVPosition   = 0;
    private static uint   _x            = 25625625;
    private static uint   _y            = 51251251;
    private static uint   _z            = 10241024;
    private static uint   _w            = 20482048;
        
    public static void InitializeRandom(uint start) {
        _x            = start;
        _y            = _x + (_x/2);
        _z            = _y/2;
        _w            = _w/4;
        _randomValues = Xorshift();
        _rVPosition   = 0;
    }

    // returns 4 random bytes (uint 0-255)
    private static uint[] Xorshift() {
        uint t  = _x ^ (_x<<11);
             _x = _y;
             _y = _z;
             _z = _w;
             _w = _w ^ (_w>>19) ^ (t ^ (t>>8));
        return new uint[4] {_w & 0xFF, (_w>>8) & 0xFF, (_w>>16) & 0xFF, (_w>>24) & 0xFF};
    }

    // input from 0 to 100
    public static bool RandomChance(uint chance) {
        uint rand = (RandomUInt()%100) + 1; // from 1 to 100
        //Godot.GD.Print(chance + " " + rand + " " + (chance >= rand));
        if (chance >= rand) return true;
        return false;
    }

    public static uint RandomUInt() {
        if (_rVPosition > 3) {
            _rVPosition = 0;
            _randomValues = Xorshift();
            //Godot.GD.Print(_randomValues[0] + " " + _randomValues[1] + " "
            //             + _randomValues[2] + " " + _randomValues[3]);
        }
        uint res          = _randomValues[_rVPosition];
             _rVPosition += 1;
        return res;
    }

    public static float RandomInRangeF(float a, float b) {
        float low    = 0;
        float high   = 0;
        if (a < b) {
            low  = a;
            high = b;
        } else {
            low  = b;
            high = a;
        }

        float offset = 0;
        float diff   = high-low;
        if (low < 0) {
            offset  = -low;
            low    += offset;
            high   += offset;
        }

        // random
        float r1   = (float)RandomUInt();
        float r2   = (float)RandomUInt();
        float rand = 0;
        if (r1 < r2) rand = r1/r2;
        else         rand = r2/r1;
        rand = XB.Utils.ClampF(rand, 0.0f, 1.0f);

        float result  = 0;
              result  = low;
              result += (diff*rand);
              result -= offset;
              result  = XB.Utils.ClampF(result, a, b);

        return result;
    }

    public static Godot.Vector3 RandomInRangeV3(float a, float b) {
        Godot.Vector3 res   = new Godot.Vector3(0.0f, 0.0f, 0.0f);
                      res.X = RandomInRangeF(a, b);
                      res.Y = RandomInRangeF(a, b);
                      res.Z = RandomInRangeF(a, b);
        return res;
    }

    // returns a random integer between a and b, a and b included
    public static int RandomInRangeI(int a, int b) {
        int low    = 0;
        int high   = 0;
        if (a < b) {
            low  = a;
            high = b;
        } else {
            low  = b;
            high = a;
        }
        int offset = 0;
        if (low < 0) {
            offset  = -low;
            low    += offset;
            high   += offset;
        }
        int diff = high-low;

        int result  = (int)RandomUInt();
            result  = XB.Utils.AbsI(result);
            result %= (diff+1);
            result += low;
            result -= offset;
            result  = XB.Utils.ClampI(result, a, b);

        return result;
    }

    public static uint RandomInRangeU(uint a, uint b) {
        uint low    = 0;
        uint high   = 0;
        if (a < b) {
            low  = a;
            high = b;
        } else {
            low  = b;
            high = a;
        }
        uint diff = high-low;

        uint result  = RandomUInt();
             result %= (diff+1);
             result += low;
             result  = XB.Utils.ClampU(result, a, b);

        return result;
    }
}
} // namespace close
