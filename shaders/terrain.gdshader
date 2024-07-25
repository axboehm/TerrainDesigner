shader_type spatial;
render_mode blend_mix, depth_draw_opaque, cull_back, diffuse_burley, specular_schlick_ggx;

uniform sampler2D tAlbedoM1 : source_color, filter_linear_mipmap, repeat_enable;
uniform sampler2D tRMM1     : hint_default_black, filter_linear_mipmap, repeat_enable;
uniform sampler2D tNormalM1 : hint_roughness_normal, filter_linear_mipmap, repeat_enable;
uniform float     uv1Scale  = 1.0;

uniform sampler2D tAlbedoM2 : source_color, filter_linear_mipmap, repeat_enable;
uniform sampler2D tRMM2     : hint_default_black, filter_linear_mipmap, repeat_enable;
uniform sampler2D tNormalM2 : hint_roughness_normal, filter_linear_mipmap, repeat_enable;
uniform float     uv2Scale  = 1.0;

uniform sampler2D tBlock : source_color, filter_linear_mipmap, repeat_enable;

uniform float     scaleX     = 1.0;
uniform float     scaleY     = 1.0;
uniform float     blockScale = 1.0;
uniform float     blockStr   = 1.0;
uniform float     heightMin  = -1.0;
uniform float     heightMax  = +1.0;
uniform float     albedoMult = 0.8;

varying float     heightGrad;

void vertex() {
    heightGrad  = VERTEX.y - heightMin;
    heightGrad /= (-heightMin + heightMax);
}

void fragment() {
    vec2 UVscaled    = vec2(UV.x, UV.y);
         UVscaled   /= 2.0; // block texture is 2m x 2m
         UVscaled.x *= scaleX;
         UVscaled.y *= scaleY;

    vec2  uv1          = UVscaled*uv1Scale;
    vec3  albedoM1     = texture(tAlbedoM1, uv1).rgb;
          albedoM1    *= albedoMult;
    float roughnessM1  = texture(tRMM1, uv1).r;
    vec3  normalM1     = texture(tNormalM1, uv1).rgb;

    vec2  uv2          = UVscaled*uv2Scale;
    vec3  albedoM2     = texture(tAlbedoM2, uv2).rgb;
          albedoM2    *= albedoMult;
    float roughnessM2  = texture(tRMM2, uv2).r;
    vec3  normalM2     = texture(tNormalM2, uv2).rgb;

    float block = texture(tBlock, UVscaled*blockScale).r;
    float mask1 = heightGrad;
          mask1 = 2.0*mask1;
          mask1 = pow(mask1, 2.4);
          mask1 = clamp(mask1, 0.0, 1.0);

    vec3  albedo = mix(albedoM2, albedoM1, mask1);
          albedo = mix(albedo, albedo*block, blockStr);;

    float roughness = mix(roughnessM2, roughnessM1, mask1);;

    vec3  normal = mix(normalM2, normalM1, mask1);

    ALBEDO     = albedo;
    ROUGHNESS  = roughness;
    NORMAL_MAP = normal;
}
