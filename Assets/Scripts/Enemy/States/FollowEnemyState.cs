using UnityEngine;

namespace PLAYERTWO.PlatformerProject
{
	[AddComponentMenu("PLAYER TWO/Platformer Project/Enemy/States/Follow Enemy State")]
	public class FollowEnemyState : EnemyState
	{
		protected override void OnEnter(Enemy enemy) { }

		protected override void OnExit(Enemy enemy) { }

		protected override void OnStep(Enemy enemy)
		{
			enemy.Gravity();
			enemy.SnapToGround();

			var head = enemy.player.position - enemy.position;
			var upOffset = Vector3.Dot(enemy.transform.up, head);
			var direction = head - enemy.transform.up * upOffset;
			var localDirection = Quaternion.FromToRotation(enemy.transform.up, Vector3.up) * direction;

			localDirection = localDirection.normalized;

			enemy.Accelerate(localDirection, enemy.stats.current.followAcceleration, enemy.stats.current.followTopSpeed);
			enemy.FaceDirectionSmooth(localDirection);
		}

		public override void OnContact(Enemy enemy, Collider other) { }
	}
}
