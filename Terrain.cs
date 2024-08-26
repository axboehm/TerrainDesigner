// #define XBSINGLETHREADED
#define XBDEBUG
namespace XB { // namespace open
public enum Direction {
    Up,
    Down,
}

// Terrain deals with heightmap creation and modification
public class Terrain {
    // FBM (fractal brownian motion) noise is an addition of multiple layers of perlin noise,
    // each with increasing frequency (detail) but less amplitude (strength)
    //NOTE[ALEX]: can not use ref with parallel for loop, so the height array is hardcoded
    public static void FBM(int amountX, int amountZ, float sizeX, float sizeZ,
                           float scale, float offsetX, float offsetZ,
                           int octaves, float persistence, float lacunarity, float exponentation) {
#if XBDEBUG
        var debug = new XB.DebugTimedBlock(XB.D.TerrainFBM);
#endif

        // Godot.GD.Print("FBM: " + amountX + ", " + amountZ + ", sizeX: " + sizeX
        //                + ", sizeZ: " + sizeZ + ", height: " + height + ", scale: " + scale
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

        //TODO[ALEX]: retime this
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
                XB.WorldData.TerrainHeightsMod[i, j] = total;
            }
        }
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
                XB.WData.TerrainHeightsMod[i, j] = total;
            }
        });
#endif

#if XBDEBUG
        debug.End();
#endif 
    }

    public static void HeightScale(ref float[,] tHeights, int amountX, int amountZ,
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

    // angle should be constrained to be within 1 and 89 degrees (including), to prevent weirdness
    public static void Cone(ref float[,] tHeights, int amountX, int amountZ,
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
    // flip signs for point differences because world is in negative coordinates
    public static void UnevenCapsule(ref float[,] tHeights, int amountX, int amountZ,
                                     float worldSizeX, float worldSizeZ, 
                                     float center1X, float center1Z, float radius1,
                                     float angle1, float height1,
                                     float center2X, float center2Z, float radius2,
                                     float angle2, float height2,
                                     XB.Direction dir                                ) {
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

    public static void Flat(ref float[,] tHeights, int amountX, int amountZ, float height) {
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

    public static void GradientX(ref float[,] tHeights, int amountX, int amountZ,
                                 float low, float high                           ) {
#if XBDEBUG
        var debug = new XB.DebugTimedBlock(XB.D.TerrainGradientX);
#endif

        XB.WData.LowestPoint  = low;
        XB.WData.HighestPoint = high;

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

        XB.WData.LowestPoint  = low;
        XB.WData.HighestPoint = high;

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
                UpdateLowestHighest(tHeights[i, j]);
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
                UpdateLowestHighest(tHeights[i, j]);
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
                UpdateLowestHighest(tHeights[i, j]);
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

        var height = new Godot.Color(0.0f, 0.0f, 0.0f, 1.0f);
        for (int i = 0; i < img.GetWidth(); i++) {
            for (int j = 0; j < img.GetHeight(); j++) {
                height.R = (tHeights[i, j] - lowest) / (highest - lowest);
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

        XB.WData.LowestPoint  = float.MaxValue;
        XB.WData.HighestPoint = float.MinValue;

        // Godot.GD.Print("ResetLowestHighest, Low to: " + XB.WorldData.LowestPoint
        //                + "m, High to: " + XB.WorldData.HighestPoint + "m"       );

#if XBDEBUG
        debug.End();
#endif 
    }

    public static void UpdateLowestHighest(float value) {
#if XBDEBUG
        var debug = new XB.DebugTimedBlock(XB.D.TerrainUpdateLowestHighest);
#endif

        //NOTE[ALEX]: does not use min max functions for easier debugging/logging
        if      (value < XB.WData.LowestPoint)   { XB.WData.LowestPoint  = value; }
        else if ( value > XB.WData.HighestPoint) { XB.WData.HighestPoint = value; }

#if XBDEBUG
        debug.End();
#endif 
    }

    public static void FindLowestHighest(ref float[,] heights, int amountX, int amountZ,
                                         ref float lowest, ref float highest            ) {
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
}
} // namespace close
