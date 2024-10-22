using UnityEngine;

namespace PLAYERTWO.PlatformerProject
{
	[RequireComponent(typeof(Collider), typeof(AudioSource))]
	[AddComponentMenu("PLAYER TWO/Platformer Project/Misc/Portal")]
	public class Portal : MonoBehaviour
	{
		public bool useFlash = true;
		public Portal exit;
		public float exitOffset = 1f;
		public AudioClip teleportClip;

		protected Collider m_collider;
		protected AudioSource m_audio;

		protected PlayerCamera m_camera;

		public Vector3 position => transform.position;
		public Vector3 forward => transform.forward;

		protected virtual void Start()
		{
			m_collider = GetComponent<Collider>();
			m_audio = GetComponent<AudioSource>();
			m_camera = FindObjectOfType<PlayerCamera>();
			m_collider.isTrigger = true;
		}

		protected virtual void OnTriggerEnter(Collider other)
		{
			if (!exit || !other.TryGetComponent(out Player player)) return;

			var offset = player.unsizedPosition - transform.position;
			var yOffset = Vector3.Dot(transform.up, offset);
			var localExitForward = Quaternion.FromToRotation(exit.transform.up, Vector3.up) * exit.forward;
			var lateralSpeed = player.lateralVelocity.magnitude;
			var verticalSpeed = player.verticalVelocity.y;

			player.transform.position = exit.position + exit.transform.up * yOffset;
			player.transform.rotation = exit.transform.rotation;
			player.FaceDirection(localExitForward);
			player.LockGravity();

			player.gravityField?.IgnoreCollider(player.controller);
			player.gravityField = null;

			m_camera?.Reset();

			var inputDirection = player.inputs.GetMovementCameraDirection();

			if (Vector3.Dot(inputDirection, localExitForward) < 0)
				player.FaceDirection(-localExitForward);

			player.transform.position += player.transform.forward * exit.exitOffset;
			player.lateralVelocity = player.localForward * lateralSpeed;
			player.verticalVelocity = Vector3.up * verticalSpeed;

			if (useFlash)
				Flash.instance?.Trigger();

			m_audio.PlayOneShot(teleportClip);
		}
	}
}
