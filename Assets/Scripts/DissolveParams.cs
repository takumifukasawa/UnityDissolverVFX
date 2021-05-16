using UnityEngine;

namespace DissolverVFX
{
    public struct DissolveParams {
        public RenderTexture positionMap;
        public RenderTexture normalMap;
        public RenderTexture alphaMap;
        public Texture2D dissolveMap;
        public float dissolveRate;
        public float edgeFadeIn;
        public float edgeIn;
        public float edgeOut;
        public float edgeFadeOut;
    }
}