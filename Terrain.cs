// #define XBSINGLETHREADED
#define XBDEBUG
namespace XB { // namespace open
public enum Direction {
    Up,
    Down,
}

public class Terrain {
    //NOTE[ALEX]: parallelization with function parameters is a headache, so all the relevant
    //            factors get saved here instead of calculated in the FBM part
    public static float Height        = 0.0f;
    public static float NoiseScale    = 0.0f;   //NOTE[ALEX]: if scale is a multiple of the blue
                                                //            noise texture's size, the pattern
                                                //            will repeat visibly
    public static float OffsetX       = 0.0f;
    public static float OffsetZ       = 0.0f;
    public static int   Octaves       = 0;      // number of layers of noise
    public static float AmpMult       = 0.0f;   // controls change of amplitude for next octave's noise
    public static float Lacunarity    = 0.0f;   // change of frequency of next octave's noise
    public static float Exponentation = 0.0f;   // exponent of power function of final result
    public static float Normalization = 0.0f;   // maximum amount for added noise samples
    public static float XStep         = 0.0f;
    public static float ZStep         = 0.0f;

    public static void SetTerrainParameters(float height = 18.0f,    float scale = 0.0174f, 
                                            float offsetX = 0.0f,    float offsetZ = 0.0f, 
                                            int   octaves = 4,       float persistence = 0.5f,
                                            float lacunarity = 2.0f, float exponentation = 4.5f) {
#if XBDEBUG
        var debug = new XB.DebugTimedBlock(XB.D.TerrainSetTerrainParameters);
#endif

        if (scale % 1.0f == 0.0f) { NoiseScale = scale + 0.5f; } // mitigate visible repititions
        else                      { NoiseScale = scale;        }
        Height  = height;
        OffsetX = offsetX;
        OffsetZ = offsetZ;
        Octaves = octaves;
        AmpMult = (float)System.Math.Pow(2.0f, -persistence);
        Lacunarity    = lacunarity;
        Exponentation = exponentation;

#if XBDEBUG
        debug.End();
#endif 
    }

    // FBM (fractal brownian motion) noise is an addition of multiple layers of perlin noise,
    // each with increasing frequency (detail) but less amplitude (strength)
    //NOTE[ALEX]: parallel for loops do not work with ref parameters, so the height array is hard-coded
    public static void FBM(int amountX, int amountZ, float sizeX, float sizeZ) {
#if XBDEBUG
        var debug = new XB.DebugTimedBlock(XB.D.TerrainFBM);
#endif

        XStep = NoiseScale * (sizeX / (float)(amountX-1) );
        ZStep = NoiseScale * (sizeZ / (float)(amountZ-1) );

        float amplitude = 1.0f;
        Normalization   = 0.0f;
        for (int o = 0; o < Octaves; o++) {
            Normalization += amplitude;
            amplitude     *= AmpMult;
        }

        // non parallel way: 508.2472 - 535.7532
        // parallel way: 164.9999 (first) 34.0475 - 42.4827 (subsequent)
#if XBSINGLETHREADED
        float amp     = 1.0f;
        float freq    = 1.0f;
        float total   = 0.0f;
        float value   = 0.0f;
        for (int i = 0; i < amountX; i++) {
            for (int j = 0; j < amountZ; j++) {
                amp   = 1.0f;
                freq  = 1.0f;
                total = 0.0f;
                value = 0.0f;

                for (int o = 0; o < Octaves; o++) {
                    float x = i*XStep;
                    float z = j*ZStep;
                    value  = XB.Random.ValueNoise2D(x*freq, z*freq);
                    value *= 0.5f;
                    value += 0.5f;
                    total += value*amp;
                    amp   *= AmpMult;
                    freq  *= Lacunarity;
                }

                total /= Normalization;
                total  = (float)System.Math.Pow(total, Exponentation) * Height;
                XB.WorldData.TerrainHeightsMod[i, j] = total;
            }
        }
#else
        System.Threading.Tasks.Parallel.For(0, amountX, ParallelFBMLoop);
#endif

#if XBDEBUG
        debug.End();
#endif 
    }

    //NOTE[ALEX]: the parallel loop body reads from shared, constant variables and writes to a 
    //            shared array but in separate rows (different i each run)
    private static void ParallelFBMLoop(int i) {
        float amp   = 1.0f;
        float freq  = 1.0f;
        float total = 0.0f;
        float value = 0.0f;

        for (int j = 0; j < XB.WorldData.WorldVerts.Y; j++) {
            amp   = 1.0f;
            freq  = 1.0f;
            total = 0.0f;
            value = 0.0f;
            for (int o = 0; o < Octaves; o++) {
                float x = i*XStep;
                float z = j*ZStep;
                value  = XB.Random.ValueNoise2D(x*freq, z*freq);
                value *= 0.5f;
                value += 0.5f;
                total += value*amp;
                amp   *= AmpMult;
                freq  *= Lacunarity;
            }

            total /= Normalization;
            total  = (float)System.Math.Pow(total, Exponentation) * Height;
            XB.WorldData.TerrainHeightsMod[i, j] = total;
        }
    }

    public static void Cone(ref float[,] tHeights, int amountX, int amountZ,
                            float worldSizeX, float worldSizeZ, float centerX, float centerZ,
                            float radius, float angle, float height, XB.Direction dir        ) {
        float xStep = worldSizeX/(float)(amountX-1);
        float zStep = worldSizeZ/(float)(amountZ-1);
        var   v2    = new Godot.Vector2(0.0f, 0.0f);

        // slope for 89.0f is 57.29004m/1m
        float ramp = 0;
        if (angle <= 89.0f*XB.Constants.Deg2Rad) {
            float cos = (float)System.Math.Cos(angle);
            float sin = (float)System.Math.Sin(angle);
            ramp = sin/cos;
        }

        float direction = 1.0f;
        if (dir == XB.Direction.Down) { direction = -1.0f; }

        for (int i = 0; i < amountX; i++) {
            v2.X = i*xStep + centerX;
            for (int j = 0; j < amountZ; j++) {
                v2.Y = j*zStep + centerZ;
                float dist = v2.Length();
                if (dist <= radius) {
                    tHeights[i, j] = height;
                } else if (angle <= 89.0f*XB.Constants.Deg2Rad) {
                    tHeights[i, j] = height - direction*ramp*(dist-radius);
                } else {
                    tHeights[i, j] = direction*float.MinValue;
                }
            }
        }

        Godot.GD.Print("Cone at: " + centerX + "m " + centerZ + "m, of "
                       + worldSizeX + "m " + worldSizeZ + "m, tip height: " + height + "m, radius: "
                       + radius + "m, angle: " + angle*XB.Constants.Rad2Deg + "deg, direction: "
                       + direction+ ", xStep: " + xStep + ", zStep: " + zStep);
    }

    public static void Flat(ref float[,] tHeights, int amountX, int amountZ, float height) {
#if XBDEBUG
        var debug = new XB.DebugTimedBlock(XB.D.TerrainFlat);
#endif

        XB.WorldData.LowestPoint  = height;
        XB.WorldData.HighestPoint = height;
        for (int i = 0; i < amountX; i++) {
            for (int j = 0; j < amountZ; j++) {
                tHeights[i, j] = height;
            }
        }

#if XBDEBUG
        debug.End();
#endif 
    }

    public static void GradientX(ref float[,] tHeights, int amountX, int amountZ,
                                 float low, float high                           ) {
#if XBDEBUG
        var debug = new XB.DebugTimedBlock(XB.D.TerrainGradientX);
#endif

        XB.WorldData.LowestPoint  = low;
        XB.WorldData.HighestPoint = high;

        float diff = high - low;
        for (int i = 0; i < amountX; i++) {
            for (int j = 0; j < amountZ; j++) {
                tHeights[i, j] = low + ( (float)i/(float)(amountX-1) )*diff;
            }
        }

#if XBDEBUG
        debug.End();
#endif 
    }

    public static void GradientY(ref float[,] tHeights, int amountX, int amountZ, 
                                float low, float high                            ) {
#if XBDEBUG
        var debug = new XB.DebugTimedBlock(XB.D.TerrainGradientY);
#endif

        XB.WorldData.LowestPoint  = low;
        XB.WorldData.HighestPoint = high;

        float diff = high - low;
        for (int i = 0; i < amountZ; i++) {
            for (int j = 0; j < amountX; j++) {
                tHeights[j, i] = low + ( (float)i/(float)(amountZ-1) )*diff;
            }
        }

#if XBDEBUG
        debug.End();
#endif 
    }

    public static void HeightMin(ref float[,] tHeights, ref float[,] tHeightsM,
                                 int amountX, int amountZ                      ) {
#if XBDEBUG
        var debug = new XB.DebugTimedBlock(XB.D.TerrainHeightMin);
#endif

        ResetLowestHighest();
        for (int i = 0; i < amountX; i++) {
            for (int j = 0; j < amountZ; j++) {
                tHeights[i, j] = XB.Utils.MinF(tHeights[i, j], tHeightsM[i, j]);
                UpdateLowestHighest(tHeights[i, j], tHeights[i, j]);
            }
        }

#if XBDEBUG
        debug.End();
#endif 
    }

    public static void HeightMax(ref float[,] tHeights, ref float[,] tHeightsM,
                                 int amountX, int amountZ                      ) {
#if XBDEBUG
        var debug = new XB.DebugTimedBlock(XB.D.TerrainHeightMax);
#endif

        ResetLowestHighest();
        for (int i = 0; i < amountX; i++) {
            for (int j = 0; j < amountZ; j++) {
                tHeights[i, j] = XB.Utils.MaxF(tHeights[i, j], tHeightsM[i, j]);
                UpdateLowestHighest(tHeights[i, j], tHeights[i, j]);
            }
        }

#if XBDEBUG
        debug.End();
#endif 
    }

    public static void HeightReplace(ref float[,] tHeights, ref float[,] tHeightsM,
                                     int amountX, int amountZ                      ) {
#if XBDEBUG
        var debug = new XB.DebugTimedBlock(XB.D.TerrainHeightReplace);
#endif

        ResetLowestHighest();
        for (int i = 0; i < amountX; i++) {
            for (int j = 0; j < amountZ; j++) {
                tHeights[i, j] = tHeightsM[i, j];
                UpdateLowestHighest(tHeights[i, j], tHeights[i, j]);
            }
        }

#if XBDEBUG
        debug.End();
#endif 
    }

    public static void CalculateNormals(ref Godot.Vector3[] norms,
                                        ref Godot.Vector3[] verts, ref int[] tris) {
#if XBDEBUG
        var debug = new XB.DebugTimedBlock(XB.D.TerrainCalculateNormals);
#endif

        // assign normals to vertices of each triangle
        for (int i = 0; i < tris.Length/3; i++) {
            int vIDA  = tris[i*3 +0];
            int vIDB  = tris[i*3 +1];
            int vIDC  = tris[i*3 +2];
            var dirAB = verts[vIDB]-verts[vIDA];
            var dirAC = verts[vIDC]-verts[vIDA];

            var nrm = dirAC.Cross(dirAB);
                nrm = nrm.Normalized();

            // add the normal to each vertex, adding them up to consider all adjacent triangles
            // this leaves the normals not normalized
            norms[vIDA] += nrm;
            norms[vIDB] += nrm;
            norms[vIDC] += nrm;
        }

        for (int i = 0; i < norms.Length; i++) {
            norms[i] = norms[i].Normalized();
        }

#if XBDEBUG
        debug.End();
#endif 
    }

    // expects img to be of type L8
    public static void UpdateHeightMap(ref float[,] tHeights, float lowest, float highest,
                                       ref Godot.Image img                                ) {
#if XBDEBUG
        var debug = new XB.DebugTimedBlock(XB.D.TerrainUpdateHeightMap);
#endif

        var height = new Godot.Color(0.0f, 0.0f, 1.0f, 0.0f); // only the blue value will be used
        for (int i = 0; i < img.GetWidth(); i++) {
            for (int j = 0; j < img.GetHeight(); j++) {
                height.B = (tHeights[i, j] - lowest) / (highest - lowest);
                img.SetPixel(i, j, height);
            }
        }

#if XBDEBUG
        debug.End();
#endif 
    }

    // takes world coordinates (negative coordinates) and world size along with heightmap
    // returns the sampled value from the heightmap (between 0 and 1)
    //
    // height map's pixels do not align with world dimension but are offset by 1/2
    // this is done so that the world dimension and the texture resolution can be the same
    // see diagram for how points are sampled:
    //
    // bottom edge of 4x4 height texture with bottom row of pixels ABCD
    // |-|-|-|-|
    // |A|B|C|D|
    // ---------
    //
    // bottom edge of world with 1m*1m tiles WXYZ, vertices are in edges of tiles,
    // so the bottom edge of the world mesh will have 5 vertices
    // |W|X|Y|Z|
    // ---------
    //
    // vertices: vW0 and vW1 are at bottom edge of tile W, vW1 = vX0, vX1 = vY0, ...
    // vW0 samples the texture at pixel A (x = 0), 
    // vW1 samples the texture at pixels A (x = 0) and B (x = 1) and blends linearly
    // the same vertically
    //
    public static float HeightMapSample(float sampleX, float sampleZ,
                                        float worldXSize, float worldZSize, ref Godot.Image img) {
#if XBDEBUG
        var debug = new XB.DebugTimedBlock(XB.D.TerrainHeightMapSample);
#endif

        float sX  = (1.0f/worldXSize) * (img.GetWidth() -1);
        float sZ  = (1.0f/worldZSize) * (img.GetHeight()-1);
        float x   = -sampleX * sX;
        float z   = -sampleZ * sZ;
        int   xI0 = (int)x;
        int   zI0 = (int)z;
        int   xI1 = XB.Utils.MinI(xI0+1, img.GetWidth() -1);
        int   zI1 = XB.Utils.MinI(zI0+1, img.GetHeight()-1);

        // 0    0|0 in top left, in world: 0|0 in "top left", but X and Z axis go to negative
        //  AB  samples surrounding the sampled coordinates
        //  CD  sample coordinates are inside these points (including)

        float sampleA = img.GetPixel(xI0, zI0).B;
        float sampleB = img.GetPixel(xI1, zI0).B;
        float sampleC = img.GetPixel(xI0, zI1).B;
        float sampleD = img.GetPixel(xI1, zI1).B;
        float upper   = XB.Utils.LerpF(sampleA, sampleB, x-xI0);
        float lower   = XB.Utils.LerpF(sampleC, sampleD, x-xI0);
        float result  = XB.Utils.LerpF(upper,   lower,   z-zI0);

        // Godot.GD.Print(" world: " +sampleX +"m/" +worldXSize +"m, " +sampleZ +"m/" +worldZSize +"m");
        // Godot.GD.Print(x + " " + z + " of " + img.GetSize() + " xI0: " + xI0 + " xI1: " + xI1
        //                + " zI0: " + zI0 + " zI1: " + zI1
        //                + " samples A: " + sampleA + " B: " + sampleB + " C: " + sampleC
        //                + " D: " + sampleD + " x-xI0: " + (x-xI0) + " z-zI0: " + (z-zI0)
        //                + " upper: " + upper + " lower: " + lower + " result: "               );
        // Godot.GD.Print(result);

#if XBDEBUG
        debug.End();
#endif 

        return result;
    }

    private static void ResetLowestHighest() {
#if XBDEBUG
        var debug = new XB.DebugTimedBlock(XB.D.TerrainResetLowestHighest);
#endif

        XB.WorldData.LowestPoint  = float.MaxValue;
        XB.WorldData.HighestPoint = float.MinValue;

        // Godot.GD.Print("ResetLowestHighest, Low to: " + XB.WorldData.LowestPoint
        //                + "m, High to: " + XB.WorldData.HighestPoint + "m"       );

#if XBDEBUG
        debug.End();
#endif 
    }

    public static void UpdateLowestHighest(float low, float high) {
#if XBDEBUG
        var debug = new XB.DebugTimedBlock(XB.D.TerrainUpdateLowestHighest);
#endif

        XB.WorldData.LowestPoint  = XB.Utils.MinF(XB.WorldData.LowestPoint,  low);
        XB.WorldData.HighestPoint = XB.Utils.MaxF(XB.WorldData.HighestPoint, high);

#if XBDEBUG
        debug.End();
#endif 
    }
}
} // namespace close
