shader_type spatial;
render_mode blend_mix, depth_draw_opaque, cull_back, diffuse_burley, specular_schlick_ggx;

void fragment() {
    ALBEDO    = vec3(0.0, 0.0, 0.0);
    ROUGHNESS = 0.0;
}
