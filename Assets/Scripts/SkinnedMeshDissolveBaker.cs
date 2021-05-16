using UnityEngine;

namespace DissolverVFX {
    public class SkinnedMeshDissolveBaker : DissolveBakerBase {
        [SerializeField]
        private SkinnedMeshRenderer[] _targetSkinnedMeshRenderers = null;

        void Start()
        {
            Mesh[] meshes = new Mesh[_targetSkinnedMeshRenderers.Length];
            for(int i = 0; i < _targetSkinnedMeshRenderers.Length; i++) {
                meshes[i] = _targetSkinnedMeshRenderers[i].sharedMesh;
            }

            Initialize(meshes);
        }

        void Update()
        {
            Exec(_targetSkinnedMeshRenderers);
        }
    }
}