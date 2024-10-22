using UnityEngine;

namespace PLAYERTWO.PlatformerProject
{
	public class WallDragPlayerState : PlayerState
	{
		protected override void OnEnter(Player player)
		{
			player.ResetJumps();
			player.ResetAirSpins();
			player.ResetAirDash();
			player.velocity = Vector3.zero;
			player.skin.position += player.transform.rotation * player.stats.current.wallDragSkinOffset;
			var faceDirection = player.lastWallNormal - player.transform.up * Vector3.Dot(player.lastWallNormal, player.transform.up);
			player.FaceDirection(faceDirection, Space.World);
		}

		protected override void OnExit(Player player)
		{
			player.skin.position -= player.transform.rotation * player.stats.current.wallDragSkinOffset;

			if (!player.isGrounded && player.platform)
				player.platform.Detach(player.skin);
		}

		protected override void OnStep(Player player)
		{
			player.verticalVelocity += Vector3.down * player.stats.current.wallDragGravity * Time.deltaTime;

			var maxWallDistance = player.radius + player.stats.current.ledgeMaxForwardDistance;
			var detectingWall = player.SphereCast(-player.transform.forward, maxWallDistance,
				player.stats.current.wallDragLayers);

			if (player.isGrounded || !detectingWall)
			{
				player.states.Change<IdlePlayerState>();
				return;
			}

			if (player.inputs.GetJumpDown())
			{
				if (player.stats.current.wallJumpLockMovement)
					player.inputs.LockMovementDirection();

				player.DirectionalJump(player.localForward, player.stats.current.wallJumpHeight, player.stats.current.wallJumpDistance);
				player.states.Change<FallPlayerState>();
			}
		}

		public override void OnContact(Player player, Collider other) { }
	}
}
