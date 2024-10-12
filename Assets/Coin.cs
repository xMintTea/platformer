using System.Collections;
using System.Collections.Generic;
using DefaultNamespace.CoreMechanicObjects;
using UnityEngine;

public class Coin : MonoBehaviour, ITriggerZone
{
    public void OnTouch(PlayerController playerController)
    {
        Debug.Log("Поднята монета");
        playerController.AddCoin();
        playerController.OnCoinGet?.Invoke(gameObject);
        Destroy(gameObject);
    }
    
    void Update()
    {
        transform.Rotate(Vector3.right*Time.deltaTime);
    }
}
