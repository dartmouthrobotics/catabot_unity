using UnityEngine;
using UnityEngine.Rendering;

public class FogForCamera : MonoBehaviour
{
    public Camera cameraWithoutFog;

    private void OnEnable()
    {
        RenderPipelineManager.beginCameraRendering += OnBeginCameraRendering;
    }
    void OnDisable()
    {
        RenderPipelineManager.beginCameraRendering -= OnBeginCameraRendering;
    }
    void OnBeginCameraRendering(ScriptableRenderContext context, Camera camera)
    {
        if (camera == cameraWithoutFog)
            RenderSettings.fog = false;
        else
            RenderSettings.fog = true;
    }
}
