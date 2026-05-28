using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.XR;

namespace ProjetoRV.VR
{
    /// <summary>
    /// Fallback de Editor: quando NAO existe um HMD (sem simulador, sem headset),
    /// permite olhar com o mouse na cena VR. No Quest (build) ou com o XR Device
    /// Simulator na cena, este componente se desabilita sozinho e deixa o
    /// TrackedPoseDriver dirigir a camera normalmente.
    /// </summary>
    public class EditorMouseLook : MonoBehaviour
    {
        [Tooltip("Graus por pixel de delta do mouse.")]
        public float sensitivity = 0.15f;

        [Tooltip("Limite vertical (pitch) em graus.")]
        public float pitchClamp = 85f;

        Transform rig;            // XR Origin (avo da camera)
        TrackedPoseDriver tpd;
        float yaw;
        float pitch;
        bool active;

        void Start()
        {
            tpd = GetComponent<TrackedPoseDriver>();
            rig = transform.parent ? transform.parent.parent : null;

            // se existe HMD (simulador ou headset real), TPD dirige; este script fica fora.
            if (InputSystem.GetDevice<XRHMD>() != null)
            {
                enabled = false;
                return;
            }

            // sem HMD: assume o controle. Desabilita o TPD pra ele nao sobrescrever
            // a rotacao a cada frame com o pose default (identidade).
            active = true;
            if (tpd) tpd.enabled = false;

            // mantem yaw/pitch atuais como referencia inicial
            if (rig) yaw = rig.eulerAngles.y;
            float px = transform.localEulerAngles.x;
            if (px > 180f) px -= 360f;
            pitch = px;
        }

        void Update()
        {
            if (!active) return;
            var mouse = Mouse.current;
            if (mouse == null) return;

            Vector2 delta = mouse.delta.ReadValue();
            yaw += delta.x * sensitivity;
            pitch -= delta.y * sensitivity;
            pitch = Mathf.Clamp(pitch, -pitchClamp, pitchClamp);

            if (rig) rig.rotation = Quaternion.Euler(0f, yaw, 0f);
            transform.localRotation = Quaternion.Euler(pitch, 0f, 0f);
        }
    }
}
