using DefaultNamespace.CoreMechanicObjects;
using UnityEngine;

public class SwimableObject: MonoBehaviour, ITriggerZone
{
    public void OnTouch(PlayerController playerController)
    {
        //playerController.Swim();
    }
}