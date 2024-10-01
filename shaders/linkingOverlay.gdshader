// shader that creates two circle segments to show that the player is in linking mode
// segments are created by combining two signed distance fields (sdfs)
shader_type canvas_item;

uniform vec4 colInnerBright : source_color;
uniform vec4 colInnerDim    : source_color;
uniform vec4 colOutline     : source_color;

uniform float squareRatio = 1.777777778; // 16.0/9.0
uniform float ringRad     = 0.6;
uniform float ringThick   = 0.88;
uniform float ringOutline = 0.05;
uniform float gradientStr = 1.0;
uniform float outlineStr  = 0.6;

uniform float alphaMult = 1.0;

float sdfCircle(vec2 p, vec2 c0, float r) {
    float dist = length(c0-p);
    return (dist - r);
}

// sdf functions based on: https://iquilezles.org/articles/distfunctions2d/
// r1 is distance from center to corners at top, r2 is distance to corners at bottom,
// h is half height, c0 is center
float sdfTrapezoidYAligned(vec2 p, vec2 c0, float r1, float r2, float h) {
    p -= c0;
    vec2 k1 = vec2(r2, h);
    vec2 k2 = vec2(r2-r1, 2.0*h);
    p.x = abs(p.x);
    float a = r2;
    if (p.y < 0.0) { a = r1; }
    vec2 ca = vec2(p.x - min(p.x, a), abs(p.y) - h);
    vec2 cb = p - k1 + k2*clamp(dot(k1 - p, k2)/dot(k2, k2), 0.0, 1.0);
    float s = -1.0;
    if (cb.x < 0.0 && ca.y < 0.0) { s = 1.0; };
    return s*sqrt( min(dot(ca, ca), dot(cb, cb)) );
}

void fragment() {
    vec4 color = vec4(0.0, 0.0, 0.0, 0.0);

    vec2 uvSq = vec2(UV.x*squareRatio, UV.y);

    vec2 ctr = vec2(0.5*squareRatio, 0.5);

    float maskRing = sdfCircle(uvSq, ctr, ringRad);
          maskRing = 1.0 - abs(maskRing);
          maskRing = pow(maskRing, 36.0);
    float maskOutline = step(maskRing, ringThick-ringOutline);
          maskOutline = 1.0 - maskOutline;
    float maskInner = step(maskRing, ringThick);
          maskInner = 1.0 - maskInner;
    float gradient  = UV.y;
          gradient  = pow(gradient, 3.0);
          gradient  = clamp(gradient, 0.0, 1.0);

    float trapREdge = 0.63;
    float trapRCtr = 0.13;
    float trapTop = sdfTrapezoidYAligned(uvSq, ctr - vec2(0.0, 0.25), trapREdge, trapRCtr, 0.25);
    float trapBot = sdfTrapezoidYAligned(uvSq, ctr + vec2(0.0, 0.25), trapRCtr, trapREdge, 0.25);
    trapTop = step(trapTop, 0.0);
    trapTop = 1.0-abs(trapTop);
    trapBot = step(trapBot, 0.0);
    trapBot = 1.0-abs(trapBot);
    float maskTrap = trapTop+trapBot;

    vec4 colorInner   = mix(colInnerBright, colInnerDim, gradient*gradientStr);
    vec4 colorOutline = mix(colorInner, colOutline, outlineStr);
    color = mix(colorOutline, colorInner, maskInner);

    float alpha = maskOutline - maskTrap;
    color.a = alpha*alphaMult;

    COLOR = color;
}
