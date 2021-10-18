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
			public float lookSensitivity = 1f;
			public float softClamp = 25f;
		}
        #endregion Head
        #region Movement
        [Serializable]
		public class Movement
		{
			[Header("Speed")]
			public float runSpeed = 24;
			public float dashSpeed = 100;
			public float speedModifier = 1;
			public float acceleration = 8;

			[Header("Strafe")]
			public float strafeTime = 0.25f;
			public float strafeDistance = 3;

			[Header("Vertical")]
			public float jumpHeight = 2.4f;
			public float doubleJumpHeight = 3.2f;
			public float gravity = -20;

			[Header("Ground Check")]
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
		public Arm leftArm;
		public Arm rightArm;


        public void Start()
        {
            
        }

        public void Update()
        {
            
        }
        public void LateUpdate()
        {
            
        }
    }
}