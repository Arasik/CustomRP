using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.Rendering;

public partial class CameraRenderer {

    ScriptableRenderContext context;

    Camera camera;
    const string bufferName = "Render Camera";

    CommandBuffer buffer = new CommandBuffer {
        name = bufferName
    };
    CullingResults cullingResults;
    private static ShaderTagId unlitShaderTagId = new ShaderTagId("SRPDefaultUnlit");
    private static ShaderTagId litShaderTagId = new ShaderTagId("CustomLit");

    public void Render (ScriptableRenderContext context, Camera camera,
        bool useDynamicBatching, bool useGPUInstancing) {
        this.context = context;
        this.camera = camera;
        Profiler.BeginSample("Editor Only");
        PrepareBuffer();
        Profiler.EndSample();
        PrepareForSceneWindow();
        if (!Cull()) {
            return;
        }   
        Setup();
        DrawVisibleGeometry(useDynamicBatching,useGPUInstancing);
        // #if UNITY_EDITOR
            DrawUnsupportedShaders();
            DrawGizmos();
        // #endif
        Submit();
    }
    void Setup () 
    { 
        context.SetupCameraProperties(camera);
        CameraClearFlags flags = camera.clearFlags;
        buffer.ClearRenderTarget(flags<=CameraClearFlags.Depth, flags==CameraClearFlags.Color, flags==CameraClearFlags.Color?camera.backgroundColor:Color.clear);
        buffer.BeginSample(SampleName);
        ExecuteBuffer();
        // context.SetupCameraProperties(camera);
    }
    void DrawVisibleGeometry (bool useDynamicBatching, bool useGPUInstancing)
    {
        var sortingSettings = new SortingSettings(camera){criteria = SortingCriteria.CommonOpaque};
        var drawingSettings = new DrawingSettings(unlitShaderTagId,sortingSettings){
            enableDynamicBatching = useDynamicBatching,
            enableInstancing = useGPUInstancing
        };
        drawingSettings.SetShaderPassName(1, litShaderTagId);
        var filteringSettings = new FilteringSettings(RenderQueueRange.opaque);

        context.DrawRenderers(
            cullingResults, ref drawingSettings, ref filteringSettings
        );

        context.DrawSkybox(camera);
        
        sortingSettings.criteria = SortingCriteria.CommonTransparent;
        drawingSettings.sortingSettings = sortingSettings;
        filteringSettings.renderQueueRange = RenderQueueRange.transparent;

        context.DrawRenderers(
            cullingResults, ref drawingSettings, ref filteringSettings
        );
    }
    void Submit () {
        buffer.EndSample(SampleName);
        ExecuteBuffer();
        context.Submit();
    }
    void ExecuteBuffer () {
        context.ExecuteCommandBuffer(buffer);
        buffer.Clear();
    }
    bool Cull ()
    {
        if (camera.TryGetCullingParameters(out ScriptableCullingParameters p))
        {
            cullingResults = context.Cull(ref p);
            return true;
        }
        return false;
    }

}