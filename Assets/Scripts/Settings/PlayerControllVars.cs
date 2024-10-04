using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(menuName = "Create PlayerControllVars", fileName = "PlayerControllVars", order = 0)]
public class PlayerControllVars : ScriptableObject
{
    [Header("Abilities")]
    public bool isJump;
    public bool isSwim;
    public bool isClimb;
    public bool isFly;
    public bool isHighJump;
    
    [Header("Main Values")]
    public float accelerate = 2.5f;
    public float maxSpeed = 10;
    public int backwardSpeed = 5;

    [Header("Jump Values")]
    public int jumpPower = 10;
    
    [Header("Climb Values")]
    public int climbSpeed = 10;
    public LayerMask climbLayer;
    
    [Header("Swim Values")] 
    public float swimTime;
    public int swimSpeed;
    
    [Header("Fly Values")] 
    public float flyTime;
    public float flyMaxTime;
    public int flySpeed;
}