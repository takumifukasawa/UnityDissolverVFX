using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Dissolver : MonoBehaviour
{
    [SerializeField]
    private Texture2D _srcTexture;

    [SerializeField]
    private ComputeShader _computeShader;

    // [SerializeField]
    private RenderTexture _destTexture;

    // [SerializeField]
    // private Material _debugPlane;

    [SerializeField]
    private MeshRenderer _debugPlaneMeshRenderer;

    private MaterialPropertyBlock _debugPlaneMaterialPropertyBlock;

    // private RenderTexture _destTexture;

    private int kernelID;

    // Start is called before the first frame update
    void Start()
    {
        _destTexture = new RenderTexture(
            _srcTexture.width,
            _srcTexture.height,
            0,
            RenderTextureFormat.ARGB32,
            RenderTextureReadWrite.Linear
        );
        _destTexture.enableRandomWrite = true;
        _destTexture.Create();

        kernelID = _computeShader.FindKernel("CSMain");

        _computeShader.SetTexture(kernelID, "destTexture", _destTexture);
        _computeShader.SetTexture(kernelID, "srcTexture", _srcTexture);

        // Debug.Log(_srcTexture.format);

        _debugPlaneMaterialPropertyBlock = new MaterialPropertyBlock();
    }

    // Update is called once per frame
    void Update()
    {
        _computeShader.Dispatch(
            kernelID,
            _srcTexture.width,
            _srcTexture.height,
            1
        );

        _debugPlaneMeshRenderer.GetPropertyBlock(_debugPlaneMaterialPropertyBlock);
        _debugPlaneMaterialPropertyBlock.SetTexture("_BaseMap", _destTexture);
        _debugPlaneMeshRenderer.SetPropertyBlock(_debugPlaneMaterialPropertyBlock);

        // _debugPlane.SetTexture("_BaseMap", _destTexture);
    }
}
