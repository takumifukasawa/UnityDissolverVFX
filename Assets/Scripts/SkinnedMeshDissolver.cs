using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine.VFX;
using System.Linq;
using DissolverVFX;

public class SkinnedMeshDissolver : MonoBehaviour
{
    [SerializeField]
    private SkinnedMeshRenderer[] _targetSkinnedMeshRenderers = null;

    [SerializeField]
    private Transform _rootTransform;

    [SerializeField]
    private ComputeShader _computeShader;

    [SerializeField]
    private Texture2D _dissolveMap;

    [SerializeField, Range(0, 1)]
    private float _dissolveRate = 0.5f;

    [SerializeField, Range(0, 1)]
    private float _edgeFadeIn = 0.48f;

    [SerializeField, Range(0, 1)]
    private float _edgeIn = 0.49f;

    [SerializeField, Range(0, 1)]
    private float _edgeOut = 0.51f;

    [SerializeField, Range(0, 1)]
    private float _edgeFadeOut = 0.52f;

    [SerializeField, Range(0, 10)]
    private float _timeMultiplier = 1f;

    [SerializeField]
    private VisualEffect _visualEffect;

    [SerializeField]
    private int _destMapWidth = 512;

    [SerializeField]
    private int _destMapHeight = 512;

    [SerializeField, Range(0, 1)]
    private float _dissolveThreshold = 0.5f;

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

    private DissolveBaker _dissolveBaker;

    void Start()
    {
        Mesh[] meshes = new Mesh[_targetSkinnedMeshRenderers.Length];
        for(int i = 0; i < _targetSkinnedMeshRenderers.Length; i++) {
            meshes[i] = _targetSkinnedMeshRenderers[i].sharedMesh;
        }

        _dissolveBaker = new DissolveBaker(Instantiate(_computeShader), meshes, _dissolveMap, _destMapWidth, _destMapHeight);

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

        // ExecCompute();
        _dissolveBaker.Initialize();
    }

    void Update()
    {
        ExecCompute();
    }

    void ExecCompute() {

        _dissolveBaker.Bake(
            _targetSkinnedMeshRenderers,
            _rootTransform.localToWorldMatrix,
            _dissolveRate,
            _edgeFadeIn,
            _edgeIn,
            _edgeOut,
            _edgeFadeOut,
            _timeMultiplier,
            _destMapWidth,
            _destMapHeight,
            _dissolveThreshold
        );

        _visualEffect.SetTexture("PositionMap", _dissolveBaker.positionMap);
        _visualEffect.SetTexture("NormalMap", _dissolveBaker.normalMap);
        _visualEffect.SetTexture("AlphaMap", _dissolveBaker.alphaMap);

        UpdateMaterials();

        // for debug

        _debugPositionMapMeshRenderer.GetPropertyBlock(_debugPositionMapMaterialPropertyBlock);
        _debugPositionMapMaterialPropertyBlock.SetTexture("_BaseMap", _dissolveBaker.positionMap);
        _debugPositionMapMeshRenderer.SetPropertyBlock(_debugPositionMapMaterialPropertyBlock);

        _debugNormalMapMeshRenderer.GetPropertyBlock(_debugNormalMapMaterialPropertyBlock);
        _debugNormalMapMaterialPropertyBlock.SetTexture("_BaseMap", _dissolveBaker.normalMap);
        _debugNormalMapMeshRenderer.SetPropertyBlock(_debugNormalMapMaterialPropertyBlock);

        _debugAlphaMapMeshRenderer.GetPropertyBlock(_debugAlphaMapMaterialPropertyBlock);
        _debugAlphaMapMaterialPropertyBlock.SetTexture("_BaseMap", _dissolveBaker.alphaMap);
        _debugAlphaMapMeshRenderer.SetPropertyBlock(_debugAlphaMapMaterialPropertyBlock);
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

        for (int i = 0; i < _targetSkinnedMeshRenderers.Length; i++)
        {
            SkinnedMeshRenderer skinnedMeshRenderer = _targetSkinnedMeshRenderers[i];
            MaterialPropertyBlock materialPropertyBlock = _dissolveMaterialPropertyBlocks[i];
            skinnedMeshRenderer.GetPropertyBlock(materialPropertyBlock);

            materialPropertyBlock.SetTexture(
                "Texture2D_54ef741b959443bd9e9b02b73af70d78",
                _dissolveMap
            );
            materialPropertyBlock.SetFloat(
                "Vector1_63f8f76926274e71baf1152131955b40",
                _dissolveRate
            );
            materialPropertyBlock.SetFloat(
                "Vector1_3a7f40d2e0244addbc31eb5c9f2b8f9d",
                _edgeFadeIn
            );
            materialPropertyBlock.SetFloat(
                "Vector1_851d11a93fec42da93e7eba4b6c35708",
                _edgeIn
            );
            materialPropertyBlock.SetFloat(
                "Vector1_44e9cc11c7704e7fbc993b924f68246d",
                _edgeOut
            );
            materialPropertyBlock.SetFloat(
                "Vector1_163470858c784a7cb704a8fc07733679",
                _edgeFadeOut
            );
            skinnedMeshRenderer.SetPropertyBlock(materialPropertyBlock);
        }
    }

    void OnDisable()
    {
        Dispose();
    }

    void OnDestroy()
    {
        Dispose();
    }

    void Dispose()
    {
        _dissolveBaker.Dispose();
    }
}
