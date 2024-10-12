using UnityEngine;

namespace DefaultNamespace.CoreMechanicObjects
{
    public class FinishZone: MonoBehaviour, ITriggerZone
    {
        public void OnTouch(PlayerController playerController)
        {
            Debug.Log("Finish");
        }
    }
}