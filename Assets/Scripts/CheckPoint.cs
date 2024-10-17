using DefaultNamespace.CoreMechanicObjects;
using UnityEngine;

namespace DefaultNamespace
{
    public class CheckPoint: MonoBehaviour, ITriggerZone
    {
        public void OnTouch(PlayerController playerController)
        {
            playerController.SetSpawnPoint(transform);
        }
    }
}