using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;


namespace GalaxyRush
{
	public class Player : MonoBehaviour
	{
		#region Head
		[Serializable]
		public class Head
		{
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
		#region Shots
		[Serializable]
		public class Shots
		{
			public GameObject prefab;
			public Transform position;
			public int max = 7;
			public float chargeTime = 5;
			public LayerMask collidableLayers;
			public LayerMask targetLayers;

			[Header("Readonly")]
			public Transform[] shots;
			public int amount;
			public int current;
			public int charging;
			public float chargeTimer;
		}
		#endregion Shots
		#region Slow
		[Serializable]
		public class Slow
		{
			public GameObject prefab;
			public Transform position;
			public float max = 10;

			[Header("Readonly")]
			public float amount;
			public bool active;
		}
		#endregion Slow
		#region Arm
		[Serializable]
		public class Arm
		{
			public Animator animator;
			public GameObject mesh;

			public static float runTimer;

			public static int run;
			public static int strafeIn;
			public static int strafeOut;

			public static int dash;
			public static int jump;
			public static int grounded;

			public static int aim;
			public static int shoot;
			public static int slowTime;

			public static void SetIDs()
			{
				run = Animator.StringToHash("Run");
				strafeIn = Animator.StringToHash("Strafe In");
				strafeOut = Animator.StringToHash("Strafe Out");

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


		public Head head;
		public Movement movement;
		public Shots shots;
		public Slow slow;
		public Arm rightArm;
		public Arm leftArm;

		public CharacterController controller;
		public Transform cameraPosition;


		public void Start()
		{
			Arm.SetIDs();
			movement.armSwing = 2;

			shots.shots = new Transform[8];
			for (int i = 0; i < 4; i++) AddShot();
			slow.amount = slow.max;

			Global.player = this;
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


			if (shots.amount < shots.max)
			{
				shots.chargeTimer += Time.unscaledDeltaTime;
				if (shots.chargeTimer >= shots.chargeTime)
					AddShot();
			}

			Aim(Input.aim.Held && slow.amount > 0);
			if (Input.shoot.Down && shots.amount > 0) Shoot();


			movement.armSwing += movement.armSpeed * Time.unscaledDeltaTime;
			if (movement.armSwing >= 1) movement.armSwing -= 1;
			rightArm.Animate(Arm.run, movement.armSwing);
			leftArm.Animate(Arm.run, movement.armSwing - 0.5f);
		}
		public void LateUpdate()
		{
			TurnHead();
			MoveShots();
		}


		public void Strafe()
		{
			int lane = movement.lane;
			if (Input.strafeRight.Down) lane++;
			if (Input.strafeLeft.Down) lane--;

			if (lane != movement.lane)
			{
				if (lane > movement.lane)
				{
					leftArm.Animate(Arm.strafeOut);
					rightArm.Animate(Arm.strafeIn);
				}
				else
				{
					leftArm.Animate(Arm.strafeIn);
					rightArm.Animate(Arm.strafeOut);
				}

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
			if (Input.speedUp.Held) input += 0.5f;
			if (Input.slowDown.Held) input -= 0.5f;

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
			if (slow.active) turn *= 0.5f;
			head.yaw = SoftTurn(head.yaw, turn.x * Settings.lookSensitivity, 75);
			head.pitch = SoftTurn(head.pitch, turn.y * -Settings.lookSensitivity, 89.9f);

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


		public void Aim(bool value)
		{
			if (value)
			{
				slow.active = true;
				slow.amount -= Time.unscaledDeltaTime;

				Time.timeScale = 0.2f;

				rightArm.Animate(Arm.aim, true);
				leftArm.Animate(Arm.slowTime, true);

				Transition.Add(shots.position.gameObject, TransitionComponents.Scale, TransitionUnits.X, EaseFunctions.Quartic, EaseDirections.Out, 0.8f, 1.2f, 1, true);
				Transition.Add(shots.position.gameObject, TransitionComponents.Scale, TransitionUnits.Y, EaseFunctions.Quartic, EaseDirections.Out, 0.8f, 1.2f, 1, true);
				Transition.Add(shots.position.gameObject, TransitionComponents.Scale, TransitionUnits.Z, EaseFunctions.Quartic, EaseDirections.Out, 0.8f, 1.2f, 1, true);
			}
			else
			{
				slow.active = false;
				slow.amount = Mathf.Clamp(slow.amount + Time.unscaledDeltaTime, 0, slow.max);

				Time.timeScale = 1;

				rightArm.Animate(Arm.aim, false);
				leftArm.Animate(Arm.slowTime, false);

				Transition.Add(shots.position.gameObject, TransitionComponents.Scale, TransitionUnits.X, EaseFunctions.Cubic, EaseDirections.Out, 1.2f, 0.8f, 0.3f, true);
				Transition.Add(shots.position.gameObject, TransitionComponents.Scale, TransitionUnits.Y, EaseFunctions.Cubic, EaseDirections.Out, 1.2f, 0.8f, 0.3f, true);
				Transition.Add(shots.position.gameObject, TransitionComponents.Scale, TransitionUnits.Z, EaseFunctions.Cubic, EaseDirections.Out, 1.2f, 0.8f, 0.3f, true);
			}
		}
		public void Shoot()
		{
			GameObject shot = shots.shots[shots.current].gameObject;
			shots.shots[shots.current] = null;

			Vector3 start = shot.transform.position;
			Vector3 end = Global.game.cursor.targeted;
			float time = Vector3.Distance(start, end) / 100;

			Transition.Add(shot, TransitionComponents.Position, TransitionUnits.X, EaseFunctions.Linear, EaseDirections.InOut, start.x, end.x, time, true);
			Transition.Add(shot, TransitionComponents.Position, TransitionUnits.Y, EaseFunctions.Linear, EaseDirections.InOut, start.y, end.y, time, true);
			Transition.Add(shot, TransitionComponents.Position, TransitionUnits.Z, EaseFunctions.Linear, EaseDirections.InOut, start.z, end.z, time, true);

			Transition.Add(shot, TransitionComponents.Scale, TransitionUnits.X, EaseFunctions.Linear, EaseDirections.InOut, 1, 4, time, true);
			Transition.Add(shot, TransitionComponents.Scale, TransitionUnits.Y, EaseFunctions.Linear, EaseDirections.InOut, 1, 4, time, true);
			Transition.Add(shot, TransitionComponents.Scale, TransitionUnits.Z, EaseFunctions.Linear, EaseDirections.InOut, 1, 4, time, true);

			Destroy(shot, time);

			if (shots.current == 7) shots.current = 0;
			else shots.current++;
			shots.amount--;

			rightArm.Animate(Arm.shoot);
		}

		public void MoveShots()
		{
			shots.position.localRotation = Quaternion.Slerp(shots.position.localRotation, Quaternion.Euler(0, 0, shots.current * -45), Time.unscaledDeltaTime * 25);

			for (int i = 0; i < 8; i++)
				if (shots.shots[i] != null)
				{
					Transform shot = shots.shots[i];
					Transform target = shots.position.GetChild(i);

                    shot.position = Vector3.Lerp(shot.position, target.position, 0.9f);
                    shot.rotation = Quaternion.Slerp(shot.rotation, target.rotation, Time.unscaledDeltaTime * 25);
				}
		}
		public void AddShot()
		{
			GameObject shot = Instantiate(shots.prefab);
			shot.name = $"Shot {shots.charging}";
			shot.transform.position = shots.position.position;

			shots.shots[shots.charging] = shot.transform;

			if (shots.charging == 7) shots.charging = 0;
			else shots.charging++;
			shots.amount++;
			shots.chargeTimer = 0;
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