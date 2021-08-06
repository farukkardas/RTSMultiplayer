using Buildings;
using Mirror;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;

namespace Networking
{
    public class MyNetworkManager : NetworkManager
    {
        [SerializeField] private GameObject unitSpawnerPrefab = null;
        [SerializeField] private GameOverHandler gameOverHandlerPrefab = null;

        public override void OnServerAddPlayer(NetworkConnection conn)
        {
            base.OnServerAddPlayer(conn);

            RTSPlayer player = conn.identity.GetComponent<RTSPlayer>();

            player.SetTeamColor(new Color(Random.Range(0f, 1f), Random.Range(0f, 1f), Random.Range(0f, 1f)));

            GameObject unitSpawnerInstance = Instantiate(
                unitSpawnerPrefab,
                conn.identity.transform.position,
                conn.identity.transform.rotation);


            NetworkServer.Spawn(unitSpawnerInstance, conn);
        }

        public override void OnServerSceneChanged(string sceneName)
        {
            if (SceneManager.GetActiveScene().name.StartsWith("Scene_Map"))
            {
                GameOverHandler gameOverHandlerInstance = Instantiate(gameOverHandlerPrefab);

                NetworkServer.Spawn(gameOverHandlerInstance.gameObject);
            }
        }
    }
}