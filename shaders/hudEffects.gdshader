shader_type canvas_item;

uniform sampler2D screenT    : hint_screen_texture, repeat_disable, filter_nearest;

uniform sampler2D overMainT  : source_color, repeat_disable, filter_nearest;
uniform sampler2D overEdgesT : source_color, repeat_disable, filter_nearest;
uniform float     alphaMain  = 0.0;
uniform float     alphaEdges = 0.0;

uniform sampler2D chrAbMaskT : source_color, repeat_disable, filter_nearest;
uniform float     chrAbStr   = 0.0;

float random(vec2 st) {
    return fract(sin(dot(st, vec2(12.9898, 78.233)))*43758.5453123);
}

vec3 chromaticSlice(float t) {
	return max(vec3(1.0-t, 1.0-abs(t-1.0), t-1.0), 0.0);
}

void fragment() {
    vec2  uv     = UV; // I am using texture uvs here to make testing easier
                       // the result in the game is the same because the textures cover the entire screen
    vec3  screen = texture(screenT, SCREEN_UV).rgb;
    vec3  color  = vec3(0.0);
    float alpha  = 1.0;

    // chromatic aberration
    float gradient  = texture(chrAbMaskT, uv).r;
          gradient  = pow(gradient, 3.0);
          gradient *= 0.1;
    int   levels    = 3;
	vec3  sum       = vec3(0.0);
	vec2  offset    = SCREEN_UV-vec2(0.5);
          offset    = vec2(offset.x, -offset.y);
          offset   *= 0.02*(gradient*chrAbStr + chrAbStr);
	for(int i = 0; i < levels; i++){
		float t      = 2.0*float(i)/float(levels-1); // range 0.0->2.0
		vec3  slice  = max(chromaticSlice(t), 0.0);
              sum   += slice;
		vec2  slOff  = (t-1.0)*offset;
        	  color += slice*texture(screenT, SCREEN_UV+slOff).rgb;
	}
    color /= sum;

    // damage overlays
    vec3  oMain   = texture(overMainT,  uv).rgb;
    vec3  oEdges  = texture(overEdgesT, uv).rgb;
    vec3  oColor  = vec3(0.55, 0.06, 0.1);
          oMain  *= oColor*alphaMain;
          oEdges *= oColor*alphaEdges;
          color  += oMain;
          color  += oEdges;

    // film grain
    float res        = 1.0; // grain resolution relative to 1080p
    float grainResX  = 1920.0 / res; 
    float grainResY  = 1080.0 / res;
    vec2  uvGrain    = vec2(floor(grainResX*uv.x)/grainResX + fract(0.0000028*TIME),
                            floor(grainResY*uv.y)/grainResY - fract(0.0000011*TIME));
    float grain      = random(uvGrain);
          grain     -= 0.5; // from 0.0|1.0 to -0.5|0.5
    float grainStr   = 0.035;

          color     += grainStr*grain;

    COLOR.rgb = color;
}
