namespace XB { // namespace open
public class Terrain {

    public static void Flat(ref float[,] tHeights, int amountX, int amountY, int height) {
        XB.WorldData.LowestPoint  = height-XB.WorldData.LowHighExtra;
        XB.WorldData.HighestPoint = height+XB.WorldData.LowHighExtra;
        for (int i = 0; i < amountX; i++) {
            for (int j = 0; j < amountY; j++) {
                tHeights[i, j] = height;
            }
        }
    }

    public static void GradientX(ref float[,] tHeights, int amountX, int amountY, int low, int high) {
        XB.WorldData.LowestPoint  = low -XB.WorldData.LowHighExtra;
        XB.WorldData.HighestPoint = high+XB.WorldData.LowHighExtra;
        for (int i = 0; i < amountX; i++) {
            for (int j = 0; j < amountY; j++) {
                tHeights[i, j] = low + ((float)i/(float)amountX)*high;
            }
        }
    }

    public static void HeightsToMesh(ref float[,] tHeights, int amountX, int amountZ, int res,
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

            //TODO[ALEX]: calculate normals properly
            v3.X = 0.0f;
            v3.Y = 1.0f;
            v3.Z = 0.0f;
            norms[i] = v3;
        }

        //NOTE[ALEX[: UVs and triangles will not change on terrain modification
        if (initialize) {
            Godot.Vector2 v2 = new Godot.Vector2(0.0f, 0.0f);
            for (int i = 0; i < uvs.Length; i++) {
                int x = i%amountX;
                int z = i/amountX;
                v2.X = (float)x/(float)(amountX-1);
                v2.Y = (float)z/(float)(amountZ-1);
                uvs[i] = v2;
            }

            int tri  = 0;
            int vert = 0;
            for (int i = 0; i < amountX-1; i++) {
                for (int j = 0; j < amountZ-1; j++) {
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

            mData[(int)Godot.Mesh.ArrayType.TexUV]  = uvs;
            mData[(int)Godot.Mesh.ArrayType.Index]  = tris;
        }

        mData[(int)Godot.Mesh.ArrayType.Vertex] = verts;
        mData[(int)Godot.Mesh.ArrayType.Normal] = norms;
        arrMesh.AddSurfaceFromArrays(Godot.Mesh.PrimitiveType.Triangles, mData);
        mesh.Mesh = arrMesh;
        col.Shape = arrMesh.CreateTrimeshShape();
    }

    public static void SkirtMesh(ref Godot.Vector3[] verts, int amountX, int amountZ, int heightLow,
                                 ref Godot.Collections.Array[] mData, ref Godot.ArrayMesh[] arrMesh,
                                 ref Godot.MeshInstance3D[] meshes, 
                                 ref Godot.Vector3[] vertsX0, ref Godot.Vector3[] vertsX1,
                                 ref Godot.Vector3[] vertsZ0, ref Godot.Vector3[] vertsZ1,
                                 ref Godot.Vector3[] normsX0, ref Godot.Vector3[] normsX1,
                                 ref Godot.Vector3[] normsZ0, ref Godot.Vector3[] normsZ1,
                                 ref int[] trisX0, ref int[] trisX1,
                                 ref int[] trisZ0, ref int[] trisZ1,
                                 bool initialize = true         ) {
        // bottom, top, left, right - upper vertices
        for (int i = 0; i < amountX; i++) { vertsX0[2*i] = verts[i];                             }
        for (int i = 0; i < amountX; i++) { vertsX1[2*i] = verts[i + amountX*amountZ - amountX]; }
        for (int i = 0; i < amountZ; i++) { vertsZ0[2*i] = verts[i*amountX];                     }
        for (int i = 0; i < amountZ; i++) { vertsZ1[2*i] = verts[i*amountX + amountX - 1];       }

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
                int pos = i + amountX*amountZ - amountX;
                vertsX1[2*i +1] = new Godot.Vector3(verts[pos].X, heightLow, verts[pos].Z);
                normsX1[2*i +0] = v3;
                normsX1[2*i +1] = v3;
            }
            v3.Z = 0.0f;
            v3.X = -1.0f;
            for (int i = 0; i < amountZ; i++) { // left
                int pos = i*amountX;
                vertsZ0[2*i +1] = new Godot.Vector3(verts[pos].X, heightLow, verts[pos].Z);
                normsZ0[2*i +0] = v3;
                normsZ0[2*i +1] = v3;
            }
            v3.X = 1.0f;
            for (int i = 0; i < amountZ; i++) { // right
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
            for (int i = 0; i < amountZ-1; i++) {
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
}
} // namespace close
