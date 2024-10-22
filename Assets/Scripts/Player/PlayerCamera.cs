using UnityEngine;
using Cinemachine;

namespace PLAYERTWO.PlatformerProject
{
	[RequireComponent(typeof(CinemachineVirtualCamera))]
	[AddComponentMenu("PLAYER TWO/Platformer Project/Player/Player Camera")]
	public class PlayerCamera : MonoBehaviour
	{
		[Header("Camera Settings")]
		public Player player;
		public float maxDistance = 15f;
		public float initialAngle = 20f;
		public float heightOffset = 1f;

		[Header("Following Settings")]
		public float verticalUpDeadZone = 0.15f;
		public float verticalDownDeadZone = 0.15f;
		public float verticalAirUpDeadZone = 4f;
		public float verticalAirDownDeadZone = 0;
		public float maxVerticalSpeed = 10f;
		public float maxAirVerticalSpeed = 100f;
		public float upwardRotationSpeed = 90f;

		[Header("Orbit Settings")]
		public bool canOrbit = true;
		public bool canOrbitWithVelocity = true;
		public float orbitVelocityMultiplier = 5;

		[Range(0, 90)]
		public float verticalMaxRotation = 80;

		[Range(-90, 0)]
		public float verticalMinRotation = -20;

		[Header("Sensitivity Settings")]
		public float xSensitivity = 1f;
		public float ySensitivity = 1f;
		public float globalSensitivity = 1f;

		protected float m_cameraDistance;
		protected float m_cameraTargetYaw;
		protected float m_cameraTargetPitch;

		protected Vector3 m_cameraTargetPosition;
		protected Quaternion m_currentUpRotation;

		protected Camera m_camera;
		protected CinemachineVirtualCamera m_virtualCamera;
		protected Cinemachine3rdPersonFollow m_cameraBody;
		protected CinemachineBrain m_brain;

		protected Transform m_target;

		protected string k_targetName = "Player Follower Camera Target";

		public bool freeze { get; set; }

		protected virtual void InitializeComponents()
		{
			if (!player)
			{
				player = FindObjectOfType<Player>();
			}

			m_camera = Camera.main;
			m_virtualCamera = GetComponent<CinemachineVirtualCamera>();
			m_cameraBody = m_virtualCamera.AddCinemachineComponent<Cinemachine3rdPersonFollow>();
			m_brain = m_camera.GetComponent<CinemachineBrain>();
		}

		protected virtual void InitializeFollower()
		{
			m_target = new GameObject(k_targetName).transform;
			m_target.position = player.transform.position;
		}

		protected virtual void InitializeCamera()
		{
			m_virtualCamera.Follow = m_target.transform;
			m_virtualCamera.LookAt = player.transform;

			Reset();
		}

		protected virtual bool VerticalFollowingStates()
		{
			return player.states.IsCurrentOfType(typeof(SwimPlayerState)) ||
				player.states.IsCurrentOfType(typeof(PoleClimbingPlayerState)) ||
				player.states.IsCurrentOfType(typeof(WallDragPlayerState)) ||
				player.states.IsCurrentOfType(typeof(LedgeHangingPlayerState)) ||
				player.states.IsCurrentOfType(typeof(LedgeClimbingPlayerState)) ||
				player.states.IsCurrentOfType(typeof(RailGrindPlayerState));
		}

		public virtual void Reset()
		{
			m_cameraDistance = maxDistance;
			m_cameraTargetPitch = initialAngle;
			m_cameraTargetYaw = player.transform.rotation.eulerAngles.y;
			m_cameraTargetPosition = player.unsizedPosition + player.transform.up * heightOffset;
			m_currentUpRotation = Quaternion.FromToRotation(Vector3.up, player.transform.up);
			MoveTarget();
			m_brain.ManualUpdate();
		}

		protected virtual void HandleOffset()
		{
			var grounded = player.isGrounded && player.verticalVelocity.y <= 0;
			var target = player.unsizedPosition + player.transform.up * heightOffset;
			var head = target - m_cameraTargetPosition;

			var xOffset = Vector3.Dot(head, player.transform.right);
			var yOffset = Vector3.Dot(head, player.transform.up);
			var zOffset = Vector3.Dot(head, player.transform.forward);

			var targetXOffset = xOffset;
			var targetYOffset = 0f;
			var targetZOffset = zOffset;

			var maxGroundDelta = maxVerticalSpeed * Time.deltaTime;
			var maxAirDelta = maxAirVerticalSpeed * Time.deltaTime;

			if (grounded || VerticalFollowingStates())
			{
				if (yOffset > verticalUpDeadZone)
				{
					var offset = yOffset - verticalUpDeadZone;
					targetYOffset += Mathf.Min(offset, maxGroundDelta);
				}
				else if (yOffset < verticalDownDeadZone)
				{
					var offset = yOffset - verticalDownDeadZone;
					targetYOffset += Mathf.Max(offset, -maxGroundDelta);
				}
			}
			else if (yOffset > verticalAirUpDeadZone)
			{
				var offset = yOffset - verticalAirUpDeadZone;
				targetYOffset += Mathf.Min(offset, maxAirDelta);
			}
			else if (yOffset < verticalAirDownDeadZone)
			{
				var offset = yOffset - verticalAirDownDeadZone;
				targetYOffset += Mathf.Max(offset, -maxAirDelta);
			}

			var rightOffset = player.transform.right * targetXOffset;
			var upOffset = player.transform.up * targetYOffset;
			var forwardOffset = player.transform.forward * targetZOffset;

			m_cameraTargetPosition = m_cameraTargetPosition + rightOffset + upOffset + forwardOffset;
		}

		protected virtual void HandleOrbit()
		{
			if (canOrbit)
			{
				var direction = player.inputs.GetLookDirection();

				if (direction.sqrMagnitude > 0)
				{
					var usingMouse = player.inputs.IsLookingWithMouse();
					var deltaTimeMultiplier = usingMouse ? Time.timeScale : Time.deltaTime;
					var xSensitivity = this.xSensitivity * globalSensitivity * deltaTimeMultiplier;
					var ySensitivity = this.ySensitivity * globalSensitivity * deltaTimeMultiplier;

					m_cameraTargetYaw += direction.x * xSensitivity;
					m_cameraTargetPitch -= direction.z * ySensitivity;
					m_cameraTargetPitch = ClampAngle(m_cameraTargetPitch, verticalMinRotation, verticalMaxRotation);
				}
			}
		}

		protected virtual void HandleVelocityOrbit()
		{
			if (canOrbitWithVelocity && player.isGrounded)
			{
				var localVelocity = m_target.InverseTransformVector(player.velocity);
				m_cameraTargetYaw += localVelocity.x * orbitVelocityMultiplier * Time.deltaTime;
			}
		}

		protected virtual void MoveTarget()
		{
			var upRotationDelta = upwardRotationSpeed * Time.deltaTime;
			var upRotation = Quaternion.FromToRotation(Vector3.up, player.transform.up);

			m_target.position = m_cameraTargetPosition;
			m_currentUpRotation = Quaternion.RotateTowards(m_currentUpRotation, upRotation, upRotationDelta);
			m_target.rotation = m_currentUpRotation * Quaternion.Euler(m_cameraTargetPitch, m_cameraTargetYaw, 0.0f);
			m_cameraBody.CameraDistance = m_cameraDistance;
		}

		protected virtual void Start()
		{
			InitializeComponents();
			InitializeFollower();
			InitializeCamera();
		}

		protected virtual void LateUpdate()
		{
			if (freeze) return;

			HandleOrbit();
			HandleVelocityOrbit();
			HandleOffset();
			MoveTarget();
		}

		protected virtual float ClampAngle(float angle, float min, float max)
		{
			if (angle < -360)
			{
				angle += 360;
			}

			if (angle > 360)
			{
				angle -= 360;
			}

			return Mathf.Clamp(angle, min, max);
		}
	}
}
