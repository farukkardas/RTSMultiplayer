using System;
using Combat;
using Mirror;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Serialization;

namespace Controller
{
    public class UnitFiring : NetworkBehaviour
    {
        [SerializeField] private Targeter targeter= null;
        [SerializeField] private GameObject projectilePrefab = null;
        [SerializeField] private Transform projectileSpawnPoint = null;
        [SerializeField] private float attackRange = 5f;
        [SerializeField] private float fireRate = 0.7f;
        [SerializeField] private float rotationSpeed = 20f;

        private float lastFireTime;

        [ServerCallback]
        private void Update()
        {
            Targetable target = targeter.GetTarget();
             
            if(targeter.GetTarget() == null){ return;}
            
            if(!CanFireAtTarget()) {return;}

            Quaternion targetRotation = Quaternion.LookRotation(target.transform.position  - transform.position);

            transform.rotation =
                Quaternion.RotateTowards(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);

            if (Time.time > (1 / fireRate) + lastFireTime)
            {
                Quaternion projectileRotation = Quaternion.LookRotation(target.GetAimAtPoint().position - projectileSpawnPoint.position);
                    
                GameObject projectileInstance = Instantiate(projectilePrefab, projectileSpawnPoint.position,projectileRotation);
                
                NetworkServer.Spawn(projectileInstance,connectionToClient);
                
                lastFireTime = Time.time;
            }
        }

        [Server]
        private bool CanFireAtTarget()
        {
            return (targeter.GetTarget().transform.position - transform.position).sqrMagnitude <= attackRange * attackRange;
        }
    }
}
