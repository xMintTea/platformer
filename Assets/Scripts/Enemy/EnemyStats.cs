using UnityEngine;

namespace PLAYERTWO.PlatformerProject
{
	[CreateAssetMenu(fileName = "NewEnemyStats", menuName = "PLAYER TWO/Platformer Project/Enemy/New Enemy Stats")]
	public class EnemyStats : EntityStats<EnemyStats>
	{
		[Header("General Stats")]
		public float gravity = 35f;
		public float snapForce = 15f;
		public float rotationSpeed = 970f;
		public float deceleration = 28f;
		public float friction = 16f;
		public float turningDrag = 28f;

		[Header("Contact Attack Stats")]
		public bool canAttackOnContact = true;
		public bool contactPushback = true;
		public float contactOffset = 0.15f;
		public int contactDamage = 1;
		public float contactPushBackForce = 18f;
		public float contactSteppingTolerance = 0.1f;

		[Header("View Stats")]
		public float spotRange = 5f;
		public float viewRange = 8f;

		[Header("Follow Stats")]
		public float followAcceleration = 10f;
		public float followTopSpeed = 4f;

		[Header("Waypoint Stats")]
		public bool faceWaypoint = true;
		public float waypointMinDistance = 0.5f;
		public float waypointAcceleration = 10f;
		public float waypointTopSpeed = 2f;
	}
}
