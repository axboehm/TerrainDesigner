//#define XBDEBUG
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
    Sprint,
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
    // [Godot.Export] private Godot.NodePath       _hudNode;
    //          public static XB.HUD               Hud;
    // [Godot.Export] private Godot.NodePath       _menuNode;
    //          public static XB.Menu              Menu;
    // [Godot.Export] private Godot.NodePath       _playerSkeletonNode;
    //                private Godot.Skeleton3D     _plSkel;

    // zoom
    public static bool Zoomed = false;

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
    private       XB.MoveSt _move     = XB.MoveSt.Run;
    private       float     _moveSpd  = 0.0f;   // current move speed
    private const float     _walkAnm  = 3.8f;   // animation speed multiplier (empirical)
    private const float     _walkSpd  = -2.4f;  // speed for walking
    private const float     _runSpd   = -4.2f;  // speed for running
    private const float     _sprSpd   = -8.4f;  // speed for sprinting
    private const float     _jumpStr  = 5.0f;
    public static float     PlGrav    = 9.81f;
    private const float     _maxVertVelo = 15.0f;

    private float         _fov      = 0.0f;     // camera fov to lerp to
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
    private const float   _moveSm   = 10.0f;    // smoothing value for all move lerping
    private float         _blMove   = 0.0f;     // animation move blend value
    private float         _blIdle   = 0.0f;     // animation moveIdle blend value

    public override void _Ready() {
        CollisionMask  = XB.LayerMasks.PlayerMask;
        CollisionLayer = XB.LayerMasks.PlayerLayer;

        _cam        = GetNode<Godot.Camera3D>        (_cameraNode);
        CCtrH       = GetNode<Godot.Node3D>          (_cameraRotationHNode);
        _cCtrV      = GetNode<Godot.Node3D>          (_cameraRotationVNode);
        _gunTip     = GetNode<Godot.Node3D>          (_gunTipNode);
        PModel      = GetNode<Godot.Node3D>          (_playerRiggedNode);
        // _plSkel  = GetNode<Godot.Skeleton3D>      (_playerSkeletonNode);
        _pATree     = GetNode<Godot.AnimationTree>   (_animationTreeNode);
        // Hud         = (XB.HUD)GetNode<Godot.Control> (_hudNode);
        // Menu        = (XB.Menu)GetNode<Godot.Control>(_menuNode);
        Zoomed      = false;

        _audFootStep    = new Godot.AudioStreamPlayer3D[_audFootStepAmnt];
        _audFootStep[0] = _audFootStep0;
        _audFootStep[1] = _audFootStep1;
        _audFootStep[2] = _audFootStep2;
        _audFootStep[3] = _audFootStep3;
        _audFootStep[4] = _audFootStep4;
        _audFootStep[5] = _audFootStep5;
    }

    // get input using godot's system, used for mouse movement input
    public override void _Input(Godot.InputEvent @event) {
        if (@event is not Godot.InputEventMouseMotion) return;

        var mouseM    = (Godot.InputEventMouseMotion)@event;
        _mouse.X = -0.015625f * mouseM.Relative.X; // multipliers = -30/1920|1080
        _mouse.Y = -0.027777f * mouseM.Relative.Y;
    }

    // Called every frame at fixed time steps, Project Settings > Physics > Common > Physics Fps
    // used for calculations that must happen before the physics step, like moving colliders
    // the camera is child of CCtrV, which is child of CCtrH
    // this allows vertical rotation independent of horizontal rotation
    public override void _PhysicsProcess(double delta) {
        // UPDATE GENERAL
        float dt = (float)delta;
        // Hud.UpdateHUD(WpnMode, (float)delta);
        var spaceSt = RequestSpaceState(); // get spacestate for raycasting


        // MOVEMENT
        // STEP 1: gravity and jumping
        if (IsOnFloor() && PlGrav > 0.0f) {
            if (_plA != XB.AirSt.Grounded) PlJ = XB.JumpSt.Landed;     // landing frame
            if (_plA == XB.AirSt.Grounded) PlJ = XB.JumpSt.OnGround; { // general state
                _plA = XB.AirSt.Grounded;
            }
            _plYV      = 0.0f; // reset vertical velocity when landing on the ground
            if (XB.AData.Input.FDown) {
                _plYV    = -_jumpStr;
                PlJ      = XB.JumpSt.OneJumpS; // jumping start frame
                _plA     = XB.AirSt.Rising;
            }
        } else {
            if (PlJ == XB.JumpSt.OneJumpS) PlJ = XB.JumpSt.OneJumpP;
            if (PlJ == XB.JumpSt.TwoJumpS) PlJ = XB.JumpSt.TwoJumpP;
            if (Velocity.Y == 0.0f)        _plYV = 0.0f;       // player hit ceiling
            if (XB.AData.Input.FDown && PlJ == XB.JumpSt.OneJumpP) { // double jump
                _plYV = -_jumpStr;
                PlJ   = XB.JumpSt.TwoJumpS;
            }
            _plYV += dt*PlGrav;
            _plYV  = XB.Utils.ClampF(_plYV, -_maxVertVelo, _maxVertVelo);
            if      (_plYV < 0) _plA = XB.AirSt.Rising;
            else if (_plYV > 0) _plA = XB.AirSt.Falling;
            else                _plA = XB.AirSt.Floating;
        }

        // STEP 2: horizontal movement relative to camera rotation
        switch (_move) {
            case XB.MoveSt.Walk: {
                _moveSpd = XB.Utils.LerpF(_moveSpd, _walkSpd, _moveSm*dt);
                break;
            }
            case XB.MoveSt.Run: {
                _moveSpd = XB.Utils.LerpF(_moveSpd, _runSpd,  _moveSm*dt);
                if (XB.AData.Input.LIn) {
                    _move = XB.MoveSt.Sprint;
                }
                break;
            }
            case XB.MoveSt.Sprint: {
                _moveSpd = XB.Utils.LerpF(_moveSpd, _sprSpd,  _moveSm*dt);
                if (XB.AData.Input.MoveX == 0.0f && XB.AData.Input.MoveY == 0.0f ||
                    PlJ != XB.JumpSt.OnGround) {
                    _move = XB.MoveSt.Run;
                }
                break;
            }
        }

        var v    = new Godot.Vector3(XB.AData.Input.MoveX, 0.0f, XB.AData.Input.MoveY);
            v    = v.Normalized()*_moveSpd;
            v.Y  = -_plYV;
            v    = v.Rotated(CCtrH.Transform.Basis.Y, CCtrH.Transform.Basis.GetEuler().Y);

        // STEP 4: move using Godot's physics system
        if (v.Length() > 0.0f) _plMoved = true;
        else                   _plMoved = false; 
        Velocity = v;
        MoveAndSlide();

        // // STEP 5: footstep sounds
        if ((XB.AData.Input.MoveX != 0.0f || XB.AData.Input.MoveY != 0.0f)
            && PlJ == XB.JumpSt.OnGround                                  ) {
            _tFootStep += (dt*_walkAnm); // align footstep timing with animation multiplier
        }
        if (_tFootStep > 1.0f) {
            int rand = XB.Random.RandomInRangeI(0, _audFootStepAmnt-1);
            _audFootStep[rand].Play();
            _tFootStep = 0.0f;
        }


        // CAMERA
        // STEP 1: aiming
        if (XB.AData.Input.SLBot) {
            Zoomed    = false;
            _move     = XB.MoveSt.Walk;
            PlAiming  = true;
            _aimOff.X = _aimHOff;
            _aimOff.Y = _aimVOff;
        } else if (Zoomed) {
            _move     = XB.MoveSt.Walk;
            _aimOff.X = _aimHOff;
            _aimOff.Y = _aimVOff;
        } else {
            if (_move != XB.MoveSt.Sprint) {
                _move = XB.MoveSt.Run;
            }
            PlAiming  = false;
            _aimOff.X = 0.0f;
            _aimOff.Y = 0.0f;
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
        var   toCam     = _cam.GlobalPosition-rotCtr;
        var   toCamG    = new Godot.Vector3(toCam.X, 0.0f, toCam.Z);
        float cAngle    = toCam.AngleTo(toCamG)*XB.Constants.Rad2Deg;
        float verAmount = dt*XB.AData.CamYSens*XB.AData.Input.CamY;
        if (cAngle > _maxVAng) {
            if      (toCam.Y > 0.0f && XB.AData.Input.CamY < 0.0f) verAmount = 0.0f;
            else if (toCam.Y < 0.0f && XB.AData.Input.CamY > 0.0f) verAmount = 0.0f;
        }
        _cCtrV.RotateObjectLocal(_cCtrV.Transform.Basis.X, verAmount);

        // STEP 4: check for camera collisions and move camera
        if (PlAiming || Zoomed) {
            _cam.Position = new Godot.Vector3(_aimOff.X, _aimOff.Y, XB.AData.CamAimDist);

            var rayOrig  = XB.Utils.IntersectRayPlaneV3
                             (_cam.GlobalPosition,  _cam.GlobalTransform.Basis.Z,
                              CCtrH.GlobalPosition, _cam.GlobalTransform.Basis.Z);
            var resultCA = XB.Utils.Raycast(spaceSt, rayOrig, _cam.GlobalPosition,
                                            XB.LayerMasks.AimMask);
            if (resultCA.Count > 0) {
                var camAimHit = (Godot.Vector3)resultCA["position"];
                var camAimOff = (_cam.GlobalPosition-camAimHit).Length();
                _cam.Position = new Godot.Vector3(_aimOff.X, _aimOff.Y, 
                                                  XB.AData.CamAimDist-camAimOff);
            }
        } else {
            var resultC = XB.Utils.Raycast(spaceSt, CCtrH.GlobalPosition,
                                           CCtrH.GlobalPosition + 2.0f*toCam, XB.LayerMasks.CamMask);
            if (resultC.Count > 0) {
                var   toCollision = (Godot.Vector3)resultC["position"]-CCtrH.GlobalPosition;
                float newDist     = toCollision.Length() - XB.AData.CamCollDist;
                      newDist     = XB.Utils.ClampF(newDist, XB.AData.CamMinDist, XB.AData.CamMaxDist);
                _cam.Position = new Godot.Vector3(_aimOff.X, _aimOff.Y, newDist);
            } else {
                _cam.Position = new Godot.Vector3(_aimOff.X, _aimOff.Y, XB.AData.CamMaxDist);
            }
        }


        // STEP 5: rotating player
        if (PlAiming) {
            PModel.Rotation = new Godot.Vector3(0.0f, CCtrH.Rotation.Y+XB.Constants.Pi, 0.0f);
        } else if (_plMoved && _plA == XB.AirSt.Grounded) {
            var   mRef = new Godot.Vector3(v.X, 0.0f, v.Z);
            float rot  = Godot.Vector3.Forward.AngleTo(mRef)+XB.Constants.Pi;
            if (Godot.Vector3.Forward.Cross(mRef).Y < 0) {
                rot = 2.0f*XB.Constants.Pi -rot;
            }
            PModel.Rotation = new Godot.Vector3(0.0f, rot, 0.0f);
        }


        // AIMING
        if (PlAiming) {
            bool canShoot = true;
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
                canShoot = false;
            }

            if (XB.AData.Input.SRBot && canShoot) {
                //TODO[ALEX]
            }
            _fov = XB.AData.FovAim;
        } else if (Zoomed) {
            _fov = XB.AData.FovZoom;
        } else {
            _fov = XB.AData.FovDef;
        }

        var cAP                    = (Godot.CameraAttributesPhysical)_cam.Attributes;
            cAP.FrustumFocalLength = XB.Utils.LerpF(cAP.FrustumFocalLength, _fov, _cSmooth*dt);


        // ANIMATIONS
        float moveMode  = 0.0f;
        float idleMode  = 0.0f;
        var   walkBlend = new Godot.Vector2(0.0f, 0.0f);  // idle
        float walkSpeed = 0.0f;
        if (_plA == XB.AirSt.Grounded) {
            moveMode = 0.0f; // walk
            _blMove = XB.Utils.LerpF(_blMove, moveMode, _moveSm*dt);
            if (_plMoved) {
                idleMode = 1.0f; // walk
                if (PlAiming) {
                    walkBlend.X = XB.AData.Input.MoveX; // walk left/right
                    walkBlend.Y = XB.AData.Input.MoveY; // walk forwards/backwards
                } else {
                    walkBlend.Y = new Godot.Vector2(XB.AData.Input.MoveX, XB.AData.Input.MoveY).Length();
                }
                walkBlend /= walkBlend.Length();
                walkSpeed  = walkBlend.Length()*_walkAnm;
                float walkMult = 0.0f;
                switch (_move) {
                    case XB.MoveSt.Walk:   {walkMult = _walkSpd/_runSpd; break;}
                    case XB.MoveSt.Run:    {walkMult = 1.0f;             break;}
                    case XB.MoveSt.Sprint: {walkMult = _sprSpd/_runSpd;  break;}
                }
                walkBlend *= walkMult;
            } else {
                if (!PlAiming) { idleMode = 0.0f; } // idle
            }
            _blIdle = XB.Utils.LerpF(_blIdle, idleMode, _moveSm*dt);
            _blWalk = XB.Utils.LerpV2(_blWalk, walkBlend, _walkSm*dt);
        } else {
            moveMode = 1.0f; // jump
            _blMove = XB.Utils.LerpF(_blMove, moveMode, _moveSm*dt);
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

        // if (PlAiming) {
        //     _pATree.Set("parameters/aimingBlend/blend_amount", 1.0);
        //     float angleAmount = cAngle/90.0f; // -1.0 to 1.0
        //     if (_cam.GlobalPosition.Y > CCtrH.GlobalPosition.Y) angleAmount *= -1.0f;
        //     _pATree.Set("parameters/aimSpace/blend_position", angleAmount);
        // } else {
        //     _pATree.Set("parameters/aimingBlend/blend_amount", 0.0);
        // }


        // INPUTS
        if        (XB.AData.Input.Start) { // system menu
            Zoomed = false;
            //Menu.OpenMenu();
        } else if (XB.AData.Input.Select) { //
        } else {
            // DUp
            // DDown
            // DLeft
            // DRight
            // FUp
            // FDown - jump (handled earlier)
            // FLeft
            if (XB.AData.Input.FRight) { //
            }
            if (XB.AData.Input.SLTop) { //
            }
            // SLBot - aiming (handled earlier)
            if (XB.AData.Input.SRTop) { //
            }
            // if        (XB.AData.Input.Mode1 && WpnMode == XB.WpnMd.Impact) { //
            // } else if (XB.AData.Input.Mode2 && WpnMode == XB.WpnMd.Projectile) { //
            // }
            // SRBot - shooting (handled earlier)
        }

        // // DEBUG BUTTONS
        // if (XB.AData.Input.Debug1) {
        //     XB.Log.Msg("Debug 1 pressed", XB.D.PController_PhysicsProcess);
        // }
        // if (XB.AData.Input.Debug2) {
        //     XB.Log.Msg("Debug 2 pressed", XB.D.PController_PhysicsProcess);
        // }
        // if (XB.AData.Input.Debug3) {
        //     XB.Log.Msg("Debug 3 pressed", XB.D.PController_PhysicsProcess);
        //     XB.Geometry.SpawnMesh(XB.Geometry.CreateRingMesh(64, 16, 1.0f, 0.5f),
        //                           GlobalPosition + 1.0f*Godot.Vector3.Up        );
        // }
        // if (XB.AData.Input.Debug4) {
        //     XB.Log.Msg("Debug 4 pressed", XB.D.PController_PhysicsProcess);
        //     XB.Geometry.SpawnMesh(XB.Geometry.CreateCylinderMesh(16, 0.5f, 1.0f),
        //                           GlobalPosition + 1.0f*Godot.Vector3.Up         );
        // }
        // if (XB.AData.Input.Debug5) {
        //     XB.Log.Msg("Debug 5 pressed", XB.D.PController_PhysicsProcess);
        //     Hit(100.0f);
        // }


        // CLEANUP
        // to fix axes drifting apart due to imprecision:
        _cam.Transform  = _cam.Transform.Orthonormalized();
        CCtrH.Transform = CCtrH.Transform.Orthonormalized();
    }

    public void PlayerDied() {
        // var target = GlobalPosition + 100.0f*Godot.Vector3.Down;
        // var result = XB.Utils.Raycast(RequestSpaceState(), GlobalPosition, target, XB.LayerMasks.AimMask);
        // if (result.Count == 0) {
        //     XB.PersistData.FragmentsPos = GlobalPosition;
        // } else {
        //     XB.PersistData.FragmentsPos = (Godot.Vector3)result["position"];
        // }
        // XB.PersistData.FragmentsDrop  = XB.PersistData.Fragments;
        // XB.PersistData.FragmentsLevel = XB.Entries.LevelEntries[XB.AData.SceneID].Level;
        //
        // var toCam = _cam.GlobalPosition-CCtrH.GlobalPosition;
        //     toCam = toCam.Normalized();
        // var pos = CCtrH.GlobalPosition +  toCam*0.35f;
        // _effectDeath.InitializeEffect(XB.Utils.ConstructTransformFromV3(pos));
        // XB.PersistData.SaveDataWrite("PlayerDied");
        // DeathScreen.OpenDeathMenu();
    }

    public Godot.PhysicsDirectSpaceState3D RequestSpaceState() {
        return GetWorld3D().DirectSpaceState;
    }
}
} // namespace close
