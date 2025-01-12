// main shader for cone and dam segment geometry
shader_type spatial;
render_mode blend_mix, depth_prepass_alpha, cull_back, diffuse_burley, specular_schlick_ggx;

uniform vec4 cTopInner : source_color;
uniform vec4 cTopOuter : source_color;
uniform vec4 cBotUpper : source_color;
uniform vec4 cBotLower : source_color;

void fragment() {
    float maskTop = step(UV.y, 0.5);

    float fresnel = dot(NORMAL, VIEW);
          fresnel = 1.0 - fresnel;
          fresnel = pow(fresnel, 2.0);

    float waveAmnt = 1.0;
    float waveSpd  = 0.2;
    float wave  = waveAmnt*UV.x - waveSpd*TIME;
          wave  = fract(wave);
          wave -= 0.5;
          wave *= 2.0;
          wave  = abs(wave);
          wave  = pow(wave, 7.0);
          wave  = clamp(wave, 0.0, 1.0);
          wave *= 0.5; // add up to 1.0
          wave *= (1.0-maskTop); // wave only on sides

    float gradTop = UV.y*2.0;
          gradTop = pow(gradTop, 6.0); // 0.0 inside, 1.0 at edge
          gradTop = clamp(gradTop, 0.0, 1.0);

    float gradBot  = 2.0*UV.y-1.0;
          gradBot  = 1.0-gradBot; // 1.0 at top, 0.0 at bottom
          gradBot  = pow(gradBot, 16.0);
          gradBot *= 0.5;
          gradBot  = clamp(gradBot, 0.0, 1.0);
          gradBot += wave;

    vec3 albedoTop = mix(cTopInner.rgb, cTopOuter.rgb, gradTop);
    vec3 albedoBot = mix(cBotLower.rgb, cBotUpper.rgb, gradBot);
    vec3 albedo    = mix(albedoBot, albedoTop, maskTop);

    float roughness = 0.8 - 0.6*fresnel;

    ALBEDO    = albedo;
    ROUGHNESS = roughness;
}
