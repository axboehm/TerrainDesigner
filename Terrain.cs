// #define XBSINGLETHREADED
#define XBDEBUG
namespace XB { // namespace open
public class Terrain {
    //NOTE[ALEX]: parallelization with function parameters is a headache, so all the relevant
    //            factors get saved here instead of calculated in the FBM part
    public static float Height        = 0.0f;
    public static float NoiseScale    = 0.0f;   //NOTE[ALEX]: if scale is a multiple of the blue
                                                //            noise texture's size, the pattern
                                                //            will repeat visibly
    public static float OffsetX       = 0.0f;
    public static float OffsetY       = 0.0f;
    public static int   Octaves       = 0;      // number of layers of noise
    public static float AmpMult       = 0.0f;   // controls change of amplitude for next octave's noise
    public static float Lacunarity    = 0.0f;   // change of frequency of next octave's noise
    public static float Exponentation = 0.0f;   // exponent of power function of final result
    public static float Normalization = 0.0f;   // maximum amount for added noise samples
    public static float XStep         = 0.0f;
    public static float YStep         = 0.0f;

    public static void SetTerrainParameters(float height = 18.0f,    float scale = 0.0174f, 
                                            float offsetX = 0.0f,    float offsetY = 0.0f, 
                                            int   octaves = 4,       float persistence = 0.5f,
                                            float lacunarity = 2.0f, float exponentation = 4.5f) {
#if XBDEBUG
        var debug = new XB.DebugTimedBlock(XB.D.TerrainSetTerrainParameters);
#endif

        if (scale % 1.0f == 0.0f) { NoiseScale = scale + 0.5f; }
        else                      { NoiseScale = scale;        }
        Height  = height;
        OffsetX = offsetX;
        OffsetY = offsetY;
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
    public static void FBM(int amountX, int amountY, float sizeX, float sizeY) {
#if XBDEBUG
        var debug = new XB.DebugTimedBlock(XB.D.TerrainFBM);
#endif

        XStep = NoiseScale * (sizeX / ((float)(amountX-1)) );
        YStep = NoiseScale * (sizeY / ((float)(amountY-1)) );

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
            for (int j = 0; j < amountY; j++) {
                amp   = 1.0f;
                freq  = 1.0f;
                total = 0.0f;
                value = 0.0f;

                for (int o = 0; o < Octaves; o++) {
                    float x = i*XStep;
                    float y = j*YStep;
                    value  = XB.Random.ValueNoise2D(x*freq, y*freq);
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
                float y = j*YStep;
                value  = XB.Random.ValueNoise2D(x*freq, y*freq);
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

    public static void Flat(ref float[,] tHeights, int amountX, int amountY, float height) {
#if XBDEBUG
        var debug = new XB.DebugTimedBlock(XB.D.TerrainFlat);
#endif

        XB.WorldData.LowestPoint  = height - XB.WorldData.LowHighExtra;
        XB.WorldData.HighestPoint = height + XB.WorldData.LowHighExtra;
        for (int i = 0; i < amountX; i++) {
            for (int j = 0; j < amountY; j++) {
                tHeights[i, j] = height;
            }
        }

#if XBDEBUG
        debug.End();
#endif 
    }

    public static void GradientX(ref float[,] tHeights, int amountX, int amountY,
                                 float low, float high                           ) {
#if XBDEBUG
        var debug = new XB.DebugTimedBlock(XB.D.TerrainGradientX);
#endif

        XB.WorldData.LowestPoint  = low  - XB.WorldData.LowHighExtra;
        XB.WorldData.HighestPoint = high + XB.WorldData.LowHighExtra;
        for (int i = 0; i < amountX; i++) {
            for (int j = 0; j < amountY; j++) {
                tHeights[i, j] = low + ((float)i/(float)amountX)*high;
            }
        }

#if XBDEBUG
        debug.End();
#endif 
    }

    public static void GradientY(ref float[,] tHeights, int amountX, int amountY, 
                                float low, float high                            ) {
#if XBDEBUG
        var debug = new XB.DebugTimedBlock(XB.D.TerrainGradientY);
#endif

        XB.WorldData.LowestPoint  = low  - XB.WorldData.LowHighExtra;
        XB.WorldData.HighestPoint = high + XB.WorldData.LowHighExtra;
        for (int i = 0; i < amountY; i++) {
            for (int j = 0; j < amountX; j++) {
                tHeights[j, i] = low + ((float)i/(float)amountY)*high;
            }
        }

#if XBDEBUG
        debug.End();
#endif 
    }

    //TODO[ALEX]: delegate with function type?

    public static void HeightMax(ref float[,] tHeights, ref float[,] tHeightsM,
                                 int amountX, int amountY                      ) {
#if XBDEBUG
        var debug = new XB.DebugTimedBlock(XB.D.TerrainHeightMax);
#endif

        ResetLowestHighest();
        for (int i = 0; i < amountX; i++) {
            for (int j = 0; j < amountY; j++) {
                tHeights[i, j] = XB.Utils.MaxF(tHeights[i, j], tHeightsM[i, j]);
                UpdateLowestHighest(tHeights[i, j], tHeights[i, j]);
            }
        }

#if XBDEBUG
        debug.End();
#endif 
    }

    public static void HeightReplace(ref float[,] tHeights, ref float[,] tHeightsM,
                                     int amountX, int amountY                      ) {
#if XBDEBUG
        var debug = new XB.DebugTimedBlock(XB.D.TerrainHeightReplace);
#endif

        ResetLowestHighest();
        for (int i = 0; i < amountX; i++) {
            for (int j = 0; j < amountY; j++) {
                tHeights[i, j] = tHeightsM[i, j];
                UpdateLowestHighest(tHeights[i, j], tHeights[i, j]);
            }
        }

#if XBDEBUG
        debug.End();
#endif 
    }

    public static void HeightsToMesh(ref float[,] tHeights, int amountX, int amountY, int res,
                                     ref Godot.Collections.Array mData, ref Godot.ArrayMesh arrMesh,
                                     ref Godot.MeshInstance3D mesh, ref Godot.CollisionShape3D col, 
                                     ref Godot.Vector3[] verts, ref Godot.Vector2[] uvs,
                                     ref Godot.Vector3[] norms, ref int[] tris, bool initialize = true) {
#if XBDEBUG
        var debug = new XB.DebugTimedBlock(XB.D.TerrainHeightsToMesh);
#endif

        var v3 = new Godot.Vector3(0.0f, 0.0f, 0.0f);
        float step = 1.0f/(float)res;
        for (int i = 0; i < verts.Length; i++) {
            int x = i%amountX;
            int z = i/amountX;
            v3.X = (float)x*step;
            v3.Y = tHeights[x, z];
            v3.Z = (float)z*step;
            verts[i] = v3;
        }

        //NOTE[ALEX[: UVs and triangles will not change on terrain modification
        if (initialize) {
            var v2 = new Godot.Vector2(0.0f, 0.0f);
            for (int i = 0; i < uvs.Length; i++) {
                int x = i%amountX;
                int z = i/amountX;
                v2.X = 1.0f - (float)x/(float)(amountX-1);
                v2.Y = 1.0f - (float)z/(float)(amountY-1);
                uvs[i] = v2;
            }

            int tri  = 0;
            int vert = 0;
            for (int i = 0; i < amountX-1; i++) {
                for (int j = 0; j < amountY-1; j++) {
                    tris[tri + 0] = vert;
                    tris[tri + 1] = vert + 1;
                    tris[tri + 2] = vert + 1 + amountX;
                    tris[tri + 3] = vert;
                    tris[tri + 4] = vert + 1 + amountX;
                    tris[tri + 5] = vert     + amountX;
                    tri  += 6;
                    vert += 1;
                }
                vert += 1;
            }

            mData[(int)Godot.Mesh.ArrayType.TexUV] = uvs;
            mData[(int)Godot.Mesh.ArrayType.Index] = tris;
        }
        
        CalculateNormals(ref norms, ref verts, ref tris);

        mData[(int)Godot.Mesh.ArrayType.Vertex] = verts;
        mData[(int)Godot.Mesh.ArrayType.Normal] = norms;
        arrMesh.ClearSurfaces(); // needs to be cleared for UV's to work properly on mesh update
        arrMesh.AddSurfaceFromArrays(Godot.Mesh.PrimitiveType.Triangles, mData);
        mesh.Mesh = arrMesh;
        col.Shape = arrMesh.CreateTrimeshShape();

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

    public static void SkirtMesh(ref Godot.Vector3[] verts, int amountX, int amountY, int heightLow,
                                 ref Godot.Collections.Array[] mData, ref Godot.ArrayMesh[] arrMesh,
                                 ref Godot.MeshInstance3D[] meshes, 
                                 ref Godot.Vector3[] vertsX0, ref Godot.Vector3[] vertsX1,
                                 ref Godot.Vector3[] vertsZ0, ref Godot.Vector3[] vertsZ1,
                                 ref Godot.Vector3[] normsX0, ref Godot.Vector3[] normsX1,
                                 ref Godot.Vector3[] normsZ0, ref Godot.Vector3[] normsZ1,
                                 ref int[] trisX0, ref int[] trisX1,
                                 ref int[] trisZ0, ref int[] trisZ1,
                                 bool initialize = true                                             ) {
#if XBDEBUG
        var debug = new XB.DebugTimedBlock(XB.D.TerrainSkirtMesh);
#endif

        // bottom, top, left, right - upper vertices
        for (int i = 0; i < amountX; i++) { vertsX0[2*i] = verts[i];                             }
        for (int i = 0; i < amountX; i++) { vertsX1[2*i] = verts[i + amountX*amountY - amountX]; }
        for (int i = 0; i < amountY; i++) { vertsZ0[2*i] = verts[i*amountX];                     }
        for (int i = 0; i < amountY; i++) { vertsZ1[2*i] = verts[i*amountX + amountX - 1];       }

        //NOTE[ALEX[: UVs, triangles, normals and lower vertices will not change on terrain modification
        if (initialize) {
            var v3 = new Godot.Vector3(0.0f, 0.0f, -1.0f);
            for (int i = 0; i < amountX; i++) { // bottom
                vertsX0[2*i +1] = new Godot.Vector3(verts[i].X, heightLow, verts[i].Z);
                normsX0[2*i +0] = v3;
                normsX0[2*i +1] = v3;
            }
            v3.Z = 1.0f;
            for (int i = 0; i < amountX; i++) { // top
                int pos = i + amountX*amountY - amountX;
                vertsX1[2*i +1] = new Godot.Vector3(verts[pos].X, heightLow, verts[pos].Z);
                normsX1[2*i +0] = v3;
                normsX1[2*i +1] = v3;
            }
            v3.Z = 0.0f;
            v3.X = -1.0f;
            for (int i = 0; i < amountY; i++) { // left
                int pos = i*amountX;
                vertsZ0[2*i +1] = new Godot.Vector3(verts[pos].X, heightLow, verts[pos].Z);
                normsZ0[2*i +0] = v3;
                normsZ0[2*i +1] = v3;
            }
            v3.X = 1.0f;
            for (int i = 0; i < amountY; i++) { // right
                int pos = i*amountX + amountX - 1;
                vertsZ1[2*i +1] = new Godot.Vector3(verts[pos].X, heightLow, verts[pos].Z);
                normsZ1[2*i +0] = v3;
                normsZ1[2*i +1] = v3;
            }

            int tri  = 0;
            int vert = 0;
            for (int i = 0; i < amountX-1; i++) {
                trisX0[tri + 0] = vert;
                trisX0[tri + 1] = vert + 1;
                trisX0[tri + 2] = vert + 3;
                trisX0[tri + 3] = vert;
                trisX0[tri + 4] = vert + 3;
                trisX0[tri + 5] = vert + 2;

                trisX1[tri + 0] = vert + 2;
                trisX1[tri + 1] = vert + 3;
                trisX1[tri + 2] = vert;
                trisX1[tri + 3] = vert + 1;
                trisX1[tri + 4] = vert;
                trisX1[tri + 5] = vert + 3;
                tri  += 6;
                vert += 2;
            }
            tri  = 0;
            vert = 0;
            for (int i = 0; i < amountY-1; i++) {
                trisZ0[tri + 0] = vert + 1;
                trisZ0[tri + 1] = vert;
                trisZ0[tri + 2] = vert + 3;
                trisZ0[tri + 3] = vert + 3;
                trisZ0[tri + 4] = vert;
                trisZ0[tri + 5] = vert + 2;

                trisZ1[tri + 0] = vert;
                trisZ1[tri + 1] = vert + 1;
                trisZ1[tri + 2] = vert + 3;
                trisZ1[tri + 3] = vert;
                trisZ1[tri + 4] = vert + 3;
                trisZ1[tri + 5] = vert + 2;
                tri  += 6;
                vert += 2;
            }
        }

        mData[0][(int)Godot.Mesh.ArrayType.Vertex] = vertsX0;
        mData[0][(int)Godot.Mesh.ArrayType.Normal] = normsX0;
        mData[0][(int)Godot.Mesh.ArrayType.Index]  = trisX0;
        arrMesh[0].AddSurfaceFromArrays(Godot.Mesh.PrimitiveType.Triangles, mData[0]);
        meshes[0].Mesh = arrMesh[0];
        mData[1][(int)Godot.Mesh.ArrayType.Vertex] = vertsX1;
        mData[1][(int)Godot.Mesh.ArrayType.Normal] = normsX1;
        mData[1][(int)Godot.Mesh.ArrayType.Index]  = trisX1;
        arrMesh[1].AddSurfaceFromArrays(Godot.Mesh.PrimitiveType.Triangles, mData[1]);
        meshes[1].Mesh = arrMesh[1];
        mData[2][(int)Godot.Mesh.ArrayType.Vertex] = vertsZ0;
        mData[2][(int)Godot.Mesh.ArrayType.Normal] = normsZ0;
        mData[2][(int)Godot.Mesh.ArrayType.Index]  = trisZ0;
        arrMesh[2].AddSurfaceFromArrays(Godot.Mesh.PrimitiveType.Triangles, mData[2]);
        meshes[2].Mesh = arrMesh[2];
        mData[3][(int)Godot.Mesh.ArrayType.Vertex] = vertsZ1;
        mData[3][(int)Godot.Mesh.ArrayType.Normal] = normsZ1;
        mData[3][(int)Godot.Mesh.ArrayType.Index]  = trisZ1;
        arrMesh[3].AddSurfaceFromArrays(Godot.Mesh.PrimitiveType.Triangles, mData[3]);
        meshes[3].Mesh = arrMesh[3];

#if XBDEBUG
        debug.End();
#endif 
    }

    // expects img to be of type L8
    public static void UpdateHeightMap(ref float[,] tHeights, int amountX, int amountY,
                                       float lowest, float highest, ref Godot.Image img) {
#if XBDEBUG
        var debug = new XB.DebugTimedBlock(XB.D.TerrainUpdateHeightMap);
#endif

        var height = new Godot.Color(0.0f, 0.0f, 1.0f, 0.0f); // only the blue value will be used
        for (int i = 0; i < amountX; i++) {
            for (int j = 0; j < amountY; j++) {
                //NOTE[ALEX]: invert the id order because 0|0 of image is in top left
                //            (also see XB.HUD.UpdateMiniMapOverlayTexture)
                height.B = (tHeights[amountX-1-i, amountY-1-j] - lowest) / (highest - lowest);
                img.SetPixel(i, j, height);
            }
        }

#if XBDEBUG
        debug.End();
#endif 
    }

    public static float HeightMapSample(float sampleX, float sampleZ,
                                        float worldXSize, float worldZSize, ref Godot.Image img) {
#if XBDEBUG
        var debug = new XB.DebugTimedBlock(XB.D.TerrainHeightMapSample);
#endif

        float x  = (1.0f - (sampleX/worldXSize) ) * (img.GetWidth() -1);
        float z  = (1.0f - (sampleZ/worldZSize) ) * (img.GetHeight()-1);
        int   xI0 = (int)x;
        int   zI0 = (int)z;
        int   xI1 = XB.Utils.MinI(xI0+1, img.GetWidth() -1);
        int   zI1 = XB.Utils.MinI(zI0+1, img.GetHeight()-1);

        // 0    0|0 in top left, in world: 0|0 in "bottom right"
        //  AB  samples surrounding the sampled coordinates
        //  CD  sample coordinates are inside these points (including)

        float sampleA = img.GetPixel(xI0, zI0).B;
        float sampleB = img.GetPixel(xI1, zI0).B;
        float sampleC = img.GetPixel(xI0, zI1).B;
        float sampleD = img.GetPixel(xI1, zI1).B;
        float upper   = XB.Utils.LerpF(sampleA, sampleB, x-xI0);
        float lower   = XB.Utils.LerpF(sampleC, sampleD, x-xI0);
        float result  = XB.Utils.LerpF(lower,   upper,   z-zI0);

        // Godot.GD.Print(" world: " +sampleX +"m/" +worldXSize +"m, " +sampleZ +"m/" +worldZSize +"m");
        // Godot.GD.Print(x + " " + z + " of " + img.GetSize() + " xI0: " + xI0 + " zI0: " + zI0);
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
