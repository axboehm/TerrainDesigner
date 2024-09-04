#define XBDEBUG
//#define BLUENOISETEXTURE
namespace XB { // namespace open
public class Random {
    public  static uint   RandomSeed    = 0;    // last used random seed
    private static uint[] _randomValues = new uint[4] {1, 1, 1, 1};
    private static uint   _rVPosition   = 0;
    private static uint   _x            = 25625625;
    private static uint   _y            = 51251251;
    private static uint   _z            = 10241024;
    private static uint   _w            = 20482048;
    public  static Godot.Image BlueNoise;            // square noise texture
    private static int         _blueNoiseSize = 64;  // pixels on one side (height = width), mult of 4

    public static void InitializeRandom(uint seed) {
#if XBDEBUG
        var debug = new XB.DebugTimedBlock(XB.D.RandomInitializeRandom);
#endif

        RandomSeed    = seed;
        _rVPosition   = 0;
        _x            = (uint)1 << (int)seed;
        _y            = (uint)1 << (int)(_x+1);
        _z            = (uint)1 << (int)(_y+1);
        _w            = (uint)1 << (int)(_z+1);
        Xorshift();
        _rVPosition   = 0;

#if BLUENOISETEXTURE
        BlueNoise      = Godot.Image.LoadFromFile(XB.ScenePaths.BlueNoiseTex);
        _blueNoiseSize = BlueNoise.GetWidth();
#else
        BlueNoise = Godot.Image.Create(_blueNoiseSize, _blueNoiseSize,
                                        false, Godot.Image.Format.L8  );
        var  randomColor = new Godot.Color(0.0f, 0.0f, 0.0f, 0.0f);
        for (int i = 0; i < _blueNoiseSize; i ++) {
            for (int j = 0; j < _blueNoiseSize; j ++) {
                randomColor.B = ((float)RandomUInt()) / ((float)255);
                BlueNoise.SetPixel(i, j, randomColor);
            }
        }
#endif

#if XBDEBUG
        debug.End();
#endif 
    }

    // returns 4 random bytes (uint 0-255)
    private static void Xorshift() {
#if XBDEBUG
        var debug = new XB.DebugTimedBlock(XB.D.RandomXorshift);
#endif

        uint t  = _x ^ (_x << 11);
             _x = _y;
             _y = _z;
             _z = _w;
             _w = _w ^ (_w >> 19) ^ (t ^ (t >> 8));

        _randomValues[0] = (_w >>  0) & 0xFF;
        _randomValues[1] = (_w >>  8) & 0xFF;
        _randomValues[2] = (_w >> 16) & 0xFF;
        _randomValues[3] = (_w >> 24) & 0xFF;

#if XBDEBUG
        debug.End();
#endif 
    }

    //NOTE[ALEX]: not thread-safe
    public static uint RandomUInt() {
#if XBDEBUG
       var debug = new XB.DebugTimedBlock(XB.D.RandomRandomUInt);
#endif

        if (_rVPosition > 3) {
            _rVPosition = 0;
            Xorshift();
            //Godot.GD.Print(_randomValues[0] + " " + _randomValues[1] + " "
            //             + _randomValues[2] + " " + _randomValues[3]);
        }
        uint res          = _randomValues[_rVPosition];
             _rVPosition += 1;
#if XBDEBUG
        debug.End();
#endif 
        return res;
    }

    public static float RandomInRangeF(float a, float b) {
#if XBDEBUG
        var debug = new XB.DebugTimedBlock(XB.D.RandomRandomInRangeF);
#endif

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
        if (r1 < r2) { rand = r1/r2; }
        else         { rand = r2/r1; }
        rand = XB.Utils.ClampF(rand, 0.0f, 1.0f);

        float result  = 0;
              result  = low;
              result += (diff*rand);
              result -= offset;
              result  = XB.Utils.ClampF(result, a, b);

#if XBDEBUG
        debug.End();
#endif 
        return result;
    }

    // returns a random integer between a and b, a and b included
    public static int RandomInRangeI(int a, int b) {
#if XBDEBUG
        var debug = new XB.DebugTimedBlock(XB.D.RandomRandomInRangeI);
#endif

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

#if XBDEBUG
        debug.End();
#endif 
        return result;
    }

    public static uint RandomInRangeU(uint a, uint b) {
#if XBDEBUG
        var debug = new XB.DebugTimedBlock(XB.D.RandomRandomInRangeU);
#endif

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

#if XBDEBUG
        debug.End();
#endif 
        return result;
    }

    public static float RandomBlueNoise(int x, int y) {
        x %= _blueNoiseSize;
        y %= _blueNoiseSize;
        return BlueNoise.GetPixel(x, y).R;
    }

    public static float ValueNoise2D(float x, float y) {
        int   xI = (int)x;
        int   yI = (int)y;
        float xF = x - xI;
        float yF = y - yI;

        float a = RandomBlueNoise(xI + 0, yI + 0);
        float b = RandomBlueNoise(xI + 1, yI + 0);
        float c = RandomBlueNoise(xI + 0, yI + 1);
        float d = RandomBlueNoise(xI + 1, yI + 1);

        var interpX = xF*xF*(3.0f - 2.0f*xF);
        var interpY = yF*yF*(3.0f - 2.0f*yF);

        float bot = XB.Utils.LerpF(a, b, interpX);
        float top = XB.Utils.LerpF(c, d, interpX);
        return XB.Utils.LerpF(bot, top, interpY);
    }
}
} // namespace close
