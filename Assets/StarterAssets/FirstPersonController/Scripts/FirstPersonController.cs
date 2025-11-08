using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
using Unity.Cinemachine;
#endif

namespace StarterAssets
{
	[RequireComponent(typeof(CharacterController))]
#if ENABLE_INPUT_SYSTEM
	[RequireComponent(typeof(PlayerInput))]
#endif
	public class FirstPersonController : MonoBehaviour
	{
		[Header("Player")]
		[Tooltip("Move speed of the character in m/s")]
		public float MoveSpeed = 4.0f;
		[Tooltip("Sprint speed of the character in m/s")]
		public float SprintSpeed = 6.0f;
		[Tooltip("Rotation speed of the character")]
		public float RotationSpeed = 1.0f;
		[Tooltip("Acceleration and deceleration")]
		public float SpeedChangeRate = 10.0f;

		[Space(10)]
		[Tooltip("The height the player can jump")]
		public float JumpHeight = 1.2f;
		[Tooltip("The character uses its own gravity value. The engine default is -9.81f")]
		public float Gravity = -15.0f;

		[Space(10)]
		[Tooltip("Time required to pass before being able to jump again. Set to 0f to instantly jump again")]
		public float JumpTimeout = 0.1f;
		[Tooltip("Time required to pass before entering the fall state. Useful for walking down stairs")]
		public float FallTimeout = 0.15f;

		[Header("Player Grounded")]
		[Tooltip("If the character is grounded or not. Not part of the CharacterController built in grounded check")]
		public bool Grounded = true;
		[Tooltip("Useful for rough ground")]
		public float GroundedOffset = -0.14f;
		[Tooltip("The radius of the grounded check. Should match the radius of the CharacterController")]
		public float GroundedRadius = 0.5f;
		[Tooltip("What layers the character uses as ground")]
		public LayerMask GroundLayers;

		[Header("Cinemachine")]
		[Tooltip("The follow target set in the Cinemachine Virtual Camera that the camera will follow")]
		public GameObject CinemachineCameraTarget;
		[Tooltip("How far in degrees can you move the camera up")]
		public float TopClamp = 90.0f;
		[Tooltip("How far in degrees can you move the camera down")]
		public float BottomClamp = -90.0f;

		// cinemachine
		private float _cinemachineTargetPitch;

		// player
		private float _speed;
		private float _rotationVelocity;
		private float _verticalVelocity;
		private float _terminalVelocity = 53.0f;

		// timeout deltatime
		private float _jumpTimeoutDelta;
		private float _fallTimeoutDelta;


        //Modify
        [Header("Camera Tilt (Strafe)")]
        [Tooltip("左右平移时镜头最大倾斜角（度，右移一般是负Z）")]
        public float StrafeTiltMax = 6f;
        [Tooltip("倾斜平滑时间（秒），越小越跟手")]
        public float StrafeTiltSmoothTime = 0.08f;
        private float _strafeTiltZ;
		private float _strafeTiltVel;

        // ====================== 关键修改开始 ======================
        [Header("Slide Settings")] // [MOD]
        public KeyCode SlideKey = KeyCode.LeftControl; // 触发滑铲的按键
        public float SlideSpeed = 10f; // 滑铲初速度
        public float SlideTime = 0.5f; // 滑铲持续时间
        private bool _isSliding = false; // 是否正在滑铲
        private float _slideTimer = 0f;
        private Vector3 _slideDirection;

        [Header("SlideCD")]
        public float SlideCoolDown;
		public float SlideCoolDownTimer=0;
		public bool CanSlide=true;

        // [MOD] 镜头下沉部分
        [Header("Slide Camera Effect")]
        public Transform CameraRoot;               // 你的相机根物体
		public bool EnableSlide;
		public float SlideFriction;
        public float SlideCameraOffset = -0.6f;    // 滑铲时相机下沉距离（负值向下）
        public float CameraLerpSpeed = 8f;         // 镜头移动平滑速度
        private Vector3 _originalCamLocalPos;      // 原始相机位置
        private Vector3 _targetCamLocalPos;        // 当前目标相机位置
        private float _currentSlideSpeed = 0f;

		[Header("Slide Collider Change")]
		public CapsuleCollider CapsuleCollider;
		public Vector3 NormalSize;
		public Vector3 SlideSize;
		public float NormalHeight;
		public float SlideHeight;
        // ====================== 关键修改结束 ======================

        public enum ControllerState
        {
            CanMove,
            StopMove,
			Shock
        }
        public ControllerState CS;


#if ENABLE_INPUT_SYSTEM
        private PlayerInput _playerInput;
#endif
		private CharacterController _controller;
		private StarterAssetsInputs _input;
		private GameObject _mainCamera;

		private const float _threshold = 0.01f;

		private bool IsCurrentDeviceMouse
		{
			get
			{
				#if ENABLE_INPUT_SYSTEM
				return _playerInput.currentControlScheme == "KeyboardMouse";
				#else
				return false;
				#endif
			}
		}

		private void Awake()
		{
			// get a reference to our main camera
			if (_mainCamera == null)
			{
				_mainCamera = GameObject.FindGameObjectWithTag("MainCamera");
			}
		}

		private void Start()
		{
			_controller = GetComponent<CharacterController>();
			_input = GetComponent<StarterAssetsInputs>();
#if ENABLE_INPUT_SYSTEM
			_playerInput = GetComponent<PlayerInput>();
#else
			Debug.LogError( "Starter Assets package is missing dependencies. Please use Tools/Starter Assets/Reinstall Dependencies to fix it");
#endif

			// reset our timeouts on start
			_jumpTimeoutDelta = JumpTimeout;
			_fallTimeoutDelta = FallTimeout;
			//CamPosRecord
            _originalCamLocalPos = CameraRoot.localPosition;
            _targetCamLocalPos = _originalCamLocalPos;
			//SlideCoolDown
			SlideCoolDownTimer = SlideCoolDown;
			//Capusle ReCORD
			NormalSize = CapsuleCollider.center;
			NormalHeight = CapsuleCollider.height;
        }

		private void Update()
		{
			
			switch (CS)
			{
				case ControllerState.CanMove:
                    // [MOD] 更新滑铲（不移动）
                    if (EnableSlide)
                    {
                        if (Input.GetKeyDown(SlideKey) && Grounded && _input.move.sqrMagnitude > 0.1f && !_isSliding && CanSlide)
                            StartSlide();

                        if (_isSliding) HandleSlide();
                    }

                    if (SlideCoolDownTimer >= SlideCoolDown)
                    {
                        SlideCoolDownTimer = SlideCoolDown;
                        CanSlide = true;
                    }

					if (_isSliding == false && CanSlide == false)
					{
						SlideCoolDownTimer += Time.deltaTime;
					}
                    // [MOD] 始终走 Move()，由 Move() 统一结算滑铲/常规
                    Move();
                    JumpAndGravity();
                    GroundedCheck();
                    // 镜头平滑
                    UpdateCameraSlideEffect();
                    break;

				case ControllerState.StopMove:
					//ReplaceCamera
                    _targetCamLocalPos = _originalCamLocalPos;
					UpdateCameraSlideEffect();

                    CapsuleCollider.center = NormalSize;
                    CapsuleCollider.height = NormalHeight;
                    break;

				case ControllerState.Shock:

				break;
			}
			
		}

		private void LateUpdate()
		{
			CameraRotation();
		}

		private void GroundedCheck()
		{
			// set sphere position, with offset
			Vector3 spherePosition = new Vector3(transform.position.x, transform.position.y - GroundedOffset, transform.position.z);
			Grounded = Physics.CheckSphere(spherePosition, GroundedRadius, GroundLayers, QueryTriggerInteraction.Ignore);
		}

        private void CameraRotation()
        {
            // 鼠标与手柄的时间系数
            float deltaTimeMultiplier = IsCurrentDeviceMouse ? 1.0f : Time.deltaTime;

            // 处理俯仰与水平旋转输入（与原逻辑一致）
            if (_input.look.sqrMagnitude >= _threshold)
            {
                _cinemachineTargetPitch += _input.look.y * RotationSpeed * deltaTimeMultiplier;
                _rotationVelocity = _input.look.x * RotationSpeed * deltaTimeMultiplier;

                // 旋转玩家左右朝向（Yaw）
                transform.Rotate(Vector3.up * _rotationVelocity);
            }

            // 限制俯仰
            _cinemachineTargetPitch = ClampAngle(_cinemachineTargetPitch, BottomClamp, TopClamp);

            // === 新增：根据左右移动输入产生“镜头滚转（Z轴倾斜）” ===
            // 右移希望画面右倾：通常为负Z；如需反向把负号去掉
            float targetTilt = -_input.move.x * StrafeTiltMax;
            _strafeTiltZ = Mathf.SmoothDampAngle(_strafeTiltZ, targetTilt, ref _strafeTiltVel, StrafeTiltSmoothTime);

            // 应用到 CM 跟随目标：俯仰 + 滚转（保留原来的 Y=0）
            CinemachineCameraTarget.transform.localRotation = Quaternion.Euler(_cinemachineTargetPitch, 0f, _strafeTiltZ);
        }

        private void Move()
		{
			// set target speed based on move speed, sprint speed and if sprint is pressed
			//float targetSpeed = _input.sprint ? SprintSpeed : MoveSpeed;

			// a simplistic acceleration and deceleration designed to be easy to remove, replace, or iterate upon
			float targetSpeed = MoveSpeed;
            // note: Vector2's == operator uses approximation so is not floating point error prone, and is cheaper than magnitude
            // if there is no input, set the target speed to 0
            if (_input.move == Vector2.zero) targetSpeed = 0.0f;

			// a reference to the players current horizontal velocity
			float currentHorizontalSpeed = new Vector3(_controller.velocity.x, 0.0f, _controller.velocity.z).magnitude;

			float speedOffset = 0.1f;
			float inputMagnitude = _input.analogMovement ? _input.move.magnitude : 1f;

			// accelerate or decelerate to target speed
			if (currentHorizontalSpeed < targetSpeed - speedOffset || currentHorizontalSpeed > targetSpeed + speedOffset)
			{
				// creates curved result rather than a linear one giving a more organic speed change
				// note T in Lerp is clamped, so we don't need to clamp our speed
				_speed = Mathf.Lerp(currentHorizontalSpeed, targetSpeed * inputMagnitude, Time.deltaTime * SpeedChangeRate);

				// round speed to 3 decimal places
				_speed = Mathf.Round(_speed * 1000f) / 1000f;
			}
			else
			{
				_speed = targetSpeed;
			}

            // normalise input direction
            Vector3 inputDirection = new Vector3(_input.move.x, 0.0f, _input.move.y).normalized;
            if (_input.move != Vector2.zero)
                inputDirection = transform.right * _input.move.x + transform.forward * _input.move.y;

            // [MOD] 滑铲时用滑铲方向，并以“滑铲当前速度 vs 普通速度”二者取大
            Vector3 moveDir = inputDirection.normalized;
            float moveSpeed = _speed;

            if (_isSliding)
            {
                moveDir = _slideDirection;                              // 锁定滑铲方向
                moveSpeed = Mathf.Max(_speed, _currentSlideSpeed);       // 速度平滑衔接
            }

            _controller.Move(
                moveDir * (moveSpeed * Time.deltaTime) +
                new Vector3(0.0f, _verticalVelocity, 0.0f) * Time.deltaTime
            );
        }

		private void JumpAndGravity()
		{
			if (Grounded)
			{
				// reset the fall timeout timer
				_fallTimeoutDelta = FallTimeout;

				// stop our velocity dropping infinitely when grounded
				if (_verticalVelocity < 0.0f)
				{
					_verticalVelocity = -2f;
				}

				// Jump
				if (_input.jump && _jumpTimeoutDelta <= 0.0f)
				{
					// the square root of H * -2 * G = how much velocity needed to reach desired height
					_verticalVelocity = Mathf.Sqrt(JumpHeight * -2f * Gravity);
				}

				// jump timeout
				if (_jumpTimeoutDelta >= 0.0f)
				{
					_jumpTimeoutDelta -= Time.deltaTime;
				}
			}
			else
			{
				// reset the jump timeout timer
				_jumpTimeoutDelta = JumpTimeout;

				// fall timeout
				if (_fallTimeoutDelta >= 0.0f)
				{
					_fallTimeoutDelta -= Time.deltaTime;
				}

				// if we are not grounded, do not jump
				_input.jump = false;
			}

			// apply gravity over time if under terminal (multiply by delta time twice to linearly speed up over time)
			if (_verticalVelocity < _terminalVelocity)
			{
				_verticalVelocity += Gravity * Time.deltaTime;
			}
		}

		private static float ClampAngle(float lfAngle, float lfMin, float lfMax)
		{
			if (lfAngle < -360f) lfAngle += 360f;
			if (lfAngle > 360f) lfAngle -= 360f;
			return Mathf.Clamp(lfAngle, lfMin, lfMax);
		}

		private void OnDrawGizmosSelected()
		{
			Color transparentGreen = new Color(0.0f, 1.0f, 0.0f, 0.35f);
			Color transparentRed = new Color(1.0f, 0.0f, 0.0f, 0.35f);

			if (Grounded) Gizmos.color = transparentGreen;
			else Gizmos.color = transparentRed;

			// when selected, draw a gizmo in the position of, and matching radius of, the grounded collider
			Gizmos.DrawSphere(new Vector3(transform.position.x, transform.position.y - GroundedOffset, transform.position.z), GroundedRadius);
		}

        // ====================== 滑铲核心 ======================

        // [MOD] 开始滑铲
        private void StartSlide()
        {
            _isSliding = true;
            _slideTimer = SlideTime;

            _slideDirection = (transform.right * _input.move.x + transform.forward * _input.move.y).normalized;
            if (_slideDirection.sqrMagnitude < 0.01f)
                _slideDirection = transform.forward;

            // [MOD] 不要清空输入，保持玩家可随时转向；速度用“滑铲速度”叠加控制
            _currentSlideSpeed = SlideSpeed;

            // [MOD] 镜头目标下沉位置
            if (CameraRoot != null)
                _targetCamLocalPos = _originalCamLocalPos + new Vector3(0, SlideCameraOffset, 0);

			CanSlide = false;
			SlideCoolDownTimer = 0;

			//Collider
			CapsuleCollider.center = SlideSize;
			CapsuleCollider.height = SlideHeight;
        }


        // [MOD] 滑铲中逻辑
        private void HandleSlide()
        {
            // [MOD] 只做计时与速度衰减，不直接 Move（避免和 Move() 冲突导致顿挫）
            _slideTimer -= Time.deltaTime;

            // 速度按“摩擦”衰减（可用 MoveTowards 更线性）
            _currentSlideSpeed = Mathf.Max(0f, _currentSlideSpeed - SlideFriction * Time.deltaTime);

            // 结束条件
            if (_slideTimer <= 0f || !Grounded || _currentSlideSpeed < 0.1f)
            {
                EndSlide();
            }
        }


        // [MOD] 结束滑铲
        private void EndSlide()
        {
            _isSliding = false;
            _currentSlideSpeed = 0f; // [MOD] 收尾清零

            if (CameraRoot != null)
                _targetCamLocalPos = _originalCamLocalPos; // 镜头恢复高度

            CapsuleCollider.center = NormalSize;
            CapsuleCollider.height = NormalHeight;
        }


        // [MOD] 平滑更新镜头下沉/回弹效果
        private void UpdateCameraSlideEffect()
        {
            if (CameraRoot != null)
            {
                CameraRoot.localPosition = Vector3.Lerp(
                    CameraRoot.localPosition,
                    _targetCamLocalPos,
                    Time.deltaTime * CameraLerpSpeed
                );
            }
        }
    }
}