using UnityEngine;

namespace ProjetoRV.Systems
{
    [DefaultExecutionOrder(-1000)]
    public class LobbySpawnController : MonoBehaviour
    {
        void Awake()
        {
            GameObject player = GameObject.Find("Player");
            if (player == null) return;

            Vector3 spawnPos;
            Quaternion spawnRot;

            switch (ScenePortal.LastSceneLeft)
            {
                case "Scene_Europe":
                    spawnPos = new Vector3(3.5f, 0f, 0f);
                    spawnRot = Quaternion.Euler(0f, -90f, 0f);
                    break;
                case "Scene_Asia":
                    spawnPos = new Vector3(-3.5f, 0f, 0f);
                    spawnRot = Quaternion.Euler(0f, 90f, 0f);
                    break;
                case "Scene_Africa":
                    spawnPos = new Vector3(0f, 0f, -3.5f);
                    spawnRot = Quaternion.identity;
                    break;
                case "Scene_America":
                    spawnPos = new Vector3(0f, 0f, 3.5f);
                    spawnRot = Quaternion.Euler(0f, 180f, 0f);
                    break;
                default:
                    return;
            }

            CharacterController cc = player.GetComponent<CharacterController>();
            if (cc != null) cc.enabled = false;
            player.transform.SetPositionAndRotation(spawnPos, spawnRot);
            if (cc != null) cc.enabled = true;
        }
    }
}
