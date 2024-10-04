using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Entity : MonoBehaviour
{
    [SerializeField, Range(1,5)] private int maxHealth = 1;
    [SerializeField] private GameObject EntityPrefab;

    protected int health;
    public int Health => health;
    
    public delegate void DeathEvent();
    public delegate void TakeDamageEvent();
    
    public DeathEvent OnDeath;
    public TakeDamageEvent OnTakeDamage;

    void Start()
    {
        health = maxHealth;
    }

    protected virtual void OnDie()
    {
        Destroy(this);
    }
}
