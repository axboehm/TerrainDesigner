#define XBDEBUG
namespace XB { // namespace open
using SysCG = System.Collections.Generic;
public partial class Sphere : Godot.CharacterBody3D {
    [Godot.Export] private Godot.NodePath        _sphereMesh;
                   private Godot.ShaderMaterial  _shellMat;
                   private Godot.ShaderMaterial  _screenMat;
                   private Godot.MeshInstance3D  _meshInstCone;
    [Godot.Export] private Godot.AnimationPlayer _animPl;
                   private Godot.Image           _imgScrolling;
                   private Godot.ImageTexture    _texScrolling;

    private const int _repeats    = 8;   // how often the id gets repeated in the texture
    private const int _dimScrollX = 128; //NOTE[ALEX]: based on textures created for sphere
    private const int _dimScrollY = 16;
    private const int _dimThick   = 1;   // thickness of digits
    private const int _dimDigitX  = _dimScrollY/2 - 2*_dimThick;
    private const int _dimDigitY  = _dimScrollY   - 2*_dimThick;

    public        int   ID           = 0;
    public        bool  Active       = false;
    public        float Highlighted  = 0.0f;
    public        bool  Linked       = false;
    public        bool  LinkedTo     = false;
    public        float Radius       = 0.0f;
    private const float _radiusMult  = 40.0f;  // empirical
    private const float _radiusReset = 1.0f;
    public        float Angle        = 0.0f;   // in degrees
    private const float _angleMult   = 600.0f; // empirical
    private const float _angleReset  = 60.0f;
    private const float _angleMin    = 1.0f;
    private const float _angleMax    = 89.0f;

    public  XB.SphereTexSt TexSt = XB.SphereTexSt.Inactive;
    private SysCG.List<XB.Sphere> _linkedSpheres = new SysCG.List<XB.Sphere>();

    private Godot.Color _sphereColor    = new Godot.Color(0.0f, 0.0f, 0.0f, 1.0f); // modulating
    private const float _sphEmitStrDef  = 2.1f;
    private const float _sphEmitStrLink = 5.8f;
    private       float _sphEmitStrTar  = _sphEmitStrDef; // lerp target
    private       float _sphEmitStr     = _sphEmitStrDef; // current actual emission strength
    private const float _sphScrSpeed    = 0.018f;
    private       float _hlMult         = 0.0f;
    private const float _hlSm           = 12.0f;

    private Godot.Collections.Array _meshDataCone;
    private Godot.ArrayMesh         _arrMesh;
    private Godot.Vector3[]         _verticesCone;
    private Godot.Vector2[]         _uvsCone;
    private Godot.Vector3[]         _normalsCone;
    private int[]                   _trianglesCone;
    private const float             _edgeLength  = 64.0f;
    private const int               _circleSteps = 36; // subdivisions for the cone circle


    public void InitializeSphere(int id, ref Godot.Rect2I[] rects, 
                                 ref int rSize, ref Godot.Vector2I vect) {
#if XBDEBUG
        var debug = new XB.DebugTimedBlock(XB.D.SphereInitializeSphere);
#endif

        ID          = id;
        Active      = false;
        Highlighted = 0.0f;
        Linked      = false;
        LinkedTo    = false;
        Radius      = _radiusReset;
        Angle       = _angleReset;
        CollisionLayer = XB.LayerMasks.SphereLayer;

        _meshInstCone  = new Godot.MeshInstance3D();
        AddChild(_meshInstCone);
        _meshDataCone  = new Godot.Collections.Array();
        _meshDataCone.Resize((int)Godot.Mesh.ArrayType.Max);
        _arrMesh       = new Godot.ArrayMesh();
        _verticesCone  = new Godot.Vector3[1 + 3*(_circleSteps+1)];
        _uvsCone       = new Godot.Vector2[1 + 3*(_circleSteps+1)];
        _normalsCone   = new Godot.Vector3[1 + 3*(_circleSteps+1)];
        _trianglesCone = new int          [3*_circleSteps + 6*_circleSteps];

        // see UpdateConeMesh() for layout of cone mesh
        var v2 = new Godot.Vector2(0.0f, 0.0f);
        _uvsCone[0] = v2; // cone plateau center
        for (int i = 0; i <= _circleSteps; i++) {
            v2.X = i/(float)_circleSteps;
            v2.Y = 0.5f;
            _uvsCone[1 + 0*(_circleSteps+1) + i] = v2; // cone plateau outer edge
            _uvsCone[1 + 1*(_circleSteps+1) + i] = v2; // cone side plateau edge
            v2.Y = 1.0f;
            _uvsCone[1 + 2*(_circleSteps+1) + i] = v2; // cone side outer edge
        }

        int tri  = 0;
        for (int i = 0; i < _circleSteps; i++) { // cone plateau disc
            _trianglesCone[tri + 0] = 0;
            _trianglesCone[tri + 1] = 1 + ((i+1) % (_circleSteps+1));
            _trianglesCone[tri + 2] = 1 + i;
            tri += 3;
        }
        for (int i = 0; i < _circleSteps; i++) { // cone side
            _trianglesCone[tri + 0] = 1 + 1*(_circleSteps+1) + i;
            _trianglesCone[tri + 1] = 1 + 1*(_circleSteps+1) + ((i+1)%(_circleSteps+1));
            _trianglesCone[tri + 2] = 1 + 2*(_circleSteps+1) + i;
            _trianglesCone[tri + 3] = 1 + 1*(_circleSteps+1) + ((i+1)%(_circleSteps+1));
            _trianglesCone[tri + 4] = 1 + 2*(_circleSteps+1) + ((i+1)%(_circleSteps+1));
            _trianglesCone[tri + 5] = 1 + 2*(_circleSteps+1) + i;
            tri += 6;
        }

        _meshDataCone[(int)Godot.Mesh.ArrayType.TexUV] = _uvsCone;
        _meshDataCone[(int)Godot.Mesh.ArrayType.Index] = _trianglesCone;

        var v3 = new Godot.Vector3(0.0f, 1.0f, 0.0f);
        for (int i = 0; i <= _circleSteps+1; i++) { // normals for plateau
            _normalsCone[i] = v3;
        }

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
                XB.Utils.DigitRectangles(ID/10, xStart+_dimThick, _dimThick, _dimDigitX,
                                         _dimDigitY, _dimThick, ref rects, ref rSize, ref vect);
                for (int j = 0; j < rSize; j++ ) { _imgScrolling.FillRect(rects[j], XB.Col.White); }
                xStart += _dimDigitX;
            }
            XB.Utils.DigitRectangles(ID%10, xStart+_dimThick, _dimThick, _dimDigitX,
                                     _dimDigitY, _dimThick, ref rects, ref rSize, ref vect);
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

        if (XB.ManagerSphere.Linking) { 
            if (LinkedTo) {
                _sphereColor = _sphereColor.Lerp(XB.Col.SpLink,   _hlSm*dt);
                Highlighted  = 1.0f;
            } else {
                _sphereColor = _sphereColor.Lerp(XB.Col.SpHlLink, _hlSm*dt);
            }
        } else {
            _sphereColor = _sphereColor.Lerp(XB.Col.SpHl, _hlSm*dt);
        }

        if (XB.ManagerSphere.LinkingID == ID) {
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
        XB.ManagerSphere.UpdateActiveSpheres();
        TexSt = XB.SphereTexSt.Active;
        XB.PController.Hud.UpdateSphereTexture(ID, TexSt);
        UpdateConeMesh();

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
        _linkedSpheres.Add(XB.ManagerSphere.Spheres[idLinkFrom]);
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

        //TODO[ALEX]: remove sphere dam and cone geometry
        UnlinkFromAllSpheres();
        _animPl.Play("expand");
        _animPl.Stop(); // stop animation at beginning of expand animation (contracted state)
        Hide();
        Active = false;
        XB.ManagerSphere.UpdateActiveSpheres();
        TexSt = XB.SphereTexSt.Inactive;
        XB.PController.Hud.UpdateSphereTexture(ID, TexSt);
        if (ID == XB.ManagerSphere.LinkingID) { 
            XB.ManagerSphere.LinkingID = XB.ManagerSphere.MaxSphereAmount; 
        }
        Radius = _radiusReset;
        Angle  = _angleReset;

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

        UpdateConeMesh();
        if (Linked) {
            //TODO[ALEX]: update dam geometry
        }

#if XBDEBUG
        debug.End();
#endif 
    }

    public void ChangeSphereRadius(float amount) {
#if XBDEBUG
        var debug = new XB.DebugTimedBlock(XB.D.SphereChangeSphereRadius);
#endif

        Radius += amount*_radiusMult; // mouse down will reduce radius
        Radius  = XB.Utils.MaxF(0.0f, Radius);
        UpdateConeMesh();

#if XBDEBUG
        debug.End();
#endif 
    }

    public void ChangeSphereAngle(float amount) {
#if XBDEBUG
        var debug = new XB.DebugTimedBlock(XB.D.SphereChangeSphereAngle);
#endif

        Angle -= amount*_angleMult; // mouse down will "push down" angle toward 90 deg
        Angle  = XB.Utils.ClampF(Angle, _angleMin, _angleMax);
        UpdateConeMesh();

#if XBDEBUG
        debug.End();
#endif 
    }

    // the cone mesh for each point consists of a disc platform at the top and the "cylinder" side
    // the vertices are laid out from the center outwards in rings, see the following segment diagram
    // n is equal to the number of divisions along the circle of the cone
    // the mesh then has 3 triangles from the center going outwards for each division segment
    //
    //    / |     ..    |
    //      n|n+n+1 -- 2n+n+2   // double up vertices on seam
    //   /- 1|n+1+1 -- 2n+1+2
    //  / A |    C/D    |
    // 0 -- 2|n+2+1 -- 2n+2+2
    //  \ B |    E/F    |
    //   \- 3|n+3+1 -- 2n+3+2
    //    \ |     ..    |
    //
    // at 0/360 deg the vertex gets doubled up to guarantee continuous UVs
    //
    private void UpdateConeMesh() {
#if XBDEBUG
        var debug = new XB.DebugTimedBlock(XB.D.SphereUpdateConeMesh);
#endif

        _verticesCone[0] = new Godot.Vector3(0.0f, 0.0f, 0.0f);
        var dir    = new Godot.Vector3(Radius, 0.0f, 0.0f);
        var dirAng = dir.Rotated(Godot.Vector3.Forward, Angle*XB.Constants.Deg2Rad);
        var nrmAng = Godot.Vector3.Up.Rotated(Godot.Vector3.Forward, Angle*XB.Constants.Deg2Rad);
        for (int i = 0; i <= _circleSteps; i++) {
            float rotAmnt = (i/(float)_circleSteps)*XB.Constants.Tau;
            _verticesCone[1 + 0*(_circleSteps+1) + i] = _verticesCone[0]
                +dir.Rotated(Godot.Vector3.Up, rotAmnt);
            _verticesCone[1 + 1*(_circleSteps+1) + i] = _verticesCone[1 + 0*(_circleSteps+1) + i];
            _normalsCone [1 + 1*(_circleSteps+1) + i] = nrmAng.Rotated(Godot.Vector3.Up, rotAmnt);
            _verticesCone[1 + 2*(_circleSteps+1) + i] = _verticesCone[1 + 0*(_circleSteps+1) + i] 
                + _edgeLength*dirAng.Rotated(Godot.Vector3.Up, rotAmnt);
            _normalsCone [1 + 2*(_circleSteps+1) + i] = nrmAng.Rotated(Godot.Vector3.Up, rotAmnt);
        }

        _meshDataCone[(int)Godot.Mesh.ArrayType.Vertex] = _verticesCone;
        _meshDataCone[(int)Godot.Mesh.ArrayType.Normal] = _normalsCone;

        _arrMesh.ClearSurfaces();
        _arrMesh.AddSurfaceFromArrays(Godot.Mesh.PrimitiveType.Triangles, _meshDataCone);
        
        _meshInstCone.Mesh = _arrMesh;
        //TODO[ALEX]: cone material
        //_meshInstCone.Mesh.SurfaceSetMaterial(0, xx);

#if XBDEBUG
        debug.End();
#endif 
    }
}
} // namespace close
