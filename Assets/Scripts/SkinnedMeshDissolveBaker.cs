using UnityEngine;
using UnityEngine.VFX;

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
        private VisualEffect _visualEffect;

        [SerializeField]
        private int _destMapWidth = 512;

        [SerializeField]
        private int _destMapHeight = 512;

        [SerializeField, Range(0, 1)]
        private float _dissolveThreshold = 0.5f;

        // for debug
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

        void Start()
        {
            Mesh[] meshes = new Mesh[_targetSkinnedMeshRenderers.Length];
            for(int i = 0; i < _targetSkinnedMeshRenderers.Length; i++) {
                meshes[i] = _targetSkinnedMeshRenderers[i].sharedMesh;
            }

            _dissolveBaker = new DissolveBaker(Instantiate(_computeShader), meshes, _dissolveMap, _destMapWidth, _destMapHeight);

            _dissolveBaker.Initialize();
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
        }

        void UpdateVFX() {
            _visualEffect.SetTexture("PositionMap", _dissolveBaker.positionMap);
            _visualEffect.SetTexture("NormalMap", _dissolveBaker.normalMap);
            _visualEffect.SetTexture("AlphaMap", _dissolveBaker.alphaMap);
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