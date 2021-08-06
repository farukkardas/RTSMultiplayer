using System;
using Buildings;
using Combat;
using Mirror;
using UnityEngine;
using UnityEngine.AI;


namespace Controller
{
    public class UnitController : NetworkBehaviour
    {
        [SerializeField] private NavMeshAgent _agent = null;
        [SerializeField] private Targeter targeter = null;
        [SerializeField] private float chaseRange = 10f;


        #region Server

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
            _agent.ResetPath();
        }

        [Server]
        private void ServerHandleDie()
        {
            NetworkServer.Destroy(gameObject);
        }

        [Server]
        public void ServerMove(Vector3 position)
        {
            targeter.ClearTarget();

            if (!NavMesh.SamplePosition(position, out NavMeshHit hit, 1f, NavMesh.AllAreas))
            {
                return;
            }

            _agent.SetDestination(position);
        }

        [ServerCallback]
        private void Update()
        {
            Targetable target = targeter.GetTarget();

            if (target != null)
            {
                if ((target.transform.position - transform.position).sqrMagnitude > chaseRange * chaseRange)
                {
                    _agent.SetDestination(targeter.GetTarget().transform.position);
                }

                else if (_agent.hasPath)
                {
                    _agent.ResetPath();
                }

                return;
            }

            if (!_agent.hasPath)
            {
                return;
            }

            if (_agent.remainingDistance > _agent.stoppingDistance)
            {
                return;
            }

            _agent.ResetPath();
        }

        [Command]
        public void CmdMove(Vector3 position)
        {
          ServerMove(position);
        }

        #endregion
    }
}