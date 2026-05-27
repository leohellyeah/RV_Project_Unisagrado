using UnityEngine;

namespace ProjetoRV.Systems
{
    public class Billboard : MonoBehaviour
    {
        Transform target;

        void Start()
        {
            if (Camera.main != null) target = Camera.main.transform;
        }

        void LateUpdate()
        {
            if (target == null) return;
            Vector3 dir = transform.position - target.position;
            dir.y = 0f;
            if (dir.sqrMagnitude < 0.0001f) return;
            transform.rotation = Quaternion.LookRotation(dir);
        }
    }
}
