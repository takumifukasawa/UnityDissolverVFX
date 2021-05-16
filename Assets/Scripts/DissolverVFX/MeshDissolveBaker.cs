using UnityEngine;

namespace DissolverVFX {
    public class MeshDissolveBaker : DissolveBakerBase {
        [SerializeField]
        private MeshFilter[] _targetMeshFilters = null;

        private Mesh[] _meshes;

        void Start()
        {
            _meshes = new Mesh[_targetMeshFilters.Length];
            for(int i = 0; i < _targetMeshFilters.Length; i++) {
                _meshes[i] = _targetMeshFilters[i].sharedMesh;
            }

            Initialize(_meshes);
        }

        void Update()
        {
            Exec(_meshes);
        }
    }
}