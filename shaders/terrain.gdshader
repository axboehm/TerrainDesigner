
shader_type spatial;
render_mode blend_mix, depth_draw_opaque, cull_disabled, diffuse_burley, specular_schlick_ggx;

void fragment() {
    vec2 uvs    = vec2(UV.x, UV.y);
    uvs *= 8.0;
    vec3 albedo = vec3(fract(uvs.x), fract(uvs.y), 0.0);

    ALBEDO = albedo;
    ROUGHNESS = 1.0;
}
