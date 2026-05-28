// VrRigSetup.cs
// Monta um XR Origin (VR) minimo na cena, reaproveitando o IInteractable existente.
// Camera head-tracked (TrackedPoseDriver) + VrGazeInteractor (mira+gatilho) + VrLocomotion.
// Mantem o player desktop na cena como FALLBACK, apenas neutralizando os componentes
// que conflitam (camera, audio listener, mouselook, movimentacao, interactor desktop).
//
// Menus (Unity):
//   Tools > VR Project > Setup VR Rig (current scene)
//   Tools > VR Project > Setup VR Rig in ALL Scenes
//   Tools > VR Project > Remove VR Rig (current)
#if UNITY_EDITOR
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.XR;
using UnityEditor;
using UnityEditor.SceneManagement;
using Unity.XR.CoreUtils;
using ProjetoRV.VR;

public static class VrRigSetup
{
    const string RIG_NAME = "XR Origin (VR)";

    static readonly string[] SCENES =
    {
        "Assets/Cenas/Scene_Lobby.unity",
        "Assets/Cenas/Scene_Europe.unity",
        "Assets/Cenas/Scene_America.unity",
        "Assets/Cenas/Scene_Asia.unity",
        "Assets/Cenas/Scene_Africa.unity",
    };

    [MenuItem("Tools/VR Project/Setup VR Rig (current scene)")]
    public static void SetupCurrent()
    {
        BuildRig();
        var s = SceneManager.GetActiveScene();
        EditorSceneManager.MarkSceneDirty(s);
        EditorSceneManager.SaveScene(s);
    }

    [MenuItem("Tools/VR Project/Setup VR Rig in ALL Scenes")]
    public static void SetupAll()
    {
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
        foreach (var path in SCENES)
        {
            var s = EditorSceneManager.OpenScene(path, OpenSceneMode.Single);
            BuildRig();
            EditorSceneManager.MarkSceneDirty(s);
            EditorSceneManager.SaveScene(s);
            Debug.Log("[VrRigSetup] rig em " + path);
        }
        Debug.Log("[VrRigSetup] Concluido nas " + SCENES.Length + " cenas.");
    }

    [MenuItem("Tools/VR Project/Remove VR Rig (current)")]
    public static void RemoveCurrent()
    {
        var old = GameObject.Find(RIG_NAME);
        if (old) Object.DestroyImmediate(old);
        var cc = Object.FindObjectOfType<CharacterController>();
        if (cc) SetDesktopEnabled(cc.gameObject, true);
        var s = SceneManager.GetActiveScene();
        EditorSceneManager.MarkSceneDirty(s);
        EditorSceneManager.SaveScene(s);
    }

    static void BuildRig()
    {
        // spawn = onde estava o player desktop (CharacterController)
        Vector3 spawn = Vector3.zero; float yaw = 0f;
        GameObject desktop = null;
        var cc = Object.FindObjectOfType<CharacterController>();
        if (cc) { desktop = cc.gameObject; spawn = cc.transform.position; yaw = cc.transform.eulerAngles.y; }
        spawn.y = 0f;

        // idempotente
        var old = GameObject.Find(RIG_NAME);
        if (old) Object.DestroyImmediate(old);

        var originGO = new GameObject(RIG_NAME);
        originGO.transform.SetPositionAndRotation(spawn, Quaternion.Euler(0f, yaw, 0f));
        var origin = originGO.AddComponent<XROrigin>();

        var offset = new GameObject("Camera Offset");
        offset.transform.SetParent(originGO.transform, false);

        var camGO = new GameObject("Main Camera");
        camGO.transform.SetParent(offset.transform, false);
        camGO.tag = "MainCamera";
        var cam = camGO.AddComponent<Camera>();
        camGO.AddComponent<AudioListener>();

        var tpd = camGO.AddComponent<TrackedPoseDriver>();
        tpd.positionInput = new InputActionProperty(new InputAction(
            "HMDpos", binding: "<XRHMD>/centerEyePosition", expectedControlType: "Vector3"));
        tpd.rotationInput = new InputActionProperty(new InputAction(
            "HMDrot", binding: "<XRHMD>/centerEyeRotation", expectedControlType: "Quaternion"));

        origin.Camera = cam;
        origin.CameraFloorOffsetObject = offset;
        // Device mode + Y offset = altura do "olho" constante (1.7m, mesma do player
        // desktop). Em Floor mode a camera fica em y=0 se o HMD/simulador nao reportar
        // a altura (caso da fallback EditorMouseLook), causando spawn enfiado no chao.
        origin.RequestedTrackingOriginMode = XROrigin.TrackingOriginMode.Device;
        origin.CameraYOffset = 1.7f;
        offset.transform.localPosition = new Vector3(0f, 1.7f, 0f); // garante a altura em Edit time

        camGO.AddComponent<VrGazeInteractor>();
        camGO.AddComponent<EditorMouseLook>();   // fallback p/ olhar com mouse no Editor sem simulador
        var loco = originGO.AddComponent<VrLocomotion>();
        loco.head = camGO.transform;

        if (desktop != null) SetDesktopEnabled(desktop, false);

        Debug.Log("[VrRigSetup] " + RIG_NAME + " criado (spawn " + spawn +
                  "). Player desktop mantido como fallback (componentes neutralizados).");
    }

    // Liga/desliga os componentes do player desktop que conflitam com o rig VR,
    // sem desativar o GameObject (assim outros scripts ainda o encontram).
    static void SetDesktopEnabled(GameObject player, bool enabled)
    {
        var dcam = player.GetComponentInChildren<Camera>(true);
        if (dcam) dcam.enabled = enabled;
        var dal = player.GetComponentInChildren<AudioListener>(true);
        if (dal) dal.enabled = enabled;
        foreach (var b in player.GetComponentsInChildren<MonoBehaviour>(true))
        {
            string n = b.GetType().Name;
            if (n.Contains("MouseLook") || n.Contains("RayInteractor") || n.Contains("Movement"))
                b.enabled = enabled;
        }
    }
}
#endif
