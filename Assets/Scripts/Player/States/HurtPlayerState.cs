using UnityEngine;

namespace PLAYERTWO.PlatformerProject
{
	[AddComponentMenu("PLAYER TWO/Platformer Project/Player/States/Hurt Player State")]
	public class HurtPlayerState : PlayerState
	{
		protected override void OnEnter(Player player) { }

		protected override void OnExit(Player player) { }

		protected override void OnStep(Player player)
		{
			player.Gravity();

			if (player.isGrounded && (player.verticalVelocity.y <= 0))
			{
				if (player.health.current > 0)
				{
					player.states.Change<IdlePlayerState>();
				}
				else
				{
					player.states.Change<DiePlayerState>();
				}
			}
		}

		public override void OnContact(Player player, Collider other) { }
	}
}
