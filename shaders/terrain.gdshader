shader_type spatial;
render_mode blend_mix, depth_draw_opaque, cull_back, diffuse_burley, specular_schlick_ggx;

uniform sampler2D tAlbedoM1 : source_color, filter_linear_mipmap, repeat_enable;
uniform sampler2D tRMM1     : hint_default_black, filter_linear_mipmap, repeat_enable;
uniform sampler2D tNormalM1 : hint_roughness_normal, filter_linear_mipmap, repeat_enable;
uniform sampler2D tHeightM1 : hint_default_black, filter_linear_mipmap, repeat_enable;
uniform float     uv1Scale  = 1.0;

uniform sampler2D tAlbedoM2 : source_color, filter_linear_mipmap, repeat_enable;
uniform sampler2D tRMM2     : hint_default_black, filter_linear_mipmap, repeat_enable;
uniform sampler2D tNormalM2 : hint_roughness_normal, filter_linear_mipmap, repeat_enable;
uniform sampler2D tHeightM2 : hint_default_black, filter_linear_mipmap, repeat_enable;
uniform float     uv2Scale  = 1.0;

uniform sampler2D tBlock  : source_color, filter_linear_mipmap, repeat_enable;
uniform sampler2D tHeight : source_color, filter_linear_mipmap, repeat_enable;
uniform sampler2D tNoise  : source_color, filter_linear_mipmap, repeat_enable;

uniform float     scaleX     = 1.0;
uniform float     scaleY     = 1.0;
uniform float     blockScale = 1.0;
uniform float     noiseScale = 0.1;
uniform float     blockStr   = 1.0;
uniform float     albedoMult = 0.8;


float v3Sum(vec3 v) {
    return (v.x + v.y + v.z);
}

// texture bombing based on "https://iquilezles.org/articles/texturerepetition/"
vec3 textureBomb(sampler2D tAlbedo, vec2 uv, float noise) {
    float index = noise*8.0f; // divide into 8 different offset regions
    float i     = floor(index);
    float f     = fract(index);
          f     = abs(f-0.5);

    vec2 offA = sin(vec2(3.0, 7.0)*(i+0.0));
    vec2 offB = sin(vec2(3.0, 7.0)*(i+1.0));

    vec3 colA = texture(tAlbedo, uv+offA).rgb;
    vec3 colB = texture(tAlbedo, uv+offB).rgb;

    float amnt = smoothstep(0.2, 0.8, f-0.1*v3Sum(colA-colB));
    return mix(colA, colB, amnt);
}

// height based texture blending based on "https://www.gamedeveloper.com/programming/advanced-terrain-texture-splatting"
float heightBlend(float heightA, float heightB, float opacityA, float opacityB, float depth) {
    float ma  = max(heightA + opacityA, heightB + opacityB) - depth;
    float b1  = max(heightA + opacityA - ma, 0);
    float b2  = max(heightB + opacityB - ma, 0);
    float res1 = (2.0*b1)/(b1+b2); // returns opacity of material A only
    //float res2 = (2.0*b2)/(b1+b2);
    return res1;
}

void fragment() {
    vec2 UVscaled    = vec2(UV.x, UV.y);
         UVscaled   /= 2.0; // block texture is 2m x 2m
         UVscaled.x *= scaleX;
         UVscaled.y *= scaleY;

    float noise = texture(tNoise, UVscaled*noiseScale).r;

    vec2  uv1          = UVscaled*uv1Scale;
    vec3  albedoM1     = textureBomb(tAlbedoM1, uv1, noise);
          albedoM1    *= albedoMult;
          //albedoM1     = vec3(1.0, 0.0, 0.0); // for visualization
    float roughnessM1  = textureBomb(tRMM1, uv1, noise).r;
    vec3  normalM1     = textureBomb(tNormalM1, uv1, noise);
    float heightM1     = textureBomb(tHeightM1, uv1, noise).r;

    vec2  uv2          = UVscaled*uv2Scale;
    vec3  albedoM2     = textureBomb(tAlbedoM2, uv2, noise);
          albedoM2    *= albedoMult;
          //albedoM2     = vec3(0.0, 0.0, 1.0); // for visualization
    float roughnessM2  = textureBomb(tRMM2, uv2, noise).r;
    vec3  normalM2     = textureBomb(tNormalM2, uv2, noise);
    float heightM2     = textureBomb(tHeightM2, uv2, noise).r;

    float block = texture(tBlock, UVscaled*blockScale).r;
    float mask1 = texture(tHeight, UV).r;
          mask1 = 2.0*mask1;
          mask1 = pow(mask1, 2.4);
          mask1 = clamp(mask1, 0.0, 1.0);

    float mask2 = heightBlend(heightM1, heightM2, mask1, 1.0-mask1, 0.2);

    vec3  albedo = mix(albedoM2, albedoM1, mask2);
          albedo = mix(albedo, albedo*block, blockStr);;

    float roughness = mix(roughnessM2, roughnessM1, mask2);;

    vec3  normal = mix(normalM2, normalM1, mask2);

    ALBEDO     = albedo;
    ROUGHNESS  = roughness;
    NORMAL_MAP = normal;
}
