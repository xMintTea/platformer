using UnityEngine;

namespace DefaultNamespace.CoreMechanicObjects
{
    public class DieZone: MonoBehaviour, ITriggerZone
    {
        public void OnTouch(PlayerController playerController)
        {
            Destroy(playerController.gameObject);
        }
    }
}