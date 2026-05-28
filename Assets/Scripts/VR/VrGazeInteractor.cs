using UnityEngine;
using UnityEngine.InputSystem;
using ProjetoRV.Systems;

namespace ProjetoRV.VR
{
    /// <summary>
    /// Interacao em VR por MIRA (gaze) + gatilho, reaproveitando o mesmo IInteractable
    /// do desktop. Faz raycast a partir da camera (cabeca) e, ao apertar o gatilho de
    /// qualquer controle (ou E / clique como fallback no simulador), chama Interact()
    /// no objeto mirado. Espelha o PlayerRayInteractor, mas sem depender de teclado.
    /// </summary>
    [RequireComponent(typeof(Camera))]
    public class VrGazeInteractor : MonoBehaviour
    {
        [Tooltip("Distancia maxima da mira (m).")]
        public float interactDistance = 4f;

        [Tooltip("Camadas interativas. Se ficar vazio, usa a layer 'Interactable'.")]
        public LayerMask interactMask;

        Camera cam;
        InputAction activate;

        void Awake()
        {
            cam = GetComponent<Camera>();
            if (interactMask.value == 0)
            {
                int m = LayerMask.GetMask("Interactable");
                interactMask = m != 0 ? m : (1 << 8);
            }
        }

        void OnEnable()
        {
            activate = new InputAction("VR_Activate", InputActionType.Button);
            activate.AddBinding("<XRController>/triggerButton");
            activate.AddBinding("<XRController>/trigger");
            activate.AddBinding("<Keyboard>/e");        // fallback Editor/simulador
            activate.AddBinding("<Mouse>/leftButton");  // fallback Editor/simulador
            activate.performed += OnActivate;
            activate.Enable();
        }

        void OnDisable()
        {
            if (activate != null)
            {
                activate.performed -= OnActivate;
                activate.Disable();
                activate.Dispose();
                activate = null;
            }
        }

        void OnActivate(InputAction.CallbackContext ctx)
        {
            Transform t = cam ? cam.transform : transform;
            if (Physics.Raycast(t.position, t.forward, out RaycastHit hit,
                                interactDistance, interactMask, QueryTriggerInteraction.Ignore))
            {
                var target = hit.collider.GetComponentInParent<IInteractable>();
                if (target != null) target.Interact();
            }
        }
    }
}
