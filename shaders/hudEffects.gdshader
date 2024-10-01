// create filmgrain overlay shader
shader_type canvas_item;

uniform sampler2D screenT : hint_screen_texture, repeat_disable, filter_nearest;

uniform float grainStr  = 0.035;
uniform float resMult   = 1.0; // grain resolution relative to 1080p
uniform float grainResX = 1920.0;
uniform float grainResY = 1080.0;

float random(vec2 st) {
    return fract(sin(dot(st, vec2(12.9898, 78.233)))*43758.5453123);
}

void fragment() {
    vec3 screen = texture(screenT, SCREEN_UV).rgb;
    vec3 color  = screen;

    vec2 grainRes  = vec2(grainResX, grainResY);
         grainRes /= resMult;
    vec2 uvGrain   = vec2(floor(grainRes.x*SCREEN_UV.x)/grainRes.x + fract(0.0000028*TIME),
                          floor(grainRes.y*SCREEN_UV.y)/grainRes.y - fract(0.0000011*TIME));

    float grain  = random(uvGrain);
          grain -= 0.5; // from 0.0|1.0 to -0.5|0.5
          grain *= grainStr;

    color += grain;

    COLOR.rgb = color;
}
