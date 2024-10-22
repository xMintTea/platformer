using UnityEngine;
using UnityEngine.Events;

namespace PLAYERTWO.PlatformerProject
{
	[RequireComponent(typeof(Collider), typeof(AudioSource))]
	[AddComponentMenu("PLAYER TWO/Platformer Project/Misc/Breakable")]
	public class Breakable : MonoBehaviour
	{
		public GameObject display;
		public AudioClip clip;

		/// <summary>
		/// Called when this object breaks.
		/// </summary>
		public UnityEvent OnBreak;

		protected Collider m_collider;
		protected AudioSource m_audio;
		protected Rigidbody m_rigidBody;

		public bool broken { get; protected set; }

		public virtual void Break()
		{
			if (!broken)
			{
				if (m_rigidBody)
				{
					m_rigidBody.isKinematic = true;
				}

				broken = true;
				display.SetActive(false);
				m_collider.enabled = false;
				m_audio.PlayOneShot(clip);
				OnBreak?.Invoke();
			}
		}

		protected virtual void Start()
		{
			m_audio = GetComponent<AudioSource>();
			m_collider = GetComponent<Collider>();
			TryGetComponent(out m_rigidBody);
		}
	}
}
