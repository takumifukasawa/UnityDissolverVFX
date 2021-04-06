using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

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
    private GameObject _targetObject;

    private MeshFilter _targetMeshFilter;
    private Mesh _targetMesh;

    [SerializeField]
    private VisualEffect _visualEffect;

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

    private ComputeBuffer _verticesBuffer;

    // Start is called before the first frame update
    void Start()
    {
        // init member

        _targetMeshFilter = _targetObject.GetComponent<MeshFilter>();
        _targetMesh = _targetMeshFilter.mesh;

        // init textures

        _destMap = CreateTexture(
            _dissolveMap.width,
            _dissolveMap.height
        );
        _destMap.Create();

        // init buffer

        Vector3[] vertices = _targetMesh.vertices;
        _verticesBuffer = new ComputeBuffer(vertices.Length, sizeof(float) * 3);
        _verticesBuffer.SetData(vertices);

        // init compute shader

        kernelID = _computeShader.FindKernel("CSMain");

        // _computeShader.SetBuffer(kernelID, "Triangles", _verticesBuffer);

        _computeShader.SetTexture(kernelID, "SrcTexture", _dissolveMap);
        _computeShader.SetTexture(kernelID, "DestTexture", _destMap);

        // init material

        _dissolveLitMeshMaterialPropertyBlock = new MaterialPropertyBlock();
        _debugPlaneMaterialPropertyBlock = new MaterialPropertyBlock();
    }

    // Update is called once per frame
    void Update()
    {
        // _computeShader.SetFloat("dissolveInput", _dissolveInput);
        _computeShader.SetBuffer(kernelID, "VerticesBuffer", _verticesBuffer);
        _computeShader.SetFloat("DissolveRate", _dissolveRate);
        _computeShader.SetFloat("EdgeFadeIn", _edgeFadeIn);
        _computeShader.SetFloat("EdgeFadeIn", _edgeFadeIn);
        _computeShader.SetFloat("EdgeIn", _edgeIn);
        _computeShader.SetFloat("EdgeOut", _edgeOut);
        _computeShader.SetFloat("EdgeFadeOut", _edgeFadeOut);
        _computeShader.SetMatrix("Transform", _targetObject.transform.localToWorldMatrix);

        _computeShader.Dispatch(
            kernelID,
            _dissolveMap.width,
            _dissolveMap.height,
            1
        );

        _visualEffect.SetTexture("PositionMap", _destMap);

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

    void OnDisable()
    {
        Dispose();
    }

    void OnDestroy()
    {
        Dispose();
    }

    RenderTexture CreateTexture(int width, int height)
    {
        RenderTexture map = new RenderTexture(
            width,
            height,
            0,
            RenderTextureFormat.ARGBFloat,
            RenderTextureReadWrite.Linear
        );
        map.enableRandomWrite = true;
        map.hideFlags = HideFlags.DontSave;
        return map;
    }

    // https://github.com/keijiro/Smrvfx/blob/898ced94e22c28fca591a1a268fdb034dec43292/Packages/jp.keijiro.smrvfx/Runtime/Internal/Utility.cs#L15
    void DestroyObj(Object o)
    {
        if (o == null) return;
        if (Application.isPlaying)
            Object.Destroy(o);
        else
            Object.DestroyImmediate(o);
    }

    void Dispose()
    {
        _verticesBuffer?.Dispose();
        _verticesBuffer = null;

        DestroyObj(_destMap);
    }
}
