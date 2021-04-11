using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

public class Dissolver : MonoBehaviour
{

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


    private MeshRenderer _targetMeshRenderer;

    private MeshFilter _targetMeshFilter;
    private Mesh _targetMesh;

    [SerializeField]
    private VisualEffect _visualEffect;

    [SerializeField]
    private MeshRenderer _debugPlaneMeshRenderer;

    // [SerializeField]
    private RenderTexture _positionMap;
    private RenderTexture _alphaMap;

    [SerializeField]
    private int _destMapWidth = 512;

    [SerializeField]
    private int _destMapHeight = 512;

    [SerializeField, Range(0, 1)]
    private float _dissolveThreshold = 0.5f;

    // [SerializeField]
    // private Material _debugPlane;

    private MaterialPropertyBlock _dissolveLitMeshMaterialPropertyBlock;
    private MaterialPropertyBlock _debugPlaneMaterialPropertyBlock;

    // private RenderTexture _positionMap;

    private int kernelID;

    private ComputeBuffer _trianglesBuffer;

    private ComputeBuffer _verticesBuffer;

    private ComputeBuffer _uvBuffer;

    // Start is called before the first frame update
    void Start()
    {
        // init member

        _targetMeshRenderer = _targetObject.GetComponent<MeshRenderer>();

        _targetMeshFilter = _targetObject.GetComponent<MeshFilter>();
        _targetMesh = _targetMeshFilter.mesh;

        // init textures

        _positionMap = CreateTexture(
            _destMapWidth,
            _destMapHeight,
            RenderTextureFormat.ARGBFloat
        );
        _positionMap.Create();

        _alphaMap = CreateTexture(
            _destMapWidth,
            _destMapHeight,
            RenderTextureFormat.ARGB32
        );
        _alphaMap.Create();

        // init buffer

        int[] triangles = _targetMesh.GetTriangles(0);
        _trianglesBuffer = new ComputeBuffer(triangles.Length, sizeof(int));
        _trianglesBuffer.SetData(triangles);

        Vector3[] vertices = _targetMesh.vertices;
        _verticesBuffer = new ComputeBuffer(vertices.Length, sizeof(float) * 3);
        _verticesBuffer.SetData(vertices);

        Vector2[] uv = _targetMesh.uv;
        _uvBuffer = new ComputeBuffer(uv.Length, sizeof(float) * 2);
        _uvBuffer.SetData(uv);

        // init compute shader

        kernelID = _computeShader.FindKernel("CSMain");

        // _computeShader.SetBuffer(kernelID, "Triangles", _verticesBuffer);

        _computeShader.SetTexture(kernelID, "DissolveMap", _dissolveMap);
        _computeShader.SetTexture(kernelID, "PositionMap", _positionMap);
        _computeShader.SetTexture(kernelID, "AlphaMap", _alphaMap);
        _computeShader.SetBuffer(kernelID, "TrianglesBuffer", _trianglesBuffer);
        _computeShader.SetBuffer(kernelID, "VerticesBuffer", _verticesBuffer);
        _computeShader.SetBuffer(kernelID, "UvBuffer", _uvBuffer);
        _computeShader.SetInt("TrianglesCount", triangles.Length / 3);
        _computeShader.SetInt("DissolveMapWidth", _dissolveMap.width);
        _computeShader.SetInt("DissolveMapHeight", _dissolveMap.height);
        _computeShader.SetInt("DestMapWidth", _destMapWidth);
        _computeShader.SetInt("DestMapHeight", _destMapHeight);

        // init material

        _dissolveLitMeshMaterialPropertyBlock = new MaterialPropertyBlock();
        _debugPlaneMaterialPropertyBlock = new MaterialPropertyBlock();

        // debug

        // Debug.Log(_destMapWidth);
        // Debug.Log(_destMapHeight);
        // Debug.Log(triangles.Length);

        // Debug.Log("w");
        // Debug.Log(_positionMap.width);
        // Debug.Log("h");
        // Debug.Log(_positionMap.height);
        // Debug.Log("uv length");
        // Debug.Log(uv.Length);

        // for (int i = 0; i < triangles.Length; i++)
        // {
        //     Debug.Log(triangles[i]);
        // }
    }

    // Update is called once per frame
    void Update()
    {
        // _computeShader.SetFloat("dissolveInput", _dissolveInput);
        _computeShader.SetFloat("DissolveRate", _dissolveRate);
        _computeShader.SetFloat("EdgeFadeIn", _edgeFadeIn);
        _computeShader.SetFloat("EdgeFadeIn", _edgeFadeIn);
        _computeShader.SetFloat("EdgeIn", _edgeIn);
        _computeShader.SetFloat("EdgeOut", _edgeOut);
        _computeShader.SetFloat("EdgeFadeOut", _edgeFadeOut);
        _computeShader.SetMatrix("Transform", _targetObject.transform.localToWorldMatrix);
        _computeShader.SetFloat("DissolveThreshold", _dissolveThreshold);
        // _computeShader.SetFloat("Time", Time.time * 100 % 1024); // multiply speed and clamp time
        _computeShader.SetFloat("Time", Time.time); // multiply speed and clamp time

        _computeShader.Dispatch(
            kernelID,
            _destMapWidth,
            _destMapHeight,
            1
        );

        _visualEffect.SetTexture("PositionMap", _positionMap);
        _visualEffect.SetTexture("AlphaMap", _alphaMap);

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

        _targetMeshRenderer.GetPropertyBlock(_dissolveLitMeshMaterialPropertyBlock);
        // _dissolveLitMeshMaterialPropertyBlock.SetTexture("_DissolveMap", _dissolveMap);
        _dissolveLitMeshMaterialPropertyBlock.SetTexture(
            "Texture2D_54ef741b959443bd9e9b02b73af70d78",
            _dissolveMap
        // _positionMap
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
        _targetMeshRenderer.SetPropertyBlock(_dissolveLitMeshMaterialPropertyBlock);

        // for debug

        _debugPlaneMeshRenderer.GetPropertyBlock(_debugPlaneMaterialPropertyBlock);
        _debugPlaneMaterialPropertyBlock.SetTexture("_BaseMap", _positionMap);
        // _debugPlaneMaterialPropertyBlock.SetTexture("_BaseMap", _alphaMap);
        _debugPlaneMeshRenderer.SetPropertyBlock(_debugPlaneMaterialPropertyBlock);

        // _debugPlane.SetTexture("_BaseMap", _positionMap);
    }

    void OnDisable()
    {
        Dispose();
    }

    void OnDestroy()
    {
        Dispose();
    }

    RenderTexture CreateTexture(int width, int height, UnityEngine.RenderTextureFormat format)
    {
        RenderTexture map = new RenderTexture(
            width,
            height,
            0,
            format,
            RenderTextureReadWrite.Linear
        );
        map.filterMode = FilterMode.Point;
        map.enableRandomWrite = true;
        map.hideFlags = HideFlags.DontSave;
        return map;
    }

    // ref:
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
        _trianglesBuffer?.Dispose();
        _trianglesBuffer = null;

        _verticesBuffer?.Dispose();
        _verticesBuffer = null;

        _uvBuffer?.Dispose();
        _uvBuffer = null;

        DestroyObj(_positionMap);
        DestroyObj(_alphaMap);
    }
}
