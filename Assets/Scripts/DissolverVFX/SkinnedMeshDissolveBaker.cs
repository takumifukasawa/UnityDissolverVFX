using UnityEngine;

namespace DissolverVFX {
    public class SkinnedMeshDissolveBaker : DissolveBakerBase {
        [SerializeField]
        private GameObject[] _targetSkinnedMeshObjects = null;

        private SkinnedMeshRenderer[] _skinnedMeshRenderers;

        public SkinnedMeshRenderer[] skinnedMeshRenderers
        {
            get { return _skinnedMeshRenderers; }
        }

        void Start()
        {
            int counts = _targetSkinnedMeshObjects.Length;
            Mesh[] meshes = new Mesh[counts];
            _skinnedMeshRenderers = new SkinnedMeshRenderer[counts];
            for(int i = 0; i < counts; i++) {
                SkinnedMeshRenderer skinnedMeshRenderer = _targetSkinnedMeshObjects[i].GetComponent<SkinnedMeshRenderer>();
                _skinnedMeshRenderers[i] = skinnedMeshRenderer;
                meshes[i] = skinnedMeshRenderer.sharedMesh;
            }

            Initialize(meshes);
        }

        void Update()
        {
            Exec(_skinnedMeshRenderers);
        }
    }
}