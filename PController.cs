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

// PController is responsible for controlling the player character and camera,
// as it is called every frame, various other updates are also called from here
// pulling all updates together like this makes it obvious in which order they are processed
// and allows for all the non engine code to be timed together
public partial class PController : Godot.CharacterBody3D {
    [Godot.Export] private Godot.NodePath       _cameraNode;
                   private Godot.Camera3D       _cam;                 // camera object
    [Godot.Export] private Godot.NodePath       _cameraRotationHNode; // camera's rot ctr horizontally
             public static Godot.Node3D         CCtrH;
    [Godot.Export] private Godot.NodePath       _cameraRotationVNode; // camera's rot ctr vertically
                   private Godot.Node3D         _cCtrV;
    [Godot.Export] private Godot.NodePath       _playerRiggedNode;
             public static Godot.Node3D         PModel; //TODO[ALEX]: remove static 
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
                   private const float          _colSm      = 14.0f;
                   private Godot.Color          _colCurrent = new Godot.Color(1.0f, 1.0f, 1.0f, 1.0f);

    public        bool          ThirdP            = true;
    private       bool          _canShoot         = false;
    private       bool          _stickyDrag       = false; // for when drag modifying spheres
    private       int           _stickyID         = -1;
    private const float         _respawnOff       = 0.1f;  // distance to ground when respawning
    private const float         _sphereSpawnDist  = 6.0f;  // distance to newly placed sphere in meter
    private       bool          _spawn            = false; // spawn player delayed for raycast to work
    private       Godot.Vector2 _spawnPos         = new Godot.Vector2(0.0f, 0.0f);
    private       int           _spawnAttempts    = 0;
    private const int           _spawnAttemptsMax = 20;

    private Godot.AudioStreamPlayer3D[] _audFootStep;
    private const int                   _audFootStepAmnt = 6;
    private float                       _tFootStep       = 0.0f;

    private Godot.Vector2 _mouse = new Godot.Vector2(0.0f, 0.0f); // mouse motion this tick

    private       XB.AirSt  _plA;               // player's air state
    private       XB.JumpSt _plJ;               // player's jump state
    public        bool      PlAiming  = false;  // is the player aiming
    private       bool      _plMoved  = false;  // player moved this frame
    private       float     _plYV     = 0.0f;   // player's velocity in y direction
    public        XB.MoveSt Move      = XB.MoveSt.Walk;
    private       float     _moveSpd  = 0.0f;   // current move speed
    private const float     _walkSpd  = -1.8f;  // speed for walking
    private const float     _runSpd   = -4.2f;  // speed for running
    private const float     _walkAnm  = 3.8f;   // animation speed multiplier (empirical)
    private const float     _jumpStr  = 5.0f;
    private const float     _plGrav   = 9.81f;
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
    private Godot.Vector3     _spSpawnPos   = new Godot.Vector3(0.0f, 0.0f, 0.0f);
    private Godot.Transform3D _camTransThis = new Godot.Transform3D(); // for moving spheres
    private Godot.Transform3D _camTransPrev = new Godot.Transform3D(); // for moving spheres
    private Godot.Collections.Dictionary   _resultRC = new Godot.Collections.Dictionary();
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

        _cam    = GetNode<Godot.Camera3D>        (_cameraNode);
        CCtrH   = GetNode<Godot.Node3D>          (_cameraRotationHNode);
        _cCtrV  = GetNode<Godot.Node3D>          (_cameraRotationVNode);
        _gunTip = GetNode<Godot.Node3D>          (_gunTipNode);
        PModel  = GetNode<Godot.Node3D>          (_playerRiggedNode);
        _pATree = GetNode<Godot.AnimationTree>   (_animationTreeNode);
        Hud     = (XB.HUD)GetNode<Godot.Control> (_hudNode);
        Menu    = (XB.Menu)GetNode<Godot.Control>(_menuNode);
        Menu.Hide();

        ThirdP   = true;
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

    // to avoid having to give an additional reference of hud and menu to Initialize
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

        if (@event is not Godot.InputEventMouseMotion) { return; }

        var mouseM = (Godot.InputEventMouseMotion)@event;
        _mouse.X   = XB.AData.S.SC.MouseMultX * mouseM.Relative.X;
        _mouse.Y   = XB.AData.S.SC.MouseMultY * mouseM.Relative.Y;

#if XBDEBUG
        debug.End();
#endif 
    }

    // Called every frame at fixed time steps (_PhysicsProcess is a Godot built in that every
    // object has, however the order of their calling is not fixed among all the objects)
    //NOTE[ALEX]: the update is split into functions to make timing easier,
    //            and to make explicit which variables are touched by each part
    public override void _PhysicsProcess(double delta) {
        float dt = (float)delta;

#if XBDEBUG
         DebugHud.UpdateDebugHUD(dt); // not part of the debug block or it will be included in the time
        var debug = new XB.DebugTimedBlock(XB.D.PController_PhysicsProcess);
#endif

        UpdateGeneral(dt, Hud, ref _pModelPos, ref PModel, ref _spaceSt);

        if (GetTree().Paused) {
#if XBDEBUG
            debug.End();
#endif
            return;
        }

        if (_spawn) { 
            SpawnPlayerDelayed(ref _spawnPos, _respawnOff, ref _spaceSt, ref _resultRC,
                               ref _spawn, ref _spawnAttempts, _spawnAttemptsMax       );
        }

        UpdateMovement(dt, ref _plJ, ref _plA, ref _plYV, _plGrav, _maxVertVelo, this,
                       _respawnOff, ref Move, _jumpStr, ref _moveSpd, _walkSpd, _runSpd, _moveSm,
                       ref _v, ref _vRot, ref _spV, CCtrH, ref _plMoved,
                       ref _tFootStep, _walkAnm, _audFootStep, _audFootStepAmnt                   );

        UpdateCamera(dt, ref ThirdP, Hud, ref PlAiming, ref _aimOff, _cSmooth, Move,
                     _aimHOff, _aimVOff, ref _mouse, ref CCtrH, ref _rotCtr, ref _toCam,
                     ref _toCamG, _maxVAng, ref _cCtrV, ref _cam, ref _cDist, ref _cOrig, ref _cDir,
                     ref _pOrig, ref _pNrm, ref _rDest, ref _spaceSt, ref _resultRC, ref _camAimHit,
                     ref _rOrig, ref _toCollision, _camCollDist, ref _camZoom, ref _camNewPos,
                     ref _plNewRot, ref _plMoved, ref _plA, ref _vRot, ref PModel                   );

        UpdateAiming(dt, ref ThirdP, ref _canShoot, ref PlAiming, ref _fov, ref _cam, ref _camHit,
                     ref _cOrig, ref _cDir, ref _pOrig, ref _pNrm, ref _gunTip, ref _rOrig,
                     ref _spaceSt, ref _resultRC, ref _gunDir, ref _rDest, ref _stickyDrag,
                     ref _stickyID, ref _cAP, _cSmooth, _aimEmpty                               );

        UpdateSphereInteractions(dt, ref ThirdP, ref PlAiming, ref _rOrig, ref _cam, ref _cOrig,
                                 ref _cDir, ref _pOrig, ref _pNrm, ref CCtrH, ref _rDest,
                                 ref _spaceSt, ref _resultRC, _sphere                         );

        UpdateAnimations(dt, ref _walkBlend, ref _blMove, ref _plA, _moveSm, ref _plMoved, _walkAnm,
                         ref PlAiming, ref _blIdle, ref _blWalk, _walkSm, ref _plJ, ref _pATree,
                         ref Move                                                                  );

        UpdatePlayerMaterial(dt, ref ThirdP, ref _colCurrent, _colSm,
                             ref _hairMat, ref _lashMat, ref _eyesMat, ref _bodyMat, ref _headMat);

        UpdateInputs(dt, Menu, Hud, ref _camZoom, _camZoomSens, _camZoomMax, ref ThirdP,
                     ref _canShoot, _sphereSpawnDist, ref _stickyDrag, ref _stickyID,
                     ref _camTransThis, ref _camTransPrev, ref _spV, ref _spaceSt, ref _cam, ref CCtrH);

#if XBDEBUG
        UpdateDebugInputs(DebugHud);
#endif 

        // to fix axes drifting apart due to imprecision (recommended in Godot wiki)
        _cam.Transform  = _cam.Transform.Orthonormalized();
        CCtrH.Transform = CCtrH.Transform.Orthonormalized();
        _camTransPrev   = _cam.GlobalTransform; // store for sphere movement

#if XBDEBUG
        debug.End();
#endif 
    }

    private void UpdateGeneral(float dt, XB.HUD hud, ref Godot.Vector2 pModelPos,
                               ref Godot.Node3D pModel, ref Godot.PhysicsDirectSpaceState3D spaceSt) {
#if XBDEBUG
        var debug = new XB.DebugTimedBlock(XB.D.PControllerUpdateGeneral);
#endif

        XB.AData.Input.GetInputs();
        hud.UpdateHUD(dt);
        XB.ManagerSphere.UpdateSpheres(dt);
        pModelPos.X = pModel.GlobalPosition.X;
        pModelPos.Y = pModel.GlobalPosition.Z;
        XB.ManagerTerrain.UpdateQTreeMeshes(ref pModelPos, XB.WData.LowestPoint, 
                                            XB.WData.HighestPoint, XB.WData.ImgMiniMap);
        RequestSpaceState(ref spaceSt); // get spacestate for raycasting

#if XBDEBUG
        debug.End();
#endif 
    }

    // Velocity is a Godot internal vector and used only in this function
    private void UpdateMovement(float dt, ref XB.JumpSt plJ, ref XB.AirSt plA, ref float plYV,
                                float plGrav, float maxVertVelo, Godot.Node3D contr,
                                float respawnOff, ref XB.MoveSt move, float jumpStr,
                                ref float moveSpd, float walkSpd, float runSpd, float moveSm,
                                ref Godot.Vector3 v, ref Godot.Vector3 vRot, ref Godot.Vector3 spV,
                                Godot.Node3D cCtrH, ref bool plMoved,
                                ref float tFootStep, float walkAnm, 
                                Godot.AudioStreamPlayer3D[] audFootStep, int audFootStepAmnt) {
#if XBDEBUG
        var debug = new XB.DebugTimedBlock(XB.D.PControllerUpdateMovement);
#endif

        // STEP 1: gravity and jumping
        if (IsOnFloor() && plGrav > 0.0f) {
            if        (plA != XB.AirSt.Grounded) { // landing frame
                plJ = XB.JumpSt.Landed;
                plA = XB.AirSt.Grounded;
            } else if (plA == XB.AirSt.Grounded) { // general state
                plJ = XB.JumpSt.OnGround;
            }

            plYV = 0.0f; // reset vertical velocity when landing on the ground
            if (XB.AData.Input.FDown) {
                plYV = -jumpStr;
                plJ  = XB.JumpSt.OneJumpS; // jumping start frame
                plA  = XB.AirSt.Rising;
            }
        } else {
            if      (plJ == XB.JumpSt.OneJumpS) { plJ = XB.JumpSt.OneJumpP; }
            else if (plJ == XB.JumpSt.TwoJumpS) { plJ = XB.JumpSt.TwoJumpP; }

            if (Velocity.Y == 0.0f) { plYV = 0.0f; }   // player hit ceiling

            if (XB.AData.Input.FDown && plJ == XB.JumpSt.OneJumpP) { // double jump
                plYV = -jumpStr;
                plJ  = XB.JumpSt.TwoJumpS;
            }

            plYV += dt*plGrav; // gravity
            plYV  = XB.Utils.ClampF(plYV, -maxVertVelo, maxVertVelo);
            
            if      (plYV < 0) { plA = XB.AirSt.Rising;   }
            else if (plYV > 0) { plA = XB.AirSt.Falling;  }
            else               { plA = XB.AirSt.Floating; }
        }

        // STEP 2: check for player being outside of terrain area
        //NOTE[ALEX]: PlacePlayer should rarely if ever be called, so the new allocations are acceptable
        if        (contr.GlobalPosition.Y < XB.WData.KillPlane || 
                   contr.GlobalPosition.Y < (XB.WData.LowestPoint - XB.WData.LowHighExtra)) {
            // Godot.GD.Print(">> out of bounds high/low");
            PlacePlayer(new Godot.Vector3(contr.GlobalPosition.X, 
                                          XB.WData.HighestPoint + XB.WData.LowHighExtra,
                                          contr.GlobalPosition.Z                        ));
        } else if (contr.GlobalPosition.X > 0.0f) {
            // Godot.GD.Print(">> out of bounds X high");
            PlacePlayer(new Godot.Vector3(-respawnOff,
                                          contr.GlobalPosition.Y + XB.WData.LowHighExtra,
                                          contr.GlobalPosition.Z                         ));
        } else if (contr.GlobalPosition.X < -XB.WData.WorldDim.X) {
            // Godot.GD.Print(">> out of bounds X low");
            PlacePlayer(new Godot.Vector3(-XB.WData.WorldDim.X + respawnOff,
                                          contr.GlobalPosition.Y + XB.WData.LowHighExtra,
                                          contr.GlobalPosition.Z                         ));
        } else if (contr.GlobalPosition.Z > 0.0f) {
            // Godot.GD.Print(">> out of bounds Z high");
            PlacePlayer(new Godot.Vector3(contr.GlobalPosition.X,
                                          contr.GlobalPosition.Y + XB.WData.LowHighExtra,
                                          -respawnOff                                    ));
        } else if (contr.GlobalPosition.Z < -XB.WData.WorldDim.Y) {
            // Godot.GD.Print(">> out of bounds Z low");
            PlacePlayer(new Godot.Vector3(contr.GlobalPosition.X,
                                          contr.GlobalPosition.Y + XB.WData.LowHighExtra,
                                          -XB.WData.WorldDim.Y + respawnOff              ));
        }

        // STEP 3: horizontal movement relative to camera rotation
        switch (move) {
            case XB.MoveSt.Walk: {
                moveSpd = XB.Utils.LerpF(moveSpd, walkSpd, moveSm*dt);
                if (XB.AData.Input.DUp) {
                    move = XB.MoveSt.Run;
                }
                break;
            }
            case XB.MoveSt.Run: {
                moveSpd = XB.Utils.LerpF(moveSpd, runSpd,  moveSm*dt);
                if (XB.AData.Input.MoveX == 0.0f && XB.AData.Input.MoveY == 0.0f ||
                    plJ != XB.JumpSt.OnGround || XB.AData.Input.LIn) {
                    move = XB.MoveSt.Walk;
                }
                break;
            }
        }

        v.X = XB.AData.Input.MoveX;
        v.Y = 0.0f; //NOTE[ALEX]: technically not necessary
        v.Z = XB.AData.Input.MoveY;
        v   = v.Normalized()*moveSpd;
        spV.X = 0.0f;
        spV.Y = 0.0f;
        spV.Z = v.Z; // horizontal player movement for use when moving spheres
        v.Y = -plYV;
        v   = v.Rotated(cCtrH.Transform.Basis.Y, cCtrH.Transform.Basis.GetEuler().Y);
        spV = spV.Rotated(cCtrH.Transform.Basis.Y, cCtrH.Transform.Basis.GetEuler().Y);
        vRot.X = v.X; // as reference when rotating the player
        vRot.Y = 0.0f;
        vRot.Z = v.Z;

        if        (contr.GlobalPosition.X > -respawnOff) { // limit x movement
            v.X   = XB.Utils.MinF(v.X,   0.0f);
            spV.X = XB.Utils.MinF(spV.X, 0.0f);
        } else if (contr.GlobalPosition.X < -(XB.WData.WorldDim.X-respawnOff)) {
            v.X   = XB.Utils.MaxF(v.X,   0.0f);
            spV.X = XB.Utils.MaxF(spV.X, 0.0f);
        } 
        if        (contr.GlobalPosition.Z > -respawnOff) { // limit z movement
            v.Z   = XB.Utils.MinF(v.Z,   0.0f);
            spV.Z = XB.Utils.MinF(spV.Z, 0.0f);
        } else if (contr.GlobalPosition.Z < -(XB.WData.WorldDim.Y-respawnOff)) {
            v.Z   = XB.Utils.MaxF(v.Z,   0.0f);
            spV.Z = XB.Utils.MaxF(spV.Z, 0.0f);
        }

        // STEP 4: move using Godot's physics system
        if (v.Length() > 0.0f) { plMoved = true;  }
        else                   { plMoved = false; }
        Velocity = v;
        MoveAndSlide();

        // STEP 5: footstep sounds
        if ((XB.AData.Input.MoveX != 0.0f || XB.AData.Input.MoveY != 0.0f)
            && plJ == XB.JumpSt.OnGround                                  ) {
            // align footstep timing with animation
            switch (move) {
                case XB.MoveSt.Walk: { tFootStep += (dt*walkAnm*0.5f); break; }
                case XB.MoveSt.Run:  { tFootStep += (dt*walkAnm);      break; }
            }
        }
        if (tFootStep > 1.0f) {
            int rand = XB.Random.RandomInRangeI(0, audFootStepAmnt-1);
            audFootStep[rand].Play();
            tFootStep = 0.0f;
        }

#if XBDEBUG
        debug.End();
#endif 
    }

    private void UpdateCamera(float dt, ref bool thirdP, XB.HUD hud, ref bool plAiming,
                              ref Godot.Vector2 aimOff, float cSmooth, XB.MoveSt move,
                              float aimHOff, float aimVOff, ref Godot.Vector2 mouse,
                              ref Godot.Node3D cCtrH, ref Godot.Vector3 rotCtr,
                              ref Godot.Vector3 toCam, ref Godot.Vector3 toCamG,
                              float maxVAng, ref Godot.Node3D cCtrV, ref Godot.Camera3D cam,
                              ref float cDist, ref Godot.Vector3 cOrig, ref Godot.Vector3 cDir,
                              ref Godot.Vector3 pOrig, ref Godot.Vector3 pNrm,
                              ref Godot.Vector3 rDest, ref Godot.PhysicsDirectSpaceState3D spaceSt,
                              ref Godot.Collections.Dictionary resultRC, ref Godot.Vector3 camAimHit,
                              ref Godot.Vector3 rOrig, ref Godot.Vector3 toCollision,
                              float camCollDist, ref float camZoom, ref Godot.Vector3 camNewPos,
                              ref Godot.Vector3 plNewRot, ref bool plMoved, ref XB.AirSt plA,
                              ref Godot.Vector3 vRot, ref Godot.Node3D pModel                        ) {
#if XBDEBUG
        var debug = new XB.DebugTimedBlock(XB.D.PControllerUpdateCamera);
#endif

        // STEP 1: aiming offset
        if (!thirdP) {
            hud.CrossVisible = true;
            plAiming = false;
            aimOff.X = XB.Utils.LerpF(aimOff.X, 0.0f, cSmooth*dt);
            aimOff.Y = XB.Utils.LerpF(aimOff.Y, 0.0f, cSmooth*dt);
        } else if (XB.AData.Input.SLBot) {
            hud.CrossVisible = true;
            move     = XB.MoveSt.Walk;
            plAiming = true;
            aimOff.X = XB.Utils.LerpF(aimOff.X, aimHOff, cSmooth*dt);
            aimOff.Y = XB.Utils.LerpF(aimOff.Y, aimVOff, cSmooth*dt);
        } else {
            hud.CrossVisible = false;
            plAiming = false;
            aimOff.X = XB.Utils.LerpF(aimOff.X, 0.0f, cSmooth*dt);
            aimOff.Y = XB.Utils.LerpF(aimOff.Y, 0.0f, cSmooth*dt);
        }

        XB.AData.Input.CamX += mouse.X;
        XB.AData.Input.CamY += mouse.Y;
        mouse.X = 0.0f;
        mouse.Y = 0.0f;

        // STEP 2: horizontal camera movement
        float horAmount = dt*XB.AData.S.SC.CamXSens*XB.AData.Input.CamX;
        cCtrH.RotateObjectLocal(cCtrH.Transform.Basis.Y, horAmount);

        // STEP 3: vertical camera movement
        rotCtr  = cCtrH.GlobalPosition;
        rotCtr += aimOff.X*cam.GlobalTransform.Basis.X;
        rotCtr += aimOff.Y*cam.GlobalTransform.Basis.Y;
        if (thirdP) { toCam = cam.GlobalPosition-rotCtr;  }
        else        { toCam = cam.GlobalTransform.Basis.Z; }
        toCamG   = toCam;
        toCamG.Y = 0.0f;
        float cAngle    = toCam.AngleTo(toCamG)*XB.Constants.Rad2Deg;
        float verAmount = dt*XB.AData.S.SC.CamYSens*XB.AData.Input.CamY;
        if (cAngle > maxVAng) {
            if      (toCam.Y > 0.0f && XB.AData.Input.CamY < 0.0f) { verAmount = 0.0f; }
            else if (toCam.Y < 0.0f && XB.AData.Input.CamY > 0.0f) { verAmount = 0.0f; }
        }
        cCtrV.RotateObjectLocal(cCtrV.Transform.Basis.X, verAmount);

        // STEP 4: check for camera collisions and move camera
        if (!thirdP) {
            cDist = XB.Utils.LerpF(cDist, 0.0f, cSmooth*dt);
        } else if (plAiming) {
            cDist = XB.Utils.LerpF(cDist, XB.AData.S.SC.CamAimDist, cSmooth*dt);

            cOrig = cam.GlobalPosition;
            cDir  = cam.GlobalTransform.Basis.Z;
            pOrig = cCtrH.GlobalPosition;
            pNrm  = cam.GlobalTransform.Basis.Z;
            XB.Utils.IntersectRayPlaneV3(ref cOrig, ref cDir, ref pOrig, ref pNrm, ref rOrig);
            rDest = cam.GlobalPosition;
            XB.Utils.Raycast(ref spaceSt, ref rOrig, ref rDest, XB.LayerMasks.AimMask, ref resultRC);
            if (resultRC.Count > 0) {
                camAimHit = (Godot.Vector3)resultRC["position"];
                float camAimOff = (cam.GlobalPosition - camAimHit).Length();
                cDist = XB.AData.S.SC.CamAimDist - camAimOff;
            }
        } else {
            rOrig = cCtrH.GlobalPosition;
            rDest = cCtrH.GlobalPosition + 2.0f*toCam;
            XB.Utils.Raycast(ref spaceSt, ref rOrig, ref rDest, XB.LayerMasks.CamMask, ref resultRC);
            if (resultRC.Count > 0) {
                toCollision = (Godot.Vector3)resultRC["position"]-cCtrH.GlobalPosition;
                float newDist = toCollision.Length() - camCollDist;
                      newDist = XB.Utils.ClampF(newDist, XB.Settings.CamMinDist,
                                                XB.AData.S.SC.CamMaxDist + camZoom);
                cDist = newDist;
            } else {
                cDist = XB.Utils.LerpF(cDist, XB.AData.S.SC.CamMaxDist + camZoom, cSmooth*dt);
            }
        }
        camNewPos.X  = aimOff.X;
        camNewPos.Y  = aimOff.Y;
        camNewPos.Z  = cDist;
        cam.Position = camNewPos;

        // STEP 5: rotating player
        if (plAiming) {
            plNewRot.Y = cCtrH.Rotation.Y + XB.Constants.Pi;
        } else if (plMoved && plA == XB.AirSt.Grounded) {
            float rot = Godot.Vector3.Forward.AngleTo(vRot)+XB.Constants.Pi;
            if (Godot.Vector3.Forward.Cross(vRot).Y < 0) {
                rot = 2.0f*XB.Constants.Pi - rot;
            }
            plNewRot.Y = rot;
        }
        pModel.Rotation = plNewRot; // x and y are never changed from 0.0f

#if XBDEBUG
        debug.End();
#endif 
    }

    private void UpdateAiming(float dt, ref bool thirdP, ref bool canShoot, ref bool plAiming,
                              ref float fov, ref Godot.Camera3D cam, ref Godot.Vector3 camHit,
                              ref Godot.Vector3 cOrig, ref Godot.Vector3 cDir,
                              ref Godot.Vector3 pOrig, ref Godot.Vector3 pNrm, 
                              ref Godot.Node3D gunTip, ref Godot.Vector3 rOrig,
                              ref Godot.PhysicsDirectSpaceState3D spaceSt,
                              ref Godot.Collections.Dictionary resultRC, ref Godot.Vector3 gunDir,
                              ref Godot.Vector3 rDest, ref bool stickyDrag, ref int stickyID,
                              ref Godot.CameraAttributesPhysical cAP, float cSmooth, float aimEmpty) {
#if XBDEBUG
        var debug = new XB.DebugTimedBlock(XB.D.PControllerUpdateAiming);
#endif

        if (!thirdP) {
            canShoot = true;
            fov = XB.AData.S.SC.FovDef;
        } else if (plAiming) {
            canShoot = true;
            camHit = cam.GlobalPosition - cam.GlobalTransform.Basis.Z*1000.0f;
            cOrig  = cam.GlobalPosition;
            cDir   = cam.GlobalTransform.Basis.Z;
            pOrig  = gunTip.GlobalPosition;
            pNrm   = cam.GlobalTransform.Basis.Z;
            XB.Utils.IntersectRayPlaneV3(ref cOrig, ref cDir, ref pOrig, ref pNrm, ref rOrig);
            // check from gun tip in direction of view
            XB.Utils.Raycast(ref spaceSt, ref rOrig, ref camHit, XB.LayerMasks.AimMask, ref resultRC);
            if (resultRC.Count > 0) {
                camHit = (Godot.Vector3)resultRC["position"];
            } else {
                camHit = cam.GlobalPosition - cam.GlobalTransform.Basis.Z*aimEmpty;
            }
            gunDir = camHit-gunTip.GlobalPosition;
            // check backwards for hits with geometry near to the player
            rOrig = camHit             - 0.1f*gunDir;
            rDest = cam.GlobalPosition + 0.1f*gunDir;
            XB.Utils.Raycast(ref spaceSt, ref rOrig, ref rDest, XB.LayerMasks.AimMask, ref resultRC);
            if (resultRC.Count > 0) { canShoot = false; }
            fov = XB.AData.S.SC.FovAim;
        } else {
            canShoot   = false;
            stickyDrag = false;
            stickyID   = XB.ManagerSphere.MaxSphereAmount;
            fov = XB.AData.S.SC.FovDef;
        }

        cAP                    = (Godot.CameraAttributesPhysical)cam.Attributes;
        cAP.FrustumFocalLength = XB.Utils.LerpF(cAP.FrustumFocalLength, fov, cSmooth*dt);
    }

    private void UpdateSphereInteractions(float dt, ref bool thirdP, ref bool plAiming,
                                          ref Godot.Vector3 rOrig, ref Godot.Camera3D cam,
                                          ref Godot.Vector3 cOrig, ref Godot.Vector3 cDir,
                                          ref Godot.Vector3 pOrig, ref Godot.Vector3 pNrm,
                                          ref Godot.Node3D cCtrH, ref Godot.Vector3 rDest,
                                          ref Godot.PhysicsDirectSpaceState3D spaceSt,
                                          ref Godot.Collections.Dictionary resultRC,
                                          XB.Sphere sphere                                ) {
#if XBDEBUG
        var debug = new XB.DebugTimedBlock(XB.D.PControllerUpdateSphereInteractions);
#endif

        if (!thirdP || plAiming) {
            rOrig = cam.GlobalPosition;
            if (plAiming) {
                cOrig = cam.GlobalPosition;
                cDir  = cam.GlobalTransform.Basis.Z;
                pOrig = cCtrH.GlobalPosition;
                pNrm  = cam.GlobalTransform.Basis.Z;
                XB.Utils.IntersectRayPlaneV3(ref cOrig, ref cDir, ref pOrig, ref pNrm, ref rOrig);
            }
            float rayDistance  = XB.WData.WorldDim.X+XB.WData.WorldDim.Y;
                  rayDistance *= -1.0f;
            rDest = rOrig + rayDistance*cam.GlobalTransform.Basis.Z;
            XB.Utils.Raycast(ref spaceSt, ref rOrig, ref rDest, XB.LayerMasks.SphereMask, ref resultRC);
            if (resultRC.Count > 0) {
                sphere = (XB.Sphere)resultRC["collider"];
                XB.ManagerSphere.ChangeHighlightSphere(sphere.ID);
            } else {
                XB.ManagerSphere.ChangeHighlightSphere(XB.ManagerSphere.MaxSphereAmount);
            }
        } else {
            XB.ManagerSphere.ChangeHighlightSphere(XB.ManagerSphere.MaxSphereAmount);
        }

#if XBDEBUG
        debug.End();
#endif 
    }

    private void UpdateAnimations(float dt, ref Godot.Vector2 walkBlend, ref float blMove,
                                  ref XB.AirSt plA, float moveSm, ref bool plMoved, float walkAnm,
                                  ref bool plAiming, ref float blIdle, ref Godot.Vector2 blWalk,
                                  float walkSm, ref XB.JumpSt plJ, ref Godot.AnimationTree pATree,
                                  ref XB.MoveSt move                                              ) {
#if XBDEBUG
        var debug = new XB.DebugTimedBlock(XB.D.PControllerUpdateAnimations);
#endif

        float idleMode  = 0.0f;
        float walkSpeed = 0.0f;
        walkBlend.X = 0.0f;
        walkBlend.Y = 0.0f;
        if (plA == XB.AirSt.Grounded) { // walking - on ground
            blMove = XB.Utils.LerpF(blMove, 0.0f, moveSm*dt); // walk - 0.0f
            if (plMoved) {
                idleMode = 1.0f; // walk
                if (plAiming) {
                    walkBlend.X = XB.AData.Input.MoveX; // walk left/right
                    walkBlend.Y = XB.AData.Input.MoveY; // walk forwards/backwards
                } else { // condense 2D movement into single value representing amount
                    float length  = XB.AData.Input.MoveX*XB.AData.Input.MoveX;
                          length += XB.AData.Input.MoveY*XB.AData.Input.MoveY;
                          length  = System.MathF.Sqrt(length);
                    walkBlend.Y = length;
                }
                walkBlend /= walkBlend.Length();
                walkSpeed  = walkBlend.Length()*walkAnm;
                switch (move) {
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
                if (!plAiming) { idleMode = 0.0f; } // idle
            }
            blIdle = XB.Utils.LerpF(blIdle, idleMode, moveSm*dt);
            XB.Utils.LerpV2(ref blWalk, ref walkBlend, walkSm*dt, ref blWalk);
        } else { // jumping - in air
            blMove = XB.Utils.LerpF(blMove, 1.0f, moveSm*dt); // jump is 1.0f in animation tree
            if        (plJ == XB.JumpSt.OneJumpS) {
                pATree.Set("parameters/jumpTr/transition_request", "state_0");
            } else if (plA == XB.AirSt.Rising || plA == XB.AirSt.Falling) {
                pATree.Set("parameters/jumpTr/transition_request", "state_1");
            } else if (plJ == XB.JumpSt.Landed) {
                pATree.Set("parameters/jumpTr/transition_request", "state_2");
            }
        }

        pATree.Set("parameters/walkSpeed/scale",          walkSpeed);
        pATree.Set("parameters/moveIdle/blend_amount",    blIdle   );
        pATree.Set("parameters/moveJump/blend_amount",    blMove   );
        pATree.Set("parameters/walkSpace/blend_position", blWalk   );

#if XBDEBUG
        debug.End();
#endif 
    }

    // used for fade in / fade out when changing camera mode
    private void UpdatePlayerMaterial(float dt, ref bool thirdP, ref Godot.Color colCurrent,
                                      float colSm, 
                                      ref Godot.BaseMaterial3D hairMat,
                                      ref Godot.BaseMaterial3D lashMat,
                                      ref Godot.BaseMaterial3D eyesMat,
                                      ref Godot.BaseMaterial3D bodyMat,
                                      ref Godot.BaseMaterial3D headMat                      ) {
#if XBDEBUG
        var debug = new XB.DebugTimedBlock(XB.D.PControllerUpdatePlayerMaterial);
#endif

        if (thirdP) {
            if (colCurrent.A > 0.98f) {
                colCurrent.A = 1.0f; //NOTE[ALEX]: avoids visual artifacts at the end of fade in
            } else {
                colCurrent.A = XB.Utils.LerpF(colCurrent.A, 1.0f, colSm*dt);
            }
        } else {
            colCurrent.A = XB.Utils.LerpF(colCurrent.A, 0.0f, colSm*dt);
        }
        hairMat.AlbedoColor = colCurrent;
        lashMat.AlbedoColor = colCurrent;
        eyesMat.AlbedoColor = colCurrent;
        bodyMat.AlbedoColor = colCurrent;
        headMat.AlbedoColor = colCurrent;

#if XBDEBUG
        debug.End();
#endif 
    }

    private void UpdateInputs(float dt, XB.Menu menu, XB.HUD hud, ref float camZoom,
                              float camZoomSens, float camZoomMax, ref bool thirdP,
                              ref bool canShoot, float sphereSpawnDist, ref bool stickyDrag,
                              ref int stickyID, ref Godot.Transform3D camTransThis,
                              ref Godot.Transform3D camTransPrev, ref Godot.Vector3 spV,
                              ref Godot.PhysicsDirectSpaceState3D spaceSt, ref Godot.Camera3D cam,
                              ref Godot.Node3D cCtrH                                              ) {
#if XBDEBUG
        var debug = new XB.DebugTimedBlock(XB.D.PControllerUpdateInputs);
#endif

        if        (XB.AData.Input.Start) { // system menu
            menu.OpenMenu();
        } else if (XB.AData.Input.Select) { // toggle HUD visibility
            hud.ToggleHUD();
        } else {
            // Zooming
            if (XB.AData.Input.Zoom != 0.0f) {
                camZoom = camZoom + XB.AData.Input.Zoom*camZoomSens;
                camZoom = XB.Utils.ClampF(camZoom , 0.0f, camZoomMax);
            }
            // LIn
            // RIn
            // DUp // toggle walk/run (handled earlier)
            if (XB.AData.Input.DDown) { // swap between first and third person view
                thirdP = !thirdP;
            }
            // DLeft  // modifier for sphere radius (handled below)
            // DRight // modifier for sphere angle (handled below)
            if (canShoot && XB.AData.Input.FUp) { // link
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
            if (canShoot && XB.AData.Input.SLTop) { // place sphere
                _spSpawnPos =   cCtrH.GlobalPosition
                              + cam.GlobalTransform.Basis.Z*-sphereSpawnDist;
                if (!XB.ManagerSphere.RequestSphere(ref _spSpawnPos)) {
                    // Godot.GD.Print("all spheres used");
                }
            }
            // SLBot - aiming (handled earlier)
            if (canShoot && XB.AData.Input.SRTop) { // remove sphere
                if (XB.ManagerSphere.HLSphereID < XB.ManagerSphere.MaxSphereAmount) {
                    XB.ManagerSphere.Spheres[XB.ManagerSphere.HLSphereID].RemoveSphere();
                    stickyDrag = false;
                    stickyID   = XB.ManagerSphere.MaxSphereAmount;
                }
            }
            // SRBot - Sphere interactions
            if (canShoot && XB.AData.Input.SRBot
                    && !XB.AData.Input.DLeft && !XB.AData.Input.DRight) {
                if (XB.ManagerSphere.HLSphereID < XB.ManagerSphere.MaxSphereAmount) {
                    camTransThis = cam.GlobalTransform;
                    XB.ManagerSphere.Spheres[XB.ManagerSphere.HLSphereID].MoveSphere
                        (ref camTransThis, ref camTransPrev, ref spaceSt, ref spV, dt);
                }
            }
            if (!stickyDrag
                && (   (canShoot && XB.AData.Input.SRBot && XB.AData.Input.DLeft)
                    || (canShoot && XB.AData.Input.SRBot && XB.AData.Input.DRight))) {
                stickyDrag = true;
                stickyID   = XB.ManagerSphere.HLSphereID;
            }
            if (stickyDrag) {
                if (XB.AData.Input.SRBot) {
                    if (stickyID < XB.ManagerSphere.MaxSphereAmount) {
                        if (XB.AData.Input.DLeft) {
                            XB.ManagerSphere.Spheres[stickyID].ChangeSphereRadius
                                (XB.AData.Input.CamY*dt);
                        }
                        if (XB.AData.Input.DRight) {
                            XB.ManagerSphere.Spheres[stickyID].ChangeSphereAngle
                                (XB.AData.Input.CamY*dt);
                        }
                    }
                } else {
                    stickyDrag = false;
                    stickyID   = XB.ManagerSphere.MaxSphereAmount;
                }
            }
        }

#if XBDEBUG
        debug.End();
#endif 
    }

#if XBDEBUG
    private void UpdateDebugInputs(XB.DebugHUD debugHud) {
        // DEBUG BUTTONS
        if (XB.AData.Input.Debug1) { debugHud.Debug1(); }
        if (XB.AData.Input.Debug2) { debugHud.Debug2(); }
        if (XB.AData.Input.Debug3) { debugHud.Debug3(); }
        if (XB.AData.Input.Debug4) { debugHud.Debug4(); }
        if (XB.AData.Input.Debug5) { debugHud.Debug5(); }
    }
#endif 

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

    private void SpawnPlayerDelayed(ref Godot.Vector2 spawnPos, float respawnOff,
                                    ref Godot.PhysicsDirectSpaceState3D spaceSt,
                                    ref Godot.Collections.Dictionary resultRC, ref bool spawn,
                                    ref int spawnAttempts, int spawnAttemptsMax               ) {
#if XBDEBUG
        var debug = new XB.DebugTimedBlock(XB.D.PControllerSpawnPlayerDelayed);
#endif

        float high = XB.WData.HighestPoint + XB.WData.LowHighExtra;
        float low  = XB.WData.LowestPoint  - XB.WData.LowHighExtra;
        var spawnPoint  = new Godot.Vector3(spawnPos.X,   high, spawnPos.Y  );
        var origin      = new Godot.Vector3(spawnPoint.X, high, spawnPoint.Z);
        var destination = new Godot.Vector3(spawnPoint.X, low,  spawnPoint.Z);
        RequestSpaceState(ref spaceSt);
        XB.Utils.Raycast(ref spaceSt, ref origin, ref destination, XB.LayerMasks.EnvironmentMask,
                         ref resultRC                                                            );
        // Godot.GD.Print("try spawn at: " + spawnPoint + ", o: " + origin + ", d: " + destination
        //                + ", PlPos: " + GlobalPosition);
        if (resultRC.Count > 0) {
            spawnPoint      = (Godot.Vector3)resultRC["position"];
            spawnPoint.Y   += respawnOff;
            spawn          = false;
            spawnAttempts  = 0;
            PlacePlayer(spawnPoint);
            // Godot.GD.Print("spawn hit " + spawnPoint);
        } else if (spawnAttempts >= spawnAttemptsMax) {
            spawn         = false;
            spawnAttempts = 0;
            PlacePlayer(spawnPoint);
            // Godot.GD.Print("spawn out of attempts at " + spawnPoint);
        } else {
            spawnAttempts += 1;
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
