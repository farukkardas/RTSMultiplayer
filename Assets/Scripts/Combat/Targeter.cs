using Buildings;
using Mirror;
using UnityEngine;

namespace Combat
{
    public class Targeter : NetworkBehaviour
    {
        [SerializeField] private Targetable target;

        
        public Targetable GetTarget()
        {
            return target;
        }

        public override void OnStartServer()
        {
            GameOverHandler.ServerOnGameOver += ServerHandleGameOver;
        }

        public override void OnStopServer()
        {
            GameOverHandler.ServerOnGameOver -= ServerHandleGameOver;

        }

        [Server]
        private void ServerHandleGameOver()
        {
            ClearTarget();
        }

        [Command]
        public void CmdSetTarget(GameObject targetGameObject)
        {
            if (!targetGameObject.TryGetComponent<Targetable>(out Targetable targetable))
            {
                return;
            }

            target = targetable;
        }

        [Server]
        public void ClearTarget()
        {
            target = null;
        }
    }
}