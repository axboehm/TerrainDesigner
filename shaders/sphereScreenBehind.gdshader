shader_type spatial;
render_mode blend_mix, depth_test_disabled, cull_back, diffuse_burley, specular_schlick_ggx;

uniform sampler2D tAlbedo   : source_color, filter_linear_mipmap, repeat_enable;
uniform sampler2D tRM       : hint_default_black, filter_linear_mipmap, repeat_enable;
uniform sampler2D tNormal   : hint_roughness_normal, filter_linear_mipmap, repeat_enable;
uniform sampler2D tEmission : source_color, filter_linear_mipmap, repeat_enable;
uniform sampler2D tWriting  : source_color, filter_nearest, repeat_enable;

uniform float scrollSpeed = 0.013;
uniform float emissionStr = 1.5;
uniform float fresnelPow  = 2.0;

void fragment() {
    vec3  albedo   = texture(tAlbedo,   UV).rgb;
    float rough    = texture(tRM,       UV).r;
    float metal    = texture(tRM,       UV).g;
    vec3  normal   = texture(tNormal,   UV).rgb;
    vec3  emission = texture(tEmission, UV).rgb;

    vec2  uvScroll    = vec2(UV.x, UV.y);
          uvScroll.x += TIME*scrollSpeed;
    float mask        = texture(tWriting, uvScroll).r;

    emission = mix(vec3(0.0), emission, mask);

    float fresnel = dot(NORMAL, VIEW);
          fresnel = 1.0 - fresnel;
          fresnel = pow(fresnel, fresnelPow);

    float alpha = fresnel + mask;

    ALBEDO     = albedo;
    ALPHA      = alpha;
    ROUGHNESS  = rough;
    METALLIC   = metal;
    NORMAL_MAP = normal;
    EMISSION   = emission*emissionStr;
}
