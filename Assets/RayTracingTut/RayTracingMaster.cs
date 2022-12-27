using UnityEngine;

public class RayTracingMaster : MonoBehaviour {

    public ComputeShader RayTracingShader;
    public Camera curCamera;
    public Vector3 skyboxColor;
    public Texture SkyboxTexture;
    public Light DirectionalLight;

    private uint currentSample = 0;
    private Material addMaterial;
    private RenderTexture target;

    private void Update() {
        if (transform.hasChanged) {
            this.currentSample = 0;
            transform.hasChanged = false;
        }
        if (this.DirectionalLight.transform.hasChanged) {
            this.currentSample = 0;
            DirectionalLight.transform.hasChanged = false;
        }
    }

    private void OnRenderImage(RenderTexture source, RenderTexture destination) {
        this.setShaderParameters();
        Render(destination);
    }

    private void Render(RenderTexture destination) {
        // Make sure we have a current render target
        InitRenderTexture();

        // Set the target and dispatch the compute shader
        RayTracingShader.SetTexture(0, "Result", this.target);
        int threadGroupsX = Mathf.CeilToInt(Screen.width / 8.0f);
        int threadGroupsY = Mathf.CeilToInt(Screen.height / 8.0f);
        RayTracingShader.Dispatch(0, threadGroupsX, threadGroupsY, 1);

        if (this.addMaterial == null)
            this.addMaterial = new Material(Shader.Find("Hidden/AdditionShader"));
        this.addMaterial.SetFloat("sample", currentSample);

        // Blit the result texture to the screen
        Graphics.Blit(this.target, destination, this.addMaterial);

        this.currentSample++;
    }

    private void InitRenderTexture() {
        if (this.target == null || this.target.width != Screen.width || this.target.height != Screen.height)
        {
            // Release render texture if we already have one
            if (this.target != null)
                this.target.Release();
            // Get a render target for Ray Tracing
            this.target = new RenderTexture(Screen.width, Screen.height, 0,
                RenderTextureFormat.ARGBFloat, RenderTextureReadWrite.Linear);
            this.target.enableRandomWrite = true;
            this.target.Create();
            // Reset sampling
            this.currentSample = 0;
        }
    }

    private void setShaderParameters() {
        RayTracingShader.SetMatrix("cameraToWorld", this.curCamera.cameraToWorldMatrix);
        RayTracingShader.SetMatrix("cameraInverseProjection", this.curCamera.projectionMatrix.inverse);
        RayTracingShader.SetVector("skyboxColor", this.skyboxColor);
        RayTracingShader.SetVector("pixelOffset", new Vector2(Random.value, Random.value));
        RayTracingShader.SetTexture(0, "_SkyboxTexture", SkyboxTexture);
        Vector3 l = DirectionalLight.transform.forward;
        RayTracingShader.SetVector("directionalLight", new Vector4(l.x, l.y, l.z, DirectionalLight.intensity));
    }

}