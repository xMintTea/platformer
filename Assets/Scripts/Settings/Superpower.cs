using UnityEngine;

[CreateAssetMenu(menuName = "Create Superpower", fileName = "Superpower", order = 0)]
public class Superpower: ScriptableObject
{
    public float duration;
    public float speedBoost;
    public float jumpBoost;
    // add more properties as needed
}