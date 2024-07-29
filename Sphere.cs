#define XBDEBUG
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

    public int   ID          = 0;
    public bool  Active      = false;
    public float Highlighted = 0.0f;
    public bool  Linked      = false;
    public bool  LinkedTo    = false;
    public XB.SphereTexSt TexSt = XB.SphereTexSt.Inactive;
    private SysCG.List<XB.Sphere> _linkedSpheres = new SysCG.List<XB.Sphere>();

    private Godot.Color _sphereColor    = new Godot.Color(0.0f, 0.0f, 0.0f, 1.0f); // modulating
    private const float _sphEmitStrDef  = 2.1f;
    private const float _sphEmitStrLink = 5.8f;
    private       float _sphEmitStrTar  = _sphEmitStrDef; // lerp target
    private       float _sphEmitStr     = _sphEmitStrDef; // current actual emission strength
    private const float _sphScrSpeed    = 0.018f;
    private       float _hlMult         = 0.0f;
    private       float _hlSm           = 12.0f;

    public void InitializeSphere(int id, ref Godot.Rect2I[] rects, 
                                 ref int rSize, ref Godot.Vector2I vect) {
#if XBDEBUG
        var debug = new XB.DebugTimedBlock(XB.D.SphereInitializeSphere);
#endif

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
        _imgScrolling.Fill(XB.Col.Black);

        for (int i = 0; i < _repeats; i++) {
            int xStart = i*(_dimScrollX/_repeats);
            int width  = _dimDigitX + 2*_dimThick;
            if (ID > 9) { width += _dimDigitX; }
            var numberField = new Godot.Rect2I(xStart, _dimThick, width, _dimDigitY);
            _imgScrolling.FillRect(numberField, XB.Col.Black);

            if (ID > 9) { // decimal digit
                XB.Utils.DigitRectangles(ID/10, xStart+_dimThick, _dimThick, _dimDigitX, _dimDigitY, _dimThick,
                                         ref rects, ref rSize, ref vect                 );
                for (int j = 0; j < rSize; j++ ) { _imgScrolling.FillRect(rects[j], XB.Col.White); }
                xStart += _dimDigitX;
            }
            XB.Utils.DigitRectangles(ID%10, xStart+_dimThick, _dimThick, _dimDigitX, _dimDigitY, _dimThick,
                                     ref rects, ref rSize, ref vect                 );
            for (int j = 0; j < rSize; j++ ) { _imgScrolling.FillRect(rects[j], XB.Col.White); }
        }

        _texScrolling = new Godot.ImageTexture();
        _texScrolling.SetImage(_imgScrolling);
        _screenMat.SetShaderParameter("tWriting", _texScrolling);

        Hide();

#if XBDEBUG
        debug.End();
#endif 
    }

    public void UpdateSphere(float dt) {
#if XBDEBUG
        var debug = new XB.DebugTimedBlock(XB.D.SphereUpdateSphere);
#endif

        if (XB.Manager.Linking) { 
            if (LinkedTo) {
                _sphereColor = _sphereColor.Lerp(XB.Col.SpLink,   _hlSm*dt);
                Highlighted  = 1.0f;
            } else {
                _sphereColor = _sphereColor.Lerp(XB.Col.SpHlLink, _hlSm*dt);
            }
        } else {
            _sphereColor = _sphereColor.Lerp(XB.Col.SpHl, _hlSm*dt);
        }

        if (XB.Manager.LinkingID == ID) {
            _sphEmitStrTar = _sphEmitStrLink;
            _hlMult = XB.Utils.LerpF(_hlMult, 1.0f, _hlSm*dt);
            foreach (XB.Sphere lS in _linkedSpheres) { lS.LinkedTo = true; }
        } else {
            _sphEmitStrTar = _sphEmitStrDef;
        }

        _sphEmitStr = XB.Utils.LerpF(_sphEmitStr, _sphEmitStrTar, _hlSm*dt);
        _hlMult     = XB.Utils.LerpF(_hlMult, Highlighted, _hlSm*dt);
        Highlighted = 0.0f;
        LinkedTo    = false;
        _shellMat.SetShaderParameter ("emissionStr",   _sphEmitStr );
        _shellMat.SetShaderParameter ("highlightCol",  _sphereColor);
        _shellMat.SetShaderParameter ("highlightMult", _hlMult     );

#if XBDEBUG
        debug.End();
#endif 
    }

    // player places sphere in world
    public void PlaceSphere(Godot.Vector3 pos) {
#if XBDEBUG
        var debug = new XB.DebugTimedBlock(XB.D.SpherePlaceSphere);
#endif

        Show();
        GlobalPosition = pos;
        Active         = true;
        XB.Manager.UpdateActiveSpheres();
        TexSt = XB.SphereTexSt.Active;
        XB.PController.Hud.UpdateSphereTexture(ID, TexSt);

#if XBDEBUG
        debug.End();
#endif 
    }

    public void SphereTextureAddLinked() {
#if XBDEBUG
        var debug = new XB.DebugTimedBlock(XB.D.SphereSphereTextureAddLinked);
#endif

        TexSt = XB.SphereTexSt.ActiveLinking;
        XB.PController.Hud.UpdateSphereTexture(ID, TexSt);
        foreach (XB.Sphere lS in _linkedSpheres) {
            lS.TexSt = XB.SphereTexSt.ActiveLinked;
            XB.PController.Hud.UpdateSphereTexture(lS.ID, lS.TexSt);
        }    

#if XBDEBUG
        debug.End();
#endif 
    }

    public void SphereTextureRemoveLinked() {
#if XBDEBUG
        var debug = new XB.DebugTimedBlock(XB.D.SphereSphereTextureRemoveLinked);
#endif

        TexSt = XB.SphereTexSt.Active;
        XB.PController.Hud.UpdateSphereTexture(ID, TexSt);
        foreach (XB.Sphere lS in _linkedSpheres) {
            lS.TexSt = XB.SphereTexSt.Active;
            XB.PController.Hud.UpdateSphereTexture(lS.ID, lS.TexSt);
        }    

#if XBDEBUG
        debug.End();
#endif 
    }

    public void LinkSphere(int idLinkFrom) {
#if XBDEBUG
        var debug = new XB.DebugTimedBlock(XB.D.SphereLinkSphere);
#endif

        foreach (XB.Sphere lS in _linkedSpheres) {
            if (lS.ID == idLinkFrom) { return; }
        }
        _linkedSpheres.Add(XB.Manager.Spheres[idLinkFrom]);
        if (!Linked && _animPl.CurrentAnimation != "expand") { _animPl.Play("expand"); }
        Linked = true;

#if XBDEBUG
        debug.End();
#endif 
    }

    public void UnlinkSphere(XB.Sphere sphereUnlinkFrom) {
#if XBDEBUG
        var debug = new XB.DebugTimedBlock(XB.D.SphereUnlinkSphere);
#endif

        _linkedSpheres.Remove(sphereUnlinkFrom);

        if (_linkedSpheres.Count == 0) {
            if (Linked && _animPl.CurrentAnimation != "contract") { _animPl.Play("contract"); }
            Linked = false;
        }

#if XBDEBUG
        debug.End();
#endif 
    }

    public void UnlinkFromAllSpheres() {
#if XBDEBUG
        var debug = new XB.DebugTimedBlock(XB.D.SphereUnlinkFromAllSpheres);
#endif

        if (!Linked) { return; }

        foreach (XB.Sphere lS in _linkedSpheres) { lS.UnlinkSphere(this); }
        _linkedSpheres.Clear();

        if (_animPl.CurrentAnimation != "contract") { _animPl.Play("contract"); }
        Linked = false;

#if XBDEBUG
        debug.End();
#endif 
    }

    // remove sphere from world (does not remove from Manager Spheres array)
    public void RemoveSphere() {
#if XBDEBUG
        var debug = new XB.DebugTimedBlock(XB.D.SphereRemoveSphere);
#endif

        //TODO[ALEX]: remove sphere dam geometry
        UnlinkFromAllSpheres();
        _animPl.Play("expand");
        _animPl.Stop(); // stop animation at beginning of expand animation (contracted state)
        Hide();
        Active = false;
        XB.Manager.UpdateActiveSpheres();
        TexSt = XB.SphereTexSt.Inactive;
        XB.PController.Hud.UpdateSphereTexture(ID, TexSt);
        if (ID == XB.Manager.LinkingID) { XB.Manager.LinkingID = XB.Manager.MaxSphereAmount; }

#if XBDEBUG
        debug.End();
#endif 
    }

    // when sphere gets moved
    // move sphere by taking the current frames relation of the camera to the sphere and comparing
    // to the previous frame's, then calculating an offset vector and moving the sphere accordingly
    //NOTE[ALEX]: sphere movement gets slightly off when the player is moving into the terrain's
    //            edge aggressively, this limitation is acceptable for now
    public void MoveSphere(Godot.Transform3D camTrans, Godot.Transform3D camTransPrev,
                           Godot.PhysicsDirectSpaceState3D spaceState, Godot.Vector3 playerMovement) {
#if XBDEBUG
        var debug = new XB.DebugTimedBlock(XB.D.SphereMoveSphere);
#endif

        var   move     = new Godot.Vector3(0.0f, 0.0f, 0.0f);
        var   rayOrigT = camTrans.Origin; 
        var   rayOrigP = camTransPrev.Origin; 
        float rayDist  = XB.WorldData.WorldDim.X+XB.WorldData.WorldDim.Y;
        var   rayDestT = camTrans.Origin    -rayDist*camTrans.Basis.Z;
        var   rayDestP = camTransPrev.Origin-rayDist*camTransPrev.Basis.Z;
        var   resultT  = XB.Utils.Raycast(spaceState, rayOrigT, rayDestT, XB.LayerMasks.SphereMask);
        var   resultP  = XB.Utils.Raycast(spaceState, rayOrigP, rayDestP, XB.LayerMasks.SphereMask);

        if (resultT.Count > 0 && resultP.Count > 0) {
            var hitPosT   = (Godot.Vector3)resultT["position"];
            var hitPosP   = (Godot.Vector3)resultP["position"];

            // vertical movement
            var hitNrmT    = new Godot.Vector3(camTrans.Origin.X, 0.0f, camTrans.Origin.Z);
                hitNrmT.X -= hitPosT.X; // sphere to player this frame
                hitNrmT.Z -= hitPosT.Z;
            //NOTE[ALEX]: when using the previous frames hit vector in the following calculation, 
            //            the sphere jitters heavily, so using the same one is intentional
            //            for the previous frame's camera position the player's movement has
            //            to be compensated for
            var hitPrevG  = XB.Utils.IntersectRayPlaneV3(camTransPrev.Origin + playerMovement,
                                                         camTransPrev.Basis.Z, hitPosT, hitNrmT);
            var hitThisG  = XB.Utils.IntersectRayPlaneV3(camTrans.Origin, camTrans.Basis.Z,
                                                         hitPosT, hitNrmT                  );

            // horizontal movement
            var   toPosT      = hitPosT-camTrans.Origin;
            var   toPosP      = (hitPosP-camTrans.Origin).Normalized();
                  toPosP     *= toPosT.Length(); // previous position with same distance as current frame
            var   prevToThis  = toPosT-toPosP;

            // apply movement
            move.X = prevToThis.X + playerMovement.X;
            move.Y = hitThisG.Y-hitPrevG.Y;
            move.Z = prevToThis.Z + playerMovement.Z;
            GlobalPosition += move; // since the sphere does not have in-world collisions
        }

        //TODO[ALEX]: update cone geometry
        if (Linked) {
            //TODO[ALEX]: update dam geometry
        }

#if XBDEBUG
        debug.End();
#endif 
    }
}
} // namespace close
