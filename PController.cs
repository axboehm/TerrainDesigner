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
             public static Godot.Node3D         CCtrH;
    [Godot.Export] private Godot.NodePath       _cameraRotationVNode; // camera's rot ctr vertically
                   private Godot.Node3D         _cCtrV;
    [Godot.Export] private Godot.NodePath       _playerRiggedNode;
             public static Godot.Node3D         PModel;
    [Godot.Export] private Godot.NodePath       _animationTreeNode;
                   private Godot.AnimationTree  _pATree;
    [Godot.Export] private Godot.NodePath       _gunTipNode;
                   private Godot.Node3D         _gunTip;    // reference point for camera ray casting
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
    private const float         _respawnOff       = 0.1f;  // distance to ground when respawning
    private const float         _sphereSpawnDist  = 6.0f;  // distance to newly placed sphere in meter
    private       bool          _spawn            = false; // spawn player delayed for raycast to work
    private       Godot.Vector2 _spawnPos         = new Godot.Vector2(0.0f, 0.0f);
    private       int           _spawnAttempts    = 0;
    private const int           _spawnAttemptsMax = 20;

    private Godot.AudioStreamPlayer3D[] _audFootStep;
    private const int                   _audFootStepAmnt = 6;
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

    private float       _camZoom     = 0.0f;
    private const float _camZoomSens = 0.5f;
    private const float _camZoomMax  = 16.0f;
    private float       _fov         = 0.0f;     // camera fov to lerp to
    private float       _cDist       = 0.0f;     // distance of camera from rotation center
    private const float _camCollDist = 0.2f;     // min distance from camera to colliders
    private const float _cSmooth     = 28.0f;    // smoothing value for camera pos and fov lerping
    private const float _cPushSpd    = 12.0f;    // speed with which the camera pushes in
    private const float _cPullSpd    = 5.0f;     // speed with which the camera pulls back
    private const float _maxVAng     = 80.0f;    // maximum vertical angle of camera

    private Godot.Vector3 _rotCtr   = new Godot.Vector3(0.0f, 0.0f, 0.0f);
    private Godot.Vector3 _toCam    = new Godot.Vector3(0.0f, 0.0f, 0.0f);
    private Godot.Vector3 _toCamG   = new Godot.Vector3(0.0f, 0.0f, 0.0f);
    private const float   _aimHOff  = 0.4f;     // horizontal camera offset while aiming
    private const float   _aimVOff  = 0.0f;     // vertical camera offset while aiming
    private Godot.Vector2 _aimOff   = new Godot.Vector2(0.0f, 0.0f); // camera offset this frame
    private const float   _aimEmpty = 20.0f;    // distance from gun to aim target with no rayhit

    private Godot.Vector2 _walkBlend = new Godot.Vector2(0.0f, 0.0f); // move animation in 2D blendspace
    private const float   _walkSm    = 10.0f;    // smoothing value for walk blend lerping
    private Godot.Vector2 _blWalk    = new Godot.Vector2(0.0f, 0.0f); // animation walking blend value
    private const float   _moveSm    = 15.0f;    // smoothing value for all move lerping
    private float         _blMove    = 0.0f;     // animation move blend value
    private float         _blIdle    = 0.0f;     // animation moveIdle blend value

    private Godot.PhysicsDirectSpaceState3D _spaceSt;
    private Godot.Vector3     _v            = new Godot.Vector3(0.0f, 0.0f, 0.0f);
    private Godot.Vector3     _spV          = new Godot.Vector3(0.0f, 0.0f, 0.0f);
    private Godot.Vector3     _vRot         = new Godot.Vector3(0.0f, 0.0f, 0.0f);
    private Godot.Vector3     _cOrig        = new Godot.Vector3(0.0f, 0.0f, 0.0f);
    private Godot.Vector3     _cDir         = new Godot.Vector3(0.0f, 0.0f, 0.0f);
    private Godot.Vector3     _rOrig        = new Godot.Vector3(0.0f, 0.0f, 0.0f);
    private Godot.Vector3     _rDest        = new Godot.Vector3(0.0f, 0.0f, 0.0f);
    private Godot.Vector3     _pOrig        = new Godot.Vector3(0.0f, 0.0f, 0.0f);
    private Godot.Vector3     _pNrm         = new Godot.Vector3(0.0f, 0.0f, 0.0f);
    private Godot.Vector3     _camAimHit    = new Godot.Vector3(0.0f, 0.0f, 0.0f);
    private Godot.Vector3     _camHit       = new Godot.Vector3(0.0f, 0.0f, 0.0f);
    private Godot.Vector3     _toCollision  = new Godot.Vector3(0.0f, 0.0f, 0.0f);
    private Godot.Vector3     _camNewPos    = new Godot.Vector3(0.0f, 0.0f, 0.0f);
    private Godot.Vector3     _plNewRot     = new Godot.Vector3(0.0f, 0.0f, 0.0f);
    private Godot.Vector3     _gunDir       = new Godot.Vector3(0.0f, 0.0f, 0.0f);
    private Godot.Vector2     _pModelPos    = new Godot.Vector2(0.0f, 0.0f);
    private Godot.Transform3D _camTransThis = new Godot.Transform3D(); // for moving spheres
    private Godot.Transform3D _camTransPrev = new Godot.Transform3D(); // for moving spheres
    private Godot.Collections.Dictionary _resultRC = new Godot.Collections.Dictionary();
    private Godot.CameraAttributesPhysical _cAP;
    private XB.Sphere _sphere;
#if XBDEBUG
    public XB.DebugHUD DebugHud;
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

        _thirdP  = true;
        _cDist   = XB.AData.S.SC.CamMaxDist;
        _camZoom = 0.0f;
        _spawn   = false;

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
        var footStepScn = Godot.ResourceLoader.Load<Godot.PackedScene>(XB.ResourcePaths.FootStep01);
        //NOTE[ALEX]: somewhat hard coded footstep array initialization
        //            but will populate the array to any length
        for (int i = 0; i < _audFootStepAmnt; i++) {
            switch (i) {
                case 1: {
                    footStepScn = Godot.ResourceLoader.Load<Godot.PackedScene>
                        (XB.ResourcePaths.FootStep02);
                    break;
                }
                case 2: {
                    footStepScn = Godot.ResourceLoader.Load<Godot.PackedScene>
                        (XB.ResourcePaths.FootStep03);
                    break;
                }
                case 3: {
                    footStepScn = Godot.ResourceLoader.Load<Godot.PackedScene>
                        (XB.ResourcePaths.FootStep04);
                    break;
                }
                case 4: {
                    footStepScn = Godot.ResourceLoader.Load<Godot.PackedScene>
                        (XB.ResourcePaths.FootStep05);
                    break;
                }
                case 5: {
                    footStepScn = Godot.ResourceLoader.Load<Godot.PackedScene>
                        (XB.ResourcePaths.FootStep06);
                    break;
                }
                default: {
                    footStepScn = Godot.ResourceLoader.Load<Godot.PackedScene>
                        (XB.ResourcePaths.FootStep01);
                    break;
                }
            }

            _audFootStep[i] = (Godot.AudioStreamPlayer3D)footStepScn.Instantiate();
            AddChild(_audFootStep[i]);
        }

#if XBDEBUG
        debug.End();
#endif 
    }

    public void InitializeHud() {
        Hud.InitializeHud();
    }

    public void InitializeMenu() {
        Menu.InitializeMenu();
    }

#if XBDEBUG
    public void InitializeDebugHud() {
        DebugHud = new XB.DebugHUD();
        AddChild(DebugHud);
        DebugHud.InitializeDebugHUD();
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
        _pModelPos.X = PModel.GlobalPosition.X;
        _pModelPos.Y = PModel.GlobalPosition.Z;
        XB.ManagerTerrain.UpdateQTreeMeshes(ref _pModelPos, XB.WData.LowestPoint, 
                                            XB.WData.HighestPoint, ref XB.WData.ImgMiniMap);
        RequestSpaceState(ref _spaceSt); // get spacestate for raycasting

#if XBDEBUG
         DebugHud.UpdateDebugHUD(dt);
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
        if        (GlobalPosition.Y < XB.WData.KillPlane || 
                   GlobalPosition.Y < (XB.WData.LowestPoint - XB.WData.LowHighExtra)) {
            // Godot.GD.Print(">> out of bounds high/low");
            PlacePlayer(new Godot.Vector3(GlobalPosition.X, 
                                          XB.WData.HighestPoint + XB.WData.LowHighExtra,
                                          GlobalPosition.Z                              ));
        } else if (GlobalPosition.X > 0.0f) {
            // Godot.GD.Print(">> out of bounds X high");
            PlacePlayer(new Godot.Vector3(-_respawnOff,
                                          GlobalPosition.Y + XB.WData.LowHighExtra,
                                          GlobalPosition.Z                         ));
        } else if (GlobalPosition.X < -XB.WData.WorldDim.X) {
            // Godot.GD.Print(">> out of bounds X low");
            PlacePlayer(new Godot.Vector3(-XB.WData.WorldDim.X + _respawnOff,
                                          GlobalPosition.Y + XB.WData.LowHighExtra,
                                          GlobalPosition.Z                         ));
        } else if (GlobalPosition.Z > 0.0f) {
            // Godot.GD.Print(">> out of bounds Z high");
            PlacePlayer(new Godot.Vector3(GlobalPosition.X,
                                          GlobalPosition.Y + XB.WData.LowHighExtra,
                                          -_respawnOff                             ));
        } else if (GlobalPosition.Z < -XB.WData.WorldDim.Y) {
            // Godot.GD.Print(">> out of bounds Z low");
            PlacePlayer(new Godot.Vector3(GlobalPosition.X,
                                          GlobalPosition.Y + XB.WData.LowHighExtra,
                                          -XB.WData.WorldDim.Y + _respawnOff       ));
        }

        // STEP 3: horizontal movement relative to camera rotation
        switch (_move) {
            case XB.MoveSt.Walk: {
                _moveSpd = XB.Utils.LerpF(_moveSpd, _walkSpd, _moveSm*dt);
                if (XB.AData.Input.DUp) {
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

        _v.X = XB.AData.Input.MoveX;
        _v.Y = 0.0f; //NOTE[ALEX]: technically not necessary
        _v.Z = XB.AData.Input.MoveY;
        _v   = _v.Normalized()*_moveSpd;
        _spV.X = 0.0f;
        _spV.Y = 0.0f;
        _spV.Z = _v.Z; // horizontal player movement for use when moving spheres
        _v.Y = -_plYV;
        _v   = _v.Rotated(CCtrH.Transform.Basis.Y, CCtrH.Transform.Basis.GetEuler().Y);
        _spV = _spV.Rotated(CCtrH.Transform.Basis.Y, CCtrH.Transform.Basis.GetEuler().Y);
        _vRot.X = _v.X; // as reference when rotating the player
        _vRot.Y = 0.0f;
        _vRot.Z = _v.Z;

        if        (GlobalPosition.X > -_respawnOff) { // limit x movement
            _v.X   = XB.Utils.MinF(_v.X,   0.0f);
            _spV.X = XB.Utils.MinF(_spV.X, 0.0f);
        } else if (GlobalPosition.X < -(XB.WData.WorldDim.X-_respawnOff)) {
            _v.X   = XB.Utils.MaxF(_v.X,   0.0f);
            _spV.X = XB.Utils.MaxF(_spV.X, 0.0f);
        } 
        if        (GlobalPosition.Z > -_respawnOff) { // limit z movement
            _v.Z   = XB.Utils.MinF(_v.Z,   0.0f);
            _spV.Z = XB.Utils.MinF(_spV.Z, 0.0f);
        } else if (GlobalPosition.Z < -(XB.WData.WorldDim.Y-_respawnOff)) {
            _v.Z   = XB.Utils.MaxF(_v.Z,   0.0f);
            _spV.Z = XB.Utils.MaxF(_spV.Z, 0.0f);
        }

        // STEP 4: move using Godot's physics system
        if (_v.Length() > 0.0f) { _plMoved = true;  }
        else                    { _plMoved = false; }
        Velocity = _v;
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
        float horAmount = dt*XB.AData.S.SC.CamXSens*XB.AData.Input.CamX;
        CCtrH.RotateObjectLocal(CCtrH.Transform.Basis.Y, horAmount);

        // STEP 3: vertical camera movement
        _rotCtr  = CCtrH.GlobalPosition;
        _rotCtr += _aimOff.X*_cam.GlobalTransform.Basis.X;
        _rotCtr += _aimOff.Y*_cam.GlobalTransform.Basis.Y;
        _toCam   = _cam.GlobalTransform.Basis.Z;
        if (_thirdP) {
            _toCam = _cam.GlobalPosition-_rotCtr;
        }
        _toCamG   = _toCam;
        _toCamG.Y = 0.0f;
        float cAngle    = _toCam.AngleTo(_toCamG)*XB.Constants.Rad2Deg;
        float verAmount = dt*XB.AData.S.SC.CamYSens*XB.AData.Input.CamY;
        if (cAngle > _maxVAng) {
            if      (_toCam.Y > 0.0f && XB.AData.Input.CamY < 0.0f) verAmount = 0.0f;
            else if (_toCam.Y < 0.0f && XB.AData.Input.CamY > 0.0f) verAmount = 0.0f;
        }
        _cCtrV.RotateObjectLocal(_cCtrV.Transform.Basis.X, verAmount);

        // STEP 4: check for camera collisions and move camera
        if (!_thirdP) {
            _cDist = XB.Utils.LerpF(_cDist, 0.0f, _cSmooth*dt);
        } else if (PlAiming) {
            _cDist = XB.Utils.LerpF(_cDist, XB.AData.S.SC.CamAimDist, _cSmooth*dt);

            _cOrig = _cam.GlobalPosition;
            _cDir  = _cam.GlobalTransform.Basis.Z;
            _pOrig = CCtrH.GlobalPosition;
            _pNrm  = _cam.GlobalTransform.Basis.Z;
            XB.Utils.IntersectRayPlaneV3(ref _cOrig, ref _cDir, ref _pOrig, ref _pNrm, ref _rOrig);
            _rDest = _cam.GlobalPosition;
            XB.Utils.Raycast(ref _spaceSt, ref _rOrig, ref _rDest, XB.LayerMasks.AimMask,
                             ref _resultRC                                               );
            if (_resultRC.Count > 0) {
                _camAimHit = (Godot.Vector3)_resultRC["position"];
                float camAimOff = (_cam.GlobalPosition - _camAimHit).Length();
                _cDist = XB.AData.S.SC.CamAimDist - camAimOff;
            }
        } else {
            _rOrig = CCtrH.GlobalPosition;
            _rDest = CCtrH.GlobalPosition + 2.0f*_toCam;
            XB.Utils.Raycast(ref _spaceSt, ref _rOrig, ref _rDest, XB.LayerMasks.CamMask,
                             ref _resultRC                                               );
            if (_resultRC.Count > 0) {
                _toCollision  = (Godot.Vector3)_resultRC["position"]-CCtrH.GlobalPosition;
                float newDist = _toCollision.Length() - _camCollDist;
                      newDist = XB.Utils.ClampF(newDist, XB.Settings.CamMinDist,
                                                XB.AData.S.SC.CamMaxDist+_camZoom);
                _cDist = newDist;
            } else {
                _cDist = XB.Utils.LerpF(_cDist, XB.AData.S.SC.CamMaxDist+_camZoom, _cSmooth*dt);
            }
        }
        _camNewPos.X = _aimOff.X;
        _camNewPos.Y = _aimOff.Y;
        _camNewPos.Z = _cDist;
        _cam.Position = _camNewPos;

        // STEP 5: rotating player
        if (PlAiming) {
            _plNewRot.Y = CCtrH.Rotation.Y + XB.Constants.Pi;
        } else if (_plMoved && _plA == XB.AirSt.Grounded) {
            float rot = Godot.Vector3.Forward.AngleTo(_vRot)+XB.Constants.Pi;
            if (Godot.Vector3.Forward.Cross(_vRot).Y < 0) {
                rot = 2.0f*XB.Constants.Pi -rot;
            }
            _plNewRot.Y = rot;
        }
        PModel.Rotation = _plNewRot; // x and y are never changed from 0.0f


        // AIMING
        if (!_thirdP) {
            _canShoot = true;
            _fov = XB.AData.S.SC.FovDef;
        } else if (PlAiming) {
            _canShoot = true;
            _camHit = _cam.GlobalPosition - _cam.GlobalTransform.Basis.Z*1000.0f;
            _cOrig  = _cam.GlobalPosition;
            _cDir   = _cam.GlobalTransform.Basis.Z;
            _pOrig  = _gunTip.GlobalPosition;
            _pNrm   = _cam.GlobalTransform.Basis.Z;
            XB.Utils.IntersectRayPlaneV3(ref _cOrig, ref _cDir, ref _pOrig, ref _pNrm, ref _rOrig);
            // check from gun tip in direction of view
            XB.Utils.Raycast(ref _spaceSt, ref _rOrig, ref _camHit, XB.LayerMasks.AimMask,
                             ref _resultRC                                                );
            if (_resultRC.Count > 0) {
                _camHit = (Godot.Vector3)_resultRC["position"];
            } else {
                _camHit = _cam.GlobalPosition - _cam.GlobalTransform.Basis.Z*_aimEmpty;
            }
            _gunDir = _camHit-_gunTip.GlobalPosition;
            // check backwards for hits with geometry near to the player
            _rOrig = _camHit             - 0.1f*_gunDir;
            _rDest = _cam.GlobalPosition + 0.1f*_gunDir;
            XB.Utils.Raycast(ref _spaceSt, ref _rOrig, ref _rDest, XB.LayerMasks.AimMask,
                             ref _resultRC                                               );
            if (_resultRC.Count > 0) { _canShoot = false; }
            _fov = XB.AData.S.SC.FovAim;
        } else {
            _canShoot = false;
            _fov = XB.AData.S.SC.FovDef;
        }

        _cAP                    = (Godot.CameraAttributesPhysical)_cam.Attributes;
        _cAP.FrustumFocalLength = XB.Utils.LerpF(_cAP.FrustumFocalLength, _fov, _cSmooth*dt);


        // SPHERE INTERACTIONS
        if (!_thirdP || PlAiming) {
            _rOrig = _cam.GlobalPosition;
            if (PlAiming) {
                _cOrig = _cam.GlobalPosition;
                _cDir  = _cam.GlobalTransform.Basis.Z;
                _pOrig = CCtrH.GlobalPosition;
                _pNrm  = _cam.GlobalTransform.Basis.Z;
                XB.Utils.IntersectRayPlaneV3(ref _cOrig, ref _cDir, ref _pOrig, ref _pNrm, ref _rOrig);
            }
            float rayDistance  = XB.WData.WorldDim.X+XB.WData.WorldDim.Y;
                  rayDistance *= -1.0f;
            _rDest = _rOrig + rayDistance*_cam.GlobalTransform.Basis.Z;
            XB.Utils.Raycast(ref _spaceSt, ref _rOrig, ref _rDest, XB.LayerMasks.SphereMask,
                             ref _resultRC                                                  );
            if (_resultRC.Count > 0) {
                _sphere = (XB.Sphere)_resultRC["collider"];
                XB.ManagerSphere.ChangeHighlightSphere(_sphere.ID);
            } else {
                XB.ManagerSphere.ChangeHighlightSphere(XB.ManagerSphere.MaxSphereAmount);
            }
        } else {
            XB.ManagerSphere.ChangeHighlightSphere(XB.ManagerSphere.MaxSphereAmount);
        }


        // ANIMATIONS
        float idleMode  = 0.0f;
        float walkSpeed = 0.0f;
        _walkBlend.X = 0.0f;
        _walkBlend.Y = 0.0f;
        if (_plA == XB.AirSt.Grounded) { // walking - on ground
            _blMove = XB.Utils.LerpF(_blMove, 0.0f, _moveSm*dt); // walk - 0.0f
            if (_plMoved) {
                idleMode = 1.0f; // walk
                if (PlAiming) {
                    _walkBlend.X = XB.AData.Input.MoveX; // walk left/right
                    _walkBlend.Y = XB.AData.Input.MoveY; // walk forwards/backwards
                } else { // condense 2D movement into single value representing amount
                    _walkBlend.Y = new Godot.Vector2(XB.AData.Input.MoveX, XB.AData.Input.MoveY).Length();
                }
                _walkBlend /= _walkBlend.Length();
                walkSpeed   = _walkBlend.Length()*_walkAnm;
                switch (_move) {
                    case XB.MoveSt.Walk: { 
                        _walkBlend *= 0.5f; 
                        walkSpeed  *= 0.5f;
                        break;
                    }
                    case XB.MoveSt.Run:  { 
                        _walkBlend *= 1.0f; 
                        walkSpeed  *= 1.0f;
                        break;
                    }
                }
            } else {
                if (!PlAiming) { idleMode = 0.0f; } // idle
            }
            _blIdle = XB.Utils.LerpF(_blIdle, idleMode, _moveSm*dt);
            _blWalk = XB.Utils.LerpV2(_blWalk, _walkBlend, _walkSm*dt);
        } else { // jumping - in air
            _blMove = XB.Utils.LerpF(_blMove, 1.0f, _moveSm*dt); // jump - 1.0f
            if        (PlJ == XB.JumpSt.OneJumpS) {
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
            // Zooming
            if (XB.AData.Input.Zoom != 0.0f) {
                _camZoom = _camZoom + XB.AData.Input.Zoom*_camZoomSens;
                _camZoom = XB.Utils.ClampF(_camZoom , 0.0f, _camZoomMax);
            }
            // LIn
            // RIn
            // DUp // toggle walk/run (handled earlier)
            if (XB.AData.Input.DDown) { // swap between first and third person view
                _thirdP = !_thirdP;
            }
            // DLeft  // modifier for sphere radius (handled below)
            // DRight // modifier for sphere angle (handled below)
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
                    // Godot.GD.Print("all spheres used");
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
                    _camTransThis = _cam.GlobalTransform;
                    XB.ManagerSphere.Spheres[XB.ManagerSphere.HLSphereID].MoveSphere
                        (ref _camTransThis, ref _camTransPrev, ref _spaceSt, ref _spV, dt);
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
        if (XB.AData.Input.Debug1) { DebugHud.Debug1(); }
        if (XB.AData.Input.Debug2) { DebugHud.Debug2(); }
        if (XB.AData.Input.Debug3) { DebugHud.Debug3(); }
        if (XB.AData.Input.Debug4) { DebugHud.Debug4(); }
        if (XB.AData.Input.Debug5) { DebugHud.Debug5(); }
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
    public void SpawnPlayer(Godot.Vector2 spawnXZ) {
#if XBDEBUG
        var debug = new XB.DebugTimedBlock(XB.D.PControllerSpawnPlayer);
#endif

        // Godot.GD.Print("spawnplayer " + spawnXZ);
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

        float high = XB.WData.HighestPoint+XB.WData.LowHighExtra;
        float low  = XB.WData.LowestPoint -XB.WData.LowHighExtra;
        var spawnPoint  = new Godot.Vector3(_spawnPos.X,  high, _spawnPos.Y );
        var origin      = new Godot.Vector3(spawnPoint.X, high, spawnPoint.Z);
        var destination = new Godot.Vector3(spawnPoint.X, low,  spawnPoint.Z);
        RequestSpaceState(ref _spaceSt);
        XB.Utils.Raycast(ref _spaceSt, ref origin, ref destination, XB.LayerMasks.EnvironmentMask,
                         ref _resultRC                                                            );
        // Godot.GD.Print("try spawn at: " + spawnPoint + ", o: " + origin + ", d: " + destination
        //                + ", PlPos: " + GlobalPosition);
        if (_resultRC.Count > 0) {
            spawnPoint      = (Godot.Vector3)_resultRC["position"];
            spawnPoint.Y   += _respawnOff;
            _spawn          = false;
            _spawnAttempts  = 0;
            PlacePlayer(spawnPoint);
            // Godot.GD.Print("spawn hit " + spawnPoint);
        } else if (_spawnAttempts >= _spawnAttemptsMax) {
            _spawn         = false;
            _spawnAttempts = 0;
            PlacePlayer(spawnPoint);
            // Godot.GD.Print("spawn out of attempts at " + spawnPoint);
        } else {
            _spawnAttempts += 1;
            // Godot.GD.Print("spawn no hit");
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

    public void RequestSpaceState(ref Godot.PhysicsDirectSpaceState3D spaceSt) {
        spaceSt = GetWorld3D().DirectSpaceState;
    }
}
} // namespace close
