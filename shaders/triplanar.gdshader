shader_type spatial;
render_mode blend_mix, depth_draw_opaque, cull_back, diffuse_burley, specular_schlick_ggx;

uniform sampler2D tBlock  : source_color, filter_linear_mipmap, repeat_enable;

uniform sampler2D tAlbedo : source_color,          filter_linear_mipmap, repeat_enable;
uniform sampler2D tRM     : hint_default_black,    filter_linear_mipmap, repeat_enable;
uniform sampler2D tNormal : hint_roughness_normal, filter_linear_mipmap, repeat_enable;
uniform sampler2D tHeight : hint_default_black,    filter_linear_mipmap, repeat_enable;
uniform float     uvScale = 1.0;

varying vec2 uvX;
varying vec2 uvY;
varying vec2 uvZ;
varying vec3 axisBlendSmooth;
uniform float blendSharpen = 0.5;
uniform float blendWidth   = 3.0;
uniform float blendDepth   = 0.3;

// if sharpness is above 0.577, the corners will appear black because blend will be divided by 0
vec3 smoothTriplanarBlend(vec3 worldNormal, float sharpness) {
    vec3 blend  = abs(worldNormal.xyz);
         blend  = blend - sharpness;
         blend  = max(blend, 0.0);
         blend /= blend.x + blend.y + blend.z; // normalize the sum of components, 
                                               // not just the magnitude
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
                          float width, float heightBlendDepth                               ) {
    vec3 axisHeight;
    axisHeight.x  = texture(samplerHeight, uvx).r;
    axisHeight.y  = texture(samplerHeight, uvy).r;
    axisHeight.z  = texture(samplerHeight, uvz).r;
    axisHeight   += blend * blendWidth;

    float heightStart = max(max(axisHeight.x, axisHeight.y), axisHeight.z) - heightBlendDepth;

    blend  = max(axisHeight - vec3(heightStart), vec3(0.0, 0.0, 0.0));
    blend /= blend.x + blend.y + blend.z; // normalize the sum of components, 
                                          // not just the magnitude
    
    return blend;
}

vec3 triplanarSampleV3(vec3 blend, sampler2D samplerX, sampler2D samplerY, sampler2D samplerZ,
                       vec2 uvx, vec2 uvy, vec2 uvz                                           ) {
    vec3 sampleX = texture(samplerX, uvx).rgb;
    vec3 sampleY = texture(samplerY, uvy).rgb;
    vec3 sampleZ = texture(samplerZ, uvz).rgb;

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
    //scale uvs
    uvX *= uvScale;
    uvY *= uvScale;
    uvZ *= uvScale;

    axisBlendSmooth = smoothTriplanarBlend(NORMAL, blendSharpen);
}

void fragment() {
    vec3 albedo = vec3(0.0);
    vec3 rm     = vec3(0.0);
    vec3 normal = vec3(0.0);
    
    vec3 axisBlend = heightTriplanarBlend(axisBlendSmooth, tHeight, uvX, uvY, uvZ,
                                          blendWidth, blendDepth                  ); 

    albedo = triplanarSampleV3(axisBlend, tAlbedo, tAlbedo, tAlbedo, uvX, uvY, uvZ);
    rm     = triplanarSampleV3(axisBlend, tRM, tRM, tRM, uvX, uvY, uvZ);
    vec3 worldNrm = (INV_VIEW_MATRIX*vec4(NORMAL, 1.0)).rgb;
    normal = triplanarSampleV3(axisBlend, tNormal, tNormal, tNormal, uvX, uvY, uvZ);

    //albedo = triplanarVisualize(albedo, axisBlend);

    ALBEDO = albedo;
    ROUGHNESS = rm.r;
    METALLIC = rm.g;
    NORMAL_MAP = normal;
}
