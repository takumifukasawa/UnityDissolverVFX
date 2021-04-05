#ifndef DISSOLVER_INCLUDED
#define DISSOLVER_INCLUDED

#include "Assets/Shaders/Includes/MathUtils.cginc"

struct Dissolver {
    float DissolveEdge;
    float DissolveUnder;
    float DissolveOver;
};

Dissolver dissolver(
    float DissolveInput,
    float DissolveRate,
    float EdgeFadeIn,
    float EdgeIn,
    float EdgeOut,
    float EdgeFadeOut
) {
    float input = saturate(DissolveInput) - 1;
    float rate = saturate(DissolveRate) * 2;
    float dissolve = input + rate;
    float dissolveOver = smoothstep(
        EdgeFadeIn,
        EdgeIn,
        dissolve
    );
    float dissolveUnder = 1 - smoothstep(
        EdgeOut,
        EdgeFadeOut,
        dissolve
    );
    float dissolveEdge = dissolveOver * dissolveUnder;
    Dissolver o;
    o.DissolveEdge = dissolveEdge;
    o.DissolveUnder = dissolveUnder;
    o.DissolveOver = dissolveOver;
    return o;
}

#endif