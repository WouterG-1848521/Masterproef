using UnityEngine;

[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(MeshFilter))]

public class ObjComponent : MonoBehaviour {

    private void OnEnable() {
        tracingMaster.RegisterObject(this);
    }

    private void OnDisable() {
        tracingMaster.UnregisterObject(this);
    }

}