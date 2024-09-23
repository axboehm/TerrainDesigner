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

uniform sampler2D tAlbedoM3 : source_color,          filter_linear_mipmap, repeat_enable;
uniform sampler2D tRMM3     : hint_default_black,    filter_linear_mipmap, repeat_enable;
uniform sampler2D tNormalM3 : hint_roughness_normal, filter_linear_mipmap, repeat_enable;
uniform sampler2D tHeightM3 : hint_default_black,    filter_linear_mipmap, repeat_enable;
uniform float     uv3Scale  = 1.0;

uniform sampler2D tAlbedoM4 : source_color,          filter_linear_mipmap, repeat_enable;
uniform sampler2D tRMM4     : hint_default_black,    filter_linear_mipmap, repeat_enable;
uniform sampler2D tNormalM4 : hint_roughness_normal, filter_linear_mipmap, repeat_enable;
uniform sampler2D tHeightM4 : hint_default_black,    filter_linear_mipmap, repeat_enable;
uniform float     uv4Scale  = 1.0;

uniform sampler2D tBlock    : source_color, filter_linear_mipmap, repeat_enable;
uniform sampler2D tHeight   : source_color, filter_linear_mipmap, repeat_enable;
uniform sampler2D tNoiseP1  : source_color, filter_linear,        repeat_enable;
uniform sampler2D tNoiseP2  : source_color, filter_linear,        repeat_enable;
uniform sampler2D tPointy   : source_color, filter_linear_mipmap, repeat_enable;
uniform sampler2D tColShift : source_color, filter_linear_mipmap, repeat_enable;

//NOTE[ALEX]: with depth test, nothing can be drawn behind
//uniform sampler2D depthT    : hint_depth_texture;
//uniform vec4      colFog    : source_color;
//uniform float     fogDist = 4000.0; // zFar for depth calculation (in m)

uniform vec3  albVis    : source_color;
uniform float albVisStr = 0.0;

uniform float scaleX      = 1.0;
uniform float scaleY      = 1.0;
uniform float blockScale  = 1.0;
uniform float noisePScale = 0.1;
uniform float blockStr    = 1.0;
uniform float albedoMult  = 0.8;
uniform float blendDepth  = 0.2;
uniform float blendWidth  = 0.1;
uniform float blend34     = 0.25;
uniform float blend23     = 0.5;
uniform float blend12     = 0.75;
uniform float pointyStr   = 0.08;
uniform float pointyPow   = 3.0;
uniform float blendCStr   = 0.5;
uniform float blendCScale = 4.0;

varying vec2  uvX;
varying vec2  uvY;
varying vec2  uvZ;
varying vec3  axisBlendSmooth;
uniform float axisBlendSharpen = 0.5;
uniform float axisBlendWidth   = 3.0;
uniform float axisBlendDepth   = 0.3;

float v3Sum(vec3 v) {
    return (v.x + v.y + v.z);
}

// triplanar implementation based on: 
// "https://bgolus.medium.com/normal-mapping-for-a-triplanar-shader-10bf39dca05a#f41d"
//
// if sharpness is above 0.577, the corners will appear black because blend will be divided by 0
vec3 smoothTriplanarBlend(vec3 worldNormal, float sharpness) {
    vec3 blend  = abs(worldNormal.xyz);
         blend  = blend - sharpness;
         blend  = max(blend, 0.0);
         blend /= v3Sum(blend); // normalize the sum of components, not just the magnitude
    return blend;
}

// blend is the smooth triplanar blend vector with blend amounts for each axis
// width controls the width of the blend region, a good starting value is 3.0,
// smaller values widen the blend region, larger values sharpen the blend region
// heightBlendDepth is a relative value representing the depth of the blend as part of the whole 
// height range: 0.0 < heightDepth <= 1.0
//
// when reorienting uvs using triplanar projection, the tangent and bitangent vectors
// do not necessarily point in the correct directions anymore, this is fixed by swizzling
// tangent space is local to the surface of the triangle
// z axis is along the normal (world normal z axis) of the triangle and not stored in the texture
// x axis runs along the tangent (left right) and y axis along the bitangent (up down)
vec3 heightTriplanarBlend(vec3 blend, sampler2D samplerHeight, vec2 uvx, vec2 uvy, vec2 uvz,
                          float uvScaleX, float uvScaleY, float width, float heightBlendDepth) {
    vec3 axisHeight;
    axisHeight.x  = texture(samplerHeight, vec2(uvx.x*uvScaleX, uvx.y*uvScaleY)).r;
    axisHeight.y  = texture(samplerHeight, vec2(uvy.x*uvScaleX, uvy.y*uvScaleY)).r;
    axisHeight.z  = texture(samplerHeight, vec2(uvz.x*uvScaleX, uvz.y*uvScaleY)).r;
    axisHeight   += blend*width;

    float heightStart = max(max(axisHeight.x, axisHeight.y), axisHeight.z) - heightBlendDepth;

    blend  = max(axisHeight - vec3(heightStart), vec3(0.0, 0.0, 0.0));
    blend /= v3Sum(blend); // normalize the sum of components, not just the magnitude
    
    return blend;
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

vec3 triplanarSampleV3(vec3 blend, sampler2D sampler, vec2 uvx, vec2 uvy, vec2 uvz,
                       float uvScaleX, float uvScaleY                              ) {
    vec3 sampleX = texture(sampler, vec2(uvx.x*uvScaleX, uvx.y*uvScaleY)).rgb;
    vec3 sampleY = texture(sampler, vec2(uvy.x*uvScaleX, uvy.y*uvScaleY)).rgb;
    vec3 sampleZ = texture(sampler, vec2(uvz.x*uvScaleX, uvz.y*uvScaleY)).rgb;

    return (sampleX * blend.x + sampleY * blend.y + sampleZ * blend.z);
}

vec3 triplanarTextureBombSampleV3(vec3 blend, sampler2D sampler, vec2 uvx, vec2 uvy, vec2 uvz,
                                  float uvScaleX, float uvScaleY,
                                  sampler2D noiseSampler, float noiseScale                    ) {
    vec3 noise = triplanarSampleV3(blend, noiseSampler, uvx, uvy, uvz,
                                   uvScaleX*noiseScale, uvScaleY*noiseScale);
    vec3 sampleX = textureBomb(sampler, vec2(uvx.x*uvScaleX, uvx.y*uvScaleY), noise.x).rgb;
    vec3 sampleY = textureBomb(sampler, vec2(uvy.x*uvScaleX, uvy.y*uvScaleY), noise.y).rgb;
    vec3 sampleZ = textureBomb(sampler, vec2(uvz.x*uvScaleX, uvz.y*uvScaleY), noise.z).rgb;

    return (sampleX * blend.x + sampleY * blend.y + sampleZ * blend.z);
}

float triplanarTextureBombSampleF(vec3 blend, sampler2D sampler, vec2 uvx, vec2 uvy, vec2 uvz,
                                  float uvScaleX, float uvScaleY,
                                  sampler2D noiseSampler, float noiseScale                    ) {
    vec3  noise = triplanarSampleV3(blend, noiseSampler, uvx, uvy, uvz,
                                    uvScaleX*noiseScale, uvScaleY*noiseScale);
    float sampleX = textureBomb(sampler, vec2(uvx.x*uvScaleX, uvx.y*uvScaleY), noise.x).r;
    float sampleY = textureBomb(sampler, vec2(uvy.x*uvScaleX, uvy.y*uvScaleY), noise.y).r;
    float sampleZ = textureBomb(sampler, vec2(uvz.x*uvScaleX, uvz.y*uvScaleY), noise.z).r;

    return (sampleX * blend.x + sampleY * blend.y + sampleZ * blend.z);
}

vec3 triplanarVisualize(vec3 input, vec3 blend) {
    return (  blend.x * input * vec3(1.0, 0.0, 0.0)
            + blend.y * input * vec3(0.0, 1.0, 0.0)
            + blend.z * input * vec3(0.0, 0.0, 1.0));
}

void vertex() {
    // get world space tiling uvs
    vec3 worldVertex = (MODEL_MATRIX*vec4(VERTEX, 1.0)).rgb;
    uvX = worldVertex.zy;
    uvY = worldVertex.xz;
    uvZ = worldVertex.xy;
    // adjust uvs for coordinate system handedness
    uvX   *= -1.0;
    uvY   *= -1.0;
    uvZ.y *= -1.0;
    uvX.x *= sign(NORMAL.x);
    uvZ.x *= sign(NORMAL.z);

    axisBlendSmooth = smoothTriplanarBlend(NORMAL, axisBlendSharpen);
}

void fragment() {
    vec2 UVscaled    = UV; // uvs from mesh are projected from above
         UVscaled.x *= scaleX;
         UVscaled.y *= scaleY;
    float noiseP1  = texture(tNoiseP1, UVscaled*noisePScale).r;
    float noiseP2  = texture(tNoiseP2, UV).r;
          noiseP2  = pow(noiseP2, 3.0);

    vec3  axisBlend1  = heightTriplanarBlend(axisBlendSmooth, tHeightM1, uvX, uvY, uvZ,
                                             uv1Scale, uv1Scale, axisBlendWidth, axisBlendDepth);
    vec3  albedoM1    = triplanarTextureBombSampleV3(axisBlend1, tAlbedoM1, uvX, uvY, uvZ,
                                                     uv1Scale, uv1Scale, tNoiseP1, noisePScale);
    float roughnessM1 = triplanarTextureBombSampleF (axisBlend1, tRMM1, uvX, uvY, uvZ,
                                                     uv1Scale, uv1Scale, tNoiseP1, noisePScale);
    vec3  normalM1    = triplanarTextureBombSampleV3(axisBlend1, tNormalM1, uvX, uvY, uvZ,
                                                     uv1Scale, uv1Scale, tNoiseP1, noisePScale);
    float heightM1    = triplanarTextureBombSampleF (axisBlend1, tHeightM1, uvX, uvY, uvZ,
                                                     uv1Scale, uv1Scale, tNoiseP1, noisePScale);

    vec3  axisBlend2  = heightTriplanarBlend(axisBlendSmooth, tHeightM2, uvX, uvY, uvZ,
                                             uv2Scale, uv2Scale, axisBlendWidth, axisBlendDepth);
    vec3  albedoM2    = triplanarTextureBombSampleV3(axisBlend2, tAlbedoM2, uvX, uvY, uvZ,
                                                     uv2Scale, uv2Scale, tNoiseP1, noisePScale);
    float roughnessM2 = triplanarTextureBombSampleF (axisBlend2, tRMM2, uvX, uvY, uvZ,
                                                     uv2Scale, uv2Scale, tNoiseP1, noisePScale);
    vec3  normalM2    = triplanarTextureBombSampleV3(axisBlend2, tNormalM2, uvX, uvY, uvZ,
                                                     uv2Scale, uv2Scale, tNoiseP1, noisePScale);
    float heightM2    = triplanarTextureBombSampleF (axisBlend2, tHeightM2, uvX, uvY, uvZ,
                                                     uv2Scale, uv2Scale, tNoiseP1, noisePScale);

    vec3  axisBlend3  = heightTriplanarBlend(axisBlendSmooth, tHeightM3, uvX, uvY, uvZ,
                                             uv3Scale, uv3Scale, axisBlendWidth, axisBlendDepth);
    vec3  albedoM3    = triplanarTextureBombSampleV3(axisBlend3, tAlbedoM3, uvX, uvY, uvZ,
                                                     uv3Scale, uv3Scale, tNoiseP1, noisePScale);
    float roughnessM3 = triplanarTextureBombSampleF (axisBlend3, tRMM3, uvX, uvY, uvZ,
                                                     uv3Scale, uv3Scale, tNoiseP1, noisePScale);
    vec3  normalM3    = triplanarTextureBombSampleV3(axisBlend3, tNormalM3, uvX, uvY, uvZ,
                                                     uv3Scale, uv3Scale, tNoiseP1, noisePScale);
    float heightM3    = triplanarTextureBombSampleF (axisBlend3, tHeightM3, uvX, uvY, uvZ,
                                                     uv3Scale, uv3Scale, tNoiseP1, noisePScale);

    vec3  axisBlend4  = heightTriplanarBlend(axisBlendSmooth, tHeightM4, uvX, uvY, uvZ,
                                             uv4Scale, uv4Scale, axisBlendWidth, axisBlendDepth);
    vec3  albedoM4    = triplanarTextureBombSampleV3(axisBlend4, tAlbedoM4, uvX, uvY, uvZ,
                                                     uv4Scale, uv4Scale, tNoiseP1, noisePScale);
    float roughnessM4 = triplanarTextureBombSampleF (axisBlend4, tRMM4, uvX, uvY, uvZ,
                                                     uv4Scale, uv4Scale, tNoiseP1, noisePScale);
    vec3  normalM4    = triplanarTextureBombSampleV3(axisBlend4, tNormalM4, uvX, uvY, uvZ,
                                                     uv4Scale, uv4Scale, tNoiseP1, noisePScale);
    float heightM4    = triplanarTextureBombSampleF (axisBlend4, tHeightM4, uvX, uvY, uvZ,
                                                     uv4Scale, uv4Scale, tNoiseP1, noisePScale);

    float maskBlockX  = fract(UVscaled.x*blockScale);
          maskBlockX  = step(maskBlockX, 0.5);
    float maskBlockY  = fract(UVscaled.y*blockScale);
          maskBlockY  = step(maskBlockY, 0.5);
    float maskBlock   = maskBlockX + maskBlockY;
          maskBlock  /= 2.0;
          maskBlockX  = step(maskBlock, 0.25);
          maskBlockY  = step(maskBlock, 0.75);
          maskBlock   = maskBlockY - maskBlockX; // dark tiles are in white area
    float block = texture(tBlock, UVscaled*blockScale).r;

    float height  = texture(tHeight, UV).r;
          height += 0.1*(noiseP1-0.5);
    float blend   = blendWidth;
          blend  += 0.3*(height-0.5);
    float mask34  = smoothstep(blend34 - blend, blend34 + blend, height - noiseP2);
    float mask23  = smoothstep(blend23 - blend, blend23 + blend, height - noiseP2);
    float mask12  = smoothstep(blend12 - blend, blend12 + blend, height - noiseP2);

    vec3  albedo = heightBlendV3(albedoM3, albedoM4, heightM3, heightM4,
                                 mask34, 1.0-mask34, blendDepth         );
          albedo = heightBlendV3(albedoM2, albedo, heightM2, heightM3,
                                 mask23, 1.0-mask23, blendDepth       );
          albedo = heightBlendV3(albedoM1, albedo, heightM1, heightM2,
                                 mask12, 1.0-mask12, blendDepth       );

    float pointy  = texture(tPointy, UV).r;
          pointy  = pow(pointy, pointyPow);
          pointy -= 0.5;
          pointy *= pointyStr;

          albedo += pointy;

    float blendColor  = height - 0.5;
          blendColor  = pow(blendColor, 3.0);
          blendColor *= blendCStr;
    vec3  albedoColShifted  = texture(tColShift, UV*blendCScale).rgb;
          albedoColShifted *= blendColor;

          albedo += albedoColShifted;

    //float depth = texture(depthT, SCREEN_UV).x;
    //float scaledDepth  = PROJECTION_MATRIX[3][2] / (depth + PROJECTION_MATRIX[2][2]);
    //      scaledDepth /= fogDist;

    //      scaledDepth = pow(scaledDepth, 1.3);
    //vec3  albedoLight = mix(albedo, colFog.rgb, 0.8);

    //      albedo = mix(albedo, albedoLight, scaledDepth); // lighten terrain color in distance
          albedo = mix(albedo, albVis, albVisStr);
          albedo = mix(albedo, albedo * block, blockStr * maskBlock        ); // dark tiles
          albedo = mix(albedo, albedo + block, blockStr * (1.0 - maskBlock)); // light tiles

          //albedo = triplanarVisualize(albedo, axisBlendSmooth);
          //albedo = vec3(UV.x, UV.y, 0.0);
          //albedo = vec3(texture(tPointy, UV).r);
          //albedo = vec3(noiseP2);

    float roughness = heightBlendF(roughnessM3, roughnessM4, heightM3, heightM4,
                                   mask34, 1.0-mask34, blendDepth               );
          roughness = heightBlendF(roughnessM2, roughness, heightM2, heightM3,
                                   mask23, 1.0-mask23, blendDepth             );
          roughness = heightBlendF(roughnessM1, roughness, heightM1, heightM2,
                                   mask12, 1.0-mask12, blendDepth             );
          //roughness = mix(roughness, 1.0, scaledDepth); // make distant terrain less glossy
          roughness = mix(roughness, 1.0, albVisStr);

    vec3 normal = heightBlendV3(normalM3, normalM4, heightM3, heightM4,
                                mask34, 1.0-mask34, blendDepth         );
         normal = heightBlendV3(normalM2, normal, heightM2, heightM3,
                                mask23, 1.0-mask23, blendDepth       );
         normal = heightBlendV3(normalM1, normal, heightM1, heightM2,
                                mask12, 1.0-mask12, blendDepth       );

    ALBEDO     = albedo;
    ROUGHNESS  = roughness;
    NORMAL_MAP = normal;
}
