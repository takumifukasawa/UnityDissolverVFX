#ifndef MATH_UTILS_INCLUDED
#define MATH_UTILS_INCLUDED

float saturate(float x) {
    return clamp(x, 0, 1);
}

#endif