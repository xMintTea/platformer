using UnityEngine;

namespace PLAYERTWO.PlatformerProject
{
	[RequireComponent(typeof(TrailRenderer))]
	[AddComponentMenu("PLAYER TWO/Platformer Project/Player/Player Spin Trail")]
	public class PlayerSpinTrail : MonoBehaviour
	{
		public Transform hand;

		protected Player m_player;
		protected TrailRenderer m_trail;

		protected virtual void InitializeTrail()
		{
			m_trail = GetComponent<TrailRenderer>();
			m_trail.enabled = false;
		}

		protected virtual void InitializeTransform()
		{
			transform.parent = hand;
			transform.localPosition = Vector3.zero;
		}

		protected virtual void InitializePlayer()
		{
			m_player = GetComponentInParent<Player>();
			m_player.states.events.onChange.AddListener(HandleActive);
		}

		protected virtual void HandleActive()
		{
			if (m_player.states.IsCurrentOfType(typeof(SpinPlayerState)))
			{
				m_trail.enabled = true;
			}
			else
			{
				m_trail.enabled = false;
			}
		}

		protected virtual void Start()
		{
			InitializeTrail();
			InitializeTransform();
			InitializePlayer();
		}
	}
}
