// ----------------------------------------------------------------------------------
// ref:
// https://github.com/keijiro/Smrvfx
// ----------------------------------------------------------------------------------

using UnityEngine;
using Unity.Collections;

namespace DissolverVFX {
    public class DissolveBaker
    {
        private ComputeShader _computeShader;
        private Mesh[] _meshes;

        private RenderTexture _positionMap;
        private RenderTexture _normalMap;
        private RenderTexture _alphaMap;

        public RenderTexture positionMap {
            get { return this._positionMap; }
        }

        public RenderTexture normalMap {
            get { return this._normalMap; }
        }
        public RenderTexture alphaMap {
            get { return this._alphaMap; }
        }

        private int kernelID;

        private ComputeBuffer _trianglesBuffer;

        private ComputeBuffer _verticesBuffer;

        private ComputeBuffer _uvBuffer;
        private Texture2D _dissolveMap;

        private int _destMapWidth;
        private int _destMapHeight;

        public DissolveBaker(ComputeShader computeShader, Mesh[] meshes, Texture2D dissolveMap, int destMapWidth, int destMapHeight) {
           _computeShader = computeShader;
            _meshes = meshes;
            _dissolveMap = dissolveMap;
            _destMapWidth = destMapWidth;
            _destMapHeight = destMapHeight;
        }

        public void Initialize() {

            // init maps

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

            // init buffers

            Mesh combinedMesh = new Mesh();
            combinedMesh.hideFlags = HideFlags.DontSave;
            CombineInstance[] combineInstanceArray = new CombineInstance[_meshes.Length];
            for (int i = 0; i < _meshes.Length; i++)
            {
                Mesh mesh = _meshes[i];
                combineInstanceArray[i].mesh = mesh;
            }
            combinedMesh.CombineMeshes(combineInstanceArray);

            int[] triangles = combinedMesh.triangles;
            _trianglesBuffer = new ComputeBuffer(triangles.Length, sizeof(int));

            Vector3[] vertices = combinedMesh.vertices;
            _verticesBuffer = new ComputeBuffer(vertices.Length, sizeof(float) * 3);

            Vector2[] uv = combinedMesh.uv;
            _uvBuffer = new ComputeBuffer(uv.Length, sizeof(float) * 2);

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
        }

        int[] SetVertexBuffersData(Mesh mesh, int vertexOffset, int triangleOffset, int uvOffset) {
            using (Mesh.MeshDataArray dataArray = Mesh.AcquireReadOnlyMeshData(mesh))
            {
                Mesh.MeshData data = dataArray[0];
                int vertexCount = data.vertexCount;
                int triangleCount = mesh.triangles.Length;
                int uvCount = vertexCount;

                using (NativeArray<Vector3> positionArray = new NativeArray<Vector3>(vertexCount, Allocator.TempJob, NativeArrayOptions.UninitializedMemory))
                using (NativeArray<int> triangleArray = new NativeArray<int>(triangleCount, Allocator.TempJob, NativeArrayOptions.UninitializedMemory))
                using (NativeArray<Vector2> uvArray = new NativeArray<Vector2>(uvCount, Allocator.TempJob, NativeArrayOptions.UninitializedMemory))
                {
                    data.GetVertices(positionArray);
                    data.GetIndices(triangleArray, 0);
                    data.GetUVs(0, uvArray);

                    _verticesBuffer.SetData(positionArray, 0, vertexOffset, vertexCount);
                    _trianglesBuffer.SetData(triangleArray, 0, triangleOffset, triangleCount);
                    _uvBuffer.SetData(uvArray, 0, uvOffset, uvCount);
                }

                int[] result = new int[3];
                result[0] = vertexCount;
                result[1] = triangleCount;
                result[2] = uvCount;
                return result;
            }
        }

        void SetUniformAndDispatchComputeShader(
            Matrix4x4 rootMatrix,
            float dissolveRate,
            float edgeFadeIn,
            float edgeIn,
            float edgeOut,
            float edgeFadeOut,
            float timeMultiplier,
            int destMapWidth,
            int destMapHeight,
            float dissolveThreshold
        ) {

            _computeShader.SetFloat("DissolveRate", dissolveRate);
            _computeShader.SetFloat("EdgeFadeIn", edgeFadeIn);
            _computeShader.SetFloat("EdgeFadeIn", edgeFadeIn);
            _computeShader.SetFloat("EdgeIn", edgeIn);
            _computeShader.SetFloat("EdgeOut", edgeOut);
            _computeShader.SetFloat("EdgeFadeOut", edgeFadeOut);

            _computeShader.SetMatrix("Transform", rootMatrix);

            _computeShader.SetFloat("DissolveThreshold", dissolveThreshold);
            _computeShader.SetFloat("Time", Mathf.Repeat(Time.time * timeMultiplier, 100f)); // multiply speed and clamp time

            _computeShader.Dispatch(
                kernelID,
                _destMapWidth,
                _destMapHeight,
                1
            );
        }

        // for mesh
        public void Bake(
            Mesh[] meshes,
            Matrix4x4 rootMatrix,
            float dissolveRate,
            float edgeFadeIn,
            float edgeIn,
            float edgeOut,
            float edgeFadeOut,
            float timeMultiplier,
            int destMapWidth,
            int destMapHeight,
            float dissolveThreshold
        ) {                
            int vertexOffset = 0;
            int triangleOffset = 0;
            int uvOffset = 0;

            foreach (Mesh mesh in meshes)
            {
                int[] result = SetVertexBuffersData(mesh, vertexOffset, triangleOffset, uvOffset);
                vertexOffset += result[0];
                triangleOffset += result[1];
                uvOffset += result[2];
            }

            SetUniformAndDispatchComputeShader(
                rootMatrix,
                dissolveRate,
                edgeFadeIn,
                edgeIn,
                edgeOut,
                edgeFadeOut,
                timeMultiplier,
                destMapWidth,
                destMapHeight,
                dissolveThreshold
            );
        }

        // for skinned mesh
        public void Bake(
            SkinnedMeshRenderer[] skinnedMeshRenderers,
            Matrix4x4 rootMatrix,
            float dissolveRate,
            float edgeFadeIn,
            float edgeIn,
            float edgeOut,
            float edgeFadeOut,
            float timeMultiplier,
            int destMapWidth,
            int destMapHeight,
            float dissolveThreshold
        ) {                
            int vertexOffset = 0;
            int triangleOffset = 0;
            int uvOffset = 0;

            Mesh targetMesh = new Mesh();

            foreach (SkinnedMeshRenderer skinnedMeshRenderer in skinnedMeshRenderers)
            {
                skinnedMeshRenderer.BakeMesh(targetMesh);
                int[] result = SetVertexBuffersData(targetMesh, vertexOffset, triangleOffset, uvOffset);
                vertexOffset += result[0];
                triangleOffset += result[1];
                uvOffset += result[2];
            }

            SetUniformAndDispatchComputeShader(
                rootMatrix,
                dissolveRate,
                edgeFadeIn,
                edgeIn,
                edgeOut,
                edgeFadeOut,
                timeMultiplier,
                destMapWidth,
                destMapHeight,
                dissolveThreshold
            );
        }

        static RenderTexture CreateTexture(int width, int height, UnityEngine.RenderTextureFormat format)
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

        void DestroyObj(Object o)
        {
            if (o == null) return;
            if (Application.isPlaying)
                Object.Destroy(o);
            else
                Object.DestroyImmediate(o);
        }

        public void Dispose()
        {
            _trianglesBuffer?.Dispose();
            // _trianglesBuffer?.Release();
            _trianglesBuffer = null;

            _verticesBuffer?.Dispose();
            // _verticesBuffer?.Release();
            _verticesBuffer = null;

            _uvBuffer?.Dispose();
            // _uvBuffer?.Release();
            _uvBuffer = null;

            DestroyObj(_positionMap);
            _positionMap = null;

            DestroyObj(_normalMap);
            _normalMap = null;

            DestroyObj(_alphaMap);
            _alphaMap = null;

            // DestroyObj(_targetMesh);
            // _targetMesh = null;
        }
    }
}