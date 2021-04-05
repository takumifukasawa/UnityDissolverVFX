using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Dissolver : MonoBehaviour
{
    [SerializeField]
    private MeshRenderer _dissolveLitMeshRenderer;

    [SerializeField]
    private ComputeShader _computeShader;

    [SerializeField]
    private Texture2D _dissolveMap;

    // [SerializeField]
    // private float _dissolveInput;

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

    [SerializeField]
    private MeshRenderer _debugPlaneMeshRenderer;

    // [SerializeField]
    private RenderTexture _destMap;

    // [SerializeField]
    // private Material _debugPlane;

    private MaterialPropertyBlock _dissolveLitMeshMaterialPropertyBlock;
    private MaterialPropertyBlock _debugPlaneMaterialPropertyBlock;

    // private RenderTexture _destMap;

    private int kernelID;

    // Start is called before the first frame update
    void Start()
    {
        _destMap = new RenderTexture(
            _dissolveMap.width,
            _dissolveMap.height,
            0,
            RenderTextureFormat.ARGB32,
            RenderTextureReadWrite.Linear
        );
        _destMap.enableRandomWrite = true;
        _destMap.Create();

        kernelID = _computeShader.FindKernel("CSMain");

        _computeShader.SetTexture(kernelID, "srcTexture", _dissolveMap);
        _computeShader.SetTexture(kernelID, "destTexture", _destMap);

        // Debug.Log(_dissolveMap.format);

        _dissolveLitMeshMaterialPropertyBlock = new MaterialPropertyBlock();
        _debugPlaneMaterialPropertyBlock = new MaterialPropertyBlock();
    }

    // Update is called once per frame
    void Update()
    {
        // _computeShader.SetFloat("dissolveInput", _dissolveInput);
        _computeShader.SetFloat("dissolveRate", _dissolveRate);
        _computeShader.SetFloat("edgeFadeIn", _edgeFadeIn);
        _computeShader.SetFloat("edgeFadeIn", _edgeFadeIn);
        _computeShader.SetFloat("edgeIn", _edgeIn);
        _computeShader.SetFloat("edgeOut", _edgeOut);
        _computeShader.SetFloat("edgeFadeOut", _edgeFadeOut);

        _computeShader.Dispatch(
            kernelID,
            _dissolveMap.width,
            _dissolveMap.height,
            1
        );

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

        _dissolveLitMeshRenderer.GetPropertyBlock(_dissolveLitMeshMaterialPropertyBlock);
        // _dissolveLitMeshMaterialPropertyBlock.SetTexture("_DissolveMap", _dissolveMap);
        _dissolveLitMeshMaterialPropertyBlock.SetTexture(
            "Texture2D_54ef741b959443bd9e9b02b73af70d78",
            _dissolveMap
        // _destMap
        );
        _dissolveLitMeshMaterialPropertyBlock.SetFloat(
            "Vector1_63f8f76926274e71baf1152131955b40",
            _dissolveRate
        );
        _dissolveLitMeshMaterialPropertyBlock.SetFloat(
            "Vector1_3a7f40d2e0244addbc31eb5c9f2b8f9d",
            _edgeFadeIn
        );
        _dissolveLitMeshMaterialPropertyBlock.SetFloat(
            "Vector1_851d11a93fec42da93e7eba4b6c35708",
            _edgeIn
        );
        _dissolveLitMeshMaterialPropertyBlock.SetFloat(
            "Vector1_44e9cc11c7704e7fbc993b924f68246d",
            _edgeOut
        );
        _dissolveLitMeshMaterialPropertyBlock.SetFloat(
            "Vector1_163470858c784a7cb704a8fc07733679",
            _edgeFadeOut
        );
        _dissolveLitMeshRenderer.SetPropertyBlock(_dissolveLitMeshMaterialPropertyBlock);

        _debugPlaneMeshRenderer.GetPropertyBlock(_debugPlaneMaterialPropertyBlock);
        _debugPlaneMaterialPropertyBlock.SetTexture("_BaseMap", _destMap);
        _debugPlaneMeshRenderer.SetPropertyBlock(_debugPlaneMaterialPropertyBlock);

        // _debugPlane.SetTexture("_BaseMap", _destMap);
    }
}
