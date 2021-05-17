using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;
using DissolverVFX;

[RequireComponent(typeof(MeshDissolveBaker))]
public class MeshDissolveLitVFXController : MonoBehaviour
{
    [SerializeField]
    private MeshDissolveBaker _meshDissolveBaker;

    [SerializeField]
    private MeshRenderer[] _targetMeshRenderers = null;

    [SerializeField]
    private VisualEffect _visualEffect;

    private MaterialPropertyBlock[] _dissolveMaterialPropertyBlocks = null;

    void Start()
    {
        // init material

        _dissolveMaterialPropertyBlocks = new MaterialPropertyBlock[_targetMeshRenderers.Length];
        for (int i = 0; i < _targetMeshRenderers.Length; i++)
        {
            _dissolveMaterialPropertyBlocks[i] = new MaterialPropertyBlock();
        }
    }

    void Update()
    {
        UpdateVFX();
        UpdateMaterials();
    }

    void UpdateVFX() {
        _visualEffect.SetTexture("PositionMap", _meshDissolveBaker.positionMap);
        _visualEffect.SetTexture("NormalMap", _meshDissolveBaker.normalMap);
        _visualEffect.SetTexture("AlphaMap", _meshDissolveBaker.alphaMap);
    }

    void UpdateMaterials()
    {
        // # DissolveMap
        // Texture2D_54ef741b959443bd9e9b02b73af70d78
        // # DissolveRate
        // Vector1_63f8f76926274e71baf1152131955b40
        // # DissolveEdgeFadeIn
        // Vector1_3a7f40d2e0244addbc31eb5c9f2b8f9d
        // # DissolveEdgeIn
        // Vector1_851d11a93fec42da93e7eba4b6c35708
        // # DissolveEdgeOut
        // Vector1_44e9cc11c7704e7fbc993b924f68246d
        // # DissolveEdgeFadeOut
        // Vector1_163470858c784a7cb704a8fc07733679

        for (int i = 0; i < _targetMeshRenderers.Length; i++)
        {
            MeshRenderer meshRenderer = _targetMeshRenderers[i];
            MaterialPropertyBlock materialPropertyBlock = _dissolveMaterialPropertyBlocks[i];
            meshRenderer.GetPropertyBlock(materialPropertyBlock);

            materialPropertyBlock.SetTexture(
                "Texture2D_54ef741b959443bd9e9b02b73af70d78",
                _meshDissolveBaker.dissolveMap
            );
            materialPropertyBlock.SetFloat(
                "Vector1_63f8f76926274e71baf1152131955b40",
                _meshDissolveBaker.dissolveRate
            );
            materialPropertyBlock.SetFloat(
                "Vector1_3a7f40d2e0244addbc31eb5c9f2b8f9d",
                _meshDissolveBaker.edgeFadeIn
            );
            materialPropertyBlock.SetFloat(
                "Vector1_851d11a93fec42da93e7eba4b6c35708",
                _meshDissolveBaker.edgeIn
            );
            materialPropertyBlock.SetFloat(
                "Vector1_44e9cc11c7704e7fbc993b924f68246d",
                _meshDissolveBaker.edgeOut
            );
            materialPropertyBlock.SetFloat(
                "Vector1_163470858c784a7cb704a8fc07733679",
                _meshDissolveBaker.edgeFadeOut
            );
            meshRenderer.SetPropertyBlock(materialPropertyBlock);
        }
    }
}