shader_type canvas_item;

uniform float vignette_intensity : hint_range(0.0, 2.0) = 0.0;
uniform float vignette_multiplier : hint_range(1.0, 2.0) = 1.0;
uniform vec4 vignette_rgb : source_color = vec4(0.0, 0.0, 0.0, 1.0);



void fragment(){
	float dist = distance(UV, vec2(0.5)) * vignette_intensity;
	COLOR = vec4(vignette_rgb.rgb * (2.0-dist), dist*vignette_multiplier);
}