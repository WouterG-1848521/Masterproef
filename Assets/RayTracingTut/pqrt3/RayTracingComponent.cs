using UnityEngine;

[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(MeshFilter))]

public class RayTracingComponent : MonoBehaviour {

    [Header("Spheres")]
    public Vector3 albedo;
    public Vector3 specular;
    public float smoothness;
    public Vector3 emmission;

    private void OnEnable() {
        pathtracing2.RegisterObject(this);
    }

    private void OnDisable() {
        pathtracing2.UnregisterObject(this);
    }

}