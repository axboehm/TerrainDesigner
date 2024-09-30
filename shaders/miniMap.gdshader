shader_type canvas_item;

uniform sampler2D heightMapT : source_color, repeat_disable, filter_nearest;

uniform vec4 plColor     : source_color;
uniform vec4 plColorLink : source_color;

uniform float plPosX     = 0.5; // as relative of heightmap texture (ends up halfway up the triangle)
uniform float plPosZ     = 0.5;
                                // assign direction as normalized vector
uniform float plDirX     = 0.0; // positive goes right in texture
uniform float plDirZ     = 1.0; // positive goes down in texture
uniform float halfWidth  = 0.04;
uniform float halfHeight = 0.06;
varying vec2 tri0; // tip of triangle
varying vec2 tri1; // bottom left of triangle
varying vec2 tri2; // bottom right of triangle

uniform vec2[64] spPos; //NOTE[ALEX]: has to be changed manually, undefined length is not supported
uniform vec4[64] spCol;

uniform float spRadius    = 0.1;
uniform float squareRatio = 1.0;

uniform float linking   = 1.0;
uniform float alphaMult = 1.0;

// sdf functions based on: https://iquilezles.org/articles/distfunctions2d/
float sdfTriangle(vec2 p, vec2 t0, vec2 t1, vec2 t2) {
    vec2 e0 = t1-t0;
    vec2 e1 = t2-t1;
    vec2 e2 = t0-t2;
    vec2 v0 = p -t0;
    vec2 v1 = p -t1;
    vec2 v2 = p -t2;
    vec2 pq0 = v0 - e0*clamp(dot(v0, e0)/dot(e0, e0), 0.0, 1.0);
    vec2 pq1 = v1 - e1*clamp(dot(v1, e1)/dot(e1, e1), 0.0, 1.0);
    vec2 pq2 = v2 - e2*clamp(dot(v2, e2)/dot(e2, e2), 0.0, 1.0);
    float s = sign(e0.x*e2.y - e0.y*e2.x);
    vec2 d = min(min(vec2(dot(pq0, pq0), s*(v0.x*e0.y - v0.y*e0.x)),
                     vec2(dot(pq1, pq1), s*(v1.x*e1.y - v1.y*e1.x))),
                     vec2(dot(pq2, pq2), s*(v2.x*e2.y - v2.y*e2.x))  );
    return (-sqrt(d.x) * sign(d.y));
}

float sdfCircle(vec2 p, vec2 c0, float r) {
    float dist = length(c0-p);
    return (dist - r);
}

//TODO[ALEX]: probably the coordinates to be adjusted for the squareRatio and then use squared UVs

// multiply x part with sizeY/sizeX to keep the shape of the triangle consistent
void vertex() {
    vec2 plDir   = vec2(plDirX*squareRatio,  plDirZ);
    vec2 sideDir = vec2(plDirZ*squareRatio, -plDirX);
    tri0 = vec2(plPosX, plPosZ) + halfHeight*plDir;
    tri1 = vec2(plPosX, plPosZ) - halfHeight*plDir - halfWidth*sideDir;
    tri2 = vec2(plPosX, plPosZ) - halfHeight*plDir + halfWidth*sideDir;
}

void fragment() {
    vec4 color = texture(heightMapT, UV).rrra;

    float distPl = sdfTriangle(UV, tri0, tri1, tri2);
    float maskPl = step(distPl, 0.0);

    float time         = TIME/2.0;
    float pulse        = sin(time*3.14159265359 +3.14159265359/2.0);
          pulse        = pow(abs(pulse), 12.0);
    float pulseThick   = 0.008;
    float pulseSingle  = fract(time);
          pulseSingle  = clamp(pulseSingle, 0.0, 1.0-pulseThick);
    float pulseRep     = 64.0;
    float triGrad      = fract(pulseRep*distPl + time);
          triGrad     *= pow(1.0-distPl, 16.0);
          triGrad     += 0.2*pulse*triGrad;
          maskPl      += triGrad;
          maskPl       = clamp(maskPl, 0.0, 1.0);

    for (int i = 0; i < spPos.length(); i++) {
        float distSp = sdfCircle(UV, vec2(spPos[i]), spRadius);
        float maskSp = step(distSp, 0.0);
        color = mix(color, spCol[i], maskSp);
    }

    vec4 colorPlayer = mix(plColor, plColorLink, linking);
    color = mix(color, colorPlayer, maskPl);
    color.a *= alphaMult;

    COLOR = color;
}
