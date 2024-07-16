namespace XB { // namespace open
using SysCG = System.Collections.Generic;
public class Manager {
    private static SysCG.List<XB.Sphere> _spheres = new SysCG.List<XB.Sphere>();
    public  const  int MaxSphereAmount = 64;
    public  static int ActiveSpheres = 0;
    public  static int NextSphere    = MaxSphereAmount+1;

    public static void InitializeSpheres() {
        var sphereScn = Godot.ResourceLoader.Load<Godot.PackedScene>(XB.ScenePaths.Sphere);
        XB.Sphere sphere;
        for (int i = 0; i < MaxSphereAmount; i++) {
            sphere = (XB.Sphere)sphereScn.Instantiate();
            sphere.InitializeSphere(i);
            XB.AData.MainRoot.AddChild(sphere);
            _spheres.Add(sphere);
        }
        UpdateActiveSpheres();
    }

    public static void UpdateActiveSpheres() {
        ActiveSpheres = 0;
        NextSphere = MaxSphereAmount+1;
        for (int i = 0; i < MaxSphereAmount; i++) {
            if (_spheres[i].Active) {
                ActiveSpheres += 1;
            } else {
                NextSphere = XB.Utils.MinI(i, NextSphere);
            }
        }
    }

    public static void ApplyTerrain() {
        // recalculate and assign terrain
        for (int i = 0; i < MaxSphereAmount; i++) {
            _spheres[i].RemoveSphere();
        }
    }

    public static bool RequestSphere(Godot.Vector3 pos) {
        if (NextSphere == MaxSphereAmount+1) {
            return false;
        }
        _spheres[NextSphere].PlaceSphere(pos);
        return true;
    }
}
} // namespace close
