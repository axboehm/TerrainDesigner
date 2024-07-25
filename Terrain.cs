namespace XB { // namespace open
public class Terrain {

    // FBM (fractal brownian motion) noise is an addition of multiple layers of perlin noise,
    // each with increasing frequency (detail) but less amplitude (strength)
    // octaves: number of layers of pwerlin noise
    // persistence: change of amplitude of next octave's noise
    // lacunarity: change of frequency of next octave's noise
    // exponentation: exponent for final adjustment (1 for straight result)
    //
    //NOTE[ALEX]: if scale is a multiple of the blue noise size, the pattern will repeat visibly
    public static void FBM(ref float[,] tHeights, int amountX, int amountY,
                           float sizeX, float sizeY, float height, float scale,
                           int octaves = 4, float persistence = 0.5f,
                           float lacunarity = 2.0f, float exponentation = 4.5f,
                           float offsetX = 0.0f, float offsetY = 0.0f          ) {
        ResetLowestHighest();

        float xStep   = scale * (sizeX / ((float)(amountX-1)) );
        float yStep   = scale * (sizeY / ((float)(amountY-1)) );

        float ampMult = (float)System.Math.Pow(2.0f, -persistence);
        float amp     = 1.0f;
        float norm    = 0.0f;
        for (int o = 0; o < octaves; o++) {
            norm  += amp;
            amp   *= ampMult;
        }
        float freq    = 1.0f;
        float total   = 0.0f;
        float value   = 0.0f;
        for (int i = 0; i < amountX; i++) {
            //TODO[ALEX]: parallel for loop
            for (int j = 0; j < amountY; j++) {
                amp   = 1.0f;
                freq  = 1.0f;
                total = 0.0f;
                value = 0.0f;

                for (int o = 0; o < octaves; o++) {
                    float x = i*xStep;
                    float y = j*yStep;
                    value  = Perlin2D(x*freq, y*freq);
                    value *= 0.5f;
                    value += 0.5f;
                    total += value*amp;
                    amp   *= ampMult;
                    freq  *= lacunarity;
                }

                total /= norm;
                tHeights[i, j] = (float)System.Math.Pow(total, exponentation) * height;
                UpdateLowestHighest(tHeights[i, j]);
            }
        }
    }

    private static float Perlin2D(float x, float y) {
        int   xI = (int)x;
        int   yI = (int)y;
        float xF = x - xI;
        float yF = y - yI;

        float a = XB.Random.RandomBlueNoise(xI + 0, yI + 0);
        float b = XB.Random.RandomBlueNoise(xI + 1, yI + 0);
        float c = XB.Random.RandomBlueNoise(xI + 0, yI + 1);
        float d = XB.Random.RandomBlueNoise(xI + 1, yI + 1);
        // Godot.GD.Print("x " + x + " y " + y + " a " + a + " b " + b + " c " + c + " d " + d);

        var interpX = xF*xF*(3.0f - 2.0f*xF);
        var interpY = yF*yF*(3.0f - 2.0f*yF);

        float bot = XB.Utils.LerpF(a, b, interpX);
        float top = XB.Utils.LerpF(c, d, interpX);

        return XB.Utils.LerpF(bot, top, interpY);
    }

    private static void ResetLowestHighest() {
        XB.WorldData.LowestPoint  = float.MaxValue;
        XB.WorldData.HighestPoint = float.MinValue;
    }

    private static void UpdateLowestHighest(float value) {
        XB.WorldData.LowestPoint  = XB.Utils.MinF(XB.WorldData.LowestPoint,  value);
        XB.WorldData.HighestPoint = XB.Utils.MaxF(XB.WorldData.HighestPoint, value);
    }

    public static void Flat(ref float[,] tHeights, int amountX, int amountY, float height) {
        XB.WorldData.LowestPoint  = height - XB.WorldData.LowHighExtra;
        XB.WorldData.HighestPoint = height + XB.WorldData.LowHighExtra;
        for (int i = 0; i < amountX; i++) {
            for (int j = 0; j < amountY; j++) {
                tHeights[i, j] = height;
            }
        }
    }

    public static void GradientX(ref float[,] tHeights, int amountX, int amountY,
                                 float low, float high                           ) {
        XB.WorldData.LowestPoint  = low  - XB.WorldData.LowHighExtra;
        XB.WorldData.HighestPoint = high + XB.WorldData.LowHighExtra;
        for (int i = 0; i < amountX; i++) {
            for (int j = 0; j < amountY; j++) {
                tHeights[i, j] = low + ((float)i/(float)amountX)*high;
            }
        }
    }

    public static void GradientY(ref float[,] tHeights, int amountX, int amountY, 
                                float low, float high                            ) {
        XB.WorldData.LowestPoint  = low  - XB.WorldData.LowHighExtra;
        XB.WorldData.HighestPoint = high + XB.WorldData.LowHighExtra;
        for (int i = 0; i < amountY; i++) {
            for (int j = 0; j < amountX; j++) {
                tHeights[j, i] = low + ((float)i/(float)amountY)*high;
            }
        }
    }

    public static void HeightMax(ref float[,] tHeights, ref float[,] tHeightsM,
                                 int amountX, int amountY                      ) {
        for (int i = 0; i < amountX; i++) {
            for (int j = 0; j < amountY; j++) {
                tHeights[i, j] = XB.Utils.MaxF(tHeights[i, j], tHeightsM[i, j]);
            }
        }
    }

    public static void HeightsToMesh(ref float[,] tHeights, int amountX, int amountY, int res,
                                     ref Godot.Collections.Array mData, ref Godot.ArrayMesh arrMesh,
                                     ref Godot.MeshInstance3D mesh, ref Godot.CollisionShape3D col, 
                                     ref Godot.Vector3[] verts, ref Godot.Vector2[] uvs,
                                     ref Godot.Vector3[] norms, ref int[] tris, bool initialize = true) {
        Godot.Vector3 v3 = new Godot.Vector3(0.0f, 0.0f, 0.0f);
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
            Godot.Vector2 v2 = new Godot.Vector2(0.0f, 0.0f);
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
    }

    private static void CalculateNormals(ref Godot.Vector3[] norms,
                                         ref Godot.Vector3[] verts, ref int[] tris) {
        //TODO[ALEX]: parallel?
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

            // for (int i = 0; i < vertsZ1.Length; i++) {
            //     Godot.GD.Print(vertsZ1[i]);
            // }

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
                // Godot.GD.Print(trisZ1[tri+0] + " " + trisZ1[tri+1] + " " + trisZ1[tri+2]);
                // Godot.GD.Print(trisZ1[tri+3] + " " + trisZ1[tri+4] + " " + trisZ1[tri+5]);
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
    }

    // expects img to be of type L8
    public static void UpdateHeightMap(ref float[,] tHeights, int amountX, int amountY,
                                       float lowest, float highest, ref Godot.Image img) {
        var height = new Godot.Color(0.0f, 0.0f, 1.0f, 0.0f); // only the blue value will be used
        for (int i = 0; i < amountX; i++) {
            for (int j = 0; j < amountY; j++) {
                //NOTE[ALEX]: invert the id order because 0|0 of image is in top left
                //            (also see XB.HUD.UpdateMiniMapOverlayTexture)
                height.B = (tHeights[amountX-1-i, amountY-1-j] - lowest) / (highest - lowest);
                img.SetPixel(i, j, height);
            }
        }
    }
}
} // namespace close
