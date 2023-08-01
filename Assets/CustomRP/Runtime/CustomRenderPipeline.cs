using UnityEngine;
using UnityEngine.Rendering;

public class CustomRenderPipeline : RenderPipeline
{
    CameraRenderer renderer = new CameraRenderer();
    public CustomRenderPipeline () {
        GraphicsSettings.useScriptableRenderPipelineBatching = false;
    }
    protected override void Render(ScriptableRenderContext context, Camera[] cameras)
    {
        foreach (Camera camera in cameras) {
            renderer.Render(context, camera);
        }
    }
}