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
                                     ref Godot.Vector3[] norms, ref int[] tris, bool initial = true  ) {
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

        mData[(int)Godot.Mesh.ArrayType.Vertex] = verts;
        mData[(int)Godot.Mesh.ArrayType.Normal] = norms;

        //NOTE[ALEX[: UVs and Triangles will not change on terrain modification
        if (initial) {
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

            mData[(int)Godot.Mesh.ArrayType.TexUV] = uvs;
            mData[(int)Godot.Mesh.ArrayType.Index] = tris;
        }

        arrMesh.AddSurfaceFromArrays(Godot.Mesh.PrimitiveType.Triangles, mData);
        mesh.Mesh = arrMesh;
        col.Shape = arrMesh.CreateTrimeshShape();
    }
}
} // namespace close
