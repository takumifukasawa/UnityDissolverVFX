using UnityEngine;
using Utilities;

namespace DissolverVFX {
    public class SkinnedMeshDissolveBaker : MonoBehaviour {
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
        private int _destMapWidth = 512;

        [SerializeField]
        private int _destMapHeight = 512;

        [SerializeField, Range(0, 1)]
        private float _dissolveThreshold = 0.5f;

        private DissolveBaker _dissolveBaker;

        public RenderTexture positionMap
        {
            get { return _dissolveBaker.positionMap; }
        }
        public RenderTexture normalMap
        {
            get { return _dissolveBaker.normalMap; }
        }
        public RenderTexture alphaMap
        {
            get { return _dissolveBaker.alphaMap; }
        }
        public Texture2D dissolveMap
        {
            get { return _dissolveMap; }
        }
        public float dissolveRate
        {
            get { return _dissolveRate; }
        }
        public float edgeFadeIn
        {
            get { return _edgeFadeIn; }
        }
        public float edgeIn
        {
            get { return _edgeIn; }
        }
        public float edgeOut
        {
            get { return _edgeOut; }
        }
        public float edgeFadeOut
        {
            get { return _edgeFadeOut; }
        }

        // for debug

        [SerializeField]
        private bool _enableDebug;
        private MeshRenderer _debugPositionMapMeshRenderer;
        private MeshRenderer _debugNormalMapMeshRenderer;
        private MeshRenderer _debugAlphaMapMeshRenderer;

        private MaterialPropertyBlock _debugPositionMapMaterialPropertyBlock;
        private MaterialPropertyBlock _debugNormalMapMaterialPropertyBlock;
        private MaterialPropertyBlock _debugAlphaMapMaterialPropertyBlock;

        void Start()
        {
            Mesh[] meshes = new Mesh[_targetSkinnedMeshRenderers.Length];
            for(int i = 0; i < _targetSkinnedMeshRenderers.Length; i++) {
                meshes[i] = _targetSkinnedMeshRenderers[i].sharedMesh;
            }

            _dissolveBaker = new DissolveBaker(Instantiate(_computeShader), meshes, _dissolveMap, _destMapWidth, _destMapHeight);

            _dissolveBaker.Initialize();

            // for debug

            if(_enableDebug)
            {
                Material ma = Resources.Load<Material>("Materials/Unlit");

                GameObject debugPositionMapObj = CreateDebugPlane("DEBUG position map plane");
                debugPositionMapObj.transform.SetParent(transform);
                debugPositionMapObj.transform.position = new Vector3(2, 0.5f, 0);
                debugPositionMapObj.transform.rotation = Quaternion.Euler(0, 180f, 0);
                _debugPositionMapMeshRenderer = debugPositionMapObj.GetComponent<MeshRenderer>();
                _debugPositionMapMaterialPropertyBlock = new MaterialPropertyBlock();
                _debugPositionMapMeshRenderer.material = ma;

                GameObject debugNormalMapObj = CreateDebugPlane("DEBUG normal map plane");
                debugNormalMapObj.transform.SetParent(transform);
                debugNormalMapObj.transform.position = new Vector3(3.1f, 0.5f, 0);
                debugNormalMapObj.transform.rotation = Quaternion.Euler(0, 180f, 0);
                _debugNormalMapMeshRenderer = debugNormalMapObj.GetComponent<MeshRenderer>();
                _debugNormalMapMaterialPropertyBlock = new MaterialPropertyBlock();
                _debugNormalMapMeshRenderer.sharedMaterial = ma;

                GameObject debugAlphaMapObj = CreateDebugPlane("DEBUG alpha map plane");
                _debugAlphaMapMeshRenderer = debugAlphaMapObj.GetComponent<MeshRenderer>();
                debugAlphaMapObj.transform.position = new Vector3(4.2f, 0.5f, 0);
                debugAlphaMapObj.transform.SetParent(transform);
                debugAlphaMapObj.transform.rotation = Quaternion.Euler(0, 180f, 0);
                _debugAlphaMapMaterialPropertyBlock = new MaterialPropertyBlock();
                _debugAlphaMapMeshRenderer.sharedMaterial = ma;
            }
        }

        private GameObject CreateDebugPlane(string name)
        {
            GameObject obj = new GameObject(name);
            MeshRenderer meshRenderer = obj.AddComponent<MeshRenderer>();
            MeshFilter meshFilter = obj.AddComponent<MeshFilter>();
            Mesh mesh = MeshUtilities.CreatePlane();
            meshFilter.mesh = mesh;
            return obj;
        }

        void Update()
        {
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

            // for debug
                
            if(_enableDebug)
            {
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

}