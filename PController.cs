#define XBDEBUG
namespace XB { // namespace open
public enum AirSt {
    Grounded,   // player on the ground
    Rising,     // player going up
    Falling,    // player going down
    Floating,   // player floating in place
}

public enum JumpSt {
    Landed,     // player just landed
    OnGround,   // player has not jumped
    OneJumpS,   // player just jumped once
    OneJumpP,   // player jumped once and is in process (in air)
    TwoJumpS,   // player just jumped twice
    TwoJumpP,   // player jumped twice and is in process (in air)
}

public enum MoveSt {
    Walk,
    Run,
}

public partial class PController : Godot.CharacterBody3D {
    [Godot.Export] private Godot.NodePath       _cameraNode;
                   private Godot.Camera3D       _cam;                 // camera object
    [Godot.Export] private Godot.NodePath       _cameraRotationHNode; // camera's rot ctr horizontally
                   private Godot.Node3D         CCtrH;
    [Godot.Export] private Godot.NodePath       _cameraRotationVNode; // camera's rot ctr vertically
                   private Godot.Node3D         _cCtrV;
    [Godot.Export] private Godot.NodePath       _playerRiggedNode;
             public static Godot.Node3D         PModel;
    [Godot.Export] private Godot.NodePath       _animationTreeNode;
                   private Godot.AnimationTree  _pATree;
    [Godot.Export] private Godot.NodePath       _gunTipNode;
                   private Godot.Node3D         _gunTip;              // point from which bullets spawn
    [Godot.Export] private Godot.NodePath       _hudNode;
             public static XB.HUD               Hud;
    [Godot.Export] private Godot.NodePath       _menuNode;
             public static XB.Menu              Menu;
    [Godot.Export] private Godot.NodePath       _hairNode;
                   private Godot.BaseMaterial3D _hairMat;
    [Godot.Export] private Godot.NodePath       _lashNode;
                   private Godot.BaseMaterial3D _lashMat;
    [Godot.Export] private Godot.NodePath       _eyesNode;
                   private Godot.BaseMaterial3D _eyesMat;
    [Godot.Export] private Godot.NodePath       _bodyNode;
                   private Godot.BaseMaterial3D _bodyMat;
                   private Godot.BaseMaterial3D _headMat;
                   private float                _colSm      = 14.0f;
                   private Godot.Color          _colCurrent = new Godot.Color(1.0f, 1.0f, 1.0f, 1.0f);

    private       bool          _thirdP           = true;
    private       bool          _canShoot         = false;
    private const float         _respawnOff       = 0.5f;  // distance to ground when respawning
    private const float         _sphereSpawnDist  = 2.0f;  // distance to newly placed sphere in meter
    private       bool          _spawn            = false; // spawn player delayed for raycast to work
    private       Godot.Vector2 _spawnPos         = new Godot.Vector2(0.0f, 0.0f);
    private       int           _spawnAttempts    = 0;
    private const int           _spawnAttemptsMax = 20;

    // audio
    //NOTE[ALEX]: exporting the array directly does not consistently work in Godot 4.2.2 (bug)
    [Godot.Export] private Godot.AudioStreamPlayer3D   _audFootStep0;
    [Godot.Export] private Godot.AudioStreamPlayer3D   _audFootStep1;
    [Godot.Export] private Godot.AudioStreamPlayer3D   _audFootStep2;
    [Godot.Export] private Godot.AudioStreamPlayer3D   _audFootStep3;
    [Godot.Export] private Godot.AudioStreamPlayer3D   _audFootStep4;
    [Godot.Export] private Godot.AudioStreamPlayer3D   _audFootStep5;
                   private Godot.AudioStreamPlayer3D[] _audFootStep;
                   private int                         _audFootStepAmnt = 6;
                   private float                       _tFootStep       = 0.0f;

    private Godot.Vector2   _mouse    = new Godot.Vector2(0.0f, 0.0f); // mouse motion this tick

    private       XB.AirSt  _plA;               // player's air state
    public static XB.JumpSt PlJ;                // player's jump state
    public static bool      PlAiming  = false;  // is the player aiming
    private       bool      _plMoved  = false;  // player moved this frame
    private       float     _plYV     = 0.0f;   // player's velocity in y direction
    private       XB.MoveSt _move     = XB.MoveSt.Walk;
    private       float     _moveSpd  = 0.0f;   // current move speed
    private const float     _walkSpd  = -1.8f;  // speed for walking (-2.4)
    private const float     _runSpd   = -4.2f;  // speed for running (-4.2)
    private const float     _walkAnm  = 3.8f;   // animation speed multiplier (empirical)
    private const float     _jumpStr  = 5.0f;
    public static float     PlGrav    = 9.81f;
    private const float     _maxVertVelo = 15.0f;

    private float         _fov      = 0.0f;     // camera fov to lerp to
    private float         _cDist    = 0.0f;     // distance of camera from rotation center
    private const float   _cSmooth  = 28.0f;    // smoothing value for camera pos and fov lerping
    private const float   _cPushSpd = 12.0f;    // speed with which the camera pushes in
    private const float   _cPullSpd = 5.0f;     // speed with which the camera pulls back
    private const float   _maxVAng  = 80.0f;    // maximum vertical angle of camera

    private const float   _aimHOff  = 0.4f;     // horizontal camera offset while aiming
    private const float   _aimVOff  = 0.0f;     // vertical camera offset while aiming
    private Godot.Vector2 _aimOff   = new Godot.Vector2(0.0f, 0.0f); // camera offset this frame
    private const float   _aimEmpty = 20.0f;    // distance from gun to aim target with no rayhit

    private const float   _walkSm   = 10.0f;    // smoothing value for walk blend lerping
    private Godot.Vector2 _blWalk   = new Godot.Vector2(0.0f, 0.0f); // animation walking blend value
    private const float   _moveSm   = 15.0f;    // smoothing value for all move lerping
    private float         _blMove   = 0.0f;     // animation move blend value
    private float         _blIdle   = 0.0f;     // animation moveIdle blend value

    private Godot.Transform3D _camTransPrev = new Godot.Transform3D();
#if XBDEBUG
    private XB.DebugHUD _debugHud;
#endif

    public void InitializePController() {
#if XBDEBUG
        var debug = new XB.DebugTimedBlock(XB.D.PControllerInitializePController);
#endif

        ProcessMode    = ProcessModeEnum.Always;
        CollisionMask  = XB.LayerMasks.PlayerMask;
        CollisionLayer = XB.LayerMasks.PlayerLayer;

        _cam       = GetNode<Godot.Camera3D>        (_cameraNode);
        CCtrH      = GetNode<Godot.Node3D>          (_cameraRotationHNode);
        _cCtrV     = GetNode<Godot.Node3D>          (_cameraRotationVNode);
        _gunTip    = GetNode<Godot.Node3D>          (_gunTipNode);
        PModel     = GetNode<Godot.Node3D>          (_playerRiggedNode);
        _pATree    = GetNode<Godot.AnimationTree>   (_animationTreeNode);
        Hud        = (XB.HUD)GetNode<Godot.Control> (_hudNode);
        Menu       = (XB.Menu)GetNode<Godot.Control>(_menuNode);
        Menu.Hide();

        _thirdP = true;
        _cDist  = XB.AData.CamMaxDist;
        _spawn  = false;

        _hairMat = (Godot.BaseMaterial3D)GetNode<Godot.MeshInstance3D>
                       (_hairNode).GetSurfaceOverrideMaterial(0);
        _lashMat = (Godot.BaseMaterial3D)GetNode<Godot.MeshInstance3D>
                       (_lashNode).GetSurfaceOverrideMaterial(0);
        _eyesMat = (Godot.BaseMaterial3D)GetNode<Godot.MeshInstance3D>
                       (_eyesNode).GetSurfaceOverrideMaterial(0);
        _bodyMat = (Godot.BaseMaterial3D)GetNode<Godot.MeshInstance3D>
                       (_bodyNode).GetSurfaceOverrideMaterial(0);
        _headMat = (Godot.BaseMaterial3D)GetNode<Godot.MeshInstance3D>
                       (_bodyNode).GetSurfaceOverrideMaterial(1);

        _audFootStep    = new Godot.AudioStreamPlayer3D[_audFootStepAmnt];
        _audFootStep[0] = _audFootStep0;
        _audFootStep[1] = _audFootStep1;
        _audFootStep[2] = _audFootStep2;
        _audFootStep[3] = _audFootStep3;
        _audFootStep[4] = _audFootStep4;
        _audFootStep[5] = _audFootStep5;

#if XBDEBUG
        debug.End();
#endif 
    }

    public void InitializeHud() {
        Hud.InitializeHud();
    }

#if XBDEBUG
    public void InitializeDebugHud() {
        _debugHud = new XB.DebugHUD();
        AddChild(_debugHud);
        _debugHud.InitializeDebugHUD();
    }
#endif 

    // get input using godot's system, used for mouse movement input, 
    // general input is handled at beginning of _PhysicsProcess below
    public override void _Input(Godot.InputEvent @event) {
#if XBDEBUG
        var debug = new XB.DebugTimedBlock(XB.D.PController_Input);
#endif

        if (@event is not Godot.InputEventMouseMotion) return;

        var mouseM = (Godot.InputEventMouseMotion)@event;
        _mouse.X   = -0.015625f * mouseM.Relative.X; // multipliers = -30/1920|1080
        _mouse.Y   = -0.027777f * mouseM.Relative.Y;

#if XBDEBUG
        debug.End();
#endif 
    }

    // Called every frame at fixed time steps
    public override void _PhysicsProcess(double delta) {
#if XBDEBUG
        var debug = new XB.DebugTimedBlock(XB.D.PController_PhysicsProcess);
#endif

        // UPDATE GENERAL
        float dt = (float)delta;
        XB.AData.Input.GetInputs();
        Hud.UpdateHUD(dt);
        XB.ManagerSphere.UpdateSpheres(dt);
        // prioritize high detail terrain around player, not camera
        XB.ManagerTerrain.UpdateQTreeMeshes(new Godot.Vector2(PModel.GlobalPosition.X,
                                                              PModel.GlobalPosition.Z),
                                            XB.WorldData.LowestPoint, XB.WorldData.HighestPoint,
                                            ref XB.WorldData.ImgMiniMap                         );
        var spaceSt = RequestSpaceState(); // get spacestate for raycasting
#if XBDEBUG
         _debugHud.UpdateDebugHUD(dt);
#endif
        if (GetTree().Paused) {
#if XBDEBUG
             debug.End();
#endif
            return;
        }


        // SPAWNPLAYER
        if (_spawn) {
            SpawnPlayerDelayed();
        }


        // MOVEMENT
        // STEP 1: gravity and jumping
        if (IsOnFloor() && PlGrav > 0.0f) {
            if        (_plA != XB.AirSt.Grounded) { // landing frame
                PlJ  = XB.JumpSt.Landed;
                _plA = XB.AirSt.Grounded;
            } else if (_plA == XB.AirSt.Grounded) { // general state
                PlJ  = XB.JumpSt.OnGround;
            }
            _plYV = 0.0f; // reset vertical velocity when landing on the ground
            if (XB.AData.Input.FDown) {
                _plYV = -_jumpStr;
                PlJ   = XB.JumpSt.OneJumpS; // jumping start frame
                _plA  = XB.AirSt.Rising;
            }
        } else {
            if      (PlJ == XB.JumpSt.OneJumpS) PlJ = XB.JumpSt.OneJumpP;
            else if (PlJ == XB.JumpSt.TwoJumpS) PlJ = XB.JumpSt.TwoJumpP;
            if (Velocity.Y == 0.0f) _plYV = 0.0f;   // player hit ceiling

            if (XB.AData.Input.FDown && PlJ == XB.JumpSt.OneJumpP) { // double jump
                _plYV = -_jumpStr;
                PlJ   = XB.JumpSt.TwoJumpS;
            }

            _plYV += dt*PlGrav; // gravity
            _plYV  = XB.Utils.ClampF(_plYV, -_maxVertVelo, _maxVertVelo);
            
            if      (_plYV < 0) _plA = XB.AirSt.Rising;
            else if (_plYV > 0) _plA = XB.AirSt.Falling;
            else                _plA = XB.AirSt.Floating;
        }

        // STEP 2: check for player being outside of terrain area
        if        (GlobalPosition.Y < XB.WorldData.KillPlane || 
                   GlobalPosition.Y < (XB.WorldData.LowestPoint - XB.WorldData.LowHighExtra)) {
            Godot.GD.Print(">> out of bounds high/low");
            PlacePlayer(new Godot.Vector3(GlobalPosition.X, 
                                          XB.WorldData.HighestPoint + XB.WorldData.LowHighExtra,
                                          GlobalPosition.Z                                      ));
        } else if (GlobalPosition.X > 0.0f) {
            Godot.GD.Print(">> out of bounds X high");
            PlacePlayer(new Godot.Vector3(-_respawnOff,
                                          GlobalPosition.Y + XB.WorldData.LowHighExtra,
                                          GlobalPosition.Z                             ));
        } else if (GlobalPosition.X < -XB.WorldData.WorldDim.X) {
            Godot.GD.Print(">> out of bounds X low");
            PlacePlayer(new Godot.Vector3(-XB.WorldData.WorldDim.X + _respawnOff,
                                          GlobalPosition.Y + XB.WorldData.LowHighExtra,
                                          GlobalPosition.Z                             ));
        } else if (GlobalPosition.Z > 0.0f) {
            Godot.GD.Print(">> out of bounds Z high");
            PlacePlayer(new Godot.Vector3(GlobalPosition.X,
                                          GlobalPosition.Y + XB.WorldData.LowHighExtra,
                                          -_respawnOff                                 ));
        } else if (GlobalPosition.Z < -XB.WorldData.WorldDim.Y) {
            Godot.GD.Print(">> out of bounds Z low");
            PlacePlayer(new Godot.Vector3(GlobalPosition.X,
                                          GlobalPosition.Y + XB.WorldData.LowHighExtra,
                                          -XB.WorldData.WorldDim.Y + _respawnOff       ));
        }

        // STEP 3: horizontal movement relative to camera rotation
        switch (_move) {
            case XB.MoveSt.Walk: {
                _moveSpd = XB.Utils.LerpF(_moveSpd, _walkSpd, _moveSm*dt);
                if (XB.AData.Input.LIn) {
                    _move = XB.MoveSt.Run;
                }
                break;
            }
            case XB.MoveSt.Run: {
                _moveSpd = XB.Utils.LerpF(_moveSpd, _runSpd,  _moveSm*dt);
                if (XB.AData.Input.MoveX == 0.0f && XB.AData.Input.MoveY == 0.0f ||
                    PlJ != XB.JumpSt.OnGround || XB.AData.Input.LIn) {
                    _move = XB.MoveSt.Walk;
                }
                break;
            }
        }

        var v    = new Godot.Vector3(XB.AData.Input.MoveX, 0.0f, XB.AData.Input.MoveY);
            v    = v.Normalized()*_moveSpd;
        // horizontal player movement for use when moving spheres
        var spV  = new Godot.Vector3(0.0f, 0.0f, v.Z);
            v.Y  = -_plYV;
            v    = v.Rotated(CCtrH.Transform.Basis.Y, CCtrH.Transform.Basis.GetEuler().Y);
            spV  = spV.Rotated(CCtrH.Transform.Basis.Y, CCtrH.Transform.Basis.GetEuler().Y);
        var vRot = new Godot.Vector3(v.X, 0.0f, v.Z); // as reference when rotating the player

        if        (GlobalPosition.X > -_respawnOff) { // limit x movement
            v.X   = XB.Utils.MinF(v.X,   0.0f);
            spV.X = XB.Utils.MinF(spV.X, 0.0f);
        } else if (GlobalPosition.X < -(XB.WorldData.WorldDim.X-_respawnOff)) {
            v.X   = XB.Utils.MaxF(v.X,   0.0f);
            spV.X = XB.Utils.MaxF(spV.X, 0.0f);
        } 
        if        (GlobalPosition.Z > -_respawnOff) { // limit z movement
            v.Z   = XB.Utils.MinF(v.Z,   0.0f);
            spV.Z = XB.Utils.MinF(spV.Z, 0.0f);
        } else if (GlobalPosition.Z < -(XB.WorldData.WorldDim.Y-_respawnOff)) {
            v.Z   = XB.Utils.MaxF(v.Z,   0.0f);
            spV.Z = XB.Utils.MaxF(spV.Z, 0.0f);
        }

        // STEP 4: move using Godot's physics system
        if (v.Length() > 0.0f) _plMoved = true;
        else                   _plMoved = false; 
        Velocity = v;
        MoveAndSlide();

        // STEP 5: footstep sounds
        if ((XB.AData.Input.MoveX != 0.0f || XB.AData.Input.MoveY != 0.0f)
            && PlJ == XB.JumpSt.OnGround                                  ) {
            // align footstep timing with animation
            switch (_move) {
                case XB.MoveSt.Walk: {
                    _tFootStep += (dt*_walkAnm*0.5f);
                    break;
                }
                case XB.MoveSt.Run: {
                    _tFootStep += (dt*_walkAnm);
                    break;
                }
            }
        }
        if (_tFootStep > 1.0f) {
            int rand = XB.Random.RandomInRangeI(0, _audFootStepAmnt-1);
            _audFootStep[rand].Play();
            _tFootStep = 0.0f;
        }


        // CAMERA
        // STEP 1: aiming
        if (!_thirdP) {
            Hud.CrossVisible = true;
            PlAiming  = false;
            _aimOff.X = XB.Utils.LerpF(_aimOff.X, 0.0f, _cSmooth*dt);
            _aimOff.Y = XB.Utils.LerpF(_aimOff.Y, 0.0f, _cSmooth*dt);
        } else if (XB.AData.Input.SLBot) {
            Hud.CrossVisible = true;
            _move     = XB.MoveSt.Walk;
            PlAiming  = true;
            _aimOff.X = XB.Utils.LerpF(_aimOff.X, _aimHOff, _cSmooth*dt);
            _aimOff.Y = XB.Utils.LerpF(_aimOff.Y, _aimVOff, _cSmooth*dt);
        } else {
            Hud.CrossVisible = false;
            PlAiming  = false;
            _aimOff.X = XB.Utils.LerpF(_aimOff.X, 0.0f, _cSmooth*dt);
            _aimOff.Y = XB.Utils.LerpF(_aimOff.Y, 0.0f, _cSmooth*dt);
        }

        XB.AData.Input.CamX += _mouse.X;
        XB.AData.Input.CamY += _mouse.Y;
        _mouse.X = 0.0f;
        _mouse.Y = 0.0f;

        // STEP 2: horizontal camera movement
        float horAmount = dt*XB.AData.CamXSens*XB.AData.Input.CamX;
        CCtrH.RotateObjectLocal(CCtrH.Transform.Basis.Y, horAmount);

        // STEP 3: vertical camera movement
        var   rotCtr    = new Godot.Vector3(0.0f, 0.0f, 0.0f);
              rotCtr   += CCtrH.GlobalPosition;
              rotCtr   += _aimOff.X*_cam.GlobalTransform.Basis.X;
              rotCtr   += _aimOff.Y*_cam.GlobalTransform.Basis.Y;
        var   toCam     = _cam.GlobalTransform.Basis.Z;
        if (_thirdP) {
            toCam = _cam.GlobalPosition-rotCtr;
        }
        var   toCamG    = new Godot.Vector3(toCam.X, 0.0f, toCam.Z);
        float cAngle    = toCam.AngleTo(toCamG)*XB.Constants.Rad2Deg;
        float verAmount = dt*XB.AData.CamYSens*XB.AData.Input.CamY;
        if (cAngle > _maxVAng) {
            if      (toCam.Y > 0.0f && XB.AData.Input.CamY < 0.0f) verAmount = 0.0f;
            else if (toCam.Y < 0.0f && XB.AData.Input.CamY > 0.0f) verAmount = 0.0f;
        }
        _cCtrV.RotateObjectLocal(_cCtrV.Transform.Basis.X, verAmount);

        // STEP 4: check for camera collisions and move camera
        if (!_thirdP) {
            _cDist = XB.Utils.LerpF(_cDist, 0.0f, _cSmooth*dt);
        } else if (PlAiming) {
            _cDist = XB.Utils.LerpF(_cDist, XB.AData.CamAimDist, _cSmooth*dt);

            var rayOrig  = XB.Utils.IntersectRayPlaneV3
                             (_cam.GlobalPosition,  _cam.GlobalTransform.Basis.Z,
                              CCtrH.GlobalPosition, _cam.GlobalTransform.Basis.Z);
            var resultCA = XB.Utils.Raycast(spaceSt, rayOrig, _cam.GlobalPosition,
                                            XB.LayerMasks.AimMask);
            if (resultCA.Count > 0) {
                var camAimHit = (Godot.Vector3)resultCA["position"];
                var camAimOff = (_cam.GlobalPosition-camAimHit).Length();
                _cDist = XB.AData.CamAimDist-camAimOff;
            }
        } else {
            var resultC = XB.Utils.Raycast(spaceSt, CCtrH.GlobalPosition,
                                           CCtrH.GlobalPosition + 2.0f*toCam,
                                           XB.LayerMasks.CamMask);
            if (resultC.Count > 0) {
                var   toCollision = (Godot.Vector3)resultC["position"]-CCtrH.GlobalPosition;
                float newDist     = toCollision.Length() - XB.AData.CamCollDist;
                      newDist     = XB.Utils.ClampF(newDist, XB.AData.CamMinDist, XB.AData.CamMaxDist);
                _cDist = newDist;
            } else {
                _cDist = XB.Utils.LerpF(_cDist, XB.AData.CamMaxDist, _cSmooth*dt);
            }
        }
        _cam.Position = new Godot.Vector3(_aimOff.X, _aimOff.Y, _cDist);

        // STEP 5: rotating player
        if (PlAiming) {
            PModel.Rotation = new Godot.Vector3(0.0f, CCtrH.Rotation.Y+XB.Constants.Pi, 0.0f);
        } else if (_plMoved && _plA == XB.AirSt.Grounded) {
            float rot  = Godot.Vector3.Forward.AngleTo(vRot)+XB.Constants.Pi;
            if (Godot.Vector3.Forward.Cross(vRot).Y < 0) {
                rot = 2.0f*XB.Constants.Pi -rot;
            }
            PModel.Rotation = new Godot.Vector3(0.0f, rot, 0.0f);
        }


        // AIMING
        if (!_thirdP) {
            _canShoot = true;
            _fov = XB.AData.FovDef;
        } else if (PlAiming) {
            _canShoot = true;
            var camHit  = _cam.GlobalPosition - _cam.GlobalTransform.Basis.Z*1000.0f;
            var hitBox  = new Godot.StaticBody3D();
            var rayOrig = XB.Utils.IntersectRayPlaneV3
                            (_cam.GlobalPosition,    _cam.GlobalTransform.Basis.Z,
                             _gunTip.GlobalPosition, _cam.GlobalTransform.Basis.Z);
            // check from gun tip in direction of bullet
            var resultA = XB.Utils.Raycast(spaceSt, rayOrig, camHit, XB.LayerMasks.AimMask);
            if (resultA.Count > 0) {
                camHit = (Godot.Vector3)resultA["position"];
                hitBox = (Godot.StaticBody3D)resultA["collider"];
            } else {
                camHit = _cam.GlobalPosition - _cam.GlobalTransform.Basis.Z*_aimEmpty;
            }
            var gunDir = new Godot.Vector3(0.0f, 0.0f, 0.0f);
                gunDir = camHit-_gunTip.GlobalPosition;
            // check backwards for hits with geometry near to the player
            var resultN = XB.Utils.Raycast(spaceSt, camHit-0.1f*gunDir, 
                                           _cam.GlobalPosition+0.1f*gunDir, XB.LayerMasks.AimMask);
            if (resultN.Count > 0) {
                _canShoot = false;
            }
            _fov = XB.AData.FovAim;
        } else {
            _canShoot = false;
            _fov = XB.AData.FovDef;
        }

        var cAP                    = (Godot.CameraAttributesPhysical)_cam.Attributes;
            cAP.FrustumFocalLength = XB.Utils.LerpF(cAP.FrustumFocalLength, _fov, _cSmooth*dt);


        // SPHERE INTERACTIONS
        if (!_thirdP || PlAiming) {
            Godot.Vector3 rayOrigin = _cam.GlobalPosition;
            if (PlAiming) {
                rayOrigin  = XB.Utils.IntersectRayPlaneV3
                                 (_cam.GlobalPosition,  _cam.GlobalTransform.Basis.Z,
                                  CCtrH.GlobalPosition, _cam.GlobalTransform.Basis.Z);
            }
            float         rayDistance  = XB.WorldData.WorldDim.X+XB.WorldData.WorldDim.Y;
                          rayDistance *= -1.0f;
            Godot.Vector3 rayTarget    = rayOrigin + rayDistance*_cam.GlobalTransform.Basis.Z;
            var resultCS = XB.Utils.Raycast(spaceSt, rayOrigin, rayTarget, XB.LayerMasks.SphereMask);
            if (resultCS.Count > 0) {
                // Godot.GD.Print((Godot.Vector3)resultCS["position"]);
                XB.Sphere sphere = (XB.Sphere)resultCS["collider"];
                XB.ManagerSphere.ChangeHighlightSphere(sphere.ID);
            } else {
                XB.ManagerSphere.ChangeHighlightSphere(XB.ManagerSphere.MaxSphereAmount);
            }
        } else {
            XB.ManagerSphere.ChangeHighlightSphere(XB.ManagerSphere.MaxSphereAmount);
        }


        // ANIMATIONS
        float idleMode  = 0.0f;
        var   walkBlend = new Godot.Vector2(0.0f, 0.0f);  // movement animation in 2D blendspace
        float walkSpeed = 0.0f;
        if (_plA == XB.AirSt.Grounded) { // walking - on ground
            _blMove = XB.Utils.LerpF(_blMove, 0.0f, _moveSm*dt); // walk - 0.0f
            if (_plMoved) {
                idleMode = 1.0f; // walk
                if (PlAiming) {
                    walkBlend.X = XB.AData.Input.MoveX; // walk left/right
                    walkBlend.Y = XB.AData.Input.MoveY; // walk forwards/backwards
                } else { // condense 2D movement into single value representing amount
                    walkBlend.Y = new Godot.Vector2(XB.AData.Input.MoveX, XB.AData.Input.MoveY).Length();
                }
                walkBlend /= walkBlend.Length();
                walkSpeed  = walkBlend.Length()*_walkAnm;
                switch (_move) {
                    case XB.MoveSt.Walk: { 
                        walkBlend *= 0.5f; 
                        walkSpeed *= 0.5f;
                        break;
                    }
                    case XB.MoveSt.Run:  { 
                        walkBlend *= 1.0f; 
                        walkSpeed *= 1.0f;
                        break;
                    }
                }
            } else {
                if (!PlAiming) { idleMode = 0.0f; } // idle
            }
            _blIdle = XB.Utils.LerpF(_blIdle, idleMode, _moveSm*dt);
            _blWalk = XB.Utils.LerpV2(_blWalk, walkBlend, _walkSm*dt);
        } else { // jumping - in air
            _blMove = XB.Utils.LerpF(_blMove, 1.0f, _moveSm*dt); // jump - 1.0f
            if      (PlJ == XB.JumpSt.OneJumpS) {
                _pATree.Set("parameters/jumpTr/transition_request", "state_0");
            } else if (_plA == XB.AirSt.Rising || _plA == XB.AirSt.Falling) {
                _pATree.Set("parameters/jumpTr/transition_request", "state_1");
            } else if (PlJ == XB.JumpSt.Landed) {
                _pATree.Set("parameters/jumpTr/transition_request", "state_2");
            }
        }

        _pATree.Set("parameters/walkSpeed/scale",          walkSpeed);
        _pATree.Set("parameters/moveIdle/blend_amount",    _blIdle  );
        _pATree.Set("parameters/moveJump/blend_amount",    _blMove  );
        _pATree.Set("parameters/walkSpace/blend_position", _blWalk  );


        // PLAYER MATERIAL
        if (_thirdP) {
            if (_colCurrent.A > 0.98f) {
                _colCurrent.A = 1.0f; //NOTE[ALEX]: avoids visual artifacts at the end of fade in
            } else {
                _colCurrent.A = XB.Utils.LerpF(_colCurrent.A, 1.0f, _colSm*dt);
            }
        } else {
            _colCurrent.A = XB.Utils.LerpF(_colCurrent.A, 0.0f, _colSm*dt);
        }
        _hairMat.AlbedoColor = _colCurrent;
        _lashMat.AlbedoColor = _colCurrent;
        _eyesMat.AlbedoColor = _colCurrent;
        _bodyMat.AlbedoColor = _colCurrent;
        _headMat.AlbedoColor = _colCurrent;


        // INPUTS
        if        (XB.AData.Input.Start) { // system menu
            Menu.OpenMenu();
        } else if (XB.AData.Input.Select) { // toggle HUD visibility
            Hud.ToggleHUD();
        } else {
            // DUp
            if (XB.AData.Input.DDown) { // swap between first and third person view
                _thirdP = !_thirdP;
            }
            // DLeft
            // DRight
            if (_canShoot && XB.AData.Input.FUp) { // link
                if (   XB.ManagerSphere.Linking
                    && XB.ManagerSphere.HLSphereID < XB.ManagerSphere.MaxSphereAmount) {
                    XB.ManagerSphere.LinkSpheres();
                } else {
                    XB.ManagerSphere.UnsetLinkingID();
                }
            }
            // FDown - jump (handled earlier)
            if (XB.AData.Input.FLeft) { // unlink highlighted sphere
                if (   XB.ManagerSphere.Linking 
                    && XB.ManagerSphere.HLSphereID < XB.ManagerSphere.MaxSphereAmount) {
                    XB.ManagerSphere.UnlinkSpheres();
                }
            }
            if (XB.AData.Input.FRight) { // toggle linking
                XB.ManagerSphere.ToggleLinking();
            }
            if (_canShoot && XB.AData.Input.SLTop) { // place sphere
                var spawnPos =   CCtrH.GlobalPosition
                               + _cam.GlobalTransform.Basis.Z*-_sphereSpawnDist;
                if (!XB.ManagerSphere.RequestSphere(spawnPos)) {
                    Godot.GD.Print("all spheres used");
                }
            }
            // SLBot - aiming (handled earlier)
            if (_canShoot && XB.AData.Input.SRTop) { // remove sphere
                if (XB.ManagerSphere.HLSphereID < XB.ManagerSphere.MaxSphereAmount) {
                    XB.ManagerSphere.Spheres[XB.ManagerSphere.HLSphereID].RemoveSphere();
                }
            }
            if (_canShoot && XB.AData.Input.SRBot
                    && !XB.AData.Input.DLeft && !XB.AData.Input.DRight) {
                if (XB.ManagerSphere.HLSphereID < XB.ManagerSphere.MaxSphereAmount) {
                    XB.ManagerSphere.Spheres[XB.ManagerSphere.HLSphereID].MoveSphere
                        (_cam.GlobalTransform, _camTransPrev, spaceSt, spV*dt);
                }
            }
            //TODO[ALEX]: this should continue until let go of mouse, not when sphere is out of reticle
            if (_canShoot && XB.AData.Input.DLeft && XB.AData.Input.SRBot) {
                if (XB.ManagerSphere.HLSphereID < XB.ManagerSphere.MaxSphereAmount) {
                    XB.ManagerSphere.Spheres[XB.ManagerSphere.HLSphereID].ChangeSphereRadius
                        (XB.AData.Input.CamY*dt);
                }
            }
            if (_canShoot && XB.AData.Input.DRight && XB.AData.Input.SRBot) {
                if (XB.ManagerSphere.HLSphereID < XB.ManagerSphere.MaxSphereAmount) {
                    XB.ManagerSphere.Spheres[XB.ManagerSphere.HLSphereID].ChangeSphereAngle
                        (XB.AData.Input.CamY*dt);
                }
            }
        }

#if XBDEBUG
        // DEBUG BUTTONS
        if (XB.AData.Input.Debug1) {
            Godot.GD.Print("Debug1 - Toggle DebugHUD");
            _debugHud.ToggleDebugHUD();
        }
        if (XB.AData.Input.Debug2) {
            Godot.GD.Print("Debug2 - Toggle PauseDebug");
            _debugHud.TogglePauseDebug();
        }
        if (XB.AData.Input.Debug3) {
            SpawnPlayer(new Godot.Vector2(GlobalPosition.X, GlobalPosition.Z));
            Godot.GD.Print("Debug3");
        }
        if (XB.AData.Input.Debug4) {
            Godot.GD.Print("Debug4");
        }
        if (XB.AData.Input.Debug5) {
            Godot.GD.Print("Debug5");
        }
#endif


        // CLEANUP
        // to fix axes drifting apart due to imprecision:
        _cam.Transform  = _cam.Transform.Orthonormalized();
        CCtrH.Transform = CCtrH.Transform.Orthonormalized();
        _camTransPrev   = _cam.GlobalTransform;

#if XBDEBUG
        debug.End();
#endif 
    }

    //NOTE[ALEX]: if a raycast happens in the same frame that a mesh gets assigned,
    //            then that mesh will not be hit, 
    //            to avoid this, SpawnPlayer is called one frame delayed
    //
    //TODO
    //            occasionally the raycast just does not return anything at all and 
    //            respawning is not successful, the out of bounds check in STEP 2
    //            ensures those cases do not cause an irrecoverable state
    //            I have not found any issues with collider generation or the provided parameters
    //            in those cases so the issue appears to not be in my code
    //            for now I will accept this limitation
    public void SpawnPlayer(Godot.Vector2 spawnXZ) {
#if XBDEBUG
        var debug = new XB.DebugTimedBlock(XB.D.PControllerSpawnPlayer);
#endif

        Godot.GD.Print("spawnplayer " + spawnXZ);
        _spawn    = true;
        _spawnPos = spawnXZ;

#if XBDEBUG
        debug.End();
#endif 
    }

    private void SpawnPlayerDelayed() {
#if XBDEBUG
        var debug = new XB.DebugTimedBlock(XB.D.PControllerSpawnPlayerDelayed);
#endif

        float high = XB.WorldData.HighestPoint+XB.WorldData.LowHighExtra;
        float low  = XB.WorldData.LowestPoint -XB.WorldData.LowHighExtra;
        var spawnPoint  = new Godot.Vector3(_spawnPos.X,  high, _spawnPos.Y );
        var origin      = new Godot.Vector3(spawnPoint.X, high, spawnPoint.Z);
        var destination = new Godot.Vector3(spawnPoint.X, low,  spawnPoint.Z);
        var resultCD    = XB.Utils.Raycast(RequestSpaceState(), origin, destination,
                                           XB.LayerMasks.EnvironmentMask            );
        Godot.GD.Print("try spawn at: " + spawnPoint + ", o: " + origin + ", d: " + destination
                       + ", PlPos: " + GlobalPosition);
        if (resultCD.Count > 0) {
            spawnPoint      = (Godot.Vector3)resultCD["position"];
            spawnPoint.Y   += _respawnOff;
            _spawn          = false;
            _spawnAttempts  = 0;
            PlacePlayer(spawnPoint);
            Godot.GD.Print("spawn hit " + spawnPoint);
        } else if (_spawnAttempts >= _spawnAttemptsMax) {
            _spawn         = false;
            _spawnAttempts = 0;
            PlacePlayer(spawnPoint);
            Godot.GD.Print("spawn out of attempts at " + spawnPoint);
        } else {
            _spawnAttempts += 1;
            Godot.GD.Print("spawn no hit");
        }

#if XBDEBUG
        debug.End();
#endif 
    }

    private void PlacePlayer(Godot.Vector3 pos) {
#if XBDEBUG
        var debug = new XB.DebugTimedBlock(XB.D.PControllerPlacePlayer);
#endif

        GlobalPosition = pos;

#if XBDEBUG
        debug.End();
#endif 
    }

    public Godot.PhysicsDirectSpaceState3D RequestSpaceState() {
        return GetWorld3D().DirectSpaceState;
    }
}
} // namespace close
