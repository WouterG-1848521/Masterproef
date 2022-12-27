using UnityEngine;
using System.Collections.Generic;

struct Own2Sphere {
    public Vector3 position;
    public float radius;
    public Vector3 albedo;
    public Vector3 specular;
    public float smoothness;
    public Vector3 emission;
};

public class pathtracing : MonoBehaviour {

    public ComputeShader RayTracingShader;
    public Camera curCamera;
    public Vector3 skyboxColor;
    public Texture SkyboxTexture;
    public Light DirectionalLight;

    private uint currentSample = 0;
    private Material addMaterial;
    private RenderTexture target;
    private RenderTexture converged;
    private float lastFieldOfView;
    private ComputeBuffer sphereBuffer;
    private List<Transform> transformsToWatch = new List<Transform>();

    [Header("Spheres")]
    public uint spheresMax = 100;
    public float spherePlacementRadius = 100.0f;
    public Vector2 sphereRadius = new Vector2(3.0f, 8.0f);
    public int SphereSeed = 1223832719;

    private void Awake() {
        transformsToWatch.Add(transform);
        transformsToWatch.Add(DirectionalLight.transform);
    }

    private void OnEnable() {
        this.currentSample = 0;
        this.setUpScene();
    }

    private void OnDisable() {
        if (this.sphereBuffer != null)
            this.sphereBuffer.Release();
    }

    private void Update() {
        if (this.curCamera.fieldOfView != this.lastFieldOfView) {
            this.currentSample = 0;
            this.lastFieldOfView = this.curCamera.fieldOfView;
        }

        for (int i = 0; i < this.transformsToWatch.Count; i++) {
            if (this.transformsToWatch[i].hasChanged) {
                this.currentSample = 0;
                this.transformsToWatch[i].hasChanged = false;
            }
        }
    }

    private void setUpScene() {
        Random.InitState(this.SphereSeed);
        List<Own2Sphere> spheres = new List<Own2Sphere>();

        // Add a number of random spheres
        for (int i = 0; i < this.spheresMax; i++) {
            Own2Sphere sphere = new Own2Sphere();
            // Radius and radius
            sphere.radius = this.sphereRadius.x + Random.value * (this.sphereRadius.y - this.sphereRadius.x);
            Vector2 randomPos = Random.insideUnitCircle * this.spherePlacementRadius;
            sphere.position = new Vector3(randomPos.x, sphere.radius, randomPos.y);

            // Reject spheres that are intersecting others
            foreach (Own2Sphere other in spheres) {
                float minDist = sphere.radius + other.radius;
                if (Vector3.SqrMagnitude(sphere.position - other.position) < minDist * minDist)
                    goto SkipSphere;
            }

            // Albedo and specular color
            Color color = Random.ColorHSV();
            if ( Random.value < 0.8f) { // no light emmitor
                bool metal = Random.value < 0.4f;
                if (metal) {
                    sphere.albedo = Vector3.zero;
                    sphere.specular = new Vector3(color.r, color.g, color.b);
                } else {
                    sphere.albedo = new Vector3(color.r, color.g, color.b);
                    sphere.specular = new Vector3(0.04f, 0.04f, 0.04f);
                }
                sphere.smoothness = Random.value;
            } else {    // light emmitor
                Color emission = Random.ColorHSV(0, 1, 0, 1, 3.0f, 8.0f);
                sphere.emission = new Vector3(emission.r, emission.g, emission.b);
            }

            // Add the sphere to the list
            spheres.Add(sphere);

        SkipSphere:
            continue;
        }

        // Assign to compute buffer
        if (this.sphereBuffer != null)
            this.sphereBuffer.Release();
        
        if (spheres.Count > 0) {
            this.sphereBuffer = new ComputeBuffer(spheres.Count, 56);
            this.sphereBuffer.SetData(spheres);
        }
    }

    private void setShaderParameters() {
        RayTracingShader.SetMatrix("cameraToWorld", this.curCamera.cameraToWorldMatrix);
        RayTracingShader.SetMatrix("cameraInverseProjection", this.curCamera.projectionMatrix.inverse);

        RayTracingShader.SetVector("skyboxColor", this.skyboxColor);
        RayTracingShader.SetTexture(0, "_SkyboxTexture", SkyboxTexture);

        RayTracingShader.SetVector("pixelOffset", new Vector2(Random.value, Random.value));
        RayTracingShader.SetFloat("seed", Random.value);

        Vector3 l = DirectionalLight.transform.forward;
        RayTracingShader.SetVector("directionalLight", new Vector4(l.x, l.y, l.z, DirectionalLight.intensity));

        if (this.sphereBuffer != null)  
            RayTracingShader.SetBuffer(0, "spheres", sphereBuffer);
    }

    private void InitRenderTexture() {
        if (this.target == null || this.target.width != Screen.width || this.target.height != Screen.height)
        {
            // Release render texture if we already have one
            if (this.target != null) {
                this.target.Release();
                this.converged.Release();
            }

            // Get a render target for Ray Tracing
            this.target = new RenderTexture(Screen.width, Screen.height, 0,
                RenderTextureFormat.ARGBFloat, RenderTextureReadWrite.Linear);
            this.target.enableRandomWrite = true;
            this.target.Create();
            this.converged = new RenderTexture(Screen.width, Screen.height, 0,
                RenderTextureFormat.ARGBFloat, RenderTextureReadWrite.Linear);
            this.converged.enableRandomWrite = true;
            this.converged.Create();

            // Reset sampling
            this.currentSample = 0;
        }
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
        Graphics.Blit(this.target, this.converged, this.addMaterial);
        Graphics.Blit(this.converged, destination);

        this.currentSample++;
    }

    private void OnRenderImage(RenderTexture source, RenderTexture destination) {
        this.setShaderParameters();
        Render(destination);
    }

}
