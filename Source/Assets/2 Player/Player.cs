using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;


namespace Game
{
	public class Player : MonoBehaviour
	{
		#region Head
		[Serializable]
		public class Head
		{
			public float lookSensitivity = 1f; // Move to settings
			public float softClamp = 25f;

			[Header("Readonly")]
			public float yaw;
			public float pitch;
		}
		#endregion Head
		#region Movement
		[Serializable]
		public class Movement
		{
			public float runSpeed = 24;
			public float dashSpeed = 100;
			public float speedModifier = 1;
			public float acceleration = 8;
			public float armSpeed = 2;

			[Space(4)]
			public float strafeTime = 0.25f;
			public float strafeDistance = 3;

			[Space(4)]
			public float jumpHeight = 2.4f;
			public float doubleJumpHeight = 3.2f;
			public float gravity = -20;

			[Space(4)]
			public bool grounded = true;
			public float groundedHitboxOffset = -0.14f;
			public float groundedHitboxRadius = 0.5f;
			public LayerMask groundedHitboxLayers;

			[Header("Readonly")]
			public float speed;
			public float verticalVelocity;
			public int lane;
			public float strafeCooldown;
			public bool dash;
			public float dashCooldown;
			public bool doubleJump;
			public float armSwing;
		}
		#endregion Movement
		#region Powers
		[Serializable]
		public class Powers
		{
			public int maxShots = 6;
			public float shotChargeTime = 2;
			public float maxSlowTime = 10;

			[Header("Readonly")]
			public int shots;
			public float shotCharge;
			public float slowTime;
		}
		#endregion Powers
		#region Arm
		[Serializable]
		public class Arm
		{
			public Animator animator;
			public GameObject mesh;

			public static float runTimer;

			public static int run;
			public static int strafeLeft;
			public static int strafeRight;

			public static int dash;
			public static int jump;
			public static int grounded;

			public static int aim;
			public static int shoot;
			public static int slowTime;

			public static void SetIDs()
			{
				run = Animator.StringToHash("Run");
				strafeLeft = Animator.StringToHash("Strafe Left");
				strafeRight = Animator.StringToHash("Strafe Right");

				dash = Animator.StringToHash("Dash");
				jump = Animator.StringToHash("Jump");
				grounded = Animator.StringToHash("Grounded");

				aim = Animator.StringToHash("Aim");
				shoot = Animator.StringToHash("Shoot");
				slowTime = Animator.StringToHash("Slow Time");
			}

			public void Animate(int id) => animator.SetTrigger(id);
			public void Animate(int id, bool value) => animator.SetBool(id, value);
			public void Animate(int id, float value) => animator.SetFloat(id, value);
		}
		#endregion Arm


		public CharacterController controller;
		public Transform cameraPosition;

		public Head head;
		public Movement movement;
		public Powers powers;
		public Arm rightArm;
		public Arm leftArm;


		public void Start()
		{
			Arm.SetIDs();

			movement.armSwing = 2;
			powers.shots = powers.maxShots;
			powers.slowTime = powers.maxSlowTime;
		}

		public void Update()
		{
			movement.dashCooldown += Time.deltaTime;
			movement.strafeCooldown += Time.deltaTime;
			if (movement.strafeCooldown >= movement.strafeTime) Strafe();
			CheckGround();

			float y = Jump();
			float z = Run();
			controller.Move(new Vector3(0, y, z));


			Aim();
			if (Input.shoot.Down) rightArm.Animate(Arm.shoot);


			movement.armSwing += movement.armSpeed * Time.unscaledDeltaTime;
			if (movement.armSwing >= 1) movement.armSwing -= 1;
			rightArm.Animate(Arm.run, movement.armSwing);
			leftArm.Animate(Arm.run, movement.armSwing - 0.5f);
		}
		public void LateUpdate()
		{
			TurnHead();
		}


		public void Strafe()
		{
			int lane = movement.lane;
			if (Input.strafeRight.Down) lane++;
			if (Input.strafeLeft.Down) lane--;

			if (lane != movement.lane)
			{
				movement.strafeCooldown = 0;
				Transition.Add(v => SetPosition(v, null, null), EaseFunctions.Cubic, EaseDirections.Out, movement.lane * movement.strafeDistance, lane * movement.strafeDistance, movement.strafeTime);
				movement.lane = lane;
			}
		}
		public void CheckGround()
		{
			Vector3 spherePosition = new Vector3(transform.position.x, transform.position.y - movement.groundedHitboxOffset, transform.position.z);
			movement.grounded = Physics.CheckSphere(spherePosition, movement.groundedHitboxRadius, movement.groundedHitboxLayers, QueryTriggerInteraction.Ignore);
		}
		public float Run()
		{
			float input = 0;
			if (Input.speedUp.Held) input += 0.25f;
			if (Input.slowDown.Held) input -= 0.25f;

			float speed = movement.runSpeed * (movement.speedModifier + input) * Time.timeScale;
			if (controller.velocity.z < speed - 0.1f || controller.velocity.z > speed + 0.1f)
			{
				movement.speed = Mathf.Lerp(controller.velocity.z, speed, Time.deltaTime * movement.acceleration);
				movement.speed = Mathf.Round(movement.speed * 1000f) / 1000f;
			}
			else
				movement.speed = speed;

			if (Input.dash.Down && movement.dashCooldown >= 0.25f && (movement.grounded || movement.dash))
			{
				movement.dash = false;
				movement.dashCooldown = 0;
				rightArm.Animate(Arm.dash);
				leftArm.Animate(Arm.dash);
				return movement.dashSpeed * Time.deltaTime;
			}
			else
				return movement.speed * Time.deltaTime;
		}
		public float Jump()
		{
			if (movement.grounded)
			{
				movement.dash = true;
				movement.doubleJump = true;
				rightArm.Animate(Arm.grounded, true);
				leftArm.Animate(Arm.grounded, true);

				if (movement.verticalVelocity < 0)
					movement.verticalVelocity = -2f;

				if (Input.jump.Down)
				{
					movement.verticalVelocity = Mathf.Sqrt(movement.jumpHeight * -2f * movement.gravity);
					rightArm.Animate(Arm.jump);
					leftArm.Animate(Arm.jump);
				}
			}
			else
			{
				rightArm.Animate(Arm.grounded, false);
				leftArm.Animate(Arm.grounded, false);

				if (Input.jump.Down && movement.doubleJump)
				{
					movement.doubleJump = false;
					movement.verticalVelocity = Mathf.Sqrt(movement.doubleJumpHeight * -2f * movement.gravity);
					rightArm.Animate(Arm.dash);
					leftArm.Animate(Arm.dash);
				}
			}

			movement.verticalVelocity += movement.gravity * Time.deltaTime;
			return movement.verticalVelocity * Time.deltaTime;
		}

		public void TurnHead()
		{
			Vector2 turn = Input.mouseDelta;
			head.yaw = SoftTurn(head.yaw, turn.x * head.lookSensitivity, 75);
			head.pitch = SoftTurn(head.pitch, turn.y * -head.lookSensitivity, 89.9f);

			cameraPosition.transform.localRotation = Quaternion.Euler(head.pitch, head.yaw, 0);
		}
		public float SoftTurn(float angle, float turn, float clamp)
		{
			if (turn == 0) return angle;

			if (turn > 0)
			{
				if (angle > clamp - head.softClamp)
					turn *= (clamp - angle) / head.softClamp;
			}
			else
			{
				if (-angle > clamp - head.softClamp)
					turn *= (clamp + angle) / head.softClamp;
			}

			angle += turn;
			if (angle < -360f) angle += 360f;
			if (angle > 360f) angle -= 360f;
			return angle;
		}


		public void Aim()
		{
			if (Input.aim.Held && powers.slowTime > 0)
			{
				powers.slowTime -= Time.unscaledDeltaTime;
				Time.timeScale = 0.2f;
				rightArm.Animate(Arm.aim, true);
				leftArm.Animate(Arm.slowTime, true);
			}
			else
			{
				powers.slowTime = Mathf.Clamp(powers.slowTime + Time.unscaledDeltaTime, 0, powers.maxSlowTime);
				Time.timeScale = 1;
				rightArm.Animate(Arm.aim, false);
				leftArm.Animate(Arm.slowTime, false);
			}
		}


		public void SetPosition(float? x = null, float? y = null, float? z = null)
		{
			if (x == null) x = transform.position.x;
			if (y == null) y = transform.position.y;
			if (z == null) z = transform.position.z;

			SetPosition(new Vector3(x.Value, y.Value, z.Value));
		}
		public void SetPosition(Vector3 position)
		{
			controller.enabled = false;
			transform.position = position;
			controller.enabled = true;
		}
	}
}