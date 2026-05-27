using UnityEngine;
using UnityEngine.SceneManagement;

namespace ProjetoRV.Systems
{
    public class ScenePortal : MonoBehaviour, IInteractable
    {
        [Header("Destino")]
        [Tooltip("Nome exato da cena (como aparece em File > Build Settings, sem .unity).")]
        public string targetScene;

        public static string LastSceneLeft;

        public void Interact()
        {
            if (string.IsNullOrEmpty(targetScene))
            {
                Debug.LogWarning($"ScenePortal em '{gameObject.name}' sem targetScene definida.");
                return;
            }
            LastSceneLeft = SceneManager.GetActiveScene().name;
            SceneManager.LoadScene(targetScene);
        }
    }
}
