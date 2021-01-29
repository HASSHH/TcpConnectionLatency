#version 330

in vec4 in_coord;
uniform mat4 vp_matrix;

void main(void){
    gl_Position = vp_matrix * in_coord;
}