using UnityEngine;
using UnityEngine.SceneManagement;

namespace ProjetoRV.Systems
{
    [DefaultExecutionOrder(-2000)]
    public class BootstrapToLobby : MonoBehaviour
    {
        void Awake()
        {
            if (string.IsNullOrEmpty(ScenePortal.LastSceneLeft))
            {
                SceneManager.LoadScene("Scene_Lobby");
            }
        }
    }
}
