// shader for sphere shell, has mask for areas that show status of sphere (highlighted, linking)
shader_type spatial;
render_mode blend_mix, depth_draw_opaque, cull_back, diffuse_burley, specular_schlick_ggx;

uniform sampler2D tAlbedo   : source_color, filter_linear_mipmap, repeat_enable;
uniform sampler2D tRM       : hint_default_black, filter_linear_mipmap, repeat_enable;
uniform sampler2D tNormal   : hint_roughness_normal, filter_linear_mipmap, repeat_enable;
uniform sampler2D tEmission : source_color, filter_linear_mipmap, repeat_enable;
uniform sampler2D tMask     : source_color, filter_nearest, repeat_enable;

uniform vec4  highlightCol  : source_color;
uniform float highlightMult = 1.0;
uniform float emissionStr   = 1.5;

void fragment() {
    vec3  albedo   = texture(tAlbedo,   UV).rgb;
    float rough    = texture(tRM,       UV).r;
    float metal    = texture(tRM,       UV).g;
    vec3  normal   = texture(tNormal,   UV).rgb;
    vec3  emission = texture(tEmission, UV).rgb;
    float maskHl   = texture(tMask,     UV).r;
    float maskAlw  = 1.0 - maskHl;

    vec3 emissionAlw = mix(vec3(0.0), emission, maskAlw);
         maskHl      = maskHl + (0.2*sin(2.0*TIME));
    vec3 emissionHl  = mix(vec3(0.0), emission*highlightCol.rgb, maskHl*highlightMult);

    ALBEDO     = albedo;
    ROUGHNESS  = rough;
    METALLIC   = metal;
    NORMAL_MAP = normal;
    EMISSION   = (emissionAlw+emissionHl)*emissionStr;
}
