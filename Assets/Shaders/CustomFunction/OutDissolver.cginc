#ifndef OUT_DISSOLVER_INCLUDED
#define OUT_DISSOLVER_INCLUDED

#include "Assets/Shaders/Includes/Dissolver.cginc"

void OutDissolver_float(
    // in
    float DissolveInput,
    float DissolveRate,
    float EdgeFadeIn,
    float EdgeIn,
    float EdgeOut,
    float EdgeFadeOut,
    // out
    out float DissolveEdge,
    out float DissolveUnder,
    out float DissolveOver
) {
    Dissolver o = dissolver(
        DissolveInput,
        DissolveRate,
        EdgeFadeIn,
        EdgeIn,
        EdgeOut,
        EdgeFadeOut
    );
    DissolveEdge = o.DissolveEdge;
    DissolveUnder = o.DissolveUnder;
    DissolveOver = o.DissolveOver;
}

#endif