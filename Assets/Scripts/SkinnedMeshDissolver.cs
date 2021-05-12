using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine.VFX;
using System.Linq;

public class SkinnedMeshDissolver : MonoBehaviour
{
    // [SerializeField]
    // private SkinnedMeshRenderer _targetMeshRenderer;

    [SerializeField]
    private SkinnedMeshRenderer[] _targetSkinnedMeshRenderers = null;

    // [SerializeField]
    // private MeshFilter _targetMeshFilter;

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

    private Mesh _targetMesh;

    private RenderTexture _positionMap;
    private RenderTexture _normalMap;
    private RenderTexture _alphaMap;

    private int kernelID;

    private ComputeBuffer _trianglesBuffer;

    private ComputeBuffer _verticesBuffer;

    private ComputeBuffer _uvBuffer;

    private MaterialPropertyBlock[] _dissolveMaterialPropertyBlocks = null;

    // for debug
    private MaterialPropertyBlock _debugPositionMapMaterialPropertyBlock;
    private MaterialPropertyBlock _debugNormalMapMaterialPropertyBlock;
    private MaterialPropertyBlock _debugAlphaMapMaterialPropertyBlock;

    // private Mesh _tmpMesh;

    void Start()
    {
        // init mesh

        _targetMesh = new Mesh();
        _targetMesh.hideFlags = HideFlags.DontSave;

        // _targetMesh = _targetMeshFilter.mesh;
        // _targetMesh = _targetMeshRenderer.sharedMesh

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
            RenderTextureFormat.ARGBFloat
        );
        _alphaMap.Create();

        _normalMap = CreateTexture(
            _destMapWidth,
            _destMapHeight,
            RenderTextureFormat.ARGB32
        );
        _normalMap.Create();

        // init buffer

        Mesh combinedMesh = new Mesh();
        // combinedMesh.name = "hoge";
        combinedMesh.hideFlags = HideFlags.DontSave;
        CombineInstance[] combineInstanceArray = new CombineInstance[_targetSkinnedMeshRenderers.Length];
        for (int i = 0; i < _targetSkinnedMeshRenderers.Length; i++)
        {
            combineInstanceArray[i].mesh = _targetSkinnedMeshRenderers[i].sharedMesh;
            combineInstanceArray[i].transform = _targetSkinnedMeshRenderers[i].transform.localToWorldMatrix;
            Debug.Log("### mesh info ###");
            Debug.Log(i);
            Debug.Log("triangles");
            Debug.Log(_targetSkinnedMeshRenderers[i].sharedMesh.GetTriangles(0).Length);
            Debug.Log("vertices");
            Debug.Log(_targetSkinnedMeshRenderers[i].sharedMesh.vertices.Length);
            // Debug.Log("uvs");
            // Debug.Log(_targetSkinnedMeshRenderers[i].sharedMesh.GetUVs(0));
        }
        combinedMesh.CombineMeshes(combineInstanceArray);

        // int[] triangles = _targetMeshRenderer.sharedMesh.GetTriangles(0);
        int[] triangles = combinedMesh.triangles;
        _trianglesBuffer = new ComputeBuffer(triangles.Length, sizeof(int));

        // Vector3[] vertices = _targetMeshRenderer.sharedMesh.vertices;
        Vector3[] vertices = combinedMesh.vertices;
        _verticesBuffer = new ComputeBuffer(vertices.Length, sizeof(float) * 3);

        // Vector2[] uv = _targetMeshRenderer.sharedMesh.uv;
        Vector2[] uv = combinedMesh.uv;
        _uvBuffer = new ComputeBuffer(uv.Length, sizeof(float) * 2);

        Debug.Log("==========");
        Debug.Log("vertices");
        Debug.Log(vertices.Length);
        Debug.Log("uv count");
        Debug.Log(uv.Length);
        Debug.Log("triangles");
        Debug.Log(triangles.Length / 3);

        // init compute shader

        kernelID = _computeShader.FindKernel("CSMain");

        _computeShader.SetTexture(kernelID, "DissolveMap", _dissolveMap);
        _computeShader.SetTexture(kernelID, "PositionMap", _positionMap);
        _computeShader.SetTexture(kernelID, "NormalMap", _normalMap);
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

        _dissolveMaterialPropertyBlocks = new MaterialPropertyBlock[_targetSkinnedMeshRenderers.Length];
        for (int i = 0; i < _targetSkinnedMeshRenderers.Length; i++)
        {
            _dissolveMaterialPropertyBlocks[i] = new MaterialPropertyBlock();
        }

        // for debug

        _debugPositionMapMaterialPropertyBlock = new MaterialPropertyBlock();
        _debugNormalMapMaterialPropertyBlock = new MaterialPropertyBlock();
        _debugAlphaMapMaterialPropertyBlock = new MaterialPropertyBlock();

        // for debug 2

        int vertexOffset = 0;
        int triangleOffset = 0;
        foreach (SkinnedMeshRenderer skinnedMeshRenderer in _targetSkinnedMeshRenderers)
        {
            int[] result = Bake(skinnedMeshRenderer, vertexOffset, triangleOffset);
            vertexOffset += result[0];
            triangleOffset += result[1];
        }
    }

    int[] Bake(SkinnedMeshRenderer skinnedMeshRenderer, int vertexOffset, int triangleOffset)
    {
        skinnedMeshRenderer.BakeMesh(_targetMesh);

        using (Mesh.MeshDataArray dataArray = Mesh.AcquireReadOnlyMeshData(_targetMesh))
        {
            Mesh.MeshData data = dataArray[0];
            int vertexCount = data.vertexCount;

            // Debug.Log("-----------------------------------------------------------");
            // Debug.Log(_targetMesh.vertexCount);
            // Debug.Log(data.vertexCount);

            int triangleCount = _targetMesh.triangles.Length;

            // int triangleCount = _targetMesh.GetIndexCount(0);
            // Debug.Log("==========");

            using (NativeArray<Vector3> positionArray = new NativeArray<Vector3>(vertexCount, Allocator.TempJob, NativeArrayOptions.UninitializedMemory))
            // using(NativeArray<Vector3> normalArray = new NativeArray<Vector3>(vertexCount, Allocator.TempJob, NativeArrayOptions.UninitializedMemory))
            using (NativeArray<Vector2> uvArray = new NativeArray<Vector2>(vertexCount, Allocator.TempJob, NativeArrayOptions.UninitializedMemory))
            using (NativeArray<ushort> triangleArray = new NativeArray<ushort>(triangleCount, Allocator.TempJob, NativeArrayOptions.UninitializedMemory))
            {
                data.GetVertices(positionArray);
                // data.GetNormals(normalArray);
                data.GetUVs(0, uvArray);
                data.GetIndices(triangleArray, 0);

                Debug.Log("----------------------------------------------------");
                Debug.Log("vertex count");
                Debug.Log(vertexCount);
                Debug.Log("triangle count");
                Debug.Log(triangleCount);
                Debug.Log("positionArray length");
                Debug.Log(positionArray.Length);
                Debug.Log("uvArray length");
                Debug.Log(uvArray.Length);
                Debug.Log("triangleArray length");
                Debug.Log(triangleArray.Length);
                Debug.Log("positionArray[vertexCount - 1]");
                Debug.Log(positionArray[vertexCount - 1]);
                Debug.Log("uvArray[vertexCount - 1]");
                Debug.Log(uvArray[vertexCount - 1]);
                Debug.Log("triangleArray[triangleCount - 1]");
                Debug.Log(triangleArray[triangleCount - 1]);

                _verticesBuffer.SetData(positionArray, 0, vertexOffset, vertexCount);
                _uvBuffer.SetData(uvArray, 0, vertexOffset, vertexCount);
                _trianglesBuffer.SetData(triangleArray, 0, triangleOffset, triangleCount);
            }

            int[] result = new int[2];
            result[0] = vertexCount;
            result[1] = triangleCount;
            return result;
        }
    }

    // // Update is called once per frame
    // void Update()
    // {
    //     // Debug.Log("################");

    //     // _targetMeshRenderer.BakeMesh(_targetMesh);

    //     int vertexOffset = 0;
    //     int triangleOffset = 0;
    //     foreach (SkinnedMeshRenderer skinnedMeshRenderer in _targetSkinnedMeshRenderers)
    //     {
    //         int[] result = Bake(skinnedMeshRenderer, vertexOffset, triangleOffset);
    //         vertexOffset += result[0];
    //         triangleOffset += result[1];
    //     }

    //     UpdateVFX();
    //     UpdateMaterials();

    //     // for debug

    //     _debugPositionMapMeshRenderer.GetPropertyBlock(_debugPositionMapMaterialPropertyBlock);
    //     _debugPositionMapMaterialPropertyBlock.SetTexture("_BaseMap", _positionMap);
    //     _debugPositionMapMeshRenderer.SetPropertyBlock(_debugPositionMapMaterialPropertyBlock);

    //     _debugNormalMapMeshRenderer.GetPropertyBlock(_debugNormalMapMaterialPropertyBlock);
    //     _debugNormalMapMaterialPropertyBlock.SetTexture("_BaseMap", _normalMap);
    //     _debugNormalMapMeshRenderer.SetPropertyBlock(_debugNormalMapMaterialPropertyBlock);

    //     _debugAlphaMapMeshRenderer.GetPropertyBlock(_debugAlphaMapMaterialPropertyBlock);
    //     _debugAlphaMapMaterialPropertyBlock.SetTexture("_BaseMap", _alphaMap);
    //     _debugAlphaMapMeshRenderer.SetPropertyBlock(_debugAlphaMapMaterialPropertyBlock);
    // }

    void UpdateVFX()
    {
        _computeShader.SetFloat("DissolveRate", _dissolveRate);
        _computeShader.SetFloat("EdgeFadeIn", _edgeFadeIn);
        _computeShader.SetFloat("EdgeFadeIn", _edgeFadeIn);
        _computeShader.SetFloat("EdgeIn", _edgeIn);
        _computeShader.SetFloat("EdgeOut", _edgeOut);
        _computeShader.SetFloat("EdgeFadeOut", _edgeFadeOut);
        _computeShader.SetMatrix("Transform", transform.localToWorldMatrix);
        _computeShader.SetFloat("DissolveThreshold", _dissolveThreshold);
        _computeShader.SetFloat("Time", Mathf.Repeat(Time.time * _timeMultiplier, 100f)); // multiply speed and clamp time

        _computeShader.Dispatch(
            kernelID,
            _destMapWidth,
            _destMapHeight,
            1
        );

        _visualEffect.SetTexture("PositionMap", _positionMap);
        _visualEffect.SetTexture("NormalMap", _normalMap);
        _visualEffect.SetTexture("AlphaMap", _alphaMap);


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
        DestroyObj(_normalMap);
        DestroyObj(_alphaMap);

        DestroyObj(_targetMesh);
        _targetMesh = null;
    }
}
