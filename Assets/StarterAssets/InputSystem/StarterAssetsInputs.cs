using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace StarterAssets
{
    public class StarterAssetsInputs : MonoBehaviour
    {
        [Header("Character Input Values")]
        public Vector2 move;
        public Vector2 look;
        public bool jump;
        public bool sprint;

        [Header("Movement Settings")]
        public bool analogMovement;

        [Header("Mouse Cursor Settings")]
        public bool cursorLocked = true;
        public bool cursorInputForLook = true;

#if ENABLE_INPUT_SYSTEM
        public void OnMove(InputValue value)
        {
            MoveInput(value.Get<Vector2>());
        }

        public void OnLook(InputValue value)
        {
            if (cursorInputForLook)
            {
                Vector2 delta = value.Get<Vector2>();

                // --- 新增：WebGL 异常跳动过滤器 ---
#if UNITY_WEBGL && UNITY_EDITOR
                // 设定一个合理的阈值。普通甩鼠标通常在 10-50 之间。
                // 如果一帧超过 200 (甚至更高，视具体分辨率而定)，通常是浏览器的重置信号
                const float THRESHOLD = 200f; 

                // 检查是否出现异常大的跳跃
                if (delta.sqrMagnitude > THRESHOLD * THRESHOLD)
                {
                    // 这是一个“幽灵数据”，直接丢弃，不执行 LookInput
                    return; 
                }
#endif
                // ------------------------------------

                LookInput(delta);
            }
        }

        public void OnJump(InputValue value)
        {
            JumpInput(value.isPressed);
        }

        public void OnSprint(InputValue value)
        {
            SprintInput(value.isPressed);
        }
#endif


        public void MoveInput(Vector2 newMoveDirection)
        {
            move = newMoveDirection;
        }

        public void LookInput(Vector2 newLookDirection)
        {
            look = newLookDirection;
        }

        public void JumpInput(bool newJumpState)
        {
            jump = newJumpState;
        }

        public void SprintInput(bool newSprintState)
        {
            sprint = newSprintState;
        }

        private void OnApplicationFocus(bool hasFocus)
        {
            SetCursorState(cursorLocked);
        }

        private void SetCursorState(bool newState)
        {
            Cursor.lockState = newState ? CursorLockMode.Locked : CursorLockMode.None;
        }
    }
}
