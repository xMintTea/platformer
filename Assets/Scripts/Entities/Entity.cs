using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IDamageble
{
    void TakeDamage(int damage = 1);
}

public class Entity : MonoBehaviour, IDamageble
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

    public void TakeDamage(int damage = 1)
    {
        if (Health>=0)
        {
            health -= damage;
            OnTakeDamage.Invoke();
            return;
        }
        OnDeath.Invoke();
    }

    protected virtual void OnDie()
    {
        Destroy(this);
    }
}
