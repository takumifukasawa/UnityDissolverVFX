#ifndef OUT_DISSOLVER_INCLUDED
#define OUT_DISSOLVER_INCLUDED

#include "Assets/Shaders/Includes/Dissolver.cginc"

void OutDissolver_float(float a, float b, out float Out) {
    Out = dissolver(a) + b;
}

#endif