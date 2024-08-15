shader_type spatial;
render_mode blend_mix, depth_draw_opaque, cull_back, diffuse_burley, specular_schlick_ggx;

uniform sampler2D tAlbedoM1 : source_color,          filter_linear_mipmap, repeat_enable;
uniform sampler2D tRMM1     : hint_default_black,    filter_linear_mipmap, repeat_enable;
uniform sampler2D tNormalM1 : hint_roughness_normal, filter_linear_mipmap, repeat_enable;
uniform sampler2D tHeightM1 : hint_default_black,    filter_linear_mipmap, repeat_enable;
uniform float     uv1Scale  = 1.0;

uniform sampler2D tAlbedoM2 : source_color,          filter_linear_mipmap, repeat_enable;
uniform sampler2D tRMM2     : hint_default_black,    filter_linear_mipmap, repeat_enable;
uniform sampler2D tNormalM2 : hint_roughness_normal, filter_linear_mipmap, repeat_enable;
uniform sampler2D tHeightM2 : hint_default_black,    filter_linear_mipmap, repeat_enable;
uniform float     uv2Scale  = 1.0;

uniform sampler2D tBlock  : source_color, filter_linear_mipmap, repeat_enable;
uniform sampler2D tHeight : source_color, filter_linear_mipmap, repeat_enable;
uniform sampler2D tNoiseP : source_color, filter_linear,        repeat_enable;

uniform vec3  albVis    : source_color;
uniform float albVisStr = 0.0;

uniform float scaleX      = 1.0;
uniform float scaleY      = 1.0;
uniform float blockScale  = 1.0;
uniform float noisePScale = 0.1;
uniform float blockStr    = 1.0;
uniform float albedoMult  = 0.8;
uniform float blendDepth  = 0.2;

float v3Sum(vec3 v) {
    return (v.x + v.y + v.z);
}

// texture bombing based on "https://iquilezles.org/articles/texturerepetition/"
// additional information: 
// "https://developer.nvidia.com/gpugems/gpugems/part-iii-materials/chapter-20-texture-bombing"
vec3 textureBomb(sampler2D tAlbedo, vec2 uv, float noise) {
    vec2 duvdx = dFdx(uv); // calculate derivatives to remove mipmapping artifacts at region bounds
    vec2 duvdy = dFdy(uv);

    float index = noise*8.0; // divide into 8 different offset regions
    float i     = floor(index);
    float f     = fract(index);

    vec2 offA = sin(vec2(3.0, 7.0)*(i+0.0));
    vec2 offB = sin(vec2(3.0, 7.0)*(i+1.0));

    vec2 uvA = uv+offA;
    vec2 uvB = uv+offB;

    vec3 colA = textureGrad(tAlbedo, uvA, duvdx, duvdy).rgb;
    vec3 colB = textureGrad(tAlbedo, uvB, duvdx, duvdy).rgb;

    float amnt   = smoothstep(0.2, 0.8, f-0.1*v3Sum(colA-colB));
          amnt   = clamp(amnt, 0.0, 1.0);
    vec3  result = mix(colA, colB, amnt);
    return result;
}

// height based texture blending (texture splatting) based on
// "https://www.gamedeveloper.com/programming/advanced-terrain-texture-splatting"
vec3 heightBlendV3(vec3 vecA, vec3 vecB, float heightA, float heightB,
                   float opacityA, float opacityB, float depth        ) {
    float ma  = max(heightA + opacityA, heightB + opacityB) - depth;
    float blA = max(heightA + opacityA - ma, 0);
    float blB = max(heightB + opacityB - ma, 0);
    vec3  res = (vecA*blA + vecB*blB) / (blA+blB);
    return res;
}
float heightBlendF(float floatA, float floatB, float heightA, float heightB,
                   float opacityA, float opacityB, float depth              ) {
    float ma  = max(heightA + opacityA, heightB + opacityB) - depth;
    float blA = max(heightA + opacityA - ma, 0);
    float blB = max(heightB + opacityB - ma, 0);
    float res = (floatA*blA + floatB*blB) / (blA+blB);
    return res;
}

void fragment() {
    vec2 UVscaled    = vec2(UV.x, UV.y);
         UVscaled.x *= scaleX;
         UVscaled.y *= scaleY;

    float noiseP = texture(tNoiseP, UVscaled*noisePScale).r;

    vec2  uv1          = UVscaled*uv1Scale;
    vec3  albedoM1     = textureBomb(tAlbedoM1, uv1, noiseP);
    vec3  albedoM1Dark = albedoM1*albedoMult;
          //albedoM1     = vec3(1.0, 0.0, 0.0); // for visualization
    float roughnessM1  = textureBomb(tRMM1,     uv1, noiseP).r;
    vec3  normalM1     = textureBomb(tNormalM1, uv1, noiseP);
    float heightM1     = textureBomb(tHeightM1, uv1, noiseP).r;

    vec2  uv2          = UVscaled*uv2Scale;
    vec3  albedoM2     = textureBomb(tAlbedoM2, uv2, noiseP);
    vec3  albedoM2Dark = albedoM2*albedoMult;
          //albedoM2     = vec3(0.0, 0.0, 1.0); // for visualization
    float roughnessM2  = textureBomb(tRMM2,     uv2, noiseP).r;
    vec3  normalM2     = textureBomb(tNormalM2, uv2, noiseP);
    float heightM2     = textureBomb(tHeightM2, uv2, noiseP).r;

    float block = texture(tBlock, UVscaled*blockScale).r;
    float mask1 = texture(tHeight, UV).r;

    float mask2 = pow(mask1, 0.5);
          mask2 = clamp(mask2, 0.0, 1.0);

          mask1 = 2.0*mask1;
          mask1 = pow(mask1, 2.4);
          mask1 = clamp(mask1, 0.0, 1.0);

          albedoM1 = mix(albedoM1Dark, albedoM1, mask2);
          albedoM2 = mix(albedoM2Dark, albedoM2, mask2);

    vec3  albedo = heightBlendV3(albedoM1, albedoM2, heightM1, heightM2,
                                 mask1, 1.0-mask1, blendDepth           );
          albedo = mix(albedo, albedo*albVis, albVisStr);
          albedo = mix(albedo, albedo*block, blockStr);;
          //albedo = vec3(UV.x, UV.y, 0.0);

    float roughness = heightBlendF(roughnessM1, roughnessM2, heightM1, heightM2,
                                   mask1, 1.0-mask1, blendDepth                 );

    vec3  normal = heightBlendV3(normalM1, normalM2, heightM1, heightM2,
                                 mask1, 1.0-mask1, blendDepth           );

    ALBEDO     = albedo;
    ROUGHNESS  = roughness;
    NORMAL_MAP = normal;
}
