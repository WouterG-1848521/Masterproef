#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum mode {
    normal,
    depthmap,
    deptTOF
}

public class CameraSwitch : MonoBehaviour {

    public mode currentMode = mode.normal;

    public GameObject depthCustomPass;

#if ENABLE_INPUT_SYSTEM
    InputAction switchToDepthmap;
    InputAction switchToDepthTOF;
    InputAction switchToNormal;

#endif


    // Start is called before the first frame update
    void Start() {
#if ENABLE_INPUT_SYSTEM
        switchToDepthmap = new InputAction("SwitchToDepthmap", binding: "<Gamepad>/x");
        switchToDepthmap.AddBinding("<Keyboard>/v");

        switchToDepthTOF = new InputAction("switchToDepthTOF", binding: "<Gamepad>/y");
        switchToDepthTOF.AddBinding("<Keyboard>/b");

        switchToNormal = new InputAction("switchToNormal", binding: "<Gamepad>/b");
        switchToNormal.AddBinding("<Keyboard>/n");

        switchToDepthmap.Enable();
        switchToDepthTOF.Enable();
        switchToNormal.Enable();
#endif
        setDepthCustomPass(false);
    }

    // Update is called once per frame
    void Update() {
#if ENABLE_INPUT_SYSTEM
        if (switchToDepthmap.triggered) {
            currentMode = mode.depthmap;
            setDepthCustomPass(true);
        } else if (switchToDepthTOF.triggered) {
            currentMode = mode.deptTOF;
            setDepthCustomPass(false);
        } else if (switchToNormal.triggered) {
            currentMode = mode.normal;
            setDepthCustomPass(false);
        }
#else
        if (Input.GetKeyDown(KeyCode.V)) {
            currentMode = mode.depthmap;
            setDepthCustomPass(true);
        } else if (Input.GetKeyDown(KeyCode.B)) {
            currentMode = mode.deptTOF;
            setDepthCustomPass(false);
        } else if (Input.GetKeyDown(KeyCode.N)) {
            currentMode = mode.normal;
            setDepthCustomPass(false);
        }
#endif

    if (currentMode == mode.depthmap) {
        GetComponent<Camera>().depthTextureMode = DepthTextureMode.Depth;
    }
        
    }


    void setDepthCustomPass(bool enabled) {
        if (enabled) {
            depthCustomPass.SetActive(true);
        } else {
            depthCustomPass.SetActive(false);
        }
    }
}
