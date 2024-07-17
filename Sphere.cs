namespace XB { // namespace open
using SysCG = System.Collections.Generic;
public partial class Sphere : Godot.CharacterBody3D {
    [Godot.Export] private Godot.NodePath        _sphereMesh;
                   private Godot.ShaderMaterial  _shellMat;
                   private Godot.ShaderMaterial  _screenMat;
    [Godot.Export] private Godot.AnimationPlayer _animPl;
                   private Godot.Image           _imgScrolling;
                   private Godot.ImageTexture    _texScrolling;

    private const int _repeats    = 8;   // how often the id gets repeated in the texture
    private const int _dimScrollX = 128; //NOTE[ALEX]: based on textures created for sphere
    private const int _dimScrollY = 16;
    private const int _dimThick   = 1;   // thickness of digits
    private const int _dimDigitX  = _dimScrollY/2 - 2*_dimThick;
    private const int _dimDigitY  = _dimScrollY   - 2*_dimThick;
    private Godot.Color _colBlack = new Godot.Color(0.0f, 0.0f, 0.0f, 1.0f);
    private Godot.Color _colWhite = new Godot.Color(1.0f, 1.0f, 1.0f, 1.0f);

    public int   ID          = 0;
    public bool  Active      = false;
    public float Highlighted = 0.0f;
    public bool  Linked      = false;
    private SysCG.List<XB.Sphere> _linkedSpheres = new SysCG.List<XB.Sphere>();

    private Godot.Color _sphereColor    = new Godot.Color(0.6f, 1.0f, 0.6f, 1.0f);
    private Godot.Color _highlightColor = new Godot.Color(0.6f, 1.0f, 0.6f, 1.0f);
    private Godot.Color _linkColor      = new Godot.Color(1.0f, 0.68f, 0.0f, 1.0f);
    private const float _sphEmitStrDef  = 2.1f;
    private const float _sphEmitStrLink = 5.8f;
    private       float _sphEmitStrTar  = _sphEmitStrDef; // lerp target
    private       float _sphEmitStr     = _sphEmitStrDef; // current actual emission strength
    private const float _sphScrSpeed    = 0.018f;
    private       float _hlMult         = 0.0f;
    private       float _hlSm           = 12.0f;

    public void InitializeSphere(int id) {
        ID             = id;
        CollisionLayer = XB.LayerMasks.SphereLayer;

        _shellMat  = (Godot.ShaderMaterial)GetNode<Godot.MeshInstance3D>
                         (_sphereMesh).GetSurfaceOverrideMaterial(0);
        _screenMat = (Godot.ShaderMaterial)GetNode<Godot.MeshInstance3D>
                         (_sphereMesh).GetSurfaceOverrideMaterial(1);

        _shellMat.SetShaderParameter ("highlightCol",  _sphereColor);
        _shellMat.SetShaderParameter ("highlightMult", 0.0f        );
        _shellMat.SetShaderParameter ("emissionStr",   _sphEmitStr );
        _screenMat.SetShaderParameter("scrollSpeed",   _sphScrSpeed);
        _screenMat.SetShaderParameter("emissionStr",   _sphEmitStr );

        _imgScrolling = Godot.Image.Create(_dimScrollX, _dimScrollY, false, Godot.Image.Format.Rgba8);
        _imgScrolling.Fill(_colBlack);

        for (int i = 0; i < _repeats; i++) {
            int xStart = i*(_dimScrollX/_repeats);
            int width  = _dimDigitX + 2*_dimThick;
            if (ID > 9) { width += _dimDigitX; }
            var numberField = new Godot.Rect2I(xStart, _dimThick, width, _dimDigitY);
            _imgScrolling.FillRect(numberField, _colBlack);

            if (ID > 9) { // decimal digit
                var segmentsD = XB.Utils.DigitRectangles(ID/10, xStart+_dimThick, _dimThick,
                                                         _dimDigitX, _dimDigitY, _dimThick  );
                foreach (var segment in segmentsD) { _imgScrolling.FillRect(segment, _colWhite); }
                xStart += _dimDigitX;
            }
            var segments = XB.Utils.DigitRectangles(ID%10, xStart+_dimThick, _dimThick,
                                                    _dimDigitX, _dimDigitY, _dimThick  );
            foreach (var segment in segments) { _imgScrolling.FillRect(segment, _colWhite); }
        }

        _texScrolling = new Godot.ImageTexture();
        _texScrolling.SetImage(_imgScrolling);
        _screenMat.SetShaderParameter("tWriting", _texScrolling);

        Hide();
    }

    public void UpdateSphere(float dt) {
        if (XB.Manager.Linking) { _sphereColor = _sphereColor.Lerp(_linkColor,      _hlSm*dt); }
        else                    { _sphereColor = _sphereColor.Lerp(_highlightColor, _hlSm*dt); }

        if (XB.Manager.LinkingID == ID) {
            _sphEmitStrTar = _sphEmitStrLink;
            _hlMult = XB.Utils.LerpF(_hlMult, 1.0f, _hlSm*dt);
            foreach (XB.Sphere lS in _linkedSpheres) { lS.Highlighted = 1.0f; }
        } else {
            _sphEmitStrTar = _sphEmitStrDef;
        }

        _sphEmitStr = XB.Utils.LerpF(_sphEmitStr, _sphEmitStrTar, _hlSm*dt);
        _hlMult     = XB.Utils.LerpF(_hlMult, Highlighted, _hlSm*dt);
        Highlighted = 0.0f;
        _shellMat.SetShaderParameter ("emissionStr",   _sphEmitStr );
        _shellMat.SetShaderParameter ("highlightCol",  _sphereColor);
        _shellMat.SetShaderParameter ("highlightMult", _hlMult     );
    }

    // player places sphere in world
    public void PlaceSphere(Godot.Vector3 pos) {
        Show();
        GlobalPosition = pos;
        Active         = true;
        XB.Manager.UpdateActiveSpheres();
        XB.PController.Hud.UpdateSphereTexture(ID, XB.SphereTexSt.Active);
    }

    public void SphereTextureAddLinked() {
        XB.PController.Hud.UpdateSphereTexture(ID, XB.SphereTexSt.ActiveLinking);
        foreach (XB.Sphere lS in _linkedSpheres) {
            XB.PController.Hud.UpdateSphereTexture(lS.ID, XB.SphereTexSt.ActiveLinked);
        }    
    }

    public void SphereTextureRemoveLinked() {
        XB.PController.Hud.UpdateSphereTexture(ID, XB.SphereTexSt.Active);
        foreach (XB.Sphere lS in _linkedSpheres) {
            XB.PController.Hud.UpdateSphereTexture(lS.ID, XB.SphereTexSt.Active);
        }    
    }

    public void LinkSphere(int idLinkFrom) {
        foreach (XB.Sphere lS in _linkedSpheres) {
            if (lS.ID == idLinkFrom) { return; }
        }
        _linkedSpheres.Add(XB.Manager.Spheres[idLinkFrom]);
        if (!Linked && _animPl.CurrentAnimation != "expand") { _animPl.Play("expand"); }
        Linked = true;
    }

    public void UnlinkSphere(XB.Sphere sphereUnlinkFrom) {
        _linkedSpheres.Remove(sphereUnlinkFrom);

        if (_linkedSpheres.Count == 0) {
            if (Linked && _animPl.CurrentAnimation != "contract") { _animPl.Play("contract"); }
            Linked = false;
        }
    }

    public void UnlinkFromAllSpheres() {
        if (!Linked) { return; }

        foreach (XB.Sphere lS in _linkedSpheres) { lS.UnlinkSphere(this); }
        _linkedSpheres.Clear();

        if (_animPl.CurrentAnimation != "contract") { _animPl.Play("contract"); }
        Linked = false;
    }

    // remove sphere from world (does not remove from Manager Spheres array)
    public void RemoveSphere() {
        //TODO[ALEX]: remove sphere dam geometry
        UnlinkFromAllSpheres();
        Hide();
        Active = false;
        _animPl.Stop();
        XB.Manager.UpdateActiveSpheres();
        XB.PController.Hud.UpdateSphereTexture(ID, XB.SphereTexSt.Available);
    }

    // when sphere gets moved
    public void MoveSphere(Godot.Transform3D camTrans, Godot.Transform3D camTransPrev,
                           Godot.PhysicsDirectSpaceState3D spaceState                 ) {
        var   move    = new Godot.Vector3(0.0f, 0.0f, 0.0f);
        var   rayOrig = camTrans.Origin; 
        float rayDist = XB.WorldData.WorldDim.X+XB.WorldData.WorldDim.Y;
        var   rayDest = camTrans.Origin-rayDist*camTrans.Basis.Z;
        var result = XB.Utils.Raycast(spaceState, rayOrig, rayDest, XB.LayerMasks.SphereMask);
        if (result.Count > 0) {
            var hitPos    = (Godot.Vector3)result["position"];
            var hitNrm    = new Godot.Vector3(camTrans.Origin.X, 0.0f, camTrans.Origin.Z);
                hitNrm.X -= hitPos.X; // sphere to player
                hitNrm.Z -= hitPos.Z;
            var hitPrev   = XB.Utils.IntersectRayPlaneV3(camTransPrev.Origin, camTransPrev.Basis.Z,
                                                         hitPos, hitNrm                            );
            var hitThis   = XB.Utils.IntersectRayPlaneV3(camTrans.Origin, camTrans.Basis.Z,
                                                         hitPos, hitNrm                            );
            move.Y = hitThis.Y-hitPrev.Y;
        } else {
            Godot.GD.Print("MoveSphere has no ray hits");
        }
        GlobalPosition += move;

        //TODO[ALEX]: consider wether spheres should move along plane or in arc around player?

        //TODO[ALEX]: update cone geometry
        if (Linked) {
            //TODO[ALEX]: update dam geometry
        }
    }
}
} // namespace close
