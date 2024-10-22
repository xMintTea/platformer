using UnityEngine;

namespace PLAYERTWO.PlatformerProject
{
	[RequireComponent(typeof(PlayerInputManager))]
	[RequireComponent(typeof(PlayerStatsManager))]
	[RequireComponent(typeof(PlayerStateManager))]
	[RequireComponent(typeof(Health))]
	[AddComponentMenu("PLAYER TWO/Platformer Project/Player/Player")]
	public class Player : Entity<Player>
	{
		[Header("Player Settings")]
		public PlayerEvents playerEvents;
		public Transform pickableSlot;
		public Transform skin;

		protected Vector3 m_respawnPosition;
		protected Quaternion m_respawnRotation;

		protected Vector3 m_skinInitialPosition;
		protected Quaternion m_skinInitialRotation;

		/// <summary>
		/// Returns the Player Input Manager instance.
		/// </summary>
		public PlayerInputManager inputs { get; protected set; }

		/// <summary>
		/// Returns the Player Stats Manager instance.
		/// </summary>
		public PlayerStatsManager stats { get; protected set; }

		/// <summary>
		/// Returns the Health instance.
		/// </summary>
		public Health health { get; protected set; }

		/// <summary>
		/// Returns true if the Player is on water.
		/// </summary>
		public bool onWater { get; protected set; }

		/// <summary>
		/// Returns true if the Player is holding an object.
		/// </summary>
		public bool holding { get; protected set; }

		/// <summary>
		/// If true, the Player can take damage.
		/// </summary>
		public bool canTakeDamage { get; set; } = true;

		/// <summary>
		/// Returns how many times the Player jumped.
		/// </summary>
		public int jumpCounter { get; protected set; }

		/// <summary>
		/// Returns how many times the Player performed an air spin.
		/// </summary>
		public int airSpinCounter { get; protected set; }

		/// <summary>
		/// Returns how many times the Player performed a Dash.
		/// </summary>
		/// <value></value>
		public int airDashCounter { get; protected set; }

		/// <summary>
		/// The last time the Player performed an dash.
		/// </summary>
		/// <value></value>
		public float lastDashTime { get; protected set; }

		/// <summary>
		/// Returns the normal of the last wall the Player touched.
		/// </summary>
		public Vector3 lastWallNormal { get; protected set; }

		/// <summary>
		/// Returns the Pole instance in which the Player is colliding with.
		/// </summary>
		public Pole pole { get; protected set; }

		/// <summary>
		/// Returns the Collider of the water the Player is swimming.
		/// </summary>
		public Collider water { get; protected set; }

		/// <summary>
		/// Returns the Collider of the ledge the Player is hanging.
		/// </summary>
		public Collider ledge { get; protected set; }

		/// <summary>
		/// Return the Pickable instance which the Player is holding.
		/// </summary>
		public Pickable pickable { get; protected set; }

		/// <summary>
		/// Returns true if the Player health is not empty.
		/// </summary>
		public virtual bool isAlive => !health.isEmpty;

		/// <summary>
		/// Returns true if the Player can stand up.
		/// </summary>
		public virtual bool canStandUp => !SphereCast(transform.up,
			originalHeight * 0.5f + radius - controller.skinWidth);

		protected const float k_waterExitOffset = 0.25f;

		protected virtual void InitializeInputs() => inputs = GetComponent<PlayerInputManager>();
		protected virtual void InitializeStats() => stats = GetComponent<PlayerStatsManager>();
		protected virtual void InitializeHealth() => health = GetComponent<Health>();
		protected virtual void InitializeTag() => tag = GameTags.Player;

		protected virtual void InitializeRespawn()
		{
			m_respawnPosition = transform.position;
			m_respawnRotation = transform.rotation;
		}

		protected virtual void InitializeSkin()
		{
			if (!skin) return;

			m_skinInitialPosition = skin.localPosition;
			m_skinInitialRotation = skin.localRotation;
		}

		/// <summary>
		/// Resets Player state, health, position, and rotation.
		/// </summary>
		public virtual void Respawn()
		{
			health.ResetHealth();
			velocity = Vector3.zero;
			gravityField = null;
			transform.SetPositionAndRotation(m_respawnPosition, m_respawnRotation);
			states.Change<IdlePlayerState>();
		}

		/// <summary>
		/// Sets the position and rotation of the Player for the next respawn.
		/// </summary>
		public virtual void SetRespawn(Vector3 position, Quaternion rotation)
		{
			m_respawnPosition = position;
			m_respawnRotation = rotation;
		}

		/// <summary>
		/// Applies damage to this Player decreasing its health with proper reaction.
		/// </summary>
		/// <param name="amount">The amount of health you want to decrease.</param>
		public override void ApplyDamage(int amount, Vector3 origin)
		{
			if (health.isEmpty || health.recovering || !canTakeDamage)
				return;

			health.Damage(amount);

			var head = origin - transform.position;
			var upOffset = Vector3.Dot(transform.up, head);
			var damageDir = (head - transform.up * upOffset).normalized;
			var localDamageDir = Quaternion.FromToRotation(transform.up, Vector3.up) * damageDir;

			FaceDirection(localDamageDir);
			lateralVelocity = -localForward * stats.current.hurtBackwardsForce;

			if (!onWater)
			{
				verticalVelocity = Vector3.up * stats.current.hurtUpwardForce;
				states.Change<HurtPlayerState>();
			}

			playerEvents.OnHurt?.Invoke();

			if (health.isEmpty)
			{
				Throw();
				playerEvents.OnDie?.Invoke();
			}
		}

		/// <summary>
		/// Kills the Player.
		/// </summary>
		public virtual void Die()
		{
			health.Set(0);
			playerEvents.OnDie?.Invoke();
		}

		/// <summary>
		/// Makes the Player transition to the Swim State.
		/// </summary>
		/// <param name="water">The instance of the water collider.</param>
		public virtual void EnterWater(Collider water)
		{
			if (!onWater && !health.isEmpty)
			{
				Throw();
				onWater = true;
				this.water = water;
				states.Change<SwimPlayerState>();
			}
		}

		/// <summary>
		/// Makes the Player exit the current water instance.
		/// </summary>
		public virtual void ExitWater()
		{
			if (onWater)
			{
				onWater = false;
			}
		}

		/// <summary>
		/// Attaches the Player to a given Pole.
		/// </summary>
		/// <param name="pole">The Pole you want to attach the Player to.</param>
		public virtual void GrabPole(Collider other)
		{
			if (!stats.current.canPoleClimb || !other.CompareTag(GameTags.Pole)) return;

			if (verticalVelocity.y <= 0 && !holding &&
				other.TryGetComponent(out Pole pole))
			{
				this.pole = pole;
				states.Change<PoleClimbingPlayerState>();
			}
		}

		protected override bool EvaluateLanding(RaycastHit hit)
		{
			return base.EvaluateLanding(hit) && !hit.collider.CompareTag(GameTags.Spring);
		}

		public override bool CanChangeToGravityField(GravityField field) =>
			base.CanChangeToGravityField(field) && !IgnoreGravityFieldChanges();

		protected override void UpdateGravityField()
		{
			if (IgnoreGravityFieldChanges()) return;

			base.UpdateGravityField();
		}

		protected virtual bool IgnoreGravityFieldChanges() =>
			states.IsCurrentOfType(typeof(PoleClimbingPlayerState),
				typeof(LedgeHangingPlayerState), typeof(LedgeClimbingPlayerState),
				typeof(SwimPlayerState));

		/// <summary>
		/// Moves the Player smoothly in a given direction.
		/// </summary>
		/// <param name="direction">The direction you want to move.</param>
		public virtual void Accelerate(Vector3 direction)
		{
			var turningDrag = isGrounded && inputs.GetRun() ? stats.current.runningTurningDrag : stats.current.turningDrag;
			var acceleration = isGrounded && inputs.GetRun() ? stats.current.runningAcceleration : stats.current.acceleration;
			var finalAcceleration = isGrounded ? acceleration : stats.current.airAcceleration;
			var topSpeed = inputs.GetRun() ? stats.current.runningTopSpeed : stats.current.topSpeed;

			Accelerate(direction, turningDrag, finalAcceleration, topSpeed);

			if (inputs.GetRunUp())
			{
				lateralVelocity = Vector3.ClampMagnitude(lateralVelocity, topSpeed);
			}
		}

		/// <summary>
		/// Moves the Player smoothly in the input direction relative to the camera.
		/// </summary>
		public virtual void AccelerateToInputDirection()
		{
			var inputDirection = inputs.GetMovementCameraDirection();
			Accelerate(inputDirection);
		}

		/// <summary>
		/// Applies the standard slope factor to the Player.
		/// </summary>
		public virtual void RegularSlopeFactor()
		{
			if (stats.current.applySlopeFactor)
				SlopeFactor(stats.current.slopeUpwardForce, stats.current.slopeDownwardForce);
		}

		/// <summary>
		/// Moves the Player smoothly in a given direction with water stats.
		/// </summary>
		/// <param name="direction">The direction you want to move.</param>
		public virtual void WaterAcceleration(Vector3 direction) =>
			Accelerate(direction, stats.current.waterTurningDrag, stats.current.swimAcceleration, stats.current.swimTopSpeed);

		/// <summary>
		/// Moves the Player smoothly in a given direction with crawling stats.
		/// </summary>
		/// <param name="direction">The direction you want to move.</param>
		public virtual void CrawlingAccelerate(Vector3 direction) =>
			Accelerate(direction, stats.current.crawlingTurningSpeed, stats.current.crawlingAcceleration, stats.current.crawlingTopSpeed);

		/// <summary>
		/// Moves the Player smoothly using the backflip stats.
		/// </summary>
		public virtual void BackflipAcceleration()
		{
			var direction = inputs.GetMovementCameraDirection();
			Accelerate(direction, stats.current.backflipTurningDrag, stats.current.backflipAirAcceleration, stats.current.backflipTopSpeed);
		}

		/// <summary>
		/// Smoothly sets Lateral Velocity to zero by its deceleration stats.
		/// </summary>
		public virtual void Decelerate() => Decelerate(stats.current.deceleration);

		/// <summary>
		/// Smoothly sets Lateral Velocity to zero by its friction stats.
		/// </summary>
		public virtual void Friction()
		{
			if (OnSlopingGround())
				Decelerate(stats.current.slopeFriction);
			else
				Decelerate(stats.current.friction);
		}

		/// <summary>
		/// Applies a downward force by its gravity stats.
		/// </summary>
		public virtual void Gravity()
		{
			if (!isGrounded && verticalVelocity.y > -stats.current.gravityTopSpeed)
			{
				var speed = verticalVelocity.y;
				var force = verticalVelocity.y > 0 ? stats.current.gravity : stats.current.fallGravity;
				speed -= force * gravityMultiplier * Time.deltaTime;
				speed = Mathf.Max(speed, -stats.current.gravityTopSpeed);
				verticalVelocity = new Vector3(0, speed, 0);
			}
		}

		/// <summary>
		/// Applies a downward force when ground by its snap stats.
		/// </summary>
		public virtual void SnapToGround() => SnapToGround(stats.current.snapForce);

		/// <summary>
		/// Rotate the Player forward to a given direction.
		/// </summary>
		/// <param name="direction">The direction you want it to face.</param>
		public virtual void FaceDirectionSmooth(Vector3 direction) => FaceDirection(direction, stats.current.rotationSpeed);

		/// <summary>
		/// Rotates the Player forward to a given direction with water stats.
		/// </summary>
		/// <param name="direction">The direction you want it to face.</param>
		public virtual void WaterFaceDirection(Vector3 direction) => FaceDirection(direction, stats.current.waterRotationSpeed);

		/// <summary>
		/// Makes a transition to the Fall State if the Player is not grounded.
		/// </summary>
		public virtual void Fall()
		{
			if (!isGrounded)
			{
				states.Change<FallPlayerState>();
			}
		}

		/// <summary>
		/// Handles ground jump with proper evaluations and height control.
		/// </summary>
		public virtual void Jump()
		{
			var canMultiJump = (jumpCounter > 0) && (jumpCounter < stats.current.multiJumps);
			var canCoyoteJump = (jumpCounter == 0) && (Time.time < lastGroundTime + stats.current.coyoteJumpThreshold);
			var holdJump = !holding || stats.current.canJumpWhileHolding;

			if ((isGrounded || onRails || canMultiJump || canCoyoteJump) && holdJump)
			{
				if (inputs.GetJumpDown())
				{
					Jump(stats.current.maxJumpHeight);
				}
			}

			if (inputs.GetJumpUp() && (jumpCounter > 0) && (verticalVelocity.y > stats.current.minJumpHeight))
			{
				verticalVelocity = Vector3.up * stats.current.minJumpHeight;
			}
		}

		/// <summary>
		/// Applies an upward force to the Player.
		/// </summary>
		/// <param name="height">The force you want to apply.</param>
		public virtual void Jump(float height)
		{
			jumpCounter++;
			verticalVelocity = Vector3.up * height;
			states.Change<FallPlayerState>();
			playerEvents.OnJump?.Invoke();
		}

		/// <summary>
		/// Applies jump force to the Player in a given direction.
		/// </summary>
		/// <param name="direction">The direction that you want to jump.</param>
		/// <param name="height">The upward force that you want to apply.</param>
		/// <param name="distance">The force towards the direction that you want to apply.</param>
		public virtual void DirectionalJump(Vector3 direction, float height, float distance)
		{
			jumpCounter++;
			verticalVelocity = Vector3.up * height;
			lateralVelocity = direction * distance;
			playerEvents.OnJump?.Invoke();
		}

		/// <summary>
		/// Sets the air dash counter to zero.
		/// </summary>
		public virtual void ResetAirDash() => airDashCounter = 0;

		/// <summary>
		/// Sets the jump counter to zero affecting further jump evaluations.
		/// </summary>
		public virtual void ResetJumps() => jumpCounter = 0;

		/// <summary>
		/// Sets the jump couter to a specific value.
		/// </summary>
		/// <param name="amount">The amount of jumps.</param>
		public virtual void SetJumps(int amount) => jumpCounter = amount;

		/// <summary>
		/// Sets the air spin counter back to zero.
		/// </summary>
		public virtual void ResetAirSpins() => airSpinCounter = 0;

		public virtual void Spin()
		{
			var canAirSpin = (isGrounded || stats.current.canAirSpin) && airSpinCounter < stats.current.allowedAirSpins;

			if (stats.current.canSpin && canAirSpin && !holding && inputs.GetSpinDown())
			{
				if (!isGrounded)
				{
					airSpinCounter++;
				}

				states.Change<SpinPlayerState>();
				playerEvents.OnSpin?.Invoke();
			}
		}

		public virtual void PickAndThrow()
		{
			if (stats.current.canPickUp && inputs.GetPickAndDropDown())
			{
				if (!holding)
				{
					if (CapsuleCast(transform.forward,
						stats.current.pickDistance, out var hit))
					{
						if (hit.transform.TryGetComponent(out Pickable pickable))
						{
							PickUp(pickable);
						}
					}
				}
				else
				{
					Throw();
				}
			}
		}

		public virtual void PickUp(Pickable pickable)
		{
			if (!holding && (isGrounded || stats.current.canPickUpOnAir))
			{
				holding = true;
				this.pickable = pickable;
				pickable.PickUp(pickableSlot);
				pickable.onRespawn.AddListener(RemovePickable);
				playerEvents.OnPickUp?.Invoke();
			}
		}

		public virtual void Throw()
		{
			if (holding)
			{
				var force = lateralVelocity.magnitude * stats.current.throwVelocityMultiplier;
				pickable.Release(transform.forward, force);
				pickable = null;
				holding = false;
				playerEvents.OnThrow?.Invoke();
			}
		}

		public virtual void RemovePickable()
		{
			if (holding)
			{
				pickable = null;
				holding = false;
			}
		}

		public virtual void AirDive()
		{
			if (stats.current.canAirDive && !isGrounded && !holding && inputs.GetAirDiveDown())
			{
				states.Change<AirDivePlayerState>();
				playerEvents.OnAirDive?.Invoke();
			}
		}

		public virtual void StompAttack()
		{
			if (!isGrounded && !holding && stats.current.canStompAttack && inputs.GetStompDown())
			{
				states.Change<StompPlayerState>();
			}
		}

		public virtual void LedgeGrab()
		{
			if (stats.current.canLedgeHang && verticalVelocity.y < 0 && !holding &&
				states.ContainsStateOfType(typeof(LedgeHangingPlayerState)) &&
				DetectingLedge(stats.current.ledgeMaxForwardDistance,
				stats.current.ledgeMaxDownwardDistance, out var hit))
			{
				if (Vector3.Angle(hit.normal, transform.up) > 0) return;
				if (hit.collider is CapsuleCollider || hit.collider is SphereCollider) return;

				var ledgeDistance = radius + stats.current.ledgeMaxForwardDistance;
				var lateralOffset = transform.forward * ledgeDistance;
				var verticalOffset = -transform.up * height * 0.5f - center;
				ledge = hit.collider;
				velocity = Vector3.zero;
				transform.position = hit.point - lateralOffset + verticalOffset;
				HandlePlatform(hit.collider);
				states.Change<LedgeHangingPlayerState>();
				playerEvents.OnLedgeGrabbed?.Invoke();
			}
		}

		public virtual void Backflip(float force)
		{
			if (stats.current.canBackflip && !holding)
			{
				verticalVelocity = Vector3.up * stats.current.backflipJumpHeight;
				lateralVelocity = -localForward * force;
				states.Change<BackflipPlayerState>();
				playerEvents.OnBackflip.Invoke();
			}
		}

		public virtual void Dash()
		{
			var canAirDash = stats.current.canAirDash && !isGrounded &&
				airDashCounter < stats.current.allowedAirDashes;
			var canGroundDash = stats.current.canGroundDash && isGrounded &&
				Time.time - lastDashTime > stats.current.groundDashCoolDown;

			if (inputs.GetDashDown() && (canAirDash || canGroundDash))
			{
				if (!isGrounded) airDashCounter++;

				lastDashTime = Time.time;
				states.Change<DashPlayerState>();
			}
		}

		public virtual void Glide()
		{
			if (!isGrounded && inputs.GetGlide() &&
				verticalVelocity.y <= 0 && stats.current.canGlide)
				states.Change<GlidingPlayerState>();
		}

		/// <summary>
		/// Sets the Skin parent to a given transform.
		/// </summary>
		/// <param name="parent">The transform you want to parent the skin to.</param>
		public virtual void SetSkinParent(Transform parent, Vector3 offset = default)
		{
			if (!skin) return;

			skin.parent = parent;
			skin.position += transform.rotation * offset;
		}

		/// <summary>
		/// Resets the Skin parenting to its initial one, with original position and rotation.
		/// </summary>
		public virtual void ResetSkinParent()
		{
			if (!skin) return;

			skin.parent = transform;
			skin.localPosition = m_skinInitialPosition;
			skin.localRotation = m_skinInitialRotation;
		}

		public virtual void WallDrag(Collider other)
		{
			if (holding || !stats.current.canWallDrag || verticalVelocity.y > 0) return;

			var maxWallDistance = radius + stats.current.ledgeMaxForwardDistance;
			var minGroundDistance = height * 0.5f + stats.current.minGroundDistanceToDrag;

			var detectingLedge = DetectingLedge(maxWallDistance, height, out _);
			var detectingWall = SphereCast(transform.forward, maxWallDistance,
				out var hit, stats.current.wallDragLayers);
			var detectingGround = SphereCast(-transform.up, minGroundDistance);
			var wallAngle = Vector3.Angle(transform.up, hit.normal);

			if (!detectingWall || detectingGround || detectingLedge ||
				wallAngle < stats.current.minWallAngleToDrag)
				return;

			HandlePlatform(hit.collider);
			lastWallNormal = hit.normal;
			states.Change<WallDragPlayerState>();
		}

		public virtual void PushRigidbody(Collider other) =>
			PushRigidbody(other, Quaternion.FromToRotation(Vector3.up, transform.up) * lateralVelocity);

		public virtual void PushRigidbody(Collider other, Vector3 motion)
		{
			var point = transform.InverseTransformPoint(stepPosition);
			var otherPoint = transform.InverseTransformPoint(other.bounds.center);

			if (point.y <= otherPoint.y && other.TryGetComponent(out Rigidbody rigidbody))
			{
				if (rigidbody.isKinematic)
					return;

				var motionMag = Mathf.Max(1, motion.magnitude);
				var pushForce = stats.current.pushForce * motionMag;
				var force = motion.normalized * pushForce;
				var closestPoint = other.ClosestPoint(position);
				rigidbody.AddForceAtPosition(force, closestPoint);
			}
		}

		protected virtual bool DetectingLedge(float forwardDistance, float downwardDistance, out RaycastHit ledgeHit)
		{
			var contactOffset = Physics.defaultContactOffset + positionDelta;
			var ledgeMaxDistance = radius + forwardDistance;
			var ledgeHeightOffset = height * 0.5f + contactOffset;
			var upwardOffset = transform.up * ledgeHeightOffset;
			var forwardOffset = transform.forward * ledgeMaxDistance;

			if (Physics.Raycast(position + upwardOffset, transform.forward, ledgeMaxDistance,
				Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore) ||
				Physics.Raycast(position + forwardOffset * .01f, transform.up, ledgeHeightOffset,
				Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore))
			{
				ledgeHit = new RaycastHit();
				return false;
			}

			var origin = position + upwardOffset + forwardOffset;
			var distance = downwardDistance + contactOffset;

			return Physics.Raycast(origin, -transform.up, out ledgeHit, distance,
				stats.current.ledgeHangingLayers, QueryTriggerInteraction.Ignore);
		}

		public virtual void StartGrind() => states.Change<RailGrindPlayerState>();

		protected virtual void HandleWaterCollision(Collider other)
		{
			if (!other.CompareTag(GameTags.VolumeWater)) return;

			if (onWater)
			{
				var exitPoint = position - transform.up * k_waterExitOffset;

				if (!GravityHelper.IsPointInsideField(other, exitPoint))
					ExitWater();

				return;
			}

			if (GravityHelper.IsPointInsideField(other, unsizedPosition))
				EnterWater(other);
		}

		protected virtual void HandleWaterExitCollision(Collider other)
		{
			if (other.CompareTag(GameTags.VolumeWater))
				ExitWater();
		}

		protected override void Awake()
		{
			base.Awake();
			InitializeInputs();
			InitializeStats();
			InitializeHealth();
			InitializeTag();
			InitializeRespawn();

			entityEvents.OnGroundEnter.AddListener(() =>
			{
				ResetJumps();
				ResetAirSpins();
				ResetAirDash();
			});

			entityEvents.OnRailsEnter.AddListener(() =>
			{
				ResetJumps();
				ResetAirSpins();
				ResetAirDash();
				StartGrind();
			});
		}

		protected virtual void OnTriggerStay(Collider other)
		{
			HandleWaterCollision(other);
		}

		protected virtual void OnTriggerExit(Collider other)
		{
			HandleWaterExitCollision(other);
		}
	}
}
