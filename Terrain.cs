// #define XBSINGLETHREADED
#define XBDEBUG
// #define XBPROFILE // for timing FBM generation
namespace XB { // namespace open
public enum Direction {
    Up,
    Down,
}

// Terrain deals with heightmap creation and modification
// the functions use an array of floats (called tHeights in this class),
// each float represents the height of the vertex offset from the "top left" by i/j * stepsize (1/res)
// amountX and amountZ are the dimensions of the array
// the terrain of the world has n vertices on a side, whereas the heightmap has n-1 pixels on a side
// this is done to keep the heightmap size and worldsize a power of two,
// as a result, all pixels of the heightmap are a result of linear interpolation of the heights array
// these functions expect the number of world vertices as the inputs for amountX and amountZ
public class Terrain {
    // FBM (fractal brownian motion) noise is an addition of multiple layers of perlin noise,
    // each with increasing frequency (detail) but less amplitude (strength)
    public static void FBM(float[,] tHeights, int amountX, int amountZ, float sizeX, float sizeZ,
                           float scale, float offsetX, float offsetZ,
                           int octaves, float persistence, float lacunarity, float exponentation ) {
#if XBDEBUG
        var debug = new XB.DebugTimedBlock(XB.D.TerrainFBM);
#endif
#if XBPROFILE
        var stopWatch = new System.Diagnostics.Stopwatch();
        stopWatch.Start();
#endif

        // Godot.GD.Print("FBM: " + amountX + ", " + amountZ + ", sizeX: " + sizeX
        //                + ", sizeZ: " + sizeZ + ", scale: " + scale
        //                + ", off: " + offsetX + ", " + offsetZ + ", octaves: " + octaves
        //                + ", persistence: " + lacunarity + ", exponentation: " + exponentation);

        float ampMult = System.MathF.Pow(2.0f, -persistence);

        float xStep = scale * (sizeX / (float)(amountX-1) );
        float zStep = scale * (sizeZ / (float)(amountZ-1) );

        float amplitude     = 1.0f;
        float normalization = 0.0f;
        for (int o = 0; o < octaves; o++) {
            normalization += amplitude;
            amplitude     *= ampMult;
        }

        //NOTE[ALEX]: profiled times during test runs (AMD Ryzen 5950X)
        //            FBM: 2049, 2049, sizeX: 256, sizeZ: 256, scale: 0.0174,
        //            off: 0.46, 0.33, octaves: 6, persistence: 2.15, exponentation: 5.21
        //            random seed was changed for re-generation via menu
        //
        // successive runs within variance (+- ~100ms of these values)
        // Single Threaded FBM Creation: 6428.535900ms // startup generation
        // Single Threaded FBM Creation: 6141.665300ms // generation via menu
        // Single Threaded FBM Creation: 6148.978400ms // generation via menu
        // Single Threaded FBM Creation: 6212.752200ms // generation via menu
        // Single Threaded FBM Creation: 6133.478700ms // generation via menu
        //
        // successive runs within variance (+- ~10ms of these values)
        // Multi Threaded FBM Creation: 560.302700ms // startup generation
        // Multi Threaded FBM Creation: 368.527900ms // generation via menu
        // Multi Threaded FBM Creation: 364.526400ms // generation via menu
        // Multi Threaded FBM Creation: 362.160600ms // generation via menu
        // Multi Threaded FBM Creation: 347.743700ms // generation via menu
        //
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

                for (int o = 0; o < octaves; o++) {
                    float x = i*xStep + offsetX;
                    float z = j*zStep + offsetZ;
                    value  = XB.Random.ValueNoise2D(x*freq, z*freq);
                    value *= 0.5f;
                    value += 0.5f;
                    total += value*amp;
                    amp   *= ampMult;
                    freq  *= lacunarity;
                }

                total /= normalization;
                total  = System.MathF.Pow(total, exponentation);
                tHeights[i, j] = total;
            }
        }

#if XBPROFILE
        Godot.GD.Print("Single Threaded FBM Creation: " 
                       + stopWatch.Elapsed.TotalMilliseconds.ToString("F6") + "ms");
#endif
#else
        System.Threading.Tasks.Parallel.For(0, amountX, i => {
            float amp   = 1.0f;
            float freq  = 1.0f;
            float total = 0.0f;
            float value = 0.0f;

            for (int j = 0; j < XB.WData.WorldVerts.Y; j++) {
                amp   = 1.0f;
                freq  = 1.0f;
                total = 0.0f;
                value = 0.0f;
                for (int o = 0; o < octaves; o++) {
                    float x = i*xStep + offsetX;
                    float z = j*zStep + offsetZ;
                    value  = XB.Random.ValueNoise2D(x*freq, z*freq);
                    value *= 0.5f;
                    value += 0.5f;
                    total += value*amp;
                    amp   *= ampMult;
                    freq  *= lacunarity;
                }

                total /= normalization;
                total  = System.MathF.Pow(total, exponentation);
                tHeights[i, j] = total;
            }
        });

#if XBPROFILE
        Godot.GD.Print("Multi Threaded FBM Creation: " 
                       + stopWatch.Elapsed.TotalMilliseconds.ToString("F6") + "ms");
#endif
#endif

#if XBDEBUG
        debug.End();
#endif 
    }

    // scales the heights array to extend exactly from 0.0 to the parameter height
    // the lowest and highest point are updated and require ref
    public static void HeightScale(float[,] tHeights, int amountX, int amountZ,
                                   float height, ref float lowestPoint, ref float highestPoint) {
        float diff = highestPoint - lowestPoint;

        for (int i = 0; i < amountX; i++) {
            for (int j = 0; j < amountZ; j++) {
                tHeights[i, j] -= lowestPoint;
                tHeights[i, j] /= diff;
                tHeights[i, j] *= height;
            }
        }

        lowestPoint  = 0.0f;
        highestPoint = height;
    }

    // fills a height array with values that represent a cone using signed distance field calculation
    // angle should be constrained to be within 1 and 89 degrees (including), to prevent weirdness
    public static void Cone(float[,] tHeights, int amountX, int amountZ,
                            float worldSizeX, float worldSizeZ, float centerX, float centerZ,
                            float radius, float angle, float height, XB.Direction dir        ) {
#if XBDEBUG
        var debug = new XB.DebugTimedBlock(XB.D.TerrainCone);
#endif

        float xStep = worldSizeX/(float)(amountX-1);
        float zStep = worldSizeZ/(float)(amountZ-1);
        var   v2    = new Godot.Vector2(0.0f, 0.0f);

        float cos  = System.MathF.Cos(angle);
        float sin  = System.MathF.Sin(angle);
        float ramp = sin/cos;

        float direction = 1.0f;
        if (dir == XB.Direction.Down) { direction = -1.0f; }

        for (int i = 0; i < amountX; i++) {
            v2.X = i*xStep + centerX;
            for (int j = 0; j < amountZ; j++) {
                v2.Y = j*zStep + centerZ;
                float dist = v2.Length();
                if (dist <= radius) {
                    tHeights[i, j] = height;
                } else {
                    tHeights[i, j] = height - direction*ramp*(dist-radius);
                }
            }
        }

        // Godot.GD.Print("Cone at: " + centerX + "m " + centerZ + "m, of "
        //                + worldSizeX + "m " + worldSizeZ + "m, tip height: " + height + "m, radius: "
        //                + radius + "m, angle: " + angle*XB.Constants.Rad2Deg + "deg, direction: "
        //                + direction + ", xStep: " + xStep + ", zStep: " + zStep);

#if XBDEBUG
        debug.End();
#endif 
    }

    // signed distance field based on: https://iquilezles.org/articles/distfunctions2d/
    //
    // fills a height array with values that represent an uneven capsule between two points
    // using signed distance field calculation
    // used to represent the geometric shape that appears when two spheres are linked
    // flip signs for point differences because world is in negative coordinates
    public static void UnevenCapsule(float[,] tHeights, int amountX, int amountZ,
                                     float worldSizeX, float worldSizeZ, 
                                     float center1X, float center1Z, float radius1,
                                     float angle1, float height1,
                                     float center2X, float center2Z, float radius2,
                                     float angle2, float height2,
                                     XB.Direction dir                              ) {
#if XBDEBUG
        var debug = new XB.DebugTimedBlock(XB.D.TerrainUnevenCapsule);
#endif

        float xStep = worldSizeX/(float)(amountX-1);
        float zStep = worldSizeZ/(float)(amountZ-1);
        var   p  = new Godot.Vector2(0.0f, 0.0f);
        var   pb = new Godot.Vector2(center1X - center2X, center1Z - center2Z);
        float h  = pb.Dot(pb);
        var   q  = new Godot.Vector2(0.0f, 0.0f);
        float b  = radius1-radius2;
        var   c  = new Godot.Vector2(System.MathF.Sqrt(h - b*b), b);

        float cos   = System.MathF.Cos(angle1);
        float sin   = System.MathF.Sin(angle1);
        float ramp1 = sin/cos;
              cos   = System.MathF.Cos(angle2);
              sin   = System.MathF.Sin(angle2);
        float ramp2 = sin/cos;

        float direction = 1.0f;
        if (dir == XB.Direction.Down) { direction = -1.0f; }

        for (int i = 0; i < amountX; i++) {
            p.X = i*xStep + center1X;
            for (int j = 0; j < amountZ; j++) {
                p.Y = j*zStep  + center1Z;
                q.X = pb.Y;
                q.Y = -pb.X;
                q.X = p.Dot(q)  / h;
                q.Y = p.Dot(pb) / h;

                q.X = XB.Utils.AbsF(q.X);

                float k = c.Cross(q);
                float m = c.Dot(q);
                float n = q.Dot(q);

                float dist   = 0.0f;
                float height = 0.0f;
                if        (k < 0.0f) { // point 1 side
                    dist = System.MathF.Sqrt(h * n) - radius1; 
                    if (dist < 0.0f) { height = height1;                        }
                    else             { height = height1 - direction*ramp1*dist; }
                } else if (k > c.X ) { // point 2 side
                    dist = System.MathF.Sqrt(h * (n + 1.0f - 2.0f*q.Y)) - radius2; 
                    if (dist < 0.0f) { height = height2;                        }
                    else             { height = height2 - direction*ramp2*dist; }
                }else { // in between points, t as ratio between distances
                    dist = m - radius1;
                    float t = k/c.X;
                    if (dist < 0.0f) {
                        height = XB.Utils.LerpF(height1, height2, t);
                    } else {
                        float heightLerp = XB.Utils.LerpF(height1, height2, t);
                        float rampLerp   = XB.Utils.LerpF(ramp1, ramp2, t);
                        height = heightLerp - direction*rampLerp*dist;
                    }
                }

                tHeights[i, j] = height;
            }
        }

        // Godot.GD.Print("Uneven Capsule with world size: " + worldSizeX + "m, " + worldSizeZ 
        //                + "m, direction: " + direction + ", xStep: " + xStep + ", zStep: " + zStep
        //                + "\nP1: " + center1X + "m " + center1Z
        //                + "m, point height: " + height1 + "m, radius: " + radius1 +  "m, angle: "
        //                + angle1*XB.Constants.Rad2Deg + "deg."
        //                + "\nP2: " + center2X + "m " + center2Z
        //                + "m, point height: " + height2 + "m, radius: " + radius2 +  "m, angle: "
        //                + angle2*XB.Constants.Rad2Deg + "deg.")                                   ;

#if XBDEBUG
        debug.End();
#endif 
    }

    // flattens the entire heights array to the value of height parameter
    public static void Flat(float[,] tHeights, int amountX, int amountZ, float height) {
#if XBDEBUG
        var debug = new XB.DebugTimedBlock(XB.D.TerrainFlat);
#endif

        XB.WData.LowestPoint  = height;
        XB.WData.HighestPoint = height;
        for (int i = 0; i < amountX; i++) {
            for (int j = 0; j < amountZ; j++) {
                tHeights[i, j] = height;
            }
        }

#if XBDEBUG
        debug.End();
#endif 
    }

    // creates a linear gradient in x direction from value of left to value of right
    // starting at the left
    public static void GradientX(float[,] tHeights, int amountX, int amountZ,
                                 float left, float right                     ) {
#if XBDEBUG
        var debug = new XB.DebugTimedBlock(XB.D.TerrainGradientX);
#endif

        XB.WData.LowestPoint  = XB.Utils.MinF(left, right);
        XB.WData.HighestPoint = XB.Utils.MaxF(left, right);

        float step = (right - left) / (float)(amountX-1);
        for (int i = 0; i < amountX; i++) {
            for (int j = 0; j < amountZ; j++) {
                tHeights[i, j] = left + (float)i * step;
            }
        }

#if XBDEBUG
        debug.End();
#endif 
    }

    // creates a linear gradient in Z direction from value of top to value of bottom
    // starting at the top
    public static void GradientY(float[,] tHeights, int amountX, int amountZ, 
                                float top, float bottom                          ) {
#if XBDEBUG
        var debug = new XB.DebugTimedBlock(XB.D.TerrainGradientY);
#endif

        XB.WData.LowestPoint  = XB.Utils.MinF(top, bottom);
        XB.WData.HighestPoint = XB.Utils.MaxF(top, bottom);

        float step = (bottom - top) / (float)(amountZ-1);
        for (int i = 0; i < amountZ; i++) {
            for (int j = 0; j < amountX; j++) {
                tHeights[j, i] = top + (float)i * step;
            }
        }

#if XBDEBUG
        debug.End();
#endif 
    }

    // compares two height arrays and overwrites values in the first with the lower of the two
    // updates static lowest and highest values
    public static void HeightMin(float[,] tHeights, float[,] tHeightsM, int amountX, int amountZ,
                                 ref float lowest, ref float highest                             ) {
#if XBDEBUG
        var debug = new XB.DebugTimedBlock(XB.D.TerrainHeightMin);
#endif

        ResetLowestHighest(ref lowest, ref highest);
        for (int i = 0; i < amountX; i++) {
            for (int j = 0; j < amountZ; j++) {
                tHeights[i, j] = XB.Utils.MinF(tHeights[i, j], tHeightsM[i, j]);
                UpdateLowestHighest(ref lowest, ref highest, tHeights[i, j]);
            }
        }

#if XBDEBUG
        debug.End();
#endif 
    }

    // compares two height arrays and overwrites values in the first with the higher of the two
    // updates static lowest and highest values
    public static void HeightMax(float[,] tHeights, float[,] tHeightsM, int amountX, int amountZ,
                                 ref float lowest, ref float highest                             ) {
#if XBDEBUG
        var debug = new XB.DebugTimedBlock(XB.D.TerrainHeightMax);
#endif

        ResetLowestHighest(ref lowest, ref highest);
        for (int i = 0; i < amountX; i++) {
            for (int j = 0; j < amountZ; j++) {
                tHeights[i, j] = XB.Utils.MaxF(tHeights[i, j], tHeightsM[i, j]);
                UpdateLowestHighest(ref lowest, ref highest, tHeights[i, j]);
            }
        }

#if XBDEBUG
        debug.End();
#endif 
    }

    // replaces all values in the first height array with those of the first
    // updates static lowest and highest values
    public static void HeightReplace(float[,] tHeights, float[,] tHeightsM, int amountX, int amountZ,
                                     ref float lowest, ref float highest                             ) {
#if XBDEBUG
        var debug = new XB.DebugTimedBlock(XB.D.TerrainHeightReplace);
#endif

        ResetLowestHighest(ref lowest, ref highest);
        for (int i = 0; i < amountX; i++) {
            for (int j = 0; j < amountZ; j++) {
                tHeights[i, j] = tHeightsM[i, j];
                UpdateLowestHighest(ref lowest, ref highest, tHeights[i, j]);
            }
        }

#if XBDEBUG
        debug.End();
#endif 
    }

    // calculates normals for given arrays of vertex position and triangle indices
    // and writes them into normals array
    public static void CalculateNormals(Godot.Vector3[] norms, Godot.Vector3[] verts, int[] tris) {
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

    // fills heightmap texture by linearly interpolating between values of height array
    // resulting texture goes from 0.0 for the lowest value to 1.0 for the highest value
    // (flat heights will result in 0.5 grey texture)
    // expects img to be of type L8
    // since there is one vertex more than pixels along each edge, the heightmap values
    // are interpolations of the neighboring vertices (opposite to HeightMapSample)
    //
    // heights of vertices A, B, C and D contribute equally to the value H of heightMap
    // A-B
    // |H|
    // C-D
    //
    // example 3x3 vertex terrain mesh with 2x2 pixel heightmap:
    // *-*-*   X X
    // | | |
    // *-*-*   X X
    // | | |
    // *-*-*
    //
    // width of heightMap is one smaller than count of first dimension of tHeights,
    // height of heightMap is one smaller than count of second dimension of tHeights
    //NOTE[ALEX]: there is no check for this condition as errors will show up if there
    //            is a mismatch of amounts
    public static void UpdateHeightMap(float[,] tHeights, float lowest, float highest,
                                       Godot.Image heightMap                          ) {
#if XBDEBUG
        var debug = new XB.DebugTimedBlock(XB.D.TerrainUpdateHeightMap);
#endif

        float scale = highest - lowest;
        var height = new Godot.Color(0.0f, 0.0f, 0.0f, 1.0f);

        for (int i = 0; i < heightMap.GetWidth(); i++) {
            for (int j = 0; j < heightMap.GetHeight(); j++) {
                float sampleA = tHeights[i+0, j+0];
                float sampleB = tHeights[i+1, j+0];
                float sampleC = tHeights[i+0, j+1];
                float sampleD = tHeights[i+1, j+1];
                float upper   = XB.Utils.LerpF(sampleA, sampleB, 0.5f);
                float lower   = XB.Utils.LerpF(sampleC, sampleD, 0.5f);
                float value   = XB.Utils.LerpF(upper,   lower,   0.5f);
                height.R = (value - lowest) / scale;
                heightMap.SetPixel(i, j, height);
            }
        }

#if XBDEBUG
        debug.End();
#endif 
    }

    // takes world coordinates (negative coordinates) and world size along with heightmap
    // returns the sampled value from the heightmap (between 0.0 and 1.0)
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
    // bottom edge of world with 1m*1m tiles WXYZ, vertices are in corners of tiles,
    // so the bottom edge of the world mesh will have 5 vertices and 4 edges
    // |W|X|Y|Z|
    // ---------
    //
    // vertices: vW0 and vW1 are at bottom edge of tile W, vW1 = vX0, vX1 = vY0, ...
    // vW0 samples the texture at pixel A (x = 0), 
    // vW1 samples the texture at pixels A (x = 0) and B (x = 1) and blends linearly
    // the same vertically
    //
    //NOTE[ALEX]: no mipmaps
    public static float HeightMapSample(float sampleX, float sampleZ,
                                        float worldXSize, float worldZSize, Godot.Image heightMap) {
#if XBDEBUG
        var debug = new XB.DebugTimedBlock(XB.D.TerrainHeightMapSample);
#endif

        float sX  = (1.0f/worldXSize) * (heightMap.GetWidth() -1);
        float sZ  = (1.0f/worldZSize) * (heightMap.GetHeight()-1);
        float x   = -sampleX * sX;
        float z   = -sampleZ * sZ;
        int   xI0 = (int)x;
        int   zI0 = (int)z;
        int   xI1 = XB.Utils.MinI(xI0+1, heightMap.GetWidth() -1);
        int   zI1 = XB.Utils.MinI(zI0+1, heightMap.GetHeight()-1);

        // 0    0|0 in top left, in world: 0|0 in "top left", but X and Z axis go to negative
        //  AB  samples surrounding the sampled coordinates
        //  CD  sample coordinates are inside these points (including)

        float sampleA = heightMap.GetPixel(xI0, zI0).B;
        float sampleB = heightMap.GetPixel(xI1, zI0).B;
        float sampleC = heightMap.GetPixel(xI0, zI1).B;
        float sampleD = heightMap.GetPixel(xI1, zI1).B;
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

    // sets lowest and highest static variables to the limits of float to prepare them for 
    // new comparisons within the height array
    private static void ResetLowestHighest(ref float lowest, ref float highest) {
#if XBDEBUG
        var debug = new XB.DebugTimedBlock(XB.D.TerrainResetLowestHighest);
#endif

        lowest  = float.MaxValue;
        highest = float.MinValue;

        // Godot.GD.Print("ResetLowestHighest, Low to: " + XB.WorldData.LowestPoint
        //                + "m, High to: " + XB.WorldData.HighestPoint + "m"       );

#if XBDEBUG
        debug.End();
#endif 
    }

    // compares existing lowest and highest static variables with a value,
    // updates them is required
    public static void UpdateLowestHighest(ref float lowest, ref float highest, float value) {
#if XBDEBUG
        var debug = new XB.DebugTimedBlock(XB.D.TerrainUpdateLowestHighest);
#endif

        //NOTE[ALEX]: does not use min max functions for easier debugging/logging
        if      (value < lowest)  { lowest  = value; }
        else if (value > highest) { highest = value; }

#if XBDEBUG
        debug.End();
#endif 
    }

    // iterates through the height array and updates lowest and highest static variables
    public static void FindLowestHighest(float[,] heights, int amountX, int amountZ,
                                         ref float lowest, ref float highest        ) {
#if XBDEBUG
        var debug = new XB.DebugTimedBlock(XB.D.TerrainFindLowestHighest);
#endif

        lowest  = heights[0, 0];
        highest = heights[0, 0];
        for (int i = 0; i < amountX; i++) {
            for (int j = 0; j < amountZ; j++) {
                lowest  = XB.Utils.MinF(lowest, heights[i, j]);
                highest = XB.Utils.MaxF(highest, heights[i, j]);
            }
        }

#if XBDEBUG
        debug.End();
#endif 
    }

    // calculates a pointiness texture for the terrain by comparing height to immediate
    // neighbor heights in 8 directions
    // pointiness is used in the terrain shader for additional modulation (purely visual)
    // same method and requirements as UpdateHeightMap
    //NOTE[ALEX]: no mipmaps
    public static void BakePointiness(float[,] heights, int amountX, int amountZ,
                                      Godot.Image pointinessMap                  ) {
        pointinessMap.Fill(XB.Col.Black);
        
        float pointiness = 0.0f;
        float thisPx     = 0.0f;
        float compPx     = 0.0f; // average of surrounding values
        float compPxUL   = 0.0f; // up left
        float compPxU    = 0.0f; // up
        float compPxUR   = 0.0f; // up right
        float compPxR    = 0.0f; // right
        float compPxDR   = 0.0f; // down right
        float compPxD    = 0.0f; // down
        float compPxDL   = 0.0f; // down left
        float compPxL    = 0.0f; // left
        int   up         = 0;
        int   right      = 0;
        int   down       = 0;
        int   left       = 0;
        var   pointyCol  = new Godot.Color(0.0f, 0.0f, 0.0f, 1.0f);

        for (int i = 0; i < pointinessMap.GetWidth(); i++) {
            for (int j = 0; j < pointinessMap.GetHeight(); j++) {
                //NOTE[ALEX]: at the edges the pointiness is not technically correct
                //            as some pixels are counted twice
                //            this shortcoming is acceptable since it keeps this code much simpler
                up    = XB.Utils.MaxI(j-1, 0        );
                right = XB.Utils.MinI(i+1, amountX-1);
                down  = XB.Utils.MinI(j+1, amountZ-1);
                left  = XB.Utils.MaxI(i-1, 0        );
                thisPx   = heights[i,     j   ];
                compPxUL = heights[left,  up  ];
                compPxU  = heights[i,     up  ];
                compPxUR = heights[right, up  ];
                compPxR  = heights[right, j   ];
                compPxDR = heights[right, down];
                compPxD  = heights[i,     down];
                compPxDL = heights[left,  down];
                compPxL  = heights[left,  j   ];

                compPx = (  compPxUL + compPxU + compPxUR + compPxR
                          + compPxDR + compPxD + compPxDL + compPxL) / 8.0f;
                pointiness = 0.5f + (thisPx - compPx);

                pointyCol.R = XB.Utils.ClampF(pointiness, 0.0f, 1.0f);
                pointinessMap.SetPixel(i, j, pointyCol);
            }
        }
    }
}
} // namespace close
