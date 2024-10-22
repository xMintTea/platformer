using UnityEngine;

namespace PLAYERTWO.PlatformerProject
{
	[AddComponentMenu("PLAYER TWO/Platformer Project/Player/States/Fall Player State")]
	public class FallPlayerState : PlayerState
	{
		protected override void OnEnter(Player player) { }

		protected override void OnExit(Player player) { }

		protected override void OnStep(Player player)
		{
			player.Gravity();
			player.SnapToGround();
			player.FaceDirectionSmooth(player.lateralVelocity);
			player.AccelerateToInputDirection();
			player.Jump();
			player.Spin();
			player.PickAndThrow();
			player.AirDive();
			player.StompAttack();
			player.LedgeGrab();
			player.Dash();
			player.Glide();

			if (player.isGrounded)
			{
				player.states.Change<IdlePlayerState>();
			}
		}

		public override void OnContact(Player player, Collider other)
		{
			player.PushRigidbody(other);
			player.WallDrag(other);
			player.GrabPole(other);
		}
	}
}
