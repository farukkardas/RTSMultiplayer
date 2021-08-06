using Mirror;
using UnityEngine;

namespace Combat
{
    public class Targetable : NetworkBehaviour
    {
        [SerializeField] private Transform aimAtPoint = null;

        public Transform GetAimAtPoint()
        {
            return aimAtPoint;
        }
    }
}
