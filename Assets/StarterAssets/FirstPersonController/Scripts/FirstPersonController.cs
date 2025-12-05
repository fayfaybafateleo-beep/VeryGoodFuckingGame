using UnityEngine;
using Unity.VisualScripting;
using UnityEngine.Windows;
using Unity.Mathematics;




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
        public KeyCode SlideKey = KeyCode.LeftControl; 
        public float SlideSpeed = 10f; 
        public float SlideTime = 0.5f; 
        private bool _isSliding = false; 
        private float _slideTimer = 0f;
        private Vector3 _slideDirection;
        public AudioClip SlideSound1;
        public AudioClip SlideSound2;

        [Header("SlideCD")]
        public float SlideCoolDown;
		public float SlideCoolDownTimer=0;
		public bool CanSlide=true;

    
        [Header("Slide Camera Effect")]
        public Transform CameraRoot;              
		public bool EnableSlide;
		public float SlideFriction;
        public float SlideCameraOffset = -0.6f;    
        public float CameraLerpSpeed = 8f;        
        private Vector3 _originalCamLocalPos;      
        private Vector3 _targetCamLocalPos;        
        private float _currentSlideSpeed = 0f;
        public CinemachineImpulseSource SlideScreenShake;

		[Header("Slide Collider Change")]
		public CharacterController Controller;
		public Vector3 NormalCenter;
		public Vector3 SlideCenter;
		public Vector3 TargetCenter;
		public float NormalHeight;
		public float SlideHeight;
        public float TargetHeight;
        public float CapsuleLerpSpeed = 10f;

        public CapsuleCollider CapsuleCollider;
        public Vector3 NormalCapsuleCenter;
        public Vector3 SlideCapsuleCenter;
        public float NormalCapsuleHeight;
        public float SlideCapsuleHeight;
        // 

        [Header("CameraWalkingWagingPos")]
        public Vector3 PointA = new Vector3(0, 1.375f, 0);
        public Vector3 PointB = new Vector3(0, 1, 0);

        [Header("SpeedOfWagging")]
        public float speed = 1f;
        public bool useLocalPosition = true;

		[Header("SpeedLine")]
        public float SpeedThreshold = 20f;
        public float CurrentSpeed;

        private Vector3 LastPos;
        public ParticleSystem SpeedLine;

        [Header("DualJump")]
        public bool IsDualJump;
        public int MaxJumps = 2;
        public int CurrentjumpCount = 0;
        public bool WasGrounded;
        public float FirstJumpSpeed;
        public float SecondJumpSpeed;
        public AudioClip JumpSound;

        public float DoubleJumpMaxHeight = 1.5f;
        private float DoubleJumpStartY;
        // IsHeightLimitation
        public bool LimitDoubleJumpHeight = false;
        public CinemachineImpulseSource DualJumpScreenShake;


        [Header("Footstep SFX")]
        public AudioSource FootstepSource;   
        public AudioClip[] FootstepClips;    

        public float StepInterval = 0.5f;

        public float MinMoveSpeedForStep = 0.5f;

        private int FootstepIndex = 0;
        private float FootstepTimer = 0f;

        [Header("DeadSetting")]
        public PlayerHealth PH;
        private float DeathTilt = 0f;     // 玩家死亡时的 z 轴倾斜角
        private bool IsDeadTilting = false;

        public PlayerGetInCar PIC;
        public enum ControllerState
        {
            CanMove,
            StopMove,
			Shock
        }
        public ControllerState CS;

		public bool Lock;
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
			NormalCenter = Controller.center;
			NormalHeight = Controller.height;

			TargetCenter = NormalCenter;
			TargetHeight = NormalHeight;

			SpeedLine = GameObject.FindGameObjectWithTag("SpeedLine").GetComponent<ParticleSystem>();
            SpeedLine.Stop();

            LastPos = transform.position;
			SpeedThreshold = 17;
        }

		private void Update()
		{
            if (PIC.IsMounted)
            {
                SpeedLine.Stop();
            }
            switch (PH.PS) 
            {
                case PlayerHealth.PlayerState.Alive:

                    if (CurrentSpeed >= SpeedThreshold)
                    {
                        if (!SpeedLine.isPlaying)
                        {
                            SpeedLine.Play();
                        }
                    }
                    else
                    {
                        if (SpeedLine.isPlaying)
                        {
                            SpeedLine.Stop();
                        }
                    }

                    switch (CS)
                    {
                        case ControllerState.CanMove:
                            // [MOD] 更新滑铲（不移动）
                            if (EnableSlide)
                            {
                                if (UnityEngine.Input.GetKeyDown(SlideKey) && Grounded && _input.move.sqrMagnitude > 0.1f && !_isSliding && CanSlide)
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

                            Move();
                            JumpAndGravity();
                            GroundedCheck();

                            UpdateCameraSlideEffect();
                            UpdateCapsuleSize();

                            float currentSpeed = Mathf.Sqrt(_controller.velocity.x * _controller.velocity.x + _verticalVelocity * _verticalVelocity + _controller.velocity.z * _controller.velocity.z);
                            CurrentSpeed = currentSpeed;

                            HandleFootsteps();
                            break;

                        case ControllerState.StopMove:
                            //ReplaceCamera
                            _targetCamLocalPos = _originalCamLocalPos;
                            UpdateCameraSlideEffect();
                            if (CapsuleCollider.center != NormalCapsuleCenter) ChangeToNormalCapsule();
                            //SpeedLine.Stop();
                            break;

                        case ControllerState.Shock:

                            break;
                    }

                    break;
                case PlayerHealth.PlayerState.Die:
                    SpeedLine.Stop();
                    ClearAllInput();
                   
                    DeathTilt = Mathf.Lerp(DeathTilt, -80f, Time.deltaTime * 3f);

                    transform.rotation = Quaternion.Euler(transform.eulerAngles.x,transform.eulerAngles.y,DeathTilt);
                    break;
            }
			
		}

		private void LateUpdate()
		{
			CameraRotation();
		}

		private void GroundedCheck()
		{
            WasGrounded = Grounded;

            // set sphere position, with offset
            Vector3 spherePosition = new Vector3(
                transform.position.x,
                transform.position.y - GroundedOffset,
                transform.position.z);

            Grounded = Physics.CheckSphere(
                spherePosition,
                GroundedRadius,
                GroundLayers,
                QueryTriggerInteraction.Ignore
            );
            if (Grounded && !WasGrounded)
            {
                CurrentjumpCount = 0;    
                _fallTimeoutDelta = FallTimeout;
            }
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
			if (_input.move == Vector2.zero)
			{
				targetSpeed = 0.0f;
/*
                //CamWagging
                float t = (Mathf.Sin(Time.time * speed) + 1f) / 2f;
                Vector3 newPosition = Vector3.Lerp(pointA, pointB, t);

                // Apply
                if (useLocalPosition)
                {
                    CameraRoot.localPosition = newPosition;
                }
                else
                {
                    CameraRoot.position = newPosition;
                }*/
            }

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
                if (_input.jump && _jumpTimeoutDelta <= 0.0f && CurrentjumpCount < MaxJumps)
                {
                    DoJump(FirstJumpSpeed,false);
                  
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
                //DualJunmp
                if (_input.jump && CurrentjumpCount < MaxJumps)
                {
                    DoJump(SecondJumpSpeed,true);
                }

                // if we are not grounded, do not jump
                _input.jump = false;
			}

			// apply gravity over time if under terminal (multiply by delta time twice to linearly speed up over time)
			if (_verticalVelocity < _terminalVelocity)
			{
				_verticalVelocity += Gravity * Time.deltaTime;
			}
            //HeightLimited
            if (LimitDoubleJumpHeight)
            {
                float currentY = transform.position.y;
                float maxY = DoubleJumpStartY + DoubleJumpMaxHeight;

                if (currentY >= maxY && _verticalVelocity > 0f)
                {
                    _verticalVelocity = 0f;
                    LimitDoubleJumpHeight = false; 
                }
            }

            // apply gravity
            if (_verticalVelocity < _terminalVelocity)
            {
                _verticalVelocity += Gravity * Time.deltaTime;
            }
        }
        public void DoJump(float multiplier, bool isDoubleJump = false)
        {
            _verticalVelocity = Mathf.Sqrt(JumpHeight * -2f * Gravity * multiplier);

            CurrentjumpCount++;

            if (isDoubleJump)
            {
                DoubleJumpStartY = transform.position.y;
                LimitDoubleJumpHeight = true;
                SpeedLine.Play();
                DualJumpScreenShake.GenerateImpulse();

               
            }
            else
            {
                FootstepSource.PlayOneShot(SlideSound2);
            }
            _input.jump = false;

           
            FootstepSource.PlayOneShot(JumpSound);
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

            TargetHeight = SlideHeight;
            TargetCenter = new Vector3(
                NormalCenter.x,
                SlideHeight / 2f, // 中心保持在脚上方
                NormalCenter.z
            );

            //CmaeraEffect
            if (CameraRoot != null)
			{
                _targetCamLocalPos = _originalCamLocalPos + new Vector3(0, SlideCameraOffset, 0);
            }

			//SpeedLine.Play();

			CanSlide = false;
			SlideCoolDownTimer = 0;

            //Collider
            ChangeToSlideCapsule();
            //ScreenShake
            SlideScreenShake.GenerateImpulse();

            FootstepSource.PlayOneShot(SlideSound1);
            FootstepSource.PlayOneShot(SlideSound2);

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

            TargetHeight = NormalHeight;
            TargetCenter = NormalCenter;

            if (CameraRoot != null)
			{
                _targetCamLocalPos = _originalCamLocalPos;
            }
            ChangeToNormalCapsule();
         //   SpeedLine.Stop();

        }


        // [MOD] 平滑更新镜头下沉/回弹效果
        private void UpdateCameraSlideEffect()
        {
            if (CameraRoot == null) return;

            // 1) 滑铲/站立等逻辑的基础目标位
            Vector3 baseTarget = _targetCamLocalPos;

            // 2) 用“实际水平速度”判定是否在移动（更可靠）
            Vector2 vel2D = new Vector2(_controller.velocity.x, _controller.velocity.z);
            bool isMoving = vel2D.sqrMagnitude > 0.01f && Grounded && !_isSliding;  // ← 这里改了

            // 3) 目标位：默认用基础目标；移动时叠加你的往返摆动
            Vector3 target = baseTarget;
            if (isMoving)
            {
				if (CS == ControllerState.CanMove)
				{
                    float t = (Mathf.Sin(Time.time * speed) + 1f) / 2f;
                    Vector3 newPosition = Vector3.Lerp(PointA, PointB, t);
                    target = Vector3.Lerp(baseTarget, newPosition, 0.5f);
                    // 如果 pointA/pointB 是“绝对局部坐标”且你想在 baseTarget 周围摆动，
                    // 也可以用： target = baseTarget + (newPosition - pointA);
                }


            }

            // 4) 统一 Lerp 到目标，确保可“归位”
            CameraRoot.localPosition = Vector3.Lerp(
                CameraRoot.localPosition,
                target,
                Time.deltaTime * CameraLerpSpeed
            );
        }

		void UpdateCapsuleSize()
        {
            
            Controller.height = Mathf.Lerp(_controller.height, TargetHeight, Time.deltaTime * CapsuleLerpSpeed);
            
            Controller.center = Vector3.Lerp(_controller.center, TargetCenter, Time.deltaTime * CapsuleLerpSpeed);
        }

        public void ChangeToNormalCapsule()
        {
            CapsuleCollider.center = NormalCapsuleCenter;
            CapsuleCollider.height = NormalCapsuleHeight;
        }
        public void ChangeToSlideCapsule()
        {
            CapsuleCollider.center = SlideCapsuleCenter;
            CapsuleCollider.height = SlideCapsuleHeight;
        }

        private void HandleFootsteps()
        {
            if (FootstepSource == null) return;
            if (FootstepClips == null || FootstepClips.Length == 0) return;
            if (CS != ControllerState.CanMove) return;

            if (!Grounded)
            {
                FootstepTimer = 0f;
                return;
            }
            if (_isSliding)
            {
                FootstepTimer = 0f;
                return;
            }

            Vector3 horizontalVel = new Vector3(_controller.velocity.x, 0f, _controller.velocity.z);
            float horizontalSpeed = horizontalVel.magnitude;

            if (horizontalSpeed < MinMoveSpeedForStep)
            {
                FootstepTimer = 0f;
                return;
            }

            //FoddStepTimer
            FootstepTimer -= Time.deltaTime;
            if (FootstepTimer > 0f) return;

            AudioClip clip = FootstepClips[FootstepIndex];

            FootstepSource.PlayOneShot(clip);

            FootstepIndex++;
            if (FootstepIndex >= FootstepClips.Length)
            {
                FootstepIndex = 0;
            }
  
            // ResetTimer
            FootstepTimer = StepInterval;
        }
        public void ClearAllInput()
        {
            _input.move = Vector2.zero;
            _input.jump = false;
            _input.look = Vector2.zero;
            _input.sprint = false;
        }
    }
   
}