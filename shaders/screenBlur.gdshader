shader_type canvas_item;

uniform sampler2D screenT : hint_screen_texture, repeat_disable, filter_nearest;
uniform float     bDir = 16.0;
uniform float     bQ   = 3.0;
uniform float     bSz  = 8.0;
uniform float     bStr = 1.0;

vec3 gaussianBlur(sampler2D tex, vec2 uv, float directions, float quality, float size, float strength) {
    vec4 scCol  = textureLod(tex, uv, 0.0);
    vec4 blCol  = textureLod(tex, uv, 0.0);
    vec2 radius = size/vec2(1920.0, 1080.0);
    for (float d = 0.0; d < TAU; d += TAU/directions) {
        for (float i = 1.0/quality; i <= 1.0; i += 1.0/quality) {
            vec2 offset = vec2(cos(d), sin(d))*radius*i;
            blCol += textureLod(tex, uv+offset, 0.0);
        }
    }
    blCol /= quality*directions - 15.0;
    return mix(scCol.rgb, blCol.rgb, strength);
}

void fragment() {
    //NOTE[ALEX]: here UV would also work, because the texture is covering the entire screen
    COLOR.rgb = gaussianBlur(screenT, SCREEN_UV, bDir, bQ, bSz, bStr);
}
