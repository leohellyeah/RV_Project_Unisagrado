using UnityEngine;

public class MouseLook360_2 : MonoBehaviour
{
    [Header("Referência (opcional)")]
    public Transform player; // pode deixar vazio: o script tenta achar sozinho

    [Header("Sensibilidade")]
    public float sensitivity = 1600f;

    [Header("Limite vertical (pitch)")]
    public float minPitch = -85f;
    public float maxPitch = 85f;

    [Header("Suavização (opcional)")]
    public bool smooth = true;
    public float smoothTime = 0.03f;

    float pitch;
    float yaw;
    float pitchVel;
    float yawVel;

    void Awake()
    {
        AutoAssignPlayer();
    }

    void Start()
    {
        if (player == null)
        {
            Debug.LogError("MouseLook360: Não encontrei o Player. " +
                           "Arraste o Player no campo 'player' OU deixe o CameraPivot como filho do Player.");
            enabled = false;
            return;
        }

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        yaw = player.eulerAngles.y;

        pitch = transform.localEulerAngles.x;
        if (pitch > 180f) pitch -= 360f;
    }

    void Update()
    {
        if (player == null) return;

        float mouseX = Input.GetAxis("Mouse X") * sensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * sensitivity * Time.deltaTime;

        // Yaw infinito (circular)
        yaw += mouseX;

        // Pitch com clamp
        pitch -= mouseY;
        pitch = Mathf.Clamp(pitch, minPitch, maxPitch);

        if (smooth)
        {
            float smoothYaw = Mathf.SmoothDampAngle(
                player.eulerAngles.y, yaw, ref yawVel, smoothTime
            );

            float smoothPitch = Mathf.SmoothDampAngle(
                GetLocalPitch(), pitch, ref pitchVel, smoothTime
            );

            player.rotation = Quaternion.Euler(0f, smoothYaw, 0f);
            transform.localRotation = Quaternion.Euler(smoothPitch, 0f, 0f);
        }
        else
        {
            player.rotation = Quaternion.Euler(0f, yaw, 0f);
            transform.localRotation = Quaternion.Euler(pitch, 0f, 0f);
        }

        // ESC solta o cursor
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }

    void AutoAssignPlayer()
    {
        // 1) Já setado no inspector
        if (player != null) return;

        // 2) Player como pai do pivot (hierarquia recomendada)
        if (transform.parent != null)
        {
            player = transform.parent;
            return;
        }

        // 3) Procura por nome "Player" na cena
        GameObject go = GameObject.Find("Player");
        if (go != null)
        {
            player = go.transform;
        }
    }

    float GetLocalPitch()
    {
        float x = transform.localEulerAngles.x;
        if (x > 180f) x -= 360f;
        return x;
    }
}