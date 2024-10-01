// shader for cone and dam segment geometry that always shows, 
// regardless of whether it is occluded by other geometry
shader_type spatial;
render_mode blend_mix, depth_test_disabled, cull_back, diffuse_burley, specular_schlick_ggx;

uniform vec4 cTopInner : source_color;
uniform vec4 cTopOuter : source_color;
uniform vec4 cBotUpper : source_color;
uniform vec4 cBotLower : source_color;

void fragment() {
    float maskTop = step(UV.y, 0.5);

    float fresnel = dot(NORMAL, VIEW);
          fresnel = 1.0 - fresnel;
          fresnel = pow(fresnel, 2.0);

    float gradTop  = UV.y*2.0;
          gradTop  = pow(gradTop, 6.0); // 0.0 inside, 1.0 at edge
          gradTop *= 0.8; 
          gradTop  = clamp(gradTop, 0.0, 1.0);

    float gradBot  = 2.0*UV.y-1.0;
          gradBot  = 1.0-gradBot; // 1.0 at top, 0.0 at bottom
          gradBot *= 0.9;
          gradBot  = pow(gradBot, 6.0);
          gradBot *= 0.5;
          gradBot  = clamp(gradBot, 0.0, 1.0);

    vec3 albedoTop = mix(cTopInner.rgb, cTopOuter.rgb, gradTop);
    vec3 albedoBot = mix(cBotLower.rgb, cBotUpper.rgb, gradBot);
    vec3 albedo    = mix(albedoBot, albedoTop, maskTop);

    float alpha = gradTop*maskTop + gradBot*(maskTop-1.0);

    float roughness = 1.0;

    ALBEDO    = albedo;
    ALPHA     = alpha;
    ROUGHNESS = roughness;
}
