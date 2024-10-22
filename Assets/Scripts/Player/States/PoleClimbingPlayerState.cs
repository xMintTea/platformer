using UnityEngine;

namespace PLAYERTWO.PlatformerProject
{
	[AddComponentMenu("PLAYER TWO/Platformer Project/Player/States/Pole Climbing Player State")]
	public class PoleClimbingPlayerState : PlayerState
	{
		protected float m_collisionRadius;

		protected const float k_poleOffset = 0.01f;

		protected override void OnEnter(Player player)
		{
			player.ResetJumps();
			player.ResetAirSpins();
			player.ResetAirDash();
			player.velocity = Vector3.zero;
			player.pole.GetDirectionToPole(player.transform, out m_collisionRadius);
			player.pole.RotateToPole(player.transform);
			player.skin.position += player.transform.rotation * player.stats.current.poleClimbSkinOffset;
		}

		protected override void OnExit(Player player)
		{
			player.skin.position -= player.transform.rotation * player.stats.current.poleClimbSkinOffset;
		}

		protected override void OnStep(Player player)
		{
			var poleDirection = player.pole.GetDirectionToPole(player.transform);
			var localPoleDirection = Quaternion.FromToRotation(player.pole.transform.up, Vector3.up) * poleDirection;
			var inputDirection = player.inputs.GetMovementDirection();

			player.lateralVelocity = player.localRight * inputDirection.x * player.stats.current.climbRotationSpeed;

			if (inputDirection.z != 0)
			{
				var speed = inputDirection.z > 0 ? player.stats.current.climbUpSpeed : -player.stats.current.climbDownSpeed;
				player.verticalVelocity = Vector3.up * speed;
			}
			else
			{
				player.verticalVelocity = Vector3.zero;
			}

			if (player.inputs.GetJumpDown())
			{
				player.FaceDirection(-localPoleDirection);
				player.DirectionalJump(-localPoleDirection, player.stats.current.poleJumpHeight, player.stats.current.poleJumpDistance);
				player.states.Change<FallPlayerState>();
			}

			if (player.isGrounded)
			{
				player.states.Change<IdlePlayerState>();
			}

			player.pole.RotateToPole(player.transform);
			player.FaceDirection(localPoleDirection);

			var playerPos = player.transform.position;
			var poleCenter = player.pole.center;
			var poleUp = player.pole.transform.up;
			var center = poleCenter - poleUp * Vector3.Dot(poleCenter - playerPos, poleUp);
			var position = center - poleDirection * (m_collisionRadius + k_poleOffset);
			var offset = player.height * 0.5f + player.center.y;

			player.transform.position = player.pole.ClampPointToPoleHeight(position, offset);
		}

		public override void OnContact(Player player, Collider other) { }
	}
}
