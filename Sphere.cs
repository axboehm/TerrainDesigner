namespace XB { // namespace open
using SysCG = System.Collections.Generic;
public partial class Sphere : Godot.CharacterBody3D {
    [Godot.Export] private Godot.NodePath        _sphereMesh;
                   private Godot.ShaderMaterial  _shellMat;
                   private Godot.ShaderMaterial  _screenMat;
    [Godot.Export] private Godot.AnimationPlayer _animPl;
                   private Godot.Image           _imgScrolling;
                   private Godot.ImageTexture    _texScrolling;
    [Godot.Export] private Godot.OmniLight3D     _sphereLight;

    public  int   ID          = 0;
    public  bool  Active      = false;
    public  float Highlighted = 0.0f;
    private float _hlMult     = 0.0f;
    private float _hlSm       = 12.0f;
    private const int _repeats    = 8;   // how often the id gets repeated in the texture
    private const int _dimScrollX = 128; //NOTE[ALEX]: based on textures created for sphere
    private const int _dimScrollY = 16;
    private const int _dimThick   = 1;   // thickness of digits
    private const int _dimDigitX  = _dimScrollY/2 - 2*_dimThick;
    private const int _dimDigitY  = _dimScrollY   - 2*_dimThick;
    private Godot.Color _colBlack = new Godot.Color(0.0f, 0.0f, 0.0f, 1.0f);
    private Godot.Color _colWhite = new Godot.Color(1.0f, 1.0f, 1.0f, 1.0f);

    // public static Godot.Color   SphereColor  = new Godot.Color(0.6f, 1.0f, 0.6f, 1.0f);
    private Godot.Color _sphereColor  = new Godot.Color(1.0f, 0.37f, 0.43f, 1.0f);
    private Godot.Color _lightActCol  = new Godot.Color(1.0f, 1.0f, 1.0f, 1.0f);
    private Godot.Color _lightLinkCol = new Godot.Color(1.0f, 0.88f, 0.0f, 1.0f);
    private const float _sphEmitStr   = 2.1f;
    private const float _sphScrSpeed  = 0.013f;
    private const float _lightStrAct  = 0.8f;
    private const float _lightStrLink = 1.5f;
    private       float _lightStrTar  = 0.0f;
    private       float _lightStr     = 0.0f;
    private       float _lSm          = 5.0f;

    public SysCG.List<XB.Sphere> _linkedSpheres = new SysCG.List<XB.Sphere>();

    public void InitializeSphere(int id) {
        ID     = id;
        Active = false;

        CollisionLayer = XB.LayerMasks.SphereLayer;

        _shellMat  = (Godot.ShaderMaterial)GetNode<Godot.MeshInstance3D>
                         (_sphereMesh).GetSurfaceOverrideMaterial(0);
        _screenMat = (Godot.ShaderMaterial)GetNode<Godot.MeshInstance3D>
                         (_sphereMesh).GetSurfaceOverrideMaterial(1);
        _sphereLight.LightColor = _lightActCol;

        _shellMat.SetShaderParameter ("highlightCol",  _sphereColor);
        _shellMat.SetShaderParameter ("highlightMult", 0.0f                    );
        _shellMat.SetShaderParameter ("emissionStr",   _sphEmitStr );
        _screenMat.SetShaderParameter("scrollSpeed",   _sphScrSpeed);
        _screenMat.SetShaderParameter("emissionStr",   _sphEmitStr );

        _imgScrolling = Godot.Image.Create(_dimScrollX, _dimScrollY, false, Godot.Image.Format.Rgba8);
        _imgScrolling.Fill(_colBlack);

        for (int i = 0; i < _repeats; i++) {
            int xStart = i*(_dimScrollX/_repeats);
            int width  = _dimDigitX + 2*_dimThick;
            if (ID > 9)  { width += _dimDigitX; }
            if (ID > 99) { width += _dimDigitX; }
            var numberField = new Godot.Rect2I(xStart, _dimThick, width, _dimDigitY);
            _imgScrolling.FillRect(numberField, _colBlack);

            if (ID > 9) {
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
        _hlMult     = XB.Utils.LerpF(_hlMult, Highlighted, _hlSm*dt);
        Highlighted = 0.0f;
        _shellMat.SetShaderParameter ("highlightMult", _hlMult);
        _lightStr = XB.Utils.LerpF(_lightStr, _lightStrTar, _lSm*dt);
        _sphereLight.LightEnergy = _lightStr;
    }

    // player places sphere in world
    public void PlaceSphere(Godot.Vector3 pos) {
        Show();
        GlobalPosition = pos;
        Active = true;
        _sphereLight.LightColor = _lightActCol;
        _lightStrTar = _lightStrAct;
        XB.Manager.UpdateActiveSpheres();
        XB.PController.Hud.UpdateSphereTexture(ID);
    }

    // when linking this sphere with other spheres
    public void LinkSphere() {
        if (_animPl.CurrentAnimation != "expand") { _animPl.Play("expand"); }
        // add linked spheres to list
        // update other linked spheres
        _sphereLight.LightColor = _lightLinkCol;
        _lightStrTar = _lightStrLink;
    }

    public void UnlinkSphere() {
        if (_animPl.CurrentAnimation != "contract") { _animPl.Play("contract"); }
        // remove linked spheres list
        // remove sphere from linked spheres' lists
        // update all affected spheres
        _sphereLight.LightColor = _lightActCol;
        _lightStrTar = _lightStrAct;
    }

    // remove sphere from world
    public void RemoveSphere() {
        // remove sphere dam geometry
        Hide();
        Active = false;
        _animPl.Stop();
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
