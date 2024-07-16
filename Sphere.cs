namespace XB { // namespace open
using SysCG = System.Collections.Generic;
public partial class Sphere : Godot.Node3D {
    [Godot.Export] private Godot.NodePath        _sphereMesh;
                   private Godot.BaseMaterial3D  _shellMat;
                   private Godot.BaseMaterial3D  _screenMat;
    [Godot.Export] private Godot.AnimationPlayer _animPl;

    public int  ID     = 0;
    public bool Active = false;

    public SysCG.List<XB.Sphere> _linkedSpheres = new SysCG.List<XB.Sphere>();

    public void InitializeSphere(int id) {
        ID     = id;
        Active = false;

        _shellMat  = (Godot.BaseMaterial3D)GetNode<Godot.MeshInstance3D>
                         (_sphereMesh).GetSurfaceOverrideMaterial(0);
        _screenMat = (Godot.BaseMaterial3D)GetNode<Godot.MeshInstance3D>
                         (_sphereMesh).GetSurfaceOverrideMaterial(1);
        // build texture for screen
        // scroll texture in shader
        Hide();
    }

    public override void _PhysicsProcess(double delta) {
        // use this for lerping/etc.?
    }

    // player places sphere in world
    public void PlaceSphere(Godot.Vector3 pos) {
        Show();
        GlobalPosition = pos;
        Active = true;
        XB.Manager.UpdateActiveSpheres();
    }

    // when linking this sphere with other spheres
    public void LinkSphere() {
        // extend antennas
        // add linked spheres to list
        // update other linked spheres
    }

    public void UnlinkSphere() {
        // retract antennas
        // remove linked spheres list
        // remove sphere from linked spheres' lists
        // update all affected spheres
    }

    // remove sphere from world
    public void RemoveSphere() {
        // remove sphere dam geometry
        Hide();
        Active = false;
        XB.Manager.UpdateActiveSpheres();
    }

    // when sphere gets moved
    public void UpdateSphere() {
        // update cone geometry
        if (_linkedSpheres.Count > 0) {
            // update dam geometry
        }
    }
}
} // namespace close
