using UnityEngine;
using UnityEngine.InputSystem;

namespace ProjetoRV.VR
{
    /// <summary>
    /// Locomocao VR simples aplicada ao transform do XR Origin:
    /// analogico esquerdo move (relativo a direcao da cabeca), analogico direito
    /// faz snap-turn. Fallback de teclado (WASD) para testar no Editor sem simulador.
    /// </summary>
    public class VrLocomotion : MonoBehaviour
    {
        [Tooltip("Camera/cabeca usada como referencia de direcao.")]
        public Transform head;
        public float moveSpeed = 2.5f;
        public float snapTurnDegrees = 45f;

        InputAction move;
        InputAction turn;
        bool canTurn = true;

        void OnEnable()
        {
            move = new InputAction("VR_Move", InputActionType.Value, expectedControlType: "Vector2");
            move.AddBinding("<XRController>{LeftHand}/thumbstick");
            move.AddBinding("<XRController>{LeftHand}/primary2DAxis");
            move.AddCompositeBinding("2DVector")
                .With("Up", "<Keyboard>/w").With("Down", "<Keyboard>/s")
                .With("Left", "<Keyboard>/a").With("Right", "<Keyboard>/d");
            move.Enable();

            turn = new InputAction("VR_Turn", InputActionType.Value, expectedControlType: "Vector2");
            turn.AddBinding("<XRController>{RightHand}/thumbstick");
            turn.AddBinding("<XRController>{RightHand}/primary2DAxis");
            turn.Enable();
        }

        void OnDisable()
        {
            if (move != null) { move.Disable(); move.Dispose(); move = null; }
            if (turn != null) { turn.Disable(); turn.Dispose(); turn = null; }
        }

        void Update()
        {
            Transform h = head ? head : transform;

            Vector2 m = move.ReadValue<Vector2>();
            if (m.sqrMagnitude > 0.02f)
            {
                Vector3 fwd = h.forward; fwd.y = 0f; fwd.Normalize();
                Vector3 right = h.right; right.y = 0f; right.Normalize();
                Vector3 dir = right * m.x + fwd * m.y;
                transform.position += dir * (moveSpeed * Time.deltaTime);
            }

            float tx = turn.ReadValue<Vector2>().x;
            if (Mathf.Abs(tx) > 0.7f)
            {
                if (canTurn)
                {
                    transform.RotateAround(h.position, Vector3.up, Mathf.Sign(tx) * snapTurnDegrees);
                    canTurn = false;
                }
            }
            else canTurn = true;
        }
    }
}
