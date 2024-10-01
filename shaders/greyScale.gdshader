// simple shader that shows a texture with only a red channel into greyscale
shader_type canvas_item;

uniform sampler2D texToGreyT : source_color, repeat_disable, filter_nearest;

void fragment() {
    vec4 color = texture(texToGreyT, UV).rrra;

    COLOR = color;
}
