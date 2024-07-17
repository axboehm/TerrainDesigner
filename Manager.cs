namespace XB { // namespace open
public class Manager {
    public const  int MaxSphereAmount = 64; // limited to <= 999 because of sphere texture
    public static int ActiveSpheres   = 0;
    public static int NextSphere      = MaxSphereAmount;
    public static int HLSphereID      = MaxSphereAmount;
    public static XB.Sphere[] Spheres = new XB.Sphere[MaxSphereAmount];

    public static void InitializeSpheres() {
        var sphereScn = Godot.ResourceLoader.Load<Godot.PackedScene>(XB.ScenePaths.Sphere);
        XB.Sphere sphere;
        for (int i = 0; i < MaxSphereAmount; i++) {
            sphere = (XB.Sphere)sphereScn.Instantiate();
            sphere.InitializeSphere(i);
            XB.AData.MainRoot.AddChild(sphere);
            Spheres[i] = sphere;
        }
        UpdateActiveSpheres();
    }

    public static void UpdateSpheres(float dt) {
        for (int i = 0; i < MaxSphereAmount; i++) {
            Spheres[i].UpdateSphere(dt);
        }
        if (HLSphereID < MaxSphereAmount) {
            Spheres[HLSphereID].Highlighted = 1.0f;
        }
    }

    public static void UpdateActiveSpheres() {
        ActiveSpheres = 0;
        NextSphere = MaxSphereAmount+1;
        for (int i = 0; i < MaxSphereAmount; i++) {
            if (Spheres[i].Active) {
                ActiveSpheres += 1;
            } else {
                NextSphere = XB.Utils.MinI(i, NextSphere);
            }
        }
    }

    public static void ApplyTerrain() {
        // recalculate and assign terrain
        for (int i = 0; i < MaxSphereAmount; i++) {
            Spheres[i].RemoveSphere();
        }
    }

    public static bool RequestSphere(Godot.Vector3 pos) {
        if (NextSphere == MaxSphereAmount+1) {
            return false;
        }
        Spheres[NextSphere].PlaceSphere(pos);
        return true;
    }
}
} // namespace close
