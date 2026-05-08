using UnityEngine;
using UnityEngine.UI;

namespace ProjetoRV.Systems
{
    public class PlayerRayInteractor : MonoBehaviour
    {
        [Header("Ray Settings")]
        public float interactDistance = 3f;
        public LayerMask interactMask;

        [Header("UI")]
        public Image crosshairImage;
        public Color defaultColor = Color.white;
        public Color highlightColor = Color.red;

        [Header("Input")]
        public KeyCode interactKey = KeyCode.E;

        private IInteractable currentTarget;

        void Update()
        {
            Ray ray = new Ray(transform.position, transform.forward);

            if (Physics.Raycast(ray, out RaycastHit hit, interactDistance, interactMask))
            {
                currentTarget = hit.collider.GetComponentInParent<IInteractable>();

                if (currentTarget != null)
                {
                    if (crosshairImage) crosshairImage.color = highlightColor;

                    if (Input.GetKeyDown(interactKey))
                    {
                        currentTarget.Interact();
                    }
                    return;
                }
            }

            // No target
            currentTarget = null;
            if (crosshairImage) crosshairImage.color = defaultColor;
        }
    }

    public interface IInteractable
    {
        void Interact();
    }
}
