using UnityEngine;
using System.Collections.Generic;

namespace PLAYERTWO.PlatformerProject
{
	[AddComponentMenu("PLAYER TWO/Platformer Project/Entity/Entity Controller")]
	public class EntityController : MonoBehaviour
	{
		[Range(0, 180f)]
		public float slopeLimit = 45f;

		[Min(0)]
		public float stepOffset = 0.3f;

		[Min(0.0001f)]
		public float skinWidth = 0.01f;
		public Vector3 center;

		[Min(0)]
		[SerializeField]
		protected float m_radius = 0.5f;

		[Min(0)]
		[SerializeField]
		protected float m_height = 2f;

		public LayerMask collisionLayer = -5;

		protected const int k_maxCollisionSteps = 3;

		protected Rigidbody m_rigidbody;
		protected Collider[] m_overlaps = new Collider[128];
		protected List<Collider> m_ignoredColliders = new();

		public bool handleCollision { get; set; } = true;
		public bool handleSteps { get; set; } = true;

		/// <summary>
		/// The radius of the entity collider.
		/// </summary>
		public float radius
		{
			get { return Mathf.Max(m_radius, skinWidth); }
			set { m_radius = value; }
		}

		/// <summary>
		/// The height of the entity collider.
		/// </summary>
		public float height
		{
			get { return Mathf.Max(m_height, radius * 2); }
			set { m_height = value; }
		}

		public new CapsuleCollider collider { get; protected set; }

		public Bounds bounds => collider.bounds;
		public Vector3 capsuleOffset => transform.up * (height * 0.5f - radius);

		protected virtual void Awake()
		{
			InitializeCollider();
			InitializeRigidbody();
			RefreshCollider();
		}

		protected virtual void OnDisable() => collider.enabled = false;
		protected virtual void OnEnable() => collider.enabled = true;

		protected virtual void InitializeCollider()
		{
			collider = gameObject.AddComponent<CapsuleCollider>();
			collider.isTrigger = true;
		}

		protected virtual void InitializeRigidbody()
		{
			if (!TryGetComponent(out m_rigidbody))
			{
				m_rigidbody = gameObject.AddComponent<Rigidbody>();
			}

			m_rigidbody.isKinematic = true;
		}

		public virtual void Move(Vector3 motion)
		{
			if (!enabled) return;

			var position = transform.position;

			if (handleCollision)
			{
				var localMotion = transform.InverseTransformDirection(motion);
				var lateralMotion = new Vector3(localMotion.x, 0, localMotion.z);
				var verticalMotion = new Vector3(0, localMotion.y, 0);

				lateralMotion = transform.TransformDirection(lateralMotion);
				verticalMotion = transform.TransformDirection(verticalMotion);

				position = MoveAndSlide(position, lateralMotion);
				position = MoveAndSlide(position, verticalMotion, true);
				position = HandlePenetration(position);
			}
			else
			{
				position += motion;
			}

			transform.position = position;
		}

		public virtual void Resize(float height)
		{
			var delta = height - this.height;
			this.height = height;
			center += Vector3.up * delta * 0.5f;
			RefreshCollider();
		}

		/// <summary>
		/// Add or remove a collider to the ignore list, making it so the controller will not collide with it.
		/// </summary>
		/// <param name="collider">The collider you want to add or remove.</param>
		/// <param name="ignore">If true, the collider will be added to the ignore list.</param>
		public virtual void IgnoreCollider(Collider collider, bool ignore = true)
		{
			if (ignore)
			{
				if (!m_ignoredColliders.Contains(collider))
					m_ignoredColliders.Add(collider);
			}
			else if (m_ignoredColliders.Contains(collider))
				m_ignoredColliders.Remove(collider);
		}

		protected virtual void RefreshCollider()
		{
			collider.radius = radius - skinWidth;
			collider.height = height - skinWidth;
			collider.center = center;
		}

		protected virtual Vector3 MoveAndSlide(Vector3 position, Vector3 motion, bool verticalPass = false)
		{
			for (int i = 0; i < k_maxCollisionSteps; i++)
			{
				var moveDistance = motion.magnitude;
				var moveDirection = motion / moveDistance;

				if (moveDistance <= Mathf.Epsilon) break;

				var distance = moveDistance + radius - skinWidth;
				var origin = position + transform.rotation * center - moveDirection * radius;
				var skinOffset = transform.up * skinWidth;
				var point1 = origin + capsuleOffset - skinOffset;
				var point2 = origin - capsuleOffset + skinOffset;

				if (!verticalPass && handleSteps && height > radius * 2f)
					point2 += transform.up * stepOffset;

				var colliding = SweepTest(position, point1, point2, moveDirection, distance, out var hit);

				if (colliding && !m_ignoredColliders.Contains(hit.collider))
				{
					var safeDistance = hit.distance - skinWidth - radius;
					var offset = moveDirection * safeDistance;
					var leftover = motion - offset;
					var angle = Vector3.Angle(transform.up, hit.normal);

					position += offset;

					if (angle <= slopeLimit && verticalPass) continue;

					motion = Vector3.ProjectOnPlane(leftover, hit.normal);
				}
				else
				{
					position += motion;
					break;
				}
			}

			return position;
		}

		protected virtual Vector3 HandlePenetration(Vector3 position)
		{
			var origin = position + transform.rotation * center;
			var skinOffset = transform.up * skinWidth;
			var point1 = origin + capsuleOffset - skinOffset;
			var point2 = origin - capsuleOffset + skinOffset;
			var penetrations = Physics.OverlapCapsuleNonAlloc(point1, point2, radius,
				m_overlaps, collisionLayer, QueryTriggerInteraction.Ignore);

			for (int i = 0; i < penetrations; i++)
			{
				if (m_ignoredColliders.Contains(m_overlaps[i]))
					continue;

				if (Physics.ComputePenetration(collider, transform.position, transform.rotation,
					m_overlaps[i], m_overlaps[i].transform.position, m_overlaps[i].transform.rotation,
					out var direction, out var distance))
				{
					if (m_overlaps[i].transform == transform)
						continue;

					if (GameTags.IsPlatform(m_overlaps[i]))
					{
						position += transform.up * height * 0.5f;
						continue;
					}

					position += direction * distance;
				}
			}

			return position;
		}

		protected bool SweepTest(Vector3 position, Vector3 topSphere,
			Vector3 bottomSphere, Vector3 direction, float distance, out RaycastHit hit)
		{
			var capsuleCollision = Physics.CapsuleCast(topSphere, bottomSphere, radius,
				direction, out hit, distance, collisionLayer, QueryTriggerInteraction.Ignore);
			return capsuleCollision || Physics.Raycast(position, direction, out hit,
				distance, collisionLayer, QueryTriggerInteraction.Ignore);
		}

		public static implicit operator Collider(EntityController controller) => controller.collider;
	}
}
