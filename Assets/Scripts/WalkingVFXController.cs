using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;
using DissolverVFX;

public class WalkingVFXController : MonoBehaviour
{
    [SerializeField]
    private SkinnedMeshDissolveBaker _skinnedMeshDissolveBaker;

    [SerializeField]
    private SkinnedMeshRenderer[] _targetSkinnedMeshRenderers = null;

    [SerializeField]
    private VisualEffect _visualEffect;

    // for debug
    [SerializeField]
    private MeshRenderer _debugPositionMapMeshRenderer;

    // for debug
    [SerializeField]
    private MeshRenderer _debugNormalMapMeshRenderer;

    // for debug
    [SerializeField]
    private MeshRenderer _debugAlphaMapMeshRenderer;

    private MaterialPropertyBlock[] _dissolveMaterialPropertyBlocks = null;

    // for debug
    private MaterialPropertyBlock _debugPositionMapMaterialPropertyBlock;
    private MaterialPropertyBlock _debugNormalMapMaterialPropertyBlock;
    private MaterialPropertyBlock _debugAlphaMapMaterialPropertyBlock;

    void Start()
    {
        // init material

        _dissolveMaterialPropertyBlocks = new MaterialPropertyBlock[_targetSkinnedMeshRenderers.Length];
        for (int i = 0; i < _targetSkinnedMeshRenderers.Length; i++)
        {
            _dissolveMaterialPropertyBlocks[i] = new MaterialPropertyBlock();
        }

        // for debug

        _debugPositionMapMaterialPropertyBlock = new MaterialPropertyBlock();
        _debugNormalMapMaterialPropertyBlock = new MaterialPropertyBlock();
        _debugAlphaMapMaterialPropertyBlock = new MaterialPropertyBlock();
    }

    void Update()
    {
        UpdateVFX();
        UpdateMaterials();

        // for debug

        DissolveParams dissolveParams = _skinnedMeshDissolveBaker.GetDissolveParams();

        _debugPositionMapMeshRenderer.GetPropertyBlock(_debugPositionMapMaterialPropertyBlock);
        _debugPositionMapMaterialPropertyBlock.SetTexture("_BaseMap", dissolveParams.positionMap);
        _debugPositionMapMeshRenderer.SetPropertyBlock(_debugPositionMapMaterialPropertyBlock);

        _debugNormalMapMeshRenderer.GetPropertyBlock(_debugNormalMapMaterialPropertyBlock);
        _debugNormalMapMaterialPropertyBlock.SetTexture("_BaseMap", dissolveParams.normalMap);
        _debugNormalMapMeshRenderer.SetPropertyBlock(_debugNormalMapMaterialPropertyBlock);

        _debugAlphaMapMeshRenderer.GetPropertyBlock(_debugAlphaMapMaterialPropertyBlock);
        _debugAlphaMapMaterialPropertyBlock.SetTexture("_BaseMap", dissolveParams.alphaMap);
        _debugAlphaMapMeshRenderer.SetPropertyBlock(_debugAlphaMapMaterialPropertyBlock);
    }

    void UpdateVFX() {
        DissolveParams dissolveParams = _skinnedMeshDissolveBaker.GetDissolveParams();
        _visualEffect.SetTexture("PositionMap", dissolveParams.positionMap);
        _visualEffect.SetTexture("NormalMap", dissolveParams.normalMap);
        _visualEffect.SetTexture("AlphaMap", dissolveParams.alphaMap);
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

        DissolveParams dissolveParams = _skinnedMeshDissolveBaker.GetDissolveParams();

        for (int i = 0; i < _targetSkinnedMeshRenderers.Length; i++)
        {
            SkinnedMeshRenderer skinnedMeshRenderer = _targetSkinnedMeshRenderers[i];
            MaterialPropertyBlock materialPropertyBlock = _dissolveMaterialPropertyBlocks[i];
            skinnedMeshRenderer.GetPropertyBlock(materialPropertyBlock);

            materialPropertyBlock.SetTexture(
                "Texture2D_54ef741b959443bd9e9b02b73af70d78",
                dissolveParams.dissolveMap
            );
            materialPropertyBlock.SetFloat(
                "Vector1_63f8f76926274e71baf1152131955b40",
                dissolveParams.dissolveRate
            );
            materialPropertyBlock.SetFloat(
                "Vector1_3a7f40d2e0244addbc31eb5c9f2b8f9d",
                dissolveParams.edgeFadeIn
            );
            materialPropertyBlock.SetFloat(
                "Vector1_851d11a93fec42da93e7eba4b6c35708",
                dissolveParams.edgeIn
            );
            materialPropertyBlock.SetFloat(
                "Vector1_44e9cc11c7704e7fbc993b924f68246d",
                dissolveParams.edgeOut
            );
            materialPropertyBlock.SetFloat(
                "Vector1_163470858c784a7cb704a8fc07733679",
                dissolveParams.edgeFadeOut
            );
            skinnedMeshRenderer.SetPropertyBlock(materialPropertyBlock);
        }


    // _debugPositionMapMeshRenderer.GetPropertyBlock(_debugPositionMapMaterialPropertyBlock);
    // _debugPositionMapMaterialPropertyBlock.SetTexture("_BaseMap", _dissolveBaker.positionMap);
    // _debugPositionMapMeshRenderer.SetPropertyBlock(_debugPositionMapMaterialPropertyBlock);

    // _debugNormalMapMeshRenderer.GetPropertyBlock(_debugNormalMapMaterialPropertyBlock);
    // _debugNormalMapMaterialPropertyBlock.SetTexture("_BaseMap", _dissolveBaker.normalMap);
    // _debugNormalMapMeshRenderer.SetPropertyBlock(_debugNormalMapMaterialPropertyBlock);

    // _debugAlphaMapMeshRenderer.GetPropertyBlock(_debugAlphaMapMaterialPropertyBlock);
    // _debugAlphaMapMaterialPropertyBlock.SetTexture("_BaseMap", _dissolveBaker.alphaMap);
    // _debugAlphaMapMeshRenderer.SetPropertyBlock(_debugAlphaMapMaterialPropertyBlock);
    }
}