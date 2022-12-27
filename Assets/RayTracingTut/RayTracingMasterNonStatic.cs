using UnityEngine;
using System.Collections.Generic;

struct Sphere {
    public Vector3 position;
    public float radius;
    public Vector3 albedo;
    public Vector3 specular;
    // public float smoothness;
};

public class RayTracingMasterNonStatic : MonoBehaviour {

    public ComputeShader RayTracingShader;
    public Camera curCamera;
    public Vector3 skyboxColor;
    public Texture SkyboxTexture;
    public Light DirectionalLight;

    private uint currentSample = 0;
    private Material addMaterial;
    private RenderTexture target;

    [Header("Spheres")]
    public Vector2 sphereRadius = new Vector2(3.0f, 8.0f);
    public uint spheresMax = 100;
    public float spherePlacementRadius = 100.0f;
    private ComputeBuffer sphereBuffer;

    private void OnEnable() {
        this.setUpScene();
    }

    private void OnDisable() {
        if (this.sphereBuffer != null)
            this.sphereBuffer.Release();
    }

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

        RayTracingShader.SetBuffer(0, "spheres", sphereBuffer);
    }


    private void setUpScene() {
        List<Sphere> spheres = new List<Sphere>();
        // Add a number of random spheres
        for (int i = 0; i < this.spheresMax; i++)
        {
            Sphere sphere = new Sphere();
            // Radius and radius
            sphere.radius = this.sphereRadius.x + Random.value * (this.sphereRadius.y - this.sphereRadius.x);
            Vector2 randomPos = Random.insideUnitCircle * this.spherePlacementRadius;
            sphere.position = new Vector3(randomPos.x, sphere.radius, randomPos.y);
            // Reject spheres that are intersecting others
            foreach (Sphere other in spheres) {
                float minDist = sphere.radius + other.radius;
                if (Vector3.SqrMagnitude(sphere.position - other.position) < minDist * minDist)
                    goto SkipSphere;
            }
            // Albedo and specular color
            Color color = Random.ColorHSV();
            bool metal = Random.value < 0.5f;
            if (metal) {
                sphere.albedo = Vector3.zero;
                sphere.specular = new Vector3(0.6f, 0.6f, 0.6f);
            } else {
                sphere.albedo = new Vector3(color.r, color.g, color.b);
                sphere.specular = new Vector3(0.04f, 0.04f, 0.04f);
            }
            // Add the sphere to the list
            spheres.Add(sphere);
        SkipSphere:
            continue;
        }
        // Assign to compute buffer
        this.sphereBuffer = new ComputeBuffer(spheres.Count, 40);
        this.sphereBuffer.SetData(spheres);
    }

}