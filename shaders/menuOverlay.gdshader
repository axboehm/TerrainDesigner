// visual overlay for pause menu, creates some movement to avoid menu screen appearing static
shader_type canvas_item;
render_mode blend_add;

uniform sampler2D overlayT  : source_color, repeat_disable, filter_nearest;
uniform sampler2D gradientT : source_color, repeat_disable, filter_nearest;

uniform vec2  res          = vec2(1920.0, 1080.0);
uniform float eThick       = 50.0;
uniform float squareFactor = 1.777777778; // 16.0/9.0;

void fragment() {
    vec4  gridTex   = texture(overlayT, UV);
    float gradient  = texture(gradientT, UV).r;
          gradient  = gradient + pow(gradient, 2.0);
          gradient /= 2.0;

    float edgeL    = 1.0 - step(eThick        /res.x, UV.x);
    float edgeR    =       step((res.x-eThick)/res.x, UV.x);
    float edgeT    = 1.0 - step(eThick        /res.y, UV.y);
    float edgeB    =       step((res.y-eThick)/res.y, UV.y);
    float edgeMask = 1.0 - edgeL - edgeR - edgeT - edgeB;
          edgeMask = clamp(edgeMask, 0.0, 1.0);

    vec2  uvSquared    = vec2(UV.x*squareFactor, UV.y);
    vec2  nearestPt    = vec2(clamp(uvSquared.x, squareFactor*eThick/res.x,
                                                 squareFactor*(res.x-eThick)/res.x),
                              clamp(uvSquared.y, eThick/res.y, (res.y-eThick)/res.y));
    float sdf          = distance(uvSquared, nearestPt);
          sdf         *= res.y/eThick;
          sdf          = 1.0 - sdf;

    float time        = TIME/2.0;
    // pulse and pulseSingle are synchronized to have their peaks at the same time
    float pulse       = sin(time*3.14159265359 +3.14159265359/2.0);
          pulse       = pow(abs(pulse), 12.0);
    float pulseThick  = 0.08;
    float pulseSingle = fract(time);
          pulseSingle = clamp(pulseSingle, 0.0, 1.0-pulseThick);

    float sdfInv    = 1.0-sdf;
    float edgeGrad  = step(sdfInv, pulseSingle+pulseThick);
          edgeGrad -= step(sdfInv, pulseSingle);
          edgeGrad *= gridTex.r;

    vec4  color  = gridTex;
          color *= 1.0 + 0.5*pulse;
          color += edgeGrad;
          color *= sdf;
          color *= gradient;
          color *= 0.52;

    COLOR = color;
}
